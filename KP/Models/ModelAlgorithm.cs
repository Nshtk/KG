using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KP.Context;
using KP.Context.Interface;
using Soldatov.Wpf.MVVM.Core;

namespace KP.Models
{
    public class AlgorithmModel : ViewModelBase         // TODO add asymmetric and symmetric model
    {
        private IAlgorithm _algorithm;
        private Utility.CipheringMode _ciphering_mode;
        private Task[] _tasks;
        private byte[] _key;
        private byte[][] _keys_round, _bytes_input_parts;
        private ulong _init_vector=0;
        private int _message_length, _bytes_number_messages, _i;
        private BigInteger _mask;
        
        private Visibility _key_visibility;
        private Visibility _cryption_mode_visibility;

        public ObservableCollection<IAlgorithm> Algorithms
        {
            get;
            set;
        }
        public IAlgorithm Algorithm
        {
            get { return _algorithm; }
            set 
            { 
                _algorithm=value; 
                if(_algorithm is ElGamal)
                {
                    Key_Visibility=Visibility.Collapsed;
                    Cryption_Mode_Visibility=Visibility.Collapsed;
                }
                else
                {
                    Key_Visibility=Visibility.Visible;
                    Cryption_Mode_Visibility=Visibility.Visible;
                }
                invokePropertyChanged("Algorithm");
            }
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
        
        public Visibility Key_Visibility
        {
            get {return _key_visibility;}
            set {_key_visibility=value; invokePropertyChanged("Key_Visibility");}
        }
        public Visibility Cryption_Mode_Visibility
        {
            get {return _cryption_mode_visibility;}
            set {_cryption_mode_visibility=value; invokePropertyChanged("Cryption_Mode_Visibility");}
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
        {
            if(_algorithm.IsSymmetric)
            {
                switch(_algorithm)
                {
                    case Camellia:
                        if(_key==null)
                        {
                            byte[] number_bytes_possible=new byte[] {16, 24, 32}, tmp=new byte[number_bytes_possible[Utility.Rng.Next(3)]];
                            Utility.Rng.NextBytes(tmp);
                            Key=tmp;
                        }
                        break;
                    default:
                        break;
                }
                if(_init_vector==0)
                {
                    byte[] buffer=new byte[8];
                    Utility.Rng.NextBytes(buffer);
                    _init_vector=BitConverter.ToUInt64(buffer, 0);
                }
            }
            else
                if(_keys_round==null)
                    _keys_round=_algorithm.getKeysRound(_key);
        }
        
        private class Context           // for threading
        {
            //public IAlgorithm _algorithm;
            public byte[] _bytes_input;
            public byte[][] _bytes_output;
            public int _i;
            
            public Context(ref IAlgorithm algorithm, byte[] bytes_input, ref byte[][] bytes_output, int i)
            {
                //_algorithm=algorithm;
                _bytes_input=bytes_input;
                _bytes_output=bytes_output;
                _i=i;
            }
        }
        private void doStuff(object? obj)
        {
            if (obj is Context context)
            {
                _algorithm.encrypt(in context._bytes_input, ref context._bytes_output[context._i], _keys_round);
            }
        }
        public async Task<byte[][]> encrypt(byte[] bytes_input, CancellationToken token) // TODO refactor
        {
            initialize();

            _message_length=_algorithm.MessageLength;
            _bytes_number_messages=(bytes_input.Length+_message_length-1)/_message_length;
            _tasks=new Task[_bytes_number_messages];
            Thread[] threads=new Thread[_bytes_number_messages];
            byte[][] bytes_output=new byte[_bytes_number_messages][];
            _i=0;
            Thread thread;
            if(_algorithm.IsSymmetric)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length, Utility.PaddingType.RKCS7);
                        Parallel.For(_i, _bytes_number_messages, i => 
                        {
                            _algorithm.encrypt(in _bytes_input_parts[i], ref bytes_output[i], _keys_round);
                        });
                        /*for(; _i<_bytes_number_messages; _i++)            // why threading is so much harder than in c++???
                        {
                            threads[_i]=new Thread(doStuff);
                            threads[_i].Start(new Context(ref _algorithm, bytes_input, ref bytes_output, _i));
                            //thread=new Thread((ref byte[] bytes_output) => _algorithm.encrypt(in _bytes_input_parts[_i], ref bytes_output, _keys_round));
                            /*_tasks[_i]=Task.Run(delegate() 
                            {
                                met(_bytes_input_parts[_i], bytes_output[_i], _keys_round);
                            });#1#
                        }*/
                        break;
                    case Utility.CipheringMode.CBC:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length, Utility.PaddingType.RKCS7);
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        byte[] tmp;

                        for(; _i<_bytes_number_messages; _i++)
                        {
                            tmp=(new BigInteger(_bytes_input_parts[_i])^bytes_input_part_init_biginteger).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            bytes_input_part_init_biginteger=new BigInteger(bytes_output[_i]);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                        bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();

                        for(_i++; _i<_bytes_number_messages; _i++)
                        {
                            _algorithm.encrypt(in bytes_output[_i-1], ref bytes_output[_i], _keys_round);
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                        tmp=bytes_output[_i];
                        bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        _i++;
                        Parallel.For(_i, _bytes_number_messages, i => 
                        {
                            _algorithm.encrypt(in tmp, ref bytes_output[i], _keys_round);
                            tmp=bytes_output[i];
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(_bytes_input_parts[i])).ToByteArray();
                        });
                        /*for(_i++; _i<_bytes_number_messages; _i++)
                        {
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            tmp=bytes_output[_i];
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }*/
                        break;
                    case Utility.CipheringMode.CTR:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        Parallel.For(_i, _bytes_number_messages, i => 
                        {
                            tmp=(bytes_input_part_init_biginteger^i).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(_bytes_input_parts[i])).ToByteArray();
                        });
                        /*for(; _i<_bytes_number_messages; _i++)
                        {
                            tmp=(bytes_input_part_init_biginteger^_i).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }*/
                        break;
                    case Utility.CipheringMode.RD:
                        _mask=Utility.getNBitMask(_message_length/2);
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        bytes_output=new byte[bytes_output.Length+1][];

                        tmp=bytes_input_part_init_biginteger.ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i++], _keys_round);
                        for(int j=0; _i<_bytes_number_messages+1; _i++, j++)
                        {
                            _bytes_input_parts[j]=(bytes_input_part_init_biginteger^new BigInteger(_bytes_input_parts[j])).ToByteArray();
                            Utility.pad(ref _bytes_input_parts[j], _message_length);
                            _algorithm.encrypt(in _bytes_input_parts[j], ref bytes_output[_i], _keys_round);
                            bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger&_mask;
                        }
                        break;
                    case Utility.CipheringMode.RD_H:
                        break;
                }
            }
            else
            {
                _bytes_input_parts=bytes_input.toArray2D(_message_length);
                for( ; _i<_bytes_number_messages; _i++)
                {
                    _algorithm.encrypt(in _bytes_input_parts[_i], ref bytes_output[_i], _keys_round);
                }
            }
            return bytes_output;
        }
        public async Task<byte[][]> decrypt(byte[] bytes_input, CancellationToken token)                      // TODO refactor
        {
            initialize();

            _message_length=_algorithm.MessageLength;
            _bytes_number_messages=(bytes_input.Length+_message_length-1)/_message_length;
            _tasks=new Task[_bytes_number_messages];
            
            byte[][] bytes_output=new byte[_bytes_number_messages][];
            _i=0;
            if(_algorithm.IsSymmetric)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        Parallel.For(_i, _bytes_number_messages, i => 
                        {
                            _algorithm.decrypt(in _bytes_input_parts[i], ref bytes_output[i], _keys_round);
                        });
                        /*for(; _i<_bytes_number_messages; _i++)
                        {
                            _algorithm.decrypt(in _bytes_input_parts[_i], ref bytes_output[_i], _keys_round);
                        }*/
                        break;
                    case Utility.CipheringMode.CBC:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        for(; _i<_bytes_number_messages; _i++)
                        {
                            _algorithm.decrypt(in _bytes_input_parts[_i], ref bytes_output[_i], _keys_round);
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^bytes_input_part_init_biginteger).ToByteArray();
                            bytes_input_part_init_biginteger=new BigInteger(_bytes_input_parts[_i]);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        byte[] tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                        bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();

                        for(_i++; _i<_bytes_number_messages; _i++)
                        {
                            _algorithm.encrypt(in _bytes_input_parts[_i-1], ref bytes_output[_i], _keys_round);
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                        tmp=bytes_output[_i];
                        bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        _i++;
                        Parallel.For(_i, _bytes_number_messages, i => 
                        {
                            _algorithm.encrypt(in tmp, ref bytes_output[i], _keys_round);
                            tmp=bytes_output[i];
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(_bytes_input_parts[i])).ToByteArray();
                        });
                        /*for(_i++; _i<_bytes_number_messages; _i++)
                        {
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            tmp=bytes_output[_i];
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }*/
                        break;
                    case Utility.CipheringMode.CTR:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        Parallel.For(_i, _bytes_number_messages, i => 
                        {
                            tmp=(bytes_input_part_init_biginteger^i).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(_bytes_input_parts[i])).ToByteArray();
                        });
                        /*for(; _i<_bytes_number_messages; _i++)
                        {
                            tmp=(bytes_input_part_init_biginteger^_i).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }*/
                        break;
                    case Utility.CipheringMode.RD:
                        _mask=Utility.getNBitMask(_message_length/2);
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        _algorithm.decrypt(in _bytes_input_parts[_i], ref bytes_output[_i], _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output[_i++]);
                        bytes_output=new byte[bytes_output.Length-1][];

                        for(int j=0; _i<_bytes_number_messages; _i++, j++)
                        {
                            _bytes_input_parts[_i]=(bytes_input_part_init_biginteger^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                            Utility.pad(ref _bytes_input_parts[_i], _message_length);
                            _algorithm.encrypt(in _bytes_input_parts[_i], ref bytes_output[j], _keys_round);
                            bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger&_mask;
                        }
                        break;
                    case Utility.CipheringMode.RD_H:
                        break;
                }
            }
            else
            {
                _bytes_number_messages=(bytes_input.Length+_message_length*2-1)/(_message_length*2);
                _bytes_input_parts=bytes_input.toArray2D(_message_length*2);
                bytes_output=new byte[_bytes_number_messages][];
                for( ; _i<_bytes_number_messages; _i++)
                {
                    _algorithm.decrypt(in _bytes_input_parts[_i], ref bytes_output[_i], _keys_round);
                }
            }
            return bytes_output;
        }
    }
}