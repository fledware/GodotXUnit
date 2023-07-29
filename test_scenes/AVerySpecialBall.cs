using Godot;

namespace GodotXUnitTest;

public partial class AVerySpecialBall : CharacterBody2D
{
    [Signal]
    public delegate void WeCollidedEventHandler();

    // this ball doesnt really like to go anywhere
    public float gravity = 10f;

    public override void _Process(double delta) => QueueRedraw();

    public override void _PhysicsProcess(double delta)
    {
        Velocity += new Vector2(0, gravity * (float)delta);
        MoveAndSlide();
        if (IsOnFloor())
        {
            GD.Print("yay");
            EmitSignal(nameof(WeCollided));
        }
    }

    public override void _Draw() => DrawCircle(new Vector2(), 10f, Colors.Aqua);
}