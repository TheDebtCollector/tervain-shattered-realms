using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tervain;

public partial class BlockWorld
{
    private readonly record struct Decoration(Vector2I Anchor, string Texture, int Scale, bool Hanging);
    private readonly Dictionary<Vector2I, (string Texture, bool Surface)> _liquids = new();
    private readonly List<Decoration> _details = new();
    private readonly List<(Vector2I Top, string Crown, bool Conifer)> _biomeTrees = new();
    private int _collisionCenter = -10000, _collisionSandCount = -1;
    private bool IsSolidCell(Vector2I cell) => _blocks.ContainsKey(cell) && !_vegetation.Contains(cell);
    private bool IsCollisionColumnActive(int column) => Math.Abs(column - _collisionCenter) <= 40
        || _fallingSand.Any(sand => Math.Abs(column - CellAt(sand.Position).X) <= 4);
    public void UpdateNearbyCollision(Vector2 position)
    {
        int center = CellAt(position).X / 8 * 8;
        if (center == _collisionCenter && _collisionSandCount == _fallingSand.Count) return;
        _collisionCenter = center;
        _collisionSandCount = _fallingSand.Count;
        // Keep the accepted baseline active; stream added terrain around the player and moving sand.
        foreach (var pair in _bodies.Where(p => p.Key.X >= LegacyWidth && !IsCollisionColumnActive(p.Key.X)).ToArray())
        {
            pair.Value.CollisionLayer = 0;
            pair.Value.QueueFree();
            _bodies.Remove(pair.Key);
        }
        foreach (Vector2I cell in _blocks.Keys)
            if (cell.X >= LegacyWidth && IsSolidCell(cell) && IsCollisionColumnActive(cell.X) && !_bodies.ContainsKey(cell))
                AddBlockCollider(cell);
    }
    public int BiomeCount => BiomeCatalog.All.Length;
    public int ActiveColliderCount => _bodies.Count;
    public bool HasLiquid(Vector2I cell) => _liquids.ContainsKey(cell);
    public int BiomeAt(Vector2 position) => BiomeCatalog.IndexAtColumn(CellAt(position).X);
    private static int CaveCeiling(int local) => 10 + Math.Max(0, 2 - Math.Abs(local / 3 % 8 - 4));

    private int TrailSurface(BiomeDefinition biome, int local)
    {
        // Keep the connection lanes level; all steps are one block and fit the accepted jump.
        if (local < 4 || local >= BiomeWidth - 4) return SurfaceRow;
        if (biome.Underground) return SurfaceRow + Math.Min(3, Math.Min(local - 3, BiomeWidth - 4 - local));
        if (biome.Profile == "alpine") return SurfaceRow - Math.Max(0, 4 - Math.Abs(local - 17) / 3);
        if (biome.Profile == "mesa") return SurfaceRow - (local is >= 13 and <= 22 ? 3 : local is >= 10 and <= 25 ? 2 : local is >= 7 and <= 27 ? 1 : 0);
        if (biome.Profile == "shattered") return SurfaceRow - (local is >= 17 and <= 24 ? 2 : local is >= 14 and <= 27 ? 1 : 0);
        return SurfaceRow + (local is >= 12 and <= 21 ? 1 : 0);
    }

