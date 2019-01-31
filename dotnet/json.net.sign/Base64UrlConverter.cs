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

// Annotation argument for Base64Url encoding/decoding

namespace json.net.signaturesupport
{
    public class Base64UrlConverter : JsonConverter
    {
        public static string Encode(byte[] binary)
        {
            var output = Convert.ToBase64String(binary);
            return output.Split('=')[0].Replace('+', '-').Replace('/', '_');
        }

        public static byte[] Decode(string base64url)
        {
            base64url = base64url.Replace('-', '+').Replace('_', '/');
            switch (base64url.Length % 4)
            {
                case 0: break;
                case 2: base64url += "=="; break;
                case 3: base64url += "="; break;
                default: throw new ArgumentException("input", "Illegal base64url string!");
            }
            return Convert.FromBase64String(base64url);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Decode((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Encode((byte[])value));
        }
    }
}
