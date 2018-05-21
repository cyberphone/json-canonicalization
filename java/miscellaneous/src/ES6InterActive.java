import java.util.Scanner;

import org.webpki.jcs.NumberToJSON;

public class ES6InterActive {
    
    public static void main(String[] args) throws Exception {
        Scanner input = new Scanner(System.in);
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
                d = new Double(inputFp);
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
            String outputFp = NumberToJSON.serializeNumber(d);
            System.out.println("\nInput floating point: " + inputFp);
            System.out.println("Output floating point: " + outputFp);
            System.out.println("Hex value: " + ieeeHex);
            System.out.println("Binary value: " + ieeeBin);
        }
    }
}
