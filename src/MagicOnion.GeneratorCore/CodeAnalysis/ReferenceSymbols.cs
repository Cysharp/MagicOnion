using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MagicOnion.Generator.CodeAnalysis
{
    public class ReferenceSymbols
    {
        public readonly INamedTypeSymbol Void;
        public readonly INamedTypeSymbol Task;
        public readonly INamedTypeSymbol TaskOfT;
        public readonly INamedTypeSymbol UnaryResult;
        public readonly INamedTypeSymbol ClientStreamingResult;
        public readonly INamedTypeSymbol ServerStreamingResult;
        public readonly INamedTypeSymbol DuplexStreamingResult;
        public readonly INamedTypeSymbol IServiceMarker;
        public readonly INamedTypeSymbol IService;
        public readonly INamedTypeSymbol IStreamingHubMarker;
        public readonly INamedTypeSymbol IStreamingHub;
        public readonly INamedTypeSymbol MethodIdAttribute;

        public ReferenceSymbols(Compilation compilation, Action<string> logger)
        {
            INamedTypeSymbol GetTypeSymbolOrThrow(string name, SpecialType type = SpecialType.None, bool required = true)
            {
                var symbol = compilation.GetTypeByMetadataName(name)
                             ?? GetWellKnownType(name)
                             ?? GetSpecialType(type);

                if (symbol == null)
                {
                    var message = "failed to get metadata of " + name;
                    if (required) throw new InvalidOperationException(message);
                    logger(message);
                }

                return symbol;
            }

            var getTypeFromMetadataName = typeof(Compilation).Assembly
                .GetType("Microsoft.CodeAnalysis.WellKnownTypes")
                .GetMethod("GetTypeFromMetadataName", BindingFlags.Static | BindingFlags.Public);

            var getWellKnownType = compilation.GetType()
                .GetMethod("CommonGetWellKnownType", BindingFlags.Instance | BindingFlags.NonPublic);

            INamedTypeSymbol GetWellKnownType(string name)
            {
                var wellKnownType = getTypeFromMetadataName?.Invoke(null, new object[] { name });
                var instance = wellKnownType != null && (int)wellKnownType > 0
                    ? getWellKnownType?.Invoke(compilation, new[] { wellKnownType })
                    : null;

                // Roslyn returns PENamedTypeSymbol which is not convertable to INamedTypeSymbol
                // https://github.com/dotnet/roslyn/blob/main/src/Compilers/CSharp/Portable/Symbols/Metadata/PE/PENamedTypeSymbol.cs#L2408
                return instance as INamedTypeSymbol;
            }

            INamedTypeSymbol GetSpecialType(SpecialType type)
            {
                return type != SpecialType.None
                    ? compilation.GetSpecialType(type)
                    : null;
            }

            Void = GetTypeSymbolOrThrow("System.Void", SpecialType.System_Void);
            Task = GetTypeSymbolOrThrow("System.Threading.Tasks.Task", required: false);
            TaskOfT = GetTypeSymbolOrThrow("System.Threading.Tasks.Task`1", required: false);
            UnaryResult = GetTypeSymbolOrThrow("MagicOnion.UnaryResult`1");
            ClientStreamingResult = GetTypeSymbolOrThrow("MagicOnion.ClientStreamingResult`2");
            DuplexStreamingResult = GetTypeSymbolOrThrow("MagicOnion.DuplexStreamingResult`2");
            ServerStreamingResult = GetTypeSymbolOrThrow("MagicOnion.ServerStreamingResult`1");
            IStreamingHubMarker = GetTypeSymbolOrThrow("MagicOnion.IStreamingHubMarker");
            IServiceMarker = GetTypeSymbolOrThrow("MagicOnion.IServiceMarker");
            IStreamingHub = GetTypeSymbolOrThrow("MagicOnion.IStreamingHub`2");
            IService = GetTypeSymbolOrThrow("MagicOnion.IService`1");
            MethodIdAttribute = GetTypeSymbolOrThrow("MagicOnion.Server.Hubs.MethodIdAttribute");
        }
    }
}