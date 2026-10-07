#modname "Edit test base: every type"
#description "Base mod for the per-type scripted-edit tests (Dom5Tests edit + tools/fidelity stage 4): every entity type the oracle compares, new and selected, copies, references across types, mounted units."

-- ===== Weapons: a new one, a copy of it, a vanilla one the mod changes =====
#newweapon 1200
#name "Test Blade"
#dmg 7
#att 1
#def 1
#len 1
#nratt 1
#slash
#end

-- copies the blade and sets its own damage
#newweapon 1201
#copyweapon 1200
#name "Test Blade Copy"
#dmg 9
#end

#selectweapon 9 -- Dagger
#att 2
#end

-- ===== Armor =====
#newarmor 450
#name "Test Plate"
#type 5
#prot 14
#def -1
#enc 3
#rcost 12
#end

#newarmor 451
#copyarmor 450
#name "Test Plate Copy"
#enc 2
#end

#selectarmor 5 -- Leather Cuirass
#rcost 6
#end

-- ===== Monsters =====
-- a mount, a co-rider, a rider using both, a copy of the rider, a second mount
#newmonster 5100
#name "Test Mount"
#hp 20
#size 4
#att 10
#def 8
#prot 4
#mr 5
#mor 10
#str 14
#enc 2
#prec 5
#mapmove 20
#ap 16
#weapon 20 -- Bite
#armor 253 -- Cataphract Barding
#animal
#end

#newmonster 5101
#name "Test Corider"
#hp 9
#size 2
#att 9
#def 9
#mr 10
#mor 10
#str 9
#prec 11
#mapmove 18
#ap 12
#weapon 9 -- Dagger
#end

#newmonster 5102
#name "Test Rider"
#hp 11
#size 3
#att 11
#def 10
#prot 2
#mr 10
#mor 12
#str 11
#enc 3
#prec 10
#mapmove 18
#ap 12
#weapon 1200 -- Test Blade
#weapon 357 -- Light Lance
#armor 450 -- Test Plate
#armor 2 -- Shield
#mountmnr 5100
#coridermnr 5101
#skilledrider 1
#swampsurvival
#magicskill 0 1
#end

-- a copy of the rider: inherits mount, co-rider, weapons and magic
#newmonster 5103
#copystats 5102
#name "Test Rider Copy"
#mor 14
#end

-- a second mount, made from a vanilla one
#newmonster 5104
#copystats 3536 -- Armored Serpent
#name "Second Mount"
#hp 30
#end

-- infantry and a commander, recruited, summoned and hired below
#newmonster 5105
#name "Test Soldier"
#hp 10
#size 2
#att 10
#def 10
#prot 5
#mr 10
#mor 10
#str 10
#enc 3
#prec 10
#mapmove 12
#ap 12
#weapon 1 -- Spear
#armor 5 -- Leather Cuirass
#end

#newmonster 5106
#copystats 5105
#name "Test Captain"
#okleader
#mor 12
#weapon 8 -- Broad Sword
#end

-- vanilla cavalry the mod changes (Serpent Cataphract: mount 3536 Armored Serpent)
#selectmonster 3
#hp 13
#end

-- ===== Items =====
#selectitem 700
#name "Test Sword"
#type 1
#constlevel 2
#mainpath 0
#mainlevel 1
#weapon 1200 -- Test Blade
#end

#selectitem 701
#copyitem 700
#name "Test Sword Copy"
#mainlevel 2
#end

#selectitem 702
#name "Test Mail"
#type 6
#constlevel 4
#mainpath 3
#mainlevel 1
#armor 450 -- Test Plate
#end

#selectitem 1 -- Fire Sword
#constlevel 2
#end

-- ===== Spells =====
#selectspell 2000
#name "Test Summoning"
#school 0
#researchlevel 3
#path 0 6
#pathlevel 0 2
#fatiguecost 300
#effect 10001
#damage 5105 -- Test Soldier
#nreff 2
#end

#selectspell 2001
#copyspell 2000
#name "Test Summoning Copy"
#damage 5106 -- Test Captain
#end

#selectspell 12 -- Court of Flame Childs
#researchlevel 2
#end

-- ===== Sites =====
#newsite 1800
#name "Test Site"
#path 6
#level 1
#rarity 2
#gems 6 2
#homemon 5105
#homecom 5106
#mon 5105
#gold 50
#end

#newsite 1801
#copysite 1800
#name "Test Site Copy"
#rarity 1
#end

#selectsite 1 -- The Smouldercone
#gold 25
#end

-- ===== Nations =====
#selectnation 160
#name "Testland"
#epithet "Kingdom of Tests"
#era 2
#addrecunit 5105
#addrecunit 5102
#addreccom 5106
#startcom 5106
#startunittype1 5105
#startunitnbrs1 10
#hero1 5106
#addgod 250 -- Frost Father
#startsite "Test Site"
#homerealm 3
#end

#selectnation 5 -- Arcoscephale
#addrecunit 5105
#end

-- ===== A mercenary band (no number; the oracle takes only one #newmerc without one: it numbers
-- it "" and rejects a second as "id already in use") =====
#newmerc
#name "Test Band"
#bossname "Captain Test"
#com 5106
#unit 5105
#nrunits 20
#level 1
#minmen 5
#minpay 100
#xp 10
#randequip 1
#recrate 50
#eramask 7
#item 1 -- Fire Sword
#end

-- ===== Nametypes, poptypes, blesses (the oracle doesn't read them: checked by re-reading) =====
#selectnametype 170
#addname "Testor"
#addname "Probe"
#end

#selectnametype 100
#addname "Extra"
#end

#selectpoptype 25
#addrecunit 5105
#end

#selectbless 1 -- Superior Morale
#cost0 2
#end

-- ===== Events (no number: the first, second and third #newevent) =====
#newevent
#rarity 2
#req_minpop 10
#req_fornation 5
#gold 50
#msg "Test event one: a gift of gold."
#end

#newevent
#rarity 0
#req_code -301
#req_targmnr 5102
#com 5106
#1d6units 5105
#code 0
#msg "Test event two: soldiers arrive."
#end

#newevent
#rarity -1
#req_unique 1
#req_land 1
#nation -2
#tempunits 2
#2d6units 5105
#code -301
#msg "Test event three: the chain starts."
#end

#selectevent 5
#rarity 2
#end
