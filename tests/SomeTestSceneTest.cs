using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public class SomeTestSceneTest
    {
        [FactOnScene("res://test_scenes/SomeTestScene.tscn")]
        public void IsOnCorrectScene()
        {
            var scene = GDU.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }

        [FactOnProcess]
        public void IsNotInSomeTestScene()
        {
            var scene = GDU.CurrentScene;
            Assert.NotEqual(typeof(SomeTestSceneRoot), scene?.GetType());
        }
        
        [FactOnScene("res://SomeSceneNotFound.tscn")]
        public void SceneUnableToBeFound()
        {
            var scene = GDU.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }
    }
}