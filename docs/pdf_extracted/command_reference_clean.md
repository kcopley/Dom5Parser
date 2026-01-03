# Dominions 6 Command Reference (Cleaned)

Auto-generated and cleaned from dom6modman.pdf

Total unique commands: 1162

## Armor Commands (13)

### `#aftercloud`
**Arguments:** `<cloudstr> <cloudtype>`

Be an #end command at the end.

### `#clear`

Clears all properties from the selected entity.

### `#copyarmor`
**Arguments:** `<"armor_name">`

Copies all stats from another armor.

### `#def`

Sets defense skill.

### `#enc`
**Arguments:** `<encumbrance>`

Sets encumbrance.

### `#end`

Ends the current entity definition block.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#newarmor`
**Arguments:** `<armor_id>`

Creates a new armor and selects it for modding. Armor number should be 400+.

### `#prot`
**Arguments:** `<protection>`

Sets natural protection.

### `#protparts`
**Arguments:** `<head prot> <body prot>`

Sets the protection value of an armor that has both head and.

### `#rcost`
**Arguments:** `<resources>`

Sets the resource cost.

### `#selectarmor`
**Arguments:** `<"armor_name">`

Selects an existing armor for modification by name or ID.

### `#type`
**Arguments:** `<id>`

Magic Item Modding Defines whether the item is 1-handed or 2- handed weapon, a shield, a helmet, a body armor, a pair of boots or something These commands allow the modding of magic items.

## Bless Commands (32)

### `#aigoodbless`
**Arguments:** `<0-100>`

### `#airblessbonus`
**Arguments:** `<0 - 9>`

Clears the list of pretender gods for this nation.

### `#astralblessbonus`
**Arguments:** `<0 - 9>`

### `#autobless`

#minsize <size> This bearer of this item will be blessed automatically if it is This item can only by a unit of this size or larger.

### `#bless`

The item automatically applies the Bless spell to the bearer, Defines what kind of armor, if any, the units gets when it uses like the Shroud of the Battle Saint.

### `#blessairshld`
**Arguments:** `<value>`

Marverni, Time of Druids Blessed troops get Air Shield like from an Air bless.

### `#blessanimawe`
**Arguments:** `<value>`

Fall victim to lethal traps or bloodthirsty monsters.

### `#blessatt`
**Arguments:** `<value>`

Command

### `#blessawe`
**Arguments:** `<value>`

Constructs a temple in the province when the site is Blessed troops get Awe.

### `#blessbers`

Scales or other conditions.

### `#blessbonus`
**Arguments:** `<0 - 9>`

Gods of this nation will get extra bless design points.

### `#blesscoldres`
**Arguments:** `<value>`

Mekone, Brazen Giants Blessed troops get increased Cold Resistance.

### `#blessdarkvis`
**Arguments:** `<value>`

#selectnation <nation nbr> Blessed troops get Darkvision.

### `#blessdef`
**Arguments:** `<value>`

For various independents and temporary monsters in the game Blessed troops get increased Defense skill.

### `#blessdtv`
**Arguments:** `<value>`

Ur, The First City Blessed troops get undying, like the undying bless effect.

### `#blessfireres`
**Arguments:** `<value>`

Nbr Nation Epithet Blessed troops get increased Fire Resistance.

### `#blessfly`

Decrease to Strength, Attack, Defense and Action Points per Will grant the unit flying when blessed.

### `#blesshp`
**Arguments:** `<value>`

A commander who enters the ruin has a chance to discover Blessed troops get increased Hit Points.

### `#blessmor`
**Arguments:** `<value>`

### `#blessmr`
**Arguments:** `<value>`

Lab is already present, there is no effect.

### `#blesspoisres`
**Arguments:** `<value>`

Fomoria, The Cursed Ones Blessed troops get increased Poison Resistance.

### `#blessprec`
**Arguments:** `<value>`

Nation numbers, Early Era Blessed troops get increased Precision skill.

### `#blessreinvig`
**Arguments:** `<value>`

Agartha, Pale Ones Blessed troops get increased Reinvigoration, like from a bless 16 Abysia, Children of Flame with the same name.

### `#blessshockres`
**Arguments:** `<value>`

Ermor, New Faith Blessed troops get increased Shock Resistance.

### `#blessstr`
**Arguments:** `<value>`

A fort is already present, there is no effect.

### `#bloodblessbonus`
**Arguments:** `<0 - 9>`

Nbr Realm Gods of this nation will get extra bless design points of this 1 North type.

### `#deathblessbonus`
**Arguments:** `<0 - 9>`

Included in the nation's default list of pretenders and need not Gods of this nation will get extra bless design points of this be separately added to the list with the #addgod command.

### `#disbless`
**Arguments:** `"bless name"`

<nbr> Disables a bless effect for this nation.

### `#earthblessbonus`
**Arguments:** `<0 - 9>`

Monster must have the #startdom and #pathcost commands Gods of this nation will get extra bless design points of this set.

### `#natureblessbonus`
**Arguments:** `<0 - 9>`

God, the homerealm of a nation cannot be cleared.

### `#selectbless`
**Arguments:** `"<bless name>"`

<nbr> 119 Misc female Selects an existing blessing that will be affected by the 120 C'tis female following modding commands.

### `#waterblessbonus`
**Arguments:** `<0 - 9>`

Must be removed with the #delgod command.

## Event Commands (8)

### `#com`
**Arguments:** `<"monster_name">`

What type of monster the commander is.

### `#domstr`
**Arguments:** `<level>`

Important Notes Set dominion strength.

### `#end`

Ends the current entity definition block.

### `#form`
**Arguments:** `"monster name"`

### `#gold`
**Arguments:** `<gold / month>`

Sets gold income/cost.

### `#prison`
**Arguments:** `<id>`

### `#rarity`
**Arguments:** `<rarity>`

Sets rarity for sites/events.

### `#scale`
**Arguments:** `<scale nbr> <value>`

Modding Number Limits Scales are numbered from 0-5 and they can have values of -5 to These are the number ranges that can be used for modding.

## Item Commands (124)

### `#acidres`
**Arguments:** `<prot>`

### `#aiassmod`
**Arguments:** `<bonus>`

Uses a user made image for item sprite.

### `#aibadlvl`
**Arguments:** `<level>`

Number from -1 to 7 from.

### `#ainocast`
**Arguments:** `<0 | 1>`

Can be anything from 1 to.

### `#aispellmod`
**Arguments:** `<bonus>`

Works like #mainlevel.

### `#armor`
**Arguments:** `<"armor_name">`

Assigns armor to the monster.

### `#autospell`
**Arguments:** `"<spell name>"`

### `#bers`

The bearer of the item will go berserk as soon as battle begins.

### `#bestowtomount`

The item can only be used by immobile beings.

### `#bluntres`

#corpseeater <value> The monster takes half damage from blunt weapons.

### `#caveres`
**Arguments:** `<bonus>`

The Sun Below 4 fire gems Bonus in percent to resources for all cave forts.

### `#champprize`

#loseeye This item is given as a reward for winning the championship of The item bearer loses an eye.

### `#chestwound`

#reqeyes The item bearer suffers a Chest Wound affliction, which cannot The item can only be used by a being with eyes.

### `#clear`

Clears all properties from the selected entity.

### `#clearallitems`

#reqplant All forgeable magic items are removed from the game.

### `#cold`

#inanimateimmune This weapon does cold damage.

### `#coldblood`

#dungeon Cold blooded like the lizards of C’tis.

### `#coldincome`
**Arguments:** `<percent>`

### `#coldpower`
**Arguments:** `<bonus>`

The true essence of beings.

### `#coldrec`

Enchantment is active.

### `#coldrecscale`
**Arguments:** `<value>`

The monster will be 75 gold cheaper to recruit when this Cold scale requirement for recruitment.

### `#coldres`
**Arguments:** `<value>`

Magic & Spells The item grants a Cold Resistance bonus.

### `#coldscale`
**Arguments:** `<value>`

#loc <locmask> The bless requires a cold scale of value or more.

### `#coldsupply`
**Arguments:** `<percent>`

### `#constlevel`
**Arguments:** `<level>`

### `#copyitem`
**Arguments:** `"<item name>"`

Copies all stats from another item.

### `#copyspr`
**Arguments:** `<item_id>`

Likable for the spell AI.

### `#crippled`

#itemdrawsize <value> The item bearer becomes Crippled.

### `#cursed`

#run The item is cursed and cannot be dropped.

### `#decayres`
**Arguments:** `<0 | 1>`

Priests of R'lyeh have a value of 50 in this attribute.

### `#decunrest`
**Arguments:** `<value>`

Adds to castle defenders.

### `#descr`
**Arguments:** `"This is a test nation, it has no units and
"`

Sets the description text for the entity.

### `#diseaseres`

#deathfire

### `#domunrest`
**Arguments:** `<value>`

The nation will only be half as affected by the death/growth The nation's Dominion causes unrest.

### `#enchantedblood`
**Arguments:** `<points>`

#noforgebonus Gets points bonus to MR and heals about.

### `#end`

Ends the current entity definition block.

### `#extralife`

The bearer of the item is resurrected once when killed in combat.

### `#favrit`
**Arguments:** `<disschool> <level>`

"ritual name" | "item name" * Nations: 0-499, 150+ for modding Makes the AI prioritize casting this ritual or forging this magic * Poptypes: 0-249, 125+ for modding item.

### `#feeblemind`

Command can be used multiple times on the same item.

### `#fire`

Flying and floating beings are immune to this weapon.

### `#fireattuned`

### `#fireblessbonus`
**Arguments:** `<0 - 9>`

These commands set the gods for a nation.

### `#fireelementals`
**Arguments:** `<bonus>`

Use this to set the type of co-riders if any.

### `#firepower`
**Arguments:** `<bonus>`

True sight makes it possible to see through illusions and The monster will get stat increases or decreases depending on glamour.

### `#firerange`
**Arguments:** `<range boost>`

Increases the selected scale by one point per turn to a All Fire rituals cast in this province have their range increased maximum of -3 /.

### `#fireres`
**Arguments:** `<value>`

Item

### `#fireshield`
**Arguments:** `<damage>`

Shuten-doji has this ability with area.

### `#fixforgebonus`
**Arguments:** `<value>`

Gem reduction when forging items.

### `#fly`

The item grants its bearer the ability to fly.

### `#forestrec`
**Arguments:** `"Moose"`

### `#forestshape`
**Arguments:** `<"monster_name">`

Taking damage.

### `#forgebonus`
**Arguments:** `<percent>`

Makes it cheaper to create magic items.

### `#fortunrest`
**Arguments:** `<value>`

Combined with #nopreach and #sacrificedom.

### `#futuresite`
**Arguments:** `"<site name>"`

Sets the nation's preference for starting in cave provinces.

### `#godpathspell`
**Arguments:** `<-1 - 7>`

Not affect undeads This is used for divine spells that should only be available when 20 Def roll negates damage the God is best with this particular magic path.

### `#guardspiritbonus`
**Arguments:** `<value>`

#nationrebate <nation nbr> | "nation name" Increases the chance of receiving a guardian spirit with +value The item is 20% cheaper to forge for this nation.

### `#heavyitem`
**Arguments:** `<0 | 1>`

### `#hp`
**Arguments:** `<value>`

Sets hit points.

### `#humanoid`

Value Item Slots The default bodytype.

### `#incunrest`
**Arguments:** `<value>`

Destroyed each turn.

### `#itemcost2`
**Arguments:** `<bonus>`

The item bearer suffers a Never Healing Wound, which cannot This command makes the item bonus percent more expensive be healed until the item is removed.

### `#itemdrawsize`
**Arguments:** `<value>`

The item bearer becomes Crippled.

### `#loseeye`

This item is given as a reward for winning the championship of The item bearer loses an eye.

### `#luck`

Magic item will remove its description as well, so make sure to The item grants Luck to its bearer, like Faithful.

### `#mainlevel`
**Arguments:** `<path>`

Main path level requirement to forge this magic item.

### `#mainpath`
**Arguments:** `<path>`

Sets main magic path for item.

### `#mapspeed`
**Arguments:** `<value>`

An artifact without being construction level.

### `#morale`
**Arguments:** `<value>`

### `#mountedhumanoid`

Hands Regarding hit locations this is the same as humanoid, but it 14 3 hands removes the boot item slot and is provided for convenience for 30 4 hands centaurs and similar.

### `#mute`

Miscellaneous The item bearer becomes Mute.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#nationrebate`
**Arguments:** `<nation_id>`

"nation name" Increases the chance of receiving a guardian spirit with +value The item is 20% cheaper to forge for this nation.

### `#newitem`

Creates a new magic item and selects it for modding.

### `#nextspell`
**Arguments:** `"<spell name>"`

<nbr> 10044 Transform 1 With this command the effect of another spell will also take 10045 Force-transform 1 place when the effect of the main spell occurs.

### `#nhwound`

#itemcost2 <bonus> The item bearer suffers a Never Healing Wound, which cannot This command makes the item bonus percent more expensive be healed until the item is removed.

### `#noaging`
**Arguments:** `<percent>`

The wielder of this item has a chance of not aging each year.

### `#nodemon`

Restrictions The item cannot be used by demons.

### `#nofemale`

#notfornation <nation nbr> | "nation name" The item cannot be used by females.

### `#nofind`

The item cannot be used by immobile beings.

### `#noforgebonus`

Gets points bonus to MR and heals about.

### `#noimmobile`

#nofind The item cannot be used by immobile beings.

### `#noinanim`

Restricts the item to the last manipulated nation, so this The item cannot be used by inanimate beings.

### `#nomounted`

(double combat speed).

### `#noundead`

The item is restricted to this nation only.

### `#onebattlespell`
**Arguments:** `"<spell name>"`

<nbr> The monster is automatically a communion master and does Monster will automatically cast this spell just before the battle not need to cast the Communion Master spell to join a starts.

### `#onlycoldblood`

The itemcost1 command only affects the cost of the first magic The item can only be used by coldblooded beings.

### `#onlydemon`

The itemcost2 command only affects the cost of the second The item can only be used by demons.

### `#onlyimmobile`

#bestowtomount The item can only be used by immobile beings.

### `#onlyinanim`

The bearer of this item will automatically compete in the Arena The item can only be used by inanimate beings.

### `#onlymounted`

This command makes the item bonus percent more expensive The item can only be used by mounted beings.

### `#pen`
**Arguments:** `<value>`

### `#pierceres`

The monster takes half damage from piercing weapons.

### `#poison`

The effects of the weapon can be resisted by MR.

### `#poisonarmor`
**Arguments:** `<dmg>`

Anyone striking this monster with short weapons will be Immortality poisoned.

### `#poisoncloud`

#poisonskin DAMAGE REDUCTION #poisonarmor.

### `#poisonres`
**Arguments:** `<value>`

Makes the item cast this spell automatically in battle.

### `#poisonskin`
**Arguments:** `<0-500>`

This section covers different the different kinds of The monster has a skin that exudes a paralyzing poison, which immortality available for monsters.

### `#randomspell`
**Arguments:** `<percent>`

### `#recuperation`

The item grants the recuperation ability.

### `#reqeyes`

The item bearer suffers a Chest Wound affliction, which cannot The item can only be used by a being with eyes.

### `#reqnospellsinger`

Removes all abilities and stats from the magic item.

### `#reqnotaskmaster`

Level of construction required to forge this item.

### `#reqplant`

All forgeable magic items are removed from the game.

### `#reqseduce`

Always use this command at the end of modifying a magic item.

### `#reqspellsinger`

Command

### `#resources`
**Arguments:** `<value>`

The monster will become more powerful every time it dies, up The monster generates <value> resources in the province it is to at most limit number of deaths.

### `#restricted`
**Arguments:** `<nation_id>`

"nation name".

### `#restricteditem`
**Arguments:** `<value>`

Be healed until the item is removed.

### `#run`

The item is cursed and cannot be dropped.

### `#secondarylevel`
**Arguments:** `<path>`

The required magic path.

### `#secondarypath`
**Arguments:** `<path>`

Never be cast.

### `#selectitem`
**Arguments:** `"<item name>"`

Selects an existing magic item for modification by name or ID.

### `#shock`

#friendlyimmune This weapon does shock damage.

### `#shockres`
**Arguments:** `<value>`

Enables user of item to cast this spell in battle or as a ritual The item grants a Shock Resistance bonus.

### `#sizeresist`

The weapon does slashing damage.

### `#slashres`

The monster takes half damage from slashing weapons.

### `#sleepres`

MAGIC ABILITIES NON-COMBAT ABILITIES #douse.

### `#spell`
**Arguments:** `"<spell name>"`

### `#spellreqfly`
**Arguments:** `<0 | 1>`

Roll negates 1 means only flying units can cast this ritual.

