# Project Notes

Quick reference notes for development context. See related documents for full details:
- `ENHANCEMENT_PLAN.md` - Roadmap and feature tracking
- `ISSUES.md` - Known bugs and technical debt
- `MOD_LAYERING.md` - Architecture for property inheritance
- `BADGE_UI_REDESIGN.md` - Badge system specification

---

## Current Development Status

**Last Updated:** 2026-01-06

### Working Features
- JSON-driven badge UI system for all entity properties
- **Generalized badge infrastructure in EntityViewModel base class** (ready for other entity types)
- Undo/redo via CommandHistory (Ctrl+Z/Ctrl+Y)
- ChangesMod session tracking
- Layered property access (vanilla → mod → session) for all properties including equipment
- Equipment display with full layered fallback (vanilla → mod → copystats) with name resolution fallback
- **Centralized entity caches** for dropdown performance (weapons, armors, monsters, items, spells, sites, nations)
- **MonsterView** - Stats grid, magic paths, equipment, and badge sections (572 commands)
- **WeaponView** - Stats, damage types, special properties, secondary effects, badge panel
- **ArmorView** - Stats and badge panel
- **ItemView** - Construction stats, slot type selector, equipment display with weapon/armor stats
- **SiteView** - Core stats (path/level/rarity), gems display with icons, copy-from reference, unified property badges with monster/nation references
- **NationView** - Identity/Era, 13 badge sections for recruitment, terrain, heroes, PD, pretenders, buildings, scales, mechanics
- **MercenaryView** - Band info, unit composition, economics, equipment (13 commands)
- **PoptypeView** - Recruitment, province defense (11 commands)
- **NametypeView** - Name list management (2 commands)

### In Progress
- **CUSTOMMAGIC Editor** - Complex bitmask (not started)
- Entity navigation (clicking weapon/armor to view that entity)

