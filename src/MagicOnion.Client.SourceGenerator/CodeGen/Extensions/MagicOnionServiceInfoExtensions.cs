using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen.Extensions;

public static class MagicOnionServiceInfoExtensions
{
    // NOTE: A client name is derived from original interface name without 'I' prefix.
    // - ImportantService  --> ImportantServiceClient
    // - IImportantService --> ImportantServiceClient
    // - I0123Service      --> I0123ServiceClient
    public static string GetClientName(this IMagicOnionServiceInfo serviceInfo)
    => (
        serviceInfo.ServiceType.Name.Length > 1 && serviceInfo.ServiceType.Name.StartsWith("I") && !Char.IsNumber(serviceInfo.ServiceType.Name[1])
            ? serviceInfo.ServiceType.Name.Substring(1)
            : serviceInfo.ServiceType.Name
        ) + "Client";

    // - Foo.Bar.BazClient --> Foo_Bar_BazClient
    public static string GetClientFullName(this IMagicOnionServiceInfo serviceInfo)
        => ((serviceInfo.ServiceType.Namespace != null ? serviceInfo.ServiceType.Namespace + "." : "") + serviceInfo.GetClientName()).Replace(".", "_");
}
