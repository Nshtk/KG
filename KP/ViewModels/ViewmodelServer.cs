using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf;
using Soldatov.Wpf.MVVM.Core;
using Proto;

namespace KP.ViewModels
{
    public class ViewmodelServer : ViewModelBase
    {
        private Channel _channel=new Channel("localhost", 5001, ChannelCredentials.Insecure);
        private FileStorage.FileStorageClient _client_file_storage;
        private FileStorageUploadRequest _request_file_storage_upload;
        private FileStorageUploadReply _reply_file_storage_upload;
        private FileStorageDownloadRequest _request_file_storage_download;
        private FileStorageDownloadReply _reply_file_storage_download;
        private FileStorageRemoveRequest _request_file_storage_remove;
        private FileStorageRemoveReply _reply_file_storage_remove;

        private ObservableCollection<FileInfo> files;
        private string _grpc_info;

        private RelayCommandAsync _command_connect_to_server;
        private RelayCommandAsync _command_upload_file_to_server;
        private RelayCommandAsync _command_download_file_from_server;
        private RelayCommandAsync _command_remove_file_from_server;

        public string GRPC_Info
        {
            get {return _grpc_info;}
            set {_grpc_info=value; invokePropertyChanged("GRPC_Info");}
        }

        public RelayCommandAsync CommandConnectToServer
        {
            get {return _command_connect_to_server??=new RelayCommandAsync(connectToServer_execute, connectToServer_canExecute, (ex) => {return;});}
        }
        public RelayCommandAsync CommandUploadFileToServer
        {
            get {return _command_upload_file_to_server??=new RelayCommandAsync(uploadFileToServer_execute, uploadFileToServer_canExecute, (ex) => {return;});}
        }
        public RelayCommandAsync CommandDownloadFileFromServer
        {
            get {return _command_download_file_from_server??=new RelayCommandAsync(downloadFileFromServer_execute, downloadFileFromServer_canExecute, (ex) => {return;});}
        }
        public RelayCommandAsync CommandRemoveFileFromServer
        {
            get {return _command_remove_file_from_server??=new RelayCommandAsync(removeFileFromServer_execute, removeFileFromServer_canExecute, (ex) => {return;});}
        }
        
        public ViewmodelServer()
        {
            
        }

        private void initializeGRPC()
        {
            _client_file_storage=new FileStorage.FileStorageClient(_channel);
            _request_file_storage_upload=new FileStorageUploadRequest();
            _reply_file_storage_upload=new FileStorageUploadReply();
            _request_file_storage_download=new FileStorageDownloadRequest();
            _reply_file_storage_download=new FileStorageDownloadReply();
            _request_file_storage_remove=new FileStorageRemoveRequest();
            _reply_file_storage_remove=new FileStorageRemoveReply();
        }
        
        private async Task connectToServer_execute()
        {
            Authentication.AuthenticationClient client=new Authentication.AuthenticationClient(_channel);
            AuthenticationJoinReply reply;

            reply=await client.connectAsync(new AuthenticationJoinRequest
                                            {UserName="Nishtyak"});
            System.Diagnostics.Trace.WriteLine($"Server answer: {reply.Result.Info}, {reply.UserNames}"); // TODO check
            GRPC_Info=reply.Result.Info;

            initializeGRPC();
        }
        private bool connectToServer_canExecute(object parameter)
        {
            return true;
        }
        private async Task uploadFileToServer_execute()
        {
            ViewmodelClient viewmodel_client=(ViewmodelClient)ViewModelMain.View_Model_Main.viewmodels[0];
            
            if(viewmodel_client.FileInfo_Input is not FileInfo file_info)   // TODO exception
                return;
            _request_file_storage_upload.FileName=file_info.Name;
            _request_file_storage_upload.Content=ByteString.CopyFrom(viewmodel_client.fileinfo_input_content_bytes);

            _reply_file_storage_upload=await _client_file_storage.uploadAsync(_request_file_storage_upload);
            GRPC_Info=_reply_file_storage_upload.Result.Info;
        }
        private bool uploadFileToServer_canExecute(object parameter)
        {
            return true;
        }
        private async Task downloadFileFromServer_execute()
        {

        }
        private bool downloadFileFromServer_canExecute(object parameter)
        {
            return true;
        }
        private async Task removeFileFromServer_execute()
        {

        }
        private bool removeFileFromServer_canExecute(object parameter)
        {
            return true;
        }
    }
}