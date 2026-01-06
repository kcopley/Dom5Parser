# Nation Export Gaps Reference

This document details incomplete data in nation exports and what would be needed to support full nation exports.

## Current Export Status

### Fully Exportable Properties (in ModExport.js)

These properties ARE stored when parsing mods and CAN be exported:

| Property | Mod Command | Type | Notes |
|----------|-------------|------|-------|
| name | #name | str | |
| epithet | #epithet | str | |
| era | #era | num | |
| units | #addrecunit | array_ref | Fort recruitable units |
| commanders | #addreccom | array_ref | Fort recruitable commanders |
| landunit | #landrec | array_ref | |
| landcom | #landcom | array_ref | |
| heroes | #hero1-6 | indexed_ref | |
| multiheroes | #multihero1-2 | indexed_ref | |
| uwunit | #uwunit1-5 | indexed_ref | |
| uwcom | #uwcom1-5 | indexed_ref | |
| coastrec | #coastunit1-3 | indexed_ref | |
| coastcom | #coastcom1-2 | indexed_ref | |
| forestrec | #forestrec | array_ref | |
| forestcom | #forestcom | array_ref | |
| swamprec | #swamprec | array_ref | |
| swampcom | #swampcom | array_ref | |
| mountainrec | #mountainrec | array_ref | |
| mountaincom | #mountaincom | array_ref | |
| wasterec | #wasterec | array_ref | |
| wastecom | #wastecom | array_ref | |
| caverec | #caverec | array_ref | |
| cavecom | #cavecom | array_ref | |
| foreignunits | #addforeignunit | array_ref | |
| foreigncommanders | #addforeigncom | array_ref | |
| sites | #startsite | array_ref | Starting sites |
| homerealm | #homerealm | array_ref | |
| addgod | #addgod | array_ref | Added pretenders |
| delgod | #delgod | array_ref | Removed pretenders |
| uwnation | #uwnation | bool | |
| noforeignrec | #noforeignrec | bool | |
| disableoldnations | #disableoldnations | bool | |
| cleargods | #cleargods | bool | |
| aigoodbless | #aigoodbless | num | |
| cheapgod20 | #cheapgod20 | array_ref | Data from CSV attr 314 |
| cheapgod40 | #cheapgod40 | array_ref | Data from CSV attr 315 |

---

## NOT Exported - Ignored in parsemod.js (_ignore)

These mod commands exist but are NOT stored when parsing .dm files.
The data is never captured, so it cannot be exported.

### Starting Units/Commanders
| Command | Purpose | Location in parsemod.js |
|---------|---------|------------------------|
| #startcom | Starting commander | Line 2084 |
| #startscout | Starting scout | Line 2085 |
| #startunittype1, #startunitnbrs1 | Starting unit type 1 | Lines 2086-2087 |
| #startunittype2, #startunitnbrs2 | Starting unit type 2 | Lines 2088-2089 |

### Province Defense (PD) System
| Command | Purpose |
|---------|---------|
| #defcom1, #defcom2 | PD commanders |
| #defunit1, #defunit1b, #defunit1c, #defunit1d | PD unit types |
| #defmult1, #defmult1b, #defmult1c, #defmult1d | PD multipliers |
| #defunit2, #defunit2b | Secondary PD units |
| #defmult2, #defmult2b | Secondary PD multipliers |
| #wallcom, #wallunit, #wallmult | Wall defenders |
| #guardcom, #guardunit, #guardmult | Province guards |
| #foreignwallcom, #foreignwallunit, #foreignwallmult | Foreign province wall defenders |
| #foreignguardcom, #foreignguardunit, #foreignguardmult | Foreign province guards |
| #badindpd | Bad independent PD flag |

### Text/Display Properties
| Command | Purpose |
|---------|---------|
| #descr | Nation description |
| #summary | Nation summary |
| #brief | Brief description |
| #color | Nation color (RGB) |
| #secondarycolor | Secondary color |
| #flag | Flag image file |
| #templepic | Temple picture number |
| #mapbackground | Map background image |

### Building Costs
| Command | Purpose |
|---------|---------|
| #labcost | Laboratory cost modifier |
| #templecost | Temple cost modifier |

### Special Nation Mechanics
| Command | Purpose | Example Nations |
|---------|---------|-----------------|
| #nopreach | Disable preaching | Mictlan |
| #dyingdom | Dominion kills population | Mictlan |
| #sacrificedom | Blood sacrifice for dominion | Mictlan |
| #nodeathsupply | No supply penalty from death | Abysia |
| #domkill | Dominion kills | LA Ermor |
| #domunrest | Dominion causes unrest | |
| #autoundead | Auto-undead production | LA Ermor |
| #zombiereanim | Priests reanimate zombies | LA Ermor |
| #horsereanim | Priests reanimate horsemen | LA Ermor |
| #wightreanim | Priests reanimate wights | LA Ermor |
| #manikinreanim | Priests reanimate manikins | LA Pangaea |
| #tombwyrmreanim | Priests reanimate C'tis undead | C'tis |
| #godrebirth | God rebirth mechanics | |

