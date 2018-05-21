# JSON Canonicalizer for Java
This library implements the canonicalization scheme described in:
https://tools.ietf.org/html/draft-rundgren-json-canonicalization-scheme-00
as well as in the [core](../../../) of this repository.

A compiled JAR is available in the `dist` sub directory while the source is available in the `src` sub directory.

The JSON canonicalizer is used as follows:

```java
import org.webpki.jcs.JsonCanonicalizer;

    JsonCanonicalizer jsonCanonicalizer = new JsonCanonicalizer(jsonString);
    String result = jsonCanonicalizer.getEncodedString();

```
