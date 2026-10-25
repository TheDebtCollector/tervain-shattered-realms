namespace Tervain;

public enum EquipmentKind { Pickaxe, Axe, Sword, Dirt, Stone, Wood }

/// <summary>One shared equipment mapping for the hotbar, held item, and mining rules.</summary>
public static class EquipmentRules
{
    public const int SlotCount = 6;
    public static bool IsMiningTool(EquipmentKind item) => item is EquipmentKind.Pickaxe or EquipmentKind.Axe;
    public static bool CanMine(EquipmentKind item, BlockKind block) => block switch
    {
        BlockKind.Dirt or BlockKind.Stone => item == EquipmentKind.Pickaxe,
        BlockKind.Wood => item == EquipmentKind.Axe,
        _ => false
    };
    public static bool TryGetMaterial(EquipmentKind item, out BlockKind material)
    {
        switch (item)
        {
            case EquipmentKind.Dirt: material = BlockKind.Dirt; return true;
            case EquipmentKind.Stone: material = BlockKind.Stone; return true;
            case EquipmentKind.Wood: material = BlockKind.Wood; return true;
            default: material = default; return false;
        }
    }
    public static string Purpose(EquipmentKind item) => item switch
    {
        EquipmentKind.Pickaxe => "Dirt / Stone",
        EquipmentKind.Axe => "Wood",
        EquipmentKind.Sword => "Fight",
        _ => "Build"
    };
}
