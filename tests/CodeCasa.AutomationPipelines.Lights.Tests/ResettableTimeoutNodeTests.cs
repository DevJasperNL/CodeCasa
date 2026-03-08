using CodeCasa.AutomationPipelines.Lights.Nodes;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;
using CodeCasa.Lights;
using Moq;

namespace CodeCasa.AutomationPipelines.Lights.Tests
{
    [TestClass]
    public sealed class ResettableTimeoutNodeTests
    {
        private TestScheduler _scheduler = null!;
        private Mock<IPipelineNode<LightTransition>> _childNodeMock = null!;
        private Subject<LightTransition?> _childOutputSubject = null!;
        private Subject<bool> _persistSubject = null!;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

        [TestInitialize]
        public void Initialize()
        {
            _scheduler = new TestScheduler();
            _childNodeMock = new Mock<IPipelineNode<LightTransition>>();
            _childOutputSubject = new Subject<LightTransition?>();
            _persistSubject = new Subject<bool>();

            _childNodeMock.Setup(x => x.OnNewOutput).Returns(_childOutputSubject);
            _childNodeMock.Setup(x => x.Output).Returns((LightTransition?)null);
        }

        [TestMethod]
        public void Constructor_InitializesWithChildNodeOutput()
        {
            // Arrange
            var expectedTransition = LightTransition.On();
            _childNodeMock.Setup(x => x.Output).Returns(expectedTransition);

            // Act
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _scheduler.AdvanceBy(1);

            // Assert
            Assert.AreEqual(expectedTransition, node.Output);
        }

        [TestMethod]
        public void TimeoutElapsed_TurnsOffLight()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Act
            _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);

            // Assert
            Assert.IsNotNull(node.Output);
            Assert.AreEqual(0, node.Output.LightParameters.Brightness);
        }

        [TestMethod]
        public void ChildOutputChange_RestartsTimer()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Advance halfway through the timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);

            // Act - trigger another output change to restart the timer
            var newTransition = new LightTransition { LightParameters = new LightParameters { Brightness = 50 } };
            _childOutputSubject.OnNext(newTransition);
            _scheduler.AdvanceBy(1);

            // Advance past the original timeout but not past the new one
            _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);

            // Assert - light should still be on
            Assert.AreEqual(newTransition, node.Output);
        }

        [TestMethod]
        public void ChildOutputChange_RestartsTimer_EventuallyTurnsOff()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Advance halfway through the timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);

            // Restart timer
            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Act - advance past the full timeout from the restart
            _scheduler.AdvanceBy(DefaultTimeout.Ticks);

            // Assert
            Assert.IsNotNull(node.Output);
            Assert.AreEqual(0, node.Output.LightParameters.Brightness);
        }

        [TestMethod]
        public void PersistTrue_StopsTimer()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Act - enable persist mode
            _persistSubject.OnNext(true);
            _scheduler.AdvanceBy(1);

            // Advance past the timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

            // Assert - light should still be on because persist is enabled
            Assert.AreNotEqual(0, node.Output?.LightParameters.Brightness);
        }

        [TestMethod]
        public void PersistFalse_RestartsTimer()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Enable persist
            _persistSubject.OnNext(true);
            _scheduler.AdvanceBy(1);

            // Advance past original timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

            // Act - disable persist
            _persistSubject.OnNext(false);
            _scheduler.AdvanceBy(1);

            // Advance past the new timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks);

            // Assert - light should be off now
            Assert.AreEqual(0, node.Output?.LightParameters.Brightness);
        }

        [TestMethod]
        public void PersistTrue_ChildOutputChange_DoesNotRestartTimer()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            // Enable persist first
            _persistSubject.OnNext(true);
            _scheduler.AdvanceBy(1);

            // Act - trigger child output change
            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Advance past timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

            // Assert - light should still be on (no timer running)
            Assert.AreNotEqual(0, node.Output?.LightParameters.Brightness);
        }

        [TestMethod]
        public void PersistDuplicateValues_IgnoredDueToDistinctUntilChanged()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Advance halfway
            _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);

            // Act - send duplicate persist values (should be ignored)
            _persistSubject.OnNext(false);
            _scheduler.AdvanceBy(1);
            _persistSubject.OnNext(false);
            _scheduler.AdvanceBy(1);

            // Advance remaining time from original start
            _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);

            // Assert - light should still be on because duplicate false doesn't restart timer
            Assert.AreNotEqual(0, node.Output?.LightParameters.Brightness);
        }

        [TestMethod]
        public void Dispose_CleansUpSubscriptions()
        {
            // Arrange
            var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);

            // Act
            node.Dispose();

            // Assert - no exceptions should occur when subjects complete after disposal
            _childOutputSubject.OnCompleted();
            _persistSubject.OnCompleted();
        }

        [TestMethod]
        public void MultipleChildOutputChanges_OnlyLastTimerIsActive()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            // Act - rapid fire multiple output changes
            _childOutputSubject.OnNext(LightTransition.On());
            _scheduler.AdvanceBy(1);
            _childOutputSubject.OnNext(new LightTransition { LightParameters = new LightParameters { Brightness = 50 } });
            _scheduler.AdvanceBy(1);
            _childOutputSubject.OnNext(new LightTransition { LightParameters = new LightParameters { Brightness = 100 } });
            _scheduler.AdvanceBy(1);

            // Advance to just before timeout
            _scheduler.AdvanceBy(DefaultTimeout.Ticks - 10);

            // Assert - light should still be on
            Assert.AreEqual(100, node.Output?.LightParameters.Brightness);

            // Advance past timeout
            _scheduler.AdvanceBy(20);

            // Assert - light should be off
            Assert.AreEqual(0, node.Output?.LightParameters.Brightness);
        }

        [TestMethod]
        public void NullChildOutput_HandledCorrectly()
        {
            // Arrange
            using var node = new ResettableTimeoutNode(
                _childNodeMock.Object,
                DefaultTimeout,
                _persistSubject,
                _scheduler);

            // Act
            _childOutputSubject.OnNext(null);
            _scheduler.AdvanceBy(1);

            // Assert
            Assert.IsNull(node.Output);
        }
    }
}
