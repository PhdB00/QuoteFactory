using QuoteFetcher.Application.Quotes.GetQuotes;

namespace QuoteFetcher.Application.UnitTests.Quotes.GetQuotes;

[TestFixture]
public class QuoteHashSetTests
{
    [Test]
    public void Should_Add_Unique_Quote_And_Count_It()
    {
        // Arrange
        var quoteHashSet = new QuoteHashSet();

        // Act
        var (wasAdded, count) = quoteHashSet.AddUniqueAndCount(new Quote { Text = "A wise quote" });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(wasAdded, Is.True);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(quoteHashSet.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Should_Not_Add_Duplicate_Quote_And_Count_Remains_One()
    {
        // Arrange
        var quoteHashSet = new QuoteHashSet();

        // Act
        quoteHashSet.AddUniqueAndCount(new Quote { Text = "A wise quote" });
        var (wasAdded, count) = quoteHashSet.AddUniqueAndCount(new Quote { Text = "A wise quote" });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(wasAdded, Is.False);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(quoteHashSet.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Should_Handle_High_Contention_Without_Races()
    {
        // Arrange
        var quoteHashSet = new QuoteHashSet();
        var tasks = Enumerable.Range(1, 500)
            .Select(i => Task.Run(() => quoteHashSet.AddUniqueAndCount(new Quote { Text = $"quote-{i}" })))
            .ToArray();

        // Act
        await Task.WhenAll(tasks);

        // Assert
        Assert.That(quoteHashSet.Count, Is.EqualTo(500));
    }

    [Test]
    public void Should_Treat_Quote_Text_As_Case_Insensitive()
    {
        // Arrange
        var quoteHashSet = new QuoteHashSet();

        // Act
        var first = quoteHashSet.AddUniqueAndCount(new Quote { Text = "Case Test" });
        var second = quoteHashSet.AddUniqueAndCount(new Quote { Text = "case test" });

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(first.Item1, Is.True);
            Assert.That(second.Item1, Is.False);
            Assert.That(quoteHashSet.Count, Is.EqualTo(1));
        });
    }
}
