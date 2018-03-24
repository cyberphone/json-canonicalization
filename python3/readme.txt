JCS support for Python
======================

1. Setup
--------
Set PYTHONPATH to the src sub-directory

On Windows perform "CHCP 65001" to set the console in UTF-8 mode


2. Running
----------
$ cd test


3a Testing signature validation
-------------------------------
$ python validate.py ec-signature.json
$ python validate.py invalid-signature.json

3b Signing data
---------------
$ python signdata.py private-ec-p256-key.jwk unsigned.json
$ python signdata.py private-rsa-key.pem unsigned.json

