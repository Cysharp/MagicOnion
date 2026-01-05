using Microsoft.CodeAnalysis;

namespace MagicOnion.Server.SourceGenerator.CodeAnalysis;

public static class MagicOnionDiagnosticDescriptors
{
    const string Category = "MagicOnionServer";

    public static readonly DiagnosticDescriptor ServiceUnsupportedMethodReturnType = new(
        id: "MOCS001",
        title: "Unsupported method return type",
        messageFormat: "The return type '{0}' of method '{1}' is not supported. The method must return UnaryResult, UnaryResult<T>, Task<ClientStreamingResult<TRequest, TResponse>>, Task<ServerStreamingResult<T>>, or Task<DuplexStreamingResult<TRequest, TResponse>>.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor StreamingMethodMustHaveNoParameters = new(
        id: "MOCS002",
        title: "Streaming method must have no parameters",
        messageFormat: "The method '{0}' is a streaming method and must have no parameters",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor StreamingHubUnsupportedMethodReturnType = new(
        id: "MOCS003",
        title: "Unsupported StreamingHub method return type",
        messageFormat: "The return type '{0}' of StreamingHub method '{1}' is not supported. The method must return void, Task, ValueTask, Task<T>, or ValueTask<T>.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ServiceImplementationNotFound = new(
        id: "MOCS004",
        title: "Service implementation not found",
        messageFormat: "No service implementation found for the specified types.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidServiceType = new(
        id: "MOCS005",
        title: "Invalid service type",
        messageFormat: "The type '{0}' is not a valid MagicOnion service implementation. It must implement IService<T> or IStreamingHub<T, TReceiver>.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ServiceTypeMustBeNonAbstract = new(
        id: "MOCS006",
        title: "Service type must be non-abstract",
        messageFormat: "The service type '{0}' must be a non-abstract class.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor GenerationAttributeRequiresPartialClass = new(
        id: "MOCS007",
        title: "Generation attribute requires partial class",
        messageFormat: "The class '{0}' with [MagicOnionServerGeneration] attribute must be declared as partial.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
