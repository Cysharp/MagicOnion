using MessagePack;

namespace MagicOnion.Client.NativeAot.Tests;

public interface IUnaryTestService : IService<IUnaryTestService>
{
    UnaryResult<int> TwoParametersReturnValueType(int arg1, string arg2);

    UnaryResult Enum(MyEnumValue value);
    UnaryResult<MyEnumValue> EnumReturn();

    UnaryResult BuiltInGeneric(List<MyObject> arg);
    UnaryResult<Dictionary<MyObject, string>> BuiltInGenericReturn();
}

public enum MyEnumValue
{
    A,
    B,
    C,
}

[MessagePackObject]
public record MyObject([property: Key(0)] int Value);