### Scale/Terrain Preferences
| Command | Purpose |
|---------|---------|
| #idealcold | Ideal cold scale |
| #castleprod | Castle production bonus |
| #spreadcold, #spreadheat | Temperature spread |
| #spreadchaos, #spreadlazy, #spreaddeath | Scale spread |
| #moreorder, #moreprod, #moreheat | Bonus scales |
| #moregrowth, #moreluck, #moremagic | Bonus scales |
| #likesterr | Preferred terrain |
| #hatesterr | Disliked terrain |
| #likespop | Preferred population type |

### Fort System
| Command | Purpose |
|---------|---------|
| #fortcost | Fort cost modifier |
| #fortera | Fort era restriction |
| #uwbuild | Underwater building ability |
| #homefort | Home fort type |
| #buildfort | Buildable fort type |
| #tradecoast | Coast trade bonus |

### AI Hints
| Command | Purpose |
|---------|---------|
| #bloodnation | AI blood magic hint |
| #coastnation | AI coastal hint |
| #cavenation | AI cave hint |
| #aiholdgod | AI god strategy |
| #aiawake | AI awake strategy |
| #aimusthavemag | AI required magic |
| #aifirenation through #aibloodnation | AI path preferences |

### Misc
| Command | Purpose |
|---------|---------|
| #golemhp | Golem HP modifier |
| #merccost | Mercenary cost modifier |
| #templegems | Temple gem production |
| #killcappop | Capital population kill |
| #guardspirit | Guard spirit summon |

---

## READ-ONLY Properties (Cannot be Exported)

These properties are derived/calculated during data loading, not from mod commands.
They have no corresponding mod command and cannot be exported.

| Property | Source | Notes |
|----------|--------|-------|
| pretenders | Computed from realms, pretender_types_by_nation, addgod, delgod | Derived list |
| capunits | Computed from starting sites | Units only from capital sites |
| capcommanders | Computed from starting sites | Commanders only from capital sites |
| futurecapunits | Computed from future sites | |
| futurecapcommanders | Computed from future sites | |
| plainsrec | Populated from CSV | Plains recruits |
| plainscom | Populated from CSV | Plains commanders |
| futuresites | Derived | |
| spells | Derived | National spells |

---

## CSV Data Sources

The nations.csv is minimal:
```
id, name, epithet, abbreviation, file_name_base, era, end
```

Most nation data comes from these files:
- `attributes_by_nation.csv` - National attributes (includes cheapgod20/40 via attrs 314/315)
- `pretender_types_by_nation.csv` - Available pretenders
- `unpretender_types_by_nation.csv` - Removed pretenders
- `fort_leader_types_by_nation.csv` - Fort recruitables
- `fort_troop_types_by_nation.csv` - Fort troops
- `coast_leader_types_by_nation.csv` - Coastal recruitables
- `coast_troop_types_by_nation.csv` - Coastal troops
- `nonfort_leader_types_by_nation.csv` - Non-fort leaders
- `nonfort_troop_types_by_nation.csv` - Non-fort troops
- `realms.csv` - Home realms for pretender matching

---

## Implementation Priority

To support complete nation exports, these would need to be added:

### High Priority (Common in mods)
1. **PD System** - defcom/defunit/defmult commands
2. **Starting units** - startcom, startscout, startunittype/nbrs
3. **Text properties** - descr, summary, brief
4. **Visual properties** - color, flag

### Medium Priority (Less common)
5. **Special mechanics** - nopreach, dyingdom, sacrificedom, reanim variants
6. **Scale modifiers** - idealcold, spread*, more*
7. **Building costs** - labcost, templecost

### Low Priority (Rare/AI only)
8. **AI hints** - bloodnation, coastnation, ai* flags
9. **Fort system** - fortcost, homefort, etc.

---

## Notes

1. **cheapgod20/cheapgod40**: These ARE exportable even though parsemod.js marks them as `_ignore`. The data is loaded from CSV attributes 314/315, and the mod commands exist.

2. **Indexed arrays**: Properties like heroes, uwunit, coastrec use 1-based indexing (hero1-6, uwunit1-5, etc.) and require special handling via the `indexed_ref` formatter.

3. **pretenders**: This is a computed property combining realms, nation-specific pretenders, addgod, and delgod. It cannot be directly exported but the underlying addgod/delgod CAN be exported.
