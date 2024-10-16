namespace Microsoft.AspNetCore.Builder;

public class MagicOnionServiceEndpointConventionBuilder(GrpcServiceEndpointConventionBuilder inner) : IEndpointConventionBuilder
{
    public void Add(Action<EndpointBuilder> convention)
        => inner.Add(convention);
}
