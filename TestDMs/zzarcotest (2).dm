#modname "Parser Test #2"
#description "A Test mod for the merger of DMs"
#icon "blah/blah.tga"
#version 1.?

#newmonster 6500 -- overlap #1
#name "DragonBase"
#spr1 "Dragonborn/Kobolds/Blue_KoboldMilitiaAxe1.tga" 
#spr2 "Dragonborn/Kobolds/Blue_KoboldMilitiaAxe2.tga" 
#hp 7
#prot 4
#fireres 4
#darkvision 50
#mountainsurvival
#mr 10
#mor 9
#str 9
#att 10
#def 11
#prec 9
#ap 12 -- Combat Speed
#mapmove 14 -- Ingame is +2
#enc 3
#size 1
#maxage 30
#humanoid
#itemslots 15494 -- 15494 = 2 hands, head, body, feet, 2 misc
#gcost 10011 -- 11g base
#rcost 3 -- Extra res cost
#rpcost 11
#nametype 100
#end

#newmonster 6501 -- overlap that copies
#copystats 6500
#name "DragonBase2"
#spr1 "Dragonborn/Kobolds/Blue_KoboldMilitiaAxe1.tga" 
#spr2 "Dragonborn/Kobolds/Blue_KoboldMilitiaAxe2.tga" 
#hp 7
#prot 4
#fireres 4
#darkvision 50
#mountainsurvival
#mr 10
#mor 9
#str 9
#att 10
#def 11
#prec 9
#ap 12 -- Combat Speed
#mapmove 14 -- Ingame is +2
#enc 3
#size 1
#maxage 30
#humanoid
#itemslots 15494 -- 15494 = 2 hands, head, body, feet, 2 misc
#gcost 10011 -- 11g base
#rcost 3 -- Extra res cost
#rpcost 11
#nametype 100
#end