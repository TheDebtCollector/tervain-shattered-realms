using Godot;
using System;

namespace Tervain;

public partial class SandboxHud : CanvasLayer
{
    public Action<int>? SlotSelected { get; set; }
    private readonly Button[] _slots = new Button[EquipmentRules.SlotCount];
    private Label _status = null!, _target = null!, _notice = null!, _enemyStatus = null!;
    private float _noticeLeft;
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
        Text("A place to call home", new Vector2(26, 33), 28, "f1e5cb");
        Text("1 Pickaxe: dirt / stone   2 Axe: wood   3 Sword   ·   All three can fight", new Vector2(28, 77), 16, "b5c7bd");
        _status = Text("", new Vector2(825, 27), 16, "b3e9d8");
        _enemyStatus = Text("", new Vector2(825, 83), 12, "f4c396");
        _target = Text("", new Vector2(28, 124), 17, "e4e3c8");
        _notice = Text("", new Vector2(28, 151), 15, "ffcb8a");

        var footer = new ColorRect { Color = new Color(0.05f, 0.11f, 0.14f, 0.94f), MouseFilter = Control.MouseFilterEnum.Ignore };
        footer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        footer.OffsetTop = -142;
        root.AddChild(footer);
        var hotbar = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        hotbar.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterBottom);
        float halfWidth = (EquipmentRules.SlotCount * 115 + (EquipmentRules.SlotCount - 1) * 8) / 2f;
        hotbar.OffsetLeft = -halfWidth;
        hotbar.OffsetRight = halfWidth;
        hotbar.OffsetTop = -132;
        hotbar.OffsetBottom = -62;
        hotbar.AddThemeConstantOverride("separation", 8);
        root.AddChild(hotbar);
        for (int i = 0; i < EquipmentRules.SlotCount; i++)
        {
            int slot = i;
            var button = new Button
            {
                CustomMinimumSize = new Vector2(115, 68), ToggleMode = true,
                FocusMode = Control.FocusModeEnum.None,
                TooltipText = i < 2 ? $"Left click or J to swing and damage enemies. Hold left mouse to harvest {EquipmentRules.Purpose((EquipmentKind)i).ToLowerInvariant()} within {BlockWorld.MiningReach / BlockWorld.TileSize:0} blocks."
                    : i == 2 ? "Left click or J to swing the equipped sword."
                    : "Left or right click to place the selected material."
            };
            button.AddThemeFontSizeOverride("font_size", 15);
            button.AddThemeStyleboxOverride("normal", Box("263e43", "46615e", 1));
            button.AddThemeStyleboxOverride("hover", Box("344e50", "91b5a4", 1));
            button.AddThemeStyleboxOverride("pressed", Box("405b50", "d6d6a0", 3));
            button.AddThemeStyleboxOverride("hover_pressed", Box("496454", "e6e0af", 3));
            button.Pressed += () => SlotSelected?.Invoke(slot);
            _slots[i] = button;
            hotbar.AddChild(button);
        }
        var controls = Text("A/D Move   SPACE Jump   SHIFT Dodge   1–6 / Wheel Select   LMB Use   J Swing   RMB Build",
            Vector2.Zero, 15, "dfddc8");
        controls.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        controls.OffsetLeft = 28; controls.OffsetTop = -52; controls.OffsetRight = -20; controls.OffsetBottom = -28;
        var warning = Text("R Reset world  /  Prototype progress is not saved  /  F2 Combat test room",
            Vector2.Zero, 12, "92ada7");
        warning.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        warning.OffsetLeft = 28; warning.OffsetTop = -27; warning.OffsetRight = -20; warning.OffsetBottom = -6;
    }

    private static StyleBoxFlat Box(string fill, string border, int width) => new()
    {
        BgColor = new Color(fill), BorderColor = new Color(border),
        BorderWidthLeft = width, BorderWidthRight = width, BorderWidthTop = width, BorderWidthBottom = width,
        CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5, CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5
    };

    public void Refresh(Player player, Enemy enemy, SandboxInventory inventory, int selected, string target)
    {
        _status.Text = $"HEALTH  {player.Health}/100\n" + (player.DodgeReadyIn <= 0 ? "DODGE  Ready" : $"DODGE  {player.DodgeReadyIn:0.0}s");
        _target.Text = player.IsDead ? "You fell. Press R to reset this unsaved world." : target;
        _enemyStatus.Text = $"CREATURE {enemy.Health}/100 · {enemy.StateText}";
        for (int i = 0; i < EquipmentRules.SlotCount; i++)
        {
            EquipmentKind item = (EquipmentKind)i;
            string detail = EquipmentRules.TryGetMaterial(item, out BlockKind material)
                ? inventory.Count(material).ToString() : EquipmentRules.Purpose(item);
            _slots[i].Text = $"{i + 1}  {item.ToString().ToUpperInvariant()}\n{detail}";
            _slots[i].SetPressedNoSignal(i == selected);
        }
    }

    public void Notify(string message) { _notice.Text = message; _noticeLeft = 2.5f; }
    public override void _Process(double delta)
    {
        _noticeLeft -= (float)delta;
        if (_noticeLeft <= 0) _notice.Text = "";
    }
}
