using Godot;
using System.Collections.Generic;

namespace Tervain;

/// <summary>Small authored block terrain. Generation and chunking come in later milestones.</summary>
public partial class BlockWorld : Node2D
{
    public const int TileSize = 32;
    public const int Width = 72;
    public const int Height = 24;
    public const float Reach = 160;
    private readonly Dictionary<Vector2I, BlockKind> _blocks = new();
    private readonly Dictionary<Vector2I, StaticBody2D> _bodies = new();
    public int BlockCount => _blocks.Count;
    public Vector2I? HighlightCell { get; set; }
    public bool HighlightValid { get; set; }
    public float MiningProgress { get; set; }

    public static Vector2I CellAt(Vector2 point) => new(Mathf.FloorToInt(point.X / TileSize), Mathf.FloorToInt(point.Y / TileSize));
    public static Vector2 Center(Vector2I cell) => new Vector2(cell.X + 0.5f, cell.Y + 0.5f) * TileSize;
    public static Rect2 CellRect(Vector2I cell) => new(new Vector2(cell.X, cell.Y) * TileSize, Vector2.One * TileSize);
    public static bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 3 && cell.Y < Height;
    public bool TryGetBlock(Vector2I cell, out BlockKind kind) => _blocks.TryGetValue(cell, out kind);
    public bool HasBlock(Vector2I cell) => _blocks.ContainsKey(cell);
    public static Color Colour(BlockKind kind) => new(kind switch
    {
        BlockKind.Dirt => "947356", BlockKind.Stone => "7c9093", BlockKind.Wood => "bd8c56", _ => "36434b"
    });
    public static float MineDuration(BlockKind kind) => kind switch
    { BlockKind.Dirt => 0.28f, BlockKind.Stone => 0.6f, BlockKind.Wood => 0.4f, _ => float.PositiveInfinity };

