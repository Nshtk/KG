using System;
using System.Numerics;
using KP.Context.Interface;

namespace KP.Context
{
    public class FeistelNet : IAlgorithm
    {
        public IRoundCiphering _round_ciphering;

        public FeistelNet(IRoundCiphering round_ciphering)
        {
            _round_ciphering=round_ciphering;
        }
        
        public void encrypt(byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round) //kw, ke, k
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
        public void decrypt(byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round) 
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
        private Utility.CipheringMode _ciphering_mode;
        private byte[][] keys_round;
        private byte[] _key;
        private ulong _init_vector;
        
        public Camellia(Utility.CipheringMode ciphering_mode, byte[] key, ulong init_vector) : base( null)
        {
            _key_expansion=new KeyExpansionCamellia();
            _ciphering_mode = ciphering_mode;
            _key = key;
            _init_vector = init_vector;
            keys_round=_key_expansion.getKeysRound(_key);
        }

        public void encrypt(byte[] bytes_input, ref byte[] bytes_output)
        {
            base.encrypt(bytes_input, ref bytes_output, keys_round);
        }
        public void decrypt(byte[] bytes_input, ref byte[] bytes_output)
        {
            base.decrypt(bytes_input, ref bytes_output, keys_round);
        }
    }
    public sealed class ElGamal : IAlgorithm
    {
        public IKeyExpansion key_expansion;

        public ElGamal()
        {
            
        }
        
        public void encrypt(byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round)
        {
            
        }
        public void decrypt(byte[] bytes_input, ref byte[] bytes_output, byte[][] keys_round)
        {
            
        }
    }
}