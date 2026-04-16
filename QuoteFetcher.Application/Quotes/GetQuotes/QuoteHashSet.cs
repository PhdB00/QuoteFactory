using System.Collections.Concurrent;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

public interface IQuoteHashSet
{
    int Count { get; }
    (bool, int) AddUniqueAndCount(Quote quote);
}

internal sealed class QuoteHashSet : IQuoteHashSet
{
    // As the API /Quote endpoint offers no options to exclude previously received
    // quotes, we need to manually track unique quote text values for each request.
    // We use a ConcurrentDictionary as a set-like structure where dictionary keys
    // are the unique quote texts.
    private readonly ConcurrentDictionary<string, byte> uniqueQuotes =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => uniqueQuotes.Count;

    public (bool, int) AddUniqueAndCount(Quote quote)
    {
        var wasAdded = uniqueQuotes.TryAdd(quote.Text, 0);
        return (wasAdded, Count);
    }
}
