import java.io.File;

import org.webpki.json.JSONParser;
import org.webpki.json.JSONOutputFormats;

import org.webpki.util.ArrayUtil;
import org.webpki.util.DebugFormatter;

public class BrowserCodeGenerator {

    static StringBuilder html = new StringBuilder(
		"<!DOCTYPE html><html><head><title>JSON Canonicalization</title>\n" +
		"<meta http-equiv=Content-Type content=\"text/html; charset=utf-8\">\n" +
		"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n" +
		"<link rel=\"icon\" href=\"../webpkiorg.png\" sizes=\"192x192\"><style type=\"text/css\">\n" +
		".tftable {border-collapse: collapse}\n" +
		".tftable th {font-size:10pt;background: linear-gradient(to bottom, #eaeaea 14%," +
		"#fcfcfc 52%,#e5e5e5 89%);border-width:1px;padding:4pt 10pt 4pt 10pt;border-style:solid;" +
		"border-color: #a9a9a9;text-align:center;font-family:arial,verdana,helvetica}\n" +
		".tftable tr {background-color:#FFFFE0}\n" +
		".tftable td {font-size:10pt;border-width:1px;padding:4pt 8pt 4pt 8pt;border-style:solid;" +
		"border-color:#a9a9a9;font-family:arial,verdana,helvetica}\n" +
		"body {font-size:10pt;font-family:arial,verdana,helvetica}\n" +
		"a {color:blue;font-family:verdana,helvetica;text-decoration:none}\n" +
		"</style></head><body onload=\"testing()\">\n" +
		"<script>\n" +
		"'use strict';\n" +
		"\n" +
		"var canonicalize = function(object) {\n" +
		"\n" +
		"    var buffer = '';\n" +
		"    serialize(object);\n" +
		"    return buffer;\n" +
		"\n" +
		"    function serialize(object) {\n" +
		"        if (object !== null && typeof object === 'object') {\n" +
		"            if (Array.isArray(object)) {\n" +
		"                buffer += '[';\n" +
		"                let next = false;\n" +
		"                // Array - Maintain element order\n" +
		"                object.forEach((element) => {\n" +
		"                    if (next) {\n" +
		"                        buffer += ',';\n" +
		"                    }\n" +
		"                    next = true;\n" +
		"                    // Recursive call\n" +
		"                    serialize(element);\n" +
		"                });\n" +
		"                buffer += ']';\n" +
		"            } else {\n" +
		"                buffer += '{';\n" +
		"                let next = false;\n" +
		"                // Object - Sort properties before serializing\n" +
		"                Object.keys(object).sort().forEach((property) => {\n" +
		"                    if (next) {\n" +
		"                        buffer += ',';\n" +
		"                    }\n" +
		"                    next = true;\n" +
		"                    // Properties are just strings - Use ES6\n" +
		"                    buffer += JSON.stringify(property);\n" +
		"                    buffer += ':';\n" +
		"                    // Recursive call\n" +
		"                    serialize(object[property]);\n" +
		"                });\n" +
		"                buffer += '}';\n" +
		"            }\n" +
		"        } else {\n" +
		"            // Primitive data type - Use ES6\n" +
		"            buffer += JSON.stringify(object);\n" +
		"        }\n" +
		"    }\n" +
		"};\n" +
		"\n" +
		"const tests = [");

    static String inputDirectory;
    static String outputDirectory;

	static boolean nextTest = false;

    static void createOneTest(String fileName) throws Exception {
        String rawInput = new String(ArrayUtil.readFile(inputDirectory + File.separator + fileName), "utf-8");
		String expected = "";
		String visualExpected = "";
		boolean next = false;
		for (byte b : ArrayUtil.readFile(outputDirectory + File.separator + fileName)) {
			if (next) {
				expected += ',';
				visualExpected += ' ';
			}
			next = true;
			String hex = DebugFormatter.getHexString(new byte[]{b});
			expected += "0x" + hex;
			visualExpected += hex;
		}
		if (nextTest) {
			html.append(',');
		}
		nextTest = true;
		html.append("{\n  inputData: ")
			.append(rawInput)
			.append(",\n  expectedData: new Uint8Array([")
			.append(expected)
		    .append("])\n}");
    }

    public static void main(String[] args) throws Exception {
        inputDirectory = args[0] + File.separator + "input";
        outputDirectory = args[0] + File.separator + "output";
        JSONParser.setStrictNumericMode(false);
        File[] files = new File(inputDirectory).listFiles();
        for (File f : files) {
            createOneTest(f.getName());
        }
		html.append("];\n\n");
		html.append(
		    "function testing() {\n" +
			"  if (typeof TextDecoder !== 'function') {\n" +
			"    document.getElementById('message\').innerHTML = '<b>Your browser does not support TextDecoder &#9785;</b>';\n" +
			"    return;\n" +
			"  }\n" +
			"  tests.forEach((element) => {\n" +
			"    console.debug(element.expectedData);\n" +
			"  });\n" +
			"}\n" +
			"</script>\n" +
			"<div id=\"message\"></div>\n" +
			"<div>HI</div>" +
			"</body></html>");
		ArrayUtil.writeFile(args[1], html.toString().getBytes("utf-8"));
    }
}
