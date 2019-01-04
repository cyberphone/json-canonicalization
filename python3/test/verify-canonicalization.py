##############################################################################
#                                                                            #
#  Copyright 2006-2019 WebPKI.org (http://webpki.org).                       #
#                                                                            #
#  Licensed under the Apache License, Version 2.0 (the "License");           #
#  you may not use this file except in compliance with the License.          #
#  You may obtain a copy of the License at                                   #
#                                                                            #
#      https://www.apache.org/licenses/LICENSE-2.0                           #
#                                                                            #
#  Unless required by applicable law or agreed to in writing, software       #
#  distributed under the License is distributed on an "AS IS" BASIS,         #
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  #
#  See the License for the specific language governing permissions and       #
#  limitations under the License.                                            #
#                                                                            #
##############################################################################

# Test program for JCS

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
    print("\nFile: " + fileName)
    jsonData = readFile(os.path.join(inputPath,fileName))
    obj = loads(jsonData, object_pairs_hook=OrderedDict)
    canres = canonicalize(obj)
    expected = readFile(os.path.join(outputPath,fileName)).encode()
    if canres == expected:
      result = "\nSuccess"
    else:
      result = "\nFAILURE\n"
    byteCount = 0
    next = False
    utf8InHex = ''
    for c in canres:
      if byteCount > 0 and byteCount % 32 == 0:
        utf8InHex += '\n'
        next = False
      byteCount += 1
      if next:
        utf8InHex += ' '
      next = True
      utf8InHex += '{0:02x}'.format(c)
    print(utf8InHex + result)

testData = os.path.join(os.path.split(os.path.split(os.path.dirname(os.path.abspath(__file__)))[0])[0],'testdata')
inputPath = os.path.join(testData, 'input')
outputPath = os.path.join(testData, 'output')
if len(sys.argv) == 1:
    for fileName in os.listdir(inputPath):
        oneTurn(fileName)
else:
    oneTurn(sys.argv[1])






