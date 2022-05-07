namespace KP.Context.Interface
{
    public interface IAlgorithm
    {
        public void encrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round);
        public void decrypt(byte[] bytes_input, byte[] bytes_output, byte[][] keys_round);
    }
}