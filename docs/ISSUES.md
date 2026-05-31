# Known Issues

Potential bugs and critical problems identified during code review.

**Last Updated:** 2026-01-09

---

## Current Priority Issues

### ~~0. Copy Reference Changes Don't Refresh Inherited Properties~~ FIXED (2026-01-08)

**Status:** FIXED

**Problem:** When changing a copy reference (e.g., `#copystats` on a monster), the UI stats/badges didn't update to reflect the new inherited values from the copy source. The copy selector worked (value was set), but the dependent properties didn't refresh.

**Root Causes (Two Issues Found):**

1. **Badge Collections Not Refreshed:** Badge collections (StatsBadges, TypeBadges, etc.) are lazily initialized and cached. When a copy reference changed, the setter only notified a few properties but didn't invalidate/rebuild the cached badge collections.

2. **Reference Resolution Used Stale Entity:** In `StringOrIDRef.Resolve()`, the code used `ID` and `Name` property getters which return `Entity.ID`/`Entity.Name` if Entity is not null. When re-resolving after an ID change, this caused the lookup to use the OLD entity's ID instead of the new `_id` backing field value.

**Solutions Implemented:**

**Part 1 - ViewModel Badge Refresh:**
Added `RefreshAllCopyDependentProperties()` method to each ViewModel with copy commands. Called in the copy ID setter after the value is set.

**ViewModels Fixed:**
- `MonsterViewModel.cs` - `CopyStatsId` and `CopySprId` setters
- `WeaponViewModel.cs` - `CopyWeaponId` setter
- `ArmorViewModel.cs` - `CopyArmorId` setter
- `ItemViewModel.cs` - `CopyItemId` setter
- `SpellViewModel.cs` - `CopySpellId` setter
- `SiteViewModel.cs` - `CopySiteId` setter

**Part 2 - Reference Resolution Fix:**
Fixed `StringOrIDRef.Resolve()` and `IDRef.Resolve()` to:
1. Clear `Entity = null` and `Resolved = false` before resolving
2. Use backing fields `_id` and `_name` directly instead of property getters

```csharp
// StringOrIDRef.cs - Fixed Resolve()
public override void Resolve()
{
    // Clear existing entity before resolving to ensure we look up the new ID
    Entity = null;
    Resolved = false;

    if (Parent.ParentMod.TryGet(GetEntityType(), _id, _name, out IDEntity e))
    //                                           ^^^  ^^^^^
    //                               Uses backing fields, not getters
    {
        Entity = e;
        Resolved = true;
    }
    ...
}
```

**Files Modified:**
- `Dom5Edit/Props/References/StringOrIDRef.cs` - Fixed `Resolve()` to use `_id`/`_name`
- `Dom5Edit/Props/References/IDRef.cs` - Fixed `Resolve()` to clear state first
- `Dom5Editor/UI/ViewModels/*ViewModel.cs` - Added `RefreshAllCopyDependentProperties()`

**Note:** This fix enables the copy reference UI to work correctly, but does not yet implement the "clear" semantics where properties before a copy command are reset. See Issue #1 for that work.

---

### ~~1. CopyStats Reference Resolution Inconsistent~~ FIXED (2026-01-09)

**Status:** FIXED

**Added:** 2026-01-09

**Symptoms:**
- When setting `#copystats` on a vanilla monster (e.g., ID 6) to copy from another (e.g., ID 651), the stats didn't display
- Worked correctly for mod-defined entities, but not for unmodified vanilla entities

**Root Cause:**

For vanilla-based entities (`FromVanilla` or `VanillaModified`), the `_entity` object IS the vanilla entity with all its vanilla properties loaded directly. When `TryGet(HP)` was called, it found the vanilla HP directly on the entity and returned it - never checking the copystats chain.

The semantic distinction:
- **FromMod entities**: Direct properties are intentional overrides → copystats is fallback (correct)
- **Vanilla-based entities with session-added copystats**: Direct properties are vanilla base data → should be REPLACED by copystats

