#modname "duplicate names"
#description "Names several entities share. The game takes the lowest ID: Twin Blade = 1550, not 1600 (defined first); Spear = vanilla 1, not 96."

#newweapon 1600
#name "Twin Blade"
#dmg 5
#att 0
#end

#newweapon 1550
#name "Twin Blade"
#dmg 9
#att 1
#end

#newmonster 5000
#copystats 20
#name "Twin Wielder"
#clearweapons
#weapon "Twin Blade"
#weapon "Spear"
#end
