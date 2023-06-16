using Godot;

namespace GodotXUnitTest
{
    public partial class AVerySpecialBall : CharacterBody2D
    {
        [Signal]
        public delegate void WeCollidedd();

        // this ball doesnt really like to go anywhere
        public float gravity = 10f;

        public Vector2 velocity = new Vector2();

        public override void _Process(double delta)
        {
            QueueRedraw();
        }

        public override void _PhysicsProcess(double delta)
        {
            velocity.Y += (float)(gravity * delta);
            velocity = MoveAndSlide(velocity, Vector2.Up);
            if (IsOnFloor())
            {
                GD.Print("yay");
                EmitSignal(nameof(WeCollidedd));
            }
        }

        public override void _Draw()
        {
            DrawCircle(new Vector2(), 10f, Colors.Aqua);
        }
    }
}