import java.io.FileReader;
import java.io.FileWriter;
import java.io.BufferedReader;

import java.util.LinkedHashMap;

import org.webpki.jcs.NumberToJSON;

public class AddEdgeCases {
    
    static final int TURNS = 10000000;

    static LinkedHashMap<String,String> ieeeValues = new LinkedHashMap<String,String>();
    
    static void add(long exponent, long mantissa) throws Exception {
        long value = (exponent << 52) + mantissa;
        String ieeeHex = Long.toHexString(value);
        Double ieeeDouble = Double.longBitsToDouble(value);
        if (ieeeDouble == 0 || ieeeDouble.isNaN() || ieeeDouble.isInfinite()) {
            System.out.println("Dropped value: " + ieeeHex);
            return;
        }
        String es6Number = NumberToJSON.serializeNumber(ieeeDouble);
        System.out.println("New " + ieeeHex + " " + es6Number);
        ieeeValues.put(ieeeHex, es6Number);
    }

    public static void main(String[] args) throws Exception {
// Add something
/*
        ieeeValues.put("3eb0c6f7a0b5ed8d", "0.000001");
        add(5, 70l);
*/
        BufferedReader in = new BufferedReader(new FileReader(args[0]));
        int q = 0;
        long total = 0;
        while (true) {
            String s = in.readLine();
            if (s == null) {
                in.close();
                System.out.println("\nTest was successful");
                if (ieeeValues.isEmpty()) {
                    System.out.println("\nThere were no new values to add");
                    return;
                }
                FileWriter writer = new FileWriter(args[0], true);
                for (String ieeeHex : ieeeValues.keySet()) {
                    writer.write(ieeeHex + "," + ieeeValues.get(ieeeHex) + "\n");
                }
                writer.close();
                return;
            }
            String hex = s.substring(0, s.indexOf(','));
            String text = s.substring(s.indexOf(',') + 1);
            if (ieeeValues.containsKey(hex)) {
                System.out.println("Duplicate: " + hex);
                ieeeValues.remove(hex);
            }
            while (hex.length() < 16) {
                hex = '0' + hex;
            }
            double d = Double.longBitsToDouble(Long.parseUnsignedLong(hex,16));
            String res = NumberToJSON.serializeNumber(d);
            if (!res.equals(text)) {
                System.out.println("FAIL res=" + res + " d=" + d);
                return;
            }
            total++;
            if (q++ == TURNS) {
                System.out.println("TURN:" + total + " " + text + " d=" + d);
                q = 0;
            }
        }
    }
}
