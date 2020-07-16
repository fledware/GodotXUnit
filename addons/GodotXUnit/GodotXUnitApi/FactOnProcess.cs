using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// 
    /// </summary>
    [XunitTestCaseDiscoverer("GodotXUnitApi.FactOnProcessDiscoverer", "GodotXUnitApi")]
    public class FactOnProcessAttribute : FactAttribute
    {
        
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
            yield return new TestCaseAfterSignal(() => GodotXUnitEvents.OnProcessAwaiter,
                diagnosticMessageSink, discoveryOptions, testMethod);
        }
    }
}