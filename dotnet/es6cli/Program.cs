using System;

using Org.Webpki.Es6Numbers;

namespace test
{
    class Program
    {
        static void ShowDouble(string j)
        {
            double d = double.Parse(j);
            String res = ES6NumberFormatter.Format(d);
            Console.WriteLine("d=" + d.ToString("G17") + " orig=" + j + " res=" + (res == j) + " es6=" + res);
        }

        static void Main(string[] args)
        {
            ShowDouble("4.3413853813852797e+192");
            ShowDouble("4.34138538138528e+192");
            ShowDouble("6.894058331974955e-160");
            ShowDouble("-3333333.3333333335");
            ShowDouble("-0.3333333333333333");
            ShowDouble("-996809979449669.2");
            ShowDouble("1e+23");
            ShowDouble("9.999999999999997e+22");
            ShowDouble("-5e-324");
            ShowDouble("9007199254740991");
            ShowDouble("999999999999999700000");
            ShowDouble("333333333.33333325");
            ShowDouble("1.7976931348623157e+308");
            ShowDouble("1.0000000000000001e+23");
            ShowDouble("10");
            ShowDouble("1");
            ShowDouble("1.0000000000000002");
        }
    }
}

