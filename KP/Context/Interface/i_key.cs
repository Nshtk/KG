namespace KP.Context.Interface
{
    public interface IKeyExpansion
    {
        public ulong[] generateKeysRound(byte[] key);
    }

    public interface IRoundCiphering
    {
        public uint functionF(uint bits_input_part, ulong key_round);
        public byte[] perform(byte[] bytes, byte[] key_round);
    }
}