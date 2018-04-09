import java.util.Scanner;

import org.webpki.json.JSONObjectWriter;
import org.webpki.json.JSONParser;

public class ES6InterActive {
	
    public static void main(String[] args) throws Exception {
    	Scanner input = new Scanner(System.in);
    	JSONParser.setStrictNumericMode(false);
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
        		d = JSONParser.parse("[" + inputFp + "]").getJSONArrayReader().getDouble();
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
        	String outputFp = JSONObjectWriter.es6JsonNumberSerialization(d);
        	System.out.println("\nInput floating point: " + inputFp);
        	System.out.println("Output floating point: " + outputFp);
        	System.out.println("Hex value: " + ieeeHex);
        	System.out.println("Binary value: " + ieeeBin);
      	}
    }
}
