// ES6 based JSON canonicalizer
'use strict';
var canonicalize = function(object) {

    var buffer = '';
    serialize(object);
    return buffer;

    function serialize(object) {
        if (object !== null && typeof object === 'object') {
            if (Array.isArray(object)) {
                // Array - Maintain element order
                buffer += '[';
                let next = false;
                object.forEach((element) => {
                    if (next) {
                        buffer += ',';
                    }
                    next = true;
                    // Recursive call
                    serialize(element);
                });
                buffer += ']';
            } else {
                // Object - Sort properties before serializing
                buffer += '{';
                let next = false;
                Object.keys(object).sort().forEach((property) => {
                    if (next) {
                        buffer += ',';
                    }
                    next = true;
                    // Properties are just strings - Use ES6/JSON
                    buffer += JSON.stringify(property);
                    buffer += ':';
                    // Recursive call
                    serialize(object[property]);
                });
                buffer += '}';
            }
        } else {
            // Primitive data type - Use ES6/JSON
            buffer += JSON.stringify(object);
        }
    }
};

module.exports = canonicalize;
