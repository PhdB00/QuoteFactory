using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace QuoteFetcher.MenuSystem.GetCategories;

internal sealed class GetCategoriesMenuItem(ISender sender) : IMenuItem
{
    public bool Hidden => false;
    public string Prompt => "Press c to get a list of categories";
    public char MenuChar => 'c';
    
    public Task Invoke(CancellationToken cancellationToken = default)
    {
        return sender.Send(new GetCategoriesMenuRequest(), cancellationToken);
    }
}
