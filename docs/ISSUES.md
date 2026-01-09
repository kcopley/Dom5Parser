# Known Issues

Potential bugs and critical problems identified during code review.

**Last Updated:** 2026-01-08

---

## Current Priority Issues

### 0. Copy Reference Changes Don't Refresh Inherited Properties

**Status:** OPEN - Needs Investigation

**Problem:** When changing a copy reference (e.g., `#copystats` on a monster), the UI stats/badges don't update to reflect the new inherited values from the copy source. The copy selector works (value is set), but the dependent properties don't refresh.

**Expected Behavior:** When `#copystats` is changed from Monster A to Monster B, all stat badges should update to show Monster B's values (unless overridden locally).

**Investigation Areas:**

1. **Property Change Notifications** - The copy ID setters notify their own properties but may not trigger badge refreshes:
   ```csharp
   // MonsterViewModel.cs - CopyStatsId setter
   OnPropertyChanged(nameof(CopyStatsId));
   OnPropertyChanged(nameof(CopyStatsName));
   OnPropertyChanged(nameof(HasCopyStats));
   // Missing: RefreshStatsBadges() or similar?
   ```

2. **Entity.TryGet() with checkCopy** - Badge system uses `TryGet(command, out prop, checkCopy: true)` which should resolve through copy chains. The issue may be:
   - Entity's internal copy reference cache not updated when property changes
   - Badge collections built once and cached, not rebuilt when copy changes

3. **Layered Property Resolution** - `EntityViewModel.BuildBadgesFromSection()` uses `TryGet()` which calls into the entity's copy resolution. Check if:
   - `Entity.cs` caches the copy source entity
   - Copy resolution happens at parse time vs. access time

4. **Badge Refresh Trigger** - May need to call `RefreshStatsBadges()` (and similar) when copy reference changes:
   ```csharp
   // In CopyStatsId setter, after setting value:
   RefreshStatsBadges();
   RefreshCombatBadges();
   RefreshMiscBadges();
   // ... all badge sections that could inherit from copy source
   ```

**Related Files:**
- `Dom5Editor/UI/ViewModels/MonsterViewModel.cs` - Copy property setters, badge refresh methods
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - `BuildBadgesFromSection()`, `TryGet()` wrappers
- `Dom5Edit/Entities/Entity.cs` - `TryGet()` with `checkCopy` parameter
- `Dom5Edit/Entities/IDEntity.cs` - Copy chain resolution logic

**Related Documentation:**
- `docs/MOD_LAYERING.md` - Explains layered property access pattern

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
