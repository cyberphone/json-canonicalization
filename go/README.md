## JSON Canonicalizer for Go

The [jsoncanonicalizer](src/webpki.org/jsoncanonicalizer)
folder contains the source code for a 
JCS (RFC 8785) compliant JSON canonicalizer written in Go.

### Building and testing

- Set GOPATH to this directory.

- For running `verify-numbers.go` you need to download a 3Gb+ file with test
data described in the root directory [testdata](../testdata).  This file can be stored in
any directory and requires updating the file path in `verify-numbers.go`.

- Perform the commands:
```code
$ cd test
$ go build webpki.org/jsoncanonicalizer
$ go run verify-canonicalization.go
$ go run verify-numbers.go
```


### Using the JSON canonicalizer

```go
import "webpki.org/jsoncanonicalizer"

func Transform(jsonData []byte) (result []byte, e error)
```
Note that both the input and the result is assumed to be in UTF-8 format.

### Constraints
The JSON canonicalizer only accepts a JSON _Object_ or _Array_ as the top level data type.
