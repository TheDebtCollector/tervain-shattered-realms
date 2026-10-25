using System.Collections.Generic;

namespace Tervain;

public enum BlockKind { Dirt, Stone, Wood, Bedrock, IronOre, Sand, Sandstone, CrackedEarth, Cactus, Snow, Ice, FrozenEarth, TundraWood }

/// <summary>Material stacks for this prototype. Starter tools are permanent equipment, not stacks.</summary>
public sealed class SandboxInventory
{
    private readonly Dictionary<BlockKind, int> _counts = new();
    public int Count(BlockKind kind) => _counts.GetValueOrDefault(kind);
    public void Add(BlockKind kind, int amount = 1)
    {
        if (kind == BlockKind.Bedrock || amount <= 0) return;
        _counts[kind] = Count(kind) + amount;
    }
    public bool Spend(BlockKind kind)
    {
        if (Count(kind) <= 0) return false;
        _counts[kind]--;
        return true;
    }
}
