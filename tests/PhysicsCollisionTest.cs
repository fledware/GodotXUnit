using System;
using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests
{
    public partial class PhysicsCollisionTest
    {
        [GodotFact(Scene = "res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestOhNoTooSlowOfFall()
        {
            GDU.Print("this will fail");
            var ball = (AVerySpecialBall)GDU.CurrentScene.FindChild("AVerySpecialBall");
            Assert.NotNull(ball);

            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                // this will throw a TimeoutException
                await ball.ToSignalWithTimeout(nameof(AVerySpecialBall.WeCollided), 1000);
            });
        }

        [GodotFact(Scene = "res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestOhNoTooSlowOfFallButNoException()
        {
            var ball = (AVerySpecialBall)GDU.CurrentScene.FindChild("AVerySpecialBall");
            Assert.NotNull(ball);

            // it will not throw here, it will just continue
            await ball.ToSignalWithTimeout(nameof(AVerySpecialBall.WeCollided), 1000, throwOnTimeout: false);

            // it will get here, but it will fail this equals check
            Assert.NotEqual(new Vector2(), ball.Velocity);
        }

        // passing test
        [GodotFact(Scene = "res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestThatItWorksBecauseWeSetGravitySuperHigh()
        {
            var ball = (AVerySpecialBall)GDU.CurrentScene.FindChild("AVerySpecialBall");
            Assert.NotNull(ball);
            ball.gravity = 1500f;

            // it will pass here
            await ball.ToSignalWithTimeout(nameof(AVerySpecialBall.WeCollided), 1000);
            Assert.Equal(new Vector2(), ball.Velocity);
        }
    }
}