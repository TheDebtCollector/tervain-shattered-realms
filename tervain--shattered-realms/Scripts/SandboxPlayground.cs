using Godot;
using System;
using System.Threading.Tasks;

namespace Tervain;

public partial class SandboxPlayground : Node2D
{
    private Player _player = null!;
    private BlockWorld _world = null!;
    private SandboxHud _hud = null!;
    private readonly SandboxInventory _inventory = new();
    private int _selected;
    private Vector2I? _miningCell;
    private float _mineElapsed, _placeCooldown;
    private Vector2? _testTarget;
    private static bool _testingRestart;
    private bool _combatSwitchPending;

    public override void _Ready()
    {
        Bind("move_left", Key.A, Key.Left);
        Bind("move_right", Key.D, Key.Right);
        Bind("jump", Key.Space, Key.W, Key.Up);
        Bind("dodge", Key.Shift);
        Bind("attack", Key.J);
        Bind("restart", Key.R);
        Bind("combat_playground", Key.F2);
        // Left click is a tool action in the sandbox. The original combat room keeps its own binding.
        var leftClick = new InputEventMouseButton { ButtonIndex = MouseButton.Left };
        if (InputMap.ActionHasEvent("attack", leftClick)) InputMap.ActionEraseEvent("attack", leftClick);
        BindMouse("use_primary", MouseButton.Left);
        BindMouse("use_secondary", MouseButton.Right);
        _player = GetNode<Player>("Player");
        _world = GetNode<BlockWorld>("World");
        _hud = new SandboxHud { SlotSelected = SelectSlot };
        AddChild(_hud);
        RenderingServer.SetDefaultClearColor(new Color("182f37"));
        if (HasArgument("--sandbox-test"))
        {
            if (_testingRestart)
            {
                Check(_inventory.Count(BlockKind.Dirt) == 0 && _world.HasBlock(new Vector2I(11, 14)), "Reset restores terrain and clears inventory.");
                GetTree().ChangeSceneToFile("res://Scenes/CombatPlayground.tscn");
            }
            else CallDeferred(MethodName.RunSandboxTests);
        }
        if (HasArgument("--sandbox-preview")) CallDeferred(MethodName.SavePreview);
    }

    private static bool HasArgument(string arg) => Array.Exists(OS.GetCmdlineUserArgs(), value => value == arg);
    private static void Bind(string action, params Key[] keys)
    {
        if (InputMap.HasAction(action)) return;
        InputMap.AddAction(action);
        foreach (Key key in keys) InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }
    private static void BindMouse(string action, MouseButton button)
    {
        if (InputMap.HasAction(action)) return;
        InputMap.AddAction(action);
        InputMap.ActionAddEvent(action, new InputEventMouseButton { ButtonIndex = button });
    }

