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
        const ulong MANTISSA_ROUNDER        = 0x0000000000000800L;
        const ulong SCALE_POINT             = 0xe000000000000000L;
        const ulong LARGEST_SAFE_MULTIPLIER = 0x1000000000000000L;

        const int MAX_EXPONENT = 0x7fe; // 2046

        // Number format: d{f...}   Note: "d" must be 1-9 
        internal static double Parse(bool sign, string digitAndOptionalFraction, int exponent)
        {
            int base10index = exponent + Base2Lookup.EXPONENT_OFFSET;
            if (base10index < 0)
            {
                return 0;  // Too small to make any IEEE-754 bits
            }
            if (base10index >= Base2Lookup.Cache.Length)
            {
                return Double.NaN;   // Overflow are dealt with as a Not a Number
            }
            Base2Lookup base2Entry = Base2Lookup.Cache[base10index];
            int exp2Update = 0;
            decimal base2 = Decimal.Parse(digitAndOptionalFraction.Substring(0, 1) + 
                                          '.' +
                                          digitAndOptionalFraction.Substring(1));
            base2 *= base2Entry.MantissaMultiplier;
            if (base2 > 17)
            {
                base2 /= 2;  // Be kind to "unlong"
                exp2Update++;
            }
            ulong binary = (ulong)(base2 * LARGEST_SAFE_MULTIPLIER);
            while ((binary & SCALE_POINT) != 0)
            {
                binary >>= 1;
                exp2Update++;
            }
            while ((binary & MSB_U64) == 0)
            {
                binary <<= 1;
            }
            exp2Update += base2Entry.Base2Exponent;
            if (exp2Update > MAX_EXPONENT)
            {
                return Double.NaN;   // Overflow are dealt with as a Not a Number
            }
            if (exp2Update < 0)  // Are we dealing with unormalized numbers?
            {
                if (exp2Update < -ES6NumberFormatter.MANTISSA_SIZE)
                {
                    return 0;  // Too small to make any IEEE-754 bits
                }
                binary >>= -exp2Update;
                exp2Update = 0;
            }
            else
            {
                binary <<= 1;  // Normalized numbers use a "virtual" 1.nnnnnn
            }
            binary += MANTISSA_ROUNDER;
            binary >>= 12;
            binary += (ulong)exp2Update << ES6NumberFormatter.MANTISSA_SIZE;
            if (sign)
            {
                binary |= MSB_U64;
            }
            return BitConverter.Int64BitsToDouble((long)binary);
        }

        public static double TryParse(string number, out string error)
        {
            if (!NUMBER_FORMAT.IsMatch(number))
            {
                error = "Number syntax error";
                return Double.NaN;
            }
            error = null;
            int startNum = 0;
            bool signBit = false;
            if (number[0] == '-')
            {
                signBit = true;
                startNum = 1;
            }
            int exponent = 0;
            int startExp = number.IndexOfAny(EXPONENT_LETTERS);
            if (startExp > 0)
            {
                exponent = int.Parse(number.Substring(startExp + 1));
                number = number.Substring(0, startExp);
            }
            while (number.Length > 0 && number[0] == '0')
            {
                number = number.Substring(1);
            }
            while (number.Length > 0 && number[number.Length - 1] == '0')
            {
                number = number.Substring(0, number.Length - 1);
            }
            if (number.Length == 0)
            {
                return 0;
            }
            int point = number.IndexOf('.');
            if (point >= 0)
            {
                if (number.Length == 1)
                {
                    return 0;
                }
                number = number.Substring(0, point) + number.Substring(point + 1);
                if (point == 0)
                {
                    exponent--;
                    while (number[0] == '0')
                    {
                        number = number.Substring(1);
                        exponent--;
                    }
                }
                else
                {
                    exponent -= point - 1;
                }
            }
            double d = Parse(signBit, number.Substring(startNum), exponent);
            if (Double.IsNaN(d))
            {
                error = "Number out of limits";
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
