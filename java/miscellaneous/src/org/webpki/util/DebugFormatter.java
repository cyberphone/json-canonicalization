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
package org.webpki.util;


import java.io.IOException;

public class DebugFormatter {

    private DebugFormatter() {}

    private StringBuilder res = new StringBuilder(1000);

    private void put(char c) {
        res.append(c);
    }

    private void hex(int i) {
        if (i < 10) {
            put((char) (i + '0'));
        } else {
            put((char) (i + 'a' - 10));
        }
    }

    private void twohex(int i) {
        i &= 0xFF;
        hex(i / 16);
        hex(i % 16);
    }

    private void addrhex(int i) {
        if (i > 65535) {
            twohex(i / 65536);
            i %= 65536;
        }
        twohex(i / 256);
        twohex(i % 256);
    }

    private String toHexDebugData(byte[] indata, int bytesPerLine) {
        int index = 0;
        int i = 0;
        if (indata.length == 0) {
            return "No data";
        }
        boolean onlyData = false;
        if (bytesPerLine < 0) {
            bytesPerLine = -bytesPerLine;
            onlyData = true;
        }
        while (index < indata.length) {
            if (index > 0) {
                put('\n');
            }
            addrhex(index);
            put(':');
            int q = indata.length - index;
            if (q > bytesPerLine) {
                q = bytesPerLine;
            }
            for (i = 0; i < q; i++) {
                put(' ');
                twohex(indata[index + i]);
            }
            if (onlyData) {
                index += q;
                continue;
            }
            while (i++ <= bytesPerLine) {
                put(' ');
                put(' ');
                put(' ');
            }
            put('\'');
            for (i = 0; i < q; i++) {
                int c = (int) indata[index++];
                if (c < 32 || c >= 127) {
                    put('.');
                } else {
                    put((char) c);
                }
            }
            put('\'');
            while (i++ < bytesPerLine) {
                put(' ');
            }
        }
        return res.toString();
    }

    private String toHexString(byte[] indata) {
        int i = 0;
        while (i < indata.length) {
            twohex(indata[i++]);
        }
        return res.toString();
    }

    public static String getHexDebugData(byte[] binaryBlob, int bytesPerLine) {
        return new DebugFormatter().toHexDebugData(binaryBlob, bytesPerLine);
    }

    public static String getHexDebugData(byte[] binaryBlob) {
        return getHexDebugData(binaryBlob, 16);
    }

    public static String getHexString(byte[] binaryBlob) {
        return new DebugFormatter().toHexString(binaryBlob);
    }

    public static int toHex(char c) throws IOException {
        if (c >= '0') {
            if (c <= '9') return c - '0';
            if (c >= 'a') {
                if (c <= 'f') return c - ('a' - 10);
            }
            if (c >= 'A') {
                if (c <= 'F') return c - ('A' - 10);
            }
        }
        throw new IOException("Bad hexchar: " + c);
    }

    public static byte[] getByteArrayFromHex(String hex) throws IOException {
        int l = hex.length();
        int bl;
        if (l == 0 || l % 2 != 0) throw new IOException("Bad hexstring: " + hex);
        byte[] data = new byte[bl = l / 2];
        while (--bl >= 0) {
            data[bl] = (byte) (toHex(hex.charAt(--l)) + (toHex(hex.charAt(--l)) << 4));
        }
        return data;
    }


    /*##################################################################*/
    /*                                                                  */
    /*  Method: main                                                    */
    /*                                                                  */
    /*  Description: This is a command-line interface for testing only  */
    /*                                                                  */
    /*##################################################################*/

    public static void main(String[] args) throws IOException {
        if (args.length == 3 && args[0].equals("tobin")) {
            ArrayUtil.writeFile(args[2], DebugFormatter.getByteArrayFromHex(new String(ArrayUtil.readFile(args[1]), "UTF-8")));
            System.exit(0);
        }
        if (args.length != 2 || !(args[0].equals("hex") || args[0].equals("dump"))) {
            System.out.println("Usage: DebugFormatter hex|dump bininputfile \n" +
                    "                      tobin inputfileinhex outputfilebin\n");
            System.exit(0);
        }
        byte[] data = ArrayUtil.readFile(args[1]);
        System.out.print(args[0].equals("dump") ? DebugFormatter.getHexDebugData(data) : DebugFormatter.getHexString(data));
    }

}
