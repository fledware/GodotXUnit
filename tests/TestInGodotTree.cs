using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public class TestInGodotTree : Node
    {
        [FactOnTree]
        public void IsInTree()
        {
            Assert.NotNull(GetTree());
            Assert.NotNull(GetParent());
        }
    }
}