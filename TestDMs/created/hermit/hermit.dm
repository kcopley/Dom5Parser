#modname "The Hermit of the Fen"
#version 1.0
#description "An event chain: a hermit offers to teach one of your commanders for a price. Made with Dom5Editor from scratch as a test."

#newevent
#rarity -1
#nation -2
#msg "A hermit wades out of the fen near ##landname## and offers to teach one of your commanders the old ways, for a gift of gold. Will you pay him?"
#req_land 1
#req_commander 1
#req_rare 25
#req_code 0
#code -300
#order 12
#end

#newevent
#rarity 0
#nation -2
#req_code -300
#req_targorder 103
#msg "The hermit spits into the water and curses ##landname## before he vanishes into the fen."
#code 0
#unrest 15
#end

#newevent
#rarity 0
#nation -2
#req_code -300
#req_targorder 102
#msg "##targname## pays the hermit and spends the month with him in the fen, learning the old ways."
#code 0
#gold -60
#xp 40
#delay 1
#end

#newevent
#rarity 5
#nation -2
#msg "A month later a boy from the fen brings a gift from the hermit. [Main Gauche of Parrying]"
#magicitem 9
#end

