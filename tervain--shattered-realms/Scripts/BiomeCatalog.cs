using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Tervain;

public sealed class BiomeMaterial
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Texture { get; set; } = "";
    public string Surface { get; set; } = "";
    public string Wall { get; set; } = "";
    public bool Wood { get; set; }
    public float Seconds { get; set; } = 0.5f;
    public BlockKind Kind => (BlockKind)Id;
}

public sealed class BiomeDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "surface";
    public string Profile { get; set; } = "woodland";
    public string Sky { get; set; } = "263f48";
    public string Horizon { get; set; } = "56675f";
    public string Silhouette { get; set; } = "344d46";
    public int Soil { get; set; }
    public int Stone { get; set; }
    public int Material { get; set; }
    public bool Trees { get; set; }
    public string[] Decorations { get; set; } = Array.Empty<string>();
    public string Liquid { get; set; } = "";
    public string LiquidSurface { get; set; } = "";
    public BiomeMaterial[] Materials { get; set; } = Array.Empty<BiomeMaterial>();
    public bool Underground => Category == "underground";
}

/// <summary>Stable content IDs and data definitions, independent of terrain layout and structures.</summary>
public static class BiomeCatalog
{
    public static readonly BiomeDefinition[] All = Load();
    public static readonly IReadOnlyDictionary<BlockKind, BiomeMaterial> Materials = All
        .SelectMany(b => b.Materials).ToDictionary(m => m.Kind);

    private static BiomeDefinition[] Load()
    {
        using var file = FileAccess.Open("res://Data/biomes.json", FileAccess.ModeFlags.Read)
            ?? throw new InvalidOperationException("Biome definitions are missing.");
        var biomes = JsonSerializer.Deserialize<BiomeDefinition[]>(file.GetAsText(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Biome definitions could not be read.");
        if (biomes.Length < 3 || biomes.Select(b => b.Id).Distinct().Count() != biomes.Length)
            throw new InvalidOperationException("Biome IDs must be unique and include the baseline biomes.");
        return biomes;
    }

    public static int IndexAtColumn(int column) => column < BlockWorld.DesertStartColumn ? 0
        : column < BlockWorld.TundraStartColumn ? 1 : column < BlockWorld.LegacyWidth ? 2
        : Math.Clamp(3 + (column - BlockWorld.LegacyWidth) / BlockWorld.BiomeWidth, 3, All.Length - 1);
    public static int Start(int index) => index switch
    { 0 => 0, 1 => BlockWorld.DesertStartColumn, 2 => BlockWorld.TundraStartColumn,
        _ => BlockWorld.LegacyWidth + (index - 3) * BlockWorld.BiomeWidth };
    public static int End(int index) => index + 1 < All.Length ? Start(index + 1) : BlockWorld.Width;
}

public static class BiomeArt
{
    private static readonly Dictionary<string, Texture2D> Textures = new();
    public static Texture2D Texture(string path)
    {
        if (!Textures.TryGetValue(path, out Texture2D? texture))
        {
            texture = GD.Load<Texture2D>(path) ?? throw new InvalidOperationException($"Missing biome art: {path}");
            Textures[path] = texture;
        }
        return texture;
    }
    public static void DrawWall(CanvasItem canvas, Rect2 rect, BiomeMaterial material)
    {
        TileArt.Draw(canvas, Texture(material.Wall), rect,
            material.Wood ? new Color(0.42f, 0.42f, 0.42f) : new Color(0.66f, 0.66f, 0.66f));
    }
}
