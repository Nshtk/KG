using KP.Context.Interface;

namespace KP.Context
{
    public class FeistelNet : IAlgorithm
    {
        public KeyExpansionFeistel _key_expansion = new KeyExpansionFeistel();
        public IRoundCiphering _round_ciphering;
        
        public void encrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round) 
        {
            for(sbyte i=0; i<16; i++)
                bytes_input=_round_ciphering.perform(bytes_input, keys_round[i]);
            bytes_output=swapParts64(bytes_input);
        }
        public void decrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round) 
        {
            for(sbyte i=15; i>-1; i--)
                bytes_input=_round_ciphering.perform(bytes_input, keys_round[i]);
            bytes_output=swapParts64(bytes_input);
        }
    };
    
    public sealed class Camelia : FeistelNet
    {
        private Utility.CipheringMode _ciphering_mode;
        private byte[] _key;
        private ulong _init_vector;
        
        public Camelia(Utility.CipheringMode ciphering_mode, byte[] key, ulong init_vector)
        {
            _ciphering_mode = ciphering_mode;
            _key = key;
            _init_vector = init_vector;
        }

        public void encrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round)
        {
            
        }
        public void decrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round)
        {
            
        }
        

    }
    public sealed class ElGamal : IAlgorithm
    {
        public IKeyExpansion key_expansion;

        public ElGamal()
        {
            
        }
        
        public void encrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round)
        {
            
        }
        public void decrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round)
        {
            
        }
    }
}