using System;
using System.IO;
using System.Linq;

using Org.Webpki.Json;

namespace test
{
    class Program
    {
        static string testData;

        static void PerformOneTest(string inputFilePath)
        {
            string fileName = Path.GetFileName(inputFilePath);
            Console.WriteLine(fileName);
            byte[] actual = new JSONCanonicalizer(ArrayUtil.ReadFile(inputFilePath)).GetEncodedUTF8();
//            Console.WriteLine(new JSONCanonicalizer(ArrayUtil.ReadFile(inputFilePath)).GetEncodedString());
            byte[] expected = ArrayUtil.ReadFile(Path.Combine(Path.Combine(testData, "output"), fileName));
            if (!actual.SequenceEqual(expected))
            {
                Console.WriteLine("FAIL");
                Console.WriteLine(actual.Length);
                Console.WriteLine(expected.Length);
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
                    Console.WriteLine(path);
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
            Console.WriteLine(testData);
            string[] files = Directory.GetFiles(Path.Combine(testData, "input"));
            foreach (string file in files)
            {
                PerformOneTest(file);
            }
        }
    }
}
