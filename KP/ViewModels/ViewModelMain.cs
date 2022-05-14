using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using KP.Context;
using KP.Context.Interface;
using KP.Models;
using Microsoft.Win32;
using Soldatov.Wpf.MVVM.Core;

namespace KP.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        public enum CryptionMode
        {
            Encryption,
            Decryption
        }

        private FileInfo _file;
        private AlgorithmModel _algorithm_model;
        private CryptionMode _cryption_mode;
        private byte[] _content_bytes;
        private string _content_string;
        
        
        private RelayCommand _command_crypt;
        private RelayCommand _command_open_file;

        public FileInfo File
        {
            get {return _file;}
            set {_file=value; OnPropertiesChanged("File");}
        }

        public AlgorithmModel Algorithm_Model
        {
            get {return _algorithm_model;}
            set {_algorithm_model=value; OnPropertyChanged("Algorithm_Model");}
        }

        public CryptionMode Cryption
        {
            get {return _cryption_mode;}
            set {_cryption_mode=value;}
        }

        public string Content_String
        {
            get {return _content_string;}
            set {_content_string=value; OnPropertyChanged("Content_String");}
        }

        public RelayCommand CommandEncrypt
        {
            get {return _command_crypt??=new RelayCommand(cryptionExecute, cryptionCanExecute);}
        }
        public RelayCommand CommandOpenFile
        {
            get {return _command_open_file??=new RelayCommand(fileOpenExecute, fileOpenCanExecute);}
        }
        
        public ViewModelMain()
        {
            Algorithm_Model=new AlgorithmModel();
        }
        
        private void cryptionExecute(object parameter)
        {
            switch(_cryption_mode)
            {
                case CryptionMode.Encryption:
                    _algorithm_model.encrypt();
                    break;
                case CryptionMode.Decryption:
                    _algorithm_model.decrypt();
                    break;
            }
        }
        private bool cryptionCanExecute(object parameter)
        {
            return _content_bytes.Length!=0;
        }
        
        private void fileOpenExecute(object parameter)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog();

            if(open_file_dialog.ShowDialog()==false)
                return;
            
            File=new FileInfo(open_file_dialog.FileName);
            using(FileStream file_stream=File.OpenRead())
            {
                _content_bytes=new byte[File.Length];
                
                for(int length=(int)File.Length, number_bytes_read_sum=0, number_bytes_read; length>0; length-=number_bytes_read, number_bytes_read_sum+=number_bytes_read)
                {
                    number_bytes_read=file_stream.Read(_content_bytes, number_bytes_read_sum, length);
                    if(number_bytes_read==0)
                        break;
                }
            }
            Content_String=Encoding.UTF8.GetString(_content_bytes);
        }
        private bool fileOpenCanExecute(object parameter)
        {
            return true;
        }
    }
}