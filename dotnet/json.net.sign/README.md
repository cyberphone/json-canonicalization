## Clear Text Signature Sample Solution

The sample is based on Newtonsoft's Json.NET library.  

The code is (deliberately) simplistic
with respect to the cryptographic part (using a hard-coded algorithm and key),
while the rest is pretty universal.

For creating "signable" JSON objects, developers only need adding a `SignatureObject` property
with a JSON property name of their preference:
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
