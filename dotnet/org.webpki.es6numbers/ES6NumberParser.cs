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
using System.Diagnostics;
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
 
        const int MAX_EXPONENT = 0x7fe; // 2046

        const decimal LSB_FACTOR = 0.000000000000000011102230246m;

        public static string ieeeString;

        public static bool roundBit;

        public static string higher;
        public static string lower;
        public static string orig;
        public static bool upRound;
        public static bool evenHigh;
        public static bool evenLow;

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

        static string truncate(decimal v)
        {
            string str = v.ToString();
            if (str.Length > 22)
            {
                str = str.Substring(0, 22);
            }
            else
            {
                while (str.Length < 22)
                {
                    str += '0';
                }
            }
            return str;
        }
        
        // Number format: d{f...}   Note: "d" MUST be 1-9 
        internal static double Parse(bool sign, string digitAndOptionalFraction, int base10Exponent)
        {
            

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
            decimal decimalValue= Decimal.Parse(digitAndOptionalFraction.Substring(0, 1) +
                                                '.' +
                                                digitAndOptionalFraction.Substring(1));

            // Entering the "bit fiddling" where we are stuffing an "ulong" with IEEE-754 data
            int base2Exponent = base2Entry.Base2Exponent;
            ulong ieee754 = (ulong)(decimalValue * base2Entry.MantissaMultiplier);
            ieeeString = DebugBinary(ieee754);
            while ((ieee754 & SCALE_POINT_MASK) != 0)
            {
                ieee754 >>= 1;
                ieeeString = DebugBinary(ieee754);
                base2Exponent++;
            }

            // Setup. "Roundcontrol to major Tom"
            ulong roundControl = SCALE_POINT_MASK;

            // Normalized versus Denormalized
            string adjusted = "no";

            if (base2Exponent <= 0)
            {
                // Denormalized number. Ignore numbers that do not make any IEEE-754 bits 
                if (base2Exponent < -ES6NumberFormatter.MANTISSA_SIZE)
                {
                    return 0; 
                }
                ieeeString = DebugBinary(ieee754);
                ieee754 >>= -base2Exponent;
                ieeeString = DebugBinary(ieee754);

                // Denormalized numbers have a zero exponent
                base2Exponent = 0;
            }
            else
            {
                // Normalized number. Assume a "virtual" 1 like 1.nnnnnn
                ieee754 <<= 1;
                ieeeString = DebugBinary(ieee754);
                 roundControl <<= 1;
            }

            orig = truncate(decimalValue);
            roundBit = (ieee754 & MANTISSA_ROUNDER) != 0;
            decimal lowerValue = decimalValue  - (decimalValue * (LSB_FACTOR * (ieee754 & ((MANTISSA_ROUNDER << 1) - 1)))) / (MANTISSA_ROUNDER << 1);
            lower = truncate(lowerValue);
            decimal higherValue = decimalValue + decimalValue * LSB_FACTOR;
                                                 
            higher = truncate(higherValue);
            upRound = (higherValue - decimalValue) < (decimalValue - lowerValue);
            evenHigh = true;
            int q = 0;
            foreach (char c in higher)
            {
                char d = orig[q++];
                if (c != d)
                {
                    if ((c & 1) == 1 || (d & 1) == 0)
                    {
                        evenHigh = false;
                    }
                    break;
                }
            }
            q = 0;
            evenLow = true;
            foreach (char c in lower)
            {
                char d = orig[q++];
                if (c != d)
                {
                    if ((c & 1) == 1 || (d & 1) == 0)
                    {
                        evenLow = false;
                    }
                    break;
                }
            }
            decimal j2 = decimal.Round(((decimal)(ieee754 >> 8) * Base10Lookup.Cache[base2Exponent].FractionMultiplier) / 0x0010000000000000, 22);
            decimal j3 = decimal.Round(((decimal)((ieee754 >> 8) + 1) * Base10Lookup.Cache[base2Exponent].FractionMultiplier) / 0x0010000000000000, 22);
            if (j3 > 10)
            {
                j3 /= 10;
                j2 /= 10;
            }
            lower = truncate(j2);
            higher = truncate(j3);
            j3 -= decimalValue + decimalValue - j2;
            evenHigh = j3 < 0;
            upRound = j3 == 0;
            //           if (j3 < 0)
            if (j3 < 0 || (j3 == 0 && (ieee754 & 0xff) != 0 && (ieee754 & 0x100) != 0))
            {
                ieee754 += 0x100;
            }
            /*
            if ((ieee754 & MANTISSA_ROUNDER) != 0)
            {
                if ((ieee754 & (MANTISSA_ROUNDER - 1)) != 0)
                {
                    Console.WriteLine("Round CORE: " + ieeeString + " o=" + digitAndOptionalFraction + " a=" + adjusted);
                    ieee754 += MANTISSA_ROUNDER;
                }
                else
                {
                    adjusted = (decimalValue * 1.000000000000000055511151m).ToString();
                    string refValue = digitAndOptionalFraction + "0000000000000000000000000";
                    int q = 0;
                    foreach (char c in adjusted)
                    {
                        if (c == '.')
                        {
                            continue;
                        }
                        if (c != refValue[q++])
                        {
                            if ((c & 1) == 0 || c < '2' || c > '8')
                            {
                                Console.WriteLine("Round CLOSEST: " + ieeeString + " o=" + refValue + " a=" + adjusted);
                                ieee754 += MANTISSA_ROUNDER;
                            }
                            else
                            {
                                Console.WriteLine("NO Round: " + ieeeString + " o=" + refValue + " a=" + adjusted);
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("NO Round: " + ieeeString + " o=" + digitAndOptionalFraction + " a=" + adjusted);
            }
            */

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
 //           ieeeString = DebugBinary(ieee754);

            // Mantissa MSB is now at bit 59. Move it down to its proper position at bit 51
            ieee754 >>= 8;
 //           ieeeString = DebugBinary(ieee754);

            // Add exponent shifted into the proper position
            ieee754 += (ulong)base2Exponent << ES6NumberFormatter.MANTISSA_SIZE;
 //           ieeeString = DebugBinary(ieee754);

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
            // 1.0228401905118014e+64
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
