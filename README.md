# JSON Canonicalization

Cryptographic operations like hashing and signing depend on that the target 
data does not change during serialization, transport, or parsing. 
By applying the rules defined by JCS (JSON Canonicalization Scheme), 
data provided in the JSON [[RFC8259](https://tools.ietf.org/html/rfc8259)]
format can be exchanged "as is", while still being subject to secure cryptographic operations.
JCS achieves this by combining the JSON serialization
methods defined in ECMAScript [[ES6](https://www.ecma-international.org/ecma-262/6.0/index.html)],
with an elementary sorting scheme.

Specification: https://cyberphone.github.io/doc/security/draft-rundgren-json-canonicalization-scheme.html

The JSON Canonicalization Scheme concept in a nutshell:
- Serialization of primitive JSON data types using methods compatible with ECMAScript's `JSON.stringify()`
- Lexicographic sorting of `Object` properties in a *recursive* process
- Array data is also subject to canonicalization, *but element order remains untouched*

### Sample Input:
```code
{
  "numbers": [333333333.33333329, 1E30, 4.50, 2e-3, 0.000000000000000000000000001],
  "string": "\u20ac$\u000F\u000aA'\u0042\u0022\u005c\\\"\/",
  "literals": [null, true, false]
}
```
### Expected Output:
```code
{"literals":[null,true,false],"numbers":[333333333.3333333,1e+30,4.5,0.002,1e-27],"string":"€$\u000f\nA'B\"\\\\\"/"}
```

Note: for platform interoperable canonicalization, the output must be converted to UTF-8
as well, here shown in hexadecimal notation:

```code
7b 22 6c 69 74 65 72 61 6c 73 22 3a 5b 6e 75 6c 6c 2c 74 72 75 65 2c 66 61 6c 73 65 5d 2c 22 6e
75 6d 62 65 72 73 22 3a 5b 33 33 33 33 33 33 33 33 33 2e 33 33 33 33 33 33 33 2c 31 65 2b 33 30
2c 34 2e 35 2c 30 2e 30 30 32 2c 31 65 2d 32 37 5d 2c 22 73 74 72 69 6e 67 22 3a 22 e2 82 ac 24
5c 75 30 30 30 66 5c 6e 41 27 42 5c 22 5c 5c 5c 5c 5c 22 2f 22 7d
```

### On-line Browser Sample
https://cyberphone.github.io/doc/security/browser-json-canonicalization.html

### ECMAScript Proposal: JSON.canonicalize()
[JSON.canonicalize()](https://github.com/cyberphone/json-canonicalization/blob/master/JSON.canonicalize.md)

### NPM Compatible With JCS:
https://www.npmjs.com/package/canonicalize

### Other Canonicalization Efforts
https://tools.ietf.org/html/draft-staykov-hu-json-canonical-form-00

http://wiki.laptop.org/go/Canonical_JSON

https://gibson042.github.io/canonicaljson-spec/

https://gist.github.com/mikesamuel/20710f94a53e440691f04bf79bc3d756
