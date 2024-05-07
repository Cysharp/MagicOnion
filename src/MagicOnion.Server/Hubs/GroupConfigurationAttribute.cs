using Multicaster;

namespace MagicOnion.Server.Hubs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class GroupConfigurationAttribute : Attribute
{
    public Type FactoryType { get; }

    public GroupConfigurationAttribute(Type groupProviderType)
    {
        if (!typeof(IMulticastGroupProvider).IsAssignableFrom(groupProviderType) && (groupProviderType.IsAbstract || groupProviderType.IsInterface))
        {
            throw new ArgumentException("A Group provider must implement IMulticastGroupProvider interface and must be a concrete class.");
        }

        this.FactoryType = groupProviderType;
    }
}
