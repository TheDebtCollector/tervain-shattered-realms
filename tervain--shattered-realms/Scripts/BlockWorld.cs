using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Tervain;

/// <summary>Small authored block terrain. Generation and chunking come in later milestones.</summary>
public partial class BlockWorld : Node2D
{
    public const int TileSize = 24;
    public const int Width = 128;
    public const int Height = 32;
    public const int SurfaceRow = 19;
    public const int CaveBottomRow = 28;
    public const int DesertStartColumn = 68;
    public const int TundraStartColumn = 96;
    public const float BuildReach = 160;
    public const float MiningReach = 192;
    private static readonly int[] TreeColumns = { 5, 9, 27, 32, 56, 63 };
    private static readonly int[] PineColumns = { 99, 103, 115, 123 };
    private readonly Dictionary<Vector2I, BlockKind> _blocks = new();
    // Generated scenery only: kept independently of foreground blocks, with no physics bodies.
    private readonly Dictionary<Vector2I, BlockKind> _walls = new();
    private readonly Dictionary<Vector2I, StaticBody2D> _bodies = new();
    private readonly HashSet<Vector2I> _bushes = new();
    private readonly List<FallingSand> _fallingSand = new();
    private bool _sandDirty;
    public int FallingSandCount => _fallingSand.Count;
    public int BushCount => _bushes.Count;
    public bool HasBush(Vector2I cell) => _bushes.Contains(cell);
    public int BlockCount => _blocks.Count + _fallingSand.Count;
    public Vector2I? HighlightCell { get; set; }
    public bool HighlightValid { get; set; }
    public float MiningProgress { get; set; }

