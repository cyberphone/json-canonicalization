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

import base64

from decimal import Decimal

import simplejson as json

from Crypto.Util.number import bytes_to_long
from Crypto.Util.number import long_to_bytes

from Crypto.PublicKey import RSA

from collections import OrderedDict

from Crypto.Hash import SHA256
from Crypto.Hash import SHA384
from Crypto.Hash import SHA512

from ecdsa.curves import NIST256p
from ecdsa.curves import NIST384p
from ecdsa.curves import NIST521p

############################################
# Crypto and JSON support methods and data #
############################################

algorithms = OrderedDict([
    ('RS256', (True,  SHA256)),
    ('RS384', (True,  SHA384)),
    ('RS512', (True,  SHA512)),
    ('ES256', (False, SHA256)),
    ('ES384', (False, SHA384)),
    ('ES512', (False, SHA512))
])

ecCurves = OrderedDict([
    ('P-256', NIST256p),
    ('P-384', NIST384p),
    ('P-521', NIST521p)
])

publicKeyExportFormats = OrderedDict([
    ('PEM', 'Privacy Enhanced Mail format'),
    ('JWK', 'JOSE Web Key format'),
    ('JCS', 'JSON Cleartext Signature format')
])

def cryptoBigNumDecode(base64String):
    return bytes_to_long(base64UrlDecode(base64String))
    
def cryptoBigNumEncode(bigPostiveNumber):
    return base64UrlEncode(long_to_bytes(bigPostiveNumber))

def base64UrlDecode(data):
    if not isinstance(data, str):
        raise TypeError('Argument should be a str or unicode')
    return base64.urlsafe_b64decode(data + '=' * (4 - (len(data) % 4)))

def base64UrlEncode(data):
    if isinstance(data, bytes):
        return base64.urlsafe_b64encode(data).rstrip(b'=').decode(encoding='UTF-8')
    raise TypeError('Argument should be bytes')

def getEcCurve(curveName):
    if curveName in ecCurves:
        return ecCurves[curveName]
    raise TypeError('Found "' + curveName + '". Supported EC curves: ' + listKeys(ecCurves))
    
def listKeys(dictionary):
    comma = False
    result = ''
    for item in dictionary:
        if comma:
            result += ', '
        comma = True
        result += item
    return result
    
def getEcCurveName(nativeKey):
    for curve in ecCurves:
        if nativeKey.curve == ecCurves[curve]:
            return curve;
    raise TypeError('Curve "' + nativeKey.curve.name + '" not supported')

def getAlgorithmEntry(algorithm):
    if algorithm in algorithms:
        return algorithms[algorithm]
    raise TypeError('Found "' + algorithm + '". Supported algorithms: ' + listKeys(algorithms))

def exportPublicKeyAsPem(nativePublicKey):
    if isinstance(nativePublicKey,RSA._RSAobj):
        return nativePublicKey.exportKey(format='PEM').decode(encoding='UTF-8') + '\n'
    return nativePublicKey.to_pem().decode(encoding='UTF-8')

def exportFormatCheck(format):
    if format in publicKeyExportFormats:
        return format
    raise TypeError('Found "' + format + '". Supported formats: ' + listKeys(publicKeyExportFormats))


############################################
# JCS Compatible Parser                                        #
############################################

def parseJson(jsonString):
    return json.loads(jsonString, object_pairs_hook=OrderedDict,parse_float=EnhancedDecimal)

############################################
# JCS Compatible Serializer                                #
############################################

def serializeJson(jsonObject):
    return json.dumps(jsonObject,separators=(',',':'),ensure_ascii=False)

# Support class
class EnhancedDecimal(Decimal):
     def __str__ (self):
         return self.saved_string

     def __new__(cls, value="0", context=None):
         obj = Decimal.__new__(cls,value,context)
         obj.saved_string = value
         return obj;    
