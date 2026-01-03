# Dominions 6 Command Reference

Auto-generated from dom6modman.pdf

Total commands: 1162

## Commands by Entity Type

### Armor (13 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#aftercloud` | <cloudstr> <cloudtype> | be an #end command at the end. |
| `#aftercloudarea` | <aoe> | #name "<name>" Size of the aftercloud effect of this weapon. |
| `#coldifhit` | <dmg> | 12 Scale Mail Hauberk AP cold damage. |
| `#copyarmor` | "<armor name>" | \| <armor nbr> 64 Poison Copies all stats from an existing armor. |
| `#enc` | <encumbrance> | 20 Iron Cap Set the encumbrance value. |
| `#fireifhit` | <dmg> | 10 Leather Hauberk AP fire damage. |
| `#newarmor` | <armor nbr> | regardless of hit or miss. |
| `#poisonifdmg` | <dmg> | 3 Kite Shield AN poison damage. |
| `#prot` | <protection> | 16777216 Leeching Sets the protection value of the armor. |
| `#protparts` | <head prot> <body prot> | Sets the protection value of an armor that has both head and |
| `#rcost` | <resources> | 7 Scale Mail Cuirass Sets the resource cost for the armor. |
| `#selectarmor` | "<armor name>" | \| <nbr> body protecion, e. |
| `#shockifhit` | <dmg> | 14 Plate Hauberk AN shock damage. |

### Event (9 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#descr` | "This is a test nation, it has no units and
" | Dragon (265)" to distinguish between different monsters of requires more work to |
| `#domstr` | <level> | Important Notes Set dominion strength 1-10. |
| `#end` | - | #modname "Minimal Mod" Always put this command last, after all commands belongin |
| `#favrit` | <disschool> <level> | "ritual name" \| "item name" * Nations: 0-499, 150+ for modding Makes the AI pri |
| `#form` | "monster name" | - |
| `#modname` | "Minimal Mod" | Always put this command last, after all commands belonging to #description "Add  |
| `#prison` | <nbr> | - |
| `#researchgoal` | "spell name" | \| "item name" * Nametypes: 100-399, 170+ for modding This will make the AI rese |
| `#scale` | <scale nbr> <value> | Modding Number Limits Scales are numbered from 0-5 and they can have values of - |

