using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.Webpki.Json;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Class1 o = new Class1();
            Console.WriteLine("Kurt=" + o.kurt);
            object json = new JSONReader("[]").Read();
            Console.WriteLine("json=" + json.ToString());
            int i;
            int q = 3;
            string path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            while (--q > 0)
            {
                i = System.IO.Path.GetDirectoryName(path).LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                Console.WriteLine(path);
                if (i <= 0)
                {
                    throw new Exception("Strange file path");
                }
                path = System.IO.Path.GetDirectoryName(path).Substring(0, i);
            }
            string testData = System.IO.Path.Combine(path, "testdata");
            Console.WriteLine(testData);
            string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(testData, "input"));
            foreach (string file in files)
            {
                new JSONReader(new System.IO.StreamReader(file)).Read();
                System.Console.WriteLine(System.IO.Path.GetFileName(file));
            }
        }
    }
}