**Solution:**

Modified `EntityViewModel.BuildBadgesFromSection()` to detect this case and get values from the copy source instead of the entity when:
1. Source is NOT `FromMod` (i.e., `FromVanilla` or `VanillaModified`)
2. Entity has copystats
3. The specific property was NOT edited in the current session

```csharp
// Special handling for vanilla-based entities with copystats
bool useOnlyCopySource = false;
IDEntity copySourceForVanilla = null;
if (_source != EntitySource.FromMod &&
    _entity.TryGetCopyFrom(out copySourceForVanilla) &&
    copySourceForVanilla != null &&
    !IsPropertyEditedInSession(command))
{
    useOnlyCopySource = true;
}

if (useOnlyCopySource && copySourceForVanilla != null)
{
    // Get from copy source (bypasses vanilla properties on entity)
    var copyResult = copySourceForVanilla.TryGet<IntProperty>(command, out var copyProp);
    // ... use copy source value
}
else
{
    // Normal path: Get from entity (for FromMod, or session-edited properties)
    var entityResult = _entity.TryGet<IntProperty>(command, out var entityProp);
    // ... use entity value
}
```

**Files Modified:**
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Added vanilla-based + copystats detection in `BuildBadgesFromSection()`

**Priority Rules (now working correctly):**
1. Session-edited properties take highest priority (user explicitly set a value)
2. For vanilla-based entities with copystats, copy source values are used (vanilla base is bypassed)
3. For FromMod, entity properties override copystats (mod-defined properties are intentional overrides)

---

### ~~1b. Stats Badge Remove Button Not Working~~ FIXED (2026-01-09)

**Status:** FIXED

**Added:** 2026-01-09

**Symptoms:**
- On FromMod entities with stats defined, clicking the "x" button on stats badges did nothing
- Other badge types (weapons, armor, etc.) could be removed successfully
- The "x" button was visible and enabled (`CanRemove=True`), but clicking had no effect

**Root Cause:**

`MonsterView.xaml` was missing the `RemoveCommand` binding for the stats `BadgeGridPanel`, and `MonsterViewModel` was missing the `RemoveStatsBadgeCommand` property.

Other entity views (ArmorView, WeaponView) had this wired up correctly:
```xml
<!-- ArmorView.xaml - Correct -->
<controls:BadgeGridPanel
    ItemsSource="{Binding StatsBadges}"
    RemoveCommand="{Binding RemoveStatsBadgeCommand}"
    ... />

<!-- MonsterView.xaml - Was Missing RemoveCommand -->
<controls:BadgeGridPanel
    ItemsSource="{Binding StatsBadges}"
    ShowAddButton="False"/>
```

**Solution:**

1. Added `RemoveStatsBadgeCommand` property to `MonsterViewModel.cs`:
```csharp
private RelayCommand<PropertyItem> _removeStatsBadgeCommand;
public RelayCommand<PropertyItem> RemoveStatsBadgeCommand =>
    _removeStatsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshStatsBadges);
```

2. Added `RemoveCommand` binding in `MonsterView.xaml`:
```xml
<controls:BadgeGridPanel
    Columns="3"
    ItemsSource="{Binding StatsBadges}"
    RemoveCommand="{Binding RemoveStatsBadgeCommand}"
    ShowAddButton="False"/>
```

**Files Modified:**
- `Dom5Editor/UI/ViewModels/MonsterViewModel.cs` - Added `RemoveStatsBadgeCommand`
- `Dom5Editor/UI/Views/MonsterView.xaml` - Added `RemoveCommand` binding

**Behavior After Fix:**
- Clicking "x" removes the property from the entity
- Removal is recorded in ChangesMod (for export tracking)
- Badge refreshes to show the fallback value (from copystats or default)

---

