// ES6 JSON Canonicalizer
'use strict';
const CanonicalizedJSON = {

    stringify: function(object) {

        var buffer = '';
        serialize(object);
        return buffer;

        function serialize(object) {
            if (object !== null && typeof object === 'object') {
                if (Array.isArray(object)) {
                    buffer += '[';
                    let next = false;
                    object.forEach((element) => {
                        if (next) {
                            buffer += ',';
                        }
                        next = true;
                        serialize(element);
                    });
                    buffer += ']';
                } else {
                    buffer += '{';
                    let next = false;
                    Object.keys(object).sort().forEach((property) => {
                        if (next) {
                            buffer += ',';
                        }
                        next = true;
                        buffer += JSON.stringify(property);
                        buffer += ':';
                        serialize(object[property]);
                    });
                    buffer += '}';
                }
            } else {
                buffer += JSON.stringify(object);
            }
        }
    }
};

module.exports = CanonicalizedJSON;
