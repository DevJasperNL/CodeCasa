using System.Reactive.Linq;
using CodeCasa.Abstractions;


namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    /// <summary>
    /// A composite dimmer that combines multiple dimmers and emits true when any of them are actively dimming or brightening.
    /// </summary>
    public class CompositeDimmer : IDimmer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDimmer"/> class.
        /// </summary>
        /// <param name="dimmers">The collection of dimmers to combine. Must contain at least one dimmer.</param>
        /// <exception cref="ArgumentException">Thrown when the dimmers collection is null or empty.</exception>
        public CompositeDimmer(IEnumerable<IDimmer> dimmers)
        {
            dimmers = dimmers.ToArray();
            if (dimmers == null || !dimmers.Any())
                throw new ArgumentException("At least one dimmer must be provided.", nameof(dimmers));

            Dimming = dimmers
                .Select(d => d.Dimming)
                .CombineLatest(x => x.Any())
                .DistinctUntilChanged();

            Brightening = dimmers
                .Select(d => d.Brightening)
                .CombineLatest(x => x.Any())
                .DistinctUntilChanged();
        }

        /// <inheritdoc />
        public IObservable<bool> Dimming { get; }

        /// <inheritdoc />
        public IObservable<bool> Brightening { get; }
    }
}
