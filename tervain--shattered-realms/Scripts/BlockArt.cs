using Godot;
using System.Collections.Generic;

namespace Tervain;

/// <summary>Shared native-size pixel art for terrain, walls, pickups, and equipment.</summary>
public static class BlockArt
{
	private static readonly Dictionary<BlockKind, Texture2D> Textures = new();
	private static readonly Dictionary<string, Texture2D> WallTextures = new();

	public static void DrawWall(CanvasItem canvas, Rect2 rect, BlockKind kind)
	{
		if (BiomeCatalog.Materials.TryGetValue(kind, out BiomeMaterial? material))
		{
			BiomeArt.DrawWall(canvas, rect, material);
			return;
		}
		// There is no separate wood-wall tile yet; keep a recessed version of the trunk.
		if (kind is BlockKind.Wood or BlockKind.TundraWood)
		{
			TileArt.Draw(canvas, Texture(kind)!, rect, new Color(0.42f, 0.42f, 0.42f));
			return;
		}
		string name = kind switch
		{ BlockKind.Dirt => "dirt", BlockKind.Sand => "sand",
			BlockKind.Snow or BlockKind.FrozenEarth => "snow", BlockKind.Ice => "ice",
			BlockKind.Sandstone or BlockKind.CrackedEarth => "sandstone", _ => "cave" };
		if (!WallTextures.TryGetValue(name, out Texture2D? texture))
		{
			texture = GD.Load<Texture2D>($"res://Art-work/Pixel16/Exported/{name}-background-16.png");
			WallTextures[name] = texture;
		}
		// These tiles already include their background shading.
		TileArt.Draw(canvas, texture, rect);
	}

	public static Texture2D? Texture(BlockKind kind)
	{
		if (BiomeCatalog.Materials.TryGetValue(kind, out BiomeMaterial? material)) return BiomeArt.Texture(material.Texture);
		string? name = kind switch
		{
			BlockKind.Dirt => "dirt-block", BlockKind.Stone => "stone-block",
			BlockKind.Wood => "wood-block", BlockKind.IronOre => "iron-ore-block",
			BlockKind.Sand => "sand-block", BlockKind.Sandstone => "sandstone-block",
			BlockKind.CrackedEarth => "cracked-earth-block", BlockKind.Cactus => "cactus",
			BlockKind.Snow => "snow-block", BlockKind.Ice => "ice-block", BlockKind.FrozenEarth => "frozen-earth-block",
			BlockKind.TundraWood => "tundra-wood-block", _ => null
		};
		if (name == null) return null; // Bedrock retains its distinct protected-floor appearance.
		if (!Textures.TryGetValue(kind, out Texture2D? texture))
		{
			texture = GD.Load<Texture2D>($"res://Art-work/Pixel16/Exported/{name}-16.png");
			Textures[kind] = texture;
		}
		return texture;
	}

	private static Texture2D? _pineTexture, _shrubTexture;
	public static Texture2D PineTexture => _pineTexture ??= GD.Load<Texture2D>("res://Art-work/Pixel16/Exported/snow-pine-16.png");
	public static Texture2D ShrubTexture => _shrubTexture ??= GD.Load<Texture2D>("res://Art-work/Pixel16/Exported/tundra-shrub-16.png");

	private static Texture2D? _bushTexture;
	public static Texture2D BushTexture => _bushTexture ??= GD.Load<Texture2D>("res://Art-work/Pixel16/Exported/dry-bush-16.png");

	public static void Draw(CanvasItem canvas, Rect2 rect, BlockKind kind, Color? tint = null)
	{
		Texture2D? texture = Texture(kind);
		if (texture != null) canvas.DrawTextureRect(texture, rect, false, tint ?? Colors.White);
		else canvas.DrawRect(rect, BlockWorld.Colour(kind) * (tint ?? Colors.White));
	}
}
