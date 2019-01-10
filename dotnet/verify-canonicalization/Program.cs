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
using System.Linq;
using System.Text;

using Org.Webpki.JsonCanonicalizer;

// Test program for verifying the JsonCanonicalizer

namespace VerifyJsonCanonicalizer
{
    class Program
    {
        static string testData;

        static int failures = 0;

        static void PerformOneTest(string inputFilePath)
        {
            string fileName = Path.GetFileName(inputFilePath);
            byte[] actual = new JsonCanonicalizer(ArrayUtil.ReadFile(inputFilePath)).GetEncodedUTF8();
            byte[] recycled = new JsonCanonicalizer(actual).GetEncodedUTF8();
            byte[] expected = ArrayUtil.ReadFile(Path.Combine(Path.Combine(testData, "output"), fileName));
            StringBuilder utf8InHex = new StringBuilder("\nFile: ");
            utf8InHex.Append(fileName);
            int byteCount = 0;
            bool next = false;
            foreach (byte b in actual)
            {
                if (byteCount++ % 32 == 0)
                {
                    utf8InHex.Append('\n');
                    next = false;
                }
                if (next)
                {
                    utf8InHex.Append(' ');
                }
                next = true;
                utf8InHex.Append(((int)b).ToString("x02"));
            }
            Console.WriteLine(utf8InHex.Append('\n').ToString());
            if (!actual.SequenceEqual(expected) || !actual.SequenceEqual(recycled))
            {
                failures++;
                Console.WriteLine("THE TEST ABOVE FAILED!");
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            if (args.Length == 0)
            {
                // This code is based on the directory structure of the repository
                int q = 6;
                string path = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                while (--q > 0)
                {
                    int i = path.LastIndexOf(Path.DirectorySeparatorChar);
                    if (i <= 0)
                    {
                        throw new Exception("Strange file path");
                    }
                    path = path.Substring(0, i);
                }
                testData = Path.Combine(path, "testdata");
            }
            else
            {
                // Alternatively you may give the full path to the testdata folder
                testData = args[0];
            }
            foreach (string file in Directory.GetFiles(Path.Combine(testData, "input")))
            {
                PerformOneTest(file);
            }
            if (failures == 0)
            {
                Console.WriteLine("All tests succeeded!\n");
            }
            else
            {
                Console.WriteLine("\n****** ERRORS: " + failures + " *******\n");
            }
        }
    }
}
