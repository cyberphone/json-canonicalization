JCS support for Go
==================

1. Setup
--------
Set GOPATH to this directory

On Windows perform "CHCP 65001" to set the console in UTF-8 mode


2. Running
----------
$ cd test
$ go build webpki.org/es6numfmt
$ go build webpki.org/jcs
$ go run verify-canonicalization.go
$ go run verify-numbers.go
