#modname "Copy test A"
#description "Changes game entities B copies, and has a unit on the number B's summon uses"

#selectspell 1024 -- Awaken Sleeper
#researchlevel 2
#fatiguecost 4000
#end

#selectmonster 20 -- Heavy Cavalry
#hp 99
#end

#selectsite 400 -- Lava Lake
#gold 200
#end

#newmonster 5000
#copystats 1
#name "A's Unit"
#end

#selectitem 5 -- Enchanted Sword
#constlevel 5
#end

#selectitem 508 -- Dragon Pearl
#constlevel 13
#end
