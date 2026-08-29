using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;
using Underdog.Application.Repositories;
using Underdog.Application.Services;

namespace Underdog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUnderdogApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiModelOptions>(configuration.GetSection(AiModelOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<IIntentCacheRepository, IntentCacheRepository>();
        services.AddScoped<IIntentObservabilityRepository, IntentObservabilityRepository>();
        services.AddScoped<IIntentCacheService, IntentCacheService>();
        services.AddScoped<IIntentProcessingService, IntentProcessingService>();
        services.AddSingleton<IGgufModelService, GgufModelService>();

        return services;
    }
}
