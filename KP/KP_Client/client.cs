using Grpc.Net.Client;
using KP_Server;

namespace KP_Client;

public class Client
{
    public async void connect()
    {
        using (GrpcChannel channel=GrpcChannel.ForAddress("https://localhost:7197"))
        {
            Authentication.AuthenticationClient client=new Authentication.AuthenticationClient(channel);
            AuthenticationJoinReply reply;

            while(true)
            {
                reply=await client.connectAsync(new AuthenticationJoinRequest
                                                {UserName=Console.ReadLine()});
                Console.WriteLine($"Server answer: {reply.Result}, {reply.UserNames}");
            }
        }
    }
}