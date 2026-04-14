using QuoteFetcher.MenuSystem;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem;

[TestFixture]
public class MenuItemsTests
{
    private static List<IMenuItem> MockedMenuItems()
    {
        var item1 = Substitute.For<IMenuItem>();
        var item2 = Substitute.For<IMenuItem>();
        var item3 = Substitute.For<IMenuItem>();
        var item4 = Substitute.For<IMenuItem>();
        
        item1.MenuChar.Returns('a');
        item2.MenuChar.Returns('b');
        item3.MenuChar.Returns('c');
        item4.MenuChar.Returns('\0');
        
        return [item1, item2, item3, item4];
    }
    
    [Test]
    public void Should_Enumerate_MenuItems()
    {
        // Arrange
        var menuItems = new MenuItems(MockedMenuItems());
        
        // Act
        var count = menuItems.Count();

        // Assert
        Assert.That(count, Is.EqualTo(4));
    }
    
    [Test]
    public void Should_Get_Valid_MenuItem()
    {
        // Arrange
        var menuItems = new MenuItems(MockedMenuItems());
        
        // Act
        var menuItem = menuItems.GetItemByMenuCharacter('a');

        // Assert
        Assert.That(menuItem.MenuChar, Is.EqualTo('a'));
    }
    
    [Test]
    public void Should_Get_Invalid_MenuItem()
    {
        // Arrange
        var menuItems = new MenuItems(MockedMenuItems());
        
        // Act
        var menuItem = menuItems.GetItemByMenuCharacter('x');

        // Assert
        Assert.That(menuItem.MenuChar, Is.EqualTo('\0'));
    }
}