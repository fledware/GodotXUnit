using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    public class FactOnPhysicsProcessDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public FactOnPhysicsProcessDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            yield return new TestCaseAfterSignal(() => GDU.OnPhysicsProcessAwaiter,
                diagnosticMessageSink, discoveryOptions, testMethod);
        }
    }
    
    public class FactOnProcessDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public FactOnProcessDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            yield return new TestCaseAfterSignal(() => GDU.OnProcessAwaiter,
                diagnosticMessageSink, discoveryOptions, testMethod);
        }
    }
    
    public class FactOnSceneDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public FactOnSceneDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            var sceneName = factAttribute.GetNamedArgument<string>(nameof(FactOnSceneAttribute.SceneRes));
            yield return new TestCaseOnScene(sceneName, diagnosticMessageSink, discoveryOptions, testMethod);
        }
    }
    
    public class FactOnTreeDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public FactOnTreeDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            yield return new TestCaseOnTree(diagnosticMessageSink, discoveryOptions, testMethod);
        }
    }
}