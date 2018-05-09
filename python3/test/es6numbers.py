import binascii
import struct

from org.webpki.json.NumberToJson import convert2Es6Format

#
# Test program using a 100 million value file formatted as follows:
# value in ieee hex representation (1-16 digits) + ',' + correct ES6 format + '\n'
#
f = open('c:\\es6\\numbers\\es6testfile100m.txt','rb')
l = 0;
string = '';
while True:
    byte = f.read(1);
    if len(byte) == 0:
        exit(0)
    if byte == b'\n':
        l = l + 1;
        i = string.find(',')
        if i <= 0 or i >= len(string) - 1:
            print('Bad string: ' + str(i))
            exit(0)
        hex = string[:i]
        while len(hex) < 16:
            hex = '0' + hex
        value = struct.unpack('>d',binascii.a2b_hex(hex))[0]
        es6Format = string[i + 1:]
        pyFormat = convert2Es6Format(value)
        if pyFormat != es6Format or value != float(pyFormat) or repr(value) != str(value):
            print('IEEE:   ' + hex + '\nPython: ' + str(value) + '\nES6/V8: ' + es6Format)
            exit(0)
        string = ''
        if l % 1000000 == 0:
            print(l)
    else:
        string += byte.decode(encoding='UTF-8')

