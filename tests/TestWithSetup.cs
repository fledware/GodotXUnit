using Xunit;

namespace GodotXUnitTest.Tests
{
    public class TestWithSetup
    {
        public int sayWhat = 0;
        
        public TestWithSetup()
        {
            sayWhat = 234;
        }

        [Fact]
        public void TestSayWhat()
        {
            Assert.Equal(234, sayWhat);
        }

        [Fact(Skip = "we need to skip i guess")]
        public void SkipOrSomething()
        {
            Assert.False(true);
        }
    }
}