## Clear Text Signature Sample Solution

The sample is based on Newtonsoft's Json.NET API (nowadays recommended by Microsoft).  

The code is _deliberately_ simplistic
with respect to the cryptographic part (using a hard-coded algorithm and key),
while the rest is pretty universal. The system uses a combination
of detached JWS and JCS.

For creating "signable" JSON objects, developers needs adding a signature property
with a JSON property name of their liking and extending the class with one
constant and one method:
```c#
public class MyObject : ISigned
{
    const String SIGNATURE_PROPERTY = "signature";
    
    // Other properties

    [JsonProperty(SIGNATURE_PROPERTY, NullValueHandling = NullValueHandling.Ignore)]
    public string Signature { get; set; }
    
    public string GetSignatureProperty()
    {
        return SIGNATURE_PROPERTY;
    }
}
```

Expected printout from the sample program:
```json
{
  "id": "johndoe",
  "counter": "1000000000007800000",
  "time": "2019-01-31T19:09:31Z",
  "list": [
    "yes",
    "no"
  ],
  "€": true,
  "amount": "3.56",
  "signature": "eyJhbGciOiJIUzI1NiIsImtpZCI6Im15a2V5In0..WWO6rMoEFG53COKfnR88rUIqHcWCTm1pyDOmq1hUfW8"
}
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
            Console.WriteLine("Signature verified=" + (Signature.Verify(receivedObject)));
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
  "eyJhbGciOiJIUzI1NiIsImtpZCI6Im15a2V5In0..bCtxgVj76sIcRgNjfaY3xoqc85fp5y0DppFYWkZoZfo"
]
```
