using System;
using System.Collections.ObjectModel;
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
            {
                byte[] number_bytes_possible=new byte[] {16, 24, 32}, tmp=new byte[number_bytes_possible[Utility.random.Next(3)]];

                Utility.random.NextBytes(tmp);
                Key=tmp; //System.Diagnostics.Trace.WriteLine($"AAAAAA: {}");
            }
        }
        public void encrypt(byte[] bytes_input, out byte[][] bytes_output)
        {
            initialize();

            int block_length=16;
            byte[] bytes_input_part=new byte[block_length];
            int bytes_number_full_blocks=(bytes_input.Length+block_length-1)/block_length;
            int i=0, j=0;
            Task[] tasks=new Task[bytes_number_full_blocks];

            bytes_output=new byte[bytes_number_full_blocks][];
            if(bytes_input.Length%block_length!=0)
                PaddingMode                             //TODO pad
            
            switch (Ciphering)
            {
                case Utility.CipheringMode.ECB:
                    for(; i<bytes_number_full_blocks; i++, j+=block_length)
                    {
                        Array.Copy(bytes_input, j, bytes_input_part, 0, block_length);
                        Algorithm.encrypt(bytes_input_part, out bytes_output[i], _keys_round);
                    }
                    break;
                case Utility.CipheringMode.CBC:
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
            
            int block_length=16;
            byte[] bytes_input_part=new byte[block_length];
            int bytes_number_full_blocks=(bytes_input.Length+block_length-1)/block_length;
            int i=0, j=0;
            Task[] tasks=new Task[bytes_number_full_blocks];
            
            bytes_output=new byte[bytes_number_full_blocks][];
            
            switch (Ciphering)
            {
                case Utility.CipheringMode.ECB:
                    for(; i<bytes_number_full_blocks; i++, j+=block_length)
                    {
                        Array.Copy(bytes_input, j, bytes_input_part, 0, block_length);
                        Algorithm.decrypt(bytes_input_part, out bytes_output[i], _keys_round);
                    }
                    break;
                case Utility.CipheringMode.CBC:
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