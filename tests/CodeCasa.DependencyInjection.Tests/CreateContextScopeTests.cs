using CodeCasa.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.DependencyInjection.Tests
{
    [TestClass]
    public sealed class CreateContextScopeTests
    {
        [TestMethod]
        public void CreateContextScope_Should_Only_Dispose_Context_Services()
        {
            var services = new ServiceCollection();
            services.AddScoped<IGlobalService, GlobalService>();
            var rootProvider = services.BuildServiceProvider();

            IGlobalService globalServiceInstance;
            IContextService contextServiceInstance;

            using (var scope = rootProvider.CreateContextScope(ctx =>
                   {
                       ctx.AddScoped<IContextService, ContextService>();
                   }))
            {
                globalServiceInstance = scope.ServiceProvider.GetRequiredService<IGlobalService>();
                contextServiceInstance = scope.ServiceProvider.GetRequiredService<IContextService>();

                // Assert instances are created and not disposed yet
                Assert.IsFalse(globalServiceInstance.IsDisposed);
                Assert.IsFalse(contextServiceInstance.IsDisposed);
            }

            // 3. Assert Cleanup
            // Context service MUST be disposed
            Assert.IsTrue(contextServiceInstance.IsDisposed);

            // Global service MUST be disposed (because it was resolved from the linked parent scope)
            Assert.IsTrue(globalServiceInstance.IsDisposed);

            // 4. Verify the Root Provider is still healthy
            using var finalCheck = rootProvider.CreateScope();
            var newGlobal = finalCheck.ServiceProvider.GetRequiredService<IGlobalService>();
            Assert.IsFalse(newGlobal.IsDisposed);
        }

        private class TrackedService : IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }

        private interface IGlobalService : IDisposable { bool IsDisposed { get; } }
        private class GlobalService : TrackedService, IGlobalService;

        private interface IContextService : IDisposable { bool IsDisposed { get; } }
        private class ContextService : TrackedService, IContextService;
    }
}