    private void SelectSlot(int slot)
    {
        _selected = Mathf.PosMod(slot, 4);
        _miningCell = null;
        _mineElapsed = 0;
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (input is InputEventKey { Pressed: true, Echo: false } key)
        {
            Key code = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;
            if (code >= Key.Key1 && code <= Key.Key4)
            { SelectSlot((int)(code - Key.Key1)); GetViewport().SetInputAsHandled(); }
        }
        if (input is InputEventMouseButton { Pressed: true } mouse)
        {
            if (mouse.ButtonIndex == MouseButton.WheelUp) SelectSlot(_selected - 1);
            if (mouse.ButtonIndex == MouseButton.WheelDown) SelectSlot(_selected + 1);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_combatSwitchPending) return;
        float dt = (float)delta;
        _placeCooldown = Mathf.Max(0, _placeCooldown - dt);
        Vector2 point = _testTarget ?? GetGlobalMousePosition();
        Vector2I cell = BlockWorld.CellAt(point);
        bool overUi = _testTarget == null && (GetViewport().GuiGetHoveredControl() != null ||
            GetViewport().GetMousePosition().Y < 114 || GetViewport().GetMousePosition().Y > GetViewportRect().Size.Y - 142);
        bool busy = _player.IsDead || _player.IsDodging || _player.IsAttacking;
        bool primary = Input.IsActionPressed("use_primary");
        bool secondary = Input.IsActionPressed("use_secondary");
        bool mine = _selected == 0;
        bool valid = !overUi && !busy && (mine ? _world.CanMine(cell, _player) : _world.CanPlace(cell, _player) && _inventory.Count((BlockKind)(_selected - 1)) > 0);
        _world.HighlightCell = overUi || !BlockWorld.InBounds(cell) ? null : cell;
        _world.HighlightValid = valid;
        string target = mine ? "PICKAXE  /  Hold left mouse on a nearby block to mine." : $"{((BlockKind)(_selected - 1)).ToString().ToUpperInvariant()}  /  Hold left or right mouse to build.";
        if (_world.TryGetBlock(cell, out BlockKind material) && !overUi)
            target = material == BlockKind.Bedrock ? "BEDROCK  /  This foundation cannot be mined."
                : $"{material.ToString().ToUpperInvariant()}  /  " + (mine ? valid ? "Hold left mouse to mine." : "Move closer or clear a path to this block." : "This space is occupied.");
        if (mine && primary && valid)
        {
            if (_miningCell != cell) { _miningCell = cell; _mineElapsed = 0; }
            _mineElapsed += dt;
            if (_mineElapsed >= BlockWorld.MineDuration(material))
            {
                if (_world.TryMine(cell, _player, out BlockKind mined))
                    AddChild(new MaterialDrop { Kind = mined, Position = BlockWorld.Center(cell), Player = _player, Inventory = _inventory });
                _mineElapsed = 0;
                _miningCell = null;
            }
        }
        else { _mineElapsed = 0; _miningCell = null; }

        if (!mine && (primary || secondary) && !overUi && !busy && _placeCooldown <= 0)
        {
            if (_world.TryPlace(cell, (BlockKind)(_selected - 1), _player, _inventory)) _placeCooldown = 0.16f;
            else if (Input.IsActionJustPressed("use_primary") || Input.IsActionJustPressed("use_secondary"))
                _hud.Notify(_inventory.Count((BlockKind)(_selected - 1)) == 0 ? "Mine and collect this material first."
                    : "Place within reach, against an existing block, with a clear path and room for your character.");
        }
        _world.MiningProgress = _miningCell != null ? Mathf.Clamp(_mineElapsed / BlockWorld.MineDuration(material), 0, 1) : 0;
        _world.QueueRedraw();
        _hud.Refresh(_player, _inventory, _selected, target);
        if (Input.IsActionJustPressed("restart")) GetTree().ReloadCurrentScene();
        if (Input.IsActionJustPressed("combat_playground"))
        {
            _combatSwitchPending = true;
            GetTree().ChangeSceneToFile("res://Scenes/CombatPlayground.tscn");
        }
    }

