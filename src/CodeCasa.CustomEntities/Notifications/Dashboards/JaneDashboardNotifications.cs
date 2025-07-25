using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Notifications.InputSelect.Interact;

namespace CodeCasa.CustomEntities.Notifications.Dashboards;

public class JaneDashboardNotifications(
    [FromKeyedServices("input_select.jane_notifications")] IInputSelectNotificationEntity inputSelectNotifications)
    : InputSelectNotificationEntity(inputSelectNotifications);