using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Underdog.Application.Services;

public static class TextNormalizer
{
    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static (string NormalizedText, string Hash) CleanAndHash(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (string.Empty, string.Empty);
        }

        var cleaned = text.ToLowerInvariant().Trim();
        cleaned = PunctuationRegex.Replace(cleaned, string.Empty);
        cleaned = MultiSpaceRegex.Replace(cleaned, " ");

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cleaned));
        var hash = Convert.ToHexString(hashBytes);

        return (cleaned, hash);
    }
}
