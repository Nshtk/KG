using Google.Protobuf;
using Grpc.Core;
using Proto;

namespace KP_Server.Services;

public class FileStorageService : FileStorage.FileStorageBase
{
    private List<FileInfo> files=new List<FileInfo>();
    private string files_folder=Environment.CurrentDirectory+"\\Storage\\";

    public FileStorageService()
    {
        foreach (FileInfo file in new DirectoryInfo(files_folder).GetFiles())
            files.Add(file);
    }
    
    private string[] getFileNames()
    {
        string[] file_names=new string[files.Count];

        for(int i=0; i<files.Count; i++)
            file_names[i]=files[i].Name;

        return file_names;
    }
    
    public override Task<FileStorageUploadReply> upload(FileStorageUploadRequest file_storage_request, ServerCallContext context)
    {
        if(file_storage_request.FileName==null || file_storage_request.Content==null)
            return Task.FromResult(new FileStorageUploadReply
                                   {Result=new ResultReply
                                            {Result=false,
                                            Info="File upload ERROR: filename of file content is empty."}});

        FileStorageFilesStored files_stored=new FileStorageFilesStored();
        string file_full_name=files_folder+file_storage_request.FileName;

        if(File.Exists(file_full_name))
            file_full_name+=$"{DateTime.Now.Ticks}";
        
        files.Add(new FileInfo(file_full_name));
        using (FileStream file_stream=files[files.Count-1].OpenWrite())
        {
            file_stream.Write(file_storage_request.Content.ToByteArray());
        }
        
        files_stored.FileNames.Add(getFileNames());
        
        return Task.FromResult(new FileStorageUploadReply
                               {Result=new ResultReply
                                        {Result=true,
                                        Info=$"File {file_storage_request.FileName} was successfully uploaded."},
                                Files=files_stored});
    }
    public override Task<FileStorageDownloadReply> download(FileStorageDownloadRequest file_storage_request, ServerCallContext context)
    {
        /*if(files.Find(f => f.Name==file_storage_request.FileName) is FileInfo file_info && file_info!=default(FileInfo))
            return Task.FromResult(new FileStorageDownloadReply
                                   {Result=new ResultReply
                                    {Result=false,
                                     Info="File download ERROR: Requested file doesnt exist."}});*/
        if(!files.Any(f => f.Name==file_storage_request.FileName))
            return Task.FromResult(new FileStorageDownloadReply
                                   {Result=new ResultReply
                                            {Result=false,
                                            Info="File download ERROR: Requested file doesn't exist."}});
        
        return Task.FromResult(new FileStorageDownloadReply
                               {Result=new ResultReply
                                        {Result=true,
                                        Info=$"Downloading file {file_storage_request.FileName}."},
                                Content=ByteString.CopyFrom(File.ReadAllBytes(files_folder+file_storage_request.FileName))});
    }
    public override Task<FileStorageRemoveReply> remove(FileStorageRemoveRequest file_storage_request, ServerCallContext context)
    {
        for(int i=0; i<files.Count; i++)
        {
            if(files[i].Name==file_storage_request.FileName)
            {
                files.RemoveAt(i);
                return Task.FromResult(new FileStorageRemoveReply
                                       {Result=new ResultReply
                                                {Result=true,
                                                Info=$"File {file_storage_request.FileName} removed from server."}});
            }
        }
        
        return Task.FromResult(new FileStorageRemoveReply
                               {Result=new ResultReply
                                        {Result=false,
                                        Info=$"ERROR: File {file_storage_request.FileName} was not found on server."}});
    }
}


