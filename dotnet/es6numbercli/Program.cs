/*
 *  Copyright 2006-2019 WebPKI.org (http://webpki.org).
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
using Org.Webpki.Es6NumberSerialization;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("es6cli {xhhhhhhhhh | floating point number}");
                Environment.Exit(0);
            }
            string inData = args[0];
            double value;
            if (inData.StartsWith("x"))
            {
                string origIeeeHex = inData.Substring(1);
                while (origIeeeHex.Length < 16)
                {
                    origIeeeHex = '0' + origIeeeHex;
                }
                ulong origBin = Convert.ToUInt64(origIeeeHex, 16);
                value = BitConverter.Int64BitsToDouble((long)origBin);
            }
            else
            {
                value = double.Parse(inData, System.Globalization.CultureInfo.InvariantCulture);
            }
            string es6 = NumberToJson.SerializeNumber(value);
            ulong ieeeLong = (ulong)BitConverter.DoubleToInt64Bits(value);
            ulong ulongMask = 0x8000000000000000L;
            string binary = "";
            for (int counter = 0; counter < 64; counter++)
            {
                binary += (ulongMask & ieeeLong) == 0 ? '0' : '1';
                ulongMask >>= 1;
                if (counter == 0 || counter == 11)
                {
                    binary += ' ';
                }
            }
            string hex = ieeeLong.ToString("x16");
            Console.WriteLine("G17=" + value.ToString("G17") + " Hex=" + hex + " ES6=" + es6 + "\nBinary=" + binary);
        }
    }
}

