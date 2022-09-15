using System;
using System.Linq;
using System.Numerics;
using System.Windows.Media.Animation;
using KP.Context.Interface;

namespace KP.Context
{
    public class FeistelNet : IAlgorithm
    {
        public IRoundCiphering _round_ciphering;

        public string Name
        {
            get;
        }
        public int MessageLength
        {
            get {return 16;}
        }
        public bool IsSymmetric
        {
            get {return true;}
        }
        
        public FeistelNet(IRoundCiphering round_ciphering)
        {
            _round_ciphering=round_ciphering;
        }
        
        public virtual byte[][] getKeysRound(byte[] key)
        {
            return null;
        }
        public virtual void encrypt(in byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round) //kw, ke, k
        {
            ulong[] keys_round_converted=new ulong[keys_round.Length];
            byte[] bytes_input_local;
            byte i=0;
            for( ; i<keys_round.Length; i++)
                keys_round_converted[i]=BitConverter.ToUInt64(keys_round[i], 0);
            
            bytes_input_local=new byte[bytes_input.Length];
            bytes_input.CopyTo(bytes_input_local, 0);
            
            ulong[] bits_input_parts=new ulong[2] {BitConverter.ToUInt64(bytes_input_local, 8)^keys_round_converted[0], BitConverter.ToUInt64(bytes_input_local, 0)^keys_round_converted[1]};
            byte iterations=0, iteration_rounds=6, k, e=4;
            Array.Copy(BitConverter.GetBytes(bits_input_parts[0]), 0, bytes_input_local, 0, 8);
            Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local,       8, 8);
            i=0;
            if(keys_round_converted.Length==26)
            {
                k=8;                                                            // TODO to function
                for( ; iterations<2; iterations++, iteration_rounds+=6, e++)            
                {
                    for(; i<iteration_rounds; i++, k++)
                        bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
                    bits_input_parts[0]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local,       0), keys_round_converted[e],   RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 8), keys_round_converted[++e], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[0]), 0, bytes_input_local, 0, 8);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local, 8, 8);
                }
                for( ; i<iteration_rounds; i++, k++)
                    bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
            }
            else
            {
                k=10;
                for( ; iterations<3; iterations++, iteration_rounds+=6, e++)
                {
                    for(; i<iteration_rounds; i++, k++)
                        bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
                    bits_input_parts[0]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 0), keys_round_converted[e],   RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 8), keys_round_converted[++e], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[0]), 0, bytes_input_local, 0, 8);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local, 8, 8);
                }
                for( ; i<iteration_rounds; i++, k++)
                    bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
            }
            bits_input_parts[1]=BitConverter.ToUInt64(bytes_input_local, 8)^keys_round_converted[2];
            bits_input_parts[0]=BitConverter.ToUInt64(bytes_input_local, 0)^keys_round_converted[3];

            bytes_output=(((BigInteger)bits_input_parts[1]<<64)|bits_input_parts[0]).ToByteArray().Where((source, index) =>index != 16).ToArray();
            
        }
        public virtual void decrypt(in byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round) 
        {
            ulong[] keys_round_converted=new ulong[keys_round.Length];
            byte[] bytes_input_local;
            byte i=0;
            for( ; i<keys_round.Length; i++)
                keys_round_converted[i]=BitConverter.ToUInt64(keys_round[i], 0);

            bytes_input_local=new byte[bytes_input.Length];
            bytes_input.CopyTo(bytes_input_local, 0);
            
            ulong[] bits_input_parts=new ulong[2] {BitConverter.ToUInt64(bytes_input_local, 8)^keys_round_converted[2], BitConverter.ToUInt64(bytes_input_local, 0)^keys_round_converted[3]};
            byte iterations=0, iteration_rounds=6, k, e;
            Array.Copy(BitConverter.GetBytes(bits_input_parts[0]), 0, bytes_input_local, 0, 8);
            Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local, 8, 8);
            i=0;
            if(keys_round_converted.Length==26)
            {
                k=25; e=7;
                for( ; iterations<2; iterations++, iteration_rounds+=6, e--)
                {
                    for(; i<iteration_rounds; i++, k--)
                        bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
                    bits_input_parts[0]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 0), keys_round_converted[e], RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 8), keys_round_converted[--e], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[0]), 0, bytes_input_local, 0, 8);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local, 8, 8);
                }
                for( ; i<iteration_rounds; i++, k--)
                    bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
            }
            else
            {
                k=33; e=9;
                for( ; iterations<3; iterations++, iteration_rounds+=6, e--)
                {
                    for(; i<iteration_rounds; i++, k--)
                        bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
                    bits_input_parts[0]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 0), keys_round_converted[e], RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(BitConverter.ToUInt64(bytes_input_local, 8), keys_round_converted[--e], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[0]), 0, bytes_input_local, 0, 8);
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local, 8, 8);
                }
                for( ; i<iteration_rounds; i++, k--)
                    bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
            }
            bits_input_parts[1]=BitConverter.ToUInt64(bytes_input_local, 8)^keys_round_converted[0];
            bits_input_parts[0]=BitConverter.ToUInt64(bytes_input_local, 0)^keys_round_converted[1];
            
            bytes_output=(((BigInteger)bits_input_parts[1]<<64)|bits_input_parts[0]).ToByteArray().Where((source, index) =>index!=16).ToArray();
        }
    }
    
    public sealed class Camellia : FeistelNet
    {
        private readonly IKeyExpansion _key_expansion;

        public string Name
        {
            get {return "Camellia"; }
        }
        public int MessageLength
        {
            get {return 16;}
        }
        public bool IsSymmetric
        {
            get {return true;}
        }
        public Camellia() : base(new RoundCipheringCamelia())
        {
            _key_expansion=new KeyExpansionCamellia();
        }

        public override byte[][] getKeysRound(byte[] key)
        {
            return _key_expansion.getKeysRound(key);
        }

        public override void encrypt(in byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round)
        {
            base.encrypt(in bytes_input, ref bytes_output, keys_round);
        }

        public override void decrypt(in byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round)
        {
            base.decrypt(in bytes_input, ref bytes_output, keys_round);
        }
    }
    public sealed class ElGamal : IAlgorithm
    {
        private readonly IKeyExpansion _key_expansion;

        public string Name
        {
            get {return "ElGamal";}
        }
        public int MessageLength
        {
            get;
        }
        public bool IsSymmetric
        {
            get {return false;}
        }
        public ElGamal(KeyExpansionElGamal.PrimalityTestingMode primality_testing_mode=KeyExpansionElGamal.PrimalityTestingMode.FERMAT, double probability_minimal=0.999, int prime_numbers_length_bits=64)
        {
            _key_expansion=new KeyExpansionElGamal(primality_testing_mode, probability_minimal, prime_numbers_length_bits);
            MessageLength=prime_numbers_length_bits/8;
        }
    
        public byte[][] getKeysRound(byte[] key)
        {
            return _key_expansion.getKeysRound(key);
        }
        public void encrypt(in byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round)
        {
            BigInteger p=new BigInteger(keys_round[0]), p_minus_one=p-1, k;
            byte[] tmp;
            int ie=0;

            begin:
            do
                k=Utility.getRandomBigInteger(2, p_minus_one-1);
            while(BigInteger.GreatestCommonDivisor(k, p_minus_one)!=1);
            
            bytes_output=new byte[MessageLength*2];
            tmp=BigInteger.ModPow(new BigInteger(keys_round[1]), k, p).ToByteArray();
            tmp.CopyTo(bytes_output, 0);

            var a=Utility.modMultiplyBigInteger(BigInteger.ModPow(new BigInteger(keys_round[2]), k, p), new BigInteger(bytes_input), p);
            if(a==0 && ie<1000)
            {
                ie++;
                goto begin;
            }
                    

            tmp=a.ToByteArray().Where((source, index) => index!=MessageLength+1).ToArray();
            tmp.CopyTo(bytes_output, MessageLength);

        }
        public void decrypt(in byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round)
        {
            BigInteger p=new BigInteger(keys_round[0]);
            bytes_output=Utility.modMultiplyBigInteger(Utility.byteArrayConvertToBigInteger(bytes_input, MessageLength, MessageLength*2), BigInteger.ModPow(Utility.byteArrayConvertToBigInteger(bytes_input, 0, MessageLength), p-1-new BigInteger(keys_round[3]), p), p).ToByteArray();
        }
    }

    /*class AlgorithmSymmetric
    {
        private IAlgorithm _algorithm;
        public Utility.CipheringMode ciphering_mode;
        
    }*/
}