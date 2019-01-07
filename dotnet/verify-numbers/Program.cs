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
using System.IO;

using Org.Webpki.Es6NumberSerialization;

namespace verify_numbers
{
    // Test program for a .NET ES6 Number Serializer
    class Program
    {
        static int conversionErrors = 0;

        const string INVALID_NUMBER = "null";

        static void Verify(string ieeeHex, string expected)
        {
            while (ieeeHex.Length < 16)
            {
                ieeeHex = '0' + ieeeHex;
            }
            double ieeeF64 = BitConverter.Int64BitsToDouble((long)Convert.ToUInt64(ieeeHex, 16));
            try
            {
                String es6Created = NumberToJson.SerializeNumber(ieeeF64);
                if (!es6Created.Equals(expected))
                {
                    conversionErrors++;
                    Console.WriteLine("ES6={0,-24:S} C#={1,-24:S} Original=" + ieeeHex, expected, es6Created);
                }
                else
                {
                    if (ieeeF64 != double.Parse(expected, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        Console.WriteLine("ES6={0,-24:S} C#={1,-24:S} Original=" + ieeeHex, expected, es6Created);
                    }
                }
            }
            catch (ArgumentException)
            {
                if (!expected.Equals(INVALID_NUMBER))
                {
                    conversionErrors++;
                    Console.WriteLine("ES6={0,-24:S} Original=" + ieeeHex, expected);
                }
            }
        }

        static void Main(string[] args)
        {
            Verify("4340000000000001", "9007199254740994");
            Verify("4340000000000002", "9007199254740996");
            Verify("444b1ae4d6e2ef50", "1e+21");
            Verify("3eb0c6f7a0b5ed8d", "0.000001");
            Verify("3eb0c6f7a0b5ed8c", "9.999999999999997e-7");
            Verify("8000000000000000", "0");
            Verify("7fffffffffffffff", INVALID_NUMBER);
            Verify("7ff0000000000000", INVALID_NUMBER);
            Verify("fff0000000000000", INVALID_NUMBER);
            using (StreamReader sr = new StreamReader("c:\\es6\\numbers\\es6testfile100m.txt"))
            {
                string line;
                // Read test lines from the file until EOF is reached
                long lineCount = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    // Each line contains
                    //    Hexadecimal,Number\n
                    // where Hexadecimal is the IEEE-754 double precision
                    // equivalent of an optimal (ES6 compliant) Number
                    int comma = line.IndexOf(',');
                    Verify(line.Substring(0, comma), line.Substring(comma + 1));
                    if (++lineCount % 1000000 == 0)
                    {
                        Console.WriteLine("Line: " + lineCount);
                    }
                }
                if (conversionErrors == 0)
                {
                    Console.WriteLine("\nSuccessful Operation. Lines read: " + lineCount);
                }
                else
                {
                    Console.WriteLine("\nNumber of failures: " + conversionErrors);
                }
            }
        }
    }
}
