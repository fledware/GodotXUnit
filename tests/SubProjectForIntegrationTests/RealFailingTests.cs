using GodotXUnitApi;
using GodotXUnitTest;
using Xunit;

namespace SubProjectForIntegrationTests
{
    /// <summary>
    /// these tests are required to fail. so i'm putting these inside
    /// this integration tests sub project so:
    /// - we can build a CI example of running tests from a subproject
    /// - we can see an example of what happens when it fails
    /// </summary>
    public partial class RealFailingTests
    {
        [GodotFact(Scene = "res://SomeSceneNotFound.tscn")]
        public void SceneUnableToBeFound()
        {
            GDU.Print("this will fail");
            var scene = GDU.CurrentScene;
            Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
        }
    }
}