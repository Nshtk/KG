using System;
using System.Collections.Generic;
using System.Numerics;
using KP.Context.Interface;

namespace KP.Context
{
    public class KeyExpansionCamellia : IKeyExpansion
    {
        public readonly List<BigInteger> _constants = new List<BigInteger>(6) {
            0xA09E667F3BCC908B,
            0xB67AE8584CAA73B2,
            0xC6EF372FE94F82BE,
            0x54FF53A5F1D36F1C,
            0x10E527FADE682D1D,
            0xB05688C2B3E6C1FD
        };
        
        public RoundCipheringCamelia _round_ciphering;

        public KeyExpansionCamellia()
        {
            _round_ciphering=new RoundCipheringCamelia();
        }
        
        /*public byte[][] getKeysRound(byte[] key)
        {
            BigInteger key_bits=new BigInteger(key), key_part_l_bits, key_part_r_bits;
            BigInteger ka, kb;
            ulong[] kw=new ulong[4], ke=new ulong[6], k=new ulong[24], d=new ulong[2];
            byte[][] keys_round;

            switch(key.Length)
            {
                case 16:
                    keys_round=new byte[26][];
                    key_part_l_bits=key_bits;
                    key_part_r_bits=0;
                    break;
                case 24:
                    keys_round=new byte[34][];
                    key_part_l_bits=key_bits>>64;
                    key_part_r_bits=((key_bits&Utility.MASK64)<<64) | (~(key_bits&Utility.MASK64));
                    break;
                case 32:
                    keys_round=new byte[34][];
                    key_part_l_bits=key_bits>>128;
                    key_part_r_bits=key_bits&BigInteger.Parse("10000000000000000000000000000000");
                    break;
                default:
                    /*key_part_l_bits=key_bits;             //dummy
                    key_part_r_bits=0;#1#
                    return null;
            }

            d[0] = BitConverter.ToUInt64(((key_part_l_bits ^ key_part_r_bits) >> 64).ToByteArray(), 0);      // TODO place inside functoion
            d[1] = BitConverter.ToUInt64(((key_part_l_bits ^ key_part_r_bits) & Utility.MASK64).ToByteArray(), 0);
            d[1] ^= _round_ciphering.functionF(d[0], (ulong)_constants[0]);
            d[0] ^= _round_ciphering.functionF(d[1], (ulong)_constants[1]);
            d[0] ^= BitConverter.ToUInt64((key_part_l_bits >> 64).ToByteArray(), 0);
            d[1] ^= BitConverter.ToUInt64((key_part_l_bits & Utility.MASK64).ToByteArray(), 0);
            d[1] ^= _round_ciphering.functionF(d[0], (ulong)_constants[2]);
            d[0] ^= _round_ciphering.functionF(d[1], (ulong)_constants[3]);
            ka = ((BigInteger)d[0] << 64) | d[1];
            d[0] = BitConverter.ToUInt64(((ka ^ key_part_r_bits) >> 64).ToByteArray(), 0);
            d[1] = BitConverter.ToUInt64(((ka ^ key_part_r_bits) & Utility.MASK64).ToByteArray(), 0);
            d[1] ^= _round_ciphering.functionF(d[0], (ulong)_constants[4]);
            d[0] ^= _round_ciphering.functionF(d[1], (ulong)_constants[5]);
            kb = ((BigInteger)d[0] << 64) | d[1];

            if(key.Length==16)                                          // TODO place inside functoion
            {
                kw[0] = BitConverter.ToUInt64(((key_part_l_bits << 0) >> 64).ToByteArray(), 0);
                kw[1] = BitConverter.ToUInt64(((key_part_l_bits << 0) & Utility.MASK64).ToByteArray(), 0);
                k[0]  = BitConverter.ToUInt64(((ka << 0) >> 64).ToByteArray(), 0);
                k[1]  = BitConverter.ToUInt64(((ka << 0) & Utility.MASK64).ToByteArray(), 0);
                k[2]  = BitConverter.ToUInt64(((key_part_l_bits << 15 | key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-15)) >> 64).ToByteArray(), 0);
                k[3]  = BitConverter.ToUInt64(((key_part_l_bits <<  15 | key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-15)) & Utility.MASK64).ToByteArray(), 0);
                k[4]  = BitConverter.ToUInt64(((ka <<  15 | ka>>(ka.ToByteArray().Length-15)) >> 64).ToByteArray(), 0);
                k[5]  = BitConverter.ToUInt64(((ka <<  15| ka>>(ka.ToByteArray().Length-15)) & Utility.MASK64).ToByteArray(), 0);
                k[6]  = BitConverter.ToUInt64(((key_part_l_bits <<  45| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-45)) >> 64).ToByteArray(), 0);
                k[7]  = BitConverter.ToUInt64(((key_part_l_bits <<  45| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-45)) & Utility.MASK64).ToByteArray(), 0);
                k[8]  = BitConverter.ToUInt64(((ka <<  45| ka>>(ka.ToByteArray().Length-45)) >> 64).ToByteArray(), 0);
                k[9] = BitConverter.ToUInt64(((key_part_l_bits <<  60| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-60)) & Utility.MASK64).ToByteArray(), 0);
                k[10] =BitConverter.ToUInt64(( (ka <<  60| ka>>(ka.ToByteArray().Length-60)) >> 64).ToByteArray(), 0);
                k[11] =BitConverter.ToUInt64(( (ka <<  60| ka>>(ka.ToByteArray().Length-60)) & Utility.MASK64).ToByteArray(), 0);
                k[12] =BitConverter.ToUInt64(( (key_part_l_bits <<  94| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-94)) >> 64).ToByteArray(), 0);
                k[13] =BitConverter.ToUInt64(( (key_part_l_bits <<  94| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-94)) & Utility.MASK64).ToByteArray(), 0);
                k[14] =BitConverter.ToUInt64(( (ka <<  94| ka>>(ka.ToByteArray().Length-94)) >> 64).ToByteArray(), 0);
                k[15] =BitConverter.ToUInt64(( (ka <<  94| ka>>(ka.ToByteArray().Length-94)) & Utility.MASK64).ToByteArray(), 0);
                k[16] = BitConverter.ToUInt64(((key_part_l_bits << 111| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-111)) >> 64).ToByteArray(), 0);
                k[17] = BitConverter.ToUInt64(((key_part_l_bits << 111| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-111)) & Utility.MASK64).ToByteArray(), 0);
                ke[0] =BitConverter.ToUInt64(( (ka <<  30| ka>>(ka.ToByteArray().Length-30)) >> 64).ToByteArray(), 0);
                ke[1] = BitConverter.ToUInt64(((ka <<  30| ka>>(ka.ToByteArray().Length-30)) & Utility.MASK64).ToByteArray(), 0);
                ke[2] =BitConverter.ToUInt64(( (key_part_l_bits <<  77| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-77)) >> 64).ToByteArray(), 0);
                ke[3] =BitConverter.ToUInt64(( (key_part_l_bits <<  77| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-77)) & Utility.MASK64).ToByteArray(), 0);
                kw[2] = BitConverter.ToUInt64(((ka << 111| ka>>(ka.ToByteArray().Length-111)) >> 64).ToByteArray(), 0);
                kw[3] = BitConverter.ToUInt64(((ka << 111| ka>>(ka.ToByteArray().Length-111)) & Utility.MASK64).ToByteArray(), 0);
            }
            else
            {
                kw[0] = BitConverter.ToUInt64(((key_part_l_bits << 0) >> 64).ToByteArray(), 0);
                kw[1] = BitConverter.ToUInt64(((key_part_l_bits << 0) & Utility.MASK64).ToByteArray(), 0);
                k[0]  = BitConverter.ToUInt64(((kb <<   0) >> 64).ToByteArray(), 0);
                k[1]  = BitConverter.ToUInt64(((kb <<   0) & Utility.MASK64).ToByteArray(), 0);
                k[2]  = BitConverter.ToUInt64(((key_part_r_bits <<  15| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-15)) >> 64).ToByteArray(), 0);
                k[3]  =BitConverter.ToUInt64(( (key_part_r_bits <<  15| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-15)) & Utility.MASK64).ToByteArray(), 0);
                k[4]  =BitConverter.ToUInt64(( (ka <<  15| ka>>(ka.ToByteArray().Length-15)) >> 64).ToByteArray(), 0);
                k[5]  =BitConverter.ToUInt64(( (ka <<  15| ka>>(ka.ToByteArray().Length-15)) & Utility.MASK64).ToByteArray(), 0);
                k[6]  =BitConverter.ToUInt64(( (kb <<  30| kb>>(kb.ToByteArray().Length-30)) >> 64).ToByteArray(), 0);
                k[7]  = BitConverter.ToUInt64(((kb <<  30| kb>>(kb.ToByteArray().Length-30)) & Utility.MASK64).ToByteArray(), 0);
                k[8]  =BitConverter.ToUInt64(( (key_part_l_bits <<  45| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-45)) >> 64).ToByteArray(), 0);
                k[9] =BitConverter.ToUInt64(( (key_part_l_bits <<  45| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-45)) & Utility.MASK64).ToByteArray(), 0);
                k[10] =BitConverter.ToUInt64(( (ka <<  45| ka>>(ka.ToByteArray().Length-45)) >> 64).ToByteArray(), 0);
                k[11] =BitConverter.ToUInt64(( (ka <<  45| ka>>(ka.ToByteArray().Length-45)) & Utility.MASK64).ToByteArray(), 0);
                k[12] =BitConverter.ToUInt64(( (key_part_r_bits <<  60| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-60)) >> 64).ToByteArray(), 0);
                k[13] =BitConverter.ToUInt64(( (key_part_r_bits <<  60| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-60)) & Utility.MASK64).ToByteArray(), 0);
                k[14] =BitConverter.ToUInt64(( (kb <<  60| kb>>(kb.ToByteArray().Length-60)) >> 64).ToByteArray(), 0);
                k[15] =BitConverter.ToUInt64(( (kb <<  60| kb>>(kb.ToByteArray().Length-60)) & Utility.MASK64).ToByteArray(), 0);
                k[16] =BitConverter.ToUInt64(( (key_part_l_bits <<  77| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-77)) >> 64).ToByteArray(), 0);
                k[17] =BitConverter.ToUInt64(( (key_part_l_bits <<  77| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-77)) & Utility.MASK64).ToByteArray(), 0);
                k[18] = BitConverter.ToUInt64(((key_part_r_bits <<  94| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-94)) >> 64).ToByteArray(), 0);
                k[19] = BitConverter.ToUInt64(((key_part_r_bits <<  94| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-94)) & Utility.MASK64).ToByteArray(), 0);
                k[20] =BitConverter.ToUInt64(( (ka <<  94| ka>>(ka.ToByteArray().Length-94)) >> 64).ToByteArray(), 0);
                k[21] =BitConverter.ToUInt64(( (ka <<  94| ka>>(ka.ToByteArray().Length-94)) & Utility.MASK64).ToByteArray(), 0);
                k[22] =BitConverter.ToUInt64(( (key_part_l_bits << 111| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-111)) >> 64).ToByteArray(), 0);
                k[23] = BitConverter.ToUInt64(((key_part_l_bits << 111| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-111)) & Utility.MASK64).ToByteArray(), 0);
                ke[0] = BitConverter.ToUInt64(((key_part_r_bits <<  30| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-30)) >> 64).ToByteArray(), 0);
                ke[1] = BitConverter.ToUInt64(((key_part_r_bits <<  30| key_part_r_bits>>(key_part_r_bits.ToByteArray().Length-30)) & Utility.MASK64).ToByteArray(), 0);
                ke[2] = BitConverter.ToUInt64(((key_part_l_bits <<  60| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-60)) >> 64).ToByteArray(), 0);
                ke[3] =BitConverter.ToUInt64(( (key_part_l_bits <<  60| key_part_l_bits>>(key_part_l_bits.ToByteArray().Length-60)) & Utility.MASK64).ToByteArray(), 0);
                ke[4] =BitConverter.ToUInt64(( (ka <<  77| ka>>(ka.ToByteArray().Length-77)) >> 64).ToByteArray(), 0);
                ke[5] =BitConverter.ToUInt64(( (ka <<  77| ka>>(ka.ToByteArray().Length-77)) & Utility.MASK64).ToByteArray(), 0);
                kw[2] = BitConverter.ToUInt64(((kb << 111| kb>>(kb.ToByteArray().Length-111)) >> 64).ToByteArray(), 0);
                kw[3] = BitConverter.ToUInt64(((kb << 111| kb>>(kb.ToByteArray().Length-111)) & Utility.MASK64).ToByteArray(), 0);
            }
            
            byte i=0, j;
            for(j=0; j<kw.Length; i++, j++) // TODO Utility.toArray()
                keys_round[i]=BitConverter.GetBytes(kw[i]);
            for(j=0; j<ke.Length; i++, j++)        // i<i+ke.length
                keys_round[i]=BitConverter.GetBytes(ke[j]);
            for(j=0; j<k.Length; i++, j++)
                keys_round[i]=BitConverter.GetBytes(k[j]);
            
            return keys_round;
        }*/
        public byte[][] getKeysRound(byte[] key)
        {
            BigInteger key_bits=new BigInteger(key), key_part_l_bits, key_part_r_bits;
            BigInteger ka, kb;
            BigInteger[] kw=new BigInteger[4], ke, k, d=new BigInteger[2];
            byte[][] keys_round;

            switch(key.Length)
            {
                case 16:
                    ke=new BigInteger[4]; k=new BigInteger[18];
                    keys_round=new byte[26][];
                    key_part_l_bits=key_bits;
                    key_part_r_bits=0;
                    break;
                case 24:
                    ke=new BigInteger[6]; k=new BigInteger[24];
                    keys_round=new byte[34][];
                    key_part_l_bits=key_bits>>64;
                    key_part_r_bits=((key_bits&Utility.MASK64)<<64) | (~(key_bits&Utility.MASK64));
                    break;
                case 32:
                    ke=new BigInteger[6]; k=new BigInteger[24];
                    keys_round=new byte[34][];
                    key_part_l_bits=key_bits>>128;
                    key_part_r_bits=key_bits&BigInteger.Parse("10000000000000000000000000000000");
                    break;
                default:
                    /*key_part_l_bits=key_bits;             //dummy
                    key_part_r_bits=0;*/
                    return null;
            }

            d[0]=(key_part_l_bits^key_part_r_bits) >> 64;      // TODO to function
            d[1]=(key_part_l_bits^key_part_r_bits) & Utility.MASK64;
            d[1]^=_round_ciphering.functionF(Utility.bigIntegerConvertToUlong(d[0]), (ulong)_constants[0]);        // why casting from biginteger is so broken
            d[0]^=_round_ciphering.functionF(Utility.bigIntegerConvertToUlong(d[1]), (ulong)_constants[1]);
            d[0]^=key_part_l_bits>>64;
            d[1]^=key_part_l_bits&Utility.MASK64;
            try
            {
                d[1]^=_round_ciphering.functionF(Utility.bigIntegerConvertToUlong(d[0]), (ulong)_constants[2]);
                d[0]^=_round_ciphering.functionF(Utility.bigIntegerConvertToUlong(d[1]), (ulong)_constants[3]);
            }
            catch
            {
                Console.WriteLine();
            }
            
            ka = ((BigInteger)d[0] << 64) | d[1];
            d[0] = ((ka ^ key_part_r_bits) >> 64);
            d[1] = ((ka ^ key_part_r_bits) & Utility.MASK64);
            d[1] ^= _round_ciphering.functionF(Utility.bigIntegerConvertToUlong(d[0]), (ulong)_constants[4]);
            d[0] ^= _round_ciphering.functionF(Utility.bigIntegerConvertToUlong(d[1]), (ulong)_constants[5]);
            kb = ((BigInteger)d[0] << 64) | d[1];

            if(key.Length==16)                                          // TODO to function
            {
                k[0] =ka>>64;
                k[1] =ka&Utility.MASK64;
                k[2] =Utility.circularShiftBigInteger(key_part_l_bits, 15)>>64&Utility.MASK64;
                k[3] =Utility.circularShiftBigInteger(key_part_l_bits, 15)&Utility.MASK64;
                k[4] =Utility.circularShiftBigInteger(ka, 15)>>64&Utility.MASK64;
                k[5] =Utility.circularShiftBigInteger(ka, 15)&Utility.MASK64;           //неверно
                k[6] =Utility.circularShiftBigInteger(key_part_l_bits, 45)>>64&Utility.MASK64;
                k[7] =Utility.circularShiftBigInteger(key_part_l_bits, 45)&Utility.MASK64;
                k[8] =Utility.circularShiftBigInteger(ka, 45)>>64&Utility.MASK64;
                k[9] =Utility.circularShiftBigInteger(key_part_l_bits, 60)&Utility.MASK64;
                k[10]=Utility.circularShiftBigInteger(ka, 60)>>64&Utility.MASK64;
                k[11]=Utility.circularShiftBigInteger(ka, 60)&Utility.MASK64;           //неверно
                k[12]=Utility.circularShiftBigInteger(key_part_l_bits, 94)>>64&Utility.MASK64; 
                k[13]=Utility.circularShiftBigInteger(key_part_l_bits, 94)&Utility.MASK64;
                k[14]=Utility.circularShiftBigInteger(ka, 94)>>64&Utility.MASK64;       //неверно
                k[15]=Utility.circularShiftBigInteger(ka, 94)&Utility.MASK64;
                k[16]=Utility.circularShiftBigInteger(key_part_l_bits, 111)>>64&Utility.MASK64;
                k[17]=Utility.circularShiftBigInteger(key_part_l_bits, 111)&Utility.MASK64;
                ke[0]=Utility.circularShiftBigInteger(ka, 30)>>64&Utility.MASK64;
                ke[1]=Utility.circularShiftBigInteger(ka, 30)&Utility.MASK64;           //неверно
                ke[2]=Utility.circularShiftBigInteger(key_part_l_bits, 77)>>64&Utility.MASK64;
                ke[3]=Utility.circularShiftBigInteger(key_part_l_bits, 77)&Utility.MASK64;
                kw[0]=key_part_l_bits>>64;
                kw[1]=key_part_l_bits&Utility.MASK64;
                kw[2]=Utility.circularShiftBigInteger(ka, 111)>>64&Utility.MASK64;      //неверно
                kw[3]=Utility.circularShiftBigInteger(ka, 111)&Utility.MASK64;          //неверно
            }
            else
            {
                k[0] =kb>>64;
                k[1] =kb&Utility.MASK64;
                k[2] =Utility.circularShiftBigInteger(key_part_r_bits, 15)>>64&Utility.MASK64;
                k[3] =Utility.circularShiftBigInteger(key_part_r_bits, 15)&Utility.MASK64;
                k[4] =Utility.circularShiftBigInteger(ka, 15)>>64&Utility.MASK64;
                k[5] =Utility.circularShiftBigInteger(ka, 15)&Utility.MASK64;
                k[6] =Utility.circularShiftBigInteger(kb, 30)>>64&Utility.MASK64;
                k[7] =Utility.circularShiftBigInteger(kb, 30)&Utility.MASK64;
                k[8] =Utility.circularShiftBigInteger(key_part_l_bits, 45)>>64&Utility.MASK64;
                k[9] =Utility.circularShiftBigInteger(key_part_l_bits, 45)&Utility.MASK64;
                k[10]=Utility.circularShiftBigInteger(ka, 45)>>64&Utility.MASK64;
                k[11]=Utility.circularShiftBigInteger(ka, 45)&Utility.MASK64;
                k[12]=Utility.circularShiftBigInteger(key_part_r_bits, 60)>>64&Utility.MASK64;
                k[13]=Utility.circularShiftBigInteger(key_part_r_bits, 60)&Utility.MASK64;
                k[14]=Utility.circularShiftBigInteger(kb, 60)>>64&Utility.MASK64;
                k[15]=Utility.circularShiftBigInteger(kb, 60)&Utility.MASK64;
                k[16]=Utility.circularShiftBigInteger(key_part_l_bits, 77)>>64&Utility.MASK64;
                k[17]=Utility.circularShiftBigInteger(key_part_l_bits, 77)&Utility.MASK64;
                k[18]=Utility.circularShiftBigInteger(key_part_r_bits, 94)>>64&Utility.MASK64;
                k[19]=Utility.circularShiftBigInteger(key_part_r_bits, 94)&Utility.MASK64;
                k[20]=Utility.circularShiftBigInteger(ka, 94)>>64&Utility.MASK64;
                k[21]=Utility.circularShiftBigInteger(ka, 94)&Utility.MASK64;
                k[22]=Utility.circularShiftBigInteger(key_part_l_bits, 111)>>64&Utility.MASK64;
                k[23]=Utility.circularShiftBigInteger(key_part_l_bits, 111)&Utility.MASK64;
                ke[0]=Utility.circularShiftBigInteger(key_part_r_bits, 30)>>64&Utility.MASK64;
                ke[1]=Utility.circularShiftBigInteger(key_part_r_bits, 30)&Utility.MASK64;
                ke[2]=Utility.circularShiftBigInteger(key_part_l_bits, 60)>>64&Utility.MASK64;
                ke[3]=Utility.circularShiftBigInteger(key_part_l_bits, 60)&Utility.MASK64;
                ke[4]=Utility.circularShiftBigInteger(ka, 77)>>64&Utility.MASK64;
                ke[5]=Utility.circularShiftBigInteger(ka, 77)&Utility.MASK64;
                kw[0]=key_part_l_bits>>64;
                kw[1]=key_part_l_bits&Utility.MASK64;
                kw[2]=Utility.circularShiftBigInteger(kb, 111)>>64&Utility.MASK64;
                kw[3]=Utility.circularShiftBigInteger(kb, 111)&Utility.MASK64;
            }

            byte[] tmp;
            byte i=0, j;
            for(j=0; j<kw.Length; i++, j++) // TODO Utility.toArray()
            {
                tmp=kw[j].ToByteArray();
                keys_round[i]=new byte[9] {0, 0, 0, 0, 0, 0, 0, 0, 0};
                Array.Copy(tmp, 0, keys_round[i], 0, tmp.Length);
            }
            //keys_round[i]=kw[i].ToByteArray();
            for(j=0; j<ke.Length; i++, j++)
            {
                tmp=ke[j].ToByteArray();
                keys_round[i]=new byte[9] {0, 0, 0, 0, 0, 0, 0, 0, 0};
                Array.Copy(tmp, 0, keys_round[i], 0, tmp.Length);
            }

            for(j=0; j<k.Length; i++, j++)
            {
                tmp=k[j].ToByteArray();
                keys_round[i]=new byte[9] {0, 0, 0, 0, 0, 0, 0, 0, 0};
                Array.Copy(tmp, 0, keys_round[i], 0, tmp.Length);
            }

            return keys_round;
        }
    }
    
