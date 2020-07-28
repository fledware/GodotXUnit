using System.Threading.Tasks;
using Godot;

namespace GodotXUnitApi
{
    public static class GDI
    {
        public static float PositionXByScreenPercent(float percent)
        {
            return GDU.Viewport.Size.x * percent;
        }
        
        public static float PositionYByScreenPercent(float percent)
        {
            return GDU.Viewport.Size.y * percent;
        }

        public static Vector2 PositionByScreenPercent(float xPercent, float yPercent)
        {
            return new Vector2(PositionXByScreenPercent(xPercent), PositionYByScreenPercent(yPercent));
        }
        
        public static void InputMousePressed(Vector2 screenPosition, ButtonList index = ButtonList.Left)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Pressed = true;
            inputEvent.ButtonIndex = (int) index;
            Input.ParseInputEvent(inputEvent);
        }

        public static void InputMouseUp(Vector2 screenPosition, ButtonList index = ButtonList.Left)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Pressed = false;
            inputEvent.ButtonIndex = (int) index;
            Input.ParseInputEvent(inputEvent);
        }

        public static void InputMouseMove(Vector2 screenPosition)
        {
            var inputEvent = new InputEventMouseMotion();
            inputEvent.GlobalPosition = screenPosition;
            Input.ParseInputEvent(inputEvent);
        }

        public static async Task SimulateMouseClick(Vector2 position, ButtonList index = ButtonList.Left)
        {
            await GDU.OnProcessAwaiter;
            InputMousePressed(position, index);
            await GDU.OnProcessAwaiter;
            await GDU.OnProcessAwaiter;
            InputMouseUp(position, index);
            await GDU.OnProcessAwaiter;
        }

        public static void SimulateMouseClickNoWait(Vector2 position, ButtonList index = ButtonList.Left)
        {
#pragma warning disable 4014
            SimulateMouseClick(position, index);
#pragma warning restore 4014
        }
    }
}