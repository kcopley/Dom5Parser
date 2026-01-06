# Recently Added Commands (Dom6 Vanilla.dm Testing)

These commands were added during vanilla.dm parsing testing and should be verified against the Dom6 modding manual to confirm they are documented and usable.

## Commands Added to Command.cs Enum and CommandsMap

| Command | Property Type | Entity | Occurrences in vanilla.dm | Notes |
|---------|--------------|--------|---------------------------|-------|
| #mindless | CommandProperty | Monster | ~200 | Flag command (no args) |
| #chorusmaster | IntProperty | Monster | ~2 | Takes int argument |
| #chorusslave | IntProperty | Monster | ~1 | Takes int argument |

## Commands Added to Entity Property Maps Only (Already in Enum)

| Command | Property Type | Entity | Notes |
|---------|--------------|--------|-------|
| #stunimmunity | CommandProperty | Monster | Flag command (no args) |
| #unseen | CommandProperty | Monster | Flag command (no args) |
| #incunrest | IntProperty | Site | Takes int argument - was missing from Site._propertyMap |

## Commands Found in Mods (Not Yet Implemented)

These commands are used in mods like DomEnhanced2_13.dm but are not yet implemented:

| Command | Likely Type | Entity | Notes |
|---------|------------|--------|-------|
| #selectbless | Unknown | New entity type? | Dom6 blessing system - needs investigation |
| #path1 | IntProperty? | Bless? | Part of blessing definition |

## Bug Fixes Applied

| Fix | Description |
|-----|-------------|
| MontagIDRef null check | Added null check when AddDependent returns null for edge cases |
| EVENT_VAR in Dependents | Added EVENT_VAR to Mod.Dependents dictionary |
| Spell cycle detection | Added HashSet<Spell> tracking to IsSummon/IsEnchant/IsEventEffect to prevent infinite recursion on circular copyspell references |

## Status

- [ ] Verify #mindless against mod manual
- [ ] Verify #chorusmaster against mod manual
- [ ] Verify #chorusslave against mod manual
- [ ] Verify #stunimmunity against mod manual
- [ ] Verify #unseen against mod manual
- [ ] Verify #incunrest against mod manual
- [ ] Investigate Dom6 blessing system (#selectbless, #path1)

## Spell Entity Commands Added (2026-01-06)

Added missing spell commands to Spell.cs property map:

| Command | Property Type | Notes |
|---------|--------------|-------|
| #dispimmune | IntProperty | Enchantment dispel immunity (0-2) |
| #napbreakrit | IntProperty | NAP break ritual flag |
| #sumhealaffs | IntProperty | Afflictions healed on summon |
| #localglobal | IntProperty | Localized global enchantment (0-1) |
| #worldvisible | IntProperty | Enchantment visible worldwide (0-1) |
| #globallook | IntProperty | Global visual appearance (1-9) |
| #speedmult | IntProperty | Speed multiplier (1-3) |

Bug fix:
| Fix | Description |
|-----|-------------|
| #nextingeo type | Changed from SpellRef to BitmaskProperty (it's a terrain mask, not spell ref) |

Verification completed: 63 spell commands in spell_badges.json match Spell.cs types exactly.

## Date Added

2026-01-03 - During Phase 2 testing of vanilla.dm and DomEnhanced2_13.dm loading
2026-01-06 - Spell entity command verification and additions
