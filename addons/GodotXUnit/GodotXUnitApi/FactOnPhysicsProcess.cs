using Xunit;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// this will make this test run in the physics frame
    /// </summary>
    /*
        [FactOnPhysicsProcess]
        public void IsInPhysicsProcess()
        {
            Assert.True(Engine.IsInPhysicsFrame());
        }
     */
    [XunitTestCaseDiscoverer("GodotXUnitApi.Internal.FactOnPhysicsProcessDiscoverer", "GodotXUnitApi")]
    public class FactOnPhysicsProcessAttribute : FactAttribute
    {
        
    }
}