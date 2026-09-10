using CodeCasa.AutomationPipelines;
using CodeCasa.Lights;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights.Tests;

[TestClass]
public sealed class LightNotificationManagerTests
{
    private sealed class TestConfig(int priority) : ILightNotificationConfig
    {
        public int Priority { get; set; } = priority;
        public Func<IServiceProvider, IPipelineNode<LightTransition>?> CreateFactory() => _ => null;
    }

    private LightNotificationManager _manager = null!;
    private List<ILightNotificationConfig?> _emitted = null!;

    [TestInitialize]
    public void Initialize()
    {
        _manager = new LightNotificationManager();
        _emitted = new List<ILightNotificationConfig?>();
        _manager.LightNotifications().Subscribe(_emitted.Add);
    }

    [TestMethod]
    public void Notify_ReplaceDisplayedWithLowerPriority_EmitsNextHighest()
    {
        var a = new TestConfig(5);
        var b = new TestConfig(3);
        var replacement = new TestConfig(1);
        _manager.Notify(a, "id1");
        _manager.Notify(b, "id2");

        _manager.Notify(replacement, "id1");

        Assert.AreSame(b, _emitted.Last());
    }

    [TestMethod]
    public void Notify_HigherPriority_EmitsNew()
    {
        var a = new TestConfig(1);
        var b = new TestConfig(5);
        _manager.Notify(a, "id1");

        _manager.Notify(b, "id2");

        Assert.AreSame(b, _emitted.Last());
    }

    [TestMethod]
    public void Notify_LowerPriority_DoesNotEmit()
    {
        var a = new TestConfig(5);
        _manager.Notify(a, "id1");
        var count = _emitted.Count;

        _manager.Notify(new TestConfig(1), "id2");

        Assert.HasCount(count, _emitted);
        Assert.AreSame(a, _emitted.Last());
    }

    [TestMethod]
    public void Notify_SamePriority_NewestWins()
    {
        var a = new TestConfig(5);
        var b = new TestConfig(5);
        _manager.Notify(a, "id1");

        _manager.Notify(b, "id2");

        Assert.AreSame(b, _emitted.Last());
    }

    [TestMethod]
    public void Remove_Displayed_EmitsNextHighest()
    {
        var a = new TestConfig(5);
        var b = new TestConfig(3);
        _manager.Notify(a, "id1");
        _manager.Notify(b, "id2");

        Assert.IsTrue(_manager.Remove("id1"));

        Assert.AreSame(b, _emitted.Last());
    }

    [TestMethod]
    public void Remove_NotDisplayed_DoesNotEmit()
    {
        var a = new TestConfig(5);
        _manager.Notify(a, "id1");
        _manager.Notify(new TestConfig(3), "id2");
        var count = _emitted.Count;

        _manager.Remove("id2");

        Assert.HasCount(count, _emitted);
    }

    [TestMethod]
    public void Remove_Last_EmitsNull()
    {
        _manager.Notify(new TestConfig(5), "id1");

        _manager.Remove("id1");

        Assert.IsNull(_emitted.Last());
    }

    [TestMethod]
    public void Remove_Unknown_ReturnsFalse()
    {
        Assert.IsFalse(_manager.Remove("missing"));
    }

    [TestMethod]
    public void Dispose_ReturnedNotification_Removes()
    {
        var notification = _manager.Notify(new TestConfig(5), "id1");

        notification.Dispose();

        Assert.IsNull(_emitted.Last());
    }
}
