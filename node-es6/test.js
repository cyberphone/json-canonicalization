// JavaScript source code
'use strict';
const Fs = require('fs');
const JWC = require('./canonicalize.js');

const inputData = '../testdata/input';
const outputData = '../testdata/output';

function readFile(path) {
    return Fs.readFileSync(path);
}

Fs.readdirSync(inputData).forEach((fileName) => {
    var expected = readFile(outputData + '/' + fileName);
    var actual = new Buffer(JWC.stringify(JSON.parse(readFile(inputData + '/' + fileName))));
    if (expected.compare(actual) == 0) {
        var next = false;
        var byteCount = 0;
        var utf8 = '\n\nFile: ' + fileName + '\n';
        for (let i = 0; i < actual.length; i++) {
            if (byteCount++ % 32 == 0) {
                utf8 += '\n';
                next = false;
            }
            if (next) {
                utf8 += ' ';
            }
            next = true;
            utf8 += actual[i].toString(16);
        }
        console.log(utf8);
    } else {
        throw new Error(fileName);
    }
});
