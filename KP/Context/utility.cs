namespace KP.Context
{
    public static class Utility
    {
        public enum CipheringMode
        {
            CipheringMode_ECB,
            CipheringMode_CBC,
            CipheringMode_CFB,
            CipheringMode_OFB,
            CipheringMode_CTR,
            CipheringMode_RD,
            CipheringMode_RD_H
        }
        
        public class Bits
        {
            
        }

        public static CipheringMode Ciphering_Mode;
    }
}