using System.Numerics;

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
        } public static CipheringMode Ciphering_Mode;
        
        public static bitsSplit128To64(ulong[] bits_input)
        {
            BigInteger bits = new BigInteger(128);
            
            bits
        }
    }
}