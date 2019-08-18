using System;
using System.Threading.Tasks;
using Xunit;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Memory;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MagicOnion.Server;
using System.Linq;

namespace MagicOnion.Hosting.Tests
{

    public class HostingTest
    {
        [Fact]
        public async Task TestHosting()
        {
            var randomPort = new Random().Next(10000, 20000);
            var ports = new[] { new ServerPort("localhost", randomPort, ServerCredentials.Insecure) };
            using (var host = new HostBuilder()
                .UseMagicOnion(ports, new MagicOnion.Server.MagicOnionOptions(), types: new []{ typeof(TestServiceImpl) }).Build())
            {
                host.Start();
                var channel = new Channel("localhost", randomPort, ChannelCredentials.Insecure);
                var client = MagicOnion.Client.MagicOnionClient.Create<ITestService>(channel);
                for (int i = 0; i < 10; i++)
                {
                    var ret = await client.Sum(i, i);
                    Assert.Equal(i * 2, ret);
                }
                await host.StopAsync();
            }
        }

        [Fact]
        public async Task TestHosting2()
        {
            var randomPort = new Random().Next(10000, 20000);
            using (var host = new HostBuilder()
                .UseMagicOnion(types: new[] { typeof(TestServiceImpl) })
                .ConfigureServices(services =>
                {
                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        options.ServerPorts = new[] { new MagicOnionHostingServerPortOptions { Host = "localhost", Port = randomPort, UseInsecureConnection = true } };
                    });
                })
                .Build())
            {
                host.Start();
                var channel = new Channel("localhost", randomPort, ChannelCredentials.Insecure);
                var client = MagicOnion.Client.MagicOnionClient.Create<ITestService>(channel);
                for (int i = 0; i < 10; i++)
                {
                    var ret = await client.Sum(i, i);
                    Assert.Equal(i * 2, ret);
                }
                await host.StopAsync();
            }
        }

        [Fact]
        public async Task TestHostingMultiHost()
        {
            var randomPort = new Random().Next(10000, 20000);
            var randomPortSecondary = new Random().Next(10000, 20000);
            using (var host = new HostBuilder()
                .UseMagicOnion(types: new[] { typeof(TestServiceImpl) })
                .UseMagicOnion("Secondary", types: new[] { typeof(TestServiceImpl) })
                .ConfigureServices(services =>
                {
                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        options.ServerPorts = new[] { new MagicOnionHostingServerPortOptions { Host = "localhost", Port = randomPort, UseInsecureConnection = true } };
                    });
                    services.Configure<MagicOnionHostingOptions>("Secondary", options =>
                    {
                        options.ServerPorts = new[] { new MagicOnionHostingServerPortOptions { Host = "localhost", Port = randomPortSecondary, UseInsecureConnection = true } };
                    });
                })
                .Build())
            {
                host.Start();
                var channel = new Channel("localhost", randomPort, ChannelCredentials.Insecure);
                var client = MagicOnion.Client.MagicOnionClient.Create<ITestService>(channel);
                for (int i = 0; i < 10; i++)
                {
                    var ret = await client.Sum(i, i);
                    Assert.Equal(i * 2, ret);
                }

                var channel2 = new Channel("localhost", randomPortSecondary, ChannelCredentials.Insecure);
                var client2 = MagicOnion.Client.MagicOnionClient.Create<ITestService>(channel);
                for (int i = 0; i < 10; i++)
                {
                    var ret = await client2.Sum(i, i);
                    Assert.Equal(i * 2, ret);
                }
                await host.StopAsync();
            }
        }

        [Fact]
        public async Task TestHostingConfigurationDefaults()
        {
            using (var host = new HostBuilder()
                .UseMagicOnion()
                .Build())
            {
                var optionsAccessor = host.Services.GetService<IOptionsMonitor<MagicOnionHostingOptions>>();
                var options = optionsAccessor.CurrentValue;

                Assert.NotNull(options.Service);
                Assert.NotNull(options.ChannelOptions);
                Assert.NotNull(options.ServerPorts);

                Assert.False(options.Service.DisableEmbeddedService);
                Assert.False(options.Service.IsReturnExceptionStackTraceInErrorDetail);

                Assert.Null(options.ChannelOptions.Census);
                Assert.Null(options.ChannelOptions.MaxReceiveMessageLength);
                Assert.Null(options.ChannelOptions.MaxSendMessageLength);

                Assert.Single(options.ServerPorts);
                Assert.Equal("localhost", options.ServerPorts[0].Host);
                Assert.Equal(12345, options.ServerPorts[0].Port);
                Assert.True(options.ServerPorts[0].UseInsecureConnection);
                Assert.Empty(options.ServerPorts[0].ServerCredentials);
            }
        }

        [Fact]
        public async Task TestHostingConfigurationFromSource()
        {
            using (var host = new HostBuilder()
                .UseMagicOnion()
                .ConfigureAppConfiguration(config =>
                {
                    var memoryConfiguration = new MemoryConfigurationSource();
                    memoryConfiguration.InitialData = new[]
                    {
                        new KeyValuePair<string, string>("MagicOnion:Service:DisableEmbeddedService", "true"),
                        new KeyValuePair<string, string>("MagicOnion:Service:IsReturnExceptionStackTraceInErrorDetail", "true"),

                        new KeyValuePair<string, string>("MagicOnion:ChannelOptions:MaxReceiveMessageLength", "12345"),
                        new KeyValuePair<string, string>("MagicOnion:ChannelOptions:Census", "false"),

                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:Host", "host.example.local"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:Port", "9876"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:UseInsecureConnection", "false"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:ServerCredentials:0:CertificatePath", "/path/to/server.crt"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:ServerCredentials:0:KeyPath", "/path/to/server.key"),
                    };
                    config.Add(memoryConfiguration);
                })
                .Build())
            {
                var optionsAccessor = host.Services.GetService<IOptionsMonitor<MagicOnionHostingOptions>>();
                var options = optionsAccessor.CurrentValue;

                Assert.True(options.Service.DisableEmbeddedService);
                Assert.True(options.Service.IsReturnExceptionStackTraceInErrorDetail);

                Assert.False(options.ChannelOptions.Census);
                Assert.Equal(12345, options.ChannelOptions.MaxReceiveMessageLength);

                Assert.Single(options.ServerPorts);
                Assert.Equal("host.example.local", options.ServerPorts[0].Host);
                Assert.Equal(9876, options.ServerPorts[0].Port);
                Assert.False(options.ServerPorts[0].UseInsecureConnection);
                Assert.Single(options.ServerPorts[0].ServerCredentials);
                Assert.Equal("/path/to/server.crt", options.ServerPorts[0].ServerCredentials[0].CertificatePath);
                Assert.Equal("/path/to/server.key", options.ServerPorts[0].ServerCredentials[0].KeyPath);
            }
        }

        [Fact]
        public async Task TestHostingConfigurationMulti()
        {
            using (var host = new HostBuilder()
                .UseMagicOnion()
                .UseMagicOnion("MagicOnion-Secondary", types: new[] { typeof(TestServiceImpl) })
                .ConfigureAppConfiguration(config =>
                {
                    var memoryConfiguration = new MemoryConfigurationSource();
                    memoryConfiguration.InitialData = new[]
                    {
                        new KeyValuePair<string, string>("MagicOnion:Service:DisableEmbeddedService", "true"),
                        new KeyValuePair<string, string>("MagicOnion:Service:IsReturnExceptionStackTraceInErrorDetail", "true"),
                        new KeyValuePair<string, string>("MagicOnion:ChannelOptions:MaxReceiveMessageLength", "12345"),
                        new KeyValuePair<string, string>("MagicOnion:ChannelOptions:Census", "false"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:Host", "host.example.local"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:Port", "9876"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:UseInsecureConnection", "false"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:ServerCredentials:0:CertificatePath", "/path/to/server.crt"),
                        new KeyValuePair<string, string>("MagicOnion:ServerPorts:0:ServerCredentials:0:KeyPath", "/path/to/server.key"),

                        new KeyValuePair<string, string>("MagicOnion-Secondary:Service:DisableEmbeddedService", "false"),
                        new KeyValuePair<string, string>("MagicOnion-Secondary:Service:IsReturnExceptionStackTraceInErrorDetail", "false"),
                        new KeyValuePair<string, string>("MagicOnion-Secondary:ChannelOptions:MaxReceiveMessageLength", "54321"),
                        new KeyValuePair<string, string>("MagicOnion-Secondary:ChannelOptions:Census", "true"),
                        new KeyValuePair<string, string>("MagicOnion-Secondary:ServerPorts:0:Host", "secondary.example.local"),
                        new KeyValuePair<string, string>("MagicOnion-Secondary:ServerPorts:0:Port", "12345"),
                        new KeyValuePair<string, string>("MagicOnion-Secondary:ServerPorts:0:UseInsecureConnection", "true"),
                    };
                    config.Add(memoryConfiguration);
                })
                .Build())
            {
                {
                    var optionsAccessor = host.Services.GetService<IOptionsMonitor<MagicOnionHostingOptions>>();
                    var options = optionsAccessor.CurrentValue;

                    Assert.True(options.Service.DisableEmbeddedService);
                    Assert.True(options.Service.IsReturnExceptionStackTraceInErrorDetail);

                    Assert.False(options.ChannelOptions.Census);
                    Assert.Equal(12345, options.ChannelOptions.MaxReceiveMessageLength);

                    Assert.Single(options.ServerPorts);
                    Assert.Equal("host.example.local", options.ServerPorts[0].Host);
                    Assert.Equal(9876, options.ServerPorts[0].Port);
                    Assert.False(options.ServerPorts[0].UseInsecureConnection);
                    Assert.Single(options.ServerPorts[0].ServerCredentials);
                    Assert.Equal("/path/to/server.crt", options.ServerPorts[0].ServerCredentials[0].CertificatePath);
                    Assert.Equal("/path/to/server.key", options.ServerPorts[0].ServerCredentials[0].KeyPath);
                }
                {
                    var optionsAccessor = host.Services.GetService<IOptionsMonitor<MagicOnionHostingOptions>>();
                    var options = optionsAccessor.Get("MagicOnion-Secondary");

                    Assert.False(options.Service.DisableEmbeddedService);
                    Assert.False(options.Service.IsReturnExceptionStackTraceInErrorDetail);

                    Assert.True(options.ChannelOptions.Census);
                    Assert.Equal(54321, options.ChannelOptions.MaxReceiveMessageLength);

                    Assert.Single(options.ServerPorts);
                    Assert.Equal("secondary.example.local", options.ServerPorts[0].Host);
                    Assert.Equal(12345, options.ServerPorts[0].Port);
                    Assert.True(options.ServerPorts[0].UseInsecureConnection);
                    Assert.Empty(options.ServerPorts[0].ServerCredentials);
                }
            }
        }

        [Fact]
        public async Task TestHostingConfigurationServiceDefinition()
        {
            using (var host = new HostBuilder()
                .UseMagicOnion(types: new[] { typeof(TestServiceImpl) })
                .Build())
            {
                Assert.NotNull(host.Services.GetService<MagicOnionHostedServiceDefinition>());
                Assert.NotNull(host.Services.GetService<MagicOnionHostedServiceDefinition>().ServiceDefinition);
            }
        }

        [Fact]
        public async Task TestHostingConfigurationServiceDefinitionMulti()
        {
            using (var host = new HostBuilder()
                .UseMagicOnion(types: new[] { typeof(TestServiceImpl) })
                .UseMagicOnion("Secondary", types: new[] { typeof(TestServiceImpl) })
                .Build())
            {
                Assert.NotNull(host.Services.GetService<MagicOnionHostedServiceDefinition>());
                Assert.NotNull(host.Services.GetService<MagicOnionHostedServiceDefinition>().ServiceDefinition);
                Assert.Equal(2, host.Services.GetServices<MagicOnionHostedServiceDefinition>().Count());
                Assert.Contains(host.Services.GetServices<MagicOnionHostedServiceDefinition>(), x => x.Name == "" /* Default is empty */);
                Assert.Contains(host.Services.GetServices<MagicOnionHostedServiceDefinition>(), x => x.Name == "Secondary");
            }
        }

        [Fact]
        public async Task TestHostingConfigurationWithDI()
        {
            var randomPort = new Random().Next(10000, 20000);
            using (var host = new HostBuilder()
                .UseMagicOnion(types: new[] { typeof(TestServiceWithDIImpl) })
                .ConfigureServices(services =>
                {
                    services.Configure<MagicOnionHostingOptions>(options =>
                    {
                        options.ServerPorts = new[] { new MagicOnionHostingServerPortOptions { Host = "localhost", Port = randomPort, UseInsecureConnection = true } };
                    });
                })
                .Build())
            {
                host.Start();
                var channel = new Channel("localhost", randomPort, ChannelCredentials.Insecure);
                var client = MagicOnion.Client.MagicOnionClient.Create<ITestService>(channel);
                for (int i = 0; i < 10; i++)
                {
                    var ret = await client.Sum(i, i);
                    Assert.Equal(i * 2, ret);
                }

                await host.StopAsync();
            }
        }
    }
}
