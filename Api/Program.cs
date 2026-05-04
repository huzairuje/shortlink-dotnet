using System.Text.Json;
using MyFirstApi.Api.Endpoints;
using MyFirstApi.Api.Swagger;
using MyFirstApi.Infrastructure;
using MyFirstApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<SnakeCaseSchemaFilter>();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

// Tambah ini — configure JSON serialization globally
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var persistence = app.Services.GetRequiredService<JsonPersistenceService>();

// Restore saat startup — ini sudah benar, await di top-level
await persistence.LoadAsync();

// Fix: Register tidak support async, jadi block dengan .GetAwaiter().GetResult()
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    persistence.SaveAsync().GetAwaiter().GetResult();
});

app.MapLinkEndpoints();

app.Run();