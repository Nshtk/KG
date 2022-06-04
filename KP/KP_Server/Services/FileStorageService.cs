using Google.Protobuf;
using Grpc.Core;
using Proto;

namespace KP_Server.Services;

public class FileStorageService : FileStorage.FileStorageBase           // TODO rename to server storage
{
    private List<FileInfo> files=new List<FileInfo>();
    private string files_folder=Environment.CurrentDirectory+"\\file_storage\\";
    private string user_chat_folder="user_chat\\";

    //private FileStorageServerDataReply _file_storage_server_data_reply=new FileStorageServerDataReply();

    public FileStorageService()
    {
        foreach (FileInfo file in new DirectoryInfo(files_folder).GetFiles())
            files.Add(file);
    }

    private FileStorageServerDataReply getServerData(uint id)
    {
        AuthenticationService.Users[id].last_time_connected=DateTime.Now;
        FileStorageServerDataReply reply=new FileStorageServerDataReply();

        if(AuthenticationService.Users[id].status==AuthenticationService.User.Status.PENDING)
        {
            if(AuthenticationService.Users[id].public_key==null)
            {
                AuthenticationService.Users[id].public_key=AuthenticationService.Users[AuthenticationService.Users[id].id_connected_to].public_key;
                reply.IdConnectedTo=AuthenticationService.Users[id].id_connected_to;
                for(int i=0; i<3; i++)
                    reply.PublicKey.Add(ByteString.CopyFrom(AuthenticationService.Users[id].public_key[i]));
                AuthenticationService.Users[id].status=AuthenticationService.User.Status.DEFAULT;
            }
            else if(AuthenticationService.Users[id].symmetric_key==null)
            {
                AuthenticationService.Users[id].symmetric_key=AuthenticationService.Users[AuthenticationService.Users[id].id_connected_to].symmetric_key;
                reply.SymmetricKey=ByteString.CopyFrom(AuthenticationService.Users[id].symmetric_key);
                AuthenticationService.Users[id].status=AuthenticationService.User.Status.DEFAULT;
            }
        }

        foreach (var user in AuthenticationService.Users)
        {
            if(user.Value!=null && !String.IsNullOrWhiteSpace(user.Value.name) && user.Key!=id)
            {
                reply.Users.Add(new FileStorageServerUser() 
                                {Name=user.Value.name,
                                 Id=user.Key});
            }
        }

        if(AuthenticationService.Users[id].id_connected_to!=id && AuthenticationService.Users[id].symmetric_key!=null && AuthenticationService.Users[id].chat_directory!=null)
        {
            foreach (FileInfo file in AuthenticationService.Users[id].chat_directory.GetFiles())
                reply.FileNames.Add(file.Name);
        }
        else
        {
            foreach (FileInfo file in new DirectoryInfo(files_folder).GetFiles())
                reply.FileNames.Add(file.Name);
            /*for(int i=0; i<files.Count; i++)
                reply.FileNames.Add(files[i].Name);*/
        }

        reply.Result=new ResultReply();
        return reply;
    }
    
