using Godot;

namespace Tervain;

/// <summary>A full terrain block in flight; becomes grid terrain again when supported.</summary>
public partial class FallingSand : CharacterBody2D
{
    public BlockWorld World { get; init; } = null!;
    public Rect2 BodyBounds => new(GlobalPosition - Vector2.One * 12, Vector2.One * 24);

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        CollisionLayer = 1;
        CollisionMask = 1 | 2 | 4;
        SafeMargin = 0.001f;
        // A tiny horizontal clearance prevents adjacent tile seams from catching a vertical fall.
        AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(BlockWorld.TileSize - 0.2f, BlockWorld.TileSize) } });
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsQueuedForDeletion()) return;
        Velocity = new Vector2(0, Mathf.Min(Velocity.Y + 900 * (float)delta, 600));
        MoveAndSlide();
        World.BreakBushes(BodyBounds);
        if (IsOnFloor()) World.TrySettleSand(this);
    }

    public override void _Draw() => BlockArt.Draw(this, new Rect2(-12, -12, 24, 24), BlockKind.Sand);
}
