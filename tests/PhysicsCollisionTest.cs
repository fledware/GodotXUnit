using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotCSUnitTest.Tests
{
    public class PhysicsCollisionTest
    {
        // failing test
        [FactOnScene("res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestOhNoTooSlowOfFall()
        {
            var ball = (AVerySpecialBall) GDU.CurrentScene.FindNode("AVerySpecialBall");
            Assert.NotNull(ball);

            // it will throw a TimeoutException here because the gravity value is too low
            await GDU.ToSignalWithTimeout(ball, nameof(AVerySpecialBall.WeCollidedd), 1000);
            // it will never get here
            Assert.Equal(new Vector2(), ball.velocity);
        }
        
        // failing test
        [FactOnScene("res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestOhNoTooSlowOfFallButNoException()
        {
            var ball = (AVerySpecialBall) GDU.CurrentScene.FindNode("AVerySpecialBall");
            Assert.NotNull(ball);

            // it will not throw here, it will just continue
            await GDU.ToSignalWithTimeout(ball, nameof(AVerySpecialBall.WeCollidedd), 1000, throwOnTimeout: false);
            // it will get here, but it will fail this equals check
            Assert.Equal(new Vector2(), ball.velocity);
        }
        
        // passing test
        [FactOnScene("res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestThatItWorksBecauseWeSetGravitySuperHigh()
        {
            var ball = (AVerySpecialBall) GDU.CurrentScene.FindNode("AVerySpecialBall");
            Assert.NotNull(ball);
            ball.gravity = 1500f;

            // it will pass here
            await GDU.ToSignalWithTimeout(ball, nameof(AVerySpecialBall.WeCollidedd), 1000);
            Assert.Equal(new Vector2(), ball.velocity);
        }
    }
}