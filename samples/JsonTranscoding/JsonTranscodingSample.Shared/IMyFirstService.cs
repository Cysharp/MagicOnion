using MagicOnion;
using MessagePack;

namespace JsonTranscodingSample.Shared;

/// <summary>
/// This is a service interface for the demonstration.
/// </summary>
public interface IMyFirstService : IService<IMyFirstService>
{
    /// <summary>
    /// Say hello to the specified name.
    /// </summary>
    /// <param name="name"> A name to say hello. </param>
    /// <param name="age">An age of the person to say hello.</param>
    /// <returns></returns>
    UnaryResult<string> SayHelloAsync(string name, int age);

    /// <summary>
    /// Register a user with the specified request.
    /// </summary>
    /// <param name="request">The request to register a user.</param>
    /// <returns></returns>
    UnaryResult<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);

    /// <summary>
    /// Throw an exception.
    /// </summary>
    /// <returns></returns>
    UnaryResult ThrowAsync();

    /// <summary>
    /// Throw an exception with the specified status code and detail.
    /// </summary>
    /// <param name="statusCode">A status code to return.</param>
    /// <param name="detail">A detail message to return.</param>
    /// <returns></returns>
    UnaryResult ThrowWithReturnStatusCodeAsync(int statusCode, string detail);
}

/// <summary>
/// Represents a request to register a user.
/// </summary>
[MessagePackObject]
public class RegisterUserRequest
{
    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    [Key(0)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the age of the user.
    /// </summary>
    [Key(1)]
    public required int Age { get; init; }
}

/// <summary>
/// Represents a response to a user registration request.
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    [Key(0)]
    public required bool Success { get; init; }

    /// <summary>
    /// Gets or sets a message.
    /// </summary>
    [Key(1)]
    public required string Message { get; init; }
}