    public class RoundCipheringCamelia : IRoundCiphering
    {
        public enum FunctionFModes
        {
            F,
            FL,
            FL_INVERSE
        }
        private readonly byte[,] _sboxes= new byte[4, 256] {
            {0x70, 0x82, 0x2C, 0xEC, 0xB3, 0x27, 0xC0, 0xE5, 0xE4, 0x85, 0x57, 0x35, 0xEA, 0x0C, 0xAE, 0x41,
            0x23, 0xEF, 0x6B, 0x93, 0x45, 0x19, 0xA5, 0x21, 0xED, 0x0E, 0x4F, 0x4E, 0x1D, 0x65, 0x92, 0xBD,
            0x86, 0xB8, 0xAF, 0x8F, 0x7C, 0xEB, 0x1F, 0xCE, 0x3E, 0x30, 0xDC, 0x5F, 0x5E, 0xC5, 0x0B, 0x1A,
            0xA6, 0xE1, 0x39, 0xCA, 0xD5, 0x47, 0x5D, 0x3D, 0xD9, 0x01, 0x5A, 0xD6, 0x51, 0x56, 0x6C, 0x4D,
            0x8B, 0x0D, 0x9A, 0x66, 0xFB, 0xCC, 0xB0, 0x2D, 0x74, 0x12, 0x2B, 0x20, 0xF0, 0xB1, 0x84, 0x99,
            0xDF, 0x4C, 0xCB, 0xC2, 0x34, 0x7E, 0x76, 0x05, 0x6D, 0xB7, 0xA9, 0x31, 0xD1, 0x17, 0x04, 0xD7,
            0x14, 0x58, 0x3A, 0x61, 0xDE, 0x1B, 0x11, 0x1C, 0x32, 0x0F, 0x9C, 0x16, 0x53, 0x18, 0xF2, 0x22,
            0xFE, 0x44, 0xCF, 0xB2, 0xC3, 0xB5, 0x7A, 0x91, 0x24, 0x08, 0xE8, 0xA8, 0x60, 0xFC, 0x69, 0x50,
            0xAA, 0xD0, 0xA0, 0x7D, 0xA1, 0x89, 0x62, 0x97, 0x54, 0x5B, 0x1E, 0x95, 0xE0, 0xFF, 0x64, 0xD2,
            0x10, 0xC4, 0x00, 0x48, 0xA3, 0xF7, 0x75, 0xDB, 0x8A, 0x03, 0xE6, 0xDA, 0x09, 0x3F, 0xDD, 0x94,
            0x87, 0x5C, 0x83, 0x02, 0xCD, 0x4A, 0x90, 0x33, 0x73, 0x67, 0xF6, 0xF3, 0x9D, 0x7F, 0xBF, 0xE2,
            0x52, 0x9B, 0xD8, 0x26, 0xC8, 0x37, 0xC6, 0x3B, 0x81, 0x96, 0x6F, 0x4B, 0x13, 0xBE, 0x63, 0x2E,
            0xE9, 0x79, 0xA7, 0x8C, 0x9F, 0x6E, 0xBC, 0x8E, 0x29, 0xF5, 0xF9, 0xB6, 0x2F, 0xFD, 0xB4, 0x59,
            0x78, 0x98, 0x06, 0x6A, 0xE7, 0x46, 0x71, 0xBA, 0xD4, 0x25, 0xAB, 0x42, 0x88, 0xA2, 0x8D, 0xFA,
            0x72, 0x07, 0xB9, 0x55, 0xF8, 0xEE, 0xAC, 0x0A, 0x36, 0x49, 0x2A, 0x68, 0x3C, 0x38, 0xF1, 0xA4,
            0x40, 0x28, 0xD3, 0x7B, 0xBB, 0xC9, 0x43, 0xC1, 0x15, 0xE3, 0xAD, 0xF4, 0x77, 0xC7, 0x80, 0x9E},
            
            {0xE0, 0x05, 0x58, 0xD9, 0x67, 0x4E, 0x81, 0xCB, 0xC9, 0x0B, 0xAE, 0x6A, 0xD5, 0x18, 0x5D, 0x82,
            0x46, 0xDF, 0xD6, 0x27, 0x8A, 0x32, 0x4B, 0x42, 0xDB, 0x1C, 0x9E, 0x9C, 0x3A, 0xCA, 0x25, 0x7B,
            0x0D, 0x71, 0x5F, 0x1F, 0xF8, 0xD7, 0x3E, 0x9D, 0x7C, 0x60, 0xB9, 0xBE, 0xBC, 0x8B, 0x16, 0x34,
            0x4D, 0xC3, 0x72, 0x95, 0xAB, 0x8E, 0xBA, 0x7A, 0xB3, 0x02, 0xB4, 0xAD, 0xA2, 0xAC, 0xD8, 0x9A,
            0x17, 0x1A, 0x35, 0xCC, 0xF7, 0x99, 0x61, 0x5A, 0xE8, 0x24, 0x56, 0x40, 0xE1, 0x63, 0x09, 0x33,
            0xBF, 0x98, 0x97, 0x85, 0x68, 0xFC, 0xEC, 0x0A, 0xDA, 0x6F, 0x53, 0x62, 0xA3, 0x2E, 0x08, 0xAF,
            0x28, 0xB0, 0x74, 0xC2, 0xBD, 0x36, 0x22, 0x38, 0x64, 0x1E, 0x39, 0x2C, 0xA6, 0x30, 0xE5, 0x44,
            0xFD, 0x88, 0x9F, 0x65, 0x87, 0x6B, 0xF4, 0x23, 0x48, 0x10, 0xD1, 0x51, 0xC0, 0xF9, 0xD2, 0xA0,
            0x55, 0xA1, 0x41, 0xFA, 0x43, 0x13, 0xC4, 0x2F, 0xA8, 0xB6, 0x3C, 0x2B, 0xC1, 0xFF, 0xC8, 0xA5,
            0x20, 0x89, 0x00, 0x90, 0x47, 0xEF, 0xEA, 0xB7, 0x15, 0x06, 0xCD, 0xB5, 0x12, 0x7E, 0xBB, 0x29,
            0x0F, 0xB8, 0x07, 0x04, 0x9B, 0x94, 0x21, 0x66, 0xE6, 0xCE, 0xED, 0xE7, 0x3B, 0xFE, 0x7F, 0xC5,
            0xA4, 0x37, 0xB1, 0x4C, 0x91, 0x6E, 0x8D, 0x76, 0x03, 0x2D, 0xDE, 0x96, 0x26, 0x7D, 0xC6, 0x5C,
            0xD3, 0xF2, 0x4F, 0x19, 0x3F, 0xDC, 0x79, 0x1D, 0x52, 0xEB, 0xF3, 0x6D, 0x5E, 0xFB, 0x69, 0xB2,
            0xF0, 0x31, 0x0C, 0xD4, 0xCF, 0x8C, 0xE2, 0x75, 0xA9, 0x4A, 0x57, 0x84, 0x11, 0x45, 0x1B, 0xF5,
            0xE4, 0x0E, 0x73, 0xAA, 0xF1, 0xDD, 0x59, 0x14, 0x6C, 0x92, 0x54, 0xD0, 0x78, 0x70, 0xE3, 0x49,
            0x80, 0x50, 0xA7, 0xF6, 0x77, 0x93, 0x86, 0x83, 0x2A, 0xC7, 0x5B, 0xE9, 0xEE, 0x8F, 0x01, 0x3D},
            
            {0x38, 0x41, 0x16, 0x76, 0xD9, 0x93, 0x60, 0xF2, 0x72, 0xC2, 0xAB, 0x9A, 0x75, 0x06, 0x57, 0xA0,
            0x91, 0xF7, 0xB5, 0xC9, 0xA2, 0x8C, 0xD2, 0x90, 0xF6, 0x07, 0xA7, 0x27, 0x8E, 0xB2, 0x49, 0xDE,
            0x43, 0x5C, 0xD7, 0xC7, 0x3E, 0xF5, 0x8F, 0x67, 0x1F, 0x18, 0x6E, 0xAF, 0x2F, 0xE2, 0x85, 0x0D,
            0x53, 0xF0, 0x9C, 0x65, 0xEA, 0xA3, 0xAE, 0x9E, 0xEC, 0x80, 0x2D, 0x6B, 0xA8, 0x2B, 0x36, 0xA6,
            0xC5, 0x86, 0x4D, 0x33, 0xFD, 0x66, 0x58, 0x96, 0x3A, 0x09, 0x95, 0x10, 0x78, 0xD8, 0x42, 0xCC,
            0xEF, 0x26, 0xE5, 0x61, 0x1A, 0x3F, 0x3B, 0x82, 0xB6, 0xDB, 0xD4, 0x98, 0xE8, 0x8B, 0x02, 0xEB,
            0x0A, 0x2C, 0x1D, 0xB0, 0x6F, 0x8D, 0x88, 0x0E, 0x19, 0x87, 0x4E, 0x0B, 0xA9, 0x0C, 0x79, 0x11,
            0x7F, 0x22, 0xE7, 0x59, 0xE1, 0xDA, 0x3D, 0xC8, 0x12, 0x04, 0x74, 0x54, 0x30, 0x7E, 0xB4, 0x28,
            0x55, 0x68, 0x50, 0xBE, 0xD0, 0xC4, 0x31, 0xCB, 0x2A, 0xAD, 0x0F, 0xCA, 0x70, 0xFF, 0x32, 0x69,
            0x08, 0x62, 0x00, 0x24, 0xD1, 0xFB, 0xBA, 0xED, 0x45, 0x81, 0x73, 0x6D, 0x84, 0x9F, 0xEE, 0x4A,
            0xC3, 0x2E, 0xC1, 0x01, 0xE6, 0x25, 0x48, 0x99, 0xB9, 0xB3, 0x7B, 0xF9, 0xCE, 0xBF, 0xDF, 0x71,
            0x29, 0xCD, 0x6C, 0x13, 0x64, 0x9B, 0x63, 0x9D, 0xC0, 0x4B, 0xB7, 0xA5, 0x89, 0x5F, 0xB1, 0x17,
            0xF4, 0xBC, 0xD3, 0x46, 0xCF, 0x37, 0x5E, 0x47, 0x94, 0xFA, 0xFC, 0x5B, 0x97, 0xFE, 0x5A, 0xAC,
            0x3C, 0x4C, 0x03, 0x35, 0xF3, 0x23, 0xB8, 0x5D, 0x6A, 0x92, 0xD5, 0x21, 0x44, 0x51, 0xC6, 0x7D,
            0x39, 0x83, 0xDC, 0xAA, 0x7C, 0x77, 0x56, 0x05, 0x1B, 0xA4, 0x15, 0x34, 0x1E, 0x1C, 0xF8, 0x52,
            0x20, 0x14, 0xE9, 0xBD, 0xDD, 0xE4, 0xA1, 0xE0, 0x8A, 0xF1, 0xD6, 0x7A, 0xBB, 0xE3, 0x40, 0x4F},
            
            {0x70, 0x2C, 0xB3, 0xC0, 0xE4, 0x57, 0xEA, 0xAE, 0x23, 0x6B, 0x45, 0xA5, 0xED, 0x4F, 0x1D, 0x92,
            0x86, 0xAF, 0x7C, 0x1F, 0x3E, 0xDC, 0x5E, 0x0B, 0xA6, 0x39, 0xD5, 0x5D, 0xD9, 0x5A, 0x51, 0x6C,
            0x8B, 0x9A, 0xFB, 0xB0, 0x74, 0x2B, 0xF0, 0x84, 0xDF, 0xCB, 0x34, 0x76, 0x6D, 0xA9, 0xD1, 0x04,
            0x14, 0x3A, 0xDE, 0x11, 0x32, 0x9C, 0x53, 0xF2, 0xFE, 0xCF, 0xC3, 0x7A, 0x24, 0xE8, 0x60, 0x69,
            0xAA, 0xA0, 0xA1, 0x62, 0x54, 0x1E, 0xE0, 0x64, 0x10, 0x00, 0xA3, 0x75, 0x8A, 0xE6, 0x09, 0xDD,
            0x87, 0x83, 0xCD, 0x90, 0x73, 0xF6, 0x9D, 0xBF, 0x52, 0xD8, 0xC8, 0xC6, 0x81, 0x6F, 0x13, 0x63,
            0xE9, 0xA7, 0x9F, 0xBC, 0x29, 0xF9, 0x2F, 0xB4, 0x78, 0x06, 0xE7, 0x71, 0xD4, 0xAB, 0x88, 0x8D,
            0x72, 0xB9, 0xF8, 0xAC, 0x36, 0x2A, 0x3C, 0xF1, 0x40, 0xD3, 0xBB, 0x43, 0x15, 0xAD, 0x77, 0x80,
            0x82, 0xEC, 0x27, 0xE5, 0x85, 0x35, 0x0C, 0x41, 0xEF, 0x93, 0x19, 0x21, 0x0E, 0x4E, 0x65, 0xBD,
            0xB8, 0x8F, 0xEB, 0xCE, 0x30, 0x5F, 0xC5, 0x1A, 0xE1, 0xCA, 0x47, 0x3D, 0x01, 0xD6, 0x56, 0x4D,
            0x0D, 0x66, 0xCC, 0x2D, 0x12, 0x20, 0xB1, 0x99, 0x4C, 0xC2, 0x7E, 0x05, 0xB7, 0x31, 0x17, 0xD7,
            0x58, 0x61, 0x1B, 0x1C, 0x0F, 0x16, 0x18, 0x22, 0x44, 0xB2, 0xB5, 0x91, 0x08, 0xA8, 0xFC, 0x50,
            0xD0, 0x7D, 0x89, 0x97, 0x5B, 0x95, 0xFF, 0xD2, 0xC4, 0x48, 0xF7, 0xDB, 0x03, 0xDA, 0x3F, 0x94,
            0x5C, 0x02, 0x4A, 0x33, 0x67, 0xF3, 0x7F, 0xE2, 0x9B, 0x26, 0x37, 0x3B, 0x96, 0x4B, 0xBE, 0x2E,
            0x79, 0x8C, 0x6E, 0x8E, 0xF5, 0xB6, 0xFD, 0x59, 0x98, 0x6A, 0x46, 0xBA, 0x25, 0x42, 0xA2, 0xFA,
            0x07, 0x55, 0xEE, 0x0A, 0x49, 0x68, 0x38, 0xA4, 0x28, 0x7B, 0xC9, 0xC1, 0xE3, 0xF4, 0xC7, 0x9E}
        };

