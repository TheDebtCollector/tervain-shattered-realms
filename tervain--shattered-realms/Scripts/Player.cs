using Godot;

namespace Tervain;

public partial class Player : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 280f;
    // Preserve the established ~124px full jump when changing the terrain grid.
    [Export] public float JumpSpeed { get; set; } = 606.3542f;
    [Export] public float GravityStrength { get; set; } = 1550f;
    [Export] public float DodgeSpeed { get; set; } = 650f;
    [Export] public float DodgeDuration { get; set; } = 0.18f;
    [Export] public float DodgeCooldown { get; set; } = 0.8f;
    [Export] public int AttackDamage { get; set; } = 25;
    [Export] public int ToolDamage { get; set; } = 15;

    public int Health { get; private set; } = 100;
    public int Facing { get; private set; } = 1;
    public bool IsDodging => _dodgeLeft > 0;
    public bool IsAttacking => _attackLeft > 0;
    public float DodgeReadyIn => _dodgeCooldownLeft;
    public bool IsDead => Health <= 0;
    public EquipmentKind EquippedItem { get; private set; } = EquipmentKind.Sword;
    public const float BodyHeight = 72;
    public Vector2 BodyCenter => GlobalPosition + new Vector2(0, -BodyHeight / 2);
    public Rect2 BodyBounds => new(GlobalPosition + new Vector2(-14, -BodyHeight), new Vector2(28, BodyHeight));
    public Rect2 AttackBounds => new(
        GlobalPosition + new Vector2(Facing > 0 ? 0 : -96, -73), new Vector2(96, 84));

    private float _dodgeLeft, _dodgeCooldownLeft, _attackLeft, _invulnerableLeft;
    private float _coyoteLeft, _jumpBufferLeft, _hurtLeft;
    private bool _attackConnected;

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        CollisionLayer = 2;
        CollisionMask = 1;
        AddChild(new CollisionShape2D
        {
            Position = new Vector2(0, -BodyHeight / 2),
            Shape = new RectangleShape2D { Size = new Vector2(28, BodyHeight) }
        });
        AddToGroup("player");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _dodgeCooldownLeft = Mathf.Max(0, _dodgeCooldownLeft - dt);
        _invulnerableLeft = Mathf.Max(0, _invulnerableLeft - dt);
        _hurtLeft = Mathf.Max(0, _hurtLeft - dt);
        _coyoteLeft = IsOnFloor() ? 0.1f : Mathf.Max(0, _coyoteLeft - dt);
        _jumpBufferLeft = Input.IsActionJustPressed("jump") ? 0.12f : Mathf.Max(0, _jumpBufferLeft - dt);

        if (IsDead)
        {
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, 1200 * dt), Velocity.Y + GravityStrength * dt);
            MoveAndSlide();
            QueueRedraw();
            return;
        }

        float axis = Input.GetAxis("move_left", "move_right");
        if (axis != 0 && !IsAttacking && !IsDodging) Facing = axis > 0 ? 1 : -1;

        if (Input.IsActionJustPressed("dodge") && _dodgeCooldownLeft <= 0 && !IsDodging)
        {
            _dodgeLeft = DodgeDuration;
            _dodgeCooldownLeft = DodgeCooldown;
            _attackLeft = 0; // A defensive dodge can cancel a committed attack.
        }

        if (IsDodging)
        {
            _dodgeLeft = Mathf.Max(0, _dodgeLeft - dt);
            Velocity = new Vector2(Facing * DodgeSpeed, 0);
        }
        else
        {
            float speed = IsAttacking ? MoveSpeed * 0.3f : MoveSpeed;
            float vertical = Velocity.Y + GravityStrength * dt;
            if (_jumpBufferLeft > 0 && _coyoteLeft > 0)
            {
                vertical = -JumpSpeed;
                _jumpBufferLeft = _coyoteLeft = 0;
            }
            if (Input.IsActionJustReleased("jump") && vertical < -220) vertical *= 0.55f;
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, axis * speed, 2200 * dt), vertical);
            if (Input.IsActionJustPressed("attack")) TryStartAttack();
        }

        MoveAndSlide();
        if (_attackLeft > 0)
        {
            _attackLeft = Mathf.Max(0, _attackLeft - dt);
            if (_attackLeft <= 0.27f && _attackLeft >= 0.14f)
                foreach (Node node in GetTree().GetNodesInGroup("block_worlds"))
                    if (node is BlockWorld world) world.BreakBushes(AttackBounds, this);
            if (_attackLeft <= 0.27f && _attackLeft >= 0.14f && !_attackConnected)
            {
                foreach (Node node in GetTree().GetNodesInGroup("enemies"))
                    if (node is Enemy enemy && !enemy.IsDead && AttackBounds.Intersects(enemy.BodyBounds)
                        && CombatRules.HasClearPath(this, enemy))
                    {
                        enemy.ReceiveHit(EquipmentRules.IsMiningTool(EquippedItem) ? ToolDamage : AttackDamage, Facing);
                        _attackConnected = true;
                    }
            }
        }
        if (Position.Y > 850) ReceiveHit(1000, 0);
        QueueRedraw();
    }

    public void Equip(EquipmentKind item)
    {
        if (EquippedItem == item) return;
        EquippedItem = item;
        // Switching equipment ends the old action; a hidden weapon cannot finish dealing damage.
        _attackLeft = 0;
        QueueRedraw();
    }

    public bool TryStartAttack()
    {
        if (!EquipmentRules.CanSwing(EquippedItem) || IsDead || IsDodging || IsAttacking) return false;
        float axis = Input.GetAxis("move_left", "move_right");
        if (axis != 0) Facing = axis > 0 ? 1 : -1;
        _attackLeft = 0.38f;
        _attackConnected = false;
        return true;
    }

    public void ReceiveHit(int damage, int direction)
    {
        if (IsDead || IsDodging || _invulnerableLeft > 0) return;
        Health = Mathf.Max(0, Health - damage);
        _invulnerableLeft = 0.75f;
        _hurtLeft = 0.2f;
        Velocity = new Vector2(direction * 210, -160);
    }

    public override void _Draw()
    {
        Color coat = IsDead ? new Color("657176") : IsDodging ? new Color("9bf4ec") : new Color("58c8bd");
        if (_hurtLeft > 0) coat = Colors.White;
        if (_invulnerableLeft > 0 && (int)(_invulnerableLeft * 20) % 2 == 0) coat.A = 0.5f;
        DrawCircle(new Vector2(0, 1), 17, new Color(0, 0, 0, 0.2f));
        DrawRect(new Rect2(-13, -51, 26, 47), coat);
        DrawCircle(new Vector2(0, -60), 12, new Color("edcfaa"));
        DrawRect(new Rect2(-12, -72, 24, 10), new Color("263d49"));
        DrawCircle(new Vector2(Facing * 5, -60), 2, new Color("182b36"));
        DrawLine(new Vector2(-7, -5), new Vector2(-7, 0), new Color("243a46"), 7);
        DrawLine(new Vector2(7, -5), new Vector2(7, 0), new Color("243a46"), 7);
        Vector2 hand = new(Facing * 17, -36);
        // Animation follows the swing itself, including swings into empty air.
        float toolAngle = IsAttacking ? Mathf.Lerp(-0.65f, 1.15f, 1 - _attackLeft / 0.38f) : 0;
        DrawSetTransform(hand, toolAngle * Facing, new Vector2(Facing, 1));
        Color steel = new("d9e8e9");
        Color handle = new("bd8c56");
        switch (EquippedItem)
        {
            case EquipmentKind.Pickaxe:
                DrawLine(new Vector2(0, 5), new Vector2(10, -23), handle, 4);
                DrawLine(new Vector2(-2, -23), new Vector2(23, -18), steel, 5);
                DrawLine(new Vector2(23, -18), new Vector2(26, -10), steel, 3);
                break;
            case EquipmentKind.Axe:
                DrawLine(new Vector2(0, 5), new Vector2(9, -25), handle, 4);
                DrawColoredPolygon(new[] { new Vector2(7, -26), new Vector2(23, -24),
                    new Vector2(25, -12), new Vector2(8, -12) }, steel);
                break;
            case EquipmentKind.Sword:
                DrawLine(new Vector2(0, 5), new Vector2(3, -3), handle, 4);
                DrawLine(new Vector2(-2, -5), new Vector2(10, -1), new Color("d7bb75"), 3);
                DrawLine(new Vector2(4, -3), new Vector2(14, -31), steel, 5);
                break;
            default:
                if (EquipmentRules.TryGetMaterial(EquippedItem, out BlockKind block))
                {
                    BlockArt.Draw(this, new Rect2(0, -24, 24, 24), block);
                }
                break;
        }
        DrawSetTransform(Vector2.Zero);
        if (_attackLeft is > 0.14f and <= 0.27f)
        {
            DrawRect(AttackBounds with { Position = AttackBounds.Position - GlobalPosition }, new Color(0.65f, 1, 0.9f, 0.12f));
            float angle = Facing > 0 ? 0 : Mathf.Pi;
            DrawArc(new Vector2(0, -35), 73, angle - 0.75f, angle + 0.75f, 16, new Color("d4fff0"), 7, true);
        }
    }
}
