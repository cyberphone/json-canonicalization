using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.Webpki.Es6Numbers;

namespace datacontract
{
    class Program
    {
        static private void OneRoundTrip(MyObject myObject)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MyObject));
            // Use the WriteObject method to write JSON data to the stream.
            ser.WriteObject(stream1, myObject);
            // Show the JSON output.
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Write("JSON DataContractJsonSerializer write: ");
            Console.WriteLine(sr.ReadToEnd());
            stream1.Position = 0;
            byte[] sentData = new JSONCanonicalizer(sr).GetEncodedUTF8();
            Console.WriteLine(new UTF8Encoding().GetString(sentData));

            // Now deserialize
            ser = new DataContractJsonSerializer(typeof(MyObject));
            MyObject readObject = (MyObject)ser.ReadObject(new MemoryStream(sentData));
            Console.WriteLine("Success=" + myObject.Equals(readObject));
        }

        static void Main(string[] args)
        {
            MyObject myObject = new MyObject();
            myObject.escaping = "\u20ac$\u000F\u000aA'\u0042\u0022\u005c\\\"";
            myObject.aDouble = 1.5e+33;
            myObject.interoperableLong = 9223372036854775807;     // Is treated as a "string" on the wire
            myObject.nonInteroperableLong = 9007199254740991;     // Max integer fitting an IEEE-754 double
            OneRoundTrip(myObject);

            myObject.nonInteroperableLong = 9223372036854775807;  // Max "long" doesn't fit an IEEE-754 double
            // ***Throws an exception because the value is expressed in scientific notation***
            OneRoundTrip(myObject);
        }
    }
}
