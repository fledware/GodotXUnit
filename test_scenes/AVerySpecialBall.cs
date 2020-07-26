using Godot;

namespace GodotCSUnitTest
{
    public class AVerySpecialBall : KinematicBody2D
    {
        [Signal]
        public delegate void WeCollidedd();
        
        // this ball doesnt really like to go anywhere
        public float gravity = 10f;

        public Vector2 velocity = new Vector2();
        
        public override void _Process(float delta)
        {
            Update();
        }

        public override void _PhysicsProcess(float delta)
        {
            velocity.y += gravity * delta;
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