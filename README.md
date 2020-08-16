# GodotXUnit

A plugin for executing Xunit tests within godot.

## Examples

you can look at all the examples in the ./tests directory.

#### Run Test On Scene

    [GodotFact(Scene = "res://test_scenes/SomeTestScene.tscn")]
    public void IsOnCorrectScene()
    {
        var scene = GDU.CurrentScene;
        Assert.Equal(typeof(SomeTestSceneRoot), scene?.GetType());
    }
 
#### Run In Different Thread Contexts

    [GodotFact]
    public async void AllThreadContextInOne()
    {
        Assert.False(Engine.IsInPhysicsFrame());

        await GDU.OnPhysicsProcessAwaiter;
        Assert.True(Engine.IsInPhysicsFrame());

        await GDU.OnProcessAwaiter;
        Assert.False(Engine.IsInPhysicsFrame());
    }
    
#### Signal Waiting Or Timeout

I've added a signal waiter method with a timeout:

    GDU.ToSignalWithTimeout(source, signal, timeoutMillis, throwOnTimeout)
    
This method helps simplify tests by allowing you to wait for signals
but also ensuring the tests actually finish or different parts have
timeouts. full example at ./tests/PhysicsCollisionTest.cs

        [GodotFact(Scene = "res://test_scenes/PhysicsCollisionTest.tscn")]
        public async void TestOhNoTooSlowOfFall()
        {
            var ball = (AVerySpecialBall) GDU.CurrentScene.FindNode("AVerySpecialBall");
            Assert.NotNull(ball);

            // it will throw a TimeoutException here because the gravity value is too low
            await GDU.ToSignalWithTimeout(ball, nameof(AVerySpecialBall.WeCollidedd), 2000);
            // it will never get here
            Assert.Equal(new Vector2(), ball.velocity);
        }

## Installation

* Go to AssetLib tab and search for GodotXUnit and Install project. Don't enable yet.
* include these scripts to your mono project csproj
    * addons/GodotXUnit/GodotTestRunner.cs
    * addons/GodotXUnit/Plugin.cs
    * addons/GodotXUnit/XUnitDock.cs
* execute this command on the cli at the root of your project:

        nuget restore ./addons/GodotXUnit/GodotXUnitApi/GodotXUnitApi.csproj

* add this reference to your main project:
    * ./addons/GodotXUnit/GodotXUnitApi/GodotXUnitApi.csproj
* build the solutions and let me know if there are any errors.
    * even if the errors are easy to fix, feel free to let me know so i can better
      document the installation process
* enable the plugin and you should see `GodotXUnit` on the bottom dock
  and there should be no errors in output


If there is a better way to do this, please let me know. I'd really like
to make the installation of this easier and remove the manual steps.

## Running Tests From A Sub Project

It's common in C# projects to separate unit and integration test into
sub projects. This helps with package size and enforcing dependencies.

By default, GodotXUnit will attempt to run tests in the root project.
If you have a subproject (with a csproj file at the root), you can
run that by selecting the desired project in the target assembly option
located at the top right of the GodotXUnit panel.

If you have a test project that is external from the godot project,
GodotXUnit will not be able to automatically find the project. You will
need to select 'Custom Location' in the drop down and put the absolute
path to the dll in the 'Custom Assembly Path (dll)' label. Make sure to
choose the dll that is in the `bin` directory, as xunit will need to
load the dll dependencies, and they must live next to the assembly being
discovered on.

## How It Works

When you click a run button, the first thing is the run args gets written
to ./addons/GodotXUnit/_work/RunArgs.json. this allows the runner to
know what needs to be ran. 

then, the runner scene is started with this command: 

`OS.Execute(OS.GetExecutablePath(), new [] {"res://addons/GodotXUnit/runner/GodotTestRunnerScene.tscn"}, false);`

then, events are listened to with file system events pulled by the editor. events
are sent by the runner for tests starting/finishing, and the entire summary.
this way we can update the editor while tests come in. events are passed
at the ./addons/GodotXUnit/_work directory.

finally, the results will be written to res://TestSummary.json. you can change
the location with the GodotXUnit/results_summary ProjectSetting.

## Known Issues

* debugging will not work.
* sometimes the editor will just stop being able to pull events. i don't know
  why this is. but if it happens, hit the stop button and it should clean everything
  up safely and you can attempt rerun.
* this does not integrate with IDEs, so tests have to be executed in the godot editor.
  But, you can run the tests via the cli if you execute the 
  res://addons/GodotXUnit/runner/GodotTestRunnerScene.tscn scene.

## Next steps

* try to figure out how to get an cs script running from an assembly other 
  than the main. that way we can get rid of the manual includes.
* make the results be in the junit format

## Thanks To

icons made by:
- https://freeicons.io/profile/3484
- https://freeicons.io/profile/3

for great work:
* https://github.com/CodeDarigan/WATSharp
* https://github.com/NathanWarden/godot-test-tools
