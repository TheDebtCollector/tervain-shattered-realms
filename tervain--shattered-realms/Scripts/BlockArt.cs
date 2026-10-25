using Godot;
using System.Collections.Generic;

namespace Tervain;

/// <summary>Shared native-size pixel art for terrain, walls, pickups, and equipment.</summary>
public static class BlockArt
{
    private static readonly Dictionary<BlockKind, Texture2D> Textures = new();

    public static Texture2D? Texture(BlockKind kind)
    {
        string? name = kind switch
        {
            BlockKind.Dirt => "dirt", BlockKind.Stone => "stone",
            BlockKind.Wood => "wood", BlockKind.IronOre => "iron-ore", _ => null
        };
        if (name == null) return null; // Bedrock retains its distinct protected-floor appearance.
        if (!Textures.TryGetValue(kind, out Texture2D? texture))
        {
            texture = GD.Load<Texture2D>($"res://Art-work/Exported/{name}-block-24.png");
            Textures[kind] = texture;
        }
        return texture;
    }

    public static void Draw(CanvasItem canvas, Rect2 rect, BlockKind kind, Color? tint = null)
    {
        Texture2D? texture = Texture(kind);
        if (texture != null) canvas.DrawTextureRect(texture, rect, false, tint ?? Colors.White);
        else canvas.DrawRect(rect, BlockWorld.Colour(kind) * (tint ?? Colors.White));
    }
}
