# Project Notes

Quick reference notes for development context. See related documents for full details:
- `ENHANCEMENT_PLAN.md` - Roadmap and feature tracking
- `ISSUES.md` - Known bugs and technical debt
- `MOD_LAYERING.md` - Architecture for property inheritance
- `BADGE_UI_REDESIGN.md` - Badge system specification

---

## Current Development Status

**Last Updated:** 2026-01-05

### Working Features
- JSON-driven badge UI system for all entity properties
- **Generalized badge infrastructure in EntityViewModel base class** (ready for other entity types)
- Undo/redo via CommandHistory (Ctrl+Z/Ctrl+Y)
- ChangesMod session tracking
- Layered property access (vanilla → mod → session) for all properties including equipment
- Equipment display with full layered fallback (vanilla → mod → copystats) with name resolution fallback
- **MonsterView** - Stats grid, magic paths, equipment, and badge sections (572 commands)
- **WeaponView** - Stats, damage types, special properties, secondary effects, badge panel
- **ArmorView** - Stats and badge panel
- **ItemView** - Construction stats, slot type selector, equipment display with weapon/armor stats

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

### Recently Completed (2026-01-05)
- **PathSelector with icons** - Icon-based magic path selection with gem cost display
- **ItemView equipment display** - Shows weapon/armor damage types, special properties, secondary effects
- **WeaponView improvements** - Damage types, special properties, secondary effect display with chance %
- **Slot type selector** - Editable dropdown with clear slot names, auto-clears incompatible equipment

### Not Started
- **SpellView** - Path requirements, effect types, gem/fatigue cost editor
- **SiteView** - Site abilities, summons, gem income
- **NationView** - Many reference properties, recruitment lists
- **EventView** - Event codes, conditions, effects
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

### Key Files for Entity Editing

```
Dom5Editor/UI/
  Views/
    MonsterView.xaml(.cs)     - Monster editor UI (572 commands)
    WeaponView.xaml(.cs)      - Weapon editor with damage types, secondary effects
    ArmorView.xaml(.cs)       - Armor editor
    ItemView.xaml(.cs)        - Item editor with equipment display
    MainWindow.xaml(.cs)      - App shell
    MainWindowViewModel.cs    - Mod/entity management
  ViewModels/
    EntityViewModel.cs        - Base class with badge infrastructure & property helpers
    EntityViewModels.cs       - MonsterViewModel, WeaponViewModel, ArmorViewModel, ItemViewModel
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
- `BuildBadgesFromSection(sectionId, valueChangedHandler)` - Builds badges from JSON
- `CreateBadgeValueChangedHandler()` - Standard handler for value edits
- `CreateRemoveBadgeCommand(refreshAction)` - Command for removing badges
- `CreateAddBadgeCommand(refreshAction)` - Command for adding badges
- `AddBadgeProperty(badge)` / `RemoveBadgeProperty(badge)` - Direct property manipulation
- `ResolveEntityReference(type, id)` - Layered entity lookup (mod → vanilla)
- `GetEntityName(type, id)` - Get entity name with layered fallback
- `GetReferenceName(reference, entityType)` - Get reference name with fallback
- `GetLayeredReferenceList<TRef>(command, entityType, getId)` - Get multi-value refs with full layering

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
