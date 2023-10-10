using System.Text.RegularExpressions;
using MagicOnion.Client.SourceGenerator.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeGen.Extensions;

public static class MagicOnionServiceInfoExtensions
{
    // NOTE: A client name is derived from original interface name without 'I' prefix.
    // - ImportantService  --> ImportantServiceClient
    // - IImportantService --> ImportantServiceClient
    // - I0123Service      --> I0123ServiceClient
    public static string GetClientName(this IMagicOnionServiceInfo serviceInfo)
        => (Regex.IsMatch(serviceInfo.ServiceType.Name, "I[^a-z0-9]") ? serviceInfo.ServiceType.Name.Substring(1) : serviceInfo.ServiceType.Name) + "Client";

    public static string GetClientFullName(this IMagicOnionServiceInfo serviceInfo)
        => (serviceInfo.ServiceType.Namespace != null ? serviceInfo.ServiceType.Namespace + "." : "") + serviceInfo.GetClientName();
}
