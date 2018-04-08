import java.io.FileOutputStream;

import java.math.BigDecimal;
import java.math.MathContext;

import java.lang.Math;

public class Base2ExponentGenerator {

    static final int EXPONENT_OFFSET = 350;

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
	    "//////////////////////////////////////////////////////\n" +
		"// For quick lookup of 10base to IEEE-754 exponents //\n" +
		"// Index: Base10 exponent                           //\n" +
		"//                                                  //\n" +
		"// Author: Anders Rundgren                          //\n" +
	    "//////////////////////////////////////////////////////\n" +
		"\n" +
		"namespace Org.Webpki.Es6Numbers\n" +
		"{\n" +
		"    internal class Base2Lookup\n" +
		"    {\n" +
		"        internal const int EXPONENT_OFFSET = "  + EXPONENT_OFFSET + ";\n" +
		"\n" +
		"        // What to multiply the mantissa with\n" +
        "        internal decimal MantissaMultiplier;\n" +
		"\n" +
		"        // The Base2 exponent we (presumably) are looking for\n" +
        "        internal int Base2Exponent;\n" +
        "\n" +
        "        private Base2Lookup(decimal MantissaMultiplier, int Base2Exponent)\n" +
        "        {\n" +
        "            this.MantissaMultiplier = MantissaMultiplier;\n" +
        "            this.Base2Exponent = Base2Exponent;\n" +
        "        }\n" +
		"\n" +
        "        internal readonly static Base2Lookup[] Cache = {\n");

    public static void main(String[] args) throws Exception {
	    BigDecimal TWO = new BigDecimal(2, MathContext.DECIMAL128);
		boolean next = false;
	    for (int i = -EXPONENT_OFFSET; i < EXPONENT_OFFSET; i++) {
		    int exp = 1023;
			BigDecimal v = BigDecimal.ONE;
			if (i < 0) {
				for (int q = i; q < 0; q++) {
					v = v.divide(BigDecimal.TEN);
				}
			} else for (int q = 0; q < i; q++) {
				v = v.multiply(BigDecimal.TEN);
			}
			while (v.compareTo(BigDecimal.ONE) < 0) {
				exp--;
				v = v.multiply(TWO, MathContext.DECIMAL128);
			}
			while (v.compareTo(TWO) > 0) {
				exp++;
				v = v.divide(TWO, MathContext.DECIMAL128);
			}
			if (next) {
				s.append(",\n");
			}
			next = true;
			String mantissaMultiplier = v.toString();
			s.append("            new Base2Lookup(").append(mantissaMultiplier).append("m, ");
			for (int l = mantissaMultiplier.length(); l < 36; l++) {
				s.append(' ');
			}
			s.append(exp).append(") /* [").append(i + EXPONENT_OFFSET).append("] */");
		}
		s.append("\n        };\n    }\n}\n");
		System.out.println(s.toString());
		FileOutputStream file = new FileOutputStream(args[0]);
		file.write(s.toString().getBytes("utf-8"));
		file.close();
    }
}
