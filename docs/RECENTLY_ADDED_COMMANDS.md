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

## Commands Found in Mods (Previously Not Implemented)

These commands were used in mods like DomEnhanced2_13.dm and have now been implemented:

| Command | Likely Type | Entity | Notes |
|---------|------------|--------|-------|
| #selectbless | Entity selector | Bless | **IMPLEMENTED 2026-01-10** - Dom6 blessing system |
| #path1 | IntProperty | Bless | **IMPLEMENTED 2026-01-10** - Secondary path for bless |

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
- [x] Investigate Dom6 blessing system (#selectbless, #path1) - **DONE 2026-01-10** - Full Bless entity implemented

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

## Bless Entity Commands Added (2026-01-10)

New entity type for Dom6 bless modifications. Select-only entity (no #newbless command exists).

| Command | Property Type | Notes |
|---------|--------------|-------|
| #selectbless | N/A | Entity selector command |
| #path0 | IntProperty | Primary path type (0-8) |
| #cost0 | IntProperty | Primary path cost |
| #path1 | IntProperty | Secondary path type (0-8) |
| #cost1 | IntProperty | Secondary path cost |
| #clearscales | CommandProperty | Clear all scale requirements |
| #orderscale | IntProperty | Order scale requirement |
| #prodscale | IntProperty | Productivity scale requirement |
| #heatscale | IntProperty | Heat scale requirement |
| #growthscale | IntProperty | Growth scale requirement |
| #luckscale | IntProperty | Luck scale requirement |
| #magicscale | IntProperty | Magic scale requirement |
| #chaosscale | IntProperty | Turmoil scale requirement |
| #slothscale | IntProperty | Sloth scale requirement |
| #coldscale | IntProperty | Cold scale requirement |
| #deathscale | IntProperty | Death scale requirement |
| #misfortscale | IntProperty | Misfortune scale requirement |
| #drainscale | IntProperty | Drain scale requirement |
| #clearfx | CommandProperty | Clear visual effects |

## Template Entity Commands Added (2026-01-10)

New entity type for AI pretender design templates. Keyed by nation ID.

| Command | Property Type | Notes |
|---------|--------------|-------|
| #newtemplate | N/A | Entity creation (takes nation ID) |
| #form | StringProperty | Pretender form (monster name with optional ID) |
| #prison | IntProperty | Starting state: 0=Awake, 1=Dormant, 2=Imprisoned |
| #magic | IntIntProperty | Magic path assignment: path level |
| #domstr | IntProperty | Dominion strength (1-10) |
| #scale | IntIntProperty | Scale preference: scale value |
| #bless | StringProperty | Bless effect name to select |
| #researchgoal | StringProperty | Spell/item name for AI research priority |
| #favrit | StringProperty | Favorite ritual specification |

## Date Added

2026-01-03 - During Phase 2 testing of vanilla.dm and DomEnhanced2_13.dm loading
2026-01-06 - Spell entity command verification and additions
2026-01-10 - Bless entity (19 commands) and Template entity (8 commands) for Dom6 support
