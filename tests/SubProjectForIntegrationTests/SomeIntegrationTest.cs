using GodotXUnitApi;
using GodotXUnitTest;
using Xunit;

namespace SubProjectForIntegrationTests
{
    public class SomeIntegrationTest
    {
        [GodotFact(Scene = "res://test_scenes/SomeTestScene.tscn")]
        public void IsOnCorrectScene()
        {
            var scene = GDU.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }
    }
}