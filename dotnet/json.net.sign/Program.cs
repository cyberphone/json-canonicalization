using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using json.net.signaturesupport;

namespace json.net.sign
{
    public class MyObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("counter")]
        public long Counter { get; set; }

        [JsonProperty("list")]
        public string[] List { get; set; }

        [JsonProperty("\u20ac")]
        public bool EuroIsGreat { get; set; }

        [JsonProperty("signature", Required = Required.Always)]
        public SignatureObject Signature { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create an object instance
            MyObject myObject = new MyObject
            {
                Counter = 3,
                Id = "johndoe",
                EuroIsGreat = true,
                List = new string[] { "yes", "no" }
            };

            // Sign object
            Signature.Sign(myObject);

            // Serialize object to JSON
            String json = JsonConvert.SerializeObject(myObject, Formatting.Indented);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine(json);

            // Recreate object using received JSON
            MyObject receivedObject = JsonConvert.DeserializeObject<MyObject>(json, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error // Reject undeclared properties
            });

            // Verify signature
            Console.WriteLine("Signature verified=" + Signature.Verify(receivedObject));
        }
    }
}
