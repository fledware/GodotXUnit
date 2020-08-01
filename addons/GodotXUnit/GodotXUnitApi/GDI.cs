using System.Threading.Tasks;
using Godot;

namespace GodotXUnitApi
{
    /// <summary>
    /// helper methods for simulating mouse events.
    /// </summary>
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

        public static Vector2 PositionByScreenPercent(float screenPercentX, float screenPercentY)
        {
            return new Vector2(PositionXByScreenPercent(screenPercentX), PositionYByScreenPercent(screenPercentY));
        }
        
        public static void InputMousePressed(Vector2 screenPosition, ButtonList index = ButtonList.Left)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Position = screenPosition;
            inputEvent.Pressed = true;
            inputEvent.ButtonIndex = (int) index;
            inputEvent.ButtonMask = (int) index;
            Input.ParseInputEvent(inputEvent);
        }

        public static void InputMouseUp(Vector2 screenPosition, ButtonList index = ButtonList.Left)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Position = screenPosition;
            inputEvent.Pressed = false;
            inputEvent.ButtonIndex = (int) index;
            inputEvent.ButtonMask = (int) index;
            Input.ParseInputEvent(inputEvent);
        }

        public static void InputMouseMove(Vector2 screenPosition)
        {
            var inputEvent = new InputEventMouseMotion();
            inputEvent.GlobalPosition = screenPosition;
            inputEvent.Position = screenPosition;
            Input.ParseInputEvent(inputEvent);
        }

        public static async Task SimulateMouseClick(float screenPercentX, float screenPercentY, ButtonList index = ButtonList.Left)
        {
            var position = PositionByScreenPercent(screenPercentX, screenPercentY);
            await SimulateMouseClick(position, index);
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