### `#spellsinger`

#voidret <chance> Unit takes 50% longer to cast spells, but at half fatigue cost.

### `#spr`
**Arguments:** `<"file_path">`

### `#swift`
**Arguments:** `<percent>`

#disease Grants extra combat speed to the wielder.

### `#waterbreathing`

A heavy item cannot be transported with the help of certain The item grants water breathing to its bearer.

### `#weapon`
**Arguments:** `<"weapon_name">`

Assigns a weapon to the monster.

## Mercenary Commands (11)

### `#cleardef`

The minimum amount of gold the band can be hired for.

### `#clearmercs`

Normal, then press ctrl+shift+s to save a file containing the Removes all mercenary bands.

### `#end`

Ends the current entity definition block.

### `#item`
**Arguments:** `"<item name>"`

### `#minmen`
**Arguments:** `<value>`

End modding each poptype with this command.

### `#minpay`
**Arguments:** `<gold>`

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#newmerc`

Creates a new mercenary unit.

### `#newtemplate`
**Arguments:** `<nation_id>`

### `#randequip`
**Arguments:** `<0-3>`

Adds a commander to the recruitment list.

### `#recrate`
**Arguments:** `<value>`

Is 20 or higher.

## Mod Commands (5)

### `#description`
**Arguments:** `"<piece of text>"`

Sets the mod description text.

### `#domversion`

Sets minimum required Dominions version.

### `#icon`
**Arguments:** `"mod_subdirectory/mod_icon.tga"`

Sets the mod icon image (128x32 or 256x64 TGA).

### `#modname`
**Arguments:** `"Minimal Mod"`

Sets the mod name displayed in preferences.

### `#version`

## Monster Commands (560)

### `#aciddigest`
**Arguments:** `<dmg>`

This monster gets reduced attack penalty when using two The monster digests any swallowed creatures and causes their weapons.

### `#acidres`
**Arguments:** `<prot>`

### `#acidshield`
**Arguments:** `<damage>`

The season of power the monster gets a percentage increase Anyone striking this monster will take <damage> points of to its maximum hit points.

### `#adeptsacr`
**Arguments:** `<value>`

Upkeep will be calculated as if the unit cost this much more to Adept Sacrificer.

### `#aicheapholy`

#moremagic <-5 - 5> Informs the AI that this nation has cheap expendable sacred Alters the minimum/maximum allowed value of this scale when units.

### `#aifirenation`

These commands are used to assign a nation's units, Hint to AI that Fire magic is used a lot in this nation and that a commanders and heroes.

### `#aiholyranged`

AI Hints Informs the AI that this nation has sacred ranged units.

### `#ainorec`

The cost in resources.

### `#airattuned`

Etc)

### `#airshield`
**Arguments:** `<percent>`

Combat Auras The monster gains the airshield special ability.

### `#aisinglerec`

Gold & Resource Cost Will tell the AI to only recruit a single one of these per batch.

### `#alchemy`

#fastcast

### `#allret`
**Arguments:** `<chance>`

This monster gets a blood magic bonus when searching for Extra chance of returning from any of the other planes a blood slaves.

### `#almostliving`

Beings

### `#almostundead`

(unless it it is a mage).

### `#ambidextrous`

#xpgain #clumsy.

### `#amphibian`

This monster is an undead.

### `#animal`

Indicates that the monster is an animal.

### `#animalawe`
**Arguments:** `<bonus>`

Immortal to reappear the next month.

### `#animated`
**Arguments:** `<"monster_name">`

The monster will change into the next monster type (next The monster becomes this if affected by the animate tree spell.

### `#ap`
**Arguments:** `<action points>`

Sets action points (movement in combat).

### `#appetite`
**Arguments:** `<value>`

The monster erupts in a shock explosion when it dies, inflicting A monster with this ability will eat supplies as if he was this capped 1 AN shock damage to everyone in the area of effect.

### `#aquatic`

Retained experience, curses, afflictions, magic etc.

### `#armor`
**Arguments:** `<"armor_name">`

Assigns armor to the monster.

### `#assassin`

This ability is used by certain horrors to make them move on This monster is an assassin.

### `#astralrange`
**Arguments:** `<range>`

Chance should be a number from 1 to.

### `#att`
**Arguments:** `<attack>`

Sets attack skill.

### `#autoberserk`
**Arguments:** `<value>`

= will start the combat by going berserk.

### `#autocorpsehealer`
**Arguments:** `<value>`

Death magic, inanimate creatures by Earth magic, demons by Gives the Corpse Stitcher ability.

### `#autodisgrinder`
**Arguments:** `<value>`

Getting afflictions and eventually die.

### `#autodishealer`
**Arguments:** `<value>`

Start age to zero.

### `#autohealer`
**Arguments:** `<value>`

The monster automatically heals <value> afflictions from units.

### `#autumnshape`
**Arguments:** `<"monster_name">`

Province to a water province.

### `#awe`
**Arguments:** `<bonus>`

#slimer <strength> Bonus can be a value of one or more.

### `#banefireshield`

#acidshield HEALING & DISEASE #damagerev.

### `#batstartsum1d3`
**Arguments:** `<"monster_name">`

Summoned

### `#battleshape`
**Arguments:** `<"monster_name">`

### `#battlesum1`

Summons 0-1 monsters etc.

### `#battlesum1d2`
**Arguments:** `<"monster_name">`

Automatically summons 1-2 monsters to the battlefield each Entries marked with an asterisk may be made worse if the combat round.

### `#battlesum1d3`
**Arguments:** `<"monster_name">`

Cataclysm has occurred.

### `#battlesum5`

Command summons 0-5 and.

### `#battlesumwarm`
**Arguments:** `<"monster_name">`

This command assigns a monster tag value to a creature.

### `#beartattoo`
**Arguments:** `<value>`

Supplies

### `#beastmaster`
**Arguments:** `<bonus>`

This monster has an innate ability to command 150 magic All animals under the command of this monster have their beings.

### `#beckon`
**Arguments:** `<value>`

#addrandomage <years> Gives the monster the ability to lure enemy commanders like Makes the monster start 1 to years (a random die) older.

### `#berserk`
**Arguments:** `<bonus>`

Until dead or until the monster that swallowed them is killed.

### `#bird`

Hands, bow, head (crown), body, feet, 2 misc Use this for birds, rocs, couatl and similar things.

### `#blind`

The pass connects.

### `#blink`

#statbreak <value> Like #teleport, but only in combat.

### `#bloodrange`
**Arguments:** `<range>`

The type ranges from 0 (fire gems) to 8 (blood slaves).

### `#bloodvengeance`
**Arguments:** `<strength>`

The height of each season and will get increased Strength, The monster has the Blood Vengeance ability.

### `#bluntres`

#corpseeater <value> The monster takes half damage from blunt weapons.

### `#boartattoo`
**Arguments:** `<value>`

### `#bodyguard`
**Arguments:** `<bonus>`

This monster has an innate ability to command 50 undead The monster's morale is increased by <bonus> when its orders beings.

### `#bonusspells`
**Arguments:** `<spells per round>`

#mastersmith <value> The monster gets the Innate Spellcaster ability and can cast a The monster's magic paths are counted as <value> higher than number of spells every combat round in addition to its.

### `#bravemount`
**Arguments:** `<percent>`

Commands

### `#bringeroffortune`

#decscale #combatcaster.

### `#bug`

#pooramphibian Monsters with this tag are summoned by the Swarm spell on This monster can travel under water, but is hindered by it.

### `#bugreform`
**Arguments:** `<nbr of bugs>`

Works like regeneration, but only affects inanimate beings.

### `#bugshape`
**Arguments:** `<"monster_name">`

### `#bugswarmshape`
**Arguments:** `<"monster_name">`

The monster takes half damage from slashing weapons.

### `#bugswarmuwshape`
**Arguments:** `<"monster_name">`

Lower fatigue reduces the chance of the monster suffering This is the shape of the random bugs if they are underwater.

### `#buguwshape`
**Arguments:** `<"monster_name">`

The monster takes half damage from piercing weapons.

### `#carcasscollector`
**Arguments:** `<value>`

#tainted <chance> The monster can turn <value> nature gems to <value> death Chance in percent of being horror marked each turn.

### `#castledef`

#researchbonus.

### `#casttime`
**Arguments:** `<1-1000>`

Immunity negates Set the casting time of a spell, the casting time is 100 for a 14 Always hits someone in square standard spell.

### `#caverecpt`
**Arguments:** `<bonus>`

Palace of Dreams 4 air gems Bonus in percent to recruitment points (for units) for all cave Weeping Stone 1 water gem forts.

### `#caveres`
**Arguments:** `<bonus>`

The Sun Below 4 fire gems Bonus in percent to resources for all cave forts.

### `#chaospower`
**Arguments:** `<bonus>`

Under darkness.

### `#chaosrec`

Of not aging each year.

### `#chaosrecscale`
**Arguments:** `<value>`

The monster will be 50 percent cheaper to recruit when this Chaos scale requirement for recruitment.

### `#chorusmaster`

#crossbreeder <value> The monster is automatically a chorus master and does not The monster is skilled in crossbreeding and gets a bonus when need to cast the Communion Master spell to join a chorus.

### `#chorusslave`

From the Blood magic school.

### `#cleanshape`

Home province.

### `#clear`

Clears all properties from the selected entity.

### `#cleararmor`

Monster number is not used, Dominions will automatically use Removes all armor from the active monster.

### `#clearmagic`

Coded numbers later in the mod may overwrite the number Removes all magic skills from the active monster.

### `#clearrec`

#startunittype1 "<monster name>" Clears the list of recruitable units and commanders (but not The commander will have units of this type.

### `#clearspec`

Removes all special abilities from the active monster.

### `#clearweapons`

Command to remove the existing weapons Enkidu 16 first.

### `#clumsy`

#berserk SEASONAL POWERS #blessbers.

### `#cold`

#inanimateimmune This weapon does cold damage.

### `#coldblood`

#dungeon Cold blooded like the lizards of C’tis.

### `#coldincome`
**Arguments:** `<percent>`

### `#coldpower`
**Arguments:** `<bonus>`

The true essence of beings.

### `#coldrec`

Enchantment is active.

### `#coldrecscale`
**Arguments:** `<value>`

The monster will be 75 gold cheaper to recruit when this Cold scale requirement for recruitment.

### `#coldres`
**Arguments:** `<value>`

Magic & Spells The item grants a Cold Resistance bonus.

### `#coldscale`
**Arguments:** `<value>`

#loc <locmask> The bless requires a cold scale of value or more.

### `#coldsupply`
**Arguments:** `<percent>`

### `#copyspr`
**Arguments:** `<item_id>`

Likable for the spell AI.

### `#copystats`
**Arguments:** `<monster_id>`

Copies all stats from another monster.

### `#coridermnr`
**Arguments:** `<"monster_name">`

### `#corpseeater`
**Arguments:** `<value>`

The monster takes half damage from blunt weapons.

### `#corpselord`
**Arguments:** `<id>`

Creates a sprite that is used when the rider loses his mount.

### `#corruptor`
**Arguments:** `<value>`

Hinder movement.

### `#crossbreeder`
**Arguments:** `<value>`

The monster is automatically a chorus master and does not The monster is skilled in crossbreeding and gets a bonus when need to cast the Communion Master spell to join a chorus.

### `#curse`
**Arguments:** `<chance>`

Every turn any unit in the province has the indicated chance #claim (percent) to become cursed.

### `#curseluckshield`
**Arguments:** `<penetration bonus>`

The monster has increased hit points in spring and lowered hit Grants the Fateweaving ability.

### `#custommagic`

Nbr Magic Path mask causes the mod to crash.

### `#damagemon`
**Arguments:** `<"monster_name">`

Mon nbr Use this to set a spell's damage to a monster if you don't know 10038 Indep.

### `#damagerev`
**Arguments:** `<strength>`

The monster has increased hit points in autumn and lowered The monster has the Damage Reversal ability.

### `#dancenof`
**Arguments:** `<nbr of sprites>`

The number of sprites circling the unit.

### `#dancenratt`
**Arguments:** `<attacks>`

#norange The number of attacks made per combat round.

### `#dancesize`
**Arguments:** `<size>`

#seduce The size in percent of normal size, default is.

### `#dancespr`
**Arguments:** `<flysprite nbr>`

#statstorm The look of the sprite circling the unit.

### `#darkpower`

#invisible

### `#darkvision`
**Arguments:** `<percent>`

Gives monster darkvision, lessening penalties for fighting.

### `#deadhp`
**Arguments:** `<value>`

Chance of harming the monster when they hit, but when they Number of HP gained per corpse eaten.

### `#deathbanish`
**Arguments:** `<-11 to -13>`

#grandcom <0 or 1> Whoever strikes the killing blow against this monster is This monster can participate in grand communions.

### `#deathcurse`

#patrolbonus <value> When this monster dies, the unit that strikes the killing blow is A value of ten will make this monster count as ten extra cursed.

### `#deathdisease`
**Arguments:** `<aoe>`

Pillage bonus of one which makes them count as one man extra The monster bursts in a cloud of disease ridden fumes when it when it comes to pillaging.

### `#deathfire`
**Arguments:** `<aoe>`

A monster with this ability produces extra supplies.

### `#deathgrab`
**Arguments:** `<aoe>`

If it was an enemy priest engaged in preaching.

### `#deathparalyze`
**Arguments:** `<aoe>`

This monster has a bonus when preaching against enemy The monster erupts in a paralyzing explosion on death, forcing Dominion.

### `#deathpoison`
**Arguments:** `<aoe>`

Negative for extra poor performance.

### `#deathpower`

#twistfate

### `#deathrec`
**Arguments:** `<value>`

This mechanic.

### `#deathrecscale`
**Arguments:** `<value>`

### `#deathshock`
**Arguments:** `<aoe>`

### `#deathslime`
**Arguments:** `<aoe>`

Icon

### `#decayres`
**Arguments:** `<0 | 1>`

Priests of R'lyeh have a value of 50 in this attribute.

### `#decscale`

For The monster will reduce the chance of negative events in his the opposite scales.

### `#decunrest`
**Arguments:** `<value>`

Adds to castle defenders.

### `#def`

Sets defense skill.

### `#defector`
**Arguments:** `<percent>`

Gets the turmoil discount back into the treasury when the The monster has a chance to become independent if owned by monster joins the army.

### `#defmult`
**Arguments:** `<multiplier>`

Adds a monster that can be recruited as commander by the Number of units per 10 points of defense.

### `#defmult1`

Will yield 2 units per point of defense, which is also the default.

### `#defmult1b`
**Arguments:** `<multiplier>`

Mask Era Number of units per 10 points of defense for the second unit 1 Early era type.

### `#defmult1c`
**Arguments:** `<multiplier>`

Will replace the non-foreign variant for forts that is not the Number of units per 10 points of defense for the third unit capital.

### `#defmult1d`
**Arguments:** `<multiplier>`

#foreignguardmult <multiplier> Number of units per 10 points of defense for the fourth unit Will replace the non-foreign variant for forts that is not the type.

### `#demon`

The monster floats in the air and can cross rivers.

### `#descr`
**Arguments:** `"This is a test nation, it has no units and
"`

Sets the description text for the entity.

### `#deserter`

, but the desertion chance is increased during less gold to recruit.

### `#digest`
**Arguments:** `<dmg>`

The monster digests any swallowed creatures.

### `#diseasecloud`

#corruptor #poisoncloud.

### `#diseaseres`

#deathfire

### `#divinebeing`

Monsters with this tag are used as doom horrors.

### `#divineins`

The monster can use his own blood equal to this number of There can only be a number of divinely inspired researchers blood slaves for combat spell casting.

### `#djinn`

Monster Modding, Special Abilities Use this for djinns with a humanoid body, but no lower part.

### `#doheal`

Resistances because they are intended to be immune to any Unit heals normally despite not usually being able to heal.

### `#domimmortal`

Monster is surrounded by a poison cloud.

### `#dompower`

#fearofflood.

### `#domrec`
**Arguments:** `<dominion>`

The monster will be 20 gold cheaper to recruit when this This monster can only be recruited if the dominion value of the enchantment is active.

### `#domsail`

#tradecoast <income boost in percent> The nation's Dominion enables all units to sail like the dark Income bonus for coastal forts.

### `#domshape`
**Arguments:** `<"monster_name">`

#forcess The monster changes to this shape when inside a friendly Usually being soul slayed will prevent any shape change or dominion.

### `#domsummon2`
**Arguments:** `<"monster_name">`

Soulless (requires corpses) Half as effective as #domsummon.

### `#domsummon20`
**Arguments:** `<"monster_name">`

Random animal A twentieth as effective as #domsummon.

### `#domunrest`
**Arguments:** `<value>`

The nation will only be half as affected by the death/growth The nation's Dominion causes unrest.

### `#doomhorror`

#divinebeing Monsters with this tag are used as doom horrors.

### `#douse`
**Arguments:** `<bonus>`

#allret <chance> This monster gets a blood magic bonus when searching for Extra chance of returning from any of the other planes a blood slaves.

### `#dragonlord`
**Arguments:** `<id>`

Here are the commands that are used to pair riders and This monster will receive extra units when summoning Drakes mounts.

### `#drainimmune`

#falsesupply #magicimmune.

### `#drake`

#lanceok The monster is a drake and is affected by the Dragon Master This monster can use lances even though it is not flying or ability.

### `#drawsize`
**Arguments:** `<value>`

### `#dread`
**Arguments:** `<value>`

#sleepaura <area> Like fear, but negated by true sight.

### `#dungeon`

Cold blooded like the lizards of C’tis.

### `#earthelementals`
**Arguments:** `<bonus>`

Gives a size bonus to summoned earth elementals.

### `#elegist`
**Arguments:** `<value>`

Magic tattoo like the units of Marverni and Sauromatia have.

### `#elementgems`
**Arguments:** `<gems>`

Holy magic cast by the monster have increased range.

### `#elementrange`
**Arguments:** `<range>`

They are of a single random type of elemental gem.

### `#enc`
**Arguments:** `<encumbrance>`

Sets encumbrance.

### `#enchrebate10`
**Arguments:** `<enchantment number>`

Magic penalty for trinity gods when they are not in the same The monster will be 10 gold cheaper to recruit when this province.

### `#enchrebate100`
**Arguments:** `<enchantment number>`

#growthrecscale <value> The monster will be 100 gold cheaper to recruit when this Growth scale requirement for recruitment.

### `#enchrebate20`
**Arguments:** `<enchantment number>`

#domrec <dominion> The monster will be 20 gold cheaper to recruit when this This monster can only be recruited if the dominion value of the enchantment is active.

### `#enchrebate25p`
**Arguments:** `<enchantment number>`

Death scale requirement for recruitment.

### `#enchrebate50`
**Arguments:** `<enchantment number>`

#heatrecscale <value> The monster will be 50 gold cheaper to recruit when this Heat scale requirement for recruitment.

### `#enchrebate50p`
**Arguments:** `<enchantment number>`

#chaosrecscale <value> The monster will be 50 percent cheaper to recruit when this Chaos scale requirement for recruitment.

### `#enchrebate75`
**Arguments:** `<enchantment number>`

#coldrecscale <value> The monster will be 75 gold cheaper to recruit when this Cold scale requirement for recruitment.

### `#end`

Ends the current entity definition block.

### `#entangle`

Works like Awe, except that it doesn't work if there is no sun Anyone striking this monster may get entangled.

### `#eramask`
**Arguments:** `<value>`

Sets the third type of unit in the poptype PD.

### `#ethereal`

Possible and gain HP for it equal to the value in #deadhp.

### `#evil`

Use number 150 and above in order to create new nations The throne is evil and is likely to be defended by evil monsters.

### `#expertleader`

Ground

### `#expertmagicleader`

#beastmaster <bonus> This monster has an innate ability to command 150 magic All animals under the command of this monster have their beings.

### `#expertundeadleader`

#standard <bonus> This monster has an innate ability to command 150 undead The monster increases the morale of units in the same squad by beings.

### `#extralives`
**Arguments:** `<percent>`

Province

### `#eyeloss`

#haltheretic <bonus> Anyone striking this monster may lose an eye.

### `#eyes`
**Arguments:** `<nbr of eyes>`

Giving a monster 50 in morale makes it mindless and prone to Sets the number of eyes for a monster.

### `#fallpower`

#trample

### `#falsearmy`
**Arguments:** `<value>`

Of its total hit points every turn it spends underwater (i.

### `#falseregen`
**Arguments:** `<points>`

The following 4 commands.

### `#falsesupply`
**Arguments:** `<value>`

AP fire damage to everyone in the area of effect.

### `#farsail`

#dancenratt <attacks> #norange The number of attacks made per combat round.

### `#farthronekill`
**Arguments:** `<part>`

Increased by <value> per turn, to a maximum of.

### `#fastcast`

#mason #magicstudy.

### `#faysummon`
**Arguments:** `<id>`

But e

### `#fear`
**Arguments:** `<value>`

Anyone striking this monster may be horror marked.

### `#fearofflood`
**Arguments:** `<value>`

Counts as <value> extra monsters when defending a fort from a Morale penalty when it is raining.

### `#female`

Command can be used multiple times on the same monster for Being female is a minor advantage that makes you immune to more than 1 starting weapon.

### `#fire`

Flying and floating beings are immune to this weapon.

### `#fireattuned`

### `#fireblessbonus`
**Arguments:** `<0 - 9>`

These commands set the gods for a nation.

### `#fireelementals`
**Arguments:** `<bonus>`

Use this to set the type of co-riders if any.

### `#firepower`
**Arguments:** `<bonus>`

True sight makes it possible to see through illusions and The monster will get stat increases or decreases depending on glamour.

### `#firerange`
**Arguments:** `<range boost>`

Increases the selected scale by one point per turn to a All Fire rituals cast in this province have their range increased maximum of -3 /.

### `#fireres`
**Arguments:** `<value>`

Item

### `#fireshield`
**Arguments:** `<damage>`

Shuten-doji has this ability with area.

### `#firstshape`
**Arguments:** `<"monster_name">`

The monster is more efficient when casting spells the converts This monster will change shape to its first shape when it feels gems into gold.

### `#fixedname`
**Arguments:** `"<Name>"`

Selected monster, so it should be used as the first command Gives a fixed name to a monster if it is a commander.

### `#fixedresearch`
**Arguments:** `<value>`

The monster produces a number of water gems that can be The monster produces this amount of research even without used in combat.

### `#float`

#demon The monster floats in the air and can cross rivers.

### `#flying`

#autocompete This monster can fly.

### `#foolscouts`
**Arguments:** `<value>`

Grants Pangaea-like healing powers to the monster The monster creates the false impression of uniformity in its (Recuperation special ability).

### `#forcess`

The monster changes to this shape when inside a friendly Usually being soul slayed will prevent any shape change or dominion.

### `#foreignguardmult`
**Arguments:** `<multiplier>`

Number of units per 10 points of defense for the fourth unit Will replace the non-foreign variant for forts that is not the type.

### `#foreignshape`
**Arguments:** `<"monster_name">`

#airattuned etc).

### `#foreignwallmult`
**Arguments:** `<multiplier>`

Second type of bonus units for local defense equal to or greater Will replace the non-foreign variant for forts that is not the than 20 in provinces with forts.

### `#forestrec`
**Arguments:** `"Moose"`

### `#forestshape`
**Arguments:** `<"monster_name">`

Taking damage.

### `#forestsurvival`

#stealthy in order to be meaningful.

### `#formationfighter`

#allrange

### `#fortkill`
**Arguments:** `<chance>`

Commander) does not obey orders but does something else This monster will automatically destroy forts in the same instead.

### `#fortunrest`
**Arguments:** `<value>`

Combined with #nopreach and #sacrificedom.

### `#futuresite`
**Arguments:** `"<site name>"`

Sets the nation's preference for starting in cave provinces.

### `#gcost`
**Arguments:** `<gold>`

Sets the gold cost for recruitment.

### `#gemprod`
**Arguments:** `<type> <value>`

Glamour magic cast by the monster have increased range.

### `#giftofwater`
**Arguments:** `<size points>`

Detecting a standard stealthy unit, while 50 patrolling units are A commander with this ability is able to bring a number of units required to have an equal chance of detecting a sneaking scout.

### `#glamour`

Aura is <value> + Fire magic squares in size.

### `#glamourmanip`
**Arguments:** `<0 | 1>`

This monster is a glamour manipulator and counts as a glamour mage when it comes to upholding illusions and false damage.

### `#glamourrange`

#inspirational #bloodrange.

### `#golemhp`
**Arguments:** `<percent>`

Recruiting sacred units).

