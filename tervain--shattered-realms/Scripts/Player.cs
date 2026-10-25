using Godot;

namespace Tervain;

public partial class Player : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 280f;
    [Export] public float JumpSpeed { get; set; } = 565f;
    [Export] public float GravityStrength { get; set; } = 1550f;
    [Export] public float DodgeSpeed { get; set; } = 650f;
    [Export] public float DodgeDuration { get; set; } = 0.18f;
    [Export] public float DodgeCooldown { get; set; } = 0.8f;
    [Export] public int AttackDamage { get; set; } = 25;

    public int Health { get; private set; } = 100;
    public int Facing { get; private set; } = 1;
    public bool IsDodging => _dodgeLeft > 0;
    public bool IsAttacking => _attackLeft > 0;
    public float DodgeReadyIn => _dodgeCooldownLeft;
    public bool IsDead => Health <= 0;
    public Rect2 AttackBounds => new(
        GlobalPosition + new Vector2(Facing > 0 ? 0 : -96, -65), new Vector2(96, 76));

    private float _dodgeLeft, _dodgeCooldownLeft, _attackLeft, _invulnerableLeft;
    private float _coyoteLeft, _jumpBufferLeft, _hurtLeft;
    private bool _attackConnected;

    public override void _Ready()
    {
        CollisionLayer = 2;
        CollisionMask = 1;
        AddChild(new CollisionShape2D
        {
            Position = new Vector2(0, -24),
            Shape = new RectangleShape2D { Size = new Vector2(28, 48) }
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
            if (Input.IsActionJustPressed("attack") && !IsAttacking)
            {
                _attackLeft = 0.38f;
                _attackConnected = false;
            }
        }

        MoveAndSlide();
        if (_attackLeft > 0)
        {
            _attackLeft = Mathf.Max(0, _attackLeft - dt);
            if (_attackLeft <= 0.27f && _attackLeft >= 0.14f && !_attackConnected)
            {
                foreach (Node node in GetTree().GetNodesInGroup("enemies"))
                    if (node is Enemy enemy && !enemy.IsDead && AttackBounds.Intersects(enemy.BodyBounds))
                    {
                        enemy.ReceiveHit(AttackDamage, Facing);
                        _attackConnected = true;
                    }
            }
        }
        if (Position.Y > 850) ReceiveHit(1000, 0);
        QueueRedraw();
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
        DrawRect(new Rect2(-13, -34, 26, 30), coat);
        DrawCircle(new Vector2(0, -43), 12, new Color("edcfaa"));
        DrawRect(new Rect2(-12, -54, 24, 10), new Color("263d49"));
        DrawCircle(new Vector2(Facing * 5, -43), 2, new Color("182b36"));
        DrawLine(new Vector2(-7, -5), new Vector2(-7, 0), new Color("243a46"), 7);
        DrawLine(new Vector2(7, -5), new Vector2(7, 0), new Color("243a46"), 7);
        Vector2 hand = new(Facing * 17, -24);
        DrawLine(hand, hand + new Vector2(Facing * 10, -23), new Color("d9e8e9"), 5);
        if (_attackLeft is > 0.14f and <= 0.27f)
        {
            DrawRect(AttackBounds with { Position = AttackBounds.Position - GlobalPosition }, new Color(0.65f, 1, 0.9f, 0.12f));
            float angle = Facing > 0 ? 0 : Mathf.Pi;
            DrawArc(new Vector2(0, -27), 73, angle - 0.75f, angle + 0.75f, 16, new Color("d4fff0"), 7, true);
        }
    }
}
