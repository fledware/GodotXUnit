using Xunit;

namespace GodotXUnitTest.Tests;

public partial class SomeBasicTests
{
    [Fact]
    public void SanityCheckThatWeGotHere() => Assert.True(true);
}
