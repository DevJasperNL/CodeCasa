namespace NetDaemon.PhoneNotifications
{
    public class PhoneNotification : IDisposable
    {
        private readonly IDisposable _disposable;

        public PhoneNotification(string id, IDisposable disposable)
        {
            _disposable = disposable;
            Id = id;
        }

        public string Id { get; }
        public void Dispose() => _disposable.Dispose();
    }
}
