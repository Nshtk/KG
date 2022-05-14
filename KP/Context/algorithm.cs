using System;
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
        
        public void encrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round) //kw, ke, k
        {
            ulong[] keys_round_converted=new ulong[keys_round.Length];
            byte i=0;
            for( ; i<keys_round.Length; i++)
                keys_round_converted[i]=BitConverter.ToUInt64(keys_round[i], 0);
            
            BigInteger bits_input=new BigInteger(bytes_input);
            ulong[] bits_input_parts=new ulong[2] {((ulong)(bits_input>>64))^keys_round_converted[0], ((ulong)(bits_input&1UL))^keys_round_converted[1]};

            byte iterations=0, cycles=6;
            i=0;
            if(keys_round_converted.Length==26)
            {
                for(byte j=0; iterations<2; iterations++, cycles+=6)
                {
                    for( ; i<cycles; i++)
                        bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
                    bits_input_parts[0]=_round_ciphering.functionF(bits_input_parts[0], keys_round_converted[j], RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(bits_input_parts[1], keys_round_converted[++j], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                }
                for( ; i<cycles; i++)
                    bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
            }
            else
            {
                for(byte j=0; iterations<3; iterations++, cycles+=6)
                {
                    for( ; i<cycles; i++)
                        bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
                    bits_input_parts[0]=_round_ciphering.functionF(bits_input_parts[0], keys_round_converted[j], RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(bits_input_parts[1], keys_round_converted[++j], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                }
                for( ; i<cycles; i++)
                    bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
            }
            bits_input_parts[1]^=keys_round_converted[2];
            bits_input_parts[0]^=keys_round_converted[3];
            
            bytes_output=(((BigInteger)bits_input_parts[1]<<64)|bits_input_parts[0]).ToByteArray();
        }
        public void decrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round) 
        {
            ulong[] keys_round_converted=new ulong[keys_round.Length];
            byte i=0;
            for( ; i<keys_round.Length; i++)
                keys_round_converted[i]=BitConverter.ToUInt64(keys_round[i], 0);
            
            BigInteger bits_input=new BigInteger(bytes_input);
            ulong[] bits_input_parts=new ulong[2] {((ulong)(bits_input>>64))^keys_round_converted[2], ((ulong)(bits_input&1UL))^keys_round_converted[3]};

            byte iterations=0, cycles;
            if(keys_round_converted.Length==26)
            {
                i=18; cycles=12;
                for(byte j=4; iterations<2; iterations++, cycles-=6)
                {
                    for( ; i>cycles; i--)
                        bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
                    bits_input_parts[0]=_round_ciphering.functionF(bits_input_parts[0], keys_round_converted[j], RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(bits_input_parts[1], keys_round_converted[--j], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                }
                for( ; i>cycles; i--)
                    bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
            }
            else
            {
                i=24; cycles=18;
                for(byte j=6; iterations<3; iterations++, cycles+=6)
                {
                    for( ; i<cycles; i++)
                        bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
                    bits_input_parts[0]=_round_ciphering.functionF(bits_input_parts[0], keys_round_converted[j], RoundCipheringCamelia.FunctionFModes.FL);
                    bits_input_parts[1]=_round_ciphering.functionF(bits_input_parts[1], keys_round_converted[++j], RoundCipheringCamelia.FunctionFModes.FL_INVERSE);
                }
                for( ; i<cycles; i++)
                    bytes_input=_round_ciphering.performRound(bytes_input, keys_round[i]);
            }
            bits_input_parts[1]^=keys_round_converted[0];
            bits_input_parts[0]^=keys_round_converted[1];
            
            bytes_output=(((BigInteger)bits_input_parts[1]<<64)|bits_input_parts[0]).ToByteArray();
            
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

        public void encrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            base.encrypt(bytes_input, out bytes_output, keys_round);
        }
        public void decrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            base.decrypt(bytes_input, out bytes_output, keys_round);
        }
    }
    public sealed class ElGamal : IAlgorithm
    {
        public string Name
        {
            get {return "ElGamal"; }
        }
        
        private readonly IKeyExpansion _key_expansion;

        public ElGamal()
        {
            _key_expansion=new KeyExpansionElGamal(KeyExpansionElGamal.PrimalityTestingMode.Fermat, (float)0.999, 64);
        }
        
        public void encrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
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
                tmp=(BigInteger.ModPow(new BigInteger(keys_round[3]), k, p)*Utility.bytesConvertToBigInteger(bytes_input, i, i+message_size)).ToByteArray();
                cipher_length+=tmp.Length;
                bytes_output_2d[j]=tmp;
            }
            bytes_output_2d[1]=new byte[] {(byte)cipher_length};
            
            Buffer.BlockCopy(bytes_output_2d, 0, bytes_output=new byte[cipher_length+bytes_output_2d[1].Length+1], 0, bytes_output_2d.Length);
        }
        public void decrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round)
        {
            BigInteger a=Utility.bytesConvertToBigInteger(bytes_input, 2, 2+bytes_input[0]), b=Utility.bytesConvertToBigInteger(bytes_input, 2+bytes_input[0], 2+bytes_input[1]);
            bytes_output=(b*BigInteger.ModPow(a, new BigInteger(keys_round[1])-new BigInteger(keys_round[3]), new BigInteger(keys_round[0]))).ToByteArray();
        }
    }
}