    public override void _Ready()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 14; y < Height; y++)
            {
                if (y is >= 18 and <= 20 && x is >= 29 and <= 40) continue;
                SetBlock(new Vector2I(x, y), y == Height - 1 ? BlockKind.Bedrock : y < 17 ? BlockKind.Dirt : BlockKind.Stone);
            }
        foreach (int x in new[] { 4, 7, 20, 24, 42, 47, 60, 65 })
            for (int y = 10; y < 14; y++) SetBlock(new Vector2I(x, y), BlockKind.Wood);
        // Surface stone outcrops offer stone without forcing a deep excavation.
        for (int x = 29; x <= 32; x++) SetBlock(new Vector2I(x, 13), BlockKind.Stone);
        AddBoundary(new Rect2(-32, -1000, 32, 2000));
        AddBoundary(new Rect2(Width * TileSize, -1000, 32, 2000));
    }

    private void AddBoundary(Rect2 rect)
    {
        var body = new StaticBody2D { Position = rect.Position, CollisionLayer = 1, CollisionMask = 0 };
        body.AddChild(new CollisionShape2D { Position = rect.Size / 2, Shape = new RectangleShape2D { Size = rect.Size } });
        AddChild(body);
    }

    private void SetBlock(Vector2I cell, BlockKind kind)
    {
        if (_blocks.ContainsKey(cell)) return;
        _blocks[cell] = kind;
        var body = new StaticBody2D { Position = Center(cell), CollisionLayer = 1, CollisionMask = 0 };
        body.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = Vector2.One * TileSize } });
        _bodies[cell] = body;
        AddChild(body);
        QueueRedraw();
    }

    public bool CanReach(Vector2I cell, Player player, bool targetIsSolid)
    {
        if (!InBounds(cell)) return false;
        Vector2 origin = player.GlobalPosition + new Vector2(0, -24);
        Vector2 destination = Center(cell);
        if (origin.DistanceTo(destination) > Reach) return false;
        var ray = PhysicsRayQueryParameters2D.Create(origin, destination, 1);
        var hit = GetWorld2D().DirectSpaceState.IntersectRay(ray);
        if (hit.Count == 0) return true;
        return targetIsSolid && _bodies.TryGetValue(cell, out var body) && hit["collider"].AsGodotObject() == body;
    }

    public bool CanMine(Vector2I cell, Player player) => !player.IsDead &&
        _blocks.TryGetValue(cell, out BlockKind kind) && kind != BlockKind.Bedrock && CanReach(cell, player, true);

    public bool TryMine(Vector2I cell, Player player, out BlockKind material)
    {
        material = default;
        if (!CanMine(cell, player)) return false;
        material = _blocks[cell];
        _blocks.Remove(cell);
        _bodies[cell].QueueFree();
        _bodies.Remove(cell);
        QueueRedraw();
        return true;
    }

    public bool CanPlace(Vector2I cell, Player player)
    {
        if (player.IsDead || HasBlock(cell) || !CanReach(cell, player, false)) return false;
        Rect2 playerBounds = new(player.GlobalPosition + new Vector2(-14, -48), new Vector2(28, 48));
        if (CellRect(cell).Intersects(playerBounds)) return false;
        return HasBlock(cell + Vector2I.Down) || HasBlock(cell + Vector2I.Up) ||
            HasBlock(cell + Vector2I.Left) || HasBlock(cell + Vector2I.Right);
    }

    public bool TryPlace(Vector2I cell, BlockKind material, Player player, SandboxInventory inventory)
    {
        if (material == BlockKind.Bedrock || !CanPlace(cell, player) || !inventory.Spend(material)) return false;
        SetBlock(cell, material);
        return true;
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, Width * TileSize, Height * TileSize), new Color("182f37"));
        DrawCircle(new Vector2(650, 180), 55, new Color("5e7163"));
        for (int x = 0; x < Width; x += 4)
        {
            float px = x * TileSize + 40;
            DrawRect(new Rect2(px, 250, 22, 240), new Color("234348"));
            DrawCircle(new Vector2(px + 11, 240), 69, new Color("234348"));
        }
        foreach (int x in new[] { 4, 7, 20, 24, 42, 47, 60, 65 })
            if (HasBlock(new Vector2I(x, 10)))
            {
                DrawCircle(new Vector2(x * TileSize + 16, 302), 57, new Color("426951"));
                DrawCircle(new Vector2(x * TileSize - 10, 290), 35, new Color("4d7658"));
            }
        foreach (var entry in _blocks)
        {
            Rect2 rect = CellRect(entry.Key);
            Color colour = Colour(entry.Value);
            float tint = ((entry.Key.X * 7 + entry.Key.Y * 3) % 5 - 2) * 0.025f;
            colour = colour.Lightened(tint);
            DrawRect(rect, colour);
            DrawLine(rect.Position, rect.Position + new Vector2(32, 0), colour.Lightened(0.12f), 1);
            DrawLine(rect.Position, rect.Position + new Vector2(0, 32), colour.Darkened(0.13f), 1);
            if (entry.Value == BlockKind.Dirt && !HasBlock(entry.Key + Vector2I.Up))
                DrawRect(new Rect2(rect.Position, new Vector2(32, 5)), new Color("8ca773"));
            if (entry.Value == BlockKind.Wood)
            {
                DrawLine(rect.Position + new Vector2(9, 5), rect.Position + new Vector2(9, 26), colour.Darkened(0.2f), 2);
                DrawLine(rect.Position + new Vector2(23, 8), rect.Position + new Vector2(23, 30), colour.Darkened(0.2f), 2);
            }
            if (entry.Value == BlockKind.Stone)
                DrawLine(rect.Position + new Vector2(5, 21), rect.Position + new Vector2(19, 14), colour.Darkened(0.2f), 2);
        }
        if (HighlightCell is Vector2I selected)
        {
            Rect2 rect = CellRect(selected);
            Color colour = new(HighlightValid ? "b3f6d4" : "e28a78");
            DrawRect(rect.Grow(-1), new Color(colour, 0.14f));
            DrawRect(rect.Grow(-1), colour, false, 2);
            if (MiningProgress > 0)
                DrawRect(new Rect2(rect.Position + new Vector2(3, 25), new Vector2(26 * MiningProgress, 4)), colour);
        }
    }
}
