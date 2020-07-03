# JSON Canonicalizer for Java
This library implements the canonicalization scheme described in
the [core](https://github.com/cyberphone/json-canonicalization#json-canonicalization) of this repository.

A compiled JAR is available in the  [dist](dist) sub directory while the source is available in the [src](src) sub directory.

### Using the JSON canonicalizer

```java
import org.webpki.jcs.JsonCanonicalizer;

    JsonCanonicalizer jsonCanonicalizer = new JsonCanonicalizer(jsonString);
    String result = jsonCanonicalizer.getEncodedString();

```
The `JsonCanonicalizer()` may also be invoked with a `byte[]` array holding JSON data in UTF-8 format.

In addition to `getEncodedString()` there is a method `getEncodedUTF8()` returning canonicalized data as
a `byte[]` array.

### Constraints
The JSON canonicalizer only accepts a JSON _Object_ or _Array_ as the top level data type.

### ES6 Number Formatting
For formatting the JSON Number data type in an ES6 compliant way, there is a static utility method
(which is also used internally by the JSON canonicalizer):
```java
public static String NumberToJSON.serializeNumber(double value) throws IOException;
```
