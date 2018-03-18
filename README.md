# JSON Canonicalization

Also see: https://cyberphone.github.io/doc/security/draft-rundgren-json-canonicalization-scheme.html

This JSON canonicalization concept works by building on:
- Serialization of primitive JSON data types using methods compatible with ECMAScript's `JSON.stringify()`
- Lexical sorting of `Object` properties in a *recursive* process
- Array data is also subject to canonicalization, *but element order remains untouched*

Note: The sorting algorithm depends on that strings are represented as arrays of
16-bit unsigned integers where each integer holds a *single* UCS2/UTF-16 code unit. 
The sorting is based on pure value comparisons, *independent of locale settings*.

### Lexical Ordering

Property strings either have different characters at some index that is a valid
index for both strings, or their lengths are different, or both. If they have
different characters at one or more index positions, let k be the smallest such index;
then the string whose character at position k has the smaller value, as determined by
using the < operator, lexically precedes the other string.

If there is no index position at which they differ, then the shorter string
lexically precedes the longer string.

### Sample Input:
```code
{
  "numbers": [1E30, 4.50, 6, 2e-3, 0.000000000000000000000000001],
  "escaping": "\u20ac$\u000F\u000aA'\u0042\u0022\u005c\\\"\/",
  "other":  [null, true, false]
}
```
### Expected Output:
```code
{"escaping":"€$\u000f\nA'B\"\\\\\"/","numbers":[1e+30,4.5,6,0.002,1e-27],"other":[null,true,false]}
```

Note: for platform interoperable canonicalization, the output must be converted to UTF-8
as well, here shown in hexadecimal notation:

```code
7b 22 65 73 63 61 70 69 6e 67 22 3a 22 e2 82 ac 24 5c 75 30 30 30 66 5c 6e 41 27 42 5c 22 5c 5c
5c 5c 5c 22 2f 22 2c 22 6e 75 6d 62 65 72 73 22 3a 5b 31 65 2b 33 30 2c 34 2e 35 2c 36 2c 30 2e
30 30 32 2c 31 65 2d 32 37 5d 2c 22 6f 74 68 65 72 22 3a 5b 6e 75 6c 6c 2c 74 72 75 65 2c 66 61
6c 73 65 5d 7d
```

### On-line Browser Sample
https://cyberphone.github.io/doc/security/browser-json-canonicalization.html


### Other Canonicalization Efforts
https://www.npmjs.com/package/canonical-json

https://tools.ietf.org/html/draft-staykov-hu-json-canonical-form-00

http://wiki.laptop.org/go/Canonical_JSON

https://gibson042.github.io/canonicaljson-spec/

https://gist.github.com/mikesamuel/20710f94a53e440691f04bf79bc3d756
