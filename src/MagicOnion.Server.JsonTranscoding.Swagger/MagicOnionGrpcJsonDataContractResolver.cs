using Swashbuckle.AspNetCore.SwaggerGen;

namespace MagicOnion.Server.JsonTranscoding.Swagger;

internal class MagicOnionGrpcJsonDataContractResolver(ISerializerDataContractResolver inner) : ISerializerDataContractResolver
{
    public DataContract GetDataContractForType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() is { FullName: not null } openType)
        {
            var typeArgs = type.GetGenericArguments();
            if (openType.FullName.StartsWith("MagicOnion.DynamicArgumentTuple`"))
            {
                return DataContract.ForObject(type, typeArgs.Select((x, i) => new DataProperty($"item{i+1}", x)).ToArray());
            }
        }

        return inner.GetDataContractForType(type);
    }
}