### `#goodleader`

Voluntarily in order to fight on the ground.

### `#goodmagicleader`

All units under the command of this monster have their morale This monster has an innate ability to command 100 magic increased by <bonus> in addition to modifiers from base beings.

### `#greaterhorror`

This monster is an illusion.

### `#growhp`
**Arguments:** `<hit points>`

The monster grows into the previous monster once it has this The monster will get this shape if it is transformed into a lich.

### `#growthpower`
**Arguments:** `<bonus>`

A unit with invisibility is sneaking, only patrolling units with The monster will get stat increases or decreases depending on spirit sight will be able to find it.

### `#growthrecscale`
**Arguments:** `<value>`

The monster will be 100 gold cheaper to recruit when this Growth scale requirement for recruitment.

### `#guardmult`
**Arguments:** `<multiplier>`

Third type of standard unit for local defense.

### `#haltheretic`
**Arguments:** `<bonus>`

Anyone striking this monster may lose an eye.

### `#heal`

#foolscouts <value> Grants Pangaea-like healing powers to the monster The monster creates the false impression of uniformity in its (Recuperation special ability).

### `#healer`

#bloodvengeance.

### `#heat`
**Arguments:** `<value>`

At the start of each new month.

### `#heatrec`

Enchantment is active.

### `#heatrecscale`
**Arguments:** `<value>`

The monster will be 50 gold cheaper to recruit when this Heat scale requirement for recruitment.

### `#heretic`

#chorusmaster.

### `#holy`

Cannot become prophet, cannot be charmed and has no upkeep Holy (sacred) troops can be blessed by priests.

### `#holycost`
**Arguments:** `<holy points>`

Mounts of pretenders.

### `#holyrange`

#taskmaster #elementrange.

### `#horrordeserter`
**Arguments:** `<percent>`

The province.

### `#horrormark`

#fear <value> Anyone striking this monster may be horror marked.

### `#horsetattoo`
**Arguments:** `<value>`

### `#hp`
**Arguments:** `<value>`

Sets hit points.

### `#hpoverflow`

To make a monster more vulnerable to a particular type of The monster's hit points can increase past the normal elemental damage, it can be given a negative resistance maximum.

### `#iceforging`
**Arguments:** `<value>`

Monster will get the Undreaming ability giving this bonus to its The monster generates <value> resources in cold provinces.

### `#icenatprot`

#haltheretic.

### `#iceprot`
**Arguments:** `<prot>`

The monster has a chance of starting with a heroic ability, like Armor protection varies with the temperature in the province.

### `#illusion`

, all of these commands should be combined with.

### `#immobile`

Led to combat by a beast master.

### `#inanimate`

#stormpower attribute, this command is redundant.

### `#incorporate`
**Arguments:** `<dmg>`

This monster will only receive half att/def bonuses from The monster incorporates any swallowed creatures as a part of weapons.

### `#incprovdef`
**Arguments:** `<value>`

Chance in percent that the throne will be destroyed each turn.

### `#incunrest`
**Arguments:** `<value>`

Destroyed each turn.

### `#indepmove`
**Arguments:** `<percent>`

#assassin This ability is used by certain horrors to make them move on This monster is an assassin.

### `#indepspells`
**Arguments:** `<level>`

=Kokytos Usually independent mages will only know low level research.

### `#indepstay`
**Arguments:** `<0 | 1>`

Catch his target unawares and without bodyguards.

### `#inquisitor`

#deathparalyze <aoe> This monster has a bonus when preaching against enemy The monster erupts in a paralyzing explosion on death, forcing Dominion.

### `#insane`
**Arguments:** `<percent>`

Grants acid resistance to the monster.

### `#insanify`
**Arguments:** `<percent>`

First form as the value.

### `#inspirational`

#bloodrange.

### `#inspiringres`

#pillagebonus #divineins.

### `#invisible`

#slothpower #unseen.

### `#invulnerable`
**Arguments:** `<prot>`

The monster has invulnerability to non-magical weapons.

### `#ironarmor`

Human being should be about 32 pixels high and there should Indicates that the armor is made of iron.

### `#ironskin`

The monster has a <percent> chance of casting a random spell The item automatically applies the Ironskin spell to the bearer.

### `#ironvul`
**Arguments:** `<points>`

Battlefield around it or enemies that attack it in some The monster will take points amount of extra damage when manner.

### `#islance`

This monster cannot be polymorphed in combat or by the use This item can only be used by mounted or flying units or of transformation rituals.

### `#itemcost1`
**Arguments:** `<bonus>`

### `#itemslots`
**Arguments:** `<slot value>`

### `#ivylord`
**Arguments:** `<id>`

This monster will receive extra units when summoning Vine Mounts & Riders Men of different kind.

### `#kokytosret`

#taxcollector #infernoret.

### `#labxpshape`
**Arguments:** `<xp value>`

### `#lanceok`

The monster is a drake and is affected by the Dragon Master This monster can use lances even though it is not flying or ability.

### `#landdamage`
**Arguments:** `<percent>`

A commander with this ability can use the make plague order.

### `#landrec`

”<monster name>” #(terrain)fortrec "<monster name>" Add a unit to the list of recruitable units for this nation in This unit can be recruited in (terrain) provinces wit.

### `#landshape`
**Arguments:** `<"monster_name">`

### `#latehero`
**Arguments:** `<min turn>`

Chance of getting one level higher in Fire or Air magic.

### `#leper`
**Arguments:** `<percent>`

Number of extra blood magic level for this purpose.

### `#lesserhorror`

#nospiritform Monsters with this tag are used as lesser horrors.

### `#lich`
**Arguments:** `<"monster_name">`

The monster grows into the previous monster once it has this The monster will get this shape if it is transformed into a lich.

### `#likespop`
**Arguments:** `<poptype>`

Uwdefunit1d The nation gets cheaper PD from this poptype.

### `#lizard`

Feet Four legged beast, but lower.

### `#localsun`

The monster generates gold that is added to the treasury.

### `#magic`

The weapon only affects undead.

### `#magicarmor`

#spr1 "<imgfile>" Indicates that the armor is magic.

### `#magicbeing`

Ground bound monsters.

### `#magicboost`
**Arguments:** `<path> <boost>`

Gives a boost or reduction to the monster's magic ability for 4 Astral one or all magic paths.

### `#magiccommand`
**Arguments:** `<value>`

And herded into battle.

### `#magicimmune`

The monster produces a number of glamour gems that can be The monster is immune to the research effects of Drain and used in combat.

### `#magiconly`

The weapon only affects magic beings.

### `#magicpower`
**Arguments:** `<bonus>`

The monster automatically swallows the targets of a successful The monster will get stat increases or decreases depending on trampling attack.

### `#magicscale`
**Arguments:** `<value>`

The magic path associated with this site.

### `#magicskill`
**Arguments:** `<path> <level>`

Rituals cast by the monster.

### `#magicstudy`
**Arguments:** `<bonus>`

Between worlds and the home of horrors and the Void beings Research bonus depending on the magic scale of the province.

### `#makemonsters1`

### `#makemonsters2`

To #makemonsters5 can also be -16 Random yazad used to summon more monsters per month.

### `#makemonsters5`

Can also be -16 Random yazad used to summon more monsters per month.

### `#makepearls`
**Arguments:** `<value>`

The monster has a <percent> chance of casting a random spell The monster can turn <value> water gems to <value> astral in combat instead of a good one.

### `#mapmove`
**Arguments:** `<speed>`

Sets strategic map movement.

### `#mapteleport`

#statstorm <value> Like #teleport but only grants teleport ability on the map, not 1+ = Can storm forts even if it is stationary (map move 0), in combat.

### `#mason`

Nbr can be negative for montags.

### `#masterrit`
**Arguments:** `<value>`

Pathboost for the purpose of casting rituals.

### `#mastersmith`
**Arguments:** `<value>`

The monster gets the Innate Spellcaster ability and can cast a The monster's magic paths are counted as <value> higher than number of spells every combat round in addition to its normal actual when fo.

### `#maxage`
**Arguments:** `<age>`

Disease healers should have higher values.

### `#maxdeadhp`
**Arguments:** `<HP>`

Ethereal monsters can pass through walls during the storming The monster's bonus HP gained from corpse eating can never of a fortress.

### `#maxsize`
**Arguments:** `<size>`

This item can only by a unit of this size or smaller.

### `#mindcollar`
**Arguments:** `<dmg>`

#siegebonus <value> Unit will take this amount of damage if it breaks in combat.

### `#mindslime`

#reinvigoration #heat.

### `#mindvessel`
**Arguments:** `<0 | 1>`

And infects military units in the province.

### `#minsize`
**Arguments:** `<size>`

This bearer of this item will be blessed automatically if it is This item can only by a unit of this size or larger.

### `#miscshape`

Apparent on gods as dominion strength and path costs are Use this for strange things like cubes or fountains.

