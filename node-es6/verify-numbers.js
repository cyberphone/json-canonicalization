'use strict';
const Fs = require('fs');

var file = Fs.openSync("c:\\es6\\numbers\\es6testfile100m.txt", "r");
var count = 0;
var fileBuffer = new Buffer(1024);
var line = "";
var length = 0;
while (length = Fs.readSync(file, fileBuffer, 0, 1024, null)) {
    for (let q = 0; q < length; q++) {
        var c = fileBuffer[q]; 
        if (c == 0x0a) {
            var ieeeHex = line.substring(0, line.indexOf(','));
            while (ieeeHex.length < 16) {
                ieeeHex = '0' + ieeeHex;
            }
            var ieeeDouble = Buffer(ieeeHex, 'hex').readDoubleBE();
            var es6Format = line.substring(line.indexOf(',') + 1);
            if (Number(es6Format) != ieeeDouble || String(ieeeDouble) != es6Format) {
                throw new Exception("V=" + es6Format);
            }
            if (++count % 1000000 == 0) {
                console.log(count + " " + ieeeDouble + " " + es6Format);
            }
            line = "";
        } else {
            line += String.fromCharCode(c);
        }
    }
}
Fs.closeSync(file);

console.log("SUCCESSFUL");
