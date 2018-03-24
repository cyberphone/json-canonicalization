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

from Crypto.PublicKey import RSA
from Crypto.Signature import PKCS1_v1_5

from ecdsa import VerifyingKey as EC

from org.webpki.json.Utils import cryptoBigNumDecode
from org.webpki.json.Utils import base64UrlDecode
from org.webpki.json.Utils import listKeys
from org.webpki.json.Utils import getEcCurve
from org.webpki.json.Utils import serializeJson
from org.webpki.json.Utils import getAlgorithmEntry
from org.webpki.json.Utils import exportPublicKeyAsPem
from org.webpki.json.Utils import exportFormatCheck

from org.webpki.json.Writer import JSONObjectWriter

############################################
# JCS (JSON Cleartext Signature) validator #
############################################

class new:
    def __init__(self,jsonObject):
        """
        Validate the signature of an already parsed JSON object
        
        The current implementation is limited to RSA and EC
        signatures and usage of the IETF JOSE algorithms
        
        An invalid signature raises an exception
        """
        if not isinstance(jsonObject, OrderedDict):
            raise TypeError('JCS requires JSON to be parsed into a "OrderedDict"')
        signatureObject = jsonObject['signature']
        clonedSignatureObject = OrderedDict(signatureObject)
        signatureValue = base64UrlDecode(signatureObject.pop('value'))
        algorithmEntry = getAlgorithmEntry(signatureObject['algorithm'])
        hashObject = algorithmEntry[1].new(serializeJson(jsonObject).encode("utf-8"))
        jsonObject['signature'] = clonedSignatureObject
        self.publicKey = signatureObject['publicKey']
        keyType = self.publicKey['kty']
        if algorithmEntry[0]:
            if keyType != 'RSA':
                raise TypeError('"RSA" expected')
            self.nativePublicKey = RSA.construct([cryptoBigNumDecode(self.publicKey['n']),
                                                  cryptoBigNumDecode(self.publicKey['e'])])
            if not PKCS1_v1_5.new(self.nativePublicKey).verify(hashObject,signatureValue):
                raise ValueError('Invalid Signature!')
        else:
            if keyType != 'EC':
                raise TypeError('"EC" expected')
            self.nativePublicKey = EC.from_string(base64UrlDecode(self.publicKey['x']) + 
                                                  base64UrlDecode(self.publicKey['y']),
                                                  curve=getEcCurve(self.publicKey['crv']))
            self.nativePublicKey.verify_digest(signatureValue,hashObject.digest())
            

    def getPublicKey(self,format='JWK'):
        """
        Return public key as a PEM or JWK string or as a JCS in an JSONObjectWriter
        """
        if exportFormatCheck(format) == 'PEM':
            return exportPublicKeyAsPem(self.nativePublicKey)
        if format == 'JWK':
            return serializeJson(self.publicKey)
        return JSONObjectWriter(self.publicKey)

# TODO: "extensions", "version", "keyId" and checks for extranous properties

