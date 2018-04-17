using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;

using Org.Webpki.Json;

namespace datacontract
{
    class Program
    {
        static byte[] SECRET_KEY = { 0xF4, 0xC7, 0x4F, 0x33,
                                     0x98, 0xC4, 0x9C, 0xF4,
                                     0x6D, 0x93, 0xEC, 0x98,
                                     0x18, 0x83, 0x26, 0x61,
                                     0xA4, 0x0B, 0xAE, 0x4D,
                                     0x20, 0x4D, 0x75, 0x50,
                                     0x36, 0x14, 0x10, 0x20,
                                     0x74, 0x34, 0x69, 0x09 };

        static byte[] HmacObject(byte[] data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(SECRET_KEY))
            {
                using (MemoryStream inStream = new MemoryStream(data))
                {
                    return hmac.ComputeHash(inStream);
                }
            }
        }

        static byte[] SignAndSend(MyObject myObject)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MyObject));
            // Use the WriteObject method to write JSON data to the stream.
            ser.WriteObject(stream1, myObject);
            // Show the JSON output.
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine("JSON DataContractJsonSerializer write:");
            Console.WriteLine(sr.ReadToEnd());
            stream1.Position = 0;
            byte[] dataToBeCanonicalized = new JSONCanonicalizer(sr).GetEncodedUTF8();
            Console.WriteLine("Canonicalizer write:");
            Console.WriteLine(new UTF8Encoding().GetString(dataToBeCanonicalized));
            myObject.hmac = Convert.ToBase64String(HmacObject(dataToBeCanonicalized));
            stream1.Position = 0;
            ser.WriteObject(stream1, myObject);
            stream1.Position = 0;
            sr = new StreamReader(stream1);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine("JSON DataContractJsonSerializer updated write:");
            Console.WriteLine(sr.ReadToEnd());
            stream1.Position = 0;
            StreamWriter sw = new StreamWriter(stream1);
            return stream1.ToArray();
        }

        static void OneRoundTrip(MyObject myObject)
        {
            byte[] receivedJsonData = SignAndSend(myObject);

            // Now deserialize and verify
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MyObject));
            MyObject readObject = (MyObject)ser.ReadObject(new MemoryStream(receivedJsonData));
            byte[] hmac = Convert.FromBase64String(readObject.hmac);
            readObject.hmac = null;
            stream1.Position = 0;
            ser.WriteObject(stream1, readObject);
            // Show the JSON output.
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine("JSON DataContractJsonSerializer read:");
            Console.WriteLine(sr.ReadToEnd());
            stream1.Position = 0;
            if (hmac.SequenceEqual(HmacObject(new JSONCanonicalizer(sr).GetEncodedUTF8())))
            {
                Console.WriteLine("SUCCESS");
            }
            else
            {
                Console.WriteLine("FAIL");
            }
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