### ~~2. Copy Commands Need Clear/Reset Mechanism~~ FIXED (2026-01-09)

**Status:** FIXED

**Implementation Summary:**

Clear commands (`#clearweapons`, `#cleararmor`, `#clearmagic`, `#clearspec`, `#clear`) are fully implemented with:

1. **PropertyGroupMap.cs** - Maps commands to property groups:
   - `PropertyGroup` enum: None, Weapons, Armor, Magic, Special, Sprites, All, Gods, Sites, NationSettings, Recruitment, Defense
   - `GetMonsterGroup(Command)` / `GetNationGroup(Command)` - Entity-specific mappings
   - `GetClearCommand(PropertyGroup)` - Returns the clear command for a group

2. **IDEntity.cs** - Core clear command logic:
   - `HasClearCommand(Command)` - Checks if entity has a specific clear command
   - `IsPropertyGroupCleared(Command)` - Returns true if property should NOT be inherited
   - `GetPropertyGroup(Command)` - Virtual method for entity-specific groupings
   - Integrated into `TryGet<T>()` to block inheritance when cleared

3. **Monster.cs / Nation.cs** - Override `GetPropertyGroup()` with entity-specific mappings

4. **MonsterViewModel.cs** - ViewModel properties:
   - `HasClearAll`, `HasClearWeapons`, `HasClearArmor`, `HasClearMagic`, `HasClearSpec`
   - `SetClearCommand()` helper auto-refreshes dependent properties after changes

5. **MonsterView.xaml** - UI checkbox row for all 5 clear commands with tooltips

6. **EntityViewModel.cs** - Fallback logic integration:
   - Badge building suppresses vanilla when cleared (line ~591)
   - Multi-value reference lists check clear status (line ~1255)
   - Copystats chain traversal respects clears (line ~1373)
   - VanillaModified fallback blocked when cleared (line ~1545)

**Behavior:**
- Clear commands block inheritance for their property group from both vanilla AND copystats sources
- `#clear` (all) blocks everything except identity commands
- Cascading copies respect clear commands at each level of the chain
- UI checkboxes toggle clear commands with immediate property refresh

---

### Copy Command Reference

**Key Rule:** `#copystats` copies everything EXCEPT sprites. Use `#copyspr` separately for sprites.

| Command | Entity | Copies |
|---------|--------|--------|
| `#copystats` | Monster | Everything except sprites |
| `#copyspr` | Monster | Sprites only (`#spr1`, `#spr2`) |
| `#copyweapon` | Weapon | All weapon properties |
| `#copyarmor` | Armor | All armor properties |
| `#copyitem` | Item | All item properties |
| `#copyspell` | Spell | All spell properties |
| `#copysite` | Site | All site properties |

**Clear Commands (Monster):** `#clearweapons`, `#cleararmor`, `#clearmagic`, `#clearspec`

Details on clear commands and their exact scope will be documented during feature planning/implementation.

---

### ~~0. AddBadgeProperty Does Not Support Reference Types~~ FIXED (2026-01-07)

**Status:** FIXED

**Problem:** When using the badge dropdown to add a reference-type property (e.g., `#addrecunit` on Nations), nothing happened because the code was using `IntProperty.Create()` instead of the correct reference property type from the entity's property map.

**Solution Implemented:**
1. Added `AddPropertyFromMap()` method that uses the entity's `GetPropertyMap()` to get the correct property factory
2. Updated `AddBadgeProperty()` to call `AddPropertyFromMap()` for reference types
3. Updated `RemoveIntPropertyByValue()` to handle both `IntProperty` and Reference types (`StringOrIDRef`, `MonsterOrMontagRef`)
4. Changed `GetPropertyMap()` from `internal` to `public` in `IDEntity.cs` and all entity subclasses