    public static Vector2I CellAt(Vector2 point) => new(Mathf.FloorToInt(point.X / TileSize), Mathf.FloorToInt(point.Y / TileSize));
    public static Vector2 Center(Vector2I cell) => new Vector2(cell.X + 0.5f, cell.Y + 0.5f) * TileSize;
    public static Rect2 CellRect(Vector2I cell) => new(new Vector2(cell.X, cell.Y) * TileSize, Vector2.One * TileSize);
    public static bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 4 && cell.Y < Height;
    public bool TryGetBlock(Vector2I cell, out BlockKind kind)
    {
        if (_blocks.TryGetValue(cell, out kind)) return true;
        kind = BlockKind.Sand;
        return FallingAt(cell) != null;
    }
    private FallingSand? FallingAt(Vector2I cell) => _fallingSand.FirstOrDefault(sand => CellAt(sand.GlobalPosition) == cell);
    public bool HasBlock(Vector2I cell) => TryGetBlock(cell, out _);
    public bool HasWall(Vector2I cell) => _walls.ContainsKey(cell);
    public static Color Colour(BlockKind kind) => new(kind switch
    {
        BlockKind.Dirt => "947356", BlockKind.Stone => "7c9093", BlockKind.Wood => "bd8c56", BlockKind.IronOre => "b58a6a",
        BlockKind.Sand => "e6be73", BlockKind.Sandstone => "c7a16a", BlockKind.CrackedEarth => "b68d62", BlockKind.Cactus => "718b42",
        BlockKind.Snow => "e4f1f5", BlockKind.Ice => "8cc7da", BlockKind.FrozenEarth => "8a827d", BlockKind.TundraWood => "877d6d", _ => "36434b"
    });
    public static float MineDuration(BlockKind kind) => kind switch
    { BlockKind.Dirt => 0.28f, BlockKind.Stone => 0.6f, BlockKind.IronOre => 0.8f, BlockKind.Wood or BlockKind.Cactus => 0.4f,
        BlockKind.Sand => 0.25f, BlockKind.Sandstone => 0.65f, BlockKind.CrackedEarth => 0.35f,
        BlockKind.Snow => 0.25f, BlockKind.Ice => 0.45f, BlockKind.FrozenEarth => 0.55f, BlockKind.TundraWood => 0.4f, _ => float.PositiveInfinity };

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        AddToGroup("block_worlds");
        for (int x = 0; x < Width; x++)
            for (int y = SurfaceRow; y < Height; y++)
            {
                if (y >= 24 && y < CaveBottomRow && x is >= 39 and <= 54) continue;
                BlockKind kind = y == Height - 1 ? BlockKind.Bedrock : y < 23 ? BlockKind.Dirt : BlockKind.Stone;
                if (x >= DesertStartColumn && kind != BlockKind.Bedrock)
                    kind = y <= 21 ? BlockKind.Sand : BlockKind.Sandstone;
                if (x >= TundraStartColumn && kind != BlockKind.Bedrock)
                    kind = y <= 20 ? BlockKind.Snow : y <= 23 ? BlockKind.FrozenEarth : BlockKind.Ice;
                // A frozen surface pond exposes ice before the player digs underground.
                if (x is >= 106 and <= 110 && y <= 21) kind = BlockKind.Ice;
                if (x is >= 72 and <= 75 && y == SurfaceRow) kind = BlockKind.CrackedEarth;
                if (x is >= 55 and <= 56 && y is >= 25 and <= 26) kind = BlockKind.IronOre;
                SetBlock(new Vector2I(x, y), kind);
            }
        foreach (int x in TreeColumns)
            for (int y = 13; y < SurfaceRow; y++) SetBlock(new Vector2I(x, y), BlockKind.Wood);
        // Surface stone outcrops offer stone without forcing a deep excavation.
        for (int x = 39; x <= 43; x++) SetBlock(new Vector2I(x, SurfaceRow - 1), BlockKind.Stone);
        for (int x = 80; x <= 84; x++) SetBlock(new Vector2I(x, SurfaceRow - 1), BlockKind.Sand);
        foreach (int x in new[] { 70, 78, 88, 93 }) SetBlock(new Vector2I(x, SurfaceRow - 1), BlockKind.Cactus);
        foreach (int x in new[] { 74, 82, 90 }) _bushes.Add(new Vector2I(x, x == 82 ? SurfaceRow - 2 : SurfaceRow - 1));
        for (int x = 116; x <= 118; x++) SetBlock(new Vector2I(x, SurfaceRow - 1), BlockKind.Snow);
        foreach (int x in PineColumns)
            for (int y = 17; y < SurfaceRow; y++) SetBlock(new Vector2I(x, y), BlockKind.TundraWood);
        foreach (int x in new[] { 97, 112, 119, 126 }) _bushes.Add(new Vector2I(x, SurfaceRow - 1));
        // Snapshot only generated solid cells. Open sky, cave air, and later player builds
        // do not create walls; mining the foreground leaves this scenery intact.
        foreach (var block in _blocks)
            if (block.Value != BlockKind.Cactus) _walls[block.Key] = block.Value;
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
        _sandDirty = true;
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
    public Vector2I? ClosestBlockAhead(Player player)
    {
        if (player.IsDead) return null;
        Vector2 origin = player.BodyCenter;
        Rect2 body = player.BodyBounds;
        Vector2I? closest = null;
        float bestDistance = MiningReach * MiningReach;
        void Consider(Vector2I cell)
        {
            Rect2 rect = CellRect(cell);
            Vector2 center = Center(cell);
            if (!InBounds(cell) || (center.X - origin.X) * player.Facing <= 0) return;
            // Stay within the body's height; a horizontal mining shortcut should not dig out its floor.
            if (rect.End.Y <= body.Position.Y || rect.Position.Y >= body.End.Y) return;
            float distance = origin.DistanceSquaredTo(center);
            if (distance > bestDistance) return;
            if (closest.HasValue && distance == bestDistance && cell.Y >= closest.Value.Y) return;
            closest = cell;
            bestDistance = distance;
        }
        foreach (Vector2I cell in _blocks.Keys) Consider(cell);
        foreach (FallingSand sand in _fallingSand) Consider(CellAt(sand.GlobalPosition));
        // Tool and protection checks happen after selection: never skip a nearer wrong-tool block.
        return closest;
    }

    public bool CanMine(Vector2I cell, Player player) => !player.IsDead && InBounds(cell) &&
        TryGetBlock(cell, out BlockKind kind) && EquipmentRules.CanMine(player.EquippedItem, kind) &&
        player.BodyCenter.DistanceTo(Center(cell)) <= MiningReach;

