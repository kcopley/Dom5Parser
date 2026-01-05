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
- JSON-driven badge UI system for monster properties
- Undo/redo via CommandHistory (Ctrl+Z/Ctrl+Y)
- ChangesMod session tracking
- Layered property access (vanilla -> mod -> session)
- Equipment inheritance from copystats
- Monster editor with stats grid, magic paths, equipment, and badge sections

### In Progress
- Migrating from legacy ViewModel system to new UI ViewModels
- CUSTOMMAGIC bitmask editor (not started)
- Entity navigation (clicking weapon/armor to view that entity)

### Not Started
- Other entity editors (Weapon, Armor, Spell, Item, Site, Nation views)
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

### Key Files for Monster Editing

```
Dom5Editor/UI/
  Views/
    MonsterView.xaml(.cs)     - Main monster editor UI
    MainWindow.xaml(.cs)      - App shell
    MainWindowViewModel.cs    - Mod/entity management
  ViewModels/
    EntityViewModel.cs        - Base class with property helpers
    EntityViewModels.cs       - MonsterViewModel and other entity VMs
  Controls/
    CompactBadge.xaml(.cs)    - Individual badge control
    BadgeWrapPanel.xaml(.cs)  - Badge container with add dropdown
    IntPropertyEditor.xaml    - Number input with modification indicators
    MagicPathEditor.xaml      - Magic path level editor

Dom5Editor/Data/
  monster_badges.json         - Monster property definitions (572 commands)
  BadgeConfig.cs              - JSON deserialization models
  BadgeConfigLoader.cs        - Loads JSON, provides command lookup
```

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
