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

public class Unicode2UTF16 {
    
    public static void main(String[] args) throws Exception {
        Scanner input = new Scanner(System.in);
        while (true) {
            System.out.println("\nEnter uncode code point in hex: ");
            String line = input.next();
            char[] utf16 = Character.toChars(Integer.parseInt(line, 16));
            for (char c : utf16) {
              String hex = Integer.toHexString(c);
              while (hex.length() < 4) {
                 hex = "0" + hex;
              }       
              System.out.print(" " + hex);
            }
            System.out.println();
        }
    }
}
