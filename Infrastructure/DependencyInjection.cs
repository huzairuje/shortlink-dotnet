using MyFirstApi.Core.Ports.Repositories;
using MyFirstApi.Core.Ports.Services;
using MyFirstApi.Core.UseCases;
using MyFirstApi.Infrastructure.Persistence;

namespace MyFirstApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryLinkRepository>();
        services.AddSingleton<ILinkRepository>(sp =>
            sp.GetRequiredService<InMemoryLinkRepository>());
        services.AddSingleton<JsonPersistenceService>();

        // Background service yang save setiap 1 menit
        services.AddHostedService<PersistenceBackgroundService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILinkService, LinkService>();
        return services;
    }
}