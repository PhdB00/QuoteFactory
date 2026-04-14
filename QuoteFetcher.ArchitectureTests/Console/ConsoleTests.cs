using QuoteFetcher.Abstractions.Messaging;
using QuoteFetcher.MenuSystem;
using NetArchTest.Rules;

namespace QuoteFetcher.ArchitectureTests.Console;

public class Tests : BaseTest
{
    [Test]
    public void MenuRequest_ShouldHave_NameEndingWith_MenuRequest()
    {
        var result = Types.InAssembly(ConsoleAssembly)
            .That()
            .ImplementInterface(typeof(IMenuRequest))
            .Or()
            .ImplementInterface(typeof(IMenuRequest<>))
            .Should()
            .HaveNameEndingWith("MenuRequest")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void MenuRequest_ShouldBe_Sealed()
    {
        var result = Types.InAssembly(ConsoleAssembly)
            .That()
            .ImplementInterface(typeof(IMenuRequest))
            .Or()
            .ImplementInterface(typeof(IMenuRequest<>))
            .Should()
            .BeSealed()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void MenuRequestHandler_ShouldHave_NameEndingWith_MenuRequestHandler()
    {
        var result = Types.InAssembly(ConsoleAssembly)
            .That()
            .ImplementInterface(typeof(IMenuRequestHandler<>))
            .Or()
            .ImplementInterface(typeof(IMenuRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("MenuRequestHandler")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void MenuRequestHandler_Should_NotBePublic()
    {
        var result = Types.InAssembly(ConsoleAssembly)
            .That()
            .ImplementInterface(typeof(IMenuRequestHandler<>))
            .Or()
            .ImplementInterface(typeof(IMenuRequestHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void MenuItem_ShouldHave_NameEndingWith_MenuItem()
    {
        var result = Types.InAssembly(ConsoleAssembly)
            .That()
            .ImplementInterface(typeof(IMenuItem))
            .Should()
            .HaveNameEndingWith("MenuItem")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
    
    [Test]
    public void IMenuItem_Should_NotBePublic()
    {
        var result = Types.InAssembly(ConsoleAssembly)
            .That()
            .ImplementInterface(typeof(IMenuItem))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
}