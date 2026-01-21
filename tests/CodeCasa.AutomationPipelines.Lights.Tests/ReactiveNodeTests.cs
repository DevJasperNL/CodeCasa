using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using CodeCasa.Lights;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reactive.Subjects;
using ReactiveNodeClass = CodeCasa.AutomationPipelines.Lights.ReactiveNode.ReactiveNode;

namespace CodeCasa.AutomationPipelines.Lights.Tests
{
    [TestClass]
    public sealed class ReactiveNodeTests
    {
        private Mock<IServiceProvider> _serviceProviderMock = null!;
        private IScheduler _scheduler = null!;
        private ReactiveNodeFactory _reactiveNodeFactory = null!;
        private Mock<ILight> _lightMock = null!;
        private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
        private Mock<IServiceScope> _scopeMock = null!;
        private Mock<IServiceProvider> _scopedServiceProviderMock = null!;

        [TestInitialize]
        public void Initialize()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _scheduler = Scheduler.Immediate;
            _reactiveNodeFactory = new ReactiveNodeFactory(_serviceProviderMock.Object, _scheduler);

            var pipelineLoggerMock = new Mock<ILogger<Pipeline<LightTransition>>>();
            var lightPipelineFactory = new LightPipelineFactory(pipelineLoggerMock.Object, _serviceProviderMock.Object, _reactiveNodeFactory);

            _serviceProviderMock.Setup(x => x.GetService(typeof(LightPipelineFactory)))
                .Returns(lightPipelineFactory);

