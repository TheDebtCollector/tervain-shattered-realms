using Godot;

namespace Tervain;

/// <summary>Solid terrain shields both combatants, including blocks placed during combat.</summary>
public static class CombatRules
{
    public static bool HasClearPath(Node2D attacker, Node2D target)
    {
        var ray = PhysicsRayQueryParameters2D.Create(
            attacker is Player playerAttacker ? playerAttacker.BodyCenter : attacker.GlobalPosition + new Vector2(0, -24),
            target is Player playerTarget ? playerTarget.BodyCenter : target.GlobalPosition + new Vector2(0, -24), 1);
        return attacker.GetWorld2D().DirectSpaceState.IntersectRay(ray).Count == 0;
    }
}
