using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace QuoteFetcher.MenuSystem.Invalid;

internal sealed class InvalidMenuItem(ISender sender) : IMenuItem
{
    public bool Hidden => true;
    public string Prompt => "";
    public char MenuChar => '\0';

    public Task Invoke(CancellationToken cancellationToken = default)
    {
        return sender.Send(new InvalidMenuRequest(), cancellationToken);
    }
}