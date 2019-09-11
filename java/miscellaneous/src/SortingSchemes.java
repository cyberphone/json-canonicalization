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

import java.io.IOException;

import java.util.TreeMap;
import java.util.LinkedHashMap;

import org.webpki.jcs.JsonCanonicalizer;

public class SortingSchemes {
    
	static LinkedHashMap<String,String> elements = new LinkedHashMap<String,String>();
	static LinkedHashMap<String,String> rawKeys = new LinkedHashMap<String,String>();

	static {
		elements.put("\\u20ac",        "Euro Sign");
		elements.put("\\r",            "Carriage Return");
		elements.put("\\ufb33",        "Hebrew Letter Dalet With Dagesh");
		elements.put("1",              "One");
		elements.put("\\u0080",        "Control");
		elements.put("\\ud83d\\ude00", "Emoji: Grinning Face");
		elements.put("\\u00f6",        "Latin Small Letter O With Diaeresis");
	}

    static char getHexChar(char c) throws IOException {
        switch (c) {
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return (char) (c - '0');

            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
                return (char) (c - 'a' + 10);

            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
                return (char) (c - 'A' + 10);
        }
        throw new IOException("Bad hex in \\u escape: " + c);
    }

    static String parseString(String property) throws IOException {
        StringBuilder result = new StringBuilder();
        int index = 0;
        while (index < property.length()) {
            char c = property.charAt(index++);
            if (c == '\\') {
                switch (c = property.charAt(index++)) {
                    case '"':
                    case '\\':
                    case '/':
                        break;

                    case 'b':
                        c = '\b';
                        break;

                    case 'f':
                        c = '\f';
                        break;

                    case 'n':
                        c = '\n';
                        break;

                    case 'r':
                        c = '\r';
                        break;

                    case 't':
                        c = '\t';
                        break;

                    case 'u':
                        c = 0;
                        for (int i = 0; i < 4; i++) {
                            c = (char) ((c << 4) + getHexChar(property.charAt(index++)));
                        }
                        break;

                    default:
                        throw new IOException("Unsupported escape:" + c);
                }
            }
            result.append(c);
        }
        return result.toString();
    }

    public static void main(String[] args) throws Exception {
		StringBuilder json = new StringBuilder("{\n");
		boolean next = false;
		for (String key : elements.keySet()) {
			rawKeys.put(key, parseString(key));
			if (next) {
				json.append(",\n");
			}
			next = true;
			json.append("  \"")
			    .append(key)
			    .append("\": \"")
			    .append(elements.get(key))
			    .append('"');
		}
		json.append("\n}");
		System.out.println(json.toString());
		String canon = new JsonCanonicalizer(json.toString()).getEncodedString();
		int i = 0;
		while ((i = canon.indexOf(":\"", i)) > 0) {
			i += 2;
			int j = canon.indexOf('"', i);
			System.out.println(canon.substring(i, j));
		}
		json = new StringBuilder("{\n");
		next = false;
		for (String key : elements.keySet()) {
			if (next) {
				json.append(",\n");
			}
			byte[] utf8 = rawKeys.get(key).getBytes("utf-8");
			String utf8Key = "";
			for (byte b : utf8) {
				utf8Key += "\\u00";
				String hex = Integer.toUnsignedString(b & 0xff, 16);
				if (hex.length() == 1) {
					hex = "0" + hex;
				}
				utf8Key += hex;
			}
			next = true;
			json.append("  \"")
			    .append(utf8Key)
			    .append("\": \"")
			    .append(elements.get(key))
			    .append('"');
		}
		json.append("\n}");
		System.out.println(json.toString());
		canon = new JsonCanonicalizer(json.toString()).getEncodedString();
		i = 0;
		while ((i = canon.indexOf(":\"", i)) > 0) {
			i += 2;
			int j = canon.indexOf('"', i);
			System.out.println(canon.substring(i, j));
		}
    }
}
