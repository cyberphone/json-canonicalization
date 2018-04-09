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
        static internal Regex NUMBER_FORMAT = new Regex("-?[0-9]+(\\.[0-9]+)?([eE][-+]?[0-9]+)?");

        static internal char[] EXPONENT_LETTERS = {'e', 'E'};

        // Number format: d{f...}   Note: ZERO is not permitted, "d" must be 1-9 
        internal static double Parse(bool sign, string digitAndOptionalFraction, int exponent)
        {
            return 0;
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
                while (point > 1)
                {
                    point--;
                    exponent++;
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
