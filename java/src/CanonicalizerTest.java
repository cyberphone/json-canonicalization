import java.io.File;

import org.webpki.json.JSONParser;
import org.webpki.json.JSONOutputFormats;

import org.webpki.util.ArrayUtil;

public class CanonicalizerTest {

    static String inputDirectory;
    static String outputDirectory;

    static void performOneTest(String fileName) throws Exception {
        byte[] rawInput = ArrayUtil.readFile(inputDirectory + File.separator + fileName);
        byte[] expected = ArrayUtil.readFile(outputDirectory + File.separator + fileName);
        byte[] actual = JSONParser.parse(rawInput).serializeToBytes(JSONOutputFormats.CANONICALIZED);
        System.out.println ("Result for file: " + fileName + "=" + (ArrayUtil.compare(expected, actual) ? "SUCCESS" : "FAIL"));
    }

    public static void main(String[] args) throws Exception {
        inputDirectory = args[0] + File.separator + "input";
        outputDirectory = args[0] + File.separator + "output";
        JSONParser.setStrictNumericMode(false);
        if (args.length == 1) {
            File[] files = new File(inputDirectory).listFiles();
            for (File f : files) {
                performOneTest(f.getName());
            }
        } else {
            performOneTest(args[1]);
        }
    }
}