    public bool TryMine(Vector2I cell, Player player, out BlockKind material)
    {
        material = default;
        if (!CanMine(cell, player)) return false;
        FallingSand? falling = FallingAt(cell);
        if (_blocks.TryGetValue(cell, out material)) RemoveBlock(cell);
        else if (falling != null)
        {
            material = BlockKind.Sand;
            _fallingSand.Remove(falling);
            falling.CollisionLayer = 0;
            falling.QueueFree();
        }
        _sandDirty = true;
        QueueRedraw();
        return true;
    }

    public bool CanPlace(Vector2I cell, Player player)
    {
        if (player.IsDead || HasBlock(cell) || !CanReachForBuilding(cell, player)) return false;
        if (CellRect(cell).Intersects(player.BodyBounds)) return false;
        if (_fallingSand.Any(sand => CellRect(cell).Intersects(sand.BodyBounds))) return false;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
            if (node is Enemy enemy && !enemy.IsDead && CellRect(cell).Intersects(enemy.BodyBounds)) return false;
        return HasWall(cell) || HasBlock(cell + Vector2I.Down) || HasBlock(cell + Vector2I.Up) ||
            HasBlock(cell + Vector2I.Left) || HasBlock(cell + Vector2I.Right);
    }

    public bool TryPlace(Vector2I cell, BlockKind material, Player player, SandboxInventory inventory)
    {
        if (material == BlockKind.Bedrock || !CanPlace(cell, player) || !inventory.Spend(material)) return false;
        _bushes.Remove(cell);
        SetBlock(cell, material);
        return true;
    }

