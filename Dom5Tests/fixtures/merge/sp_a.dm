#modname "Spell damage test A"
#description "What a spell's #damage is per #effect, as Dominions6.exe reads it (tools/dom6exe spelleffects)"

#newmonster 5000
#copystats 1
#name "Alpha Hawk"
#end

#newmonster 5001
#copystats 2
#name "Alpha Lord"
#end

#newsite 1700
#name "Alpha Grove"
#path 6
#level 1
#rarity 5
#end

#newspell
#name "Alpha Lord Call"
#school 0
#researchlevel 1
#effect 10089
#damage 5001
#end

#newspell
#name "Alpha Time Halt"
#school 2
#researchlevel 1
#effect 133
#damage 301
#end

#newevent
#rarity 5
#id 60
#msg "Alpha's spell event."
#gold 10
#end

#newspell
#name "Alpha Omen"
#school 4
#researchlevel 1
#effect 10042
#damage 60
#end
