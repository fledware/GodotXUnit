using Godot;
using GodotXUnitApi;
using Xunit;

namespace GodotXUnitTest.Tests;

public partial class TestInGodotCycle
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
    [GodotFact(Frame = GodotFactFrame.Process)]
    public void IsNotInPhysicsProcess2()
    {
        Assert.False(Engine.IsInPhysicsFrame());
    }

    /// <summary>
    /// this test will run in the godot physics process thread. 
    /// </summary>
    [GodotFact(Frame = GodotFactFrame.PhysicsProcess)]
    public void IsInPhysicsProcess()
    {
        Assert.True(Engine.IsInPhysicsFrame());
    }

    /// <summary>
    /// this will run in the xunit thread, then wait to be
    /// in the physics frame, then run on the process frame.
    /// </summary>
    [GodotFact]
    public async void AllThreadContextInOne()
    {
        Assert.False(Engine.IsInPhysicsFrame());

        await GDU.OnPhysicsProcessAwaiter;
        Assert.True(Engine.IsInPhysicsFrame());

        await GDU.OnProcessAwaiter;
        Assert.False(Engine.IsInPhysicsFrame());
    }
}