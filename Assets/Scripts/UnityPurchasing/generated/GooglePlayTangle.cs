// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Q7rMkY+GS4wzUhwM+XJrazJ5HoKiMU69q/LLvt3EWFGn7hKmfcYFXJmGAEkvVy9p4OwDpEqHb7Pfy8WLVa1uhnuies8wgqLDVmPiO94Ap2Esnh0+LBEaFTaaVJrrER0dHRkcH61d2bw4I9IYbCWqk+ukI7gnB4Lq0O/039QYqWGEnoY9Jek6avKN6VueHRMcLJ4dFh6eHR0cxwkZM3r8Ma3ZmJy6jQ8CkZRYc80Zgn5gldO4PH7z7JN1C1WF5n1QlUY1Ec80Yh6kna+/tEwhL5+76MJYGP8PsR48HCsej/hktUnePu4/3NLssMNqO7Wc/QhyBHz4VNPmLCgdK3oyix2N4xxDU2+7FXFItRJ445u4MeMRWj/KD4KWrgRfncArpx4fHRwd");
        private static int[] order = new int[] { 13,10,7,12,13,8,12,10,11,10,11,13,12,13,14 };
        private static int key = 28;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
