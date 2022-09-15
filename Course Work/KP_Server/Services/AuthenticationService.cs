using Grpc.Core;
using Proto;

namespace KP_Server.Services;

public class AuthenticationService : Authentication.AuthenticationBase          //TODO service aggregator (pass object(request) and enum)
{
    private readonly ILogger<AuthenticationService> _logger;

    public class User
    {
        public enum Status
        {
            DEFAULT,
            PENDING
        }
        
        public uint id;
        public uint id_connected_to;
        public string? name;
        public DateTime last_time_connected;
        public DirectoryInfo? chat_directory;
        public byte[][]? public_key=null;
        public byte[]? symmetric_key=null;
        public Status status;
        //public List<FileInfo> files;
    }
    
    private uint _id_counter=0;
    public static Dictionary<uint,User?> Users=new Dictionary<uint,User?>();
    private string _data_folder=Environment.CurrentDirectory+"\\data\\";
    private FileInfo _file_info_users;              // TODO write users and blocked ids to file
    
    public AuthenticationService()
    {
        _file_info_users=new FileInfo(_data_folder+"Users.txt");
        //Task.Run(() => invalidateUsers());
    }
    
    private async void invalidateUsers(int delay=500)
    {
        DateTime date_time;
        
        while(true)
        {
            date_time=DateTime.Now;
            foreach (var user in Users)
            {
                if(user.Value!=null)
                    if((date_time-user.Value.last_time_connected).Seconds>10)
                        Users.Remove(user.Key);
            }
            await Task.Delay(delay);
        }
    }
    public override Task<AuthenticationConnectReply> connect(AuthenticationConnectRequest request, ServerCallContext context)
    {
        Users.Add(_id_counter, new User                    //TODO add locker for id
                                {id=_id_counter,
                                id_connected_to=_id_counter,
                                name=request.UserName,
                                last_time_connected=DateTime.Now});

        AuthenticationConnectReply reply=new AuthenticationConnectReply
        { Result=new ResultReply
          { Result=true,
            Info=$"Connected as {request.UserName} #{_id_counter}" },
          Id=_id_counter };
        
        _id_counter++;
        return Task.FromResult(reply);
    }
    public override Task<AuthenticationMaintainConnectionReply> maintainConnection(AuthenticationMaintainConnectionRequest request, ServerCallContext context)
    {
        if(Users[request.Id]==null)
        {
            return Task.FromResult(new AuthenticationMaintainConnectionReply
                                   {Result=new ResultReply
                                    {Result=false,
                                     Info="ERROR: Connection blocked, user is not authorised."}});
        }
        if(!Users.ContainsKey(request.Id))
        {
            Users.Add(_id_counter++, null); 
            return Task.FromResult(new AuthenticationMaintainConnectionReply
                                   {Result=new ResultReply
                                    {Result=false,
                                     Info="ERROR: Blocking connection, user id not found."}});
        }
        Users[request.Id].last_time_connected=DateTime.Now;
        return Task.FromResult(new AuthenticationMaintainConnectionReply
                               {Result=new ResultReply
                                {Result=true,
                                 Info="Welcome "}});
    }
}