### `#mobilearcher`

#dancespr <flysprite nbr> #statstorm The look of the sprite circling the unit.

### `#monpresentrec`
**Arguments:** `<"monster_name">`

This monster can only be recruited if a unit of "monster name" #gcost <gold> type is present in the recruiting province.

### `#montag`

Could be set as random monster summons by assigning #summon.

### `#montagweight`
**Arguments:** `<weight>`

Anyway

### `#mor`
**Arguments:** `<morale>`

Sets morale.

### `#moregrowth`
**Arguments:** `<-5 - 5>`

East Alters the minimum/maximum allowed value of this scale when 5 Middle East designing the pretender's dominion.

### `#moreluck`
**Arguments:** `<-5 - 5>`

Alters the minimum/maximum allowed value of this scale when 8 India designing the pretender's dominion.

### `#moremagic`
**Arguments:** `<-5 - 5>`

Informs the AI that this nation has cheap expendable sacred Alters the minimum/maximum allowed value of this scale when units.

### `#mountainsurvival`

Gives the monster the ability to seduce like a Nagini.

### `#mounted`

Squad

### `#mountmnr`
**Arguments:** `<"monster_name">`

This monster will receive extra units when summoning Lamias Use this to give a mount to a rider.

### `#mr`
**Arguments:** `<magic resistance>`

Sets magic resistance.

### `#mrhalf`

But will be repaired if there are enough resources in the A successful MR check will halve the damage received from province the monster is in.

### `#naga`

Miscs Snake like lower part and a humanoid upper body.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#nametype`
**Arguments:** `<name type nbr>`

Weight)

### `#natmon`
**Arguments:** `<"monster_name">`

Adds a monster that can be recruited if the site is owned by the nation set by the #nat command.

### `#neednoteat`

This monster doesn’t need any food and doesn’t consume any.

### `#newmonster`

Creates a new monster and selects it for modding. Optional monster number parameter.

### `#nightmareaura`
**Arguments:** `<area>`

Armor piercing fire damage.

### `#noagingland`
**Arguments:** `<percent>`

#singlebattle Friendly units in the same province as this item have a chance #chaosrec of not aging each year.

### `#nobadevents`
**Arguments:** `<value>`

Maximum of -3 /.

### `#nobarding`

Squad and an additional -1 for every additional squad.

### `#nocastmindless`
**Arguments:** `<0 | 1>`

Immunity negates 1 means mindless units cannot cast this spell.

### `#nocoldblood`

The item will grant a stealth bonus to already stealthy units.

### `#nofalldmg`

Modifier of +1 for up to 3 squads and -1 for every additional This rider will not take any damage when falling off his mount.

### `#nofmounts`
**Arguments:** `<mounts>`

Gets the capture slave ability.

### `#noforeignrec`

Nation cannot recruit independent units.

### `#nofriders`
**Arguments:** `<riders>`

### `#noheal`

Only contain one type of troop.

### `#nohof`

Unit will change into this monster when made a prophet.

### `#noitem`

Monster (up to the limit of three).

### `#noleader`

The mount's armor.

### `#nomagicleader`

Increases undead leadership by this amount.

### `#norange`
**Arguments:** `<chance>`

Reduces the effectiveness of the assassin's Patience.

### `#noremount`

These commands are used to set basic leadership values for The commander will no be able to reclaim his mount through a monsters.

### `#noreqlab`

Service to a player and when it leaves.

### `#noreqtemple`

Wounded to count as having fought.

### `#noriverpass`

The commands in this section deal with stealth, spying and The monster is unable to cross rivers even when they are various forms of assassination.

### `#noslowrec`

#triplegod <type> Removes the slowrec attribute.

### `#nospiritform`

Monsters with this tag are used as lesser horrors.

### `#notdomshape`
**Arguments:** `<"monster_name">`

That the shape change takes place even if the target got soul The monster changes to this shape when not inside a friendly slayed.

### `#nothrowoff`

Standard for non-elite commanders.

### `#notmnr`
**Arguments:** `<"monster_name">`

Does not affect true sight This type of monster is unable to cast this spell.

### `#noundeadleader`

Can only deploy in the skirmish formation and will attack the This monster cannot lead undead units when it is a commander enemy without regard for any battle plan.

### `#noweapon`
**Arguments:** `<0 | 1>`

The item slots can be tweaked afterwards by using the item Use this on monsters with no weapons that are not supposed slot commands in the next section.

### `#nowish`

The province.

### `#okleader`

Humans that carry a throne with an emperor in it.

### `#okmagicleader`

Command

### `#okundeadleader`

#bodyguard <bonus> This monster has an innate ability to command 50 undead The monster's morale is increased by <bonus> when its orders beings.

### `#older`
**Arguments:** `<age>`

Protects a unit when diseased.

### `#onlyfemale`

As well, if any.

### `#onlymnr`
**Arguments:** `<"monster_name">`

Does not affect flying/floating Only this type of monster is able to cast this spell.

### `#onlyundead`

Draw at a smaller size than normal.

### `#orderrecscale`
**Arguments:** `<value>`

Enchantment is active.

### `#overcharged`
**Arguments:** `<dmg>
<value>`

Squares around it.

### `#ownsmonrec`
**Arguments:** `<"monster_name">`

The cost in recruitment points for the monster.

### `#pathcost`
**Arguments:** `<design points>`

Created after one another and the first one should be used as The cost for a new path in design points when this monster is the god.

### `#patience`
**Arguments:** `<value>`

Of moving each month as long as it is owned by independents.

### `#patrolbonus`
**Arguments:** `<value>`

When this monster dies, the unit that strikes the killing blow is A value of ten will make this monster count as ten extra cursed.

### `#pierce`

And 2 units never resist the effect.

### `#pierceres`

The monster takes half damage from piercing weapons.

### `#pillagebonus`
**Arguments:** `<value>`

Its killer and anyone nearby with poison (str 2 poison cloud).

### `#plaguedoctor`
**Arguments:** `<percent>`

#landdamage <percent> A commander with this ability can use the make plague order.

### `#plainshape`
**Arguments:** `<"monster_name">`

Value can be -1 for a bad result, 0 to disable and 1 for a good The monster changes to this shape when moving from a forest result.

### `#plant`

#spiritform The monster is a plant.

### `#poison`

The effects of the weapon can be resisted by MR.

### `#poisonarmor`
**Arguments:** `<dmg>`

Anyone striking this monster with short weapons will be Immortality poisoned.

### `#poisoncloud`

#poisonskin DAMAGE REDUCTION #poisonarmor.

### `#poisonres`
**Arguments:** `<value>`

Makes the item cast this spell automatically in battle.

### `#poisonskin`
**Arguments:** `<0-500>`

This section covers different the different kinds of The monster has a skin that exudes a paralyzing poison, which immortality available for monsters.

### `#polygetmagic`
**Arguments:** `<0 | 1>`

Damage 1 means a unit polymorphed by this ritual will get the magic of 57 Animals are immune the target creature.

### `#polyimmune`

#islance This monster cannot be polymorphed in combat or by the use This item can only be used by mounted or flying units or of transformation rituals.

### `#pooramphibian`

Monsters with this tag are summoned by the Swarm spell on This monster can travel under water, but is hindered by it.

### `#poorleader`

If this is set to one the mount is also a commander.

### `#poormagicleader`

An undead or a demon.

### `#poorundeadleader`

A formation fighter is well drilled in using tight formations and This monster has an innate ability to command 10 undead can fit more units into one square.

### `#popkill`

#commaster

### `#powerofdeath`

#dompower #fearofflood.

### `#praise`
**Arguments:** `<value>`

#incscale <scale> The praise ability raises friendly dominion in the province.

### `#prec`
**Arguments:** `<precision>`

Sets precision.

### `#prophetshape`
**Arguments:** `<"monster_name">`

### `#prot`
**Arguments:** `<protection>`

Sets natural protection.

### `#quadruped`

Head Four legged beasts.

### `#quickness`

The item grants Quickness (double movement, +2 attack & Defines what kind of a weapon, if any, the unit gets when it defense, attacks twice as often).

### `#raiseonkill`
**Arguments:** `<chance>`

The monster will get stat increases or decreases depending on Monster has a chance in percent to raise the people it kills as the Sloth scale.

### `#raiseshape`
**Arguments:** `<"monster_name">`

Non-Combat Abilities Changes soulless to another kind of unit for the #raiseonkill or.

### `#raredomsummon`
**Arguments:** `<"monster_name">`

Horror* There is a flat 8% chance of summoning one creature of this -8 Doom Horror type when in a province with friendly dominion.

### `#rcost`
**Arguments:** `<resources>`

Sets the resource cost.

### `#reanimator`
**Arguments:** `<id>`

Can be used up to 3 times on one monster.

### `#reanimpriest`

Number values to provide a random summon of a specific A priest with this attribute will be able to raise undead.

### `#reclimit`
**Arguments:** `<units / turn>`

Starts

### `#reconst`
**Arguments:** `<percent>`

#bugreform <nbr of bugs> Works like regeneration, but only affects inanimate beings.

### `#reform`
**Arguments:** `<chance>`

To get a 50% chance.

### `#reformtime`

If you want the #animalawe <bonus> immortal to reappear the next month.

### `#regainmount`
**Arguments:** `<0 | 1>`

Monster Modding, Leadership & Morale If this is set to one the rider will regain any lost mount(s) for free after the battle.

### `#regeneration`
**Arguments:** `<percent>`

