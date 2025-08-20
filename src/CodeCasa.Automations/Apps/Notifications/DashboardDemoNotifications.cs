using CodeCasa.CustomEntities.Automation.Notifications.Dashboards;
using CodeCasa.CustomEntities.Core.Events;
using NetDaemon.Notifications.InputSelect;
using CodeCasa.NetDaemon.Utilities.Extensions;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace CodeCasa.Automations.Apps.Notifications;

[NetDaemonApp]
internal class DashboardDemoNotifications
{
    private List<InputSelectNotification> _notifications = [];
    private int _manuallyAddedIndex;

    public DashboardDemoNotifications(
        IHaContext haContext,
        LivingRoomPanelDashboardNotifications livingRoomPanelNotifications)
    {
        haContext.Events.Filter(Events.LivingRoomPanelNotificationDemoEvent).Subscribe(_ =>
        {
            _notifications.Clear();
            _manuallyAddedIndex = 0;

            var clickToRemoveNotificationId = $"{nameof(DashboardDemoNotifications)}_ClickToRemove";
            var clickToUndoRemoveNotificationId = $"{nameof(DashboardDemoNotifications)}_ClickToUndoRemove";

            void AddClickToRemoveNotificationAction() =>
                _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
                {
                    Order = 900,
                    Message = "Paper bin will be picked up tomorrow.",
                    SecondaryMessage = "Click here if the bin is already outside.",
                    Icon = "Material.Filled.Delete",
                    Action = () =>
                    {
                        livingRoomPanelNotifications.RemoveNotification(clickToRemoveNotificationId);
                        _notifications = _notifications.Where(n => n.Id != clickToRemoveNotificationId).ToList();

                        _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
                        {
                            Message = "Notification removed!",
                            SecondaryMessage = "Click to undo remove.",
                            Icon = "Material.Filled.Undo",
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

            var addNotificationNotificationId = $"{nameof(DashboardDemoNotifications)}_Add";
            void AddNotificationNotificationAction() => _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
            {
                Order = 901,
                Message = "Curtains will close in 23 minutes.",
                SecondaryMessage = "Click here to close them early.",
                Icon = "Material.Filled.Curtains",
                Action = () =>
                {
                    _manuallyAddedIndex++;
                    var notificationId = Guid.NewGuid().ToString();
                    _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
                    {
                        Order = 902,
                        Message = $"Added Demo Notification ({_manuallyAddedIndex})",
                        SecondaryMessage = "Click to remove me.",
                        Icon = "Material.Filled.AutoAwesome",
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

            var clearNotificationId = $"{nameof(DashboardDemoNotifications)}_Clear";
            _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
            {
                Order = 903,
                Message = "Laundry is done!",
                Icon = "Material.Filled.LocalLaundryService",
                Action = () =>
                {
                    foreach (var notification in _notifications)
                    {
                        livingRoomPanelNotifications.RemoveNotification(notification);
                    }

                    _notifications.Clear();
                }
            }, clearNotificationId));

            var clearNotificationId2 = $"{nameof(DashboardDemoNotifications)}_Clear2";
            _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
            {
                Order = 903,
                Message = "Gerrit is sitting in front of the window and might want to come in!",
                Icon = "Material.Filled.Pets",
                Action = () =>
                {
                    foreach (var notification in _notifications)
                    {
                        livingRoomPanelNotifications.RemoveNotification(notification);
                    }

                    _notifications.Clear();
                }
            }, clearNotificationId2));

            _notifications.Add(livingRoomPanelNotifications.Notify(new LivingRoomPanelNotificationConfig
            {
                Order = 899,
                Message = "The grill is already on for 46 minutes.",
                SecondaryMessage = "Did you forget to turn it off?",
                Icon = "Material.Filled.OutdoorGrill",
                Action = () =>
                {
                    foreach (var notification in _notifications)
                    {
                        livingRoomPanelNotifications.RemoveNotification(notification);
                    }

                    _notifications.Clear();
                }
            }, clearNotificationId + "l"));
        });
    }
}