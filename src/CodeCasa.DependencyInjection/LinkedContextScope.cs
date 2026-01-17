using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.DependencyInjection;

internal sealed class LinkedContextScope(IServiceProvider composite, ServiceProvider context, IServiceScope parent)
    : IServiceScope, IAsyncDisposable
{
    public IServiceProvider ServiceProvider { get; } = composite;

    public void Dispose()
    {
        context.Dispose();
        parent.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
        if (parent is IAsyncDisposable ad)
        {
            await ad.DisposeAsync();
        }
        else
        {
            parent.Dispose();
        }
    }
}