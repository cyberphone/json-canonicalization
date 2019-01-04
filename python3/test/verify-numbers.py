import binascii
import struct

from org.webpki.json.NumberToJson import convert2Es6Format

INVALID_NUMBER = 'null'

#
# Test program using a 100 million value file formatted as follows:
# value in ieee hex representation (1-16 digits) + ',' + correct ES6 format + '\n'
#
def verify(ieeeHex, expected):
    while len(ieeeHex) < 16:
        ieeeHex = '0' + ieeeHex
    value = struct.unpack('>d',binascii.a2b_hex(ieeeHex))[0]
    try:
        pyFormat = convert2Es6Format(value)
    except ValueError:
        if expected == INVALID_NUMBER:
            return
    if pyFormat == expected and value == float(pyFormat) and repr(value) == str(value):
        return
    print('IEEE:   ' + ieeeHex + '\nPython: ' + pyFormat + '\nExpected: ' + expected)
    exit(0)

verify('4340000000000001', '9007199254740994')
verify('4340000000000002', '9007199254740996')
verify('444b1ae4d6e2ef50', '1e+21')
verify('3eb0c6f7a0b5ed8d', '0.000001')
verify('3eb0c6f7a0b5ed8c', '9.999999999999997e-7')
verify('8000000000000000', '0')
verify('7fffffffffffffff', INVALID_NUMBER)
verify('7ff0000000000000', INVALID_NUMBER)
verify('fff0000000000000', INVALID_NUMBER)

file = open('c:\\es6\\numbers\\es6testfile100m.txt','rb')
lineCount = 0;
line = '';
while True:
    byte = file.read(1);
    if len(byte) == 0:
        print('Successful Operation. Lines read: ' + str(lineCount))
        exit(0)
    if byte == b'\n':
        lineCount = lineCount + 1;
        i = line.find(',')
        if i <= 0 or i >= len(line) - 1:
            print('Bad line: ' + str(i))
            exit(0)
        verify(line[:i], line[i + 1:])
        line = ''
        if lineCount % 1000000 == 0:
            print('Line: ' + str(lineCount))
    else:
        line += byte.decode(encoding='UTF-8')

