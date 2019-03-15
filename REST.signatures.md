## REST Signatures

Since there is no standard for signing REST requests, the industry is currently using
various proprietary solutions mostly based on HTTP bindings and detached data.
This note outlines a simple REST signature scheme.

### Unsigned REST Requests

Assume you have REST system using URIs for holding parameters as well as optionally using the HTTP Body as holder of additional data.

A REST HTTP request could then look like:

```code
POST /foo HTTP/1.1
Host: example.com
Content-Type: application/json
Content-Length: 1234
{
  "something": "data",

      Additional application specific properties

}
```

In this scenario the request is qualified by:
-	The URI
-	The HTTP Verb
-	The HTTP Body

### Adding a Signature
The following is a modified HTTP Body providing a signed counterpart:

```code
{
  "something": "data",

      Additional application specific properties

  ".secinf": {
    "uri": "https://example.com/foo",
    "mtd": "POST",
    "iat": 1551709923,
    "jws": "eyJhbGciOiJIUzI1NiJ9..VHVItCBCb849imarDtjw4"
  }
}
```
The argument to `jws` would preferably be a JWS in "detached" mode as described in:<br>
https://tools.ietf.org/html/rfc7515#appendix-F

Before signing, the JWS "payload" (all but the `jws` element), would pass through JCS
(https://tools.ietf.org/html/draft-rundgren-json-canonicalization-scheme-05)
to make the signature insensitive
to whitespace handling and property ordering as well as to *JSON compliant* variances in string and
number formatting.


### Summary
The depicted signature scheme accomplishes the following:
- Signs the core parameters of a REST request
- Maintains clear text messaging using JSON
- Supports proxying and storage in databases without signature breakage
- Enables embedding in other JSON objects for counter signing etc.

v0.1
