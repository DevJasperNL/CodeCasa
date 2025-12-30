namespace CodeCasa.AutomationPipelines.Lights
{
    /// <summary>
    /// A composite disposable that manages both synchronous and asynchronous disposable resources.
    /// </summary>
    public sealed class CompositeAsyncDisposable : IAsyncDisposable
    {
        private readonly List<IAsyncDisposable> _asyncDisposables = new();
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        /// <summary>
        /// Adds an asynchronous disposable resource to be disposed when this composite is disposed.
        /// </summary>
        /// <param name="asyncDisposable">The asynchronous disposable resource to add.</param>
        public void Add(IAsyncDisposable asyncDisposable)
        {
            _asyncDisposables.Add(asyncDisposable);
        }

        /// <summary>
        /// Adds a synchronous disposable resource to be disposed when this composite is disposed.
        /// </summary>
        /// <param name="disposable">The synchronous disposable resource to add.</param>
        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Adds a range of synchronous disposable resources to be disposed when this composite is disposed.
        /// </summary>
        /// <param name="disposables">The collection of synchronous disposable resources to add.</param>
        public void AddRange(IEnumerable<IDisposable> disposables)
        {
            _disposables.AddRange(disposables);
        }

        /// <summary>
        /// Disposes all managed asynchronous and synchronous resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            foreach (var d in _asyncDisposables)
            {
                await d.DisposeAsync();
            }
            foreach (var d in _disposables)
            {
                d.Dispose();
            }
        }
    }
}
