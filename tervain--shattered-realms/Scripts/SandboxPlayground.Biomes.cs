using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tervain;

public partial class SandboxPlayground
{
    private static bool _biomeRestartChecked;
    private void SelectNextAvailableSlot(int direction)
    {
        int slot = _selected;
        do
        {
            slot = Mathf.PosMod(slot + direction, EquipmentRules.SlotCount);
            if (slot < 3 ||
                (EquipmentRules.TryGetMaterial(EquipmentRules.AtSlot(slot), out BlockKind kind) && _inventory.Count(kind) > 0))
            { SelectSlot(slot); return; }
        } while (slot != _selected);
    }

    private void TravelToBiome(int index)
    {
        if (_player.IsDead) { _hud.Notify("Press R to start again before travelling."); return; }
        index = Mathf.PosMod(index, _world.BiomeCount);
        try { _player.Position = _world.ArrivalFor(index); }
        catch (InvalidOperationException)
        { _hud.Notify("Clear a safe arrival space in that biome before travelling."); return; }
        _player.Velocity = Vector2.Zero;
        _world.UpdateNearbyCollision(_player.Position);
        _miningCell = null;
        _mineElapsed = 0;
        _world.HighlightCell = null;
        var camera = GetNode<Camera2D>("Player/Camera2D");
        camera.ResetSmoothing();
        camera.ForceUpdateScroll();
        _hud.Notify(BiomeCatalog.All[index].Name);
    }

