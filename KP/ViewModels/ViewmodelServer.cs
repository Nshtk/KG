using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Grpc.Core;
using Google.Protobuf;
using Soldatov.Wpf.MVVM.Core;
using Proto;

namespace KP.ViewModels
{
    public class ViewmodelServer : ViewModelBase
    {
        private Channel _channel=new Channel("localhost", 5001, ChannelCredentials.Insecure);
        private Authentication.AuthenticationClient _client_authentication;
        private FileStorage.FileStorageClient _client_file_storage;
        private FileStorageUploadRequest _request_file_storage_upload;
        private FileStorageUploadReply _reply_file_storage_upload;
        private FileStorageDownloadRequest _request_file_storage_download;
        private FileStorageDownloadReply _reply_file_storage_download;
        private FileStorageRemoveRequest _request_file_storage_remove;
        private FileStorageRemoveReply _reply_file_storage_remove;
        private FileStorageServerDataReply _reply_file_storage_data;
        private FileStorageServerDataRequest _request_file_storage_data;
        private FileStorageJoinUserRequest _request_file_storage_join_user;
        private FileStorageJoinUserReply _reply_file_storage_join_user;
        
        private SynchronizationContext ui_context = SynchronizationContext.Current;
        private string _instance_files_folder="..\\..\\file_storage\\";
        private string _instance_user_name="User";
        private uint _instance_id;
        private ObservableCollection<string> _users;
        private ObservableCollection<string> _users_selected;
        private ObservableCollection<FileInfo> _files;
        private ObservableCollection<FileInfo> _files_files_selected;
        private string _grpc_info;

        private bool _button_connect_status=true;
        private Visibility _files_visibility;
        
        private RelayCommandAsync _command_connect_to_server;
        private RelayCommandAsync _command_join_user;
        private RelayCommandAsync _command_upload_file_to_server;
        private RelayCommandAsync _command_download_file_from_server;
        private RelayCommandAsync _command_remove_file_from_server;

        public string GRPC_Info
        {
            get {return _grpc_info;}
            set {_grpc_info=value; invokePropertyChanged("GRPC_Info");}
        }

        public string Instance_User_Name
        {
            get {return _instance_user_name;}
            set
            {
                if(String.IsNullOrWhiteSpace(value))
                {
                    GRPC_Info="ERROR: Cannot set this user name!";
                    return;
                }
                else
                    GRPC_Info="";
                   
                _instance_user_name=value;
                invokePropertyChanged("Instance_User_Name");
            }
        }
        public ObservableCollection<string> Users
        {
            get {return _users;}
            set {_users=value; invokePropertyChanged("Users");}
        }
        public ObservableCollection<FileInfo> Files
        {
            get {return _files;}
            set {_files=value; invokePropertyChanged("Files");}
        }
        public ObservableCollection<string> ListBox_Users_Selected_Items
        {
            get {return _users_selected;}
            set {_users_selected=value; invokePropertyChanged("ListBox_Users_Selected_Items");}
        }
        public ObservableCollection<FileInfo> ListBox_Files_Selected_Items
        {
            get {return _files_files_selected;}
            set {_files_files_selected=value; invokePropertyChanged("ListBox_Files_Selected_Items");}
        }

        public bool Button_Connect_Status
        {
            get {return _button_connect_status;}
            set {_button_connect_status=value; invokePropertyChanged("Button_Connect_Status");}
        }
        public Visibility Files_Visibility
        {
            get {return _files_visibility;}
            set {_files_visibility=value; invokePropertyChanged("Files_Visibility");}
        }
        