    public override Task<FileStorageUploadReply> upload(FileStorageUploadRequest request, ServerCallContext context)
    {
        if(request.FileName==null || request.Content==null)
            return Task.FromResult(new FileStorageUploadReply
                                   {Result=new ResultReply
                                            {Result=false,
                                            Info="ERROR: Filename of file content is empty."}});

        string file_full_name;
        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id && AuthenticationService.Users[request.Id].symmetric_key!=null)
            file_full_name=files_folder+user_chat_folder+AuthenticationService.Users[request.Id].chat_directory.Name+"\\"+request.FileName;
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
        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id && AuthenticationService.Users[request.Id].symmetric_key!=null)
            directory_name=AuthenticationService.Users[request.Id].chat_directory.Name;
        else
            directory_name=files_folder.Remove(0, 1);
        
        if(!files.Any(f => f.Name==request.FileName && f.Directory.Name==directory_name))
            return Task.FromResult(new FileStorageDownloadReply
                                   {Result=new ResultReply
                                            {Result=false,
                                            Info="ERROR: Requested file doesn't exist."}});

        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id && AuthenticationService.Users[request.Id].symmetric_key!=null)
            directory_name=files_folder+user_chat_folder+directory_name+"\\";

        return Task.FromResult(new FileStorageDownloadReply
                               {Result=new ResultReply
                                {Result=true,
                                 Info=$"Downloaded file {request.FileName}."},
                                Content=ByteString.CopyFrom(File.ReadAllBytes(directory_name+request.FileName))});
    }
    public override Task<FileStorageRemoveReply> remove(FileStorageRemoveRequest request, ServerCallContext context)
    {
        string directory_name;
        if(AuthenticationService.Users[request.Id].id_connected_to!=request.Id && AuthenticationService.Users[request.Id].public_key!=null)
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
        FileStorageServerDataReply reply=getServerData(request.Id);

        if(!request.Key.IsEmpty && AuthenticationService.Users[request.Id].symmetric_key==null && AuthenticationService.Users[request.Id].id_connected_to!=request.Id)
        {
            AuthenticationService.Users[request.Id].symmetric_key=request.Key.ToByteArray();
            AuthenticationService.Users[AuthenticationService.Users[request.Id].id_connected_to].status=AuthenticationService.User.Status.PENDING;
        }

        reply.Result.Result=true;
        reply.Result.Info="Data successfully gathered.";
        
        return Task.FromResult(reply);
    }
    public override Task<FileStorageJoinUserReply> joinUser(FileStorageJoinUserRequest request, ServerCallContext context)
    {
        if((AuthenticationService.Users[request.IdFrom].id_connected_to==request.IdTo) || (AuthenticationService.Users[request.IdFrom].symmetric_key!=null && AuthenticationService.Users[request.IdTo].symmetric_key==null))
        {
            AuthenticationService.Users[request.IdFrom].chat_directory=null;
            AuthenticationService.Users[request.IdFrom].public_key=null;
            AuthenticationService.Users[request.IdFrom].symmetric_key=null;
            AuthenticationService.Users[AuthenticationService.Users[request.IdFrom].id_connected_to].id_connected_to=AuthenticationService.Users[request.IdFrom].id_connected_to;
            AuthenticationService.Users[AuthenticationService.Users[request.IdFrom].id_connected_to].public_key=null;
            AuthenticationService.Users[AuthenticationService.Users[request.IdFrom].id_connected_to].symmetric_key=null;
            AuthenticationService.Users[AuthenticationService.Users[request.IdFrom].id_connected_to].chat_directory=null;
            AuthenticationService.Users[request.IdFrom].id_connected_to=request.IdFrom;

            return Task.FromResult(new FileStorageJoinUserReply()
                                   {Result=new ResultReply()
                                    {Result=false,
                                     Info="Previous connection aborted, try connecting again." }
                                   });
        }
        if(AuthenticationService.Users[request.IdTo].symmetric_key!=null)
        {
            return Task.FromResult(new FileStorageJoinUserReply()
                                   {Result=new ResultReply()
                                    {Result=false,
                                     Info="ERROR: This user is already connected to someone;" }
                                   });
        }

        AuthenticationService.Users[request.IdFrom].id_connected_to=request.IdTo;
        AuthenticationService.Users[request.IdTo].id_connected_to=request.IdFrom;
        AuthenticationService.Users[request.IdFrom].public_key=new byte[3][];
        for(int i=0; i<3; i++)
            AuthenticationService.Users[request.IdFrom].public_key[i]=request.PublicKey[i].ToByteArray();
        AuthenticationService.Users[request.IdTo].status=AuthenticationService.User.Status.PENDING;
        int d=0;
        /*var timer=DateTime.Now;
        while(true)
        {
            if(AuthenticationService.Users[request.IdTo].public_key!=null || (DateTime.Now-timer).Seconds>15)
            {
                d=(DateTime.Now-timer).Seconds;
                break;
            }
        }*/
        for( ; d<15; d++)
        {
            if(AuthenticationService.Users[request.IdTo].public_key!=null)
                break;
            
            Task.Delay(120).Wait();
        }

        if(d>=15)
        {
            AuthenticationService.Users[request.IdFrom].public_key=null;
            AuthenticationService.Users[request.IdFrom].id_connected_to=request.IdFrom;
            AuthenticationService.Users[request.IdTo].id_connected_to=request.IdTo;
            return Task.FromResult(new FileStorageJoinUserReply()
                                   {Result=new ResultReply()
                                    {Result=false,
                                     Info="ERROR: Connection not estabilished;" }
                                   });
        }
        AuthenticationService.Users[request.IdFrom].chat_directory=AuthenticationService.Users[request.IdTo].chat_directory=new DirectoryInfo($"{files_folder}user_chat\\{request.IdFrom}_{request.IdTo}_{DateTime.Now.ToString().Replace(":", "")}");
        AuthenticationService.Users[request.IdFrom].chat_directory.Create();

        return Task.FromResult(new FileStorageJoinUserReply()
                               {Result=new ResultReply()
                                {Result=true,
                                 Info="Connection to other user in progress..." }
                               });
    }
}


