using CodeCasa.CustomEntities.Events;
using CodeCasa.CustomEntities.Notifications.Dashboards;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.Notifications.InputSelect;
using NetDaemon.Notifications.InputSelect.Config;
using System.Drawing;
using System.Reactive.Linq;

namespace CodeCasa.Automations.Apps.Notifications;

[NetDaemonApp]
internal class DemoNotifications
{
    private List<InputSelectNotification> _notifications = [];
    private int _manuallyAddedIndex;

    public DemoNotifications(
        IHaContext haContext,
        LivingRoomPanelDashboardNotifications livingRoomPanelNotifications)
    {
        haContext.Events.Where(e => e.EventType == Events.AddDemoNotificationEvent).Subscribe(_ =>
        {
            _notifications.Clear();
            _manuallyAddedIndex = 0;

            var clickToRemoveNotificationId = $"{nameof(DemoNotifications)}_ClickToRemove";
            var clickToUndoRemoveNotificationId = $"{nameof(DemoNotifications)}_ClickToUndoRemove";

            void AddClickToRemoveNotificationAction() =>
                _notifications.Add(livingRoomPanelNotifications.Notify(new InputSelectDashboardNotificationConfig
                {
                    Order = 900,
                    Message = "Demo Notification 1",
                    SecondaryMessage = "Click to remove me.",
                    Icon = "Material.Filled.AutoAwesome",
                    IconColor = Color.Yellow,
                    BadgeIcon = "Material.Filled.Delete",
                    BadgeIconColor = Color.Red,
                    Action = () =>
                    {
                        livingRoomPanelNotifications.RemoveNotification(clickToRemoveNotificationId);
                        _notifications = _notifications.Where(n => n.Id != clickToRemoveNotificationId).ToList();

                        _notifications.Add(livingRoomPanelNotifications.Notify(new InputSelectDashboardNotificationConfig
                        {
                            Message = "Notification removed!",
                            SecondaryMessage = "Click to undo remove.",
                            Icon = "Material.Filled.Undo",
                            IconColor = Color.Green,
                            Timeout = TimeSpan.FromSeconds(3),
                            Action = () =>
                            {
                                livingRoomPanelNotifications.RemoveNotification(clickToUndoRemoveNotificationId);
                                _notifications = _notifications.Where(n => n.Id != clickToUndoRemoveNotificationId).ToList();

                                AddClickToRemoveNotificationAction();
                            }
                        }, clickToUndoRemoveNotificationId));
                    }
                }, clickToRemoveNotificationId));
            AddClickToRemoveNotificationAction();

            var addNotificationNotificationId = $"{nameof(DemoNotifications)}_Add";
            void AddNotificationNotificationAction() => _notifications.Add(livingRoomPanelNotifications.Notify(new InputSelectDashboardNotificationConfig
            {
                Order = 901,
                Message = "Demo Notification 3",
                SecondaryMessage = "Click to add notification.",
                Icon = "Material.Filled.AutoAwesome",
                IconColor = Color.Yellow,
                BadgeIcon = _manuallyAddedIndex == 0 ? "Material.Filled.Add" : null,
                BadgeContent = _manuallyAddedIndex == 0 ? null : $"{_manuallyAddedIndex}",
                BadgeIconColor = Color.Green,
                Action = () =>
                {
                    _manuallyAddedIndex++;
                    var notificationId = Guid.NewGuid().ToString();
                    _notifications.Add(livingRoomPanelNotifications.Notify(new InputSelectDashboardNotificationConfig
                    {
                        Order = 902,
                        Message = $"Added Demo Notification ({_manuallyAddedIndex})",
                        SecondaryMessage = "Click to remove me.",
                        Icon = "Material.Filled.AutoAwesome",
                        IconColor = Color.Orange,
                        BadgeContent = $"{_manuallyAddedIndex}",
                        BadgeIconColor = Color.Blue,
                        Action = () =>
                        {
                            livingRoomPanelNotifications.RemoveNotification(notificationId);
                            _notifications = _notifications.Where(n => n.Id != notificationId).ToList();
                        }
                    }, notificationId));
                    AddNotificationNotificationAction();
                }
            }, addNotificationNotificationId));
            AddNotificationNotificationAction();

            var clearNotificationId = $"{nameof(DemoNotifications)}_Clear";
            _notifications.Add(livingRoomPanelNotifications.Notify(new InputSelectDashboardNotificationConfig
            {
                Order = 903,
                Message = "Demo Notification 2",
                SecondaryMessage = "Click to clear demo notifications.",
                Icon = "Material.Filled.AutoAwesome",
                IconColor = Color.Yellow,
                BadgeIcon = "Material.Filled.DeleteForever",
                BadgeIconColor = Color.Red,
                Action = () =>
                {
                    foreach (var notification in _notifications)
                    {
                        livingRoomPanelNotifications.RemoveNotification(notification);
                    }

                    _notifications.Clear();
                }
            }, clearNotificationId));
        });
    }
}