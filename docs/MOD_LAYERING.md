# Mod Layering Architecture

## Overview

The Dom5Editor uses a layered approach to display and edit entity properties. Properties can come from multiple sources, and the UI must correctly display the combined/merged result.

## Property Layers (in order of precedence)

1. **Session Changes (ChangesMod)** - Edits made in the current editing session
2. **Loaded Mod** - Properties defined in the user's mod file
3. **Vanilla** - Base game data from vanilla.dm

When displaying properties, the system should "snake down" through these layers:
- First check session changes
- Fall back to loaded mod
- Fall back to vanilla

## The Sparse Entity Problem

**Problem**: When a mod file uses `#selectmonster 42` and modifies a few properties, the mod's entity only contains those specific properties - NOT the full vanilla entity data.

**Example**:
```dm
#selectmonster 42
#hp 100
#end
```

This creates a Monster entity in the mod with only 1 property (HP). The vanilla monster 42 might have 29+ properties including magic skills, but the mod entity is "sparse" - it only stores the delta.

**Impact**: If the ViewModel receives the mod's sparse entity and accesses `monster.MagicSkills`, it will return empty because MAGICSKILL properties aren't in the sparse entity.

## Solution: Layered Property Access

### Method 1: BuildBadgesFromSection (JSON-driven badges)

The `BuildBadgesFromSection` method in `MonsterViewModel` implements layered access for all JSON-configured properties:

```csharp
private (ObservableCollection<PropertyItem> active, ObservableCollection<AvailablePropertyItem> available)
    BuildBadgesFromSection(string sectionId, EventHandler<int>? valueChangedHandler = null)
{
    var vanillaEntity = GetVanillaEntity();

    foreach (var cmdDef in section.Commands)
    {
        // Layer 1: Get vanilla value (base layer)
        bool vanillaHasValue = false;
        int? vanillaValue = null;
        if (vanillaEntity != null)
        {
            var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
            if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
            {
                vanillaValue = vanillaProp?.Value;
                vanillaHasValue = true;
            }
        }

        // Layer 2: Get current entity value (mod + session overlay)
        var entityResult = _entity.TryGet<IntProperty>(command, out var entityProp);
        // ...

        // Determine effective value (entity overrides vanilla)
        int? effectiveValue = entityHasValue ? entityValue : vanillaValue;

        // Track modification status
        bool isModified = entityHasValue && (!vanillaHasValue || entityValue != vanillaValue);
        bool isSessionEdit = IsPropertyEditedInSession(command);
    }
}
```

### Method 2: GetIntProperty (Core stats)

The base `EntityViewModel.GetIntProperty` method has built-in fallback:

```csharp
protected int? GetIntProperty(Command command)
{
    var result = _entity.TryGet<IntProperty>(command, out var prop);
    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
        return prop?.Value;

    // For VanillaModified entities, fall back to vanilla
    if (_source == EntitySource.VanillaModified)
    {
        var vanillaEntity = GetVanillaEntity();
        if (vanillaEntity != null)
        {
            var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
            if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                return vanillaProp?.Value;
        }
    }
    return null;
}
```

### Method 3: Custom Layering (Magic Paths)

For complex properties like MAGICSKILL (IntIntProperty), custom layering is required:

```csharp
private void RefreshMagicPaths()
{
    var allSkills = new Dictionary<int, (int Level, bool IsModified, bool IsSessionEdit)>();

    // 1. Start with vanilla as base layer
    var vanillaMonster = GetVanillaEntity() as Monster;
    if (vanillaMonster != null)
    {
        foreach (var skill in vanillaMonster.MagicSkills)
        {
            allSkills[pathId] = (skill.Level, false, false);
        }
    }

    // 2. Overlay mod/current entity changes
    if (monster != vanillaMonster)
    {
        foreach (var skill in monster.MagicSkills)
        {
            // Mod properties override vanilla
            allSkills[pathId] = (skill.Level, isModified: true, isSessionEdit);
        }
    }
}
```

### Method 4: Equipment Layering (WEAPON, ARMOR)

Equipment references use recursive copystats chain traversal:

