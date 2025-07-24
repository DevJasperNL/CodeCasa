namespace NetDaemon.InputSelectNotifications.Helpers
{
    internal static class MessageShortener
    {// todo: test
        public static (string, string?) ShortenMessages(string message, string? secondaryMessage, int amount)
        {
            if (string.IsNullOrEmpty(secondaryMessage))
            {
                return (message.Substring(0, message.Length - amount - 2) + "..", secondaryMessage);
            }

            int amountToShorten1, amountToShorten2;
            if (message.Length > secondaryMessage.Length)
            {
                amountToShorten1 =
                    (int)Math.Ceiling(amount * (message.Length / (double)(message.Length + secondaryMessage.Length)));
                amountToShorten2 = amount - amountToShorten1;

                return (
                    message.Substring(0, message.Length - amountToShorten1 - 2) + "..",
                    secondaryMessage.Substring(0, secondaryMessage.Length - amountToShorten2 - 2) + "..");
            }

            amountToShorten1 = (int)Math.Ceiling(secondaryMessage.Length / (double)(message.Length + secondaryMessage.Length));
            amountToShorten2 = amount - amountToShorten1;

            return (
                message.Substring(0, message.Length - amountToShorten2 - 2) + "..",
                secondaryMessage.Substring(0, secondaryMessage.Length - amountToShorten1 - 2) + "..");
        }
    }
}
