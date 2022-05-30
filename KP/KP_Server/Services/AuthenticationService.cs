using Grpc.Core;
using Proto;

namespace KP_Server.Services;

public class AuthenticationService : Authentication.AuthenticationBase
{
    private readonly ILogger<AuthenticationService> _logger;
    
    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        _logger=logger;
    }

    public override Task<AuthenticationJoinReply> connect(AuthenticationJoinRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AuthenticationJoinReply
                               {Result=new ResultReply
                                {Result=true,
                                 Info="Welcome "+request.UserName}});
    }
}

