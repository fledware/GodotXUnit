using Godot;
using Xunit;

namespace GodotCSUnitTest.Tests
{
    public class TestInGodotTree : Node
    {
        [Fact]
        public void IsInTree()
        {
            Assert.NotNull(GetTree());
            Assert.NotNull(GetParent());
        }
    }
}