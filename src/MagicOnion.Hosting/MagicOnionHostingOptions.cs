using Grpc.Core;
using MagicOnion.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagicOnion.Hosting
{
    /// <summary>
    /// MagicOnion hosted service start-up options. Some options are passed to <see cref="MagicOnionOptions"/>.
    /// </summary>
    public class MagicOnionHostingOptions
    {
        /// <summary>
        /// Server ports to bind MagicOnion gRPC services. If not specified, the service binds to localhost:12345 (insecure) by default.
        /// </summary>
        public MagicOnionHostingServerPortOptions[] ServerPorts { get; set; } = Array.Empty<MagicOnionHostingServerPortOptions>();

        /// <summary>
        /// MagicOnion service options.
        /// </summary>
        public MagicOnionOptions Service { get; set; } = new MagicOnionOptions();

        /// <summary>
        /// gRPC channel options.
        /// </summary>
        public MagicOnionHostingServerChannelOptionsOptions ChannelOptions { get; set; } = null;
    }

    public class MagicOnionHostingServerPortOptions
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 12345;

        public bool UseInsecureConnection { get; set; } = true;

        public MagicOnionHostingServerKeyCertificatePairOptions[] ServerCredentials { get; set; } = Array.Empty<MagicOnionHostingServerKeyCertificatePairOptions>();
    }

    public class MagicOnionHostingServerKeyCertificatePairOptions
    {
        /// <summary>
        /// PEM encoded certificate file path.
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        ///PEM encoded private key file path.
        /// </summary>
        public string KeyPath { get; set; }

        public KeyCertificatePair ToKeyCertificatePair()
        {
            return new KeyCertificatePair(File.ReadAllText(CertificatePath), File.ReadAllText(KeyPath));
        }
    }

    // https://grpc.github.io/grpc/csharp/api/Grpc.Core.ChannelOptions.html
    public class MagicOnionHostingServerChannelOptionsOptions
    {
        /// <summary>
        /// Enable census for tracing and stats collection
        /// </summary>
        public bool? Census { get; set; }
        
        /// <summary>
        /// Default authority for calls.
        /// </summary>
        public string DefaultAuthority { get; set; }

        /// <summary>
        /// Initial sequence number for http2 transports
        /// </summary>
        public int? Http2InitialSequenceNumber { get; set; }

        /// <summary>
        /// Maximum number of concurrent incoming streams to allow on a http2 connection
        /// </summary>
        public int? MaxConcurrentStreams { get; set; }

        /// <summary>
        ///  Maximum message length that the channel can receive
        /// </summary>
        public int? MaxReceiveMessageLength { get; set; }

        /// <summary>
        ///Maximum message length that the channel can send
        /// </summary>
        public int? MaxSendMessageLength { get; set; }

        /// <summary>
        /// Primary user agent: goes at the start of the user-agent metadata
        /// </summary>
        public string PrimaryUserAgentString { get; set; }

        /// <summary>
        /// Secondary user agent: goes at the end of the user-agent metadata
        /// </summary>
        public string SecondaryUserAgentString { get; set; }

        /// <summary>
        /// Allow the use of SO_REUSEPORT for server if it's available (default false)
        /// </summary>
        public bool? SoReuseport { get; set; }

        /// <summary>
        /// Override SSL target check. Only to be used for testing.
        /// </summary>
        public string SslTargetNameOverride { get; set; }

        public IEnumerable<ChannelOption> ToChannelOptions()
        {
            var channelOptions = new List<ChannelOption>();

            if (Census.HasValue) channelOptions.Add(new ChannelOption(ChannelOptions.Census, Census.Value ? 1 : 0));
            if (DefaultAuthority != null) channelOptions.Add(new ChannelOption(ChannelOptions.DefaultAuthority, DefaultAuthority));
            if (Http2InitialSequenceNumber.HasValue) channelOptions.Add(new ChannelOption(ChannelOptions.Http2InitialSequenceNumber, Http2InitialSequenceNumber.Value));
            if (MaxConcurrentStreams.HasValue) channelOptions.Add(new ChannelOption(ChannelOptions.MaxConcurrentStreams, MaxConcurrentStreams.Value));
            if (MaxReceiveMessageLength.HasValue) channelOptions.Add(new ChannelOption(ChannelOptions.MaxReceiveMessageLength, MaxReceiveMessageLength.Value));
            if (MaxSendMessageLength.HasValue) channelOptions.Add(new ChannelOption(ChannelOptions.MaxSendMessageLength, MaxSendMessageLength.Value));
            if (PrimaryUserAgentString != null) channelOptions.Add(new ChannelOption(ChannelOptions.PrimaryUserAgentString, PrimaryUserAgentString));
            if (SecondaryUserAgentString != null) channelOptions.Add(new ChannelOption(ChannelOptions.SecondaryUserAgentString, PrimaryUserAgentString));
            if (SoReuseport.HasValue) channelOptions.Add(new ChannelOption(ChannelOptions.SoReuseport, SoReuseport.Value ? 1 : 0));
            if (SslTargetNameOverride != null) channelOptions.Add(new ChannelOption(ChannelOptions.SslTargetNameOverride, SslTargetNameOverride));

            return channelOptions;
        }
    }
}
