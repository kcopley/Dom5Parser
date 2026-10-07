#modname "Ash Wolves"
#version 1.0
#description "A fire ritual that summons ash wolves, and a collar that calls one to every battle. Made with Dom5Editor from scratch as a test."

#newweapon 1000
#copyweapon 20
#name "Burning Bite"
#dmg 6
#fire
#end

#newmonster 5000
#name "Ash Wolf"
#descr "A wolf of cinders and smoke, called from the ashes of burned forests."
#hp 16
#size 3
#prot 4
#mr 12
#mor 12
#str 13
#att 12
#def 9
#prec 5
#ap 22
#mapmove 22
#enc 2
#gcost 0
#rcost 0
#quadruped
#weapon 1000
#animal
#magicbeing
#fireres 15
#heat 1
#spr1 "sprites/merc.png"
#spr2 "sprites/unitatk.png"
#end

#selectspell 2000
#name "Call the Ash Wolves"
#descr "The caster scatters the ashes of a burned grove and calls the wolves that sleep in them."
#school 0
#researchlevel 3
#path 0 0
#pathlevel 0 2
#fatiguecost 1500
#effect 10001
#damage 5000
#nreff 3
#end

#selectitem 700
#name "Collar of the Ash Wolf"
#descr "A collar of blackened iron. In battle an ash wolf answers its wearer's call."
#type 8
#constlevel 3
#mainpath 0
#fireres 5
#batstartsum1 5000
#spr "sprites/item.png"
#end

