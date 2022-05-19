using System;
using System.Linq;
using System.Numerics;
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
        
        public FeistelNet(IRoundCiphering round_ciphering)
        {
            _round_ciphering=round_ciphering;
        }
        
        public virtual byte[][] getKeysRound(byte[] key)
        {
            return null;
        }
        public virtual void encrypt(in byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round) //kw, ke, k
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
                    Array.Copy(BitConverter.GetBytes(bits_input_parts[1]), 0, bytes_input_local,       8, 8);
                }
                for( ; i<iteration_rounds; i++, k++)
                    bytes_input_local=_round_ciphering.performRound(bytes_input_local, keys_round[k]);
            }
            bits_input_parts[1]=BitConverter.ToUInt64(bytes_input_local, 8)^keys_round_converted[2];
            bits_input_parts[0]=BitConverter.ToUInt64(bytes_input_local, 0)^keys_round_converted[3];

            bytes_output=(((BigInteger)bits_input_parts[1]<<64)|bits_input_parts[0]).ToByteArray().Where((source, index) =>index != 16).ToArray();
            
        }
        public virtual void decrypt(in byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round) 
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
            
            bytes_output=(((BigInteger)bits_input_parts[1]<<64)|bits_input_parts[0]).ToByteArray().Where((source, index) =>index != 16).ToArray();;
        }
    }
    
    public sealed class Camellia : FeistelNet
    {
        private readonly IKeyExpansion _key_expansion;

        public string Name
        {
            get {return "Camellia"; }
        }
        public Camellia() : base(new RoundCipheringCamelia())
        {
            _key_expansion=new KeyExpansionCamellia();
        }

        public override byte[][] getKeysRound(byte[] key)
        {
            return _key_expansion.getKeysRound(key);
        }

        public override void encrypt(in byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            base.encrypt(in bytes_input, out bytes_output, keys_round);
        }

        public override void decrypt(in byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            base.decrypt(in bytes_input, out bytes_output, keys_round);
        }
    }
    public sealed class ElGamal : IAlgorithm
    {
        private readonly IKeyExpansion _key_expansion;

        public string Name
        {
            get {return "ElGamal"; }
        }
        
        public ElGamal()
        {
            _key_expansion=new KeyExpansionElGamal(KeyExpansionElGamal.PrimalityTestingMode.Fermat, (float)0.999, 64);
        }

        public byte[][] getKeysRound(byte[] key)
        {
            return _key_expansion.getKeysRound(key);
        }
        public void encrypt(in byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            int message_size=keys_round[0].Length-1, cipher_length=0;
            byte[][] bytes_output_2d =new byte[bytes_input.Length/message_size+1][];
            byte[] tmp;
            BigInteger p=new BigInteger(keys_round[0]), p_minus_one = new BigInteger(keys_round[1]), k;
            
            do
                k=Utility.getRandomBigInteger(p_minus_one, 1);
            while(BigInteger.GreatestCommonDivisor(k, p_minus_one)!=1);
            bytes_output_2d[2]=BigInteger.ModPow(new BigInteger(keys_round[2]), k, p).ToByteArray();
            bytes_output_2d[0]=new byte[] {(byte)bytes_output_2d[1].Length};
            
            for(int i=0, j=3; i<bytes_input.Length; i+=message_size, j++)
            {
                tmp=(BigInteger.ModPow(new BigInteger(keys_round[3]), k, p)*Utility.byteArrayConvertToBigInteger(bytes_input, i, i+message_size)).ToByteArray();
                cipher_length+=tmp.Length;
                bytes_output_2d[j]=tmp;
            }
            bytes_output_2d[1]=new byte[] {(byte)cipher_length};
            
            Buffer.BlockCopy(bytes_output_2d, 0, bytes_output=new byte[cipher_length+bytes_output_2d[1].Length+1], 0, bytes_output_2d.Length);
        }
        public void decrypt(in byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            BigInteger a=Utility.byteArrayConvertToBigInteger(bytes_input, 2, 2+bytes_input[0]), b=Utility.byteArrayConvertToBigInteger(bytes_input, 2+bytes_input[0], 2+bytes_input[1]);
            bytes_output=(b*BigInteger.ModPow(a, new BigInteger(keys_round[1])-new BigInteger(keys_round[3]), new BigInteger(keys_round[0]))).ToByteArray();
        }
    }
}