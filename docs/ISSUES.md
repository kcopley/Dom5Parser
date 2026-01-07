# Known Issues

Potential bugs and critical problems identified during code review.

**Last Updated:** 2026-01-05

---

## Current Priority Issues

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

### Entity Navigation - Custom Render Panels

**Added:** 2026-01-06

Entity navigation was implemented for all badge-based reference displays. However, custom render panels (inline TextBlocks showing "Copy From" references) don't yet support click-to-navigate:

- **WeaponView** - `CopyWeaponDisplay` TextBlock (line ~41)
- **ArmorView** - `CopyArmorDisplay` TextBlock (line ~45)
- **ItemView** - `CopyItemDisplay` TextBlock (line ~45)
- **SpellView** - `CopySpellDisplay` TextBlock (line ~68), `NextSpellDisplay` (line ~208)
- **MercenaryView** - `CommanderDisplay`, `UnitDisplay`, `ItemDisplay` TextBlocks

**To implement:** Add `MouseLeftButtonDown` handlers and wire up navigation similar to MonsterView's `OnCopyStatsClick` pattern. Each view would need a code-behind handler that extracts the ID and calls `NavigateToReferenceCommand.Execute()`.

---

## Questions to Investigate

1. **Event.Assign calls base.Assign then repeats work** - Lines 382-389 call `base.Assign` but then re-do SetID, ParentMod, and Selected assignments. Is this intentional?

2. **IntProperty default value of 10** - Line 69 returns 10 as default. Is this meaningful or arbitrary?