### Already Working
- **MagicPathEditor** - Commander magic paths in MonsterView (may need polish)
- **PathSelector** - Icon-based path + level selector with:
  - Clickable path icons (F/A/W/E/S/D/N/G/B) with gold highlight on selection
  - Up/down arrows for level (1-10)
  - Mutual exclusion (primary/secondary can't be same path)
  - Gem cost calculation (5 × level, adjusted by itemcost percentage)
  - Inline display: "Requires 5N to forge (N1)"

### Recently Completed (2026-01-06)
- **Performance: Centralized Entity Caches** - Dropdown lists pre-built at mod load:
  - 7 cached lists: CachedWeapons, CachedArmors, CachedMonsters, CachedItems, CachedSpells, CachedSites, CachedNations
  - Built BEFORE ViewModels are created for instant availability
  - O(1) HashSet deduplication (was O(n²) with List.Any())
  - All ViewModels share cached lists instead of rebuilding per-VM
  - Eliminates lag when switching between entities

- **Reference Type Badge Support** - `BuildBadgesFromSection()` now handles `type: "ref"` badges:
  - Multi-value ref property handling via `BuildReferenceBadges()` method
  - `TryExtractRefInfo()` helper handles both `StringOrIDRef` and `MonsterOrMontagRef` types
  - `MonsterOrMontagRef.MonsterRef`/`.MontagRef` fields made public for cross-assembly access
  - Entity name resolution for display (monster ID → monster name)
  - Vanilla + mod layering with proper IsModified/IsInherited flags
  - Copystats chain traversal for inherited references
  - Reference commands always available in Add dropdown (can have multiple instances)
  - Montag references (negative IDs) display as "Montag #X"

- **NationView** - Complete nation editor with:
  - Header with era display, epithet, vanilla data warning banner
  - 13 collapsible badge sections: Identity, Fort Recruitment, Terrain Recruitment, Coastal/UW, Heroes, Starting Units, Province Defense, UW Defense, Pretenders, Buildings, Scales, Special Mechanics, Bless Bonuses, AI Hints, Admin
  - 200+ nation commands in `nation_badges.json`
  - Vanilla data incomplete warning for vanilla nations (shows banner with explanation)
  - Monster/Site references for recruitment, heroes, PD, pretenders

- **SiteView** - Complete site editor with:
  - Core fields: Name, Copy From Site, Path/Level/Rarity, Sprite (#look)
  - Gems display with path icons (PathLetterToGemIconConverter)
  - Single unified property badge panel with 90+ properties
  - Monster references for summons, recruitment, walls, defense (type: "ref", refType: "monster")
  - Nation reference support (refType: "nation")

### Previously Completed (2026-01-05)
- **PathSelector with icons** - Icon-based magic path selection with gem cost display
- **ItemView equipment display** - Shows weapon/armor damage types, special properties, secondary effects
- **WeaponView improvements** - Damage types, special properties, secondary effect display with chance %
- **Slot type selector** - Editable dropdown with clear slot names, auto-clears incompatible equipment

### Recently Completed
- **EventView** - Event requirements (general, nation, location, province, site, dominion, path, commander, target, code, enchantment) and effects (message, resource, province, scale, unit spawn, unit effects, path boost, world, event control). Uses `event_badges.json` configuration with ~160 commands across 21 badge sections.
- **SpellView** - Path requirements, effect types, combat stats, next spell chains, and badge panel for 40+ spell properties. Uses `spell_badges.json` configuration.

### Recently Completed (2026-01-06)
- **MercenaryView** - Mercenary band editor with:
  - Header with era display (EA/MA/LA bitmask)
  - Basic info section: Name, Boss Name, Level
  - Unit composition: Commander, Unit references with monster name resolution, Nr Units, Min Men
  - Economics: Min Pay, Rec Rate
  - Equipment & Experience: Item reference, XP, Rand Equip
  - 4 badge sections with JSON configuration (`mercenary_badges.json`)

- **PoptypeView** - Population type editor with:
  - Recruitment section: Clear Rec, Add Rec Unit, Add Rec Com
  - Province Defense section: Clear Def, Def Com, Def Units 1/1B/1C, Def Mult 1/1B/1C
  - 2 badge sections with JSON configuration (`poptype_badges.json`)

- **NametypeView** - Name type editor with:
  - Names section: Clear, Add Name
  - 1 badge section with JSON configuration (`nametype_badges.json`)

### Not Started
- **Other Entity Views** - Check for: Enchantment, Montag, RestrictedItem (ID-only containers, may not need views)
- Sprite preview
- Validation report panel

---

## Architecture Quick Reference

### Two ViewModel Systems (Migration In Progress)

| System | Location | Status |
|--------|----------|--------|
| **New UI VMs** | `Dom5Editor/UI/ViewModels/` | ACTIVE - Use for new work |
| **Legacy VMs** | `Dom5Editor/VMs/EntityVMs/` | DEPRECATED - Will be removed |

**Keep from Legacy:** `ModViewModel.cs`, `RelayCommand.cs`, `ViewModelBase.cs`

### Centralized Entity Caches (Performance)

Entity dropdown lists are pre-built at mod load for instant availability:

```
MainWindowViewModel.InitializeCollections()
├── BuildEntityCaches()              ← FIRST: Build all cached lists
│   ├── CachedWeapons   (IReadOnlyList<AvailableEquipmentItem>)
│   ├── CachedArmors
│   ├── CachedMonsters
│   ├── CachedItems
│   ├── CachedSpells
│   ├── CachedSites
│   └── CachedNations
└── LoadEntities<T>()                ← ViewModels created after caches ready
```

**Access from ViewModels:**
```csharp
// EntityViewModel base class exposes cached lists:
protected static IReadOnlyList<AvailableEquipmentItem> CachedWeapons => ...
protected static IReadOnlyList<AvailableEquipmentItem> CachedArmors => ...
// etc.

// ViewModels use them directly:
public IReadOnlyList<AvailableEquipmentItem> AvailableWeapons => CachedWeapons;
```

**Key points:**
- O(1) HashSet deduplication (was O(n²) with List.Any())
- Shared readonly lists across all ViewModels (no per-VM copies)
- Caches built before any ViewModel construction

### Key Files for Entity Editing

```
Dom5Editor/UI/
  Views/
    MonsterView.xaml(.cs)     - Monster editor UI (572 commands)
    WeaponView.xaml(.cs)      - Weapon editor with damage types, secondary effects
    ArmorView.xaml(.cs)       - Armor editor
    ItemView.xaml(.cs)        - Item editor with equipment display
    SiteView.xaml(.cs)        - Site editor with gems, summons, recruitment
    NationView.xaml(.cs)      - Nation editor with 13 badge sections, vanilla data warning
    MercenaryView.xaml(.cs)   - Mercenary band editor (13 commands)
    PoptypeView.xaml(.cs)     - Population type editor (11 commands)
    NametypeView.xaml(.cs)    - Name type editor (2 commands)
    MainWindow.xaml(.cs)      - App shell
    MainWindowViewModel.cs    - Mod/entity management
  ViewModels/
    EntityViewModel.cs        - Base class with badge infrastructure & property helpers
    EntityViewModels.cs       - MonsterViewModel, WeaponViewModel, ArmorViewModel, ItemViewModel, SiteViewModel, NationViewModel
  Controls/
    CompactBadge.xaml(.cs)    - Individual badge control
    BadgeWrapPanel.xaml(.cs)  - Badge container with add dropdown
    IntPropertyEditor.xaml    - Number input with modification indicators
    MagicPathEditor.xaml      - Magic path level editor (multi-path, for commanders)
    PathSelector.xaml(.cs)    - Single path + level selector (for item/spell requirements)

Dom5Editor/Data/
  monster_badges.json         - Monster property definitions (572 commands)
  weapon_badges.json          - Weapon property definitions
  armor_badges.json           - Armor property definitions
  item_badges.json            - Item property definitions
  site_badges.json            - Site property definitions (90+ commands, includes monster/nation refs)
  nation_badges.json          - Nation property definitions (200+ commands, 13 sections)
  mercenary_badges.json       - Mercenary property definitions (13 commands, 4 sections)
  poptype_badges.json         - Poptype property definitions (11 commands, 2 sections)
  nametype_badges.json        - Nametype property definitions (2 commands, 1 section)
  BadgeConfig.cs              - JSON deserialization models
  BadgeConfigLoader.cs        - Loads JSON, provides command lookup
```

### Adding a New Entity View (Step-by-Step)

1. **Create JSON config** (`Dom5Editor/Data/{entity}_badges.json`):
   - Copy structure from `monster_badges.json`
   - Define sections with commands relevant to the entity type
   - See "Quick Reference: Badge JSON Format" below for schema

2. **Update ViewModel** (`Dom5Editor/UI/ViewModels/EntityViewModels.cs`):
   - Add `protected override string EntityTypeName => "{entity}";` to the ViewModel
   - Add badge collection properties (TypeBadges, GeneralBadges, etc.)
   - Use base class helpers: `BuildBadgesFromSection()`, `CreateBadgeValueChangedHandler()`, etc.

3. **Create View** (`Dom5Editor/UI/Views/{Entity}View.xaml`):
   - Use BadgeWrapPanel controls for badge sections
   - Bind to ViewModel badge collections

4. **Register in MainWindowViewModel**:
   - Add initialization in `InitializeCollections()`

**Base class methods available for ViewModels:**
- `BuildBadgesFromSection(sectionId, valueChangedHandler)` - Builds badges from JSON with full layering
- `CreateBadgeValueChangedHandler()` - Standard handler for value edits
- `CreateRemoveBadgeCommand(refreshAction)` - Command for removing badges
- `CreateAddBadgeCommand(refreshAction)` - Command for adding badges
- `AddBadgeProperty(badge)` / `RemoveBadgeProperty(badge)` - Direct property manipulation
- `ResolveEntityReference(type, id)` - Layered entity lookup (mod → vanilla)
- `GetEntityName(type, id)` - Get entity name with layered fallback
- `GetReferenceName(reference, entityType)` - Get reference name with fallback
- `GetLayeredReferenceList<TRef>(command, entityType, getId)` - Get multi-value refs with full layering

**IMPORTANT: Property Access with VanillaModified Fallback**

When adding explicit properties to ViewModels (not using badge system), ALWAYS use the base class helper methods that include VanillaModified fallback:

```csharp
// ✓ CORRECT - Uses base class with fallback
public int? Level
{
    get => GetIntProperty(Command.LEVEL);        // Has VanillaModified fallback
    set => SetIntProperty(Command.LEVEL, value); // With undo/redo support
}

public string BossName
{
    get => GetStringProperty(Command.BOSSNAME);
    set => SetStringProperty(Command.BOSSNAME, value);
}

public bool IsSomeFlag => GetCommandProperty(Command.SOMEFLAG);

// ✓ CORRECT - For reference properties that work with GetReferenceProperty<T>
var prop = GetReferenceProperty<ItemRef>(Command.ITEM);

// ✗ WRONG - Direct entity access without fallback
var result = _entity.TryGet<IntProperty>(command, out var prop);  // Missing fallback!
```

For reference display properties that can't use `GetReferenceProperty<T>` (e.g., `MonsterOrMontagRef`), implement fallback manually:

```csharp
public string CommanderDisplay
{
    get
    {
        var result = _entity.TryGet<MonsterOrMontagRef>(Command.COM, out var prop, checkCopy: false);
        if (result == ReturnType.TRUE && prop != null)
            return GetDisplayName(prop);

        // VanillaModified fallback - REQUIRED for proper layering
        if (_source == EntitySource.VanillaModified)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var vanillaResult = vanillaEntity.TryGet<MonsterOrMontagRef>(Command.COM, out var vanillaProp);
                if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                    return GetDisplayName(vanillaProp);
            }
        }
        return null;
    }
}
```

**Available base class property helpers:**
| Method | Purpose |
|--------|---------|
| `GetIntProperty(cmd)` | Get int with VanillaModified fallback |
| `SetIntProperty(cmd, val)` | Set int with undo/redo support |
| `GetStringProperty(cmd)` | Get string with VanillaModified fallback |
| `SetStringProperty(cmd, val)` | Set string with undo/redo support |
| `GetCommandProperty(cmd)` | Get flag with VanillaModified fallback |
| `SetCommandProperty(cmd, val)` | Set flag with undo/redo support |
| `GetReferenceProperty<T>(cmd)` | Get reference with VanillaModified fallback |
| `HasReferenceProperty<T>(cmd)` | Check if reference exists with fallback |
| `IsIntPropertyModifiedFromVanilla(cmd)` | Check if int differs from vanilla |
| `IsStringPropertyModifiedFromVanilla(cmd)` | Check if string differs from vanilla |
| `IsPropertyEditedInSession(cmd)` | Check if edited in current session |
| `GetVanillaEntity()` | Get vanilla version for manual fallback |

### Property Layering (for #selectmonster)

When editing a vanilla entity modified by a mod:

1. **Session edits** (ChangesMod) - highest priority
2. **Mod properties** - sparse entity with only modified values
3. **Vanilla properties** - base game data

The `BuildBadgesFromSection()` method in MonsterViewModel implements this layering.

### Property Modification Rules

| Source | Can Edit Value | Can Remove |
|--------|---------------|------------|
| Read-only (JSON config) | No | No |
| Vanilla property | Yes | No - only override |
| Inherited (copystats) | Yes | No - only override |
| Mod property | Yes | Yes |
| Session edit | Yes | Yes |

Visual indicators:
- **Gold bar** - Modified from vanilla
- **Cyan dot** - Edited in this session (debounced 400ms)
- **"inh" badge** - Inherited from copystats

---

## Known Technical Debt

### Large Files
- `Dom5Edit/Entities/Spell.cs` - Very large, needs data-driven refactor
- `Dom5Edit/Commands/Command.cs` - 1600+ enum values, hard to maintain

### Missing Tests
No automated test suite. Priority tests needed:
- Round-trip parsing (parse -> export -> parse = same)
- Command mapping coverage
- Reference resolution

---

## Build Notes

### WSL Build Command
```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" build "D:\\Projects\\Dom5Parser\\Dom5Edit.sln"
```

### Run Editor
```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project "D:\\Projects\\Dom5Parser\\Dom5Editor\\Dom5Editor.csproj"
```

---

## Critical Notes

### DO NOT READ dom6modman.pdf DIRECTLY
The PDF file is too large and will cause issues. Always use the pre-extracted files:
- `docs/pdf_extracted/commands.json` - All commands in JSON format
- `docs/pdf_extracted/command_reference.md` - Readable command reference
- `docs/pdf_extracted/chunks/` - Text chunks from PDF pages

If new extraction is needed, update and run `tools/pdf_extractor.py`.

---

## Quick Reference: Badge JSON Format

```json
{
  "sections": [
    {
      "id": "types",
      "displayName": "TYPES",
      "renderer": "badge",
      "readOnly": true,
      "commands": [
        { "name": "undead", "display": "Undead", "type": "flag", "description": "Unit is undead" }
      ]
    }
  ]
}
```

Command types: `flag` (boolean), `int` (number value)
Renderers: `badge`, `coloredBadge`, `statsGrid`, `magicPathEditor`

---

## WPF UI Patterns & Gotchas

### ComboBox Foreground Color in Dark Themes

**Problem:** WPF's default ComboBox template displays selected items with system default colors, causing white-on-white text in dark themes. Setting `Foreground` on the ComboBox or using `ItemTemplate` with foreground bindings doesn't fix the selection box area.

**Solution:** Create a custom ComboBox ControlTemplate that explicitly sets `TextBlock.Foreground` on the ContentPresenter:

```xml
<!-- In AppTheme.xaml -->
<ControlTemplate TargetType="ComboBox">
    <Grid>
        <ToggleButton ... />
        <ContentPresenter Name="ContentSite"
                          Content="{TemplateBinding SelectionBoxItem}"
                          TextBlock.Foreground="{StaticResource TextPrimaryBrush}"/>
        <!-- ... rest of template -->
    </Grid>
</ControlTemplate>

<!-- Apply globally to all ComboBoxes -->
<Style TargetType="ComboBox" BasedOn="{StaticResource ComboBoxBase}"/>
```

**Key insight:** The `TextBlock.Foreground` attached property on ContentPresenter propagates to all TextBlocks rendered inside it, including the selection box display.

**Location:** `Dom5Editor/UI/Theme/AppTheme.xaml` - ComboBoxBase style with full control template

### Badge Icons Not Rendering

**Problem:** Icons defined in badge JSON weren't displaying because the `IconSource` property wasn't triggering UI updates.

**Solution:** Make `IconPath` a full property with setter that raises PropertyChanged for all related properties:

```csharp
public string IconPath
{
    get => _iconPath;
    set
    {
        if (_iconPath != value)
        {
            _iconPath = value;
            _iconSource = null; // Reset cached icon
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasIcon));
            OnPropertyChanged(nameof(IconSource));
        }
    }
}
```

**Location:** `Dom5Editor/UI/Controls/BadgeItem.cs`

### Badge Color Styling Guidelines

For dark theme consistency, use very subtle background tints with muted border colors:

| Element Type | Background | Border |
|--------------|------------|--------|
| Fire/warm | #322D2A | #6B4030 |
| Cold/water | #2A2D32 | #4A5A6A |
| Shock/air | #32302A | #5A5540 |
| Poison/nature | #2A322A | #3A5A3A |
| Physical (neutral) | #303033 | #505055 |
| Magic/astral | #302A32 | #504560 |

For elemental properties with available icons (magic paths, gem types), prefer icons over colored backgrounds for better clarity.

**Icon paths:** `magicicons/Path_F.png`, `magicicons/Gem_F.png`, etc. (F/A/W/E/S/D/N/B/G/H)

---

## ID Ranges (Dom6)

| Entity | Range |
|--------|-------|
| Monsters | 5000-19999 |
| Weapons | 1000-3999 |
| Armor | 400-1999 |
| Spells | 2000-7999 |
| Items | 700-1999 |
| Sites | 1700-3999 |
| Nations | 150-499 |
