using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace QuoteFetcher.MenuSystem.GetQuotes;

internal sealed class GetQuotesMenuItem(ISender sender) : IMenuItem
{
    public bool Hidden => false;
    public string Prompt => "Press r to get random quotes";
    public char MenuChar => 'r';

    public Task Invoke(CancellationToken cancellationToken = default)
    {
        return sender.Send(new GetQuotesMenuRequest(), cancellationToken);
    }
}