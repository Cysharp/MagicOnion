#nullable enable

using Grpc.Core;
using MagicOnion.Server.Binder;
using MagicOnion.Server.Internal;
using MagicOnion.Server.NativeAot.Tests;
using MessagePack;

namespace MagicOnion.Server.Tests;

/// <summary>Compares precomputed metadata with the existing runtime discovery path.</summary>
public class GeneratedMethodMetadataTest
{
    /// <summary>Preserves discovered service metadata for every RPC method kind.</summary>
    [Fact]
    public void ServiceMetadataMatchesDiscovery()
    {
        Compare(nameof(MetadataService.Ping), MethodType.Unary, typeof(Nil), typeof(Nil));
        Compare(nameof(MetadataService.Echo), MethodType.Unary, typeof(int), typeof(int));
        Compare(nameof(MetadataService.Greet), MethodType.Unary, typeof(DynamicArgumentTuple<string, int>), typeof(string));
        Compare(nameof(MetadataService.Upload), MethodType.ClientStreaming, typeof(int), typeof(string));
        Compare(nameof(MetadataService.Download), MethodType.ServerStreaming, typeof(DynamicArgumentTuple<int, string>), typeof(string));
        Compare(nameof(MetadataService.Duplex), MethodType.DuplexStreaming, typeof(int), typeof(string));

        static void Compare(string name, MethodType kind, Type request, Type response)
        {
            var method = typeof(MetadataService).GetMethod(name)!;
            var expected = MethodHandlerMetadataFactory.CreateServiceMethodHandlerMetadata<MetadataService>(name);
            var actual = MethodHandlerMetadata.Create(typeof(MetadataService), method, kind, response, request, typeof(IMetadataService));
            Assert.Equal(expected.ServiceImplementationMethod, actual.ServiceImplementationMethod);
            AssertServiceMetadata(expected, actual);
        }
    }

    /// <summary>Preserves hub metadata for inherited contracts and methods without results.</summary>
    [Fact]
    public void HubMetadataMatchesDiscovery()
    {
        Compare(typeof(IMetadataHubParent), nameof(MetadataHub.Query), typeof(DynamicArgumentTuple<string, int>), typeof(int));
        Compare(typeof(IMetadataHub), nameof(MetadataHub.Notify), typeof(Nil), null);
        Compare(typeof(IMetadataHub), nameof(MetadataHub.Update), typeof(int), null);
        Compare(typeof(IMetadataHub), nameof(MetadataHub.Send), typeof(string), null);

        static void Compare(Type declaringInterface, string name, Type request, Type? response)
        {
            var expected = MethodHandlerMetadataFactory.CreateStreamingHubMethodHandlerMetadata<MetadataHub>(name);
            var actual = StreamingHubMethodHandlerMetadata.Create(expected.MethodId, typeof(MetadataHub),
                declaringInterface.GetMethod(name)!, typeof(MetadataHub).GetMethod(name)!, response, request, typeof(IMetadataHub));

            Assert.Equal(expected.MethodId, actual.MethodId);
            Assert.Equal(expected.StreamingHubImplementationType, actual.StreamingHubImplementationType);
            Assert.Equal(expected.StreamingHubInterfaceType, actual.StreamingHubInterfaceType);
            Assert.Equal(expected.InterfaceMethod, actual.InterfaceMethod);
            Assert.Equal(expected.ImplementationMethod, actual.ImplementationMethod);
            Assert.Equal(expected.RequestType, actual.RequestType);
            Assert.Equal(expected.ResponseType, actual.ResponseType);
            Assert.Equal(expected.Parameters, actual.Parameters);
            Assert.Equal(expected.Metadata.Select(x => x.GetType()), actual.Metadata.Select(x => x.GetType()));
            Assert.Equal(expected.AttributeLookup[typeof(MetadataMarkerAttribute)].Cast<MetadataMarkerAttribute>().Select(x => x.Value),
                actual.AttributeLookup[typeof(MetadataMarkerAttribute)].Cast<MetadataMarkerAttribute>().Select(x => x.Value));
        }
    }

    /// <summary>Preserves the built-in Connect method's definition and concrete hub attributes.</summary>
    [Fact]
    public void ConnectMetadataMatchesDiscovery()
    {
        var expected = new MagicOnionStreamingHubConnectMethod<MetadataHub>("Hub").Metadata;
        var actual = MethodHandlerMetadata.CreateStreamingHubConnect<MetadataHub, IMetadataHub, IMetadataReceiver>();
        AssertServiceMetadata(expected, actual);
    }

    static void AssertServiceMetadata(MethodHandlerMetadata expected, MethodHandlerMetadata actual)
    {
        Assert.Equal(expected.ServiceImplementationType, actual.ServiceImplementationType);
        Assert.Equal(expected.ServiceInterface, actual.ServiceInterface);
        Assert.True(expected.ServiceImplementationMethod.HasSameMetadataDefinitionAs(actual.ServiceImplementationMethod));
        Assert.Equal(expected.ServiceImplementationMethod.DeclaringType, actual.ServiceImplementationMethod.DeclaringType);
        Assert.Equal(expected.ServiceImplementationMethod.ReturnType, actual.ServiceImplementationMethod.ReturnType);
        Assert.Equal(expected.MethodType, actual.MethodType);
        Assert.Equal(expected.RequestType, actual.RequestType);
        Assert.Equal(expected.ResponseType, actual.ResponseType);
        Assert.Equal(expected.Parameters, actual.Parameters);
        Assert.Equal(expected.Metadata.Select(x => x.GetType()), actual.Metadata.Select(x => x.GetType()));
        Assert.Equal(expected.AttributeLookup[typeof(MetadataMarkerAttribute)].Cast<MetadataMarkerAttribute>().Select(x => x.Value),
            actual.AttributeLookup[typeof(MetadataMarkerAttribute)].Cast<MetadataMarkerAttribute>().Select(x => x.Value));
    }
}
