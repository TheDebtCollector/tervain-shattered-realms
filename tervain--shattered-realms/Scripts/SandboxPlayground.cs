using Godot;
using System;
using System.Threading.Tasks;

namespace Tervain;

public partial class SandboxPlayground : Node2D
{
    private const float GroundY = BlockWorld.SurfaceRow * BlockWorld.TileSize;
    private Player _player = null!;
    private BlockWorld _world = null!;
    private Enemy _enemy = null!;
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
        _enemy = GetNode<Enemy>("TrainingCreature");
        SelectSlot(0);
        _hud = new SandboxHud { SlotSelected = SelectSlot };
        AddChild(_hud);
        RenderingServer.SetDefaultClearColor(new Color("182f37"));
        if (HasArgument("--sandbox-test"))
        {
            if (_testingRestart)
            {
                Check(_inventory.Count(BlockKind.Dirt) == 0 && _world.HasBlock(new Vector2I(15, BlockWorld.SurfaceRow))
                    && _player.Health == 100 && _enemy.Health == 100 && _player.EquippedItem == EquipmentKind.Pickaxe,
                    "Reset restores terrain, inventory, both combatants, and the starter pickaxe.");
                GetTree().ChangeSceneToFile("res://Scenes/CombatPlayground.tscn");
            }
            else CallDeferred(MethodName.RunSandboxTests);
        }
        if (HasArgument("--sandbox-preview") || HasArgument("--wall-preview") || HasArgument("--equipment-preview") || HasArgument("--ore-preview"))
            CallDeferred(MethodName.SavePreview);
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
        _selected = Mathf.PosMod(slot, EquipmentRules.SlotCount);
        _miningCell = null;
        _mineElapsed = 0;
        _player.Equip((EquipmentKind)_selected);
    }

    public override void _UnhandledInput(InputEvent input)
    {
        if (input is InputEventKey { Pressed: true, Echo: false } key)
        {
            Key code = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;
            if (code >= Key.Key1 && code <= Key.Key7)
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
        Vector2I cell = BlockWorld.CellAt(_testTarget ?? GetGlobalMousePosition());
        bool overUi = _testTarget == null && (GetViewport().GuiGetHoveredControl() != null ||
            GetViewport().GetMousePosition().Y < 114 || GetViewport().GetMousePosition().Y > GetViewportRect().Size.Y - 166);
        EquipmentKind item = _player.EquippedItem;
        bool miningTool = EquipmentRules.IsMiningTool(item);
        bool building = EquipmentRules.TryGetMaterial(item, out BlockKind buildingMaterial);
        bool sword = item == EquipmentKind.Sword;
        bool canSwing = EquipmentRules.CanSwing(item);
        bool primary = Input.IsActionPressed("use_primary");
        bool secondary = Input.IsActionPressed("use_secondary");
        if (canSwing && !overUi && (miningTool ? primary : Input.IsActionJustPressed("use_primary")))
            _player.TryStartAttack();
        if (!canSwing && Input.IsActionJustPressed("attack")) _hud.Notify("Select a tool or sword (1–3) to swing.");
        // Tool swings and harvesting run together. Sword attacks still take priority over building.
        bool busy = _player.IsDead || _player.IsDodging || Input.IsActionPressed("dodge") ||
            (!miningTool && (_player.IsAttacking || Input.IsActionPressed("attack")));
        bool valid = !overUi && !busy && (miningTool ? _world.CanMine(cell, _player)
            : building && _world.CanPlace(cell, _player) && _inventory.Count(buildingMaterial) > 0);
        _world.HighlightCell = overUi || (!miningTool && !building) || !BlockWorld.InBounds(cell) ? null : cell;
        _world.HighlightValid = valid;
        bool hasBlock = _world.TryGetBlock(cell, out BlockKind block);
        string target = sword ? "SWORD  /  Left click or J to attack. Shift to dodge."
            : building ? $"{(item == EquipmentKind.IronOre ? "IRON ORE" : item.ToString().ToUpperInvariant())}  /  Hold left or right mouse to build."
            : $"{item.ToString().ToUpperInvariant()}  /  Left click or J to swing; hold left mouse to harvest {EquipmentRules.Purpose(item).ToLowerInvariant()}.";
        if (!overUi && hasBlock && (miningTool || building))
        {
            string instruction = building ? "This space is occupied."
                : block == BlockKind.Bedrock ? "This foundation cannot be mined."
                : !EquipmentRules.CanMine(item, block) ? block == BlockKind.Wood ? "Requires axe (2)." : "Requires pickaxe (1)."
                : valid ? "Hold left mouse to harvest."
                : $"Move closer; tool reach is {BlockWorld.MiningReach / BlockWorld.TileSize:0} blocks through terrain.";
            target = $"{(block == BlockKind.IronOre ? "IRON ORE" : block.ToString().ToUpperInvariant())}  /  {instruction}";
        }
        else if (miningTool && !overUi && _world.HasWall(cell))
            target = "BACKGROUND WALL  /  Select a building material to attach a block here.";
        if (miningTool && primary && valid)
        {
            if (_miningCell != cell) { _miningCell = cell; _mineElapsed = 0; }
            _mineElapsed += dt;
            if (_mineElapsed >= BlockWorld.MineDuration(block))
            {
                if (_world.TryMine(cell, _player, out BlockKind mined))
                    AddChild(new MaterialDrop { Kind = mined, Position = BlockWorld.Center(cell), Player = _player, Inventory = _inventory });
                _mineElapsed = 0;
                _miningCell = null;
            }
        }
        else { _mineElapsed = 0; _miningCell = null; }
        if (building && (primary || secondary) && !overUi && !busy && _placeCooldown <= 0)
        {
            if (_world.TryPlace(cell, buildingMaterial, _player, _inventory)) _placeCooldown = 0.16f;
            else if (Input.IsActionJustPressed("use_primary") || Input.IsActionJustPressed("use_secondary"))
                _hud.Notify(_inventory.Count(buildingMaterial) == 0 ? "Gather and collect this material first."
                    : "Build within reach, on a block or background wall, with a clear path and room for characters.");
        }
        _world.MiningProgress = _miningCell != null ? Mathf.Clamp(_mineElapsed / BlockWorld.MineDuration(block), 0, 1) : 0;
        _world.QueueRedraw();
        _hud.Refresh(_player, _enemy, _inventory, _selected, target);
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
        if (HasArgument("--equipment-preview"))
        {
            for (int slot = 0; slot < 3; slot++)
            {
                SelectSlot(slot);
                _testTarget = BlockWorld.Center(new Vector2I(14, BlockWorld.SurfaceRow - 5));
                Input.ActionPress("use_primary");
                await Frames(10);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                Error capture = GetViewport().GetTexture().GetImage().SavePng($"res://.godot/equipment-{_player.EquippedItem}.png");
                if (capture != Error.Ok) { GetTree().Quit(1); return; }
                Input.ActionRelease("use_primary");
                await Frames(30);
            }
            GD.Print("Equipment previews: Ok");
            GetTree().Quit();
            return;
        }
        bool wallPreview = HasArgument("--wall-preview");
        if (HasArgument("--ore-preview"))
        {
            _enemy.ProcessMode = ProcessModeEnum.Disabled;
            _player.Position = new Vector2(52.5f * BlockWorld.TileSize, BlockWorld.CaveBottomRow * BlockWorld.TileSize);
            _player.Velocity = Vector2.Zero;
            _inventory.Add(BlockKind.IronOre);
            SelectSlot(6);
        }
        if (wallPreview)
        {
            _enemy.ProcessMode = ProcessModeEnum.Disabled;
            _player.Position = new Vector2(348, GroundY);
            _player.Velocity = Vector2.Zero;
            for (int x = 12; x <= 18; x++)
                for (int y = BlockWorld.SurfaceRow; y <= BlockWorld.SurfaceRow + 5; y++)
                    _world.TryMine(new Vector2I(x, y), _player, out _);
            await Frames(2);
            _player.Position = new Vector2(348, (BlockWorld.SurfaceRow + 6) * BlockWorld.TileSize);
            _player.Velocity = Vector2.Zero;
            _inventory.Add(BlockKind.Wood);
            _world.TryPlace(new Vector2I(17, BlockWorld.SurfaceRow + 2), BlockKind.Wood, _player, _inventory);
        }
        await Frames(30);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Error result = GetViewport().GetTexture().GetImage().SavePng(HasArgument("--ore-preview") ? "res://.godot/ore-preview.png" : wallPreview ? "res://.godot/wall-preview.png" : "res://.godot/sandbox-preview.png");
        GD.Print("Sandbox preview: " + result);
        GetTree().Quit(result == Error.Ok ? 0 : 1);
    }

    private async Task<float> MeasureJump(float speed)
    {
        _player.Position = new Vector2(336, GroundY);
        _player.Velocity = Vector2.Zero;
        _player.JumpSpeed = speed;
        await Frames(5);
        float ground = _player.Position.Y;
        float highest = ground;
        Input.ActionPress("jump");
        for (int i = 0; i < 50; i++)
        {
            await Frames(1);
            highest = Mathf.Min(highest, _player.Position.Y);
        }
        Input.ActionRelease("jump");
        await Frames(20);
        return ground - highest;
    }

    private async Task UseTool(Vector2I cell, int frames, bool secondary = false)
    {
        _testTarget = BlockWorld.Center(cell);
        string action = secondary ? "use_secondary" : "use_primary";
        Input.ActionPress(action);
        await Frames(frames);
        Input.ActionRelease(action);
        await Frames(15);
    }

    private async void RunSandboxTests()
    {
        try
        {
            _enemy.ProcessMode = ProcessModeEnum.Disabled;
            _enemy.Position = new Vector2(2000, GroundY);
            await Frames(30);
            Check(_player.IsOnFloor(), "Player stands on the current terrain grid.");
            Check(_world.HasWall(new Vector2I(15, BlockWorld.SurfaceRow)) && _world.HasWall(new Vector2I(5, 13)),
                "Generated terrain and wood blocks receive background walls.");
            Check(!_world.HasWall(new Vector2I(14, BlockWorld.SurfaceRow - 5)) && !_world.HasWall(new Vector2I(47, 26)),
                "Open sky and generated cave air keep scenery without block walls.");
            Check(BlockWorld.TileSize == 24 && _player.BodyBounds.Size.Y == BlockWorld.TileSize * 3,
                "The player is exactly three 24px blocks tall.");
            Check(BlockWorld.Width * BlockWorld.TileSize == 2304 && BlockWorld.Height * BlockWorld.TileSize == 768,
                "The grid preserves the world's physical dimensions.");
            float newSpeed = _player.JumpSpeed;
            float newHeight = await MeasureJump(newSpeed);
            Check(Mathf.Abs(newHeight - 123.8f) < 1,
                $"Full jump height is preserved at {newHeight:0.0}px.");
            Check(_player.MoveSpeed == 280 && _player.DodgeDuration == 0.18f, "Run and dodge settings are preserved.");

            Vector2I minedCell = new(15, BlockWorld.SurfaceRow);
            Check(_player.EquippedItem == EquipmentKind.Pickaxe, "The sandbox starts with the pickaxe equipped.");
            GetViewport().PushInput(new InputEventKey { PhysicalKeycode = Key.Key7, Pressed = true });
            await Frames(2);
            Check(_player.EquippedItem == EquipmentKind.IronOre, "The seventh hotbar slot can be selected with the keyboard.");
            GetViewport().PushInput(new InputEventKey { PhysicalKeycode = Key.Key7, Pressed = false });
            GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.WheelDown, Pressed = true });
            await Frames(2);
            Check(_player.EquippedItem == EquipmentKind.Pickaxe, "Mouse wheel selection wraps across all seven equipment slots.");
            int blocksBeforeSwinging = _world.BlockCount;
            for (int slot = 0; slot < 2; slot++)
            {
                SelectSlot(slot);
                _testTarget = BlockWorld.Center(new Vector2I(14, BlockWorld.SurfaceRow - 5));
                Input.ActionPress("use_primary");
                await Frames(3);
                Check(_player.IsAttacking, $"{_player.EquippedItem} swings in empty air without a mining target.");
                await Frames(30);
                Check(_player.IsAttacking, $"Holding left click repeats {_player.EquippedItem} swings in empty air.");
                Input.ActionRelease("use_primary");
                await Frames(30);
                Check(!_player.IsAttacking, "Releasing left click lets the tool swing finish.");
            }
            Check(_world.BlockCount == blocksBeforeSwinging && _inventory.Count(BlockKind.Dirt) == 0
                && _inventory.Count(BlockKind.Wood) == 0 && _inventory.Count(BlockKind.Stone) == 0,
                "Empty-air swings do not change terrain or generate materials.");
            SelectSlot(2);
            await UseTool(minedCell, 28);
            Check(_world.HasBlock(minedCell) && _inventory.Count(BlockKind.Dirt) == 0,
                "The sword swings without mining dirt or generating material drops.");
            SelectSlot(0);
            await UseTool(minedCell, 24);
            await Frames(20);
            Check(!_world.HasBlock(minedCell) && _inventory.Count(BlockKind.Dirt) == 1,
                "Mining removes a 24px block and collects one material.");
            SelectSlot(3);
            Vector2I placedCell = new(16, BlockWorld.SurfaceRow - 1);
            await UseTool(placedCell, 3);
            Check(_world.HasBlock(placedCell) && _inventory.Count(BlockKind.Dirt) == 0, "Placement consumes one material.");
            Check(!_world.HasWall(placedCell), "Player-built blocks in original air do not generate background walls.");
            Check(!_world.TryPlace(new Vector2I(17, BlockWorld.SurfaceRow - 1), BlockKind.Dirt, _player, _inventory), "Empty inventory cannot create free blocks.");
            _inventory.Add(BlockKind.Wood, 10);
            Check(!_world.TryPlace(placedCell, BlockKind.Wood, _player, _inventory), "Occupied cells reject placement.");
            Check(!_world.TryPlace(new Vector2I(13, BlockWorld.SurfaceRow - 2), BlockKind.Wood, _player, _inventory), "Placement respects the full 72px player body.");
            Check(!_world.TryPlace(new Vector2I(14, BlockWorld.SurfaceRow - 5), BlockKind.Wood, _player, _inventory), "Floating unsupported blocks are rejected.");
            Check(!_world.TryPlace(new Vector2I(27, BlockWorld.SurfaceRow - 1), BlockKind.Wood, _player, _inventory), "Building still uses its shorter reach.");
            SelectSlot(0);
            Check(!_world.CanMine(new Vector2I(80, BlockWorld.SurfaceRow), _player), "Mining beyond the extended range is rejected.");
            Check(_world.CanMine(new Vector2I(21, BlockWorld.SurfaceRow), _player) && !_world.CanMine(new Vector2I(22, BlockWorld.SurfaceRow), _player),
                "The halved 192px mining distance is enforced at its boundary.");
            Check(!_world.CanMine(new Vector2I(14, BlockWorld.Height - 1), _player), "Bedrock remains unmineable.");
            SelectSlot(0);
            await UseTool(new Vector2I(20, BlockWorld.SurfaceRow + 2), 24);
            Check(!_world.HasBlock(new Vector2I(20, BlockWorld.SurfaceRow + 2)) && _world.HasBlock(new Vector2I(20, BlockWorld.SurfaceRow)),
                "Long-range mining removes a buried block without clearing intervening terrain.");
            Check(_inventory.Count(BlockKind.Wood) == 10, "Rejected placements preserve material counts.");

            _player.Position = new Vector2(396, GroundY - 120);
            _player.Velocity = Vector2.Zero;
            await Frames(40);
            Check(_player.IsOnFloor() && Mathf.Abs(_player.Position.Y - (GroundY - BlockWorld.TileSize)) < 1, "A single 24px block supports the taller player.");
            await UseTool(placedCell, 24);
            await Frames(20);
            Check(_player.IsOnFloor() && Mathf.Abs(_player.Position.Y - GroundY) < 1, "Mining removes the support's collision.");
            Check(_inventory.Count(BlockKind.Dirt) == 1, "Placed material can be recovered.");

            _player.Position = new Vector2(906, GroundY);
            _player.Velocity = Vector2.Zero;
            await Frames(3);
            SelectSlot(1);
            await UseTool(new Vector2I(39, BlockWorld.SurfaceRow - 1), 45);
            Check(_world.HasBlock(new Vector2I(39, BlockWorld.SurfaceRow - 1)) && _inventory.Count(BlockKind.Stone) == 0,
                "The axe cannot mine stone, even when held longer than its mining time.");
            SelectSlot(0);
            _testTarget = BlockWorld.Center(new Vector2I(39, BlockWorld.SurfaceRow - 1));
            Input.ActionPress("use_primary");
            await Frames(20);
            Check(_world.HasBlock(new Vector2I(39, BlockWorld.SurfaceRow - 1)), "Stone retains its longer mining time.");
            await Frames(24);
            Input.ActionRelease("use_primary");
            await Frames(35);
            Check(_inventory.Count(BlockKind.Stone) == 1, "Stone can be mined and collected on the current grid.");
            _player.Position = new Vector2(618, GroundY);
            _player.Velocity = Vector2.Zero;
            await Frames(3);
            await UseTool(new Vector2I(27, BlockWorld.SurfaceRow - 1), 40);
            Check(_world.HasBlock(new Vector2I(27, BlockWorld.SurfaceRow - 1)) && _inventory.Count(BlockKind.Wood) == 10,
                "The pickaxe cannot harvest wood or create wood drops.");
            SelectSlot(1);
            await UseTool(new Vector2I(27, BlockWorld.SurfaceRow - 1), 30);
            await Frames(20);
            Check(_inventory.Count(BlockKind.Wood) == 11, "Tree trunks yield individual 24px wood blocks.");
            SelectSlot(5);
            await UseTool(new Vector2I(23, BlockWorld.SurfaceRow - 1), 3, true);
            Check(_world.TryGetBlock(new Vector2I(23, BlockWorld.SurfaceRow - 1), out BlockKind built) && built == BlockKind.Wood,
                "Secondary input places the selected wood material.");
            SelectSlot(1);
            await UseTool(new Vector2I(23, BlockWorld.SurfaceRow - 1), 30);
            await Frames(20);
            SelectSlot(0);
            _testTarget = BlockWorld.Center(new Vector2I(25, BlockWorld.SurfaceRow));
            Input.ActionPress("use_primary");
            await Frames(4);
            SelectSlot(4);
            await Frames(25);
            Input.ActionRelease("use_primary");
            Check(_world.HasBlock(new Vector2I(25, BlockWorld.SurfaceRow)), "Tool switching cancels unfinished mining.");
            _player.Position = new Vector2(1140, BlockWorld.CaveBottomRow * BlockWorld.TileSize);
            _player.Velocity = Vector2.Zero;
            await Frames(60);
            Vector2 screenPosition = _player.GetGlobalTransformWithCanvas().Origin;
            Check(screenPosition.Y > 170 && screenPosition.Y < GetViewportRect().Size.Y - 150,
                "Underground exploration stays clear of the HUD.");
            await TestBackgroundWalls();
            await TestCombinedSystems();
            await TestIronOre();
            _testingRestart = true;
            Check(GetTree().ReloadCurrentScene() == Error.Ok, "Sandbox can restart.");
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async Task TestIronOre()
    {
        _player.Position = new Vector2(52.5f * BlockWorld.TileSize, BlockWorld.CaveBottomRow * BlockWorld.TileSize);
        _player.Velocity = Vector2.Zero;
        await Frames(5);
        Vector2I ore = new(55, 26);
        Check(_world.TryGetBlock(ore, out BlockKind kind) && kind == BlockKind.IronOre && _world.HasWall(ore),
            "An iron ore deposit generates beside the cave with a matching wall.");
        SelectSlot(1);
        await UseTool(ore, 55);
        Check(_world.HasBlock(ore) && _inventory.Count(BlockKind.IronOre) == 0, "The axe cannot harvest iron ore.");
        SelectSlot(0);
        await UseTool(ore, 20);
        Check(_world.HasBlock(ore), "Iron ore requires a sustained pickaxe harvest.");
        await UseTool(ore, 52);
        await Frames(20);
        Check(!_world.HasBlock(ore) && _world.HasWall(ore) && _inventory.Count(BlockKind.IronOre) == 1,
            "Mining iron ore leaves its scenery wall and collects one ore block.");
        SelectSlot(6);
        Vector2I placement = new(54, 27);
        await UseTool(placement, 3);
        Check(_world.TryGetBlock(placement, out kind) && kind == BlockKind.IronOre && _inventory.Count(BlockKind.IronOre) == 0,
            "Slot seven places the iron ore block and spends its own stack.");
        SelectSlot(0);
        await UseTool(placement, 52);
        await Frames(20);
        Check(!_world.HasBlock(placement) && _inventory.Count(BlockKind.IronOre) == 1,
            "Placed iron ore can be mined and recovered.");
    }

    private async Task TestBackgroundWalls()
    {
        SelectSlot(0);
        _player.Position = new Vector2(1692, GroundY);
        _player.Velocity = Vector2.Zero;
        await Frames(3);
        int excavated = 0;
        for (int x = 67; x <= 73; x++)
            for (int y = BlockWorld.SurfaceRow; y <= BlockWorld.SurfaceRow + 5; y++)
                if (_world.TryMine(new Vector2I(x, y), _player, out _)) excavated++;
        Check(excavated == 42, "Excavation clears foreground blocks for background-wall checks.");
        await Frames(3);
        Vector2I anchor = new(71, BlockWorld.SurfaceRow + 2);
        float floor = (BlockWorld.SurfaceRow + 6) * BlockWorld.TileSize;
        Check(_world.HasWall(anchor) && !_world.HasBlock(anchor), "Mining reveals and preserves the generated background wall.");
        Check(!_world.CanMine(anchor, _player) && !_world.TryMine(anchor, _player, out _) && _world.HasWall(anchor),
            "The exposed background wall cannot be mined.");
        var point = new PhysicsPointQueryParameters2D { Position = BlockWorld.Center(anchor), CollisionMask = 1 };
        Check(_world.GetWorld2D().DirectSpaceState.IntersectPoint(point).Count == 0, "Background walls have no solid collision.");
        _player.Position = new Vector2(1644, floor);
        _player.Velocity = Vector2.Zero;
        await Frames(3);
        Input.ActionPress("move_right");
        await Frames(20);
        Input.ActionRelease("move_right");
        Check(_player.Position.X > BlockWorld.Center(anchor).X && _player.IsOnFloor(), "The player can walk through exposed background walls.");
        _player.Position = new Vector2(1644, floor);
        _player.Velocity = Vector2.Zero;
        await Frames(3);
        Check(!_world.HasBlock(anchor + Vector2I.Up) && !_world.HasBlock(anchor + Vector2I.Down)
            && !_world.HasBlock(anchor + Vector2I.Left) && !_world.HasBlock(anchor + Vector2I.Right),
            "The wall-supported target has no adjacent foreground blocks.");
        _inventory.Add(BlockKind.Wood);
        int before = _inventory.Count(BlockKind.Wood);
        SelectSlot(5);
        await UseTool(anchor, 3);
        Check(_world.HasBlock(anchor) && _inventory.Count(BlockKind.Wood) == before - 1,
            "Tool input attaches a floating foreground block to its background wall.");
        Check(_world.GetWorld2D().DirectSpaceState.IntersectPoint(point).Count > 0, "The new foreground block provides collision.");
        SelectSlot(1);
        await UseTool(anchor, 30);
        await Frames(20);
        Check(!_world.HasBlock(anchor) && _world.HasWall(anchor) && _inventory.Count(BlockKind.Wood) == before,
            "Mining recovers the foreground block while preserving the background wall.");
        Check(_world.GetWorld2D().DirectSpaceState.IntersectPoint(point).Count == 0, "Only the non-solid background remains after mining.");
    }
    private async Task TestCombinedSystems()
    {
        _player.Position = new Vector2(336, GroundY);
        _player.Velocity = Vector2.Zero;
        _enemy.Position = new Vector2(592, GroundY);
        _enemy.Velocity = Vector2.Zero;
        _enemy.ProcessMode = ProcessModeEnum.Inherit;
        await Frames(20);
        Check(Mathf.Abs(_enemy.Position.X - 592) < 1 && _player.Health == 100, "The creature waits outside notice range.");
        _enemy.ProcessMode = ProcessModeEnum.Disabled;
        _player.Position = new Vector2(448, GroundY);
        _player.Velocity = Vector2.Zero;
        _enemy.Position = new Vector2(544, GroundY);
        _enemy.Velocity = Vector2.Zero;
        _inventory.Add(BlockKind.Wood, 10);
        await Frames(3);
        int woodBefore = _inventory.Count(BlockKind.Wood);
        Check(!_world.TryPlace(new Vector2I(22, BlockWorld.SurfaceRow - 1), BlockKind.Wood, _player, _inventory), "Building cannot overlap a live enemy.");
        Vector2I lower = new(20, BlockWorld.SurfaceRow - 1), upper = new(20, BlockWorld.SurfaceRow - 2);
        SelectSlot(5);
        await UseTool(lower, 3);
        await UseTool(upper, 3);
        Check(_world.HasBlock(lower) && _world.HasBlock(upper), "Two 24px blocks form a defensive barrier.");
        Check(!_world.TryPlace(new Vector2I(24, BlockWorld.SurfaceRow - 1), BlockKind.Wood, _player, _inventory), "Building still requires a clear path.");
        SelectSlot(2);
        Input.ActionPress("attack");
        await Frames(28);
        Input.ActionRelease("attack");
        Check(_enemy.Health == 100, "Player melee cannot pass through solid terrain.");
        _enemy.ProcessMode = ProcessModeEnum.Inherit;
        await Frames(59);
        Check(_player.Health == 100, "Built terrain still shields the taller player from enemy attacks.");
        _enemy.ProcessMode = ProcessModeEnum.Disabled;
        _enemy.Position = new Vector2(2000, GroundY); // Isolate harvesting from intentional tool melee damage.
        SelectSlot(1);
        _testTarget = BlockWorld.Center(upper);
        Input.ActionPress("use_primary");
        await Frames(4);
        SelectSlot(2);
        Input.ActionPress("attack");
        await Frames(28);
        Input.ActionRelease("attack");
        Check(_world.HasBlock(upper), "Switching from axe to sword cancels an unfinished harvest.");
        SelectSlot(1);
        await Frames(35);
        Input.ActionRelease("use_primary");
        await Frames(25);
        await UseTool(lower, 30);
        await Frames(20);
        Check(!_world.HasBlock(lower) && !_world.HasBlock(upper) && _inventory.Count(BlockKind.Wood) == woodBefore,
            "Mining recovers both defensive blocks after combat.");
        _enemy.Position = new Vector2(544, GroundY);
        _enemy.Velocity = Vector2.Zero;
        SelectSlot(0);
        Input.ActionPress("attack");
        await Frames(28);
        Input.ActionRelease("attack");
        Check(_enemy.Health == 85 && !_player.IsAttacking, "A pickaxe swing with J deals 15 damage once, without a mineable target.");
        SelectSlot(1);
        await UseTool(lower, 3);
        await Frames(15);
        Check(_enemy.Health == 70 && !_player.IsAttacking, "An axe swing with left click deals 15 damage once, without harvesting.");
        SelectSlot(2);
        Input.ActionPress("attack");
        await Frames(28);
        Input.ActionRelease("attack");
        Check(_enemy.Health == 45, "The equipped sword retains its stronger 25 damage with J.");
        _enemy.ProcessMode = ProcessModeEnum.Inherit;
        await Frames(115);
        Check(_player.Health < 100, "Enemy attacks damage the taller player when exposed.");
        _enemy.ProcessMode = ProcessModeEnum.Disabled;
        _player.Position = new Vector2(448, GroundY);
        _player.Velocity = Vector2.Zero;
        _enemy.Position = new Vector2(544, GroundY);
        _enemy.Velocity = Vector2.Zero;
        SelectSlot(2);
        for (int i = 0; i < 2; i++)
        {
            Input.ActionPress("use_primary"); await Frames(28);
            Input.ActionRelease("use_primary"); await Frames(2);
        }
        Check(_enemy.IsDead && !_world.HasBlock(lower) && _inventory.Count(BlockKind.Wood) == woodBefore,
            "Sword left-click attacks defeat the creature without mining or placing blocks.");
        _testTarget = BlockWorld.Center(lower);
        Input.ActionPress("use_primary");
        await Frames(3);
        Input.ActionRelease("use_primary");
        SelectSlot(5);
        Check(!_player.IsAttacking, "Changing equipment cancels a pending sword swing.");
        Input.ActionPress("use_secondary");
        await Frames(3);
        Input.ActionRelease("use_secondary");
        Check(_world.HasBlock(lower) && _inventory.Count(BlockKind.Wood) == woodBefore - 1,
            "Building continues after victory in the same scene.");
        var nearbyEnemy = new Enemy { Position = new Vector2(544, GroundY), ProcessMode = ProcessModeEnum.Disabled };
        AddChild(nearbyEnemy);
        _player.Position = new Vector2(448, GroundY);
        _player.Velocity = Vector2.Zero;
        SelectSlot(0);
        await Frames(3);
        int dirtBefore = _inventory.Count(BlockKind.Dirt);
        Vector2I harvestCell = new(18, BlockWorld.SurfaceRow + 1);
        _testTarget = BlockWorld.Center(harvestCell);
        Input.ActionPress("use_primary");
        await Frames(10);
        Check(nearbyEnemy.Health == 85 && _world.HasBlock(harvestCell),
            "A harvesting swing damages a nearby enemy before the targeted dirt finishes breaking.");
        await Frames(14);
        Input.ActionRelease("use_primary");
        await Frames(35);
        Check(!_world.HasBlock(harvestCell) && _inventory.Count(BlockKind.Dirt) == dirtBefore + 1,
            "Harvesting still completes and collects material while tool swings deal melee damage.");
    }
}
