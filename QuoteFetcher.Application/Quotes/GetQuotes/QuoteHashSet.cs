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
    // quotes, we need to manually track the quotes we receive and ensure that
    // for each request we return a distinct set to the user.
    //
    // Hashing and storing the Quote text will give us this ability.
    //
    // However, we will be working with multiple Tasks that get individual quotes,
    // and therefore we need a thread-safe way of tracking & evaluating.
    //
    // System.Collections.Concurrent lacks a ConcurrentHashSet but the thread-unsafe
    // HashSet implementation *does* provide a suitable hashing method (as opposed
    // to creating our own).
    //
    // This class stores a single HashSet in a ConcurrentDictionary so that access
    // to the HashSet will be thread-safe.
    
    private readonly ConcurrentDictionary<string, HashSet<string>> 
        hashSetDictionary = new(StringComparer.OrdinalIgnoreCase);

    private const string KeyOfHashSet = "hs";
    
    public QuoteHashSet()
    {
        hashSetDictionary.TryAdd(KeyOfHashSet, []);
    }

    public int Count => hashSetDictionary[KeyOfHashSet].Count;

    public (bool, int) AddUniqueAndCount(Quote quote)
    {
        return (hashSetDictionary[KeyOfHashSet].Add(quote.Text), Count);
    }
}