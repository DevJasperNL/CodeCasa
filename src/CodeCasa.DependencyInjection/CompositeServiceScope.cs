using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.DependencyInjection;

public class CompositeServiceScope : IServiceScope, IAsyncDisposable
{
    public IServiceProvider ServiceProvider { get; }
    private readonly IServiceScope[] _childScopes;

    public CompositeServiceScope(IServiceScope[] childScopes)
    {
        _childScopes = childScopes;
        ServiceProvider = new CompositeServiceProvider(
            childScopes.Select(s => s.ServiceProvider).ToArray()
        );
    }

    public void Dispose()
    {
        foreach (var scope in _childScopes)
        {
            scope.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var scope in _childScopes)
        {
            if (scope is IAsyncDisposable ad)
            {
                await ad.DisposeAsync();
            }
            else
            {
                scope.Dispose();
            }
        }
    }
}