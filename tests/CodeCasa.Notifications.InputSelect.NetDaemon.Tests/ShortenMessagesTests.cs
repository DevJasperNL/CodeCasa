using CodeCasa.Notifications.InputSelect.NetDaemon.Helpers;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Tests
{
    [TestClass]
    public class ShortenMessagesTests
    {
        [TestMethod]
        public void SingleMessage_ShouldShortenCorrectly()
        {
            var (shortened, secondary) = MessageShortener.ShortenMessages("HelloWorld", null, 4);
            Assert.AreEqual("Hell..", shortened);
            Assert.IsNull(secondary);
        }

        [TestMethod]
        public void MultipleMessages_PrimaryLonger_ShouldShortenProportionally()
        {
            var (shortened1, shortened2) = MessageShortener.ShortenMessages("HelloWorld", "Hi", 4);

            Assert.AreEqual("Hell..", shortened1);
            Assert.AreEqual("..", shortened2);
        }

        [TestMethod]
        public void MultipleMessages_SecondaryLonger_ShouldShortenProportionally()
        {
            var (shortened1, shortened2) = MessageShortener.ShortenMessages("Short", "MuchLongerSecondary", 6);

            Assert.AreEqual("Sh..", shortened1);
            Assert.AreEqual("MuchLongerSe..", shortened2);
        }

        [TestMethod]
        public void MultipleMessages_EverythingRemoves()
        {
            var (shortened1, shortened2) = MessageShortener.ShortenMessages("Hi", "This is a test", 12);
            Assert.AreEqual("..", shortened1);
            Assert.AreEqual("..", shortened2);
        }

        [TestMethod]
        public void AmountZero_ShouldNotShorten()
        {
            var (shortened1, shortened2) = MessageShortener.ShortenMessages("Message", "Other", 0);
            Assert.AreEqual("Message", shortened1);
            Assert.AreEqual("Other", shortened2);
        }

        [TestMethod]
        public void SingleMessage_TooMuchRemoved_ShouldReturnExceptionIfTooShort()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                MessageShortener.ShortenMessages("Hi", null, 10)
            );
        }

        [TestMethod]
        public void MultipleMessages_TooMuchRemoved_ShouldReturnExceptionIfTooShort()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                    MessageShortener.ShortenMessages("Hi", "This is a test", 13)
            );
        }
    }
}
