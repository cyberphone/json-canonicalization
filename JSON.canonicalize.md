## ECMAScript Proposal: JSON.canonicalize()

`JSON.canonicalize()` would copy all the functionality of `JSON.stringify()` with one signficant change; properties (keys) are *sorted*.  For details on the sorting scheme turn to: [README.md](https://github.com/cyberphone/json-canonicalization/blob/master/README.md)

Implementation wise `JSON.canonicalize()` is expected to require very modest changes to the existing ECMAScript `JSON` object; possibly something along the following:

```js
    // We have an 'Object'
    (canonicalizeMode ? Object.keys(object).sort() ?  Object.keys(object)).forEach((key) => {
```
