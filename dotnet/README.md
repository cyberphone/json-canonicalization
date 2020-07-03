## JSON Canonicalizer for .NET

The [jsoncanonicalizer](jsoncanonicalizer)
folder contains the source code for a JCS (RFC 8785) compliant
canonicalizer written in C# for .NET&nbsp;Core&nbsp;3 and forward.

### Using the JSON canonicalizer

```c#
using Org.Webpki.JsonCanonicalizer;

    JsonCanonicalizer jsonCanonicalizer = new JsonCanonicalizer(jsonString);
    string result = jsonCanonicalizer.GetEncodedString();

```
The `JsonCanonicalizer()` may also be invoked with a `byte[]` array holding JSON data in UTF-8 format.

In addition to `GetEncodedString()` there is a method `GetEncodedUTF8()` returning canonicalized data as
a `byte[]` array.

### Constraints
The JSON canonicalizer only accepts a JSON _Object_ or _Array_ as the top level data type.
