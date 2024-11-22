using Grpc.Core;
using JsonTranscodingSample.Shared;
using MagicOnion;
using MagicOnion.Server;

namespace JsonTranscodingSample.Server;

public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    public UnaryResult<string> SayHelloAsync(string name, int age)
    {
        return UnaryResult.FromResult($"Hello {name} ({age})!");
    }

    public UnaryResult<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request)
    {
        return new UnaryResult<RegisterUserResponse>(new RegisterUserResponse()
        {
            Success = true,
            Message = $"Welcome {request.Name}!"
        });
    }

    public UnaryResult ThrowAsync()
    {
        throw new InvalidOperationException("Something went wrong.");
    }

    public UnaryResult ThrowWithReturnStatusCodeAsync(int statusCode, string detail)
    {
        throw new ReturnStatusException((StatusCode)statusCode, detail);
    }
}
