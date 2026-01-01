namespace CodeCasa.NetDaemon.Notifications.Phone.Config;

/// <summary>
/// Represents an action that can be performed directly from a notification (e.g., opening an app, URL, or executing a command).
/// </summary>
/// <param name="Action">The action delegate or command to execute when the user selects this action.</param>
/// <param name="Title">The display title of the action button.</param>
public record NotificationAction(Action Action, string Title)
{
    /// <summary>
    /// Optional URI to associate with the action.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// Converts the notification action into a serializable data object.
    /// </summary>
    public virtual object ToData(int index)
    {
        return new
        {
            action = $"{index}",
            title = Title,
            uri = Uri
        };
    }
}