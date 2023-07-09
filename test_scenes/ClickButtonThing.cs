using Godot;

namespace GodotXUnitTest
{
    public partial class ClickButtonThing : CheckBox
    {
        public override void _UnhandledInput(InputEvent inputEvent)
        {
            GD.Print("_UnhandledInput: ", inputEvent.AsText());
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent is InputEventMouseButton buttonEvent)
            {
                GD.Print("_Input: ", buttonEvent.AsText(), buttonEvent.GlobalPosition);
            }
            
        }
    }
}