    private async void RunBiomeTests()
    {
        try
        {
            _enemy.ProcessMode = ProcessModeEnum.Disabled;
            if (_biomeRestartChecked)
            {
                await Frames(8);
                TravelToBiome(19);
                await Frames(8);
                Check(_player.IsOnFloor() && _world.BiomeAt(_player.Position) == 19 && _inventory.Count((BlockKind)BiomeCatalog.All[3].Soil) == 0,
                    "Reset after visiting every biome restores the world and inventory, and final-biome travel still works.");
                GD.Print("BIOME TESTS PASSED.");
                GetTree().Quit();
                return;
            }
            Check(BiomeCatalog.All.Length == 20 && BlockWorld.Width == 944, "All twenty biomes have playable regions.");
            int validated = 0;
            foreach (BiomeDefinition biome in BiomeCatalog.All)
            {
                foreach (string path in biome.Decorations)
                    Check(BiomeArt.Texture(path).GetWidth() == 24 && BiomeArt.Texture(path).GetHeight() == 24, biome.Name + " decorations use native 24px assets.");
                foreach (BiomeMaterial material in biome.Materials)
                {
                    Check(BiomeArt.Texture(material.Texture).GetWidth() == 24 && BiomeArt.Texture(material.Wall).GetWidth() == 24,
                        material.Name + " foreground/wall textures load.");
                    Check(EquipmentRules.CanMine(material.Wood ? EquipmentKind.Axe : EquipmentKind.Pickaxe, material.Kind)
                        && !EquipmentRules.CanMine(material.Wood ? EquipmentKind.Pickaxe : EquipmentKind.Axe, material.Kind),
                        material.Name + " preserves tool requirements.");
                    Check(float.IsFinite(BlockWorld.MineDuration(material.Kind)), material.Name + " is harvestable.");
                    var inv = new SandboxInventory();
                    inv.Add(material.Kind); Check(inv.Spend(material.Kind) && inv.Count(material.Kind) == 0 && !inv.Spend(material.Kind),
                        material.Name + " has a distinct inventory stack.");
                    validated++;
                }
                Check(BiomeArt.Texture(biome.Liquid).GetWidth() == 24 && BiomeArt.Texture(biome.LiquidSurface).GetWidth() == 24,
                    biome.Name + " liquid assets load.");
            }
            Check(validated == 51, "Fifty-one additional materials are registered without enum duplication.");
            var seen = new HashSet<EquipmentKind>();
            for (int slot = 0; slot < EquipmentRules.SlotCount; slot++)
                Check(seen.Add(EquipmentRules.AtSlot(slot)), "Every hotbar slot maps to a distinct item.");
            for (int index = 0; index < _world.BiomeCount; index++)
            {
                TravelToBiome(index);
                await Frames(8);
                Check(_world.BiomeAt(_player.Position) == index && _player.IsOnFloor() && _player.Health == 100,
                    BiomeCatalog.All[index].Name + " arrival is clear, grounded, and inside its region.");
                Check(_world.ActiveColliderCount < 5000, "Offscreen biome terrain does not accumulate physics objects.");
                if (index < 3) continue;
                BiomeDefinition biome = BiomeCatalog.All[index];
                Vector2I floor = BlockWorld.CellAt(_player.Position + new Vector2(0, 1));
                Check(_world.TryGetBlock(floor, out BlockKind original) && original == (BlockKind)biome.Soil,
                    biome.Name + " uses its own terrain material.");
                SelectSlot(0);
                int count = _inventory.Count(original);
                Check(_world.TryMine(floor, _player, out BlockKind mined) && mined == original && _world.HasWall(floor),
                    biome.Name + " mining preserves the dedicated background wall.");
                _inventory.Add(mined);
                // Move aside before restoring the excavated floor, retaining the original biome identity.
                _player.Position += new Vector2(36, 0);
                await Frames(2);
                Check(_world.TryPlace(floor, mined, _player, _inventory) && _inventory.Count(mined) == count
                    && _world.TryGetBlock(floor, out BlockKind placed) && placed == original,
                    biome.Name + " blocks can be placed and retain their art identity.");
                int waterRow = biome.Underground ? 23 : 20;
                Check(_world.HasLiquid(new Vector2I(BiomeCatalog.Start(index) + 31, waterRow))
                    && !_world.HasBlock(new Vector2I(BiomeCatalog.Start(index) + 31, waterRow)),
                    biome.Name + " pool is visible scenery without a solid water collider.");
            }
            SelectSlot(14);
            SelectNextAvailableSlot(1);
            Check(_player.EquippedItem == EquipmentKind.Pickaxe, "Wheel navigation skips empty additional stacks.");
            BlockKind collected = (BlockKind)BiomeCatalog.All[3].Soil;
            _inventory.Add(collected);
            SelectSlot(14);
            SelectNextAvailableSlot(1);
            Check(EquipmentRules.TryGetMaterial(_player.EquippedItem, out BlockKind selected) && selected == collected,
                "Collected biome materials become selectable through the real hotbar mapping.");
            TravelToBiome(0);
            GetViewport().PushInput(new InputEventKey { PhysicalKeycode = Key.Pagedown, Pressed = true });
            await Frames(4);
            GetViewport().PushInput(new InputEventKey { PhysicalKeycode = Key.Pagedown, Pressed = false });
            Check(_world.BiomeAt(_player.Position) == 1, "Page Down travels through the actual input handler.");
            GetViewport().PushInput(new InputEventKey { PhysicalKeycode = Key.Pageup, Pressed = true });
            await Frames(4);
            GetViewport().PushInput(new InputEventKey { PhysicalKeycode = Key.Pageup, Pressed = false });
            Check(_world.BiomeAt(_player.Position) == 0, "Page Up returns to the previous biome.");
            foreach (Node control in _hud.GetChild<Control>(0).GetChildren())
                if (control is OptionButton selector) selector.EmitSignal(OptionButton.SignalName.ItemSelected, 5L);
            await Frames(4);
            Check(_world.BiomeAt(_player.Position) == 5, "The biome selector is connected to playable travel.");
            _biomeRestartChecked = true;
            GetTree().ReloadCurrentScene();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async void SaveBiomePreviews()
    {
        try
        {
            _enemy.ProcessMode = ProcessModeEnum.Disabled;
            _testTarget = new Vector2(-10000, -10000);
            DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("res://.godot/biome-previews"));
            for (int index = 0; index < _world.BiomeCount; index++)
            {
                TravelToBiome(index);
                await Frames(12);
                _hud.Notify("");
                _world.HighlightCell = null;
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                Error result = GetViewport().GetTexture().GetImage().SavePng($"res://.godot/biome-previews/{index:00}-{BiomeCatalog.All[index].Id}.png");
                if (result != Error.Ok) throw new Exception("Biome preview failed: " + result);
            }
            GD.Print("TWENTY BIOME PREVIEWS SAVED.");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
}
