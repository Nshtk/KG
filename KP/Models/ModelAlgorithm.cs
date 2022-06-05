using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private byte[] _key;
        private byte[][] _keys_round;
        private ulong _init_vector=0;
        private int _message_length, _bytes_number_messages=100, _i=0;

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

        public void generateSymmetricKey(int min=0, int max=3)
        {
            byte[] number_bytes_possible=new byte[] {16, 24, 32}, tmp=new byte[number_bytes_possible[Utility.Rng.Next(min, max)]];
            Utility.Rng.NextBytes(tmp);
            Key=tmp;
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
                            generateSymmetricKey();
                        }
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

        public async Task<int> encrypt(FileInfo file_input, FileInfo file_output, CancellationToken token)
        {
            initialize();

            _message_length=_algorithm.MessageLength;
            Bytes_Number_Messages=((int)file_input.Length+_message_length-1)/_message_length;
            ConcurrentDictionary<int, byte[]> dict=new ConcurrentDictionary<int, byte[]>();
            byte[] bytes_input_part=new byte[_message_length], bytes_output_part=new byte[_message_length], bytes_output;
            object locker=new object();
            using FileStream file_stream_input=file_input.OpenRead();
            using FileStream file_stream_output=file_output.Create();
            int bytes_read, thread_count=20;;

            Bytes_Current_Message_Number=0;
            if(_algorithm.IsSymmetric)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        for(int j=0; j<_bytes_number_messages && file_stream_input.CanRead; j+=thread_count)
                        {
                            Parallel.For(j,  Math.Min(j+thread_count, _bytes_number_messages), new ParallelOptions {MaxDegreeOfParallelism=thread_count}, (i, state) =>
                            {
                                int bytes_read_parallel, seek=i*_message_length;
                                byte[] bytes_input_part_parallel=new byte[_message_length], bytes_output_part_parallel=new byte[_message_length];

                                lock(locker)
                                {
                                    file_stream_input.Seek(seek, SeekOrigin.Begin);
                                    if((bytes_read_parallel=file_stream_input.Read(bytes_input_part_parallel, 0, _message_length))==0)
                                        state.Break();
                                    else if(bytes_read_parallel<_message_length)
                                        Utility.pad(ref bytes_input_part_parallel, _message_length, Utility.PaddingType.RKCS7);
                                }
                                
                                _algorithm.encrypt(in bytes_input_part_parallel, ref bytes_output_part_parallel, _keys_round);
                                
                                dict.TryAdd(i, bytes_output_part_parallel);
                                Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                            });
                            bytes_output=dict.Values.ToArray().SelectMany(a => a).ToArray();
                            file_stream_output.Write(bytes_output, 0, bytes_output.Length);
                            dict.Clear();
                        }
                        Bytes_Current_Message_Number++;
                        break;
                    case Utility.CipheringMode.CBC:
                        byte[] tmp;
                        bytes_output_part=new byte[_message_length];
                        BigInteger bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        
                        tmp=bytes_input_part_init_biginteger.ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        
                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        
                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages+1; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            tmp=(new BigInteger(bytes_input_part)^bytes_input_part_init_biginteger).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                            bytes_input_part_init_biginteger=new BigInteger(bytes_output_part);
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                        
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;
                        if(bytes_read<_message_length)
                            Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                        
                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                        
                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);

                        byte[] bytes_output_part_prev;
                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            bytes_output_part_prev=bytes_output_part;
                            Utility.pad(ref bytes_output_part_prev, _message_length);
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            _algorithm.encrypt(in bytes_output_part_prev, ref bytes_output_part, _keys_round);
                            bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }

                        break;
                    case Utility.CipheringMode.OFB:
                        bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                        
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;
                        if(bytes_read<_message_length)
                            Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                        
                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        tmp=bytes_output_part;
                        bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                        
                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);

                        for(Bytes_Current_Message_Number++; _i<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                            tmp=bytes_output_part;
                            bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                    case Utility.CipheringMode.CTR:
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        for(int j=0; j<_bytes_number_messages && file_stream_input.CanRead; j+=thread_count)
                        {
                            Parallel.For(j, Math.Min(j+thread_count, _bytes_number_messages), new ParallelOptions {MaxDegreeOfParallelism=thread_count}, (i, state) => 
                            {
                                int bytes_read_parallel, seek=i*_message_length;
                                byte[] bytes_input_part_parallel=new byte[_message_length], bytes_output_part_parallel=new byte[_message_length], tmp_parallel;

                                lock(locker)
                                    file_stream_input.Seek(seek, SeekOrigin.Begin);
                                if((bytes_read_parallel=file_stream_input.Read(bytes_input_part_parallel, 0, _message_length))==0)
                                    state.Break();
                                else if(bytes_read_parallel<_message_length)
                                    Utility.pad(ref bytes_input_part_parallel, _message_length, Utility.PaddingType.RKCS7);
                            
                                tmp_parallel=(bytes_input_part_init_biginteger^i).ToByteArray();
                                Utility.pad(ref tmp_parallel, _message_length);
                                _algorithm.encrypt(in tmp_parallel, ref bytes_output_part_parallel, _keys_round);
                                bytes_output_part_parallel=(new BigInteger(bytes_output_part_parallel)^new BigInteger(bytes_input_part_parallel)).ToByteArray();
                                
                                dict.TryAdd(i, bytes_output_part_parallel);
                                Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                            });
                            bytes_output=dict.Values.ToArray().SelectMany(a => a).ToArray();
                            file_stream_output.Write(bytes_output, 0, bytes_output.Length);
                            dict.Clear();
                        }
                        break;
                    case Utility.CipheringMode.RD:
                        bytes_output_part=new byte[_message_length];

                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        tmp=bytes_input_part_init_biginteger.ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        
                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        
                        for(int j=0; Bytes_Current_Message_Number<_bytes_number_messages+1; Bytes_Current_Message_Number++, j++, bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                                
                            bytes_input_part=(bytes_input_part_init_biginteger^new BigInteger(bytes_input_part)).ToByteArray();
                            Utility.pad(ref bytes_input_part, _message_length);
                            _algorithm.encrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }

                        break;
                    case Utility.CipheringMode.RD_H:
                        byte[] buf=new byte[file_stream_input.Length];
                        bytes_output_part=new byte[_message_length];

                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        tmp=bytes_input_part_init_biginteger.ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        
                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        
                        file_stream_input.Read(buf, 0, (int)file_stream_input.Length);
                        tmp=BitConverter.GetBytes(buf.GetHashCode());
                        Utility.pad(ref tmp, _message_length, Utility.PaddingType.RKCS7);
                        tmp=(bytes_input_part_init_biginteger^new BigInteger(tmp)).ToByteArray();
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        file_stream_input.Seek(0, SeekOrigin.Begin);

                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        
                        for( ; Bytes_Current_Message_Number<_bytes_number_messages+2; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger;
                            tmp=(bytes_input_part_init_biginteger^new BigInteger(bytes_input_part)).ToByteArray();
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                }
            }
            else
            {
                for( ; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                {
                    bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                    if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                        break;
                    if(bytes_read<_message_length)
                        Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                    
                    _algorithm.encrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                    await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                }
            }
            return 0;
        }
        public async Task<int> decrypt(FileInfo file_input, FileInfo file_output, CancellationToken token)
        {
            initialize();
            
            _message_length=_algorithm.MessageLength;
            Bytes_Number_Messages=((int)file_input.Length+_message_length-1)/_message_length;
            ConcurrentDictionary<int, byte[]> dict=new ConcurrentDictionary<int, byte[]>();
            byte[] bytes_input_part=new byte[_message_length], bytes_output_part=new byte[_message_length], bytes_output;
            object locker=new object();
            using FileStream file_stream_input=file_input.OpenRead();
            using FileStream file_stream_output=file_output.Create();
            int bytes_read, thread_count=20;
            
            Bytes_Current_Message_Number=0;
            if(_algorithm.IsSymmetric)
            {
                switch(Ciphering)
                {
                    case Utility.CipheringMode.ECB:
                        for(int j=0; j<_bytes_number_messages && file_stream_input.CanRead; j+=thread_count)
                        {
                            Parallel.For(j, Math.Min(j+thread_count, _bytes_number_messages), new ParallelOptions {MaxDegreeOfParallelism=thread_count}, (i, state) =>
                            {
                                int bytes_read_parallel, seek=i*_message_length;
                                byte[] bytes_input_part_parallel=new byte[_message_length], bytes_output_part_parallel=new byte[_message_length];

                                lock(locker)
                                    file_stream_input.Seek(seek, SeekOrigin.Begin);
                                if((bytes_read_parallel=file_stream_input.Read(bytes_input_part_parallel, 0, _message_length))==0)
                                    state.Break();
                                else if(bytes_read_parallel<_message_length)
                                    Utility.pad(ref bytes_input_part_parallel, _message_length, Utility.PaddingType.RKCS7);

                                _algorithm.decrypt(in bytes_input_part_parallel, ref bytes_output_part_parallel, _keys_round);
                                
                                dict.TryAdd(i, bytes_output_part_parallel);
                                Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                            });
                            bytes_output=dict.Values.ToArray().SelectMany(a => a).ToArray();
                            file_stream_output.Write(bytes_output, 0, bytes_output.Length);
                            dict.Clear();
                        }
                        Bytes_Current_Message_Number++;
                        break;
                    case Utility.CipheringMode.CBC:
                        BigInteger bytes_input_part_init_biginteger;
                        
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;

                        _algorithm.decrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output_part);
                        
                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages+1; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            _algorithm.decrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                            bytes_output_part=(new BigInteger(bytes_output_part)^bytes_input_part_init_biginteger).ToByteArray();
                            bytes_input_part_init_biginteger=new BigInteger(bytes_input_part);
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                    case Utility.CipheringMode.CFB:
                        bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;

                        byte[] tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();

                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);

                        byte[] bytes_input_part_prev;
                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part_prev=bytes_input_part;
                            Utility.pad(ref bytes_input_part_prev, _message_length);
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            _algorithm.encrypt(in bytes_input_part_prev, ref bytes_output_part, _keys_round);
                            bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                    case Utility.CipheringMode.OFB:
                        bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;

                        tmp=new BigInteger(_init_vector).ToByteArray();
                        Utility.pad(ref tmp, _message_length);
                        _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                        tmp=bytes_output_part;
                        bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                        
                        await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        for(Bytes_Current_Message_Number++; _i<_bytes_number_messages; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            Utility.pad(ref tmp, _message_length);
                            _algorithm.encrypt(in tmp, ref bytes_output_part, _keys_round);
                            tmp=bytes_output_part;
                            bytes_output_part=(new BigInteger(bytes_output_part)^new BigInteger(bytes_input_part)).ToByteArray();
                            
                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                    case Utility.CipheringMode.CTR:
                        bytes_input_part_init_biginteger=new BigInteger(_init_vector);
                        for(int j=0; j<_bytes_number_messages && file_stream_input.CanRead; j+=thread_count)
                        {
                            Parallel.For(j, Math.Min(j+thread_count, _bytes_number_messages), new ParallelOptions {MaxDegreeOfParallelism=thread_count}, (i, state) => 
                            {
                                int bytes_read_parallel, seek=i*_message_length;
                                byte[] bytes_input_part_parallel=new byte[_message_length], bytes_output_part_parallel=new byte[_message_length], tmp_parallel;

                                lock(locker)
                                    file_stream_input.Seek(seek, SeekOrigin.Begin);
                                if((bytes_read_parallel=file_stream_input.Read(bytes_input_part_parallel, 0, _message_length))==0)
                                    state.Break();
                                else if(bytes_read_parallel<_message_length)
                                    Utility.pad(ref bytes_input_part_parallel, _message_length, Utility.PaddingType.RKCS7);
                            
                                tmp_parallel=(bytes_input_part_init_biginteger^i).ToByteArray();
                                Utility.pad(ref tmp_parallel, _message_length);
                                _algorithm.encrypt(in tmp_parallel, ref bytes_output_part_parallel, _keys_round);
                                bytes_output_part_parallel=(new BigInteger(bytes_output_part_parallel)^new BigInteger(bytes_input_part_parallel)).ToByteArray();
                                
                                dict.TryAdd(i, bytes_output_part_parallel);
                                Bytes_Current_Message_Number=Interlocked.Increment(ref _i);
                            });
                            bytes_output=dict.Values.ToArray().SelectMany(a => a).ToArray();
                            file_stream_output.Write(bytes_output, 0, bytes_output.Length);
                            dict.Clear();
                        }
                        break;
                    case Utility.CipheringMode.RD:
                        bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;

                        _algorithm.decrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output_part);

                        for(Bytes_Current_Message_Number++; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++, bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            Utility.pad(ref bytes_input_part, _message_length);
                            _algorithm.decrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                            bytes_output_part=(bytes_input_part_init_biginteger^new BigInteger(bytes_output_part)).ToByteArray();
                            Utility.pad(ref bytes_output_part, _message_length);

                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        break;
                    case Utility.CipheringMode.RD_H:
                        tmp=new byte[_message_length];
                        bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length];
                            
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;

                        _algorithm.decrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                        bytes_input_part_init_biginteger=new BigInteger(bytes_output_part);
                        
                        if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                            break;
                        _algorithm.decrypt(in bytes_input_part, ref tmp, _keys_round);
                        Utility.pad(ref tmp, _message_length);
                        tmp=(bytes_input_part_init_biginteger^new BigInteger(tmp)).ToByteArray();

                        for( ; Bytes_Current_Message_Number<_bytes_number_messages-2; Bytes_Current_Message_Number++)
                        {
                            bytes_input_part=new byte[_message_length]; bytes_output_part=new byte[_message_length+1];
                            
                            if((bytes_read=file_stream_input.Read(bytes_input_part, 0, _message_length))==0)
                                break;
                            if(bytes_read<_message_length)
                                Utility.pad(ref bytes_input_part, _message_length, Utility.PaddingType.RKCS7);
                            
                            bytes_input_part_init_biginteger+=bytes_input_part_init_biginteger;
                            _algorithm.decrypt(in bytes_input_part, ref tmp, _keys_round);
                            bytes_output_part=(bytes_input_part_init_biginteger^new BigInteger(tmp)).ToByteArray();

                            await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                        }
                        Bytes_Current_Message_Number+=2;
                        break;
                }
            }
            else
            {
                int message_length=_message_length*2;
                _bytes_number_messages=((int)file_input.Length+message_length-1)/message_length;
                
                for( ; Bytes_Current_Message_Number<_bytes_number_messages; Bytes_Current_Message_Number++)
                {
                    bytes_input_part=new byte[message_length]; bytes_output_part=new byte[message_length];
                            
                    if((bytes_read=file_stream_input.Read(bytes_input_part, 0, message_length))==0)
                        break;
                    if(bytes_read<message_length)
                        Utility.pad(ref bytes_input_part, message_length, Utility.PaddingType.RKCS7);
                    
                    _algorithm.decrypt(in bytes_input_part, ref bytes_output_part, _keys_round);
                    await file_stream_output.WriteAsync(bytes_output_part, 0, bytes_output_part.Length);
                }

            }
            return 0;
        }
    }
}