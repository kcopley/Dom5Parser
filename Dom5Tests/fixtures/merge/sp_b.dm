#modname "Spell damage test B"
#description "The same numbers; each #damage below moves only where the game reads it as a monster, enchantment, event #id or site"

#newmonster 5000
#copystats 3
#name "Beta Hawk"
#end

#newmonster 5001
#copystats 4
#name "Beta Lord"
#end

#newsite 1700
#name "Beta Grove"
#path 6
#level 1
#rarity 5
#end

-- monster 5001 (10089: 100 and up is the monster itself): follows Beta Lord
#newspell
#name "Beta Lord Call"
#school 0
#researchlevel 1
#effect 10089
#damage 5001
#end

-- 10089 key 3: one of the game's lists of uniques (the Heliophagi): stays
#newspell
#name "Beta Heliophagus"
#school 0
#researchlevel 1
#effect 10089
#damage 3
#end

-- a battlefield enchantment (133: newench): follows enchantment 301
#newspell
#name "Beta Time Halt"
#school 2
#researchlevel 1
#effect 133
#damage 301
#end

-- a province enchantment (10085: newench): follows enchantment 301
#newspell
#name "Beta Blessed Land"
#school 4
#researchlevel 1
#effect 10085
#damage 301
#end

-- a combat summon read as 43 (6043: the game's AI reads effect % 1000): follows Beta Hawk
#newspell
#name "Beta Hawks"
#school 0
#researchlevel 1
#effect 6043
#damage 5000
#end

-- afflictions (web): a bitmask, stays
#newspell
#name "Beta Web"
#school 3
#researchlevel 1
#effect 11
#damage 536870912
#end

-- 10500: a monster ability number (301) set on the caster, not an enchantment: stays
#newspell
#name "Beta Boon"
#school 4
#researchlevel 1
#effect 10500
#damage 301
#end

-- an event #id (10042): follows event id 60
#newevent
#rarity 5
#id 60
#msg "Beta's spell event."
#gold 20
#end

#newspell
#name "Beta Omen"
#school 4
#researchlevel 1
#effect 10042
#damage 60
#end

-- a site added to the province (10154: addfeatnr): follows Beta Grove
#newspell
#name "Beta Grove Rite"
#school 3
#researchlevel 1
#effect 10154
#damage 1700
#end
