#modname "Merge test A"
#description "Events with code -300, variable 1, enchantment 300, monster tag 1500"

#newmonster 5000
#copystats 1
#name "Alpha Guard"
#montag 1500
#end

#newspell
#name "Alpha Ward"
#school 0
#researchlevel 1
#effect 10081
#damage 300
#nreff 1
#end

#newevent
#rarity 5
#req_ench 300
#msg "Alpha sets the code."
#code -300
#incvar 1
#end

#newevent
#rarity 5
#req_code -300
#req_varpos 1
#msg "Alpha follows its code."
#1d6units -1500
#end
