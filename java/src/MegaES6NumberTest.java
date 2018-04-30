import java.io.FileReader;
import java.io.BufferedReader;

import org.webpki.json.NumberToJSON;

public class MegaES6NumberTest {
	
	static final int TURNS = 1000000;

    public static void main(String[] args) throws Exception {
    	BufferedReader in = new BufferedReader(new FileReader(args[0]));
    	int q = 0;
    	long total = 0;
    	while (true) {
    		String s = in.readLine();
    		if (s == null) {
    			System.out.println("\nTest was successful");
    			return;
    		}
    		String hex = s.substring(0, s.indexOf(','));
    		while (hex.length() < 16) {
    			hex = '0' + hex;
    		}
    		double d = Double.longBitsToDouble(Long.parseUnsignedLong( hex,16));
    		String res = NumberToJSON.serializeNumber(d);
    		if (!res.equals(s.substring(s.indexOf(',') + 1))) {
    			System.out.println("FAIL res=" + res + " d=" + d);
    			return;
    		}
    		total++;
    		if (q++ == TURNS) {
    			System.out.println("TURN:" + total + " " + s + " d=" + d);
    			q = 0;
    		}
    	}
    }
}
