using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotCSUnitTest.Tests
{
    public class TestInGodotCycle
    {
        /// <summary>
        /// this test will run in the xunit thread. 
        /// </summary>
        [Fact]
        public void IsNotInPhysicsProcess1()
        {
            Assert.False(Engine.IsInPhysicsFrame());
        }

        /// <summary>
        /// this test will run in the godot process thread. 
        /// </summary>
        [FactOnProcess]
        public void IsNotInPhysicsProcess2()
        {
            Assert.False(Engine.IsInPhysicsFrame());
        }

        /// <summary>
        /// this test will run in the godot physics process thread. 
        /// </summary>
        [FactOnPhysicsProcess]
        public void IsInPhysicsProcess()
        {
            Assert.True(Engine.IsInPhysicsFrame());
        }

        /// <summary>
        /// this will run in the xunit thread, then wait to be
        /// in the physics frame, then run on the process frame.
        /// </summary>
        [Fact]
        public async void AllThreadContextInOne()
        {
            Assert.False(Engine.IsInPhysicsFrame());

            await GodotXUnitEvents.OnPhysicsProcessAwaiter;
            Assert.True(Engine.IsInPhysicsFrame());

            await GodotXUnitEvents.OnProcessAwaiter;
            Assert.False(Engine.IsInPhysicsFrame());
        }
    }
}