# JSON Canonicalization

This JSON canonicalization concept works by building on:
- ECMAScript V6 serialization of primitive JSON data types
- Lexical sorting of properties in a *recursive* process
- Array data is also included in the process *but element order remains untouched*

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

Note: for platform interoperable canonicalization, the output **must** be converted to UTF-8
as well, here shown in hexadecimal notation:

```code
7b 22 65 73 63 61 70 69 6e 67 22 3a 22 e2 82 ac 24 5c 75 30 30 30 66 5c 6e 41 27 42 5c 22 5c 5c
5c 5c 5c 22 2f 22 2c 22 6e 75 6d 62 65 72 73 22 3a 5b 31 65 2b 33 30 2c 34 2e 35 2c 36 2c 30 2e
30 30 32 2c 31 65 2d 32 37 5d 2c 22 6f 74 68 65 72 22 3a 5b 6e 75 6c 6c 2c 74 72 75 65 2c 66 61
6c 73 65 5d 7d
```
