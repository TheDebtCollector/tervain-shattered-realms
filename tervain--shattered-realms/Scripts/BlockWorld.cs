using Godot;
using System.Collections.Generic;

namespace Tervain;

/// <summary>Small authored block terrain. Generation and chunking come in later milestones.</summary>
public partial class BlockWorld : Node2D
{
    public const int TileSize = 16;
    public const int Width = 144;
    public const int Height = 48;
    public const float BuildReach = 160;
    public const float MiningReach = 384;
    private static readonly int[] TreeColumns = { 8, 14, 40, 48, 84, 94, 120, 130 };
    private readonly Dictionary<Vector2I, BlockKind> _blocks = new();
    // Generated scenery only: kept independently of foreground blocks, with no physics bodies.
    private readonly Dictionary<Vector2I, BlockKind> _walls = new();
    private readonly Dictionary<Vector2I, StaticBody2D> _bodies = new();
    public int BlockCount => _blocks.Count;
    public Vector2I? HighlightCell { get; set; }
    public bool HighlightValid { get; set; }
    public float MiningProgress { get; set; }

    public static Vector2I CellAt(Vector2 point) => new(Mathf.FloorToInt(point.X / TileSize), Mathf.FloorToInt(point.Y / TileSize));
    public static Vector2 Center(Vector2I cell) => new Vector2(cell.X + 0.5f, cell.Y + 0.5f) * TileSize;
    public static Rect2 CellRect(Vector2I cell) => new(new Vector2(cell.X, cell.Y) * TileSize, Vector2.One * TileSize);
    public static bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 6 && cell.Y < Height;
    public bool TryGetBlock(Vector2I cell, out BlockKind kind) => _blocks.TryGetValue(cell, out kind);
    public bool HasBlock(Vector2I cell) => _blocks.ContainsKey(cell);
    public bool HasWall(Vector2I cell) => _walls.ContainsKey(cell);
    public static Color Colour(BlockKind kind) => new(kind switch
    {
        BlockKind.Dirt => "947356", BlockKind.Stone => "7c9093", BlockKind.Wood => "bd8c56", _ => "36434b"
    });
    public static float MineDuration(BlockKind kind) => kind switch
    { BlockKind.Dirt => 0.28f, BlockKind.Stone => 0.6f, BlockKind.Wood => 0.4f, _ => float.PositiveInfinity };

