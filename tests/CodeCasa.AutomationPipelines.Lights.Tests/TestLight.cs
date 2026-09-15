using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.Abstractions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

public sealed class TestLight(string id, params ILight[] children) : ILight, ILightAvailability
{
    private readonly Subject<bool> _availabilityChanges = new();

    public string Id => id;
    public bool IsAvailable { get; private set; } = true;

    public IObservable<bool> AvailabilityChanges() => _availabilityChanges;

    public void SetAvailable(bool isAvailable)
    {
        IsAvailable = isAvailable;
        _availabilityChanges.OnNext(isAvailable);
    }

    public List<LightTransition> Applied { get; } = new();
    public LightParameters Current { get; set; } = LightParameters.Off();

    public LightParameters GetParameters() => Current;

    public void ApplyTransition(LightTransition transition)
    {
        lock (Applied)
        {
            Applied.Add(transition);
        }
        Current = transition.LightParameters;
    }

    public ILight[] GetChildren() => children;

    public IObservable<StateChange<ILight, LightParameters>> StateChanges() =>
        Observable.Never<StateChange<ILight, LightParameters>>();

    public IObservable<StateChange<ILight, LightParameters>> StateChangesWithCurrent() =>
        Observable.Never<StateChange<ILight, LightParameters>>();

    public DateTime? LastChangedUtc { get; set; }
    public DateTime? LastUpdatedUtc => null;

    public int CountApplied(int brightness)
    {
        lock (Applied)
        {
            return Applied.Count(t => t.LightParameters.Brightness == brightness);
        }
    }
}
