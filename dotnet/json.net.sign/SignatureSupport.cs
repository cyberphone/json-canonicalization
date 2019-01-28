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
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Org.Webpki.JsonCanonicalizer;

// Ultra-simple "Signed JSON" based on Canonicalization

namespace json.net.signaturesupport
{
    public class SignatureObject
    {
        [JsonProperty("alg", Required = Required.Always)]
        public string Algorithm { get; internal set; }

        [JsonProperty("kid", Required = Required.Always)]
        public string KeyId { get; internal set; }

        [JsonConverter(typeof(Base64UrlConverter))]
        [JsonProperty("val", Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public byte[] SignatureValue { get; internal set; }
    }

    public static class Signature
    {
        const string ALGORITHM = "HS256";

        const string KEY_ID = "mykey";

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

        private static PropertyInfo GetSignatureProperty(object obj)
        {
            foreach (var property in obj.GetType().GetProperties(BindingFlags.DeclaredOnly |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance))
            {
                if (property.PropertyType == typeof(SignatureObject))
                {
                    return property;
                }
            }
            throw new MemberAccessException("Property \"SignatureObject\" missing in: " + obj.GetType().ToString());
        }

        private static byte[] CanonicalizeObject(object obj)
        {
            return new JsonCanonicalizer(JsonConvert.SerializeObject(obj)).GetEncodedUTF8();
        }

        public static SignatureObject Sign(object obj)
        {
            // Create and initialize an empty signature object
            SignatureObject signatureObject = new SignatureObject
            {
                Algorithm = ALGORITHM,
                KeyId = KEY_ID
            };

            if (obj is List<object>)
            {
                // We are signing an array, append signature
                ((List<object>)obj).Add(signatureObject);
            }
            else
            {
                // We are signing an object, assign signature to it
                GetSignatureProperty(obj).SetValue(obj, signatureObject);
            }

            // Canonicalize the completed object
            byte[] canonicalizedUtf8 = CanonicalizeObject(obj);

            // Finally add the signature value to the signature object
            signatureObject.SignatureValue = HmacObject(canonicalizedUtf8);
            return signatureObject;
        }

        public static SignatureObject Verify(object obj)
        {
            SignatureObject signatureObject;
            if (obj is List<object>)
            {
                // We are verifying a signed array, fetch the last element
                JObject jobject = (JObject)((List<object>)obj).Last();

                // Since the deserializer does not know what a SignatureObject is,
                // it returs a generic object which we remap to a SignatureObject.
                signatureObject = jobject.ToObject<SignatureObject>();

                // Finally, the last element is replaced by the true SignatureObject.
                ((List<object>)obj).Remove(jobject);
                ((List<object>)obj).Add(signatureObject);
            }
            else
            {
                // We are verifying a signed object, get the signature object
                signatureObject = (SignatureObject)GetSignatureProperty(obj).GetValue(obj);
            }

            // Verify correctness of container
            if (!signatureObject.Algorithm.Equals(ALGORITHM) || !signatureObject.KeyId.Equals(KEY_ID))
            {
                throw new CryptographicException("Unexpected \"SignatureObject\" arguments: " +
                    JsonConvert.SerializeObject(signatureObject));
            }

            // Fetch signature value
            byte[] signatureValue = signatureObject.SignatureValue;

            // Hide signature value from the serializer
            signatureObject.SignatureValue = null;

            // Canonicalize the object - signature value
            byte[] canonicalizedUtf8 = CanonicalizeObject(obj);

            // Restore signature object
            signatureObject.SignatureValue = signatureValue;

            // Verify signature value
            return signatureValue.SequenceEqual(HmacObject(canonicalizedUtf8)) ? signatureObject : null;
        }
    }
}
