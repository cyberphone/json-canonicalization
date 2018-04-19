using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;

using Org.Webpki.Json;

// Ultra-simple "Signed JSON" based on Canonicalization

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

        static byte[] SerializeObject(MyObject myObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MyObject));
                ser.WriteObject(stream, myObject);
                return stream.ToArray();
            }
        }

        static void LogData(string what, byte[] utf8)
        {
            using (MemoryStream stream = new MemoryStream(utf8))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    Console.OutputEncoding = System.Text.Encoding.Unicode;
                    Console.WriteLine(what);
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }

        static byte[] SignAndSend(MyObject myObject)
        {
            byte[] originalData = SerializeObject(myObject);

            LogData("Raw JSON data:", originalData);

            byte[] canonicalizedData = new JSONCanonicalizer(originalData).GetEncodedUTF8();

            LogData("Canonicalized data", canonicalizedData);

            myObject.hmac = Convert.ToBase64String(HmacObject(canonicalizedData));

            byte[] dataToBeSent = SerializeObject(myObject);

            LogData("Signed JSON Data", dataToBeSent);

            return dataToBeSent;
        }

        static void OneRoundTrip(MyObject myObject)
        {
            byte[] receivedData = SignAndSend(myObject);

            // Now deserialize and verify
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MyObject));
            MyObject readObject = (MyObject)ser.ReadObject(new MemoryStream(receivedData));
            byte[] hmac = Convert.FromBase64String(readObject.hmac);
            readObject.hmac = null;

            byte[] originalData = SerializeObject(readObject);

            LogData("Received raw JSON data", originalData);

            if (hmac.SequenceEqual(HmacObject(new JSONCanonicalizer(originalData).GetEncodedUTF8())))
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
            // Signature-wise fails due to incompatible JSON number handling 
            OneRoundTrip(myObject);
        }
    }
}
