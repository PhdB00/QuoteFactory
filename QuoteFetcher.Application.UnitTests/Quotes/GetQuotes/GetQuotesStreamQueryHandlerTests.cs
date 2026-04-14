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
    private IOptions<QuoteApiSettings> settings;
    private ILogger<GetQuotesStreamQueryHandler> logger;
    private IQuoteApi api;
    private IValidator<GetQuotesStreamQuery> validator;
    private IQuoteHashSet quoteHashSet;
    
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
    public void Should_Handle_Validation_Failure_When_StreamQueryResult_Is_Invalid()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(1, "funny");

        settings.Value.Returns(new QuoteApiSettings
        {
            ApiHost = "http://localhost",
            MaxConcurrentApiCalls = 10,
            MaxRetryOnDuplicate = 10, 
            CacheExpireAfterMinutes = 1
        });
        validator.Validate(request)
            .Returns(new ValidationResult(
                [new ValidationFailure("Category", "Category is required")]));
            
        var handler = new GetQuotesStreamQueryHandler(
            settings,
            logger,
            api,
            validator,
            quoteHashSet);
        
        // Act
        var handlerTask = handler.Handle(request, CancellationToken.None);
        var enumerator = handlerTask.GetAsyncEnumerator();
        var task = enumerator.MoveNextAsync();
        
        // Assert
        Assert.That(task.IsCompletedSuccessfully, Is.False);
    }
    
    [Test]
    public async Task Should_Exhaust_Api_Calls()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(9, "wise");

        settings.Value.Returns(new QuoteApiSettings
        {
            ApiHost = "http://localhost",
            MaxConcurrentApiCalls = 10,
            MaxRetryOnDuplicate = 10, 
            CacheExpireAfterMinutes = 1
        });
        validator.ValidateAsync(request).Returns(new ValidationResult());

        api.GetQuoteAsync(Arg.Any<GetQuoteQueryParameters>())
            .Returns(new Quote { Text = "treated as a duplicate" });

        quoteHashSet.AddUniqueAndCount(Arg.Any<Quote>())
            .Returns((false, 0));
        
        var handler = new GetQuotesStreamQueryHandler(
            settings,
            logger,
            api,
            validator,
            quoteHashSet);
        
        // Act
        var handlerTask = handler.Handle(request, CancellationToken.None);

        var received = 0;
        await foreach (var unused in handlerTask)
        {
            received++;
        }
        
        // Assert
        Assert.That(received, Is.Zero);
    }
    
    [Test]
    public async Task Should_Return_Quotes()
    {
        // Arrange
        var request = new GetQuotesStreamQuery(9, "wise");

        settings.Value.Returns(new QuoteApiSettings
        {
            ApiHost = "http://localhost",
            MaxConcurrentApiCalls = 10,
            MaxRetryOnDuplicate = 10, 
            CacheExpireAfterMinutes = 1
        });
        validator.ValidateAsync(request).Returns(new ValidationResult());

        api.GetQuoteAsync(Arg.Any<GetQuoteQueryParameters>())
            .Returns(new Quote { Text = "wise words" });

        var number = 0;
        quoteHashSet.Count.Returns(number);
        quoteHashSet.AddUniqueAndCount(Arg.Any<Quote>())
            .Returns(_ =>
            {
                number++;
                quoteHashSet.Count.Returns(number);
                return (true, quoteHashSet.Count);
            });
        
        var handler = new GetQuotesStreamQueryHandler(
            settings,
            logger,
            api,
            validator,
            quoteHashSet);
        
        // Act
        var handlerTask = handler.Handle(request, CancellationToken.None);

        var received = 0;
        await foreach (var quote in handlerTask)
        {
            Assert.That(quote, Is.EqualTo("wise words"));
            received++;
        }
        
        // Assert
        Assert.That(received, Is.EqualTo(9));
    }
    
    
}