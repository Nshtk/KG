using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
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
        }
        
        public enum PaddingType
        {
            NONE,
            ZERO,
            RKCS7
        }
        
        public const ulong MASK8 =0xff;
        public const ulong MASK32=0xffffffff;
        public const ulong MASK64=0xffffffffffffffff;
        
        public static readonly Random Rng;
        
        static Utility()
        {
            Rng=new Random();
        }

        public static BigInteger getNBitMask(int n)
        {
            return BigInteger.One<<n-1;
        }
        public static BigInteger getRandomNBitNumber(int n)
        {
            BigInteger t=1;
            n--;
            return (t<<n)|(Rng.Next()%(t<<n));
        }
        public static BigInteger getRandomBigInteger(BigInteger min, BigInteger max)
        {
            byte[] max_bytes=max.ToByteArray();
            
            Rng.NextBytes(max_bytes);
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
        public static BigInteger circularShiftBigInteger(BigInteger number, int shift_bits)
        {
            return number<<shift_bits | number>>(number.ToByteArray().Length*8-shift_bits);
        }
        public static ulong bigIntegerConvertToUlong(BigInteger number)
        {
            return number<0 ? (ulong)Int64.Parse(number.ToString()) : UInt64.Parse(number.ToString());
        }
        
        public static void pad(ref byte[] bytes, int block_length, PaddingType padding_type=PaddingType.ZERO, int number_bytes_to_pad=0)
        {
            byte value;
            switch(padding_type)
            {
                case PaddingType.ZERO:
                    value=0;
                    break;
                case PaddingType.RKCS7:
                    value=(byte)(block_length-bytes.Length);
                    break;
                default:
                    return;
            }
            if(number_bytes_to_pad==0)
            {
                byte[] bytes_padded=new byte[block_length];

                bytes.CopyTo(bytes_padded, 0);
                for(int i=bytes.Length; i<block_length; i++)
                    bytes_padded[i]=value;
                bytes=bytes_padded;
            }
            else
                for(int i=bytes.Length-number_bytes_to_pad; i<bytes.Length; i++)
                    bytes[i]=value;
        }
        public static byte[][] toArray2D(this byte[] bytes, int part_length, PaddingType padding_type=PaddingType.NONE)
        {
            int parts_number=bytes.Length/part_length, remainder=bytes.Length%part_length;
            byte[][] bytes_2d=remainder==0 ?new byte[parts_number][] :new byte[parts_number+1][];;

            int i=0, j;
            for ( ; i<parts_number; i++)
            {
                bytes_2d[i]=new byte[part_length];
                for(j=0; j<part_length; j++)
                    bytes_2d[i][j]=bytes[i*part_length+j];
            }

            if(remainder!=0)
            {
                bytes_2d[parts_number]=new byte[part_length];
                for(i=0, j=parts_number*part_length; i<remainder; i++, j++)
                    bytes_2d[parts_number][i]=bytes[j];
                if(padding_type!=PaddingType.NONE)
                    pad(ref bytes_2d[parts_number], part_length, padding_type, part_length-remainder);
            }
            
            return bytes_2d;
        }
    }

    /*public static class BigIntegerExtensions
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
    }*/
}