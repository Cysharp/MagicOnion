#nullable enable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;

namespace MagicOnion.Server.Tests;

public interface IUnaryTestService : IService<IUnaryTestService>
{
    UnaryResult<Nil> ThrowAsync();
    UnaryResult<Nil> ThrowOneValueTypeParameterReturnNilAsync(int a);
    UnaryResult<Nil> ThrowTwoValueTypeParameterReturnNilAsync(int a, int b);
    UnaryResult<Nil> ThrowOneRefTypeParameterReturnNilAsync(UnaryTestMyRequest a);
    UnaryResult<Nil> ThrowTwoRefTypeParametersReturnNilAsync(UnaryTestMyRequest a, UnaryTestMyRequest b);

    UnaryResult<UnaryTestMyResponse> ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode statusCode);
    UnaryResult<Nil> ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode statusCode);
    UnaryResult<Nil> NoParameterReturnNilAsync();
    UnaryResult<int> NoParameterReturnValueTypeAsync();
    UnaryResult<UnaryTestMyResponse> NoParameterReturnRefTypeAsync();
    UnaryResult<Nil> OneValueTypeParameterReturnNilAsync(int a);
    UnaryResult<Nil> TwoValueTypeParametersReturnNilAsync(int a, int b);
    UnaryResult<int> OneValueTypeParameterReturnValueTypeAsync(int a);
    UnaryResult<int> TwoValueTypeParametersReturnValueTypeAsync(int a, int b);
    UnaryResult<UnaryTestMyResponse> OneValueTypeParameterReturnRefTypeAsync(int a);
    UnaryResult<UnaryTestMyResponse> TwoValueTypeParametersReturnRefTypeAsync(int a, int b);

    UnaryResult<Nil> OneRefTypeParameterReturnNilAsync(UnaryTestMyRequest a);
    UnaryResult<Nil> TwoRefTypeParametersReturnNilAsync(UnaryTestMyRequest a, UnaryTestMyRequest b);
    UnaryResult<int> OneRefTypeParameterReturnValueTypeAsync(UnaryTestMyRequest a);
    UnaryResult<int> TwoRefTypeParametersReturnValueTypeAsync(UnaryTestMyRequest a, UnaryTestMyRequest b);
    UnaryResult<UnaryTestMyResponse> OneRefTypeParameterReturnRefTypeAsync(UnaryTestMyRequest a);
    UnaryResult<UnaryTestMyResponse> TwoRefTypeParametersReturnRefTypeAsync(UnaryTestMyRequest a, UnaryTestMyRequest b);

    UnaryResult NonGenericNoParameterAsync();
    UnaryResult NonGenericOneValueTypeParameterAsync(int a);
    UnaryResult NonGenericTwoValueTypeParameterAsync(int a, int b);
    UnaryResult<UnaryTestMyResponse?> NullResponseAsync(UnaryTestMyRequest? a);
}

[MessagePackObject(true)]
public class UnaryTestMyRequest
{
    public int Value { get; }
    public UnaryTestMyRequest(int value)
    {
        Value = value;
    }
}

[MessagePackObject(true)]
public class UnaryTestMyResponse
{
    public string Value { get; }
    public UnaryTestMyResponse(string value)
    {
        Value = value;
    }
}

public class UnaryTestService : ServiceBase<IUnaryTestService>, IUnaryTestService
{
    public UnaryResult<Nil> ThrowAsync()
        => throw new InvalidOperationException("Something went wrong");
    public UnaryResult<Nil> ThrowOneValueTypeParameterReturnNilAsync(int a)
        => throw new InvalidOperationException("Something went wrong");
    public UnaryResult<Nil> ThrowTwoValueTypeParameterReturnNilAsync(int a, int b)
        => throw new InvalidOperationException("Something went wrong");
    public UnaryResult<Nil> ThrowOneRefTypeParameterReturnNilAsync(UnaryTestMyRequest a)
        => throw new InvalidOperationException("Something went wrong");
    public UnaryResult<Nil> ThrowTwoRefTypeParametersReturnNilAsync(UnaryTestMyRequest a, UnaryTestMyRequest b)
        => throw new InvalidOperationException("Something went wrong");