        public RelayCommandAsync CommandConnectToServer
        {
            get {return _command_connect_to_server??=new RelayCommandAsync(connectToServer_execute, connectToServer_canExecute, (ex) => {return;});}
        }
        public RelayCommandAsync CommandJoinUser
        {
            get {return _command_join_user??=new RelayCommandAsync(joinUser_execute, joinUser_canExecute, (ex) => {return;});}
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
            ListBox_Files_Selected_Items=new ObservableCollection<FileInfo>();
            ListBox_Users_Selected_Items=new ObservableCollection<string>();
            _client_authentication=new Authentication.AuthenticationClient(_channel);
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
            _request_file_storage_data=new FileStorageServerDataRequest();
            _reply_file_storage_data=new FileStorageServerDataReply();
            _request_file_storage_join_user=new FileStorageJoinUserRequest();
            _reply_file_storage_join_user=new FileStorageJoinUserReply();

            _users=new ObservableCollection<string>();
            _files=new ObservableCollection<FileInfo>();
        }
        private async Task updateDataFromServer(uint amount_times, int delay=500)       //TODO maintain coonnection
        {
            Users=new ObservableCollection<string>();
            Files=new ObservableCollection<FileInfo>();
            if(amount_times!=0)
            {
                for(int i=0; i<amount_times; i++)
                {
                    _reply_file_storage_data=await _client_file_storage.getServerDataAsync(_request_file_storage_data);
                    await Task.Delay(delay);
                }
            }
            else
            {
                while(true)
                {
                    _reply_file_storage_data=await _client_file_storage.getServerDataAsync(_request_file_storage_data);
                    if(_reply_file_storage_data.Result.Result==false)
                        return;
                    foreach (var file_name in _reply_file_storage_data.FileNames)
                    {
                        if(Files.FirstOrDefault(i=> i.Name==file_name)==null)
                            ui_context.Send(x => Files.Add(new FileInfo(file_name)), null);
                    }
                    foreach (var user in _reply_file_storage_data.Users)
                    {
                        string name=user.Name+"#"+user.Id;
                        if(Users.FirstOrDefault(i=> i==name)==null)
                            ui_context.Send(x => Users.Add(name), null);
                    }
                    Users.Remove($"{_instance_user_name}#{_instance_id}");
                    
                    await Task.Delay(delay);
                }
            }
            
        }
        private async Task connectToServer_execute()
        {
            AuthenticationConnectReply reply;

            reply=await _client_authentication.connectAsync(new AuthenticationConnectRequest
                                            {UserName=Instance_User_Name});
            //System.Diagnostics.Trace.WriteLine($"Server answer: {reply.Result.Info}");      // TODO check
            _instance_id=reply.Id;
            GRPC_Info=reply.Result.Info;

            initializeGRPC();
            await Task.Run(() => updateDataFromServer(0));
            Button_Connect_Status=false;
        }
        private bool connectToServer_canExecute(object parameter)
        {
            return true;
        }
        private async Task joinUser_execute()
        {
            _request_file_storage_join_user.IdFrom=_instance_id;
            _request_file_storage_join_user.IdTo=Convert.ToUInt32(ListBox_Users_Selected_Items[0].Substring(ListBox_Users_Selected_Items[0].IndexOf("#")));
            _reply_file_storage_join_user=await _client_file_storage.joinUserAsync(_request_file_storage_join_user);
            GRPC_Info=_reply_file_storage_join_user.Result.Info;
        }
        private bool joinUser_canExecute(object parameter)
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
            await Task.Run(() =>                    // hehheeheh
            {
                Parallel.ForEach(ListBox_Files_Selected_Items, file =>
                {
                    FileStorageDownloadRequest request=new FileStorageDownloadRequest();
                    FileStorageDownloadReply reply;
                    
                    request.FileName=file.Name;
                    reply=_client_file_storage.download(request);
                    GRPC_Info=reply.Result.Info;
                    if(reply.Result.Result==false)
                        return;
                    
                    using (FileStream file_stream=new FileInfo(_instance_files_folder+request.FileName).OpenWrite())
                    {
                        file_stream.Write(reply.Content.ToByteArray(), 0, reply.Content.Length);
                    }
                });
            });
            
            /*foreach (var file in ListBox_Files_Selected_Items)
            {
                _request_file_storage_download.FileName=file.Name;
                _reply_file_storage_download=await _client_file_storage.downloadAsync(_request_file_storage_download);
            }*/
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
        public void ListBox_files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ListBox_Files_Selected_Items.Remove((FileInfo)item);
            }
            foreach (var item in e.AddedItems)
            {
                ListBox_Files_Selected_Items.Add((FileInfo)item);
            }
        }
        public void ListBox_users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                ListBox_Users_Selected_Items.Remove((string)item);
            }
            foreach (var item in e.AddedItems)
            {
                ListBox_Users_Selected_Items.Add((string)item);
            }
        }
    }
}