JCS support for Go
==================

1. Setup
--------
Set GOPATH to this directory

For running verify-numbers.go you need to download a 3Gb+ file with test
data described in the root directory testdata.  This file can be stored in
any directory and requires updating the file path in verify-numbers.go


2. Running
----------
$ cd test
$ go build webpki.org/es6numfmt
$ go build webpki.org/jsoncanonicalizer
$ go run verify-canonicalization.go
$ go run verify-numbers.go


3. API

import "webpki.org/jsoncanonicalizer"

func Transform(jsonData []byte) (result string, e error)

jsonData must be UTF-8 encoded
the resulting string is also UTF-8 encoded
