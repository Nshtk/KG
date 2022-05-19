using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using KP.Context;
using KP.Context.Interface;
using Soldatov.Wpf.MVVM.Core;

namespace KP.Models
{
    public class AlgorithmModel : ViewModelBase
    {
        private IAlgorithm _algorithm;
        private Utility.CipheringMode _cipheringMode;
        private byte[] _key;
        private byte[][] _keys_round;
        //private byte[] _content_bytes;
        private ulong _init_vector;

        public ObservableCollection<IAlgorithm> Algorithms
        {
            get;
            set;
        }
        public IAlgorithm Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; invokePropertyChanged("Algorithm"); }
        }
        public Utility.CipheringMode Ciphering
        {
            get {return _cipheringMode;}
            set {_cipheringMode=value; invokePropertyChanged("Ciphering");}
        }
        public byte[] Key
        {
            get {return _key;}
            set {_key=value; invokePropertyChanged("Key"); _keys_round=Algorithm.getKeysRound(_key); }
        }

        public AlgorithmModel()
        {
            Algorithms = new ObservableCollection<IAlgorithm>
            {
                new Camellia(),
                new ElGamal()
            };
            Algorithm=Algorithms[0];
        }

        public void initialize()
        {
            if(Algorithm==Algorithms[0] && Key==null)
            { //System.Diagnostics.Trace.WriteLine($"AAAAAA: {}");
                byte[] number_bytes_possible=new byte[] {16, 24, 32}, tmp=new byte[number_bytes_possible[Utility.Rng.Next(3)]], buffer=new byte[8];;

                Utility.Rng.NextBytes(tmp);
                Key=tmp;
                Utility.Rng.NextBytes(buffer);
                _init_vector=BitConverter.ToUInt64(buffer, 0);
            }
        }
        public void encrypt(byte[] bytes_input, out byte[][] bytes_output)
        {
            initialize();

            byte[][] bytes_input_parts;
            int block_length=16, bytes_number_blocks=(bytes_input.Length+block_length-1)/block_length, i=0;
            Task[] tasks=new Task[bytes_number_blocks];

            bytes_output=new byte[bytes_number_blocks][];
            switch(Ciphering)
            {
                case Utility.CipheringMode.ECB:
                    bytes_input_parts=bytes_input.toArray2D(block_length, Utility.PaddingType.RKCS7);
                    for( ; i<bytes_number_blocks; i++)
                    {
                        Algorithm.encrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                    }
                    break;
                case Utility.CipheringMode.CBC:
                    BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                    bytes_input_parts=bytes_input.toArray2D(block_length, Utility.PaddingType.RKCS7);
                    
                    for(byte[] tmp; i<bytes_number_blocks; i++)
                    {
                        tmp=(new BigInteger(bytes_input_parts[i])^bytes_input_part_init_biginteger).ToByteArray();
                        Utility.pad(ref tmp, block_length);
                        Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output[i]);
                    }
                    break;
                case Utility.CipheringMode.CFB:
                    break;
                case Utility.CipheringMode.OFB:
                    break;
                case Utility.CipheringMode.CTR:
                    break;
                case Utility.CipheringMode.RD:
                    break;
                case Utility.CipheringMode.RD_H:
                    break;
            }
        }
        public void decrypt(byte[] bytes_input, out byte[][] bytes_output)
        {
            initialize();
            
            byte[][] bytes_input_parts;
            int block_length=16, bytes_number_blocks=(bytes_input.Length+block_length-1)/block_length, i=0;
            Task[] tasks=new Task[bytes_number_blocks];
            
            bytes_output=new byte[bytes_number_blocks][];
            switch (Ciphering)
            {
                case Utility.CipheringMode.ECB:
                    bytes_input_parts=bytes_input.toArray2D(block_length);
                    for( ; i<bytes_number_blocks; i++)
                    {
                        Algorithm.decrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                    }
                    break;
                case Utility.CipheringMode.CBC:
                    BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                    bytes_input_parts=bytes_input.toArray2D(block_length);
                    
                    for( ; i<bytes_number_blocks; i++)
                    {
                        Algorithm.decrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                        bytes_output[i]=(new BigInteger(bytes_output[i])^bytes_input_part_init_biginteger).ToByteArray();
                        bytes_input_part_init_biginteger=new BigInteger(bytes_input_parts[i]);
                    }
                    break;
                case Utility.CipheringMode.CFB:
                    break;
                case Utility.CipheringMode.OFB:
                    break;
                case Utility.CipheringMode.CTR:
                    break;
                case Utility.CipheringMode.RD:
                    break;
                case Utility.CipheringMode.RD_H:
                    break;
            }
        }
    }
}