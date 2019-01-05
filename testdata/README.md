## Test Data

The [input](input) directory contains files with non-canonicalized data which is
supposed be transformed as specified by the corresponding file in the
[output](output) directory.

## ES6 Numbers

For testing ES6 number serialization there is a file on 
https://onedrive.live.com/embed?cid=9341770E0D0D5468&resid=9341770E0D0D5468%21222&authkey=ADOClRsuPv3_pTk
containing about a 100 million of random and edge-case values.  The test file consists of lines
```code
hex-ieee,expected\n
```
where `hex-ieee` holds 1-16 hexadecimal characters representing an IEEE-754 double precision value
while `expected` holds the expected serialized value.  The lines are terminated by a new-line.
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
