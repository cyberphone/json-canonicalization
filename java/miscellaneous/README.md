### Java Test Programs

This directory contains a number of test programs for the canonicalizer stored in the sibling directory.

Note that `verify-numbers` require download of a 3Gb+ file with testdata.
See root directory `testdata` for details.
This file can be stored in any suitable place.
After that the file `webpki.properties` must be updated with the actual path. 

```code
$ ant verify-canonicalization
$ ant verify-numbers
```
