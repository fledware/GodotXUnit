using Godot;
using Xunit;

namespace GodotCSUnitTest.Tests
{
    public class TestInGodotTree : Node
    {
        /// <summary>
        /// NOTE: this does not work. i'm not sure if its worth getting
        ///       this type of pattern to work. 
        /// </summary>
        [Fact]
        public void IsInTree()
        {
            Assert.NotNull(GetTree());
            Assert.NotNull(GetParent());
        }
    }
}