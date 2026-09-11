using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MagicOnion.Server.Tests;

/// <summary>
/// Tests response queue limits configured through the server's configuration binding.
/// </summary>
public class ResponseQueueOptionsTest
{
    /// <summary>
    /// The MagicOnion section binds both limits, including byte limits larger than Int32.MaxValue.
    /// </summary>
    [Fact]
    public void Configuration_BindsResponseQueueLimits()
    {
        using var provider = CreateProvider("7", "5000000000");
        var options = provider.GetRequiredService<IOptions<MagicOnionOptions>>().Value;

        Assert.Equal(7, options.StreamingHubResponseQueueMaxLength);
        Assert.Equal(5_000_000_000L, options.StreamingHubResponseQueueMaxSize);
    }

    /// <summary>
    /// Nonpositive limits are rejected when the server loads its configuration.
    /// </summary>
    [Theory]
    [InlineData("0", "64")]
    [InlineData("-1", "64")]
    [InlineData("16", "0")]
    [InlineData("16", "-1")]
    public void Configuration_InvalidLimit_IsRejected(string length, string size)
    {
        using var provider = CreateProvider(length, size);

        var error = Assert.Throws<TargetInvocationException>(() => provider.GetRequiredService<IOptions<MagicOnionOptions>>().Value);
        Assert.IsType<ArgumentOutOfRangeException>(error.InnerException);
    }

    /// <summary>
    /// The configure callback can override configured limits and disable either limit with null.
    /// </summary>
    [Fact]
    public void ConfigureOptions_CanDisableLimits()
    {
        using var provider = CreateProvider("7", "64", options =>
        {
            options.StreamingHubResponseQueueMaxLength = null;
            options.StreamingHubResponseQueueMaxSize = null;
        });
        var options = provider.GetRequiredService<IOptions<MagicOnionOptions>>().Value;

        Assert.Null(options.StreamingHubResponseQueueMaxLength);
        Assert.Null(options.StreamingHubResponseQueueMaxSize);
    }

    /// <summary>
    /// JSON null values disable the limits instead of retaining the finite defaults.
    /// </summary>
    [Fact]
    public void Configuration_NullLimits_DisablesLimits()
    {
        using var provider = CreateProvider("null", "null");
        var options = provider.GetRequiredService<IOptions<MagicOnionOptions>>().Value;

        Assert.Null(options.StreamingHubResponseQueueMaxLength);
        Assert.Null(options.StreamingHubResponseQueueMaxSize);
    }

    static ServiceProvider CreateProvider(string length, string size, Action<MagicOnionOptions> configure = null)
    {
        var json = $$"""
            { "MagicOnion": {
                "StreamingHubResponseQueueMaxLength": {{length}},
                "StreamingHubResponseQueueMaxSize": {{size}}
            } }
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMagicOnion(configure);
        return services.BuildServiceProvider();
    }
}
