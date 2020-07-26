using Xunit;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// will have the test run in the process notification
    /// </summary>
    /*
        [FactOnScene("res://test_scenes/SomeTestScene.tscn")]
        public void IsOnCorrectScene()
        {
            var scene = GDU.Tree.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }
     */
    [XunitTestCaseDiscoverer("GodotXUnitApi.Internal.FactOnProcessDiscoverer", "GodotXUnitApi")]
    public class FactOnProcessAttribute : FactAttribute
    {
        
    }
}