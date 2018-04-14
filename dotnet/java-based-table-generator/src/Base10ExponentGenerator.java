import java.io.FileOutputStream;

import java.math.BigDecimal;
import java.math.MathContext;

import java.lang.Math;

public class Base10ExponentGenerator {

    static final int EXPONENT_OFFSET = 1023;  // From the IEEE-754 spec

	static final long MANTISSA_DIVIDER = 0x0010000000000000L;

    static StringBuilder s = new StringBuilder(
	    "/*\n" +
		" *  Copyright 2006-2018 WebPKI.org (http://webpki.org).\n" +
		" *\n" +
		" *  Licensed under the Apache License, Version 2.0 (the \"License\");\n" +
		" *  you may not use this file except in compliance with the License.\n" +
		" *  You may obtain a copy of the License at\n" +
		" *\n" +
		" *      https://www.apache.org/licenses/LICENSE-2.0\n" +
		" *\n" +
		" *  Unless required by applicable law or agreed to in writing, software\n" +
		" *  distributed under the License is distributed on an \"AS IS\" BASIS,\n" +
		" *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.\n" +
		" *  See the License for the specific language governing permissions and\n" +
		" *  limitations under the License.\n" +
		" *\n" +
		" */\n" +
		"\n" +
		"\n" +
	    "          //////////////////////////////////////////////////////\n" +
	    "          // For quick lookup of IEEE-754 to 10base exponents //\n" +
		"          // Index: IEEE-754 exponent                         //\n" +
		"          //                                                  //\n" +
		"          // Author: Anders Rundgren                          //\n" +
	    "          //////////////////////////////////////////////////////\n" +
		"\n" +
		"namespace Org.Webpki.Es6Numbers\n" +
		"{\n" +
		"    internal class Base10Lookup\n" +
		"    {\n" +
		"        // According to IEEE-754\n" +
		"        internal const int EXPONENT_OFFSET = "  + EXPONENT_OFFSET + ";\n" +
		"\n" +
		"        // What to divide the mantissa with\n" +
        "        internal decimal Divider;\n" +
		"\n" +
		"        // The Base10 exponent we (presumably) are looking for\n" +
        "        internal int Base10Exponent;\n" +
        "\n" +
        "        private Base10Lookup(decimal Divider, int Base10Exponent)\n" +
        "        {\n" +
        "            this.Divider = Divider;\n" +
        "            this.Base10Exponent = Base10Exponent;\n" +
        "        }\n" +
		"\n" +
        "        internal readonly static Base10Lookup[] Cache = {\n");

    public static void main(String[] args) throws Exception {
	    BigDecimal TWO = new BigDecimal(2, MathContext.DECIMAL128);
		BigDecimal mantissaSize = new BigDecimal(MANTISSA_DIVIDER, MathContext.DECIMAL128);
	    for (int i = -EXPONENT_OFFSET; i <= EXPONENT_OFFSET; i++) {
		    int exp = 0;
			BigDecimal v = BigDecimal.ONE;
			if (i < 0) {
				for (int q = i; q < 0; q++) {
					v = v.divide(TWO);
				}
			} else for (int q = 0; q < i; q++) {
				v = v.multiply(TWO);
			}
			while (v.compareTo(BigDecimal.ONE) < 0) {
				exp--;
				v = v.multiply(BigDecimal.TEN, MathContext.DECIMAL128);
			}
			while (v.compareTo(BigDecimal.TEN) > 0) {
				exp++;
				v = v.divide(BigDecimal.TEN, MathContext.DECIMAL128);
			}
			String divider = mantissaSize.divide(v, MathContext.DECIMAL128).toPlainString();
			if (!divider.contains(".")) {
				divider += ".0";
			}
			s.append("            new Base10Lookup(").append(divider).append("m, ");
			for (int l = divider.length(); l < 36; l++) {
				s.append(' ');
			}
			String expString = String.valueOf(exp);
			s.append(exp).append(")").append(i == EXPONENT_OFFSET ? ' ' : ',');
			for (int l = expString.length(); l < 5; l++) {
				s.append(' ');
			}
			s.append("/* [").append(i + EXPONENT_OFFSET).append("] */\n");
		}
		s.append("        };\n    }\n}\n");
		System.out.println(s.toString());
		FileOutputStream file = new FileOutputStream(args[0]);
		file.write(s.toString().getBytes("utf-8"));
		file.close();
    }
}
