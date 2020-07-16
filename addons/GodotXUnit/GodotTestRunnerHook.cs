using Godot;

namespace GodotXUnit
{
    /// <summary>
    /// we do this so the user can load scenes and the runner won't
    /// be lost. it will still be up to the user to 
    /// </summary>
    public class GodotTestRunnerHook : Node
    {
        public override void _Process(float delta)
        {
            if (!HasNode("/root/GodotTestRunner"))
            {
                var runner = new GodotTestRunner
                {
                    Name = nameof(GodotTestRunner)
                };
                GetTree().Root.AddChild(runner);
            }
            GetTree().CurrentScene = this;
            GD.Print($"ME: {GetPath()}");
            SetProcess(false);
        }
    }
}