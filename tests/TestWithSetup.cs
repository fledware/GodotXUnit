using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public partial class TestWithSetup
    {
        public int sayWhat = 0;
        
        public TestWithSetup()
        {
            sayWhat = 234;
        }

        [GodotFact]
        public void TestSayWhat()
        {
            Assert.Equal(234, sayWhat);
        }

        [GodotFact(Skip = "we need to skip i guess")]
        public void SkipOrSomething()
        {
            Assert.False(true);
        }
    }
}