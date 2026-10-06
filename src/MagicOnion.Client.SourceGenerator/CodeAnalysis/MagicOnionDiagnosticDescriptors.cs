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

    public static readonly DiagnosticDescriptor StreamingHubTransportAttributeConflict =
        new DiagnosticDescriptor(
            id: "MOC1009",
            title: "Must specify a single transport attribute",
            messageFormat: "'{0}' has multiple transport attributes ({1}). Specify only one of [Transport], [Reliable], [Unreliable], or [UnreliableUnordered].",
            category: "Usage",
            description: "An interface or method must not specify more than one transport attribute.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StreamingHubTransportModeAmbiguous =
        new DiagnosticDescriptor(
            id: "MOC1010",
            title: "Transport mode is ambiguous",
            messageFormat: "The transport mode of '{0}' is ambiguous for '{1}'. '{2}' specifies {3} and '{4}' specifies {5} at the same inheritance distance. Specify the mode on the method or on '{1}'.",
            category: "Usage",
            description: "Inherited interfaces at the same inheritance distance specify different transport modes.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StreamingHubUnreliableMethodMustNotReturnValue =
        new DiagnosticDescriptor(
            id: "MOC1011",
            title: "Unreliable method must not return a value",
            messageFormat: "The unreliable StreamingHub method '{0}' has return type '{1}'. An unreliable method must return void, Task, or ValueTask. Mark the method with [Reliable] to override an unreliable interface default.",
            category: "Usage",
            description: "A StreamingHub method whose effective transport mode is unreliable cannot return a result.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StreamingHubUnreliableReceiverMethodMustReturnVoid =
        new DiagnosticDescriptor(
            id: "MOC1012",
            title: "Unreliable receiver method must return void",
            messageFormat: "The unreliable StreamingHub receiver method '{0}' has return type '{1}'. A client result method cannot be unreliable. Mark the method with [Reliable] to override an unreliable interface default.",
            category: "Usage",
            description: "A StreamingHub receiver method whose effective transport mode is unreliable must return void.",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
}
