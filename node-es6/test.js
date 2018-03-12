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
    var result = expected.compare(actual) == 0 ? "SUCCESS" : "FAIL";
    console.log("File: " + fileName + '=' + result);
});