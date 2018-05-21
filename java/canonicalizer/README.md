# JSON Canonicalizer for Java
This library implements the canonicalization scheme described in
https://tools.ietf.org/html/draft-rundgren-json-canonicalization-scheme-00
as well as in the [core](../../../) of this repository.

A compiled JAR is available in the  [dist](dist) sub directory while the source is available in the [src](src) sub directory.

Using the JSON canonicalizer:

```java
import org.webpki.jcs.JsonCanonicalizer;

    JsonCanonicalizer jsonCanonicalizer = new JsonCanonicalizer(jsonString);
    String result = jsonCanonicalizer.getEncodedString();

```
The `JsonCanonicalizer()` may also be invoked with a `byte[]` array holding JSON data in UTF-8 format.

In addition to `getEncodedString()` there is a method `getEncodedUTF8()` returning canonicalized data as
a `byte[]` array.

For formatting the JSON Number data type in an ES6 compliant way, there is a static utility method:
```java
public String org.webpki.jcs.NumberToJSON.serializeNumber(double value) throws IOException;
```
