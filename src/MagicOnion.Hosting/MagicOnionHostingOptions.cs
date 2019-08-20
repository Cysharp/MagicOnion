using Grpc.Core;
using MagicOnion.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

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
        public MagicOnionHostingServerChannelOptionsOptions ChannelOptions { get; set; } = new MagicOnionHostingServerChannelOptionsOptions();
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
    public class MagicOnionHostingServerChannelOptionsOptions : IDictionary<string, string>
    {
        private Dictionary<string, ChannelOption> _channelOptions = new Dictionary<string, ChannelOption>();

        /// <summary>
        /// Enable census for tracing and stats collection
        /// </summary>
        public bool? Census
        {
            get => GetBoolOrDefault(ChannelOptions.Census);
            set => SetBool(ChannelOptions.Census, value);
        }

        /// <summary>
        /// Default authority for calls.
        /// </summary>
        public string DefaultAuthority
        {
            get => GetStringOrDefault(ChannelOptions.DefaultAuthority);
            set => SetString(ChannelOptions.DefaultAuthority, value);
        }

        /// <summary>
        /// Initial sequence number for http2 transports
        /// </summary>
        public int? Http2InitialSequenceNumber
        {
            get => GetIntOrDefault(ChannelOptions.Http2InitialSequenceNumber);
            set => SetInt(ChannelOptions.Http2InitialSequenceNumber, value);
        }

        /// <summary>
        /// Maximum number of concurrent incoming streams to allow on a http2 connection
        /// </summary>
        public int? MaxConcurrentStreams
        {
            get => GetIntOrDefault(ChannelOptions.MaxConcurrentStreams);
            set => SetInt(ChannelOptions.MaxConcurrentStreams, value);
        }

        /// <summary>
        ///  Maximum message length that the channel can receive
        /// </summary>
        public int? MaxReceiveMessageLength
        {
            get => GetIntOrDefault(ChannelOptions.MaxReceiveMessageLength);
            set => SetInt(ChannelOptions.MaxReceiveMessageLength, value);
        }

        /// <summary>
        ///Maximum message length that the channel can send
        /// </summary>
        public int? MaxSendMessageLength
        {
            get => GetIntOrDefault(ChannelOptions.MaxSendMessageLength);
            set => SetInt(ChannelOptions.MaxSendMessageLength, value);
        }

        /// <summary>
        /// Primary user agent: goes at the start of the user-agent metadata
        /// </summary>
        public string PrimaryUserAgentString
        {
            get => GetStringOrDefault(ChannelOptions.PrimaryUserAgentString);
            set => SetString(ChannelOptions.PrimaryUserAgentString, value);
        }

        /// <summary>
        /// Secondary user agent: goes at the end of the user-agent metadata
        /// </summary>
        public string SecondaryUserAgentString
        {
            get => GetStringOrDefault(ChannelOptions.SecondaryUserAgentString);
            set => SetString(ChannelOptions.SecondaryUserAgentString, value);
        }

        /// <summary>
        /// Allow the use of SO_REUSEPORT for server if it's available (default false)
        /// </summary>
        public bool? SoReuseport
        {
            get => GetBoolOrDefault(ChannelOptions.SoReuseport);
            set => SetBool(ChannelOptions.SoReuseport, value);
        }

        /// <summary>
        /// Override SSL target check. Only to be used for testing.
        /// </summary>
        public string SslTargetNameOverride
        {
            get => GetStringOrDefault(ChannelOptions.SslTargetNameOverride);
            set => SetString(ChannelOptions.SslTargetNameOverride, value);
        }

        public IEnumerable<ChannelOption> ToChannelOptions()
        {
            return _channelOptions.Values;
        }

        public IEnumerator<ChannelOption> GetEnumerator()
            => _channelOptions.Values.GetEnumerator();

        public int Count => _channelOptions.Count;

        public bool TryGetValue(string key, out ChannelOption value)
            => _channelOptions.TryGetValue(key, out value);

        public ChannelOption this[string key]
        {
            get => _channelOptions[key];
            set => _channelOptions[key] = value ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Adds the ChannelOption to options.
        /// </summary>
        /// <param name="channelOption"></param>
        public void Add(ChannelOption channelOption)
        {
            _channelOptions.Add(channelOption.Name, channelOption);
        }

        /// <summary>
        /// Adds the specified key and integer value as ChannelOption to options.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, int value)
        {
            Add(new ChannelOption(key, value));
        }

        /// <summary>
        /// Adds the specified key and string value as ChannelOption to options.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            Add(new ChannelOption(key, value));
        }

        public bool ContainsKey(string key)
        {
            return _channelOptions.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _channelOptions.Remove(key);
        }

        public void Clear()
        {
            _channelOptions.Clear();
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            foreach (var keyValue in _channelOptions)
            {
                yield return new KeyValuePair<string, string>(keyValue.Key, GetValueAsObject(keyValue.Value).ToString());
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)this).GetEnumerator();

        void IDictionary<string, string>.Add(string key, string value)
        {
            _channelOptions.Add(key, CreateChannelOption(key, value));
        }

        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            if (TryGetValue(key, out var channelOption))
            {
                value = GetValueAsObject(channelOption).ToString();
                return true;
            }

            value = default;
            return false;
        }

        string IDictionary<string, string>.this[string key]
        {
            get => GetValueAsObject(_channelOptions[key]).ToString();
            set => _channelOptions[key] = CreateChannelOption(key, value);
        }

        ICollection<string> IDictionary<string, string>.Keys => _channelOptions.Keys;
        ICollection<string> IDictionary<string, string>.Values => _channelOptions.Values.Select(x => GetValueAsObject(x).ToString()).ToList();


        public ICollection<string> Keys => _channelOptions.Keys;
        public ICollection<ChannelOption> Values => _channelOptions.Values;

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
            => throw new NotSupportedException();

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly
            => throw new NotSupportedException();

        private bool? GetBoolOrDefault(string key)
            => _channelOptions.TryGetValue(key, out var value)
                ? (bool?)(value.IntValue == 1)
                : default;

        private void SetBool(string key, bool? value)
        {
            if (value.HasValue)
            {
                _channelOptions[key] = new ChannelOption(key, value.Value ? 1 : 0);
            }
            else
            {
                if (_channelOptions.ContainsKey(key))
                {
                    _channelOptions.Remove(key);
                }
            }
        }

        private int? GetIntOrDefault(string key)
            => _channelOptions.TryGetValue(key, out var value)
                ? (int?)value.IntValue
                : default;

        private void SetInt(string key, int? value)
        {
            if (value.HasValue)
            {
                _channelOptions[key] = new ChannelOption(key, value.Value);
            }
            else
            {
                if (_channelOptions.ContainsKey(key))
                {
                    _channelOptions.Remove(key);
                }
            }
        }

        private string GetStringOrDefault(string key)
            => _channelOptions.TryGetValue(key, out var value)
                ? value.StringValue
                : default;

        private void SetString(string key, string value)
        {
            if (value != null)
            {
                _channelOptions[key] = CreateChannelOption(key, value);
            }
            else
            {
                if (_channelOptions.ContainsKey(key))
                {
                    _channelOptions.Remove(key);
                }
            }
        }

        private static ChannelOption CreateChannelOption(string key, string value)
            => int.TryParse(value, out var intValue)
                ? new ChannelOption(key, intValue)
                : String.Compare("true", value, StringComparison.OrdinalIgnoreCase) == 0
                    ? new ChannelOption(key, 1)
                        : String.Compare("false", value, StringComparison.OrdinalIgnoreCase) == 0
                            ? new ChannelOption(key, 0)
                            : new ChannelOption(key, value);

        private static object GetValueAsObject(ChannelOption channelOption)
            => channelOption.Type == ChannelOption.OptionType.Integer
                ? (object) channelOption.IntValue
                : (object) channelOption.StringValue;
    }
}