    private async Task Frames(int count)
    { for (int i = 0; i < count; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("SANDBOX TEST FAILED: " + message);
        GD.Print("PASS: " + message);
    }
    private async void SavePreview()
    {
        await Frames(30);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Error result = GetViewport().GetTexture().GetImage().SavePng("res://.godot/sandbox-preview.png");
        GD.Print("Sandbox preview: " + result);
        GetTree().Quit(result == Error.Ok ? 0 : 1);
    }

    private async void RunSandboxTests()
    {
        try
        {
            await Frames(30);
            Check(_player.IsOnFloor(), "Player stands on editable terrain.");
            Check(_player.MoveSpeed == 280 && _player.JumpSpeed == 565 && _player.DodgeDuration == 0.18f, "Accepted movement settings are preserved.");
            Vector2I minedCell = new(11, 14);
            _testTarget = BlockWorld.Center(minedCell);
            Input.ActionPress("use_primary");
            await Frames(24);
            Input.ActionRelease("use_primary");
            Check(!_world.HasBlock(minedCell), "Held mining input removes the targeted dirt block.");
            await Frames(35);
            Check(_inventory.Count(BlockKind.Dirt) == 1, "Dropped dirt is collected exactly once.");

            SelectSlot(1);
            Vector2I placedCell = new(12, 13);
            _testTarget = BlockWorld.Center(placedCell);
            Input.ActionPress("use_primary");
            await Frames(3);
            Input.ActionRelease("use_primary");
            Check(_world.HasBlock(placedCell) && _inventory.Count(BlockKind.Dirt) == 0, "Building consumes exactly one collected block.");
            Check(!_world.TryPlace(new Vector2I(13, 13), BlockKind.Dirt, _player, _inventory), "Empty inventory cannot place free blocks.");
            _inventory.Add(BlockKind.Wood, 10);
            Check(!_world.TryPlace(placedCell, BlockKind.Wood, _player, _inventory), "Occupied cells reject placement.");
            Check(!_world.TryPlace(new Vector2I(13, 13), BlockKind.Wood, _player, _inventory), "Placement cannot pass through an intervening wall.");
            Check(!_world.TryPlace(new Vector2I(10, 13), BlockKind.Wood, _player, _inventory), "Placement cannot overlap the player.");
            Check(!_world.TryPlace(new Vector2I(10, 9), BlockKind.Wood, _player, _inventory), "Unsupported floating blocks are rejected.");
            Check(!_world.TryPlace(new Vector2I(60, 13), BlockKind.Wood, _player, _inventory), "Distant placement is rejected.");
            Check(!_world.TryMine(new Vector2I(60, 14), _player, out _), "Distant mining is rejected.");
            Check(!_world.TryMine(new Vector2I(10, 16), _player, out _), "Mining cannot pass through intervening terrain.");
            Check(_inventory.Count(BlockKind.Wood) == 10, "Rejected placements preserve inventory.");
            Check(!_world.TryMine(new Vector2I(10, 23), _player, out _), "Bedrock cannot be mined.");

            _player.Position = new Vector2(400, 330);
            _player.Velocity = Vector2.Zero;
            await Frames(40);
            Check(_player.IsOnFloor() && Mathf.Abs(_player.Position.Y - 416) < 1, "Placed blocks physically support the player.");
            SelectSlot(0);
            _testTarget = BlockWorld.Center(placedCell);
            Input.ActionPress("use_primary");
            await Frames(24);
            Input.ActionRelease("use_primary");
            await Frames(35);
            Check(_player.IsOnFloor() && Mathf.Abs(_player.Position.Y - 448) < 1, "Mining a supporting block removes its collision.");
            Check(_inventory.Count(BlockKind.Dirt) == 1, "Placed material can be mined and recovered.");

            _player.Position = new Vector2(29 * 32 - 30, 448);
            _player.Velocity = Vector2.Zero;
            await Frames(3);
            _testTarget = BlockWorld.Center(new Vector2I(29, 13));
            SelectSlot(0);
            Input.ActionPress("use_primary");
            await Frames(20);
            Check(_world.HasBlock(new Vector2I(29, 13)), "Stone requires longer mining than dirt.");
            await Frames(24);
            Input.ActionRelease("use_primary");
            await Frames(35);
            Check(_inventory.Count(BlockKind.Stone) == 1, "Stone can be mined and collected.");

            _player.Position = new Vector2(19 * 32 + 16, 448);
            _player.Velocity = Vector2.Zero;
            await Frames(3);
            _testTarget = BlockWorld.Center(new Vector2I(20, 13));
            Input.ActionPress("use_primary");
            await Frames(30);
            Input.ActionRelease("use_primary");
            await Frames(35);
            Check(_inventory.Count(BlockKind.Wood) == 11, "Tree trunks yield collectible wood.");
            SelectSlot(3);
            _testTarget = BlockWorld.Center(new Vector2I(18, 13));
            Input.ActionPress("use_secondary");
            await Frames(3);
            Input.ActionRelease("use_secondary");
            Check(_world.TryGetBlock(new Vector2I(18, 13), out BlockKind built) && built == BlockKind.Wood, "Secondary input places the selected wood material.");

            _testTarget = BlockWorld.Center(new Vector2I(19, 14));
            SelectSlot(0);
            Input.ActionPress("use_primary");
            await Frames(4);
            SelectSlot(2);
            await Frames(25);
            Input.ActionRelease("use_primary");
            Check(_world.HasBlock(new Vector2I(19, 14)), "Switching tools cancels an unfinished mining action.");
            _player.Position = new Vector2(35 * 32 + 16, 21 * 32);
            _player.Velocity = Vector2.Zero;
            await Frames(60);
            Vector2 screenPosition = _player.GetGlobalTransformWithCanvas().Origin;
            Check(screenPosition.Y > 170 && screenPosition.Y < GetViewportRect().Size.Y - 150,
                "Camera keeps underground exploration clear of the HUD.");
            _testingRestart = true;
            Check(GetTree().ReloadCurrentScene() == Error.Ok, "Sandbox can restart.");
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
}
