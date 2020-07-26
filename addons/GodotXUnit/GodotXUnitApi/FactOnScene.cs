using Xunit;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// use this on a test to have a scene get loaded before the test starts.
    /// an empty scene will be loaded after the test finishes.
    /// </summary>
    /*
        [FactOnScene("res://test_scenes/SomeTestScene.tscn")]
        public void IsOnCorrectScene()
        {
            var scene = GDU.Tree.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }
     */
    [XunitTestCaseDiscoverer("GodotXUnitApi.Internal.FactOnSceneDiscoverer", "GodotXUnitApi")]
    public class FactOnSceneAttribute : FactAttribute
    {
        public string SceneRes { get; set; }

        public FactOnSceneAttribute(string sceneRes)
        {
            this.SceneRes = sceneRes;
        }
    }
}