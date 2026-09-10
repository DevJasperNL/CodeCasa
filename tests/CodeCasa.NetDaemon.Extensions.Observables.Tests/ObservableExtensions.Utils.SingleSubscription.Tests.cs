using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables.Tests;

[TestClass]
public class ObservableExtensionsUtilsSingleSubscriptionTests
{
    private const string Unavailable = nameof(Unavailable);
    private const string TestEntityId = "domain.testEntity";

    private Entity _testEntity = null!;
    private Mock<IHaContext> _haContextMock = null!;
    private Subject<StateChange> _subject = null!;

    [TestInitialize]
    public void Initialize()
    {
        _haContextMock = new Mock<IHaContext>();
        _subject = new Subject<StateChange>();
        _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = Unavailable });
        _testEntity = new Entity(_haContextMock.Object, TestEntityId);
    }

    private void ChangeEntityState(string newState)
    {
        var old = _testEntity.EntityState;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState });
        _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
    }

    [TestMethod]
    public void RepeatWhenEntitiesBecomeAvailable_ColdSource_SubscribedOnceAndStillRepeats()
    {
        var subscriptions = 0;
        var source = Observable.Create<bool>(observer =>
        {
            subscriptions++;
            observer.OnNext(true);
            return Disposable.Empty;
        });
        var results = new List<bool>();

        source.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);
        Assert.AreEqual(1, subscriptions);
        CollectionAssert.AreEqual(new[] { true }, results);

        ChangeEntityState("Available");
        CollectionAssert.AreEqual(new[] { true, true }, results);
        Assert.AreEqual(1, subscriptions);
    }
}
