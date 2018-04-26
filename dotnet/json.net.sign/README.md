### Miniscule Clear Text Signature Solution

Expected printout:
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
Result line:
```code
Signature verified=True
```
