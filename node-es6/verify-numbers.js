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

verify("4340000000000001", "9007199254740994");
verify("4340000000000002", "9007199254740996");
verify("444b1ae4d6e2ef50", "1e+21");
verify("3eb0c6f7a0b5ed8d", "0.000001");
verify("3eb0c6f7a0b5ed8c", "9.999999999999997e-7");
verify("8000000000000000", "0");
verify("7fffffffffffffff", INVALID_NUMBER);
verify("7ff0000000000000", INVALID_NUMBER);
verify("fff0000000000000", INVALID_NUMBER);

// Change the file name below to fit your environment
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
                console.log("Line: " + lineCount);
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
    console.log("\nSuccessful Operation.  Lines read: " + lineCount);
}
