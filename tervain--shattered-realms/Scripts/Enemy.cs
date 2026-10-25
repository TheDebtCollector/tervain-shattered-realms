using Godot;

namespace Tervain;

public partial class Enemy : CharacterBody2D
{
    public enum Intent { Approach, Windup, Strike, Recover, Hurt, Dead }
    [Export] public float MoveSpeed { get; set; } = 100;
    [Export] public float WindupDuration { get; set; } = 0.65f;
    [Export] public float RecoveryDuration { get; set; } = 0.85f;
    [Export] public int AttackDamage { get; set; } = 20;
    // Zero keeps the original combat room's always-active behaviour.
    [Export] public float NoticeRange { get; set; }
    public int Health { get; private set; } = 100;
    public bool IsDead => Health <= 0;
    public Intent State { get; private set; } = Intent.Approach;
    public Rect2 BodyBounds => new(GlobalPosition + new Vector2(-20, -48), new Vector2(40, 48));
    public string StateText => State switch
    {
        Intent.Windup => "WIND-UP — DODGE!",
        Intent.Strike => "STRIKE",
        Intent.Recover => "RECOVERING — ATTACK!",
        Intent.Hurt => "STAGGERED",
        Intent.Dead => "DEFEATED",
        _ => _alerted ? "APPROACHING" : "WATCHING"
    };
    private Player _player = null!;
    private float _timer;
    private int _facing = -1;
    private bool _didHit;
    private bool _alerted;

    public override void _Ready()
    {
        CollisionLayer = 4;
        CollisionMask = 1;
        AddChild(new CollisionShape2D
        {
            Position = new Vector2(0, -24),
            Shape = new RectangleShape2D { Size = new Vector2(38, 48) }
        });
        _player = GetTree().GetFirstNodeInGroup("player") as Player ?? throw new System.InvalidOperationException("Player missing.");
        AddToGroup("enemies");
        _alerted = NoticeRange <= 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _timer -= dt;
        float horizontal = 0;
        float distance = _player.GlobalPosition.X - GlobalPosition.X;
        if (!IsDead && !_player.IsDead)
        {
            switch (State)
            {
                case Intent.Approach:
                    _alerted = NoticeRange <= 0 || GlobalPosition.DistanceTo(_player.GlobalPosition) <= NoticeRange;
                    if (!_alerted) break;
                    _facing = distance >= 0 ? 1 : -1;
                    horizontal = _facing * MoveSpeed;
                    if (Mathf.Abs(distance) < 108 && Mathf.Abs(_player.Position.Y - Position.Y) < 65)
                        SetState(Intent.Windup, WindupDuration);
                    break;
                case Intent.Windup:
                    // Direction stays locked: the player can dodge through or retreat.
                    if (_timer <= 0) { SetState(Intent.Strike, 0.2f); _didHit = false; }
                    break;
                case Intent.Strike:
                    horizontal = _facing * 230;
                    Rect2 hit = new(GlobalPosition + new Vector2(_facing > 0 ? 0 : -105, -60), new Vector2(105, 65));
                    foreach (Node node in GetTree().GetNodesInGroup("block_worlds"))
                        if (node is BlockWorld world) world.BreakBushes(hit, this);
                    Rect2 target = _player.BodyBounds;
                    if (!_didHit && hit.Intersects(target) && CombatRules.HasClearPath(this, _player))
                    { _player.ReceiveHit(AttackDamage, _facing); _didHit = true; }
                    if (_timer <= 0) SetState(Intent.Recover, RecoveryDuration);
                    break;
                case Intent.Recover:
                case Intent.Hurt:
                    if (_timer <= 0) SetState(Intent.Approach, 0);
                    break;
            }
        }
        Velocity = new Vector2(horizontal, Velocity.Y + 1550 * dt);
        MoveAndSlide();
        QueueRedraw();
    }

    private void SetState(Intent state, float duration) { State = state; _timer = duration; }

    public void ReceiveHit(int damage, int direction)
    {
        if (IsDead) return;
        Health = Mathf.Max(0, Health - damage);
        // Hits only stagger during recovery, preserving the threat of a committed wind-up.
        if (IsDead) SetState(Intent.Dead, 0);
        else if (State is Intent.Recover or Intent.Hurt) SetState(Intent.Hurt, 0.18f);
        QueueRedraw();
    }

    public override void _Draw()
    {
        Color body = State switch
        {
            Intent.Windup => new Color("ffc36b"),
            Intent.Strike => new Color("ff715e"),
            Intent.Recover => new Color("96b89c"),
            Intent.Hurt => Colors.White,
            Intent.Dead => new Color("575966"),
            _ => new Color("b78c9a")
        };
        DrawCircle(new Vector2(0, 0), 23, new Color(0, 0, 0, 0.22f));
        if (IsDead) { DrawRect(new Rect2(-25, -14, 50, 14), body); return; }
        DrawRect(new Rect2(-20, -38, 40, 33), body);
        DrawCircle(new Vector2(0, -39), 20, body);
        DrawLine(new Vector2(-16, -47), new Vector2(-23, -62), body, 7);
        DrawLine(new Vector2(16, -47), new Vector2(23, -62), body, 7);
        DrawCircle(new Vector2(_facing * 10, -42), 4, new Color("302737"));
        DrawRect(new Rect2(-25, -79, 50, 5), new Color("343140"));
        DrawRect(new Rect2(-25, -79, 50 * Health / 100f, 5), new Color("f0b190"));
        if (State == Intent.Windup)
        {
            DrawCircle(new Vector2(0, -100), 12, new Color("ffc36b"));
            DrawLine(new Vector2(0, -107), new Vector2(0, -100), new Color("382e3b"), 3);
            DrawCircle(new Vector2(0, -94), 2, new Color("382e3b"));
        }
        if (State == Intent.Strike)
            DrawArc(new Vector2(0, -28), 95, _facing > 0 ? -0.7f : Mathf.Pi - 0.7f,
                _facing > 0 ? 0.7f : Mathf.Pi + 0.7f, 16, new Color("ff9370"), 8, true);
    }
}
