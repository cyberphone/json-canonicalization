/*
 *  Copyright 2006-2018 WebPKI.org (http://webpki.org).
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

using System;
using System.Text.RegularExpressions;

//////////////////////////////////////////////////////
// Parsing of ES6/JSON compatible numbers and       //
// returning them in IEEE-754 double precision      //
//                                                  //
// Author: Anders Rundgren                          //
//////////////////////////////////////////////////////

namespace Org.Webpki.Es6Numbers
{
    public static class ES6NumberParser
    {
        static internal Regex NUMBER_FORMAT = new Regex("^-?[0-9]+(\\.[0-9]+)?([eE][-+]?[0-9]+)?$");

        static internal char[] EXPONENT_LETTERS = {'e', 'E'};

        const ulong MSB_U64                 = 0x8000000000000000L;
        const ulong MANTISSA_ROUNDER        = 0x0000000000000080L;
        const ulong SCALE_POINT_MASK        = 0xf000000000000000L;
        const ulong LARGEST_SAFE_MULTIPLIER = 0x0800000000000000L;

        const int MAX_EXPONENT = 0x7fe; // 2046

        public static string DebugBinary(ulong v)
        {
            string res = Convert.ToString((long)v, 2);
            while (res.Length < 64)
            {
                res = '0' + res;
            }
            for (int q = 4, position = 0; q <= 60; q += 4, position++)
            {
                res = res.Substring(0, q + position) + ' ' + res.Substring(q + position);
            }
            return res;
        }
        
        // Number format: d{f...}   Note: "d" MUST be 1-9 
        internal static double Parse(bool sign, string digitAndOptionalFraction, int base10Exponent)
        {
            string ieeeString;

            int base10index = base10Exponent + Base2Lookup.EXPONENT_OFFSET;

            // Sanity check
            if (base10index < 0)
            {
                // Very small number, not worth bothering with. Return as 0
                return 0;
            }

            // Sanity check
            if (base10index >= Base2Lookup.Cache.Length)
            {
                // Huge number, outside of IEEE 754 double precision. Return as NaN
                return Double.NaN;
            }

            // Core base10 to base2 operation
            Base2Lookup base2Entry = Base2Lookup.Cache[base10index];
            decimal base2 = Decimal.Parse(digitAndOptionalFraction.Substring(0, 1) + 
                                          '.' +
                                          digitAndOptionalFraction.Substring(1));

            // The number in "base2" MUST after this be >= 1 and < 20
            base2 *= base2Entry.MantissaMultiplier;

            // Entering the "bit fiddling" where we are stuffing an "ulong" with IEEE-754 data
            int base2Exponent = 0;
            ulong ieee754 = (ulong)(base2 * LARGEST_SAFE_MULTIPLIER);
            ieeeString = DebugBinary(ieee754);
            while ((ieee754 & SCALE_POINT_MASK) != 0)
            {
                ieee754 >>= 1;
                ieeeString = DebugBinary(ieee754);
                base2Exponent++;
            }

            // Add base10 to base2 exponent offset
            base2Exponent += base2Entry.Base2Exponent;

            // Setup. "Roundcontrol to major Tom"
            ulong roundControl = SCALE_POINT_MASK;

            // Normalized versus Unormalized
            if (base2Exponent <= 0)
            {
                // Unormalized number. Ignore numbers that do not make any IEEE-754 bits 
                if (base2Exponent < -ES6NumberFormatter.MANTISSA_SIZE)
                {
                    return 0; 
                }
                ieee754 += MANTISSA_ROUNDER << -base2Exponent;
                ieeeString = DebugBinary(ieee754);
                ieee754 >>= -base2Exponent;
                ieeeString = DebugBinary(ieee754);

                // Unormalized numbers have a zero exponent
                base2Exponent = 0;
            }
            else
            {
                // Normalized number. Assume a "virtual" 1 like 1.nnnnnn
                ieee754 <<= 1;
                ieeeString = DebugBinary(ieee754);
                ieee754 += MANTISSA_ROUNDER;
                ieeeString = DebugBinary(ieee754);
                roundControl <<= 1;
            }

            // There could be a carry from rounding
            if ((ieee754 & roundControl) != 0)
            {
                ieee754 >>= 1;
                ieeeString = DebugBinary(ieee754);
                base2Exponent++;
            }

            // Check again for overflow
            if (base2Exponent > MAX_EXPONENT)
            {
                return Double.NaN;   // Return as a Not a Number
            }

            // Remove possible "virtual" 1
            ieee754 &= ~SCALE_POINT_MASK;
            ieeeString = DebugBinary(ieee754);

            // Mantissa MSB is now at bit 59. Move it down to its proper position at bit 51
            ieee754 >>= 8;
            ieeeString = DebugBinary(ieee754);

            // Add exponent shifted into the proper position
            ieee754 += (ulong)base2Exponent << ES6NumberFormatter.MANTISSA_SIZE;
            ieeeString = DebugBinary(ieee754);

            // Finally, add the optional sign bit
            if (sign)
            {
                ieee754 |= MSB_U64;
            }

            // Make it double please!
            return BitConverter.Int64BitsToDouble((long)ieee754);
        }

        public static double TryParse(string number, out string error)
        {
            error = null;
            if (!NUMBER_FORMAT.IsMatch(number))
            {
                error = "Number syntax error";
                return Double.NaN;
            }

            // Find and remove possible sign
            bool signBit = false;
            if (number[0] == '-')
            {
                signBit = true;
                number = number.Substring(1);
            }

            // Find and remove possible exponent
            int exponent = 0;
            int startExp = number.IndexOfAny(EXPONENT_LETTERS);
            if (startExp > 0)
            {
                exponent = int.Parse(number.Substring(startExp + 1));
                number = number.Substring(0, startExp);
            }

            // Remove leading zeroes
            while (number.Length > 0 && number[0] == '0')
            {
                number = number.Substring(1);
            }

            // Find and remove possible decimal point
            int point = number.IndexOf('.');
            if (point == 0)
            {
                // .{000}fff
                number = number.Substring(1);

                // Normalize to d.ff
                exponent--;
                while (number.Length > 0 && number[0] == '0')
                {
                    number = number.Substring(1);
                    exponent--;
                }
            }
            else if (point > 0)
            {
                // ddd.{fff}
                number = number.Substring(0, point) + (point == number.Length - 1 ? "" : number.Substring(point + 1));

                // Normalize to d.dd{fff}
                exponent += point - 1;
            }
            else
            {
                // Normalize ddd to d.ff
                exponent += number.Length - 1;
            }

            // One or more zeroes only?
            if (number.Length == 0)
            {
                return 0;
            }

            // Fnally, the low level stuff!
            double d = Parse(signBit, number, exponent);
            if (Double.IsNaN(d))
            {
                error = "Number out of range";
            }
            return d;
        }

        public static double Parse(string number)
        {
            string error;
            double result = TryParse(number, out error);
            if (error == null)
            {
                return result;
            }
            throw new ArgumentException(error);
        }
    }
}
