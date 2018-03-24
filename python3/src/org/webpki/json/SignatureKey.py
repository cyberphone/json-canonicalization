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

from Crypto.PublicKey import RSA
from Crypto.Signature import PKCS1_v1_5

from ecdsa import SigningKey as EC

from org.webpki.json.Utils import base64UrlDecode
from org.webpki.json.Utils import cryptoBigNumDecode
from org.webpki.json.Utils import parseJson
from org.webpki.json.Utils import getEcCurveName
from org.webpki.json.Utils import getEcCurve
from org.webpki.json.Utils import getAlgorithmEntry
from org.webpki.json.Utils import exportPublicKeyAsPem
from org.webpki.json.Utils import exportFormatCheck

from org.webpki.json.Writer import JSONObjectWriter

from org.webpki.json.BaseKey import BaseKey

###################################################
# Private key and signature support wrapper class #
###################################################

class new(BaseKey):
    def __init__(self,privateKeyString):
        """
        Initialize object with an RSA or EC private key in JWK or PEM format
        
        Signature algorithms are assumed to be given in the IETF JOSE format
        
        This class is essentially a wrapper over the currently disparate Python
        EC and RSA libraries, not limited to JSON or JCS
        """
        if '"kty"' in  privateKeyString:
            jwk = parseJson(privateKeyString)
            keyType = jwk['kty']
            if keyType == 'RSA':
                self.nativePrivateKey = RSA.construct([cryptoBigNumDecode(jwk['n']),
                                                       cryptoBigNumDecode(jwk['e']),
                                                       cryptoBigNumDecode(jwk['d']),
                                                       cryptoBigNumDecode(jwk['p']),
                                                       cryptoBigNumDecode(jwk['q'])])
                """ JWK syntax checking... """
                cryptoBigNumDecode(jwk['dp'])
                cryptoBigNumDecode(jwk['dq'])
                cryptoBigNumDecode(jwk['qi'])
            elif keyType == 'EC':
                self.nativePrivateKey = EC.from_string(base64UrlDecode(jwk['d']),getEcCurve(jwk['crv']))
            else:
                raise ValueError('Unsupported key type: "' + keyType + '"');
        else:
            if ' RSA ' in privateKeyString:
                self.nativePrivateKey = RSA.importKey(privateKeyString)
            else:
                self.nativePrivateKey = EC.from_pem(privateKeyString)
        """
        Set default signature algorithm
        """
        if self.isRSA():
            self.algorithm = 'RS256'
        else:
            self.algorithm = 'ES256'


    def isRSA(self):
        """
        Return True for RSA, False for EC
        """
        return isinstance(self.nativePrivateKey,RSA._RSAobj)


    def setAlgorithm(self,algorithm):
        """
        Override the default algorithm setting
        """
        if getAlgorithmEntry(algorithm)[0] != self.isRSA():
            raise TypeError('Algorithm is not compatible with key type')
        self.algorithm = algorithm
        return self


    def signData(self,data):
        """
        Well, use the private key + hash method and return a signature blob
        """
        algorithmEntry = getAlgorithmEntry(self.algorithm)
        hashObject = algorithmEntry[1].new(data)
        if algorithmEntry[0]:
            return PKCS1_v1_5.new(self.nativePrivateKey).sign(hashObject)
        return self.nativePrivateKey.sign_digest(hashObject.digest())


    def setSignatureMetaData(self,jsonObjectWriter):
        """
        Only for usage by JSONObjectWriter
        """
        jsonObjectWriter.setString('algorithm',self.algorithm)
        jsonObjectWriter.setObject('publicKey',self.getPublicKey('JCS'))


    def getPublicKey(self,format='JWK'):
        """
        Return public key as a PEM or JWK string or as a JCS/JWK in an JSONObjectWriter
        """ 
        if exportFormatCheck(format) == 'PEM':
            if self.isRSA():
                return exportPublicKeyAsPem(self.nativePrivateKey.publickey())
            return exportPublicKeyAsPem(self.nativePrivateKey.get_verifying_key())
        publicKey = JSONObjectWriter()
        if self.isRSA():
            publicKey.setString('kty', 'RSA')
            publicKey.setCryptoBigNum('n', self.nativePrivateKey.n)
            publicKey.setCryptoBigNum('e', self.nativePrivateKey.e)
        else:
            publicKey.setString('kty', 'EC')
            publicKey.setString('crv', getEcCurveName(self.nativePrivateKey))
            point = self.nativePrivateKey.get_verifying_key().to_string()
            length = len(point)
            if length % 2:
                raise ValueError('EC point length error')
            length >>= 1
            publicKey.setBinary('x', point[:length])
            publicKey.setBinary('y', point[length:])
        if format == 'JWK':
            return publicKey.serialize()
        return publicKey

