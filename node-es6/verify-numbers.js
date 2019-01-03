// JavaScript source code for testing number parsing and serialization

'use strict';
const Fs = require('fs');

var conversionErrors = 0;

const INVALID_NUMBER = "null";

function verify(ieeeHex, expected) {
    while (ieeeHex.length < 16) {
        ieeeHex = '0' + ieeeHex;
    }
    var ieeeDouble = Buffer.from(ieeeHex, 'hex').readDoubleBE();
    var es6Created = JSON.stringify(ieeeDouble);
    if (es6Created == INVALID_NUMBER && expected == INVALID_NUMBER) {
        return;
    } else if (es6Created == expected && Number(expected) == ieeeDouble) {
        return;
    }
    conversionErrors++;
    console.log("Hex=" + ieeeHex + " Expected=" + expected + " Created=" + es6Created);
}

verify("7fffffffffffffff", INVALID_NUMBER);
verify("7ff0000000000000", INVALID_NUMBER);
verify("fff0000000000000", INVALID_NUMBER);

var file = Fs.openSync("c:\\es6\\numbers\\es6testfile100m.txt", "r");
var lineCount = 0;
var fileBuffer = Buffer.alloc(1024);
var line = "";
var length = 0;
while (length = Fs.readSync(file, fileBuffer, 0, 1024, null)) {
    for (let q = 0; q < length; q++) {
        var c = fileBuffer[q]; 
        if (c == 0x0a) {
            if (++lineCount % 1000000 == 0) {
                console.log("line: " + lineCount);
            }
            let comma = line.indexOf(',');
            verify(line.substring(0, comma), line.substring(comma + 1));
            line = "";
        } else {
            line += String.fromCharCode(c);
        }
    }
}
Fs.closeSync(file);
if (conversionErrors) {
    console.log("\n****** ERRORS: " + conversionErrors + " *******");
} else {
    console.log("Successful Operation.  Lines read: " + lineCount);
}
