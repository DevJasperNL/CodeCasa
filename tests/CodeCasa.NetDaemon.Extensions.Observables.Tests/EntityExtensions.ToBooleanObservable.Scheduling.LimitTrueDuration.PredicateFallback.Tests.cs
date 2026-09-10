namespace CodeCasa.NetDaemon.Extensions.Observables.Tests;

[TestClass]
public class EntityExtensionsToBooleanObservableSchedulingLimitTrueDurationPredicateFallbackTests
    : EntityExtensionsToBooleanObservableSchedulingEntityTestsSetup
{
    private const string Open = nameof(Open);

    [TestMethod]
    public void LimitTrueDuration_Predicate_PredicateInitiallyFalse_UsesPredicateAfterSubscription()
    {
        // The entity is "On", which the predicate does not match, so the fallback path is taken.
        var observable = TestEntity.LimitTrueDuration(TimeSpan.FromTicks(10), s => s.State == Open, Scheduler, LastChanged.ToUniversalTime);
        var results = new List<bool>();

        observable.Subscribe(results.Add);
        CollectionAssert.AreEqual(new[] { false }, results);

        ChangeEntityState(Open, LastChanged);
        CollectionAssert.AreEqual(new[] { false, true }, results);

        Scheduler.AdvanceBy(11);
        CollectionAssert.AreEqual(new[] { false, true, false }, results);
    }

    [TestMethod]
    public void LimitTrueDuration_Predicate_PredicateInitiallyFalse_IgnoresNonMatchingStates()
    {
        var observable = TestEntity.LimitTrueDuration(TimeSpan.FromTicks(10), s => s.State == Open, Scheduler, LastChanged.ToUniversalTime);
        var results = new List<bool>();
        observable.Subscribe(results.Add);

        ChangeEntityState(Off, LastChanged);
        ChangeEntityState(On, LastChanged);

        CollectionAssert.AreEqual(new[] { false }, results);
    }
}
