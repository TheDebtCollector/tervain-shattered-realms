namespace Tervain;

public enum EquipmentKind { Pickaxe, Axe, Sword, Dirt, Stone, Wood, IronOre, Sand, Sandstone, CrackedEarth, Cactus }

/// <summary>One shared equipment mapping for the hotbar, held item, and mining rules.</summary>
public static class EquipmentRules
{
    public const int SlotCount = 11;
    public static bool IsMiningTool(EquipmentKind item) => item is EquipmentKind.Pickaxe or EquipmentKind.Axe;
    public static bool CanSwing(EquipmentKind item) => IsMiningTool(item) || item == EquipmentKind.Sword;
    public static bool CanMine(EquipmentKind item, BlockKind block) => block switch
    {
        BlockKind.Dirt or BlockKind.Stone or BlockKind.IronOre or BlockKind.Sand or BlockKind.Sandstone or BlockKind.CrackedEarth => item == EquipmentKind.Pickaxe,
        BlockKind.Wood or BlockKind.Cactus => item == EquipmentKind.Axe,
        _ => false
    };
    public static bool TryGetMaterial(EquipmentKind item, out BlockKind material)
    {
        switch (item)
        {
            case EquipmentKind.Dirt: material = BlockKind.Dirt; return true;
            case EquipmentKind.Stone: material = BlockKind.Stone; return true;
            case EquipmentKind.Wood: material = BlockKind.Wood; return true;
            case EquipmentKind.IronOre: material = BlockKind.IronOre; return true;
            case EquipmentKind.Sand: material = BlockKind.Sand; return true;
            case EquipmentKind.Sandstone: material = BlockKind.Sandstone; return true;
            case EquipmentKind.CrackedEarth: material = BlockKind.CrackedEarth; return true;
            case EquipmentKind.Cactus: material = BlockKind.Cactus; return true;
            default: material = default; return false;
        }
    }
    public static string Name(EquipmentKind item) => item switch
    { EquipmentKind.IronOre => "Iron ore", EquipmentKind.CrackedEarth => "Dry earth", _ => item.ToString() };
    public static string Shortcut(int slot) => slot < 9 ? (slot + 1).ToString() : slot == 9 ? "0" : "−";
    public static string Purpose(EquipmentKind item) => item switch
    {
        EquipmentKind.Pickaxe => "Terrain / Ore",
        EquipmentKind.Axe => "Wood / Cactus",
        EquipmentKind.Sword => "Fight",
        _ => "Build"
    };
}
