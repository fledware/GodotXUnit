using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// the annotation to make the test start in the physics process
    /// </summary>
    [XunitTestCaseDiscoverer("GodotXUnitApi.FactOnPhysicsProcessDiscoverer", "GodotXUnitApi")]
    public class FactOnPhysicsProcessAttribute : FactAttribute
    {
        
    }
    
    /// <summary>
    /// xunit integration so the given annotation can be worked with or something
    /// </summary>
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
            yield return new TestCaseAfterSignal(() => GodotXUnitEvents.OnPhysicsProcessAwaiter,
                diagnosticMessageSink, discoveryOptions, testMethod);
        }
    }
}