            var reactiveNodeLoggerMock = new Mock<ILogger<ReactiveNodeClass>>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<ReactiveNodeClass>)))
                .Returns(reactiveNodeLoggerMock.Object);

            _serviceProviderMock.Setup(x => x.GetService(typeof(IScheduler)))
                .Returns(_scheduler);

            _lightMock = new Mock<ILight>();
            _lightMock.Setup(l => l.Id).Returns("test_light");
            _lightMock.Setup(l => l.GetParameters()).Returns(new LightParameters());
            _lightMock.Setup(l => l.GetChildren()).Returns(Array.Empty<ILight>());

            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _scopeMock.As<IAsyncDisposable>();
            _scopedServiceProviderMock = new Mock<IServiceProvider>();

            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_scopeFactoryMock.Object);

            _scopeFactoryMock.Setup(x => x.CreateScope())
                .Returns(_scopeMock.Object);

            _scopeMock.Setup(x => x.ServiceProvider)
                .Returns(_scopedServiceProviderMock.Object);

            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(IScheduler)))
                .Returns(_scheduler);
        }

        [TestMethod]
        public void CreateNode()
        {
            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, _ => { });

            // Assert
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void On_Triggered_AppliesTransition()
        {
            // Arrange
            var triggerSubject = new Subject<int>();
            var expectedParameters = new LightParameters { Brightness = 50 };

            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.On(triggerSubject, expectedParameters);
            });

            LightTransition? lastOutput = null;
            node.OnNewOutput.Subscribe(output => lastOutput = output);

            triggerSubject.OnNext(1);

            // Assert
            Assert.IsNotNull(lastOutput);
            Assert.AreEqual(expectedParameters.Brightness, lastOutput.LightParameters.Brightness);
        }

        [TestMethod]
        public void On_Triggered_ActivatesNode_Generic()
        {
            // Arrange
            var triggerSubject = new Subject<int>();

            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.On<int, TestPipelineNode>(triggerSubject);
            });

            LightTransition? lastOutput = null;
            node.OnNewOutput.Subscribe(output => lastOutput = output);

            triggerSubject.OnNext(1);

            // Assert
            Assert.IsNotNull(lastOutput);
            Assert.AreEqual(100, lastOutput.LightParameters.Brightness);
        }

        [TestMethod]
        public void On_Triggered_ReplacesAndDisposesOldNode()
        {
            // Arrange
            var triggerSubject = new Subject<int>();

            var trackerMock = new Mock<ILifecycleTracker>();
            _serviceProviderMock.Setup(x => x.GetService(typeof(ILifecycleTracker)))
                .Returns(trackerMock.Object);
            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(ILifecycleTracker)))
                .Returns(trackerMock.Object);

            var createdIds = new List<Guid>();
            var disposedIds = new List<Guid>();

            trackerMock.Setup(x => x.Created(It.IsAny<Guid>()))
                .Callback<Guid>(id => createdIds.Add(id));
            trackerMock.Setup(x => x.Disposed(It.IsAny<Guid>()))
                .Callback<Guid>(id => disposedIds.Add(id));

            // Act
            _ = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.On<int, LifecycleTrackingPipelineNode>(triggerSubject);
            });

            // Trigger 1
            triggerSubject.OnNext(1);

            // Assert 1
            Assert.HasCount(1, createdIds, "Should have created one node");
            Assert.HasCount(0, disposedIds, "Should not have disposed any node yet");

            // Trigger 2
            triggerSubject.OnNext(2);

            // Assert 2
            Assert.HasCount(2, createdIds, "Should have created second node");
            Assert.HasCount(1, disposedIds, "Should have disposed one node");
            Assert.AreEqual(createdIds[0], disposedIds[0], "Should have disposed the first node");
        }

        [TestMethod]
        public void On_Triggered_ActivatesNode_WithContext()
        {
            // Arrange
            var triggerSubject = new Subject<int>();
            
            _serviceProviderMock.Setup(x => x.GetService(typeof(ILight)))
                .Returns(_lightMock.Object);
            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(ILight)))
                .Returns(_lightMock.Object);

            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.On<int, ContextAwarePipelineNode>(triggerSubject);
            });

            LightTransition? lastOutput = null;
            node.OnNewOutput.Subscribe(output => lastOutput = output);

            triggerSubject.OnNext(1);

            // Assert
            Assert.IsNotNull(lastOutput);
            Assert.AreEqual(100, lastOutput.LightParameters.Brightness);
        }

        public class TestPipelineNode : PipelineNode<LightTransition>
        {
            public TestPipelineNode()
            {
                Output = new LightParameters { Brightness = 100 }.AsTransition();
            }
        }

        public interface ILifecycleTracker
        {
            void Created(Guid id);
            void Disposed(Guid id);
        }

        public class LifecycleTrackingPipelineNode : PipelineNode<LightTransition>, IDisposable
        {
            private readonly ILifecycleTracker _tracker;
            public Guid Id { get; } = Guid.NewGuid();

            public LifecycleTrackingPipelineNode(ILifecycleTracker tracker)
            {
                _tracker = tracker;
                _tracker.Created(Id);
                Output = new LightParameters { Brightness = 100 }.AsTransition();
            }

            public void Dispose()
            {
                _tracker.Disposed(Id);
            }
        }

        public class ContextAwarePipelineNode : PipelineNode<LightTransition>
        {
            public ContextAwarePipelineNode(ILight light)
            {
                if (light.Id == "test_light")
                {
                    Output = new LightParameters { Brightness = 100 }.AsTransition();
                }
            }
        }

        [TestMethod]
        public void On_Triggered_CreatesScopedServiceProvider_AndDisposesIt()
        {
            // Arrange
            var triggerSubject = new Subject<int>();

            // Allow resolving IServiceProvider from itself
            _scopedServiceProviderMock.Setup(x => x.GetService(typeof(IServiceProvider)))
                .Returns(_scopedServiceProviderMock.Object);

            // Act
            _ = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.On<int, ScopedPipelineNode>(triggerSubject);
            });

            // Trigger 1
            triggerSubject.OnNext(1);

            // Assert 1
            _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once, "Scope should be created on trigger");
            _scopeMock.As<IAsyncDisposable>().Verify(x => x.DisposeAsync(), Times.Never, "Scope should not be disposed yet");

            // Trigger 2
            triggerSubject.OnNext(2);

            // Assert 2
            _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Exactly(2), "New scope should be created on second trigger");
            _scopeMock.As<IAsyncDisposable>().Verify(x => x.DisposeAsync(), Times.Once, "Old scope should be disposed");
        }

        [TestMethod]
        public void TurnOffWhen_Triggered_EmitsOffAndPassesThrough()
        {
            // Arrange
            var triggerSubject = new Subject<int>();
            
            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.TurnOffWhen(triggerSubject);
            });

            LightTransition? lastOutput = null;
            node.OnNewOutput.Subscribe(output => lastOutput = output);

            // Trigger
            triggerSubject.OnNext(1);

            // Assert
            Assert.IsNotNull(lastOutput);
            Assert.AreEqual(LightTransition.Off(), lastOutput);
            
            // Verify pass-through behavior
            var inputTransition = new LightParameters { Brightness = 100 }.AsTransition();
            node.Input = inputTransition;
            Assert.AreEqual(inputTransition, lastOutput);
        }

        [TestMethod]
        public void PassThroughOn_Triggered_PassesThrough()
        {
            // Arrange
            var triggerSubject = new Subject<int>();
            
            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.PassThroughOn(triggerSubject);
            });

            LightTransition? lastOutput = null;
            node.OnNewOutput.Subscribe(output => lastOutput = output);

            // Trigger
            triggerSubject.OnNext(1);

            // Verify pass-through behavior
            var inputTransition = new LightParameters { Brightness = 100 }.AsTransition();
            node.Input = inputTransition;
            Assert.AreEqual(inputTransition, lastOutput);
        }

        [TestMethod]
        public void Input_PropagatedToActiveNode()
        {
            // Arrange
            var triggerSubject = new Subject<int>();
            
            // Act
            var node = _reactiveNodeFactory.CreateReactiveNode(_lightMock.Object, config => 
            {
                config.On(triggerSubject, _ => new FactoryNode<LightTransition>(input => input == null
                    ? null
                    : input with { LightParameters = new LightParameters { Brightness = 100 } }));
            });

            LightTransition? lastOutput = null;
            node.OnNewOutput.Subscribe(output => lastOutput = output);

            triggerSubject.OnNext(1); // Activate the node

            // Act
            node.Input = new LightTransition { LightParameters = new LightParameters { Brightness = 10 } };

            // Assert
            Assert.IsNotNull(lastOutput);
            Assert.AreEqual(100, lastOutput.LightParameters.Brightness);
        }

        public class ScopedPipelineNode : PipelineNode<LightTransition>
        {
            public ScopedPipelineNode()
            {
                Output = new LightParameters { Brightness = 100 }.AsTransition();
            }
        }
    }
}