### Item (75 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#aiassmod` | <bonus> | Uses a user made image for item sprite. |
| `#aibadlvl` | <level> | number from -1 to 7 from Table 20. |
| `#ainocast` | <0 or 1> | can be anything from 1 to 8. |
| `#aispellmod` | <bonus> | Works like #mainlevel. |
| `#armor` | "<armor name>" | \| <armor nbr> The item automatically applies the Bless spell to the bearer, Def |
| `#autobless` | - | #minsize <size> This bearer of this item will be blessed automatically if it is  |
| `#autospell` | "<spell name>" | - |
| `#autospellrepeat` | <spells / round> | The item automatically applies the Stoneskin spell to the Makes an item cast its |
| `#barkskin` | - | The item grants a penetration bonus that makes spells cast by The item automatic |
| `#bers` | - | The bearer of the item will go berserk as soon as battle begins. |
| `#bless` | - | #armor "<armor name>" \| <armor nbr> The item automatically applies the Bless sp |
| `#chestwound` | - | #reqeyes The item bearer suffers a Chest Wound affliction, which cannot The item |
| `#clearallitems` | - | #reqplant All forgeable magic items are removed from the game. |
| `#coldres` | <value> | Magic & Spells The item grants a Cold Resistance bonus. |
| `#constlevel` | <level> | - |
| `#copyitem` | "<item name>" | \| <item nbr> no need for this command. |
| `#copyspr` | <item nbr> | likable for the spell AI. |
| `#cursed` | - | #run The item is cursed and cannot be dropped. |
| `#enchantedblood` | <points> | #noforgebonus Gets points bonus to MR and heals about 0. |
| `#extralife` | - | The bearer of the item is resurrected once when killed in combat. |
| `#feeblemind` | - | command can be used multiple times on the same item. |
| `#fireres` | <value> | item. |
| `#fly` | - | The item grants its bearer the ability to fly. |
| `#guardspiritbonus` | <value> | #nationrebate <nation nbr> \| "nation name" Increases the chance of receiving a  |
| `#heavyitem` | <0 or 1> | - |
| `#hp` | <value> | 1 1-handed Weapon The item grants a bonus to hit points. |
| `#ironskin` | - | The monster has a <percent> chance of casting a random spell The item automatica |
| `#islance` | - | This monster cannot be polymorphed in combat or by the use This item can only be |
| `#limitedregen` | <percent> | nation nbr of -1 uses the last manipulated nation, so this Like regeneration, bu |
| `#luck` | - | magic item will remove its description as well, so make sure to The item grants  |
| `#mainlevel` | <path> | Main path level requirement to forge this magic item. |
| `#mainpath` | <path> | Trees and plants cannot cast this spell. |
| `#mapspeed` | <value> | an artifact without being construction level 8. |
| `#maxsize` | <size> | This item can only by a unit of this size or smaller. |
| `#minsize` | <size> | This bearer of this item will be blessed automatically if it is This item can on |
| `#morale` | <value> | - |
| `#nationrebate` | <nation nbr> | \| "nation name" Increases the chance of receiving a guardian spirit with +value |
| `#newitem` | - | Creates a new magic item and selects it for modding by the following commands. |
| `#nocoldblood` | - | The item will grant a stealth bonus to already stealthy units. |
| `#nodemon` | - | Restrictions The item cannot be used by demons. |
| `#nofemale` | - | #notfornation <nation nbr> \| "nation name" The item cannot be used by females. |
| `#nofind` | - | The item cannot be used by immobile beings. |
| `#noforgebonus` | - | Gets points bonus to MR and heals about 0. |
| `#noimmobile` | - | #nofind The item cannot be used by immobile beings. |
| `#noinanim` | - | -1 restricts the item to the last manipulated nation, so this The item cannot be |
| `#nomounted` | - | (double combat speed). |
| `#noundead` | - | The item is restricted to this nation only. |
| `#pen` | <value> | - |
| `#poisonres` | <value> | Makes the item cast this spell automatically in battle. |
| `#polyimmune` | - | #islance This monster cannot be polymorphed in combat or by the use This item ca |
| `#quickness` | - | #weapon "<weapon name>" \| <weapon nbr> The item grants Quickness (double moveme |
| `#reqeyes` | - | The item bearer suffers a Chest Wound affliction, which cannot The item can only |
| `#reqnoplant` | - | #mainpath <path> Trees and plants cannot cast this spell. |
| `#reqnoseduce` | - | unforgable item, 13 = unforgable unique artifact, 15 = Units with the seduce abi |
| `#reqnospellsinger` | - | Removes all abilities and stats from the magic item. |
| `#reqnotaskmaster` | - | Level of construction required to forge this item. |
| `#reqplant` | - | All forgeable magic items are removed from the game. |
| `#reqseduce` | - | Always use this command at the end of modifying a magic item. |
| `#reqspellsinger` | - | command. |
| `#reqtaskmaster` | - | Only units with the task master ability can cast this spell. |
| `#restricteditem` | <value> | be healed until the item is removed. |
| `#run` | - | The item is cursed and cannot be dropped. |
| `#secondarylevel` | <path> | the required magic path. |
| `#secondarypath` | <path> | never be cast. |
| `#selectitem` | "<item name>" | \| <item nbr> requirement. |
| `#shockres` | <value> | Enables user of item to cast this spell in battle or as a ritual The item grants |
| `#sneakunit` | <value> | #curse The item will grant stealth to non-stealthy units. |
| `#spell` | "<spell name>" | - |
| `#spr` | "<filename>" | - |
| `#stealthboost` | <value> | #nocoldblood The item will grant a stealth bonus to already stealthy units. |
| `#stoneskin` | - | #autospellrepeat <spells / round> The item automatically applies the Stoneskin s |
| `#swift` | <percent> | #disease Grants extra combat speed to the wielder. |
| `#type` | <nbr> | Magic Item Modding Defines whether the item is 1-handed or 2- handed weapon, a s |
| `#waterbreathing` | - | A heavy item cannot be transported with the help of certain The item grants wate |
| `#weapon` | "<weapon name>" | \| <weapon nbr> The item grants Quickness (double movement, +2 attack & Defines  |

### Mercenary (17 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#bossname` | "<name>" | templates for the same nation, in which case a random one Name of the band's lea |
| `#cleardef` | - | The minimum amount of gold the band can be hired for. |
| `#clearmercs` | - | normal, then press ctrl+shift+s to save a file containing the Removes all mercen |
| `#com` | "<monster name>" | What type of monster the commander is. |
| `#defmult1b` | <multiplier> | Mask Era Number of units per 10 points of defense for the second unit 1 Early er |
| `#defunit1b` | "<monster name>" | \| <monster nbr> A specific magic item for the commander. |
| `#eramask` | <value> | Sets the third type of unit in the poptype PD. |
| `#item` | "<item name>" | - |
| `#level` | <0-2> | Each template will determine how the AI designs its god for Level 0 bands appear |
| `#minmen` | <value> | End modding each poptype with this command. |
| `#minpay` | <gold> | - |
| `#newmerc` | - | and specific research targets for AI players. |
| `#newtemplate` | <nation nbr> | - |
| `#nrunits` | <value> | Number of troops in the band. |
| `#randequip` | <0-3> | Adds a commander to the recruitment list. |
| `#recrate` | <value> | is 20 or higher. |
| `#unit` | "<monster name>" | Always start a new template with this command. |

### Monster (526 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#aciddigest` | <dmg> | This monster gets reduced attack penalty when using two The monster digests any  |
| `#acidres` | <prot> | - |
| `#acidshield` | <damage> | the season of power the monster gets a percentage increase Anyone striking this  |
| `#addrandomage` | <years> | Gives the monster the ability to lure enemy commanders like Makes the monster st |
| `#addupkeep` | <gold> | #adeptsacr <value> Upkeep will be calculated as if the unit cost this much more  |
| `#adeptsacr` | <value> | Upkeep will be calculated as if the unit cost this much more to Adept Sacrificer |
| `#ainorec` | - | The cost in resources. |
| `#airattuned` | - | etc). |
| `#airshield` | <percent> | Combat Auras The monster gains the airshield special ability. |
| `#aisinglerec` | - | Gold & Resource Cost Will tell the AI to only recruit a single one of these per  |
| `#alchemy` | - | #fastcast |
| `#allret` | <chance> | This monster gets a blood magic bonus when searching for Extra chance of returni |
| `#almostliving` | - | beings. |
| `#almostundead` | - | (unless it it is a mage). |
| `#ambidextrous` | - | #xpgain #clumsy |
| `#amphibian` | - | This monster is an undead. |
| `#animal` | - | Indicates that the monster is an animal. |
| `#animalawe` | <bonus> | immortal to reappear the next month. |
| `#animated` | "<monster name>" | \| <monster nbr> The monster will change into the next monster type (next The mo |
| `#ap` | <action points> | has magic resistance above 18, except some beings from the The number of action  |
| `#appetite` | <value> | The monster erupts in a shock explosion when it dies, inflicting A monster with  |
| `#aquatic` | - | retained experience, curses, afflictions, magic etc. |
| `#assassin` | - | This ability is used by certain horrors to make them move on This monster is an  |
| `#assencloc` | <value> | percentage of its total hit points every turn it spends in a land Specifies an e |
| `#astralrange` | <range> | Chance should be a number from 1 to 100. |
| `#autoberserk` | <value> | 1 = will start the combat by going berserk. |
| `#autocompete` | - | #onlyinanim The bearer of this item will automatically compete in the Arena The  |
| `#autocorpsehealer` | <value> | Death magic, inanimate creatures by Earth magic, demons by Gives the Corpse Stit |
| `#autodisgrinder` | <value> | getting afflictions and eventually die. |
| `#autodishealer` | <value> | start age to zero. |
| `#autohealer` | <value> | The monster automatically heals <value> afflictions from units |
| `#autumnshape` | "<monster name>" | \| <monster nbr> province to a water province. |
| `#awe` | <bonus> | #slimer <strength> Bonus can be a value of one or more. |
| `#banefireshield` | - | #acidshield HEALING & DISEASE #damagerev |
| `#batstartsum1d3` | "<monster name>" | \| <monster nbr> summoned. |
| `#battleshape` | "<monster name>" | \| <monster nbr> |
| `#battlesum1` | - | summons 0-1 monsters etc. |
| `#battlesum1d2` | "<monster name>" | \| <monster nbr> Automatically summons 1-2 monsters to the battlefield each Entr |
| `#battlesum1d3` | "<monster name>" | \| <monster nbr> cataclysm has occurred. |
| `#battlesum5` | - | command summons 0-5 and -25 Horror |
| `#battlesumwarm` | "<monster name>" | \| <monster nbr> This command assigns a monster tag value to a creature. |
| `#beartattoo` | <value> | supplies. |
| `#beastmaster` | <bonus> | This monster has an innate ability to command 150 magic All animals under the co |
| `#beckon` | <value> | #addrandomage <years> Gives the monster the ability to lure enemy commanders lik |
| `#berserk` | <bonus> | until dead or until the monster that swallowed them is killed. |
| `#bestowtomount` | - | The item can only be used by immobile beings. |
| `#bird` | - | 17768966 2 hands, bow, head (crown), body, feet, 2 misc Use this for birds, rocs |
| `#blessbers` | - | scales or other conditions. |
| `#blessfly` | - | decrease to Strength, Attack, Defense and Action Points per Will grant the unit  |
| `#blind` | - | the pass connects. |
| `#blink` | - | #statbreak <value> Like #teleport, but only in combat. |
| `#bloodrange` | <range> | The type ranges from 0 (fire gems) to 8 (blood slaves). |
| `#bloodvengeance` | <strength> | the height of each season and will get increased Strength, The monster has the B |
| `#bluntres` | - | #corpseeater <value> The monster takes half damage from blunt weapons. |
| `#boartattoo` | <value> | - |
| `#bodyguard` | <bonus> | This monster has an innate ability to command 50 undead The monster's morale is  |
| `#bonusspells` | <spells per round> | #mastersmith <value> The monster gets the Innate Spellcaster ability and can cas |
| `#bravemount` | <percent> | commands. |
| `#bringeroffortune` | - | #decscale #combatcaster |
| `#bug` | - | #pooramphibian Monsters with this tag are summoned by the Swarm spell on This mo |
| `#bugreform` | <nbr of bugs> | Works like regeneration, but only affects inanimate beings. |
| `#bugshape` | "<monster name>" | \| <monster nbr> |
| `#bugswarmshape` | "<monster name>" | \| <monster nbr> The monster takes half damage from slashing weapons. |
| `#bugswarmuwshape` | "<monster name>" | \| <monster nbr> Lower fatigue reduces the chance of the monster suffering This  |
| `#buguwshape` | "<monster name>" | \| <monster nbr> The monster takes half damage from piercing weapons. |
| `#carcasscollector` | <value> | #tainted <chance> The monster can turn <value> nature gems to <value> death Chan |
| `#castledef` | - | #researchbonus |
| `#champprize` | - | #loseeye This item is given as a reward for winning the championship of The item |
| `#chaospower` | <bonus> | under darkness. |
| `#chaosrec` | - | of not aging each year. |
| `#chaosrecscale` | <value> | The monster will be 50 percent cheaper to recruit when this Chaos scale requirem |
| `#chorusmaster` | - | #crossbreeder <value> The monster is automatically a chorus master and does not  |
| `#chorusslave` | - | from the Blood magic school. |
| `#cleanshape` | - | home province. |
| `#cleararmor` | - | monster number is not used, Dominions will automatically use Removes all armor f |
| `#clearmagic` | - | coded numbers later in the mod may overwrite the number Removes all magic skills |
| `#clearspec` | - | Removes all special abilities from the active monster. |
| `#clearweapons` | - | command to remove the existing weapons Enkidu 16 first. |
| `#clumsy` | - | #berserk SEASONAL POWERS #blessbers |
| `#coldblood` | - | #dungeon Cold blooded like the lizards of C’tis. |
| `#coldpower` | <bonus> | the true essence of beings. |
| `#coldrec` | - | enchantment is active. |
| `#coldrecscale` | <value> | The monster will be 75 gold cheaper to recruit when this Cold scale requirement  |
| `#combatcaster` | - | #fortkill #glamourmanip |
| `#command` | <value> | command will give a defence bonus to the unit as well as Increases leadership va |
| `#commaster` | - | #onebattlespell "<spell name>" \| <nbr> The monster is automatically a communion |
| `#comslave` | - | can modify the spell used by this command though. |
| `#copystats` | <monster nbr> | monster. |
| `#coridermnr` | "<monster name>" | \| <monster nbr> |
| `#corpseeater` | <value> | The monster takes half damage from blunt weapons. |
| `#corpselord` | <nbr> | Creates a sprite that is used when the rider loses his mount. |
| `#corruptor` | <value> | hinder movement. |
| `#crippled` | - | #itemdrawsize <value> The item bearer becomes Crippled. |
| `#crossbreeder` | <value> | The monster is automatically a chorus master and does not The monster is skilled |
| `#curseluckshield` | <penetration bonus> | The monster has increased hit points in spring and lowered hit Grants the Fatewe |
| `#custommagic` | - | Nbr Magic Path mask causes the mod to crash. |
| `#damagerev` | <strength> | The monster has increased hit points in autumn and lowered The monster has the D |
| `#dancenof` | <nbr of sprites> | The number of sprites circling the unit. |
| `#dancenratt` | <attacks> | #norange The number of attacks made per combat round. |
| `#dancesize` | <size> | #seduce The size in percent of normal size, default is 100. |
| `#dancespr` | <flysprite nbr> | #statstorm The look of the sprite circling the unit. |
| `#danceweapon` | "<weapon name>" | \| <weapon nbr> #nomovepen The weapon used for the attack. |
| `#darkpower` | - | #invisible |
| `#darkvision` | <percent> | Gives monster darkvision, lessening penalties for fighting |
| `#deadhp` | <value> | chance of harming the monster when they hit, but when they Number of HP gained p |
| `#deathbanish` | <-11 to -13> | #grandcom <0 or 1> Whoever strikes the killing blow against this monster is This |
| `#deathcurse` | - | #patrolbonus <value> When this monster dies, the unit that strikes the killing b |
| `#deathdisease` | <aoe> | pillage bonus of one which makes them count as one man extra The monster bursts  |
| `#deathfire` | <aoe> | A monster with this ability produces extra supplies. |
| `#deathgrab` | <aoe> | if it was an enemy priest engaged in preaching. |
| `#deathparalyze` | <aoe> | This monster has a bonus when preaching against enemy The monster erupts in a pa |
| `#deathpoison` | <aoe> | negative for extra poor performance. |
| `#deathpower` | - | #twistfate |
| `#deathrec` | <value> | this mechanic. |
| `#deathrecscale` | <value> | - |
| `#deathshock` | <aoe> | - |
| `#deathslime` | <aoe> | icon. |
| `#decayres` | <0 or 1> | Priests of R'lyeh have a value of 50 in this attribute. |
| `#decscale` | - | for The monster will reduce the chance of negative events in his the opposite sc |
| `#defector` | <percent> | gets the turmoil discount back into the treasury when the The monster has a chan |
| `#demon` | - | The monster floats in the air and can cross rivers. |
| `#deserter` | - | , but the desertion chance is increased during less gold to recruit. |
| `#digest` | <dmg> | The monster digests any swallowed creatures. |
| `#diseasecloud` | - | #corruptor #poisoncloud |
| `#diseaseres` | - | #deathfire |
| `#divinebeing` | - | Monsters with this tag are used as doom horrors. |
| `#divineins` | - | The monster can use his own blood equal to this number of There can only be a nu |
| `#djinn` | - | Monster Modding, Special Abilities Use this for djinns with a humanoid body, but |
| `#doheal` | - | resistances because they are intended to be immune to any Unit heals normally de |
| `#domimmortal` | - | Monster is surrounded by a poison cloud. |
| `#dompower` | - | #fearofflood 59 |
| `#domrec` | <dominion> | The monster will be 20 gold cheaper to recruit when this This monster can only b |
| `#domshape` | "<monster name>" | \| <monster nbr> #forcess The monster changes to this shape when inside a friend |
| `#domsummon2` | "<monster name>" | \| <monster nbr> -3 Soulless (requires corpses) Half as effective as #domsummon. |
| `#domsummon20` | "<monster name>" | \| <monster nbr> -5 Random animal A twentieth as effective as #domsummon. |
| `#doomhorror` | - | #divinebeing Monsters with this tag are used as doom horrors. |
| `#douse` | <bonus> | #allret <chance> This monster gets a blood magic bonus when searching for Extra  |
| `#dragonlord` | <nbr> | Here are the commands that are used to pair riders and This monster will receive |
| `#drainimmune` | - | #falsesupply #magicimmune |
| `#drake` | - | #lanceok The monster is a drake and is affected by the Dragon Master This monste |
| `#drawsize` | <value> | - |
| `#dread` | <value> | #sleepaura <area> Like fear, but negated by true sight. |
| `#dungeon` | - | Cold blooded like the lizards of C’tis. |
| `#earthelementals` | <bonus> | Gives a size bonus to summoned earth elementals. |
| `#elegist` | <value> | Magic tattoo like the units of Marverni and Sauromatia have. |
| `#elementgems` | <gems> | Holy magic cast by the monster have increased range. |
| `#elementrange` | <range> | They are of a single random type of elemental gem. |
| `#enchrebate10` | <enchantment number> | Magic penalty for trinity gods when they are not in the same The monster will be |
| `#enchrebate100` | <enchantment number> | #growthrecscale <value> The monster will be 100 gold cheaper to recruit when thi |
| `#enchrebate20` | <enchantment number> | #domrec <dominion> The monster will be 20 gold cheaper to recruit when this This |
| `#enchrebate25p` | <enchantment number> | Death scale requirement for recruitment. |
| `#enchrebate50` | <enchantment number> | #heatrecscale <value> The monster will be 50 gold cheaper to recruit when this H |
| `#enchrebate50p` | <enchantment number> | #chaosrecscale <value> The monster will be 50 percent cheaper to recruit when th |
| `#enchrebate75` | <enchantment number> | #coldrecscale <value> The monster will be 75 gold cheaper to recruit when this C |
| `#entangle` | - | Works like Awe, except that it doesn't work if there is no sun Anyone striking t |
| `#ethereal` | - | possible and gain HP for it equal to the value in #deadhp. |
| `#expertleader` | - | ground. |
| `#expertmagicleader` | - | #beastmaster <bonus> This monster has an innate ability to command 150 magic All |
| `#expertundeadleader` | - | #standard <bonus> This monster has an innate ability to command 150 undead The m |
| `#extralives` | <percent> | province. |
| `#eyeloss` | - | #haltheretic <bonus> Anyone striking this monster may lose an eye. |
| `#eyes` | <nbr of eyes> | Giving a monster 50 in morale makes it mindless and prone to Sets the number of  |
| `#fallpower` | - | #trample |
| `#falsearmy` | <value> | of its total hit points every turn it spends underwater (i. |
| `#falseregen` | <points> | the following 4 commands. |
| `#falsesupply` | <value> | 10 AP fire damage to everyone in the area of effect. |
| `#farsail` | - | #dancenratt <attacks> #norange The number of attacks made per combat round. |
| `#farthronekill` | <part> | increased by <value> per turn, to a maximum of 19. |
| `#fastcast` | - | #mason #magicstudy |
| `#faysummon` | <nbr> | But e. |
| `#fear` | <value> | Anyone striking this monster may be horror marked. |
| `#fearofflood` | <value> | counts as <value> extra monsters when defending a fort from a Morale penalty whe |
| `#female` | - | command can be used multiple times on the same monster for Being female is a min |
| `#fireattuned` | - | - |
| `#fireelementals` | <bonus> | Use this to set the type of co-riders if any. |
| `#firepower` | <bonus> | True sight makes it possible to see through illusions and The monster will get s |
| `#fireshield` | <damage> | Shuten-doji has this ability with area 10. |
| `#firstshape` | "<monster name>" | \| <monster nbr> The monster is more efficient when casting spells the converts  |
| `#fixedname` | "<Name>" | selected monster, so it should be used as the first command Gives a fixed name t |
| `#fixedresearch` | <value> | The monster produces a number of water gems that can be The monster produces thi |
| `#fixforgebonus` | <value> | Gem reduction when forging items. |
| `#float` | - | #demon The monster floats in the air and can cross rivers. |
| `#flying` | - | #autocompete This monster can fly. |
| `#foolscouts` | <value> | Grants Pangaea-like healing powers to the monster The monster creates the false  |
| `#forcess` | - | The monster changes to this shape when inside a friendly Usually being soul slay |
| `#foreignshape` | "<monster name>" | \| <monster nbr> #airattuned etc). |
| `#forestshape` | "<monster name>" | \| <monster nbr> taking damage. |
| `#forestsurvival` | - | #stealthy in order to be meaningful. |
| `#forgebonus` | <percent> | Makes it cheaper to create magic items. |
| `#formationfighter` | - | #allrange |
| `#fortkill` | <chance> | commander) does not obey orders but does something else This monster will automa |
| `#gcost` | <gold> | The cost in gold is also the cost in design points for pretenders. |
| `#gemprod` | <type> <number> | Glamour magic cast by the monster have increased range. |
| `#giftofwater` | <size points> | detecting a standard stealthy unit, while 50 patrolling units are A commander wi |
| `#glamour` | - | aura is <value> + Fire magic squares in size. |
| `#glamourmanip` | <0 or 1> | This monster is a glamour manipulator and counts as a glamour mage when it comes |
| `#glamourrange` | - | #inspirational #bloodrange |
| `#godsite` | "<site name>" | \| <site nbr> as a default choice for nations that belong to the same realm. |
| `#gold` | <gold / month> | #localsun The monster generates gold that is added to the treasury. |
| `#goodleader` | - | voluntarily in order to fight on the ground. |
| `#goodmagicleader` | - | All units under the command of this monster have their morale This monster has a |
| `#goodundeadleader` | - | bodyguard is present during an assassination. |
| `#grandcom` | <0 or 1> | Whoever strikes the killing blow against this monster is This monster can partic |
| `#greaterhorror` | - | This monster is an illusion. |
| `#growhp` | <hit points> | #lich "<monster name>" \| <monster nbr> The monster grows into the previous mons |
| `#growthpower` | <bonus> | a unit with invisibility is sneaking, only patrolling units with The monster wil |
| `#growthrecscale` | <value> | The monster will be 100 gold cheaper to recruit when this Growth scale requireme |
| `#haltheretic` | <bonus> | Anyone striking this monster may lose an eye. |
| `#heal` | - | #foolscouts <value> Grants Pangaea-like healing powers to the monster The monste |
| `#healer` | - | #bloodvengeance |
| `#heat` | <value> | at the start of each new month. |
| `#heatrec` | - | enchantment is active. |
| `#heatrecscale` | <value> | The monster will be 50 gold cheaper to recruit when this Heat scale requirement  |
| `#heretic` | - | #chorusmaster |
| `#holy` | - | cannot become prophet, cannot be charmed and has no upkeep Holy (sacred) troops  |
| `#holycost` | <holy points> | mounts of pretenders. |
| `#holyrange` | - | #taskmaster #elementrange |
| `#homeshape` | "<monster name>" | \| <monster nbr> from secondshape to firstshape. |
| `#homesick` | <percent> | 7 summoning circle (assassin inside circle) This monster takes damage equal to t |
| `#horrordeserter` | <percent> | the province. |
| `#horrormark` | - | #fear <value> Anyone striking this monster may be horror marked. |
| `#horsetattoo` | <value> | - |
| `#hpoverflow` | - | To make a monster more vulnerable to a particular type of The monster's hit poin |
| `#humanoid` | - | Value Item Slots The default bodytype. |
| `#iceforging` | <value> | Monster will get the Undreaming ability giving this bonus to its The monster gen |
| `#icenatprot` | - | #haltheretic |
| `#iceprot` | <prot> | The monster has a chance of starting with a heroic ability, like Armor protectio |
| `#illusion` | - | , all of these commands should be combined with |
| `#immobile` | - | led to combat by a beast master. |
| `#immortal` | - | weapons. |
| `#inanimate` | - | #stormpower attribute, this command is redundant. |
| `#incorporate` | <dmg> | This monster will only receive half att/def bonuses from The monster incorporate |
| `#incprovdef` | <value> | chance in percent that the throne will be destroyed each turn. |
| `#incunrest` | <value> | destroyed each turn. |
| `#indepmove` | <percent> | #assassin This ability is used by certain horrors to make them move on This mons |
| `#indepspells` | <level> | -13=Kokytos Usually independent mages will only know low level research |
| `#indepstay` | <0 - 1> | catch his target unawares and without bodyguards. |
| `#infernoret` | <chance> | Affects the spell casting time for commanders. |
| `#inquisitor` | - | #deathparalyze <aoe> This monster has a bonus when preaching against enemy The m |
| `#insane` | <percent> | Grants acid resistance to the monster. |
| `#insanify` | <percent> | first form as the value. |
| `#inspirational` | - | #bloodrange |
| `#inspiringres` | - | #pillagebonus #divineins |
| `#invisible` | - | #slothpower #unseen |
| `#invulnerable` | <prot> | The monster has invulnerability to non-magical weapons. |
| `#ironarmor` | - | human being should be about 32 pixels high and there should Indicates that the a |
| `#ironskin` | - | The monster has a <percent> chance of casting a random spell The item automatica |
| `#ironvul` | <points> | battlefield around it or enemies that attack it in some The monster will take po |
| `#islance` | - | This monster cannot be polymorphed in combat or by the use This item can only be |
| `#itemcost1` | <bonus> | - |
| `#itemcost2` | <bonus> | The item bearer suffers a Never Healing Wound, which cannot This command makes t |
| `#itemdrawsize` | <value> | The item bearer becomes Crippled. |
| `#itemslots` | <slot value> | - |
| `#ivylord` | <nbr> | This monster will receive extra units when summoning Vine Mounts & Riders Men of |
| `#kokytosret` | - | #taxcollector #infernoret |
| `#labxpshape` | <xp value> | - |
| `#lanceok` | - | The monster is a drake and is affected by the Dragon Master This monster can use |
| `#landdamage` | <percent> | A commander with this ability can use the make plague order. |
| `#landshape` | "<monster name>" | \| <monster nbr> |
| `#latehero` | <min turn> | chance of getting one level higher in Fire or Air magic. |
| `#leper` | <percent> | number of extra blood magic level for this purpose. |
| `#lesserhorror` | - | #nospiritform Monsters with this tag are used as lesser horrors. |
| `#lich` | "<monster name>" | \| <monster nbr> The monster grows into the previous monster once it has this Th |
| `#lizard` | - | 131072 1 feet Four legged beast, but lower. |
| `#localsun` | - | The monster generates gold that is added to the treasury. |
| `#loseeye` | - | This item is given as a reward for winning the championship of The item bearer l |
| `#magicarmor` | - | #spr1 "<imgfile>" Indicates that the armor is magic. |
| `#magicbeing` | - | ground bound monsters. |
| `#magicboost` | <path> <boost> | 3 Earth Gives a boost or reduction to the monster's magic ability for 4 Astral o |
| `#magiccommand` | <value> | and herded into battle. |
| `#magicimmune` | - | The monster produces a number of glamour gems that can be The monster is immune  |
| `#magicpower` | <bonus> | The monster automatically swallows the targets of a successful The monster will  |
| `#magicskill` | <path> <level> | rituals cast by the monster. |
| `#magicstudy` | <bonus> | between worlds and the home of horrors and the Void beings Research bonus depend |
| `#makemonsters1` | - | - |
| `#makemonsters2` | - | to #makemonsters5 can also be -16 Random yazad used to summon more monsters per  |
| `#makemonsters5` | - | can also be -16 Random yazad used to summon more monsters per month. |
| `#makepearls` | <value> | The monster has a <percent> chance of casting a random spell The monster can tur |
| `#mapmove` | <speed> | Weapons & Armor The speed at which the monster travels on the world map. |
| `#mapteleport` | - | #statstorm <value> Like #teleport but only grants teleport ability on the map, n |
| `#mason` | - | nbr can be negative for montags. |
| `#masterrit` | <value> | 8 Blood Pathboost for the purpose of casting rituals. |
| `#mastersmith` | <value> | The monster gets the Innate Spellcaster ability and can cast a The monster's mag |
| `#maxage` | <age> | disease healers should have higher values. |
| `#maxdeadhp` | <HP> | Ethereal monsters can pass through walls during the storming The monster's bonus |
| `#mindcollar` | <dmg> | #siegebonus <value> Unit will take this amount of damage if it breaks in combat. |
| `#mindslime` | - | #reinvigoration #heat |
| `#mindvessel` | <0 or 1> | and infects military units in the province. |
| `#miscshape` | - | apparent on gods as dominion strength and path costs are Use this for strange th |
| `#mobilearcher` | - | #dancespr <flysprite nbr> #statstorm The look of the sprite circling the unit. |
| `#monpresentrec` | "<monster name>" | \| <monster nbr> This monster can only be recruited if a unit of "monster name"  |
| `#montag` | - | 2500 could be set as random monster summons by assigning #summon2 29 |
| `#montagweight` | <weight> | anyway. |
| `#mor` | <morale> | knight or 25 for light cavalry. |
| `#moregrowth` | <-5 - 5> | 4 Far East Alters the minimum/maximum allowed value of this scale when 5 Middle  |
| `#moreluck` | <-5 - 5> | 7 Africa Alters the minimum/maximum allowed value of this scale when 8 India des |
| `#moreorder` | <-5 - 5> | command is not needed and should not be used if you only Alters the minimum/maxi |
| `#mountainsurvival` | - | Gives the monster the ability to seduce like a Nagini. |
| `#mounted` | - | squad. |
| `#mountedhumanoid` | - | 6 2 hands Regarding hit locations this is the same as humanoid, but it 14 3 hand |
| `#mountiscom` | <0 or 1> | #poorleader If this is set to one the mount is also a commander. |
| `#mountmnr` | "<monster name>" | \| <monster nbr> This monster will receive extra units when summoning Lamias Use |
| `#mr` | <magic resistance> | Undead 20 The magic resistance of the monster. |
| `#mrhalf` | - | but will be repaired if there are enough resources in the A successful MR check  |
| `#mute` | - | Miscellaneous The item bearer becomes Mute. |
| `#naga` | - | 786432 2 miscs Snake like lower part and a humanoid upper body. |
| `#nametype` | <name type nbr> | weight). |
| `#neednoteat` | - | This monster doesn’t need any food and doesn’t consume any |
| `#nhwound` | - | #itemcost2 <bonus> The item bearer suffers a Never Healing Wound, which cannot T |
| `#nightmareaura` | <area> | armor piercing fire damage. |
| `#noaging` | <percent> | The wielder of this item has a chance of not aging each year. |
| `#noagingland` | <percent> | #singlebattle Friendly units in the same province as this item have a chance #ch |
| `#nobadevents` | <value> | maximum of -3 / 3. |
| `#nobarding` | - | squad and an additional -1 for every additional squad. |
| `#nofalldmg` | - | modifier of +1 for up to 3 squads and -1 for every additional This rider will no |
| `#nofmounts` | <mounts> | Gets the capture slave ability. |
| `#nofriders` | <riders> | - |
| `#noheal` | - | only contain one type of troop. |
| `#nohof` | - | Unit will change into this monster when made a prophet. |
| `#noitem` | - | monster (up to the limit of three). |
| `#noleader` | - | the mount's armor. |
| `#nomagicleader` | - | Increases undead leadership by this amount. |
| `#nomovepen` | - | The weapon used for the attack. |
| `#norange` | <chance> | reduces the effectiveness of the assassin's Patience. |
| `#noremount` | - | These commands are used to set basic leadership values for The commander will no |
| `#noreqlab` | - | service to a player and when it leaves. |
| `#noreqtemple` | - | wounded to count as having fought. |
| `#noriverpass` | - | The commands in this section deal with stealth, spying and The monster is unable |
| `#noslowrec` | - | #triplegod <type> Removes the slowrec attribute. |
| `#nospiritform` | - | Monsters with this tag are used as lesser horrors. |
| `#notdomshape` | "<monster name>" | \| <monster nbr> that the shape change takes place even if the target got soul T |
| `#nothrowoff` | - | standard for non-elite commanders. |
| `#noundeadleader` | - | can only deploy in the skirmish formation and will attack the This monster canno |
| `#noweapon` | <0 or 1> | The item slots can be tweaked afterwards by using the item Use this on monsters  |
| `#nowish` | - | the province. |
| `#okleader` | - | humans that carry a throne with an emperor in it. |
| `#okmagicleader` | - | command. |
| `#okundeadleader` | - | #bodyguard <bonus> This monster has an innate ability to command 50 undead The m |
| `#older` | <age> | Protects a unit when diseased. |
| `#onebattlespell` | "<spell name>" | \| <nbr> The monster is automatically a communion master and does Monster will a |
| `#onlycoldblood` | - | The itemcost1 command only affects the cost of the first magic The item can only |
| `#onlydemon` | - | The itemcost2 command only affects the cost of the second The item can only be u |
| `#onlyfemale` | - | as well, if any. |
| `#onlyimmobile` | - | #bestowtomount The item can only be used by immobile beings. |
| `#onlyinanim` | - | The bearer of this item will automatically compete in the Arena The item can onl |
| `#onlymounted` | - | This command makes the item bonus percent more expensive The item can only be us |
| `#onlyundead` | - | draw at a smaller size than normal. |
| `#orderrecscale` | <value> | enchantment is active. |
| `#overcharged` | <dmg>
<value> | squares around it. |
| `#ownsmonrec` | "<monster name>" | \| <monster nbr> The cost in recruitment points for the monster. |
| `#pathcost` | <design points> | created after one another and the first one should be used as The cost for a new |
| `#patience` | <value> | of moving each month as long as it is owned by independents. |
| `#patrolbonus` | <value> | When this monster dies, the unit that strikes the killing blow is A value of ten |
| `#pierceres` | - | #buguwshape "<monster name>" \| <monster nbr> The monster takes half damage from |
| `#pillagebonus` | <value> | its killer and anyone nearby with poison (str 2 poison cloud). |
| `#plaguedoctor` | <percent> | #landdamage <percent> A commander with this ability can use the make plague orde |
| `#plainshape` | "<monster name>" | \| <monster nbr> Value can be -1 for a bad result, 0 to disable and 1 for a good |
| `#plant` | - | #spiritform The monster is a plant. |
| `#poisonarmor` | <dmg> | Anyone striking this monster with short weapons will be Immortality poisoned. |
| `#poisoncloud` | - | #poisonskin DAMAGE REDUCTION #poisonarmor |
| `#poisonskin` | <0-500> | This section covers different the different kinds of The monster has a skin that |
| `#polyimmune` | - | #islance This monster cannot be polymorphed in combat or by the use This item ca |
| `#pooramphibian` | - | Monsters with this tag are summoned by the Swarm spell on This monster can trave |
| `#poorleader` | - | If this is set to one the mount is also a commander. |
| `#poormagicleader` | - | an undead or a demon. |
| `#poorundeadleader` | - | A formation fighter is well drilled in using tight formations and This monster h |
| `#popkill` | - | #commaster |
| `#powerofdeath` | - | #dompower #fearofflood 59 |
| `#praise` | <value> | #incscale <scale> The praise ability raises friendly dominion in the province. |
| `#prec` | <precision> | only apply to the monster when it is recruited as a The basic precision of the m |
| `#prophetshape` | "<monster name>" | \| <monster nbr> |
| `#quadruped` | - | 8192 1 head Four legged beasts. |
| `#raiseonkill` | <chance> | The monster will get stat increases or decreases depending on Monster has a chan |
| `#raiseshape` | "<monster name>" | \| <monster nbr> Non-Combat Abilities Changes soulless to another kind of unit f |
| `#randomspell` | <percent> | - |
| `#raredomsummon` | "<monster name>" | \| <monster nbr> -7 Horror* There is a flat 8% chance of summoning one creature  |
| `#reanimator` | <nbr> | can be used up to 3 times on one monster. |
| `#reanimpriest` | - | number values to provide a random summon of a specific A priest with this attrib |
| `#reclimit` | <units / turn> | starts. |
| `#reconst` | <percent> | #bugreform <nbr of bugs> Works like regeneration, but only affects inanimate bei |
| `#recuperation` | - | The item grants the recuperation ability. |
| `#reform` | <chance> | 20 to get a 50% chance. |
| `#reformtime` | - | -2 if you want the #animalawe <bonus> immortal to reappear the next month. |
| `#regainmount` | <0 or 1> | Monster Modding, Leadership & Morale If this is set to one the rider will regain |
| `#regeneration` | <percent> | Elemental resistances function like armor by lessening The monster regenerates l |
| `#reinvigoration` | <points> | main part of the swarm. |
| `#reqlab` | - | Desertion Recruiting the monster requires a lab. |
| `#reqtemple` | - | The monster only fights in one battle and then leaves, like the Recruiting the m |
| `#researchbonus` | - | #siegebonus #slothresearch |
| `#resources` | <value> | The monster will become more powerful every time it dies, up The monster generat |
| `#ressize` | <size> | Harpy, Vaetti, Hoburg 7 Use this command with a size value of 3 to give a flier  |
| `#rpcost` | <recruitment points> | - |
| `#sabbathmaster` | - | #inquisitor #sabbathslave |
| `#sabbathslave` | - | Works just like the previous ability, except that at least three The monster is  |
| `#sailing` | <ship size> <max unit size> | This monster can sneak into enemy provinces. |
| `#scalewalls` | - | COMBAT AURAS |
| `#secondshape` | "<monster name>" | \| <monster nbr> forts than what would otherwise be possible for the nation. |
| `#secondtmpshape` | "<monster name>" | \| <monster nbr> The monster will change into the next monster type (next This m |
| `#seduce` | - | The size in percent of normal size, default is 100. |
| `#selectmonster` | "<monster name>" | \| <monster nbr> The value indicates how many percent larger size the sprite Sel |
| `#shapechance` | <percent> | Monster number can be negative for montag usage. |
| `#shapechange` | "<monster name>" | \| <monster nbr> The monster kills (10 x amount) of population in the province i |
| `#shatteredsoul` | <percent> | maximum of -3 / 3. |
| `#shrinkhp` | <hit points> | The monster changes to this shape when in battle. |
| `#siegebonus` | <value> | Unit will take this amount of damage if it breaks in combat. |
| `#singlebattle` | - | #reqtemple The monster only fights in one battle and then leaves, like the Recru |
| `#size` | <size> | Vanara 14 The size of the monster. |
| `#skilledrider` | <value> | Demons require undead leadership Magic beings require A skilled rider will incre |
| `#slashres` | - | #bugswarmshape "<monster name>" \| <monster nbr> The monster takes half damage f |
| `#slave` | - | possess. |
| `#slaver` | "<monster name>" | \| <monster nbr> #nofmounts <mounts> Gets the capture slave ability. |
| `#slaverbonus` | <modifier> | mounts. |
| `#sleepaura` | <area> | Like fear, but negated by true sight. |
| `#sleepres` | - | MAGIC ABILITIES NON-COMBAT ABILITIES #douse |
| `#slimer` | <strength> | Bonus can be a value of one or more. |
| `#slothpower` | - | #unseen |
| `#slothresearch` | - | #patrolbonus #inspiringres |
| `#slowrec` | - | different rules than autocalc for normal units and commanders. |
| `#smartmount` | <smartness> | - |
| `#snake` | - | 16777216 Head slots can only have crowns Use this for snakes, wyrms and other mo |
| `#snaketattoo` | <value> | mere presence. |
| `#snow` | - | underwater. |
| `#sorcerygems` | <gems> | have increased range. |
| `#sorceryrange` | <range> | They are of a single random type of sorcery gem. |
| `#speciallook` | <value> | modding and general commands that are not related to a This command surround a m |
| `#spellsinger` | - | #voidret <chance> Unit takes 50% longer to cast spells, but at half fatigue cost |
| `#spikes` | - | #doheal |
| `#spiritform` | - | The monster is a plant. |
| `#spiritsight` | - | #stormpower #truesight |
| `#spr1` | - | command as it would then change the looks of both monsters. |
| `#spr2` | "<imgfile>" | The file name of the attack image for the monster. |
| `#spreaddom` | <candles> | Magic tattoo like the units of Marverni and Sauromatia have. |
| `#springimmortal` | - | that animals with a morale of 11 have about 50% chance of The immortal will refo |
| `#springpower` | - | #blessfly |
| `#springshape` | "<monster name>" | \| <monster nbr> The monster changes to this shape when moving from a water Chan |
| `#spy` | - | not exceed the value of this ability. |
| `#standard` | <bonus> | This monster has an innate ability to command 150 undead The monster increases t |
| `#startaff` | <percent> | Grants cold resistance to the monster. |
| `#startage` | <age> | in the same province. |
| `#startheroab` | <percent> | - |
| `#startingaff` | <affliction bitmask> | Grants poison resistance to the monster. |
| `#startitem` | "item name" | \| <item nbr> Creature Type & Status The monster starts with this item if it is  |
| `#startmajoraff` | <percent> | Grants fire resistance to the monster. |
| `#statbreak` | - | #dancenof <nbr of sprites> The number of sprites circling the unit. |
| `#statstorm` | <value> | Like #teleport but only grants teleport ability on the map, not 1+ = Can storm f |
| `#stealthy` | - | in order to be meaningful. |
| `#stonebeing` | - | #stormimmune This monster is a stone being and immune to petrification. |
| `#stormimmune` | - | This monster is a stone being and immune to petrification. |
| `#stormpower` | - | attribute, this command is redundant. |
| `#str` | <strength> | should have 2 or 3 points lower base price while elite units The strength of the |
| `#succubus` | <value> | mountain passes even if it is cold in the provinces the pass Gives the monster t |
| `#summerpower` | <percent> | next few rounds of battle. |
| `#summershape` | "<monster name>" | \| <monster nbr> #watershape "<monster name>" \| <monster nbr> Changes into anot |
| `#summon1` | - | command summons one monster per month and -20 Random bird |
| `#summon2` | - | to #summon5 more according to the number in the -21 Random directional dwarf com |
| `#summon5` | - | more according to the number in the -21 Random directional dwarf command. |
| `#sunawe` | <bonus> | #entangle Works like Awe, except that it doesn't work if there is no sun Anyone  |
| `#superiorleader` | - | Dominions 6 system of seperate stats for rider and mount is Leadership value 200 |
| `#superiormagicleader` | - | All slaves under the command of this monster have their This monster has an inna |
| `#superiorundeadleader` | - | serves a nation that can reanimate undead. |
| `#supplybonus` | - | #drainimmune |
| `#swampsurvival` | - | standard. |
| `#swimming` | - | #uwbug The monster can cross rivers. |
| `#tainted` | <chance> | The monster can turn <value> nature gems to <value> death Chance in percent of b |
| `#taskmaster` | - | #elementrange |
| `#taxcollector` | - | province, even if they are protected by a fort. |
| `#teleport` | - | but only grants teleport ability on the map, not 1+ = Can storm forts even if it |
| `#templetrainer` | "<monster name>" | \| <monster nbr> -10 Random good crossbreed A commander with this ability will b |
| `#thronekill` | <chance> | The value divided by 10 is the amount increased per month. |
| `#tmpairgems` | <gems> | Magic Research The monster produces a number of air gems that can be used in The |
| `#tmpdeathgems` | <gems> | The monster gains <value> research bonus per step of sloth in The monster produc |
| `#tmpearthgems` | <gems> | Makes a commander better or worse at magic research. |
| `#tmpfiregems` | <gems> | monster have increased range. |
| `#tmpglamourgems` | <gems> | - |
| `#tmpwatergems` | <gems> | - |
| `#tolerateund` | - | 200% random chance means +2 levels in the path. |
| `#trample` | - | The monster will get stat increases or decreases depending on This monster can t |
| `#trampswallow` | - | #magicpower <bonus> The monster automatically swallows the targets of a successf |
| `#transformation` | <value> | non-forest province to a forest province. |
| `#triple3mon` | - | Pretender God Commands The trinity god has 3 different monsters. |
| `#triplegod` | <type> | Removes the slowrec attribute. |
| `#triplegodmag` | <penalty> | - |
| `#troglodyte` | - | can only have a limited number of special abilities so don’t Use this for humano |
| `#truesight` | - | #firepower <bonus> True sight makes it possible to see through illusions and The |
| `#twiceborn` | "<monster name>" | \| <monster nbr> |
| `#twistfate` | - | Will start with a twist fate in combat. |
| `#undcommand` | - | #tmpairgems |
| `#undead` | - | #amphibian This monster is an undead. |
| `#undisciplined` | - | Undisciplined monsters cannot be given orders in battle. |
| `#undisleader` | <value> | (getting two or more levels of the same random path from 1=will negate undiscipl |
| `#undregen` | <percent> | example, all undead and inanimate creatures have poison Works like regeneration, |
| `#unify` | - | Any one of the trinity gods can use the unify order to call the other parts of t |
| `#unique` | - | Immobile does not affect map move, use "#mapmove 0" to There can only be one of  |
| `#unmountedspr1` | "<imgfile>" | - |
| `#unmountedspr2` | "<imgfile>" | - |
| `#unseen` | - | #deathpower #twistfate |
| `#unsurr` | - | #magicpower #spiritsight |
| `#unteleportable` | - | its owner and attacks automatically, like the dancing trident. |
| `#userestricteditem` | <value> | feminine names. |
| `#uwbug` | - | The monster can cross rivers. |
| `#uwdamage` | - | 100 so they cannot than there really are. |
| `#uwfireshield` | <damage> | This unit is covered in sharp spikes barbs. |
| `#uwheat` | <0-100> | does not grant stealth to monsters that have no stealth to Like #heat, but also  |
| `#uwregen` | <percent> | myriad of bugs and reform a little while later. |
| `#voidret` | <chance> | Unit takes 50% longer to cast spells, but at half fatigue cost. |
| `#voidsanity` | <value> | The basic encumbrance of the monster. |
| `#warning` | - | #sleepres MAGIC ABILITIES NON-COMBAT ABILITIES #douse |
| `#wastesurvival` | - | value indicates the difficulty of the morale check, 10 is Monster has the Waste  |
| `#waterelementals` | <bonus> | monsters from those five being summoned. |
| `#waterrange` | <range> | Gives a chance for another magic skill to the active monster. |
| `#watershape` | "<monster name>" | \| <monster nbr> Changes into another monster when this season is active. |
| `#winterpower` | <percent> | suffer the damage itself instead of harming the target. |
| `#wintershape` | "<monster name>" | \| <monster nbr> The monster will get this shape if it dies and rises again due  |
| `#wolftattoo` | <value> | calling a god or disciple back from death. |
| `#woodenarmor` | - | image. |
| `#worldshape` | "<monster name>" | \| <monster nbr> many hit points or less. |
| `#woundfend` | <value> | giants have cold resistance 25. |
| `#xpgain` | <percent> | the Cold scale. |
| `#xploss` | <0-100> | of getting to his second shape when the first one gets killes. |
| `#xpshape` | <xp value> | #animated "<monster name>" \| <monster nbr> The monster will change into the nex |
| `#xpshapeloss` | <0-100> | transformations actually taking effect. |
| `#xpshapemon` | "<monster name>" | \| <monster nbr> negative for montags. |
| `#xspr1` | "<imgfile>" | Mount commands Creates a sprite that is used when one corider is lost. |
| `#xspr2` | "<imgfile>" | The smartness determines the chance for the mount to try to Creates the attack s |
| `#yearaging` | <value> | The following commands are exactly like the monster The wielder of this item wil |
| `#yearturn` | <bonus> | with no penalty. |

### Nation (187 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#addforeigncom` | "<monster name>" | \| <monster nbr> #addrecunit "<monster name>" \| <monster nbr> This commander ca |
| `#addforeignunit` | "<monster name>" | \| <monster nbr> The number of start units in the second squad. |
| `#addgod` | "<monster name>" | \| <nbr> type. |
| `#addreccom` | "<monster name>" | \| <monster nbr> Add a unit to the list of recruitable commanders for this natio |
| `#addrecunit` | "<monster name>" | \| <monster nbr> This commander can only be recruited in provinces with no Add a |
| `#aiairnation` | - | Makes the nation less likely to start in one of the terrains in the Hint to AI t |
| `#aiastralnation` | - | Reduces the killing effect of cold scales in the capital by this Hint to AI that |
| `#aiawake` | <percent> | reconsider a non-mage commander recruitment. |
| `#aibloodnation` | - | #moreprod <-5 - 5> Hint to AI that Blood magic is used a lot in this nation and  |
| `#aicheapholy` | - | #moremagic <-5 - 5> Informs the AI that this nation has cheap expendable sacred  |
| `#aideathnation` | - | Reduces the killing effect of heat scales in the capital by this Hint to AI that |
| `#aiearthnation` | - | Reduces the killing effect of cold scales in forts by this many Hint to AI that  |
| `#aifirenation` | - | These commands are used to assign a nation's units, Hint to AI that Fire magic i |
| `#aiglamournation` | - | #moreorder <-5 - 5> Hint to AI that Glamour magic is used a lot in this nation a |
| `#aigoodbless` | <0-100> | - |
| `#aiheavyrec` | <0-99> | nation. |
| `#aiholdgod` | - | #aimagerec <0-99> When playing this nation, the AI will never leave the home Wil |
| `#aiholyranged` | - | AI Hints Informs the AI that this nation has sacred ranged units. |
| `#aimagerec` | <0-99> | When playing this nation, the AI will never leave the home Will make the AI more |
| `#aimusthavemag` | <magic path number> | - |
| `#ainaturenation` | - | Reduces the killing effect of heat scales in forts by this many Hint to AI that  |
| `#airblessbonus` | <0 - 9> | Clears the list of pretender gods for this nation. |
| `#aiwaternation` | - | Kills a percentage of the capital's population when the game Hint to AI that Wat |
| `#astralblessbonus` | <0 - 9> | - |
| `#badindpd` | <0 or 1> | greater than 20. |
| `#blessairshld` | <value> | 12 Marverni, Time of Druids Blessed troops get Air Shield like from an Air bless |
| `#blessatt` | <value> | command. |
| `#blessbonus` | <0 - 9> | Gods of this nation will get extra bless design points. |
| `#blesscoldres` | <value> | 6 Mekone, Brazen Giants Blessed troops get increased Cold Resistance. |
| `#blessdarkvis` | <value> | #selectnation <nation nbr> Blessed troops get Darkvision. |
| `#blessdef` | <value> | for various independents and temporary monsters in the game Blessed troops get i |
| `#blessdtv` | <value> | 19 Ur, The First City Blessed troops get undying, like the undying bless effect. |
| `#blessfireres` | <value> | Nbr Nation Epithet Blessed troops get increased Fire Resistance. |
| `#blesspoisres` | <value> | 10 Fomoria, The Cursed Ones Blessed troops get increased Poison Resistance. |
| `#blessprec` | <value> | Nation numbers, Early Era Blessed troops get increased Precision skill. |
| `#blessreinvig` | <value> | 15 Agartha, Pale Ones Blessed troops get increased Reinvigoration, like from a b |
| `#blessshockres` | <value> | 8 Ermor, New Faith Blessed troops get increased Shock Resistance. |
| `#bloodblessbonus` | <0 - 9> | Nbr Realm Gods of this nation will get extra bless design points of this 1 North |
| `#bloodnation` | - | #moreheat <-5 - 5> Makes the AI more likely to research blood magic and hunt for |
| `#brief` | "<nation name>" | 104 Abysia, Blood of Humans A very brief description for popups. |
| `#buildcoastfort` | <fort nbr> | Dominion is dying and needs blood sacrifice. |
| `#buildfort` | <fort nbr> | spreads. |
| `#builduwfort` | <fort nbr> | Priests of this nation cannot preach. |
| `#castleprod` | <resource boost in percent> | #domwar <value> Resource bonus for forts. |
| `#caveinc` | <bonus> | Bonus in percent to income for all cave forts. |
| `#cavelabcost` | <price> | Gold cost for building a lab in a cave. |
| `#cavenation` | <0-3> | - |
| `#caverecpt` | <bonus> | Palace of Dreams 4 air gems Bonus in percent to recruitment points (for units) f |
| `#caveres` | <bonus> | The Sun Below 4 fire gems Bonus in percent to resources for all cave forts. |
| `#cavetemplecost` | <price> | Fort nbr Fort name Gold cost for building a temple in a cave. |
| `#cheapgod20` | "<monster name>" | \| <monster nbr> |
| `#cheapgod40` | "<monster name>" | \| <monster nbr> Sets wall commander for underwater forts. |
| `#cleargods` | - | #airblessbonus <0 - 9> Clears the list of pretender gods for this nation. |
| `#clearnation` | - | 79 Vanarus, Land of the Chuds Clears away all special settings for the nation, l |
| `#clearrec` | - | #startunittype1 "<monster name>" \| <monster nbr> Clears the list of recruitable |
| `#clearsites` | - | Underwater nation. |
| `#coastcom1` | - | - |
| `#coastnation` | - | #startsite "<site name>" The nation's capital is in a coastal land province. |
| `#coastunit1` | - | - |
| `#color` | <red> <green> <blue> | Library of Time 4 astral pearls Each of the three colors is a number between 0. |
| `#deathblessbonus` | <0 - 9> | included in the nation's default list of pretenders and need not Gods of this na |
| `#defchaos` | <-5 - 5> | not be used for any real nations. |
| `#defcom1` | "<monster name>" | \| <monster nbr> deep and kelp have no fortcom variants, use #uwrec and Commande |
| `#defcom2` | "<monster name>" | \| <monster nbr> #guardcom "<monster name>" \| <monster nbr> Extra commander for |
| `#defdeath` | <-5 - 5> | observer mods and should not be used for any real nations. |
| `#defdrain` | <-5 - 5> | These commands set a nation's starting sites and its terrain Sets the default va |
| `#defmisfortune` | <-5 - 5> | Sites, Terrain, Temperature Sets the default value of the misfortune scale for t |
| `#defmult1` | - | 20 #foreignguardcom "<monster name>" \| <monster nbr> will yield 2 units per poi |
| `#defmult1c` | <multiplier> | Will replace the non-foreign variant for forts that is not the Number of units p |
| `#defmult1d` | <multiplier> | #foreignguardmult <multiplier> Number of units per 10 points of defense for the  |
| `#defmult2` | <multiplier> | never need it. |
| `#defmult2b` | <multiplier> | province except the home province. |
| `#defsloth` | <-5 - 5> | This nation gets to view all battles for all players, including Sets the default |
| `#defunit1` | "<monster name>" | \| <monster nbr> #guardunit "<monster name>" \| <monster nbr> Standard unit for  |
| `#defunit1c` | "<monster name>" | \| <monster nbr> #guardmult <multiplier> Third type of standard unit for local d |
| `#defunit1d` | "<monster name>" | \| <monster nbr> Will replace the non-foreign variant for forts that is not the  |
| `#defunit2` | "<monster name>" | \| <monster nbr> Will replace the non-foreign variant for forts that is not the  |
| `#defunit2b` | "<monster name>" | \| <monster nbr> #foreignwallmult <multiplier> Second type of bonus units for lo |
| `#delgod` | "<monster>" | \| <monster nbr> uwdefunit1c Deletes a god that is otherwise gained through home |
| `#disableoldnations` | - | the terrain mask. |
| `#disbless` | "bless name" | \| <nbr> Disables a bless effect for this nation. |
| `#domdeathsense` | - | Ashen Empire Ermor. |
| `#domkill` | <value> | A death scale does not adversely affect supplies. |
| `#domsail` | - | #tradecoast <income boost in percent> The nation's Dominion enables all units to |
| `#domunrest` | <value> | The nation will only be half as affected by the death/growth The nation's Domini |
| `#domwar` | <value> | 28 Machaka, Lion Kings Dominion conflict bonus. |
| `#dyingdom` | - | #buildcoastfort <fort nbr> Dominion is dying and needs blood sacrifice. |
| `#earthblessbonus` | <0 - 9> | monster must have the #startdom and #pathcost commands Gods of this nation will  |
| `#epithet` | "<nation name>" | 88 Atlantis, Kings of the Deep Sets the epithet of a nation, e. |
| `#era` | <era nbr> | Nation numbers, Late Era Which era should this nation appear in. |
| `#evil` | - | use number 150 and above in order to create new nations The throne is evil and i |
| `#fireblessbonus` | <0 - 9> | These commands set the gods for a nation. |
| `#flag` | "<imgfile>" | Singing Tree 2 glamour gems Replace the flag with an image. |
| `#foreignguardcom` | "<monster name>" | \| <monster nbr> will yield 2 units per point of defense, which is also the defa |
| `#foreignguardmult` | <multiplier> | Number of units per 10 points of defense for the fourth unit Will replace the no |
| `#foreignguardunit` | "<monster name>" | \| <monster nbr> |
| `#foreignwallcom` | "<monster name>" | \| <monster nbr> |
| `#foreignwallmult` | <multiplier> | Second type of bonus units for local defense equal to or greater Will replace th |
| `#foreignwallunit` | "<monster name>" | \| <monster nbr> |
| `#forestlabcost` | <price> | command does not affect what type of forts the nation can Gold cost for building |
| `#forestrec` | "Moose" | - |
| `#foresttemplecost` | <price> | deviate from the standard one for their fortera. |
| `#fortcoldscaleres` | <steps> | #aiearthnation Reduces the killing effect of cold scales in forts by this many H |
| `#fortcost` | <extra cost> | 32 Opulent hall Extra cost is the additional amount of gold the nation must pay  |
| `#fortera` | <0-4> | 14 Mayan Determines what kind of forts the nation can build. |
| `#fortheatscaleres` | <steps> | #ainaturenation Reduces the killing effect of heat scales in forts by this many  |
| `#fortunrest` | <value> | combined with #nopreach and #sacrificedom. |
| `#futuresite` | "<site name>" | Sets the nation's preference for starting in cave provinces. |
| `#godrebirth` | - | #uwwallunit "<monster name>" \| <monster nbr> The nation's god does not lose any |
| `#golemhp` | <percent> | recruiting sacred units). |
| `#guardcom` | "<monster name>" | \| <monster nbr> Extra commander for fortified provinces with a province Command |
| `#guardmult` | <multiplier> | Third type of standard unit for local defense. |
| `#guardspirit` | "<monster name>" | \| <nbr> dominion has influence. |
| `#guardunit` | "<monster name>" | \| <monster nbr> Standard unit for local defense in provinces with forts. |
| `#halfdeathinc` | - | #domunrest <value> The nation will only be half as affected by the death/growth  |
| `#halfdeathpop` | - | unrest reducing Dominions. |
| `#hatesterr` | <terrain mask> | #aiairnation Makes the nation less likely to start in one of the terrains in the |
| `#hero1` | - | - |
| `#hidedom` | <0 or 1> | Berytos has this ability. |
| `#homecoldscaleres` | <steps> | #aiastralnation Reduces the killing effect of cold scales in the capital by this |
| `#homefort` | <fort nbr> | Gold cost for building a temple. |
| `#homeheatscaleres` | <steps> | #aideathnation Reduces the killing effect of heat scales in the capital by this  |
| `#homerealm` | <realm nbr> | Gods of this nation will get extra bless design points of this Any gods that bel |
| `#idealcold` | <cold> | - |
| `#indepflag` | "<imgfile>" | 43 Atlantis, Emergence of the Deep Ones Replace the flag of independents with an |
| `#islandnation` | - | Frozen Fountain 3 water gems The nation prefers to start on an island if possibl |
| `#islandsite` | "<site name>" | wide and 128 pixels high. |
| `#killcappop` | <percent> | #aiwaternation Kills a percentage of the capital's population when the game Hint |
| `#labcost` | <price> | dominion strength. |
| `#landcom` | - | ”<monster name>” \| <monster nbr> plain Add a unit to the list of recruitable co |
| `#landrec` | - | ”<monster name>” \| <monster nbr> #(terrain)fortrec "<monster name>" \| <monster |
| `#likespop` | <poptype> | uwdefunit1d The nation gets cheaper PD from this poptype. |
| `#likesterr` | <terrain mask> | the height. |
| `#maxprison` | <0 - 2> | 6 Middle America Gods of this nation must not be imprisoned to more than this 7  |
| `#merccost` | <percent> | waste Mercenaries are this much more expensive. |
| `#minprison` | <0 - 2> | 3 Mediterranean Gods of this nation must be imprisoned to at least this level. |
| `#moreheat` | <-5 - 5> | Makes the AI more likely to research blood magic and hunt for Alters the minimum |
| `#moremagic` | <-5 - 5> | Informs the AI that this nation has cheap expendable sacred Alters the minimum/m |
| `#moreprod` | <-5 - 5> | Hint to AI that Blood magic is used a lot in this nation and that a Alters the m |
| `#mountlabcost` | <price> | 6 Fortress (underwater era 1) Gold cost for building a lab in highlands. |
| `#mounttemplecost` | <price> | 8 Castle of Bronze and Crystal Gold cost for building a temple in highlands. |
| `#multihero1` | - | deep (deep seas) to set the first of seven possible multiheroes. |
| `#nationinc` | <percent> | angel when in battle. |
| `#natureblessbonus` | <0 - 9> | god, the homerealm of a nation cannot be cleared. |
| `#newnation` | - | 70 Shinuyama, Land of the Bakemono This command can be used to add a new nation  |
| `#nodeathsupply` | - | #domkill <value> A death scale does not adversely affect supplies. |
| `#noforeignrec` | - | #startscout "<monster name>" \| <monster nbr> Nation cannot recruit independent  |
| `#nopreach` | - | #builduwfort <fort nbr> Priests of this nation cannot preach. |
| `#noundeadgods` | - | uwdefmult2 This nation cannot choose any pretender that is undead. |
| `#recallgod` | <value> | 25 Mictlan, Reign of Blood This value is added to the priest level of everyone d |
| `#riverstart` | - | shows some suitable start sites. |
| `#sacrificedom` | - | each month. |
| `#seatrace` | - | scout in the province. |
| `#secondarycolor` | <red> <green> <blue> | Wild Forest 1 nature gem The animated background is made up of two colors, the A |
| `#selectnation` | <nation nbr> | Blessed troops get Darkvision. |
| `#startcom` | "<monster name>" | \| <monster nbr> The number of start units. |
| `#startdom` | - | and #pathcost commands Gods of this nation will get extra bless design points of |
| `#startscout` | "<monster name>" | \| <monster nbr> Nation cannot recruit independent units. |
| `#startsite` | - | , but the nation will only get the site if it starts on bottom of this image and |
| `#startunitnbrs1` | <nbr of units> | - |
| `#startunitnbrs2` | <nbr of units> | - |
| `#startunittype1` | "<monster name>" | \| <monster nbr> Clears the list of recruitable units and commanders (but not Th |
| `#startunittype2` | "<monster name>" | \| <monster nbr> removes all old start troops and must be used when changing The |
| `#summary` | "<nation name>" | 102 Agartha, Ktonian Dead A summary of the benefits and dominion themes of the n |
| `#swamplabcost` | <price> | 2 Fortress (standard era 1) Gold cost for building a lab in a swamp. |
| `#swamptemplecost` | <price> | 4 Citadel (standard era 3) Gold cost for building a temple in a swamp. |
| `#syncretism` | <0 or 1> | percentage value can be negative to make a nation that earn Syncretism enable al |
| `#templecost` | <price> | - |
| `#templegems` | <type> | building a fort. |
| `#templeholypoints` | <value> | Vanheim. |
| `#templepic` | <pic nbr> | 14 Great Walled City Temple should look like this. |
| `#tradecoast` | <income boost in percent> | The nation's Dominion enables all units to sail like the dark Income bonus for c |
| `#uwbuild` | <0 or 1> | - |
| `#uwcom` | "<monster name>" | \| <monster nbr> recruitable in a specific terrain regardless of the presence of |
| `#uwnation` | - | #clearsites Underwater nation. |
| `#uwrec` | "<monster name>" | \| <monster nbr> This unit can be recruited in (terrain) provinces without a for |
| `#uwwallmult` | <multiplier> | Nation can choose this god for 20 design points less. |
| `#uwwallunit` | "<monster name>" | \| <monster nbr> The nation's god does not lose any magic path levels when Sets  |
| `#viewallbat` | - | #defsloth <-5 - 5> This nation gets to view all battles for all players, includi |
| `#viewallprov` | - | Sets the preferred level of cold for the nation. |
| `#wallcom` | "<monster name>" | \| <monster nbr> The following commands works just like their land Commander tha |
| `#wallmult` | <multiplier> | uwdefcom2 Modifier for the number of units from the previous #wallunit uwdefunit |
| `#wallunit` | "<monster name>" | \| <monster nbr> nations, a few land nations that also thrive underwater (like U |
| `#wastelabcost` | <price> | 10 Bramble Fort Gold cost for building a lab in a waste. |
| `#wastetemplecost` | <price> | 12 Walled City Gold cost for building a temple in a waste. |
| `#waterblessbonus` | <0 - 9> | must be removed with the #delgod command. |
| `#wild` | - | 22 T'ien Ch'i, Spring and Autumn The throne is wild and is likely to be defended |

### Poptype (5 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#arenagems` | <amount> | 60 Militia, Lt Inf, Archers About this many fire gems will be awarded as the pri |
| `#arenagold` | <amount> | 57 Shamblers About this much gold will be awarded as the prize for winning 58 Lt |
| `#gemlongevity` | <level> | 63 Tritons This command can be used to make magic gems last for 64 Tritons multi |
| `#selectpoptype` | <poptype> | 71 Trolls A poptype must be selected before using any other poptype 72 Mermen co |
| `#startresearch` | <RP> | 54 Hvy Inf, Hvy Cavalry The amount of start research per magic scale above -3. |

### Site (84 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#adventureruin` | <success chance> | #blesshp <value> A commander who enters the ruin has a chance to discover Blesse |
| `#airrange` | <range boost> | All Air rituals cast in this province have their range increased |
| `#allrange` | <range boost> | All rituals of the Thaumaturgy school cast in this province cost All magic ritua |
| `#altcost` | <bonus> | increased by <range boost> provinces. |
| `#blessanimawe` | <value> | fall victim to lethal traps or bloodthirsty monsters. |
| `#blessawe` | <value> | Constructs a temple in the province when the site is Blessed troops get Awe. |
| `#blesshp` | <value> | A commander who enters the ruin has a chance to discover Blessed troops get incr |
| `#blessmor` | <value> | - |
| `#blessmr` | <value> | lab is already present, there is no effect. |
| `#blessstr` | <value> | a fort is already present, there is no effect. |
| `#bloodcost` | <bonus> | Scale Effects All rituals of the Blood Magic school cast in this province cost T |
| `#chaosscale` | <value> | Set what the site should look like. |
| `#claim` | - | (percent) to become cursed. |
| `#clearfx` | - | The value Unique is special and indicates that there can only Clear all effects  |
| `#clearscales` | - | unique name, because this will lessen the risk of conflicts Clear the scale requ |
| `#cluster` | <value> | command are not active until the throne is claimed by a god, a Assigns a cluster |
| `#coldscale` | <value> | #loc <locmask> The bless requires a cold scale of value or more. |
| `#conjcost` | <bonus> | increased by <range boost> provinces. |
| `#constcost` | <bonus> | province have their range increased by <range boost> All rituals of the Construc |
| `#copysite` | "<site name>" | \| <site nbr> |
| `#cost0` | <value> | Basic Site Modding The minimum skill required in path0 and also the cost in bles |
| `#cost1` | <value> | The minimum skill required in path1 and also added to the cost |
| `#curse` | <chance> | Every turn any unit in the province has the indicated chance #claim (percent) to |
| `#deathrange` | <range boost> | enchantments cast at a discount, the gems between its All Death rituals cast in  |
| `#deathscale` | <value> | certain terrains or flag the site as unique. |
| `#decunrest` | <value> | Adds to castle defenders. |
| `#defcom` | "<monster name>" | \| <monster nbr> |
| `#defmult` | <multiplier> | Adds a monster that can be recruited as commander by the Number of units per 10  |
| `#defunit` | "<monster name>" | \| <monster nbr> |
| `#disease` | <chance> | initiating a number of temple checks per month. |
| `#dominion` | <temple checks per month> | can be a number from 1 to 32000. |
| `#drainscale` | <value> | in any sea. |
| `#earthrange` | <range boost> | All Earth rituals cast in this province have their range increased These command |
| `#enchcost` | <bonus> | All Sorcery magic (Astral, Death, Nature, Blood) rituals cast in All rituals of  |
| `#evocost` | <bonus> | increased by <range boost> provinces. |
| `#firerange` | <range boost> | Increases the selected scale by one point per turn to a All Fire rituals cast in |
| `#fort` | <fort nbr> | Blessed troops get increased Morale. |
| `#gems` | <path> <amount> | #summonlvl4 "<monster name>" \| <monster nbr> Gives gem income to the magic site |
| `#goddomchaos` | <value> | (percent) to become diseased. |
| `#goddomcold` | <value> | the indicated chance (percent) to be struck by holy fire, which Increases the Co |
| `#goddomdeath` | <value> | - |
| `#goddomdrain` | <value> | A commander may enter the site to gain <value> experience Increases the Drain sc |
| `#goddomlazy` | <value> | (percent) to be horror marked. |
| `#goddommisfortune` | <value> | deals 10 points of armor- negating damage if they fail a MR Increases the Misfor |
| `#growthscale` | <value> | Clears the attributes of the selected magic site. |
| `#heatscale` | <value> | This sets the name of the site. |
| `#holyfire` | <chance> | value to increase Production. |
| `#holypower` | <chance> | Increases the Death scale of the god's dominion. |
| `#homecom` | "<monster name>" | \| <monster nbr> Extra commander for province defense. |
| `#homemon` | "<monster name>" | \| <monster nbr> Adds to castle defenders. |
| `#incscale` | - | for the All Water rituals cast in this province have their range opposite scales |
| `#lab` | - | is augmented. |
| `#loc` | <locmask> | The bless requires a cold scale of value or more. |
| `#look` | <site spr> | - |
| `#luckscale` | <value> | Copies all site stats from this site, including name. |
| `#magicscale` | <value> | The magic path associated with this site. |
| `#misfortscale` | <value> | Useful numbers are 735 for a site that can be located in any The bless requires  |
| `#mon` | "<monster name>" | \| <monster nbr> Extra units for province defense. |
| `#nat` | <nation nbr> | Sets the nation for the following two commands. |
| `#natcom` | "<monster name>" | \| <monster nbr> Mask Terrain Adds a monster that can be recruited as commander  |
| `#natmon` | "<monster name>" | \| <monster nbr> Adds a monster that can be recruited if the site is owned by th |
| `#naturerange` | <range boost> | All Nature rituals cast in this province have their range |
| `#newsite` | - | [<site nbr>] in bless points. |
| `#orderscale` | <value> | The bless requires an order scale of value or more. |
| `#path0` | <path nbr> | use up all of the attribute slots. |
| `#path1` | <path nbr> | modding commands. |
| `#popgrowth` | <per mille> | - |
| `#prodscale` | <value> | Always use this command at the end of modifying a site. |
| `#rarity` | <rarity> | #wallunit "<monster name>" \| <monster nbr> Rarity should be 0 for common, 1 for |
| `#res` | <amount> | Void creatures. |
| `#scry` | <duration> | been discovered yet. |
| `#scryrange` | <range> | 2 Cold Heat Set the maximum range of the scry ability. |
| `#selectsite` | "<site name>" | \| <site nbr> points. |
| `#slothscale` | <value> | last ones will look the same). |
| `#summon` | "<monster name>" | \| <monster nbr> 4 Mountain A mage of the same magic path as the site may enter  |
| `#summonlvl2` | "<monster name>" | \| <monster nbr> 128 Swamp Like #summon, except that the mage summoning must be  |
| `#summonlvl3` | "<monster name>" | \| <monster nbr> 16384 unique Like #summon, except that the mage summoning must  |
| `#summonlvl4` | "<monster name>" | \| <monster nbr> Gives gem income to the magic site. |
| `#supply` | <value> | Adds to castle defenders. |
| `#temple` | - | #blessawe <value> Constructs a temple in the province when the site is Blessed t |
| `#thaucost` | <bonus> | - |
| `#uwwallcom` | "<monster name>" | \| <monster nbr> |
| `#voidgate` | <success chance> | Adds a gold income to the site. |
| `#xp` | <value> | #goddomdrain <value> A commander may enter the site to gain <value> experience I |

### Spell (93 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#autoundead` | - | All researchable spells are removed from the game. |
| `#casttime` | <1-1000> | 13 Poison immunity negates Set the casting time of a spell, the casting time is  |
| `#clear` | - | Reanimating priests can reanimate soulless of C'tis, longdead Clears the current |
| `#clearallspells` | - | reanimate various types of undead. |
| `#copyspell` | "<spell name>" | \| <spell nbr> Tombs C'tis has this attribute. |
| `#cure` | "text" | afflictions healed. |
| `#damage` | <dmg> | Ritual spell effects Set the damage for this spell. |
| `#damagemon` | "<monster name>" | 10037 Farsummon Mon nbr Use this to set a spell's damage to a monster if you don |
| `#details` | "<text>" | Therodos. |
| `#dishe` | - | - |
| `#dishim` | - | ## Global Enchantments ##godhimself## Global enchantments have some special sett |
| `#dishimself` | - | ## manipulated with the following commands. |
| `#dishis` | - | ## (value)*(size difference after twiceborn). |
| `#disname` | - | ## and other battlefield wide spells cannot be cast indoors by ##fullplayername# |
| `#disnat` | - | - |
| `#dispimmune` | <0 - 2> | This command will make an enchantment immune to dispels. |
| `#effect` | <eff> | 10163 Create Nexus gate 1 Set the effect of the spell. |
| `#farsumcom` | "<monster name>" | \| <monster nbr> 9 Cold immunity negates Sets the commander for farsummoned unit |
| `#fatiguecost` | <fat> | 24 Holy damage (x3 vs undead and demons) Set the fatigue cost for this spell. |
| `#flightspr` | <flysprite nbr> | #nogeosrc <terrain mask>__ Spell or ritual cannot be cast Set the sprite or part |
| `#friendlyench` | <0 - 1> | 24 MR roll negates (easy) 1 means the enchantment created by this spell is frien |
| `#fullgodname` | - | ## 1 = cannot be cast indoors, -1 = can be cast indoors, even it it ##godname##  |
| `#fullplayergodname` | - | ## adjusted by (value)*(size difference). |
| `#fullplayername` | - | ## default. |
| `#ghostreanim` | - | spell detail text under the main spell description. |
| `#globallook` | <1 - 9> | a disciple or a god. |
| `#godhe` | - | ## zero. |
| `#godhim` | - | ## ##dishim## Global Enchantments ##godhimself## Global enchantments have some s |
| `#godhimself` | - | ## Global enchantments have some special settings that can be ##dishimself## man |
| `#godhis` | - | ## Units that are not human sized after being transformed by the ##godHis## twic |
| `#godname` | - | ## usually should not be castable there. |
| `#godnat` | - | ## the province where it was cast. |
| `#godpathspell` | <-1 - 7> | 19 Does not affect undeads This is used for divine spells that should only be av |
| `#godthrone` | - | ## ##playerthrone## |
| `#greekreanim` | - | A text description of the spell. |
| `#hiddenench` | <0 - 1> | 29 Inanimates are immune 1 means the enchantment created by this spell is not vi |
| `#horsereanim` | - | 1 Alteration Reanimating priests with holy magic of level 3 or higher can 2 Evoc |
| `#localglobal` | <0 - 1> | ##playergodthrone## This is a localized global enchantment. |
| `#makecrater` | <0 or 1> | 1 = ritual range cannot trace through water provinces. |
| `#manikinreanim` | - | current spell. |
| `#maxbounces` | <bounces> | Values for globallook Set the maximum number of bounces for a chain lightning 1  |
| `#napbreakrit` | <value> | that is sent to all players. |
| `#newspell` | - | Creates a new spell and selects it for modding by the following |
| `#nextingeo` | <terrain mask> | 10094 Beckoning 999 The spell after this will also take effect if it cast in thi |
| `#nextspell` | "<spell name>" | \| <nbr> 10044 Transform 1 With this command the effect of another spell will al |
| `#nocastmindless` | <0 - 1> | 41 Acid immunity negates 1 means mindless units cannot cast this spell. |
| `#nogeodst` | <terrain mask> | explosion. |
| `#nogeosrc` | <terrain mask> | __ Spell or ritual cannot be cast Set the sprite or particle effect to be used w |
| `#nolandtrace` | <0 or 1> | Spell Range & Targeting 1 = ritual range cannot trace over land. |
| `#notfornation` | <nation nbr> | \| "nation name" 4 Non-magic beings immune Restricts a spell so that it cannot b |
| `#notindoors` | <-1 to 1> | ##fullgodname## 1 = cannot be cast indoors, -1 = can be cast indoors, even it it |
| `#notmnr` | "<monster name>" | \| <monster nbr> 50 Does not affect true sight This type of monster is unable to |
| `#nowatertrace` | <0 or 1> | - |
| `#nreff` | <nbr of effects> | Terrain masks Sets the number of effects for this spell. |
| `#onlyatsite` | <"site name"> | \| <site nbr> The sample that will sound when this spell strikes down. |
| `#onlycoastsrc` | <0 - 1> | effects". |
| `#onlyfriendlydst` | <0 - 2> | sample must be in . |
| `#onlygeodst` | <terrain mask> | "Flysprites". |
| `#onlygeosrc` | <terrain mask> | range need not be walkable (default) Spell or ritual can only be cast from one o |
| `#onlymnr` | "<monster name>" | \| <monster nbr> 46 Does not affect flying/floating Only this type of monster is |
| `#onlyowndst` | <0 or 1> | - |
| `#onlysitedst` | <"site name"> | \| <site nbr> table "Sound effects". |
| `#path` | <reqnr> <path nbr> | The path of the spell. |
| `#pathlevel` | <reqnr> <level> | reserved and numbers 1300 – 3999 can be used for Level required to cast this spe |
| `#playergodname` | - | ## Units that are not human sized will have the cost of the ritual ##fullplayerg |
| `#playergodthrone` | - | ## This is a localized global enchantment. |
| `#playername` | - | - |
| `#playerthrone` | - | - |
| `#polygetmagic` | <0 - 1> | 55 Internal damage 1 means a unit polymorphed by this ritual will get the magic  |
| `#portent` | "text" | This command works for both local and global enchentments. |
| `#precision` | <prec> | 10 1024 Many Sites Set the precision for this spell. |
| `#priestreanim` | - | commands. |
| `#provrange` | <range> | - |
| `#range` | <range> | 6 64 Waste Sets the battlefield range for this spell. |
| `#reqsun` | <0 - 1> | 6 Blood Vortex This command can make a combat spell uncastable when there 7 Wind |
| `#researchlevel` | <level> | monkey undead are hard coded for their respective nations Level of research requ |
| `#restricted` | <nation nbr> | \| "nation name" |
| `#school` | <school nbr> | Lemuria. |
| `#selectspell` | "<spell name>" | \| <nbr> Dominion, not priest levels or actions by priest Selects an existing sp |
| `#sethome` | - | 3 Well of Misery The commander casting this ritual will get his home province 4  |
| `#sizecost` | <value> | ##playergodname## Units that are not human sized will have the cost of the ritua |
| `#spec` | <spec bitmask> | Spell will only be available for this nation. |
| `#speedmult` | <1 - 3> | 1 = can only target own provinces. |
| `#spellreqfly` | <0 - 1> | 43 Morale roll negates 1 means only flying units can cast this ritual. |
| `#strikesound` | <sample nbr> | #onlyatsite <"site name"> \| <site nbr> The sample that will sound when this spe |
| `#sumhealaffs` | <value> | things like the name of the caster's god. |
| `#supayareanim` | - | also remove its description, so make sure to set name before Reanimating priest  |
| `#tombwyrmreanim` | - | #clear Reanimating priests can reanimate soulless of C'tis, longdead Clears the  |
| `#twiceborncost` | <value> | ##godhis## Units that are not human sized after being transformed by the ##godHi |
| `#undeadreanim` | - | Nbr School All undead priests of this nation are able to reanimate the dead – 1  |
| `#walkable` | <0 or 1> | Sets the range of a ritual in provinces. |
| `#wightreanim` | - | 5 Thaumaturgy Reanimating priests with holy magic of level 4 or higher can 6 Blo |
| `#worldvisible` | <0 - 1> | This enchantment can be seen from all over the world. |

### Unknown (57 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#addname` | "name" | 147 Rus female Adds a name to the selected nametype. |
| `#airelementals` | - | The amount of research bonus received per mage from a magic |
| `#batstartsum1` | - | The effect sloth and productivity has on income. |
| `#batstartsum1d6` | - | The effect cold and heat have on income. |
| `#batstartsum2` | - | #slothresources <percent> |
| `#batstartsum2d6` | - | #coldsupply <percent> |
| `#batstartsum3` | - | The effect sloth and productivity have on resources. |
| `#batstartsum3d6` | - | The effect cold and heat has on supplies. |
| `#batstartsum4` | - | - |
| `#batstartsum4d6` | - | #tempscalecap <value> |
| `#batstartsum5` | - | #coldincome <percent> |
| `#batstartsum5d6` | - | Changing any scale more than this does not yield extra design |
| `#battlesum2` | - | The effect death and growth has on income. |
| `#battlesum3` | - | #deathsupply <percent> |
| `#battlesum4` | - | The effect death and growth has on supplies. |
| `#coldincome` | <percent> | - |
| `#coldsupply` | <percent> | - |
| `#deathdeath` | <0.01 percent> | - |
| `#deathincome` | <percent> | - |
| `#deathsupply` | <percent> | - |
| `#description` | "<piece of text>" | Sound samples A description of what the mod does, who has created it and so fort |
| `#domsummon` | - | A multiplier for the amount of supplies found in a land. |
| `#domversion` | - | commands 4=no random pitch, 5=extra random pitch. |
| `#eventisrare` | <percent> | - |
| `#icon` | "mod_subdirectory/mod_icon.tga" | spells and magic items as well as magic sites, population types and the effects  |
| `#lamialord` | - | How (mis)fortune affects the possibility of an event being good. |
| `#loop` | <sample nbr> | This should be -1 for sound effects, -2 for music, -4 for battle |
| `#luckevents` | <percent> | - |
| `#makemonsters3` | - | The effect turmoil and order has on income. |
| `#makemonsters4` | - | #turmoilevents <percent> |
| `#minsizeleader` | <0 - 6> | 135 Amazon 136 Sauromatia Name Modding 137 Marverni male 138 Marverni female |
| `#misfortune` | <percent> | - |
| `#newmonster` | - | command (including units already bought in-game) will be instantly at the end, b |
| `#onisummon` | - | #luckevents <percent> |
| `#poppergold` | <people> | - |
| `#researchscale` | <bonus> | - |
| `#resourcemult` | <percent> | - |
| `#sample` | "<filename>" | that plays nicely with ongoing games. |
| `#selectbless` | "<bless name>" | \| <nbr> 119 Misc female Selects an existing blessing that will be affected by t |
| `#selectnametype` | <nametype nbr> | 139 Angels Selects the nametype that will be affected by the following 140 Demon |
| `#selectsound` | <nbr> | 87 Whip Selects an existing or a new sample (sound effect or 89 Explosion backgr |
| `#skirmisher` | <-25 - 25> | 134 Japanese male |
| `#slothincome` | <percent> | - |
| `#slothresources` | <percent> | - |
| `#smpmode` | <mode> | Mod Info Mode should be 0 for standard sound effect and 2 for music. |
| `#summon3` | - | #eventisrare <percent> |
| `#summon4` | - | Random events are divided into two categories, common and |
| `#supplymult` | <percent> | - |
| `#tempscalecap` | <value> | - |
| `#tmpastralgems` | - | #tmpdeathgems #poppergold <people> |
| `#tmpbloodslaves` | - | #resourcemult <percent> |
| `#tmpnaturegems` | - | The amount of people required for one gold in taxes. |
| `#turmoilevents` | <percent> | - |
| `#turmoilincome` | <percent> | - |
| `#unresthalfinc` | <unrest level> | - |
| `#unresthalfres` | <unrest level> | - |
| `#version` | - | - |

### Weapon (103 commands)

| Command | Arguments | Description |
|---------|-----------|-------------|
| `#acid` | - | The effects of the weapon may be resisted by MR, but there is a This weapon does |
| `#afflictions` | - | - |
| `#aftercloudarea` | <aoe> | #name "<name>" Size of the aftercloud effect of this weapon. |
| `#ammo` | - | value of 0 and a sword has 1. |
| `#aoe` | <squares> | 765 Frozen Flames This is the area of effect in squares. |
| `#armornegating` | - | #demonundead The weapon is armor negating. |
| `#armorpiercing` | - | The weapon only affects sacred troops. |
| `#att` | <attack> | This command also turns the weapon into a missile weapon Sets the attack value o |
| `#beam` | - | secondary effect itself has an area of effect of one or greater. |
| `#blunt` | - | Mindless beings are immune to this weapon. |
| `#bonus` | - | #iceweapon This is an intrinsic weapon that will not incur a multiple weapon The |
| `#bowstr` | - | #def 0 This is the same as the #thirdstr command. |
| `#charge` | - | a magic weapon item. |
| `#cold` | - | #inanimateimmune This weapon does cold damage. |
| `#copyweapon` | "<weapon name>" | \| <weapon nbr> 22 Mind blast Copies all stats from an existing weapon. |
| `#danceweapon` | "<weapon name>" | \| <weapon nbr> #nomovepen The weapon used for the attack. |
| `#def` | - | 0 This is the same as the #thirdstr command. |
| `#defroll` | - | 216 Fire (AP 8) If the victim passes a defence roll vs 3d6, there will be no 221 |
| `#demononly` | - | when determining damage. |
| `#demonundead` | - | The weapon is armor negating. |
| `#dmg` | - | command, so the weapon's damage value is The weapon does double damage against c |
| `#dt_aff` | - | command that was used for bows in Dominions 5. |
| `#dt_bouncekill` | - | 50 caught in net This effect will bounce a few times to nearby targets, like a 5 |
| `#dt_cap` | - | 7 rage Sets the damage type to capped damage (max 1 HP damage) 8 decay like a wh |
| `#dt_constructonly` | - | 0 disease Only inanimate beings are affected by this weapon. |
| `#dt_demon` | - | #dt_aff Sets the damage type to anti-demon damage. |
| `#dt_drain` | - | 17 false fetters The weapon drains life force from its target, healing damage 18 |
| `#dt_holy` | - | 13 bleed Sets the damage type to holy damage. |
| `#dt_interrupt` | - | 36 slowed This damage is not real, but it can still interrupt mages casting 41 r |
| `#dt_large` | - | Affliction numbers The weapon does triple damage against creatures larger than t |
| `#dt_magic` | - | powers of 2. |
| `#dt_normal` | - | 10 Club, axe Sets the damage type to normal damage. |
| `#dt_paralyze` | - | you must enter the value of 2^3 as an argument to #dmg in Sets the damage type t |
| `#dt_poison` | - | 12 Spear, pike Sets the damage delivery mechanism to poison damage. |
| `#dt_raise` | - | 3 plague If the target is killed by the weapon, it is animated as a soulless 5 c |
| `#dt_realstun` | - | 29 webbed Sets the damage type to stun. |
| `#dt_sizestun` | - | 20 weakness Sets the damage type to fatigue damage that is less effective on 21  |
| `#dt_small` | - | the #dmg command, so the weapon's damage value is The weapon does double damage  |
| `#dt_stun` | - | 27 slime Sets the damage type to fatigue damage. |
| `#dt_weakness` | - | 10 asleep The weapon drains strength from its target instead of doing 11 rusty a |
| `#dt_weapondrain` | - | 23 chest wound The weapon drains life, but max 5 points of the damage is used 24 |
| `#enemyimmune` | - | reduced damage from this weapon. |
| `#explspr` | <fx nbr> | 313 4 Sticky goo Use this command to set how the explosion looks like when a 339 |
| `#false` | - | 137 Entanglement The weapon causes false damage (like most glamour magic) 143 Di |
| `#fire` | - | Flying and floating beings are immune to this weapon. |
| `#flail` | - | This secondary effect will affect anyone harmed by the The weapon has a +2 attac |
| `#flyingimmune` | - | #fire Flying and floating beings are immune to this weapon. |
| `#flyspr` | <flysprite nbr> <animation lgth> | secret and don't show it in weapon info). |
| `#friendlyimmune` | - | This weapon does shock damage. |
| `#fullstr` | - | enemies, only doing damage to specific types of creatures, The full weapon wield |
| `#halfstr` | - | Only one half of the weapon wielder's strength will be added to These commands a |
| `#hardmrneg` | - | #acid The effects of the weapon may be resisted by MR, but there is a This weapo |
| `#holyifhit` | <dmg> | Flysprite Anim lgth Looks like AN damage that only affect demons and undeads. |
| `#holystunifhit` | <dmg> | 137 4 Frost swirl Stun that only affects demons and undead. |
| `#iceweapon` | - | This is an intrinsic weapon that will not incur a multiple weapon The weapon is  |
| `#illusionsimmune` | - | 104 Area Petrification Illusions are immune to this weapon. |
| `#inanimateimmune` | - | This weapon does cold damage. |
| `#internal` | - | 50 Weak Poison The weapon inflicts internal damage that cannot be negated by 51  |
| `#ironweapon` | - | or more indicates the number of squares that will be affected. |
| `#killdemonifhit` | <dmg> | 111 1 Sling stone AN damage that only affect demons. |
| `#killmagicifhit` | <dmg> | 109 1 Arrow AN damage that only affect magic beings. |
| `#len` | <length> | value is 0, a spear costs 1 resource and a sword costs 3. |
| `#magic` | - | The weapon only affects undead. |
| `#magiconly` | - | The weapon only affects magic beings. |
| `#melee50` | - | variant with area effects. |
| `#mind` | - | #blunt Mindless beings are immune to this weapon. |
| `#morroll` | - | 232 Shock (AN 10) If the victim passes a morale roll vs 3d6, there will be no ef |
| `#mrhalf` | - | but will be repaired if there are enough resources in the A successful MR check  |
| `#mrnegates` | - | #poison The effects of the weapon can be resisted by MR. |
| `#mrnegateseasily` | - | example, Poison Sling and Snake Bladder Stick both have this The effects of the  |
| `#name` | "<name>" | 16 Fire flare This must be the first command for every new weapon. |
| `#natural` | - | damage from strength. |
| `#newweapon` | <weapon nbr> | fired in combat. |
| `#nomovepen` | - | The weapon used for the attack. |
| `#norepel` | - | This weapon cannot be used to repel attacks. |
| `#nostr` | - | #name "Plague Stick" The strength of the weapon wielder will not be added to the |
| `#notdismounted` | - | weapon. |
| `#notmounted` | <1 - 2> | This weapon cannot be used while mounted (2=keep this |
| `#nouw` | - | 10108 Cold Blast This ranged weapon cannot be used underwater. |
| `#nratt` | <nbr of attacks> | 87 Whip Sets the number of attacks per round for a weapon. |
| `#petrifyifhit` | <dmg> | 274 4 Bane fire arrow Petrification. |
| `#pierce` | - | and 2 units never resist the effect. |
| `#poison` | - | The effects of the weapon can be resisted by MR. |
| `#range0` | - | some premade effects. |
| `#range050` | - | activate when the target is hit, even if actual damage is not This ranged weapon |
| `#sacredonly` | - | #armorpiercing The weapon only affects sacred troops. |
| `#secondaryeffect` | "<weapon name>" | \| <weapon nbr> #flail This secondary effect will affect anyone harmed by the Th |
| `#secondaryeffectalways` | "<weapon name>" | \| <weapon nbr> #unrepel This secondary effect will affect anyone attacked by th |
| `#selectweapon` | "<weapon name>" | \| <weapon nbr> 55 Hoof Selects the weapon that will be affected by the followin |
| `#shock` | - | #friendlyimmune This weapon does shock damage. |
| `#sizeresist` | - | The weapon does slashing damage. |
| `#skip` | - | weapons. |
| `#skip2` | - | Secondary weapon effects Once this weapon is used, skip the next 2 weapons. |
| `#slash` | - | #sizeresist The weapon does slashing damage. |
| `#sound` | <sample nbr> | things like breath weapons or mind blasts. |
| `#spiritformimmune` | - | 54 Paralyzing Poison Spiritform beings and illusions are immune to this weapon. |
| `#thirdstr` | - | #end Only one third of the weapon wielder's strength will be added to the damage |
| `#twohanded` | - | be in . |
| `#undeadimmune` | - | Resistance only take half damage. |
| `#undeadonly` | - | #magic The weapon only affects undead. |
| `#unrepel` | - | This secondary effect will affect anyone attacked by the Attacks with this weapo |
| `#uwok` | - | 10041-10069 Rising mists This ranged weapon can be used underwater and is not 10 |
| `#woodenweapon` | - | The weapon is made of wood. |
