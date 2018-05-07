import sys
import codecs
import os

from json import loads

from org.webpki.json.Canonicalize import canonicalize
from org.webpki.json.Canonicalize import serialize

from collections import OrderedDict

# Our test program
if not len(sys.argv) in (1,2):
    print ('[JSON-in-file]')
    sys.exit(1)

def readFile(name):
  return codecs.open(name, "r", "utf-8").read()

def oneTurn(fileName):
    print(fileName)
    jsonData = readFile(os.path.join(inputPath,fileName))
    obj = loads(jsonData, object_pairs_hook=OrderedDict)
    canres = canonicalize(obj)
    expected = readFile(os.path.join(outputPath,fileName)).encode()
    if canres == expected:
      result = "Success"
    else:
      result = "\nFAILURE\n"
    print(serialize(obj,utf8=False))
    print(result)

testData = os.path.join(os.path.split(os.path.split(os.path.dirname(os.path.abspath(__file__)))[0])[0],'testdata')
inputPath = os.path.join(testData, 'input')
outputPath = os.path.join(testData, 'output')
if len(sys.argv) == 1:
    for fileName in os.listdir(inputPath):
        oneTurn(fileName)
else:
    oneTurn(sys.argv[1])






