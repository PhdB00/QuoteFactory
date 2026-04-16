using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using QuoteFetcher.Application.Abstractions.Api;
using QuoteFetcher.Application.Quotes.GetQuotes;

namespace QuoteFetcher.Application.UnitTests.Quotes.GetQuotes;

[TestFixture]
public class GetQuotesStreamQueryHandlerTests
{
    private IOptions<QuoteApiSettings> settings = null!;
    private ILogger<GetQuotesStreamQueryHandler> logger = null!;
    private IQuoteApi api = null!;
    private IValidator<GetQuotesStreamQuery> validator = null!;
    private IQuoteHashSet quoteHashSet = null!;

    [SetUp]
    public void Setup()
    {
        settings = Substitute.For<IOptions<QuoteApiSettings>>();
        logger = new NullLogger<GetQuotesStreamQueryHandler>();
        api = Substitute.For<IQuoteApi>();
        validator = Substitute.For<IValidator<GetQuotesStreamQuery>>();
        quoteHashSet = Substitute.For<IQuoteHashSet>();
    }

    [Test]
    public void Should_Throw_ValidationException_When_StreamQuery_Is_Invalid()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(1, "funny");
        SetupSettings(maxConcurrentApiCalls: 2, maxRetryOnDuplicate: 0);
        validator.ValidateAsync(Arg.Any<GetQuotesStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Category", "Category is required")]));

        var handler = CreateHandler();

        // Act / Assert
        Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await foreach (var _ in handler.Handle(request, CancellationToken.None))
            {
                // do nothing
            }
        });
    }

    [Test]
    public async Task Should_Never_Exceed_Max_Concurrent_Api_Calls_Per_Request()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(6, "wise");
        SetupSettings(maxConcurrentApiCalls: 2, maxRetryOnDuplicate: 0);
        validator.ValidateAsync(Arg.Any<GetQuotesStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        quoteHashSet.Count.Returns(0);
        quoteHashSet.AddUniqueAndCount(Arg.Any<Quote>()).Returns((false, 0));

        var inFlight = 0;
        var observedMax = 0;
        api.GetQuoteAsync(Arg.Any<GetQuoteQueryParameters>())
            .Returns(async _ =>
            {
                var current = Interlocked.Increment(ref inFlight);
                InterlockedExtensions.Max(ref observedMax, current);
                await Task.Delay(50);
                Interlocked.Decrement(ref inFlight);
                return new Quote { Text = "duplicate" };
            });

        var handler = CreateHandler();

        // Act
        await foreach (var _ in handler.Handle(request, CancellationToken.None))
        {
            // do nothing
        }

        // Assert
        Assert.That(observedMax, Is.LessThanOrEqualTo(2));
    }

    [Test]
    public async Task Should_Not_Leak_Throttle_Capacity_Between_Requests()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(5, "wise");
        SetupSettings(maxConcurrentApiCalls: 1, maxRetryOnDuplicate: 0);
        validator.ValidateAsync(Arg.Any<GetQuotesStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        quoteHashSet.Count.Returns(0);
        quoteHashSet.AddUniqueAndCount(Arg.Any<Quote>()).Returns((false, 0));

        async Task<int> RunAndCaptureObservedMaxAsync()
        {
            var inFlight = 0;
            var observedMax = 0;
            api.GetQuoteAsync(Arg.Any<GetQuoteQueryParameters>())
                .Returns(async _ =>
                {
                    var current = Interlocked.Increment(ref inFlight);
                    InterlockedExtensions.Max(ref observedMax, current);
                    await Task.Delay(40);
                    Interlocked.Decrement(ref inFlight);
                    return new Quote { Text = Guid.NewGuid().ToString() };
                });

            var handler = CreateHandler();
            await foreach (var _ in handler.Handle(request, CancellationToken.None))
            {
                // do nothing
            }

            return observedMax;
        }

        // Act
        var firstRunMax = await RunAndCaptureObservedMaxAsync();
        var secondRunMax = await RunAndCaptureObservedMaxAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(firstRunMax, Is.LessThanOrEqualTo(1));
            Assert.That(secondRunMax, Is.LessThanOrEqualTo(1));
        });
    }

    [Test]
    public void Should_Propagate_OperationCanceledException_And_Keep_Already_Yielded_Quotes()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(6, "wise");
        SetupSettings(maxConcurrentApiCalls: 2, maxRetryOnDuplicate: 0);
        validator.ValidateAsync(Arg.Any<GetQuotesStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var realQuoteHashSet = new QuoteHashSet();
        var handler = new GetQuotesStreamQueryHandler(settings, logger, api, validator, realQuoteHashSet);
        using var cts = new CancellationTokenSource();

        var counter = 0;
        api.GetQuoteAsync(Arg.Any<GetQuoteQueryParameters>())
            .Returns(async _ =>
            {
                var number = Interlocked.Increment(ref counter);
                await Task.Delay(25);
                return new Quote { Text = $"quote-{number}" };
            });

        var received = new List<string>();

        // Act
        Assert.That(async () =>
        {
            await foreach (var quote in handler.Handle(request, cts.Token))
            {
                received.Add(quote);
                if (received.Count == 2)
                {
                    cts.Cancel();
                }
            }
        }, Throws.InstanceOf<OperationCanceledException>());

        // Assert
        Assert.That(received.Count, Is.EqualTo(2));
    }

    [Test]
    public void Should_Respect_PreCanceled_Token()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(3, "wise");
        SetupSettings(maxConcurrentApiCalls: 2, maxRetryOnDuplicate: 0);
        validator.ValidateAsync(Arg.Any<GetQuotesStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handler = CreateHandler();

        // Act / Assert
        Assert.That(async () =>
        {
            await foreach (var _ in handler.Handle(request, cts.Token))
            {
                // do nothing
            }
        }, Throws.InstanceOf<OperationCanceledException>());
    }

    private GetQuotesStreamQueryHandler CreateHandler() =>
        new(settings, logger, api, validator, quoteHashSet);

    private void SetupSettings(int maxConcurrentApiCalls, int maxRetryOnDuplicate)
    {
        settings.Value.Returns(new QuoteApiSettings
        {
            ApiHost = "http://localhost",
            MaxConcurrentApiCalls = maxConcurrentApiCalls,
            MaxRetryOnDuplicate = maxRetryOnDuplicate,
            CacheExpireAfterMinutes = 1
        });
    }

    private static class InterlockedExtensions
    {
        public static void Max(ref int target, int value)
        {
            while (true)
            {
                var snapshot = target;
                if (value <= snapshot)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref target, value, snapshot) == snapshot)
                {
                    return;
                }
            }
        }
    }
}
