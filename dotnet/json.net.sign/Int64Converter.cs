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
using System.Text.RegularExpressions;

using Newtonsoft.Json;

// Annotation argument for Int64 encoding/decoding

namespace json.net.signaturesupport
{
    public class Int64Converter : JsonConverter
    {
        static Regex INTEGER_PATTERN = new Regex("^(0|-?[1-9]+[0-9]*)$");

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)reader.Value;
            if (INTEGER_PATTERN.IsMatch(value))
            {
                return long.Parse(value);
            }
            throw new ArgumentException("Invalid int64 format: " + value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((long)value).ToString());
        }
    }
}
