using AotSample.Server;
using AotSample.Server.Services;
using MagicOnion.Serialization.MessagePack;
using MessagePack;
using MessagePack.Resolvers;

var builder = WebApplication.CreateBuilder(args);

// Configure MessagePack with AOT-compatible formatters
// IMPORTANT: Do NOT use StandardResolver.Instance as it includes GeneratedMessagePackResolver
// which uses reflection to instantiate formatters and is not AOT-compatible.
var messagePackOptions = MessagePackSerializerOptions.Standard
    .WithResolver(CompositeResolver.Create(
        MagicOnionAotFormatterResolver.Instance,           // Custom formatters for DynamicArgumentTuple
        AotSample.Shared.AotSampleResolver.Instance,       // Source-generated formatters for DTOs (UserProfile, CreateUserRequest)
        BuiltinResolver.Instance,                          // Built-in types (primitives, arrays, etc.)
        AttributeFormatterResolver.Instance,               // Types with [MessagePackFormatter] attribute
        PrimitiveObjectResolver.Instance                   // object type support
    ));

// Create a custom serializer provider with AOT-compatible options
var serializerProvider = MessagePackMagicOnionSerializerProvider.Default.WithOptions(messagePackOptions);

// Add MagicOnion services with static providers for AOT support
builder.Services.AddGrpc();

// Register service implementations for AOT DI
builder.Services.AddScoped<GreeterService>();
builder.Services.AddScoped<ChatHub>();

builder.Services.AddMagicOnion(options =>
{
    // Use the AOT-compatible MessagePack serializer
    options.MessageSerializer = serializerProvider;
})
    .UseStaticMethodProvider<MagicOnionMethodProvider>()    // AOT-compatible service methods
    .UseStaticProxyFactory<MulticasterProxyFactory>();      // AOT-compatible StreamingHub broadcast

var app = builder.Build();

// Map MagicOnion services (uses static method provider for AOT)
app.MapMagicOnionService();

app.MapGet("/", () => "MagicOnion AOT Sample Server");

app.Run();
