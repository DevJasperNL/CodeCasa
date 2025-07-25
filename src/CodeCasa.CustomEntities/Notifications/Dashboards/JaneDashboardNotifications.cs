using Microsoft.Extensions.DependencyInjection;
using NetDaemon.InputSelectNotifications.Interact;

namespace CodeCasa.CustomEntities.Notifications.Dashboards;

public class JaneDashboardNotifications(
    [FromKeyedServices("input_select.jane_notifications")] IInputSelectNotificationEntity inputSelectNotifications)
    : InputSelectNotificationEntity(inputSelectNotifications);