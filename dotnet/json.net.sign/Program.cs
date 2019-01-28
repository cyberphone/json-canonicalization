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

        [JsonConverter(typeof(Int64Converter))]
        [JsonProperty("counter")]
        public long Counter { get; set; }

        [JsonConverter(typeof(UTCStrictDateConverter))]
        [JsonProperty("instant")]
        public DateTime Instant { get; set; }

        [JsonProperty("list")]
        public string[] List { get; set; }

        [JsonProperty("\u20ac")]
        public bool EuroIsGreat { get; set; }

        [JsonProperty("signature", Required = Required.Always)]
        public SignatureObject Signature { get; set; }
    }

    class Program
    {
        const long BIG_LONG = 1000000000007800000L;

        static void Main(string[] args)
        {
            DateTime Now = DateTime.Now;

            // Create an object instance
            MyObject myObject = new MyObject
            {
                Counter = BIG_LONG,
                Id = "johndoe",
                Instant = Now,
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
                MissingMemberHandling = MissingMemberHandling.Error, // Reject undeclared properties
                DateParseHandling = DateParseHandling.None           // Remove Json.NET's stupid default
            });
            if (receivedObject.Counter != BIG_LONG)
            {
                throw new ArgumentException("Long value error");
            }
            if (receivedObject.Instant.Ticks / 10000000 != Now.Ticks / 10000000)
            {
                throw new ArgumentException("Date value error");
            }

            // Verify signature
            Console.WriteLine("Signature verified=" + (Signature.Verify(receivedObject) != null));
        }
    }
}
