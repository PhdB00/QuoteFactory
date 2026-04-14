using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace QuoteFetcher.MenuSystem;

public interface IMenuItems
{
    IEnumerator<IMenuItem> GetEnumerator();
    IMenuItem GetItemByMenuCharacter(char menuChar);
    List<string> MenuKeys();
}

internal sealed class MenuItems : IMenuItems, IEnumerable<IMenuItem>
{
    private readonly FrozenDictionary<char, IMenuItem> items;
    
    public MenuItems(List<IMenuItem> menuItems)
    {
        items = menuItems.ToDictionary(x => char.ToLower(x.MenuChar))
            .ToFrozenDictionary();
    }
    
    public IEnumerator<IMenuItem> GetEnumerator()
    {
        return items.Select(x => x.Value).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public IMenuItem GetItemByMenuCharacter(char menuChar)
    {
        const char nullMenuKeyChar = '\0';
        
        return items.TryGetValue(char.ToLower(menuChar), out var item) 
            ? item 
            : items[nullMenuKeyChar];
    }

    public List<string> MenuKeys()
    {
        return items
            .Where(x => !x.Value.Hidden)
            .Select(x => x.Key.ToString()).ToList();
    }
}