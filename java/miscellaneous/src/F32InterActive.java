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

public class F32InterActive {
    
    public static void main(String[] args) throws Exception {
        Scanner input = new Scanner(System.in);
        while (true) {
            System.out.println("\nEnter xhhhhhhhhh or floating point 32-bit number: ");
            String line = input.next();
            String inputFp = "N/A";
            float f;
            if (line.startsWith("x")) {
                String hex = line.substring(1);
                while (hex.length() < 8) {
                    hex = '0' + hex;
                }
                f = Float.intBitsToFloat(Integer.parseUnsignedInt(hex,16));            
            } else {
                inputFp = line;
                f = Float.valueOf(inputFp);
            }
            double d = f;
            long ieee64 = Double.doubleToRawLongBits(d);
            String ieeeHex64 = Long.toHexString(ieee64);
            while (ieeeHex64.length() < 16) {
                ieeeHex64 = '0' + ieeeHex64;
            }
            String ieeeBin64 = Long.toBinaryString(ieee64);
            while (ieeeBin64.length() < 64) {
                ieeeBin64 = '0' + ieeeBin64;
            }
            ieeeBin64 = ieeeBin64.substring(0,1) + ' ' + 
                        ieeeBin64.substring(1,12) + ' ' +
                        ieeeBin64.substring(12);
            long ieee32 = Float.floatToRawIntBits(f) & 0xffffffffL;
            String ieeeHex32 = Long.toHexString(ieee32);
            while (ieeeHex32.length() < 8) {
                ieeeHex32 = '0' + ieeeHex32;
            }
            String ieeeBin32 = Long.toBinaryString(ieee32);
            while (ieeeBin32.length() < 32) {
                ieeeBin32 = '0' + ieeeBin32;
            }
            ieeeBin32 = ieeeBin32.substring(0,1) + ' ' + 
                        ieeeBin32.substring(1,9) + ' ' +
                        ieeeBin32.substring(9);
            String outputFp = NumberToJSON.serializeNumber(d);
            System.out.println("\nInput floating point: " + inputFp);
            System.out.println("Output floating point: " + outputFp);
            System.out.println("Hex 64 value: " + ieeeHex64);
            System.out.println("Binary 64 value: " + ieeeBin64);
            System.out.println("Hex 32 value: " + ieeeHex32);
            System.out.println("Binary 32 value: " + ieeeBin32);
        }
    }
}
