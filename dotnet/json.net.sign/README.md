## Clear Text Signature Sample Solution

The sample is based on Newtonsoft's Json.NET API (nowadays recommended by Microsoft).  

The code is _deliberately_ simplistic
with respect to the cryptographic part (using a hard-coded algorithm and key),
while the rest is pretty universal.

For creating "signable" JSON objects, developers only need adding a `SignatureObject` property
with a JSON property name of their liking:
```c#
[JsonProperty("signature", Required = Required.Always)]
public SignatureObject Signature { get; set; }
```

Expected printout from the sample program:
```json
{
  "id": "johndoe",
  "counter": 3,
  "list": [
    "yes",
    "no"
  ],
  "€": true,
  "signature": {
    "alg": "HS256",
    "kid": "mykey",
    "val": "rlKLoCBExrwB7NaChPtQ3cAxr83eGdpLA7txrg49slw"
  }
}
```
Expected result line:
```code
Signature verified=True
```
## Signing JSON Arrays
The system also provides support for signing arrays:
```c#
using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using json.net.signaturesupport;

namespace json.net.sign
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an array instance
            List<object> jsonArray = new List<object>();
            jsonArray.Add(3);
            jsonArray.Add("johndoe");
            jsonArray.Add(true);
            jsonArray.Add(new string[] { "yes", "no" });

            // Sign array
            Signature.Sign(jsonArray);

            // Serialize array to JSON
            String json = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.WriteLine(json);

            // Recreate array using received JSON
            List<object> receivedObject = JsonConvert.DeserializeObject<List<object>>(json);

            // Verify signature
            Console.WriteLine("Signature verified=" + (Signature.Verify(receivedObject) != null));
        }
    }
}
```
Expected JSON printout from the sample above:
```json
[
  3,
  "johndoe",
  true,
  [
    "yes",
    "no"
  ],
  {
    "alg": "HS256",
    "kid": "mykey",
    "val": "FXlIAzZ6UUIAOoTYRI84FOwcNFw9tDEPQEd0ZlBWix4"
  }
]
```
