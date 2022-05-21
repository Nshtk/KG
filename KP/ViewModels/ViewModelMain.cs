using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using KP.Models;
using Microsoft.Win32;
using Soldatov.Wpf.MVVM.Core;

namespace KP.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        public enum CryptionMode
        {
            ENCRYPTION,
            DECRYPTION
        }

        private FileInfo _fileinfo_input, _fileinfo_output;
        private AlgorithmModel _algorithm_model;
        private CryptionMode _cryption_mode;
        private byte[] _fileinfo_input_content_bytes, _fileinfo_output_content_bytes;
        private string _fileinfo_input_content_string, _fileinfo_output_content_string;

        private Visibility _file_output_visiblity;
        private Visibility _file_log_visiblity;
        private RelayCommand _command_crypt;
        private RelayCommand _command_open_file;

        public FileInfo FileInfo_Input
        {
            get {return _fileinfo_input;}
            set {_fileinfo_input=value; FileInfo_Output=null; invokePropertiesChanged("FileInfo_Input", "FileInfo_Input_Name");}
        }
        public string FileInfo_Input_Name
        {
            get {return FileInfo_Input==null ? "File: " : "File: "+FileInfo_Input.Name;}
        }
        public string FileInfo_Input_Content_String
        {
            get {return _fileinfo_input_content_string;}
            set {_fileinfo_input_content_string=value; invokePropertyChanged("FileInfo_Input_Content_String");}
        }
        public FileInfo FileInfo_Output
        {
            get {return _fileinfo_output;}
            set {_fileinfo_output=value; File_Output_Visibility=Visibility.Hidden; invokePropertyChanged("FileInfo_Output");}
        }
        public string FileInfo_Output_Content_String
        {
            get {return _fileinfo_output_content_string;}
            set {_fileinfo_output_content_string=value; invokePropertyChanged("FileInfo_Output_Content_String");}
        }
        public AlgorithmModel Algorithm_Model
        {
            get {return _algorithm_model;}
            set {_algorithm_model=value; invokePropertyChanged("Algorithm_Model");}
        }
        public CryptionMode Cryption
        {
            get {return _cryption_mode;}
            set {_cryption_mode=value;}
        }

        public Visibility File_Output_Visibility
        {
            get {return _file_output_visiblity;}
            set {_file_output_visiblity=value; invokePropertyChanged("File_Output_Visibility");}
        }
        public Visibility File_Log_Visibility
        {
            get {return _file_log_visiblity;}
            set {_file_log_visiblity=value; invokePropertyChanged("File_Log_Visibility");}
        }
        public RelayCommand CommandOpenFile
        {
            get {return _command_open_file??=new RelayCommand(openFile_execute, openFile_canExecute);}
        }
        public RelayCommand CommandCrypt
        {
            get {return _command_crypt??=new RelayCommand(crypt_execute, crypt_canExecute);}
        }
        public RelayCommand CommandShowLog
        {
            get {return _command_open_file??=new RelayCommand(showLog_execute, showLog_canExecute);}
        }
        
        public ViewModelMain()
        {
            Algorithm_Model=new AlgorithmModel();
            File_Output_Visibility=Visibility.Hidden;
            File_Log_Visibility=Visibility.Hidden;
        }

        private void openFile_execute(object parameter)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog();

            if(open_file_dialog.ShowDialog()==false)
                return;
            
            FileInfo_Input=new FileInfo(open_file_dialog.FileName);
            using(FileStream file_stream=FileInfo_Input.OpenRead())
            {
                _fileinfo_input_content_bytes=new byte[FileInfo_Input.Length];
                
                for(int length=(int)FileInfo_Input.Length, number_bytes_read_sum=0, number_bytes_read; length>0; length-=number_bytes_read, number_bytes_read_sum+=number_bytes_read)
                {
                    number_bytes_read=file_stream.Read(_fileinfo_input_content_bytes, number_bytes_read_sum, length);
                    if(number_bytes_read==0)
                        break;
                }
            }
            FileInfo_Input_Content_String=Encoding.UTF8.GetString(_fileinfo_input_content_bytes);
        }
        private bool openFile_canExecute(object parameter)
        {
            return true;
        }
        private void crypt_execute(object parameter)
        {
            byte[][] bytes_output;
            string file_extension=".encrypted";
            
            if(_cryption_mode==CryptionMode.ENCRYPTION)
            {
                bytes_output=_algorithm_model.encrypt(_fileinfo_input_content_bytes);
                FileInfo_Output??=new FileInfo(FileInfo_Input.FullName+file_extension);
            }
            else
            {
                bytes_output=_algorithm_model.decrypt(_fileinfo_input_content_bytes);
                FileInfo_Output??=new FileInfo(FileInfo_Input.FullName+".decrypted");
                //FileInfo_Output??=new FileInfo(FileInfo_Input.FullName.Replace(file_extension, ""));
            }

            _fileinfo_output_content_bytes=bytes_output.SelectMany(a => a).ToArray();
            FileInfo_Output_Content_String=Encoding.ASCII.GetString(_fileinfo_output_content_bytes);
            using (FileStream file_stream=_fileinfo_output.Create())
            {
                file_stream.Write(_fileinfo_output_content_bytes, 0, _fileinfo_output_content_bytes.Length);
            }
            File_Output_Visibility=Visibility.Visible;
        }
        private bool crypt_canExecute(object parameter)
        {
            return _fileinfo_input_content_bytes!=null;
        }
        private void showLog_execute(object parameter)
        {

        }
        private bool showLog_canExecute(object parameter)
        {
            return true;
        }
    }
}