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
    public class AlgorithmModel : ViewModelBase         // TODO add asymmetric and symmetric model (maybe?)
    {
        private IAlgorithm _algorithm;
        private Utility.CipheringMode _ciphering_mode;
        //private Task[] _tasks;
        private byte[] _key;
        private byte[][] _keys_round, _bytes_input_parts;
        private ulong _init_vector=0;
        private int _message_length, _bytes_number_messages=100, _i=0;
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
        public int Bytes_Number_Messages
        {
            get {return _bytes_number_messages;}
            set {_bytes_number_messages=value; invokePropertyChanged("Bytes_Number_Messages");}
        }
        public int Bytes_Current_Message_Number
        {
            get {return _i;}
            set {_i=value; invokePropertyChanged("Bytes_Current_Message_Number");}
        }
        public int I_Increment()
        {
            return Interlocked.Increment(ref _i);
            invokePropertyChanged("Bytes_Current_Message_Number");
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

        public async Task<byte[][]> encrypt(byte[] bytes_input, CancellationToken token) // TODO refactor
        {
            initialize();

            _message_length=_algorithm.MessageLength;
            Bytes_Number_Messages=(bytes_input.Length+_message_length-1)/_message_length;
            //_tasks=new Task[_bytes_number_messages];

            byte[][] bytes_output=new byte[_bytes_number_messages][];
            Bytes_Current_Message_Number=0;
            if(_algorithm.IsSymmetric)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length, Utility.PaddingType.RKCS7);
                        Parallel.For(Bytes_Current_Message_Number, _bytes_number_messages, i => 
                        {
                            _algorithm.encrypt(in _bytes_input_parts[i], ref bytes_output[i], _keys_round);
                            Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                        });
                        break;
                    case Utility.CipheringMode.CBC:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length, Utility.PaddingType.RKCS7);
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        byte[] tmp;

                        for(; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            tmp=(new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number])^bytes_input_part_init_biginteger).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                            bytes_input_part_init_biginteger=new BigInteger(bytes_output[Bytes_Current_Message_Number]);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                        bytes_output[Bytes_Current_Message_Number]=(new BigInteger(bytes_output[Bytes_Current_Message_Number])^new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number])).ToByteArray();

                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            _algorithm.encrypt(in bytes_output[Bytes_Current_Message_Number-1], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                            bytes_output[Bytes_Current_Message_Number]=(new BigInteger(bytes_output[Bytes_Current_Message_Number])^new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                        tmp=bytes_output[_i];
                        bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        for(Bytes_Current_Message_Number++; _i<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            tmp=bytes_output[_i];
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.CTR:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        Parallel.For(Bytes_Current_Message_Number, _bytes_number_messages, i => 
                        {
                            tmp=(bytes_input_part_init_biginteger^i).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(_bytes_input_parts[i])).ToByteArray();
                            Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                        });
                        break;
                    case Utility.CipheringMode.RD:
                        _mask=Utility.getNBitMask(_message_length/2);
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        bytes_output=new byte[bytes_output.Length+1][];

                        tmp=bytes_input_part_init_biginteger.ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[Bytes_Current_Message_Number++], _keys_round);
                        for(int j=0; Bytes_Current_Message_Number<_bytes_number_messages+1; Bytes_Current_Message_Number++, j++)
                        {
                            _bytes_input_parts[j]=(bytes_input_part_init_biginteger^new BigInteger(_bytes_input_parts[j])).ToByteArray();
                            Utility.pad(ref _bytes_input_parts[j], _message_length);
                            _algorithm.encrypt(in _bytes_input_parts[j], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
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
                for( ; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                {
                    _algorithm.encrypt(in _bytes_input_parts[Bytes_Current_Message_Number], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                }
            }
            return bytes_output;
        }
        public async Task<byte[][]> decrypt(byte[] bytes_input, CancellationToken token)                      // TODO refactor
        {
            initialize();

            _message_length=_algorithm.MessageLength;
            Bytes_Number_Messages=(bytes_input.Length+_message_length-1)/_message_length;
            //_tasks=new Task[_bytes_number_messages];
            
            byte[][] bytes_output=new byte[_bytes_number_messages][];
            Bytes_Current_Message_Number=0;
            if(_algorithm.IsSymmetric)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        Parallel.For(Bytes_Current_Message_Number, _bytes_number_messages, i => 
                        {
                            _algorithm.decrypt(in _bytes_input_parts[i], ref bytes_output[i], _keys_round);
                            Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                        });
                        break;
                    case Utility.CipheringMode.CBC:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        for(; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            _algorithm.decrypt(in _bytes_input_parts[Bytes_Current_Message_Number], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                            bytes_output[Bytes_Current_Message_Number]=(new BigInteger(bytes_output[Bytes_Current_Message_Number])^bytes_input_part_init_biginteger).ToByteArray();
                            bytes_input_part_init_biginteger=new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number]);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        byte[] tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                        bytes_output[Bytes_Current_Message_Number]=(new BigInteger(bytes_output[Bytes_Current_Message_Number])^new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number])).ToByteArray();

                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            _algorithm.encrypt(in _bytes_input_parts[Bytes_Current_Message_Number-1], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                            bytes_output[Bytes_Current_Message_Number]=(new BigInteger(bytes_output[Bytes_Current_Message_Number])^new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                        tmp=bytes_output[_i];
                        bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        for(Bytes_Current_Message_Number++; _i<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            _algorithm.encrypt(in tmp, ref bytes_output[_i], _keys_round);
                            tmp=bytes_output[_i];
                            bytes_output[_i]=(new BigInteger(bytes_output[_i])^new BigInteger(_bytes_input_parts[_i])).ToByteArray();
                        }
                        break;
                    case Utility.CipheringMode.CTR:                                     // BUG random block error
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);

                        Parallel.For(Bytes_Current_Message_Number, _bytes_number_messages, i => 
                        {
                            tmp=(bytes_input_part_init_biginteger^i).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output[i], _keys_round);
                            bytes_output[i]=(new BigInteger(bytes_output[i])^new BigInteger(_bytes_input_parts[i])).ToByteArray();
                            Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                        });
                        break;
                    case Utility.CipheringMode.RD:
                        _mask=Utility.getNBitMask(_message_length/2);
                        _bytes_input_parts=bytes_input.toArray2D(_message_length);
                        _algorithm.decrypt(in _bytes_input_parts[Bytes_Current_Message_Number], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output[Bytes_Current_Message_Number++]);
                        bytes_output=new byte[bytes_output.Length-1][];

                        for(int j=0; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++, j++)
                        {
                            _bytes_input_parts[Bytes_Current_Message_Number]=(bytes_input_part_init_biginteger^new BigInteger(_bytes_input_parts[Bytes_Current_Message_Number])).ToByteArray();
                            Utility.pad(ref _bytes_input_parts[Bytes_Current_Message_Number], _message_length);
                            _algorithm.encrypt(in _bytes_input_parts[Bytes_Current_Message_Number], ref bytes_output[j], _keys_round);
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
                for( ; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                {
                    _algorithm.decrypt(in _bytes_input_parts[Bytes_Current_Message_Number], ref bytes_output[Bytes_Current_Message_Number], _keys_round);
                }
            }
            return bytes_output;
        }
    }
}