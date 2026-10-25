using Godot;
using System;

namespace Tervain;

public partial class SandboxHud : CanvasLayer
{
    public Action<int>? SlotSelected { get; set; }
    private readonly Button[] _slots = new Button[4];
    private Label _status = null!, _target = null!, _notice = null!;
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
        Text("TERVAIN / SANDBOX FOUNDATION", new Vector2(28, 14), 13, "94bcb0");
        Text("A place to call home", new Vector2(26, 33), 28, "f1e5cb");
        Text("Gather dirt, stone and wood. Use your materials to build a shelter.", new Vector2(28, 77), 16, "b5c7bd");
        _status = Text("", new Vector2(825, 27), 16, "b3e9d8");
        _target = Text("", new Vector2(28, 124), 17, "e4e3c8");
        _notice = Text("", new Vector2(28, 151), 15, "ffcb8a");

        var footer = new ColorRect { Color = new Color(0.05f, 0.11f, 0.14f, 0.94f), MouseFilter = Control.MouseFilterEnum.Ignore };
        footer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        footer.OffsetTop = -142;
        root.AddChild(footer);
        var hotbar = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        hotbar.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterBottom);
        hotbar.OffsetLeft = -242;
        hotbar.OffsetRight = 242;
        hotbar.OffsetTop = -132;
        hotbar.OffsetBottom = -62;
        hotbar.AddThemeConstantOverride("separation", 8);
        root.AddChild(hotbar);
        for (int i = 0; i < 4; i++)
        {
            int slot = i;
            var button = new Button
            {
                CustomMinimumSize = new Vector2(115, 68), ToggleMode = true,
                FocusMode = Control.FocusModeEnum.None,
                TooltipText = i == 0 ? "Hold left mouse to mine a reachable block." : "Left or right click to place the selected material."
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
        var controls = Text("A/D Move   SPACE Jump   SHIFT Dodge   J Attack   1–4 / Wheel Select   LMB Use   RMB Build",
            Vector2.Zero, 15, "dfddc8");
        controls.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        controls.OffsetLeft = 28; controls.OffsetTop = -52; controls.OffsetRight = -20; controls.OffsetBottom = -28;
        var warning = Text("F2 Combat playground  /  R Reset world  /  Prototype progress is not saved",
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

    public void Refresh(Player player, SandboxInventory inventory, int selected, string target)
    {
        _status.Text = $"HEALTH  {player.Health}/100\n" + (player.DodgeReadyIn <= 0 ? "DODGE  Ready" : $"DODGE  {player.DodgeReadyIn:0.0}s");
        _target.Text = player.IsDead ? "You fell. Press R to reset this unsaved world." : target;
        _slots[0].Text = "1  PICKAXE\nMine blocks";
        for (int i = 1; i <= 3; i++)
        {
            var kind = (BlockKind)(i - 1);
            _slots[i].Text = $"{i + 1}  {kind.ToString().ToUpperInvariant()}\n{inventory.Count(kind)}";
        }
        for (int i = 0; i < 4; i++) _slots[i].SetPressedNoSignal(i == selected);
    }

    public void Notify(string message) { _notice.Text = message; _noticeLeft = 2.5f; }
    public override void _Process(double delta)
    {
        _noticeLeft -= (float)delta;
        if (_noticeLeft <= 0) _notice.Text = "";
    }
}
