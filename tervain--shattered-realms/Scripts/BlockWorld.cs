using Godot;
using System.Collections.Generic;

namespace Tervain;

/// <summary>Small authored block terrain. Generation and chunking come in later milestones.</summary>
public partial class BlockWorld : Node2D
{
    public const int TileSize = 24;
    public const int Width = 96;
    public const int Height = 32;
    public const int SurfaceRow = 19;
    public const int CaveBottomRow = 28;
    public const float BuildReach = 160;
    public const float MiningReach = 192;
    private static readonly int[] TreeColumns = { 5, 9, 27, 32, 56, 63, 80, 87 };
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
    public static bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 4 && cell.Y < Height;
    public bool TryGetBlock(Vector2I cell, out BlockKind kind) => _blocks.TryGetValue(cell, out kind);
    public bool HasBlock(Vector2I cell) => _blocks.ContainsKey(cell);
    public bool HasWall(Vector2I cell) => _walls.ContainsKey(cell);
    public static Color Colour(BlockKind kind) => new(kind switch
    {
        BlockKind.Dirt => "947356", BlockKind.Stone => "7c9093", BlockKind.Wood => "bd8c56", BlockKind.IronOre => "b58a6a", _ => "36434b"
    });
    public static float MineDuration(BlockKind kind) => kind switch
    { BlockKind.Dirt => 0.28f, BlockKind.Stone => 0.6f, BlockKind.IronOre => 0.8f, BlockKind.Wood => 0.4f, _ => float.PositiveInfinity };

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        for (int x = 0; x < Width; x++)
            for (int y = SurfaceRow; y < Height; y++)
            {
                if (y >= 24 && y < CaveBottomRow && x is >= 39 and <= 54) continue;
                BlockKind kind = y == Height - 1 ? BlockKind.Bedrock : y < 23 ? BlockKind.Dirt : BlockKind.Stone;
                if (x is >= 55 and <= 56 && y is >= 25 and <= 26) kind = BlockKind.IronOre;
                SetBlock(new Vector2I(x, y), kind);
            }
        foreach (int x in TreeColumns)
            for (int y = 13; y < SurfaceRow; y++) SetBlock(new Vector2I(x, y), BlockKind.Wood);
        // Surface stone outcrops offer stone without forcing a deep excavation.
        for (int x = 39; x <= 43; x++) SetBlock(new Vector2I(x, SurfaceRow - 1), BlockKind.Stone);
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

    // Mining ignores visibility; the correct tool, range, and protection rules still apply.
    public bool CanMine(Vector2I cell, Player player) => !player.IsDead && InBounds(cell) &&
        _blocks.TryGetValue(cell, out BlockKind kind) && EquipmentRules.CanMine(player.EquippedItem, kind) &&
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
        for (int x = 0; x < Width; x += 5)
        {
            float px = x * TileSize + 40;
            DrawRect(new Rect2(px, 250, 22, 240), new Color("234348"));
            DrawCircle(new Vector2(px + 11, 240), 69, new Color("234348"));
        }
        foreach (int x in TreeColumns)
            if (HasBlock(new Vector2I(x, 13)))
            {
                DrawCircle(new Vector2(x * TileSize + TileSize / 2f, 294), 57, new Color("426951"));
                DrawCircle(new Vector2(x * TileSize - 10, 282), 35, new Color("4d7658"));
            }
        foreach (var entry in _walls)
        {
            if (HasBlock(entry.Key)) continue; // Opaque foreground already covers this scenery.
            Rect2 rect = CellRect(entry.Key);
            BlockArt.Draw(this, rect, entry.Value, new Color(0.42f, 0.42f, 0.42f));
        }
        foreach (var entry in _blocks)
        {
            Rect2 rect = CellRect(entry.Key);
            BlockArt.Draw(this, rect, entry.Value);
            if (entry.Value == BlockKind.Dirt && !HasBlock(entry.Key + Vector2I.Up))
                DrawRect(new Rect2(rect.Position, new Vector2(TileSize, 3)), new Color("8ca773"));
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
