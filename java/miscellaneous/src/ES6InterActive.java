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

import java.util.Scanner;

import org.webpki.jcs.NumberToJSON;

public class ES6InterActive {
    
    public static void main(String[] args) throws Exception {
        Scanner input = new Scanner(System.in);
        while (true) {
            System.out.println("\nEnter xhhhhhhhhh or floating point number: ");
            String line = input.next();
            String inputFp = "N/A";
            double d;
            if (line.startsWith("x")) {
                String hex = line.substring(1);
                while (hex.length() < 16) {
                    hex = '0' + hex;
                }
                d = Double.longBitsToDouble(Long.parseUnsignedLong(hex,16));            
            } else {
                inputFp = line;
                d = Double.valueOf(inputFp);
            }
            long ieee = Double.doubleToRawLongBits(d);
            String ieeeHex = Long.toHexString(ieee);
            while (ieeeHex.length() < 16) {
                ieeeHex = '0' + ieeeHex;
            }
            String ieeeBin = Long.toBinaryString(ieee);
            while (ieeeBin.length() < 64) {
                ieeeBin = '0' + ieeeBin;
            }
            ieeeBin = ieeeBin.substring(0,1) + ' ' + 
                      ieeeBin.substring(1,12) + ' ' +
                      ieeeBin.substring(12);
            String outputFp = NumberToJSON.serializeNumber(d);
            System.out.println("\nInput floating point: " + inputFp);
            System.out.println("Output floating point: " + outputFp);
            System.out.println("Hex value: " + ieeeHex);
            System.out.println("Binary value: " + ieeeBin);
        }
    }
}
