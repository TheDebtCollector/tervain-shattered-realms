using Godot;
using System;

namespace Tervain;

public partial class SandboxHud : CanvasLayer
{
    public Action<int>? SlotSelected { get; set; }
    public Action<int>? BiomeSelected { get; set; }
    private readonly Button[] _slots = new Button[EquipmentRules.SlotCount];
    private Label _status = null!, _target = null!, _notice = null!, _enemyStatus = null!;
    private float _noticeLeft;
    private ScrollContainer _hotbarScroll = null!;
    private int _lastSelected = -1;
    private OptionButton _biomes = null!;
    private Label _biomeName = null!;
    public override void _Ready()
    {
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(root);
        var header = new ColorRect { Color = new Color(0.05f, 0.11f, 0.14f, 0.94f), MouseFilter = Control.MouseFilterEnum.Ignore };
        header.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
        header.OffsetBottom = 114;
        root.AddChild(header);
        Label Text(string text, Vector2 position, int size, string colour)
        {
            var label = new Label { Text = text, Position = position, MouseFilter = Control.MouseFilterEnum.Ignore };
            label.AddThemeFontSizeOverride("font_size", size);
            label.AddThemeColorOverride("font_color", new Color(colour));
            root.AddChild(label);
            return label;
        }
        Text("TERVAIN / BUILD · EXPLORE · FIGHT", new Vector2(28, 14), 13, "94bcb0");
        _biomeName = Text("Plains", new Vector2(26, 33), 28, "f1e5cb");
        Text("EXPLORE THE BIOMES", new Vector2(452, 15), 12, "94bcb0");
        _biomes = new OptionButton { Position = new Vector2(448, 37), Size = new Vector2(320, 34),
            FocusMode = Control.FocusModeEnum.None, TooltipText = "Travel to a biome. Page Up / Page Down also cycle through the trail." };
        _biomes.AddThemeFontSizeOverride("font_size", 15);
        _biomes.GetPopup().MaxSize = new Vector2I(420, 460);
        for (int i = 0; i < BiomeCatalog.All.Length; i++)
        {
            BiomeDefinition biome = BiomeCatalog.All[i];
            _biomes.AddItem($"{i + 1:00}  {biome.Name}", i);
        }
        _biomes.ItemSelected += index => BiomeSelected?.Invoke((int)index);
        root.AddChild(_biomes);
        Text("1 Pickaxe: terrain / ore   2 Axe: wood / cactus   3 Sword   ·   Hit dead bushes with any weapon", new Vector2(28, 77), 14, "b5c7bd");
        _status = Text("", new Vector2(825, 27), 16, "b3e9d8");
        _enemyStatus = Text("", new Vector2(825, 83), 12, "f4c396");
        _target = Text("", new Vector2(28, 124), 17, "e4e3c8");
        _notice = Text("", new Vector2(28, 151), 15, "ffcb8a");
        foreach (Label label in new[] { _target, _notice })
        {
            label.AddThemeColorOverride("font_outline_color", new Color(0.04f, 0.07f, 0.08f, 0.9f));
            label.AddThemeConstantOverride("outline_size", 3);
        }

        var footer = new ColorRect { Color = new Color(0.05f, 0.11f, 0.14f, 0.94f), MouseFilter = Control.MouseFilterEnum.Ignore };
        footer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        footer.OffsetTop = -142;
        root.AddChild(footer);
        _hotbarScroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled, MouseFilter = Control.MouseFilterEnum.Ignore };
        _hotbarScroll.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        _hotbarScroll.OffsetLeft = 28;
        _hotbarScroll.OffsetRight = -28;
        _hotbarScroll.OffsetTop = -134;
        _hotbarScroll.OffsetBottom = -58;
        root.AddChild(_hotbarScroll);
        var hotbar = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        hotbar.AddThemeConstantOverride("separation", 6);
        _hotbarScroll.AddChild(hotbar);
        root.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        // Native controls retain these shared resources; release the temporary C# owners after setup.
        using var normal = Box("263e43", "46615e", 1);
        using var hover = Box("344e50", "91b5a4", 1);
        using var pressed = Box("405b50", "d6d6a0", 3);
        using var hoverPressed = Box("496454", "e6e0af", 3);
        for (int i = 0; i < EquipmentRules.SlotCount; i++)
        {
            int slot = i;
            var button = new Button
            {
                CustomMinimumSize = new Vector2(94, 68), ToggleMode = true,
                FocusMode = Control.FocusModeEnum.None,
                TooltipText = i < 2 ? $"Left click or J to swing and damage enemies. Hold left mouse to harvest {EquipmentRules.Purpose((EquipmentKind)i).ToLowerInvariant()} within {BlockWorld.MiningReach / BlockWorld.TileSize:0} blocks."
                    : i == 2 ? "Left click or J to swing the equipped sword."
                    : EquipmentRules.Name(EquipmentRules.AtSlot(i)) + " · Left or right click to place this material."
            };
            if (EquipmentRules.TryGetMaterial(EquipmentRules.AtSlot(i), out BlockKind material))
            {
                button.Icon = BlockArt.Texture(material);
                button.IconAlignment = HorizontalAlignment.Center;
                button.VerticalIconAlignment = VerticalAlignment.Top;
                button.AddThemeConstantOverride("icon_max_width", 24);
            }
            button.AddThemeFontSizeOverride("font_size", 12);
            button.AddThemeStyleboxOverride("normal", normal);
            button.AddThemeStyleboxOverride("hover", hover);
            button.AddThemeStyleboxOverride("pressed", pressed);
            button.AddThemeStyleboxOverride("hover_pressed", hoverPressed);
            button.Pressed += () => SlotSelected?.Invoke(slot);
            _slots[i] = button;
            hotbar.AddChild(button);
        }
        var controls = Text("A/D Move   SPACE Jump   SHIFT Dodge   CTRL Mine ahead   1–9 / 0 / − / Wheel Select   LMB Use   J Swing   RMB Build",
            Vector2.Zero, 12, "dfddc8");
        controls.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        controls.OffsetLeft = 28; controls.OffsetTop = -45; controls.OffsetRight = -20; controls.OffsetBottom = -26;
        var warning = Text("Page Up / Down: Travel biomes   ·   R Reset   ·   Progress is not saved   ·   F2 Combat room",
            Vector2.Zero, 11, "92ada7");
        warning.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        warning.OffsetLeft = 28; warning.OffsetTop = -22; warning.OffsetRight = -20; warning.OffsetBottom = -4;
    }

    private static StyleBoxFlat Box(string fill, string border, int width) => new()
    {
        BgColor = new Color(fill), BorderColor = new Color(border),
        BorderWidthLeft = width, BorderWidthRight = width, BorderWidthTop = width, BorderWidthBottom = width,
        CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5, CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5
    };

    public void Refresh(Player player, Enemy enemy, SandboxInventory inventory, int selected, string target, int biomeIndex)
    {
        _biomeName.Text = BiomeCatalog.All[biomeIndex].Name;
        if (_biomes.Selected != biomeIndex) _biomes.Select(biomeIndex);
        _status.Text = $"HEALTH  {player.Health}/100\n" + (player.DodgeReadyIn <= 0 ? "DODGE  Ready" : $"DODGE  {player.DodgeReadyIn:0.0}s");
        _target.Text = player.IsDead ? "You fell. Press R to reset this unsaved world." : target;
        _enemyStatus.Text = player.Position.DistanceTo(enemy.Position) < 600 ? $"CREATURE {enemy.Health}/100 · {enemy.StateText}"
            : BiomeCatalog.All[biomeIndex].Underground ? "CAVERNS · EXPLORE"
            : BiomeCatalog.All[biomeIndex].Category == "fantasy" ? "REALM · EXPLORE" : "WILDERNESS · EXPLORE";
        for (int i = 0; i < EquipmentRules.SlotCount; i++)
        {
            EquipmentKind item = EquipmentRules.AtSlot(i);
            string detail = EquipmentRules.TryGetMaterial(item, out BlockKind material)
                ? inventory.Count(material).ToString() : EquipmentRules.Purpose(item).Replace(" / ", " /\n");
            string name = EquipmentRules.Name(item).ToUpperInvariant();
            if (name.Length > 12) name = name[..11] + "…";
            _slots[i].Text = $"{EquipmentRules.Shortcut(i)} {name}\n{detail}";
            _slots[i].Visible = i < 3 || i == selected ||
                (EquipmentRules.TryGetMaterial(item, out BlockKind owned) && inventory.Count(owned) > 0);
            _slots[i].SetPressedNoSignal(i == selected);
        }
        if (_lastSelected != selected)
        {
            _hotbarScroll.EnsureControlVisible(_slots[selected]);
            _lastSelected = selected;
        }
    }

    public void Notify(string message) { _notice.Text = message; _noticeLeft = 2.5f; }
    public override void _Process(double delta)
    {
        _noticeLeft -= (float)delta;
        if (_noticeLeft <= 0) _notice.Text = "";
    }
}
