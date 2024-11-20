using MagicOnion;
using MessagePack;

namespace JsonTranscodingSample.Shared;

public interface IMyFirstService
{
    UnaryResult<string> SayHelloAsync(string name, int age);
    UnaryResult<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);
    UnaryResult ThrowAsync();
    UnaryResult ThrowWithReturnStatusCodeAsync(int statusCode, string detail);
}

[MessagePackObject]
public class RegisterUserRequest
{
    [Key(0)]
    public required string Name { get; init; }
    [Key(1)]
    public required int Age { get; init; }
}

public class RegisterUserResponse
{
    [Key(0)]
    public required bool Success { get; init; }
    [Key(1)]
    public required string Message { get; init; }
}
