using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public class DebugDrawingTest
    {
        [Fact]
        public async void TestDrawingStuffs()
        {
            int frame = 0;
            await GDU.RequestDrawing(60, drawer =>
            {
                frame++;
                drawer.DrawCircle(new Vector2(frame * 5, frame * 5), 30f, Colors.Azure);
            });
            Assert.Equal(60, frame);
        }
    }
}