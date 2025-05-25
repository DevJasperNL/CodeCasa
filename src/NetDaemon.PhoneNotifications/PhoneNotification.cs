namespace NetDaemon.PhoneNotifications;

public class PhoneNotification(string id, IDisposable disposable) : IDisposable
{
    public string Id { get; } = id;
    public void Dispose() => disposable.Dispose();
}