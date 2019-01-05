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
            Console.WriteLine("Signature verified=" + (Signature.Verify(receivedObject) != null));
        }
    }
}
