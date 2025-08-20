
using System.Reactive.Linq;

namespace CodeCasa.CustomEntities.Automation.People;

public class PeopleEntities(Jane jane, Jasper jasper)
{
    public Jane Jane { get; } = jane;
    public Jasper Jasper { get; } = jasper;

    public IEnumerable<CompositePersonEntity> All { get; } = [jane, jasper];

    public IObservable<bool> OnLastPersonToAsleepOrAwayObservable()
    {
        return All.Select(p => p.CreateStateChangeObservable()).CombineLatest()
            .Select(tuple =>
            {
                var prevAnyAwake = tuple.Any(change => change.Old == PersonStates.Awake);
                var currentAnyAwake = tuple.Any(change => change.New == PersonStates.Awake);
                return prevAnyAwake && !currentAnyAwake;
            }).DistinctUntilChanged();
    }
}