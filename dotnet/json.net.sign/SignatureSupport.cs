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

// Ultra-simple "Signed JSON" based on JCS and detached JWS

namespace json.net.signaturesupport
{
    public interface ISigned
    {
        string GetSignatureProperty();
    }

    public class JWSHeader
    {
        [JsonProperty("alg", Required = Required.Always)]
        public string Algorithm { get; internal set; }

        [JsonProperty("kid", Required = Required.Always)]
        public string KeyId { get; internal set; }
    }

    public static class Signature
    {
        public const string PROPERTY = "signature";
 
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
            String signatureProperty = "\"" + ((ISigned)obj).GetSignatureProperty() + "\"";
            foreach (var property in obj.GetType().GetProperties(BindingFlags.DeclaredOnly |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance))
            {
                foreach (var attribute in property.GetCustomAttributesData())
                {
                    if (attribute.AttributeType == typeof(JsonPropertyAttribute))
                    {
                        foreach (var argument in attribute.ConstructorArguments)
                        {
                            if (signatureProperty.Equals(argument.ToString()))
                            {
                                return property;
                            }
                        }
                    }
                }
            }
            throw new MemberAccessException("Property \"SignatureObject\" missing in: " + obj.GetType().ToString());
        }

        private static byte[] CanonicalizeObject(object obj)
        {
            return new JsonCanonicalizer(JsonConvert.SerializeObject(obj)).GetEncodedUTF8();
        }

        public static void Sign(object obj)
        {
            string payloadB64U = Base64UrlConverter.Encode(CanonicalizeObject(obj));
            // Create and initialize an empty signature object
            JWSHeader jwsHeader = new JWSHeader
            {
                Algorithm = ALGORITHM,
                KeyId = KEY_ID
            };
            string jwsHeaderB64U = Base64UrlConverter.Encode(
                new UTF8Encoding(false, true).GetBytes(JsonConvert.SerializeObject(jwsHeader)));
            string jwsString = jwsHeaderB64U + ".." + Base64UrlConverter.Encode(
                HmacObject(new UTF8Encoding(false, true).GetBytes(jwsHeaderB64U + "." + payloadB64U)));
             
            if (obj is List<object>)
            {
                // We are signing an array, append signature
                ((List<object>)obj).Add(jwsString);
            }
            else
            {
                // We are signing an object, assign signature to it
                GetSignatureProperty(obj).SetValue(obj, jwsString);
            }
        }

        public static bool Verify(object obj)
        {
            string jwsString;
            if (obj is List<object>)
            {
                // We are verifying a signed array, fetch the last element containing a JWS string
                jwsString = ((String)((List<object>)obj).Last());

                // After that the last element is removed
                ((List<object>)obj).Remove(((List<object>)obj).Last());
            }
            else
            {
                // We are verifying a signed object, get the JWS string
                jwsString = (String)GetSignatureProperty(obj).GetValue(obj);
 
                // After that set this element to 
                GetSignatureProperty(obj).SetValue(obj, null);
            }
            
            // Canonicalize the object - Payload to be signed
            string payloadB64U = Base64UrlConverter.Encode(CanonicalizeObject(obj));
            // Header - To be signed
            string jwsHeaderB64U = jwsString.Substring(0, jwsString.IndexOf('.'));

            JWSHeader jwsHeader = JsonConvert.DeserializeObject<JWSHeader>(
                new UTF8Encoding(false, true).GetString(Base64UrlConverter.Decode(jwsHeaderB64U)),
                new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error, // Reject undeclared properties
            });

            // Verify correctness of container
            if (!jwsHeader.Algorithm.Equals(ALGORITHM) || !jwsHeader.KeyId.Equals(KEY_ID))
            {
                throw new CryptographicException("Unexpected JWS header arguments: " +
                    JsonConvert.SerializeObject(jwsHeader));
            }

            // Fetch signature value
            byte[] signatureValue = Base64UrlConverter.Decode(jwsString.Substring(jwsString.LastIndexOf('.') + 1));

            // Data to be signed
            byte[] dataToBeSigned = new UTF8Encoding(false, true).GetBytes((jwsHeaderB64U + "." + payloadB64U));

            // Verify signature value
            return signatureValue.SequenceEqual(HmacObject(dataToBeSigned));
        }
    }
}
