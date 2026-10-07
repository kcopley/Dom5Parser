#modname "Thornvale"
#version 1.0
#description "Adds Thornvale, Wardens of the Briar: a middle era forest nation. Made with Dom5Editor from scratch as a test."

#newweapon 1000
#name "Thorn Spear"
#dmg 4
#att 0
#def 0
#len 3
#rcost 1
#pierce
#secondaryeffect 50
#end

#newweapon 1001
#copyweapon 23
#name "Briar Bow"
#dmg 9
#end

#newweapon 1002
#copyweapon 8
#name "Thornblade"
#dmg 8
#end

#newarmor 400
#copyarmor 5
#name "Bark Cuirass"
#prot 9
#enc 1
#end

#newmonster 5000
#name "Briar Warden"
#descr "Wardens patrol the thorn hedges that ring Thornvale. They fight with spears hardened in the sap of the briar."
#hp 11
#size 2
#prot 1
#mr 10
#mor 11
#str 10
#att 10
#def 10
#prec 10
#ap 12
#mapmove 16
#enc 3
#gcost 10
#rcost 1
#rpcost 10
#noleader
#forestsurvival
#spr1 "sprites/unit.png"
#spr2 "sprites/unitatk.png"
#weapon 1000
#armor 400
#armor 1
#end

#newmonster 5001
#copystats 5000
#clearweapons
#cleararmor
#copyspr 5000
#name "Thorn Archer"
#prec 12
#def 9
#weapon 1001
#weapon 9
#armor 400
#end

#newmonster 5002
#name "Bramble Knight"
#hp 15
#size 2
#prot 2
#mr 11
#mor 13
#str 12
#att 12
#def 12
#prec 10
#ap 12
#mapmove 14
#enc 3
#gcost 25
#rcost 2
#rpcost 18
#weapon 1002
#armor 8
#armor 21
#armor 3
#spr1 "sprites/unit.png"
#end

#newmonster 5003
#copystats 5000
#copyspr 5000
#name "Warden Captain"
#hp 13
#mr 11
#mor 12
#gcost 40
#rpcost 1
#goodleader
#end

#newmonster 5004
#copyspr 2468
#name "Briar Witch"
#descr "The witches of Thornvale speak with the hedge and the roots beneath it."
#hp 9
#size 2
#mr 14
#mor 10
#str 9
#att 8
#def 8
#prec 10
#ap 10
#mapmove 14
#enc 3
#gcost 10010
#rpcost 1
#poorleader
#magicskill 6 2
#magicskill 3 1
#custommagic 9728 100
#magicskill 9 1
#weapon 9
#end

#newmonster 5005
#copyspr 606
#name "The Thorn Mother"
#hp 60
#size 6
#prot 8
#mr 18
#mor 30
#str 18
#att 10
#def 6
#prec 8
#ap 6
#mapmove 0
#enc 0
#gcost 180
#okleader
#pathcost 40
#startdom 2
#magicskill 6 2
#magicskill 3 1
#immobile
#regeneration 1
#weapon 90
#end

#newsite 1700
#name "Heart of the Briar"
#path 6
#level 0
#rarity 5
#gems 6 2
#gems 3 1
#homemon 5002
#homecom 5004
#end

#selectnation 150
#name "Thornvale"
#epithet "Wardens of the Briar"
#era 2
#descr "Thornvale is a land of hedges and briars where the old pacts with the forest still hold."
#summary "Race: Humans. Military: Spearmen, archers, elite bramble knights. Magic: Nature, Earth. Priests: Weak."
#brief "A forest kingdom bound by the briar."
#color 0.25 0.5 0.25
#secondarycolor 0.6 0.35 0.1
#flag "sprites/flag.png"
#startsite "Heart of the Briar"
#addrecunit 5000
#addrecunit 5001
#addrecunit 5002
#addreccom 5003
#addreccom 5004
#startcom 5003
#startscout 426
#startunittype1 5000
#startunitnbrs1 10
#startunittype2 5001
#startunitnbrs2 8
#defcom1 5003
#defunit1 5000
#defmult1 20
#defunit2 5001
#defmult2 10
#addgod 5005
#addgod 606
#addgod 812
#homerealm 4
#end

