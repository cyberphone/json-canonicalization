using System;
using System.IO;
using System.Linq;
using System.Text;

using Org.Webpki.Json;

// Test program for verifying the JSON canonicalizer

namespace test
{
    class Program
    {
        static string testData;

        static void PerformOneTest(string inputFilePath)
        {
            string fileName = Path.GetFileName(inputFilePath);
            byte[] actual = new JSONCanonicalizer(ArrayUtil.ReadFile(inputFilePath)).GetEncodedUTF8();
            byte[] expected = ArrayUtil.ReadFile(Path.Combine(Path.Combine(testData, "output"), fileName));
            StringBuilder utf8InHex = new StringBuilder("\nFile: ");
            utf8InHex.Append(fileName).Append('\n');
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
            if (!actual.SequenceEqual(expected))
            {
                Console.WriteLine("Failed:\n" + new UTF8Encoding().GetString(actual));
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // This code is based on the directory structure of the repository
                int i;
                int q = 3;
                string path = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                while (--q > 0)
                {
                    i = Path.GetDirectoryName(path).LastIndexOf(Path.DirectorySeparatorChar);
                    if (i <= 0)
                    {
                        throw new Exception("Strange file path");
                    }
                    path = Path.GetDirectoryName(path).Substring(0, i);
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
        }
    }
}
