#modname "Fire and Spear"
#version 1.0
#description "A small balance tweak: sturdier hoplites with a better spear, gorgons without fear, ashen angels that walk, shorter short bows, a costlier but wider fireball. Made with Dom5Editor as a test."

#newweapon 1000
#copyweapon 28
#name "Hoplite Long Spear"
#dmg 4
#end

#selectweapon 23
#range 30
#end

#selectmonster 14
#clearweapons
#hp 12
#gcost 10012
#weapon 1000
#end

#selectmonster 138
#fear 0
#end

#selectmonster 392
#clearspec
#amphibian
#undead
#neednoteat
#ethereal
#nametype 105
#fear 5
#invulnerable 25
#spiritsight
#coldres 15
#poisonres 25
#itemslots 991750
#almostliving
#end

#selectspell 659
#fatiguecost 25
#aoe 2
#end

