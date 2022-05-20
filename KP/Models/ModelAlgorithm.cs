using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using KP.Context;
using KP.Context.Interface;
using Soldatov.Wpf.MVVM.Core;

namespace KP.Models
{
    public class AlgorithmModel : ViewModelBase
    {
        private IAlgorithm _algorithm;
        private Utility.CipheringMode _ciphering_mode;
        private byte[] _key;
        private byte[][] _keys_round;
        //private byte[] _content_bytes;
        private ulong _init_vector=0;

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
            get {return _ciphering_mode;}
            set {_ciphering_mode=value; invokePropertyChanged("Ciphering");}
        }
        public byte[] Key
        {
            get {return _key;}
            set 
            {
                _key=value;
                invokePropertyChanged("Key");
                if(_key!=null)
                    _keys_round=Algorithm.getKeysRound(_key);
            }
        }

        public AlgorithmModel()
        {
            Algorithms=new ObservableCollection<IAlgorithm>
            {
                new Camellia(),
                new ElGamal()
            };
            Algorithm=Algorithms[0];
        }

        public void initialize()
        { //System.Diagnostics.Trace.WriteLine($"AAAAAA: {}");
            if(Algorithm==Algorithms[0])
            {
                if(_key==null)
                {
                    byte[] number_bytes_possible=new byte[] { 16, 24, 32 }, tmp=new byte[number_bytes_possible[Utility.Rng.Next(3)]];
                    Utility.Rng.NextBytes(tmp);
                    Key=tmp;
                }
                if(_init_vector==0)
                {
                    byte[] buffer=new byte[8];
                    Utility.Rng.NextBytes(buffer);
                    _init_vector=BitConverter.ToUInt64(buffer, 0);
                }
            }
        }
        public void encrypt(byte[] bytes_input, out byte[][] bytes_output)                  // TODO refactor
        {
            initialize();

            byte[][] bytes_input_parts;
            int block_length=16, bytes_number_blocks=(bytes_input.Length+block_length-1)/block_length, i=0;
            Task[] tasks=new Task[bytes_number_blocks];
            BigInteger mask=Utility.getNBitMask(block_length/2);

            bytes_output=new byte[bytes_number_blocks][];
            if(_algorithm is Camellia)                      // TODO algorythm.isSymmetric
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        bytes_input_parts=bytes_input.toArray2D(block_length, Utility.PaddingType.RKCS7);
                        for(; i<bytes_number_blocks; i++)
                        {
                            Algorithm.encrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                        }
                        break;
                    case Utility.CipheringMode.CBC:
                        bytes_input_parts=bytes_input.toArray2D(block_length, Utility.PaddingType.RKCS7);
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        byte[] tmp;

                        for(; i<bytes_number_blocks; i++)
                        {
                            tmp=(new BigInteger(bytes_input_parts[i])^bytes_input_part_init_biginteger).ToByteArray();
                            Utility.pad(ref tmp, block_length);
                            Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                            bytes_input_part_init_biginteger=new BigInteger(bytes_output[i]);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        bytes_input_parts=bytes_input.toArray2D(block_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, block_length);
                        Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                        bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();

                        for(i++; i<bytes_number_blocks; i++)
                        {
                            Algorithm.encrypt(in bytes_output[i-1], out bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        bytes_input_parts=bytes_input.toArray2D(block_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, block_length);
                        Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                        tmp=bytes_output[i];
                        bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();

                        for(i++; i<bytes_number_blocks; i++)
                        {
                            Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                            tmp=bytes_output[i];
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.CTR:
                        bytes_input_parts=bytes_input.toArray2D(block_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        for(; i<bytes_number_blocks; i++)
                        {
                            tmp=(bytes_input_part_init_biginteger^i).ToByteArray();
                            Utility.pad(ref tmp, block_length);
                            Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.RD:
                        bytes_input_parts=bytes_input.toArray2D(block_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        bytes_output=new byte[bytes_output.Length+1][];

                        tmp=bytes_input_part_init_biginteger.ToByteArray();
                        Utility.pad(ref tmp, block_length);
                        Algorithm.encrypt(in tmp, out bytes_output[i++], _keys_round);
                        for(int j=0; i<bytes_number_blocks+1; i++, j++)
                        {
                            bytes_input_parts[j]=(bytes_input_part_init_biginteger^new BigInteger(bytes_input_parts[j])).ToByteArray();
                            Utility.pad(ref bytes_input_parts[j], block_length);
                            Algorithm.encrypt(in bytes_input_parts[j], out bytes_output[i], _keys_round);
                            bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger&mask;
                        }
                        break;
                    case Utility.CipheringMode.RD_H:
                        break;
                }
            }
            else
            {
                _algorithm.encrypt(in bytes_input, out byte[] bytes_output_1d, _keys_round);
                bytes_output_1d.toArray2D(block_length/2);
            }
        }
        public void decrypt(byte[] bytes_input, out byte[][] bytes_output)                      // TODO refactor
        {
            initialize();
            
            byte[][] bytes_input_parts;
            int block_length=16, bytes_number_blocks=(bytes_input.Length+block_length-1)/block_length, i=0;
            Task[] tasks=new Task[bytes_number_blocks];
            BigInteger mask=Utility.getNBitMask(block_length/2);
            
            bytes_output=new byte[bytes_number_blocks][];
            if(_algorithm is Camellia)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        bytes_input_parts=bytes_input.toArray2D(block_length);
                        for(; i<bytes_number_blocks; i++)
                        {
                            Algorithm.decrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                        }
                        break;
                    case Utility.CipheringMode.CBC:
                        bytes_input_parts=bytes_input.toArray2D(block_length);
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        for(; i<bytes_number_blocks; i++)
                        {
                            Algorithm.decrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^bytes_input_part_init_biginteger).ToByteArray();
                            bytes_input_part_init_biginteger=new BigInteger(bytes_input_parts[i]);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        bytes_input_parts=bytes_input.toArray2D(block_length);

                        byte[] tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, block_length);
                        Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                        bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();

                        for(i++; i<bytes_number_blocks; i++)
                        {
                            Algorithm.encrypt(in bytes_input_parts[i-1], out bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        bytes_input_parts=bytes_input.toArray2D(block_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, block_length);
                        Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                        tmp=bytes_output[i];
                        bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();

                        for(i++; i<bytes_number_blocks; i++)
                        {
                            Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                            tmp=bytes_output[i];
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.CTR:
                        bytes_input_parts=bytes_input.toArray2D(block_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        for(; i<bytes_number_blocks; i++)
                        {
                            tmp=(bytes_input_part_init_biginteger^i).ToByteArray();
                            Utility.pad(ref tmp, block_length);
                            Algorithm.encrypt(in tmp, out bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(bytes_input_parts[i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.RD:
                        bytes_input_parts=bytes_input.toArray2D(block_length);
                        Algorithm.decrypt(in bytes_input_parts[i], out bytes_output[i], _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output[i++]);
                        bytes_output=new byte[bytes_output.Length-1][];

                        for(int j=0; i<bytes_number_blocks; i++, j++)
                        {
                            bytes_input_parts[i]=(bytes_input_part_init_biginteger^new BigInteger(bytes_input_parts[i])).ToByteArray();
                            Utility.pad(ref bytes_input_parts[i], block_length);
                            Algorithm.encrypt(in bytes_input_parts[i], out bytes_output[j], _keys_round);
                            bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger&mask;
                        }
                        break;
                    case Utility.CipheringMode.RD_H:
                        break;
                }
            }
            else
            {
                _algorithm.decrypt(in bytes_input, out byte[] bytes_output_1d, _keys_round);
                bytes_output_1d.toArray2D(block_length/2);
            }
        }
    }
}