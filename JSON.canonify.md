## ECMAScript Proposal: JSON.canonify()

`JSON.canonify()` would inherit all the functionality of `JSON.stringify()` with one signficant change: properties (keys) are *sorted*.
For details on the sorting scheme turn to: https://github.com/cyberphone/json-canonicalization#json-canonicalization

`JSON.canonify()` is expected to require very modest changes to existing ECMAScript `JSON` object implementations; possibly something along the following:

```js
    // We are about to serialize an 'Object'
    (canonicalizeMode ? Object.keys(object).sort() :  Object.keys(object)).forEach((key) => {
```
