using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace AuthSample;

public interface IAuthorizeClassService : IService<IAuthorizeClassService>
{
    UnaryResult<string> GetUserNameAsync();
    UnaryResult<int> AddAsync(int a, int b);
}

[Authorize]
public class AuthorizeClassService : ServiceBase<IAuthorizeClassService>, IAuthorizeClassService
{
    public UnaryResult<string> GetUserNameAsync() => UnaryResult(Context.CallContext.GetHttpContext().User.Identity?.Name ?? throw new InvalidOperationException("Unauthenticated"));

    [AllowAnonymous]
    public UnaryResult<int> AddAsync(int a, int b) => UnaryResult(a + b);
}

public interface IAuthorizeMethodService : IService<IAuthorizeMethodService>
{
    UnaryResult<string> GetUserNameAsync();
    UnaryResult<int> AddAsync(int a, int b);
}

public class AuthorizeMethodService : ServiceBase<IAuthorizeMethodService>, IAuthorizeMethodService
{
    [Authorize]
    public UnaryResult<string> GetUserNameAsync() => UnaryResult(Context.CallContext.GetHttpContext().User.Identity?.Name ?? throw new InvalidOperationException("Unauthenticated"));

    public UnaryResult<int> AddAsync(int a, int b) => UnaryResult(a + b);
}

public interface IAuthorizeHubReceiver {}

public interface IAuthorizeHub : IStreamingHub<IAuthorizeHub, IAuthorizeHubReceiver>
{
    Task<string> GetUserNameAsync();
}

[Authorize]
public class AuthorizeHub : StreamingHubBase<IAuthorizeHub, IAuthorizeHubReceiver>, IAuthorizeHub
{
    public Task<string> GetUserNameAsync()
    {
        return Task.FromResult(Context.CallContext.GetHttpContext().User.Identity?.Name ?? throw new InvalidOperationException("Unauthenticated"));
    }
}