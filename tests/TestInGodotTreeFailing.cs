using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public class TestInGodotTreeFailing
    {
        /// <summary>
        /// this test will always fail because this class
        /// doesnt extend a Godot.Node
        /// </summary>
        [FactOnTree]
        public void IsInTree()
        {
            Assert.True(true);
        }
    }
}