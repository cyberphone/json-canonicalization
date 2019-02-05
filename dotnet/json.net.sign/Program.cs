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
    public class MyObject : ISigned
    {
        const String SIGNATURE_PROPERTY = "signature";

        [JsonProperty("id")]
        public string Id;

        // Normalized data solution
        [JsonConverter(typeof(Int64Converter))]
        [JsonProperty("counter")]
        public long Counter;

        // Normalized data solution
        [JsonConverter(typeof(UTCStrictDateConverter))]
        [JsonProperty("time")]
        public DateTime Time;

        [JsonProperty("list")]
        public string[] List;

        [JsonProperty("\u20ac")]
        public bool EuroIsGreat;

        // The immutable string method uses a local
        // string variable for JSON serialization while
        // exposing another type to the application
        [JsonProperty("amount")]
        private string _amount;

        [JsonIgnore]
        public decimal Amount {
            get { return decimal.Parse(_amount); }
            set { _amount = value.ToString(); }
        }

        [JsonProperty(SIGNATURE_PROPERTY, NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }

        public string GetSignatureProperty()
        {
            return SIGNATURE_PROPERTY;
        }
    }

    class Program
    {
        const long BIG_LONG = 1000000000007800000L;

        const decimal PAY_ME = 3.56m;

        static void Main(string[] args)
        {
            DateTime Now = DateTime.Now;

            // Create an object instance
            MyObject myObject = new MyObject
            {
                Counter = BIG_LONG,
                Id = "johndoe",
                Time = Now,
                EuroIsGreat = true,
                Amount = PAY_ME,
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

            // Check data for correctness
            if (receivedObject.Counter != BIG_LONG)
            {
                throw new ArgumentException("Long value error");
            }
            if (receivedObject.Time.Ticks / 10000000 != Now.Ticks / 10000000)
            {
                throw new ArgumentException("Date value error");
            }
            if (receivedObject.Amount != PAY_ME)
            {
                throw new ArgumentException("Decimal value error");
            }

            // Verify signature
            Console.WriteLine("Signature verified=" + (Signature.Verify(receivedObject)));
        }
    }
}