    private void BuildBiomeTrail()
    {
        for (int index = 3; index < BiomeCatalog.All.Length; index++)
        {
            BiomeDefinition biome = BiomeCatalog.All[index];
            int start = BiomeCatalog.Start(index);
            for (int local = 0; local < BiomeWidth; local++)
            {
                int x = start + local;
                int surface = TrailSurface(biome, local);
                bool pond = local is >= 29 and <= 34;
                int pondRim = SurfaceRow + (biome.Underground ? 3 : 0);
                int floor = pond ? pondRim + (local is 29 or 34 ? 2 : local is 30 or 33 ? 3 : 4) : surface;
                for (int y = floor; y < Height; y++)
                    SetBlock(new Vector2I(x, y), y == Height - 1 ? BlockKind.Bedrock
                        : (BlockKind)(y < floor + 3 ? biome.Soil : biome.Stone));
                if (pond)
                    for (int y = pondRim + 1; y < floor; y++)
                        _liquids[new Vector2I(x, y)] = (y == pondRim + 1 ? biome.LiquidSurface : biome.Liquid, y == pondRim + 1);
                if (biome.Underground)
                {
                    int ceiling = CaveCeiling(local);
                    for (int y = 4; y <= ceiling; y++) SetBlock(new Vector2I(x, y), (BlockKind)biome.Stone);
                    for (int y = ceiling + 1; y < floor; y++) _walls[new Vector2I(x, y)] = (BlockKind)biome.Stone;
                }
            }
            // The material outcrop exposes the third material even before the player excavates.
            for (int local = 37; local <= 39; local++)
                SetBlock(new Vector2I(start + local, TrailSurface(biome, local) - 1), (BlockKind)biome.Material);
            if (biome.Trees)
                foreach (int local in new[] { 7, 23, 41 })
                {
                    int floor = TrailSurface(biome, local);
                    for (int y = floor - 4; y < floor; y++)
                        SetBlock(new Vector2I(start + local, y), (BlockKind)biome.Material, vegetation: true);
                    _biomeTrees.Add((new Vector2I(start + local, floor - 4), biome.Decorations[0], biome.Id is "taiga" or "alpine"));
                }
            else
                _details.Add(new Decoration(new Vector2I(start + 7, TrailSurface(biome, 7)), biome.Decorations[0], 2, false));
            foreach (int local in new[] { 5, 15, 26, 43 })
                _details.Add(new Decoration(new Vector2I(start + local, TrailSurface(biome, local)), biome.Decorations[1], 1, false));
            foreach (int local in new[] { 11, 35 })
                _details.Add(new Decoration(new Vector2I(start + local, TrailSurface(biome, local)), biome.Decorations[2], 2, false));
            if (biome.Underground)
                foreach (int local in new[] { 9, 21, 39 })
                    _details.Add(new Decoration(new Vector2I(start + local, CaveCeiling(local) + 1), biome.Decorations[3], 2, true));
            else
            {
                // A small supported overhang gives hanging roots/vines a visible attachment.
                for (int local = 44; local <= 46; local++)
                    for (int y = 14; y <= 15; y++) SetBlock(new Vector2I(start + local, y), (BlockKind)biome.Stone);
                for (int y = 16; y < TrailSurface(biome, 46); y++) SetBlock(new Vector2I(start + 46, y), (BlockKind)biome.Stone);
                _details.Add(new Decoration(new Vector2I(start + 44, 16), biome.Decorations[3], 2, true));
            }
            if (biome.Profile == "shattered")
            {
                for (int local = 16; local <= 20; local++)
                {
                    SetBlock(new Vector2I(start + local, 11), (BlockKind)biome.Soil);
                    SetBlock(new Vector2I(start + local, 12), (BlockKind)biome.Stone);
                    if (local is >= 17 and <= 19) SetBlock(new Vector2I(start + local, 13), (BlockKind)biome.Stone);
                }
                _details.Add(new Decoration(new Vector2I(start + 18, 14), biome.Decorations[3], 2, true));
            }
        }
    }

    public Vector2 ArrivalFor(int index)
    {
        int preferred = index == 0 ? 14 : index == 1 ? 80 : index == 2 ? 113 : BiomeCatalog.Start(index) + 24;
        int start = BiomeCatalog.Start(index), end = BiomeCatalog.End(index);
        for (int distance = 0; distance < end - start; distance++)
            foreach (int x in new[] { preferred + distance, preferred - distance })
            {
                if (x <= start || x >= end - 1) continue;
                for (int y = 15; y < Height - 1; y++)
                {
                    if (!IsSolidCell(new Vector2I(x, y)) || HasLiquid(new Vector2I(x, y - 1))) continue;
                    Vector2 feet = new((x + 0.5f) * TileSize, y * TileSize);
                    var bounds = new Rect2(feet + new Vector2(-Player.BodyWidth / 2, -Player.BodyHeight), new Vector2(Player.BodyWidth, Player.BodyHeight - 0.1f));
                    bool blocked = false;
                    for (int bx = CellAt(bounds.Position).X; bx <= CellAt(bounds.End - Vector2.One * 0.01f).X; bx++)
                        for (int by = CellAt(bounds.Position).Y; by < y; by++)
                            if (IsSolidCell(new Vector2I(bx, by)) && bounds.Intersects(CellRect(new Vector2I(bx, by)))) blocked = true;
                    if (!blocked) return feet;
                }
            }
        throw new InvalidOperationException("There is no clear arrival point in " + BiomeCatalog.All[index].Name);
    }

