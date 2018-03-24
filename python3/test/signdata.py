import sys
import codecs

from org.webpki.json.Writer import JSONObjectWriter
from org.webpki.json import SignatureKey

from org.webpki.json.Utils import parseJson
#REMOVE
from org.webpki.json.Utils import serializeJson

# Our test program
if not len(sys.argv) in (2,3):
    print ('Private-key [JSON-in-file]')
    sys.exit(1)

def readFile(name):
  return codecs.open(name, "r", "utf-8").read()

keyString = readFile(sys.argv[1])

signatureKey = SignatureKey.new(keyString)
if signatureKey.isRSA():
  print ("RSA key")
else:
  print ("EC key")

if len(sys.argv) == 3:
  jsonObject = JSONObjectWriter(parseJson(readFile(sys.argv[2])))
else:
  jsonObject = JSONObjectWriter()
  jsonObject.setInt("an_int", 7)

  jsonObject.setString("a_string", "Sure")
  jsonObject.setObject("an_object").setString("another_string","Yeah").setFloat("a_float",1e+5).setBinary("a_blob",b'\x00\x01\x03\x04\x05')
  jsonObject.setArray("an_array").setInt(45).setString("Nope").setObject()
  jsonObject.setArray("two_dimensional").setArray().setString("Bye")

jsonObject.setSignature(signatureKey)

print (jsonObject.serialize())