**Files Modified:**
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Added `AddPropertyFromMap()`, updated `RemoveIntPropertyByValue()`
- `Dom5Edit/Entities/IDEntity.cs` - Changed `GetPropertyMap()` to `public virtual`
- `Dom5Edit/Entities/*.cs` (11 files) - Changed `GetPropertyMap()` overrides to `public override`

---

### ~~1. Weapon/Armor Not Displaying for Mod-Edited Entities~~ FIXED (2026-01-05)

**Root Cause:** For `VanillaModified` entities (vanilla entities edited by a mod), the ViewModel was using the mod's entity which only contains the **changes** made by the mod - not the full entity data. Weapons/armor remained on the vanilla entity but weren't being fetched.

**Fix:** Added generic layered resolution methods to `EntityViewModel` base class:
- `ResolveEntityReference(type, id)` - Cascades: mod → vanilla
- `GetEntityName(type, id)` - Name lookup with fallback
- `GetReferenceName(reference, entityType)` - Reference name with fallback
- `GetLayeredReferenceList<TRef>()` - Multi-value properties with full layering

The weapon/armor lists now use `GetLayeredReferenceList<WeaponRef>()` and `GetLayeredReferenceList<ArmorRef>()`, reducing ~200 lines of duplicated code to ~50 lines.

**Files Changed:**
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Added generic resolution methods
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - Simplified to use base class methods

### ~~1. Two Parallel ViewModel Systems~~ COMPLETE (2026-01-07)

**Status:** COMPLETE - Legacy VMs removed.

The legacy ViewModel system has been fully removed:
- Deleted `Dom5Editor/VMs/` folder (all legacy VMs)
- Deleted `Dom5Editor/ViewModelBase/` folder
- Deleted `Dom5Editor/Menus/` folder (legacy views)
- Moved `RelayCommand.cs` to `Dom5Editor/UI/RelayCommand.cs`

The new UI system (`Dom5Editor/UI/`) is now the only ViewModel system.

---

### 2. Entity Views Not Implemented

**Status:** MonsterView, WeaponView, ArmorView, and ItemView are implemented in the new UI system.

**Recent Improvements (2026-01-06):**
- SpellView: Path requirements display, combat stats, next spell chains, fatigue/gem cost breakdown, 63 badge properties
- SiteView: Core stats, gems display with icons, copy-from reference, 90+ badge properties

**Recent Improvements (2026-01-05):**
- ItemView: Equipment display shows referenced weapon/armor stats, damage types, special properties, and secondary effects
- WeaponView: Added damage types, special properties, and secondary effect display
- Both views support VanillaModified fallback for reference properties

Missing entity views:
- Other potential views to check: Enchantment, Montag, RestrictedItem (ID-only containers, may not need views)

Completed entity views:
- EventView - Completed with 21 badge sections covering ~160 commands
- MercenaryView - Completed with 4 badge sections covering 13 commands
- PoptypeView - Completed with 2 badge sections covering 11 commands
- NametypeView - Completed with 1 badge section covering 2 commands

---

## Architecture Concerns

### 1. Mod Class Has Too Many Responsibilities

**File:** `Dom5Edit/Mod/Mod.cs`
**Size:** ~550 lines (reduced from 963 after ModParser/ModExporter split)

Still handles:
- Entity storage
- ID range management
- Logging
- Dependency resolution
- Nation association tracking

Consider further splitting for testability.

---

### 2. No Automated Tests

The project has no test project or test framework configured. This makes it risky to refactor and difficult to verify bug fixes.

**Recommendation:** Add a test project with at least:
- Round-trip parsing tests (parse → export → parse gives same result)
- Command mapping coverage tests
- Reference resolution tests

---

### 3. Spell.cs Exceeds Token Limits

**File:** `Dom5Edit/Entities/Spell.cs`
**Size:** ~45000 tokens

The Spell entity has so many property mappings that it exceeds reasonable file size. This indicates the need for the data-driven approach described in ENHANCEMENT_PLAN.md.

