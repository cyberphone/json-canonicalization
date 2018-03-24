import collections
import sys
import codecs

from org.webpki.json import JCSValidator

from org.webpki.json.Utils import parseJson

# Our test program
if len(sys.argv) != 2:
    print ('No input file given')
    sys.exit(1)

# There should be a file with utf-8 json in, read and parse it
jsonString = codecs.open(sys.argv[1], "r", "utf-8").read()

# print jsonString

def checkAllSignatures(jsonObject):
    for w in jsonObject:
        if isinstance(jsonObject[w],collections.OrderedDict):
            checkAllSignatures(jsonObject[w])
    if w == 'signature':
        validator = JCSValidator.new(jsonObject)
        print ('JWK=\n' + validator.getPublicKey('JWK'))
        print ('PEM=\n' + validator.getPublicKey('PEM'))

# Just check the outer signature
jsonObject = parseJson(jsonString)
JCSValidator.new(jsonObject)
print ('Valid (since it didn\'t raise an exception)')

# For fun we can traverse the entire object and look for inner signatures as well 
checkAllSignatures(jsonObject)

