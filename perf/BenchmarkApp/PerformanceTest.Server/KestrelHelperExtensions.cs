public static class KestrelHelperExtensions
{
    private static readonly string defaultListenAddress = "http://localhost:5000";
    private static readonly string defaultProtocol = "h2c";
    private record TlsFile(string PfxFileName, string Password)
    {
        public static TlsFile Default = new TlsFile("server1.pfx", "1111");
    }

    public static void ConfigureEndpoint(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            var endpoint = CreateIPEndpoint(context.Configuration);
            if (endpoint.Address == System.Net.IPAddress.Loopback)
            {
                options.ListenLocalhost(endpoint.Port, listenOptions =>
                {
                    ConfigureListenOptions(listenOptions, context.Configuration, endpoint);
                });
            }
            else
            {
                options.ListenAnyIP(endpoint.Port, listenOptions =>
                {
                    ConfigureListenOptions(listenOptions, context.Configuration, endpoint);
                });
            }

            // Other languages gRPC server don't include a server header
            options.AddServerHeader = false;
        });
    }

    private static System.Net.IPEndPoint CreateIPEndpoint(IConfiguration config)
    {
        var address = CreateBindingAddress(config);

        System.Net.IPAddress? ip;
        if (string.Equals(address.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            ip = System.Net.IPAddress.Loopback;
        }
        else if (!System.Net.IPAddress.TryParse(address.Host, out ip))
        {
            ip = System.Net.IPAddress.IPv6Any;
        }

        return new System.Net.IPEndPoint(ip, address.Port);

        static BindingAddress CreateBindingAddress(IConfiguration config)
        {
            var url = config.GetValue<string>("Url") ?? defaultListenAddress;
            return BindingAddress.Parse(url);
        }
    }

    private static void ConfigureListenOptions(Microsoft.AspNetCore.Server.Kestrel.Core.ListenOptions listenOptions, IConfiguration config, System.Net.IPEndPoint endPoint)
    {
        var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
        var certPath = Path.Combine(basePath!, "Certs", TlsFile.Default.PfxFileName);

        // default is Insecure gRPC
        var protocol = config.GetValue<string>("Protocol") ?? defaultProtocol;

        Console.WriteLine($"Listener Address: {endPoint.Address}:{endPoint.Port}, Protocol: {protocol}");

        switch (protocol)
        {
            case "h2c":
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                    break;
                }
            case "h2":
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                    listenOptions.UseHttps(certPath, TlsFile.Default.Password);
                    break;
                }
            case "h3":
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                    listenOptions.UseHttps(certPath, TlsFile.Default.Password);
                    break;
                }
            default:
                throw new NotImplementedException(protocol);
        }
    }
}
