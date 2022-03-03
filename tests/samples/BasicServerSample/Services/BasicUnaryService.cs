using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;

namespace BasicServerSample.Services;

public interface IBasicUnaryService : IService<IBasicUnaryService>
{
    UnaryResult<Nil> ThrowAsync();

    UnaryResult<MyResponse> ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode statusCode);
    UnaryResult<Nil> ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode statusCode);
    UnaryResult<Nil> NoParameterReturnNilAsync();
    UnaryResult<int> NoParameterReturnValueTypeAsync();
    UnaryResult<MyResponse> NoParameterReturnRefTypeAsync();
    UnaryResult<Nil> OneValueTypeParameterReturnNilAsync(int a);
    UnaryResult<Nil> TwoValueTypeParametersReturnNilAsync(int a, int b);
    UnaryResult<int> OneValueTypeParameterReturnValueTypeAsync(int a);
    UnaryResult<int> TwoValueTypeParametersReturnValueTypeAsync(int a, int b);
    UnaryResult<MyResponse> OneValueTypeParameterReturnRefTypeAsync(int a);
    UnaryResult<MyResponse> TwoValueTypeParametersReturnRefTypeAsync(int a, int b);

    UnaryResult<Nil> OneRefTypeParameterReturnNilAsync(MyRequest a);
    UnaryResult<Nil> TwoRefTypeParametersReturnNilAsync(MyRequest a, MyRequest b);
    UnaryResult<int> OneRefTypeParameterReturnValueTypeAsync(MyRequest a);
    UnaryResult<int> TwoRefTypeParametersReturnValueTypeAsync(MyRequest a, MyRequest b);
    UnaryResult<MyResponse> OneRefTypeParameterReturnRefTypeAsync(MyRequest a);
    UnaryResult<MyResponse> TwoRefTypeParametersReturnRefTypeAsync(MyRequest a, MyRequest b);
}

[MessagePackObject(true)]
public class MyRequest
{
    public int Value { get; }
    public MyRequest(int value)
    {
        Value = value;
    }
}

[MessagePackObject(true)]
public class MyResponse
{
    public string Value { get; }
    public MyResponse(string value)
    {
        Value = value;
    }
}

public class BasicUnaryService : ServiceBase<IBasicUnaryService>, IBasicUnaryService
{
    public UnaryResult<Nil> ThrowAsync()
        => throw new InvalidOperationException("Something went wrong");

    public UnaryResult<MyResponse> ReturnTypeIsRefTypeAndNonSuccessResponseAsync(StatusCode statusCode)
        => throw new ReturnStatusException(statusCode, nameof(ReturnTypeIsRefTypeAndNonSuccessResponseAsync));
    public UnaryResult<Nil> ReturnTypeIsNilAndNonSuccessResponseAsync(StatusCode statusCode)
        => throw new ReturnStatusException(statusCode, nameof(ReturnTypeIsNilAndNonSuccessResponseAsync));

    public UnaryResult<Nil> NoParameterReturnNilAsync()
        => UnaryResult(Nil.Default);

    public UnaryResult<int> NoParameterReturnValueTypeAsync()
        => UnaryResult(1234);

    public UnaryResult<MyResponse> NoParameterReturnRefTypeAsync()
        => UnaryResult(new MyResponse("1234"));

    public UnaryResult<Nil> OneValueTypeParameterReturnNilAsync(int a)
        => UnaryResult(Nil.Default);

    public UnaryResult<Nil> TwoValueTypeParametersReturnNilAsync(int a, int b)
        => UnaryResult(Nil.Default);

    public UnaryResult<int> OneValueTypeParameterReturnValueTypeAsync(int a)
        => UnaryResult(a);

    public UnaryResult<int> TwoValueTypeParametersReturnValueTypeAsync(int a, int b)
        => UnaryResult(a + b);

    public UnaryResult<MyResponse> OneValueTypeParameterReturnRefTypeAsync(int a)
        => UnaryResult(new MyResponse(a.ToString()));

    public UnaryResult<MyResponse> TwoValueTypeParametersReturnRefTypeAsync(int a, int b)
        => UnaryResult(new MyResponse((a + b).ToString()));

    public UnaryResult<Nil> OneRefTypeParameterReturnNilAsync(MyRequest a)
        => UnaryResult(Nil.Default);

    public UnaryResult<Nil> TwoRefTypeParametersReturnNilAsync(MyRequest a, MyRequest b)
        => UnaryResult(Nil.Default);

    public UnaryResult<int> OneRefTypeParameterReturnValueTypeAsync(MyRequest a)
        => UnaryResult(a.Value);

    public UnaryResult<int> TwoRefTypeParametersReturnValueTypeAsync(MyRequest a, MyRequest b)
        => UnaryResult(a.Value + b.Value);

    public UnaryResult<MyResponse> OneRefTypeParameterReturnRefTypeAsync(MyRequest a)
        => UnaryResult(new MyResponse(a.Value.ToString()));

    public UnaryResult<MyResponse> TwoRefTypeParametersReturnRefTypeAsync(MyRequest a, MyRequest b)
        => UnaryResult(new MyResponse((a.Value + b.Value).ToString()));
}