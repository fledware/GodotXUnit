using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    [XunitTestCaseDiscoverer("GodotXUnitApi.FactOnSceneDiscoverer", "GodotXUnitApi")]
    public class FactOnSceneAttribute : FactAttribute
    {
        public string SceneRes { get; set; }

        public FactOnSceneAttribute(string sceneRes)
        {
            this.SceneRes = sceneRes;
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
}