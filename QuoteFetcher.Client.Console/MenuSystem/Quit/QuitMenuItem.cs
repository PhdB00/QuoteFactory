using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace QuoteFetcher.MenuSystem.Quit;

internal sealed class QuitMenuItem(ISender sender) : IMenuItem
{
    public bool Hidden => false;
    public string Prompt => "Press q to quit";
    public char MenuChar => 'q';

    public Task Invoke(CancellationToken cancellationToken = default)
    {
        return sender.Send(new QuitMenuRequest(), cancellationToken);
    }
}