    public UnaryResult<UnaryTestMyResponse> ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode statusCode)
        => throw new ReturnStatusException(statusCode, nameof(ReturnTypeIsRefTypeAndNonSuccessResponseAsync));
    public UnaryResult<Nil> ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode statusCode)
        => throw new ReturnStatusException(statusCode, nameof(ReturnTypeIsNilAndNonSuccessResponseAsync));

    public UnaryResult<Nil> NoParameterReturnNilAsync()
        => UnaryResult.FromResult(Nil.Default);

    public UnaryResult<int> NoParameterReturnValueTypeAsync()
        => UnaryResult.FromResult(1234);

    public UnaryResult<UnaryTestMyResponse> NoParameterReturnRefTypeAsync()
        => UnaryResult.FromResult(new UnaryTestMyResponse("1234"));

    public UnaryResult<Nil> OneValueTypeParameterReturnNilAsync(int a)
        => UnaryResult.FromResult(Nil.Default);

    public UnaryResult<Nil> TwoValueTypeParametersReturnNilAsync(int a, int b)
        => UnaryResult.FromResult(Nil.Default);

    public UnaryResult<int> OneValueTypeParameterReturnValueTypeAsync(int a)
        => UnaryResult.FromResult(a);

    public UnaryResult<int> TwoValueTypeParametersReturnValueTypeAsync(int a, int b)
        => UnaryResult.FromResult(a + b);

    public UnaryResult<UnaryTestMyResponse> OneValueTypeParameterReturnRefTypeAsync(int a)
        => UnaryResult.FromResult(new UnaryTestMyResponse(a.ToString()));

    public UnaryResult<UnaryTestMyResponse> TwoValueTypeParametersReturnRefTypeAsync(int a, int b)
        => UnaryResult.FromResult(new UnaryTestMyResponse((a + b).ToString()));

    public UnaryResult<Nil> OneRefTypeParameterReturnNilAsync(UnaryTestMyRequest a)
        => UnaryResult.FromResult(Nil.Default);

    public UnaryResult<Nil> TwoRefTypeParametersReturnNilAsync(UnaryTestMyRequest a, UnaryTestMyRequest b)
        => UnaryResult.FromResult(Nil.Default);

    public UnaryResult<int> OneRefTypeParameterReturnValueTypeAsync(UnaryTestMyRequest a)
        => UnaryResult.FromResult(a.Value);

    public UnaryResult<int> TwoRefTypeParametersReturnValueTypeAsync(UnaryTestMyRequest a, UnaryTestMyRequest b)
        => UnaryResult.FromResult(a.Value + b.Value);

    public UnaryResult<UnaryTestMyResponse> OneRefTypeParameterReturnRefTypeAsync(UnaryTestMyRequest a)
        => UnaryResult.FromResult(new UnaryTestMyResponse(a.Value.ToString()));

    public UnaryResult<UnaryTestMyResponse> TwoRefTypeParametersReturnRefTypeAsync(UnaryTestMyRequest a, UnaryTestMyRequest b)
        => UnaryResult.FromResult(new UnaryTestMyResponse((a.Value + b.Value).ToString()));

    public UnaryResult NonGenericNoParameterAsync()
        => MagicOnion.UnaryResult.CompletedResult;

    public UnaryResult NonGenericOneValueTypeParameterAsync(int a)
        => MagicOnion.UnaryResult.CompletedResult;

    public UnaryResult NonGenericTwoValueTypeParameterAsync(int a, int b)
        => MagicOnion.UnaryResult.CompletedResult;

    public UnaryResult<UnaryTestMyResponse?> NullResponseAsync(UnaryTestMyRequest? a)
        => MagicOnion.UnaryResult.FromResult(default(UnaryTestMyResponse));
}