    private void RemoveBlock(Vector2I cell)
    {
        _blocks.Remove(cell);
        _bodies[cell].CollisionLayer = 0;
        _bodies[cell].QueueFree();
        _bodies.Remove(cell);
        _sandDirty = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Vector2I bush in _bushes.ToArray())
            if (!HasBlock(bush + Vector2I.Down)) { _bushes.Remove(bush); QueueRedraw(); }
        if (!_sandDirty) return;
        _sandDirty = false;
        foreach (Vector2I cell in _blocks.Where(pair => pair.Value == BlockKind.Sand)
                     .Select(pair => pair.Key).OrderByDescending(cell => cell.Y).ToArray())
        {
            // Scenery does not support sand. Release a whole unsupported column.
            if (_blocks.ContainsKey(cell + Vector2I.Down)) continue;
            RemoveBlock(cell);
            var falling = new FallingSand { World = this, Position = Center(cell) };
            _fallingSand.Add(falling);
            AddChild(falling);
            QueueRedraw();
        }
    }

    public void TrySettleSand(FallingSand sand)
    {
        if (!_fallingSand.Contains(sand) || sand.IsQueuedForDeletion()) return;
        Vector2I cell = CellAt(sand.GlobalPosition);
        if (!InBounds(cell) || _blocks.ContainsKey(cell) || !_blocks.ContainsKey(cell + Vector2I.Down)) return;
        if (sand.GlobalPosition.DistanceTo(Center(cell)) > 0.5f) return;
        Rect2 bounds = CellRect(cell);
        foreach (Node node in GetTree().GetNodesInGroup("player"))
            if (node is Player player && bounds.Intersects(player.BodyBounds)) return;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
            if (node is Enemy enemy && !enemy.IsDead && bounds.Intersects(enemy.BodyBounds)) return;
        _fallingSand.Remove(sand);
        sand.CollisionLayer = 0;
        sand.QueueFree();
        _bushes.Remove(cell);
        SetBlock(cell, BlockKind.Sand);
    }

    public void BreakBushes(Rect2 hit, Node2D? attacker = null)
    {
        foreach (Vector2I cell in _bushes.ToArray())
        {
            if (!hit.Intersects(CellRect(cell))) continue;
            if (attacker != null)
            {
                Vector2 origin = attacker is Player player ? player.BodyCenter : attacker.GlobalPosition + new Vector2(0, -24);
                var ray = PhysicsRayQueryParameters2D.Create(origin, Center(cell), 1);
                if (GetWorld2D().DirectSpaceState.IntersectRay(ray).Count != 0) continue;
            }
            _bushes.Remove(cell); // Dead bushes never produce items.
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, Width * TileSize, Height * TileSize), new Color("182f37"));
        DrawCircle(new Vector2(650, 180), 55, new Color("5e7163"));
        for (int x = 0; x < DesertStartColumn; x += 5)
        {
            float px = x * TileSize + 40;
            DrawRect(new Rect2(px, 250, 22, 240), new Color("234348"));
            DrawCircle(new Vector2(px + 11, 240), 69, new Color("234348"));
        }
        float desertX = DesertStartColumn * TileSize;
        float tundraX = TundraStartColumn * TileSize;
        DrawRect(new Rect2(desertX, 0, tundraX - desertX, Height * TileSize), new Color("b5a17d"));
        DrawCircle(new Vector2(desertX + 440, 190), 45, new Color("f7d78b"));
        DrawColoredPolygon(new[] { new Vector2(desertX, 456), new Vector2(desertX + 220, 290),
            new Vector2(desertX + 440, 390), new Vector2(tundraX, 270), new Vector2(tundraX, 456) }, new Color("a28a66"));
        float worldRight = Width * TileSize;
        DrawRect(new Rect2(tundraX, 0, worldRight - tundraX, Height * TileSize), new Color("698595"));
        DrawCircle(new Vector2(tundraX + 560, 170), 40, new Color("d9e9ed"));
        DrawColoredPolygon(new[] { new Vector2(tundraX, 456), new Vector2(tundraX + 160, 230),
            new Vector2(tundraX + 300, 350), new Vector2(tundraX + 510, 200),
            new Vector2(worldRight, 370), new Vector2(worldRight, 456) }, new Color("a5bec8"));
        for (int x = TundraStartColumn; x < Width; x += 5)
            DrawColoredPolygon(new[] { new Vector2(x * TileSize, 456), new Vector2(x * TileSize + 28, 330),
                new Vector2(x * TileSize + 56, 456) }, new Color("4d6977"));
        foreach (int x in TreeColumns)
            if (HasWall(new Vector2I(x, 13)))
            {
                bool backgroundOnly = !HasBlock(new Vector2I(x, 13));
                Color canopy = new("426951"), highlights = new("4d7658");
                if (backgroundOnly) { canopy = canopy.Darkened(0.58f); highlights = highlights.Darkened(0.58f); }
                DrawCircle(new Vector2(x * TileSize + TileSize / 2f, 294), 57, canopy);
                DrawCircle(new Vector2(x * TileSize - 10, 282), 35, highlights);
            }
        foreach (var entry in _walls)
        {
            if (_blocks.ContainsKey(entry.Key)) continue; // Moving sand still needs scenery behind it.
            Rect2 rect = CellRect(entry.Key);
            BlockArt.DrawWall(this, rect, entry.Value);
        }
        foreach (int x in PineColumns)
        {
            if (!HasWall(new Vector2I(x, 17))) continue;
            Color tint = _blocks.TryGetValue(new Vector2I(x, 17), out BlockKind trunk) && trunk == BlockKind.TundraWood ? Colors.White : new Color(0.42f, 0.42f, 0.42f);
            // Crop transparent padding and scale by six whole pixels to ground the pine sprite.
            DrawTextureRectRegion(BlockArt.PineTexture, new Rect2(x * TileSize - 24, SurfaceRow * TileSize - 102, 72, 102),
                new Rect2(6, 4, 12, 17), tint);
        }
        foreach (var entry in _blocks)
        {
            // The live pine sprite represents its original trunk cells. Other foreground blocks stay in front.
            if (entry.Value == BlockKind.TundraWood && entry.Key.Y is 17 or 18 && PineColumns.Contains(entry.Key.X)
                && _blocks.TryGetValue(new Vector2I(entry.Key.X, 17), out BlockKind top) && top == BlockKind.TundraWood) continue;
            Rect2 rect = CellRect(entry.Key);
            BlockArt.Draw(this, rect, entry.Value);
            if (entry.Value == BlockKind.Dirt && !HasBlock(entry.Key + Vector2I.Up))
                DrawRect(new Rect2(rect.Position, new Vector2(TileSize, 3)), new Color("8ca773"));
        }
        foreach (Vector2I bush in _bushes)
            DrawTextureRect(bush.X >= TundraStartColumn ? BlockArt.ShrubTexture : BlockArt.BushTexture, CellRect(bush), false);
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