    public override void _Ready()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 28; y < Height; y++)
            {
                if (y is >= 36 and <= 41 && x is >= 58 and <= 81) continue;
                SetBlock(new Vector2I(x, y), y == Height - 1 ? BlockKind.Bedrock : y < 34 ? BlockKind.Dirt : BlockKind.Stone);
            }
        foreach (int x in TreeColumns)
            for (int dx = 0; dx < 2; dx++)
                for (int y = 20; y < 28; y++) SetBlock(new Vector2I(x + dx, y), BlockKind.Wood);
        // Surface stone outcrops offer stone without forcing a deep excavation.
        for (int x = 58; x <= 65; x++)
            for (int y = 26; y < 28; y++) SetBlock(new Vector2I(x, y), BlockKind.Stone);
        // Snapshot only generated solid cells. Open sky, cave air, and later player builds
        // do not create walls; mining the foreground leaves this scenery intact.
        foreach (var block in _blocks) _walls[block.Key] = block.Value;
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

    private bool CanReachForBuilding(Vector2I cell, Player player)
    {
        if (!InBounds(cell)) return false;
        Vector2 origin = player.BodyCenter;
        Vector2 destination = Center(cell);
        if (origin.DistanceTo(destination) > BuildReach) return false;
        var ray = PhysicsRayQueryParameters2D.Create(origin, destination, 1);
        var hit = GetWorld2D().DirectSpaceState.IntersectRay(ray);
        return hit.Count == 0;
    }

    // Mining intentionally ignores visibility: only range and protected block rules apply.
    public bool CanMine(Vector2I cell, Player player) => !player.IsDead && InBounds(cell) &&
        _blocks.TryGetValue(cell, out BlockKind kind) && kind != BlockKind.Bedrock &&
        player.BodyCenter.DistanceTo(Center(cell)) <= MiningReach;

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
        if (player.IsDead || HasBlock(cell) || !CanReachForBuilding(cell, player)) return false;
        if (CellRect(cell).Intersects(player.BodyBounds)) return false;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
            if (node is Enemy enemy && !enemy.IsDead && CellRect(cell).Intersects(enemy.BodyBounds)) return false;
        return HasWall(cell) || HasBlock(cell + Vector2I.Down) || HasBlock(cell + Vector2I.Up) ||
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
        for (int x = 0; x < Width; x += 8)
        {
            float px = x * TileSize + 40;
            DrawRect(new Rect2(px, 250, 22, 240), new Color("234348"));
            DrawCircle(new Vector2(px + 11, 240), 69, new Color("234348"));
        }
        foreach (int x in TreeColumns)
            if (HasBlock(new Vector2I(x, 20)) || HasBlock(new Vector2I(x + 1, 20)))
            {
                DrawCircle(new Vector2(x * TileSize + 16, 302), 57, new Color("426951"));
                DrawCircle(new Vector2(x * TileSize - 10, 290), 35, new Color("4d7658"));
            }
        foreach (var entry in _walls)
        {
            if (HasBlock(entry.Key)) continue; // Opaque foreground already covers this scenery.
            Rect2 rect = CellRect(entry.Key);
            Color colour = Colour(entry.Value).Darkened(0.58f);
            DrawRect(rect, colour);
            DrawLine(rect.Position, rect.Position + new Vector2(TileSize, 0), colour.Lightened(0.08f), 1);
            DrawLine(rect.Position, rect.Position + new Vector2(0, TileSize), colour.Darkened(0.15f), 1);
            // Recessed seams distinguish scenery from the brighter foreground grid.
            DrawLine(rect.Position + new Vector2(4, 8), rect.Position + new Vector2(12, 8), colour.Darkened(0.14f), 1);
        }
        foreach (var entry in _blocks)
        {
            Rect2 rect = CellRect(entry.Key);
            Color colour = Colour(entry.Value);
            float tint = ((entry.Key.X * 7 + entry.Key.Y * 3) % 5 - 2) * 0.025f;
            colour = colour.Lightened(tint);
            DrawRect(rect, colour);
            DrawLine(rect.Position, rect.Position + new Vector2(TileSize, 0), colour.Lightened(0.12f), 1);
            DrawLine(rect.Position, rect.Position + new Vector2(0, TileSize), colour.Darkened(0.13f), 1);
            if (entry.Value == BlockKind.Dirt && !HasBlock(entry.Key + Vector2I.Up))
                DrawRect(new Rect2(rect.Position, new Vector2(TileSize, 3)), new Color("8ca773"));
            if (entry.Value == BlockKind.Wood)
            {
                DrawLine(rect.Position + new Vector2(4, 2), rect.Position + new Vector2(4, 13), colour.Darkened(0.2f), 1);
                DrawLine(rect.Position + new Vector2(11, 4), rect.Position + new Vector2(11, 15), colour.Darkened(0.2f), 1);
            }
            if (entry.Value == BlockKind.Stone)
                DrawLine(rect.Position + new Vector2(3, 11), rect.Position + new Vector2(10, 7), colour.Darkened(0.2f), 1);
        }
        if (HighlightCell is Vector2I selected)
        {
            Rect2 rect = CellRect(selected);
            Color colour = new(HighlightValid ? "b3f6d4" : "e28a78");
            DrawRect(rect.Grow(-1), new Color(colour, 0.14f));
            DrawRect(rect.Grow(-1), colour, false, 2);
            if (MiningProgress > 0)
                DrawRect(new Rect2(rect.Position + new Vector2(2, TileSize - 4), new Vector2((TileSize - 4) * MiningProgress, 2)), colour);
        }
    }
}
