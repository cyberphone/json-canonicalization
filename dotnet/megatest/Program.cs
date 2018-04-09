using System;
using System.IO;

using Org.Webpki.Es6Numbers;

// Internal test program for "roundtripping" 100M random + some specifically selected test values

namespace megatest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader("c:\\es6\\numbers\\es6testfile100m.txt"))
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                // Read lines with test values from the file until the end is reached. Each line contains
                //
                //        hhhh...,es6text\n
                //
                // where hhh... are IEEE-754 double precision values expressed as hexadecimal like 44b52d02c7e14af6
                // and es6text is the corresponding anticipated textual version like 1e+23
                // which is supposed to be the shortest "correct" way of expressing the number according to
                // the algorithm specified by ES6.
                //
                // Note: the hexadecimal number does not contain leading zeroes.
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                string line;
                long counter = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    string hex = line.Substring(0, line.IndexOf(','));
                    while (hex.Length < 16)
                    {
                        hex = '0' + hex;
                    }
                    double d = BitConverter.Int64BitsToDouble((long)Convert.ToUInt64(hex, 16));
                    string doubleInText = line.Substring(line.IndexOf(',') + 1);
                    if (++counter % 1000 == 0)
                    {
                        // Verify that we are not stuck in a loop somewhere...
                        Console.WriteLine("Hi" + counter);
                    }
                    if (doubleInText != ES6NumberFormatter.Format(d))
                    {
                        // Failure
                        Console.WriteLine(hex + " " + doubleInText);
                    }
                }
            }
        }
    }
}
