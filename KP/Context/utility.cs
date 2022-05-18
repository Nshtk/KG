using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace KP.Context
{
    public static class Utility
    {
        public enum CipheringMode
        {
            ECB,
            CBC,
            CFB,
            OFB,
            CTR,
            RD,
            RD_H
        } public static CipheringMode Ciphering_Mode;
        
        public const ulong MASK8=0xff;
        public const ulong MASK32=0xffffffff;
        public const ulong MASK64=0xffffffffffffffff;
        
        public static readonly Random random;
        

        static Utility()
        {
            random=new Random();
        }
        
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
        public static BigInteger byteArrayConvertToBigInteger(byte[] bytes, int start, int end)
        {
            BigInteger result=0;

            for(int i=start; i<end; i++)
                result=(result<<8)|bytes[i];

            return result;
        }

        public static ulong BigIntegerConvertToUlong(BigInteger number)
        {
            return number<0 ? (ulong)Int64.Parse(number.ToString()) : UInt64.Parse(number.ToString());
        }

        public static BigInteger circularShiftBigInteger(BigInteger number, int shift_bits)
        {
            return number<<shift_bits | number>>(number.ToByteArray().Length*8-shift_bits);
        }
    }

    public static class BigIntegerExtensions
    {
        public static string ToStringBinary(this BigInteger bigint)
        {
            var bytes=bigint.ToByteArray();
            var idx=bytes.Length-1;

            var base2=new StringBuilder(bytes.Length*8);
            var binary=Convert.ToString(bytes[idx], 2);
            if(binary[0]!='0' && bigint.Sign==1)
                base2.Append('0');
            base2.Append(binary);
            for(idx--; idx>=0; idx--)
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));

            return base2.ToString();
        }

        public static BigInteger ToUnsigned(this BigInteger number)
        {
            
            
            return new BigInteger(number.ToByteArray().Concat(new byte[] {0}).ToArray());
            /*byte[] bytes_signed=number.ToByteArray();
            byte[] bytes_unsigned=new byte[bytes_signed.Length];

            Array.Copy(bytes_signed, 0, bytes_unsigned, 0, bytes_signed.Length);
            bytes_unsigned[bytes_signed.Length-1]=0;

            return new BigInteger(bytes_unsigned);*/
        }
    }
}