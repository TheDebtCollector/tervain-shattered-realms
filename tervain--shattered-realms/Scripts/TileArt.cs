using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Tervain;

/// <summary>Stable, position-based variations for seamless 16px terrain; icons use the base tile.</summary>
public static class TileArt
{
    private static readonly Dictionary<string, string[]> VariantPaths = Load();
    private static readonly Dictionary<string, Texture2D[]> Textures = new();

    private static Dictionary<string, string[]> Load()
    {
        using var file = FileAccess.Open("res://Art-work/Pixel16/manifest.json", FileAccess.ModeFlags.Read)
            ?? throw new InvalidOperationException("16px art manifest is missing.");
        using var doc = JsonDocument.Parse(file.GetAsText());
        var result = new Dictionary<string, string[]>();
        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            var variants = entry.GetProperty("variants");
            if (variants.GetArrayLength() == 0) continue;
            string path = entry.GetProperty("png").GetString()!;
            var paths = new List<string> { path };
            foreach (var variant in variants.EnumerateArray()) paths.Add(variant.GetString()!);
            result.Add(path, paths.ToArray());
        }
        return result;
    }

    public static int VariantIndex(Vector2I cell)
    {
        unchecked
        {
            uint hash = (uint)cell.X * 0x8DA6B343u ^ (uint)cell.Y * 0xD8163841u;
            hash ^= hash >> 16;
            hash *= 0x7FEB352Du;
            hash ^= hash >> 15;
            return (int)(hash & 3);
        }
    }

    public static void Draw(CanvasItem canvas, Texture2D texture, Rect2 rect, Color? tint = null)
    {
        string path = texture.ResourcePath;
        if (VariantPaths.TryGetValue(path, out string[]? paths))
        {
            if (!Textures.TryGetValue(path, out Texture2D[]? set))
            {
                set = new Texture2D[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                    set[i] = GD.Load<Texture2D>(paths[i]) ?? throw new InvalidOperationException($"Missing tile variant: {paths[i]}");
                Textures[path] = set;
            }
            var cell = new Vector2I(Mathf.FloorToInt(rect.Position.X / BlockWorld.TileSize),
                Mathf.FloorToInt(rect.Position.Y / BlockWorld.TileSize));
            texture = set[VariantIndex(cell)];
        }
        canvas.DrawTextureRect(texture, rect, false, tint ?? Colors.White);
    }
}