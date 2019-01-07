// JavaScript source code for testing the JSON canonicalizer

'use strict';
const Fs = require('fs');
const canonicalize = require('./canonicalize.js');

const pathSeparator = __dirname.indexOf('\\') >= 0 ? '\\' : '/';
const rootPath = __dirname.substring(0, __dirname.lastIndexOf(pathSeparator))

const inputData = rootPath + '/testdata/input';
const outputData = rootPath + '/testdata/output';

function readFile(path) {
    return Fs.readFileSync(path);
}

var failures = 0;

Fs.readdirSync(inputData).forEach((fileName) => {
    var expected = readFile(outputData + '/' + fileName);
    var actual = Buffer.from(canonicalize(JSON.parse(readFile(inputData + '/' + fileName))));
    var next = false;
    var byteCount = 0;
    var utf8 = '\nFile: ' + fileName;
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
    console.log(utf8 + '\n');
    if (expected.compare(actual)) {
    	failures++;
    	console.log('THE TEST ABOVE FAILED!');
    }
});

if (failures == 0) {
	console.log('All tests succeeded!');
} else {
	console.log('\n****** ERRORS: ' + failures + ' *******\n');
}
