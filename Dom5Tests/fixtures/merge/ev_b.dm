#modname "Merge test B"
#description "The same numbers, other meanings"

#newmonster 5000
#copystats 2
#name "Beta Guard"
#montag 1500
#end

#newspell
#name "Beta Ward"
#school 0
#researchlevel 1
#effect 10081
#damage 300
#nreff 1
#end

#newevent
#rarity 5
#req_ench 300
#msg "Beta sets the code."
#code -300
#incvar 1
#end

#newevent
#rarity 5
#req_code -300
#req_varpos 1
#msg "Beta follows its code."
#1d6units -1500
#end
