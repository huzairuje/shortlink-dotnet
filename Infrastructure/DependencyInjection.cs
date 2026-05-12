namespace MyFirstApi.Infrastructure;

using MyFirstApi.Core.Ports.Caching;
using Core.Ports.Repositories;
using Core.Ports.Services;
using Core.UseCases;
using Caching;
using Persistence;
using StackExchange.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Persistence ───────────────────────────────────────────
        services.AddSingleton<InMemoryLinkRepository>();
        services.AddSingleton<JsonPersistenceService>();
        services.AddHostedService<PersistenceBackgroundService>();

        // ── Redis ─────────────────────────────────────────────────
        var redisConn = configuration.GetConnectionString("Redis")
                        ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConn));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "myfirstapi:";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        // Decorator — CachedLinkRepository wrap InMemoryLinkRepository
        services.AddSingleton<ILinkRepository>(sp =>
        {
            var inner = sp.GetRequiredService<InMemoryLinkRepository>();
            var cache = sp.GetRequiredService<ICacheService>();
            var logger = sp.GetRequiredService<ILogger<CachedLinkRepository>>();
            return new CachedLinkRepository(inner, cache, logger);
        });

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILinkService, LinkService>();
        return services;
    }
}