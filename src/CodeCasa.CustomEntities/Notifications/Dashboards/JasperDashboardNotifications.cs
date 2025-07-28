using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Notifications.InputSelect.Interact;

namespace CodeCasa.CustomEntities.Notifications.Dashboards;

public class JasperDashboardNotifications(
    [FromKeyedServices("input_select.jasper_notifications")] IInputSelectNotificationEntity inputSelectNotifications)
    : InputSelectNotificationEntity(inputSelectNotifications);