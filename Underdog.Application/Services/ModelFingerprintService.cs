using System.Security.Cryptography;
using Underdog.Application.Configuration;

namespace Underdog.Application.Services;

internal static class ModelFingerprintService
{
    public static string ComputeSha256(string modelPath)
    {
        using var stream = File.OpenRead(modelPath);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public static string ResolveAbsolutePath(AiModelOptions options)
    {
        return Path.GetFullPath(options.ModelPath);
    }
}
