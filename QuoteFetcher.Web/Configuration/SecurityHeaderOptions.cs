using System.ComponentModel.DataAnnotations;

namespace QuoteFetcher.Web.Configuration;

public sealed class SecurityHeaderOptions
{
    public const string SectionName = "SecurityHeaders";

    private static readonly HashSet<string> AllowedXFrameOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "DENY",
        "SAMEORIGIN"
    };

    private static readonly HashSet<string> AllowedReferrerPolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        "no-referrer",
        "no-referrer-when-downgrade",
        "origin",
        "origin-when-cross-origin",
        "same-origin",
        "strict-origin",
        "strict-origin-when-cross-origin",
        "unsafe-url"
    };

    [Required]
    public string XFrameOptions { get; set; } = string.Empty;

    [Required]
    public string XContentTypeOptions { get; set; } = string.Empty;

    [Required]
    public string ReferrerPolicy { get; set; } = string.Empty;

    [Required]
    public string ContentSecurityPolicy { get; set; } = string.Empty;

    public static bool IsValidXFrameOptions(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return AllowedXFrameOptions.Contains(value.Trim());
    }

    public static bool IsValidXContentTypeOptions(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().Equals("nosniff", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidReferrerPolicy(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return AllowedReferrerPolicies.Contains(value.Trim());
    }

    public static bool IsValidContentSecurityPolicy(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized.Contains("default-src")
               && normalized.Contains("script-src")
               && normalized.Contains("style-src");
    }
}