    private Rect2 VisibleWorld()
    {
        Transform2D inverse = GetCanvasTransform().AffineInverse();
        return new Rect2(inverse * Vector2.Zero, GetViewportRect().Size / GetCanvasTransform().Scale).Grow(160);
    }

    private Color BiomeColor(float column, Func<BiomeDefinition, string> choose)
    {
        int index = BiomeCatalog.IndexAtColumn((int)column);
        Color color = new(choose(BiomeCatalog.All[index]));
        const float blend = 5;
        float left = column - BiomeCatalog.Start(index), right = BiomeCatalog.End(index) - column;
        if (index > 0 && left < blend)
            return new Color(choose(BiomeCatalog.All[index - 1])).Lerp(color, 0.5f + left / (2 * blend));
        if (index + 1 < BiomeCount && right < blend)
            return color.Lerp(new Color(choose(BiomeCatalog.All[index + 1])), 0.5f - right / (2 * blend));
        return color;
    }

    private void DrawBiomeScenery(Rect2 visible)
    {
        int first = Math.Max(0, Mathf.FloorToInt(visible.Position.X / TileSize));
        int last = Math.Min(Width - 1, Mathf.CeilToInt(visible.End.X / TileSize));
        for (int x = first; x <= last; x++)
        {
            Color sky = BiomeColor(x + 0.5f, b => b.Sky), horizon = BiomeColor(x + 0.5f, b => b.Horizon);
            for (int y = Mathf.FloorToInt(visible.Position.Y / TileSize); y * TileSize < visible.End.Y; y++)
            {
                float t = Mathf.Clamp((y + 5f) / (SurfaceRow + 9f), 0, 1);
                Color color = sky.Lerp(horizon, t);
                if (y >= SurfaceRow) color = color.Darkened(Mathf.Clamp((y - SurfaceRow) / 12f, 0, 0.72f));
                DrawRect(new Rect2(x * TileSize, y * TileSize, TileSize, TileSize), color);
            }
        }
        for (int x = first / 3 * 3; x <= last; x += 3)
        {
            BiomeDefinition biome = BiomeCatalog.All[BiomeCatalog.IndexAtColumn(x)];
            Color shade = BiomeColor(x, b => b.Silhouette);
            int px = x * TileSize;
            int ridge = 15 * TileSize + Mathf.RoundToInt(Mathf.Sin(x * 0.17f) * TileSize + Mathf.Sin(x * 0.07f) * TileSize * 1.5f);
            if (biome.Profile == "shattered")
            {
                int top = 8 * TileSize + (x * 13 % 5) * TileSize;
                DrawRect(new Rect2(px, top, TileSize * 2.5f, TileSize / 2f), new Color(shade, 0.5f));
                DrawRect(new Rect2(px + TileSize / 2f, top + TileSize / 2f, TileSize * 1.5f, TileSize / 2f), new Color(shade, 0.5f));
                DrawRect(new Rect2(px + TileSize, top + TileSize, TileSize / 2f, TileSize / 2f), new Color(shade, 0.5f));
                continue;
            }
            DrawRect(new Rect2(px, ridge, 3 * TileSize, Height * TileSize - ridge), new Color(shade, 0.44f));
            if (biome.Trees && biome.Profile != "alpine")
            {
                int top = 10 * TileSize + ((x * 13) % 4) * TileSize / 2;
                Color silhouette = new(shade, 0.38f);
                DrawRect(new Rect2(px + TileSize * 1.25f, top + TileSize * 1.5f, TileSize / 2f, TileSize * 8.75f), silhouette);
                DrawRect(new Rect2(px + TileSize / 2f, top - TileSize / 2f, TileSize * 2, TileSize * 3), silhouette);
                DrawRect(new Rect2(px, top + TileSize / 2f, TileSize * 3, TileSize * 1.5f), silhouette);
            }
            else if (!biome.Underground)
                for (int band = 0; band < 6; band++)
                    DrawRect(new Rect2(px + TileSize * (1.5f - band * 0.25f), ridge - TileSize * (4 - band * 0.75f), TileSize * (0.5f + band * 0.5f), TileSize), new Color(shade, 0.5f));
        }
    }

