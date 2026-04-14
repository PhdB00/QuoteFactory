using System.ComponentModel.DataAnnotations;

namespace QuoteFetcher.Application;

public sealed class QuoteApiSettings
{
    public const string ConfigSectionPath = nameof(QuoteApiSettings);
    
    [Required]
    public required string ApiHost { get; init; }
    
    [Required]
    [Range(1, 50, 
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public required int MaxConcurrentApiCalls { get; init;}
    
    [Required]
    [Range(1, 50,
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public required int MaxRetryOnDuplicate { get; init; }
    
    [Required]
    [Range(1, 60,
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public required int CacheExpireAfterMinutes { get; init; }
}