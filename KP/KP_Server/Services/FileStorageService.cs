using Google.Protobuf;
using Grpc.Core;
using Proto;

namespace KP_Server.Services;

public class FileStorageService : FileStorage.FileStorageBase           // TODO rename to server storage
{
    private List<FileInfo> files=new List<FileInfo>();
    private string files_folder=Environment.CurrentDirectory+"\\file_storage\\";

    //private FileStorageServerDataReply _file_storage_server_data_reply=new FileStorageServerDataReply();

    public FileStorageService()
    {
        foreach (FileInfo file in new DirectoryInfo(files_folder).GetFiles())
            files.Add(file);
    }
    
    /*private string[] getUserNames()
    {
        string[] user_names=new string[AuthenticationService.Users.Count];

        uint i=0;
        foreach (var user in AuthenticationService.Users)
        {
            if(user.Value!=null && !String.IsNullOrWhiteSpace(user.Value.name))
                user_names[i]=user.Value.name;
            i++;
        }

        return user_names;
    }
    private string[] getFileNames()
    {
        string[] file_names=new string[files.Count];

        for(int i=0; i<files.Count; i++)
            file_names[i]=files[i].Name;

        return file_names;
    }*/
    private FileStorageServerDataReply getServerData()
    {
        FileStorageServerDataReply file_storage_server_data_reply=new FileStorageServerDataReply();
        
        int i=0;
        foreach (var user in AuthenticationService.Users)
        {
            if(user.Value!=null && !String.IsNullOrWhiteSpace(user.Value.name))
            {
                file_storage_server_data_reply.Users.Add(new FileStorageServerUser() 
                                                                {Name=user.Value.name,
                                                                Id=user.Key});
            }
                
            i++;
        }
        
        for(i=0; i<files.Count; i++)
            file_storage_server_data_reply.FileNames.Add(files[i].Name);

        file_storage_server_data_reply.Result=new ResultReply();

        return file_storage_server_data_reply;
    }
    
    public override Task<FileStorageUploadReply> upload(FileStorageUploadRequest request, ServerCallContext context)
    {
        if(request.FileName==null || request.Content==null)
            return Task.FromResult(new FileStorageUploadReply
                                   {Result=new ResultReply
                                            {Result=false,
                                            Info="ERROR: Filename of file content is empty."}});

        string file_full_name;
        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id)
            file_full_name=files_folder+AuthenticationService.Users[request.Id].chat_directory.Name+request.FileName;
        else
            file_full_name=files_folder+request.FileName;

        if(File.Exists(file_full_name))
            file_full_name+=$"{DateTime.Now.Ticks}";
        
        files.Add(new FileInfo(file_full_name));
        using (FileStream file_stream=files[files.Count-1].OpenWrite())
        {
            file_stream.Write(request.Content.ToByteArray());
        }

        return Task.FromResult(new FileStorageUploadReply
                               {Result=new ResultReply
                                {Result=true,
                                 Info=$"File {request.FileName} was successfully uploaded."}});
    }
    public override Task<FileStorageDownloadReply> download(FileStorageDownloadRequest request, ServerCallContext context)
    {
        string directory_name;
        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id)
            directory_name=AuthenticationService.Users[request.Id].chat_directory.Name;
        else
            directory_name=files_folder.Remove(0, 1);
        
        if(!files.Any(f => f.Name==request.FileName && f.Directory.Name==directory_name))
            return Task.FromResult(new FileStorageDownloadReply
                                   {Result=new ResultReply
                                            {Result=false,
                                            Info="ERROR: Requested file doesn't exist."}});

        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id)
            directory_name=files_folder+directory_name;

        return Task.FromResult(new FileStorageDownloadReply
                               {Result=new ResultReply
                                {Result=true,
                                 Info=$"Downloading file {request.FileName}."},
                                Content=ByteString.CopyFrom(File.ReadAllBytes(directory_name+request.FileName))});
    }
    public override Task<FileStorageRemoveReply> remove(FileStorageRemoveRequest request, ServerCallContext context)
    {
        string directory_name;
        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id)
            directory_name=AuthenticationService.Users[request.Id].chat_directory.Name;
        else
            directory_name=files_folder.Remove(0, 1);
        for(int i=0; i<files.Count; i++)
        {
            if(files[i].Name==request.FileName && files[i].Directory.Name==directory_name)
            {
                files.RemoveAt(i);
                return Task.FromResult(new FileStorageRemoveReply
                                       {Result=new ResultReply
                                                {Result=true,
                                                Info=$"File {request.FileName} removed from server."}});
            }
        }
        
        return Task.FromResult(new FileStorageRemoveReply
                               {Result=new ResultReply
                                        {Result=false,
                                        Info=$"ERROR: File {request.FileName} was not found on server."}});
    }

    public override Task<FileStorageServerDataReply> getServerData(FileStorageServerDataRequest request, ServerCallContext context)
    {
        FileStorageServerDataReply file_storage_server_data_reply=getServerData();

        file_storage_server_data_reply.Result.Result=true;
        file_storage_server_data_reply.Result.Info="Data successfully gathered.";
        
        return Task.FromResult(file_storage_server_data_reply);
    }
    public override Task<FileStorageJoinUserReply> joinUser(FileStorageJoinUserRequest request, ServerCallContext context)
    {
        AuthenticationService.Users[request.IdFrom].id_connected_to=request.IdTo;           // TODO await pending connection to other user
        AuthenticationService.Users[request.IdTo].id_connected_to=request.IdFrom;
        AuthenticationService.Users[request.IdFrom].chat_directory=AuthenticationService.Users[request.IdTo].chat_directory=new DirectoryInfo($"{request.IdFrom}_{request.IdTo}_{DateTime.Now}");
        AuthenticationService.Users[request.IdFrom].chat_directory.Create();
        
        return Task.FromResult(new FileStorageJoinUserReply()
                               {Result=new ResultReply()
                                        {Result=true,
                                        Info="Sucessfully joined other user." }
                                });
    }
}


