/*
 *  Copyright 2006-2018 WebPKI.org (http://webpki.org).
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

import java.io.FileReader;
import java.io.BufferedReader;
import java.io.IOException;

import org.webpki.jcs.NumberToJSON;

public class ES6NumberTest {
    
    static final int TURNS = 1000000;
    
    static final String INVALID_NUMBER = "null";
    
    static int conversionErrors;
    
    static void verify(String ieeeHex, String expected) {
        while (ieeeHex.length() < 16) {
            ieeeHex = '0' + ieeeHex;
        }
        double ieeeF64 = Double.longBitsToDouble(Long.parseUnsignedLong(ieeeHex, 16));
        try {
        	String es6Created = NumberToJSON.serializeNumber(ieeeF64);
	        if (!es6Created.equals(expected)) {
	            conversionErrors++;
	            System.out.println("ieeeHex=" + ieeeHex + 
	            		           "\ncreated=" + es6Created + 
	            		           "\nexpected=" + expected + "\n");
	        }
        } catch (IOException e) {
        	if (!expected.equals(INVALID_NUMBER)) {
	            conversionErrors++;
	            System.out.println("ieeeHex=" + ieeeHex);
        	}
        }
        if (!expected.equals(INVALID_NUMBER)) {
            if (ieeeF64 != Double.valueOf(expected)) {
                throw new RuntimeException("Parser error: " + expected);
            }
        }
    }
    
    public static void main(String[] args) throws IOException {
        verify("4340000000000001", "9007199254740994");
        verify("4340000000000002", "9007199254740996");
        verify("444b1ae4d6e2ef50", "1e+21");
        verify("3eb0c6f7a0b5ed8d", "0.000001");
        verify("3eb0c6f7a0b5ed8c", "9.999999999999997e-7");
        verify("8000000000000000", "0");
        verify("7fffffffffffffff", INVALID_NUMBER);
        verify("7ff0000000000000", INVALID_NUMBER);
        verify("fff0000000000000", INVALID_NUMBER);
        BufferedReader in = new BufferedReader(new FileReader(args[0]));
        long lineCount = 0;
        while (true) {
            String line = in.readLine();
            if (line == null) {
            	if (conversionErrors == 0) {
                    System.out.println("\nSuccessful Operation.  Lines read: " + lineCount);
                } else {
                    System.out.println("\nFailures: " + conversionErrors);
                }
                return;
            }
            int comma = line.indexOf(',');
            verify(line.substring(0, comma), line.substring(comma + 1));
            if (++lineCount % TURNS == 0) {
                System.out.println("Line: " + lineCount);
            }
        }
    }
}
