using FluentValidation;
using QuoteFetcher.Application.Abstractions.Messaging;
using NetArchTest.Rules;

namespace QuoteFetcher.ArchitectureTests.Application;

public class Tests : BaseTest
{
    [Test]
    public void Query_ShouldHave_NameEndingWith_Query()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery))
            .Or()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void QueryHandler_ShouldHave_NameEndingWith_QueryHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void QueryHandler_Should_NotBePublic()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void StreamQuery_ShouldHave_NameEndingWith_StreamQuery()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IStreamQuery<>))
            .Should()
            .HaveNameEndingWith("StreamQuery")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void StreamQueryHandler_ShouldHave_NameEndingWith_StreamQueryHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IStreamQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void StreamQueryHandler_Should_NotBePublic()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IStreamQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }

    [Test]
    public void Validator_ShouldHave_NameEndingWith_Validator()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();
        
        Assert.That(result.IsSuccessful, Is.True);
    }

    [Test]
    public void Validator_Should_NotBePublic()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IValidator<>))
            .Should()
            .NotBePublic()
            .GetResult();
        
        Assert.That(result.IsSuccessful, Is.True);
    }
}