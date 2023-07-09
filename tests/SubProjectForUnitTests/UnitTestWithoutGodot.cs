using System.Configuration;
using GodotXUnitApi;
using Xunit;

namespace SubProjectForUnitTests
{
    public partial class SomeClassTest
    {
        [GodotFact]
        public void SomePrettyCoolTest()
        {
            GDU.Print("you can run this test like a standard unit test");
            Assert.True(true);
        }

        [Fact]
        public void SomeOtherCoolTest()
        {
            Assert.False(false);
        }
    }
}