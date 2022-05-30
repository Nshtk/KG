using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KP.Models;
using Microsoft.Win32;
using Soldatov.Wpf.MVVM.Core;

namespace KP.ViewModels
{
    public class ViewmodelClient : ViewModelBase
    {
        #region Types
        public enum CryptionMode
        {
            ENCRYPTION,
            DECRYPTION
        }
        #endregion

        #region Fields
        private FileInfo _fileinfo_input, _fileinfo_output;
        private AlgorithmModel _algorithm_model;
        private CryptionMode _cryption_mode;
        public byte[] fileinfo_input_content_bytes;
        private byte[] _fileinfo_output_content_bytes;
        private string _fileinfo_input_content_string, _fileinfo_output_content_string;
        CancellationTokenSource _token_source;
        #endregion

        #region Fields_WPF
        private Visibility _file_output_visiblity;
        private Visibility _file_log_visiblity;
        
        private RelayCommandAsync _command_open_file;
        private RelayCommandAsync _command_crypt_start;
        private RelayCommand _command_crypt_stop;
        private RelayCommand _command_show_log;
        #endregion

        #region Properties
        public FileInfo FileInfo_Input
        {
            get {return _fileinfo_input;}
            set {_fileinfo_input=value; FileInfo_Output=null; invokePropertiesChanged("FileInfo_Input", "FileInfo_Input_Name");}
        }
        public string FileInfo_Input_Name
        {
            get {return FileInfo_Input==null ?"File: " :"File: "+FileInfo_Input.Name;}
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
        #endregion

        #region Properties_WPF
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
        
        public RelayCommandAsync CommandOpenFile
        {
            get {return _command_open_file??=new RelayCommandAsync(openFile_execute, null, (ex) => {return;});}
        }
        public RelayCommandAsync CommandCryptStart
        {
            get {return _command_crypt_start??=new RelayCommandAsync(cryptStart_execute, cryptStart_canExecute, (ex) => {return;});}
        }
        public RelayCommand CommandCryptStop
        {
            get {return _command_crypt_stop??=new RelayCommand(cryptStop_execute, cryptStop_canExecute);}
        }
        public RelayCommand CommandShowLog
        {
            get {return _command_show_log??=new RelayCommand(showLog_execute, cryptStart_canExecute);}
        }
        #endregion
        
        public ViewmodelClient()
        {
            Algorithm_Model=new AlgorithmModel();
            _token_source=new CancellationTokenSource();
            File_Output_Visibility=Visibility.Hidden;
            File_Log_Visibility=Visibility.Hidden;
        }

        #region Methods
        
        private async Task<byte[]> fileRead_async()
        {
            byte[] bytes;
            using(FileStream file_stream=FileInfo_Input.OpenRead())
            {
                bytes=new byte[FileInfo_Input.Length];
                
                for(int length=(int)FileInfo_Input.Length, number_bytes_read_sum=0, number_bytes_read; length>0; length-=number_bytes_read, number_bytes_read_sum+=number_bytes_read)
                {
                    number_bytes_read=await file_stream.ReadAsync(bytes, number_bytes_read_sum, length);
                    if(number_bytes_read==0)
                        break;
                }
            }
            return bytes;
            //FileInfo_Input_Content_String=Encoding.UTF8.GetString(fileinfo_input_content_bytes);
        }

        private async Task<string> getSS(string str, int i, int chunkSize)
        {
            return str.Substring(i, chunkSize);
        }
        private async void writeTextBoxAsync(string str)                    //TODO FlowDocumentReader instead of textbox
        {
            int chunkSize = 100;
            int stringLength = str.Length;
            _fileinfo_input_content_string=null;
            for (int i = 0; i < stringLength ; i += chunkSize)
            {
                if (i + chunkSize > stringLength) chunkSize = stringLength  - i;
                FileInfo_Input_Content_String+=await getSS(str, i, chunkSize);
                await Task.Delay(10);
            }
        }
        private async void writeTextBoxAsync2(string str)
        {
            int chunkSize = 100;
            int stringLength = str.Length;
            _fileinfo_output_content_string=null;
            for (int i = 0; i < stringLength ; i += chunkSize)
            {
                if (i + chunkSize > stringLength) chunkSize = stringLength  - i;
                FileInfo_Output_Content_String+=await getSS(str, i, chunkSize);
                await Task.Delay(10);
            }
        }
        
        #endregion

        #region Command_methods
        private async Task openFile_execute()
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog();

            if(open_file_dialog.ShowDialog()==false)
                return;
            
            FileInfo_Input=new FileInfo(open_file_dialog.FileName);
            fileinfo_input_content_bytes=await fileRead_async();
            //FileInfo_Input_Content_String=Encoding.UTF8.GetString(fileinfo_input_content_bytes);
            await Task.Run(()=>writeTextBoxAsync(Encoding.UTF8.GetString(fileinfo_input_content_bytes)));

            CommandManager.InvalidateRequerySuggested();
        }
        private bool openFile_canExecute(Exception e)
        {
            return true;
        }
        
        private async Task cryptStart_execute()
        {
            byte[][] bytes_output;
            string file_extension=".encrypted";

            if(_cryption_mode==CryptionMode.ENCRYPTION)
            {
                /*Task<byte[][]> tsk=Task.Run(()=>_algorithm_model.encrypt(fileinfo_input_content_bytes, _token_source.Token));
                bytes_output=tsk.Result;*/
                bytes_output=await Task.Run(()=>_algorithm_model.encrypt(fileinfo_input_content_bytes, _token_source.Token));
                FileInfo_Output??=new FileInfo(FileInfo_Input.FullName+file_extension);
            }
            else
            {
                bytes_output=await Task.Run(()=>_algorithm_model.decrypt(fileinfo_input_content_bytes, _token_source.Token));
                FileInfo_Output??=new FileInfo(FileInfo_Input.FullName+".decrypted");
                //FileInfo_Output??=new FileInfo(FileInfo_Input.FullName.Replace(file_extension, ""));
            }

            _fileinfo_output_content_bytes=bytes_output.SelectMany(a => a).ToArray();
            await Task.Run(()=>writeTextBoxAsync2(Encoding.ASCII.GetString(_fileinfo_output_content_bytes)));
            using (FileStream file_stream=_fileinfo_output.Create())
            {
                file_stream.Write(_fileinfo_output_content_bytes, 0, _fileinfo_output_content_bytes.Length);
            }
            File_Output_Visibility=Visibility.Visible;
        }
        private bool cryptStart_canExecute(object parameter)
        {
            return fileinfo_input_content_bytes!=null && !CommandOpenFile.IsExecuting;
        }
        
        private void cryptStop_execute(object parameter)
        {
            _token_source.Cancel();
        }
        private bool cryptStop_canExecute(object parameter)
        {
            return _command_crypt_start.IsExecuting;
        }
        
        private void showLog_execute(object parameter)
        {
            
        }
        /*private bool showLog_canExecute(object parameter)
        {
            return true;
        }*/
        #endregion
    }
}