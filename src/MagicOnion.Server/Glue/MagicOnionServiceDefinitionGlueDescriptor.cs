namespace MagicOnion.Server.Glue;

public class MagicOnionServiceDefinitionGlueDescriptor
{
    public Type GlueServiceType { get; }
    public MagicOnionServiceDefinition ServiceDefinition { get; }

    public MagicOnionServiceDefinitionGlueDescriptor(Type glueServiceType, MagicOnionServiceDefinition serviceDefinition)
    {
        GlueServiceType = glueServiceType;
        ServiceDefinition = serviceDefinition;
    }
}
