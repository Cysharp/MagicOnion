var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddMagicOnion().AddJsonTranscoding();
builder.Services.AddMagicOnionJsonTranscodingSwagger();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeMagicOnionXmlComments(Path.Combine(AppContext.BaseDirectory, "JsonTranscodingSample.Shared.xml"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.MapMagicOnionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
