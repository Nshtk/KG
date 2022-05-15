namespace KP.Context.Interface
{
    public interface IAlgorithm
    {
        public string Name { get; }

        public byte[][] getKeysRound(byte[] key);
        public void encrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round);
        public void decrypt(byte[] bytes_input, out byte[] bytes_output, byte[][] keys_round);
    }
}