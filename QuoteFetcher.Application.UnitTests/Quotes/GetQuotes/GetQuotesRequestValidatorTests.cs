using FluentValidation.TestHelper;
using MediatR;
using NSubstitute;
using QuoteFetcher.Application.Abstractions;
using QuoteFetcher.Application.Categories.GetCategories;
using QuoteFetcher.Application.Quotes.GetQuotes;

namespace QuoteFetcher.Application.UnitTests.Quotes.GetQuotes;

[TestFixture]
public class GetQuotesRequestValidatorTests
{
    [Test]
    public void Constructor_Should_Not_Call_Category_Query()
    {
        // Arrange
        var sender = Substitute.For<ISender>();

        // Act
        _ = new GetQuotesRequestValidator(sender);

        // Assert
        _ = sender.DidNotReceive().Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>());
    }

    [TestCase(1)]
    [TestCase(9)]
    public async Task Should_Pass_Number_Range_Validation_For_Valid_Values(int numberOfQuotes)
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<string>>(["animal", "science"]));

        var validator = new GetQuotesRequestValidator(sender);

        // Act
        var result = await validator.TestValidateAsync(new GetQuotesStreamQuery(numberOfQuotes, "animal"));

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NumberOfQuotes);
    }

    [TestCase(0)]
    [TestCase(10)]
    public async Task Should_Fail_Number_Range_Validation_For_Invalid_Values(int numberOfQuotes)
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<string>>(["animal", "science"]));

        var validator = new GetQuotesRequestValidator(sender);

        // Act
        var result = await validator.TestValidateAsync(new GetQuotesStreamQuery(numberOfQuotes, "animal"));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NumberOfQuotes);
    }

    [Test]
    public async Task Should_Accept_Empty_Category()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var validator = new GetQuotesRequestValidator(sender);

        // Act
        var result = await validator.TestValidateAsync(new GetQuotesStreamQuery(2, string.Empty));

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
        _ = sender.DidNotReceive().Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Should_Pass_When_Category_Exists()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<string>>(["animal", "science"]));

        var validator = new GetQuotesRequestValidator(sender);

        // Act
        var result = await validator.TestValidateAsync(new GetQuotesStreamQuery(2, "science"));

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Test]
    public async Task Should_Fail_With_Generic_Message_When_Category_Does_Not_Exist()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<string>>(["animal", "science"]));

        var validator = new GetQuotesRequestValidator(sender);

        // Act
        var result = await validator.TestValidateAsync(new GetQuotesStreamQuery(2, "sport"));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category is invalid.");
    }

    [Test]
    public async Task Should_Fail_With_Generic_Message_When_Category_Lookup_Throws()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetCategoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<IReadOnlyList<string>>>>(_ => throw new HttpRequestException("Lookup failed"));

        var validator = new GetQuotesRequestValidator(sender);

        // Act
        var result = await validator.TestValidateAsync(new GetQuotesStreamQuery(2, "sport"));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category is invalid.");
    }
}
