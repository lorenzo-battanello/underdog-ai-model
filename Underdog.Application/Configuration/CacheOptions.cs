namespace Underdog.Application.Configuration;

public sealed class CacheOptions
{
    public const string SectionName = "IntentCache";

    public string ConnectionString { get; set; } = "Data Source=underdog-cache.db";
}
