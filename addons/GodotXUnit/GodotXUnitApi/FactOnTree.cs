using Xunit;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// use this when you need the actual test class to be in the
    /// tree to run.
    ///
    /// A few Notes:
    /// - the thread will always be in the _Process event
    /// - instances do not get shared
    /// - the test class has to be some extension of Godot.Node
    /// - the parent will be the test runner
    /// </summary>
    /*
        [FactOnTree]
        public void IsOnCorrectScene()
        {
            Assert.NotNull(GetTree());
            Assert.NotNull(GetParent());
        }
     */
    [XunitTestCaseDiscoverer("GodotXUnitApi.Internal.FactOnTreeDiscoverer", "GodotXUnitApi")]
    public class FactOnTree : FactAttribute
    {
        
    }
}