**Mitigation (2026-01-06):** SpellView UI now uses `spell_badges.json` for property definitions (63 commands verified). The property map in Spell.cs remains for parsing/export but UI is data-driven.

---

## Fixed Issues

### ~~Equipment Add Not Working via UI~~ FIXED (2026-01-07)

**Status:** FIXED

**Problem:** Adding weapons or armor to monsters via the SearchableReferenceComboBox dropdown wasn't working. The equipment would not appear in the list after selection.

**Root Causes:**
1. **HasValue not set:** When programmatically creating `WeaponRef` or `ArmorRef` and setting the `ID` property, `HasValue` remained `false`. The `GetLayeredReferenceList<T>()` method filters references by `refProp.HasValue`, so new refs were silently ignored.
2. **Session tracking missing:** Added equipment wasn't being recorded in `ChangesMod`, so `IsSessionEdit` was false.

**Solution Implemented:**
1. Modified `ID` setter in both `StringOrIDRef.cs` and `IDRef.cs` to automatically set `HasValue = true` when a non-zero ID is assigned
2. Added `RecordPropertyChangeInSession(Property)` helper method in `EntityViewModel` that calls `_changesMod?.RecordPropertyChange(_entity, property)`
3. Updated `AddWeaponById`, `AddArmorById`, `AddWeapon`, `AddArmor` in `MonsterViewModel` to call `RecordPropertyChangeInSession()` after adding equipment
4. Improved `RelayCommand<T>.Execute()` to use pattern matching instead of direct casting for safer WPF command execution

**Files Modified:**
- `Dom5Edit/Props/References/StringOrIDRef.cs` - ID setter now sets HasValue
- `Dom5Edit/Props/References/IDRef.cs` - ID setter now sets HasValue
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Added `RecordPropertyChangeInSession()` helper
- `Dom5Editor/UI/ViewModels/MonsterViewModel.cs` - All add methods now call session tracking
- `Dom5Editor/UI/RelayCommand.cs` - Pattern matching in Execute()

---

### ~~Undo Support Not Implemented~~ FIXED (2026-01)

**Files:** `Dom5Editor/UI/ViewModels/EntityViewModel.cs`

The new UI ViewModels now properly use CommandHistory for edits:
- `SetStringProperty()` uses `_history.Execute(new SetStringPropertyCommand(...))`
- `SetIntProperty()` uses `_history.Execute(new SetIntPropertyCommand(...))`
- `SetCommandProperty()` uses `_history.Execute(new SetCommandPropertyCommand(...))`

Ctrl+Z/Ctrl+Y now works in the MonsterView UI.

---

### ~~ChangesMod Not Initialized~~ FIXED (2026-01)

**File:** `Dom5Editor/UI/Views/MainWindowViewModel.cs`

`MainWindowViewModel.LoadEntities<>()` now calls `vm.SetChangesMod(Changes)` on all ViewModels.
Session edit tracking is functional.

---

### ~~ComboBox Scroll Adding Items~~ FIXED

**Files:** `CommandListEditor.xaml.cs`, `IntPropertyListEditor.xaml.cs`

Added `PreviewMouseWheel` handler that checks `IsDropDownOpen` and marks event as handled if dropdown is closed.

---

### ~~Magic Path Editor Not Displaying~~ FIXED

**File:** `MagicPathEditor.xaml.cs`

Removed `DataContext = this` from constructor. Changed command bindings to `ElementName=Root`.

---

### ~~Session Edit Tracking Not Working~~ FIXED (2026-01)

**Root Cause:** In `MainWindowViewModel`, `ClearHistory()` created a new `ChangesMod` AFTER `InitializeCollections()` populated ViewModels with the old reference.

**Files Fixed:**
- `Dom5Editor/UI/Views/MainWindowViewModel.cs` - Reordered operations: `ClearHistory()` now runs BEFORE `InitializeCollections()`
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - Badge value change handlers now update `IsSessionEdit` on the badge after edits
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - `CanRemove` logic now checks `EntitySource` to prevent removing properties from pure vanilla entities

