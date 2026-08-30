using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Text;
using Underdog.Application.Abstractions;
using Underdog.Application.Configuration;
using Underdog.Application.Repositories;
using Underdog.Application.Services;
using Underdog.Application.Data;
using Microsoft.Extensions.Caching.Memory;

namespace Underdog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUnderdogApplication(this IServiceCollection services, IConfiguration configuration)
    {
        
        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>();
        var sqlBuilder = new SqlConnectionStringBuilder(cacheOptions!.ConnectionString);

        // Keep password encoded in config and decode it only at runtime.
        if (!string.IsNullOrWhiteSpace(sqlBuilder.Password))
        {
            try
            {
                var decodedBytes = Convert.FromBase64String(sqlBuilder.Password);
                sqlBuilder.Password = Encoding.UTF8.GetString(decodedBytes);
                cacheOptions.ConnectionString = sqlBuilder.ConnectionString;
            }
            catch (FormatException)
            {
                // If password is not Base64, keep original value unchanged.
            }
        }

        services.AddSingleton<IDBMConnection>(_ => new DBMConnection(cacheOptions.ConnectionString));
        //services.Configure<AiModelOptions>(configuration.GetSection(AiModelOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<IIntentCacheRepository, IntentCacheRepository>();
        services.AddScoped<IIntentObservabilityRepository, IntentObservabilityRepository>();
        services.AddScoped<IIntentCacheService, IntentCacheService>();
        services.AddScoped<IIntentProcessingService, IntentProcessingService>();
        // 2. Carica i Pesi del modello come SINGLETON (Solo lettura, Thread-Safe)
        // services.AddSingleton<LLamaWeights>(sp =>
        // {
        //     var options = sp.GetRequiredService<IOptions<AiModelOptions>>().Value;
        //     var modelParams = new ModelParams(options.ModelPath)
        //     {
        //         ContextSize = options.ContextSize,
        //         GpuLayerCount = options.GpuLayerCount,
        //         Threads = Math.Max(1, Environment.ProcessorCount - 1),
        //         BatchThreads = Math.Max(1, Environment.ProcessorCount - 1)
        //     };
        //     return LLamaWeights.LoadFromFile(modelParams);
        // });

        // Registra il servizio Gemini come Singleton
        services.AddSingleton<IModelService, GeminiModelService>();

        return services;
    }
}