```csharp
private void RefreshWeaponsList()
{
    _weaponsList = new ObservableCollection<EquipmentItem>();

    // Get all weapon references directly on the entity
    var directWeapons = new HashSet<int>();
    foreach (var prop in _entity.GetMultiple(Command.WEAPON))
    {
        if (prop is WeaponRef weaponRef && weaponRef.HasValue)
        {
            directWeapons.Add(weaponRef.ID);
            _weaponsList.Add(new EquipmentItem
            {
                ID = weaponRef.ID,
                Name = weaponRef.Entity?.Name,
                IsModified = IsWeaponModified(weaponRef.ID),
                IsInherited = false  // Direct property
            });
        }
    }

    // Add inherited weapons from copystats chain
    if (_entity.TryGetCopyFrom(out var copyFrom) && copyFrom != null)
    {
        AddInheritedWeapons(copyFrom, directWeapons, new HashSet<IDEntity> { _entity });
    }
}

private void AddInheritedWeapons(IDEntity source, HashSet<int> directWeapons, HashSet<IDEntity> visited)
{
    if (visited.Contains(source)) return;
    visited.Add(source);

    foreach (var prop in source.GetMultiple(Command.WEAPON))
    {
        if (prop is WeaponRef weaponRef && !directWeapons.Contains(weaponRef.ID))
        {
            directWeapons.Add(weaponRef.ID);
            _weaponsList.Add(new EquipmentItem
            {
                ID = weaponRef.ID,
                Name = weaponRef.Entity?.Name,
                IsInherited = true,  // From copystats
                CanRemove = false    // Cannot remove inherited
            });
        }
    }

    // Recurse through copystats chain
    if (source.TryGetCopyFrom(out var nextCopy) && nextCopy != null)
    {
        AddInheritedWeapons(nextCopy, directWeapons, visited);
    }
}
```

Key points:
- Direct properties can be removed; inherited cannot
- Recursively follows copystats chain to find all inherited equipment
- Avoids infinite loops via visited set
- Same pattern used for ARMOR

## Entity Sources in ViewModels

The `EntitySource` enum tracks where an entity came from:

- `Vanilla` - Pure vanilla entity, no modifications
- `VanillaModified` - Vanilla entity with mod overrides (sparse mod entity passed to VM)
- `FromMod` - New entity created by the mod (not in vanilla)
- `New` - Entity created during this editing session

## Key Classes

### VanillaLoader
- Loads vanilla.dm and caches the full game data
- `VanillaLoader.Vanilla.Database[EntityType.MONSTER]` contains all vanilla monsters with full properties
- Must be initialized with correct `VanillaDmPath` before use

### ChangesMod
- Tracks property changes made during the current editing session
- Stores deltas, not full entities
- Used for export and session edit tracking

### EntityViewModel
- `GetVanillaEntity()` - Looks up the vanilla version of the current entity
- Should be used to implement layered property access

## Implementation Checklist

Layered access is implemented for all property types:

- [x] Magic Paths (MAGICSKILL) - Custom layering in `RefreshMagicPaths()`
- [x] Type Badges (31 commands) - Via `BuildBadgesFromSection()`
- [x] General Properties (345 commands) - Via `BuildBadgesFromSection()`
- [x] Combat Properties (163 commands) - Via `BuildBadgesFromSection()`
- [x] Resistance Properties (19 commands) - Via `BuildBadgesFromSection()`
- [x] Core Stats (12 commands) - Via `GetIntProperty()` with fallback
- [x] Equipment References (WEAPON, ARMOR) - Recursive copystats chain in `RefreshWeaponsList()` / `RefreshArmorList()`

**Total: 572 commands covered (102% of reference)**

## Modification Indicators

The UI should distinguish between property sources:

- **Gold indicator** - Property modified from vanilla (either by mod or session)
- **Cyan indicator** - Property edited in current session
- **"inh" badge** - Property inherited from copystats

## Files Reference

- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Base class with `GetVanillaEntity()`
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - MonsterViewModel with:
  - `RefreshMagicPaths()` - Magic path layering
  - `RefreshWeaponsList()` / `RefreshArmorList()` - Equipment layering with copystats inheritance
  - `BuildBadgesFromSection()` - JSON-driven badge layering
  - `EquipmentItem` / `AvailableEquipmentItem` - Data classes for equipment display
- `Dom5Editor/UI/Views/MonsterView.xaml` - Equipment section with horizontal layout
- `Dom5Editor/Data/monster_badges.json` - 572 commands with descriptions and tooltips
- `Dom5Edit/VanillaLoader.cs` - Vanilla data loading
- `Dom5Edit/Mod/ChangesMod.cs` - Session change tracking
- `Dom5Editor/App.xaml.cs` - VanillaLoader initialization at startup