        public ulong functionF(ulong bits_input, ulong ke, FunctionFModes mode=FunctionFModes.F)
        {
            switch(mode)
            {
                case FunctionFModes.F:
                    ulong x_F = bits_input^ke;
                    ulong[] t_F=new ulong[8], y_F=new ulong[8];
                    
                    t_F[0]=x_F>> 56;
                    for(sbyte i=48; i>=0; i-=8)
                        t_F[i/8+1]=(x_F>>(48-i))&Utility.MASK8;
                    t_F[0]=_sboxes[0,t_F[0]];
                    t_F[1]=_sboxes[1,t_F[1]];
                    t_F[2]=_sboxes[2,t_F[2]];
                    t_F[3]=_sboxes[3,t_F[3]];
                    t_F[4]=_sboxes[1,t_F[4]];
                    t_F[5]=_sboxes[2,t_F[5]];
                    t_F[6]=_sboxes[3,t_F[6]];
                    t_F[7]=_sboxes[0,t_F[7]];
                    y_F[0]=t_F[0]^t_F[2]^t_F[3]^t_F[5]^t_F[6]^t_F[7];
                    y_F[1]=t_F[0]^t_F[1]^t_F[3]^t_F[4]^t_F[6]^t_F[7];
                    y_F[2]=t_F[0]^t_F[1]^t_F[2]^t_F[4]^t_F[5]^t_F[7];
                    y_F[3]=t_F[1]^t_F[2]^t_F[3]^t_F[4]^t_F[5]^t_F[6];
                    y_F[4]=t_F[0]^t_F[1]^t_F[5]^t_F[6]^t_F[7];
                    y_F[5]=t_F[1]^t_F[2]^t_F[4]^t_F[6]^t_F[7];
                    y_F[6]=t_F[2]^t_F[3]^t_F[4]^t_F[5]^t_F[7];
                    y_F[7]=t_F[0]^t_F[3]^t_F[4]^t_F[5]^t_F[6];
            
                    return (y_F[0]<<56) | (y_F[1]<<48) | (y_F[2]<<40) | (y_F[3]<<32)| (y_F[4]<<24) | (y_F[5]<<16) | (y_F[6]<<8) | y_F[7];
                case FunctionFModes.FL:
                    uint[] x_FL=new uint[2] {(uint)(bits_input>>32), (uint)(bits_input&Utility.MASK32)}, k_FL=new uint[2] {(uint)(ke>>32), (uint)(ke&Utility.MASK32)};
                    uint t_FL;

                    t_FL=x_FL[0]&k_FL[0];
                    x_FL[1]^=(t_FL<<1) | (t_FL>>31);
                    x_FL[0]^=x_FL[1] | k_FL[1];
                    
                    return ((ulong)x_FL[0] << 32) | x_FL[1];
                case FunctionFModes.FL_INVERSE:
                    uint[] y_FL_INVERSE=new uint[2] {(uint)(bits_input>>32), (uint)(bits_input&Utility.MASK32)}, k_FL_INVERSE=new uint[2] {(uint)(ke>>32), (uint)(ke&Utility.MASK32)};
                    uint t_FL_INVERSE;

                    y_FL_INVERSE[0]^=y_FL_INVERSE[1] | k_FL_INVERSE[1];
                    t_FL_INVERSE=y_FL_INVERSE[0]&k_FL_INVERSE[0];
                    y_FL_INVERSE[1]^=(t_FL_INVERSE<<1) | (t_FL_INVERSE>>31);
                    
                    return ((ulong)y_FL_INVERSE[0]<<32) | y_FL_INVERSE[1];
                default:
                    return 0;
            }
        }
        public byte[] performRound(byte[] bytes, byte[] key_round)
        {
            ulong[] bits_parts=new ulong[2] {BitConverter.ToUInt64(bytes, 0), BitConverter.ToUInt64(bytes, 8)};
            byte[] result=new byte[16];
            
            bits_parts[1]^=functionF(bits_parts[0], BitConverter.ToUInt64(key_round, 0));
            Array.Copy(BitConverter.GetBytes(bits_parts[1]), 0,result, 0,  8);
            Array.Copy(BitConverter.GetBytes(bits_parts[0]), 0,result, 8,  8);
            
            return result;
        }
    }

