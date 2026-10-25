using Godot;

namespace Tervain;

public partial class MaterialDrop : Node2D
{
    public BlockKind Kind { get; init; }
    public Player Player { get; init; } = null!;
    public SandboxInventory Inventory { get; init; } = null!;
    private float _age;
    private bool _collected;
    public override void _Ready() => TextureFilter = TextureFilterEnum.Nearest;

    public override void _PhysicsProcess(double delta)
    {
        _age += (float)delta;
        if (_collected || Player.IsDead) return;
        Vector2 target = Player.BodyCenter;
        if (_age > 0.12f && Position.DistanceTo(target) < 120)
            Position = Position.MoveToward(target, (float)delta * 340);
        if (_age > 0.12f && Position.DistanceTo(target) < 24)
        {
            _collected = true;
            Inventory.Add(Kind);
            QueueFree();
        }
        QueueRedraw();
    }
    public override void _Draw()
    {
        float bob = Mathf.Sin(_age * 5) * 2;
        DrawCircle(new Vector2(0, bob), 10, new Color(0.75f, 0.95f, 0.8f, 0.12f));
        BlockArt.Draw(this, new Rect2(-6, -6 + bob, 12, 12), Kind);
        DrawRect(new Rect2(-6, -6 + bob, 12, 12), new Color("e4d7b8"), false, 1);
    }
}
