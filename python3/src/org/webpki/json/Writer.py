##############################################################################
#                                                                            #
#  Copyright 2006-2017 WebPKI.org (http://webpki.org).                       #
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

from collections import OrderedDict

from org.webpki.json.BaseKey import BaseKey

from org.webpki.json.Utils import base64UrlEncode
from org.webpki.json.Utils import cryptoBigNumEncode
from org.webpki.json.Utils import serializeJson

##############################################
# JSON encoding medium-level wrapper classes #
##############################################

class JSONObjectWriter:
    def __init__(self,optionalArgument=None):
        if _checkOptionalArgument(optionalArgument,OrderedDict):
            self.root = optionalArgument
        else:
            self.root = OrderedDict()

    def setInt(self,name,value):
        if not isinstance(value,int):
            raise TypeError('Integer expected')
        return self._put(name,value)

    def setString(self,name,value):
        if not isinstance(value,str):
            raise TypeError('String expected')
        return self._put(name,value)

    def setFloat(self,name,value):
        if isinstance(value, int):
            value = float(value)
        elif not isinstance(value,float):
            raise TypeError('Float expected')
        return self._put(name,value)

    def setObject(self,name, optionalArgument=None):
        if _checkOptionalArgument(optionalArgument,JSONObjectWriter):
            return self._put(name,optionalArgument.root)
        newObject = JSONObjectWriter()
        self._put(name,newObject.root)
        return newObject

    def setArray(self,name, optionalArgument=None):
        if _checkOptionalArgument(optionalArgument,JSONArrayWriter):
            return self._put(name,optionalArgument.array)
        newArray = JSONArrayWriter()
        self._put(name,newArray.array)
        return newArray

    def setBinary(self,name,value):
        return self._put(name,base64UrlEncode(value))

    def setCryptoBigNum(self,name,value):
        return self._put(name,cryptoBigNumEncode(value))

    def setSignature(self,signatureKey):
        if not isinstance(signatureKey,BaseKey):
            raise TypeError('SignatureKey expected')
        signatureObject = JSONObjectWriter()
        signatureKey.setSignatureMetaData(signatureObject)
        self._put('signature',signatureObject.root)
        signatureObject.setBinary('value',signatureKey.signData(self.serialize().encode("utf-8")))
        return self

    def _put(self,name,value):
        if not isinstance(name,str):
            raise TypeError('Name must be a string')
        if name in self.root:
            raise ValueError('Duplicate property: "' + name + '"')
        self.root[name] = value
        return self

    def serialize(self):
        return serializeJson(self.root)
        
class JSONArrayWriter:
    def __init__(self):
        self.array = list()

    def setInt(self,value):
        if not isinstance(value,int):
            raise TypeError('Integer expected')
        return self._put(value)

    def setString(self,value):
        if not isinstance(value,str):
            raise TypeError('String expected')
        return self._put(value)

    def setFloat(self,value):
        if isinstance(value, int):
            value = float(value)
        elif not isinstance(value,float):
            raise TypeError('Float expected')
        return self._put(value)

    def setObject(self,optionalArgument=None):
        if _checkOptionalArgument(optionalArgument,JSONObjectWriter):
            return self._put(optionalArgument.root)
        newObject = JSONObjectWriter()
        self._put(newObject.root)
        return newObject

    def setArray(self,optionalArgument=None):
        if _checkOptionalArgument(optionalArgument,JSONArrayWriter):
            return self._put(optionalArgument.array)
        newArray = JSONArrayWriter()
        self._put(newArray.array)
        return newArray

    def setBinary(self,value):
        if not isinstance(value, str):
            raise TypeError('String or bytearray expected')
        return self._put(base64UrlEncode(value))

    def _put(self,value):
        self.array.append(value)
        return self

    def serialize(self):
        return serializeJson(self.array)

def _checkOptionalArgument(optionalArgument,expectedType):
    if optionalArgument:
        if isinstance(optionalArgument,expectedType):
            return True
        raise TypeError('Optional argument not "' + expectedType.__name__ + '"')
    return False