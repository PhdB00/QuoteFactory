using System.Threading;
using System.Threading.Tasks;

namespace QuoteFetcher.MenuSystem;

public interface IMenuItem
{
    bool Hidden { get; }
    string Prompt { get; }
    char MenuChar { get; }
    
    Task Invoke(CancellationToken cancellationToken = default);
}
