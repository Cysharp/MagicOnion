using Microsoft.CodeAnalysis;

namespace MagicOnion.Client.SourceGenerator.CodeAnalysis;

#pragma warning disable RS2008 // Enable analyzer release tracking for the analyzer project containing rule '{0}'

public static class MagicOnionDiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor StreamingHubUnsupportedReceiverMethodReturnType =
        new DiagnosticDescriptor(
            id: "MOC1001",
            title: "Must be void",
            messageFormat: "StreamingHub receiver method '{0}' has unsupported return type '{1}'",
            category: "Usage",
            description: "The StreamingHub receiver method has unsupported return type.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StreamingHubUnsupportedMethodReturnType =
        new DiagnosticDescriptor(
            id: "MOC1002",
            title: "Must be Task or ValueTask",
            messageFormat: "StreamingHub method '{0}' has unsupported return type '{1}'",
            category: "Usage",
            description: "The StreamingHub method has unsupported return type.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ServiceUnsupportedMethodReturnType =
        new DiagnosticDescriptor(
            id: "MOC1003",
            title: "Must be UnaryResult or Task of StreamingResult",
            messageFormat: "Unsupported return type '{0}' ({1}). Allowed return types are UnaryResult and Task of StreamingResult.",
            category: "Usage",
            description: "The service method has unsupported return type.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnaryUnsupportedMethodReturnType =
        new DiagnosticDescriptor(
            id: "MOC1004",
            title: "Must be non-StreamingResult",
            messageFormat: "Unary methods can not return '{0}' ({1})",
            category: "Usage",
            description: "The Unary method has unsupported return type.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StreamingMethodMustHaveNoParameters =
        new DiagnosticDescriptor(
            id: "MOC1005",
            title: "ClientStreaming and DuplexStreaming must be no parameters",
            messageFormat: "ClientStreaming and DuplexStreaming must have no parameters ({0})",
            category: "Usage",
            description: "ClientStreaming and DuplexStreaming must be no parameters.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MessagePackArrayRankLimitation =
        new DiagnosticDescriptor(
            id: "MOC1006",
            title: "Must be less than 5",
            messageFormat: "An array of rank must be less than 5 ({0})",
            category: "Usage",
            description: "An array of rank must be less than 5.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeSpecifyingClientGenerationAttributedMustBePartial =
        new DiagnosticDescriptor(
            id: "MOC1007",
            title: "Must be partial class",
            messageFormat: "The type specifying MagicOnionClientGeneration attribute must be partial class",
            category: "Usage",
            description: "The type specifying MagicOnionClientGeneration attribute must be partial class.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StreamingHubInterfaceHasTwoOrMoreIStreamingHub =
        new DiagnosticDescriptor(
            id: "MOC1008",
            title: "StreamingHub interface must have single interface",
            messageFormat: "The interface '{0}' has two or more IStreamingHub<THub, THubReceiver> interfaces",
            category: "Usage",
            description: "The interface '{0}' has two or more IStreamingHub<THub, THubReceiver> interfaces.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
}
