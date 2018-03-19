## ECMAScript Proposal: JSON.canonicalize()

`JSON.canonicalize()` copies all the functionality `JSON.stringify()` with one signficant change; properties (keys) are sorted.  For details on the sorting scheme turn to: [README.md](https://github.com/cyberphone/json-canonicalization/blob/master/README.md)

Implementation wise `JSON.canonicalize()` is expected to only require a couple of lines in the existing ECMAScipt `JSON` object, possibly something along the following:

```js
    // We have an 'Object'
    (canonicalizeMode ? Object.keys(object).sort() ?  Object.keys(object)).forEach((key) => {
```
