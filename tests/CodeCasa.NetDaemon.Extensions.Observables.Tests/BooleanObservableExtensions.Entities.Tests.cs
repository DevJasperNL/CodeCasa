using System.Reactive.Subjects;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables.Tests;

[TestClass]
public class BooleanObservableExtensionsEntitiesTests
{
    private Mock<IHaContext> _haContextMock = null!;
    private IEntityCore[] _entities = null!;
    private Subject<bool> _subject = null!;

    [TestInitialize]
    public void Initialize()
    {
        _haContextMock = new Mock<IHaContext>();
        _entities =
        [
            new Entity(_haContextMock.Object, "light.a"),
            new Entity(_haContextMock.Object, "light.b")
        ];
        _subject = new Subject<bool>();
    }

    [TestMethod]
    public void BindToOnOff_MultipleEntities_TrueTurnsOn()
    {
        _subject.BindToOnOff(_entities);

        _subject.OnNext(true);

        _haContextMock.Verify(h => h.CallService("homeassistant", "turn_on",
            It.Is<ServiceTarget>(t => t.EntityIds!.Contains("light.a") && t.EntityIds!.Contains("light.b")),
            It.IsAny<object?>()), Times.Once);
        _haContextMock.Verify(h => h.CallService("homeassistant", "turn_off", It.IsAny<ServiceTarget>(), It.IsAny<object?>()), Times.Never);
    }

    [TestMethod]
    public void BindToOnOff_MultipleEntities_FalseTurnsOff()
    {
        _subject.BindToOnOff(_entities);

        _subject.OnNext(false);

        _haContextMock.Verify(h => h.CallService("homeassistant", "turn_off",
            It.Is<ServiceTarget>(t => t.EntityIds!.Contains("light.a") && t.EntityIds!.Contains("light.b")),
            It.IsAny<object?>()), Times.Once);
        _haContextMock.Verify(h => h.CallService("homeassistant", "turn_on", It.IsAny<ServiceTarget>(), It.IsAny<object?>()), Times.Never);
    }
}
