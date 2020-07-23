using Godot;

namespace GodotXUnit.Runner
{
	/// <summary>
	/// all this class does is add the runner to the root node.
	/// this allows the user load scenes however they want and
	/// it wont stop the tests.
	/// </summary>
    public class GodotTestEntry : Node
    {
	    public override void _Process(float delta)
	    {
		    if (GetNodeOrNull("/root/GodotTestRunner") == null)
		    {
			    GetTree().Root.AddChild(new GodotTestRunner
			    {
				    Name = nameof(GodotTestRunner)
			    });
		    }
		    SetProcess(false);
	    }
    }
}
