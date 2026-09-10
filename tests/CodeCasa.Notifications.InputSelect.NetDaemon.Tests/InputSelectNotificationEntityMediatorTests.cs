using CodeCasa.Notifications.InputSelect.NetDaemon.Config;
using CodeCasa.Notifications.InputSelect.NetDaemon.Interact;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Tests;

[TestClass]
public sealed class InputSelectNotificationEntityMediatorTests
{
    private sealed class Config : IInputSelectNotificationConfig
    {
        public TimeSpan? Timeout { get; set; }
        public Action? Action { get; set; }
        public int? Order { get; set; }
        public string ToInputSelectOptionString() => "option";
    }

    [TestMethod]
    public void Notify_BeforeSubscribe_ReplayedOnSubscribe()
    {
        var mediator = new InputSelectNotificationEntityMediator("input_select.test", null);
        var config = new Config();
        mediator.Notify(config, "1");

        var received = new List<NotificationCommand>();
        mediator.Commands.Subscribe(received.Add);

        Assert.HasCount(1, received);
        var notify = received[0] as NotifyCommand;
        Assert.IsNotNull(notify);
        Assert.AreEqual("1", notify.Id);
        Assert.AreSame(config, notify.Config);
    }

    [TestMethod]
    public void Notify_SameIdBeforeSubscribe_KeepsLatestOnly()
    {
        var mediator = new InputSelectNotificationEntityMediator("input_select.test", null);
        var first = new Config();
        var second = new Config();
        mediator.Notify(first, "1");
        mediator.Notify(second, "1");

        var received = new List<NotificationCommand>();
        mediator.Commands.Subscribe(received.Add);

        Assert.HasCount(1, received);
        Assert.AreSame(second, ((NotifyCommand)received[0]).Config);
    }

    [TestMethod]
    public void Remove_BeforeSubscribe_DropsPendingNotification()
    {
        var mediator = new InputSelectNotificationEntityMediator("input_select.test", null);
        var notification = mediator.Notify(new Config(), "1");
        notification.Dispose();

        var received = new List<NotificationCommand>();
        mediator.Commands.Subscribe(received.Add);

        Assert.IsEmpty(received);
    }

    [TestMethod]
    public void Notify_AfterSubscribe_DeliveredDirectly()
    {
        var mediator = new InputSelectNotificationEntityMediator("input_select.test", null);
        var received = new List<NotificationCommand>();
        mediator.Commands.Subscribe(received.Add);

        mediator.Notify(new Config(), "1");
        mediator.RemoveNotification("1");

        Assert.HasCount(2, received);
        Assert.IsInstanceOfType<NotifyCommand>(received[0]);
        Assert.IsInstanceOfType<RemoveCommand>(received[1]);
    }
}
