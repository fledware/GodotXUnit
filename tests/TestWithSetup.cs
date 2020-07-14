using Xunit;

namespace GodotCSUnitTest.Tests
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
    }
}