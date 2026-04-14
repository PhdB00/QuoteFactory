using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace QuoteFetcher.MenuSystem.ShowMenu;

internal sealed class ShowMenuItem(ISender sender) : IMenuItem
{
    public bool Hidden => false;
    public string Prompt => "Press ? to get these instructions";
    public char MenuChar => '?';

    public Task Invoke(CancellationToken cancellationToken = default)
    {
        return sender.Send(new ShowMenuRequest(), cancellationToken);
    }
}