Elemental resistances function like armor by lessening The monster regenerates like a troll and heals damage every damage from attacks of a particular type of element (fire, combat round.

### `#reinvigoration`
**Arguments:** `<points>`

Main part of the swarm.

### `#reqlab`

Desertion Recruiting the monster requires a lab.

### `#reqnoseduce`

Unforgable item, 13 = unforgable unique artifact, 15 = Units with the seduce ability cannot cast this spell.

### `#reqtaskmaster`

Only units with the task master ability can cast this spell.

### `#reqtemple`

The monster only fights in one battle and then leaves, like the Recruiting the monster requires a temple.

### `#researchbonus`

#siegebonus #slothresearch.

### `#resources`
**Arguments:** `<value>`

The monster will become more powerful every time it dies, up The monster generates <value> resources in the province it is to at most limit number of deaths.

### `#ressize`
**Arguments:** `<size>`

Harpy, Vaetti, Hoburg 7 Use this command with a size value of 3 to give a flier resource Bakemono 8 cost calculated based on size 3 instead of.

### `#restricted`
**Arguments:** `<nation_id>`

"nation name".

### `#rpcost`
**Arguments:** `<recruitment points>`

### `#sabbathmaster`

#inquisitor #sabbathslave.

### `#sabbathslave`

Works just like the previous ability, except that at least three The monster is automatically a sabbath slave and does not monsters of this type must be present on the battlefield for the need to cast.

### `#sailing`
**Arguments:** `<ship size> <max unit size>`

This monster can sneak into enemy provinces.

### `#scalewalls`

COMBAT AURAS.

### `#secondtmpshape`
**Arguments:** `<"monster_name">`

The monster will change into the next monster type (next This monster will transform into another monster when it is monster number) after reaching this amount of xp and when killed in.

### `#seduce`

The size in percent of normal size, default is.

### `#selectmonster`
**Arguments:** `<"monster_name">`

Selects an existing monster for modification by name or ID.

### `#shapechance`
**Arguments:** `<percent>`

Monster number can be negative for montag usage.

### `#shapechange`
**Arguments:** `<"monster_name">`

The monster kills (10 x amount) of population in the province it This monster is able to change shape to and from another resides in every month.

### `#shatteredsoul`
**Arguments:** `<percent>`

Maximum of -3 /.

### `#shock`

#friendlyimmune This weapon does shock damage.

### `#shockres`
**Arguments:** `<value>`

Enables user of item to cast this spell in battle or as a ritual The item grants a Shock Resistance bonus.

### `#shrinkhp`
**Arguments:** `<hit points>`

The monster changes to this shape when in battle.

### `#siegebonus`
**Arguments:** `<value>`

Unit will take this amount of damage if it breaks in combat.

### `#singlebattle`

#reqtemple The monster only fights in one battle and then leaves, like the Recruiting the monster requires a temple.

### `#size`
**Arguments:** `<size>`

Sets unit size (1-6).

### `#sizecost`
**Arguments:** `<value>`

##playergodname## Units that are not human sized will have the cost of the ritual ##fullplayergodname## adjusted by (value)*(size difference).

### `#sizeresist`

The weapon does slashing damage.

### `#skilledrider`
**Arguments:** `<value>`

Demons require undead leadership Magic beings require A skilled rider will increase the Morale and Defence Skill of his magic leadership.

### `#slashres`

The monster takes half damage from slashing weapons.

### `#slave`

Possess

### `#slaver`
**Arguments:** `<"monster_name">`

#nofmounts <mounts> Gets the capture slave ability.

### `#slaverbonus`
**Arguments:** `<modifier>`

Mounts

### `#sleepaura`
**Arguments:** `<area>`

Like fear, but negated by true sight.

### `#sleepres`

MAGIC ABILITIES NON-COMBAT ABILITIES #douse.

### `#slimer`
**Arguments:** `<strength>`

Bonus can be a value of one or more.

### `#slothpower`

#unseen

### `#slothresearch`

#patrolbonus #inspiringres.

### `#slowrec`

Different rules than autocalc for normal units and commanders.

### `#smartmount`
**Arguments:** `<smartness>`

### `#snake`

Slots can only have crowns Use this for snakes, wyrms and other monsters without legs.

### `#snaketattoo`
**Arguments:** `<value>`

Mere presence.

### `#snow`

Underwater

### `#sorcerygems`
**Arguments:** `<gems>`

Have increased range.

### `#sorceryrange`
**Arguments:** `<range>`

They are of a single random type of sorcery gem.

### `#speciallook`
**Arguments:** `<value>`

Modding and general commands that are not related to a This command surround a monster with a particle effect.

### `#spikes`

#doheal

### `#spiritform`

The monster is a plant.

### `#spiritsight`

#stormpower #truesight.

### `#spr1`

Sets the normal sprite image file for a monster.

### `#spr2`
**Arguments:** `<"image_path">`

Sets the attack sprite image file for a monster.

### `#spreaddom`
**Arguments:** `<candles>`

Magic tattoo like the units of Marverni and Sauromatia have.

### `#springimmortal`

That animals with a morale of 11 have about 50% chance of The immortal will reform his body in the spring.

### `#springpower`

#blessfly

### `#springshape`
**Arguments:** `<"monster_name">`

The monster changes to this shape when moving from a water Changes into another monster when this season is active.

### `#spy`

Not exceed the value of this ability.

### `#standard`
**Arguments:** `<bonus>`

This monster has an innate ability to command 150 undead The monster increases the morale of units in the same squad by beings.

### `#startaff`
**Arguments:** `<percent>`

Grants cold resistance to the monster.

### `#startage`
**Arguments:** `<age>`

In the same province.

### `#startheroab`
**Arguments:** `<percent>`

### `#startingaff`
**Arguments:** `<affliction bitmask>`

Grants poison resistance to the monster.

### `#startitem`
**Arguments:** `"item name"`

Creature Type & Status The monster starts with this item if it is a commander.

### `#startmajoraff`
**Arguments:** `<percent>`

Grants fire resistance to the monster.

### `#startscout`
**Arguments:** `<"monster_name">`

Nation cannot recruit independent units.

### `#statbreak`

#dancenof <nbr of sprites> The number of sprites circling the unit.

### `#statstorm`
**Arguments:** `<value>`

Like #teleport but only grants teleport ability on the map, not 1+ = Can storm forts even if it is stationary (map move 0), in combat.

### `#stealthboost`
**Arguments:** `<value>`

#nocoldblood The item will grant a stealth bonus to already stealthy units.

### `#stealthy`

In order to be meaningful.

### `#stonebeing`

#stormimmune This monster is a stone being and immune to petrification.

### `#stormimmune`

This monster is a stone being and immune to petrification.

### `#stormpower`

Attribute, this command is redundant.

### `#str`
**Arguments:** `<strength>`

Sets strength.

### `#succubus`
**Arguments:** `<value>`

Mountain passes even if it is cold in the provinces the pass Gives the monster the dream seduction ability like a Succubus.

### `#summerpower`
**Arguments:** `<percent>`

Next few rounds of battle.

### `#summershape`
**Arguments:** `<"monster_name">`

Changes into another monster when this season is active.

### `#summon`
**Arguments:** `<"monster_name">`

Mountain A mage of the same magic path as the site may enter to 8 Waste summon the specified monster.

### `#summon1`

Command summons one monster per month and.

### `#summon2`

To #summon5 more according to the number in the -21 Random directional dwarf command.

### `#summon5`

More according to the number in the -21 Random directional dwarf command.

### `#sunawe`
**Arguments:** `<bonus>`

#entangle Works like Awe, except that it doesn't work if there is no sun Anyone striking this monster may get entangled.

### `#superiorleader`

Dominions 6 system of seperate stats for rider and mount is Leadership value.

### `#superiormagicleader`

All slaves under the command of this monster have their This monster has an innate ability to command 200 magic morale increased by <bonus>.

### `#supplybonus`

#drainimmune.

### `#swampsurvival`

Standard

### `#swimming`

#uwbug The monster can cross rivers.

### `#tainted`
**Arguments:** `<chance>`

The monster can turn <value> nature gems to <value> death Chance in percent of being horror marked each turn.

### `#taskmaster`

#elementrange.

### `#taxcollector`

Province, even if they are protected by a fort.

### `#teleport`

But only grants teleport ability on the map, not 1+ = Can storm forts even if it is stationary (map move 0), in combat.

### `#templetrainer`
**Arguments:** `<"monster_name">`

Random good crossbreed A commander with this ability will be able to summon a -11 Random bad crossbreed monster per turn when in a temple province.

### `#thronekill`
**Arguments:** `<chance>`

The value divided by 10 is the amount increased per month.

### `#tmpairgems`
**Arguments:** `<gems>`

Magic Research The monster produces a number of air gems that can be used in These commands affect the magic research abilities of the combat.

### `#tmpdeathgems`
**Arguments:** `<gems>`

The monster gains <value> research bonus per step of sloth in The monster produces a number of death gems that can be the province.

### `#tmpearthgems`
**Arguments:** `<gems>`

Makes a commander better or worse at magic research.

### `#tmpfiregems`
**Arguments:** `<gems>`

Monster have increased range.

### `#tmpglamourgems`
**Arguments:** `<gems>`

### `#tmpwatergems`
**Arguments:** `<gems>`

### `#tolerateund`

% random chance means +2 levels in the path.

### `#tradecoast`
**Arguments:** `<income boost in percent>`

The nation's Dominion enables all units to sail like the dark Income bonus for coastal forts.

### `#trample`

The monster will get stat increases or decreases depending on This monster can trample smaller beings.

### `#trampswallow`

#magicpower <bonus> The monster automatically swallows the targets of a successful The monster will get stat increases or decreases depending on trampling attack.

### `#transformation`
**Arguments:** `<value>`

Non-forest province to a forest province.

### `#triple3mon`

Pretender God Commands The trinity god has 3 different monsters.

### `#troglodyte`

Can only have a limited number of special abilities so don’t Use this for humanoids without a head.

### `#truesight`

#firepower <bonus> True sight makes it possible to see through illusions and The monster will get stat increases or decreases depending on glamour.

### `#twiceborn`
**Arguments:** `<"monster_name">`

### `#twiceborncost`
**Arguments:** `<value>`

##godhis## Units that are not human sized after being transformed by the ##godHis## twiceborn ritual will have the cost of the ritual adjusted by ##dishis## (value)*(size difference after twiceborn).

### `#twistfate`

Will start with a twist fate in combat.

### `#undead`

#amphibian This monster is an undead.

### `#undisciplined`

Undisciplined monsters cannot be given orders in battle.

### `#undisleader`
**Arguments:** `<value>`

(getting two or more levels of the same random path from 1=will negate undisciplined on his followers.

### `#undregen`
**Arguments:** `<percent>`

Example, all undead and inanimate creatures have poison Works like regeneration, but only affects undead beings.

### `#unify`

Any one of the trinity gods can use the unify order to call the other parts of the trinity.

### `#unique`

Immobile does not affect map move, use "#mapmove 0" to There can only be one of this monster.

### `#unmountedspr1`
**Arguments:** `<"image_path">`

### `#unmountedspr2`
**Arguments:** `<"image_path">`

### `#unseen`

#deathpower #twistfate.

### `#unsurr`

#magicpower #spiritsight.

### `#unteleportable`

Its owner and attacks automatically, like the dancing trident.

### `#userestricteditem`
**Arguments:** `<value>`

Feminine names.

### `#uwbug`

The monster can cross rivers.

### `#uwdamage`

So they cannot than there really are.

### `#uwfireshield`
**Arguments:** `<damage>`

This unit is covered in sharp spikes barbs.

### `#uwheat`
**Arguments:** `<0-100>`

Does not grant stealth to monsters that have no stealth to Like #heat, but also works underwater.

### `#uwrec`
**Arguments:** `<"monster_name">`

This unit can be recruited in (terrain) provinces without a fort.

### `#uwregen`
**Arguments:** `<percent>`

Myriad of bugs and reform a little while later.

### `#voidret`
**Arguments:** `<chance>`

Unit takes 50% longer to cast spells, but at half fatigue cost.

### `#voidsanity`
**Arguments:** `<value>`

The basic encumbrance of the monster.

### `#wallmult`
**Arguments:** `<multiplier>`

Uwdefcom2 Modifier for the number of units from the previous #wallunit uwdefunit1 command.

### `#warning`

#sleepres MAGIC ABILITIES NON-COMBAT ABILITIES #douse.

### `#wastesurvival`

Value indicates the difficulty of the morale check, 10 is Monster has the Waste Survival skill.

### `#waterelementals`
**Arguments:** `<bonus>`

Monsters from those five being summoned.

### `#waterrange`
**Arguments:** `<range>`

Gives a chance for another magic skill to the active monster.

### `#watershape`
**Arguments:** `<"monster_name">`

Changes into another monster when this season is active.

### `#weapon`
**Arguments:** `<"weapon_name">`

Assigns a weapon to the monster.

### `#wild`

T'ien Ch'i, Spring and Autumn The throne is wild and is likely to be defended by wild 23 Yomi, Oni Kings monsters.

### `#winterpower`
**Arguments:** `<percent>`

Suffer the damage itself instead of harming the target.

### `#wintershape`
**Arguments:** `<"monster_name">`

The monster will get this shape if it dies and rises again due to Changes into another monster when this season is active.

### `#wolftattoo`
**Arguments:** `<value>`

Calling a god or disciple back from death.

### `#woodenarmor`

Image

### `#worldshape`
**Arguments:** `<"monster_name">`

Many hit points or less.

### `#woundfend`
**Arguments:** `<value>`

Giants have cold resistance.

### `#xpgain`
**Arguments:** `<percent>`

The Cold scale.

### `#xploss`
**Arguments:** `<0-100>`

Of getting to his second shape when the first one gets killes.

### `#xpshape`
**Arguments:** `<xp value>`

The monster will change into the next monster type (next The monster becomes this if affected by the animate tree spell.

### `#xpshapeloss`
**Arguments:** `<0-100>`

Transformations actually taking effect.

### `#xpshapemon`
**Arguments:** `<"monster_name">`

Negative for montags.

### `#xspr1`
**Arguments:** `<"image_path">`

Mount commands Creates a sprite that is used when one corider is lost.

### `#xspr2`
**Arguments:** `<"image_path">`

The smartness determines the chance for the mount to try to Creates the attack sprite that is used when one corider is lost.

### `#yearaging`
**Arguments:** `<value>`

The following commands are exactly like the monster The wielder of this item will age this many extra years each commands with the same name, so no explanation or year.

### `#yearturn`
**Arguments:** `<bonus>`

With no penalty.

## Nametype Commands (1)

### `#selectnametype`
**Arguments:** `<nametype nbr>`

Selects the nametype that will be affected by the following 140 Demons modding commands.

## Nation Commands (223)

### `#addforeigncom`
**Arguments:** `<"monster_name">`

This commander can only be recruited in provinces with no Add a unit to the list of recruits for this nation.

### `#addforeignunit`
**Arguments:** `<"monster_name">`

The number of start units in the second squad.

### `#addgod`
**Arguments:** `<"monster_name">`

<nbr> type

### `#addname`
**Arguments:** `"name"`

Female Adds a name to the selected nametype.

### `#addrandomage`
**Arguments:** `<years>`

Gives the monster the ability to lure enemy commanders like Makes the monster start 1 to years (a random die) older.

### `#addreccom`
**Arguments:** `<"monster_name">`

Add a unit to the list of recruitable commanders for this nation.

### `#addrecunit`
**Arguments:** `<"monster_name">`

This commander can only be recruited in provinces with no Add a unit to the list of recruits for this nation.

### `#addupkeep`
**Arguments:** `<gold>`

#adeptsacr <value> Upkeep will be calculated as if the unit cost this much more to Adept Sacrificer.

### `#aiairnation`

Makes the nation less likely to start in one of the terrains in the Hint to AI that Air magic is used a lot in this nation and that an terrain mask.

### `#aiastralnation`

Reduces the killing effect of cold scales in the capital by this Hint to AI that Astral magic is used a lot in this nation and that many scale steps.

### `#aiawake`
**Arguments:** `<percent>`

Reconsider a non-mage commander recruitment.

### `#aibloodnation`

#moreprod <-5 - 5> Hint to AI that Blood magic is used a lot in this nation and that a Alters the minimum/maximum allowed value of this scale when blood god is probably good.

### `#aideathnation`

Reduces the killing effect of heat scales in the capital by this Hint to AI that Death magic is used a lot in this nation and that many scale steps.

### `#aiearthnation`

Reduces the killing effect of cold scales in forts by this many Hint to AI that Earth magic is used a lot in this nation and that a scale steps.

### `#aiglamournation`

#moreorder <-5 - 5> Hint to AI that Glamour magic is used a lot in this nation and Alters the minimum/maximum allowed value of this scale when that a glamour god is probably good.

### `#aigoodbless`
**Arguments:** `<0-100>`

### `#aiheavyrec`
**Arguments:** `<0-99>`

Nation

### `#aiholdgod`

#aimagerec <0-99> When playing this nation, the AI will never leave the home Will make the AI more likely to recruit mage commanders than province with the pretender.

### `#aimagerec`
**Arguments:** `<0-99>`

When playing this nation, the AI will never leave the home Will make the AI more likely to recruit mage commanders than province with the pretender.

### `#aimusthavemag`
**Arguments:** `<magic path number>`

### `#ainaturenation`

Reduces the killing effect of heat scales in forts by this many Hint to AI that Nature magic is used a lot in this nation and that scale steps.

### `#airblessbonus`
**Arguments:** `<0 - 9>`

Clears the list of pretender gods for this nation.

### `#aiwaternation`

Kills a percentage of the capital's population when the game Hint to AI that Water magic is used a lot in this nation and that starts.

### `#assencloc`
**Arguments:** `<value>`

Percentage of its total hit points every turn it spends in a land Specifies an encounter location that the assassinations will province instead of underwater.

### `#astralblessbonus`
**Arguments:** `<0 - 9>`

### `#autobless`

#minsize <size> This bearer of this item will be blessed automatically if it is This item can only by a unit of this size or larger.

### `#autocompete`

#onlyinanim The bearer of this item will automatically compete in the Arena The item can only be used by inanimate beings.

### `#badindpd`
**Arguments:** `<0 | 1>`

Greater than.

### `#bless`

The item automatically applies the Bless spell to the bearer, Defines what kind of armor, if any, the units gets when it uses like the Shroud of the Battle Saint.

### `#blessairshld`
**Arguments:** `<value>`

Marverni, Time of Druids Blessed troops get Air Shield like from an Air bless.

### `#blessanimawe`
**Arguments:** `<value>`

Fall victim to lethal traps or bloodthirsty monsters.

### `#blessatt`
**Arguments:** `<value>`

Command

### `#blessawe`
**Arguments:** `<value>`

Constructs a temple in the province when the site is Blessed troops get Awe.

### `#blessbers`

Scales or other conditions.

### `#blessbonus`
**Arguments:** `<0 - 9>`

Gods of this nation will get extra bless design points.

### `#blesscoldres`
**Arguments:** `<value>`

Mekone, Brazen Giants Blessed troops get increased Cold Resistance.

### `#blessdarkvis`
**Arguments:** `<value>`

#selectnation <nation nbr> Blessed troops get Darkvision.

### `#blessdef`
**Arguments:** `<value>`

For various independents and temporary monsters in the game Blessed troops get increased Defense skill.

### `#blessdtv`
**Arguments:** `<value>`

Ur, The First City Blessed troops get undying, like the undying bless effect.

### `#blessfireres`
**Arguments:** `<value>`

Nbr Nation Epithet Blessed troops get increased Fire Resistance.

### `#blessfly`

Decrease to Strength, Attack, Defense and Action Points per Will grant the unit flying when blessed.

### `#blesshp`
**Arguments:** `<value>`

A commander who enters the ruin has a chance to discover Blessed troops get increased Hit Points.

### `#blessmor`
**Arguments:** `<value>`

### `#blessmr`
**Arguments:** `<value>`

Lab is already present, there is no effect.

### `#blesspoisres`
**Arguments:** `<value>`

Fomoria, The Cursed Ones Blessed troops get increased Poison Resistance.

### `#blessprec`
**Arguments:** `<value>`

Nation numbers, Early Era Blessed troops get increased Precision skill.

### `#blessreinvig`
**Arguments:** `<value>`

Agartha, Pale Ones Blessed troops get increased Reinvigoration, like from a bless 16 Abysia, Children of Flame with the same name.

### `#blessshockres`
**Arguments:** `<value>`

Ermor, New Faith Blessed troops get increased Shock Resistance.

### `#blessstr`
**Arguments:** `<value>`

A fort is already present, there is no effect.

### `#bloodblessbonus`
**Arguments:** `<0 - 9>`

Nbr Realm Gods of this nation will get extra bless design points of this 1 North type.

### `#bloodnation`

#moreheat <-5 - 5> Makes the AI more likely to research blood magic and hunt for Alters the minimum/maximum allowed value of this scale when blood slaves.

### `#bossname`
**Arguments:** `<"name">`

Templates for the same nation, in which case a random one Name of the band's leader.

### `#brief`
**Arguments:** `"<nation name>"`

Abysia, Blood of Humans A very brief description for popups.

### `#buildcoastfort`
**Arguments:** `<fort nbr>`

Dominion is dying and needs blood sacrifice.

### `#buildfort`
**Arguments:** `<fort nbr>`

Spreads

### `#builduwfort`
**Arguments:** `<fort nbr>`

Priests of this nation cannot preach.

### `#castleprod`
**Arguments:** `<resource boost in percent>`

#domwar <value> Resource bonus for forts.

### `#caveinc`
**Arguments:** `<bonus>`

Bonus in percent to income for all cave forts.

### `#cavelabcost`
**Arguments:** `<price>`

Gold cost for building a lab in a cave.

### `#cavenation`
**Arguments:** `<0-3>`

### `#cavetemplecost`
**Arguments:** `<price>`

Fort nbr Fort name Gold cost for building a temple in a cave.

### `#cheapgod20`
**Arguments:** `<"monster_name">`

### `#cheapgod40`
**Arguments:** `<"monster_name">`

Sets wall commander for underwater forts.

### `#clear`

Clears all properties from the selected entity.

### `#cleargods`

#airblessbonus <0 - 9> Clears the list of pretender gods for this nation.

### `#clearsites`

Underwater nation.

### `#coastcom1`

### `#coastnation`

The nation's capital is in a coastal land province.

### `#coastunit1`

### `#color`
**Arguments:** `<red> <green> <blue>`

Library of Time 4 astral pearls Each of the three colors is a number between.

### `#combatcaster`

#fortkill #glamourmanip.

### `#command`
**Arguments:** `<value>`

Command will give a defence bonus to the unit as well as Increases leadership value by this amount.

### `#commaster`

| <nbr> The monster is automatically a communion master and does Monster will automatically cast this spell just before the battle not need to cast the Communion Master.

### `#comslave`

Can modify the spell used by this command though.

### `#deathblessbonus`
**Arguments:** `<0 - 9>`

Included in the nation's default list of pretenders and need not Gods of this nation will get extra bless design points of this be separately added to the list with the #addgod command.

### `#deathincome`
**Arguments:** `<percent>`

### `#defchaos`
**Arguments:** `<-5 - 5>`

Not be used for any real nations.

### `#defcom`
**Arguments:** `<"monster_name">`

### `#defcom1`
**Arguments:** `<"monster_name">`

Deep and kelp have no fortcom variants, use #uwrec and Commander for local defense.

### `#defcom2`
**Arguments:** `<"monster_name">`

Extra commander for fortified provinces with a province Commander that is in charge of the castle gate defenders.

### `#defdeath`
**Arguments:** `<-5 - 5>`

Observer mods and should not be used for any real nations.

### `#defmult2`
**Arguments:** `<multiplier>`

Never need it.

### `#defmult2b`
**Arguments:** `<multiplier>`

Province except the home province.

### `#defsloth`
**Arguments:** `<-5 - 5>`

This nation gets to view all battles for all players, including Sets the default value of the sloth scale for this nation.

### `#defunit`
**Arguments:** `<"monster_name">`

### `#defunit1`
**Arguments:** `<"monster_name">`

Standard unit for local defense in provinces with forts.

### `#defunit1b`
**Arguments:** `<"monster_name">`

A specific magic item for the commander.

### `#defunit1c`
**Arguments:** `<"monster_name">`

#guardmult <multiplier> Third type of standard unit for local defense.

### `#defunit1d`
**Arguments:** `<"monster_name">`

Will replace the non-foreign variant for forts that is not the Fourth type of standard unit for local defense.

### `#defunit2`
**Arguments:** `<"monster_name">`

Will replace the non-foreign variant for forts that is not the Bonus units for local defense equal to or greater than 20 in capital.

### `#defunit2b`
**Arguments:** `<"monster_name">`

#foreignwallmult <multiplier> Second type of bonus units for local defense equal to or greater Will replace the non-foreign variant for forts that is not the than 20 in provinces with.

### `#delgod`
**Arguments:** `"<monster>"`

Uwdefunit1c Deletes a god that is otherwise gained through homerealm use.

### `#descr`
**Arguments:** `"This is a test nation, it has no units and
"`

Sets the description text for the entity.

### `#disableoldnations`

The terrain mask.

### `#disbless`
**Arguments:** `"bless name"`

<nbr> Disables a bless effect for this nation.

### `#domdeathsense`

Ashen Empire Ermor.

### `#domkill`
**Arguments:** `<value>`

A death scale does not adversely affect supplies.

### `#domwar`
**Arguments:** `<value>`

Machaka, Lion Kings Dominion conflict bonus.

### `#dyingdom`

#buildcoastfort <fort nbr> Dominion is dying and needs blood sacrifice.

### `#earthblessbonus`
**Arguments:** `<0 - 9>`

Monster must have the #startdom and #pathcost commands Gods of this nation will get extra bless design points of this set.

### `#end`

Ends the current entity definition block.

### `#epithet`
**Arguments:** `"<nation name>"`

Atlantis, Kings of the Deep Sets the epithet of a nation, e.

### `#era`
**Arguments:** `<era nbr>`

Sets nation era (1=Early, 2=Middle, 3=Late).

### `#farsumcom`
**Arguments:** `<"monster_name">`

Cold immunity negates Sets the commander for farsummoned units to something 11 Shock immunity negates other than the normal units.

### `#flag`
**Arguments:** `<"image_path">`

Singing Tree 2 glamour gems Replace the flag with an image.

### `#foreignguardcom`
**Arguments:** `<"monster_name">`

Will yield 2 units per point of defense, which is also the default.

### `#foreignguardunit`
**Arguments:** `<"monster_name">`

### `#foreignwallcom`
**Arguments:** `<"monster_name">`

### `#foreignwallunit`
**Arguments:** `<"monster_name">`

### `#forestlabcost`
**Arguments:** `<price>`

Command does not affect what type of forts the nation can Gold cost for building a lab in a forest.

### `#foresttemplecost`
**Arguments:** `<price>`

Deviate from the standard one for their fortera.

### `#fortcoldscaleres`
**Arguments:** `<steps>`

#aiearthnation Reduces the killing effect of cold scales in forts by this many Hint to AI that Earth magic is used a lot in this nation and that a scale steps.

### `#fortcost`
**Arguments:** `<extra cost>`

Hall Extra cost is the additional amount of gold the nation must pay for its forts, expressed as a percentage of the normal cost for.

### `#fortera`
**Arguments:** `<0-4>`

Determines what kind of forts the nation can build.

### `#fortheatscaleres`
**Arguments:** `<steps>`

#ainaturenation Reduces the killing effect of heat scales in forts by this many Hint to AI that Nature magic is used a lot in this nation and that scale steps.

### `#fullgodname`

## 1 = cannot be cast indoors, -1 = can be cast indoors, even it it ##godname## usually should not be castable there.

### `#fullplayergodname`

## adjusted by (value)*(size difference).

### `#goddomchaos`
**Arguments:** `<value>`

(percent) to become diseased.

### `#goddomcold`
**Arguments:** `<value>`

The indicated chance (percent) to be struck by holy fire, which Increases the Cold scale of the god's dominion.

### `#goddomdeath`
**Arguments:** `<value>`

### `#goddomdrain`
**Arguments:** `<value>`

A commander may enter the site to gain <value> experience Increases the Drain scale of the god's dominion.

### `#goddomlazy`
**Arguments:** `<value>`

(percent) to be horror marked.

### `#goddommisfortune`
**Arguments:** `<value>`

Deals 10 points of armor- negating damage if they fail a MR Increases the Misfortune scale of the god's dominion.

### `#godhe`

## zero

### `#godhim`

## ##dishim## Global Enchantments ##godhimself## Global enchantments have some special settings that can be ##dishimself## manipulated with the following commands.

### `#godhimself`

## Global enchantments have some special settings that can be ##dishimself## manipulated with the following commands.

### `#godhis`

## Units that are not human sized after being transformed by the ##godHis## twiceborn ritual will have the cost of the ritual adjusted by ##dishis## (value)*(size difference after twiceborn).

### `#godname`

## usually should not be castable there.

### `#godnat`

## the province where it was cast.

### `#godrebirth`

The nation's god does not lose any magic path levels when Sets wall defenders for underwater forts.

### `#godsite`
**Arguments:** `"<site name>"`

As a default choice for nations that belong to the same realm.

### `#godthrone`

## ##playerthrone##.

### `#goodundeadleader`

Bodyguard is present during an assassination.

### `#grandcom`
**Arguments:** `<0 | 1>`

Whoever strikes the killing blow against this monster is This monster can participate in grand communions.

### `#guardcom`
**Arguments:** `<"monster_name">`

Extra commander for fortified provinces with a province Commander that is in charge of the castle gate defenders.

### `#guardspirit`
**Arguments:** `<"monster_name">`

<nbr> dominion has influence.

### `#guardunit`
**Arguments:** `<"monster_name">`

Standard unit for local defense in provinces with forts.

### `#halfdeathinc`

#domunrest <value> The nation will only be half as affected by the death/growth The nation's Dominion causes unrest.

### `#halfdeathpop`

Unrest reducing Dominions.

### `#hatesterr`
**Arguments:** `<terrain mask>`

#aiairnation Makes the nation less likely to start in one of the terrains in the Hint to AI that Air magic is used a lot in this nation and that an terrain mask.

### `#hero1`

### `#hidedom`
**Arguments:** `<0 | 1>`

Berytos has this ability.

### `#homecoldscaleres`
**Arguments:** `<steps>`

#aiastralnation Reduces the killing effect of cold scales in the capital by this Hint to AI that Astral magic is used a lot in this nation and that many scale steps.

### `#homefort`
**Arguments:** `<fort nbr>`

Gold cost for building a temple.

### `#homeheatscaleres`
**Arguments:** `<steps>`

#aideathnation Reduces the killing effect of heat scales in the capital by this Hint to AI that Death magic is used a lot in this nation and that many scale steps.

### `#homerealm`
**Arguments:** `<realm nbr>`

Gods of this nation will get extra bless design points of this Any gods that belong to this realm (through the use of the type.

### `#homeshape`
**Arguments:** `<"monster_name">`

From secondshape to firstshape.

### `#homesick`
**Arguments:** `<percent>`

Summoning circle (assassin inside circle) This monster takes damage equal to the indicated percentage 8 summoning circle (assassin at entrance) of its total hit points every turn it spends away from.

### `#idealcold`
**Arguments:** `<cold>`

### `#indepflag`
**Arguments:** `<"image_path">`

Atlantis, Emergence of the Deep Ones Replace the flag of independents with an image.

### `#islandnation`

Frozen Fountain 3 water gems The nation prefers to start on an island if possible, or a coast if Mineral Cave 1 earth gem no suitable island could be found.

### `#islandsite`
**Arguments:** `"<site name>"`

Wide and 128 pixels high.

### `#killcappop`
**Arguments:** `<percent>`

#aiwaternation Kills a percentage of the capital's population when the game Hint to AI that Water magic is used a lot in this nation and that starts.

### `#labcost`
**Arguments:** `<price>`

Dominion strength.

### `#landcom`

”<monster name>” plain Add a unit to the list of recruitable commanders for this nation forest in overwater forts.

### `#likesterr`
**Arguments:** `<terrain mask>`

The height

### `#limitedregen`
**Arguments:** `<percent>`

Nation nbr of -1 uses the last manipulated nation, so this Like regeneration, but doesn't work on inanimate beings.

### `#maxprison`
**Arguments:** `<0 - 2>`

America Gods of this nation must not be imprisoned to more than this 7 Africa level.

### `#merccost`
**Arguments:** `<percent>`

Waste Mercenaries are this much more expensive.

### `#minprison`
**Arguments:** `<0 - 2>`

Gods of this nation must be imprisoned to at least this level.

### `#moreheat`
**Arguments:** `<-5 - 5>`

Makes the AI more likely to research blood magic and hunt for Alters the minimum/maximum allowed value of this scale when blood slaves.

### `#moreorder`
**Arguments:** `<-5 - 5>`

Command is not needed and should not be used if you only Alters the minimum/maximum allowed value of this scale when intend the god to be used by a single nation or a few nations designing the pretend.

### `#moreprod`
**Arguments:** `<-5 - 5>`

Hint to AI that Blood magic is used a lot in this nation and that a Alters the minimum/maximum allowed value of this scale when blood god is probably good.

### `#mountiscom`
**Arguments:** `<0 | 1>`

#poorleader If this is set to one the mount is also a commander.

### `#mountlabcost`
**Arguments:** `<price>`

(underwater era 1) Gold cost for building a lab in highlands.

### `#mounttemplecost`
**Arguments:** `<price>`

Of Bronze and Crystal Gold cost for building a temple in highlands.

### `#multihero1`

Deep (deep seas) to set the first of seven possible multiheroes.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#nat`
**Arguments:** `<nation_id>`

Sets the nation for the following two commands.

### `#natcom`
**Arguments:** `<"monster_name">`

Mask Terrain Adds a monster that can be recruited as commander if the site 1 Plain is owned by the nation set by the #nat command.

### `#nationinc`
**Arguments:** `<percent>`

Angel when in battle.

### `#natureblessbonus`
**Arguments:** `<0 - 9>`

God, the homerealm of a nation cannot be cleared.

### `#newnation`

Creates a new nation and selects it for modding.

### `#nodeathsupply`

#domkill <value> A death scale does not adversely affect supplies.

### `#nopreach`

#builduwfort <fort nbr> Priests of this nation cannot preach.

### `#noundeadgods`

Uwdefmult2 This nation cannot choose any pretender that is undead.

### `#nrunits`
**Arguments:** `<value>`

Number of troops in the band.

### `#playergodname`

## Units that are not human sized will have the cost of the ritual ##fullplayergodname## adjusted by (value)*(size difference).

### `#playergodthrone`

## This is a localized global enchantment.

### `#recallgod`
**Arguments:** `<value>`

Mictlan, Reign of Blood This value is added to the priest level of everyone doing Call 26 Xibalba, Vigil of the Sun God.

### `#sacrificedom`

Each month

### `#seatrace`

Scout in the province.

### `#secondarycolor`
**Arguments:** `<red> <green> <blue>`

Wild Forest 1 nature gem The animated background is made up of two colors, the Ancient Forest 2 nature gems primary color (from #color) and this one.

### `#secondshape`
**Arguments:** `<"monster_name">`

Forts than what would otherwise be possible for the nation.

### `#selectbless`
**Arguments:** `"<bless name>"`

<nbr> 119 Misc female Selects an existing blessing that will be affected by the 120 C'tis female following modding commands.

### `#selectnation`
**Arguments:** `<nation_id>`

Selects an existing nation for modification by name or ID.

### `#slothincome`
**Arguments:** `<percent>`

### `#sneakunit`
**Arguments:** `<value>`

#curse The item will grant stealth to non-stealthy units.

### `#startcom`
**Arguments:** `<"monster_name">`

The number of start units.

### `#startdom`

And #pathcost commands Gods of this nation will get extra bless design points of this set.

### `#startsite`

Sets starting magic site for nation.

### `#startunitnbrs1`
**Arguments:** `<nbr of units>`

### `#startunitnbrs2`
**Arguments:** `<nbr of units>`

### `#startunittype1`
**Arguments:** `<"monster_name">`

Clears the list of recruitable units and commanders (but not The commander will have units of this type.

### `#startunittype2`
**Arguments:** `<"monster_name">`

Removes all old start troops and must be used when changing The commander will have a second squad with these units.

### `#summary`
**Arguments:** `"<nation name>"`

Agartha, Ktonian Dead A summary of the benefits and dominion themes of the nation.

### `#supayareanim`

Also remove its description, so make sure to set name before Reanimating priest of this nation will create supayas like description.

### `#superiorundeadleader`

Serves a nation that can reanimate undead.

### `#swamplabcost`
**Arguments:** `<price>`

(standard era 1) Gold cost for building a lab in a swamp.

### `#swamptemplecost`
**Arguments:** `<price>`

(standard era 3) Gold cost for building a temple in a swamp.

### `#syncretism`
**Arguments:** `<0 | 1>`

Percentage value can be negative to make a nation that earn Syncretism enable all priests to convert conquered temples less gold.

### `#templecost`
**Arguments:** `<price>`

### `#templegems`
**Arguments:** `<type>`

Building a fort.

### `#templeholypoints`
**Arguments:** `<value>`

Vanheim

### `#templepic`
**Arguments:** `<pic nbr>`

Walled City Temple should look like this.

### `#triplegod`
**Arguments:** `<type>`

Removes the slowrec attribute.

### `#triplegodmag`
**Arguments:** `<penalty>`

### `#turmoilincome`
**Arguments:** `<percent>`

### `#undcommand`

#tmpairgems.

### `#undeadreanim`

Nbr School All undead priests of this nation are able to reanimate the dead – 1 cannot be researched as if the had the #reanimpriest attribute.

### `#unit`
**Arguments:** `<"monster_name">`

Always start a new template with this command.

### `#uwbuild`
**Arguments:** `<0 | 1>`

### `#uwcom`
**Arguments:** `<"monster_name">`

Recruitable in a specific terrain regardless of the presence of a Add a unit to the list of recruitable commanders in underwater fort.

### `#uwwallcom`
**Arguments:** `<"monster_name">`

### `#uwwallmult`
**Arguments:** `<multiplier>`

Nation can choose this god for 20 design points less.

### `#uwwallunit`
**Arguments:** `<"monster_name">`

The nation's god does not lose any magic path levels when Sets wall defenders for underwater forts.

### `#viewallbat`

#defsloth <-5 - 5> This nation gets to view all battles for all players, including Sets the default value of the sloth scale for this nation.

### `#viewallprov`

Sets the preferred level of cold for the nation.

### `#wallcom`
**Arguments:** `<"monster_name">`

The following commands works just like their land Commander that is in charge of the wall defenders.

### `#wallunit`
**Arguments:** `<"monster_name">`

Nations, a few land nations that also thrive underwater (like Unit type the will man the walls when the castle is stormed.

### `#wastelabcost`
**Arguments:** `<price>`

Fort Gold cost for building a lab in a waste.

### `#wastetemplecost`
**Arguments:** `<price>`

City Gold cost for building a temple in a waste.

### `#waterblessbonus`
**Arguments:** `<0 - 9>`

Must be removed with the #delgod command.

## Poptype Commands (5)

### `#arenagems`
**Arguments:** `<amount>`

Militia, Lt Inf, Archers About this many fire gems will be awarded as the prize for 61 Vaettir, Trolls winning the arena.

### `#arenagold`
**Arguments:** `<amount>`

About this much gold will be awarded as the prize for winning 58 Lt Inf, Hvy Inf, X-Bow the arena.

### `#gemlongevity`
**Arguments:** `<level>`

This command can be used to make magic gems last for 64 Tritons multiple battle within one month.

### `#selectpoptype`
**Arguments:** `<poptype>`

A poptype must be selected before using any other poptype 72 Mermen commands.

### `#startresearch`
**Arguments:** `<RP>`

Inf, Hvy Cavalry The amount of start research per magic scale above.

## Site Commands (128)

### `#addforeigncom`
**Arguments:** `<"monster_name">`

This commander can only be recruited in provinces with no Add a unit to the list of recruits for this nation.

### `#addforeignunit`
**Arguments:** `<"monster_name">`

The number of start units in the second squad.

### `#addgod`
**Arguments:** `<"monster_name">`

<nbr> type

### `#addname`
**Arguments:** `"name"`

Female Adds a name to the selected nametype.

### `#addrandomage`
**Arguments:** `<years>`

Gives the monster the ability to lure enemy commanders like Makes the monster start 1 to years (a random die) older.

### `#addupkeep`
**Arguments:** `<gold>`

#adeptsacr <value> Upkeep will be calculated as if the unit cost this much more to Adept Sacrificer.

### `#adventureruin`
**Arguments:** `<success chance>`

#blesshp <value> A commander who enters the ruin has a chance to discover Blessed troops get increased Hit Points.

### `#airrange`
**Arguments:** `<range boost>`

All Air rituals cast in this province have their range increased.

### `#allrange`
**Arguments:** `<range boost>`

All rituals of the Thaumaturgy school cast in this province cost All magic rituals cast in this province have their range <bonus> % less to cast.

### `#altcost`
**Arguments:** `<bonus>`

Increased by <range boost> provinces.

### `#autocompete`

#onlyinanim The bearer of this item will automatically compete in the Arena The item can only be used by inanimate beings.

### `#bloodcost`
**Arguments:** `<bonus>`

Scale Effects All rituals of the Blood Magic school cast in this province cost These commands cause the magic site to alter the scales in <bonus> % less to cast.

### `#chaosscale`
**Arguments:** `<value>`

Set what the site should look like.

### `#claim`

(percent) to become cursed.

### `#clear`

Clears all properties from the selected entity.

### `#clearfx`

The value Unique is special and indicates that there can only Clear all effects of this bless.

### `#clearnation`

Vanarus, Land of the Chuds Clears away all special settings for the nation, like ideal cold, 80 Jotunheim, Iron Woods reanimating priests, underwater nation, starting sites, heroes, 81 Nidavangr, B.

### `#clearscales`

Unique name, because this will lessen the risk of conflicts Clear the scale requirements between mods.

### `#cluster`
**Arguments:** `<value>`

Command are not active until the throne is claimed by a god, a Assigns a cluster value to a throne.

### `#coastcom1`

### `#coastunit1`

### `#com`
**Arguments:** `<"monster_name">`

What type of monster the commander is.

### `#combatcaster`

#fortkill #glamourmanip.

### `#command`
**Arguments:** `<value>`

Command will give a defence bonus to the unit as well as Increases leadership value by this amount.

### `#commaster`

| <nbr> The monster is automatically a communion master and does Monster will automatically cast this spell just before the battle not need to cast the Communion Master.

### `#comslave`

Can modify the spell used by this command though.

### `#conjcost`
**Arguments:** `<bonus>`

Increased by <range boost> provinces.

### `#constcost`
**Arguments:** `<bonus>`

Province have their range increased by <range boost> All rituals of the Construction school cast in this province cost provinces.

### `#copysite`
**Arguments:** `"<site name>"`

### `#cost0`
**Arguments:** `<value>`

Basic Site Modding The minimum skill required in path0 and also the cost in bless.

### `#cost1`
**Arguments:** `<value>`

The minimum skill required in path1 and also added to the cost.

### `#deathincome`
**Arguments:** `<percent>`

### `#deathrange`
**Arguments:** `<range boost>`

Enchantments cast at a discount, the gems between its All Death rituals cast in this province have their range normal cost and discounted cost count toward making increased by <range boost> provinces.

### `#deathscale`
**Arguments:** `<value>`

Certain terrains or flag the site as unique.

### `#defcom`
**Arguments:** `<"monster_name">`

### `#defcom1`
**Arguments:** `<"monster_name">`

Deep and kelp have no fortcom variants, use #uwrec and Commander for local defense.

### `#defcom2`
**Arguments:** `<"monster_name">`

Extra commander for fortified provinces with a province Commander that is in charge of the castle gate defenders.

### `#defdrain`
**Arguments:** `<-5 - 5>`

These commands set a nation's starting sites and its terrain Sets the default value of the drain scale for this nation.

### `#defmisfortune`
**Arguments:** `<-5 - 5>`

Sites, Terrain, Temperature Sets the default value of the misfortune scale for this nation.

### `#defunit`
**Arguments:** `<"monster_name">`

### `#defunit1`
**Arguments:** `<"monster_name">`

Standard unit for local defense in provinces with forts.

### `#defunit1b`
**Arguments:** `<"monster_name">`

A specific magic item for the commander.

### `#defunit1c`
**Arguments:** `<"monster_name">`

#guardmult <multiplier> Third type of standard unit for local defense.

### `#defunit1d`
**Arguments:** `<"monster_name">`

Will replace the non-foreign variant for forts that is not the Fourth type of standard unit for local defense.

### `#defunit2`
**Arguments:** `<"monster_name">`

Will replace the non-foreign variant for forts that is not the Bonus units for local defense equal to or greater than 20 in capital.

### `#defunit2b`
**Arguments:** `<"monster_name">`

#foreignwallmult <multiplier> Second type of bonus units for local defense equal to or greater Will replace the non-foreign variant for forts that is not the than 20 in provinces with.

### `#descr`
**Arguments:** `"This is a test nation, it has no units and
"`

Sets the description text for the entity.

### `#disease`
**Arguments:** `<chance>`

Initiating a number of temple checks per month.

### `#dominion`
**Arguments:** `<temple checks per month>`

Can be a number from 1 to.

### `#drainscale`
**Arguments:** `<value>`

In any sea

### `#earthrange`
**Arguments:** `<range boost>`

All Earth rituals cast in this province have their range increased These commands govern the casting cost of rituals when the by <range boost> provinces.

### `#enchcost`
**Arguments:** `<bonus>`

All Sorcery magic (Astral, Death, Nature, Blood) rituals cast in All rituals of the Enchantment school cast in this province cost this province have their range increased by <range boost> <bonus> % le.

### `#end`

Ends the current entity definition block.

### `#evocost`
**Arguments:** `<bonus>`

Increased by <range boost> provinces.

### `#farsumcom`
**Arguments:** `<"monster_name">`

Cold immunity negates Sets the commander for farsummoned units to something 11 Shock immunity negates other than the normal units.

### `#foreignguardcom`
**Arguments:** `<"monster_name">`

Will yield 2 units per point of defense, which is also the default.

### `#foreignguardunit`
**Arguments:** `<"monster_name">`

### `#foreignwallcom`
**Arguments:** `<"monster_name">`

### `#foreignwallunit`
**Arguments:** `<"monster_name">`

### `#fort`
**Arguments:** `<fort nbr>`

Blessed troops get increased Morale.

### `#gems`
**Arguments:** `<path> <amount>`

Sets gem income.

### `#gold`
**Arguments:** `<gold / month>`

Sets gold income/cost.

### `#grandcom`
**Arguments:** `<0 | 1>`

Whoever strikes the killing blow against this monster is This monster can participate in grand communions.

### `#growthscale`
**Arguments:** `<value>`

Clears the attributes of the selected magic site.

### `#guardcom`
**Arguments:** `<"monster_name">`

Extra commander for fortified provinces with a province Commander that is in charge of the castle gate defenders.

### `#guardunit`
**Arguments:** `<"monster_name">`

Standard unit for local defense in provinces with forts.

### `#heatscale`
**Arguments:** `<value>`

This sets the name of the site.

### `#holyfire`
**Arguments:** `<chance>`

Value to increase Production.

### `#holypower`
**Arguments:** `<chance>`

Increases the Death scale of the god's dominion.

### `#homecoldscaleres`
**Arguments:** `<steps>`

#aiastralnation Reduces the killing effect of cold scales in the capital by this Hint to AI that Astral magic is used a lot in this nation and that many scale steps.

### `#homecom`
**Arguments:** `<"monster_name">`

Adds recruitable commander to site.

### `#homefort`
**Arguments:** `<fort nbr>`

Gold cost for building a temple.

### `#homeheatscaleres`
**Arguments:** `<steps>`

#aideathnation Reduces the killing effect of heat scales in the capital by this Hint to AI that Death magic is used a lot in this nation and that many scale steps.

### `#homemon`
**Arguments:** `<"monster_name">`

Adds recruitable monster to site.

### `#homeshape`
**Arguments:** `<"monster_name">`

From secondshape to firstshape.

### `#homesick`
**Arguments:** `<percent>`

Summoning circle (assassin inside circle) This monster takes damage equal to the indicated percentage 8 summoning circle (assassin at entrance) of its total hit points every turn it spends away from.

### `#incscale`

For the All Water rituals cast in this province have their range opposite scales.

### `#lab`

Is augmented.

### `#landcom`

”<monster name>” plain Add a unit to the list of recruitable commanders for this nation forest in overwater forts.

### `#level`
**Arguments:** `<0-2>`

Each template will determine how the AI designs its god for Level 0 bands appear early in the game.

### `#loc`
**Arguments:** `<locmask>`

The bless requires a cold scale of value or more.

### `#look`
**Arguments:** `<site spr>`

### `#luckscale`
**Arguments:** `<value>`

Copies all site stats from this site, including name.

### `#misfortscale`
**Arguments:** `<value>`

Useful numbers are 735 for a site that can be located in any The bless requires a misfortune scale of value or more.

### `#mon`
**Arguments:** `<"monster_name">`

Extra units for province defense.

### `#mountiscom`
**Arguments:** `<0 | 1>`

#poorleader If this is set to one the mount is also a commander.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#natcom`
**Arguments:** `<"monster_name">`

Mask Terrain Adds a monster that can be recruited as commander if the site 1 Plain is owned by the nation set by the #nat command.

### `#naturerange`
**Arguments:** `<range boost>`

All Nature rituals cast in this province have their range.

### `#newsite`

Creates a new magic site and selects it for modding.

### `#nrunits`
**Arguments:** `<value>`

Number of troops in the band.

### `#orderscale`
**Arguments:** `<value>`

The bless requires an order scale of value or more.

### `#path0`
**Arguments:** `<path nbr>`

Use up all of the attribute slots.

### `#path1`
**Arguments:** `<path nbr>`

Modding commands.

### `#popgrowth`
**Arguments:** `<per mille>`

### `#prodscale`
**Arguments:** `<value>`

Always use this command at the end of modifying a site.

### `#rarity`
**Arguments:** `<rarity>`

Sets rarity for sites/events.

### `#res`
**Arguments:** `<amount>`

Void creatures.

### `#riverstart`

Shows some suitable start sites.

### `#scry`
**Arguments:** `<duration>`

Been discovered yet.

### `#scryrange`
**Arguments:** `<range>`

Heat Set the maximum range of the scry ability.

### `#selectsite`
**Arguments:** `"<site name>"`

Selects an existing magic site for modification by name or ID.

### `#slothincome`
**Arguments:** `<percent>`

### `#slothscale`
**Arguments:** `<value>`

Last ones will look the same).

### `#sneakunit`
**Arguments:** `<value>`

#curse The item will grant stealth to non-stealthy units.

### `#startcom`
**Arguments:** `<"monster_name">`

The number of start units.

### `#startunitnbrs1`
**Arguments:** `<nbr of units>`

### `#startunitnbrs2`
**Arguments:** `<nbr of units>`

### `#startunittype1`
**Arguments:** `<"monster_name">`

Clears the list of recruitable units and commanders (but not The commander will have units of this type.

### `#startunittype2`
**Arguments:** `<"monster_name">`

Removes all old start troops and must be used when changing The commander will have a second squad with these units.

### `#summonlvl2`
**Arguments:** `<"monster_name">`

Swamp Like #summon, except that the mage summoning must be at 256 Deep sea least level.

### `#summonlvl3`
**Arguments:** `<"monster_name">`

Unique Like #summon, except that the mage summoning must be at least level.

### `#summonlvl4`
**Arguments:** `<"monster_name">`

Gives gem income to the magic site.

### `#supply`
**Arguments:** `<value>`

Adds to castle defenders.

### `#temple`

#blessawe <value> Constructs a temple in the province when the site is Blessed troops get Awe.

### `#thaucost`
**Arguments:** `<bonus>`

### `#turmoilincome`
**Arguments:** `<percent>`

### `#type`
**Arguments:** `<id>`

Magic Item Modding Defines whether the item is 1-handed or 2- handed weapon, a shield, a helmet, a body armor, a pair of boots or something These commands allow the modding of magic items.

### `#undcommand`

#tmpairgems.

### `#unit`
**Arguments:** `<"monster_name">`

Always start a new template with this command.

### `#uwcom`
**Arguments:** `<"monster_name">`

Recruitable in a specific terrain regardless of the presence of a Add a unit to the list of recruitable commanders in underwater fort.

### `#uwnation`

#clearsites Underwater nation.

### `#uwwallcom`
**Arguments:** `<"monster_name">`

### `#uwwallunit`
**Arguments:** `<"monster_name">`

The nation's god does not lose any magic path levels when Sets wall defenders for underwater forts.

### `#voidgate`
**Arguments:** `<success chance>`

Adds a gold income to the site.

### `#wallcom`
**Arguments:** `<"monster_name">`

The following commands works just like their land Commander that is in charge of the wall defenders.

### `#wallunit`
**Arguments:** `<"monster_name">`

Nations, a few land nations that also thrive underwater (like Unit type the will man the walls when the castle is stormed.

### `#xp`
**Arguments:** `<value>`

#goddomdrain <value> A commander may enter the site to gain <value> experience Increases the Drain scale of the god's dominion.

## Sound Commands (4)

### `#loop`
**Arguments:** `<sample nbr>`

This should be -1 for sound effects, -2 for music, -4 for battle.

### `#sample`
**Arguments:** `<"file_path">`

That plays nicely with ongoing games.

### `#selectsound`
**Arguments:** `<id>`

Selects an existing or a new sample (sound effect or 89 Explosion background music) that will be affected by the following 90-96 Music tracks modding commands.

### `#smpmode`
**Arguments:** `<mode>`

Mod Info Mode should be 0 for standard sound effect and 2 for music.

## Spell Commands (85)

### `#aoe`
**Arguments:** `<squares>`

Sets area of effect.

### `#autospell`
**Arguments:** `"<spell name>"`

### `#autospellrepeat`
**Arguments:** `<spells / round>`

The item automatically applies the Stoneskin spell to the Makes an item cast its autospell every round of combat.

### `#autoundead`

All researchable spells are removed from the game.

### `#barkskin`

The item grants a penetration bonus that makes spells cast by The item automatically applies the Barkskin spell to the bearer, the bearer harder to resist.

### `#clear`

Clears all properties from the selected entity.

### `#clearallspells`

Reanimate various types of undead.

### `#copyspell`
**Arguments:** `"<spell name>"`

Copies all stats from another spell.

### `#cure`
**Arguments:** `"text"`

Afflictions healed.

### `#damage`
**Arguments:** `<dmg>`

Sets spell damage.

### `#descr`
**Arguments:** `"This is a test nation, it has no units and
"`

Sets the description text for the entity.

### `#details`
**Arguments:** `"<text>"`

Therodos

### `#dishe`

### `#dishim`

## Global Enchantments ##godhimself## Global enchantments have some special settings that can be ##dishimself## manipulated with the following commands.

### `#dishimself`

## manipulated with the following commands.

### `#dishis`

## (value)*(size difference after twiceborn).

### `#disname`

## and other battlefield wide spells cannot be cast indoors by ##fullplayername## default.

### `#disnat`

### `#dispimmune`
**Arguments:** `<0 - 2>`

This command will make an enchantment immune to dispels.

### `#effect`
**Arguments:** `<eff>`

Sets the spell effect number.

### `#end`

Ends the current entity definition block.

### `#fatiguecost`
**Arguments:** `<fat>`

Sets spell fatigue cost.

### `#flightspr`
**Arguments:** `<flysprite nbr>`

#nogeosrc <terrain mask>__ Spell or ritual cannot be cast Set the sprite or particle effect to be used when this spell is from any of these terrains.

### `#friendlyench`
**Arguments:** `<0 | 1>`

MR roll negates (easy) 1 means the enchantment created by this spell is friendly.

### `#fullplayername`

## default

### `#ghostreanim`

Spell detail text under the main spell description.

### `#globallook`
**Arguments:** `<1 - 9>`

A disciple or a god.

### `#godpathspell`
**Arguments:** `<-1 - 7>`

Not affect undeads This is used for divine spells that should only be available when 20 Def roll negates damage the God is best with this particular magic path.

### `#greekreanim`

A text description of the spell.

### `#hiddenench`
**Arguments:** `<0 | 1>`

Are immune 1 means the enchantment created by this spell is not visible 32 Illusions are immune during battles.

### `#horsereanim`

Reanimating priests with holy magic of level 3 or higher can 2 Evocation reanimate longdead horsemen.

### `#infernoret`
**Arguments:** `<chance>`

Affects the spell casting time for commanders.

### `#localglobal`
**Arguments:** `<0 | 1>`

##playergodthrone## This is a localized global enchantment.

### `#makecrater`
**Arguments:** `<0 | 1>`

= ritual range cannot trace through water provinces.

### `#manikinreanim`

Current spell.

### `#maxbounces`
**Arguments:** `<bounces>`

Values for globallook Set the maximum number of bounces for a chain lightning 1 Eternal Pyre (effect 134) type of spell effect.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#napbreakrit`
**Arguments:** `<value>`

That is sent to all players.

### `#newspell`

Creates a new spell and selects it for modding.

### `#nextingeo`
**Arguments:** `<terrain mask>`

The spell after this will also take effect if it cast in this terrain.

### `#nextspell`
**Arguments:** `"<spell name>"`

<nbr> 10044 Transform 1 With this command the effect of another spell will also take 10045 Force-transform 1 place when the effect of the main spell occurs.

### `#nogeodst`
**Arguments:** `<terrain mask>`

Explosion

### `#nogeosrc`
**Arguments:** `<terrain mask>`

__ Spell or ritual cannot be cast Set the sprite or particle effect to be used when this spell is from any of these terrains.

### `#nolandtrace`
**Arguments:** `<0 | 1>`

Spell Range & Targeting 1 = ritual range cannot trace over land.

### `#notfornation`
**Arguments:** `<nation_id>`

"nation name" 4 Non-magic beings immune Restricts a spell so that it cannot be used by this nation.

### `#notindoors`
**Arguments:** `<-1 to 1>`

##fullgodname## 1 = cannot be cast indoors, -1 = can be cast indoors, even it it ##godname## usually should not be castable there.

### `#nowatertrace`
**Arguments:** `<0 | 1>`

### `#nreff`
**Arguments:** `<nbr of effects>`

Terrain masks Sets the number of effects for this spell.

### `#onebattlespell`
**Arguments:** `"<spell name>"`

<nbr> The monster is automatically a communion master and does Monster will automatically cast this spell just before the battle not need to cast the Communion Master spell to join a starts.

### `#onlyatsite`
**Arguments:** `<"site name">`

The sample that will sound when this spell strikes down.

### `#onlycoastsrc`
**Arguments:** `<0 | 1>`

Effects"

### `#onlyfriendlydst`
**Arguments:** `<0 - 2>`

Sample must be in.

### `#onlygeodst`
**Arguments:** `<terrain mask>`

"Flysprites".

### `#onlygeosrc`
**Arguments:** `<terrain mask>`

Range need not be walkable (default) Spell or ritual can only be cast from one of these terrains.

### `#onlyowndst`
**Arguments:** `<0 | 1>`

### `#onlysitedst`
**Arguments:** `<"site name">`

Table "Sound effects".

### `#path`
**Arguments:** `<reqnr> <path nbr>`

Sets magic path requirement.

### `#pathlevel`
**Arguments:** `<reqnr> <level>`

Sets required magic path level.

### `#playername`

### `#playerthrone`

### `#portent`
**Arguments:** `"text"`

This command works for both local and global enchentments.

### `#precision`
**Arguments:** `<prec>`

Many Sites Set the precision for this spell.

### `#priestreanim`

Commands

### `#provrange`
**Arguments:** `<range>`

### `#randomspell`
**Arguments:** `<percent>`

### `#range`
**Arguments:** `<range>`

Sets weapon range or spell range.

### `#reqnoplant`

#mainpath <path> Trees and plants cannot cast this spell.

### `#reqsun`
**Arguments:** `<0 | 1>`

Vortex This command can make a combat spell uncastable when there 7 Winds of Parched Magic is no sun present.

### `#researchgoal`
**Arguments:** `"spell name"`

"item name" * Nametypes: 100-399, 170+ for modding This will make the AI research to get access to this spell or * Spells: 0-7999, 2000+ for modding magic item.

### `#researchlevel`
**Arguments:** `<level>`

Sets spell research level.

### `#school`
**Arguments:** `<school nbr>`

Sets spell research school.

### `#selectspell`
**Arguments:** `"<spell name>"`

Selects an existing spell for modification by name or ID.

### `#sethome`

Of Misery The commander casting this ritual will get his home province 4 Stellar Focus set to the current province.

### `#spec`
**Arguments:** `<spec bitmask>`

Spell will only be available for this nation.

### `#speedmult`
**Arguments:** `<1-3>`

= can only target own provinces.

### `#spell`
**Arguments:** `"<spell name>"`

### `#spellreqfly`
**Arguments:** `<0 | 1>`

Roll negates 1 means only flying units can cast this ritual.

### `#spellsinger`

#voidret <chance> Unit takes 50% longer to cast spells, but at half fatigue cost.

### `#stoneskin`

#autospellrepeat <spells / round> The item automatically applies the Stoneskin spell to the Makes an item cast its autospell every round of combat.

### `#strikesound`
**Arguments:** `<sample nbr>`

#onlyatsite <"site name"> The sample that will sound when this spell strikes down.

### `#sumhealaffs`
**Arguments:** `<value>`

Things like the name of the caster's god.

### `#tombwyrmreanim`

#clear Reanimating priests can reanimate soulless of C'tis, longdead Clears the current spell.

### `#walkable`
**Arguments:** `<0 | 1>`

Sets the range of a ritual in provinces.

### `#wightreanim`

Reanimating priests with holy magic of level 4 or higher can 6 Blood reanimate undead Lictors.

### `#worldvisible`
**Arguments:** `<0 | 1>`

This enchantment can be seen from all over the world.

## Unknown Commands (40)

### `#airelementals`

The amount of research bonus received per mage from a magic.

### `#batstartsum1`

The effect sloth and productivity has on income.

### `#batstartsum1d6`

The effect cold and heat have on income.

### `#batstartsum2`

#slothresources <percent>.

### `#batstartsum2d6`

#coldsupply <percent>.

### `#batstartsum3`

The effect sloth and productivity have on resources.

### `#batstartsum3d6`

The effect cold and heat has on supplies.

### `#batstartsum4`

### `#batstartsum4d6`

#tempscalecap <value>.

### `#batstartsum5`

#coldincome <percent>.

### `#batstartsum5d6`

Changing any scale more than this does not yield extra design.

### `#battlesum2`

The effect death and growth has on income.

### `#battlesum3`

#deathsupply <percent>.

### `#battlesum4`

The effect death and growth has on supplies.

### `#deathdeath`
**Arguments:** `<0.01 percent>`

### `#deathsupply`
**Arguments:** `<percent>`

### `#domsummon`

A multiplier for the amount of supplies found in a land.

### `#eventisrare`
**Arguments:** `<percent>`

### `#lamialord`

How (mis)fortune affects the possibility of an event being good.

### `#luckevents`
**Arguments:** `<percent>`

### `#makemonsters3`

The effect turmoil and order has on income.

### `#makemonsters4`

#turmoilevents <percent>.

### `#minsizeleader`
**Arguments:** `<0 - 6>`

Sauromatia Name Modding 137 Marverni male.

### `#misfortune`
**Arguments:** `<percent>`

### `#onisummon`

#luckevents <percent>.

### `#poppergold`
**Arguments:** `<people>`

### `#researchscale`
**Arguments:** `<bonus>`

### `#resourcemult`
**Arguments:** `<percent>`

### `#skirmisher`
**Arguments:** `<-25 - 25>`

Male

### `#slothresources`
**Arguments:** `<percent>`

### `#summon3`

#eventisrare <percent>.

### `#summon4`

Random events are divided into two categories, common and.

### `#supplymult`
**Arguments:** `<percent>`

### `#tempscalecap`
**Arguments:** `<value>`

### `#tmpastralgems`

#tmpdeathgems #poppergold <people>.

### `#tmpbloodslaves`

#resourcemult <percent>.

### `#tmpnaturegems`

The amount of people required for one gold in taxes.

### `#turmoilevents`
**Arguments:** `<percent>`

### `#unresthalfinc`
**Arguments:** `<unrest level>`

### `#unresthalfres`
**Arguments:** `<unrest level>`

## Weapon Commands (102)

### `#acid`

The effects of the weapon may be resisted by MR, but there is a This weapon does acid damage.

### `#afflictions`

### `#aftercloudarea`
**Arguments:** `<aoe>`

Size of the aftercloud effect of this weapon.

### `#ammo`

Sets ammunition count for ranged weapons.

### `#aoe`
**Arguments:** `<squares>`

Sets area of effect.

### `#armornegating`

#demonundead The weapon is armor negating.

### `#armorpiercing`

The weapon only affects sacred troops.

### `#att`
**Arguments:** `<attack>`

Sets attack skill.

### `#beam`

Secondary effect itself has an area of effect of one or greater.

### `#blunt`

Mindless beings are immune to this weapon.

### `#bonus`

#iceweapon This is an intrinsic weapon that will not incur a multiple weapon The weapon is made of ice and cannot be fire blessed.

### `#bowstr`

#def 0 This is the same as the #thirdstr command.

### `#charge`

A magic weapon item.

### `#clear`

Clears all properties from the selected entity.

### `#coldifhit`
**Arguments:** `<dmg>`

Mail Hauberk AP cold damage.

### `#copyweapon`
**Arguments:** `<"weapon_name">`

Copies all stats from another weapon.

### `#danceweapon`
**Arguments:** `<"weapon_name">`

#nomovepen The weapon used for the attack.

### `#defroll`

(AP 8) If the victim passes a defence roll vs 3d6, there will be no 221 Fire (AP 12) effect.

### `#demononly`

When determining damage.

### `#demonundead`

The weapon is armor negating.

### `#dmg`

Sets weapon damage.

### `#dt_aff`

Command that was used for bows in Dominions.

### `#dt_bouncekill`

Caught in net This effect will bounce a few times to nearby targets, like a 53 surprised chain lightning.

### `#dt_cap`

Rage Sets the damage type to capped damage (max 1 HP damage) 8 decay like a whip or a blowgun.

### `#dt_constructonly`

Disease Only inanimate beings are affected by this weapon.

### `#dt_demon`

#dt_aff Sets the damage type to anti-demon damage.

### `#dt_drain`

False fetters The weapon drains life force from its target, healing damage 18 limp and reducing fatigue for the attacker.

### `#dt_holy`

Bleed Sets the damage type to holy damage.

### `#dt_interrupt`

Slowed This damage is not real, but it can still interrupt mages casting 41 rusty spells.

### `#dt_large`

Affliction numbers The weapon does triple damage against creatures larger than the attacker.

### `#dt_magic`

Powers of

### `#dt_normal`

Club, axe Sets the damage type to normal damage.

### `#dt_paralyze`

You must enter the value of 2^3 as an argument to #dmg in Sets the damage type to paralyze.

### `#dt_poison`

Spear, pike Sets the damage delivery mechanism to poison damage.

### `#dt_raise`

Plague If the target is killed by the weapon, it is animated as a soulless 5 curse of stones servant of the attacker.

### `#dt_realstun`

Webbed Sets the damage type to stun.

### `#dt_sizestun`

Weakness Sets the damage type to fatigue damage that is less effective on 21 battle fright large targets.

### `#dt_small`

The #dmg command, so the weapon's damage value is The weapon does double damage against creatures smaller interpreted as a bitmask value according to the (affliction than the attacker.

### `#dt_stun`

Slime Sets the damage type to fatigue damage.

### `#dt_weakness`

Asleep The weapon drains strength from its target instead of doing 11 rusty armor normal damage.

### `#dt_weapondrain`

Chest wound The weapon drains life, but max 5 points of the damage is used 24 crippled to heal the wielder.

### `#end`

Ends the current entity definition block.

### `#enemyimmune`

Reduced damage from this weapon.

### `#explspr`
**Arguments:** `<fx nbr>`

Sticky goo Use this command to set how the explosion looks like when a 339 3 Evil death thingy missile hits something or when a melee weapon strikes.

### `#false`

The weapon causes false damage (like most glamour magic) 143 Disease (non resistable) instead of real damage.

### `#fireifhit`
**Arguments:** `<dmg>`

Hauberk AP fire damage.

### `#flail`

This secondary effect will affect anyone harmed by the The weapon has a +2 attack bonus against shields.

### `#flyingimmune`

#fire Flying and floating beings are immune to this weapon.

### `#flyspr`
**Arguments:** `<flysprite nbr> <animation lgth>`

Secret and don't show it in weapon info).

### `#friendlyimmune`

This weapon does shock damage.

### `#fullstr`

Enemies, only doing damage to specific types of creatures, The full weapon wielder's strength will be added to the how it can be resisted etc.

### `#halfstr`

Only one half of the weapon wielder's strength will be added to These commands add additional modification to the type of the damage.

### `#hardmrneg`

#acid The effects of the weapon may be resisted by MR, but there is a This weapon does acid damage.

### `#holyifhit`
**Arguments:** `<dmg>`

Flysprite Anim lgth Looks like AN damage that only affect demons and undeads.

### `#holystunifhit`
**Arguments:** `<dmg>`

Frost swirl Stun that only affects demons and undead.

### `#iceweapon`

This is an intrinsic weapon that will not incur a multiple weapon The weapon is made of ice and cannot be fire blessed.

### `#illusionsimmune`

Petrification Illusions are immune to this weapon.

### `#immortal`

Weapons

### `#inanimateimmune`

This weapon does cold damage.

### `#internal`

Poison The weapon inflicts internal damage that cannot be negated by 51 Strong Poison effects like mist form, mossbody, etc.

### `#ironweapon`

Or more indicates the number of squares that will be affected.

### `#killdemonifhit`
**Arguments:** `<dmg>`

Sling stone AN damage that only affect demons.

### `#killmagicifhit`
**Arguments:** `<dmg>`

Arrow AN damage that only affect magic beings.

### `#len`
**Arguments:** `<length>`

Value is 0, a spear costs 1 resource and a sword costs.

### `#melee50`

Variant with area effects.

### `#mind`

#blunt Mindless beings are immune to this weapon.

### `#morroll`

(AN 10) If the victim passes a morale roll vs 3d6, there will be no effect.

### `#mrnegates`

#poison The effects of the weapon can be resisted by MR.

### `#mrnegateseasily`

Example, Poison Sling and Snake Bladder Stick both have this The effects of the weapon can be easily resisted by MR.

### `#name`
**Arguments:** `<"name">`

Sets the name of the entity. Must be first command after new/select.

### `#natural`

Damage from strength.

### `#newweapon`
**Arguments:** `<weapon_id>`

Creates a new weapon and selects it for modding. Weapon number should be 1000+.

### `#nomovepen`

The weapon used for the attack.

### `#norepel`

This weapon cannot be used to repel attacks.

### `#nostr`

The strength of the weapon wielder will not be added to the.

### `#notdismounted`

Weapon

### `#notmounted`
**Arguments:** `<1-2>`

This weapon cannot be used while mounted (2=keep this.

### `#nouw`

Blast This ranged weapon cannot be used underwater.

### `#nratt`
**Arguments:** `<nbr of attacks>`

Sets number of attacks.

### `#petrifyifhit`
**Arguments:** `<dmg>`

Bane fire arrow Petrification.

### `#poisonifdmg`
**Arguments:** `<dmg>`

Shield AN poison damage.

### `#range`
**Arguments:** `<range>`

Sets weapon range or spell range.

### `#range0`

Some premade effects.

### `#range050`

Activate when the target is hit, even if actual damage is not This ranged weapon has a 50% chance of being used in melee.

### `#rcost`
**Arguments:** `<resources>`

Sets the resource cost.

### `#sacredonly`

#armorpiercing The weapon only affects sacred troops.

### `#secondaryeffect`
**Arguments:** `<"weapon_name">`

#flail This secondary effect will affect anyone harmed by the The weapon has a +2 attack bonus against shields.

### `#secondaryeffectalways`
**Arguments:** `<"weapon_name">`

#unrepel This secondary effect will affect anyone attacked by the Attacks with this weapon cannot be repelled.

### `#selectweapon`
**Arguments:** `<"weapon_name">`

Selects an existing weapon for modification by name or ID.

### `#shockifhit`
**Arguments:** `<dmg>`

Hauberk AN shock damage.

### `#skip`

Weapons

### `#skip2`

Secondary weapon effects Once this weapon is used, skip the next 2 weapons.

### `#slash`

#sizeresist The weapon does slashing damage.

### `#sound`
**Arguments:** `<sample nbr>`

Things like breath weapons or mind blasts.

### `#spiritformimmune`

Poison Spiritform beings and illusions are immune to this weapon.

### `#thirdstr`

#end Only one third of the weapon wielder's strength will be added to the damage.

### `#twohanded`

Be in

### `#undeadimmune`

Resistance only take half damage.

### `#undeadonly`

#magic The weapon only affects undead.

### `#unrepel`

This secondary effect will affect anyone attacked by the Attacks with this weapon cannot be repelled.

### `#uwok`

Rising mists This ranged weapon can be used underwater and is not 10101-10119 Explosion affected by storms.

### `#woodenweapon`

The weapon is made of wood.
