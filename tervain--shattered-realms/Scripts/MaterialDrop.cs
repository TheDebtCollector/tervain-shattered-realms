using Godot;

namespace Tervain;

public partial class MaterialDrop : CharacterBody2D
{
    private const float Gravity = 900;
    private const float TerminalSpeed = 600;
    public BlockKind Kind { get; init; }
    public Player Player { get; init; } = null!;
    public SandboxInventory Inventory { get; init; } = null!;
    private float _age;
    private bool _collected;
    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        CollisionLayer = 0;
        CollisionMask = 1; // Foreground terrain only; scenery, characters, and drops are not solid to loot.
        AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(12, 12) } });
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _age += dt;
        if (_collected) return;
        Vector2 target = Player.BodyCenter;
        if (!Player.IsDead && _age > 0.12f && GlobalPosition.DistanceTo(target) < 120)
        {
            // Preserve the existing close-range pickup attraction, including buried-block loot.
            Velocity = Vector2.Zero;
            GlobalPosition = GlobalPosition.MoveToward(target, dt * 340);
        }
        else
        {
            Velocity = new Vector2(0, Mathf.Min(Velocity.Y + Gravity * dt, TerminalSpeed));
            MoveAndSlide();
        }
        if (!Player.IsDead && _age > 0.12f && GlobalPosition.DistanceTo(target) < 24)
        {
            _collected = true;
            Inventory.Add(Kind);
            QueueFree();
        }
    }
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 10, new Color(0.75f, 0.95f, 0.8f, 0.12f));
        BlockArt.Draw(this, new Rect2(-6, -6, 12, 12), Kind);
        DrawRect(new Rect2(-6, -6, 12, 12), new Color("e4d7b8"), false, 1);
    }
}
