using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public partial class SomeTestSceneTest
    {
        [GodotFact(Scene = "res://test_scenes/SomeTestScene.tscn")]
        public void IsOnCorrectScene()
        {
            var scene = GDU.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public void IsNotInSomeTestScene()
        {
            var scene = GDU.CurrentScene;
            Assert.NotEqual(typeof(SomeTestSceneRoot), scene?.GetType());
        }
    }
}