namespace KP.Context.Interface
{
    public interface IKeyExpansion
    {
        public byte[][] getKeysRound(byte[] key);
    }

    public interface IRoundCiphering
    {
        public ulong functionF(ulong bits_input_part, ulong key_round, RoundCipheringCamelia.FunctionFModes mode); // TODO bits_input_part change type to byte[]
        public byte[] performRound(byte[] bytes, byte[] key_round);
    }
}