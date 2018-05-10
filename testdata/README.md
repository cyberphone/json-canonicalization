## Test Data

The [input](input) directory contains files with non-canonicalized data which is
supposed be transformed as specified by the corresponding file in the
[output](output) directory.

For testing ES6 number serialization there is a file on 
https://onedrive.live.com/embed?cid=9341770E0D0D5468&resid=9341770E0D0D5468%21222&authkey=ADOClRsuPv3_pTk
containing about a 100 million of random and edge-case values.  The test file 
[../node-es6/verify-numbers.js](../node-es6/verify-numbers.js)
shows how to decode and use this sample data.