---

### ~~Small Bug Fixes Batch~~ FIXED

- Incorrect HasFlag usage in IDEntity.cs:113
- Hardcoded ERA value in IntProperty.cs:41-43
- Debug code in EntitySet.cs:72-76
- Unused usings in Reference.cs
- _StringExported not static in StringOrIDRef.cs
- Property.Create in Nation.cs

---

## Enhancement Notes

### ~~Planned: Copy/Clear Command Order Validation~~ IMPLEMENTED (2026-01-14)

**Status:** IMPLEMENTED

**Problem:** Mod authors sometimes place property commands before `#copy*` or `#clear*` commands, which means those properties get overwritten/cleared. This is usually unintentional.

**Implementation:**

1. **Properties before Clear** - When a clear command is parsed, all previously defined properties in the affected group are removed:
   - Clear commands: `#clear`, `#clearweapons`, `#cleararmor`, `#clearmagic`, `#clearspec`, `#clearnation`, `#cleargods`, `#clearsites`, `#clearrec`, `#cleardef`
   - Duplicate clear commands of the same type are also removed (only the last one is kept)
   - Parse issue logged: `PropertiesClearedBySubsequentClear`

2. **Properties before Copy** - When a full copy command is parsed, all previously defined properties that will be overwritten are removed:
   - Full copy commands: `#copystats`, `#copyspr`, `#copyweapon`, `#copyarmor`, `#copyitem`, `#copyspell`, `#copysite`
   - `#copystats` overwrites: Weapons, Armor, Magic, Special, Stats (NOT sprites)
   - `#copyspr` overwrites: Sprites only
   - Other copy commands overwrite all properties
   - Parse issue logged: `PropertiesClearedBySubsequentClear`

**Files Modified:**
- `Dom5Edit/Commands/PropertyGroupMap.cs` - Added `GetGroupClearedBy()`, `IsClearCommand()`, `IsFullCopyCommand()`, `GetGroupsOverwrittenByCopy()`
- `Dom5Edit/Entities/IDEntity.cs` - Added `ApplyClearCommand()`, `ApplyCopyCommand()`, modified `AddProperty()`
- `Dom5Edit/Validation/ParseIssue.cs` - Added `PropertiesClearedBySubsequentClear` issue type
- `Dom5Edit/Mod/Mod.cs` - Added overload for `AddParseIssue()` with line number parameter

**Future Enhancement:** Flag when an entity has both a full clear AND a full copy command together (e.g., `#clear` + `#copystats`), except for valid combinations like partial monster clears with `#copystats`.

---

### ~~Performance - Cache Entity Reference Lists~~ FIXED (2026-01-06)

**Symptom:** Noticeable hitching/lag when switching between entities (e.g., selecting different monsters in the list).

**Root Cause:** Each ViewModel was independently building lists of available entities (weapons, armors, monsters, etc.) using O(n²) deduplication with `List.Any()`.

**Solution Implemented:**
- Added centralized cached lists at MainWindowViewModel level:
  - `CachedWeapons`, `CachedArmors`, `CachedMonsters`, `CachedItems`, `CachedSpells`, `CachedSites`, `CachedNations`
- `BuildEntityCaches()` runs FIRST in `InitializeCollections()`, before any ViewModels are created
- Uses O(1) HashSet-based deduplication instead of O(n²) List.Any()
- EntityViewModel exposes cached lists via protected static properties
- ViewModels now return cached lists directly instead of rebuilding

**Files Changed:**
- `Dom5Editor/UI/Views/MainWindowViewModel.cs` - Cache fields, BuildEntityCaches(), BuildEntityCache<T>()
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Protected cache accessor properties
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - Updated AvailableWeapons/Armor/Monsters/Items/Nations to use caches

---

