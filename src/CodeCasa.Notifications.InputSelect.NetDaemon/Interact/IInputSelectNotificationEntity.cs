
using CodeCasa.Notifications.InputSelect.NetDaemon.Config;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Interact;

/// <summary>
/// Represents an entity that manages input select notifications.
/// Provides methods to create, update, and remove notifications associated with input select entities.
/// </summary>
public interface IInputSelectNotificationEntity
{
    /// <summary>
    /// Gets the entity ID of the input select this notification entity is associated with.
    /// </summary>
    string InputSelectEntityId { get; }

    /// <summary>
    /// Gets the entity ID of the optional input number associated with this notification entity.
    /// </summary>
    string? InputNumberEntityId { get; }

    /// <summary>
    /// Adds a new notification based on the specified configuration.
    /// </summary>
    /// <param name="notification">The notification configuration to use.</param>
    /// <returns>A handle to the notification.</returns>
    InputSelectNotification Notify(IInputSelectNotificationConfig notification);

    /// <summary>
    /// Updates an existing notification by replacing it with a new notification.
    /// </summary>
    /// <param name="notification">The new notification configuration.</param>
    /// <param name="notificationToReplace">The existing notification to replace.</param>
    /// <returns>A handle to the updated notification.</returns>
    InputSelectNotification Notify(IInputSelectNotificationConfig notification, InputSelectNotification notificationToReplace);

    /// <summary>
    /// Creates or updates a notification with the specified identifier.
    /// </summary>
    /// <param name="notification">The notification configuration to use.</param>
    /// <param name="id">The identifier of the notification.</param>
    /// <returns>A handle to the input select notification.</returns>
    InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id);

    /// <summary>
    /// Removes the specified input select notification.
    /// </summary>
    /// <param name="notificationToRemove">The notification to remove.</param>
    void RemoveNotification(InputSelectNotification notificationToRemove);

    /// <summary>
    /// Removes the input select notification with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the notification to remove.</param>
    void RemoveNotification(string id);
}