    private void DrawBiomeDetails(Rect2 visible)
    {
        foreach (var tree in _biomeTrees)
        {
            if (!HasWall(tree.Top)) continue;
            var rect = new Rect2((tree.Top.X - 1.5f) * TileSize, (tree.Top.Y - 2) * TileSize, 4 * TileSize, 4 * TileSize);
            if (!visible.Intersects(rect)) continue;
            Color tint = _vegetation.Contains(tree.Top) ? Colors.White : new Color(0.42f, 0.42f, 0.42f);
            if (tree.Conifer)
            {
                DrawTextureRect(BiomeArt.Texture(tree.Crown), new Rect2(rect.Position + new Vector2(1, -0.5f) * TileSize, Vector2.One * TileSize * 2), false, tint);
                DrawTextureRect(BiomeArt.Texture(tree.Crown), new Rect2(rect.Position + Vector2.One * TileSize / 2, Vector2.One * TileSize * 3), false, tint);
            }
            else
            {
                DrawTextureRect(BiomeArt.Texture(tree.Crown), new Rect2(rect.Position + new Vector2(-1, 0.75f) * TileSize, Vector2.One * TileSize * 3), false, tint);
                DrawTextureRect(BiomeArt.Texture(tree.Crown), new Rect2(rect.Position + new Vector2(2, 0.75f) * TileSize, Vector2.One * TileSize * 3), false, tint);
            }
            DrawTextureRect(BiomeArt.Texture(tree.Crown), rect, false, tint);
        }
        foreach (Decoration detail in _details)
        {
            Vector2I support = detail.Hanging ? detail.Anchor + Vector2I.Up : detail.Anchor;
            if (!IsSolidCell(support)) continue;
            float size = TileSize * detail.Scale;
            var rect = new Rect2(new Vector2(detail.Anchor.X * TileSize + (TileSize - size) / 2,
                detail.Anchor.Y * TileSize - (detail.Hanging ? 0 : size)), Vector2.One * size);
            if (!visible.Intersects(rect)) continue;
            // Buildable foreground always wins over decoration silhouettes.
            bool covered = false;
            Vector2I top = CellAt(rect.Position), end = CellAt(rect.End - Vector2.One);
            for (int x = top.X; x <= end.X; x++)
                for (int y = top.Y; y <= end.Y; y++)
                    if (IsSolidCell(new Vector2I(x, y))) covered = true;
            if (!covered) DrawTextureRect(BiomeArt.Texture(detail.Texture), rect, false);
        }
    }

    private void DrawTerrainBlock(Vector2I cell, BlockKind kind, Rect2 rect)
    {
        if (BiomeCatalog.Materials.TryGetValue(kind, out BiomeMaterial? material))
        {
            bool surface = material.Surface.Length > 0 && !HasBlock(cell + Vector2I.Up) && !HasLiquid(cell + Vector2I.Up);
            TileArt.Draw(this, BiomeArt.Texture(surface ? material.Surface : material.Texture), rect);
        }
        else if (BlockArt.Texture(kind) is Texture2D texture) TileArt.Draw(this, texture, rect);
        else BlockArt.Draw(this, rect, kind);
    }
}
