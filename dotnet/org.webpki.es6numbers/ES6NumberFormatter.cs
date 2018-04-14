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
using System.Linq;

//////////////////////////////////////////////////////
// Formatting of IEEE-754 double precision objects  //
// into the ES6/JSON compatible number format       //
//                                                  //
// Author: Anders Rundgren                          //
//////////////////////////////////////////////////////

namespace Org.Webpki.Es6Numbers
{
    public static class ES6NumberFormatter
    {
        internal const ulong MASK_FRACTION = 0x000fffffffffffff;
        internal const ulong IMPLICIT_ONE  = 0x0010000000000000;

        internal const int MANTISSA_SIZE   = 52;

        internal const int START_DIGITS    = 18;

        class Success
        {
            internal int exp10;
            internal int digits;
            internal string numbers;

            internal Success(int exp10, int digits, string numbers)
            {
                this.exp10 = exp10;
                this.digits = digits;
                this.numbers = numbers;
            }
        }

        static string UBin(ulong v)
        {
            string res = Convert.ToString((long)v, 2);
            while (res.Length < 64)
            {
                res = '0' + res;
            }
            return res;
        }

        static bool match(double refValue, int exp10, string fraction)
        {
            Base2Lookup base2Entry = Base2Lookup.Cache[exp10 + 349];
            int exp2 = base2Entry.Base2Exponent;
            decimal fractionDec = decimal.Parse(fraction) * base2Entry.Multiplier;
            ulong fractionBin = (ulong)fractionDec;
            string bin = Convert.ToString((long)fractionBin, 2);
            int nativeExp = exp10 - 1;
            string expString = (nativeExp >= 0 ? "e+" : "e") + nativeExp;
            string native = fraction.Substring(0, 1) + "." + fraction.Substring(1) + expString;
            //             Console.WriteLine(native);
            if (native == "1.797693134862316e+308")
            {
                return false;
            }
            return refValue == double.Parse(native);
         }

        public static string Format(double d)
        {
            if (Double.IsNaN(d))
            {
                throw new ArgumentException("Not a number");
            }
            if (Double.IsInfinity(d))
            {
                throw new ArgumentException("Infinity");
            }
            bool sign = false;
            if (d < 0)
            {
                sign = true;
                d = -d;
            }
            ulong raw = (ulong)BitConverter.DoubleToInt64Bits(d);
            if (raw == 0)
            {
                return "0";
            }
            uint index = (uint)(raw >> MANTISSA_SIZE);
            Base10Lookup base10Entry = Base10Lookup.Cache[index];
            decimal value = base10Entry.Multiplier;
            int exp10 = base10Entry.Base10Exponent;
            ulong fractionBin = raw & MASK_FRACTION;
            ulong divider = IMPLICIT_ONE;
            if (index == 0)
            {
                divider >>= 1;
            }
            else
            {
                fractionBin |= IMPLICIT_ONE;
            }

            decimal multiplier = fractionBin;
            multiplier /= divider;
            value *= multiplier;
            int digits = START_DIGITS;
            while (value < 1)
            {
                value *= 10;
                exp10--;
                digits--;
            }
            if (value >= 10)
            {
                exp10++;
                value /= 10;
            }
            string fraction = value.ToString();
            decimal edgeCaseNormal = value / 10m;
            exp10++;
            if (fraction.Contains('.'))
            {
                fraction = fraction.Substring(0, 1) + fraction.Substring(2);
            }
            digits = Math.Min(digits, fraction.Length);
            while (fraction[digits - 1] == '0')
            {
                digits--;
            }
            Success hadSuccess = null;
            string save = fraction;
            string lastDigitCheck = null;
            while (digits > 0)
            {
                string svalue = fraction.Substring(0, digits);
                bool success = match(d, exp10, svalue);
                if (success)
                {
      //              Console.WriteLine("Succ=" + native);
                    if (lastDigitCheck != null)
                    {
                        string ldc = lastDigitCheck.Substring(0, digits);
                        bool yes = match(d, exp10, ldc);
                        if (yes)
                        {
                            decimal challenger = decimal.Parse("." + ldc);
                            decimal rounded = decimal.Parse("." + svalue);
                            if (edgeCaseNormal - challenger <= rounded - edgeCaseNormal)
                            {
  //                              Console.WriteLine("LDC YES =" + ldc);
                                fraction = lastDigitCheck;
                            }
                            else
                            {
    //                            Console.WriteLine("LDC NO=" + ldc);
                            }
                        }
                    }
                    hadSuccess = new Success(exp10, digits, fraction);
                }
                fraction = save;
                if ((!success && hadSuccess != null) || (success && digits == 1))
                {
                    // s x 10** n−k is m
                    int k = hadSuccess.digits;
                    int n = hadSuccess.exp10;
                    string s = hadSuccess.numbers.Substring(0, k);
        //            Console.WriteLine("k=" + k + " n=" + n + " s=" + s);
                    if (k <= n && n <= 21)
                    {
                        svalue = s;
                        int q = n - k;
                        while (--q >= 0)
                        {
                            svalue += '0';
                        }
                    }
                    else if (n > 0  && n <= 21)
                    {
                        svalue = s.Substring(0, n) + '.' + s.Substring(n);
                    }
                    else if (n > -6 && n <= 0)
                    {
                        svalue = "0.";
                        while (++n <= 0)
                        {
                            svalue += '0';
                        }
                        svalue += s;
                    }
                    else
                    {
                        if (k == 1)
                        {
                            svalue = s;
                        }
                        else
                        {
                            svalue = s.Substring(0, 1) + '.' + s.Substring(1);
                        }
                        int e = n - 1;
                        svalue += "e" + (e < 0 ? "-" : "+") + (e < 0 ? -e : e);
                    }
                    return ((sign ? "-" : "") + svalue);
                }
                char last = fraction[--digits];
                lastDigitCheck = null;
                if (last >= '5')
                {
                    int q = digits;
                    char[] s = new char[digits];
                    int carry = 1;
                    lastDigitCheck = (q > 1 && (fraction[q -1] & 1) == 0) ? fraction : null;
                    while (--q >= 0)
                    {
                        char c = (char)(fraction[q] + carry);
                        if (c > '9')
                        {
                            c = '0';
                            lastDigitCheck = null;
                            carry = 1;
                        }
                        else
                        {
                            carry = 0;
                        }
                        s[q] = c;
                    }
                    fraction = new string(s) + fraction.Substring(digits);
                    if (carry != 0)
                    {
                        fraction = '1' + fraction;
                        exp10++;
                        digits++;
                        while (digits > 1 && fraction[digits - 1] == '0')
                        {
                            digits--;
                        }
                    }
                }
            }
            throw new InvalidProgramException("Sorry but the algorithm failed");
        }
    }
}
