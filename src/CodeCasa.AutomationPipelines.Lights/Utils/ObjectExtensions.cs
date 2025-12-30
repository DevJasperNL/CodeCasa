
namespace CodeCasa.AutomationPipelines.Lights.Utils
{
    internal static class ObjectExtensions
    {
        public static async Task DisposeOrDisposeAsync(this object obj)
        {
            switch (obj)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}
