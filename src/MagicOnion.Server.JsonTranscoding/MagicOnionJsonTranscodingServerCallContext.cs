using System.Net.Sockets;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using MagicOnion.Server.Binder;
using Microsoft.AspNetCore.Http;

namespace MagicOnion.Server.JsonTranscoding;

public class MagicOnionJsonTranscodingServerCallContext(HttpContext httpContext, IMagicOnionGrpcMethod method) : ServerCallContext, IServerCallContextFeature
{
    Metadata? requestHeaders;

    public ServerCallContext ServerCallContext => this;

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        foreach (var header in responseHeaders)
        {
            var key = header.IsBinary ? header.Key + "-bin" : header.Key;
            var value = header.IsBinary ? Convert.ToBase64String(header.ValueBytes) : header.Value;

            httpContext.Response.Headers.TryAdd(key, value);
        }

        return Task.CompletedTask;
    }

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => throw new NotImplementedException();

    protected override string MethodCore { get; } = $"{method.ServiceName}/{method.MethodName}";

    protected override string HostCore { get; } = httpContext.Request.Host.Value ?? string.Empty;

    protected override string PeerCore { get; } = httpContext.Connection.RemoteIpAddress switch
    {
        { AddressFamily: AddressFamily.InterNetwork } => $"ipv4:{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}",
        { AddressFamily: AddressFamily.InterNetworkV6 } => $"ipv6:{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}",
        { } => $"unknown:{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}",
        _ => "unknown"
    };

    protected override DateTime DeadlineCore => DateTime.MaxValue; // No deadline

    protected override Metadata RequestHeadersCore
    {
        get
        {
            if (requestHeaders is null)
            {
                requestHeaders = new Metadata();
                foreach (var header in httpContext.Request.Headers)
                {
                    var key = header.Key;
                    var value = header.Value;
                    if (key.EndsWith("-bin"))
                    {
                        key = key.Substring(0, key.Length - 4);
                        requestHeaders.Add(key, Convert.FromBase64String(value.ToString()));
                    }
                    else
                    {
                        requestHeaders.Add(key, value.ToString());
                    }
                }
            }

            return requestHeaders;
        }
    }

    protected override CancellationToken CancellationTokenCore => httpContext.RequestAborted;
    protected override Metadata ResponseTrailersCore => throw new NotImplementedException();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    protected override AuthContext AuthContextCore => throw new NotImplementedException();

}
