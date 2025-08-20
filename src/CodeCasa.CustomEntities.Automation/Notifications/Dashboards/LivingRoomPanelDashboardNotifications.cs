using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Notifications.InputSelect.Interact;

namespace CodeCasa.CustomEntities.Automation.Notifications.Dashboards;

public class LivingRoomPanelDashboardNotifications(
    [FromKeyedServices("input_select.living_room_panel_notifications")] IInputSelectNotificationEntity inputSelectNotifications)
    : InputSelectNotificationEntity(inputSelectNotifications);