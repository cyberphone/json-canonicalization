import java.io.File;

import org.webpki.util.ArrayUtil;
import org.webpki.util.DebugFormatter;

public class BrowserCodeGenerator {

    static StringBuilder html = new StringBuilder(
        "<!DOCTYPE html><html><head><title>JSON Canonicalization</title>\n" +
        "<meta http-equiv=Content-Type content=\"3ext/html; charset=utf-8\">\n" +
        "<meta name=\"viewport\" content=\"width=400, initial-scale=1\">\n" +
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

    static StringBuilder table = new StringBuilder(
        "<table class=\"tftable\">");

    static String sanitize(byte[] rawUtf8) throws Exception {
        return new String(rawUtf8, "utf-8").trim()
            .replace("&", "&amp;")
            .replace("\"", "&quot;")
            .replace("<", "&lt;")
            .replace(">", "&gt;")
            .replace("\n","<br>")
            .replace(" ","&nbsp;")
            .replace("\u0080","\u25a1");
    }

    static StringBuilder createHex(byte[] values, boolean jsMode) {
        StringBuilder hex = new StringBuilder();
        boolean next = false;
        int byteCount = 0;
        for (byte b : values) {
            if (byteCount++ % 20 == 0 && next) {
                hex.append(jsMode ? ",\n" : "\n");
            } else if (next) {
                hex.append(jsMode ? ',' : ' ');
            }
            next = true;
            hex.append(jsMode ? "0x" : "")
               .append(DebugFormatter.getHexString(new byte[]{b}));
        }
        return hex;
    }

    static void createOneTest(String fileName) throws Exception {
        byte[] rawInput = ArrayUtil.readFile(inputDirectory + File.separator + fileName);
        byte[] rawExpected = ArrayUtil.readFile(outputDirectory + File.separator + fileName);
        if (nextTest) {
            html.append(',');
            table.append("<tr><td style=\"background-color:white;border-width:0px\">&nbsp;<br>&nbsp;</td></tr>");
        }
        html.append("{\n  fileName: '")
            .append(fileName)
            .append("',\n  inputData: new Uint8Array([")
            .append(createHex(rawInput, true))
            .append("]),\n  expectedData: new Uint8Array([")
            .append(createHex(rawExpected, true))
            .append("])\n}");
        table.append("<tr><th>JSON Input File: ")
             .append(fileName)
             .append("</th><tr><td style=\"white-space:nowrap\">")
             .append(sanitize(rawInput))
             .append("</td></tr><tr><th>Expected Result / UTF-8</th></tr><tr><td style=\"word-break:break-all\">")
             .append(sanitize(rawExpected))
             .append("</td></tr>\n<tr><td><code>")
             .append(createHex(rawExpected, false))
             .append("</code></td></tr>");
        nextTest = true;
    }

    public static void main(String[] args) throws Exception {
        inputDirectory = args[0] + File.separator + "input";
        outputDirectory = args[0] + File.separator + "output";
        File[] files = new File(inputDirectory).listFiles();
        for (File f : files) {
            createOneTest(f.getName());
        }
        html.append("];\n\n");
        html.append(
            "function failed(element) {\n" +
            "    document.getElementById('message\').innerHTML = '<span style=\"color:red\">Failed for file: ' + element.fileName + '</span>';\n" +
            "    throw new Error(element.fileName);\n" +
            "}\n\n" +
            "var testNumber = 0;\n" +
            "function oneFile() {\n" +
            "  if (testNumber < tests.length) {\n" +
            "    var element = tests[testNumber++];\n" +
            "    document.getElementById('message\').innerHTML = 'Processing: <span style=\"color:blue\">' + element.fileName + '</span>';\n" +
            "    setTimeout(function() {\n" +
            "      var jsonData = JSON.parse(new TextDecoder().decode(element.inputData));\n" +
            "      var canonicalizedJson = new TextEncoder().encode(canonicalize(jsonData));\n" +
//          "      console.debug(element.expectedData);\n" +
//          "      console.debug(canonicalizedJson);\n" +
            "      if (canonicalizedJson.length != element.expectedData.length) {\n" +
            "        failed(element);\n" +
            "      }\n" +
            "      for (let i = 0; i < canonicalizedJson.length; i++) {\n" +
            "        if (canonicalizedJson[i] != element.expectedData[i]) {\n" +
            "          failed(element);\n" +
            "        }\n" +
            "      }\n" +
            "      oneFile();\n" +
            "    }, 1000);\n" +
            "  } else {\n" +
            "    document.getElementById('message\').innerHTML = '<span style=\"color:green\">All Tests Passed</span>';\n" +
            "  }\n" +
            "}\n\n" +
            "function testing() {\n" +
            "  if (typeof TextEncoder !== 'function') {\n" +
            "    document.getElementById('message\').innerHTML = 'Your browser does not support TextEncoder &#9785;';\n" +
            "    return;\n" +
            "  }\n" +
            "  oneFile();\n" +
            "}\n" +
            "</script>\n" +
            "<img style=\"cursor:pointer;position:absolute;top:5pt;right:10pt;z-index:5\"" +
            " onclick=\"document.location.href='https://github.com/cyberphone/json-canonicalization'\" title=\"Project Home\"" +
            " src=\"https://cyberphone.github.io/doc/security/jcs.svg\">" +
            "<div style=\"font-size:14pt;padding:10pt 0pt\">JSON Canonicalization Test</div>\n" +
            "<div style=\"font-weight:bold;padding-bottom:10pt\" id=\"message\"></div>\n")
        .append(table)
        .append("</table></body></html>");
        ArrayUtil.writeFile(args[1], html.toString().getBytes("utf-8"));
    }
}
