namespace CodeCasa.Notifications.InputSelect.NetDaemon.Helpers
{
    /// <summary>
    /// Provides helper methods for shortening text messages while preserving readability.
    /// </summary>
    /// <remarks>
    /// The <see cref="MessageShortener"/> class is designed to truncate one or two strings
    /// by a specified number of characters, ensuring each shortened message remains valid
    /// and ends with an ellipsis ("..") to indicate truncation.
    /// </remarks>
    public static class MessageShortener
    {
        /// <summary>
        /// Shortens one or two messages by a specified total amount of characters.
        /// </summary>
        /// <param name="message">The primary message to shorten.</param>
        /// <param name="secondaryMessage">An optional secondary message to shorten along with the primary message.</param>
        /// <param name="amount">The total number of characters to remove across both messages.</param>
        /// <returns>
        /// A tuple containing the shortened primary and secondary messages.
        /// If only one message is provided, only that message is shortened.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified amount to shorten exceeds the possible shortening length.
        /// </exception>
        /// <remarks>
        /// When both messages are provided, the shortening amount is distributed proportionally
        /// between them based on their lengths. Each shortened message has ".." appended to indicate truncation.
        /// </remarks>
        public static (string, string?) ShortenMessages(string message, string? secondaryMessage, int amount)
        {
            if (amount <= 0)
            {
                return (message, secondaryMessage);
            }
            if (string.IsNullOrEmpty(secondaryMessage))
            {
                if (amount > message.Length - 2)
                {
                    throw new ArgumentException("Not possible to short messages by this amount.");
                }
                return (message.Substring(0, message.Length - amount - 2) + "..", secondaryMessage);
            }

            var longest = message.Length >= secondaryMessage.Length ? message : secondaryMessage;
            var shortest = message.Length >= secondaryMessage.Length ? secondaryMessage : message;
            var amountToShorten1 =
                    (int)Math.Ceiling(amount * (longest.Length / (double)(longest.Length + shortest.Length)));
            var amountToShorten2 = amount - amountToShorten1;

            var overflow = Math.Min(0, shortest.Length - amountToShorten2 - 2);
            amountToShorten1 -= overflow;
            amountToShorten2 += overflow;

            if (amountToShorten1 > longest.Length - 2)
            {
                throw new ArgumentException("Not possible to short messages by this amount.");
            }

            var newLongest = amountToShorten1 == longest.Length ? longest : longest.Substring(0, longest.Length - amountToShorten1 - 2) + "..";
            var newShortest = amountToShorten2 == shortest.Length ? shortest : shortest.Substring(0, shortest.Length - amountToShorten2 - 2) + "..";

            return message.Length >= secondaryMessage.Length ? (newLongest, newShortest) : (newShortest, newLongest);
        }
    }
}
