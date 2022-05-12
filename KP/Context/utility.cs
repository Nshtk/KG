using System;
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
        
        public static Random random;
        
        public static BigInteger getRandomNBitNumber(int n)
        {
            BigInteger t=1;
            n--;
            return (t<<n)|(random.Next()%(t<<n));
        }
        public static BigInteger getRandomBigInteger(BigInteger max, BigInteger min)
        {
            byte[] max_bytes=max.ToByteArray();
            
            random.NextBytes(max_bytes);
            max_bytes[max_bytes.Length-1]&=0x7F;
            
            return new BigInteger(max_bytes)%max+min;
        }
        public static BigInteger bytesConvertToBigInteger(byte[] bytes, int start, int end)
        {
            BigInteger result=0;

            for(int i=start; i<end; i++)
                result=(result<<8)|bytes[i];

            return result;
        }
    }
}