### ~~Entity Navigation - Custom Render Panels~~ FIXED (2026-01-07)

**Added:** 2026-01-06
**Fixed:** 2026-01-07

Entity navigation has been implemented for custom render panels (inline TextBlocks showing "Copy From" references):

- **WeaponView** - `CopyWeaponDisplay` - click to navigate to source weapon
- **ArmorView** - `CopyArmorDisplay` - click to navigate to source armor
- **ItemView** - `CopyItemDisplay` - click to navigate to source item
- **SpellView** - `CopySpellDisplay` - click to navigate to source spell; `NextSpellDisplay` - click to navigate to next spell in chain

**Files Changed:**
- `WeaponViewModel.cs` - Added `CopyWeaponId` property
- `ArmorViewModel.cs` - Added `CopyArmorId` property
- `ItemViewModel.cs` - Added `CopyItemId` property
- `SpellViewModel.cs` - Added `CopySpellId` and `NextSpellId` properties
- `WeaponView.xaml.cs`, `ArmorView.xaml.cs`, `ItemView.xaml.cs`, `SpellView.xaml.cs` - Added click handlers
- `WeaponView.xaml`, `ArmorView.xaml`, `ItemView.xaml`, `SpellView.xaml` - Added `Cursor="Hand"`, `MouseLeftButtonDown`, and tooltips

**Note:** MercenaryView references (`CommanderDisplay`, `UnitDisplay`, `ItemDisplay`) are handled via badge system with `NavigateToReferenceCommand`.

---

## Questions to Investigate

1. **Event.Assign calls base.Assign then repeats work** - Lines 382-389 call `base.Assign` but then re-do SetID, ParentMod, and Selected assignments. Is this intentional?

2. **IntProperty default value of 10** - Line 69 returns 10 as default. Is this meaningful or arbitrary?

---

## Development Guidelines

### Badge Migration - Properties That Need Special Handling

**Added:** 2026-01-07

When migrating ViewModel properties to the JSON-driven badge system, certain properties require special handling and should **NOT** be moved to badges:

#### 1. Properties Using Non-Standard Property Types

The badge system assumes `IntProperty` for `type: "int"`. Properties using subclasses need special handling:

| Command | Property Class | Reason |
|---------|---------------|--------|
| `#dmg` (weapon) | `WeaponDamage` (extends StringProperty) | Can be "summonunits", "cloud", or numeric |
| `#effect` (spell) | `SpellEffect` (extends StringProperty) | Effect type codes |

**Example:** WeaponViewModel's `Damage` property was incorrectly removed during migration. The badge system couldn't handle it because:
- `WeaponDamage` extends `StringProperty`, not `IntProperty`
- Value can be `"summonunits"` (monster ID), `"cloud"` (read-only), or numeric
- Needed: `DamageLabel` (dynamic), `CanEditDamage`, `IsSummonWeapon`, `IsCloudWeapon`, `DamageDisplayString`

#### 2. Properties With Complex Getter/Setter Logic

Keep properties in ViewModel when they:
- Have validation logic in setters
- Trigger cascading property changes
- Need to check inherited values from `copyX` commands
- Transform values (e.g., decode bitmasks to display strings)

#### 3. Properties Displayed in Headers/Special Sections

Keep read-only computed properties like:
- `EraDisplay` - derived from era bitmask
- `SchoolDisplay` - school code to name
- `PathDisplay` - magic path code to name
- `DamageTypes`, `SpecialProperties` - aggregated flag displays

#### 4. Verification Checklist for Migrations

Before removing a property, verify:
1. Check the entity's `_propertyMap` - what type is the property?
2. Is it `IntProperty`, `StringProperty`, or a subclass?
3. Does the getter have special logic beyond `GetIntProperty()`?
4. Does the setter have validation or side effects?
5. Is the property bound in the View's header or special sections?
6. Compare against previous commit to catch complex logic
