using System.Reactive.Subjects;
using CodeCasa.Lights.NetDaemon.Generated;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.Lights.NetDaemon.Tests;

[TestClass]
public sealed class NetDaemonLightAvailabilityTests
{
    private const string EntityId = "light.kitchen";

    [TestMethod]
    [DataRow("on", true)]
    [DataRow("off", true)]
    [DataRow("unavailable", false)]
    [DataRow("unknown", false)]
    public void IsAvailable_ReflectsEntityState(string state, bool expected)
    {
        var (light, _) = CreateLight(state);

        Assert.AreEqual(expected, light.IsAvailable);
    }

    [TestMethod]
    public void AvailabilityChanges_EmitsOnlyWhenAvailabilityChanges()
    {
        var (light, stateChanges) = CreateLight("on");
        var emitted = new List<bool>();
        light.AvailabilityChanges().Subscribe(emitted.Add);
        var entity = new Entity(Mock.Of<IHaContext>(), EntityId);

        stateChanges.OnNext(new StateChange(entity, State("on"), State("off")));
        stateChanges.OnNext(new StateChange(entity, State("off"), State("unavailable")));
        stateChanges.OnNext(new StateChange(entity, State("unavailable"), State("unknown")));
        stateChanges.OnNext(new StateChange(entity, State("unknown"), State("on")));

        CollectionAssert.AreEqual(new[] { false, true }, emitted);
    }

    private static (NetDaemonLight Light, Subject<StateChange> StateChanges) CreateLight(string state)
    {
        var stateChanges = new Subject<StateChange>();
        var haContext = new Mock<IHaContext>();
        haContext.Setup(h => h.GetState(EntityId)).Returns(State(state));
        haContext.Setup(h => h.StateAllChanges()).Returns(stateChanges);
        return (new NetDaemonLight(new LightEntity(haContext.Object, EntityId)), stateChanges);
    }

    private static EntityState State(string state) => new() { EntityId = EntityId, State = state };
}
