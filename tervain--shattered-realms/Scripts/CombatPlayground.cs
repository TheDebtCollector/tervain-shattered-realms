using Godot;
using System;
using System.Threading.Tasks;

namespace Tervain;

public partial class CombatPlayground : Node2D
{
    private Player _player = null!;
    private Enemy _enemy = null!;
    private Label _health = null!, _status = null!, _dodge = null!;
    private ProgressBar _healthBar = null!;
    private static bool _testingRestart;

    public override void _Ready()
    {
        ConfigureInput();
        RenderingServer.SetDefaultClearColor(new Color("14232d"));
        AddPlatform(new Rect2(0, 620, 1152, 100));
        AddPlatform(new Rect2(0, -100, 24, 820));
        AddPlatform(new Rect2(1128, -100, 24, 820));
        AddPlatform(new Rect2(160, 490, 190, 20), true);
        AddPlatform(new Rect2(420, 385, 170, 20), true);
        AddPlatform(new Rect2(690, 490, 190, 20), true);
        _player = GetNode<Player>("Player");
        _enemy = GetNode<Enemy>("TrainingCreature");
        BuildHud();
        if (Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--sandbox-test"))
        {
            Check(InputMap.ActionHasEvent("attack", new InputEventMouseButton { ButtonIndex = MouseButton.Left }), "Combat room restores left-click attack after scene transition.");
            CallDeferred(MethodName.FinishSandboxVerification);
        }
        if (Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--prototype-preview"))
            CallDeferred(MethodName.SavePreview);
        if (Array.Exists(OS.GetCmdlineUserArgs(), arg => arg == "--prototype-test"))
        {
            if (_testingRestart)
            {
                if (_player.Health == 100 && _enemy.Health == 100)
                { GD.Print("PASS: restart restores both combatants. PROTOTYPE TESTS PASSED."); GetTree().Quit(); }
                else GetTree().Quit(1);
            }
            else CallDeferred(MethodName.RunPrototypeTests);
        }
    }

    private async void FinishSandboxVerification()
    {
        // Scene changes free the previous world's physics objects at the end of the frame.
        // Let that teardown finish before the automated cross-scene check exits the engine.
        await Frames(4);
        GD.Print("SANDBOX TESTS PASSED.");
        GetTree().Quit();
    }

    private static void ConfigureInput()
    {
        Bind("move_left", Key.A, Key.Left);
        Bind("move_right", Key.D, Key.Right);
        Bind("jump", Key.Space, Key.W, Key.Up);
        Bind("dodge", Key.Shift);
        Bind("attack", Key.J);
        Bind("restart", Key.R);
        Bind("sandbox_playground", Key.F3);
        if (!InputMap.ActionHasEvent("attack", new InputEventMouseButton { ButtonIndex = MouseButton.Left }))
            InputMap.ActionAddEvent("attack", new InputEventMouseButton { ButtonIndex = MouseButton.Left });
    }

    private static void Bind(string action, params Key[] keys)
    {
        if (InputMap.HasAction(action)) return;
        InputMap.AddAction(action);
        foreach (Key key in keys) InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private void AddPlatform(Rect2 rect, bool oneWay = false)
    {
        var body = new StaticBody2D { Position = rect.Position, CollisionLayer = 1, CollisionMask = 0 };
        body.AddChild(new CollisionShape2D
        {
            Position = rect.Size / 2,
            Shape = new RectangleShape2D { Size = rect.Size },
            OneWayCollision = oneWay
        });
        AddChild(body);
    }

    private void BuildHud()
    {
        var canvas = new CanvasLayer();
        AddChild(canvas);
        Label Text(string text, Vector2 position, int size, string colour)
        {
            var label = new Label { Text = text, Position = position, MouseFilter = Control.MouseFilterEnum.Ignore };
            label.AddThemeFontSizeOverride("font_size", size);
            label.AddThemeColorOverride("font_color", new Color(colour));
            canvas.AddChild(label);
            return label;
        }
        Text("TERVAIN / SHATTERED REALMS", new Vector2(44, 27), 15, "8cafa9");
        Text("The first crossing", new Vector2(42, 49), 32, "f3e6cc");
        Text("COMBAT PLAYGROUND  ·  01", new Vector2(44, 91), 13, "a1afb4");
        _health = Text("", new Vector2(44, 126), 16, "f3e6cc");
        _healthBar = new ProgressBar
        {
            Position = new Vector2(44, 153), Size = new Vector2(220, 12),
            MaxValue = 100, ShowPercentage = false, MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _healthBar.AddThemeStyleboxOverride("background", new StyleBoxFlat { BgColor = new Color("31474b") });
        _healthBar.AddThemeStyleboxOverride("fill", new StyleBoxFlat { BgColor = new Color("58c8bd") });
        canvas.AddChild(_healthBar);
        _dodge = Text("", new Vector2(44, 174), 15, "85ded0");
        _status = Text("", new Vector2(650, 130), 18, "f3e6cc");
        Text("Read the wind-up. Dodge the strike. Punish the recovery.", new Vector2(44, 231), 17, "a9bcb6");
        Text("A / D  Move     SPACE  Jump     SHIFT  Dodge     J / LMB  Attack     R  Restart",
            new Vector2(44, 668), 17, "e4ddca");
        Text("F3 Sandbox  /  Placeholder art  /  No loot, crafting or saving in this prototype", new Vector2(44, 698), 12, "91a6a6");
    }

    public override void _Process(double delta)
    {
        _health.Text = $"HEALTH  {_player.Health} / 100";
        _healthBar.Value = _player.Health;
        _dodge.Text = _player.DodgeReadyIn <= 0 ? "DODGE  Ready" : $"DODGE  {_player.DodgeReadyIn:0.0}s";
        _status.Text = _player.IsDead ? "YOU FELL\nPress R to try again."
            : _enemy.IsDead ? "ENCOUNTER COMPLETE\nPress R to practice again."
            : $"TRAINING CREATURE  {_enemy.Health} / 100\n{_enemy.StateText}";
        if (Input.IsActionJustPressed("restart")) GetTree().ReloadCurrentScene();
        if (Input.IsActionJustPressed("sandbox_playground")) GetTree().ChangeSceneToFile("res://Scenes/SandboxPlayground.tscn");
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 280, 1152, 340), new Color("1a3037"));
        for (int i = 0; i < 9; i++)
        {
            float x = i * 150 + 35;
            DrawRect(new Rect2(x, 290 + i % 3 * 30, 27, 330), new Color("203b3e"));
            DrawCircle(new Vector2(x + 13, 330 + i % 3 * 30), 69, new Color("203b3e"));
        }
        DrawCircle(new Vector2(970, 310), 58, new Color("3e4241"));
        DrawArc(new Vector2(970, 310), 58, 0, Mathf.Tau, 48, new Color("8b7962"), 7, true);
        DrawLine(new Vector2(948, 282), new Vector2(986, 338), new Color("ac9a7c"), 3);
        DrawLine(new Vector2(986, 282), new Vector2(948, 338), new Color("ac9a7c"), 3);
        DrawRect(new Rect2(0, 620, 1152, 100), new Color("303c3c"));
        DrawRect(new Rect2(0, 620, 1152, 7), new Color("819b78"));
        foreach (Rect2 platform in new[] { new Rect2(160, 490, 190, 20), new Rect2(420, 385, 170, 20), new Rect2(690, 490, 190, 20) })
        {
            DrawRect(platform, new Color("4b5750"));
            DrawRect(new Rect2(platform.Position, new Vector2(platform.Size.X, 5)), new Color("9da885"));
        }
        DrawRect(new Rect2(0, 655, 1152, 65), new Color("14232d"));
    }

    private async Task Frames(int count)
    {
        for (int i = 0; i < count; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    private async void SavePreview()
    {
        await Frames(3);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Error result = GetViewport().GetTexture().GetImage().SavePng("res://.godot/prototype-preview.png");
        GD.Print("Preview capture: " + result);
        GetTree().Quit(result == Error.Ok ? 0 : 1);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        GD.Print("PASS: " + message);
    }

    // Runs inside the actual Godot physics loop: --headless --path ... -- --prototype-test
    private async void RunPrototypeTests()
    {
        try
        {
            _enemy.ProcessMode = ProcessModeEnum.Disabled;
            await Frames(30);
            Check(_player.IsOnFloor(), "Player lands on the room floor.");
            float start = _player.Position.X;
            Input.ActionPress("move_right");
            await Frames(15);
            Input.ActionRelease("move_right");
            Check(_player.Position.X > start + 30, "Movement input moves the player.");
            float ground = _player.Position.Y;
            Input.ActionPress("jump");
            await Frames(5);
            Check(_player.Position.Y < ground - 15, "Jump leaves the floor.");
            Input.ActionRelease("jump");
            await Frames(65);
            Input.ActionPress("dodge");
            await Frames(2);
            Check(_player.IsDodging, "Dodge activates.");
            _player.ReceiveHit(20, -1);
            Check(_player.Health == 100, "Dodge prevents damage.");
            Input.ActionRelease("dodge");
            await Frames(16);
            Input.ActionPress("dodge");
            await Frames(2);
            Check(!_player.IsDodging, "Dodge cannot repeat during cooldown.");
            Input.ActionRelease("dodge");
            _player.ReceiveHit(20, -1);
            Check(_player.Health == 80, "Unprotected hits deal damage.");
            _player.ReceiveHit(20, -1);
            Check(_player.Health == 80, "Post-hit protection prevents repeated damage.");

            _player.Position = new Vector2(750, 620);
            _player.Velocity = Vector2.Zero;
            _enemy.Position = new Vector2(820, 620);
            for (int hit = 0; hit < 4; hit++)
            {
                Input.ActionPress("attack");
                await Frames(28);
                Input.ActionRelease("attack");
                await Frames(2);
                Check(_enemy.Health == 100 - (hit + 1) * 25, "Melee hits once per swing.");
            }
            Check(_enemy.IsDead, "Four attacks defeat the enemy.");

            var secondEnemy = new Enemy { Position = new Vector2(840, 620) };
            AddChild(secondEnemy);
            await Frames(5);
            Check(secondEnemy.State == Enemy.Intent.Windup, "Nearby enemy visibly winds up.");
            await Frames(44);
            Check(secondEnemy.State == Enemy.Intent.Strike, "Wind-up leads to a strike.");
            await Frames(15);
            Check(secondEnemy.State == Enemy.Intent.Recover, "Strike leaves a recovery window.");
            Check(_player.Health < 80, "Enemy strike damages an exposed player.");
            secondEnemy.ProcessMode = ProcessModeEnum.Disabled;
            await Frames(55);
            _player.ReceiveHit(1000, 0);
            Check(_player.IsDead, "Lethal damage defeats the player.");
            _testingRestart = true;
            Check(GetTree().ReloadCurrentScene() == Error.Ok, "Scene can restart.");
        }
        catch (Exception error)
        {
            GD.PushError("PROTOTYPE TEST FAILED: " + error);
            GetTree().Quit(1);
        }
    }
}
