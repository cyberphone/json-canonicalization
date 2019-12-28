## Test Data

The [input](input) directory contains files with non-canonicalized data which is
supposed be transformed as specified by the corresponding file in the
[output](output) directory.  In the [outhex](outhex) directory the expected
output is expressed in hexadecimal byte notation.

## ES6 Numbers

For testing ES6 number serialization there is a ZIP file on 
https://1drv.ms/u/s!AmhUDQ0Od0GTiXeAjaBJFLJlxyg0?e=HFG4Ao
containing about a 100 million of random and edge-case values.  The test file consists of lines
```code
hex-ieee,expected\n
```
where `hex-ieee` holds 1-16 ASCII hexadecimal characters representing an IEEE-754 double precision value
while `expected` holds the expected serialized value.  Each line is terminated by a single new-line character.
Sample lines:
```code
4340000000000001,9007199254740994
4340000000000002,9007199254740996
444b1ae4d6e2ef50,1e+21
3eb0c6f7a0b5ed8d,0.000001
3eb0c6f7a0b5ed8c,9.999999999999997e-7
8000000000000000,0
0,0
```
