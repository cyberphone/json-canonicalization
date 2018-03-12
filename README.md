# JSON Canonicalization

This JSON canonicalization concept works by building on:
- ECMAScript V6 serialization of data
- Lexical sorting of properties

### Sample Input:
```code
{
  "numbers": [1E30,4.50,6,2e-3,0.000000000000000000000000001],
  "escaping": "\u20ac$\u000F\u000aA'\u0042\u0022\u005c\\\"\/",
  "other":  [null, true, false]
}
```
### Expected Output:
```code
{"escaping":"€$\u000f\nA'B\"\\\\\"/","numbers":[1e+30,4.5,6,0.002,1e-27],"other":[null,true,false]}
```

Note: for platform interoperable canonicalization, the output **must** be converted to UTF-8 as well.
