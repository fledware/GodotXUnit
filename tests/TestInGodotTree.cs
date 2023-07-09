using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    // tests that extend Godot.Node will automatically
    // be added as a child to the runner.
    public partial class TestInGodotTree : Node
    {
        [GodotFact]
        public void IsInTree()
        {
            Assert.NotNull(GetTree());
            Assert.NotNull(GetParent());
        }
    }
}