    public class KeyExpansionElGamal : IKeyExpansion
    {
        public enum PrimalityTestingMode
        {
            FERMAT,
            SOLOVAY_STRASSEN,
            MILLER_RABIN
        }

        private PrimalityTestingMode _primality_testing_mode;
        private readonly double _probability_minimal;
        private readonly int _prime_numbers_length_bits;

        public KeyExpansionElGamal(PrimalityTestingMode primality_testing_mode, double probability_minimal, int prime_numbers_length_bits)
        {
            _primality_testing_mode=primality_testing_mode;
            _probability_minimal=probability_minimal;
            _prime_numbers_length_bits=prime_numbers_length_bits;
        }

        private BigInteger getPrimeNumber(int min, int max)
        {
            BigInteger number=Utility.getRandomNBitNumber(Utility.Rng.Next(min, max));
            
            switch(_primality_testing_mode)
            {
                case PrimalityTestingMode.FERMAT:
                    while(!Primality.performFermatTest(number, _probability_minimal))
                        number=Utility.getRandomNBitNumber(Utility.Rng.Next(min, max));
                    break; 
                case PrimalityTestingMode.SOLOVAY_STRASSEN:
                    while(!Primality.performSolovayStrassenTest(number, _probability_minimal))
                        number=Utility.getRandomNBitNumber(Utility.Rng.Next(min, max));
                    break;
                case PrimalityTestingMode.MILLER_RABIN:
                    while(!Primality.performMillerRabinTest(number, _probability_minimal))
                        number=Utility.getRandomNBitNumber(Utility.Rng.Next(min, max));
                    break;
            }

            return number;
        }
        public byte[][] getKeysRound(byte[] key)
        {
            BigInteger p=getPrimeNumber(_prime_numbers_length_bits, _prime_numbers_length_bits), p_minus_one=p-1, q=p_minus_one/2, g=1, x, y; //g=2

            for( ; g<p_minus_one; g++)
                if(BigInteger.ModPow(g, 2, p)!=1 && BigInteger.ModPow(g, q, p)!=1)
                    break;
            
            x=Utility.getRandomBigInteger(1, p_minus_one);
            y=BigInteger.ModPow(g, x, p);

            return new byte[][] {p.ToByteArray(), g.ToByteArray(), y.ToByteArray(), x.ToByteArray()};
        }
    }
}