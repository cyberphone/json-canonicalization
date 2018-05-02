using System;
using System.IO;

using Org.Webpki.Es6NumberSerialization;

namespace verify_es6numberserialization
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader sr = new StreamReader("c:\\es6\\numbers\\es6testfile100m.txt"))
            {
                string line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                long counter = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    string origIeeeHex = line.Substring(0, line.IndexOf(','));
                    while (origIeeeHex.Length < 16)
                    {
                        origIeeeHex = '0' + origIeeeHex;
                    }
                    ulong origBin = Convert.ToUInt64(origIeeeHex, 16);
                    double orig = BitConverter.Int64BitsToDouble((long)origBin);
                    string es6Representation = line.Substring(line.IndexOf(',') + 1);
                    if (++counter % 100000 == 0)
                    {
                        Console.WriteLine("Count=" + counter);
                    }
                    String serializedNumber = NumberToJSON.SerializeNumber(orig);
                    if (!serializedNumber.Equals(es6Representation))
                    {
                        Console.WriteLine("ES6={0,-24:S} C#={1,-24:S} Original=" + origIeeeHex,
                                          es6Representation, serializedNumber);
                        NumberToJSON.SerializeNumber(orig);
                    }
                }
            }
        }
    }
}
