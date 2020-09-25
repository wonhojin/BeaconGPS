using UnityEngine;
using System;
using System.Collections;
public class Beacondistance : MonoBehaviour{ 

	public static int PROXIMITY_IMMEDIATE = 1;

	public static int PROXIMITY_NEAR = 2;

	public static int PROXIMITY_FAR = 3;
	
	public static int PROXIMITY_UNKNOWN = 0;

   private static char[] hexArray = {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f'};
	private static string TAG = "IBeacon";	
	
 
	protected String proximityUuid;

	protected int major;

	protected int minor;

	protected int proximity;

	protected Double accuracy;

	protected int rssi;

	protected int txPower;

	protected Double runningAverageRssi = null;

	public double getAccuracy() {
		if (accuracy == null) {
			accuracy = calculateAccuracy(txPower, runningAverageRssi != null ? runningAverageRssi : rssi );		
		}
		return accuracy;
	}

	public int getMajor() {
		return major;
	}

	public int getMinor() {
		return minor;
	}

	public int getProximity() {
		if (proximity == null) {
			proximity = calculateProximity(getAccuracy());		
		}
		return proximity;		
	}

	public int getRssi() {
		return rssi;
	}

	public int getTxPower() {
		return txPower;
	}

	public String getProximityUuid() {
		return proximityUuid;
	}
	
	
	@Override
	public int hashCode() {
		return minor;
	}

	@Override
	public override bool Equals(object that)
	{
		if (!(that is IBeacon))
		{
			return false;
		}
		IBeacon thatIBeacon = (IBeacon) that;
		return (thatIBeacon.Major == this.Major && thatIBeacon.Minor == this.Minor && thatIBeacon.ProximityUuid == thatIBeacon.ProximityUuid);
	}


	public static IBeacon fromScanData(byte[] scanData, int rssi) {

		
		if (((int)scanData[5] & 0xff) == 0x4c &&
			((int)scanData[6] & 0xff) == 0x00 &&
			((int)scanData[7] & 0xff) == 0x02 &&
			((int)scanData[8] & 0xff) == 0x15) {	}

		
		
		else if (((int)scanData[5] & 0xff) == 0x2d &&
				((int)scanData[6] & 0xff) == 0x24 &&
				((int)scanData[7] & 0xff) == 0xbf &&
				((int)scanData[8] & 0xff) == 0x16) {	
			
			IBeacon iBeacon = new IBeacon();
			iBeacon.major = 0;
			iBeacon.minor = 0;
			iBeacon.proximityUuid = "00000000-0000-0000-0000-000000000000";
			iBeacon.txPower = -55;
			return iBeacon;
		}		
		else {
			
			 System.Console.WriteLine(TAG, "This is not an iBeacon advertisment.  The bytes I see are: "+bytesToHex(scanData));
			return null;
		}
								
		IBeacon iBeacon = new IBeacon();
		
		iBeacon.major = (scanData[25] & 0xff) * 0x100 + (scanData[26] & 0xff);
		iBeacon.minor = (scanData[27] & 0xff) * 0x100 + (scanData[28] & 0xff);
		iBeacon.txPower = (int)scanData[29]; // this one is signed
		iBeacon.rssi = rssi;
				

		byte[] proximityUuidBytes = new byte[16];
		System.arraycopy(scanData, 9, proximityUuidBytes, 0, 16); 
		String hexString = bytesToHex(proximityUuidBytes);
		StringBuilder sb = new StringBuilder();
		sb.append(hexString.substring(0,8));
		sb.append("-");
		sb.append(hexString.substring(8,12));
		sb.append("-");
		sb.append(hexString.substring(12,16));
		sb.append("-");
		sb.append(hexString.substring(16,20));
		sb.append("-");
		sb.append(hexString.substring(20,32));
		iBeacon.proximityUuid = sb.toString();

		return iBeacon;
	}

	protected IBeacon(IBeacon otherIBeacon) {
		this.major = otherIBeacon.major;
		this.minor = otherIBeacon.minor;
		this.accuracy = otherIBeacon.accuracy;
		this.proximity = otherIBeacon.proximity;
		this.rssi = otherIBeacon.rssi;
		this.proximityUuid = otherIBeacon.proximityUuid;
		this.txPower = otherIBeacon.txPower;
	}
	
	protected IBeacon() {
		
	}
	
	protected static double calculateAccuracy(int txPower, double rssi) {
		if (rssi == 0) {
			return -1.0; // if we cannot determine accuracy, return -1.
		}
		
		System.Console.WriteLine(TAG, "calculating accuracy based on rssi of "+rssi);


		double ratio = rssi*1.0/txPower;
		if (ratio < 1.0) {
			return Math.pow(ratio,10);
		}
		else {
			double accuracy =  (0.89976)*Math.pow(ratio,7.7095) + 0.111;	
			System.Console.WriteLine(TAG, " avg rssi: "+rssi+" accuracy: "+accuracy);
			return accuracy;
		}
	}	
	
	protected static int calculateProximity(double accuracy) {
		if (accuracy < 0) {
			return PROXIMITY_UNKNOWN;	 
			
		}
		if (accuracy < 0.5 ) {
			return IBeacon.PROXIMITY_IMMEDIATE;
		}
		
		if (accuracy <= 4.0) { 
			return IBeacon.PROXIMITY_NEAR;
		}
		
		return IBeacon.PROXIMITY_FAR;

	}

	private static String bytesToHex(byte[] bytes) {
        char[] hexChars = new char[bytes.length * 2];
        int v;
        for ( int j = 0; j < bytes.length; j++ ) {
            v = bytes[j] & 0xFF;
            hexChars[j * 2] = hexArray[v >>> 4];
            hexChars[j * 2 + 1] = hexArray[v & 0x0F];
        }
        return new String(hexChars);
    } 
}