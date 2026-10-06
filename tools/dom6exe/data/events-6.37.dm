-- Dominions 6.37 vanilla events, written from Dominions6.exe by tools/dom6exe (exe d77cd364fe447e85).
-- #selectevent N is the game's event N. Requirements, then effects, in the order the game stores them.
-- "-- ro:" lines are stored codes no command writes (shown read-only), with the game's own
-- debug name for the code when it has one.
-- Commands that store the same code as a documented one (written as that one): #arena1 (as #arena), #req_notnation (as #req_notfornation).
-- messages: exe d77cd364fe447e85 offset 6520928 record 2920 size 2400 count 3302

#selectevent 0
#rarity -1
#req_mydominion 1
#req_minpop 50
#incdom 3
#end

#selectevent 1
#rarity -1
#req_temple 1
#req_mydominion 2  -- outside the command's range 0..1
#req_minpop 200
#incdom 4
#decscale 0
#end

#selectevent 2
#rarity -1
#req_minpop 800
-- ro: effect 190 (gold) = 300
#end

#selectevent 3
#rarity -2
#req_minpop 400
#landgold 50
#end

#selectevent 4
#rarity -2
#req_dominion 3
#req_land 1
#nation -2
#15d6units 217
#end

#selectevent 5
#rarity -2
#req_dominion 2
#req_land 1
#req_minpop 50
#req_unluck 0
#nation -2
-- ro: effect 183 (24d6units) = 30
#end

#selectevent 6
#rarity -1
#req_maxunrest 3
#req_minpop 100
#defence 15
#end

#selectevent 7
#rarity -1
#req_minunrest 5
#req_minpop 50
#defence 10
#end

#selectevent 8
#rarity -1
-- ro: effect 190 (gold) = 200
#end

#selectevent 9
#rarity -1
#req_luck 2
-- ro: effect 190 (gold) = 400
#end

#selectevent 10
#rarity -1
#req_luck 3
-- ro: effect 190 (gold) = 800
#end

#selectevent 11
#rarity -1
#req_dominion 5
#req_era 1
-- ro: effect 190 (gold) = 150
#magicitem 2
#end

#selectevent 12
#rarity -1
#req_minunrest 5
#req_minpop 50
#defence 15
#end

#selectevent 13
#rarity -1
#req_minpop 50
#1d3vis 56
#gold -1
#end

#selectevent 14
#rarity -1
#req_minunrest 3
-- ro: effect 190 (gold) = 250
#unrest -10
#end

#selectevent 15
#rarity -1
#req_minunrest 3
#req_order 2
-- ro: effect 190 (gold) = 350
#unrest -20
#end

#selectevent 16
#rarity -1
#req_pop0ok
#req_heat 1
#1d6vis 0
#end

#selectevent 17
#rarity -1
#req_pop0ok
#req_heat 1
#req_luck 0
#2d4vis 0
#end

#selectevent 18
#rarity -1
#req_pop0ok
#req_heat 2
#req_luck 0
#2d6vis 0
#end

#selectevent 19
#rarity -1
#req_chaos -1
#req_minpop 10
#req_land 1
#1d6vis 0
#1d6vis 4
#end

#selectevent 20
#rarity -1
#req_chaos -1
#req_minpop 10
#req_land 0
#1d6vis 0
#1d6vis 4
#end

#selectevent 21
#rarity -1
#req_pop0ok
#1d6vis 1
#end

#selectevent 22
#rarity -1
#req_pop0ok
#req_luck 1
#2d4vis 1
#end

#selectevent 23
#rarity -1
#req_pop0ok
#req_luck 2
#2d6vis 1
#end

#selectevent 24
#rarity -1
#req_pop0ok
#1d6vis 7
#end

#selectevent 25
#rarity -1
#req_pop0ok
#req_luck 1
#2d4vis 7
#end

#selectevent 26
#rarity -1
#req_pop0ok
#req_luck 2
#2d6vis 7
#end

#selectevent 27
#rarity -1
#req_land 1
#req_unluck -1
#1d3vis 51
#end

#selectevent 28
#rarity -1
#req_land 1
#req_luck 2
#1d6vis 51
#end

#selectevent 29
#rarity -1
#req_land 1
#req_luck 3
#1d6vis 51
#end

#selectevent 30
#rarity -1
#req_land 0
#req_unluck -1
#1d3vis 51
#end

#selectevent 31
#rarity -1
#req_land 0
#req_luck 2
#1d6vis 51
#end

#selectevent 32
#rarity -1
#req_freshwater 1
#req_luck 0
#req_magic -1
#1d6vis 51
#end

#selectevent 33
#rarity -1
#req_land 1
#req_unluck -1
#1d3vis 2
#end

#selectevent 34
#rarity -1
#req_land 1
#req_luck 1
#2d4vis 2
#end

#selectevent 35
#rarity -1
#req_pop0ok
#req_land 1
#req_cold 2
#2d6vis 2
#end

#selectevent 36
#rarity -1
#req_pop0ok
#1d6vis 3
#end

#selectevent 37
#rarity -1
#req_pop0ok
#req_unluck -1
#1d6vis 3
#end

#selectevent 38
#rarity -1
#req_pop0ok
#req_luck 2
#2d4vis 3
#end

#selectevent 39
#rarity -1
#req_pop0ok
#req_luck 3
#2d6vis 3
#end

#selectevent 40
#rarity -1
#req_pop0ok
#req_magic 1
#2d6vis 4
#end

#selectevent 41
#rarity -1
#req_pop0ok
#req_death 1
#2d6vis 5
#end

#selectevent 42
#rarity -1
#req_pop0ok
#req_land 0
#nation 5
#1d6vis 0
#end

#selectevent 43
#rarity -1
#req_pop0ok
#req_land 0
#1d6vis 5
#end

#selectevent 44
#rarity -1
#req_growth 1
#2d4vis 6
#end

#selectevent 45
#rarity -1
#req_land 1
#req_unluck -1
#1d6vis 6
#end

#selectevent 46
#rarity -1
#req_land 1
#req_luck 0
#2d6vis 6
#end

#selectevent 47
#rarity -1
#req_minpop 100
#req_land 1
#req_unluck -1
#1d6vis 8
#end

#selectevent 48
#rarity -1
#req_minpop 100
#req_land 1
#req_luck 2
#req_rare 70
#4d6vis 8
-- ro: effect 190 (gold) = 150
#end

#selectevent 49
#rarity -1
#req_minpop 800
#req_order 1
#req_turn 5
-- ro: effect 190 (gold) = 1000
#end

#selectevent 50
#rarity -1
#req_maxunrest 50
#req_magic 3
#req_unluck -1
#magicitem 2
#end

#selectevent 51
#rarity -1
#req_maxunrest 50
#req_magic 3
#req_luck 2
#magicitem 3
#end

#selectevent 52
#rarity -1
#req_turn 4
#req_noseason 3
#req_minpop 100
#req_order 1
-- ro: effect 190 (gold) = 500
#end

#selectevent 53
#rarity -1
#req_turn 5
#req_noseason 3
#req_minpop 100
#req_order 1
#req_era 3
-- ro: effect 190 (gold) = 1000
#end

#selectevent 54
#rarity -1
#req_order 1
#req_dominion 5
#req_noera 1
-- ro: effect 190 (gold) = 300
#magicitem 2
#end

#selectevent 55
#rarity -1
#req_growth 1
#req_dominion 3
-- ro: effect 190 (gold) = 200
#incdom 1
#end

#selectevent 56
#rarity -1
#req_chaos 0
#req_dominion 3
-- ro: effect 190 (gold) = 300
#incdom 1
#unrest 10
#end

#selectevent 57
#rarity -1
#req_order 1
#req_dominion 5
#req_noera 1
-- ro: effect 190 (gold) = 250
#end

#selectevent 58
#rarity -1
#req_minpop 500
#req_order 0
#req_growth 0
#req_maxunrest 25
#req_noera 1
-- ro: effect 190 (gold) = 200
#decscale2 4
#end

#selectevent 59
#rarity -1
#req_minpop 500
#req_chaos 2
#req_maxunrest 25
-- ro: effect 190 (gold) = 150
#decscale 0
#unrest -25
#end

#selectevent 60
#rarity -1
#req_land 1
#req_cave 0
#req_season 0
#2d6vis 1
#end

#selectevent 61
#rarity -1
#req_land 1
#req_cave 0
#req_season 0
#req_luck 3
#req_magic 0
#3d6vis 1
#end

#selectevent 62
#rarity -1
#req_season 0
#req_land 1
#req_cave 0
#unrest -50
#decscale3 3
#decscale2 4
#end

#selectevent 63
#rarity -1
#req_land 1
#req_cave 0
#req_season 1
#1d6vis 0
#end

#selectevent 64
#rarity -1
#req_land 1
#req_cave 0
#req_season 1
#req_heat -1
#2d6vis 0
#end

#selectevent 65
#rarity -1
#req_land 1
#req_cave 0
#req_season 1
#req_heat -1
#req_luck 3
#4d6vis 0
#end

#selectevent 66
#rarity -1
#req_land 1
#req_cave 0
#req_season 2
#2d6vis 3
#end

#selectevent 67
#rarity -1
#req_pop0ok
#req_land 1
#req_cave 0
#req_season 2
#1d3vis 56
#end

#selectevent 68
#rarity -1
#req_pop0ok
#req_land 1
#req_cave 0
#req_season 2
#req_luck 3
#1d6vis 56
#end

#selectevent 69
#rarity -1
#req_pop0ok
#req_land 1
#req_season 3
#2d6vis 2
#end

#selectevent 70
#rarity -1
#req_maxunrest 50
#req_season 3
#req_cave 0
#req_prod 0
#req_growth 0
#unrest -15
-- ro: effect 190 (gold) = 100
#end

#selectevent 71
#rarity -1
#req_maxunrest 50
#req_season 3
#req_cave 0
#req_prod 2
#req_growth 1
#unrest -15
#taxboost 200
#end

#selectevent 72
#rarity -1
#req_maxunrest 50
#req_season 3
#req_land 1
#req_cave 0
#unrest -20
-- ro: effect 190 (gold) = 150
#end

#selectevent 73
#rarity -1
#req_season 3
#req_land 1
#req_cave 0
#req_temple 1
#req_growth 0
#1d6vis 6
#end

#selectevent 74
#rarity -1
#req_season 3
#req_land 1
#req_cold 1
#req_cave 0
#1d6vis 0
#end

#selectevent 75
#rarity -1
#req_noseason 2
#req_noseason 3  -- stored twice; a second #req_noseason in a mod replaces the first
#1d6vis 6
#end

#selectevent 76
#rarity -1
#req_luck 2
#req_noseason 2
#req_noseason 3  -- stored twice; a second #req_noseason in a mod replaces the first
#4d6vis 6
#end

#selectevent 77
#rarity -1
#req_maxunrest 50
#req_noseason 0
#req_noseason 3  -- stored twice; a second #req_noseason in a mod replaces the first
#req_growth -1
#unrest -20
#taxboost 100
#end

#selectevent 78
#rarity -1
#req_maxunrest 50
#req_noseason 0
#req_noseason 3  -- stored twice; a second #req_noseason in a mod replaces the first
#req_growth 1
#unrest -30
#taxboost 250
#end

#selectevent 79
#rarity -1
#req_noseason 0
#req_noseason 1  -- stored twice; a second #req_noseason in a mod replaces the first
#1d6vis 5
#end

#selectevent 80
#rarity -1
#req_luck 3
#req_noseason 0
#req_noseason 1  -- stored twice; a second #req_noseason in a mod replaces the first
#4d6vis 5
#end

#selectevent 81
#rarity -1
#req_pop0ok
#req_waste 1
#req_heat 2
#2d6vis 0
#end

#selectevent 82
#rarity -1
#req_waste 1
#2d6vis 5
#end

#selectevent 83
#rarity -1
#req_pop0ok
#req_waste 1
#2d6vis 3
#end

#selectevent 84
#rarity -1
#req_pop0ok
#req_swamp 1
#2d6vis 2
#end

#selectevent 85
#rarity -1
#req_pop0ok
#req_swamp 1
#2d6vis 5
#end

#selectevent 86
#rarity -1
#req_lab 0
#req_swamp 1
#lab 1
#2d6vis 6
#end

#selectevent 87
#rarity -1
#req_forest 1
#2d6vis 6
#end

#selectevent 88
#rarity -1
#req_lab 0
#req_forest 1
#lab 1
#2d6vis 6
#end

#selectevent 89
#rarity -1
#req_forest 1
#2d6vis 6
#end

#selectevent 90
#rarity -1
#req_pop0ok
#req_mountain 1
#2d6vis 1
#end

#selectevent 91
#rarity -1
#req_mountain 1
#2d6vis 3
#end

#selectevent 92
#rarity -1
#req_mountain 1
#req_freesites 1
#addsite 68
#end

#selectevent 93
#rarity -1
#req_mountain 1
#req_luck 1
#req_freesites 1
#addsite 67
#end

#selectevent 94
#rarity -1
#req_mountain 1
#req_luck 2
#req_freesites 1
#addsite 66
#end

#selectevent 95
#rarity -1
#req_mountain 1
#req_freesites 1
#addsite 69
#end

#selectevent 96
#rarity -1
#req_pop0ok
#req_land 0
#2d6vis 2
#end

#selectevent 97
#rarity -1
#req_pop0ok
#req_land 0
#2d6vis 5
#end

#selectevent 98
#rarity -1
#req_land 0
#2d6vis 4
#end

#selectevent 99
#rarity -2
#req_temple 1
#req_mydominion 1
#incdom 2
#end

#selectevent 100
#rarity -2
#req_land 1
#nation -2
#unrest -100
#com 241
-- ro: effect 179 (20d6units) = 30
#end

#selectevent 101
#rarity -2
#req_temple 1
#req_mydominion 3  -- outside the command's range 0..1
#req_minpop 200
#req_land 1
#nation -2
#com 2331
#end

#selectevent 102
#rarity -2
#landprod 30
#end

#selectevent 103
#rarity -2
#req_temple 0
#req_maxunrest 5
#temple 1
#end

#selectevent 104
#rarity -2
#req_fort 0
#req_maxunrest 100
#req_minunrest 10
#req_land 1
#fort 1
#end

#selectevent 105
#rarity -2
#req_pop0ok
#req_maxunrest 50
#req_magic -1
#magicitem 3
#end

#selectevent 106
#rarity -2
#req_turn 4
#req_maxunrest 50
#req_order 0
#req_temple 1
-- ro: effect 190 (gold) = 750
#end

#selectevent 107
#rarity -2
#req_maxunrest 50
#req_order 0
#req_temple 1
#req_mydominion 1
#incdom 4
#decscale2 0
#end

#selectevent 108
#rarity -2
#req_maxunrest 50
#req_minpop 10
#req_land 1
#3d6vis 5
#unrest 25
#decscale2 5
#end

#selectevent 109
#rarity -2
#req_minpop 10
#req_land 1
#4d6vis 2
#end

#selectevent 110
#rarity -2
#req_turn 4
#req_temple 1
-- ro: effect 190 (gold) = 500
#magicitem 3
#end

#selectevent 111
#rarity -2
#req_turn 4
-- ro: effect 190 (gold) = 750
#end

#selectevent 112
#rarity -2
#req_turn 3
#req_land 1
#req_temple 0
-- ro: effect 190 (gold) = 500
#end

#selectevent 113
#rarity -2
#req_turn 3
#req_land 0
#req_temple 1
-- ro: effect 190 (gold) = 500
#end

#selectevent 114
#rarity -2
#req_turn 5
#req_rare 50
-- ro: effect 190 (gold) = 1000
#magicitem 1
#end

#selectevent 115
#rarity -2
#req_temple 0
#req_maxunrest 5
#req_minpop 50
#temple 1
#taxboost -100
#end

#selectevent 116
#rarity -2
#req_pop0ok
#magicitem 9
#end

#selectevent 117
#rarity -2
#req_turn 20
#req_unique 1
#req_targundead 0
#req_targdemon 0
#req_targmaxmorale 15
-- ro: requirement 244 = 1
-- ro: requirement 231 = 0
#addequip 9
#end

#selectevent 118
#rarity -2
#req_unique 3
#req_land 1
#magicitem 9
#end

#selectevent 119
#rarity -2
#req_unique 2
#req_land 0
#magicitem 9
#end

#selectevent 120
#rarity -2
#magicitem 1
#end

#selectevent 121
#rarity -2
#req_minunrest 50
#unrest -50
-- ro: effect 190 (gold) = 500
#magicitem 1
#end

#selectevent 122
#rarity -2
#req_land 1
#req_minpop 500
#req_fort 0
#req_order 1
#fort 12
#landgold 40
#end

#selectevent 123
#rarity -2
#req_minunrest 5
#req_chaos 1
#defence 20
#end

#selectevent 124
#rarity -2
#req_order 2
#defence 25
#end

#selectevent 125
#rarity -2
#req_magic 2
#defence 15
#end

#selectevent 126
#rarity -2
#req_luck 1
#req_magic 1
#req_fullowner 57
#req_land 1
#magicitem 3
-- ro: effect 190 (gold) = 99
#end

#selectevent 127
#rarity -2
#req_luck 3
#req_maxunrest 50
#req_turn 10
-- ro: effect 190 (gold) = 3000
#magicitem 3
#4d6vis 0
#1d3vis 56
#end

#selectevent 128
#rarity -2
#req_luck 4
#req_maxunrest 50
#req_turn 15
-- ro: effect 190 (gold) = 3000
#magicitem 2
#magicitem 3
#2d6vis 4
#1d6vis 56
#end

#selectevent 129
#rarity -2
#req_luck 5
#req_maxunrest 50
#req_turn 15
-- ro: effect 190 (gold) = 3000
#magicitem 1
#magicitem 2
#magicitem 3
#3d6vis 0
#2d4vis 56
#end

#selectevent 130
#rarity -2
#req_turn 6
#req_noseason 3
#req_minpop 200
#req_order 1
-- ro: effect 190 (gold) = 1500
#end

#selectevent 131
#rarity -2
#req_turn 3
#req_noseason 3
#req_minpop 200
#req_order 1
-- ro: effect 190 (gold) = 600
#end

#selectevent 132
#rarity -2
#req_pop0ok
#req_waste 1
#req_magic 2
#req_heat 3
#req_land 1
#req_unique 1
#nation -2
#com 389
#2d6units 527
#2d6units 640
#end

#selectevent 133
#rarity -2
#req_pop0ok
#req_land 1
#req_magic 2
#req_death 1
#nation -2
#com 310
-- ro: effect 179 (20d6units) = 197
#end

#selectevent 134
#rarity -2
#req_pop0ok
#req_land 1
#req_magic 1
#req_death 2
#req_dominion 4
#req_noera 3
#nation -2
#com 356
#2d6units 369
#8d6units 357
#end

#selectevent 135
#rarity -2
#req_land 1
#req_magic 2
#req_mountain 1
#nation -2
#com 479
#end

#selectevent 136
#rarity -2
#req_pop0ok
#req_land 1
#req_magic 3
#nation -2
#com 106
#end

#selectevent 137
#rarity -2
#req_land 1
#req_magic 2
#req_growth 2
#req_chaos 1
#req_unique 3
#nation -2
#com 122
#3d3units 330
#end

#selectevent 138
#rarity -2
#req_pop0ok
#req_land 1
#req_magic 2
#req_order 2
#nation -2
#com 629
#1d6vis 7
#end

#selectevent 139
#rarity -2
#req_pop0ok
#req_land 1
#req_magic 3
#req_order 0
#nation -2
#magicitem 2
#2d6vis 4
-- ro: effect 190 (gold) = 200
#end

#selectevent 140
#rarity -2
#req_turn 5
#req_land 1
#req_death 3
#req_luck 2
-- ro: effect 190 (gold) = 1500
#magicitem 1
#magicitem 1
#end

#selectevent 141
#rarity -2
#req_land 1
#req_prod 2
#req_magic 2
#req_lab 1
#req_freesites 1
#req_unique 1
#addsite 36
#end

#selectevent 142
#rarity -2
#req_land 1
#req_minpop 200
#req_death 2
#req_magic 3
#req_turn 15
#4d6vis 5
#4d6vis 5
#kill 50
#unrest 10
#end

#selectevent 143
#rarity -2
#req_pop0ok
#req_land 1
#req_luck 2
#req_magic 2
#req_freesites 1
#req_nositenbr 37
#addsite 37
#com 447
#3d6units 447
#end

#selectevent 144
#rarity -2
#req_luck 1
#req_turn 5
#req_noera 1
-- ro: effect 190 (gold) = 500
#magicitem 1
#end

#selectevent 145
#rarity -2
#req_pop0ok
#req_luck 2
#req_turn 10
#req_noera 1
-- ro: effect 190 (gold) = 500
#magicitem 2
#end

#selectevent 146
#rarity -2
#req_luck 1
#req_magic 1
#req_turn 5
-- ro: effect 190 (gold) = 500
#magicitem 3
#magicitem 1
#end

#selectevent 147
#rarity -2
#req_magic 2
#req_turn 10
#req_noera 1
#magicitem 3
#com 329
#12d6units 191
#6d6units 189
#end

#selectevent 148
#rarity -2
#req_turn 3
#req_luck 1
#req_order 1
#req_dominion 5
#req_noera 1
-- ro: effect 190 (gold) = 500
#end

#selectevent 149
#rarity -2
#req_turn 5
#req_luck 1
#req_order 1
#req_era 2
-- ro: effect 190 (gold) = 1000
#end

#selectevent 150
#rarity -2
#req_turn 3
#req_death 1
#req_luck 1
#req_dominion 5
-- ro: effect 190 (gold) = 750
#end

#selectevent 151
#rarity -2
#req_luck 1
#req_dominion 5
#req_era 1
-- ro: effect 190 (gold) = 300
#magicitem 2
#end

#selectevent 152
#rarity -2
#req_noseason 3
#req_noseason 2  -- stored twice; a second #req_noseason in a mod replaces the first
#req_minpop 200
-- ro: effect 190 (gold) = 200
#end

#selectevent 153
#rarity -2
#req_temple 1
#req_mydominion 1
#decscale2 3
#landgold 5
#end

#selectevent 154
#rarity -2
#req_temple 1
#req_mydominion 1
#req_season 1
#incdom 1
#end

#selectevent 155
#rarity -2
#req_temple 1
#req_mydominion 1
#req_season 2
#req_growth 0
#incdom 1
#decscale2 0
#end

#selectevent 156
#rarity -2
#req_land 1
#req_cave 0
#req_season 2
#req_growth 3
#4d6vis 6
#end

#selectevent 157
#rarity -2
#req_temple 1
#req_mydominion 1
#req_season 3
#req_cave 0
#incdom 1
#end

#selectevent 158
#rarity -2
#req_land 1
#req_cave 0
#req_cold 2
#req_dominion 3
#req_season 3
#req_cave 0  -- stored twice; a second #req_cave in a mod replaces the first
#nation -2
#com 309
#assfollower1 0
#end

#selectevent 159
#rarity -2
#req_land 1
#req_magic 2
#req_forest 1
#nation -2
#com 552
#10d6units 284
#end

#selectevent 160
#rarity -2
#req_land 1
#req_forest 1
#req_minpop 50
#nation -2
#com 633
-- ro: effect 190 (gold) = 200
#end

#selectevent 161
#rarity -2
#req_land 1
#req_forest 1
#req_minpop 50
#nation -2
#com 405
#addequip 3
#end

#selectevent 162
#rarity -2
#req_land 1
#req_forest 1
#req_minpop 50
#req_death 1
#req_pathdeath 3
#req_unique 2
#nation -2
#com 3274
#addequip 3
#end

#selectevent 163
#rarity -2
#req_mountain 1
#req_freesites 1
#addsite 66
#end

#selectevent 164
#rarity -2
#req_mountain 1
#req_freesites 1
#req_luck 2
#addsite 66
#end

#selectevent 165
#rarity -2
#req_land 1
#req_magic 2
#req_mountain 1
#req_gem 8
#nation -2
#com 93
-- ro: effect 190 (gold) = -50
#gemlosssmall 8
#end

#selectevent 166
#rarity -2
#req_pop0ok
#req_coast 1
#4d6vis 2
#end

#selectevent 167
#rarity -2
#req_coast 1
#4d6vis 8
#end

#selectevent 168
#rarity -2
#req_coast 1
-- ro: effect 190 (gold) = 600
#unrest -5
#end

#selectevent 169
#rarity -2
#req_coast 1
#req_magic 0
#req_luck 1
#nation -2
#com 1054
#end

#selectevent 170
#rarity -2
#req_luck 1
#req_maxunrest 50
#req_turn 5
#req_land 0
-- ro: effect 190 (gold) = 1000
#end

#selectevent 171
#rarity -2
#req_pop0ok
#req_luck 1
#req_maxunrest 50
#req_turn 5
#req_land 0
#1d6vis 56
#magicitem 1
#magicitem 1
#end

#selectevent 172
#rarity -2
#req_pop0ok
#req_land 0
#4d6vis 2
#end

#selectevent 173
#rarity -2
#req_land 0
#req_luck 3
#req_magic 2
#req_fort 1
#nation -2
#com 575
#4d6units 573
#com 576
#9d6units 574
#end

#selectevent 174
#rarity -2
#req_land 0
#req_magic 2
#nation -2
#com 103
#magicitem 3
#end

#selectevent 175
#rarity -2
#req_land 0
#req_magic 1
#req_growth 1
#nation -2
#com 575
#5d6units 1062
#end

#selectevent 176
#rarity -1
#req_monster 1556
#req_land 1
#req_unique 1
#nation -2
#com 1143
#killmon 1556
-- ro: effect 190 (gold) = 200
#end

#selectevent 177
#rarity -1
#req_land 1
#req_monster 2062
#nation -2
#com 2075
#end

#selectevent 178
#rarity -1
#req_land 1
#req_monster 2063
#nation -2
#com 2075
#end

#selectevent 179
#rarity -1
#req_land 1
#req_monster 2064
#nation -2
#com 2075
#end

#selectevent 180
#rarity -1
#req_land 1
#req_monster 2065
#nation -2
#com 2075
#end

#selectevent 181
#rarity -1
#req_land 1
#req_monster 2066
#nation -2
#com 2075
#end

#selectevent 182
#rarity -1
#req_land 1
#req_monster 2067
#nation -2
#com 2075
#end

#selectevent 183
#rarity 0
#req_rare 5
#req_unique 1
#req_turn 15
#req_monster 2032
#req_land 1
#req_monster 2031  -- stored twice; a second #req_monster in a mod replaces the first
#nation -2
#killcom 2032
#bloodboost 2031
#end

#selectevent 184
#rarity 0
#req_rare 4
#req_unique 1
#req_turn 25
#req_monster 2011
#req_land 1
#req_monster 2027  -- stored twice; a second #req_monster in a mod replaces the first
#req_dominion 8
#req_nomnr 2046
#nation -2
#killcom 2011
#com 2046
#1d6units 2045
#end

#selectevent 185
#rarity 0
#req_rare 1
#req_land 1
#req_monster 805
#req_dominion 2
#req_unique 3
#incdom 3
#end

#selectevent 186
#rarity -1
#req_land 1
#req_monster 807
#req_minpop 500
#req_order 1
-- ro: effect 190 (gold) = 250
#end

#selectevent 187
#rarity -1
#req_land 1
#req_monster 804
#req_minpop 500
#req_order 1
-- ro: effect 190 (gold) = 500
#end

#selectevent 188
#rarity -1
#req_land 1
#req_monster 804
#req_minpop 100
#req_order 1
-- ro: effect 190 (gold) = 250
#end

#selectevent 189
#rarity -1
#req_land 1
#req_monster 804
#req_chaos 1
#unrest -25
#decscale3 0
#end

#selectevent 190
#rarity -1
#req_land 1
#req_monster 544
#req_turn 10
#req_luck 1
#nation -2
#3d6units 146
#6d6units 145
#end

#selectevent 191
#rarity -1
#req_monster 1673
#req_chaos 1
#req_monster 1266  -- stored twice; a second #req_monster in a mod replaces the first
#req_monster 1274  -- stored twice; a second #req_monster in a mod replaces the first
#req_land 1
#nation -2
#killmon 1274
#3d6units 1266
#end

#selectevent 192
#rarity -1
#req_land 1
#req_monster 1073
#req_unique 1
#nation -2
#3d6units 532
#end

#selectevent 193
#rarity -1
#req_land 1
#req_monster 157
#req_turn 10
#req_growth 2
#nation -2
#3d6units 394
#end

#selectevent 194
#rarity -1
#req_land 1
#req_monster 586
#req_unique 1
#nation -2
#com 403
#end

#selectevent 195
#rarity -1
#req_land 1
#req_monster 586
#req_unique 1
#nation -2
#com 1309
#end

#selectevent 196
#rarity -1
#req_land 1
#req_monster 1848
#req_turn 10
#req_luck 1
#nation -2
#9d6units 686
#com 683
-- ro: effect 190 (gold) = 400
#end

#selectevent 197
#rarity -1
#req_unique 1
#req_land 1
#req_monster 1794
#req_turn 10
#req_death 1
#nation -2
#4d6units 1810
#6d6units 189
#9d6units 197
#end

#selectevent 198
#rarity -1
#req_land 1
#req_nomnr 381
#req_minunrest 10
#req_fornation 57
#unrest -25
#end

#selectevent 199
#rarity -1
#req_land 1
#req_monster 58
#magicitem 1
#magicitem 2
#end

#selectevent 200
#rarity -1
#req_unique 1
#req_land 1
#req_monster 58
#nation -2
#com 363
#end

#selectevent 201
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 110
#req_unluck -1
#nation -2
#15d6units 1238
#end

#selectevent 202
#rarity -1
#req_dominion 2
#req_chaos 1
#req_land 1
#req_fullowner 110
#nation -2
#6d6units 1270
#end

#selectevent 203
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 70
#nation -2
-- ro: effect 183 (24d6units) = 1393
#end

#selectevent 204
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 23
#nation -2
-- ro: effect 183 (24d6units) = 1393
#end

#selectevent 205
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 76
#nation -2
-- ro: effect 177 (18d6units) = 1599
#end

#selectevent 206
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 76
#nation -2
-- ro: effect 183 (24d6units) = 878
#end

#selectevent 207
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 5
#nation -2
#15d6units 199
#end

#selectevent 208
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 50
#nation -2
#15d6units 199
#end

#selectevent 209
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 95
#nation -2
#15d6units 199
#end

#selectevent 210
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 109
#nation -2
#15d6units 794
#end

#selectevent 211
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 22
#nation -2
#15d6units 794
#end

#selectevent 212
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 69
#nation -2
#15d6units 794
#end

#selectevent 213
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 27
#nation -2
-- ro: effect 183 (24d6units) = 168
#end

#selectevent 214
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 113
#nation -2
-- ro: effect 183 (24d6units) = 168
#end

#selectevent 215
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 75
#nation -2
-- ro: effect 183 (24d6units) = 168
#end

#selectevent 216
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 52
#req_forest 1
#nation -2
-- ro: effect 177 (18d6units) = 229
#end

#selectevent 217
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 7
#req_forest 1
#nation -2
-- ro: effect 177 (18d6units) = 229
#end

#selectevent 218
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 97
#req_forest 1
#nation -2
-- ro: effect 177 (18d6units) = 229
#end

#selectevent 219
#rarity -1
#req_dominion 1
#req_forest 1
#req_fullowner 52
#incdom 3
#end

#selectevent 220
#rarity -1
#req_dominion 1
#req_forest 1
#req_fullowner 7
#incdom 3
#end

#selectevent 221
#rarity -1
#req_dominion 1
#req_forest 1
#req_fullowner 97
#incdom 3
#end

#selectevent 222
#rarity 1
#req_dominion 1
#req_forest 1
#req_notfornation 52
#req_notfornation 7  -- stored twice; a second #req_notfornation in a mod replaces the first
#req_notfornation 97  -- stored twice; a second #req_notfornation in a mod replaces the first
#incdom -2
#end

#selectevent 223
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 104
#nation -2
#12d6units 984
#end

#selectevent 224
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 63
#nation -2
#12d6units 372
#end

#selectevent 225
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 108
#nation -2
-- ro: effect 183 (24d6units) = 1118
#end

#selectevent 226
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 68
#nation -2
-- ro: effect 183 (24d6units) = 1118
#end

#selectevent 227
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 20
#nation -2
-- ro: effect 183 (24d6units) = 1118
#end

#selectevent 228
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 21
#nation -2
-- ro: effect 183 (24d6units) = 1118
#end

#selectevent 229
#rarity -1
#req_dominion 2
#req_coast 1
#req_fullowner 126
#req_cold 2
#nation -2
#com 1631
-- ro: effect 177 (18d6units) = 1617
#end

#selectevent 230
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 98
#nation -2
#15d6units 1862
#end

#selectevent 231
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 56
#nation -2
#15d6units 684
#end

#selectevent 232
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 55
#nation -2
#15d6units 662
#end

#selectevent 233
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 8
#nation -2
-- ro: effect 177 (18d6units) = 1101
#end

#selectevent 234
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 116
#nation -2
#15d6units 1929
#end

#selectevent 235
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 116
#nation -2
#15d6units 1930
#end

#selectevent 236
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 100
#nation -2
#15d6units 61
#end

#selectevent 237
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 57
#nation -2
#15d6units 61
#end

#selectevent 238
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 58
#nation -2
#15d6units 1780
#end

#selectevent 239
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 71
#nation -2
-- ro: effect 177 (18d6units) = 129
#end

#selectevent 240
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 24
#nation -2
-- ro: effect 177 (18d6units) = 129
#end

#selectevent 241
#rarity -1
#req_dominion 2
#req_land 1
#req_fullowner 106
#nation -2
-- ro: effect 177 (18d6units) = 129
#end

#selectevent 242
#rarity -1
#req_chaos 0
#req_land 1
#req_fullowner 64
#nation -2
#9d6units 2003
#9d6units 2004
#end

#selectevent 243
#rarity -2
#req_dominion 3
#req_land 1
#req_fullowner 107
#nation -2
#com 1985
#4d6units 1989
#incdom 2
#end

#selectevent 244
#rarity -1
#req_magic 1
#req_land 0
#req_fullowner 43
#nation -2
-- ro: effect 177 (18d6units) = 1686
#end

#selectevent 245
#rarity -1
#req_dominion 2
#req_land 0
#req_fullowner 88
#nation -2
-- ro: effect 183 (24d6units) = 110
#end

#selectevent 246
#rarity -1
#req_land 0
#req_fullowner 126
#req_monster 2085
#nation -2
#3d6units 2086
#end

#selectevent 247
#rarity -2
#req_magic 2
#req_land 0
#req_fullowner 44
#nation -2
#3d6units 1529
#end

#selectevent 248
#rarity -1
#req_magic 1
#req_land 0
#req_fullowner 44
#nation -2
-- ro: effect 183 (24d6units) = 1515
#end

#selectevent 249
#rarity -1
#req_magic 2
#req_land 0
#req_fullowner 89
#nation -2
-- ro: effect 177 (18d6units) = 335
#magicitem 1
#end

#selectevent 250
#rarity -1
#req_dominion 2
#req_land 0
#req_fullowner 40
#nation -2
#15d6units 1056
#end

#selectevent 251
#rarity -1
#req_dominion 2
#req_land 0
#req_fullowner 86
#nation -2
#15d6units 1056
#end

#selectevent 252
#rarity -1
#req_dominion 2
#req_land 0
#req_fullowner 41
#nation -2
-- ro: effect 183 (24d6units) = 1041
#end

#selectevent 253
#rarity -1
#req_dominion 2
#req_land 0
#req_fullowner 87
#nation -2
-- ro: effect 183 (24d6units) = 1041
#end

#selectevent 254
#rarity 1
#req_land 1
#req_turn 8
#incdom -2
#stealthcom 2275
#6d6units 2276
#end

#selectevent 255
#rarity 1
#req_minunrest 30
#req_land 1
#unrest 30
#stealthcom 1912
#12d6units 482
#end

#selectevent 256
#rarity 1
#unrest 35
#end

#selectevent 257
#rarity 1
#req_minunrest 20
#req_minpop 100
#req_maxtroops 20
#unrest 15
-- ro: effect 190 (gold) = -200
#end

#selectevent 258
#rarity 1
#req_commander 0
#req_minunrest 10
#req_maxtroops 0
#req_maxdef 10
#req_land 1
#revolt
#2com 18
#9d6units 18
#end

#selectevent 259
#rarity 1
#req_temple 1
#req_maxdominion 5
#req_turn 3
#req_mydominion 1
#incdom -2
#end

#selectevent 260
#rarity 1
#req_maxdominion 8
#req_mydominion 1
#req_turn 8
#incdom -2
#unrest 10
#end

#selectevent 261
#rarity 1
#req_maxdominion 6
#req_mydominion 1
#req_turn 8
#incdom -2
#end

#selectevent 262
#rarity 1
#req_maxdominion 6
#req_mydominion 1
#req_temple 1
#req_turn 8
#incdom -2
-- ro: effect 190 (gold) = -100
#end

#selectevent 263
#rarity 1
#req_land 1
#req_temple 1
#req_turn 17
#kill 5
#temple 0
#end

#selectevent 264
#rarity 1
#req_land 1
#req_temple 1
#req_turn 17
#req_unluck 2
#kill 15
#temple 0
#end

#selectevent 265
#rarity 1
#req_land 1
#req_lab 1
#req_heat -1
#req_turn 10
#lab 0
#end

#selectevent 266
#rarity 1
#id 5
#unrest 30
#incscale3 4
#curse 10
#end

#selectevent 267
#rarity 1
#req_mydominion 1
#req_maxdominion 5
#req_turn 8
#incdom -3
#end

#selectevent 268
#rarity 1
#req_lab 1
#req_gem 8
#req_nomnr 646
#req_nomnr 649
#req_land 1
#gemloss 8
#end

#selectevent 269
#rarity 1
#req_minpop 20
#unrest 20
#decscale 5
#end

#selectevent 270
#rarity 1
#req_mydominion 1
#req_maxdominion 3
#req_turn 8
#nation -1
#newdom 2
#end

#selectevent 271
#rarity 1
#req_mydominion 1
#req_turn 8
#incdom -2
#incscale3 1
#landprod -5
#end

#selectevent 272
#rarity 1
#req_growth -2
#req_land 1
#req_rare 66
#incscale3 4
#curse 10
#end

#selectevent 273
#rarity 1
#req_magic 1
#req_turn 5
#req_mydominion 1
#unrest 30
#incdom -3
#end

#selectevent 274
#rarity 1
#req_chaos 2
-- ro: requirement 114 = 9
#emigration 20
#unrest 20
#end

#selectevent 275
#rarity 1
#req_death 2
#req_turn 10
#unrest 100
#kill 20
-- ro: effect 190 (gold) = -100
#end

#selectevent 276
#rarity 1
#req_death 3
#req_turn 15
#kill 50
#unrest 10
#disease 10
#id 1
#end

#selectevent 277
#rarity 1
#req_land 1
#req_magic 1
#req_mintroops 5
#curse 5
#1d3vis 6
#end

#selectevent 278
#rarity 1
#req_land 1
#req_cave 0
#req_unluck 1
#req_turn 8
#nation 4
#com 141
#8d6units 140
#com 147
#8d6units 140
#kill 10
#end

#selectevent 279
#rarity 1
#req_land 1
#req_unluck 2
#req_turn 8
#req_era 2
#com 23
#6d6units 22
#com 23
#4d6units 18
#end

#selectevent 280
#rarity 1
#req_land 0
#req_unluck 1
#req_turn 5
#com 406
#9d6units 174
#com 406
#4d6units 175
#end

#selectevent 281
#rarity 1
#req_capital 0
#req_land 1
#req_cave 0
#req_unluck 3
#req_turn 10
#req_land 1  -- stored twice; a second #req_land in a mod replaces the first
#nation 4
#com 147
-- ro: effect 179 (20d6units) = 140
#com 141
#10d6units 139
#kill 20
#end

#selectevent 282
#rarity 1
#req_capital 0
#req_pop0ok
#req_land 1
#req_death 1
#req_turn 5
#req_magic 2
#req_lab 1
#com 310
#15d6units 197
#15d6units 196
#kill 5
#gemloss 5
#end

#selectevent 283
#rarity 1
#req_pop0ok
#req_lab 1
#req_unmagic 3
#req_unluck 1
#gemloss 56
#end

#selectevent 284
#rarity 1
#req_maxdominion 2
#req_mydominion 1
#req_chaos 0
#emigration 10
#incdom 1
#end

#selectevent 285
#rarity 1
-- ro: requirement 114 = 9
#req_minunrest 20
#req_chaos -2
#emigration 20
#unrest 20
#end

#selectevent 286
#rarity 1
#req_growth 1
#req_magic 1
#req_land 1
#incscale2 5
#incscale 3
#incscale3 4
#1d3vis 1
#end

#selectevent 287
#rarity 1
#req_turn 5
#req_chaos 1
#unrest 45
#taxboost -100
#end

#selectevent 288
#rarity 1
#req_mindef 7
#req_death 0
#defence -10
#end

#selectevent 289
#rarity 1
#req_chaos 1
#req_minpop 100
#req_capital 0
#unrest 30
#kill 5
#end

#selectevent 290
#rarity 1
#req_maxdominion 3
#req_turn 5
#req_mydominion 1
#req_season 0
#req_minpop 50
#incdom -4
#decscale2 3
#end

#selectevent 291
#rarity 1
#req_farm 1
#req_cave 0
#req_season 0
#gold -200
#addgeo 274877906944
#end

#selectevent 292
#rarity 1
#req_maxdominion 3
#req_mydominion 1
#req_season 1
#req_minpop 50
#req_turn 8
#req_land 1
#incdom -4
#decscale2 3
#taxboost 100
#end

#selectevent 293
#rarity 1
#req_turn 5
#req_season 1
#req_minpop 20
#req_land 1
#req_cave 0
#decscale 3
#decscale 4
#incscale3 1
#taxboost -100
#end

#selectevent 294
#rarity 1
#req_season 2
#req_minpop 50
#req_magic 1
#req_growth 1
#incscale3 3
-- ro: effect 190 (gold) = -100
#end

#selectevent 295
#rarity 1
#req_season 2
#req_minpop 10
#req_permonth 1
#taxboost -100
#end

#selectevent 296
#rarity 1
#req_cold 1
#req_season 3
#req_land 1
#req_cave 0
-- ro: effect 190 (gold) = -200
#end

#selectevent 297
#rarity 1
#req_cold 0
#req_season 3
#req_land 1
#req_cave 0
#kill 10
#end

#selectevent 298
#rarity 1
#req_land 1
#req_cave 0
#req_season 3
#com 1224
#1d6units 1224
#7d6units 284
#kill 2
#unrest 10
#end

#selectevent 299
#rarity 1
#req_land 0
#req_turn 10
#unrest 40
#kill 1
#end

#selectevent 300
#rarity 1
#req_coast 1
#req_turn 10
#unrest 25
#kill 3
#end

#selectevent 301
#rarity 1
#req_coast 1
#unrest 35
#end

#selectevent 302
#rarity 1
#req_land 1
#req_growth 1
#req_turn 8
#req_forest 1
#req_magic 1
#com 105
#2d6units 361
#com 105
#2d6units 361
#com 105
#2d6units 362
#kill 10
#end

#selectevent 303
#rarity 1
#req_cold 0
#req_cave 0
#req_mountain 1
-- ro: effect 190 (gold) = -100
#end

#selectevent 304
#rarity 2
#req_turn 15
#req_capital 0
#kill 50
#unrest 10
#end

#selectevent 305
#rarity 2
#req_death 0
#req_turn 10
#unrest 40
#kill 20
-- ro: effect 190 (gold) = -80
#end

#selectevent 306
#rarity 2
#req_death 0
#req_turn 10
#id 9
#unrest 15
#kill 5
-- ro: effect 190 (gold) = -80
#end

#selectevent 307
#rarity 2
#req_land 1
#req_cave 0
#req_turn 10
#kill 3
#unrest 5
-- ro: effect 190 (gold) = -80
#end

#selectevent 308
#rarity 2
#req_heat -2
#req_chaos -1
#req_turn 10
#id 2
#unrest 60
#kill 5
#end

#selectevent 309
#rarity 2
#req_land 1
#req_turn 10
#req_chaos -1
#com 447
#9d6units 447
#kill 25
#end

#selectevent 310
#rarity 2
#req_land 1
#decscale3 2
#end

#selectevent 311
#rarity 2
#req_land 1
#incscale3 4
#unrest 15
#end

#selectevent 312
#rarity 2
#req_lab 1
#req_gem 0
#req_chaos 1
#req_turn 12
#4d6vis 0
#lab 0
#end

#selectevent 313
#rarity 2
#req_lab 1
#req_turn 8
#req_unluck -1
#lab 0
#end

#selectevent 314
#rarity 2
#req_lab 1
#req_gem 0
#gemloss 0
#end

#selectevent 315
#rarity 2
#req_lab 1
#req_gem 1
#gemloss 1
#end

#selectevent 316
#rarity 2
#req_lab 1
#req_gem 2
#gemloss 2
#end

#selectevent 317
#rarity 2
#req_lab 1
#req_gem 3
#gemloss 3
#end

#selectevent 318
#rarity 2
#req_lab 1
#req_gem 4
#gemloss 4
#end

#selectevent 319
#rarity 2
#req_lab 1
#req_gem 7
#gemloss 7
#end

#selectevent 320
#rarity 2
#req_lab 1
#req_gem 8
#gemloss 8
#end

#selectevent 321
#rarity 2
#req_lab 1
#req_unluck -1
#gemloss 56
#end

#selectevent 322
#rarity 2
#req_pop0ok
#req_lab 1
#req_unmagic 1
#gemloss 56
#incscale 5
#end

#selectevent 323
#rarity 2
#req_pop0ok
#req_lab 1
#req_unmagic 3
#gemlosslarge 56
#end

#selectevent 324
#rarity 2
#req_lab 1
#req_humanoidres
-- ro: effect 77 (researchaff) = 524288
#end

#selectevent 325
#rarity 2
#req_pop0ok
#req_lab 1
#req_humanoidres
-- ro: effect 77 (researchaff) = 1073741824
#end

#selectevent 326
#rarity 2
#req_pop0ok
#req_lab 1
#req_humanoidres
-- ro: effect 77 (researchaff) = 8388608
#end

#selectevent 327
#rarity 2
#req_land 1
#req_maxtroops 20
#com 482
#9d6units 482
#com 482
#9d6units 482
#end

#selectevent 328
#rarity 2
#req_land 1
#req_turn 10
#req_nomnr 646
#req_nomnr 649
#req_owncapital 0
#visitors  -- stored 0; the command stores 1
#end

#selectevent 329
#rarity 2
#req_pop0ok
#req_land 1
#req_commander 1
#req_turn 6
#assassin 428
#end

#selectevent 330
#rarity 2
#req_land 1
#req_commander 1
#req_era 3
#req_turn 6
#assassin 1257
#end

#selectevent 331
#rarity 2
#req_land 1
#req_era 3
#req_turn 8
#com 1270
#15d6units 1270
#end

#selectevent 332
#rarity 2
#req_maxdominion 5
#req_mydominion 1
#req_turn 8
#nation -1
#newdom 5
#end

#selectevent 333
#rarity 2
#req_unluck 0
#req_death 0
#req_maxdominion 5
#req_land 1
#req_nation 54
#req_notforally 54
#req_turn 15
#nation 54
#newdom 2
#4com 554
#9d6units -2
#6d6units -15
#extramsg 54
#end

#selectevent 334
#rarity 2
#req_chaos -1
#incscale 3
#curse 10
#end

#selectevent 335
#rarity 2
#req_growth 0
#req_magic 1
#req_minpop 20
#req_land 1
#incscale 0
#unrest 10
#end

#selectevent 336
#rarity 2
#req_mydominion 1
#req_turn 8
#req_owncapital 0
#nation -1
#newdom 3
#decscale 5
#end

#selectevent 337
#rarity 2
#req_growth -2
#req_land 1
#incscale3 4
#end

#selectevent 338
#rarity 2
#req_growth 0
#req_land 1
#id 8
#unrest 20
#taxboost -50
#end

#selectevent 339
#rarity 2
-- ro: requirement 114 = 12
#req_chaos 0
#emigration 30
#end

#selectevent 340
#rarity 2
#req_land 1
#req_cave 0
#req_chaos 1
#req_turn 10
#id 7
#unrest 25
#kill 3
#end

#selectevent 341
#rarity 2
#req_land 1
#req_cave 0
#req_chaos 3
#req_unluck 2
#id 14
#unrest 35
#kill 4
#end

#selectevent 342
#rarity 2
#req_death 1
#req_turn 15
#req_unluck -1
#kill 50
#unrest 10
#end

#selectevent 343
#rarity 2
#req_cold 2
#req_land 1
#req_cave 0
#req_turn 10
#kill 5
#unrest 5
-- ro: effect 190 (gold) = -150
#end

#selectevent 344
#rarity 2
#req_growth 2
#req_land 1
#unrest 5
-- ro: effect 190 (gold) = -50
#end

#selectevent 345
#rarity 2
#req_chaos 2
#req_turn 10
#req_capital 0
#id 4
#maybeaddsite -1
#unrest 125
#kill 30
#decscale2 2
-- ro: effect 193 = 50
#lab 0
-- ro: effect 193 = 50
#temple 0
#end

#selectevent 346
#rarity 2
#req_land 1
#req_magic 2
#id 6
#unrest 30
#incscale3 4
#disease 4
#end

#selectevent 347
#rarity 2
#req_magic 0
#req_land 1
#req_noera 3
#unrest 10
-- ro: effect 190 (gold) = -50
#end

#selectevent 348
#rarity 2
#req_lab 1
#req_magic 1
#req_researcher
-- ro: effect 77 (researchaff) = 4194304
#end

#selectevent 349
#rarity 2
#req_magic 0
#req_land 1
#req_turn 15
#com 3274
#addequip 2
#6d6units 405
#9d6units 30
#7d6units 1022
#end

#selectevent 350
#rarity 2
#req_cold -2
#req_land 1
#req_turn 10
#id 13
#decscale3 2
#kill 5
#end

#selectevent 351
#rarity 2
#req_land 1
#req_turn 10
#id 16
#decscale3 2
#incscale 3
#kill 10
#unrest 20
#end

#selectevent 352
#rarity 2
#req_cold 1
#req_land 1
#req_turn 10
#id 3
#incscale3 2
#kill 5
#end

#selectevent 353
#rarity 2
#req_land 1
#req_minpop 100
#req_chaos -2
#req_magic 2
#req_turn 10
#incscale3 5
#kill 10
#end

#selectevent 354
#rarity 1
#req_chaos 2
#req_minpop 200
#req_turn 15
#req_capital 0
#unrest 25
#kill 10
#emigration 15
#end

#selectevent 355
#rarity 2
#req_land 1
#req_cave 0
#req_season 2
#incscale3 2
#end

#selectevent 356
#rarity 2
#req_season 3
#req_land 1
#req_cave 0
#incscale3 2
#end

#selectevent 357
#rarity 2
#req_cold 0
#req_land 1
#req_cave 0
#req_noseason 3
#req_noseason 1  -- stored twice; a second #req_noseason in a mod replaces the first
#req_turn 10
#unrest 10
#kill 1
-- ro: effect 190 (gold) = -80
#incscale 2
#end

#selectevent 358
#rarity 2
#req_land 1
#req_cave 0
#req_heat -1
#req_growth -1
#req_season 1
#req_freshwater 1
#req_cave 0  -- stored twice; a second #req_cave in a mod replaces the first
#req_turn 12
#kill 25
#addgeo 274877906944
#end

#selectevent 359
#rarity 2
#req_land 0
#req_magic 1
#req_turn 5
-- ro: requirement 114 = 15
#com 639
#6d6units 438
#end

#selectevent 360
#rarity 2
#req_temple 1
#req_maxdominion 5
#req_turn 7
#req_capital 0
#temple 0
#end

#selectevent 361
#rarity 2
#req_land 0
#req_turn 10
-- ro: requirement 114 = 20
#com 580
#9d6units 564
#end

#selectevent 362
#rarity 2
#req_land 0
#req_turn 5
-- ro: requirement 114 = 20
#com 207
#6d6units 208
#3d6units 208
#end

#selectevent 363
#rarity 2
#req_heat 0
#req_land 1
#req_turn 5
#req_swamp 1
#com 1831
#end

#selectevent 364
#rarity 2
#req_coast 1
#req_turn 10
#id 10
#unrest 50
#kill 30
#end

#selectevent 365
#rarity 2
#req_land 1
#req_turn 8
#req_mountain 1
#com 519
#9d6units 518
#end

#selectevent 366
#rarity 2
#req_capital 0
#req_land 1
#req_turn 10
#req_nomnr 641
#req_forest 1
#req_growth 1
#com 641
-- ro: effect 183 (24d6units) = 313
#15d6units 314
#kill 95
#addequip 3
#end

#selectevent 367
#rarity 2
#req_capital 0
#req_land 1
#req_turn 10
#req_nomnr 641
#req_forest 1
#req_growth 0
#nation 53
#com 1005
#9d6units 313
#3d6units 314
#unrest 10
#kill 3
#id 17
#end

#selectevent 368
#rarity 2
#req_cold 2
#req_land 1
#req_turn 8
#req_forest 1
#com 309
#6d6units 50
#3d6units 579
#4d6units 31
#addequip 1
#end

#selectevent 369
#rarity 2
#req_pop0ok
#req_maxpop 1
#req_cold 2
#req_land 1
#req_turn 8
#req_forest 1
#com 309
#6d6units 50
#3d6units 579
#4d6units 31
#addequip 1
#end

#selectevent 370
#rarity 2
#req_minpop 100
#req_forest 1
#req_nation 52
#req_notforally 52
#req_growth 3
#req_turn 8
#nation 52
#newdom 5
#com 237
#8d6units 435
#com 237
#8d6units 435
#extramsg 52
#end

#selectevent 371
#rarity 2
#req_pop0ok
#req_waste 1
#req_land 1
#req_turn 7
#com 524
#3d6units 524
#end

#selectevent 372
#rarity 2
#req_turn 10
#req_land 1
#req_temple 1
#req_monster 1991
#temple 0
#end

#selectevent 373
#rarity 1
#req_lab 1
#req_temple 1
#req_gem 8
#req_monster 1991
#gemloss 8
#end

#selectevent 374
#rarity 2
#req_minpop 100
#req_land 1
#req_nation 61
#req_fullowner 54
#req_turn 7
#nation 61
#newdom 5
#4com 148
#12d6units 217
#extramsg 61
#end

#selectevent 375
#rarity 1
#req_land 1
#req_monster 1247
#req_monster 1248  -- stored twice; a second #req_monster in a mod replaces the first
#killmon 1247
#killmon 1248
#end

#selectevent 376
#rarity 1
#req_land 1
#req_monster 1879
#req_monster 1880  -- stored twice; a second #req_monster in a mod replaces the first
#killcom 1880
#end

#selectevent 377
#rarity 1
#req_land 1
#req_monster 1879
#req_monster 1875  -- stored twice; a second #req_monster in a mod replaces the first
#killcom 1875
#end

#selectevent 378
#rarity 1
#req_land 1
#req_monster 1879
#req_monster 1873  -- stored twice; a second #req_monster in a mod replaces the first
#killcom 1873
#end

#selectevent 379
#rarity 1
#req_monster 2012
#req_land 1
#req_monster 1986  -- stored twice; a second #req_monster in a mod replaces the first
#nation -2
#killcom 2012
#end

#selectevent 380
#rarity 1
#req_monster 2031
#req_land 1
#req_monster 2030  -- stored twice; a second #req_monster in a mod replaces the first
#nation -2
#killmon 2030
#end

#selectevent 381
#rarity 1
#req_monster 2030
#req_land 1
#req_monster 2035  -- stored twice; a second #req_monster in a mod replaces the first
#nation -2
#killmon 2035
#end

#selectevent 382
#rarity 2
#req_land 1
#req_monster 781
#req_mindef 2
#defence -1
#incdom -2
#end

#selectevent 383
#rarity 2
#req_monster 150
#killmon 150
#end

#selectevent 384
#rarity 2
#req_monster 216
#req_lab 1
#req_gem 8
#gemlosssmall 8
#end

#selectevent 385
#rarity 2
#req_monster 265
#req_lab 1
#req_gem 8
#gemlosssmall 8
#end

#selectevent 386
#rarity 2
#req_monster 266
#req_lab 1
#req_gem 8
#gemlosssmall 8
#end

#selectevent 387
#rarity 1
#req_monster 656
#req_minpop 10
#kill 10
#unrest 20
#end

#selectevent 388
#rarity 1
#req_lab 1
#req_gem 8
#req_monster 1589
#gemlosssmall 8
#end

#selectevent 389
#rarity 2
#req_land 1
#req_monster 1589
#req_monster 1228  -- stored twice; a second #req_monster in a mod replaces the first
#killcom 1228
#kill 10
#bloodboost 1589
#end

#selectevent 390
#rarity 1
#req_land 1
#req_monster 508
#unrest 30
#end

#selectevent 391
#rarity 1
#req_land 1
#req_monster 508
#unrest 30
#end

#selectevent 392
#rarity 1
#req_land 1
#req_monster 508
#unrest 20
#end

#selectevent 393
#rarity 2
#req_growth 1
#req_land 1
#id 11
#unrest 15
#stealthcom 1910
#end

#selectevent 394
#rarity 2
#req_magic 2
#req_land 1
#req_turn 5
#req_chaos 1
#id 12
#stealthcom 1911
#unrest 30
#end

#selectevent 395
#rarity 13
#req_rare 0
#id 18
#worlddecscale 5
#worldmark 2
#end

#selectevent 396
#rarity 13
#req_rare 0
#id 19
#worlddecscale 5
#worldmark 4
-- ro: effect 191 = 1
#end

#selectevent 397
#rarity 11
#worldincscale 4
#end

#selectevent 398
#rarity 11
#worlddecscale 4
#end

#selectevent 399
#rarity 11
#worlddecscale 5
#end

#selectevent 400
#rarity 11
#worldincscale 5
#end

#selectevent 401
#rarity 11
#worlddecscale 4
#worlddecscale 3
#end

#selectevent 402
#rarity 11
#worldincscale 2
#worldunrest 5
#end

#selectevent 403
#rarity 11
#worlddecscale 2
#worldunrest 5
#end

#selectevent 404
#rarity 11
#req_season 2
#req_cave 0
#worlddecscale 3
#end

#selectevent 405
#rarity 11
#worldritrebate 0
#linger 3
#end

#selectevent 406
#rarity 11
#worldritrebate 1
#linger 3
#end

#selectevent 407
#rarity 11
#worldritrebate 2
#linger 3
#end

#selectevent 408
#rarity 11
#worldritrebate 3
#linger 3
#end

#selectevent 409
#rarity 11
#worldritrebate 4
#linger 3
#end

#selectevent 410
#rarity 11
#worldritrebate 5
#linger 3
#end

#selectevent 411
#rarity 11
#worldritrebate 6
#linger 3
#end

#selectevent 412
#rarity 15
#req_month 9
#worldincscale 2
#end

#selectevent 413
#rarity 15
#req_month 3
#worlddecscale 2
#end

#selectevent 414
#rarity 12
#req_capital 0
#req_code 0
#req_cave 0
#code 30000
#worldunrest 5
#end

#selectevent 415
#rarity 10
#req_code 30000
#worldunrest 15
#code 30001
#end

#selectevent 416
#rarity 10
#req_code 30001
#req_rare 50
#revealprov
#unrest 75
#code 0
#kill 10
#incscale2 3
#addsite -1
#end

#selectevent 417
#rarity 10
#req_code 30001
#revealprov
#unrest 75
#code 0
#kill 75
#incscale2 3
#strikeunits 100
#lab 0
#temple 0
#addsite -1
#end

#selectevent 418
#rarity 12
#worldunrest 10
#end

#selectevent 419
#rarity 12
#worldincscale2 4
#worlddecscale 5
#end

#selectevent 420
#rarity 12
#req_unique 1
#worldincdom -1
#end

#selectevent 421
#rarity 12
#worldincscale2 5
#worldincscale 4
#end

#selectevent 422
#rarity 12
#worldincscale2 0
#worlddecscale 5
#end

#selectevent 423
#rarity -1
#req_land 1
#req_luck 1
#req_pop0ok
-- ro: effect 190 (gold) = 350
#end

#selectevent 424
#rarity -1
#req_land 1
#req_unluck 0
#req_pop0ok
-- ro: effect 190 (gold) = 50
#end

#selectevent 425
#rarity -1
#req_pop0ok
#req_land 0
#req_luck 0
-- ro: effect 190 (gold) = 150
#end

#selectevent 426
#rarity 1
#req_magic 0
#req_minpop 50
#req_land 1
#incscale3 4
#decscale2 5
#end

#selectevent 427
#rarity 1
#req_magic 0
#req_minpop 50
#req_land 0
#incscale3 4
#decscale2 5
#end

#selectevent 428
#rarity -1
#req_minpop 50
#req_temple 1
#incdom 2
#end

#selectevent 429
#rarity -2
#req_minpop 50
#req_land 1
#4d6vis 8
#end

#selectevent 430
#rarity -1
#req_magic 1
#req_land 1
#req_growth 0
#1d6vis 6
#end

#selectevent 431
#rarity -1
#req_chaos 2
#unrest 20
-- ro: effect 190 (gold) = 150
#magicitem 1
#end

#selectevent 432
#rarity -2
#req_chaos 1
#1d6vis 53
#end

#selectevent 433
#rarity -1
#req_land 1
#req_cave 0
#req_season 0
#req_cold 0
#1d6vis 6
#end

#selectevent 434
#rarity -1
#req_land 1
#req_cave 0
#req_season 0
#req_cold 0
#req_luck 2
#3d6vis 6
#end

#selectevent 435
#rarity -2
#req_land 1
#req_magic 0
#req_forest 1
#req_unluck 1
-- ro: effect 190 (gold) = 8
#end

#selectevent 436
#rarity -2
#req_coast 1
#req_noseason 3
#nation -2
#com 629
#end

#selectevent 437
#rarity 1
#req_fornation 20
#req_maxturn 30
#req_land 1
#unrest 20
#end

#selectevent 438
#rarity 1
#req_fornation 20
#req_turn 15
#req_maxturn 35
#req_land 1
#kill 1
#unrest 5
#incdom 1
#end

#selectevent 439
#rarity 1
#req_fornation 20
#req_turn 1
#req_maxturn 50
#req_land 1
#kill 1
#unrest 5
#end

#selectevent 440
#rarity 2
#req_fornation 68
#req_turn 30
#req_commander 1
#req_magic 5
#req_lab 0
#assassin 1738
#end

#selectevent 441
#rarity 1
#req_fornation 20
#req_turn 25
#req_land 1
#req_minpop 50
#unrest 20
#end

#selectevent 442
#rarity 1
#req_fornation 20
#req_turn 25
#req_land 1
#req_minpop 50
#kill 10
#end

#selectevent 443
#rarity 2
#req_fornation 20
#req_turn 25
#req_land 1
#nation 0
#com 1714
#end

#selectevent 444
#rarity 2
#req_fornation 20
#req_turn 25
#req_maxturn 50
#req_land 1
#nation 0
#2com 1762
#6d6units 1763
#com 1735
#kill 5
#end

#selectevent 445
#rarity -2
#req_fornation 20
#req_turn 35
#req_land 1
#req_minpop 100
#2d6vis 8
#end

#selectevent 446
#rarity -1
#req_fornation 20
#req_turn 30
#req_land 1
#req_forest 1
#nation -2
#1d6units 1737
#end

#selectevent 447
#rarity 2
#req_fornation 20
#req_turn 35
#req_gem 8
#req_minpop 100
#gemloss 8
#end

#selectevent 448
#rarity 2
#req_fornation 20
#req_turn 30
#req_commander 1336  -- outside the command's range 0..1
#req_land 1
#nation 0
#2com 1738
#6d6units 1737
#1d6units 1747
#end

#selectevent 449
#rarity -2
#req_unique 1
#req_magic 0
#req_chaos 0
#magicitem 9
#end

#selectevent 450
#rarity -2
#req_magic 0
#req_fornation 20
#magicitem 9
#end

#selectevent 451
#rarity 1
#req_fornation 68
#req_chaos 1
#req_land 1
#req_minpop 50
#unrest 20
#end

#selectevent 452
#rarity 1
#req_fornation 68
#req_land 1
#kill 1
#unrest 5
#incdom 1
#end

#selectevent 453
#rarity -2
#req_magic 0
#req_fornation 68
#magicitem 9
#end

#selectevent 454
#rarity -2
#req_coast 1
#req_noseason 3
#magicitem 9
#kill 1
#end

#selectevent 455
#rarity -2
#req_mountain 1
#landgold 20
#incpop 20
#unrest 10
#end

#selectevent 456
#rarity -2
#req_pop0ok
#req_magic 3
#req_land 1
#3d6vis 1
#end

#selectevent 457
#rarity -2
#req_pop0ok
#req_magic 3
#req_land 0
#3d6vis 2
#end

#selectevent 458
#rarity 2
#req_pop0ok
#req_turn 30
#req_commander 1
#req_magic 0
#req_land 1
#assassin 526
#end

#selectevent 459
#rarity 2
#req_magic 1
#req_notfornation 89
#unrest 10
#stealthcom 443
#decscale 5
#end

#selectevent 460
#rarity -1
#req_order 3
#req_land 1
#req_minpop 100
#landgold 25
#landprod 20
#end

#selectevent 461
#rarity -2
#req_chaos 2
#req_minpop 10
#unrest 5
#decscale2 0
#end

#selectevent 462
#rarity 2
#req_turn 4
#req_chaos 2
#taxboost -100
#end

#selectevent 463
#rarity 2
#req_dominion 1
#req_turn 10
#req_minpop 10
#incdom -3
#kill 2
#end

#selectevent 464
#rarity 2
#incscale2 1
#end

#selectevent 465
#rarity -1
#req_order 1
#req_minpop 10
#landprod 25
#kill 1
#end

#selectevent 466
#rarity -2
#req_order 2
#req_minpop 10
#landgold 15
#unrest 10
#end

#selectevent 467
#rarity -2
#req_luck 2
#req_minpop 150
#landgold 15
#end

#selectevent 468
#rarity -2
#req_chaos 1
#req_minpop 150
#req_mountain 1
#req_magic 0
#landgold 15
#unrest 10
#incscale 0
#end

#selectevent 469
#rarity 2
#req_turn 4
#req_chaos 1
#req_minpop 150
#req_unluck 1
#taxboost -100
#unrest 30
#end

#selectevent 470
#rarity 1
#req_researcher
#curse 5
#end

#selectevent 471
#rarity -1
#req_pop0ok
#req_mintroops 10
#req_unluck 1
#curse 10
-- ro: effect 190 (gold) = 34
#end

#selectevent 472
#rarity 1
#req_swamp 1
#req_unluck 1
#req_turn 10
#disease 10
#kill 10
#end

#selectevent 473
#rarity 1
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_magic 1
#req_land 1
#req_targorder 4
#disease 5
#curse 3
#gainaff 4096
#gainaff 4294967296
#end

#selectevent 474
#rarity 2
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_targorder 4
#req_targsight 1
#req_turn 15
#gainaff 4096
#end

#selectevent 475
#rarity 2
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_unluck 1
#req_targorder 4
#req_turn 15
#gainaff 4194304
#gainaff 4096
#end

#selectevent 476
#rarity 2
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 1073741824
#end

#selectevent 477
#rarity 1
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 2
#end

#selectevent 478
#rarity 1
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_researcher
#req_turn 5
-- ro: effect 77 (researchaff) = 4294967296
#end

#selectevent 479
#rarity 1
#req_voidok 1
#req_pop0ok
#req_lab 1
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 4294967296
#end

#selectevent 480
#rarity 1
#req_lab 1
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 2097152
#end

#selectevent 481
#rarity 2
#req_lab 1
#req_unluck 2
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 4294967296
-- ro: effect 77 (researchaff) = 33554432
#end

#selectevent 482
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 15
-- ro: effect 39 = 6
#assassin 753
#end

#selectevent 483
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 15
#req_land 1
#req_noera 0  -- outside the command's range 1..3
-- ro: effect 39 = 6
#assassin 467
#end

#selectevent 484
#rarity 2
#req_researcher
#req_turn 15
-- ro: effect 39 = 3
#assassin 307
#end

#selectevent 485
#rarity 1
#req_voidok 1
#req_pop0ok
#req_unluck 1
#incscale 4
#end

#selectevent 486
#rarity -1
#req_voidok 1
#req_pop0ok
#req_luck 0
#decscale 4
#end

#selectevent 487
#rarity 1
#req_lazy 1
#req_minpop 20
#incscale 1
#end

#selectevent 488
#rarity 2
#req_lazy 1
#req_minpop 20
#incscale3 1
#end

#selectevent 489
#rarity 2
#req_lazy 1
#req_minpop 30
#req_heat 1
#req_growth 1
#incscale3 1
#landprod -10
#end

#selectevent 490
#rarity 1
#req_lazy 3
#req_season 3
#req_cave 0
#kill 10
#end

#selectevent 491
#rarity 1
#req_lazy 2
#req_season 1
#req_land 1
#incscale 3
#end

#selectevent 492
#rarity 2
#req_minunrest 20
#req_lazy 1
#landgold -8
#unrest -6
#end

#selectevent 493
#rarity -1
#req_researcher
#req_prod 2
#magicitem 1
#end

#selectevent 494
#rarity -2
#req_prod 2
#req_maxunrest 0
#req_minpop 80
#landprod 5
#landgold 10
#end

#selectevent 495
#rarity -1
#req_order 1
#req_prod 1
#req_minpop 20
#landgold 10
#defence 10
#end

#selectevent 496
#rarity -2
#req_order 3
#req_minpop 20
#landgold 15
#unrest -10
#end

#selectevent 497
#rarity 2
#req_chaos 3
#landgold -10
#end

#selectevent 498
#rarity 1
#req_death 1
#req_minpop 20
#req_turn 10
#kill 10
#disease 1
#end

#selectevent 499
#rarity 1
#req_freshwater 1
#req_land 1
#req_season 2
#req_heat 0
#kill 10
#addgeo 274877906944
#end

#selectevent 500
#rarity 1
#req_death 0
#req_turn 10
#kill 5
#incscale 3
#end

#selectevent 501
#rarity 1
#req_death 0
#req_unluck 1
#req_turn 10
#kill 15
#incscale 3
#end

#selectevent 502
#rarity 1
#req_freshwater 1
#req_land 1
#req_season 2
#req_unluck 1
#kill 10
#addgeo 274877906944
#end

#selectevent 503
#rarity 1
#req_death 1
#req_minpop 20
#req_unluck 1
#req_turn 10
#kill 10
#disease 2
#end

#selectevent 504
#rarity 1
#req_minpop 300
#req_land 0
#incscale 3
#emigration 5
#end

#selectevent 505
#rarity 1
#req_swamp 1
#req_heat 0
#kill 1
#disease 3
#end

#selectevent 506
#rarity 2
#req_voidok 1
#req_pop0ok
#req_gem 4
#req_unluck 1
#gemlosssmall 4
#end

#selectevent 507
#rarity -2
#req_monster 323
#req_prod 1
#magicitem 9
#end

#selectevent 508
#rarity -2
#req_monster 325
#req_prod 1
#magicitem 9
#end

#selectevent 509
#rarity -2
#req_monster 323
#req_prod 1
#magicitem 9
#end

#selectevent 510
#rarity -2
#req_monster 323
#req_prod 1
#magicitem 9
#end

#selectevent 511
#rarity 1
#req_turn 4
#req_chaos 2
#taxboost -100
#unrest 15
#end

#selectevent 512
#rarity 1
#req_pop0ok
#req_turn 12
#req_pathdeath 1
#assassin 566
#end

#selectevent 513
#rarity -1
#req_pathnature 1
#req_magic 1
#req_land 1
#nation -2
#1d6units 394
#end

#selectevent 514
#rarity -1
#req_pathwater 1
#req_magic 1
#req_land 0
#nation -2
#1d6units 1058
#end

#selectevent 515
#rarity -1
#req_pop0ok
#req_pathblood 1
#req_magic 1
#req_land 1
#nation -2
#1d6units 303
#end

#selectevent 516
#rarity -1
#req_pathearth 1
#req_magic 1
#req_land 1
#nation -2
#1d6units 345
#end

#selectevent 517
#rarity -1
#req_pop0ok
#req_land 1
#req_monster 518
#nation -2
#3d6units 518
#end

#selectevent 518
#rarity -1
#req_pop0ok
#req_land 0
#req_monster 564
#nation -2
#3d6units 564
#end

#selectevent 519
#rarity 2
#req_death 1
#req_unluck 1
#req_turn 16
#req_land 1
#incdom 1
#com 188
#addequip 2
#9d6units 194
#7d6units 195
#3d6units 189
#end

#selectevent 520
#rarity 2
#req_pop0ok
#req_death 1
#req_unluck 0
#req_turn 16
#req_land 1
#com 188
#addequip 2
#6d6units 194
#6d6units 195
#end

#selectevent 521
#rarity -1
#req_death 1
#req_fornation 54
#req_turn 16
#req_land 1
#req_pop0ok
#nation -2
#com 188
#addequip 2
#6d6units 194
#4d6units 189
#end

#selectevent 522
#rarity -2
#req_death 3
#req_luck 1
#req_turn 16
#req_land 1
#req_pop0ok
#nation -2
#com 188
#addequip 2
#6d6units 194
#4d6units 189
#end

#selectevent 523
#rarity 2
#req_pathblood 1
#req_turn 20
#req_lab 1
#req_land 1
#assassin 303
#assassin 303
#assassin 303
#assassin 303
#end

#selectevent 524
#rarity -2
#req_voidok 1
#req_pop0ok
#req_pathblood 1
#req_land 1
#req_chaos 1
#req_turn 10
#nation -2
#1d6units 304
#kill 10
#end

#selectevent 525
#rarity -1
#req_pathnature 1
#req_forest 1
#req_lab 0
#req_fort 0
#nation -2
#3d6units 592
#end

#selectevent 526
#rarity -1
#req_pathnature 1
#req_swamp 1
#req_lab 0
#req_fort 0
#nation -2
#com -13
#addequip 1
#end

#selectevent 527
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 750
#end

#selectevent 528
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 751
#end

#selectevent 529
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 752
#end

#selectevent 530
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 753
#end

#selectevent 531
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 754
#end

#selectevent 532
#rarity 2
#req_voidok 1
#req_pop0ok
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 755
#end

#selectevent 533
#rarity 2
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 756
#end

#selectevent 534
#rarity 2
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 757
#end

#selectevent 535
#rarity 2
#req_researcher
#req_unluck 2
#req_turn 20
-- ro: effect 39 = 6
#assassin 758
#end

#selectevent 536
#rarity 2
#req_order 3
#req_minpop 20
#req_turn 8
#nation -1
#newdom 3
#end

#selectevent 537
#rarity 2
-- ro: requirement 114 = 9
#req_order 3
#req_minpop 20
#emigration 20
#end

#selectevent 538
#rarity -1
#req_order 1
#req_minunrest 15
#req_minpop 20
#kill 2
#decscale2 0
#unrest -20
#end

#selectevent 539
#rarity 1
#req_chaos 2
#req_minpop 20
#incscale3 0
#unrest 10
#end

#selectevent 540
#rarity 2
#req_order 0
#req_minpop 20
#req_capital 0
#incscale3 0
#landgold -5
#landprod -5
#end

#selectevent 541
#rarity 2
#req_chaos 2
#req_minpop 20
#landgold -10
#end

#selectevent 542
#rarity -2
#req_order 2
#req_minpop 100
#landgold 10
#end

#selectevent 543
#rarity 2
#req_prod 2
#req_dominion 2
#req_minpop 20
#req_turn 8
#unrest 10
#incdom -2
#end

#selectevent 544
#rarity 1
#req_dominion 1
#req_maxdominion 4
#req_turn 8
#nation -1
#newdom 5
#end

#selectevent 545
#rarity 2
#req_dominion 1
#req_maxdominion 3
#req_minpop 20
#req_turn 8
#nation -1
#newdom 3
#end

#selectevent 546
#rarity 2
#req_dominion 1
#req_maxdominion 4
#req_minpop 20
#req_turn 8
#incdom -3
#end

#selectevent 547
#rarity 1
#req_forest 1
#req_minpop 5
#kill 1
#unrest 5
#end

#selectevent 548
#rarity -1
#req_fornation 27
#req_order 1
#req_land 1
#req_turn 8
#nation -2
#7d6units 172
#com 169
-- ro: effect 190 (gold) = -100
#end

#selectevent 549
#rarity -1
#req_swamp 1
#req_monster 937
#req_turn 8
#nation -2
#1d6units 1358
#end

#selectevent 550
#rarity -1
#req_swamp 1
#req_monster 937
#req_turn 8
#nation -2
#1d6units 403
#end

#selectevent 551
#rarity -1
#req_swamp 1
#req_monster 937
#req_turn 8
#nation -2
#1d6units 2196
#end

#selectevent 552
#rarity -1
#req_fornation 75
#req_order 1
#req_land 1
#req_turn 8
#nation -2
#7d6units 172
#com 169
-- ro: effect 190 (gold) = -100
#end

#selectevent 553
#rarity -1
#req_fornation 75
#req_swamp 1
#req_monster 937
#req_turn 8
#nation -2
#1d6units 1358
#end

#selectevent 554
#rarity -2
#req_fornation 75
#req_swamp 1
#req_monster 937
#req_turn 8
#nation -2
#1d6units 403
#end

#selectevent 555
#rarity -2
#req_fornation 75
#req_swamp 1
#req_monster 937
#req_turn 8
#nation -2
#1d6units 2196
#end

#selectevent 556
#rarity 1
#req_fornation 75
#req_swamp 1
#req_turn 10
#kill 25
#end

#selectevent 557
#rarity 1
#req_season 2
#req_swamp 1
#req_fort 0
#req_lab 0
#req_commander 1
-- ro: effect 39 = 2
#assassin 1358
#end

#selectevent 558
#rarity 1
#req_season 2
#req_swamp 1
#req_fort 0
#req_lab 0
#req_commander 1
-- ro: effect 39 = 2
#assassin 2196
#end

#selectevent 559
#rarity 2
#req_swamp 1
#req_turn 7
#req_fort 0
#com 552
#addequip 9
#4d6units 1358
#1d6units 578
#end

#selectevent 560
#rarity 1
#req_swamp 1
#req_minpop 5
#kill 1
#unrest 5
#end

#selectevent 561
#rarity -2
#req_waste 1
#req_magic 1
#magicitem 3
#end

#selectevent 562
#rarity 2
#req_turn 20
#req_land 1
#req_pathdeath 1
#incscale3 3
#com 676
#addequip 3
#12d6units 675
#12d6units 676
#end

#selectevent 563
#rarity -2
#req_forest 1
#req_order 0
#req_magic 0
#req_noseason 3
#magicitem 9
#end

#selectevent 564
#rarity -1
#req_voidok 1
#req_pop0ok
#req_hiddensite 1
#decscale 5
#end

#selectevent 565
#rarity -1
#req_foundsite 1
#nation -2
#6d6units 107
#com 104
#end

#selectevent 566
#rarity -1
#req_foundsite 1
#nation -2
#com 341
#incdom 1
#end

#selectevent 567
#rarity 1
#req_pop0ok
#req_foundsite 1
#req_pop0ok  -- stored twice; a second #req_pop0ok in a mod replaces the first
#com 3714
#end

#selectevent 568
#rarity -1
#req_foundsite 1
#req_season 2
#req_cave 0
-- ro: effect 190 (gold) = 600
#3d6vis 0
#end

#selectevent 569
#rarity -1
#req_hiddensite 1
-- ro: effect 190 (gold) = 300
#end

#selectevent 570
#rarity -2
#req_foundsite 1
#req_growth 1
#landgold 100
#end

#selectevent 571
#rarity 1
#req_pop0ok
#req_site 1
#req_noseason 3
#req_heat 0
#decscale2 2
#end

#selectevent 572
#rarity 1
#req_pop0ok
#req_foundsite 1
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 4096
-- ro: effect 190 (gold) = -200
#end

#selectevent 573
#rarity 1
#req_foundsite 1
-- ro: effect 190 (gold) = -200
#end

#selectevent 574
#rarity -1
#req_foundsite 1
#2d6vis 51
#end

#selectevent 575
#rarity -1
#req_hiddensite 1
#req_land 1
#nation -2
#com 551
-- ro: effect 190 (gold) = -30
#end

#selectevent 576
#rarity -1
#req_hiddensite 1
#decscale 5
#end

#selectevent 577
#rarity -1
#req_hiddensite 1
#nation -2
#com 478
#4d6units 527
#end

#selectevent 578
#rarity -1
#req_hiddensite 1
#end

#selectevent 579
#rarity 2
#req_rare 1
#end

#selectevent 580
#rarity 1
#req_site 1
#kill 10
#unrest 10
#end

#selectevent 581
#rarity 1
#req_site 1
#req_minpop 10
#req_noseason 3
#kill 20
#unrest 20
#incscale 2
#incscale 3
#end

#selectevent 582
#rarity 1
#req_pop0ok
#req_site 1
#com 640
#4d6units 640
#decscale 2
#end

#selectevent 583
#rarity -1
#req_pop0ok
#req_site 1
#nation -2
#com 99
#end

#selectevent 584
#rarity -2
#req_pop0ok
#req_nearbysite 1
#req_dominion 2
#req_land 1
#nation -2
#com 101
#end

#selectevent 585
#rarity 2
#req_pop0ok
#req_foundsite 1
#com 101
#addequip 3
#com 101
#com 101
#end

#selectevent 586
#rarity -1
#req_pop0ok
#req_nearbysite 1
#req_land 1
#end

#selectevent 587
#rarity -2
#req_pop0ok
#req_nearbysite 1
#req_dominion 2
#req_land 1
#nation -2
#com 100
#end

#selectevent 588
#rarity 2
#req_pop0ok
#req_foundsite 1
#com 100
#addequip 3
#com 100
#com 100
#end

#selectevent 589
#rarity 2
#req_pop0ok
#req_land 1
#req_rare 2
#end

#selectevent 590
#rarity -1
#req_pop0ok
#req_hiddensite 1
#end

#selectevent 591
#rarity -2
#req_pop0ok
#req_site 1
#nation -2
#com 312
#1d6units 527
#end

#selectevent 592
#rarity 2
#req_pop0ok
#req_foundsite 1
#com 466
#1d6units 2159
#4d6units 297
#com 3637
#end

#selectevent 593
#rarity 1
#req_pop0ok
#req_foundsite 1
#com 466
#1d6units 466
#end

#selectevent 594
#rarity 1
#req_foundsite 1
#com 514
#1d6units 514
#end

#selectevent 595
#rarity 1
#req_foundsite 1
#com 532
#1d6units 532
#end

#selectevent 596
#rarity 2
#req_hiddensite 1
#kill 1
#end

#selectevent 597
#rarity -1
#req_hiddensite 1
#end

#selectevent 598
#rarity -1
#req_pop0ok
#req_hiddensite 1
#req_noseason 3
#nation -2
#com 92
#addequip 1
#end

#selectevent 599
#rarity -1
#req_site 1
#req_land 1
#com 578
#3d6units 578
#com 578
#end

#selectevent 600
#rarity 1
#req_pop0ok
#req_site 1
#com 304
#3d6units 304
#end

#selectevent 601
#rarity 1
#req_site 1
#unrest 20
#end

#selectevent 602
#rarity 1
#req_site 1
#unrest 20
#end

#selectevent 603
#rarity -1
#req_pop0ok
#req_site 1
#nation -2
#com 304
#end

#selectevent 604
#rarity 1
#req_site 1
#req_monster 95
#assassin 304
#end

#selectevent 605
#rarity -1
#req_hiddensite 1
#end

#selectevent 606
#rarity 1
#req_foundsite 1
#req_pop0ok
#com 518
#3d6units 518
#end

#selectevent 607
#rarity 1
#req_foundsite 1
#com 782
#3d6units 782
#end

#selectevent 608
#rarity -1
#req_foundsite 1
#nation -2
#3d6units 962
#end

#selectevent 609
#rarity -1
#req_foundsite 1
#req_noera 1
#nation -2
#com 2359
#3d6units 2360
#end

#selectevent 610
#rarity -2
#req_site 1
#req_noera 1
#nation -2
#com 2332
#end

#selectevent 611
#rarity 1
#req_site 1
#com 453
#com 454
#3d6units 455
#1d6units 468
#3d6units 461
#end

#selectevent 612
#rarity 1
#req_pop0ok
#req_site 1
#com 714
#com 714
#1d6units 313
#1d6units 314
#1d6units 712
#end

#selectevent 613
#rarity -1
#req_monster 552
#req_land 1
#nation -2
#3d6units 549
#1d6units 463
#end

#selectevent 614
#rarity 1
#req_hiddensite 1
#req_minpop 10
#req_mydominion 1
#incdom -3
#decscale 2
#end

#selectevent 615
#rarity -2
#req_hiddensite 1
#req_minpop 10
#req_mydominion 1
#incdom -3
#decscale 2
#nation -2
#com 480
#end

#selectevent 616
#rarity -1
#req_foundsite 1
#req_season 3
#req_pop0ok
#3d6vis 0
#end

#selectevent 617
#rarity -1
#req_foundsite 1
#req_season 1
#req_pop0ok
#end

#selectevent 618
#rarity 2
#req_rare 1
#req_unluck 0
#decscale 2
#nation 0
#end

#selectevent 619
#rarity -1
#req_hiddensite 1
#decscale 2
#1d6vis 0
#end

#selectevent 620
#rarity -1
#req_season 1
#req_land 1
#decscale 2
#1d6vis 0
#end

#selectevent 621
#rarity -1
#req_hiddensite 1
#1d6vis 0
#end

#selectevent 622
#rarity -1
#req_site 1
#decscale 2
#magicitem 3
#end

#selectevent 623
#rarity 1
#req_site 1
#req_noseason 3
#com 633
#3d6units 633
#com 633
#end

#selectevent 624
#rarity -1
#req_nearbysite 1
#req_noera 1
#req_land 1
#nation -2
#com 1196
#end

#selectevent 625
#rarity -1
#req_nearbysite 1
#req_noera 1
#req_land 1
#nation -2
#com 1198
#end

#selectevent 626
#rarity -1
#req_foundsite 1
#req_noera 1
#nation -2
#com 1195
#4d6units 483
#incpop 10
#decscale 3
#end

#selectevent 627
#rarity 1
#req_foundsite 1
#req_turn 12
#req_noera 1
#com 272
#com 1198
#3d6units 1196
#3d6units 273
#4d6units 483
#end

#selectevent 628
#rarity -1
#req_hiddensite 1
#req_minpop 10
#1d6vis 6
#end

#selectevent 629
#rarity -2
#req_land 1
#req_forest 1
#req_minpop 10
#req_rare 5
#1d6vis 6
#end

#selectevent 630
#rarity -2
#req_hiddensite 1
#1d6vis 1
#1d6vis 0
#end

#selectevent 631
#rarity -1
#req_foundsite 1
#2d6vis 51
#end

#selectevent 632
#rarity -1
#req_foundsite 1
-- ro: effect 190 (gold) = 400
#end

#selectevent 633
#rarity -1
#req_foundsite 1
#nation -2
#com 325
#end

#selectevent 634
#rarity -1
#req_foundsite 1
#req_monster 325
#magicitem 4
#end

#selectevent 635
#rarity -1
#req_foundsite 1
#req_monster 325
#magicitem 3
#end

#selectevent 636
#rarity -1
#req_foundsite 1
#req_monster 325
#magicitem 2
#end

#selectevent 637
#rarity -1
#req_foundsite 1
#nation -2
#com 850
#addequip 3
#decscale 5
#end

#selectevent 638
#rarity -1
#req_foundsite 1
#req_magic 1
#nation -2
#com 848
#addequip 2
#1d6units 851
#3d6units 849
#end

#selectevent 639
#rarity 1
#req_foundsite 1
#req_unmagic 0
#com 848
#com 856
#addequip 1
#1d6units 851
#4d6units 849
#end

#selectevent 640
#rarity -1
#req_foundsite 1
#decscale2 5
#end

#selectevent 641
#rarity -1
#req_hiddensite 1
#req_minpop 10
#decscale 5
#end

#selectevent 642
#rarity 2
#req_land 1
#req_minpop 10
#req_rare 1
#decscale 5
#end

#selectevent 643
#rarity -1
#req_site 1
#req_minpop 10
#decscale 5
#end

#selectevent 644
#rarity 1
#req_site 1
#com 522
#1d6units 522
#end

#selectevent 645
#rarity -1
#req_foundsite 1
#nation -2
#1d6units 522
#end

#selectevent 646
#rarity 1
#req_hiddensite 1
#req_minpop 10
#unrest 15
#end

#selectevent 647
#rarity 1
#req_site 1
#com 2196
#1d6units 2196
#com 2196
#end

#selectevent 648
#rarity -1
#req_hiddensite 1
#req_pop0ok
#1d6vis 0
#1d6vis 2
#end

#selectevent 649
#rarity -1
#req_hiddensite 1
#1d6vis 0
#1d3vis 2
#end

#selectevent 650
#rarity -1
#req_hiddensite 1
#1d6vis 1
#revealsite
#end

#selectevent 651
#rarity -1
#req_site 1
#end

#selectevent 652
#rarity 1
#req_foundsite 1
#incdom -3
#unrest 10
#end

#selectevent 653
#rarity 1
#req_foundsite 1
#incdom -2
#unrest 10
#end

#selectevent 654
#rarity 1
#req_site 1
#incscale2 3
#decscale2 2
#end

#selectevent 655
#rarity 1
#req_site 1
#incscale2 3
#decscale2 2
#end

#selectevent 656
#rarity -1
#req_site 1
-- ro: effect 190 (gold) = 200
#end

#selectevent 657
#rarity -1
#req_foundsite 1
#req_season 2
#2d6vis 6
#2d6vis 1
#end

#selectevent 658
#rarity -1
#req_site 1
#req_luck 1
#req_turn 15
#req_pop0ok
#magicitem 3
#end

#selectevent 659
#rarity -1
#req_hiddensite 1
#end

#selectevent 660
#rarity 2
#req_nearbysite 1
#req_rare 50
#req_land 1
#end

#selectevent 661
#rarity -1
#req_pop0ok
#req_nearbysite 1
#req_land 1
#nation -2
#com 92
#end

#selectevent 662
#rarity -1
#req_pop0ok
#req_nearbysite 1
#req_land 1
#nation -2
#com 92
#end

#selectevent 663
#rarity -1
#req_hiddensite 1
#end

#selectevent 664
#rarity -1
#req_hiddensite 1
#end

#selectevent 665
#rarity 1
#req_hiddensite 1
#req_commander 1
#assassin 629
#end

#selectevent 666
#rarity 1
#req_hiddensite 1
#req_commander 1
#assassin 629
#unrest 20
#incscale 0
#end

#selectevent 667
#rarity -1
#req_hiddensite 1
#decscale 5
#end

#selectevent 668
#rarity 2
#req_land 1
#req_rare 1
#decscale 5
#end

#selectevent 669
#rarity 2
#req_foundsite 1
#req_commander 1
#assassin 629
#com 630
#addequip 1
#3d6units 629
#com 630
#end

#selectevent 670
#rarity -2
#req_pop0ok
#req_nearbysite 1
#req_land 1
#nation -2
#com 478
#end

#selectevent 671
#rarity -2
#req_pop0ok
#req_rare 25
#req_land 1
#nation -2
#com 478
#end

#selectevent 672
#rarity -2
#req_hiddensite 1
#req_minpop 10
#req_rare 25
#revealsite
#end

#selectevent 673
#rarity -1
#req_foundsite 1
#nation -2
#4d6units 517
#end

#selectevent 674
#rarity 0
#req_hiddensite 1
#req_rare 5
#req_unique 1
#decscale 5
#end

#selectevent 675
#rarity -1
#req_pop0ok
#req_hiddensite 1
#req_rare 25
#revealsite
#incscale3 3
#incscale 4
#end

#selectevent 676
#rarity -2
#req_nearbysite 1
#req_rare 10
#req_land 1
#nation -2
#com 311
#end

#selectevent 677
#rarity -1
#req_foundsite 1
#unrest -30
#decscale2 0
#end

#selectevent 678
#rarity -1
#req_site 1
-- ro: effect 190 (gold) = 300
#end

#selectevent 679
#rarity -2
#req_forest 1
#req_rare 5
-- ro: effect 190 (gold) = 300
#end

#selectevent 680
#rarity -2
#req_hiddensite 1
#end

#selectevent 681
#rarity -1
#req_foundsite 1
#2d6vis 1
#incscale2 0
#incdom -2
#end

#selectevent 682
#rarity -2
#req_hiddensite 1
#req_luck 1
#revealsite
#end

#selectevent 683
#rarity 1
#req_site 1
#incscale 3
#kill 8
#end

#selectevent 684
#rarity -2
#req_foundsite 1
#nation -2
#com 141
#addequip 3
#end

#selectevent 685
#rarity -2
#req_foundsite 1
#req_luck 2
#nation -2
#com 141
#addequip 4
#end

#selectevent 686
#rarity -2
#req_hiddensite 1
#revealsite
#end

#selectevent 687
#rarity -1
#req_pop0ok
#req_site 1
#2d6vis 4
-- ro: effect 190 (gold) = 100
#end

#selectevent 688
#rarity -2
#req_coast 1
#2d6vis 4
-- ro: effect 190 (gold) = 100
#end

#selectevent 689
#rarity -2
#req_coast 1
#req_luck 2
#4d6vis 4
-- ro: effect 190 (gold) = 200
#end

#selectevent 690
#rarity 2
#req_coast 1
#req_noseason 3
#req_turn 20
#req_noera 1
#com 208
#4d6units 1621
#4d6units 1681
#1d6units 206
#end

#selectevent 691
#rarity 1
#req_foundsite 1
#stealthcom 405
#kill 1
#end

#selectevent 692
#rarity 1
#req_foundsite 1
#req_pop0ok
#4com 198
#6d6units 198
#end

#selectevent 693
#rarity -2
#req_foundsite 1
#req_fornation 54
#req_pop0ok
#nation -2
#com 405
#3d6units 198
#end

#selectevent 694
#rarity -2
#req_foundsite 1
#req_death 2
#req_magic 1
#req_turn 10
#req_pop0ok
#nation -2
#3d6units 619
#3d6units 193
#end

#selectevent 695
#rarity -1
#req_hiddensite 1
#req_rare 50
#req_pop0ok
#4com 198
#6d6units 198
#revealsite
#end

#selectevent 696
#rarity 2
#req_land 1
#req_minpop 20
#req_commander 0
#req_maxtroops 0
#2com 488
#addequip 1
#end

#selectevent 697
#rarity 2
#req_hiddensite 1
#req_pathdeath 1
#revealsite
-- ro: effect 39 = 2
#assassin 533
#end

#selectevent 698
#rarity -1
#req_hiddensite 1
#end

#selectevent 699
#rarity 1
#req_hiddensite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 528
#end

#selectevent 700
#rarity 1
#req_hiddensite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 528
#end

#selectevent 701
#rarity 2
#req_land 0
#req_coast 0
#req_turn 15
#com 565
#end

#selectevent 702
#rarity 2
#req_land 0
#req_coast 0
#req_turn 15
#req_unluck 1
#com 639
#end

#selectevent 703
#rarity 2
#req_land 0
#req_coast 0
#com 438
#end

#selectevent 704
#rarity -2
#req_land 0
#req_coast 0
#req_pathwater 2
#nation -2
#com 438
#end

#selectevent 705
#rarity -2
#req_land 0
#req_coast 0
#req_pathwater 2
#nation -2
#com 565
#end

#selectevent 706
#rarity 2
#req_forest 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 884
#end

#selectevent 707
#rarity 2
#req_forest 1
#req_turn 13
#2com 884
#com 2220
#addequip 1
#3d6units 884
#6d6units 782
#6d6units 2223
#end

#selectevent 708
#rarity 2
#req_forest 1
#req_chaos 1
#com 2219
#3d6units 2219
#2com 2219
#end

#selectevent 709
#rarity 2
#req_land 1
#req_commander 1
#req_death 0
#assassin 2223
#end

#selectevent 710
#rarity -2
#req_land 1
#req_temple 1
#req_unique 3
#nation -2
#6d6units 2227
#end

#selectevent 711
#rarity -2
#req_nearbysite 1
#req_land 1
#req_rare 30
#nation -2
#com 354
#end

#selectevent 712
#rarity 1
#req_coast 1
#req_minpop 10
#req_notforally 127
#req_nation 127
#req_rare 20
#req_turn 8
#com 963
#4d6units 962
#nation 127
#newdom 3
#extramsg 127
#end

#selectevent 713
#rarity -1
#req_foundsite 1
#1d6vis 4
-- ro: effect 190 (gold) = 50
#end

#selectevent 714
#rarity 1
#req_foundsite 1
#2com 2472
#4d6units 2472
#1d6units 210
#end

#selectevent 715
#rarity 2
#req_land 1
#req_turn 8
#stealthcom 2275
#incdom -2
#end

#selectevent 716
#rarity 2
#req_pop0ok
#req_foundsite 1
#com 445
#addequip 1
#3d6units 424
#1d6units 425
#2com 443
#end

#selectevent 717
#rarity 2
#req_pop0ok
#req_hiddensite 1
#com 445
#addequip 1
#4d6units 424
#revealsite
#2com 443
#end

#selectevent 718
#rarity -1
#req_nearbysite 1
#req_land 1
#nation -2
#com 962
#end

#selectevent 719
#rarity 2
#req_rare 50
#req_land 1
#nation -2
#com 962
#end

#selectevent 720
#rarity -1
#req_rare 50
#req_foundsite 1
#nation -2
#com 962
#addequip 2
#addequip 1
-- ro: effect 190 (gold) = 20
#end

#selectevent 721
#rarity -2
#req_foundsite 1
#nation -2
#com 962
#addequip 3
#addequip 1
-- ro: effect 190 (gold) = 100
#end

#selectevent 722
#rarity -2
#req_rare 10
#req_foundsite 1
#req_luck 1
#nation -2
#com 962
#addequip 4
#addequip 1
-- ro: effect 190 (gold) = 300
#end

#selectevent 723
#rarity 2
#req_pop0ok
#req_foundsite 1
#com 445
#addequip 1
#3d6units 424
#1d6units 425
#end

#selectevent 724
#rarity 2
#req_pop0ok
#req_hiddensite 1
#com 445
#addequip 1
#6d6units 424
#revealsite
#2com 443
#end

#selectevent 725
#rarity 2
#req_foundsite 1
#com 445
#addequip 1
#3d6units 424
#1d6units 425
#4com 443
#end

#selectevent 726
#rarity 1
#req_foundsite 1
#req_rare 50
#2com 564
#3d6units 564
#end

#selectevent 727
#rarity -2
#req_foundsite 1
#req_monster 545
#nation -2
#4d6units 816
#end

#selectevent 728
#rarity -1
#req_land 0
#req_nearbysite 1
#nation -2
#com 545
#end

#selectevent 729
#rarity -1
#req_land 0
#req_nearbysite 1
#req_luck 1
#nation -2
#com 545
#addequip 2
#end

#selectevent 730
#rarity 2
#req_land 0
#nation -2
#com 545
#end

#selectevent 731
#rarity -2
#req_nearbysite 1
#req_land 0
#nation -2
#com 103
#end

#selectevent 732
#rarity -2
#req_monster 529
#req_land 0
#nation -2
#com 565
#end

#selectevent 733
#rarity -2
#req_monster 529
#req_land 0
#nation -2
#1d6units 438
#end

#selectevent 734
#rarity 1
#req_foundsite 1
#req_rare 50
#4com 1063
#6d6units 1063
#end

#selectevent 735
#rarity 1
#req_foundsite 1
#incdom -2
#decscale 5
#end

#selectevent 736
#rarity 0
#req_rare 10
#req_hiddensite 1
#revealsite
#incdom -2
#decscale 5
#end

#selectevent 737
#rarity 2
#req_unique 1
#req_foundsite 1
#req_magic 1
#com 751
#end

#selectevent 738
#rarity 1
#req_foundsite 1
#incdom -2
#decscale 5
#end

#selectevent 739
#rarity -2
#req_hiddensite 1
#req_order 2
#revealsite
#nation -2
#com 1059
#4d6units 1059
#end

#selectevent 740
#rarity -1
#req_site 1
#req_season 1
#req_growth 0
#1d6vis 6
#end

#selectevent 741
#rarity -1
#req_site 1
#req_season 1
#req_growth 2
#2d6vis 6
#end

#selectevent 742
#rarity -1
#req_hiddensite 1
#req_rare 50
-- ro: effect 190 (gold) = 100
#1d6vis 6
#end

#selectevent 743
#rarity -1
#req_foundsite 1
-- ro: effect 190 (gold) = 50
#1d6vis 6
#1d6vis 3
#end

#selectevent 744
#rarity 2
#req_foundsite 1
#2com 576
#2com 575
#4d6units 573
#6d6units 574
#end

#selectevent 745
#rarity -1
#req_land 0
#req_nearbysite 1
#nation -2
#com 575
#end

#selectevent 746
#rarity 2
#req_foundsite 1
#2com 577
#7d6units 577
#nation -2
#2com 576
#6d6units 573
#end

#selectevent 747
#rarity -2
#req_foundsite 1
#nation -2
#1d6units 1058
#end

#selectevent 748
#rarity 1
#req_foundsite 1
#req_rare 50
#com 1061
#4com 1060
#3d6units 1064
#4d6units 1057
#end

#selectevent 749
#rarity -2
#req_foundsite 1
#req_magic 1
#req_growth 1
#req_dominion 2
#nation -2
#com 1061
#2com 1060
#3d6units 1064
#4d6units 1057
#end

#selectevent 750
#rarity -2
#req_hiddensite 1
#req_luck 1
#nation -2
#1d6units 1062
#revealsite
#end

#selectevent 751
#rarity 1
#req_foundsite 1
#req_rare 50
#4com 1063
#6d6units 1063
#nation -2
#1d6units 1062
#end

#selectevent 752
#rarity 2
#req_foundsite 1
#2com 576
#2com 575
#4d6units 573
#6d6units 574
#end

#selectevent 753
#rarity -1
#req_land 0
#req_nearbysite 1
#nation -2
#com 575
#end

#selectevent 754
#rarity 2
#req_foundsite 1
#2com 577
#7d6units 577
#nation -2
#2com 575
#6d6units 573
#end

#selectevent 755
#rarity -2
#req_hiddensite 1
#req_luck 1
#revealsite
#end

#selectevent 756
#rarity -2
#req_hiddensite 1
#req_death 1
#revealsite
#end

#selectevent 757
#rarity -1
#req_foundsite 1
-- ro: effect 190 (gold) = 300
#end

#selectevent 758
#rarity -1
#req_foundsite 1
#req_luck 1
-- ro: effect 190 (gold) = 500
#end

#selectevent 759
#rarity -1
#req_foundsite 1
#2d6vis -1
-- ro: effect 190 (gold) = 50
#end

#selectevent 760
#rarity -1
#req_foundsite 1
#req_luck 1
#3d6vis -1
-- ro: effect 190 (gold) = 350
#end

#selectevent 761
#rarity -1
#req_foundsite 1
#2d6vis -1
#magicitem 2
#end

#selectevent 762
#rarity -2
#req_foundsite 1
#req_luck 1
#1d6vis 4
#magicitem 3
#end

#selectevent 763
#rarity -2
#req_nearbysite 1
#req_coast 1
#req_noera 1
#nation -2
#com 870
#4d6units 871
#end

#selectevent 764
#rarity -2
#req_nearbysite 1
#req_era 3
#req_coast 1
#nation -2
#com 1030
#com 1031
#end

#selectevent 765
#rarity -2
#req_nearbysite 1
#req_coast 1
#req_noera 1
#nation -2
#com 870
#4d6units 871
#end

#selectevent 766
#rarity -2
#req_rare 25
#req_coast 1
#req_noera 1
#nation -2
#com 870
#4d6units 871
#end

#selectevent 767
#rarity -2
#req_rare 10
#req_era 3
#req_coast 1
#req_noera 1
#nation -2
#com 1030
#com 1031
#end

#selectevent 768
#rarity 1
#req_foundsite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 2159
#end

#selectevent 769
#rarity 2
#req_mountain 1
#req_rare 50
#req_commander 1
-- ro: effect 39 = 2
#assassin 2159
#end

#selectevent 770
#rarity 2
#req_hiddensite 1
#req_commander 1
#req_luck 1
-- ro: effect 39 = 2
#assassin 2159
#revealsite
#end

#selectevent 771
#rarity 2
#req_pop0ok
#req_hiddensite 1
#req_commander 1
#req_luck 1
-- ro: effect 39 = 2
#assassin 2159
#revealsite
#end

#selectevent 772
#rarity 2
#req_foundsite 1
#3d6units 447
#1d6units 522
#2com 2135
#end

#selectevent 773
#rarity 2
#req_hiddensite 1
#req_luck 1
#3d6units 447
#1d6units 522
#2com 2135
#end

#selectevent 774
#rarity -1
#req_nearbysite 1
#req_coast 1
#end

#selectevent 775
#rarity -1
#req_nearbysite 1
#req_coast 1
#1d6vis 5
#end

#selectevent 776
#rarity 1
#req_foundsite 1
#2com 966
#3d6units 966
#end

#selectevent 777
#rarity 1
#req_hiddensite 1
#req_luck 1
#2com 966
#3d6units 966
#revealsite
#end

#selectevent 778
#rarity -2
#req_nearbysite 1
#req_magic 1
#req_luck 1
#nation -2
#com 104
#end

#selectevent 779
#rarity -1
#req_foundsite 1
#nation -2
#com 1059
#end

#selectevent 780
#rarity -1
#req_foundsite 1
#nation -2
#com 676
#end

#selectevent 781
#rarity -2
#req_foundsite 1
#nation -2
#com 1059
#3d6units 1069
#end

#selectevent 782
#rarity -1
#req_nearbysite 1
#req_land 0
#nation -2
#com 1059
#end

#selectevent 783
#rarity -1
#req_nearbysite 1
#req_land 0
#nation -2
#com 576
#end

#selectevent 784
#rarity -1
#req_pop0ok
#req_hiddensite 1
#decscale2 2
#end

#selectevent 785
#rarity 2
#req_pop0ok
#req_land 0
#req_rare 1
#decscale2 2
#end

#selectevent 786
#rarity -1
#req_hiddensite 1
#decscale2 2
#end

#selectevent 787
#rarity 2
#req_hiddensite 1
#com 438
#revealsite
#end

#selectevent 788
#rarity 1
#req_hiddensite 1
#req_turn 15
#2com 438
#1d6units 438
#end

#selectevent 789
#rarity 2
#req_land 0
#req_turn 5
#2com 438
#1d6units 438
#end

#selectevent 790
#rarity -1
#req_hiddensite 1
#incscale3 2
#end

#selectevent 791
#rarity 2
#req_land 0
#incscale3 2
#end

#selectevent 792
#rarity -1
#req_hiddensite 1
#unrest 10
#end

#selectevent 793
#rarity 2
#req_land 0
#req_rare 1
#unrest 10
#end

#selectevent 794
#rarity 1
#req_hiddensite 1
#incscale 3
#unrest 3
#end

#selectevent 795
#rarity -1
#req_hiddensite 1
-- ro: effect 190 (gold) = 100
#decscale 5
#end

#selectevent 796
#rarity -1
#req_nearbysite 1
#req_land 0
#req_rare 20
-- ro: effect 190 (gold) = 100
#decscale 5
#end

#selectevent 797
#rarity 2
#req_foundsite 1
#req_rare 50
#4com 676
#7d6units 676
#end

#selectevent 798
#rarity -2
#req_pathblood 1
#req_magic 1
#req_land 1
#nation -2
#com 303
#end

#selectevent 799
#rarity -1
#req_monster 1536
#req_freesites 1
#req_land 1
#addsite -1
#kill 3
#decscale 2
#end

#selectevent 800
#rarity -1
#req_foundsite 1
#req_monster 1536
#nation -2
#1d6units 640
#end

#selectevent 801
#rarity -2
#req_site 1
#req_monster 86
#transform 1536
#pathboost 0
#end

#selectevent 802
#rarity -2
#req_site 1
#req_monster 85
#fireboost 86
#end

#selectevent 803
#rarity 2
#req_gem 8
#req_land 1
#req_monster 1538
#req_commander 1
-- ro: effect 39 = 6
#assassin 304
#end

#selectevent 804
#rarity 2
#req_gem 8
#req_land 1
#req_monster 89
#req_commander 1538  -- outside the command's range 0..1
-- ro: effect 39 = 6
#assassin 304
#end

#selectevent 805
#rarity 2
#req_fornation 104
#req_turn 15
#req_owncapital 1
#req_chaos -1
#req_commander 1
#assassin 1967
#assassin 1966
#unrest 15
#end

#selectevent 806
#rarity 1
#req_fornation 16
#req_owncapital 1
#unrest 15
#end

#selectevent 807
#rarity 1
#req_fornation 63
#req_owncapital 1
#unrest 15
#end

#selectevent 808
#rarity 1
#req_fornation 104
#req_owncapital 1
#unrest 15
#end

#selectevent 809
#rarity 2
#req_fornation 16
#req_land 1
#req_minunrest 10
#req_turn 15
#2com 1661
#6d6units 1661
#end

#selectevent 810
#rarity 2
#req_fornation 16
#req_land 1
#req_minunrest 10
#req_turn 15
#req_commander 1
#assassin 1537
#end

#selectevent 811
#rarity -2
#req_fornation 16
#req_owncapital 1
#nation -2
#com 1661
#end

#selectevent 812
#rarity 1
#req_fornation 16
#req_owncapital 1
#req_rare 50
#req_turn 15
#req_commander 1
-- ro: effect 39 = 2
#assassin 1661
#end

#selectevent 813
#rarity 1
#req_fornation 104
#req_owncapital 1
#req_rare 50
#req_turn 15
#req_commander 1
-- ro: effect 39 = 2
#assassin 1972
#end

#selectevent 814
#rarity 2
#req_fornation 63
#req_turn 15
#req_owncapital 1
#req_chaos -1
#req_commander 1
#assassin 429
#assassin 429
#unrest 15
#end

#selectevent 815
#rarity -1
#req_fornation 16
#req_fornation 63
#req_fornation 104
#req_owncapital 1
#req_season 1
#3d6vis 0
#decscale2 2
#end

#selectevent 816
#rarity -1
#req_fornation 104
#req_owncapital 1
#3d6vis 8
#end

#selectevent 817
#rarity -2
#req_monster 216
#req_lab 1
#magicitem 9
#end

#selectevent 818
#rarity -2
#req_monster 265
#req_lab 1
#magicitem 9
#end

#selectevent 819
#rarity -2
#req_monster 266
#req_lab 1
#magicitem 9
#end

#selectevent 820
#rarity -2
#req_monster 1538
#magicitem 9
#end

#selectevent 821
#rarity -2
#req_magic 3
#req_chaos 2
#req_luck 2
#req_freesites 1
#addsite -1
#end

#selectevent 822
#rarity -2
#req_order 2
#req_luck 2
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 823
#rarity -2
#req_pop0ok
#req_luck 2
#req_rare 15
#req_freesites 1
#req_land 1
#req_cave 0
#addsite -1
#kill 5
#end

#selectevent 824
#rarity -2
#req_death 2
#req_rare 15
#req_freesites 1
#req_land 1
#addsite -1
#emigration 2
#end

#selectevent 825
#rarity -2
#req_chaos 2
#req_rare 15
#req_luck 2
#req_freesites 1
#req_swamp 1
#addsite -1
#decscale2 5
#end

#selectevent 826
#rarity -2
#req_death 1
#req_magic 2
#req_rare 15
#req_freesites 1
#req_land 1
#addsite -1
#kill 15
#incscale3 3
#end

#selectevent 827
#rarity -2
#req_growth 3
#req_rare 10
#req_dominion 6
#req_freesites 1
#req_land 1
#addsite -1
#decscale 3
#end

#selectevent 828
#rarity -2
#req_growth 3
#req_rare 40
#req_season 1
#req_freesites 1
#req_forest 1
#addsite -1
#end

#selectevent 829
#rarity -1
#req_foundsite 1
#req_noseason 3
#decscale2 3
#decscale 5
#incscale2 0
#2d6vis 6
#end

#selectevent 830
#rarity 2
#req_chaos 2
#req_rare 50
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 831
#rarity -2
#req_land 1
#req_rare 5
#req_freesites 1
#req_unique 3
#addsite -1
#end

#selectevent 832
#rarity -2
#req_rare 15
#req_order 3
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 833
#rarity -2
#req_rare 15
#req_growth 2
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 834
#rarity -2
#req_rare 15
#req_luck 1
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 835
#rarity -2
#req_pop0ok
#req_rare 10
#req_heat 1
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 836
#rarity -2
#req_pop0ok
#req_rare 10
#req_heat 1
#req_freesites 1
#addsite -1
#end

#selectevent 837
#rarity -2
#req_magic 3
#req_chaos 2
#req_luck 3
#req_freesites 1
#req_unique 1
#addsite -1
#end

#selectevent 838
#rarity -2
#req_order 2
#req_luck 3
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 839
#rarity -2
#req_pop0ok
#req_luck 3
#req_rare 50
#req_freesites 1
#req_land 1
#req_unique 4
#req_cave 0
#addsite -1
#kill 5
#end

#selectevent 840
#rarity -2
#req_luck 3
#req_death 2
#req_unique 4
#req_freesites 1
#req_land 1
#addsite -1
#emigration 2
#end

#selectevent 841
#rarity -2
#req_luck 2
#req_death 1
#req_magic 2
#req_unique 3
#req_freesites 1
#req_land 1
#addsite -1
#kill 15
#incscale3 3
#end

#selectevent 842
#rarity -2
#req_luck 2
#req_growth 3
#req_unique 3
#req_season 1
#req_freesites 1
#req_forest 1
#addsite -1
#end

#selectevent 843
#rarity -2
#req_luck 3
#req_unique 3
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 844
#rarity -1
#req_rare 35
#req_order 3
#req_freesites 1
#req_land 1
#req_dominion 9
#req_unique 2
#addsite -1
#end

#selectevent 845
#rarity -2
#req_luck 2
#req_unique 4
#req_growth 2
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 846
#rarity -2
#req_unique 5
#req_luck 3
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 847
#rarity -2
#req_luck 2
#req_pop0ok
#req_unique 3
#req_heat 1
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 848
#rarity -2
#req_luck 2
#req_pop0ok
#req_unique 2
#req_heat 1
#req_freesites 1
#addsite -1
#end

#selectevent 849
#rarity -2
#req_luck 2
#req_chaos 2
#req_unique 5
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 850
#rarity -2
#req_luck 2
#req_magic 0
#req_unique 3
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 851
#rarity -2
#req_luck 2
#req_unique 3
#req_freesites 1
#req_land 1
#req_cave 0
#addsite -1
#end

#selectevent 852
#rarity -2
#req_unique 3
#req_freesites 1
#req_land 1
#req_growth 1
#req_dominion 7
#addsite -1
#end

#selectevent 853
#rarity -2
#req_rare 20
#req_fornation 16
#req_fornation 63
#req_fornation 104
#req_dominion 3
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 854
#rarity -2
#req_rare 20
#req_fornation 16
#req_fornation 63
#req_fornation 104
#req_dominion 3
#req_freesites 1
#req_mountain 1
#addsite -1
#kill 10
#end

#selectevent 855
#rarity -2
#req_rare 20
#req_fornation 16
#req_fornation 63
#req_fornation 104
#req_dominion 3
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 856
#rarity -2
#req_rare 20
#req_monster 323
#req_luck 1
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 857
#rarity -2
#req_rare 30
#req_monster 324
#req_luck 1
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 858
#rarity -2
#req_rare 30
#req_monster 324
#req_luck 1
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 859
#rarity -2
#req_rare 30
#req_monster 324
#req_luck 1
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 860
#rarity -2
#req_rare 30
#req_monster 324
#req_luck 1
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 861
#rarity -2
#req_rare 30
#req_monster 324
#req_luck 1
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 862
#rarity -2
#req_rare 30
#req_monster 1536
#req_temple 0
#req_freesites 1
#req_mountain 1
#addsite -1
#end

#selectevent 863
#rarity 2
#req_foundsite 1
#req_unluck 1
#4com 2225
#7d6units 2225
#end

#selectevent 864
#rarity -1
#req_foundsite 1
#nation -2
#4d6units 2232
#end

#selectevent 865
#rarity -2
#req_noseason 3
#req_land 1
#req_minpop 10
#req_fort 1
#nation -2
#com 1565
#4d6units 2227
#end

#selectevent 866
#rarity -1
#req_monster 214
#req_owncapital 1
#nation -2
#1d6units 213
#end

#selectevent 867
#rarity -1
#req_monster 214
#req_owncapital 1
#req_luck 1
#nation -2
#3d6units 213
#end

#selectevent 868
#rarity 2
#req_rare 50
#req_lab 1
#req_monster 1538
#req_turn 15
#req_commander 1
#req_land 1
#assassin 1661
#end

#selectevent 869
#rarity 2
#req_rare 10
#req_lab 1
#req_monster 89
#req_turn 15
#req_commander 1
#req_land 1
#2com 433
#addequip 2
#4d6units 433
#3d6units 526
#decscale 5
#end

#selectevent 870
#rarity -2
#req_monster 90
#req_land 1
#nation -2
#com 90
#end

#selectevent 871
#rarity -2
#req_monster 990
#req_land 1
#nation -2
#com 990
#end

#selectevent 872
#rarity -2
#req_rare 20
#req_hiddensite 1
#req_dominion 3
#req_fornation 63
#revealsite
#incdom 1
#end

#selectevent 873
#rarity -2
#req_rare 50
#req_magic 1
#req_owncapital 1
#req_fornation 63
#magicitem 9
#end

#selectevent 874
#rarity -2
#req_rare 20
#req_hiddensite 1
#req_dominion 3
#req_fornation 104
#revealsite
#incdom 1
#end

#selectevent 875
#rarity -2
#req_rare 50
#req_magic 1
#req_owncapital 1
#req_fornation 104
#magicitem 9
#end

#selectevent 876
#rarity -2
#req_rare 20
#req_hiddensite 1
#req_dominion 3
#req_fornation 63
#revealsite
#incdom 1
#end

#selectevent 877
#rarity -2
#req_rare 50
#req_magic 1
#req_owncapital 1
#req_fornation 63
#magicitem 9
#end

#selectevent 878
#rarity -2
#req_foundsite 1
#nation -2
#3d6units 243
#end

#selectevent 879
#rarity -2
#req_foundsite 1
#req_growth 1
#nation -2
#4d6units 210
#end

#selectevent 880
#rarity -1
#req_foundsite 1
#req_growth 0
#nation -2
#4d6units 284
#end

#selectevent 881
#rarity 1
#req_foundsite 1
#4com 1224
#12d6units 284
#end

#selectevent 882
#rarity -1
#req_foundsite 1
#req_growth 0
#nation -2
#4d6units 284
#end

#selectevent 883
#rarity 1
#req_foundsite 1
#4com 1224
#12d6units 284
#end

#selectevent 884
#rarity -1
#req_cold 1
#req_monster 1018
#req_forest 1
#nation -2
#1unit 1224
#end

#selectevent 885
#rarity -1
#req_cold 1
#req_monster 1018
#req_mountain 1
#nation -2
#1unit 1224
#end

#selectevent 886
#rarity -1
#req_cold 1
#req_monster 2140
#req_forest 1
#nation -2
#1unit 1309
#end

#selectevent 887
#rarity -1
#req_cold 1
#req_monster 2140
#req_mountain 1
#nation -2
#1unit 1309
#end

#selectevent 888
#rarity -2
#req_season 3
#req_land 1
#req_cold 0
#req_freesites 1
#req_rare 50
#req_site 0
#addsite -1
#end

#selectevent 889
#rarity -1
#req_monster 309
#req_season 3
#req_cold 1
#req_freesites 1
#req_mountain 1
#req_site 0
#addsite -1
#end

#selectevent 890
#rarity 1
#req_site 1
#decscale2 5
#decscale2 2
#end

#selectevent 891
#rarity 1
#req_site 1
#req_turn 15
#req_heat -2
#req_noseason 3
#req_magic 1
#com 1200
#addequip 1
#com 1201
#3d6units 1202
#4d6units 1203
#end

#selectevent 892
#rarity -1
#req_hiddensite 1
#req_noseason 3
#incscale2 2
#end

#selectevent 893
#rarity -1
#req_hiddensite 1
#decscale2 2
#decscale 5
#end

#selectevent 894
#rarity -1
#req_pop0ok
#req_hiddensite 1
#incscale2 3
#decscale 5
#end

#selectevent 895
#rarity 2
#req_land 1
#req_rare 1
#req_unique 2
#req_unluck -1
#incscale2 3
#decscale2 5
#end

#selectevent 896
#rarity -1
#req_hiddensite 1
#incscale2 3
#decscale 5
#end

#selectevent 897
#rarity -1
#req_hiddensite 1
#incscale2 3
#decscale 5
#end

#selectevent 898
#rarity -1
#req_nearbysite 1
#req_rare 50
#req_land 1
#nation -2
#com 962
#end

#selectevent 899
#rarity -2
#req_land 1
#req_rare 1
#req_unluck 1
#nation -2
#com 962
#end

#selectevent 900
#rarity -2
#req_land 1
#req_freesites 1
#req_magic 1
#req_rare 60
#addsite -1
#end

#selectevent 901
#rarity -2
#req_site 1
#req_monster 1699
#fireboost 1699
#end

#selectevent 902
#rarity -2
#req_site 1
#req_monster 86
#fireboost 86
#end

#selectevent 903
#rarity -2
#req_site 1
#req_monster 1536
#fireboost 1536
#end

#selectevent 904
#rarity -2
#req_site 1
#req_monster 1970
#fireboost 1970
#end

#selectevent 905
#rarity 2
#req_fornation 63
#req_chaos 1
#req_turn 20
#req_land 1
#com 119
#com 89
#addequip 1
#4d6units 81
#3d6units 82
#end

#selectevent 906
#rarity 2
#req_fornation 16
#req_chaos 1
#req_turn 20
#req_land 1
#com 119
#com 1538
#addequip 1
#4d6units 81
#3d6units 82
#end

#selectevent 907
#rarity 2
#req_fornation 104
#req_chaos 1
#req_turn 20
#req_land 1
#com 119
#com 89
#addequip 1
#4d6units 984
#3d6units 986
#end

#selectevent 908
#rarity -2
#req_rare 20
#req_hiddensite 1
#req_dominion 3
#req_fornation 16
#revealsite
#incdom 1
#end

#selectevent 909
#rarity -2
#req_rare 50
#req_magic 1
#req_owncapital 1
#req_fornation 16
#magicitem 9
#end

#selectevent 910
#rarity -1
#req_hiddensite 1
#req_growth 1
#req_season 1
#decscale2 3
#incdom -1
#end

#selectevent 911
#rarity -1
#req_rare 50
#req_hiddensite 1
#decscale2 3
#decscale 5
#end

#selectevent 912
#rarity -2
#req_forest 1
#req_freesites 1
#req_magic 1
#req_luck 0
#addsite -1
#end

#selectevent 913
#rarity -1
#req_site 1
#req_growth 1
#nation -2
#com 1198
#end

#selectevent 914
#rarity -2
#req_hiddensite 1
#unrest 1
#end

#selectevent 915
#rarity 2
#req_hiddensite 1
#req_turn 15
#4com 1705
#9d6units 1705
#end

#selectevent 916
#rarity -1
#req_hiddensite 1
#req_magic 0
#decscale3 5
#2d6vis 6
#end

#selectevent 917
#rarity -2
#req_forest 1
#req_magic 0
#req_rare 1
#req_unluck 1
#decscale3 5
#1d6vis 6
#end

#selectevent 918
#rarity -1
#req_minunrest 20
#req_minpop 20
#req_order 1
#unrest -15
#end

#selectevent 919
#rarity -1
#req_minunrest 20
#req_minpop 20
#req_order 1
#unrest -15
#end

#selectevent 920
#rarity -1
#req_minunrest 20
#req_minpop 20
#req_mindef 12
#req_order -1
#req_land 1
#unrest -30
#decscale2 0
#defence 10
#nation -2
#com 35
#end

#selectevent 921
#rarity -2
#req_era 1
#req_nearbysite 1
#req_land 1
#nation -2
#com 1460
#end

#selectevent 922
#rarity -2
#req_era 2
#req_nearbysite 1
#req_land 1
#nation -2
#com 1473
#end

#selectevent 923
#rarity -2
#req_land 1
#req_unluck 1
#nation -2
#1unit 1560
#end

#selectevent 924
#rarity 2
#req_land 1
#req_capital 0
#req_mindef 10
#req_maxtroops 20
#defence -8
#2com 1591
#gainaff 549755813888
#3d6units 28
#1d6units 38
#end

#selectevent 925
#rarity 1
#req_swamp 1
#req_monster 28
#killmon 28
#killmon 28
#killmon 28
#killmon 28
#killmon 28
#end

#selectevent 926
#rarity -2
#req_era 3
#req_swamp 1
#incdom 2
#nation -2
#com 1614
#4d6units 1613
#end

#selectevent 927
#rarity -2
#req_era 3
#req_land 1
#req_nearbysite 1
#nation -2
#com 1614
#end

#selectevent 928
#rarity 2
#req_era 3
#req_mountain 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 1769
#end

#selectevent 929
#rarity 2
#req_era 3
#req_mountain 1
#req_magic 1
#req_unluck 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 1768
#end

#selectevent 930
#rarity 2
#req_noera 1
#req_forest 1
#req_magic 0
#req_commander 1
-- ro: effect 39 = 2
#assassin 1775
#end

#selectevent 931
#rarity 2
#req_swamp 1
#req_noseason 3
#com 1831
#1d6units 1841
#com 1841
#end

#selectevent 932
#rarity 2
#req_forest 1
#req_minpop 10
#stealthcom 1910
#end

#selectevent 933
#rarity 1
#req_land 1
#req_minpop 10
#req_maxdef 4
#req_maxtroops 2
#req_chaos 0
#4com 482
#3d6units 482
#end

#selectevent 934
#rarity 2
#req_nation 89
#req_notforally 89
#req_maxdominion 2
#req_minpop 10
#req_turn 15
#req_coast 1
#nation 89
#com 1519
#addequip 1
#7d6units 962
#com 962
#extramsg 89
#end

#selectevent 935
#rarity 2
#req_nation 89
#req_notforally 89
#req_maxdominion 2
#req_minpop 10
#req_turn 15
#req_coast 1
#defence -15
#magicitem 9
#extramsg 89
#end

#selectevent 936
#rarity -2
#req_temple 1
#req_unique 1
#magicitem 9
#end

#selectevent 937
#rarity -2
#req_rare 1
#req_researcher
#magicitem 9
#end

#selectevent 938
#rarity -1
#req_foundsite 1
#req_monster 1465
#nation -2
#com 1469
#end

#selectevent 939
#rarity 1
#req_foundsite 1
#req_nation 59
#req_notforally 59
#nation 59
#com 1470
#com 1471
#6d6units 1465
#extramsg 59
#end

#selectevent 940
#rarity 1
#req_foundsite 1
#req_nation 59
#req_notforally 59
#nation 59
#newdom 3
#end

#selectevent 941
#rarity 1
#req_foundsite 1
#req_magic 0
#req_rare 50
#2com 924
#4d6units 924
#end

#selectevent 942
#rarity -2
#req_nearbysite 1
#req_land 1
#nation -2
#com 312
#end

#selectevent 943
#rarity -2
#req_rare 1
#req_land 1
#req_unluck 1
#nation -2
#com 312
#end

#selectevent 944
#rarity -1
#req_hiddensite 1
#req_season 3
#decscale 3
#decscale 5
#end

#selectevent 945
#rarity -1
#req_hiddensite 1
#decscale 3
#decscale 5
#end

#selectevent 946
#rarity 2
#req_rare 20
#req_season 3
#req_rare 3  -- stored twice; a second #req_rare in a mod replaces the first
#decscale 3
#decscale 5
#end

#selectevent 947
#rarity 2
#req_rare 1
#decscale 3
#decscale 5
#end

#selectevent 948
#rarity 1
#req_hiddensite 1
#req_turn 10
#com 518
#4d6units 518
#end

#selectevent 949
#rarity 2
#req_land 1
#req_rare 50
#req_turn 15
#com 518
#4d6units 518
#end

#selectevent 950
#rarity 1
#req_hiddensite 1
#req_turn 10
#com 447
#4d6units 447
#end

#selectevent 951
#rarity 2
#req_land 1
#req_rare 50
#req_turn 15
#com 447
#4d6units 447
#end

#selectevent 952
#rarity -2
#req_hiddensite 1
#unrest 5
#end

#selectevent 953
#rarity 2
#req_land 1
#req_rare 1
#unrest 5
#end

#selectevent 954
#rarity -2
#req_nearbysite 1
#req_luck 1
#req_land 1
#nation -2
#com 312
#end

#selectevent 955
#rarity -2
#req_land 1
#req_rare 1
#nation -2
#com 312
#end

#selectevent 956
#rarity -2
#req_hiddensite 1
#1d6vis 3
#end

#selectevent 957
#rarity -2
#req_forest 1
#req_rare 1
#1d6vis 3
#end

#selectevent 958
#rarity 2
#req_site 1
#req_rare 20
#com 2245
#addequip 1
#1d6units 2233
#com 1649
#4d6units 2232
#end

#selectevent 959
#rarity -1
#req_monster 2245
#req_heat 1
#req_freesites 1
#req_waste 1
#addsite -1
#end

#selectevent 960
#rarity 2
#req_site 1
#req_rare 20
#4com 2233
#15d6units 2232
#end

#selectevent 961
#rarity 1
#req_site 1
#req_commander 0
#assassin 2232
#nation 0
#end

#selectevent 962
#rarity -1
#req_site 1
#req_monster 2245
#nation -2
#3d6units 2232
#1d6units 2233
#end

#selectevent 963
#rarity -2
#req_site 1
#req_monster 2245
#nation -2
#1d3units 524
#end

#selectevent 964
#rarity -1
#req_monster 1094
#req_hiddensite 1
#decscale 5
#end

#selectevent 965
#rarity -1
#req_hiddensite 1
#req_minpop 10
#req_chaos 0
#req_land 1
#end

#selectevent 966
#rarity -1
#req_foundsite 1
#magicitem 1
#end

#selectevent 967
#rarity -1
#req_foundsite 1
#req_luck 2
#magicitem 3
#end

#selectevent 968
#rarity 2
#req_hiddensite 1
#req_turn 15
#revealsite
#com 533
#9d6units 618
#12d6units 619
#2com 299
#end

#selectevent 969
#rarity 2
#req_foundsite 1
#req_turn 15
#com 533
#addequip 3
#9d6units 618
#12d6units 619
#2com 299
#end

#selectevent 970
#rarity -1
#req_pop0ok
#req_foundsite 1
#2d6vis 5
#end

#selectevent 971
#rarity -1
#req_pop0ok
#req_foundsite 1
#req_luck 3
#4d6vis 5
#end

#selectevent 972
#rarity 1
#req_foundsite 1
#req_commander 1
#req_rare 20
#assassin 1911
#end

#selectevent 973
#rarity 1
#req_foundsite 1
#req_turn 15
#req_unique 1
#stealthcom 1911
#end

#selectevent 974
#rarity 0
#req_monster 550
#req_magic 1
#req_lab 1
#req_rare 3
#req_land 1
#nation -2
#1d6units 1983
#end

#selectevent 975
#rarity 0
#req_monster 550
#req_magic 1
#req_lab 1
#req_rare 3
#req_land 1
#nation -2
#1d6units 2159
#end

#selectevent 976
#rarity -2
#req_land 1
#req_temple 1
#req_magic 1
#req_chaos 3
#req_minpop 10
#nation -2
#com 302
#end

#selectevent 977
#rarity -2
#req_land 1
#req_temple 1
#req_magic 1
#req_heat 1
#req_minpop 10
#nation -2
#com 98
#end

#selectevent 978
#rarity -2
#req_land 0
#req_temple 1
#req_magic 1
#req_chaos 1
#req_minpop 10
#nation -2
#com 96
#end

#selectevent 979
#rarity -2
#req_land 1
#req_temple 1
#req_magic 1
#req_chaos 1
#req_minpop 10
#nation -2
#com 94
#end

#selectevent 980
#rarity -2
#req_land 0
#req_temple 1
#req_magic 1
#req_luck 3
#req_minpop 10
#nation -2
#com 97
#end

#selectevent 981
#rarity -2
#req_land 1
#req_temple 1
#req_magic 1
#req_luck 3
#req_minpop 10
#nation -2
#com 95
#unrest 10
#end

#selectevent 982
#rarity -2
#req_foundsite 1
#nation -2
#com -13
#addequip 2
#end

#selectevent 983
#rarity -2
#req_foundsite 1
#req_luck 2
#nation -2
#com -13
#addequip 3
#end

#selectevent 984
#rarity -2
#req_foundsite 1
#req_luck 3
#nation -2
#com -13
#addequip 4
#end

#selectevent 985
#rarity -2
#req_hiddensite 1
#nation -2
#com 2330
#end

#selectevent 986
#rarity 2
#req_foundsite 1
#req_chaos 1
#req_maxdef 20
#com 2324
#com 2326
#com 2328
#addequip 1
#com 2329
#end

#selectevent 987
#rarity -2
#req_nearbysite 1
#req_land 1
#req_luck 1
#req_rare 30
#nation -2
#com 2329
#end

#selectevent 988
#rarity -2
#req_nearbysite 1
#req_land 1
#req_luck 1
#req_rare 30
#nation -2
#com 2329
#end

#selectevent 989
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2325
#end

#selectevent 990
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2323
#end

#selectevent 991
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2332
#end

#selectevent 992
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2325
#end

#selectevent 993
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2323
#end

#selectevent 994
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2332
#end

#selectevent 995
#rarity -2
#req_foundsite 1
#req_luck 1
#nation -2
#com 2326
#end

#selectevent 996
#rarity -2
#req_foundsite 1
#req_luck 1
#req_magic 0
#nation -2
#com 2329
#end

#selectevent 997
#rarity -2
#req_foundsite 1
#req_luck 1
#req_era 3
#nation -2
#com 2324
#end

#selectevent 998
#rarity -2
#req_foundsite 1
#nation -2
#com 2323
#end

#selectevent 999
#rarity -2
#req_foundsite 1
#nation -2
#com 2324
#end

#selectevent 1000
#rarity -2
#req_foundsite 1
#nation -2
#com 2325
#end

#selectevent 1001
#rarity -2
#req_foundsite 1
#nation -2
#com 2326
#end

#selectevent 1002
#rarity 2
#req_foundsite 1
#req_chaos 1
#req_maxdef 20
#com 2324
#com 2326
#com 2328
#addequip 1
#com 2329
#end

#selectevent 1003
#rarity 2
#req_foundsite 1
#req_chaos 1
#req_maxdef 20
#com 2324
#com 2326
#com 2328
#addequip 1
#com 2329
#end

#selectevent 1004
#rarity 2
#req_foundsite 1
#req_chaos 1
#req_maxdef 20
#com 2324
#com 2326
#com 2328
#addequip 1
#com 2329
#end

#selectevent 1005
#rarity 1
#req_foundsite 1
#req_maxdef 25
#req_maxtroops 40
#req_fort 0
#nation -2
#com -13
#com -13
#nation 0
#com 2212
#7d6units 2212
#end

#selectevent 1006
#rarity 1
#req_foundsite 1
#req_fort 0
#nation -2
#com -13
#com -13
#nation 0
#com 2213
#end

#selectevent 1007
#rarity 2
#req_land 1
#req_maxdef 5
#req_maxtroops 5
#req_chaos 0
#2com 1912
#9d6units 482
#unrest 10
#end

#selectevent 1008
#rarity 2
#req_land 1
#req_maxdef 3
#req_maxtroops 5
#req_chaos 1
#2com 1912
#9d6units 482
#unrest 10
#end

#selectevent 1009
#rarity 1
#req_land 1
#req_maxdef 5
#req_maxtroops 5
#req_chaos 1
#2com 1912
#9d6units 482
#unrest 10
#end

#selectevent 1010
#rarity 1
#req_land 1
#req_maxdef 3
#req_maxtroops 5
#req_chaos 2
#2com 1912
#9d6units 482
#unrest 10
#end

#selectevent 1011
#rarity 1
#req_land 1
#req_maxdef 5
#req_maxtroops 5
#req_hiddensite 1
#2com 1912
#9d6units 482
#unrest 10
#end

#selectevent 1012
#rarity 1
#req_land 1
#req_maxdef 3
#req_maxtroops 5
#req_hiddensite 1
#2com 1912
#9d6units 482
#unrest 10
#end

#selectevent 1013
#rarity 2
#req_land 1
#req_chaos 3
#req_rare 50
#req_freesites 1
#addsite -1
#end

#selectevent 1014
#rarity 2
#req_land 1
#req_chaos 1
#req_rare 10
#req_freesites 1
#addsite -1
#end

#selectevent 1015
#rarity -2
#req_foundsite 1
#nation -2
#com 2323
#end

#selectevent 1016
#rarity -2
#req_foundsite 1
#nation -2
#com 2325
#end

#selectevent 1017
#rarity -2
#req_foundsite 1
#nation -2
#com 2329
#addequip 2
#end

#selectevent 1018
#rarity 2
#req_chaos 3
#req_minpop 50
#landgold -10
#end

#selectevent 1019
#rarity 2
#req_chaos 2
#req_minpop 50
#landgold -8
#end

#selectevent 1020
#rarity -2
#req_chaos 3
#req_luck 3
#req_freesites 1
#req_land 1
#addsite -1
#end

#selectevent 1021
#rarity -2
#req_chaos 3
#req_luck 3
#req_freesites 1
#req_land 0
#addsite -1
#end

#selectevent 1022
#rarity -2
#req_chaos 3
#req_luck 3
#req_freesites 1
#req_land 0
#addsite -1
#decscale 2
#end

#selectevent 1023
#rarity -2
#req_chaos 3
#req_luck 3
#req_freesites 1
#req_land 0
#req_cold 1
#addsite -1
#incscale 2
#end

#selectevent 1024
#rarity -2
#req_foundsite 1
#req_chaos 3
#req_luck 3
#req_magic 2
#3d6vis 0
#decscale 2
#end

#selectevent 1025
#rarity 2
#req_era 3
#req_order 2
#req_land 1
#req_rare 30
#req_turn 15
#com 23
#15d6units 39
#4d6units 22
#com 302
#addequip 1
#end

#selectevent 1026
#rarity 2
#req_era 2
#req_order 2
#req_land 1
#req_rare 30
#req_turn 15
#com 23
#15d6units 19
#4d6units 22
#com 302
#addequip 1
#end

#selectevent 1027
#rarity 2
#req_era 2
#req_order 2
#req_land 0
#req_rare 30
#req_turn 15
#com 1060
#15d6units 1066
#4d6units 1060
#com 1061
#addequip 2
#end

#selectevent 1028
#rarity 2
#req_era 3
#req_order 2
#req_land 0
#req_rare 30
#req_turn 15
#com 1060
#15d6units 1066
#4d6units 1060
#com 1061
#addequip 2
#end

#selectevent 1029
#rarity -2
#req_foundsite 1
#req_land 0
#req_magic 0
#nation -2
#com 1060
#addequip 2
#end

#selectevent 1030
#rarity -2
#req_foundsite 1
#req_land 0
#req_magic 0
#nation -2
#com 1060
#1d6vis 4
#end

#selectevent 1031
#rarity -2
#req_foundsite 1
#req_land 0
#req_magic 3
#nation -2
#com 1060
#2d6vis 4
#end

#selectevent 1032
#rarity -2
#req_foundsite 1
#req_land 0
#req_magic 0
#req_luck 3
#nation -2
#com 1060
#addequip 3
#addequip 2
#end

#selectevent 1033
#rarity -2
#req_foundsite 1
#req_land 0
#req_pathastral 1
#magicitem 2
#end

#selectevent 1034
#rarity -2
#req_pop0ok
#req_foundsite 1
#req_land 0
#req_pathastral 1
#req_luck 3
#magicitem 3
#1d6vis 4
#end

#selectevent 1035
#rarity -2
#req_foundsite 1
#req_land 0
#req_pathearth 1
#req_luck 2
#1d6vis 3
#end

#selectevent 1036
#rarity 2
#req_forest 1
#req_growth 0
#req_chaos 2
#req_noseason 3
#stealthcom 227
#12d6units 227
#unrest 10
#stealthcom 227
#end

#selectevent 1037
#rarity -2
#req_temple 1
#req_monster 240
#req_dominion 3
#holyboost 240
#end

#selectevent 1038
#rarity -2
#req_foundsite 1
#req_monster 310
#req_magic 2
#deathboost 310
#end

#selectevent 1039
#rarity -2
#req_foundsite 1
#req_monster 310
#req_magic 2
#deathboost 310
#end

#selectevent 1040
#rarity -2
#req_foundsite 1
#req_monster 310
#req_magic 2
#req_dominion 3
#holyboost 310
#end

#selectevent 1041
#rarity 2
#req_pop0ok
#req_foundsite 1
#req_pop0ok  -- stored twice; a second #req_pop0ok in a mod replaces the first
#com 310
#addequip 1
#6d6units 618
#com 625
#3d6units 619
#end

#selectevent 1042
#rarity 2
#req_hiddensite 1
#req_era 3
#revealsite
#com 253
#addequip 1
#15d6units 187
#2com 257
#end

#selectevent 1043
#rarity 2
#req_foundsite 1
#req_unluck 1
#req_era 3
#req_commander 1
-- ro: effect 39 = 2
#assassin 254
#end

#selectevent 1044
#rarity 2
#req_foundsite 1
#req_luck 2
#req_rare 50
#req_era 3
#req_commander 1
-- ro: effect 39 = 2
#assassin 254
#curse 4
#magicitem 4
#end

#selectevent 1045
#rarity 2
#req_foundsite 1
#req_era 3
#req_commander 1
-- ro: effect 39 = 2
#assassin 254
#curse 4
#magicitem 2
#end

#selectevent 1046
#rarity -2
#req_foundsite 1
#req_magic 1
#req_death 1
#req_pop0ok
#nation -2
#com 310
#end

#selectevent 1047
#rarity 2
#req_foundsite 1
#req_pop0ok
#com 310
#addequip 1
#6d6units 615
#4com 617
#3d6units 616
#end

#selectevent 1048
#rarity 2
#req_pop0ok
#req_foundsite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 1540
#end

#selectevent 1049
#rarity -2
#req_dominion 3
#req_order 3
#req_freesites 1
#req_rare 50
#req_era 2
#req_land 1
#addsite -1
#incdom 1
#end

#selectevent 1050
#rarity -1
#req_foundsite 1
#req_order 3
#req_unique 4
#landprod 5
#landgold 10
#end

#selectevent 1051
#rarity -2
#req_monster 2222
#req_land 1
#req_magic 2
#req_era 2
#killmon 2222
#nation -2
#com 2471
#end

#selectevent 1052
#rarity 2
#req_turn 15
#req_chaos -1
#req_poptype 25
#req_magic 1
#com 147
#2com 141
#12d6units 140
#com 363
#addequip 1
#end

#selectevent 1053
#rarity -2
#req_hiddensite 1
#req_forest 1
#nation -2
#com 2325
#end

#selectevent 1054
#rarity 2
#req_land 1
#req_forest 1
#req_rare 1
#nation -2
#com 2325
#end

#selectevent 1055
#rarity -2
#req_hiddensite 1
#req_rare 20
#end

#selectevent 1056
#rarity -2
#req_hiddensite 1
#req_mountain 1
#end

#selectevent 1057
#rarity 2
#req_rare 1
#req_mountain 1
#end

#selectevent 1058
#rarity -2
#req_hiddensite 1
#req_magic 1
#req_growth 1
#decscale 5
#end

#selectevent 1059
#rarity 2
#req_forest 1
#req_magic 1
#req_growth 1
#req_rare 2
#decscale 5
#end

#selectevent 1060
#rarity -2
#req_hiddensite 1
#req_swamp 1
#req_magic 0
#req_death -1
#req_hiddensite 1  -- stored twice; a second #req_hiddensite in a mod replaces the first
#revealsite
#incscale 3
#decscale 5
#end

#selectevent 1061
#rarity 2
#req_foundsite 1
#req_swamp 1
#req_magic 0
#req_death -1
#req_turn 25
#com 310
#gainaff 549755813888
#addequip 1
#3d6units 619
#6d6units 915
#1d6units 2222
#end

#selectevent 1062
#rarity -1
#req_nearbysite 1
#req_land 1
#req_fort 1
#nation -2
#com 348
#addequip 2
#end

#selectevent 1063
#rarity 2
#req_land 1
#req_order 3
#req_minpop 100
#req_turn 30
#req_mountain 0
#com 55
#4com 1313
-- ro: effect 183 (24d6units) = 1312
#unrest 25
#incscale 0
#end

#selectevent 1064
#rarity -2
#req_hiddensite 1
#req_luck 3
#req_chaos 2
#req_rare 50
#decscale 5
#end

#selectevent 1065
#rarity -2
#req_land 1
#req_death 1
#req_magic 2
#req_luck 2
#req_freesites 1
#addsite -1
#end

#selectevent 1066
#rarity 2
#req_fort 1
#req_unluck 2
#req_land 1
#req_capital 0
#req_freesites 1
#req_commander 1
#addsite -1
-- ro: effect 39 = 2
#assassin 1541
#end

#selectevent 1067
#rarity 1
#req_foundsite 1
#req_fort 1
#req_capital 0
#req_commander 1
-- ro: effect 39 = 2
#assassin 566
#end

#selectevent 1068
#rarity 1
#req_order 2
#taxboost -50
#end

#selectevent 1069
#rarity 1
#req_turn 4
#req_order 2
#req_unluck 2
#req_land 1
#taxboost -80
#landgold -10
#end

#selectevent 1070
#rarity 1
#req_order 2
#req_unluck 1
#taxboost -25
#end

#selectevent 1071
#rarity 1
#req_order 2
#req_unluck 1
#taxboost -40
#landgold -5
#end

#selectevent 1072
#rarity 1
#req_order 2
#req_death 1
#req_season 3
#req_cave 0
#unrest 20
#kill 3
#end

#selectevent 1073
#rarity 1
#req_order 2
#req_death 1
#req_season 0
#req_cave 0
#req_turn 10
#unrest 20
#kill 3
#end

#selectevent 1074
#rarity 2
#req_order 2
#req_death 1
#req_season 3
#req_cave 0
#unrest 20
#kill 6
#incscale 0
#end

#selectevent 1075
#rarity 2
#req_order 2
#req_death 1
#req_season 0
#req_cave 0
#unrest 20
#kill 3
#incscale 0
#end

#selectevent 1076
#rarity 2
#req_death 2
#req_unluck 1
#req_land 1
#req_rare 10
#req_turn 30
#incscale 3
#com 88
#addequip 1
#decscale3 2
#incdom -2
#end

#selectevent 1077
#rarity 1
#req_death 2
#req_unluck 1
#req_land 1
#req_rare 10
#req_turn 30
#req_maxpop 10
#incscale 3
#com 88
#addequip 1
#decscale3 2
#incdom -2
#end

#selectevent 1078
#rarity 2
#req_death 2
#req_unluck 1
#req_land 1
#req_rare 25
#req_turn 15
#incscale 3
#com 1662
#addequip 1
#end

#selectevent 1079
#rarity 2
#req_death 2
#req_unluck 1
#req_land 1
#req_rare 30
#req_turn 15
#incscale 3
#com 304
#end

#selectevent 1080
#rarity 1
#req_foundsite 1
#req_unluck 1
#com 305
#end

#selectevent 1081
#rarity 1
#req_foundsite 1
#com 526
#end

#selectevent 1082
#rarity 1
#req_foundsite 1
#req_turn 15
#req_magic 0
#4com 88
#4d6units 88
#end

#selectevent 1083
#rarity 1
#req_hiddensite 1
#req_turn 15
#req_magic 0
#req_chaos 2
#4com 88
#4d6units 88
#end

#selectevent 1084
#rarity 2
#req_land 1
#req_turn 50
#req_magic 3
#req_chaos 2
#req_freesites 1
#addsite -1
#4com 88
#12d6units 88
#end

#selectevent 1085
#rarity -2
#req_site 1
#req_luck 1
#nation -2
#com 2324
#end

#selectevent 1086
#rarity -1
#req_hiddensite 1
#end

#selectevent 1087
#rarity -2
#req_land 1
#req_rare 2
#end

#selectevent 1088
#rarity -2
#req_hiddensite 1
#end

#selectevent 1089
#rarity -1
#req_hiddensite 1
#req_pathblood 1
#end

#selectevent 1090
#rarity 2
#req_nearbysite 1
#req_land 1
#req_mindef 10
#com 1389
#4d6units 1353
#end

#selectevent 1091
#rarity 2
#req_nearbysite 1
#req_land 1
#com 1389
#4d6units 1353
#end

#selectevent 1092
#rarity -1
#req_lab 1
#req_magic 3
#req_rare 10
#req_monster 1389
#req_unique 2
#bloodboost 1389
#end

#selectevent 1093
#rarity -2
#req_foundsite 1
#req_magic 1
#req_luck 1
#req_monster 339
#req_rare 10
#bloodboost 339
#end

#selectevent 1094
#rarity -1
#req_foundsite 1
#req_magic 1
#req_luck 1
#req_monster 339
#req_unique 2
#bloodboost 339
#kill 1
#incscale2 0
#end

#selectevent 1095
#rarity -2
#req_nearbysite 1
#req_luck 2
#req_rare 10
#req_land 1
#nation -2
#com 339
#end

#selectevent 1096
#rarity 2
#req_foundsite 1
#req_monster 339
#req_commander 1
-- ro: effect 39 = 6
#assassin 88
#end

#selectevent 1097
#rarity 2
#req_foundsite 1
#req_monster 339
#req_commander 1
-- ro: effect 39 = 6
#assassin 304
#end

#selectevent 1098
#rarity 2
#req_foundsite 1
#req_monster 339
#req_commander 1
-- ro: effect 39 = 6
#assassin 433
#end

#selectevent 1099
#rarity -2
#req_foundsite 1
#req_monster 339
#req_magic 1
#nation -2
#1d3units 304
#kill 1
#incscale 0
#end

#selectevent 1100
#rarity -1
#req_foundsite 1
#req_monster 339
#req_magic 1
#req_unique 3
#nation -2
#1d3units 88
#kill 1
#incscale 0
#end

#selectevent 1101
#rarity -1
#req_foundsite 1
#req_monster 339
#req_magic 1
#req_unique 3
#nation -2
#1d3units 433
#kill 1
#incscale 0
#end

#selectevent 1102
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 30
#com 1369
#addequip 1
#decscale2 0
#end

#selectevent 1103
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 40
#2com 543
#4d6units 543
#decscale2 0
#end

#selectevent 1104
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 30
#com 1369
#addequip 1
#decscale2 0
#end

#selectevent 1105
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 40
#2com 543
#4d6units 543
#decscale2 0
#end

#selectevent 1106
#rarity -2
#req_foundsite 1
#req_magic 1
#req_luck 1
#req_monster 339
#req_rare 10
#bloodboost 339
#end

#selectevent 1107
#rarity -2
#req_foundsite 1
#req_magic 1
#req_luck 1
#req_monster 339
#req_rare 10
#bloodboost 339
#kill 1
#incscale2 0
#end

#selectevent 1108
#rarity -2
#req_nearbysite 1
#req_luck 2
#req_rare 10
#req_land 1
#nation -2
#com 339
#end

#selectevent 1109
#rarity 1
#req_foundsite 1
#req_monster 339
#req_commander 1
#req_unique 2
-- ro: effect 39 = 6
#assassin 88
#end

#selectevent 1110
#rarity 1
#req_foundsite 1
#req_monster 339
#req_commander 1
#req_unique 2
-- ro: effect 39 = 6
#assassin 304
#end

#selectevent 1111
#rarity 1
#req_foundsite 1
#req_monster 339
#req_commander 1
#req_unique 2
-- ro: effect 39 = 6
#assassin 433
#end

#selectevent 1112
#rarity -1
#req_foundsite 1
#req_monster 339
#req_magic 1
#req_unique 3
#nation -2
#1d3units 304
#kill 1
#incscale 0
#end

#selectevent 1113
#rarity -1
#req_foundsite 1
#req_monster 339
#req_magic 1
#req_unique 3
#nation -2
#1d3units 88
#kill 1
#incscale 0
#end

#selectevent 1114
#rarity -1
#req_foundsite 1
#req_monster 339
#req_magic 1
#req_unique 3
#nation -2
#1d3units 433
#kill 1
#incscale 0
#end

#selectevent 1115
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 30
#com 465
#addequip 1
#decscale2 0
#end

#selectevent 1116
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 40
#2com 543
#4d6units 543
#decscale2 0
#end

#selectevent 1117
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 30
#req_noera 1
#com 241
#2com 240
#4d6units 217
#4d6units 2360
#com 2359
#end

#selectevent 1118
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 40
#req_turn 25
#req_noera 1
#com 241
#4com 240
#12d6units 217
#7d6units 2359
#com 543
#end

#selectevent 1119
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 30
#req_noera 1
#com 241
#2com 240
#4d6units 217
#4d6units 2360
#com 2359
#end

#selectevent 1120
#rarity 1
#req_foundsite 1
#req_monster 339
#req_rare 40
#req_turn 25
#req_noera 1
#com 241
#4com 240
#12d6units 217
#7d6units 2359
#com 543
#end

#selectevent 1121
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 30
#req_noera 1
#com 241
#2com 240
#4d6units 217
#4d6units 2360
#com 2359
#end

#selectevent 1122
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 40
#req_turn 25
#req_noera 1
#com 241
#4com 240
#12d6units 217
#7d6units 2359
#com 2359
#end

#selectevent 1123
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 70
#req_minunrest 5
#req_minpop 20
#5com 1565
-- ro: effect 177 (18d6units) = 1565
#1d6units 1560
#1d6units 1560
#1d3units 2227
#end

#selectevent 1124
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 70
#req_minunrest 5
#req_minpop 20
#5com 1565
-- ro: effect 177 (18d6units) = 1565
#1d6units 1560
#1d6units 1560
#1d3units 2227
#end

#selectevent 1125
#rarity 1
#req_foundsite 1
#req_monster 95
#req_rare 70
#req_minunrest 5
#req_minpop 20
#5com 1565
-- ro: effect 177 (18d6units) = 1565
#1d6units 1560
#1d6units 1560
#1d3units 2227
#end

#selectevent 1126
#rarity 2
#req_notfornation 54
#req_monster 405
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#com 38
#end

#selectevent 1127
#rarity 2
#req_notfornation 54
#req_monster 191
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1128
#rarity 2
#req_notfornation 54
#req_monster 185
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1129
#rarity 2
#req_notfornation 54
#req_monster 405
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1130
#rarity 2
#req_notfornation 54
#req_monster 915
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1131
#rarity 2
#req_notfornation 54
#req_monster 916
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1132
#rarity 2
#req_notfornation 54
#req_monster 917
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1133
#rarity 2
#req_notfornation 54
#req_monster 196
#req_turn 15
#req_noera 1
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 241
#9d6units 1565
#3d6units 217
#3d6units 38
#com 2359
#end

#selectevent 1134
#rarity 2
#req_notfornation 54
#req_monster 197
#req_turn 15
#req_fort 0
#req_maxtroops 30
#req_pop0ok
#req_land 1
#com 240
#9d6units 1565
#3d6units 217
#3d6units 38
#end

#selectevent 1135
#rarity 2
#req_rare 30
#req_monster 405
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1136
#rarity 2
#req_rare 30
#req_monster 191
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1137
#rarity 2
#req_rare 30
#req_monster 185
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1138
#rarity 2
#req_rare 30
#req_monster 405
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1139
#rarity 2
#req_rare 30
#req_monster 915
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1140
#rarity 2
#req_rare 30
#req_monster 916
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2360
#com 241
#6d6units 2360
#4d6units 217
#com 240
#end

#selectevent 1141
#rarity 2
#req_rare 30
#req_monster 917
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1142
#rarity 2
#req_rare 30
#req_monster 196
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1143
#rarity 2
#req_rare 30
#req_monster 197
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1144
#rarity 2
#req_rare 30
#req_monster 197
#req_turn 15
#req_noera 1
#req_unluck 1
#req_pop0ok
#req_land 1
#4com 2359
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1145
#rarity 2
#req_foundsite 1
#req_monster 304
#req_turn 15
#req_noera 1
#2com 241
#4com 240
#4com 2359
#12d6units 40
#end

#selectevent 1146
#rarity 2
#req_foundsite 1
#req_monster 88
#req_turn 15
#req_noera 1
#2com 241
#4com 240
#4com 2359
#12d6units 40
#end

#selectevent 1147
#rarity 2
#req_foundsite 1
#req_monster 675
#req_turn 15
#req_noera 1
#2com 241
#4com 240
#4com 2359
#12d6units 40
#end

#selectevent 1148
#rarity 2
#req_foundsite 1
#req_monster 535
#req_turn 15
#req_noera 1
#2com 241
#4com 240
#4com 2359
#12d6units 40
#end

#selectevent 1149
#rarity 2
#req_rare 30
#req_monster 304
#req_turn 15
#req_noera 1
#req_unluck 1
#req_land 1
#4com 440
#com 241
#6d6units 2359
#4d6units 217
#com 240
#end

#selectevent 1150
#rarity -1
#req_foundsite 1
#nation -2
#com -13
#end

#selectevent 1151
#rarity 2
#req_foundsite 1
#req_chaos 0
#com 2323
#com 2324
#com 2325
#1d6units 1592
#6d6units 1565
#end

#selectevent 1152
#rarity -2
#req_foundsite 1
#nation -2
#com 2326
#addequip 2
#end

#selectevent 1153
#rarity -2
#req_foundsite 1
#req_luck 2
#nation -2
#com 2326
#addequip 3
#end

#selectevent 1154
#rarity -2
#req_foundsite 1
#req_luck 3
#nation -2
#com 2326
#addequip 3
#com 2332
#addequip 2
#end

#selectevent 1155
#rarity -2
#req_foundsite 1
#magicitem 2
#end

#selectevent 1156
#rarity -2
#req_foundsite 1
#req_magic 2
#req_luck 2
#magicitem 3
#end

#selectevent 1157
#rarity 1
#req_foundsite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 2159
#end

#selectevent 1158
#rarity -1
#req_foundsite 1
#req_nativesoil
#magicitem 2
#magicitem 2
-- ro: effect 190 (gold) = 50
#end

#selectevent 1159
#rarity -2
#req_foundsite 1
#req_nativesoil
#nation -2
#com 96
#end

#selectevent 1160
#rarity -2
#req_foundsite 1
#req_nativesoil
#req_luck 1
#nation -2
#com 97
#end

#selectevent 1161
#rarity -2
#req_poptype 38
#req_freesites 1
#req_land 1
#addsite -1
#defence 15
#end

#selectevent 1162
#rarity -2
#req_growth 2
#req_freesites 1
#req_minpop 30
#req_waste 0
#req_land 1
#addsite -1
#end

#selectevent 1163
#rarity -2
#req_fort 0
#req_order 1
#req_freesites 1
#req_minunrest 10
#req_noera 1
#req_land 0
#addsite -1
#unrest -10
#defence 10
#end

#selectevent 1164
#rarity -2
#req_fort 0
#req_order 1
#req_freesites 1
#req_minunrest 10
#req_noera 1
#req_land 1
#addsite -1
#unrest -10
#defence 10
#end

#selectevent 1165
#rarity 0
#req_site 1
#req_rare 2
#defence 5
#end

#selectevent 1166
#rarity 0
#req_site 1
#req_minunrest 10
#req_rare 30
#unrest -20
#end

#selectevent 1167
#rarity 0
#req_site 1
#req_rare 3
#magicitem 3
#end

#selectevent 1168
#rarity -1
#req_site 1
#req_order 2
#nation -2
#com 23
#3d6units 22
#end

#selectevent 1169
#rarity -1
#req_site 1
#req_order 2
#req_luck 1
#nation -2
#com 23
#9d6units 22
#end

#selectevent 1170
#rarity -1
#req_site 1
#defence 5
#end

#selectevent 1171
#rarity 0
#req_site 1
#req_minunrest 10
#req_rare 30
#unrest -20
#end

#selectevent 1172
#rarity 0
#req_site 1
#req_rare 3
#magicitem 3
#end

#selectevent 1173
#rarity -1
#req_site 1
#req_order 2
#nation -2
#com 1061
#3d6units 1060
#end

#selectevent 1174
#rarity -1
#req_site 1
#req_order 2
#req_luck 1
#nation -2
#com 1061
#6d6units 1060
#end

#selectevent 1175
#rarity -2
#req_poptype 25
#req_freesites 1
#req_minpop 10
#addsite -1
#unrest 20
#defence 10
#end

#selectevent 1176
#rarity 1
#req_poptype 25
#req_order 2
#req_minpop 10
#unrest 30
#incscale 0
#end

#selectevent 1177
#rarity 1
#req_commander 0
#req_poptype 25
#req_order 2
#req_maxtroops 0
#req_minpop 30
#revolt
#kill 3
#nation 4
#4com 147
#15d6units 139
#end

#selectevent 1178
#rarity -1
#req_site 1
#req_minpop 30
#unrest 15
#nation -2
#2com 141
#4d6units 140
#3d6units 139
#end

#selectevent 1179
#rarity -1
#req_poptype 25
#req_chaos 1
#req_mindef 5
#req_maxdef 20
#req_minpop 30
#nation 4
#2com 141
-- ro: effect 177 (18d6units) = 139
#nation -2
#2com 141
-- ro: effect 177 (18d6units) = 140
#end

#selectevent 1180
#rarity -1
#req_poptype 25
#req_chaos -1
#req_turn 20
#req_minpop 30
#addsite -1
#nation -2
#2com 141
#4d6units 140
#6d6units 139
#end

#selectevent 1181
#rarity -1
#req_poptype 25
#req_chaos -1
#nation -2
#com 147
#end

#selectevent 1182
#rarity -1
#req_poptype 25
#magicitem 9
#end

#selectevent 1183
#rarity -1
#req_poptype 78
#magicitem 9
#end

#selectevent 1184
#rarity -1
#req_poptype 78
#magicitem 9
#end

#selectevent 1185
#rarity -1
#req_poptype 62
#nation -2
#com 1594
#end

#selectevent 1186
#rarity -1
#req_poptype 62
#req_magic 1
#req_death 0
#nation -2
#com 1598
#end

#selectevent 1187
#rarity -1
#req_poptype 62
#req_magic 1
#req_growth 2
#req_chaos -1
#nation -2
#com 1598
#end

#selectevent 1188
#rarity -1
#req_poptype 79
#nation -2
#com 1593
#end

#selectevent 1189
#rarity -1
#req_poptype 79
#req_magic 1
#req_death 0
#nation -2
#com 1597
#end

#selectevent 1190
#rarity -1
#req_poptype 79
#req_magic 1
#req_growth 2
#req_chaos -1
#nation -2
#com 1597
#end

#selectevent 1191
#rarity -1
#req_poptype 79
#req_mountain 1
#req_freesites 1
#addsite -1
#end

#selectevent 1192
#rarity -1
#req_poptype 78
#req_forest 1
#req_freesites 1
#addsite -1
#end

#selectevent 1193
#rarity -1
#req_poptype 30
#req_chaos 1
#req_mindef 5
#req_maxtroops 20
#req_turn 15
#2com 23
#9d6units 22
#nation -2
#2com 23
#6d6units 22
#end

#selectevent 1194
#rarity 2
#req_fornation 57
#req_chaos 1
#req_turn 15
#req_capital 0
#req_minpop 30
#req_land 1
#com 56
#9d6units 55
#4d6units 53
#6d6units 1311
#end

#selectevent 1195
#rarity 1
#req_poptype 30
#req_chaos 1
#req_turn 15
#req_capital 0
#req_minpop 30
#req_land 1
#com 1313
#6d6units 1311
#6d6units 1312
#end

#selectevent 1196
#rarity 1
#req_turn 4
#req_poptype 30
#req_chaos 0
#req_forest 1
#req_fornation 57
#taxboost -100
#end

#selectevent 1197
#rarity -1
#req_poptype 36
#req_growth 2
#nation -2
#com 1514
#6d6units 423
#end

#selectevent 1198
#rarity 1
#req_poptype 36
#req_nation 27
#req_notforally 27
#req_turn 20
#req_chaos -1
#nation 27
#2com 1514
#15d6units 423
#com 502
#extramsg 27
#end

#selectevent 1199
#rarity 1
#req_poptype 36
#req_nation 75
#req_notforally 75
#req_turn 20
#req_chaos -1
#nation 75
#2com 1514
#15d6units 423
#com 502
#extramsg 75
#end

#selectevent 1200
#rarity 1
#req_poptype 34
#req_nation 24
#req_notforally 24
#req_turn 20
#req_chaos -1
#nation 24
#2com 252
#15d6units 205
#com 202
#extramsg 24
#end

#selectevent 1201
#rarity 1
#req_poptype 34
#req_nation 24
#req_notforally 24
#nation 24
#newdom 2
#end

#selectevent 1202
#rarity 1
#req_poptype 36
#req_nation 27
#req_notforally 27
#req_turn 20
#req_chaos -1
#nation 27
#newdom 2
#end

#selectevent 1203
#rarity 2
#req_turn 15
#req_coast 1
#req_nation 79
#nation 79
#com 2349
#3d6units 2349
#extramsg 79
#end

#selectevent 1204
#rarity -1
#req_turn 15
#req_owncapital 1
#req_fornation 79
#nation 79
#3d6units 2349
#magicitem 2
-- ro: effect 190 (gold) = 100
#end

#selectevent 1205
#rarity 1
#req_fornation 79
#req_turn 18
#req_nearbysite 1
#req_capital 0
#req_land 1
#4com 136
#12d6units 137
#end

#selectevent 1206
#rarity 1
#req_fornation 79
#req_turn 5
#req_nearbysite 1
#req_capital 0
#req_land 1
#kill 2
#emigration 10
#unrest 15
#end

#selectevent 1207
#rarity -1
#req_fornation 79
#req_owncapital 1
#unrest -10
#decscale2 5
#end

#selectevent 1208
#rarity 1
#req_fornation 79
#req_turn 5
#req_nearbysite 1
#req_capital 0
#req_land 1
#kill 3
-- ro: effect 190 (gold) = -100
#unrest 10
#end

#selectevent 1209
#rarity 1
#req_fornation 79
#req_temple 1
#req_capital 0
#req_land 1
#kill 1
#unrest 5
#temple 0
-- ro: effect 190 (gold) = -50
#end

#selectevent 1210
#rarity 2
#req_fornation 79
#req_freshwater 1
#req_magic 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 1954
#end

#selectevent 1211
#rarity 1
#req_monster 2350
#req_monster 2342  -- stored twice; a second #req_monster in a mod replaces the first
#unrest 10
#end

#selectevent 1212
#rarity -1
#req_monster 2352
#req_luck 1
#magicitem 9
#end

#selectevent 1213
#rarity -1
#req_monster 2352
#req_owncapital 1
#req_fornation 79
#nation -2
#com 2352
#end

#selectevent 1214
#rarity 1
#req_season 3
#req_nearbysite 1
#kill 2
#incscale3 2
#end

#selectevent 1215
#rarity -1
#req_fornation 79
#req_season 0
#req_nearbysite 1
#req_farm 1
#req_magic 2
#decscale3 3
#landprod 5
#landgold 5
#end

#selectevent 1216
#rarity -1
#req_fornation 79
#req_forest 1
#req_magic 1
#incscale3 4
#decscale2 3
#decscale 1
#end

#selectevent 1217
#rarity -1
#req_fornation 56
#req_owncapital 1
#req_turn 15
#nation -2
#3d6units 2349
#end

#selectevent 1218
#rarity 2
#req_turn 15
#req_freshwater 1
#req_nation 79
#req_minpop 30
#nation 79
#com 2349
#3d6units 2349
#extramsg 79
#end

#selectevent 1219
#rarity 2
#req_turn 15
#req_land 0
#req_fornation 79
#req_nearbysite 1
#com 1948
#end

#selectevent 1220
#rarity 1
#req_unmagic 2
#req_poptype 40
#req_mydominion 3  -- outside the command's range 0..1
#2com 346
#addequip 1
#2com 347
#9d6units 348
#4d6units 367
#end

#selectevent 1221
#rarity 1
#req_death 2
#req_poptype 42
#req_mydominion 3  -- outside the command's range 0..1
#2com 352
#addequip 1
#2com 353
#9d6units 354
#4d6units 370
#end

#selectevent 1222
#rarity 1
#req_cold 2
#req_poptype 41
#req_mydominion 3  -- outside the command's range 0..1
#2com 349
#addequip 1
#2com 350
#9d6units 351
#4d6units 612
#end

#selectevent 1223
#rarity 1
#req_chaos 2
#req_poptype 43
#req_mydominion 3  -- outside the command's range 0..1
#2com 355
#addequip 1
#2com 356
#9d6units 357
#4d6units 369
#end

#selectevent 1224
#rarity 1
#req_rare 50
#req_poptype 41
#2com 349
#addequip 1
#2com 350
#15d6units 351
#3d6units 612
#end

#selectevent 1225
#rarity -2
#req_luck 2
#req_poptype 40
#req_turn 20
#req_chaos -1
#1d6vis 4
-- ro: effect 190 (gold) = 50
#magicitem 9
#unrest 40
#end

#selectevent 1226
#rarity -2
#req_luck 2
#req_poptype 42
#req_turn 20
#req_chaos -1
#1d6vis 2
-- ro: effect 190 (gold) = 50
#magicitem 9
#unrest 40
#1d6vis 6
#end

#selectevent 1227
#rarity -2
#req_luck 2
#req_poptype 41
#req_turn 20
#req_chaos -1
#1d6vis 0
-- ro: effect 190 (gold) = 50
#magicitem 9
#unrest 40
#end

#selectevent 1228
#rarity -2
#req_luck 2
#req_poptype 43
#req_turn 20
#req_chaos -1
#1d6vis 3
-- ro: effect 190 (gold) = 50
#magicitem 9
#unrest 40
#end

#selectevent 1229
#rarity -1
#req_poptype 40
#req_chaos -1
#1d6vis 4
-- ro: effect 190 (gold) = 25
#magicitem 9
#unrest 40
#end

#selectevent 1230
#rarity -1
#req_poptype 42
#req_chaos -1
#1d6vis 4
-- ro: effect 190 (gold) = 25
#magicitem 9
#unrest 40
#end

#selectevent 1231
#rarity -1
#req_poptype 41
#req_chaos -1
#1d6vis 4
-- ro: effect 190 (gold) = 25
#magicitem 9
#unrest 40
#end

#selectevent 1232
#rarity -1
#req_poptype 43
#req_chaos -1
#1d6vis 4
-- ro: effect 190 (gold) = 25
#magicitem 9
#unrest 40
#end

#selectevent 1233
#rarity -1
#req_luck 1
#req_poptype 40
#nation -2
#com 348
#addequip 3
#end

#selectevent 1234
#rarity -1
#req_luck 1
#req_poptype 42
#nation -2
#com 354
#addequip 3
#end

#selectevent 1235
#rarity -1
#req_luck 1
#req_poptype 41
#nation -2
#com 351
#addequip 3
#end

#selectevent 1236
#rarity -1
#req_luck 1
#req_poptype 43
#nation -2
#com 357
#addequip 3
#end

#selectevent 1237
#rarity -1
#req_luck 1
#req_poptype 40
#nation -2
#com 367
#addequip 3
#end

#selectevent 1238
#rarity -1
#req_luck 1
#req_poptype 42
#nation -2
#com 370
#addequip 3
#end

#selectevent 1239
#rarity -1
#req_luck 1
#req_poptype 41
#nation -2
#com 612
#addequip 3
#end

#selectevent 1240
#rarity -1
#req_luck 1
#req_poptype 43
#nation -2
#com 369
#addequip 3
#end

#selectevent 1241
#rarity -2
#req_chaos -1
#req_minpop 10
#req_land 1
#1d6vis 6
#1d3vis 4
#end

#selectevent 1242
#rarity -2
#req_chaos -1
#req_minpop 10
#req_land 0
#1d6vis 6
#1d3vis 4
#end

#selectevent 1243
#rarity 1
#req_death 1
#req_site 1
#req_turn 16
#req_site 1  -- stored twice; a second #req_site in a mod replaces the first
#req_pop0ok
#com 188
#addequip 2
#12d6units 194
#6d6units 195
#4d6units 189
#end

#selectevent 1244
#rarity 1
#req_death 1
#req_site 1
#req_turn 16
#req_site 1  -- stored twice; a second #req_site in a mod replaces the first
#req_pop0ok
#com 188
#addequip 2
#12d6units 194
#6d6units 195
#4d6units 189
#end

#selectevent 1245
#rarity -1
#req_death 3
#req_luck 1
#req_turn 16
#req_site 1
#req_pop0ok
#nation -2
#com 188
#addequip 2
#6d6units 194
#4d6units 189
#end

#selectevent 1246
#rarity -1
#req_death 3
#req_luck 1
#req_turn 16
#req_site 1
#req_pop0ok
#nation -2
#com 188
#addequip 2
#6d6units 194
#4d6units 189
#end

#selectevent 1247
#rarity -1
#req_death 1
#req_fornation 54
#req_turn 16
#req_site 1
#req_pop0ok
#nation -2
#com 188
#addequip 2
#6d6units 194
#4d6units 189
#end

#selectevent 1248
#rarity -1
#req_death 1
#req_fornation 54
#req_turn 16
#req_site 1
#req_pop0ok
#nation -2
#com 188
#addequip 2
#6d6units 194
#4d6units 189
#end

#selectevent 1249
#rarity 1
#req_magic 1
#req_notforally 89
#req_site 1
#unrest 10
#stealthcom 443
#decscale 5
#end

#selectevent 1250
#rarity -1
#req_fornation 63
#req_temple 1
#magicitem 9
#end

#selectevent 1251
#rarity -1
#req_monster 2352
#req_forest 1
#nation -2
#7d6units 284
#end

#selectevent 1252
#rarity -1
#req_monster 2352
#req_mountain 1
#nation -2
#1unit 694
#end

#selectevent 1253
#rarity -1
#req_coast 1
#req_order 2
#req_minpop 200
-- ro: effect 190 (gold) = 100
#end

#selectevent 1254
#rarity -1
#req_coast 1
#req_luck 1
#req_minpop 200
#landgold 5
#end

#selectevent 1255
#rarity -2
#req_farm 1
#req_turn 4
#req_minpop 200
#req_era 1
#req_growth 0
#req_unique 1
#landprod 15
#landgold 15
#end

#selectevent 1256
#rarity -2
#req_farm 1
#req_turn 4
#req_minpop 200
#req_era 2
#req_growth 0
#req_unique 1
#landprod 15
#landgold 15
#end

#selectevent 1257
#rarity 1
#req_poptype 71
#req_chaos -1
#req_rare 30
#com 519
#addequip 1
#2com 1037
#3d6units 518
#1d3units 1086
#end

#selectevent 1258
#rarity -1
#req_hiddensite 1
#req_magic 0
#nation -2
#com 2323
#end

#selectevent 1259
#rarity -1
#req_poptype 76
#req_minpop 200
#req_growth 1
#landgold 5
#landprod 5
#end

#selectevent 1260
#rarity -1
#req_monster 2323
#req_land 1
#nation -2
#3d6units 40
#end

#selectevent 1261
#rarity -2
#req_farm 1
#req_turn 20
#req_minpop 200
#req_order 1
#req_growth 0
#landprod 5
#landgold 5
#end

#selectevent 1262
#rarity -2
#req_turn 20
#req_minpop 300
#req_prod 1
#req_era 3
#landprod 15
#end

#selectevent 1263
#rarity 2
#req_temple 1
#req_land 1
#req_unluck 2
#req_turn 15
#temple 0
#end

#selectevent 1264
#rarity 1
#req_temple 1
#req_land 1
#req_unluck 3
#req_turn 15
#temple 0
#end

#selectevent 1265
#rarity 1
#req_turn 4
#req_farm 1
#req_unluck 1
#req_growth -1
#req_season 1
#req_land 1
#req_cave 0
#taxboost -100
#unrest 10
#end

#selectevent 1266
#rarity 2
#req_magic 1
#req_land 1
#req_noseason 3
#incscale3 4
#unrest 25
#end

#selectevent 1267
#rarity 1
#req_unluck 1
#req_land 1
#req_cave 0
#req_noseason 3
#taxboost -50
#landgold -5
#end

#selectevent 1268
#rarity 2
#req_unluck -1
#req_land 1
#req_cave 0
#req_noseason 3
#taxboost -50
#landgold -5
#end

#selectevent 1269
#rarity 1
#req_temple 1
#req_land 1
#req_unluck 3
#req_turn 20
#temple 0
#end

#selectevent 1270
#rarity -2
#req_coast 1
#req_luck 3
#req_turn 15
#nation -2
#com 551
#1d6vis 3
#1d6vis 0
#magicitem 9
#end

#selectevent 1271
#rarity 1
#req_coast 1
#req_unluck 2
#req_chaos 1
#req_heat 0
#landgold -10
#end

#selectevent 1272
#rarity 1
#req_poptype 71
#req_chaos -1
#req_rare 30
#req_unluck 3
#com 519
#addequip 1
#2com 1037
#6d6units 518
#3d6units 1086
#end

#selectevent 1273
#rarity 2
#req_death 1
#req_unluck 3
#req_turn 16
#com 188
#4com 194
-- ro: effect 177 (18d6units) = 194
-- ro: effect 177 (18d6units) = 195
-- ro: effect 177 (18d6units) = 189
#end

#selectevent 1274
#rarity 1
#req_land 0
#req_noseason 3
#req_turn 20
#req_unluck 3
#2com 211
#9d6units 211
#6d6units 1681
#com 1681
#3d6units 2273
#end

#selectevent 1275
#rarity 1
#req_land 1
#req_cave 0
#req_season 3
#req_unluck 3
#2com 1224
#6d6units 1224
-- ro: effect 177 (18d6units) = 284
#kill 2
#unrest 15
#end

#selectevent 1276
#rarity 2
#req_minpop 200
#req_swamp 1
#req_season 2
#req_death 0
#kill 4
#end

#selectevent 1277
#rarity -2
#req_land 0
#req_turn 15
#req_unluck -1
#nation -2
#2com 2102
#10d6units 2101
#end

#selectevent 1278
#rarity 2
#req_land 1
#req_noera 1
#req_turn 15
#com 389
#2com 339
#addequip 1
#9d6units 287
#7d6units 286
#end

#selectevent 1279
#rarity 1
#req_foundsite 1
#req_turn 25
#nation -2
#com 477
#addequip 1
#end

#selectevent 1280
#rarity -1
#req_foundsite 1
#req_turn 25
#com 477
#addequip 1
#end

#selectevent 1281
#rarity 1
#req_foundsite 1
#req_turn 25
#nation -2
#com 99
#addequip 1
#end

#selectevent 1282
#rarity -1
#req_foundsite 1
#req_turn 25
#com 99
#addequip 1
#end

#selectevent 1283
#rarity 1
#req_foundsite 1
#req_turn 25
#nation -2
#com 97
#addequip 1
#end

#selectevent 1284
#rarity -1
#req_foundsite 1
#req_turn 25
#com 97
#addequip 1
#end

#selectevent 1285
#rarity 1
#req_foundsite 1
#req_turn 25
#nation -2
#com 93
#addequip 1
#end

#selectevent 1286
#rarity -1
#req_foundsite 1
#req_turn 25
#com 93
#addequip 1
#end

#selectevent 1287
#rarity 2
#req_foundsite 1
#req_turn 15
#req_magic 1
#2com 1202
#com 1201
#4d6units 1202
#4d6units 1203
#end

#selectevent 1288
#rarity 2
#req_growth 0
#req_magic 1
#req_minpop 20
#req_land 0
#incscale 0
#unrest 10
#end

#selectevent 1289
#rarity 2
#req_mountain 1
#req_mindef 2
#req_minpop 20
#req_magic -1
#com 2230
#end

#selectevent 1290
#rarity 1
#req_hiddensite 1
#req_unluck 1
#req_commander 1
-- ro: effect 39 = 2
#assassin 636
#end

#selectevent 1291
#rarity 1
#req_hiddensite 1
#com 636
#end

#selectevent 1292
#rarity 2
#req_foundsite 1
#2com 636
#end

#selectevent 1293
#rarity 1
#req_foundsite 1
#com 636
#end

#selectevent 1294
#rarity -2
#req_hiddensite 1
#end

#selectevent 1295
#rarity -2
#req_hiddensite 1
#end

#selectevent 1296
#rarity -2
#req_hiddensite 1
#end

#selectevent 1297
#rarity -2
#req_hiddensite 1
#end

#selectevent 1298
#rarity -1
#req_lab 1
#req_monster 95
#req_monster 94  -- stored twice; a second #req_monster in a mod replaces the first
#req_magic 1
#req_freesites 1
#req_unique 1
#addsite -1
#end

#selectevent 1299
#rarity 2
#req_waste 1
#req_rare 1
#req_unluck 2
#end

#selectevent 1300
#rarity -1
#req_hiddensite 1
#end

#selectevent 1301
#rarity 1
#req_lab 1
#req_researcher
#req_turn 15
-- ro: effect 77 (researchaff) = 4294967296
#end

#selectevent 1302
#rarity -1
#req_turn 20
#req_monster 1557
#req_land 1
#incdom 5
#nation -2
#4d6units 50
#end

#selectevent 1303
#rarity 1
#req_gem 4
#req_monster 239
#req_owncapital 1
-- ro: effect 49 = 4
#end

#selectevent 1304
#rarity 1
#req_turn 10
#req_monster 1645
#assassin 482
#end

#selectevent 1305
#rarity 1
#req_site 1
#req_era 3
#curse 1
#end

#selectevent 1306
#rarity 1
#req_site 1
#req_era 3
#req_unmagic 1
#req_gem 4
#gemloss 4
#end

#selectevent 1307
#rarity 1
#req_site 1
#req_era 3
#req_unmagic 1
#req_gem 6
#gemloss 6
#end

#selectevent 1308
#rarity 1
#req_site 1
#req_era 3
#req_unmagic 1
#req_gem 3
#gemloss 3
#end

#selectevent 1309
#rarity 2
#req_story 1
#req_turn 15
#req_fort 0
#req_temple 0
#req_minpop 50
#req_code 0
#req_poptype 25
#req_unique 5
#code 100
#unrest 15
#delay50 2
#end

#selectevent 1310
#rarity 0
#req_code 100
#flagland 1
#code 101
#unrest 15
#taxboost -20
#end

#selectevent 1311
#rarity 0
#req_code 101
#req_temple 1
#incdom 4
-- ro: effect 190 (gold) = 200
#unrest -40
#code 0
#end

#selectevent 1312
#rarity 0
#req_code 101
#req_temple 0
#req_maxunrest 5
#req_targorder 3
#req_mintroops 20
#req_rare 80
#kill 2
#code 0
#end

#selectevent 1313
#rarity 0
#req_code 101
#req_minunrest 5
#req_rare 10
#unrest 10
#code 102
#delay25 5
#end

#selectevent 1314
#rarity 0
#req_code 102
#req_poptype 25
#defence -20
#4com 141
#15d6units 140
#15d6units 139
#code 0
#end

#selectevent 1315
#rarity 0
#req_code 102
#req_rare 35
#taxboost -100
#unrest 15
#end

#selectevent 1316
#rarity 0
#req_targorder 3
#req_unique 1
#req_code 102
#req_mintroops 25
#unrest 10
#kill 1
#code 104
#end

#selectevent 1317
#rarity 0
#req_unique 3
#req_code 102
#req_rare 20
#kill 1
#unrest 13
#end

#selectevent 1318
#rarity 0
#req_code 102
#req_rare 10
#code 103
#end

#selectevent 1319
#rarity 0
#req_targorder 3
#req_code 101
#req_temple 0
#req_maxtroops 19
#kill 1
#unrest 15
#end

#selectevent 1320
#rarity 0
#req_code 102
#req_mindef 5
#req_minunrest 10
#req_rare 15
#defence -10
#unrest 10
#end

#selectevent 1321
#rarity 0
#req_code 102
#unrest 18
#end

#selectevent 1322
#rarity 0
#req_unique 1
#req_code 102
#req_commander 1
#req_poptype 25
#req_rare 17
-- ro: effect 39 = 2
#assassin 141
#end

#selectevent 1323
#rarity 0
#req_code 102
#req_commander 1
#req_poptype 25
#req_rare 7
#assassin 139
#end

#selectevent 1324
#rarity 0
#req_code 103
#req_targorder 3
#req_mintroops 10
#req_poptype 25
#req_rare 75
#com 147
#4com 141
#9d6units 140
#4d6units 139
#code 0
#end

#selectevent 1325
#rarity 0
#req_code 102
#req_maxtroops 0
#req_minunrest 30
#req_poptype 25
#req_rare 20
#revolt
#4com 141
#15d6units 140
#15d6units 139
#code 0
#end

#selectevent 1326
#rarity 0
#req_code 104
#req_mintroops 35
#req_rare 30
#code 103
#end

#selectevent 1327
#rarity 0
#req_nearbysite 1
#req_capital 0
#req_turn 10
#req_fornation 100
#req_code 0
#req_unique 1
#req_rare 5
#code 20
#decscale 5
#decscale 3
#flagland 1
#end

#selectevent 1328
#rarity 0
#req_code 20
#req_rare 15
#code 21
#decscale 5
#end

#selectevent 1329
#rarity 0
#req_monster 1643
#req_nomonster 1646
#req_code 21
#code 0
#incscale2 5
#incscale 3
#magicitem 9
#end

#selectevent 1330
#rarity 0
#req_monster 1646
#req_code 21
#code 22
#end

#selectevent 1331
#rarity 0
#req_monster 1646
#req_code 22
#req_rare 10
-- ro: effect 39 = 2
#assassin 633
#end

#selectevent 1332
#rarity 0
#req_monster 1646
#req_code 22
#req_rare 10
-- ro: effect 39 = 2
#assassin 362
#end

#selectevent 1333
#rarity 0
#req_monster 1646
#req_freesites 1
#req_unique 1
#req_code 22
#req_rare 5
#addsite -1
#decscale 5
#end

#selectevent 1334
#rarity 0
#req_monster 1646
#req_code 22
#req_rare 5
#magicitem 9
#end

#selectevent 1335
#rarity 0
#req_monster 1646
#req_noseason 3
#req_code 22
#req_rare 40
#code 23
#decscale2 5
#decscale2 3
#nation -2
#com 151
#end

#selectevent 1336
#rarity 0
#req_freesites 1
#req_site 0
#req_code 23
#req_rare 50
#code 0
#addsite -1
#end

#selectevent 1337
#rarity 2
#req_freesites 1
#req_site 0
#req_monster 151
#req_unique 1
#req_fornation 100
#addsite -1
#end

#selectevent 1338
#rarity 0
#req_code 23
#req_rare 50
#code 0
#curse 10
#end

#selectevent 1339
#rarity 2
#req_magic 2
#req_fornation 100
#curse 6
#end

#selectevent 1340
#rarity -1
#req_nearbysite 1
#req_capital 0
#req_turn 10
#req_fornation 100
#decscale 5
#decscale 3
#end

#selectevent 1341
#rarity 1
#req_nearbysite 1
#req_site 0
#req_fornation 100
#req_freesites 1
#req_nomonster 1648
#req_land 1
#req_unique 5
#addsite -1
#curse 2
#decscale 5
#end

#selectevent 1342
#rarity -2
#req_monster 1538
#req_death 2
#req_heat 2
#req_magic 1
#req_minpop 300
#2d6vis 0
#kill 3
#end

#selectevent 1343
#rarity -2
#req_monster 1538
#req_death 2
#req_heat 2
#req_magic 1
#req_minpop 300
#req_luck 2
#4d6vis 0
#kill 3
#end

#selectevent 1344
#rarity -2
#req_monster 89
#req_death 2
#req_heat 2
#req_magic 1
#req_minpop 300
#2d6vis 0
#end

#selectevent 1345
#rarity 2
#req_forest 1
#req_turn 15
#req_fornation 16
#req_dominion 3
#4com 932
#9d6units 932
#end

#selectevent 1346
#rarity 2
#req_forest 1
#req_turn 15
#req_fornation 104
#req_dominion 3
#4com 932
#9d6units 932
#end

#selectevent 1347
#rarity 2
#req_forest 1
#req_turn 15
#req_fornation 63
#req_dominion 3
#4com 932
#9d6units 932
#end

#selectevent 1348
#rarity 2
#req_forest 1
#req_turn 50
#req_fornation 16
#req_dominion 5
#com 931
#addequip 1
#9d6units 932
#6d6units 362
#end

#selectevent 1349
#rarity 2
#req_forest 1
#req_turn 50
#req_fornation 104
#req_dominion 5
#com 931
#addequip 1
#9d6units 932
#6d6units 362
#end

#selectevent 1350
#rarity 2
#req_forest 1
#req_turn 50
#req_fornation 63
#req_dominion 5
#req_magic 1
#com 931
#addequip 1
#9d6units 932
#6d6units 362
#end

#selectevent 1351
#rarity 2
#req_forest 1
#req_turn 50
#req_death 2
#req_dominion 5
#req_magic 1
#com 931
#addequip 1
#9d6units 361
#decscale2 3
#6d6units 362
#end

#selectevent 1352
#rarity 0
#req_code 0
#req_foundsite 1
#req_unique 1
#req_turn 20
#com 778
#addequip 3
#addequip 1
#code -1
#end

#selectevent 1353
#rarity 10
#req_code -1
#worldincscale2 3
#worlddisease 3
#code 0
#end

#selectevent 1354
#rarity -1
#req_forest 1
#req_fornation 25
#req_maxturn 20
#req_luck 1
#req_freesites 1
#temple 1
#addsite -1
#end

#selectevent 1355
#rarity -1
#req_forest 1
#req_fornation 73
#req_maxturn 20
#req_luck 1
#req_freesites 1
#temple 1
#addsite -1
#end

#selectevent 1356
#rarity -1
#req_forest 1
#req_fornation 111
#req_maxturn 20
#req_luck 1
#req_freesites 1
#temple 1
#addsite -1
#end

#selectevent 1357
#rarity 1
#req_monster 171
#req_nomonster 169
#killmon 171
#unrest 4
#end

#selectevent 1358
#rarity 0
#req_fornation 54
#req_land 1
#req_freesites 1
#req_unique 1
#req_rare 5
#req_turn 20
#req_code 0
#addsite -1
#code 31
#flagland 1
#end

#selectevent 1359
#rarity 0
#req_monster 254
#req_nomonster 253
#req_code 31
#code 32
#end

#selectevent 1360
#rarity 0
#req_monster 258
#req_temple 0
#req_foundsite 1
#req_unique 3
#temple 1
#incdom 1
#end

#selectevent 1361
#rarity 0
#req_monster 254
#req_nomonster 253
#req_rare 10
#req_code 32
#code 33
#curse 50
#end

#selectevent 1362
#rarity 0
#req_monster 254
#req_nomonster 253
#req_rare 10
#req_code 33
#code 0
#disease 50
#nation -2
#com 778
#end

#selectevent 1363
#rarity 0
#req_monster 254
#req_nomonster 253
#req_rare 10
#req_code 33
#killcom 254
#end

#selectevent 1364
#rarity 0
#req_monster 254
#req_nomonster 253
#req_rare 15
#req_code 32
#killcom 254
#end

#selectevent 1365
#rarity 0
#req_monster 254
#req_nomonster 253
#req_unique 1
#req_rare 10
#req_code 33
#astralboost 254
#end

#selectevent 1366
#rarity 0
#req_monster 253
#req_code 31
#code 33
#curse 20
#end

#selectevent 1367
#rarity 0
#req_monster 253
#req_rare 5
#req_code 33
#killcom 253
#end

#selectevent 1368
#rarity 0
#req_monster 253
#req_unique 1
#req_rare 10
#req_code 33
#astralboost 253
#end

#selectevent 1369
#rarity 0
#req_monster 253
#req_rare 15
#req_code 33
#code 0
#disease 50
#nation -2
#com 778
#end

#selectevent 1370
#rarity 0
#req_monster 253
#req_rare 4
#req_code 33
#code 0
#magicitem 9
#1d6vis 5
#end

#selectevent 1371
#rarity 0
#req_monster 254
#req_nomonster 253
#req_rare 4
#req_code 33
#code 0
#magicitem 9
#1d6vis 5
#end

#selectevent 1372
#rarity 0
#req_monster 253
#req_code 32
#code 33
#curse 20
#end

#selectevent 1373
#rarity 0
#req_coast 1
#req_monster 2457
#req_unique 1
#req_heat 0
#req_rare 5
#nation -2
#com 2458
#end

#selectevent 1374
#rarity 2
#req_maxdef 10
#req_coast 1
#req_fort 0
#req_noseason 3
#req_code 0
#code 40
#unrest 5
#flagland 1
#end

#selectevent 1375
#rarity 0
#req_mindef 16
#req_rare 25
#req_code 40
#code 0
#landgold 5
#end

#selectevent 1376
#rarity 0
#req_mindef 11
#req_chaos 2
#req_rare 35
#req_code 40
#defence -5
#end

#selectevent 1377
#rarity 0
#req_maxdef 15
#req_rare 25
#req_code 40
#code 41
#kill 3
#unrest 30
#end

#selectevent 1378
#rarity 0
#req_maxdef 10
#req_rare 15
#req_code 40
#kill 1
#unrest 15
#end

#selectevent 1379
#rarity 0
#req_maxdef 13
#req_temple 1
#req_rare 25
#req_code 40
#temple 0
#end

#selectevent 1380
#rarity 0
#req_maxdef 5
#req_maxtroops 20
#req_rare 25
#req_noera 3
#req_code 41
#code 0
#com 1603
-- ro: effect 177 (18d6units) = 1603
#12d6units 1604
#2com 1604
#end

#selectevent 1381
#rarity 0
#req_maxdef 15
#req_maxtroops 20
#req_rare 25
#req_noera 3
#req_code 41
#code 0
#com 1603
#15d6units 1603
#9d6units 1604
#2com 1604
#end

#selectevent 1382
#rarity 0
#req_maxdef 10
#req_rare 25
#req_era 3
#req_code 41
#code 0
#com 870
-- ro: effect 177 (18d6units) = 871
#9d6units 871
#2com 871
#end

#selectevent 1383
#rarity 0
#req_maxdef 15
#req_rare 25
#req_era 3
#req_code 41
#code 0
#com 870
#15d6units 871
#9d6units 871
#2com 871
#end

#selectevent 1384
#rarity 0
#req_mindef 18
#req_rare 25
#req_code 41
#code 0
#landgold 5
#end

#selectevent 1385
#rarity 0
#req_foundsite 1
#req_turn 66
#req_code 0
#code -2
#decscale 5
#incscale2 0
#unrest 30
#flagland 1
#end

#selectevent 1386
#rarity 10
#req_foundsite 1
#req_unique 1
#req_code -2
#worldincscale 0
#incscale3 0
#unrest 30
#end

#selectevent 1387
#rarity 0
#req_pathblood 5
#req_unique 1
#req_code -2
#code -3
#end

#selectevent 1388
#rarity 0
#req_pathblood 5
#req_rare 30
#req_code -3
#code 0
#nation -2
#com 304
#4d6units 304
#4d6units 303
#end

#selectevent 1389
#rarity 0
#req_pathblood 3
#req_rare 7
#req_code -3
#code 0
#com 304
#addequip 1
#4d6units 304
#4d6units 303
#end

#selectevent 1390
#rarity 11
#req_noseason 3
#worldheal 5
#linger 2
#end

#selectevent 1391
#rarity 11
#req_turn 10
#worlddarkness
#end

#selectevent 1392
#rarity 13
#req_foundsite 1
#req_unique 1
#worldheal 15
#end

#selectevent 1393
#rarity 13
#req_foundsite 1
#req_unique 1
#worldmark 2
#end

#selectevent 1394
#rarity 11
#req_foundsite 1
#req_unique 1
#worldcurse 5
#decscale 5
#end

#selectevent 1395
#rarity 13
#req_monster 820
#req_unique 1
#req_code 0
#code -37
#worlddisease 1
#disease 5
#end

#selectevent 1396
#rarity 10
#req_monster 651
#req_unique 1
#worldmark 5
#end

#selectevent 1397
#rarity 2
#req_turn 66
#req_coast 1
#req_unique 1
#com 521
#addequip 1
#end

#selectevent 1398
#rarity 13
#req_monster 1405
#req_unique 1
#worlddecscale 2
#end

#selectevent 1399
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worldage -3
#end

#selectevent 1400
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worldage 1
#end

#selectevent 1401
#rarity 13
#req_claimedthrone
#req_site 1
#req_rare 35
#req_unique 2
#req_site 1  -- stored twice; a second #req_site in a mod replaces the first
#worlddecscale 4
#end

#selectevent 1402
#rarity 13
#req_claimedthrone
#req_site 1
#req_rare 35
#req_unique 2
#worldincscale 4
#end

#selectevent 1403
#rarity 13
#req_claimedthrone
#req_unique 1
#req_code 0
#req_site 1
#code -37
#worlddisease 1
#linger 3
#delay25 7
#end

#selectevent 1404
#rarity 0
#req_code -37
#req_indepok
#notext
#resetcode -37
#end

#selectevent 1405
#rarity 13
#req_claimedthrone
#req_unique 2
#req_site 1
#worlddecscale 2
#end

#selectevent 1406
#rarity 13
#req_claimedthrone
#req_unique 2
#req_site 1
#worldincscale 2
#linger 1
#end

#selectevent 1407
#rarity -1
#req_land 1
#req_claimedthrone
#req_dominion 5
#req_site 1
#nation -2
#6d6units 284
#3d6units 1224
#com 1224
#end

#selectevent 1408
#rarity 0
#req_land 1
#req_claimedthrone
#req_rare 5
#req_dominion 5
#req_unique 1
#req_site 1
#nation -2
#com 309
#end

#selectevent 1409
#rarity 0
#req_claimedthrone
#req_rare 10
#req_dominion 5
#req_monster 309
#req_unique 3
#req_land 1
#req_site 1
#nation -2
#3d6units 511
#end

#selectevent 1410
#rarity 0
#req_freesites 1
#req_rare 15
#req_nearbysite 1
#req_code 0
#req_land 1
#req_unique 3
#code -4
#incscale3 2
#end

#selectevent 1411
#rarity 0
#req_freesites 1
#req_unique 3
#req_code -4
#req_site 0
#code 0
#addsite -1
#end

#selectevent 1412
#rarity 0
#req_land 1
#req_claimedthrone
#req_rare 5
#req_dominion 5
#req_unique 1
#req_site 1
#nation -2
#4d6units 2231
#com 2231
#addequip 2
#end

#selectevent 1413
#rarity 0
#req_foundsite 1
#req_unique 1
#req_code 0
#code -5
#flagland 1
#end

#selectevent 1414
#rarity 0
#req_pathastral 1
#req_rare 10
#req_unique 2
#req_code -5
#magicitem 9
#end

#selectevent 1415
#rarity 0
#req_pathastral 1
#req_rare 10
#req_unique 2
#req_code -5
#magicitem 9
#end

#selectevent 1416
#rarity 0
#req_pathastral 1
#req_rare 15
#req_unique 1
#req_code -5
#magicitem 9
#end

#selectevent 1417
#rarity 0
#req_pathastral 1
#req_rare 10
#req_unique 1
#req_code -5
#magicitem 9
#code 0
#end

#selectevent 1418
#rarity 0
#req_pathastral 1
#req_rare 5
#req_code -5
#1d6vis 4
#end

#selectevent 1419
#rarity -1
#req_foundsite 1
#1d6vis 4
#end

#selectevent 1420
#rarity 0
#req_hiddensite 1
#req_monster 376
#revealsite
#end

#selectevent 1421
#rarity 0
#req_poptype 30
#req_rare 10
#req_unique 3
#req_monster 376
#nation -2
#com 23
#end

#selectevent 1422
#rarity 0
#req_owncapital 1
#req_fornation 19
#req_rare 5
#req_season 3
#req_code 0
#req_unique 1
#flagland 1
#code 55
#taxboost -10
#unrest -50
#end

#selectevent 1423
#rarity 0
#req_monster 2181
#req_monster 2269  -- stored twice; a second #req_monster in a mod replaces the first
#req_season 0
#req_code 55
#code 56
#decscale3 3
#decscale 1
#incdom 3
#end

#selectevent 1424
#rarity 0
#req_nomonster 2181
#req_monster 2269
#req_season 0
#req_code 55
#code 57
#incscale 3
#unrest 5
#incscale2 4
#end

#selectevent 1425
#rarity 0
#req_monster 2181
#req_nomonster 2269
#req_season 0
#req_code 55
#code 57
#incscale 3
#unrest 5
#incscale2 4
#end

#selectevent 1426
#rarity 0
#req_nomonster 2181
#req_nomonster 2269  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_season 0
#req_code 55
#code 57
#incscale 3
#unrest 5
#incscale2 4
#end

#selectevent 1427
#rarity 0
#req_season 1
#req_code 56
#code 58
#taxboost 100
#decscale 3
#unrest -20
#end

#selectevent 1428
#rarity 0
#req_season 2
#req_code 58
#code 0
#taxboost 100
#decscale 3
#unrest -20
#end

#selectevent 1429
#rarity 0
#req_season 1
#req_code 57
#code 59
#taxboost -100
#incscale2 3
#unrest 20
#kill 1
#end

#selectevent 1430
#rarity 0
#req_code 59
#code 0
#kill 2
#disease 5
#end

#selectevent 1431
#rarity -1
#req_turn 10
#req_monster 2182
#req_unique 2
#nation -2
#com 843
#4d6units 676
#end

#selectevent 1432
#rarity -1
#req_monster 2179
#req_minpop 100
#req_nearbysite 1
#decscale2 3
#landgold 4
#end

#selectevent 1433
#rarity 0
#req_rare 12
#req_code 55
#nation -2
#4d6units 2162
#3d6units 2163
#3d6units 2164
#com 2166
#end

#selectevent 1434
#rarity 0
#req_rare 15
#req_code 55
-- ro: effect 190 (gold) = 100
#end

#selectevent 1435
#rarity 0
#req_rare 12
#req_code 55
#nation -2
#3d6units 2183
#3d6units 2168
#com 2170
#end

#selectevent 1436
#rarity 2
#req_monster 2184
#req_fornation 17
#req_nation 19
#req_land 1
#nation 19
#2com 2184
#12d6units 2184
#extramsg 19
#end

#selectevent 1437
#rarity -1
#req_fornation 19
#req_unique 1
#req_nation 17
#req_land 1
#nation -2
#com 2184
#magicitem 9
#end

#selectevent 1438
#rarity 0
#req_swamp 1
#req_monster 2169
#req_rare 30
#nation -2
#1unit 2185
#end

#selectevent 1439
#rarity -1
#req_owncapital 1
#req_fornation 19
#req_minunrest 15
#nation -2
#com 2178
#unrest -20
#end

#selectevent 1440
#rarity 0
#req_fornation 125
#req_rare 50
#req_nomnr 3046
#req_nomnr 3047
#req_nomnr 3048
#req_nomnr 3049
#req_targmnr 3043
#transform 3047
#pathboost 9
-- ro: effect 190 (gold) = 100
#end

#selectevent 1441
#rarity 0
#req_fornation 125
#req_rare 50
#req_nomnr 3046
#req_nomnr 3047
#req_nomnr 3048
#req_nomnr 3049
#req_targmnr 3045
#transform 3049
#pathboost 9
-- ro: effect 190 (gold) = 100
#end

#selectevent 1442
#rarity 0
#req_fornation 125
#req_nomnr 3048
#req_nomnr 3049
#req_monster 3047
#req_targmnr 3045
#transform 3049
#pathboost 9
-- ro: effect 190 (gold) = 150
#unrest -20
#end

#selectevent 1443
#rarity 0
#req_fornation 125
#req_nomnr 3046
#req_nomnr 3047
#req_monster 3049
#req_targmnr 3043
#transform 3047
#pathboost 9
-- ro: effect 190 (gold) = 150
#unrest -20
#end

#selectevent 1444
#rarity 0
#req_fornation 125
#req_rare 50
#req_nomnr 3048
#req_nomnr 3049
#req_mnr 3046
#req_mnr 3047
#req_owncapital 1
#req_nomonster 3044
#req_nomonster 3045  -- stored twice; a second #req_nomonster in a mod replaces the first
#unrest 10
#end

#selectevent 1445
#rarity 0
#req_fornation 125
#req_rare 50
#req_nomnr 3046
#req_nomnr 3047
#req_mnr 3048
#req_mnr 3049
#req_owncapital 1
#req_nomonster 3042
#req_nomonster 3043  -- stored twice; a second #req_nomonster in a mod replaces the first
#unrest 10
#end

#selectevent 1446
#rarity 1
#req_fornation 125
#req_land 1
#req_2monsters 3043
#assassin 428
#end

#selectevent 1447
#rarity 1
#req_fornation 125
#req_land 1
#req_2monsters 3045
#assassin 428
#end

#selectevent 1448
#rarity 1
#req_fornation 125
#req_land 1
#req_monster 3043
#req_monster 3047  -- stored twice; a second #req_monster in a mod replaces the first
#req_targmnr 3047
#assassin 428
#end

#selectevent 1449
#rarity 1
#req_fornation 125
#req_land 1
#req_monster 3045
#req_monster 3049  -- stored twice; a second #req_monster in a mod replaces the first
#req_targmnr 3049
#assassin 428
#end

#selectevent 1450
#rarity 1
#req_fornation 125
#req_2monsters 3043
#req_targmnr 3043
#poison 15
#end

#selectevent 1451
#rarity 1
#req_fornation 125
#req_2monsters 3045
#req_targmnr 3045
#poison 15
#end

#selectevent 1452
#rarity 1
#req_fornation 125
#req_monster 3047
#req_monster 3043  -- stored twice; a second #req_monster in a mod replaces the first
#req_targmnr 3047
#poison 15
#end

#selectevent 1453
#rarity 1
#req_fornation 125
#req_monster 3049
#req_monster 3045  -- stored twice; a second #req_monster in a mod replaces the first
#req_targmnr 3049
#poison 15
#end

#selectevent 1454
#rarity 1
#req_fornation 125
#req_monster 3047
#req_monster 3043  -- stored twice; a second #req_monster in a mod replaces the first
#killcom 3043
#end

#selectevent 1455
#rarity 1
#req_fornation 125
#req_monster 3049
#req_monster 3045  -- stored twice; a second #req_monster in a mod replaces the first
#killcom 3045
#end

#selectevent 1456
#rarity -1
#req_foundsite 1
#req_turn 30
#req_code 0
#req_unique 1
#code 60
#flagland 1
#end

#selectevent 1457
#rarity 0
#req_pathdeath 2
#req_unique 1
#req_rare 20
#req_code 60
#curse 99
#end

#selectevent 1458
#rarity 0
#req_pathdeath 5
#req_rare 50
#req_code 60
#code 61
#end

#selectevent 1459
#rarity 0
#req_pathdeath 2
#req_rare 10
#req_code 60
#com 308
#end

#selectevent 1460
#rarity 0
#req_pathdeath 3
#req_rare 25
#req_code 61
#code 0
#com 774
#addequip 1
#addequip 3
#code 0
#end

#selectevent 1461
#rarity 0
#req_pathdeath 3
#req_rare 25
#req_code 61
#code 0
#nation -2
#com 776
#code 0
#end

#selectevent 1462
#rarity 0
#req_pathdeath 2
#req_unique 1
#req_rare 25
#req_code 61
#com 308
#end

#selectevent 1463
#rarity 10
#req_foundsite 1
#req_unique 1
#worlddarkness
#worlddisease 1
#linger 2
#end

#selectevent 1464
#rarity 0
#req_foundsite 1
#req_unique 1
#incscale 3
#disease 3
#end

#selectevent 1465
#rarity 10
#req_unique 1
#req_code -6
#code2 0
#worlddarkness
#linger 2
#end

#selectevent 1466
#rarity 0
#req_rare 70
#req_site 1
#req_claimedthrone
#req_unique 3
#req_code 0
#code -6
#end

#selectevent 1467
#rarity -1
#req_land 1
#req_fornation 101
#req_unique 1
#req_nearbysite 1
#req_code 29
#code -10
#end

#selectevent 1468
#rarity 0
#req_monster 740
#req_code -10
#req_land 1
#code 0
#nation -2
#1unit 405
#end

#selectevent 1469
#rarity -1
#req_fornation 101
#req_nearbysite 1
#req_land 1
#nation -2
#3d6units 198
#end

#selectevent 1470
#rarity 1
#req_maxdef 15
#req_maxtroops 20
#req_rare 25
#req_noera 3
#req_unluck 2
#req_coast 1
#com 1603
#15d6units 1603
#9d6units 1604
#2com 1604
#end

#selectevent 1471
#rarity -1
#req_mydominion 0
#req_monster 148
#req_capital 0
#nation 61
#newdom 2
#end

#selectevent 1472
#rarity -1
#req_monster 148
#req_dominion 3
#req_temple 0
#req_capital 0
#temple 1
#end

#selectevent 1473
#rarity -1
#req_monster 148
#req_dominion 1
#req_capital 0
-- ro: effect 190 (gold) = 150
#end

#selectevent 1474
#rarity -1
#req_poptype 30
#req_monster 148
#req_dominion 3
#req_capital 0
#nation -2
#6d6units 217
#end

#selectevent 1475
#rarity 0
#req_foundsite 1
#req_monster 937
#req_rare 25
#req_unique 1
#deathboost 937
#end

#selectevent 1476
#rarity 0
#req_foundsite 1
#req_monster 670
#req_rare 25
#req_unique 1
#deathboost 670
#end

#selectevent 1477
#rarity 0
#req_foundsite 1
#req_monster 41
#req_rare 25
#req_unique 1
#deathboost 41
#end

#selectevent 1478
#rarity 0
#req_foundsite 1
#req_monster 223
#req_rare 25
#req_unique 1
#deathboost 223
#end

#selectevent 1479
#rarity -1
#req_pop0ok
#req_site 1
#req_maxpop 0
#req_waste 0
#req_land 1
#nation -2
#com -13
#end

#selectevent 1480
#rarity -1
#req_pop0ok
#req_maxpop 0
#req_land 1
-- ro: effect 190 (gold) = 88
#end

#selectevent 1481
#rarity -1
#req_pop0ok
#req_maxpop 0
#req_land 1
#1d6vis 1
#end

#selectevent 1482
#rarity -1
#req_pop0ok
#req_site 1
#req_dominion 5
#req_fornation 54
#req_unique 1
#landgold 25
#end

#selectevent 1483
#rarity -1
#req_pop0ok
#req_site 1
#req_dominion 3
#req_fornation 54
#req_unique 1
#landgold 15
#end

#selectevent 1484
#rarity -1
#req_pop0ok
#req_site 1
#req_dominion 3
#req_fornation 54
#req_unique 1
#landgold 10
#end

#selectevent 1485
#rarity 1
#req_maxpop 35
#req_death 2
#req_dominion 3
#req_fornation 54
#kill 99
#end

#selectevent 1486
#rarity 2
#req_pop0ok
#req_minpop 5
#req_maxpop 30
#req_turn 10
#req_capital 0
#req_fornation 54
#req_land 1
#com 34
#2com 310
#addequip 1
#15d6units 619
#6d6units 1565
#end

#selectevent 1487
#rarity -2
#req_pop0ok
#req_maxpop 0
#req_lab 0
#req_unique 1
#req_monster 253
#lab 1
#nation -2
#com 329
#end

#selectevent 1488
#rarity 1
#req_pop0ok
#req_maxpop 25
#req_minpop 10
#req_dominion 3
#req_fornation 54
#req_season 2
#kill 30
#end

#selectevent 1489
#rarity 1
#req_pop0ok
#req_minpop 1
#req_maxpop 30
#req_forest 1
#req_capital 0
#req_fornation 54
#com 362
#9d6units 361
#3d6units 362
#com 931
#addequip 1
#end

#selectevent 1490
#rarity 1
#req_pop0ok
#req_maxpop 35
#req_minpop 10
#req_dominion 3
#req_fornation 54
#emigration 10
#kill 3
#end

#selectevent 1491
#rarity 1
#req_pop0ok
#req_poptype 25
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#2com 141
#3d6units 140
#3d6units 139
#end

#selectevent 1492
#rarity 1
#req_pop0ok
#req_poptype 26
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#2com 136
#6d6units 137
#end

#selectevent 1493
#rarity 1
#req_pop0ok
#req_poptype 30
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#com 23
#com 240
#3d6units 30
#3d6units 55
#3d6units 22
#end

#selectevent 1494
#rarity 1
#req_pop0ok
#req_poptype 32
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#com 35
#com 240
#3d6units 29
#3d6units 39
#3d6units 47
#end

#selectevent 1495
#rarity 1
#req_pop0ok
#req_poptype 34
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#2com 252
#9d6units 205
#end

#selectevent 1496
#rarity 1
#req_pop0ok
#req_poptype 36
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#2com 514
#9d6units 423
#end

#selectevent 1497
#rarity 1
#req_pop0ok
#req_poptype 40
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#com 346
#com 347
#6d6units 348
#3d6units 367
#end

#selectevent 1498
#rarity 1
#req_pop0ok
#req_poptype 42
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#com 352
#com 353
#6d6units 352
#3d6units 353
#end

#selectevent 1499
#rarity 1
#req_pop0ok
#req_poptype 43
#req_minpop 5
#req_maxpop 30
#req_fornation 54
#com 355
#com 356
#6d6units 357
#3d6units 369
#end

#selectevent 1500
#rarity 2
#req_poptype 43
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 355
#assassin 357
#end

#selectevent 1501
#rarity 2
#req_poptype 25
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 141
#assassin 140
#assassin 139
#end

#selectevent 1502
#rarity 2
#req_poptype 26
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 136
#assassin 137
#end

#selectevent 1503
#rarity 2
#req_poptype 30
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 23
#assassin 30
#end

#selectevent 1504
#rarity 2
#req_poptype 32
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 35
#assassin 29
#end

#selectevent 1505
#rarity 2
#req_poptype 34
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 252
#assassin 205
#end

#selectevent 1506
#rarity 2
#req_poptype 36
#req_pop0ok
#req_maxpop 1
#req_fornation 54
#req_commander 1
#assassin 514
#assassin 423
#end

#selectevent 1507
#rarity -1
#req_fornation 75
#req_nearbysite 1
#req_capital 0
#req_turn 10
#req_code 0
#req_unique 1
#req_freesites 1
#code 63
#incscale2 3
#end

#selectevent 1508
#rarity 0
#req_fornation 75
#req_freesites 1
#req_turn 15
#req_unique 1
#req_code 63
#code 64
#addsite -1
#flagland 1
#end

#selectevent 1509
#rarity 0
#req_monster 937
#req_unique 1
#req_code 64
#code 65
#end

#selectevent 1510
#rarity 0
#req_monster 937
#req_rare 7
#req_unique 1
#req_code 65
#killcom 937
#end

#selectevent 1511
#rarity 0
#req_monster 937
#req_rare 10
#req_unique 1
#req_code 65
#deathboost 937
#end

#selectevent 1512
#rarity 0
#req_monster 937
#req_rare 5
#req_unique 1
#req_code 65
#com 329
#9d6units 1658
#2com 566
#end

#selectevent 1513
#rarity 0
#req_monster 937
#req_rare 12
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1514
#rarity 0
#req_monster 937
#req_rare 12
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1515
#rarity 0
#req_monster 937
#req_rare 18
#req_code 65
#code 0
#nation -2
#1unit 625
#1d6vis 5
#curse 10
#end

#selectevent 1516
#rarity -1
#req_fornation 55
#req_nearbysite 1
#req_capital 0
#req_turn 10
#req_code 0
#req_unique 1
#code 63
#incscale2 3
#end

#selectevent 1517
#rarity 0
#req_freesites 1
#req_turn 15
#req_unique 1
#req_code 63
#flagland 1
#code 64
#addsite -1
#end

#selectevent 1518
#rarity 0
#req_monster 670
#req_unique 1
#req_code 64
#code 65
#end

#selectevent 1519
#rarity 0
#req_monster 670
#req_rare 5
#req_unique 1
#req_code 65
#killcom 670
#end

#selectevent 1520
#rarity 0
#req_monster 670
#req_rare 10
#req_unique 1
#req_code 65
#deathboost 670
#end

#selectevent 1521
#rarity 0
#req_monster 670
#req_rare 7
#req_unique 1
#req_code 65
#com 329
#9d6units 1658
#2com 566
#end

#selectevent 1522
#rarity 0
#req_monster 670
#req_rare 20
#req_code 65
#code 0
#nation -2
#1unit 625
#1d6vis 5
#curse 10
#end

#selectevent 1523
#rarity 0
#req_monster 670
#req_rare 20
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1524
#rarity -1
#req_fornation 56
#req_nearbysite 1
#req_capital 0
#req_turn 10
#req_code 0
#req_unique 1
#code 63
#incscale2 3
#end

#selectevent 1525
#rarity 0
#req_freesites 1
#req_turn 15
#req_unique 1
#req_code 63
#flagland 1
#code 64
#addsite -1
#end

#selectevent 1526
#rarity 0
#req_monster 41
#req_unique 1
#req_code 64
#code 65
#end

#selectevent 1527
#rarity 0
#req_monster 41
#req_rare 5
#req_unique 1
#req_code 65
#killcom 41
#end

#selectevent 1528
#rarity 0
#req_monster 41
#req_rare 10
#req_unique 1
#req_code 65
#deathboost 41
#end

#selectevent 1529
#rarity 0
#req_monster 41
#req_rare 7
#req_unique 1
#req_code 65
#com 329
#9d6units 1658
#2com 566
#end

#selectevent 1530
#rarity 0
#req_monster 41
#req_rare 20
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1531
#rarity 0
#req_monster 41
#req_rare 20
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1532
#rarity -1
#req_fornation 61
#req_nearbysite 1
#req_capital 0
#req_turn 10
#req_code 0
#req_unique 1
#code 63
#incscale2 3
#end

#selectevent 1533
#rarity 0
#req_fornation 61
#req_freesites 1
#req_turn 15
#req_unique 1
#req_code 63
#flagland 1
#code 64
#addsite -1
#end

#selectevent 1534
#rarity 0
#req_monster 223
#req_unique 1
#req_code 64
#code 65
#end

#selectevent 1535
#rarity 0
#req_monster 223
#req_rare 5
#req_unique 1
#req_code 65
#killcom 223
#end

#selectevent 1536
#rarity 0
#req_monster 223
#req_rare 10
#req_unique 1
#req_code 65
#deathboost 223
#end

#selectevent 1537
#rarity 0
#req_monster 223
#req_rare 7
#req_unique 1
#req_code 65
#com 329
#9d6units 1658
#2com 566
#end

#selectevent 1538
#rarity 0
#req_monster 223
#req_rare 20
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1539
#rarity 0
#req_monster 223
#req_rare 20
#req_code 65
#code 0
#2d6vis 5
-- ro: effect 190 (gold) = 100
#magicitem 9
#curse 10
#end

#selectevent 1540
#rarity 0
#req_unique 2
#req_poptype 30
#req_dominion 4
#req_rare 4
#req_code 0
#req_fornation 61
#flagland 1
#code 67
#incdom -2
#end

#selectevent 1541
#rarity 0
#req_unique 4
#req_rare 5
#req_code 67
#incdom -2
#end

#selectevent 1542
#rarity 0
#req_unique 1
#req_rare 5
#req_code 67
#incdom -2
#code 70
#end

#selectevent 1543
#rarity 0
#req_monster 148
#req_nomonster 149
#req_nomonster 222  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_nomonster 223  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 67
#code 68
#incdom 1
#end

#selectevent 1544
#rarity 0
#req_rare 4
#req_code 68
#code 67
#incdom -2
#end

#selectevent 1545
#rarity 0
#req_monster 149
#req_nomonster 222
#req_code 67
#code 69
#unrest 3
#end

#selectevent 1546
#rarity 0
#req_monster 149
#req_mintroops 20
#req_rare 80
#req_nomonster 222
#req_code 69
#code 0
#kill 1
#unrest 15
#incdom 3
-- ro: effect 190 (gold) = 200
#end

#selectevent 1547
#rarity 0
#req_monster 149
#req_mintroops 20
#req_rare 20
#req_nomonster 222
#req_code 69
#code 70
#kill 5
#unrest 40
#end

#selectevent 1548
#rarity 0
#req_monster 222
#req_code 67
#code 69
#end

#selectevent 1549
#rarity 0
#req_monster 222
#req_mintroops 20
#req_rare 90
#req_code 69
#code 0
#kill 1
#unrest 15
#incdom 3
-- ro: effect 190 (gold) = 200
#end

#selectevent 1550
#rarity 0
#req_monster 222
#req_mintroops 20
#req_rare 10
#req_code 69
#code 70
#kill 5
#unrest 40
#end

#selectevent 1551
#rarity 0
#req_monster 224
#req_nomonster 222
#req_rare 50
#req_nomonster 149  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 67
#code 68
#kill 1
#magicitem 2
#magicitem 9
#end

#selectevent 1552
#rarity 0
#req_monster 224
#req_nomonster 222
#req_rare 50
#req_nomonster 149  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 67
#code 70
#kill 5
#unrest 40
#end

#selectevent 1553
#rarity 0
#req_monster 223
#req_nomonster 222
#req_rare 70
#req_nomonster 149  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 67
#code 68
#kill 1
#magicitem 2
#magicitem 9
#end

#selectevent 1554
#rarity 0
#req_monster 223
#req_nomonster 222
#req_rare 30
#req_nomonster 149  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 67
#code 70
#kill 5
#unrest 40
#end

#selectevent 1555
#rarity 0
#req_rare 50
#req_code 70
#code 68
#4com 23
#9d6units 39
#6d6units 22
#incdom -6
#end

#selectevent 1556
#rarity -1
#req_monster 148
#req_minunrest 15
#unrest -30
#incdom 1
#end

#selectevent 1557
#rarity -2
#req_monster 148
#req_dominion 0
#incdom 3
#holyboost 148
#end

#selectevent 1558
#rarity -1
#req_land 1
#req_capital 0
#req_fornation 61
#req_unique 4
#req_code 0
#flagland 1
#code 73
#decscale 5
#end

#selectevent 1559
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 20
#req_code 73
#code 74
#1d6vis 6
#magicitem 2
#end

#selectevent 1560
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 20
#req_code 73
#code 76
#decscale 5
#unrest 10
#end

#selectevent 1561
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 15
#req_code 73
#code 74
#assassin 154
#end

#selectevent 1562
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 15
#req_code 73
#unrest 10
#end

#selectevent 1563
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 15
#req_code 73
#code 74
#incdom 2
#end

#selectevent 1564
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 30
#req_code 74
#code 73
#incdom 2
#unrest 10
#end

#selectevent 1565
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 15
#req_code 73
#code 74
#decscale 5
#incscale 4
#curse 15
#end

#selectevent 1566
#rarity 0
#req_mintroops 20
#req_monster 224
#req_nomonster 223
#req_rare 80
#req_code 76
#code 0
#4com 154
#addequip 1
#addequip 2
#1d6units 2136
#end

#selectevent 1567
#rarity 0
#req_mintroops 20
#req_nomonster 224
#req_nomonster 223  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 76
#end

#selectevent 1568
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 50
#req_code 74
#code 0
#end

#selectevent 1569
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 15
#req_code 73
#code 74
-- ro: effect 190 (gold) = 300
#1d3vis 5
#magicitem 3
#end

#selectevent 1570
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 20
#req_code 73
#assassin 2223
#end

#selectevent 1571
#rarity 0
#req_monster 224
#req_nomonster 223
#req_rare 20
#req_code 74
#code 73
#end

#selectevent 1572
#rarity 0
#req_monster 225
#req_nomonster 224
#req_nomonster 223  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 73
#end

#selectevent 1573
#rarity 0
#req_mintroops 20
#req_monster 224
#req_nomonster 223
#req_rare 8
#req_code 73
#code 74
#1d6vis 6
#magicitem 9
#end

#selectevent 1574
#rarity 0
#req_nomonster 225
#req_nomonster 224  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_nomonster 223  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_rare 8
#req_code 73
#decscale 5
#unrest 10
#incdom -1
#end

#selectevent 1575
#rarity 0
#req_monster 223
#req_rare 20
#req_code 73
#code 74
#1d6vis 6
#magicitem 2
#end

#selectevent 1576
#rarity 0
#req_monster 223
#req_rare 20
#req_code 73
#code 76
#decscale 5
#unrest 10
#end

#selectevent 1577
#rarity 0
#req_monster 223
#req_rare 15
#req_code 73
#code 74
#assassin 154
#end

#selectevent 1578
#rarity 0
#req_monster 223
#req_rare 15
#req_code 73
#unrest 10
#end

#selectevent 1579
#rarity 0
#req_monster 223
#req_rare 15
#req_code 73
#code 74
#incdom 2
#end

#selectevent 1580
#rarity 0
#req_monster 223
#req_rare 30
#req_code 74
#code 73
#incdom 2
#unrest 10
#end

#selectevent 1581
#rarity 0
#req_monster 223
#req_rare 15
#req_code 73
#code 74
#decscale 5
#incscale 4
#curse 15
#end

#selectevent 1582
#rarity 0
#req_monster 223
#req_rare 50
#req_code 74
#code 0
#end

#selectevent 1583
#rarity 0
#req_monster 223
#req_rare 15
#req_code 73
#code 74
-- ro: effect 190 (gold) = 300
#1d3vis 5
#magicitem 3
#end

#selectevent 1584
#rarity 0
#req_monster 223
#req_rare 20
#req_code 73
#assassin 2223
#end

#selectevent 1585
#rarity 0
#req_monster 223
#req_rare 20
#req_code 74
#code 73
#end

#selectevent 1586
#rarity 0
#req_monster 223
#req_code 76
#code 0
#4com 154
#addequip 1
#3d6units 2136
#cleartarg
#fireboost 224
#end

#selectevent 1587
#rarity 0
#req_mintroops 20
#req_monster 223
#req_rare 8
#req_code 73
#code 74
#1d6vis 6
#magicitem 9
#end

#selectevent 1588
#rarity 0
#req_minunrest 18
#req_magic 2
#req_nomonster 224
#req_rare 15
#req_code 73
#code 0
#4com 154
#addequip 1
#9d6units 817
#6d6units 2136
#end

#selectevent 1589
#rarity -1
#req_fornation 56
#req_swamp 1
#req_heat 0
#req_unique 2
#req_code 0
#flagland 1
#code 80
#end

#selectevent 1590
#rarity 0
#req_monster 296
#req_freesites 1
#req_rare 10
#req_unique 1
#req_code 80
#addsite -1
#end

#selectevent 1591
#rarity 0
#req_monster 296
#req_rare 12
#req_unique 3
#req_code 80
#nation -2
#3d6units 1840
#end

#selectevent 1592
#rarity 0
#req_monster 296
#req_rare 10
#req_unique 3
#req_code 80
-- ro: effect 39 = 2
#assassin 1840
#end

#selectevent 1593
#rarity 0
#req_monster 296
#req_rare 30
#req_code 80
#code 81
#end

#selectevent 1594
#rarity 0
#req_monster 296
#req_rare 10
#req_code 81
#code 0
#end

#selectevent 1595
#rarity 0
#req_monster 296
#req_rare 10
#req_code 81
#code 0
#end

#selectevent 1596
#rarity 0
#req_monster 296
#req_rare 10
#req_code 81
#code 0
-- ro: effect 190 (gold) = 200
#magicitem 2
#end

#selectevent 1597
#rarity 0
#req_monster 296
#req_rare 10
#req_code 80
-- ro: effect 39 = 2
#assassin 2185
#end

#selectevent 1598
#rarity 0
#req_monster 296
#req_rare 10
#req_code 81
-- ro: effect 39 = 2
#assassin 2196
#end

#selectevent 1599
#rarity 0
#req_monster 296
#req_rare 10
#req_code 80
-- ro: effect 39 = 2
#assassin 403
#end

#selectevent 1600
#rarity 0
#req_monster 296
#req_rare 10
#req_freesites 1
#req_unique 1
#req_code 80
#addsite -1
#end

#selectevent 1601
#rarity -2
#req_commander 1
#req_swamp 1
#req_freesites 1
#req_heat 0
#req_unique 2
#addsite -1
#end

#selectevent 1602
#rarity -1
#req_foundsite 1
#req_fornation 56
#req_noseason 3
#req_unique 1
#req_code 0
#flagland 1
#code 83
#end

#selectevent 1603
#rarity 0
#req_monster 42
#req_code 83
#code 84
#end

#selectevent 1604
#rarity 0
#req_monster 42
#req_rare 40
#req_code 84
#code 85
-- ro: effect 39 = 2
#assassin 310
#end

#selectevent 1605
#rarity 0
#req_monster 42
#req_rare 20
#req_code 84
#code 85
#com 310
#addequip 1
#3d6units 615
#3d6units 616
#end

#selectevent 1606
#rarity 0
#req_monster 42
#req_code 85
#code 0
#magicitem 9
#deathboost 42
#end

#selectevent 1607
#rarity 0
#req_monster 42
#req_rare 8
#req_code 84
#code 0
#nation -2
#com 310
#3d6units 615
#end

#selectevent 1608
#rarity 0
#req_nomonster 42
#req_nomonster 41  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 85
#code 0
#magicitem 9
#1d6vis 5
#end

#selectevent 1609
#rarity -2
#req_commander 1
#req_freesites 1
#req_unique 2
#req_swamp 1
#addsite -1
#end

#selectevent 1610
#rarity 0
#req_monster 41
#req_code 83
#code 84
#end

#selectevent 1611
#rarity 0
#req_monster 41
#req_rare 40
#req_code 84
#code 85
-- ro: effect 39 = 2
#assassin 310
#end

#selectevent 1612
#rarity 0
#req_monster 41
#req_rare 40
#req_code 84
#code 85
#com 310
#addequip 1
#3d6units 615
#3d6units 616
#end

#selectevent 1613
#rarity 0
#req_monster 41
#req_code 85
#code 0
#magicitem 9
#deathboost 41
#end

#selectevent 1614
#rarity 0
#req_monster 41
#req_rare 8
#req_code 84
#code 0
#nation -2
#com 310
#3d6units 615
#end

#selectevent 1615
#rarity 0
#req_nomonster 41
#req_nomonster 42  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 85
#code 0
#magicitem 9
#1d6vis 5
#end

#selectevent 1616
#rarity -1
#req_foundsite 1
#req_rare 10
#req_noseason 3
#req_unique 2
#req_code 0
#flagland 1
#code 86
#end

#selectevent 1617
#rarity 0
#req_pathdeath 1
#req_code 86
#code 87
#end

#selectevent 1618
#rarity 0
#req_pathdeath 1
#req_rare 40
#req_code 87
#code 85
-- ro: effect 39 = 2
#assassin 310
#end

#selectevent 1619
#rarity 0
#req_pathdeath 1
#req_rare 20
#req_code 87
#code 88
#com 310
#addequip 1
#3d6units 615
#3d6units 616
#end

#selectevent 1620
#rarity 0
#req_pathdeath 1
#req_code 88
#code 0
#magicitem 9
#2d6vis 5
#end

#selectevent 1621
#rarity 0
#req_pathdeath 1
#req_rare 20
#req_code 87
#code 0
#nation -2
#com 310
#3d6units 615
#end

#selectevent 1622
#rarity 0
#req_rare 40
#req_code 90
#nation 4
#com 141
#15d6units 140
#com 147
#kill 10
#code 0
#end

#selectevent 1623
#rarity 1
#req_land 1
#req_unluck 1
#req_turn 5
#req_code 0
#code 90
#end

#selectevent 1624
#rarity 0
#req_rare 40
#req_code 91
#com 406
#9d6units 174
#com 406
#4d6units 175
#code 0
#end

#selectevent 1625
#rarity 1
#req_land 0
#req_unluck 1
#req_turn 5
#req_code 0
#code 91
#end

#selectevent 1626
#rarity 0
#req_rare 40
#req_code 92
#com 23
#6d6units 22
#com 23
#4d6units 18
#code 0
#end

#selectevent 1627
#rarity 1
#req_land 1
#req_unluck 2
#req_turn 8
#req_era 2
#req_code 0
#code 92
#end

#selectevent 1628
#rarity 0
#req_rare 40
#req_code 93
#nation 4
#com 141
-- ro: effect 183 (24d6units) = 140
#com 147
#6d6units 139
#code 0
#end

#selectevent 1629
#rarity 1
#req_land 1
#req_unluck 3
#req_turn 10
#req_code 0
#code 93
#end

#selectevent 1630
#rarity 1
#req_forest 1
#req_mindef 4
#req_turn 10
#req_death 0
#req_code 0
#code 94
#end

#selectevent 1631
#rarity 0
#req_rare 40
#req_code 94
#code 0
#4com 27
#9d6units 27
#9d6units 228
#end

#selectevent 1632
#rarity -2
#req_fort 0
#req_order 1
#req_freesites 1
#req_luck 2
#req_noera 1
#req_land 1
#addsite -1
#defence 8
#end

#selectevent 1633
#rarity -1
#req_site 1
#defence 5
#end

#selectevent 1634
#rarity 0
#req_site 1
#req_minunrest 10
#req_rare 50
#unrest -10
#end

#selectevent 1635
#rarity -1
#req_site 1
#req_unique 2
#magicitem 3
#end

#selectevent 1636
#rarity -1
#req_site 1
#req_order 2
#nation -2
#com 23
#3d6units 22
#end

#selectevent 1637
#rarity -1
#req_site 1
#req_luck 3
#req_unique 1
#req_magic 1
#magicitem 4
#end

#selectevent 1638
#rarity -1
#req_site 1
#req_luck 3
#req_prod 2
#req_fort 0
#req_turn 30
#fort 3
#end

#selectevent 1639
#rarity -1
#req_luck 1
#req_unique 2
#req_monster 23
#holyboost 23
#incdom 3
#end

#selectevent 1640
#rarity -1
#req_site 1
#req_prod 1
#req_unique 3
#defence 10
#end

#selectevent 1641
#rarity -1
#req_site 1
#req_luck 1
#req_unique 2
#magicitem 3
#end

#selectevent 1642
#rarity -1
#req_site 1
#req_luck 2
#req_unique 1
#req_turn 15
#nation -2
#2com 23
#4d6units 22
#9d6units 40
#end

#selectevent 1643
#rarity -1
#req_site 1
#req_luck 1
#req_unique 1
#req_turn 15
#nation -2
#com 23
#1d6units 22
#6d6units 40
#end

#selectevent 1644
#rarity -1
#req_coast 1
#req_luck 2
#req_chaos 1
#req_heat 0
-- ro: effect 190 (gold) = 300
#nation -2
#com 479
#addequip 2
#1d6units 29
#end

#selectevent 1645
#rarity -1
#req_luck 2
#req_chaos 1
#req_capital 1
-- ro: effect 190 (gold) = 600
#magicitem 3
#magicitem 2
#end

#selectevent 1646
#rarity -1
#req_turn 4
#req_luck 3
#req_chaos 1
#req_capital 1
-- ro: effect 190 (gold) = 800
#magicitem 3
#magicitem 2
#1d3vis 56
#end

#selectevent 1647
#rarity -1
#req_owncapital 1
#req_luck 1
#req_chaos 1
#req_turn 20
#req_unique 3
-- ro: effect 190 (gold) = 800
#end

#selectevent 1648
#rarity -1
#req_owncapital 1
#req_luck 2
#req_chaos 1
#req_turn 20
#req_unique 3
-- ro: effect 190 (gold) = 1200
#end

#selectevent 1649
#rarity -1
#req_owncapital 1
#req_luck 3
#req_chaos 1
#req_turn 20
#req_unique 3
-- ro: effect 190 (gold) = 2000
#end

#selectevent 1650
#rarity -1
#req_coast 1
#req_luck 3
#req_chaos 1
#req_heat 0
-- ro: effect 190 (gold) = 300
#nation -2
#com 101
#addequip 2
#1d6units 29
#end

#selectevent 1651
#rarity -1
#req_coast 1
#req_luck 1
#req_chaos 1
#req_heat 0
-- ro: effect 190 (gold) = 300
#nation -2
#com 97
#addequip 2
#1d6units 29
#end

#selectevent 1652
#rarity -1
#req_monster 1606
#req_turn 20
#req_luck 1
#req_unique 3
#magicitem 9
#end

#selectevent 1653
#rarity -2
#req_fort 0
#req_nation 5
#req_land 1
#req_luck 2
#req_prod 2
#fort 3
#nation -2
#com 1606
#9d6units 1082
#6d6units 1078
#end

#selectevent 1654
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 27
#fort 1
#nation -2
#2com 34
#9d6units 17
#9d6units 38
#end

#selectevent 1655
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 28
#fort 1
#nation -2
#2com 35
#9d6units 33
#9d6units 39
#end

#selectevent 1656
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 29
#fort 3
#nation -2
#2com 36
#9d6units 32
#9d6units 40
#end

#selectevent 1657
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 30
#fort 1
#nation -2
#2com 23
#9d6units 55
#6d6units 22
#end

#selectevent 1658
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 54
#fort 1
#nation -2
#2com 46
#9d6units 38
#6d6units 20
#end

#selectevent 1659
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 32
#fort 1
#nation -2
#2com 35
#9d6units 39
#9d6units 47
#end

#selectevent 1660
#rarity -2
#req_fort 0
#req_turn 20
#req_land 1
#req_luck 2
#req_poptype 33
#fort 1
#nation -2
#2com 36
#9d6units 40
#9d6units 48
#end

#selectevent 1661
#rarity -1
#req_monster 1606
#req_unique 2
#req_magic 2
#req_luck 2
#req_lab 1
#earthboost 1606
#earthboost 1606
#end

#selectevent 1662
#rarity -1
#req_monster 1606
#req_unique 2
#req_magic 2
#req_luck 2
#req_lab 1
#airboost 1606
#airboost 1606
#end

#selectevent 1663
#rarity -1
#req_monster 1606
#req_unique 2
#req_magic 2
#req_luck 2
#req_lab 1
#astralboost 1606
#astralboost 1606
#end

#selectevent 1664
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 2
#req_poptype 64
#fort 15
#nation -2
#2com 406
#9d6units 175
#9d6units 577
#end

#selectevent 1665
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 2
#req_poptype 63
#fort 15
#nation -2
#2com 406
#9d6units 175
#9d6units 174
#end

#selectevent 1666
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 2
#req_poptype 45
#fort 15
#nation -2
#2com 406
#9d6units 577
#6d6units 545
#end

#selectevent 1667
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 2
#req_poptype 46
#fort 15
#nation -2
#com 576
#com 575
#12d6units 573
#end

#selectevent 1668
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 2
#req_poptype 65
#fort 15
#nation -2
#2com 976
#9d6units 974
#9d6units 975
#end

#selectevent 1669
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 2
#req_poptype 31
#fort 15
#nation -2
#2com 406
#9d6units 175
#9d6units 176
#end

#selectevent 1670
#rarity -2
#req_fort 0
#req_turn 20
#req_land 0
#req_luck 3
#req_poptype 65
#fort 16
#nation -2
#2com 576
#9d6units 573
-- ro: effect 177 (18d6units) = 975
#end

#selectevent 1671
#rarity -2
#req_monster 575
#req_magic 1
#req_land 0
#req_luck 2
#req_unique 2
#fireboost 575
#earthboost 575
#end

#selectevent 1672
#rarity -2
#req_unique 3
#req_dominion 3
#req_land 0
#req_luck 2
#req_poptype 46
#nation -2
#com 576
#holyboost 576
#holyboost 576
#incdom 3
#end

#selectevent 1673
#rarity -2
#req_unique 1
#req_land 1
#req_luck 2
#req_magic 1
#req_lab 0
#lab 1
#nation -2
#com 97
#addequip 1
#2com 96
#addequip 2
#end

#selectevent 1674
#rarity -2
#req_unique 1
#req_land 1
#req_luck 3
#req_magic 3
#req_lab 0
#lab 1
#nation -2
#com 97
#addequip 1
#4com 96
#addequip 2
#end

#selectevent 1675
#rarity -2
#req_unique 1
#req_land 1
#req_luck 2
#req_magic 1
#req_lab 0
#lab 1
#nation -2
#com 95
#addequip 1
#2com 94
#addequip 2
#end

#selectevent 1676
#rarity -2
#req_unique 1
#req_land 1
#req_luck 3
#req_magic 3
#req_lab 0
#lab 1
#nation -2
#com 95
#addequip 1
#4com 94
#addequip 2
#end

#selectevent 1677
#rarity -2
#req_unique 1
#req_land 1
#req_luck 2
#req_magic 1
#req_lab 0
#lab 1
#nation -2
#com 93
#addequip 1
#2com 92
#addequip 2
#end

#selectevent 1678
#rarity -2
#req_unique 1
#req_land 1
#req_luck 3
#req_magic 3
#req_lab 0
#lab 1
#nation -2
#com 93
#addequip 1
#4com 92
#addequip 2
#end

#selectevent 1679
#rarity -2
#req_unique 1
#req_land 0
#req_luck 2
#req_magic 1
#req_lab 0
#lab 1
#nation -2
#com 103
#addequip 1
#2com 102
#addequip 2
#end

#selectevent 1680
#rarity -2
#req_unique 1
#req_land 0
#req_luck 3
#req_magic 3
#req_lab 0
#lab 1
#nation -2
#com 103
#addequip 1
#4com 102
#addequip 2
#end

#selectevent 1681
#rarity -1
#req_monster 96
#req_unique 2
#req_luck 3
#waterboost 96
#end

#selectevent 1682
#rarity -1
#req_monster 95
#req_unique 2
#req_luck 3
#deathboost 95
#end

#selectevent 1683
#rarity -1
#req_monster 93
#req_unique 2
#req_luck 3
#airboost 93
#end

#selectevent 1684
#rarity -1
#req_monster 103
#req_unique 2
#req_luck 3
#waterboost 103
#end

#selectevent 1685
#rarity -1
#req_monster 99
#req_unique 2
#req_luck 3
#fireboost 99
#end

#selectevent 1686
#rarity -1
#req_monster 105
#req_unique 2
#req_luck 3
#req_forest 1
#req_growth 1
#natureboost 105
#end

#selectevent 1687
#rarity -2
#req_unique 1
#req_land 1
#req_luck 2
#req_magic 1
#req_lab 0
#nation -2
#lab 1
#com 99
#addequip 1
#2com 98
#addequip 0
#end

#selectevent 1688
#rarity -2
#req_unique 1
#req_land 1
#req_luck 3
#req_magic 3
#req_lab 0
#nation -2
#lab 1
#com 99
#addequip 1
#4com 98
#addequip 0
#end

#selectevent 1689
#rarity -2
#req_luck 2
#req_mountain 1
#req_turn 15
#req_noseason 3
#nation -2
#com 1565
#gainaff 549755813888
#3d6units 523
#end

#selectevent 1690
#rarity -2
#req_luck 2
#req_magic 2
#req_chaos 1
#req_land 1
#nation -2
#com 468
#4d6units 467
#3d6units 455
#3d6units 453
#end

#selectevent 1691
#rarity -1
#req_unique 3
#req_magic 2
#req_chaos 1
#req_monster 34
#req_land 1
#killmon 34
#nation -2
#1unit 466
#end

#selectevent 1692
#rarity -1
#req_unique 3
#req_magic 2
#req_chaos 1
#req_monster 35
#req_land 1
#killmon 35
#nation -2
#1unit 466
#end

#selectevent 1693
#rarity -1
#req_unique 3
#req_magic 2
#req_chaos 1
#req_monster 36
#req_land 1
#killmon 36
#nation -2
#1unit 466
#end

#selectevent 1694
#rarity -1
#req_unique 3
#req_magic 2
#req_chaos 1
#req_monster 57
#req_land 1
#killmon 57
#nation -2
#1unit 466
#end

#selectevent 1695
#rarity -1
#req_unique 1
#req_forest 1
#req_chaos 1
#req_magic 1
#req_monster 57
#nation -2
#com 1954
#end

#selectevent 1696
#rarity -1
#req_coast 1
#req_turn 20
#req_chaos 2
#req_luck 2
#req_season 0
#nation -2
#1d6units 513
#com 92
#end

#selectevent 1697
#rarity -1
#req_coast 1
#req_turn 20
#req_chaos 2
#req_luck 3
#req_season 0
#nation -2
#3d6units 513
#com 92
#end

#selectevent 1698
#rarity -1
#req_mountain 1
#req_turn 20
#req_chaos 2
#req_luck 2
#req_season 3
#nation -2
#1d6units 515
#com 98
#decscale3 2
#end

#selectevent 1699
#rarity -1
#req_mountain 1
#req_turn 20
#req_chaos 2
#req_luck 3
#req_season 3
#nation -2
#3d6units 515
#com 98
#decscale3 2
#end

#selectevent 1700
#rarity -2
#req_turn 25
#req_land 1
#req_luck 2
#req_death 1
#nation -2
#com 185
#3d6units 533
#end

#selectevent 1701
#rarity -2
#req_turn 25
#req_land 1
#req_luck 3
#req_death 2
#nation -2
#com 299
#7d6units 533
#end

#selectevent 1702
#rarity -2
#req_luck 1
#req_magic 0
#req_land 1
#1d3vis 51
#end

#selectevent 1703
#rarity -2
#req_luck 2
#req_magic 0
#req_land 1
#1d6vis 51
#end

#selectevent 1704
#rarity -2
#req_luck 3
#req_magic 0
#req_land 1
#2d4vis 51
#end

#selectevent 1705
#rarity -2
#req_rare 20
#req_luck 2
#req_magic 1
#1d6vis 0
#1d6vis 0
#magicitem 9
#magicitem 9
#end

#selectevent 1706
#rarity -2
#req_rare 20
#req_luck 2
#req_magic 0
#2d6vis 0
#2d6vis 0
#magicitem 9
#magicitem 9
#end

#selectevent 1707
#rarity -2
#req_rare 20
#req_luck 3
#req_magic 0
#4d6vis 0
#4d6vis 0
#magicitem 9
#magicitem 9
#magicitem 9
#end

#selectevent 1708
#rarity -2
#req_rare 20
#req_luck 1
#req_magic 0
#1d6vis 2
#1d6vis 2
#magicitem 9
#end

#selectevent 1709
#rarity -2
#req_rare 20
#req_luck 2
#req_magic 0
#2d6vis 2
#2d6vis 2
#magicitem 9
#magicitem 9
#end

#selectevent 1710
#rarity -2
#req_rare 20
#req_luck 3
#req_magic 0
#4d6vis 2
#4d6vis 2
#magicitem 9
#magicitem 9
#magicitem 9
#end

#selectevent 1711
#rarity -1
#req_rare 20
#req_luck 1
#req_magic 0
#req_swamp 1
#req_unique 1
#req_rare 50  -- stored twice; a second #req_rare in a mod replaces the first
#1d6vis 2
#1d6vis 2
#magicitem 9
#end

#selectevent 1712
#rarity -1
#req_rare 20
#req_luck 2
#req_magic 0
#req_swamp 1
#req_rare 50  -- stored twice; a second #req_rare in a mod replaces the first
#2d6vis 2
#2d6vis 2
#magicitem 9
#magicitem 9
#end

#selectevent 1713
#rarity -1
#req_rare 20
#req_luck 3
#req_magic 0
#req_swamp 1
#req_rare 50  -- stored twice; a second #req_rare in a mod replaces the first
#4d6vis 2
#4d6vis 2
#magicitem 9
#magicitem 9
#magicitem 9
#end

#selectevent 1714
#rarity -2
#req_rare 20
#req_luck 2
#req_magic 0
#2d6vis 0
#magicitem 9
#end

#selectevent 1715
#rarity -2
#req_pathfire 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 98
#addequip 1
#end

#selectevent 1716
#rarity -2
#req_luck 1
#req_pathfire 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 99
#addequip 1
#end

#selectevent 1717
#rarity -2
#req_luck 2
#req_pathfire 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 99
#addequip 1
#2com 98
#end

#selectevent 1718
#rarity -2
#req_pathwater 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 102
#addequip 1
#end

#selectevent 1719
#rarity -2
#req_luck 1
#req_pathwater 4
#req_lab 1
#req_magic 0
#nation -2
#com 103
#addequip 1
#end

#selectevent 1720
#rarity -2
#req_luck 2
#req_pathwater 4
#req_lab 1
#req_magic 0
#nation -2
#com 103
#addequip 1
#2com 102
#end

#selectevent 1721
#rarity -2
#req_pathair 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 92
#addequip 1
#end

#selectevent 1722
#rarity -2
#req_luck 1
#req_pathair 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 93
#addequip 1
#end

#selectevent 1723
#rarity -2
#req_luck 2
#req_pathair 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 93
#addequip 1
#2com 92
#end

#selectevent 1724
#rarity -2
#req_pathdeath 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 310
#addequip 1
#end

#selectevent 1725
#rarity -2
#req_luck 1
#req_pathdeath 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 310
#addequip 1
#end

#selectevent 1726
#rarity -2
#req_luck 1
#req_pathdeath 4
#req_lab 1
#req_magic 0
#req_land 1
#nation -2
#com 310
#addequip 1
#2com 94
#end

#selectevent 1727
#rarity -2
#req_pathfire 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 98
#addequip 1
#end

#selectevent 1728
#rarity -2
#req_luck 1
#req_pathfire 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 99
#addequip 1
#end

#selectevent 1729
#rarity -2
#req_luck 1
#req_pathfire 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 99
#addequip 1
#2com 98
#end

#selectevent 1730
#rarity -2
#req_pathwater 4
#req_lab 1
#req_magic 3
#nation -2
#com 102
#addequip 1
#end

#selectevent 1731
#rarity -2
#req_luck 1
#req_pathwater 4
#req_lab 1
#req_magic 3
#nation -2
#com 103
#addequip 1
#end

#selectevent 1732
#rarity -2
#req_luck 1
#req_pathwater 4
#req_lab 1
#req_magic 3
#nation -2
#com 103
#addequip 1
#2com 102
#end

#selectevent 1733
#rarity -2
#req_pathair 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 92
#addequip 1
#end

#selectevent 1734
#rarity -2
#req_luck 1
#req_pathair 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 93
#addequip 1
#end

#selectevent 1735
#rarity -2
#req_luck 1
#req_pathair 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 93
#addequip 1
#2com 92
#end

#selectevent 1736
#rarity -2
#req_pathdeath 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 310
#addequip 1
#end

#selectevent 1737
#rarity -2
#req_luck 1
#req_pathdeath 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 310
#addequip 1
#end

#selectevent 1738
#rarity -2
#req_luck 1
#req_pathdeath 4
#req_lab 1
#req_magic 3
#req_land 1
#nation -2
#com 310
#addequip 1
#2com 94
#end

#selectevent 1739
#rarity -2
#req_pathearth 1
#req_pathfire 1
#req_lab 1
#req_magic 3
#req_pathastral 4
#req_land 1
#nation -2
#com 480
#addequip 1
#end

#selectevent 1740
#rarity -2
#req_pathearth 1
#req_pathfire 1
#req_lab 1
#req_magic 3
#req_pathastral 4
#req_land 1
#nation -2
#com 481
#addequip 1
#end

#selectevent 1741
#rarity -2
#req_pathearth 1
#req_pathfire 1
#req_lab 1
#req_magic 3
#req_pathastral 4
#req_land 1
#nation -2
#com 481
#addequip 1
#2com 480
#end

#selectevent 1742
#rarity -1
#req_coast 1
#req_luck 2
#req_minpop 200
#landgold 15
#end

#selectevent 1743
#rarity -1
#req_coast 1
#req_luck 3
#req_minpop 200
#landgold 25
#end

#selectevent 1744
#rarity -1
#req_luck 1
#req_turn 15
-- ro: effect 190 (gold) = 500
#1d3vis 51
#magicitem 2
#end

#selectevent 1745
#rarity -1
#req_luck 2
#req_turn 15
-- ro: effect 190 (gold) = 1000
#1d6vis 51
#magicitem 3
#end

#selectevent 1746
#rarity -1
#req_luck 3
#req_turn 15
-- ro: effect 190 (gold) = 1500
#2d4vis 51
#magicitem 4
#end

#selectevent 1747
#rarity -1
#req_luck 1
#req_lab 0
#req_unique 2
#lab 1
#1d3vis 52
#magicitem 2
#end

#selectevent 1748
#rarity -1
#req_luck 2
#req_lab 0
#req_unique 2
#lab 1
#1d6vis 52
#magicitem 3
#end

#selectevent 1749
#rarity -1
#req_luck 3
#req_lab 0
#req_unique 2
#lab 1
#2d4vis 52
#magicitem 4
#end

#selectevent 1750
#rarity -1
#req_luck 1
#landgold 10
#end

#selectevent 1751
#rarity -1
#req_luck 2
#landgold 20
#end

#selectevent 1752
#rarity 1
#req_unluck 1
#req_minpop 200
#req_land 1
#landgold -10
#end

#selectevent 1753
#rarity 1
#req_unluck 2
#req_minpop 200
#req_land 1
#landgold -20
#end

#selectevent 1754
#rarity 1
#req_unluck 1
#req_minpop 200
#req_land 0
#landgold -10
#end

#selectevent 1755
#rarity 1
#req_unluck 2
#req_minpop 200
#req_land 0
#landgold -20
#end

#selectevent 1756
#rarity 2
#req_unluck 2
#req_minpop 200
#req_order 3
#landgold -10
#landprod -10
#end

#selectevent 1757
#rarity 2
#req_unluck 3
#req_minpop 200
#req_order 3
#landgold -15
#landprod -15
#end

#selectevent 1758
#rarity 2
#req_unluck 2
#req_pathfire 1
#req_lab 1
#req_turn 15
#lab 0
-- ro: effect 39 = 6
#assassin 3716
#end

#selectevent 1759
#rarity 2
#req_lazy 2
#req_temple 1
#req_turn 15
#req_dominion 3
#temple 0
#incdom -3
#end

#selectevent 1760
#rarity 1
#req_order 2
#req_minpop 300
#landgold -10
#end

#selectevent 1761
#rarity 1
#req_commander 0
#req_minunrest 10
#req_maxtroops 0
#req_capital 0
#req_order 3
#req_land 1
#revolt
#unrest 20
#incscale3 0
#incdom -2
#2com 18
#9d6units 18
#end

#selectevent 1762
#rarity 1
#req_lazy 2
#req_mindef 6
#defence -5
#end

#selectevent 1763
#rarity 1
#req_order 2
#emigration 5
#end

#selectevent 1764
#rarity -1
#req_land 1
#req_owncapital 1
#req_nation 57
#req_luck 0
#req_unique 1
#nation -2
#com 658
#addequip 2
#1unit 694
#end

#selectevent 1765
#rarity -1
#req_season 0
#req_unluck 1
#req_monster 2227
#req_monster 1565  -- stored twice; a second #req_monster in a mod replaces the first
#req_unique 3
#req_land 1
#req_cave 0
#nation -2
#3d6units 2227
#end

#selectevent 1766
#rarity 1
#req_monster 25
#req_turn 15
#req_land 1
#2com 2131
#9d6units 2131
#end

#selectevent 1767
#rarity 1
#req_monster 57
#req_turn 15
#req_land 1
#2com 2131
#9d6units 2131
#end

#selectevent 1768
#rarity 1
#req_monster 24
#req_turn 15
#req_land 1
#2com 2131
#9d6units 2131
#end

#selectevent 1769
#rarity 2
#req_growth 2
#req_land 1
#req_unluck 2
#req_turn 25
#req_unique 1
#2com 489
#com 489
#addequip 2
#6d6units 489
#end

#selectevent 1770
#rarity -2
#req_unluck 2
#req_land 1
#req_turn 15
#req_noera 1
#req_unique 2
#req_poptype 30
#com 20
#2com 36
#addequip 2
#12d6units 40
#9d6units 290
#end

#selectevent 1771
#rarity 1
#req_hiddensite 1
#kill 50
#unrest 10
#end

#selectevent 1772
#rarity -1
#req_story 1
#req_hiddensite 1
#req_luck 0
#req_magic 0
#req_unique 1
#req_code 0
#flagland 1
#code 135
#end

#selectevent 1773
#rarity 0
#req_hiddensite 1
#req_pathastral 3
#req_code 135
#code 136
#end

#selectevent 1774
#rarity 0
#req_hiddensite 1
#req_pathastral 3
#req_rare 33
#req_code 136
#code 0
#revealsite
#nation -2
#com 340
#astralboost 340
#end

#selectevent 1775
#rarity 0
#req_hiddensite 1
#req_pathastral 3
#req_rare 15
#req_code 135
#2com 1983
#9d6units 1983
#end

#selectevent 1776
#rarity 0
#req_foundsite 1
#req_code 135
#code 0
#nation -2
#com 340
#addequip 3
#astralboost 340
#end

#selectevent 1777
#rarity -1
#req_story 2
#req_hiddensite 1
#req_luck 0
#req_magic 0
#req_unique 1
#req_code 0
#flagland 1
#code 138
#end

#selectevent 1778
#rarity 0
#req_hiddensite 1
#req_pathdeath 3
#req_code 138
#code 139
#end

#selectevent 1779
#rarity 0
#req_hiddensite 1
#req_pathdeath 3
#req_rare 33
#req_code 139
#code 0
#revealsite
#nation -2
#com 178
#6d6units 533
#end

#selectevent 1780
#rarity 0
#req_hiddensite 1
#req_pathdeath 3
#req_rare 15
#req_code 139
#2com 533
#9d6units 533
#com 998
#end

#selectevent 1781
#rarity 0
#req_foundsite 1
#req_code 139
#code 0
#nation -2
#com 178
#6d6units 533
#end

#selectevent 1782
#rarity 0
#req_indepok
#req_foundsite 1
#req_targmnr 178
#req_unique 1
#req_rare 15
#deathboost 178
#end

#selectevent 1783
#rarity 0
#req_rare 1
#req_story 1
#req_hiddensite 1
#req_code 0
#code 141
#unrest 3
#end

#selectevent 1784
#rarity 0
#req_code 141
#req_hiddensite 1
#code 142
#decscale 5
#end

#selectevent 1785
#rarity 0
#req_foundsite 1
#req_code 142
#code 143
-- ro: effect 190 (gold) = 1000
#magicitem 3
#1d6vis 51
#magicitem 2
#end

#selectevent 1786
#rarity 0
#req_unique 1
#req_foundsite 1
#req_rare 70
#req_code 143
#code 144
#kill 5
#unrest 30
#com 2534
#addequip 1
#end

#selectevent 1787
#rarity 0
#req_nomonster 2534
#req_code 144
#code 0
#magicitem 9
#magicitem 9
#1d3vis 51
#incscale2 5
#end

#selectevent 1788
#rarity 2
#req_unique 2
#req_monster 2227
#req_code 0
#code 145
#killmon 2227
#end

#selectevent 1789
#rarity 0
#req_rare 4
#req_unique 1
#req_story 2
#req_monster 2227
#req_code 0
#code 145
#killmon 2227
#end

#selectevent 1790
#rarity 0
#req_monster 2227
#req_code 145
#code 0
#com 2534
#addequip 1
#end

#selectevent 1791
#rarity 0
#req_nomonster 2227
#req_code 145
#code 0
#1d6vis 0
#end

#selectevent 1792
#rarity -1
#req_story 1
#req_hiddensite 1
#req_unique 1
#req_code 0
#flagland 1
#code 146
#decscale3 3
#decscale2 5
#end

#selectevent 1793
#rarity 0
#req_hiddensite 1
#req_pathnature 3
#req_code 146
#code 147
#end

#selectevent 1794
#rarity 0
#req_hiddensite 1
#req_pathnature 3
#req_code 147
#code 148
-- ro: effect 190 (gold) = 10
#magicitem 2
#end

#selectevent 1795
#rarity 0
#req_hiddensite 1
#req_pathnature 3
#req_code 148
#code 0
#revealsite
#nation -2
#com 552
#natureboost 552
#end

#selectevent 1796
#rarity 0
#req_hiddensite 1
#req_pathfire 2
#req_code 146
#code 148
#1d6vis 6
#end

#selectevent 1797
#rarity 0
#req_hiddensite 1
#req_rare 50
#req_pathfire 2
#req_code 148
#code 0
#com 552
#addequip 1
#natureboost 552
-- ro: effect 177 (18d6units) = 362
#revealsite
#end

#selectevent 1798
#rarity 0
#req_rare 20
#req_targpath1 0
#req_hiddensite 1
#req_unique 1
#req_code 148
-- ro: effect 39 = 2
#assassin 362
#end

#selectevent 1799
#rarity 0
#req_rare 40
#req_targpath1 0
#req_hiddensite 1
#req_unique 1
#req_code 148
-- ro: effect 39 = 2
#assassin 361
#end

#selectevent 1800
#rarity 0
#req_foundsite 1
#req_code 146
#code 0
#magicitem 2
#magicitem 3
#2d6vis 6
#incscale3 3
#incscale2 5
#end

#selectevent 1801
#rarity 0
#req_foundsite 1
#req_code 147
#code 0
#magicitem 2
#magicitem 3
#2d6vis 6
#incscale3 3
#incscale2 5
#end

#selectevent 1802
#rarity 0
#req_foundsite 1
#req_code 148
#code 0
#magicitem 2
#magicitem 3
#2d6vis 6
#incscale3 3
#incscale2 5
#end

#selectevent 1803
#rarity -2
#req_magic 1
#req_land 1
#1d3vis 4
#1d3vis 6
#end

#selectevent 1804
#rarity -2
#req_magic 2
#req_land 1
#1d3vis 4
#1d3vis 6
#1d3vis 5
#end

#selectevent 1805
#rarity -2
#req_magic 3
#req_land 1
#1d3vis 4
#1d3vis 6
#1d3vis 5
#1d3vis 0
#end

#selectevent 1806
#rarity -2
#req_magic 3
#req_land 1
#2d4vis 56
#end

#selectevent 1807
#rarity -1
#req_magic 1
#req_land 1
-- ro: effect 190 (gold) = 50
#1d3vis 56
#end

#selectevent 1808
#rarity -1
#req_magic 2
#req_land 1
-- ro: effect 190 (gold) = 50
#1d6vis 56
#end

#selectevent 1809
#rarity -1
#req_magic 3
#req_land 1
-- ro: effect 190 (gold) = 50
#2d4vis 56
#magicitem 2
#end

#selectevent 1810
#rarity -2
#req_magic 1
#req_season 1
#req_farm 1
#1d6vis 6
#end

#selectevent 1811
#rarity -2
#req_magic 2
#req_season 1
#req_farm 1
#2d6vis 6
#end

#selectevent 1812
#rarity -2
#req_magic 3
#req_season 1
#req_farm 1
#3d6vis 6
#end

#selectevent 1813
#rarity -2
#req_magic 1
#req_noseason 3
#req_land 1
#1d6vis 51
#end

#selectevent 1814
#rarity -2
#req_magic 2
#req_noseason 3
#req_land 1
#2d4vis 51
#end

#selectevent 1815
#rarity -2
#req_magic 3
#req_noseason 3
#req_land 1
#2d6vis 51
#end

#selectevent 1816
#rarity -2
#req_magic 1
#req_noseason 3
#req_mountain 1
#magicitem 1
#magicitem 1
#end

#selectevent 1817
#rarity -2
#req_magic 2
#req_noseason 3
#req_mountain 1
#magicitem 2
#magicitem 1
#end

#selectevent 1818
#rarity -2
#req_magic 3
#req_noseason 3
#req_mountain 1
#magicitem 3
#magicitem 2
#end

#selectevent 1819
#rarity -2
#req_magic 1
#req_noseason 3
#req_land 1
#1d6vis 3
#end

#selectevent 1820
#rarity -2
#req_magic 2
#req_noseason 3
#req_land 1
#2d4vis 3
#end

#selectevent 1821
#rarity -2
#req_magic 3
#req_noseason 3
#req_land 1
#2d6vis 3
#end

#selectevent 1822
#rarity -1
#req_magic 1
#req_land 0
-- ro: effect 190 (gold) = 50
#1d3vis 51
#1d3vis 4
#1d3vis 6
#1d3vis 5
#end

#selectevent 1823
#rarity -1
#req_magic 2
#req_land 0
-- ro: effect 190 (gold) = 50
#1d3vis 51
#1d6vis 4
#1d6vis 6
#1d6vis 5
#end

#selectevent 1824
#rarity -1
#req_magic 3
#req_land 0
-- ro: effect 190 (gold) = 50
#1d3vis 51
#2d4vis 4
#1d3vis 6
#1d6vis 5
#end

#selectevent 1825
#rarity -2
#req_magic 1
#req_land 0
#1d6vis 51
#end

#selectevent 1826
#rarity -2
#req_magic 2
#req_land 0
#2d4vis 51
#end

#selectevent 1827
#rarity -2
#req_magic 3
#req_land 0
#2d6vis 51
#end

#selectevent 1828
#rarity -2
#req_magic 1
#req_land 0
#1d3vis 4
#1d3vis 6
#end

#selectevent 1829
#rarity -2
#req_magic 2
#req_land 0
#1d3vis 4
#1d3vis 6
#1d3vis 5
#end

#selectevent 1830
#rarity -2
#req_magic 3
#req_land 0
#1d3vis 4
#1d3vis 6
#1d3vis 5
#1d3vis 0
#end

#selectevent 1831
#rarity -1
#req_luck 2
#req_land 1
#req_turn 10
#req_era 3
#req_unique 2
#req_owncapital 1
#nation -2
#com 291
#addequip 2
#12d6units 287
#9d6units 288
#end

#selectevent 1832
#rarity -1
#req_luck 2
#req_land 0
#req_turn 10
#req_unique 2
#req_owncapital 1
#nation -2
#com 976
#addequip 2
#12d6units 975
#9d6units 2414
#end

#selectevent 1833
#rarity -2
#req_death 3
#req_land 1
#req_pathdeath 2
#req_turn 10
#req_unique 2
#req_owncapital 1
#nation -2
#com 181
#end

#selectevent 1834
#rarity -2
#req_pathnature 2
#req_forest 1
#req_unique 2
#nation -2
#com 488
#end

#selectevent 1835
#rarity -2
#req_pathblood 2
#req_land 1
#req_death 2
#req_turn 35
#req_unique 1
#nation -2
#com 489
#addequip 2
#6d6units 489
#end

#selectevent 1836
#rarity -2
#req_luck 2
#req_land 1
#req_capital 1
#req_growth 0
#req_unique 1
#nation -2
#com 2323
#com 2325
#com 2327
#addequip 2
#end

#selectevent 1837
#rarity -2
#req_luck 1
#req_magic 1
#req_growth 1
#req_farm 1
#req_season 2
#1d6vis 51
#1d3vis 4
#1d3vis 6
#end

#selectevent 1838
#rarity -2
#req_luck 2
#req_magic 1
#req_growth 1
#req_farm 1
#req_season 2
#2d4vis 51
#1d3vis 4
#1d3vis 6
#1d3vis 5
#end

#selectevent 1839
#rarity -2
#req_luck 3
#req_magic 1
#req_growth 1
#req_farm 1
#req_season 2
#2d6vis 51
#1d3vis 4
#1d3vis 6
#1d3vis 5
#1d3vis 4
#end

#selectevent 1840
#rarity -2
#req_luck 1
#req_magic 1
#req_growth 1
#req_land 1
#req_noseason 3
#1d6vis 51
#1d3vis 4
#1d3vis 6
#end

#selectevent 1841
#rarity -2
#req_luck 2
#req_magic 1
#req_growth 1
#req_land 1
#req_noseason 3
#2d4vis 51
#1d3vis 4
#1d3vis 6
#1d3vis 5
#end

#selectevent 1842
#rarity -2
#req_luck 3
#req_magic 1
#req_growth 1
#req_land 1
#req_noseason 3
#2d6vis 51
#1d3vis 4
#1d3vis 6
#1d3vis 5
#1d3vis 4
#end

#selectevent 1843
#rarity 2
#req_growth 2
#req_unluck 0
#req_forest 1
#req_noseason 3
#landgold -5
#landprod -5
#end

#selectevent 1844
#rarity 2
#req_growth 2
#req_unluck 1
#req_forest 1
#req_noseason 3
#landgold -10
#landprod -10
#end

#selectevent 1845
#rarity 2
#req_growth 2
#req_mindef 1
#req_maxtroops 8
#req_forest 1
#4com 932
#9d6units 932
#end

#selectevent 1846
#rarity 2
#req_growth 2
#req_mindef 1
#req_maxtroops 8
#req_forest 1
#4com 284
#15d6units 284
#nation -2
#com 1565
#6d6units 1565
#end

#selectevent 1847
#rarity -1
#req_turn 12
#req_unique 2
#req_growth 2
#req_mindef 5
#req_maxtroops 10
#req_forest 1
#req_turn 10  -- stored twice; a second #req_turn in a mod replaces the first
#com 310
#addequip 1
#9d6units 618
#nation -2
#com 552
#4d6units 361
#end

#selectevent 1848
#rarity -2
#req_maxunrest 10
#req_minpop 50
#req_land 0
#2d6vis 5
#unrest 25
#decscale 5
#end

#selectevent 1849
#rarity -2
#req_maxunrest 10
#req_minpop 50
#req_coast 1
#req_rare 10
#2d6vis 5
#end

#selectevent 1850
#rarity 1
#req_maxdominion 3
#req_mydominion 1
#req_season 1
#req_minpop 50
#req_land 0
#incdom -2
#decscale2 3
#taxboost 100
#end

#selectevent 1851
#rarity -1
#req_minunrest 20
#req_poptype 65
#req_mindef 12
#req_order -1
#unrest -30
#decscale 0
#nation -2
#com 976
#end

#selectevent 1852
#rarity -1
#req_land 0
#req_rare 25
#2d4vis 4
#end

#selectevent 1853
#rarity -1
#req_fornation 101
#req_turn 20
#req_unique 1
#req_nearbysite 1
#req_code 0
#code -11
#end

#selectevent 1854
#rarity 0
#req_monster 740
#req_code -11
#code 0
#nation -2
#com 3274
#end

#selectevent 1855
#rarity 11
#req_capital 0
#req_story 1
#req_unique 1
#req_turn 23
#req_coast 1
#req_season 1
#req_code 0
#req_noera 3
#req_fort 0
#req_maxtroops 20
#req_noench 28
#code -19
#code2 -14
#com 2449
#bloodboost 1
#bloodboost 1
#addequip 1
#4com 2532
-- ro: effect 183 (24d6units) = 2531
#15d6units 2533
#3d6units 544
#end

#selectevent 1856
#rarity 0
#req_indepok
#req_nomonster 2449
#req_nomonster 1605  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_nomonster 1604  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_nomonster 2192  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code -14
#req_code -73
#req_code -85
#req_code -84
#code -18
#incdom 4
-- ro: effect 190 (gold) = 1500
#magicitem 3
#magicitem 2
#resetcode -72
#resetcode -73
#resetcode -74
#resetcode -85
#resetcode -84
#end

#selectevent 1857
#rarity 13
#req_season 0
#req_code 0
#req_anycode -14
#resetcode -14
#resetcode -15
#resetcode -72
#end

#selectevent 1858
#rarity 0
#req_rare 2
#req_coast 1
#req_maxdef 19
#req_noseason 3
#req_anycode -14
#req_code 0
#code -15
#end

#selectevent 1859
#rarity 0
#req_story 2
#req_maxdef 19
#req_maxtroops 20
#req_rare 80
#req_noseason 3
#req_code -15
#code 0
#com 2532
#15d6units 2531
#6d6units 2533
#2com 2533
#end

#selectevent 1860
#rarity 0
#req_maxdef 19
#req_maxtroops 20
#req_rare 80
#req_noseason 3
#req_code -15
#code 0
#com 2532
#12d6units 2531
#4d6units 2533
#2com 2533
#end

#selectevent 1861
#rarity 0
#req_rare 4
#req_coast 1
#req_maxdef 15
#req_noseason 3
#req_anycode -14
#req_code 0
#code 0
#com 2532
#9d6units 2531
#3d6units 2533
#2com 2533
#end

#selectevent 1862
#rarity 0
#req_rare 70
#req_mindef 18
#req_noseason 3
#req_code -15
#code 0
#incdom 3
#end

#selectevent 1863
#rarity 0
#req_rare 5
#req_coast 1
#req_mindef 18
#req_noseason 3
#req_anycode 14
#req_code 0
#code 0
#incdom 3
#end

#selectevent 1864
#rarity 0
#req_story 2
#req_rare 8
#req_freshwater 1
#req_maxdef 15
#req_noseason 3
#req_anycode -14
#req_code 0
#code 0
#com 2532
#7d6units 2531
#6d6units 2533
#2com 2533
#end

#selectevent 1865
#rarity 0
#req_rare 8
#req_freshwater 1
#req_maxdef 15
#req_noseason 3
#req_anycode -14
#req_code 0
#code 0
#com 2532
#6d6units 2531
#3d6units 2533
#2com 2533
#end

#selectevent 1866
#rarity 0
#req_rare 2
#req_coast 1
#req_maxdef 18
#req_noseason 3
#req_anycode -14
#req_code 0
#code -15
#com 2533
#1d6units 2531
#1d6units 2533
#end

#selectevent 1867
#rarity 0
#req_rare 6
#req_coast 1
#req_maxdef 18
#req_noseason 3
#req_anycode -14
#req_code 0
#code 0
#com 2532
#9d6units 2531
#6d6units 2533
#2com 2533
#end

#selectevent 1868
#rarity 13
#req_unique 1
#req_code -18
#code 0
#resetcode -14
#resetcode -15
#end

#selectevent 1869
#rarity 0
#req_rare 40
#req_nearbycode -14
#req_unique 5
#req_land 1
#req_code 0
#req_indepok
#code -72
#kill 2
#taxboost -40
#unrest 15
#end

#selectevent 1870
#rarity 13
#req_indepok
#req_unique 1
#req_targmnr 2449
#req_code -14
#req_anycode -72
#resetcode -72
#addequip 1
#bloodboost 1
#revealprov
#end

#selectevent 1871
#rarity 13
#req_indepok
#req_unique 5
#req_targmnr 2449
#req_code -14
#req_anycode -72
#resetcode -72
#addequip 1
#deathboost 1
#com 2532
#bloodboost 1
#end

#selectevent 1872
#rarity 13
#req_indepok
#req_story 3  -- outside the command's range 0..2
#req_unique 1
#req_targpath3 5
#req_targmnr 2449
#req_code -14
#code -73
#deathboost 1
#com 2192
#6d6units 915
#6d6units 2119
#delay25 6
#end

#selectevent 1873
#rarity 13
#req_monster 2449
#req_code -73
#code -85
#revealprov
#6d6units 2531
#3d6units 2533
#4d6units 2190
#end

#selectevent 1874
#rarity 0
#req_rare 25
#req_unique 6
#req_indepok
#req_code -85
#2com 2192
#6d6units 2531
#3d6units 2533
#3d6units 2190
#2com 2190
#end

#selectevent 1875
#rarity 0
#req_rare 15
#req_unique 5
#req_indepok
#req_code -85
#pathboost 5
#end

#selectevent 1876
#rarity 0
#req_rare 15
#req_unique 5
#req_indepok
#req_code -85
#pathboost 8
#end

#selectevent 1877
#rarity 0
#req_rare 15
#req_unique 5
#req_indepok
#req_targhumanoid 1
#req_code -85
#addequip 2
#end

#selectevent 1878
#rarity 0
#req_rare 15
#req_unique 5
#req_indepok
#req_targpath1 1
#req_code -85
#addequip 1
#end

#selectevent 1879
#rarity 0
#req_rare 10
#req_unique 3
#req_indepok
#req_code -85
#2com 2192
#3d6units 2531
#1d6units 2533
#1d6units 2190
#2com 2190
#end

#selectevent 1880
#rarity 0
#req_rare 7
#req_unique 1
#req_indepok
#req_targmnr 2449
#req_code -85
#bloodboost 1
#addequip 1
#1d6units 638
#end

#selectevent 1881
#rarity 0
#req_rare 7
#req_unique 2
#req_indepok
#req_targmnr 2449
#req_code -85
#astralboost 1
#1d6units 638
#end

#selectevent 1882
#rarity 0
#req_rare 7
#req_unique 3
#req_indepok
#req_targmnr 2449
#req_code -85
#deathboost 1
#addequip 1
#1d6units 638
#end

#selectevent 1883
#rarity 0
#req_noench 28
#req_rare 3
#req_coast 1
#req_maxdef 16
#req_code -85
#kill 1
#unrest 10
#end

#selectevent 1884
#rarity 0
#req_rare 1
#req_land 1
#req_maxdef 16
#req_code -85
#2com 638
#3d6units 638
#end

#selectevent 1885
#rarity 0
#req_site 0
#req_rare 10
#req_land 1
#req_unique 2
#req_code 0
#req_nearbycode -85
#code -86
#2com 2192
#6d6units 2531
#6d6units 2533
#6d6units 2190
#end

#selectevent 1886
#rarity 0
#req_nomonster 2192
#req_land 1
#req_code -87
#req_code -86
#code 0
#unrest -20
#incdom 3
-- ro: effect 190 (gold) = 300
#magicitem 2
#end

#selectevent 1887
#rarity 0
#req_site 1
#req_monster 2192
#req_rare 25
#req_indepok
#req_land 1
#req_code -86
#req_unique 1
#code -87
#4d6units 2531
#4d6units 2533
#3d6units 2190
#addsite -1
#end

#selectevent 1888
#rarity 0
#req_indepok
#req_targmnr 2192
#req_unique 4
#req_rare 13
#req_land 1
#req_code -87
#deathboost 1
#addequip 1
#4d6units 2531
#4d6units 2533
#3d6units 2190
#end

#selectevent 1889
#rarity 0
#req_indepok
#req_rare 10
#req_land 0
#req_unique 1
#req_code 0
#req_nearbycode -85
#req_noench 28
#code -86
#2com 2192
#deathboost 1
#7d6units 2190
#end

#selectevent 1890
#rarity 0
#req_monster 2192
#req_rare 25
#req_indepok
#req_land 0
#req_code -86
#code -87
#addsite -1
#3d6units 2190
#end

#selectevent 1891
#rarity 0
#req_indepok
#req_targmnr 2192
#req_unique 6
#req_rare 13
#req_land 0
#req_code -87
#deathboost 1
#addequip 1
#3d6units 2190
#end

#selectevent 1892
#rarity 0
#req_nomonster 2192
#req_land 0
#req_indepok
#req_code -87
#req_code -86
#code 0
#unrest -20
#incdom 3
-- ro: effect 190 (gold) = 300
#magicitem 2
#end

#selectevent 1893
#rarity 0
#req_indepok
#req_unique 2
#req_land 0
#req_targmnr 2192
#req_code -87
#deathboost 1
#waterboost 1
#addequip 1
#end

#selectevent 1894
#rarity 0
#req_indepok
#req_unique 2
#req_land 1
#req_targmnr 2192
#req_code -87
#deathboost 1
#waterboost 1
#addequip 1
#end

#selectevent 1895
#rarity 0
#req_rare 1
#req_commander 1
#req_anycode -85
#assassin 638
#end

#selectevent 1896
#rarity 0
#req_rare 50
#req_unique 2
#req_mintroops 50
#req_nearbycode -85
#curse 20
#end

#selectevent 1897
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_anycode -85
#gainaff 33554432
#end

#selectevent 1898
#rarity 0
#req_targpath1 53
#req_targgod 0
#req_rare 50
#req_unique 3
#req_nearbycode -85
#gainaff 33554432
#end

#selectevent 1899
#rarity 0
#req_rare 15
#req_unique 2
#req_indepok
#req_targmnr 1
#req_code -85
#addequip 9
#end

#selectevent 1900
#rarity 0
#req_rare 15
#req_unique 2
#req_indepok
#req_targmnr 1
#req_code -85
#addequip 9
#end

#selectevent 1901
#rarity 0
#req_rare 15
#req_unique 2
#req_indepok
#req_targmnr 1
#req_code -85
#addequip 9
#end

#selectevent 1902
#rarity 0
#req_rare 15
#req_unique 2
#req_indepok
#req_targmnr 1
#req_code -85
#addequip 9
#end

#selectevent 1903
#rarity 0
#req_rare 55
#req_commander 1
#req_unique 4
#req_mintroops 10
#req_nearbycode -85
#assassin 638
#end

#selectevent 1904
#rarity 0
#req_rare 55
#req_unique 2
#req_mintroops 20
#req_fort 0
#req_nearbycode -85
#4com -2
-- ro: effect 177 (18d6units) = -2
-- ro: effect 177 (18d6units) = -15
#end

#selectevent 1905
#rarity 0
#req_indepok
#req_rare 30
#req_unique 3
#req_code -73
#4d6units 915
#com 2192
#deathboost 1
#addequip 1
#3d6units 2190
#end

#selectevent 1906
#rarity 0
#req_indepok
#req_rare 30
#req_unique 1
#req_targmnr 2449
#req_code -73
#addequip 9
#end

#selectevent 1907
#rarity 0
#req_rare 3
#req_coast 1
#req_maxdef 19
#req_anycode -73
#req_code 0
#code -74
#end

#selectevent 1908
#rarity 0
#req_rare 2
#req_coast 1
#req_maxdef 19
#req_anycode -85
#req_noench 28
#req_code 0
#code -74
#end

#selectevent 1909
#rarity 0
#req_maxdef 19
#req_maxtroops 20
#req_rare 80
#req_code -74
#code 0
#com 2532
#6d6units 2531
#6d6units 2533
#2com 2190
#4d6units 2190
#end

#selectevent 1910
#rarity 0
#req_coast 1
#req_maxdef 19
#req_rare 4
#req_anycode -73
#req_land 1
#com 2532
#6d6units 2531
#6d6units 2533
#2com 2190
#1d6units 2190
#end

#selectevent 1911
#rarity 0
#req_rare 9
#req_coast 1
#req_maxdef 15
#req_anycode -73
#2com 2119
#com 2532
#gainaff 549755813888
#9d6units 2531
#9d6units 2119
#2com 2533
#end

#selectevent 1912
#rarity 0
#req_rare 10
#req_mindef 20
#req_code -74
#code 0
#incdom 3
#end

#selectevent 1913
#rarity 0
#req_rare 3
#req_coast 1
#req_mindef 20
#req_anycode -73
#incdom 3
#end

#selectevent 1914
#rarity 0
#req_rare 7
#req_freshwater 1
#req_maxdef 10
#req_noseason 3
#req_anycode -73
#com 2532
#12d6units 2531
#6d6units 2533
#2com 2533
#end

#selectevent 1915
#rarity 0
#req_rare 2
#req_coast 1
#req_maxdef 18
#req_anycode -73
#com 2192
#4d6units 2190
#9d6units 2119
#2com 2190
#end

#selectevent 1916
#rarity 0
#req_rare 20
#req_unique 1
#req_targmnr 2449
#req_code -73
#addequip 9
#end

#selectevent 1917
#rarity 13
#req_ench 28
#req_code -14
#resetcode -14
#resetcode -15
#resetcode -72
#end

#selectevent 1918
#rarity 13
#req_ench 28
#req_code -73
#code -85
#revealprov
#end

#selectevent 1919
#rarity 0
#req_ench 42
#req_rare 1
#req_land 1
#req_code 0
#code -49
#notext
#end

#selectevent 1920
#rarity 0
#req_land 1
#req_rare 1
#req_ench 24
#req_forest 1
#req_code 0
#code -26
#notext
#end

#selectevent 1921
#rarity 2
#req_temple 0
#req_land 1
#req_maxdominion 3
#req_freesites 1
#req_unique 2
#req_code 0
#req_story 1
#code 166
#incdom -3
#end

#selectevent 1922
#rarity 0
#req_rare 25
#req_maxdominion 5
#req_code 166
#flagland 1
#code 167
#incdom -2
#end

#selectevent 1923
#rarity 0
#req_dominion 6
#req_code 167
#code 0
#end

#selectevent 1924
#rarity 0
#req_dominion 3
#req_preach 5
#req_code 167
#code 0
#end

#selectevent 1925
#rarity 0
#req_rare 15
#req_maxdominion 5
#req_code 167
#code 168
#incdom -3
#incscale2 0
#end

#selectevent 1926
#rarity 0
#req_rare 5
#req_code 168
#stealthcom 2275
#incdom -3
#end

#selectevent 1927
#rarity 0
#req_code 168
#req_rare 3
#kill 4
#com 308
#end

#selectevent 1928
#rarity 0
#req_monster 2275
#req_code 168
#req_rare 7
#kill 4
#com 308
#end

#selectevent 1929
#rarity 0
#req_monster 2275
#req_code 168
#req_rare 3
#kill 4
#com 2213
#end

#selectevent 1930
#rarity 1
#req_commander 1
#req_code 168
#req_rare 3
#assassin 2212
#end

#selectevent 1931
#rarity 1
#req_temple 0
#req_nearbycode 168
#req_unique 3
#req_maxdominion 6
#req_freesites 1
#req_code 0
#code 166
#incdom -3
#end

#selectevent 1932
#rarity 0
#req_monster 2275
#req_code 168
#req_rare 3
#kill 4
#com 2211
#end

#selectevent 1933
#rarity -2
#req_capital 0
#req_mountain 1
#req_unique 1
#req_magic -1
#req_freesites 2
#req_code 0
#req_story 1
#req_land 1
#req_minunrest 5
#code 151
#decscale2 5
#end

#selectevent 1934
#rarity 0
#req_rare 10
#req_unique 2
#req_commander 1
#req_code 151
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 1935
#rarity 0
#req_rare 10
#req_unique 1
#req_code 151
#com 1037
#3d6units 518
#nation -2
#com 23
#3d6units 19
#end

#selectevent 1936
#rarity 0
#req_rare 5
#req_unique 3
#req_code 151
#2com 1037
#addequip 2
#9d6units 518
#3d6units 1037
#end

#selectevent 1937
#rarity 0
#req_rare 8
#req_code 151
#flagland 1
#code 152
#order 1
#end

#selectevent 1938
#rarity 0
#req_targorder 100
#req_pathearth 2
#req_code 152
#order 0
#flagland 1
#code 153
#end

#selectevent 1939
#rarity 0
#req_rare 30
#req_unique 1
#req_lab 1
#req_code 153
#2com 1037
#9d6units 518
#3d6units 1037
#end

#selectevent 1940
#rarity 0
#req_rare 30
-- ro: requirement 133 (orpathearth) = 2
-- ro: requirement 137 (orpathglamour) = 2
#req_lab 1
#req_code 153
#code 154
#order 240
#end

#selectevent 1941
#rarity 0
#req_targorder 105
#req_code 154
#code 157
#end

#selectevent 1942
#rarity 0
#req_targorder 105
#req_code 157
#code 156
#com 2541
#addequip 1
#2com 1037
#addequip 9
#9d6units 1037
#9d6units 518
#end

#selectevent 1943
#rarity 0
#req_nomonster 2541
#req_code 156
#code 0
-- ro: effect 190 (gold) = 2300
#3d6vis 3
#magicitem 3
#magicitem 3
#addsite -1
#end

#selectevent 1944
#rarity 0
#req_targorder 106
#req_commander 1
#req_unique 1
#req_code 154
#code 157
#end

#selectevent 1945
#rarity 0
#req_targorder 107
#req_commander 1
#req_unique 1
#req_code 154
#code 157
#end

#selectevent 1946
#rarity 0
#req_targorder 107
#req_pathearth 2
#req_rare 80
#req_code 157
#req_unique 2
-- ro: effect 190 (gold) = 250
#end

#selectevent 1947
#rarity 0
#req_targorder 107
#req_pathearth 3
#req_rare 30
#req_unique 1
#req_code 157
#end

#selectevent 1948
#rarity 0
#req_targorder 107
#req_pathglamour 2
#req_rare 80
#req_code 157
#req_unique 2
-- ro: effect 190 (gold) = 250
#end

#selectevent 1949
#rarity 0
#req_targorder 107
#req_pathglamour 3
#req_rare 30
#req_unique 2
#req_code 157
-- ro: effect 190 (gold) = 300
#end

#selectevent 1950
#rarity 0
#req_targorder 107
#req_targpath2 7
#req_rare 20
#req_code 157
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 1951
#rarity 0
#req_targorder 107
#req_targpath2 3
#req_rare 20
#req_code 157
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 1952
#rarity 0
#req_targorder 104
#req_code 157
-- ro: effect 190 (gold) = 500
#code 0
#addsite -1
#end

#selectevent 1953
#rarity 0
#req_pathglamour 2
#req_code 152
#code 153
#end

#selectevent 1954
#rarity 0
#req_targorder 106
#req_code 157
#req_unique 1
#end

#selectevent 1955
#rarity 0
#req_targorder 104
#req_code 154
-- ro: effect 190 (gold) = 300
#code 0
#end

#selectevent 1956
#rarity 0
#req_targorder 107
#req_rare 30
#req_pathearth 1
#req_code 157
#code 156
#com 2541
#addequip 1
#9d6units 1037
#9d6units 518
#end

#selectevent 1957
#rarity 0
#req_targorder 107
#req_rare 30
#req_pathglamour 1
#req_code 157
#code 156
#com 2541
#addequip 1
#9d6units 1037
#9d6units 518
#end

#selectevent 1958
#rarity 0
#req_rare 10
#req_unique 2
#req_commander 1
#req_code 152
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 1959
#rarity 0
#req_rare 10
#req_unique 2
#req_code 152
#nation -2
#com -13
#addequip 2
#end

#selectevent 1960
#rarity 0
#req_rare 10
#req_unique 1
#req_code 152
#nation -2
#com -13
#addequip 2
#com -13
#com -13
#end

#selectevent 1961
#rarity 0
#req_rare 10
#req_unique 2
#req_code 153
#nation -2
#com -13
#addequip 2
#end

#selectevent 1962
#rarity 0
#req_code 153
-- ro: requirement 133 (orpathearth) = 2
-- ro: requirement 137 (orpathglamour) = 2
-- ro: requirement 140 (default) = 1
#end

#selectevent 1963
#rarity 2
#req_forest 1
#req_code 0
#req_capital 0
#code -12
#end

#selectevent 1964
#rarity 0
#req_rare 70
#req_code -12
#code 0
#2com 2219
#9d6units 2219
#end

#selectevent 1965
#rarity 2
#req_mountain 1
#req_code 0
#req_capital 0
#code -13
#end

#selectevent 1966
#rarity 0
#req_rare 70
#req_code -13
#code 0
#com 1037
#9d6units 518
#end

#selectevent 1967
#rarity -2
#req_mountain 1
#req_rare 3
#unrest 5
#kill 1
#addsite -1
#end

#selectevent 1968
#rarity -1
#req_site 1
#decscale 5
#end

#selectevent 1969
#rarity 2
#req_site 1
#unrest 5
#kill 1
#end

#selectevent 1970
#rarity 0
#req_unique 1
#req_foundsite 1
#req_rare 3
#req_turn 20
#req_code 0
#code 50
#unrest 5
#kill 1
#end

#selectevent 1971
#rarity 0
#req_rare 25
#req_code 50
#code 51
#com 3637
#6d6units 297
#end

#selectevent 1972
#rarity 0
#req_foundsite 1
#req_unique 1
#req_rare 5
#nation -2
#com 92
#addequip 1
#end

#selectevent 1973
#rarity -1
#req_foundsite 1
#1d3vis 1
#end

#selectevent 1974
#rarity 0
#req_rare 25
#req_code 51
#code 52
#com 3637
#3d6units 297
#3d6units 298
#3d6units 695
#end

#selectevent 1975
#rarity 0
#req_rare 25
#req_code 53
#code 0
#com 93
#addequip 1
#7d6units 3865
#end

#selectevent 1976
#rarity 0
#req_rare 25
#req_code 52
#code 53
#magicitem 2
#1d6vis 1
#end

#selectevent 1977
#rarity 0
#req_rare 15
#req_unique 2
#req_commander 1
#req_code 53
-- ro: effect 39 = 2
#assassin 3722
#end

#selectevent 1978
#rarity 0
#req_rare 15
#req_unique 1
#req_code 53
#2com 93
#addequip 1
#addequip 9
#4d6units 3724
#end

#selectevent 1979
#rarity 0
#req_rare 25
#req_freesites 1
#req_code 53
#code 161
#com 341
#addequip 1
#9d6units 297
#4d6units 298
#end

#selectevent 1980
#rarity 0
#req_nomonster 341
#req_code 161
#req_unique 1
#code 0
#addsite -1
#end

#selectevent 1981
#rarity 2
#req_turn 15
#req_noseason 3
#req_code 0
#code -16
#kill 2
#unrest 15
#end

#selectevent 1982
#rarity 0
#req_rare 25
#req_code 0
#req_nearbycode -16
#code -17
#taxboost -25
#unrest 15
#end

#selectevent 1983
#rarity 0
#req_rare 50
#req_code -17
#code -16
#kill 3
#unrest 15
#end

#selectevent 1984
#rarity 0
#req_rare 50
#req_code -17
#req_mydominion 1
#code 0
#taxboost 100
#incdom 3
#unrest -15
#end

#selectevent 1985
#rarity 0
#req_rare 25
#req_code -16
#code 0
#unrest -5
#end

#selectevent 1986
#rarity 0
#req_rare 20
#req_code -16
#code 0
#kill 2
#incdom 1
#unrest 5
#end

#selectevent 1987
#rarity 0
#req_rare 25
#req_pathholy 1
#req_code -16
#taxboost 20
#kill 2
#incdom 1
#unrest 5
#end

#selectevent 1988
#rarity 0
#req_season 3
#req_code -16
#code 0
#end

#selectevent 1989
#rarity 0
#req_season 3
#req_code -17
#code 0
#end

#selectevent 1990
#rarity 0
#req_mintroops 20
#req_rare 80
#req_code -17
#code 0
#taxboost -80
#unrest -10
#end

#selectevent 1991
#rarity 0
#req_rare 5
#req_temple 0
#req_maxdominion 5
#req_code -16
#flagland 1
#code -43
#incdom -2
#incscale2 3
#disease 10
#end

#selectevent 1992
#rarity 0
#req_rare 75
#req_temple 1
#req_code -43
#code -16
#incdom 2
#end

#selectevent 1993
#rarity 0
#req_rare 10
#req_code -43
#code 0
#end

#selectevent 1994
#rarity 0
#req_rare 25
#req_pathholy 1
#req_code -43
#taxboost 20
#kill 1
#incdom 1
#unrest 5
#end

#selectevent 1995
#rarity 0
#req_rare 50
#req_dominion 7
#req_code -43
#code 0
#kill 1
#end

#selectevent 1996
#rarity 0
#req_rare 30
#req_code -43
#code -41
#incdom -1
#incscale2 3
#disease 10
#end

#selectevent 1997
#rarity 0
#req_rare 80
#req_pathholy 3
#req_nomonster 2535
#req_code -41
#code 0
#nation -2
#com 2535
#4d6units 2536
#end

#selectevent 1998
#rarity 0
#req_preach 5
#req_nomonster 2535
#req_code -41
#code 0
#nation -2
#com 2535
#4d6units 2536
#end

#selectevent 1999
#rarity 0
#req_rare 25
#req_code -41
#code -42
#stealthcom 2535
#addequip 1
#4d6units 2536
#end

#selectevent 2000
#rarity 0
#req_rare 25
#req_monster 2535
#req_code -42
#disease 10
#kill 2
#incdom -1
#unrest 5
#end

#selectevent 2001
#rarity 0
#req_nomonster 2535
#req_code -42
#code 0
#end

#selectevent 2002
#rarity 0
#req_rare 50
#req_code -43
#incscale 3
#kill 1
#incdom -2
#end

#selectevent 2003
#rarity 0
#req_rare 50
#req_code -41
#incscale 3
#kill 1
#incdom -2
#end

#selectevent 2004
#rarity 0
#req_mindef 16
#req_code -17
#code 0
#taxboost -80
#unrest -10
#end

#selectevent 2005
#rarity -1
#req_story 1
#req_land 1
#req_death -1
#req_unique 2
#req_site 1
#req_code 0
#flagland 1
#code 169
#unrest 15
#incscale2 3
#end

#selectevent 2006
#rarity 0
#req_rare 15
#req_pathdeath 1
#req_code 169
#code 0
#4com 197
#6d6units 197
#3d6units 194
#end

#selectevent 2007
#rarity 0
#req_rare 13
#req_code 169
#4com 197
#6d6units 197
#3d6units 195
#end

#selectevent 2008
#rarity 0
#req_pathdeath 2
#req_unique 2
#req_rare 50
#req_code 169
#nation -2
#6d6units 197
#9d6units 195
#end

#selectevent 2009
#rarity 0
#req_pathdeath 2
#req_unique 2
#req_rare 50
#req_code 169
#nation -2
#9d6units 197
#3d6units 196
#end

#selectevent 2010
#rarity 0
#req_pathholy 3
#req_rare 70
#req_code 169
#code 0
#end

#selectevent 2011
#rarity 0
#req_preach 15
#req_rare 70
#req_code 169
#code 0
#end

#selectevent 2012
#rarity 0
#req_pathholy 1
#req_rare 20
#req_code 169
#4com 197
#15d6units 197
#15d6units 675
#com 329
#addequip 1
#end

#selectevent 2013
#rarity 0
#req_pathholy 2
#req_rare 30
#req_code 169
#code 0
#end

#selectevent 2014
#rarity -2
#req_death -1
#req_unique 2
#req_rare 20
#req_code 0
#code 169
#unrest 15
#incscale2 3
#end

#selectevent 2015
#rarity 2
#req_forest 1
#req_maxdominion 4
#req_freesites 1
#req_unique 3
#req_code 0
#req_story 1
#code 172
#incdom -3
#decscale2 3
#end

#selectevent 2016
#rarity 0
#req_rare 25
#req_maxdominion 5
#req_code 172
#flagland 1
#code 173
#incdom -2
#end

#selectevent 2017
#rarity 0
#req_dominion 6
#req_code 173
#code 0
#incdom 2
#end

#selectevent 2018
#rarity 0
#req_preach 25
#req_dominion 2
#req_code 173
#code 0
#end

#selectevent 2019
#rarity 0
#req_rare 15
#req_maxdominion 5
#req_code 173
#code 174
#incdom -3
#decscale2 3
#end

#selectevent 2020
#rarity 0
#req_rare 10
#req_code 174
#req_code 177
#stealthcom 2487
#addequip 1
#6d6units 227
#6d6units 2276
#incdom -3
#code 177
#end

#selectevent 2021
#rarity 0
#req_rare 3
#req_noseason 3
#req_code 174
#req_code 177
#kill 4
#2com 1910
#2com 237
#9d6units 227
#6d6units 361
#end

#selectevent 2022
#rarity 0
#req_monster 2487
#req_rare 7
#req_noseason 3
#req_code 174
#req_code 177
#kill 4
#com 2487
#addequip 1
#6d6units 227
#6d6units 2276
#end

#selectevent 2023
#rarity 0
#req_monster 2487
#req_rare 3
#req_noseason 3
#req_code 174
#req_code 177
#kill 4
#com 931
#addequip 1
#addequip 9
#12d6units 361
#end

#selectevent 2024
#rarity 1
#req_commander 1
#req_rare 3
#req_noseason 3
#req_code 174
#req_code 177
-- ro: effect 39 = 2
#assassin 362
#end

#selectevent 2025
#rarity 1
#req_unique 3
#req_maxdominion 6
#req_nearbycode 174
#req_code 177
#req_code 0
#code 172
#incdom -3
#end

#selectevent 2026
#rarity 0
#req_rare 40
#req_preach 25
#req_dominion 2
#req_growth 3
#req_code 173
#code 0
#nation -2
#com 2487
#4d6units 227
#incdom 4
#end

#selectevent 2027
#rarity 0
#req_dominion 6
#req_code 172
#code 0
#end

#selectevent 2028
#rarity 0
#req_rare 20
#req_dominion 6
#req_temple 1
#req_nomonster 2487
#req_code 174
#code 0
#incdom 2
#end

#selectevent 2029
#rarity 0
#req_targorder 3
#req_rare 50
#req_mintroops 15
#req_commander 1
#req_code 173
#code 0
#kill 1
#incdom 2
#incscale2 3
#end

#selectevent 2030
#rarity 0
#req_targorder 3
#req_rare 50
#req_mintroops 40
#req_commander 1
#req_nomonster 2487
#req_code 174
#code 0
#kill 1
#incdom 2
#incscale2 3
#end

#selectevent 2031
#rarity 0
#req_rare 30
#req_dominion 8
#req_nomonster 2487
#req_code 174
#code 0
#end

#selectevent 2032
#rarity 0
#req_targorder 3
#req_rare 50
#req_mintroops 20
#req_commander 1
#req_code 173
#code 0
#kill 1
#incdom 2
#incscale2 3
#landprod -4
#end

#selectevent 2033
#rarity 0
#req_targorder 3
#req_rare 50
#req_mintroops 40
#req_commander 1
#req_code 174
#code 0
#kill 1
#incdom 2
#incscale2 3
#landprod -4
#end

#selectevent 2034
#rarity 0
#req_nomonster 2487
#req_code 177
#code 174
#incscale2 3
#end

#selectevent 2035
#rarity -2
#req_forest 1
#req_unique 3
#req_growth 0
#req_code 0
#flagland 1
#code 175
#stealthcom 237
#decscale 5
#end

#selectevent 2036
#rarity 0
#req_rare 15
#req_pathnature 1
#req_code 175
#code 0
#curse 15
#incscale2 4
#end

#selectevent 2037
#rarity 0
#req_rare 50
#req_pathnature 1
#req_code 175
#code 0
#curse 15
#pathboost 6
#end

#selectevent 2038
#rarity 0
#req_nomonster 237
#req_code 175
#code 0
#incscale 5
#incscale 3
#end

#selectevent 2039
#rarity 1
#req_noseason 3
#req_code 174
#4com 27
#9d6units 27
#9d6units 228
#end

#selectevent 2040
#rarity 13
#req_dominion 2
#req_order 1
#req_claimedthrone
#req_site 1
#req_unique 1
#worlddecscale2 0
#worldunrest -10
#end

#selectevent 2041
#rarity 13
#req_site 1
#req_claimedthrone
#req_story 1
#req_unique 1
#req_code 0
#code -20
#worldunrest 4
#delay25 3
#end

#selectevent 2042
#rarity 13
#req_indepok
#resetcode -20
#code -33
#resetcode -34
#resetcode -1
#purgecalendar 0
#purgedelayed 0
#end

#selectevent 2043
#rarity 1
#req_anycode -20
#unrest 15
#end

#selectevent 2044
#rarity 1
#req_noera 1
#req_land 1
#req_anycode -20
#com 23
#6d6units 22
#com 23
#4d6units 18
#end

#selectevent 2045
#rarity 0
#req_rare 4
#req_unique 1
#req_land 1
#req_anycode -20
#com 875
#addequip 1
#12d6units 40
#9d6units 33
#9d6units 29
#end

#selectevent 2046
#rarity 1
#req_land 1
#req_anycode -20
#com 45
#2com 35
#6d6units 40
#9d6units 33
#9d6units 29
#end

#selectevent 2047
#rarity 1
#req_poptype 30
#req_mindef 4
#req_maxtroops 20
#req_anycode -20
#4com 23
#12d6units 22
#nation -2
#4com 23
#9d6units 22
#end

#selectevent 2048
#rarity 1
#req_chaos 1
#req_minpop 100
#req_capital 0
#req_anycode -20
#unrest 20
#kill 5
#end

#selectevent 2049
#rarity 1
#req_chaos 2
#req_minpop 200
#req_land 1
#req_anycode -20
#unrest 25
#kill 10
#emigration 10
#end

#selectevent 2050
#rarity 0
#req_unique 2
#req_rare 4
#req_poptype 25
#req_freesites 1
#req_minpop 10
#req_anycode -20
#addsite -1
#unrest 20
#defence 10
#end

#selectevent 2051
#rarity -1
#req_order 0
#req_anycode -20
#defence 10
#end

#selectevent 2052
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -20
#nation -2
#com 35
#6d6units 40
#9d6units 33
-- ro: effect 190 (gold) = -100
#end

#selectevent 2053
#rarity 1
#req_land 1
#req_turn 5
#req_code 0
#req_anycode -20
#code 90
#end

#selectevent 2054
#rarity 0
#req_rare 1
#req_land 0
#req_anycode -20
#nation -2
#com 406
#6d6units 174
-- ro: effect 190 (gold) = -100
#end

#selectevent 2055
#rarity 1
#req_land 0
#req_turn 5
#req_code 0
#req_anycode -20
#code 91
#end

#selectevent 2056
#rarity 0
#req_monster 1340
#req_claimedthrone
#req_unique 1
#req_site 1
#fireboost 1340
#airboost 1340
#end

#selectevent 2057
#rarity 1
#req_land 1
#req_code 0
#req_turn 8
#req_era 2
#req_anycode -20
#code 92
#end

#selectevent 2058
#rarity 1
#req_rare 50
#req_anycode -20
#req_land 1
#nation 4
#com 141
-- ro: effect 183 (24d6units) = 140
#com 147
#6d6units 139
#end

#selectevent 2059
#rarity 1
#req_land 1
#req_unluck 1
#req_turn 10
#req_code 0
#req_anycode -20
#code 93
#end

#selectevent 2060
#rarity 1
#req_forest 1
#req_mindef 4
#req_code 0
#req_death 0
#req_anycode -20
#code 94
#end

#selectevent 2061
#rarity 0
#req_rare 1
#req_land 0
#req_anycode -20
#com 406
#9d6units 174
#2com 406
#6d6units 175
#end

#selectevent 2062
#rarity -1
#req_poptype 30
#req_mindef 5
#req_maxtroops 20
#req_anycode -20
#4com 23
#12d6units 22
#nation -2
#4com 23
#9d6units 22
#end

#selectevent 2063
#rarity 1
#req_land 0
#req_turn 5
#req_anycode -20
#com 406
#9d6units 174
#com 406
#4d6units 175
#end

#selectevent 2064
#rarity -1
#req_poptype 25
#req_mindef 5
#req_maxdef 20
#req_anycode -20
#2com 141
-- ro: effect 177 (18d6units) = 139
#nation -2
#2com 141
-- ro: effect 177 (18d6units) = 140
#end

#selectevent 2065
#rarity 0
#req_rare 1
#req_turn 8
#req_land 1
#req_code 0
#req_anycode -20
#code -33
#incscale2 4
#unrest 3
#end

#selectevent 2066
#rarity 0
#req_rare 2
#req_turn 8
#req_land 1
#req_code 0
#req_anycode -20
#code -33
#defence 8
#end

#selectevent 2067
#rarity 0
#req_rare 2
#req_turn 8
#req_land 0
#req_code 0
#req_anycode -20
#code -34
#incscale2 4
#unrest 3
#end

#selectevent 2068
#rarity 0
#req_rare 3
#req_turn 8
#req_land 0
#req_code 0
#req_anycode -20
#code -34
#defence 8
#end

#selectevent 2069
#rarity 11
#req_land 1
#req_turn 10
#req_code 0
#code2 -35
#code -19
#worldincscale 4
#worldincscale 3
#delay25 3
#end

#selectevent 2070
#rarity 0
#req_anycode -35
#resetcode -35
#resetcode -19
#unrest -10
#decscale 0
#end

#selectevent 2071
#rarity 0
#req_rare 1
#req_turn 8
#req_land 1
#req_code 0
#req_anycode -35
#code -33
#incscale2 4
#unrest 3
#end

#selectevent 2072
#rarity 0
#req_rare 1
#req_turn 8
#req_land 1
#req_code 0
#req_anycode -35
#code -33
#defence 8
#end

#selectevent 2073
#rarity 0
#req_rare 2
#req_turn 8
#req_land 0
#req_code 0
#req_anycode -35
#code -34
#incscale2 4
#unrest 3
#end

#selectevent 2074
#rarity 0
#req_rare 2
#req_turn 8
#req_land 0
#req_code 0
#req_anycode -35
#code -34
#defence 8
#end

#selectevent 2075
#rarity 0
#req_rare 1
#req_land 1
#req_minpop 20
#req_code 0
#req_anycode -35
#code -36
#disease 1
#kill 1
#end

#selectevent 2076
#rarity 11
#req_story 1
#req_unique 1
#req_code 0
#code -19
#code2 -40
#worlddecscale3 3
#worldincdom -2
#delay25 5
#end

#selectevent 2077
#rarity 0
#req_indepok
#resetcode -40
#kill 2
#disease 2
#end

#selectevent 2078
#rarity 0
#req_rare 4
#req_growth -1
#req_land 1
#req_code 0
#req_anycode -40
#code -26
#decscale2 3
#landgold 3
#incdom -2
#end

#selectevent 2079
#rarity 0
#req_rare 2
#req_forest 1
#req_anycode -40
#decscale 3
#2com 237
#addequip 1
#9d6units 227
#3d6units 361
#end

#selectevent 2080
#rarity 0
#req_rare 3
#req_forest 1
#req_season 2
#req_anycode -40
#2com 1910
#end

#selectevent 2081
#rarity 0
#req_rare 4
#req_forest 1
#req_anycode -40
#com 931
#addequip 1
#1d6units 227
#9d6units 361
#3d6units 362
#end

#selectevent 2082
#rarity 1
#req_rare 1
#req_forest 1
#req_anycode -40
#4com 27
#3d6units 27
#6d6units 228
#end

#selectevent 2083
#rarity 1
#req_commander 1
#req_forest 1
#req_anycode -40
-- ro: effect 39 = 2
#assassin 362
#end

#selectevent 2084
#rarity 0
#req_rare 2
#req_mountain 1
#req_anycode -40
#2com 561
#1d3units 3741
#end

#selectevent 2085
#rarity 0
#req_rare 1
#req_mindef 1
#req_maxtroops 8
#req_forest 1
#req_anycode -40
#4com 284
#15d6units 284
#nation -2
#com 1565
#6d6units 1565
#end

#selectevent 2086
#rarity 0
#req_rare 4
#req_farm 1
#req_season 0
#req_growth -2
#req_anycode -40
#2d4vis 6
#end

#selectevent 2087
#rarity 0
#req_forest 1
#req_rare 4
#req_freesites 1
#req_code 0
#req_anycode -40
#code 172
#incdom -3
#decscale2 3
#end

#selectevent 2088
#rarity 0
#req_forest 1
#req_rare 4
#req_noseason 3
#req_anycode -40
#landgold -8
#landprod -5
#end

#selectevent 2089
#rarity 0
#req_rare 2
#req_noseason 3
#req_forest 1
#req_code 0
#req_anycode -40
#code -27
#emigration 3
#landgold -3
#end

#selectevent 2090
#rarity 0
#req_rare 3
#req_land 0
#req_noseason 3
#req_anycode -40
#landgold 2
#taxboost 100
#decscale2 3
#end

#selectevent 2091
#rarity -1
#req_land 0
#req_noseason 3
#req_anycode -40
#com 438
#end

#selectevent 2092
#rarity 0
#req_rare 2
#req_growth -1
#req_land 0
#req_anycode -40
#decscale2 3
#landgold 3
#incdom -2
#end

#selectevent 2093
#rarity 13
#req_indepok
#req_site 1
#req_claimedthrone
#req_unique 1
#req_story 1
#req_code 0
#req_land 1
#code -21
#delay25 4
#end

#selectevent 2094
#rarity 13
#req_indepok
#req_anycode -21
#resetcode -95
#resetcode -21
#end

#selectevent 2095
#rarity 0
#req_site 1
#req_claimedthrone
#req_rare 10
#req_unique 2
#req_land 1
#req_code -21
#nation -2
#1d6units 1769
#3d6units 1560
#3d6units 1560
#end

#selectevent 2096
#rarity 0
#req_rare 2
#req_land 1
#req_anycode -21
#2com 633
#3d6units 633
#end

#selectevent 2097
#rarity 0
#req_rare 2
#req_land 1
#req_anycode -21
#stealthcom 633
#1d6units 633
#unrest 5
#end

#selectevent 2098
#rarity 0
#req_rare 3
#req_land 1
#req_anycode -21
#4com 1769
#3d6units 1560
#3d6units 1560
#end

#selectevent 2099
#rarity 0
#req_rare 3
#req_land 1
#req_commander 1
#req_anycode -21
-- ro: effect 39 = 2
#assassin 1560
#end

#selectevent 2100
#rarity 0
#req_rare 10
#req_code -21
#nation -2
#3d6units 1224
#end

#selectevent 2101
#rarity 0
#req_rare 1
#req_mindef 1
#req_maxtroops 8
#req_forest 1
#req_anycode -21
#4com 284
#15d6units 284
#nation -2
#com 1565
#6d6units 1565
#end

#selectevent 2102
#rarity 0
#req_claimedthrone
#req_unique 1
#req_code 0
#req_site 1
#code -22
#nation -2
#com 104
#holyboost 104
#delay25 2
#end

#selectevent 2103
#rarity 0
#req_code -22
#req_unique 1
#code 0
#end

#selectevent 2104
#rarity 0
#req_rare 15
#req_coast 1
#req_heat 0
#req_anycode -22
#req_unique 10
#kill 3
#unrest 8
#end

#selectevent 2105
#rarity 1
#req_site 1
#req_claimedthrone
#req_unique 1
#req_turn 30
#nation -2
#com 438
#end

#selectevent 2106
#rarity 0
#req_unique 1
#req_site 1
#req_claimedthrone
#req_land 1
#req_rare 15
#req_code -24
#code 0
#nation -3
#com 310
#addequip 1
#end

#selectevent 2107
#rarity 1
#req_site 1
#req_claimedthrone
#req_monster 310
#nation -3
#2com 190
#6d6units 191
#3d6units 192
#3d6units 189
#end

#selectevent 2108
#rarity 13
#req_indepok
#req_site 1
#req_claimedthrone
#req_unique 1
#req_story 1
#req_code 0
#code -23
#delay25 5
#end

#selectevent 2109
#rarity 0
#req_code -23
#req_indepok
#code 0
#nation -3
#2com 190
-- ro: effect 183 (24d6units) = 194
#com 190
-- ro: effect 183 (24d6units) = 197
#1unit 405
#com 398
#9d6units -2
#end

#selectevent 2110
#rarity 0
#req_rare 2
#req_site 0
#req_anycode -23
#2com 190
#6d6units 194
#9d6units 197
#end

#selectevent 2111
#rarity 0
#req_rare 2
#req_site 0
#req_anycode -23
#2com 190
#6d6units 191
#3d6units 192
#6d6units 197
#end

#selectevent 2112
#rarity 0
#req_site 1
#req_claimedthrone
#req_unique 1
#req_code 0
#code -24
#2d6vis 5
#end

#selectevent 2113
#rarity 0
#req_rare 20
#req_code -24
#req_unique 1
#code 0
#magicitem 9
#end

#selectevent 2114
#rarity 13
#req_unique 1
#req_claimedthrone
#req_land 1
#req_dominion 2
#req_code 0
#delay50 4
#code -25
#worlddecscale 3
#end

#selectevent 2115
#rarity 13
#req_indepok
#resetcode -25
#end

#selectevent 2116
#rarity 0
#req_site 1
#req_claimedthrone
#req_death 0
#req_rare 20
#req_code -25
#code 0
#com 2487
#addequip 1
#9d6units 227
#12d6units 2276
#end

#selectevent 2117
#rarity 0
#req_forest 1
#req_rare 1
#req_anycode -25
#decscale 3
#2com 237
#addequip 1
#9d6units 227
#3d6units 361
#end

#selectevent 2118
#rarity 0
#req_forest 1
#req_rare 2
#req_season 2
#req_anycode -25
#2com 1910
#end

#selectevent 2119
#rarity 0
#req_forest 1
#req_rare 3
#req_anycode -25
#com 931
#addequip 1
#1d6units 227
#9d6units 361
#3d6units 362
#end

#selectevent 2120
#rarity 1
#req_forest 1
#req_anycode -25
#4com 27
#3d6units 27
#6d6units 228
#end

#selectevent 2121
#rarity 1
#req_commander 1
#req_forest 1
#req_anycode -25
-- ro: effect 39 = 2
#assassin 362
#end

#selectevent 2122
#rarity 0
#req_mountain 1
#req_rare 5
#req_anycode -25
#2com 561
#1d3units 3741
#end

#selectevent 2123
#rarity -1
#req_site 1
#req_unique 1
#req_pathnature 1
#req_claimedthrone
#pathboost 6
#end

#selectevent 2124
#rarity -1
#req_site 1
#req_unique 1
#req_pathnature 1
#req_claimedthrone
#nation -2
#com 552
#2d6vis 6
#end

#selectevent 2125
#rarity 0
#req_forest 1
#req_rare 3
#req_freesites 1
#req_code 0
#req_anycode -25
#code 172
#incdom -3
#decscale2 3
#end

#selectevent 2126
#rarity 0
#req_forest 1
#req_rare 2
#req_noseason 3
#req_anycode -25
#landgold -8
#landprod -5
#end

#selectevent 2127
#rarity 0
#req_rare 3
#req_code 0
#req_anycode -25
#code -26
#decscale2 3
#landgold 2
#end

#selectevent 2128
#rarity -2
#req_land 1
#req_growth -1
#req_code 0
#code -26
#decscale2 3
#landgold 3
#end

#selectevent 2129
#rarity 0
#req_rare 4
#req_growth -1
#req_season 0
#req_code -26
#taxboost 400
#landgold 5
#decscale2 3
#end

#selectevent 2130
#rarity 0
#req_rare 1
#req_land 1
#req_season 2
#req_nomonster 476
#req_code -26
#code 0
#nation -2
#com 476
#end

#selectevent 2131
#rarity 0
#req_rare 4
#req_forest 1
#req_noseason 3
#req_code -26
#code -27
#landgold -8
#landprod -5
#end

#selectevent 2132
#rarity 0
#req_rare 4
#req_dominion 3
#req_pathholy 1
#req_growth 1
#req_code -26
-- ro: effect 190 (gold) = 200
#incdom 1
#end

#selectevent 2133
#rarity 0
#req_rare 2
#req_maxdominion 4
#req_freesites 1
#req_forest 1
#req_code -26
#code 172
#incdom -3
#decscale2 3
#end

#selectevent 2134
#rarity 0
#req_rare 2
#req_growth -1
#req_turn 8
#req_forest 1
#req_code -26
#code 0
#com 105
#7d6units 361
#1d6units 362
#kill 10
#end

#selectevent 2135
#rarity 0
#req_rare 2
#req_magic 1
#req_minpop 20
#req_code -26
#code 0
#incscale 0
#unrest 10
#end

#selectevent 2136
#rarity 0
#req_rare 2
#req_land 1
#req_season 1
#req_code -26
#code 0
#unrest 20
-- ro: effect 190 (gold) = -100
#end

#selectevent 2137
#rarity 0
#req_rare 10
#req_land 0
#req_season 1
#req_growth -1
#req_code -26
#taxboost 400
#unrest -10
#end

#selectevent 2138
#rarity 0
#req_rare 10
#req_farm 1
#req_season 1
#req_growth -1
#req_code -26
#code 0
#taxboost 400
#unrest -10
#end

#selectevent 2139
#rarity 0
#req_rare 1
#req_land 1
#req_season 2
#req_nomonster 476
#req_code -26
#code 0
#kill 1
#com 476
#end

#selectevent 2140
#rarity 0
#req_rare 5
#req_forest 1
#req_nation 52
#req_turn 8
#req_code -26
#nation 52
#newdom 5
#com 237
#8d6units 435
#com 237
#8d6units 435
#extramsg 52
#end

#selectevent 2141
#rarity 0
#req_rare 3
#req_growth 1
#req_land 1
#req_code -26
#code 0
#id 11
#unrest 15
#stealthcom 1910
#end

#selectevent 2142
#rarity 0
#req_rare 2
#req_magic 1
#req_land 1
#req_season 3
#req_code -26
#code 0
#2d6vis 6
#end

#selectevent 2143
#rarity 0
#req_rare 5
#req_monster 105
#req_forest 1
#req_growth 1
#req_code -26
#code 0
#natureboost 105
#end

#selectevent 2144
#rarity 0
#req_rare 3
#req_land 0
#req_code -26
#code 0
#1d6vis 51
#end

#selectevent 2145
#rarity 0
#req_rare 3
#req_season 2
#req_farm 1
#req_code -26
#code 0
#2d4vis 51
#1d3vis 4
#1d3vis 6
#1d3vis 5
#end

#selectevent 2146
#rarity 0
#req_rare 3
#req_season 0
#req_code -26
#decscale2 2
#decscale2 3
#taxboost 200
#landgold 3
#end

#selectevent 2147
#rarity 0
#req_rare 3
#req_forest 1
#req_growth -1
#req_code -26
#code 175
#stealthcom 237
#decscale 5
#end

#selectevent 2148
#rarity 0
#req_rare 1
#req_luck -1
#req_code -26
#code 0
#nation -2
#com 561
#end

#selectevent 2149
#rarity 0
#req_rare 1
#req_unluck 1
#req_code -26
#code 0
#com 561
#end

#selectevent 2150
#rarity -1
#req_land 1
#req_freesites 1
#req_luck 0
#req_forest 1
#req_code -26
#code 0
#addsite -1
#end

#selectevent 2151
#rarity -1
#req_land 0
#req_freesites 1
#req_luck 0
#req_forest 1
#req_code -26
#code 0
#addsite -1
#end

#selectevent 2152
#rarity -1
#req_site 1
#req_growth -1
#req_code 0
#code -26
#end

#selectevent 2153
#rarity -1
#req_nation 52
#req_growth -1
#req_mydominion 2  -- outside the command's range 0..1
#req_code 0
#code -26
#decscale2 3
#landgold 2
#end

#selectevent 2154
#rarity 1
#req_site 1
#req_noseason 3
#req_forest 1
#req_code 0
#code -27
#emigration 3
#landgold -3
#end

#selectevent 2155
#rarity 1
#req_noseason 3
#req_forest 1
#req_code 0
#code -27
#emigration 3
#landgold -3
#end

#selectevent 2156
#rarity 0
#req_rare 20
#req_noseason 3
#req_code -27
#code -28
#landgold -8
#landprod -5
#end

#selectevent 2157
#rarity 0
#req_rare 20
#req_unique 2
#req_season 2
#req_code -28
#com 476
#end

#selectevent 2158
#rarity 0
#req_rare 20
#req_growth -1
#req_turn 8
#req_forest 1
#req_code -28
#code 0
#com 105
#7d6units 361
#1d6units 362
#kill 3
#end

#selectevent 2159
#rarity 0
#req_rare 15
#req_code -27
#code -29
#decscale 5
#end

#selectevent 2160
#rarity 0
#req_rare 80
#req_pathnature 1
#req_growth 0
#req_code -29
#flagland 0
#code 0
#pathboost 6
#nation -2
#com 105
#end

#selectevent 2161
#rarity 0
#req_rare 20
#req_code -29
#code -28
#landgold -5
#end

#selectevent 2162
#rarity 0
#req_rare 20
#req_growth -1
#req_turn 8
#req_forest 1
#req_code -29
#code 0
#com 105
#7d6units 361
#1d6units 362
#kill 3
#end

#selectevent 2163
#rarity 0
#req_rare 20
#req_pathnature 1
#req_season 2
#req_unique 1
#req_code -28
#nation -2
#com 476
#end

#selectevent 2164
#rarity 0
#req_rare 20
#req_mindef 1
#req_code -28
#code -30
#com 330
#3d6units -12
#nation -2
#com 2327
#4com -13
#addequip 2
#end

#selectevent 2165
#rarity 0
#req_monster 2327
#req_code -30
#code -31
#flagland 1
#end

#selectevent 2166
#rarity 0
#req_mintroops 10
#req_monster 2327
#req_code -31
#code -32
#4com 122
#addequip 1
#4com 330
#12d6units -12
#end

#selectevent 2167
#rarity 0
#req_nomonster 122
#req_code -32
#code 0
#nation -2
#com 2332
#2d4vis 6
#magicitem 2
#end

#selectevent 2168
#rarity -1
#req_pathnature 1
#req_forest 1
#req_growth 1
#req_code -26
#code 0
#pathboost 6
#end

#selectevent 2169
#rarity 0
#req_rare 5
#req_monster 273
#req_code -26
#nation -2
#3d6units 273
#5d6units 483
#2d6units 271
#end

#selectevent 2170
#rarity 0
#req_unique 3
#req_rare 20
#req_dominion 2
#req_growth 0
#req_anycode -25
#req_foundsite 1
#pathboost 9
#incdom 3
#end

#selectevent 2171
#rarity 0
#req_foundsite 1
#req_unique 2
#req_rare 30
#req_dominion 2
#req_growth 0
#req_code -26
#pathboost 9
#incdom 3
#end

#selectevent 2172
#rarity -1
#req_season 3
#req_forest 1
#req_freesites 1
#req_code -26
#code 0
#addsite -1
#end

#selectevent 2173
#rarity -1
#req_rare 25
#req_forest 1
#req_freesites 1
#req_code -26
#code 0
#addsite -1
#incdom -4
#end

#selectevent 2174
#rarity 0
#req_rare 4
#req_farm 1
#req_noseason 3
#req_code -26
#code 0
#2d6vis 6
#end

#selectevent 2175
#rarity 0
#req_rare 4
#req_farm 1
#req_growth 0
#req_season 0
#req_code -26
#code 0
#2d6vis 3
#end

#selectevent 2176
#rarity -1
#req_mountain 1
#req_freesites 1
#req_code -26
#addsite -1
#end

#selectevent 2177
#rarity 0
#req_rare 2
#req_land 1
#req_unique 3
#req_chaos 0
#req_code -26
#2d4vis 3
#end

#selectevent 2178
#rarity 1
#req_gem 8
#req_code -26
#gemlosssmall 8
#end

#selectevent 2179
#rarity 2
#req_land 1
#req_turn 12
#req_code 0
#code -33
#incscale2 4
#unrest 3
#end

#selectevent 2180
#rarity 2
#req_land 1
#req_turn 12
#req_code 0
#code -33
#defence 8
#end

#selectevent 2181
#rarity 0
#req_rare 4
#req_code -33
#unrest 15
#end

#selectevent 2182
#rarity 0
#req_rare 4
#req_noera 1
#req_code -33
#com 23
#6d6units 22
#com 23
#4d6units 18
#code 0
#end

#selectevent 2183
#rarity 0
#req_rare 4
#req_rare 4  -- stored twice; a second #req_rare in a mod replaces the first
#req_unique 1
#req_code -33
#com 875
#addequip 1
#12d6units 40
#9d6units 33
#9d6units 29
#code 0
#end

#selectevent 2184
#rarity 0
#req_rare 4
#req_rare 1  -- stored twice; a second #req_rare in a mod replaces the first
#req_code -33
#com 45
#2com 35
#6d6units 40
#9d6units 33
#9d6units 29
#code 0
#end

#selectevent 2185
#rarity 0
#req_rare 4
#req_poptype 30
#req_mindef 4
#req_maxtroops 20
#req_code -33
#4com 23
#12d6units 22
#nation -2
#4com 23
#9d6units 22
#code 0
#end

#selectevent 2186
#rarity 0
#req_rare 4
#req_chaos 1
#req_minpop 100
#req_capital 0
#req_code -33
#unrest 20
#kill 5
#code 0
#end

#selectevent 2187
#rarity 0
#req_rare 4
#req_chaos 2
#req_minpop 200
#req_code -33
#unrest 25
#kill 10
#emigration 10
#code 0
#end

#selectevent 2188
#rarity 0
#req_rare 4
#req_poptype 25
#req_freesites 1
#req_minpop 10
#req_code -33
#addsite -1
#unrest 20
#defence 10
#end

#selectevent 2189
#rarity 0
#req_rare 4
#req_order 0
#req_code -33
#defence 10
#end

#selectevent 2190
#rarity 0
#req_rare 4
#req_rare 1  -- stored twice; a second #req_rare in a mod replaces the first
#req_code -33
#nation -2
#com 35
#6d6units 40
#9d6units 33
-- ro: effect 190 (gold) = -100
#end

#selectevent 2191
#rarity 0
#req_rare 4
#req_land 1
#req_turn 5
#req_code -33
#code 90
#end

#selectevent 2192
#rarity 0
#req_rare 4
#req_land 0
#req_rare 1  -- stored twice; a second #req_rare in a mod replaces the first
#req_code -33
#nation -2
#com 406
#6d6units 174
#9d6units 33
-- ro: effect 190 (gold) = -100
#end

#selectevent 2193
#rarity 0
#req_rare 4
#req_turn 8
#req_era 2
#req_code -33
#code 92
#end

#selectevent 2194
#rarity 0
#req_rare 8
#req_maxdef 10
#req_coast 1
#req_noseason 3
#req_code -33
#code 40
#unrest 5
#end

#selectevent 2195
#rarity 0
#req_rare 10
#req_code -33
#code 0
#unrest -12
#end

#selectevent 2196
#rarity 0
#req_rare 4
#req_code -33
#req_land 1
#nation 4
#4com 141
-- ro: effect 183 (24d6units) = 140
#com 147
#6d6units 139
#code 0
#end

#selectevent 2197
#rarity 0
#req_rare 4
#req_unluck 1
#req_turn 10
#req_code -33
#code 93
#end

#selectevent 2198
#rarity 0
#req_rare 4
#req_forest 1
#req_turn 10
#req_death 0
#req_code -33
#code 94
#end

#selectevent 2199
#rarity 0
#req_rare 1
#req_land 0
#req_code -33
#com 406
#9d6units 174
#2com 406
#6d6units 175
#code 0
#end

#selectevent 2200
#rarity 0
#req_rare 4
#req_poptype 30
#req_mindef 5
#req_maxtroops 20
#req_code -33
#4com 23
#12d6units 22
#nation -2
#4com 23
#9d6units 22
#code 0
#end

#selectevent 2201
#rarity 0
#req_rare 4
#req_code -33
#code 0
#unrest -30
#decscale2 0
#nation -2
#com 2323
#end

#selectevent 2202
#rarity 0
#req_rare 4
#req_poptype 25
#req_mindef 5
#req_maxdef 20
#req_code -33
#nation 4
#2com 141
-- ro: effect 177 (18d6units) = 139
#nation -2
#2com 141
-- ro: effect 177 (18d6units) = 140
#end

#selectevent 2203
#rarity 0
#req_rare 4
#req_unique 2
#req_freesites 1
#req_capital 1
#req_code -33
#addsite -1
#end

#selectevent 2204
#rarity 0
#req_rare 1
#req_maxdef 12
#req_code -33
#nation -1
#2com 35
#6d6units 40
#6d6units 33
#code 0
#extramsg -1
#end

#selectevent 2205
#rarity 2
#req_land 1
#req_commander 1
#req_growth 1
#assfollower1d3 2223
#assassin 2223
#end

#selectevent 2206
#rarity 1
#req_mindef 9
#req_growth 1
#defence -10
#delay50 2
#end

#selectevent 2207
#rarity 0
#req_rare 50
#req_mindef 1
#req_growth 2
#defence -6
#delay50 2
#end

#selectevent 2208
#rarity 0
#req_rare 50
#req_mindef 1
#req_growth 3
#defence -6
#end

#selectevent 2209
#rarity 1
#req_site 1
-- ro: effect 39 = 2
#assassin 566
#end

#selectevent 2210
#rarity 1
#req_site 1
#assfollower1d3 -2
-- ro: effect 39 = 2
#assassin 190
#end

#selectevent 2211
#rarity 1
#req_site 1
-- ro: effect 39 = 2
#assassin -2
#end

#selectevent 2212
#rarity -1
#req_fort 0
#req_poptype 27
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 34
#9d6units 17
#9d6units 38
#end

#selectevent 2213
#rarity -1
#req_fort 0
#req_poptype 28
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 35
#9d6units 33
#9d6units 39
#end

#selectevent 2214
#rarity -1
#req_fort 0
#req_poptype 29
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 36
#9d6units 32
#9d6units 40
#end

#selectevent 2215
#rarity -1
#req_fort 0
#req_poptype 30
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 23
#9d6units 55
#6d6units 22
#end

#selectevent 2216
#rarity -1
#req_fort 0
#req_poptype 54
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 46
#9d6units 38
#6d6units 20
#end

#selectevent 2217
#rarity -1
#req_fort 0
#req_poptype 32
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 35
#9d6units 39
#9d6units 47
#end

#selectevent 2218
#rarity -1
#req_fort 0
#req_poptype 33
#req_land 1
#req_luck 0
#req_code -33
#fort 1
#nation -2
#2com 36
#9d6units 40
#9d6units 48
#end

#selectevent 2219
#rarity 0
#req_rare 1
#req_notforally 89
#req_nation 89
#req_coast 1
#req_code -33
#nation 89
#com 444
#com 332
#1d6units 425
#6d6units 335
#extramsg 89
#end

#selectevent 2220
#rarity 2
#req_land 0
#req_turn 12
#req_code 0
#code -34
#incscale2 4
#unrest 3
#end

#selectevent 2221
#rarity 2
#req_land 0
#req_turn 12
#req_code 0
#code -34
#defence 8
#end

#selectevent 2222
#rarity 0
#req_rare 10
#req_code -34
#code 0
#unrest -15
#decscale2 0
#end

#selectevent 2223
#rarity 0
#req_rare 5
#req_code -34
#com 406
#9d6units 174
#2com 406
#6d6units 175
#code 0
#end

#selectevent 2224
#rarity 0
#req_rare 10
#req_code -34
#code 91
#end

#selectevent 2225
#rarity 0
#req_rare 4
#req_code -34
#com 406
#6d6units 174
#com 406
#4d6units 175
#code 0
#end

#selectevent 2226
#rarity -1
#req_fort 0
#req_luck 0
#req_poptype 64
#req_code -34
#fort 1
#nation -2
#2com 406
#6d6units 175
#9d6units 577
#end

#selectevent 2227
#rarity -1
#req_fort 0
#req_luck 0
#req_poptype 63
#req_code -34
#fort 1
#nation -2
#2com 406
#6d6units 175
#9d6units 174
#end

#selectevent 2228
#rarity -1
#req_fort 0
#req_luck 0
#req_poptype 45
#req_code -34
#fort 1
#nation -2
#2com 406
#6d6units 577
#6d6units 545
#end

#selectevent 2229
#rarity -1
#req_fort 0
#req_luck 0
#req_poptype 46
#req_code -34
#fort 1
#nation -2
#com 576
#com 575
#12d6units 573
#end

#selectevent 2230
#rarity -1
#req_fort 0
#req_luck 0
#req_poptype 65
#req_code -34
#fort 1
#nation -2
#2com 976
#6d6units 974
#9d6units 975
#end

#selectevent 2231
#rarity -1
#req_fort 0
#req_luck 0
#req_poptype 31
#req_code -34
#fort 1
#nation -2
#2com 406
#6d6units 175
#9d6units 176
#end

#selectevent 2232
#rarity 0
#req_rare 4
#req_poptype 64
#req_code -34
#code 0
#2com 406
#6d6units 175
#9d6units 577
#end

#selectevent 2233
#rarity 0
#req_rare 4
#req_poptype 63
#req_code -34
#code 0
#2com 406
#6d6units 175
#9d6units 174
#end

#selectevent 2234
#rarity 0
#req_rare 4
#req_poptype 45
#req_code -34
#code 0
#2com 406
#6d6units 577
#6d6units 545
#end

#selectevent 2235
#rarity 0
#req_rare 4
#req_poptype 46
#req_code -34
#code 0
#com 576
#com 575
#12d6units 573
#end

#selectevent 2236
#rarity 0
#req_rare 4
#req_poptype 65
#req_code -34
#code 0
#2com 976
#6d6units 974
#9d6units 975
#end

#selectevent 2237
#rarity 0
#req_rare 4
#req_poptype 31
#req_code -34
#code 0
#2com 406
#6d6units 175
#9d6units 176
#end

#selectevent 2238
#rarity 0
#req_rare 4
#req_poptype 57
#req_code -34
#code 0
#2com 207
#6d6units 110
#9d6units 206
#end

#selectevent 2239
#rarity 0
#req_rare 4
#req_code -34
#code 0
#2com 208
#addequip 1
#4d6units 208
#end

#selectevent 2240
#rarity 0
#req_rare 1
#req_unique 1
#req_code -34
#code 0
#com 310
#addequip 9
#addequip 1
-- ro: effect 183 (24d6units) = 2365
#end

#selectevent 2241
#rarity 0
#req_rare 2
#req_notforally 89
#req_nation 89
#req_code -34
#nation 89
#com 444
#com 332
#1d6units 425
#6d6units 335
#extramsg 89
#end

#selectevent 2242
#rarity 0
#req_rare 12
#req_season 2
#req_code -7
#code 0
#taxboost -100
#unrest 10
#end

#selectevent 2243
#rarity -1
#req_capital 0
#req_rare 50
#req_freesites 1
#req_code -7
#code 0
#unrest 30
#kill 20
#addsite -1
#end

#selectevent 2244
#rarity 0
#req_turn 3
#req_rare 10
#req_season 0
#req_code -7
#code 0
-- ro: effect 190 (gold) = -250
#addgeo 274877906944
#end

#selectevent 2245
#rarity 0
#req_rare 8
#req_heat -1
#req_freshwater 1
#req_cave 0
#req_season 1
#req_code -7
#code 0
#kill 7
#unrest 10
#addgeo 274877906944
#end

#selectevent 2246
#rarity 0
#req_rare 10
#req_freshwater 1
#req_season 2
#req_code -7
#code 0
#kill 4
#unrest 10
#addgeo 274877906944
#end

#selectevent 2247
#rarity 0
#req_rare 3
#req_temple 1
#req_turn 7
#req_code -7
#code 0
#kill 7
#temple 0
#end

#selectevent 2248
#rarity 0
#req_pop0ok
#req_rare 3
#req_lab 1
#req_heat -1
#req_turn 10
#req_code -7
#code 0
#lab 0
#end

#selectevent 2249
#rarity 0
#req_pop0ok
#req_rare 1
#req_turn 10
#req_code -7
#code 0
#com 447
#9d6units 447
#kill 25
#end

#selectevent 2250
#rarity 0
#req_rare 2
#req_code -7
#code 0
#decscale3 2
#end

#selectevent 2251
#rarity 0
#req_rare 2
#req_cave 0
#req_code -7
#code 0
#kill 2
#unrest 5
-- ro: effect 190 (gold) = -80
#end

#selectevent 2252
#rarity 0
#req_rare 10
#req_coast 1
#req_heat 0
#req_code -7
#code 0
#landgold -10
#end

#selectevent 2253
#rarity 0
#req_rare 8
#req_season 3
#req_cold 0
#req_code -7
#code 0
-- ro: effect 190 (gold) = -200
#end

#selectevent 2254
#rarity 0
#req_rare 8
#req_season 3
#req_cold 0
#req_code -7
#code 0
#kill 4
#end

#selectevent 2255
#rarity 0
#req_rare 8
#req_season 3
#req_code -7
#com 1224
#1d6units 1224
#7d6units 284
#kill 2
#unrest 10
#end

#selectevent 2256
#rarity 0
#req_pop0ok
#req_rare 8
#req_season 2
#req_code -7
#code 0
#incscale3 2
#end

#selectevent 2257
#rarity 0
#req_rare 3
#req_cold 1
#req_code -7
#code 0
#incscale3 2
#kill 4
#end

#selectevent 2258
#rarity 0
#req_cave 0
#req_pop0ok
#req_rare 3
#req_code -7
#code 0
#unrest 25
#kill 3
#end

#selectevent 2259
#rarity 0
#req_cave 0
#req_rare 3
#req_cold 2
#req_code -7
#code 0
#kill 2
#unrest 5
-- ro: effect 190 (gold) = -150
#end

#selectevent 2260
#rarity 0
#req_rare 3
#req_growth -2
#req_code -7
#code 0
#unrest 10
-- ro: effect 190 (gold) = -50
#end

#selectevent 2261
#rarity 2
#req_land 1
#req_minpop 20
#req_turn 10
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2262
#rarity 0
#req_rare 7
#req_code -36
#code 0
#taxboost -80
#kill 1
#end

#selectevent 2263
#rarity 2
#req_coast 1
#req_heat 0
#req_code 0
#code -36
#incscale 4
#end

#selectevent 2264
#rarity 2
#req_coast 1
#req_heat 0
#req_code 0
#code -36
#unrest 1
#end

#selectevent 2265
#rarity 0
#req_rare 5
#req_code -36
#code 0
#kill 2
#incscale 3
#disease 1
#end

#selectevent 2266
#rarity 0
#req_rare 3
#req_death 1
#req_code -36
#kill 4
#incscale 3
#disease 1
#end

#selectevent 2267
#rarity 0
#req_rare 4
#req_code -36
#kill 4
#disease 2
#end

#selectevent 2268
#rarity 0
#req_rare 2
#req_temple 0
#req_maxdominion 5
#req_code -36
#code -43
#incdom -2
#incscale2 3
#disease 10
#end

#selectevent 2269
#rarity -1
#req_death 1
#req_minpop 200
#req_land 1
#req_unluck 1
#req_code -36
#code 0
#4d6vis 5
#kill 50
#unrest 10
#4d6vis 5
#end

#selectevent 2270
#rarity 0
#req_rare 4
#req_order 0
#req_code -36
#incscale3 0
#unrest 4
#landprod -2
#end

#selectevent 2271
#rarity 0
#req_rare 10
#req_heat 0
#req_swamp 1
#req_code -36
#kill 1
#disease 4
#end

#selectevent 2272
#rarity 0
#req_rare 10
#req_heat 0
#req_swamp 1
#req_code -36
#kill 1
#disease 4
#end

#selectevent 2273
#rarity 0
#req_rare 1
#req_order 0
#req_code -36
#code 0
#kill 2
#disease 2
-- ro: effect 190 (gold) = 750
#end

#selectevent 2274
#rarity 0
#req_rare 4
#req_dominion 3
#req_temple 1
#req_code -36
#code 0
#kill 2
#disease 2
-- ro: effect 190 (gold) = 750
#end

#selectevent 2275
#rarity -1
#req_freesites 1
#req_code -36
#addsite -1
#end

#selectevent 2276
#rarity 0
#req_rare 1
#req_code -36
#kill 5
#unrest 10
#taxboost -50
#code 0
#end

#selectevent 2277
#rarity 0
#req_rare 2
#req_noseason 3
#req_code -36
#code -16
#kill 2
#unrest 4
#end

#selectevent 2278
#rarity 0
#req_rare 10
#req_coast 1
#req_noseason 3
#req_code -36
#taxboost -20
#landgold -4
#end

#selectevent 2279
#rarity 0
#req_rare 3
#req_growth 0
#req_dominion 3
#req_code -36
#incdom -4
#kill 2
#disease 2
#end

#selectevent 2280
#rarity 0
#req_rare 3
#req_death 1
#req_dominion 3
#req_code -36
#incdom 4
#kill 2
#disease 2
#end

#selectevent 2281
#rarity 0
#req_rare 1
#req_temple 0
#req_code -36
#temple 1
#kill 2
#disease 2
#end

#selectevent 2282
#rarity 0
#req_rare 1
#req_death 1
#req_code -36
#req_land 1
#nation -2
#com 2535
#end

#selectevent 2283
#rarity -1
#req_unique 2
#req_code -36
#magicitem 9
#kill 2
#incscale 3
#disease 1
#end

#selectevent 2284
#rarity 0
#req_rare 1
#req_unique 3
#req_lab 0
#req_code -36
#magicitem 9
#lab 1
#1d6vis 4
#nation -2
#com 478
#end

#selectevent 2285
#rarity 0
#req_rare 1
#req_unique 3
#req_code -36
#req_land 1
#nation -2
#com 478
#addequip 1
#end

#selectevent 2286
#rarity 0
#req_rare 4
#req_pathdeath 2
#req_code -36
#3d6vis 5
#end

#selectevent 2287
#rarity 0
#req_rare 1
#req_code -36
#code 0
#code 5
#2d6vis 0
#end

#selectevent 2288
#rarity 0
#req_rare 1
#req_growth 0
#req_dominion 3
#req_code -36
#code 0
#incdom -4
#kill 2
#disease 2
#end

#selectevent 2289
#rarity 0
#req_rare 1
#req_death 1
#req_dominion 3
#req_code -36
#code 0
#incdom 4
#kill 2
#disease 2
#end

#selectevent 2290
#rarity 0
#req_rare 20
#req_season 3
#req_code -36
#code 0
#end

#selectevent 2291
#rarity 0
#req_rare 7
#req_dominion 2
#req_code -36
#code 0
-- ro: effect 190 (gold) = 200
#kill 1
#end

#selectevent 2292
#rarity 0
#req_site 1
#req_rare 2
#req_turn 10
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2293
#rarity 0
#req_site 1
#req_rare 2
#req_turn 10
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2294
#rarity 0
#req_site 1
#req_rare 2
#req_turn 10
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2295
#rarity 0
#req_site 1
#req_rare 2
#req_turn 10
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2296
#rarity 0
#req_site 1
#req_rare 2
#req_turn 10
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2297
#rarity 0
#req_monster 820
#req_unique 1
#req_rare 13
#req_land 1
#req_anycode -37
#nation -2
#com 2535
#3d6units 2536
#end

#selectevent 2298
#rarity -1
#req_monster 820
#req_code 0
#code -36
#kill 1
#disease 1
#end

#selectevent 2299
#rarity 0
#req_monster 820
#req_unique 1
#req_rare 10
#req_anycode -37
#resetcode -37
#2d6vis 8
#end

#selectevent 2300
#rarity 13
#req_rare 10
#req_code -37
#resetcode -36
#resetcode -37
#unrest -2
#decscale 3
#worldheal 1
#end

#selectevent 2301
#rarity 0
#req_rare 3
#req_minpop 20
#req_land 1
#req_code 0
#req_anycode -37
#code -36
#disease 1
#kill 1
#end

#selectevent 2302
#rarity 0
#req_rare 5
#req_heat 0
#req_coast 1
#req_code 0
#req_anycode -37
#code -36
#incscale 4
#end

#selectevent 2303
#rarity 0
#req_rare 1
#req_death 1
#req_code 0
#req_anycode -37
#code -36
#kill 2
#incscale 3
#disease 1
#end

#selectevent 2304
#rarity 2
#req_land 0
#req_minpop 20
#req_turn 10
#req_code 0
#code -38
#disease 1
#kill 1
#end

#selectevent 2305
#rarity 0
#req_rare 7
#req_code -38
#code 0
#taxboost -80
#kill 1
#end

#selectevent 2306
#rarity 2
#req_land 0
#req_heat 0
#req_code 0
#code -38
#incscale 4
#end

#selectevent 2307
#rarity 0
#req_rare 5
#req_code -38
#code 0
#kill 2
#incscale 3
#disease 1
#end

#selectevent 2308
#rarity 0
#req_rare 3
#req_death 1
#req_code -38
#kill 4
#incscale 3
#disease 1
#end

#selectevent 2309
#rarity 0
#req_rare 4
#req_code -38
#kill 4
#disease 2
#end

#selectevent 2310
#rarity 0
#req_rare 1
#req_order 0
#req_code -38
#code 0
#kill 2
#disease 2
-- ro: effect 190 (gold) = 750
#end

#selectevent 2311
#rarity 0
#req_rare 2
#req_dominion 3
#req_temple 1
#req_code -38
#code 0
#kill 2
#disease 2
-- ro: effect 190 (gold) = 750
#end

#selectevent 2312
#rarity 0
#req_rare 1
#req_growth 0
#req_dominion 3
#req_code -38
#code 0
#incdom -4
#kill 2
#disease 2
#end

#selectevent 2313
#rarity 0
#req_rare 1
#req_death 1
#req_dominion 3
#req_code -38
#code 0
#incdom 4
#kill 2
#disease 2
#end

#selectevent 2314
#rarity 0
#req_rare 4
#req_dominion 2
#req_code -38
#code 0
-- ro: effect 190 (gold) = 200
#kill 1
#end

#selectevent 2315
#rarity 0
#req_rare 4
#req_code -38
#code 0
#decscale 4
#decscale2 3
#end

#selectevent 2316
#rarity 1
#req_unluck -2
#req_land 1
#req_rare 40
#req_code 0
#req_pop0ok
#code -7
#incscale3 4
#end

#selectevent 2317
#rarity 1
#req_unluck 1
#req_land 1
#req_code 0
#code -7
#incscale3 4
#end

#selectevent 2318
#rarity 0
#req_rare 10
#req_code -7
#code 0
#1d3vis 6
#end

#selectevent 2319
#rarity 0
#req_rare 20
#req_season 1
#req_code -7
#code -8
#incscale2 3
#unrest 10
#end

#selectevent 2320
#rarity 0
#req_code -8
#code -9
#kill 3
#end

#selectevent 2321
#rarity 0
#req_code -9
#code 0
#kill 3
#end

#selectevent 2322
#rarity 0
#req_rare 15
#req_code -7
#code 0
#kill 3
#unrest 10
#incscale 3
#end

#selectevent 2323
#rarity 0
#req_unluck 1
#req_death -1
#req_rare 10
#req_code -7
#code 0
#kill 3
#end

#selectevent 2324
#rarity -1
#req_foundsite 1
#req_unique 1
#nation -2
#com 1198
#earthboost 1198
#9d6units 982
#end

#selectevent 2325
#rarity -1
#req_foundsite 1
#req_unique 1
#nation -2
#com 1198
#earthboost 1198
#9d6units 982
#end

#selectevent 2326
#rarity -2
#req_magic 0
#req_unique 1
#req_farm 1
#req_minpop 100
#req_turn 10
#nation -2
#com 1198
#earthboost 1198
#6d6units 982
#end

#selectevent 2327
#rarity -2
#req_magic 2
#req_order 1
#req_land 1
#req_turn 10
#nation -2
#com 391
#addequip 2
#3d3units 390
#end

#selectevent 2328
#rarity -2
#req_pathblood 2
#req_magic 2
#req_land 1
#req_death 0
#req_turn 30
#nation -2
#com 489
#addequip 9
#3d6units 489
#end

#selectevent 2329
#rarity -2
#req_farm 1
#req_noera 1
#req_prod 1
#req_turn 10
#nation -2
#7d6units 49
#end

#selectevent 2330
#rarity -1
#req_farm 1
#req_noera 1
#req_prod 2
#req_turn 10
#nation -2
#7d6units 49
#end

#selectevent 2331
#rarity -1
#req_foundsite 1
#req_unique 1
#req_turn 10
-- ro: effect 190 (gold) = -50
#nation -2
#com 444
#gainaff 549755813888
#6d6units 424
#1unit 425
#end

#selectevent 2332
#rarity -2
#req_coast 1
#req_unique 1
#req_turn 10
-- ro: effect 190 (gold) = -50
#nation -2
#com 444
#gainaff 549755813888
#6d6units 424
#1unit 425
#end

#selectevent 2333
#rarity -2
#req_forest 1
#req_unique 2
#req_magic 1
#req_prod 1
#req_turn 10
#nation -2
#com 552
#addequip 1
#3d6units 476
#end

#selectevent 2334
#rarity -1
#req_unique 1
#req_site 1
#req_code 0
#code 176
#2com 332
#addequip 1
#9d6units 424
#end

#selectevent 2335
#rarity 0
#req_nomonster 332
#req_code 176
#code 0
#4d6vis 4
#3d6units 3
#end

#selectevent 2336
#rarity -2
#req_rare 20
#req_chaos 2
#req_freesites 1
#req_minpop 90
#addsite -1
#end

#selectevent 2337
#rarity -2
#req_unmagic 2
#req_land 1
#req_dominion 6
#req_rare 25
#magicitem 2
#1d3vis 0
#1d3vis 2
#nation -2
#4com 2537
#end

#selectevent 2338
#rarity -2
#req_unmagic 1
#magicitem 1
#1d3vis 6
#1d3vis 5
#curse 8
#end

#selectevent 2339
#rarity -2
#req_unmagic 1
#magicitem 1
#1d3vis 3
#1d3vis 4
#magicitem 2
#end

#selectevent 2340
#rarity -2
#req_unmagic 1
#magicitem 9
#end

#selectevent 2341
#rarity -2
#req_unmagic 2
#magicitem 9
#magicitem 9
#magicitem 9
#end

#selectevent 2342
#rarity -1
#req_targpath1 6
#req_targmale 1
#req_forest 1
#req_magic 0
#req_growth 0
#req_targgod 0
#gainaff 549755813888
#nation -2
-- ro: effect 154 = -12
#end

#selectevent 2343
#rarity 12
#req_unique 2
#req_turn 33
#req_story 1
#req_code 0
#code -19
#code2 -44
#worldincscale 0
#worldritrebate 6
#delay25 5
#end

#selectevent 2344
#rarity 13
#resetcode -44
#end

#selectevent 2345
#rarity 0
#req_code 0
#req_capital 0
#req_rare 3
#req_temple 1
#req_land 1
#req_code 0
#req_anycode -44
#code -49
#incdom -5
#incscale2 0
#end

#selectevent 2346
#rarity 2
#req_story 1
#req_temple 1
#req_land 1
#req_code 0
#code -49
#incdom -5
#incscale2 0
#end

#selectevent 2347
#rarity 0
#req_code 0
#req_capital 0
#req_rare 2
#req_land 1
#req_temple 1
#req_code 0
#req_anycode -44
#code -49
#incdom -3
#incscale 0
#end

#selectevent 2348
#rarity 2
#req_story 1
#req_maxdominion 4
#req_land 1
#req_temple 1
#req_code 0
#req_capital 0
#code -49
#incdom -3
#incscale 0
#end

#selectevent 2349
#rarity 0
#req_rare 1
#req_land 1
#req_code 0
#req_anycode -44
#code -49
#kill 1
#end

#selectevent 2350
#rarity 0
#req_rare 1
#req_land 1
#req_code 0
#req_anycode -44
#code -49
#nation -2
#1unit 1000
#end

#selectevent 2351
#rarity 0
#req_rare 8
#req_land 1
#req_season 1
#req_code 0
#req_anycode -44
#code -49
#incscale 0
#decscale2 3
#taxboost 80
#incdom -2
#end

#selectevent 2352
#rarity 2
#req_story 1
#req_capital 0
#req_land 1
#req_season 1
#req_code 0
#req_maxdominion 2
#code -49
#incscale 0
#decscale2 3
#taxboost 80
#incdom -2
#end

#selectevent 2353
#rarity 0
#req_rare 1
#req_land 1
#req_code 0
#req_anycode -44
#code -49
#incscale2 0
#unrest 3
#end

#selectevent 2354
#rarity 0
#req_rare 2
#req_land 1
#req_code 0
#req_anycode -44
#code -49
#incdom -3
#incscale2 0
#end

#selectevent 2355
#rarity 0
#req_rare 1
#req_land 1
#req_code 0
#req_anycode -44
#code -49
#incscale2 0
#unrest 3
#end

#selectevent 2356
#rarity 2
#req_story 1
#req_maxdominion 3
#req_land 1
#req_code 0
#req_capital 0
#code -49
#incscale2 0
#unrest 3
#end

#selectevent 2357
#rarity 0
#req_rare 1
#req_freesites 1
#req_unique 4
#req_code -49
#req_land 1
#addsite -1
#end

#selectevent 2358
#rarity 0
#req_rare 4
#req_farm 1
#req_code -49
#code 0
#com 632
#com 1565
#addequip 1
#bloodboost 1
#end

#selectevent 2359
#rarity 0
#req_rare 1
#req_targpath1 53
#req_targgod 0
#req_code -49
#code 0
#banished -12
#bloodboost 1
#end

#selectevent 2360
#rarity 0
#req_rare 1
#req_targpath1 53
#req_targgod 0
#req_code -49
#code 0
#banished -12
#fireboost 1
#end

#selectevent 2361
#rarity 0
#req_rare 1
#req_targpath1 53
#req_targgod 0
#req_code -49
#code 0
#banished -13
#waterboost 1
#end

#selectevent 2362
#rarity 0
#req_rare 1
#req_targpath1 8
#req_targgod 0
#req_targmale 1
#req_code -49
#code 0
#gainaff 8589934592
#bloodboost 1
#end

#selectevent 2363
#rarity 0
#req_rare 1
#req_targpath1 8
#req_targgod 0
#req_targmale 1
#req_code -49
#code 0
#gainaff 4096
#end

#selectevent 2364
#rarity 0
#req_rare 1
#req_targpath2 8
#req_lab 1
#req_unique 6
#req_code -49
#code 0
#nation -2
#com 304
#end

#selectevent 2365
#rarity 0
#req_rare 1
#req_targpath2 8
#req_lab 1
#req_unique 6
#req_code -49
#code 0
-- ro: effect 39 = 6
#assassin 304
#end

#selectevent 2366
#rarity 0
#req_rare 1
#req_targpath2 8
#req_lab 1
#req_gem 8
#req_code -49
#code 0
#nation -2
#com 811
#gemloss 8
#end

#selectevent 2367
#rarity 0
#req_rare 1
#req_targpath2 8
#req_lab 1
#req_gem 8
#req_code -49
#code 0
-- ro: effect 39 = 6
#assassin 811
#gemloss 8
#end

#selectevent 2368
#rarity 0
#req_rare 6
#req_targgod 1
#req_land 1
#req_code -49
#code 0
#nation -2
#1unit 1000
#end

#selectevent 2369
#rarity 0
#req_rare 1
#req_targpath2 8
#req_lab 1
#req_targgod 0
#req_code -49
#code 0
#com 304
#end

#selectevent 2370
#rarity 0
#req_rare 1
#req_freesites 1
#req_unique 3
#req_code -49
#code 0
#4com 88
#4d6units 88
#maybeaddsite -1
#end

#selectevent 2371
#rarity 0
#req_rare 1
#req_unique 2
#req_freesites 1
#req_code -49
#incdom -2
#kill 1
#maybehiddensite -1
#stealthcom 88
#3d6units 88
#end

#selectevent 2372
#rarity 0
#req_rare 5
#req_pathblood 5
#req_unique 2
#req_lab 1
#req_code -49
#nation -2
#com 95
#addequip 9
#end

#selectevent 2373
#rarity 0
#req_rare 2
#req_code -49
#req_land 1
#3d6vis 8
#end

#selectevent 2374
#rarity 0
#req_rare 5
#req_temple 1
#req_code -49
#incdom -5
#incscale2 0
#end

#selectevent 2375
#rarity 0
#req_turn 15
#req_rare 2
#req_unique 1
#req_code -49
#code 0
#com 811
#com 489
#addequip 9
#3d6units 489
#end

#selectevent 2376
#rarity 0
#req_rare 1
#req_code -49
#stealthcom 88
#addequip 9
#3d6units 88
#end

#selectevent 2377
#rarity 1
#req_targgod 0
#req_commander 1
#req_code -49
#code 0
#banished -12
#fireboost 1
#end

#selectevent 2378
#rarity 1
#req_targgod 0
#req_commander 1
#req_code -49
#code 0
#banished -13
#waterboost 1
#end

#selectevent 2379
#rarity 1
#req_targgod 0
#req_commander 1
#req_code -49
#code 0
#banished -11
#astralboost 1
#end

#selectevent 2380
#rarity 0
#req_foundsite 1
#req_rare 30
#req_anycode -44
#req_code 0
#code -2
#decscale 5
#incscale2 0
#unrest 30
#end

#selectevent 2381
#rarity 0
#req_rare 2
#req_code -49
#code 0
#2com 638
#3d6units 638
#incscale3 0
#end

#selectevent 2382
#rarity 0
#req_rare 1
#req_code -49
#code 0
#2com 449
#3d6units 449
#incscale3 0
#incscale3 2
#end

#selectevent 2383
#rarity 0
#req_rare 2
#req_code -49
#code 0
#2com 632
#1d6units 632
#incscale3 0
#end

#selectevent 2384
#rarity 0
#req_rare 1
#req_code -49
#code 0
#2com 638
#3d6units 638
#incscale3 0
#end

#selectevent 2385
#rarity 0
#req_rare 4
#req_code -49
#code 0
#incscale3 4
#unrest 15
#end

#selectevent 2386
#rarity 0
#req_rare 2
#req_commander 1
#req_code -49
#code 0
#assassin 1738
#end

#selectevent 2387
#rarity 0
#req_rare 15
#req_anycode -44
#req_hiddensite 1
#end

#selectevent 2388
#rarity 0
#req_rare 15
#req_anycode -44
#req_foundsite 1
#3d6vis 1
#incscale3 0
#incdom -3
#end

#selectevent 2389
#rarity 0
#req_rare 4
#req_foundsite 1
#req_anycode -44
#com 305
#addequip 1
#end

#selectevent 2390
#rarity 0
#req_rare 4
#req_foundsite 1
#req_anycode -44
#com 304
#end

#selectevent 2391
#rarity 0
#req_rare 5
#req_foundsite 1
#req_anycode -44
#4com 88
#4d6units 88
#end

#selectevent 2392
#rarity 0
#req_rare 15
#req_hiddensite 1
#req_anycode -44
#4com 88
#4d6units 88
#revealsite
#end

#selectevent 2393
#rarity 0
#req_rare 3
#req_monster 339
#req_foundsite 1
#req_anycode -44
-- ro: effect 39 = 2
#assassin 88
#end

#selectevent 2394
#rarity 0
#req_rare 3
#req_monster 339
#req_foundsite 1
#req_anycode -44
-- ro: effect 39 = 2
#assassin 304
#end

#selectevent 2395
#rarity 0
#req_rare 3
#req_monster 339
#req_foundsite 1
#req_anycode -44
-- ro: effect 39 = 2
#assassin 433
#end

#selectevent 2396
#rarity 0
#req_foundsite 1
#req_rare 3
#req_anycode -44
#nation -2
#1d3units 304
#kill 1
#incscale 0
#end

#selectevent 2397
#rarity 0
#req_foundsite 1
#req_rare 3
#req_anycode -44
#nation -2
#1d3units 88
#kill 1
#incscale 0
#end

#selectevent 2398
#rarity 0
#req_foundsite 1
#req_rare 3
#req_anycode -44
#nation -2
#1d3units 433
#kill 1
#incscale 0
#end

#selectevent 2399
#rarity 0
#req_rare 3
#req_monster 339
#req_foundsite 1
#req_anycode -44
-- ro: effect 39 = 6
#assassin 88
#end

#selectevent 2400
#rarity 0
#req_rare 3
#req_monster 339
#req_foundsite 1
#req_anycode -44
-- ro: effect 39 = 6
#assassin 304
#end

#selectevent 2401
#rarity 0
#req_rare 3
#req_monster 339
#req_foundsite 1
#req_anycode -44
-- ro: effect 39 = 6
#assassin 433
#end

#selectevent 2402
#rarity 0
#req_foundsite 1
#req_rare 3
#req_anycode -44
#nation -2
#1d3units 304
#kill 1
#incscale 0
#end

#selectevent 2403
#rarity 0
#req_foundsite 1
#req_rare 3
#req_anycode -44
#nation -2
#1d3units 88
#kill 1
#incscale 0
#end

#selectevent 2404
#rarity 0
#req_foundsite 1
#req_rare 3
#req_anycode -44
#nation -2
#1d3units 433
#kill 1
#incscale 0
#end

#selectevent 2405
#rarity 0
#req_rare 3
#req_code -49
#landgold 10
#landprod 5
#end

#selectevent 2406
#rarity -2
#req_rare 10
#req_land 1
#req_freesites 1
#req_story 1
#req_anycode -44
#hiddensite -1
#end

#selectevent 2407
#rarity 1
#req_rare 50
#req_nomnr 821
#req_unique 1
#req_story 1
#req_code -49
#com 821
#addequip 1
#com 1000
#4d6units 449
#4d6units 303
#end

#selectevent 2408
#rarity 1
#req_rare 50
#req_nomnr 826
#req_unique 1
#req_story 1
#req_code -49
#com 826
#addequip 1
#com 1000
#4d6units 304
#4d6units 2286
#end

#selectevent 2409
#rarity 1
#req_rare 50
#req_nomnr 1405
#req_unique 1
#req_story 1
#req_code -49
#com 1405
#addequip 1
#com 304
#4d6units 304
#4d6units 303
#end

#selectevent 2410
#rarity 1
#req_rare 4
#req_nomnr 818
#req_unique 1
#req_story 1
#req_code -49
#com 818
#addequip 1
#addequip 9
#4d6units 88
#4d6units 304
#4d6units 2287
#end

#selectevent 2411
#rarity 1
#req_monster 810
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2412
#rarity 1
#req_monster 492
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2413
#rarity 1
#req_monster 820
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2414
#rarity 1
#req_monster 819
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2415
#rarity 1
#req_monster 821
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2416
#rarity 1
#req_monster 822
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2417
#rarity 1
#req_monster 823
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2418
#rarity 1
#req_monster 824
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2419
#rarity 1
#req_monster 825
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2420
#rarity 1
#req_monster 826
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2421
#rarity 1
#req_monster 827
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2422
#rarity 1
#req_monster 828
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2423
#rarity 1
#req_targmnr 829
#req_story 1
#req_anycode -44
#banished -12
#end

#selectevent 2424
#rarity 1
#req_monster 900
#req_story 1
#req_anycode -44
#kill 10
#end

#selectevent 2425
#rarity 10
#req_code -44
#worldincscale 0
#end

#selectevent 2426
#rarity 0
#req_rare 1
#req_capital 0
#req_minpop 70
#req_code -49
#code 187
#flagland 1
#unrest 5
#incscale 0
#end

#selectevent 2427
#rarity 0
#req_preach 25
#req_code 188
#req_code 187
#code 0
#unrest -5
#incdom 2
#end

#selectevent 2428
#rarity 0
#req_rare 70
#req_code 187
#code 188
#unrest 20
#incscale3 0
#end

#selectevent 2429
#rarity 0
#req_rare 15
#req_code 188
#landgold -5
#landprod -5
#taxboost -20
#unrest 5
#incscale 0
#end

#selectevent 2430
#rarity 0
#req_rare 3
#req_freesites 1
#req_unique 2
#req_code 188
#addsite -1
#unrest 5
#incscale 0
#end

#selectevent 2431
#rarity 0
#req_rare 3
#req_freesites 1
#req_unique 2
#req_code 188
#addsite -1
#unrest 5
#incscale 0
#end

#selectevent 2432
#rarity 0
#req_rare 15
#req_mindef 4
#req_code 188
#defence -10
#unrest 5
#incscale 0
#end

#selectevent 2433
#rarity 0
#req_rare 15
#req_code 188
#taxboost -100
#unrest 5
#incscale 0
#end

#selectevent 2434
#rarity 0
#req_rare 12
#req_code 188
#landgold -2
#landprod -2
#kill 2
#unrest 10
#incscale 0
#end

#selectevent 2435
#rarity 0
#req_rare 3
#req_luck 0
#req_code 188
#code 0
#nation -2
#com 240
#pathboost 9
#decscale2 0
#end

#selectevent 2436
#rarity 0
#req_rare 5
#req_unique 1
#req_code 188
#landgold -2
#landprod -2
#kill 2
#unrest 10
#incscale 0
#end

#selectevent 2437
#rarity 0
#req_rare 2
#req_code 188
#code 0
#kill 5
#4com 304
#3d6units 304
#3d6units 303
#end

#selectevent 2438
#rarity 0
#req_story 1
#req_rare 13
#req_hiddensite 1
#req_unique 1
#req_anycode -44
#req_code 0
#revealsite
#incscale2 0
#code -93
#end

#selectevent 2439
#rarity 0
#req_rare 80
#req_foundsite 1
#req_unique 1
#req_story 1
#req_code -93
#req_turn 9
#code -45
#flagland 1
#end

#selectevent 2440
#rarity 0
#req_freesites 1
#req_story 1
#req_anycode -45
#req_turn 10
#req_land 1
#req_unique 1
#req_code 0
#code -81
#notext
#hiddensite -1
#end

#selectevent 2441
#rarity 0
#req_freesites 1
#req_story 1
#req_anycode -45
#req_turn 10
#req_land 1
#req_unique 1
#req_code 0
#notext
#hiddensite -1
#code -82
#end

#selectevent 2442
#rarity 13
#req_unique 1
#req_code -45
#revealprov
#worldincscale 0
#worlddarkness
#linger 2
#delay50 5
#end

#selectevent 2443
#rarity 0
#req_site 1
#req_unique 1
#req_code -81
#req_anycode -45
#code -46
#resetcode -45
#revealsite
#flagland 1
#end

#selectevent 2444
#rarity 13
#req_unique 1
#req_code -46
#delay50 4
#revealprov
#worldincscale 0
#worlddisease 2
#linger 2
#end

#selectevent 2445
#rarity 0
#req_site 1
#req_unique 1
#req_code -82
#req_anycode -46
#code -47
#resetcode -45
#revealsite
#flagland 1
#4com 304
#7d6units 304
#4d6units 303
#end

#selectevent 2446
#rarity 13
#req_unique 1
#req_code -47
#revealprov
#worldincscale 0
#worldage 1
#linger 3
#delay 7
#end

#selectevent 2447
#rarity 13
#req_code -47
#code 0
#resetcode -45
#resetcode -46
#resetcode -47
#resetcode -48
#end

#selectevent 2448
#rarity 13
#req_preach 5
#req_code -45
#code 0
#decscale2 0
#incdom 2
#end

#selectevent 2449
#rarity 13
#req_preach 4
#req_code -46
#code 0
#decscale2 0
#incdom 2
#end

#selectevent 2450
#rarity 13
#req_preach 3
#req_code -47
#code 0
#decscale2 0
#incdom 2
#resetcode -46
#resetcode -45
#resetcode -49
#end

#selectevent 2451
#rarity 0
#req_targpath2 8
#req_lab 1
#req_code -45
#req_unique 1
#gainaff 2097152
#end

#selectevent 2452
#rarity 0
#req_targpath2 8
#req_lab 1
#req_code -46
#req_unique 1
#pathboost 8
#gainaff 8589934592
#end

#selectevent 2453
#rarity 0
#req_targpath2 8
#req_lab 1
#req_code -47
#req_unique 1
#pathboost 8
#gainaff 2
#end

#selectevent 2454
#rarity 0
#req_rare 3
#req_targpath1 8
#req_code -45
#req_code -46
#req_code -47
#banished -12
#pathboost 8
#pathboost 0
#end

#selectevent 2455
#rarity 0
#req_rare 3
#req_targpath1 8
#req_code -45
#req_code -46
#req_code -47
#banished -13
#pathboost 8
#pathboost 2
#end

#selectevent 2456
#rarity 0
#req_rare 10
#req_targpath4 8
#req_code -45
#req_code -46
#req_code -47
#code 0
#pathboost 8
#bloodboost 1
#end

#selectevent 2457
#rarity 0
#req_foundsite 1
#req_rare 30
#req_anycode -44
#req_code 0
#code -2
#decscale 5
#incscale2 0
#unrest 30
#end

#selectevent 2458
#rarity 0
#req_rare 14
#req_nomnr 818
#req_code -46
#req_code -47
#com 818
#addequip 1
#9d6units 88
#12d6units 304
#end

#selectevent 2459
#rarity 2
#req_land 1
#req_nomnr 821
#req_unique 1
#req_story 1
#req_anycode -46
#com 821
#addequip 1
#com 1000
#4d6units 449
#4d6units 303
#end

#selectevent 2460
#rarity 2
#req_land 1
#req_nomnr 826
#req_unique 1
#req_story 1
#req_anycode -46
#com 826
#addequip 1
#com 1000
#6d6units 304
#6d6units 2286
#end

#selectevent 2461
#rarity 2
#req_land 1
#req_nomnr 1405
#req_unique 1
#req_story 1
#req_anycode -46
#com 1405
#addequip 1
#com 304
#6d6units 304
#6d6units 303
#end

#selectevent 2462
#rarity 0
#req_nearbycode -46
#req_code 0
#code -83
#notext
#end

#selectevent 2463
#rarity 0
#req_nearbycode -47
#req_code 0
#code -83
#notext
#end

#selectevent 2464
#rarity 13
#req_rare 45
#req_land 1
#req_nomnr 818
#req_unique 1
#req_story 1
#req_anycode -83
#resetcode -83
#com 818
#addequip 1
#6d6units 88
#6d6units 304
#6d6units 2287
#end

#selectevent 2465
#rarity 1
#req_rare 20
#req_foundsite 1
#req_unique 1
#req_turn 33
#req_code 0
#code -45
#flagland 1
#end

#selectevent 2466
#rarity 12
#req_story 1
#req_rare 25
#req_nomnr 492
#req_land 1
#req_turn 66
#req_unique 1
#com 492
#addequip 1
#addequip 9
#6d6units 88
#6d6units 2287
#end

#selectevent 2467
#rarity 13
#req_story 1
#req_rare 20
#req_nomnr 820
#req_land 1
#req_anycode -83
#req_unique 1
#resetcode -83
#com 820
#addequip 1
#addequip 9
#12d6units 638
#end

#selectevent 2468
#rarity 13
#req_code 0
#req_story 1
#req_rare 20
#req_nomnr 819
#req_land 1
#req_anycode -47
#req_unique 1
#code -91
#com 819
#addequip 1
#addequip 9
#15d6units 638
-- ro: effect 177 (18d6units) = -2
#end

#selectevent 2469
#rarity 1
#req_land 1
#req_nomnr 821
#req_unique 1
#req_story 1
#req_anycode -47
#com 821
#addequip 1
#com 1000
#4d6units 449
#4d6units 303
#end

#selectevent 2470
#rarity 1
#req_land 1
#req_nomnr 826
#req_unique 1
#req_story 1
#req_anycode -47
#com 826
#addequip 1
#com 1000
#4d6units 304
#4d6units 2286
#end

#selectevent 2471
#rarity 1
#req_land 1
#req_nomnr 1405
#req_unique 1
#req_story 1
#req_anycode -47
#com 1405
#addequip 1
#com 304
#4d6units 304
#4d6units 303
#end

#selectevent 2472
#rarity 1
#req_land 1
#req_nomnr 818
#req_unique 1
#req_story 1
#req_anycode -47
#com 818
#addequip 1
#3d6units 88
#6d6units 304
#6d6units 2287
#end

#selectevent 2473
#rarity 0
#req_land 1
#req_minpop 20
#req_monster 820
#req_anycode -46
#req_code 0
#code -36
#disease 1
#kill 1
#end

#selectevent 2474
#rarity 0
#req_land 0
#req_minpop 20
#req_monster 820
#req_anycode -46
#req_code 0
#code -38
#disease 1
#kill 1
#end

#selectevent 2475
#rarity 13
#req_indepok
#req_rare 15
#req_targmnr 819
#req_anycode -47
#bloodboost 1
#deathboost 1
#4d6units 638
#3d6units 326
#end

#selectevent 2476
#rarity 13
#req_code -91
#req_unique 3
#req_land 1
#req_indepok
#req_rare 10
#req_targmnr 819
#req_anycode -47
#code -92
#bloodboost 1
#deathboost 1
#worldincscale3 0
#worldage 2
#revealprov
#end

#selectevent 2477
#rarity 13
#req_nomonster 819
#req_code -92
#code 0
#worlddecscale 0
#end

#selectevent 2478
#rarity 13
#req_rare 50
#req_indepok
#req_unique 1
#req_nomnr 492
#req_rare 13  -- stored twice; a second #req_rare in a mod replaces the first
#req_anycode -45
#req_code 0
#req_land 1
#flagland 1
#revealprov
#code 189
#stealthcom 492
#addequip 1
#addequip 9
#stealthcom 88
#6d6units 88
#end

#selectevent 2479
#rarity 0
#req_indepok
#req_rare 10
#req_monster 492
#req_code 189
#req_unique 3
#code 190
#flagland 0
#killmon 492
#end

#selectevent 2480
#rarity 13
#req_indepok
#req_rare 90
#req_unique 5
#req_land 1
#req_code 0
#req_anycode 190
#revealprov
#resetcode 190
#flagland 1
#code 189
#stealthcom 492
#addequip 1
#addequip 9
#6d6units 88
#end

#selectevent 2481
#rarity 0
#req_indepok
#req_rare 20
#req_unique 5
#req_code 189
#stealthcom 88
#3d6units 2287
#3d6units 88
#incscale2 0
#end

#selectevent 2482
#rarity 13
#req_indepok
#req_nomonster 492
#req_code 189
#code 0
#resetcode 189
#resetcode 190
#end

#selectevent 2483
#rarity 0
#req_indepok
#req_nomonster 492
#req_code 189
#req_unique 1
#req_freesites 1
#addsite -1
#3d6vis 8
#end

#selectevent 2484
#rarity 0
#req_rare 3
#req_temple 1
#req_anycode -45
#req_anycode -47
#req_anycode -46
#req_code 0
#code -49
-- ro: effect 190 (gold) = -100
#incdom -5
#incscale2 0
#end

#selectevent 2485
#rarity 0
#req_rare 3
#req_land 1
#req_temple 1
#req_anycode -46
#req_anycode -47
#req_code 0
#code -49
#incdom -3
#incscale 0
#end

#selectevent 2486
#rarity 0
#req_code 0
#req_rare 2
#req_land 1
#req_anycode -46
#req_anycode -47
#req_anycode -45
#code -49
#end

#selectevent 2487
#rarity 0
#req_code 0
#req_rare 2
#req_land 1
#req_anycode -47
#code -49
#incscale2 0
#unrest 3
#end

#selectevent 2488
#rarity 0
#req_code 0
#req_rare 2
#req_land 1
#req_anycode -47
#code -49
#incdom -2
#incscale2 0
#end

#selectevent 2489
#rarity 0
#req_code 0
#req_rare 2
#req_land 1
#req_anycode -47
#code -49
#incscale2 0
#unrest 3
#end

#selectevent 2490
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -46
#req_anycode -47
#end

#selectevent 2491
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -46
#req_anycode -47
#end

#selectevent 2492
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -46
#req_anycode -47
#end

#selectevent 2493
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -45
#req_anycode -46
#req_anycode -47
#end

#selectevent 2494
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -45
#req_anycode -46
#req_anycode -47
#end

#selectevent 2495
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -45
#req_anycode -46
#req_anycode -47
#end

#selectevent 2496
#rarity 0
#req_rare 1
#req_story 1
#req_anycode -44
#req_chaos -1
#req_code -49
#code 179
#flagland 1
#end

#selectevent 2497
#rarity 0
#req_preach 25
#req_temple 1
#req_dominion 2
#req_code 179
#req_code 180
#code 0
#end

#selectevent 2498
#rarity 0
#req_preach 12
#req_temple 0
#req_dominion 2
#req_code 179
#req_code 180
#code 0
#end

#selectevent 2499
#rarity 0
#req_rare 5
#req_targorder 6
#req_code 181
#req_code 180
#req_code 179
-- ro: effect 39 = 2
#assassin 1662
#end

#selectevent 2500
#rarity 0
#req_rare 1
#req_unique 3
#req_pathholy 1
#req_code 180
#req_code 179
#nation -2
#com 478
#end

#selectevent 2501
#rarity 0
#req_rare 2
#req_targpath1 9
#req_code 181
#req_code 180
#req_code 179
-- ro: effect 39 = 2
#assassin 303
#end

#selectevent 2502
#rarity 0
#req_rare 4
#req_targpath1 9
#req_code 181
#req_code 180
#req_code 179
-- ro: effect 39 = 2
#assassin 638
#incdom -2
#incscale2 0
#end

#selectevent 2503
#rarity 0
#req_rare 5
#req_monster 240
#req_code 181
#req_code 180
#req_code 179
#incdom -3
#curse 10
#killmon 240
#end

#selectevent 2504
#rarity 0
#req_rare 3
#req_targpath1 9
#req_code 181
#req_code 180
#req_code 179
#holyboost 1
#holyboost 1
-- ro: effect 39 = 2
#assassin 526
#end

#selectevent 2505
#rarity 0
#req_rare 3
#req_targpath1 9
#req_code 181
#req_code 180
#req_code 179
#holyboost 1
-- ro: effect 39 = 2
#assassin 303
#end

#selectevent 2506
#rarity 0
#req_rare 2
#req_code 184
#req_code 181
#req_code 180
#req_code 179
#stealthcom 88
#1d3units 88
#incdom -3
#incscale2 0
#end

#selectevent 2507
#rarity 0
#req_rare 3
#req_code 184
#req_code 181
#req_code 180
#req_code 179
#stealthcom 2287
#3d6units 2287
#incdom -3
#incscale2 0
#end

#selectevent 2508
#rarity 0
#req_rare 3
#req_code 184
#req_code 181
#req_code 180
#req_code 179
#stealthcom 811
#incdom -3
#incscale2 0
#end

#selectevent 2509
#rarity 0
#req_rare 30
#req_temple 0
#req_code 180
#req_code 179
#incdom -3
#incscale 0
#end

#selectevent 2510
#rarity 0
#req_rare 5
#req_unique 2
#req_commander 1
#req_code 180
#req_code 179
#magicitem 9
#end

#selectevent 2511
#rarity 0
#req_rare 20
#req_chaos 2
#req_code 179
#code 180
#end

#selectevent 2512
#rarity 0
#req_rare 14
#req_code 179
#code 180
#end

#selectevent 2513
#rarity 0
#req_rare 10
#req_temple 1
#req_code 183
#req_code 180
#incdom -4
-- ro: effect 190 (gold) = -150
#incscale2 0
#end

#selectevent 2514
#rarity 0
#req_rare 4
#req_targgod 0
#req_code 183
#req_code 181
#req_code 180
#banished -12
#incscale2 0
#end

#selectevent 2515
#rarity 0
#req_rare 5
#req_noera 1
#req_unique 2
#req_code 183
#req_code 180
#nation -2
#com 2359
#addequip 9
#3d6units 2360
#end

#selectevent 2516
#rarity 0
#req_rare 4
#req_freesites 1
#req_unique 2
#req_code 180
#addsite -1
#end

#selectevent 2517
#rarity 0
#req_rare 5
#req_code 180
#2d6vis 8
#end

#selectevent 2518
#rarity 0
#req_rare 40
#req_mintroops 20
#req_targorder 3
#req_code 180
#code 181
#end

#selectevent 2519
#rarity 0
#req_rare 40
#req_mintroops 20
#req_targorder 3
#req_code 181
#code 182
#com 95
#addequip 9
#addequip 1
#4com 94
#3d6units 303
#4d6units 2536
#com 2540
#addequip 1
#end

#selectevent 2520
#rarity 0
#req_nomonster 95
#req_nomonster 94  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_code 182
#code 0
-- ro: effect 190 (gold) = 300
#magicitem 2
#2d6vis 8
#incdom 2
#end

#selectevent 2521
#rarity 0
#req_rare 3
#req_nomonster 826
#req_code 181
#code 182
#com 826
#addequip 9
#addequip 1
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2522
#rarity 0
#req_rare 3
#req_nomonster 827
#req_code 181
#code 182
#com 827
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2523
#rarity 0
#req_rare 3
#req_nomonster 828
#req_code 181
#code 182
#com 828
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2524
#rarity 0
#req_rare 3
#req_nomonster 829
#req_code 181
#code 182
#com 829
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2525
#rarity 0
#req_rare 3
#req_nomonster 818
#req_code 181
#code 182
#com 818
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2526
#rarity 0
#req_rare 3
#req_nomonster 819
#req_code 181
#code 182
#com 819
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2527
#rarity 0
#req_rare 3
#req_nomonster 820
#req_code 181
#code 182
#com 820
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2528
#rarity 0
#req_rare 1
#req_nomonster 810
#req_code 181
#code 182
#com 810
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2529
#rarity 0
#req_rare 1
#req_nomonster 1405
#req_code 181
#code 182
#com 1405
#addequip 1
#addequip 9
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2530
#rarity 0
#req_rare 2
#req_nomonster 822
#req_code 181
#code 182
#com 822
#addequip 1
#com 95
#3d6units 303
#4d6units 2536
#com 2536
#addequip 1
#end

#selectevent 2531
#rarity 0
#req_rare 55
#req_targpath1 8
#req_targgod 0
#req_targhumanoid 1
#req_code 180
#code 181
#end

#selectevent 2532
#rarity 0
#req_rare 50
#req_targpath4 8
#req_targgod 0
#req_targhumanoid 1
#req_code 181
#code 0
#com 95
#3d6units 2536
#nation -2
#com 827
#end

#selectevent 2533
#rarity 0
#req_rare 3
#req_nomonster 820
#req_code 181
#code 182
#com 820
#addequip 1
#addequip 9
#com 95
#end

#selectevent 2534
#rarity 0
#req_preach 15
#req_dominion 4
#req_code 180
#code 181
#end

#selectevent 2535
#rarity 0
#req_preach 15
#req_dominion 5
#req_code 180
#code 182
#com 95
#addequip 9
#addequip 1
#4com 94
#3d6units 303
#4d6units 2536
#com 2540
#addequip 1
#nation -2
#2com 39
#6d6units 1565
#end

#selectevent 2536
#rarity 0
#req_rare 25
#req_mintroops 20
#req_targgod 0
#req_targpath2 8
#req_code 181
#code 182
#com 95
#addequip 9
#addequip 1
#4com 94
#3d6units 303
#4d6units 2536
#end

#selectevent 2537
#rarity 0
#req_rare 10
#req_mintroops 45
#req_code 179
#code 181
#2d6vis 8
#end

#selectevent 2538
#rarity 0
#req_unique 2
#req_rare 4
#req_targgod 0
#req_targpath1 53
#req_code 179
#req_code 180
#bloodboost 1
#end

#selectevent 2539
#rarity 0
#req_rare 1
#req_order 1
#req_unique 2
#req_nopathblood 1
#req_code 180
#nation -2
#com 543
#end

#selectevent 2540
#rarity 0
#req_rare 8
#req_code 179
#code 183
#end

#selectevent 2541
#rarity 0
#req_preach 25
#req_code 183
#code 184
#end

#selectevent 2542
#rarity 0
#req_rare 15
#req_code 183
#end

#selectevent 2543
#rarity 0
#req_rare 80
#req_targpath1 8
#req_code 183
#code 184
#end

#selectevent 2544
#rarity 0
#req_rare 30
#req_mintroops 35
#req_code 184
#code 0
#com 3274
#addequip 1
#4d6units 198
#3d6units 303
#end

#selectevent 2545
#rarity 0
#req_rare 20
#req_mintroops 35
#req_code 184
#code 0
#com 2538
#addequip 9
#com 811
#addequip 1
#end

#selectevent 2546
#rarity 0
#req_rare 2
#req_commander 1
#req_targgod 0
#req_unluck 2
#req_code 180
#banished -12
#fireboost 1
#end

#selectevent 2547
#rarity 0
#req_rare 1
#req_commander 1
#req_targgod 0
#req_code 181
#req_code 180
#banished -12
#fireboost 1
#end

#selectevent 2548
#rarity 0
#req_rare 1
#req_commander 1
#req_targgod 0
#req_code 181
#req_code 180
#banished -13
#waterboost 1
#end

#selectevent 2549
#rarity 0
#req_rare 1
#req_commander 1
#req_targgod 0
#req_code 181
#req_code 180
#banished -11
#astralboost 1
#end

#selectevent 2550
#rarity 2
#req_turn 25
#req_story 1
#req_chaos -2
#req_code -49
#code 179
#flagland 1
#end

#selectevent 2551
#rarity 0
#req_preach 4
#req_temple 0
#req_dominion 5
#req_code 179
#code 0
#end

#selectevent 2552
#rarity 0
#req_preach 4
#req_temple 1
#req_dominion 5
#req_code 180
#code 0
#end

#selectevent 2553
#rarity 0
#req_land 1
#req_rare 1
#req_capital 0
#req_story 1
#req_code -49
#code 186
#flagland 1
#incscale2 0
#decscale 5
#end

#selectevent 2554
#rarity 0
#req_rare 7
#req_nomnr 900
#req_code 186
#code 0
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#com 900
#addequip 9
#addequip 1
#3d6units 526
#end

#selectevent 2555
#rarity 0
#req_rare 3
#req_code 186
#code 0
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#3d6units 304
#9d6units 303
#end

#selectevent 2556
#rarity 0
#req_rare 80
#req_mintroops 30
#req_code 186
#code 0
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#6d6units 303
#end

#selectevent 2557
#rarity 0
#req_mintroops 7
#req_maxtroops 29
#req_code 186
#end

#selectevent 2558
#rarity 0
#req_preach 25
#req_code 186
#code 0
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#nation -2
#com -13
#addequip 1
#1d6units 1560
#1d3units 2227
#4d6units 1565
#end

#selectevent 2559
#rarity 0
#req_preach 1
#req_unique 3
#req_order 2
#req_code 186
#nation -2
#com 543
#end

#selectevent 2560
#rarity 0
#req_nomnr 900
#req_targpath1 8
#req_rare 15
#req_code 186
#code 0
#com 900
#addequip 1
#addequip 9
#6d6units 526
#nation -2
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#end

#selectevent 2561
#rarity 0
#req_nomnr 827
#req_targpath1 8
#req_rare 15
#req_code 186
#code 0
#com 827
#addequip 1
#addequip 9
#6d6units 526
#nation -2
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#end

#selectevent 2562
#rarity 0
#req_targpath1 8
#req_rare 15
#req_code 186
#code 0
#4com 304
#12d6units 2286
#nation -2
#com 2540
#bloodboost 1
#bloodboost 1
#addequip 1
#end

#selectevent 2563
#rarity 0
#req_targpath2 8
#req_rare 15
#req_nomonster 828
#req_code 186
#code 0
#nation -2
#com 828
#com 95
#addequip 1
#end

#selectevent 2564
#rarity 0
#req_targpath2 8
#req_rare 15
#req_code 186
#code 0
#nation -2
#4com 88
#com 95
#addequip 1
#end

#selectevent 2565
#rarity 2
#req_land 1
#req_unique 2
#req_capital 0
#req_turn 20
#req_story 1
#req_code 0
#code 186
#flagland 1
#incscale2 0
#decscale 5
#end

#selectevent 2566
#rarity 0
#req_rare 3
#req_freesites 1
#req_unique 1
#req_code 180
#addsite -1
#end

#selectevent 2567
#rarity -1
#req_code 0
#req_freesites 1
#req_unique 1
#req_anycode -44
#code -49
#addsite -1
#end

#selectevent 2568
#rarity 0
#req_rare 1
#req_unique 3
#req_farm 1
#req_capital 0
#req_code -49
#code 0
#addsite -1
#kill 20
#incscale2 0
#unrest 100
#end

#selectevent 2569
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 818
#banished -13
#end

#selectevent 2570
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 819
#banished -13
#end

#selectevent 2571
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 820
#banished -13
#end

#selectevent 2572
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 821
#banished -12
#end

#selectevent 2573
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 822
#banished -12
#end

#selectevent 2574
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 823
#banished -12
#end

#selectevent 2575
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 824
#banished -12
#end

#selectevent 2576
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 825
#banished -12
#end

#selectevent 2577
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_foundsite 1
#banished -13
#pathboost 4
#pathboost 0
#fireboost 1
#end

#selectevent 2578
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_foundsite 1
#banished -12
#pathboost 4
#pathboost 2
#waterboost 1
#end

#selectevent 2579
#rarity 1
#req_targgod 0
#req_targdemon 0
#req_monster 1405
#banished -12
#end

#selectevent 2580
#rarity 1
#req_targgod 0
#req_targdemon 0
#req_monster 305
#banished -12
#end

#selectevent 2581
#rarity 1
#req_targgod 0
#req_targdemon 0
#req_monster 826
#banished -12
#end

#selectevent 2582
#rarity 1
#req_targgod 0
#req_targdemon 0
#req_monster 827
#banished -12
#end

#selectevent 2583
#rarity 1
#req_targgod 0
#req_targdemon 0
#req_monster 828
#banished -12
#end

#selectevent 2584
#rarity 1
#req_targgod 0
#req_targdemon 0
#req_monster 829
#banished -12
#end

#selectevent 2585
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 305
#gainaff 8
#pathboost 0
#end

#selectevent 2586
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 826
#gainaff 8
#gainaff 1
#end

#selectevent 2587
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 827
#gainaff 8
#gainaff 1
#end

#selectevent 2588
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 828
#gainaff 8
#gainaff 1
#end

#selectevent 2589
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 829
#gainaff 8
#gainaff 1
#end

#selectevent 2590
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 829
#req_targhumanoid 1
#gainaff 1073741824
#end

#selectevent 2591
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 826
#req_targhumanoid 1
#gainaff 1073741824
#end

#selectevent 2592
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 827
#req_targhumanoid 1
#gainaff 1073741824
#end

#selectevent 2593
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 828
#req_targhumanoid 1
#gainaff 1073741824
#end

#selectevent 2594
#rarity 2
#req_targpath1 53
#req_targgod 0
#req_monster 829
#req_targhumanoid 1
#gainaff 1073741824
#end

#selectevent 2595
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 818
#gainaff 524288
#gainaff 524288
#bloodboost 1
#end

#selectevent 2596
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 819
#gainaff 524288
#gainaff 524288
#bloodboost 1
#end

#selectevent 2597
#rarity 1
#req_targpath1 53
#req_targgod 0
#req_monster 820
#gainaff 524288
#gainaff 524288
#bloodboost 1
#end

#selectevent 2598
#rarity 0
#req_rare 5
#req_foundsite 1
#req_unique 2
#req_luck 0
#req_code 0
#code -61
#flagland 1
#end

#selectevent 2599
#rarity 0
#req_rare 25
#req_targpath1 53
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -14
-- ro: effect 190 (gold) = 100
#magicitem 1
#magicitem 2
#end

#selectevent 2600
#rarity 0
#req_rare 75
#req_targmnr 2323
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -1
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2601
#rarity 0
#req_rare 75
#req_targmnr 2324
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin 2130
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2602
#rarity 0
#req_rare 75
#req_targmnr 2325
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -9
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2603
#rarity 0
#req_rare 75
#req_targmnr 2326
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -12
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2604
#rarity 0
#req_rare 75
#req_targmnr 2327
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -1
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2605
#rarity 0
#req_rare 75
#req_targmnr 2328
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin 2130
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2606
#rarity 0
#req_rare 75
#req_targmnr 2329
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -9
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2607
#rarity 0
#req_rare 75
#req_targmnr 2330
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin -12
-- ro: effect 190 (gold) = 150
#magicitem 1
#magicitem 2
#end

#selectevent 2608
#rarity 0
#req_rare 75
#req_targmnr 2332
#req_code -61
#code 0
-- ro: effect 39 = 2
#assassin 2329
#addequip 1
#addequip 2
#end

#selectevent 2609
#rarity 0
#req_rare 7
#req_foundsite 1
#req_unique 4
#req_luck 0
#req_code 0
#code -61
#flagland 1
#end

#selectevent 2610
#rarity 0
#req_rare 7
#req_foundsite 1
#req_unique 4
#req_luck 0
#req_code 0
#code -61
#flagland 1
#end

#selectevent 2611
#rarity 0
#req_targpath1 53
#req_code -61
-- ro: requirement 140 (default) = 1
#end

#selectevent 2612
#rarity -1
#req_fornation 57
#req_fornation 100
#req_fornation 61
#req_rare 2
#req_unique 2
#req_fort 0
#req_land 1
#req_code 0
#code -76
#delay 1
#flagland 1
#end

#selectevent 2613
#rarity 0
#code -77
#unrest 4
#end

#selectevent 2614
#rarity 0
#req_rare 75
#req_maxtroops 20
#req_code -77
#code -78
#unrest 10
#end

#selectevent 2615
#rarity 0
#req_rare 20
#req_maxtroops 20
#req_code -78
#req_code -77
#unrest 15
#taxboost -20
#end

#selectevent 2616
#rarity 0
#req_rare 10
#req_unique 1
#req_code -78
#req_code -77
#unrest 5
#decscale 5
#end

#selectevent 2617
#rarity 0
#req_mintroops 10
#req_targorder 3
#req_code -78
#req_code -77
#code -78
#end

#selectevent 2618
#rarity 0
#req_maxtroops 9
#req_targorder 3
#req_rare 80
#req_code -78
#req_code -77
#code -79
-- ro: effect 39 = 2
#assassin 628
#end

#selectevent 2619
#rarity 0
#req_maxtroops 9
#req_targorder 3
#req_rare 15
#req_code -78
#req_code -77
#code -80
-- ro: effect 39 = 2
#assassin -10
#end

#selectevent 2620
#rarity 0
#req_commander 1
#req_code -79
#code 0
-- ro: effect 190 (gold) = 10
#magicitem 1
#end

#selectevent 2621
#rarity 0
#req_commander 1
#req_code -80
#code 0
-- ro: effect 190 (gold) = 50
#magicitem 2
#end

#selectevent 2622
#rarity 0
#req_commander 0
#req_code -79
#req_code -80
#code 0
#end

#selectevent 2623
#rarity 0
#req_maxtroops 9
#req_targpath2 6
#req_rare 50
#req_code -78
#req_code -77
#code 0
#nation -2
#1unit 628
#end

#selectevent 2624
#rarity 1
#req_foundsite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin -1
#end

#selectevent 2625
#rarity 1
#req_foundsite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin -2
#end

#selectevent 2626
#rarity 1
#req_foundsite 1
#req_commander 1
-- ro: effect 39 = 2
#assassin -4
#end

#selectevent 2627
#rarity 1
#req_story 1
#req_turn 12
#req_capital 0
#req_unique 1
#req_freesites 2
#req_waste 1
#req_code 0
#code -62
#incscale2 3
#unrest 2
#end

#selectevent 2628
#rarity 0
#req_rare 15
#req_maxtroops 30
#req_maxdef 17
#req_unique 1
#req_code -62
#code -63
#com 998
#6d6units 533
-- ro: effect 183 (24d6units) = -2
#2com -3
#addsite -1
#end

#selectevent 2629
#rarity 0
#req_indepok
#req_pop0ok
#req_unique 1
#req_code -63
#code -64
#com 299
#addequip 1
#addequip 9
#deathboost 1
-- ro: effect 177 (18d6units) = -2
#emigration 80
#kill 99
#end

#selectevent 2630
#rarity 0
#req_rare 20
#req_targmnr 299
#req_indepok
#req_unique 3
#req_code -64
#deathboost 1
#addequip 1
#end

#selectevent 2631
#rarity 0
#req_rare 15
#req_pop0ok
#req_targmnr 299
#req_indepok
#req_unique 2
#req_code -64
#2com 190
-- ro: effect 177 (18d6units) = -2
#end

#selectevent 2632
#rarity 0
#req_story 2
#req_pop0ok
#req_rare 15
#req_targmnr 299
#req_indepok
#req_unique 2
#req_code -64
#com 1541
#3d6units -4
#1d6units 533
#end

#selectevent 2633
#rarity 0
#req_story 2
#req_targmnr 299
#req_pop0ok
#req_indepok
#req_unique 1
#req_code -64
#addequip 9
#end

#selectevent 2634
#rarity 13
#req_nomonster 299
#req_code -64
#req_indepok
#req_pop0ok
#code 0
#resetcode -63
#resetcode -64
#resetcode -65
#resetcode -66
#end

#selectevent 2635
#rarity 0
#req_rare 10
#req_minpop 15
#req_capital 0
#req_land 1
#req_code 0
#req_nearbycode -64
#code -65
#incscale3 3
#incdom -2
#unrest 5
#end

#selectevent 2636
#rarity 0
#req_rare 10
#req_capital 0
#req_land 1
#req_code 0
#req_nearbycode -64
#code -65
#incscale3 3
#incdom -2
#unrest 5
#end

#selectevent 2637
#rarity 0
#req_rare 10
#req_capital 0
#req_land 1
#req_code 0
#req_nearbycode -64
#code -65
#incscale3 3
#incdom -2
#unrest 5
#end

#selectevent 2638
#rarity 0
#req_preach 25
#req_unique 3
#req_code -65
#code 0
#end

#selectevent 2639
#rarity 0
#req_rare 70
#req_dominion 7
#req_unique 3
#req_code -65
#code 0
#end

#selectevent 2640
#rarity 0
#req_rare 70
#req_temple 1
#req_unique 3
#req_code -65
#code 0
#incdom 2
#end

#selectevent 2641
#rarity 0
#req_rare 15
#req_unique 2
#req_code -65
#code 0
#2com 190
#6d6units -15
#6d6units -2
#1d3units 405
#end

#selectevent 2642
#rarity 0
#req_rare 15
#req_unique 2
#req_code -65
#kill 30
#incscale2 3
#end

#selectevent 2643
#rarity 0
#req_rare 30
#req_code -66
#req_anycode -64
#kill 10
#com 998
#3d6units 533
#15d6units -2
#4com 190
#com -3
#end

#selectevent 2644
#rarity 0
#req_rare 10
#req_code -65
#kill 10
#com 998
#3d6units 533
#15d6units -2
#4com 190
#end

#selectevent 2645
#rarity 0
#req_rare 10
#req_land 0
#req_temple 0
#req_nearbycode -65
#req_code 0
#req_unique 6
#code -65
#incscale2 3
#unrest 5
#decscale2 5
#end

#selectevent 2646
#rarity 0
#req_land 1
#req_anycode -64
#req_indepok
#req_capital 0
#req_rare 15
#req_freesites 1
#req_unique 4
#req_minpop 15
#req_code -65
#code -66
#incscale3 3
#emigration 80
#kill 99
#addsite -1
#end

#selectevent 2647
#rarity 13
#req_unique 4
#req_targmnr 299
#req_code -64
#req_anycode -66
#req_indepok
#resetcode -66
#waterboost 1
#deathboost 1
#end

#selectevent 2648
#rarity 0
#req_rare 5
#req_unique 3
#req_targgod 0
#req_temple 0
#req_code -65
#assassin -3
#end

#selectevent 2649
#rarity 0
#req_targgod 0
#req_unique 1
#req_foundsite 1
#req_targpath1 5
#req_targorder 7
-- ro: effect 39 = 2
#assassin 533
#end

#selectevent 2650
#rarity 0
#req_targgod 0
#req_unique 1
#req_foundsite 1
#req_targpath1 5
#req_targorder 7
#addequip 2
-- ro: effect 39 = 2
#assassin 533
#end

#selectevent 2651
#rarity 0
#req_rare 30
#req_hiddensite 1
#req_monster 381
#revealsite
#end

#selectevent 2652
#rarity 0
#req_targgod 0
#req_targorder 107
#req_targpath2 3
#req_rare 15
#req_code 157
#addequip 2
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 2653
#rarity 0
#req_targgod 0
#req_targorder 107
#req_targpath2 1
#req_rare 15
#req_code 157
#addequip 2
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 2654
#rarity 0
#req_targgod 0
#req_maxtroops 12
#req_targorder 2
#req_rare 20
#req_code 157
#addequip 2
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 2655
#rarity 0
#req_targgod 0
#req_maxtroops 12
#req_targorder 2
#req_rare 20
#req_code 157
#addequip 2
#end

#selectevent 2656
#rarity 0
#req_targgod 0
#req_maxtroops 12
#req_targpath2 3
#req_rare 15
#req_code 157
-- ro: effect 39 = 2
#assassin 2526
#end

#selectevent 2657
#rarity 0
#req_targgod 0
#req_maxtroops 12
#req_targpath2 1
#req_rare 15
#req_code 157
-- ro: effect 39 = 2
#assassin 2526
#end

#selectevent 2658
#rarity 0
#req_turn 40
#req_nearbysite 1
#req_fornation 61
#req_unique 1
#req_rare 25
#req_pop0ok
#req_land 1
#nation -2
#com 464
#1d6units 1367
#incdom 4
#end

#selectevent 2659
#rarity 0
#req_site 1
#req_claimedthrone
#req_growth 1
#req_rare 10
#req_code -25
#code 0
#nation -2
#com 237
#end

#selectevent 2660
#rarity 0
#req_rare 2
#req_code 0
#req_anycode -25
#notext
#code -26
#end

#selectevent 2661
#rarity 0
#req_rare 15
#req_code -37
#resetcode -36
#resetcode -37
#unrest -2
#decscale 3
#end

#selectevent 2662
#rarity 0
#req_anycode 189
#req_indepok
#req_rare 15
#req_unique 5
#req_targmnr 492
#bloodboost 1
#addequip 1
#incscale2 0
#decscale2 5
#end

#selectevent 2663
#rarity 2
#req_story 1
#req_fort 0
#req_unique 1
#req_forest 1
#req_code 0
#code 195
#unrest 2
#end

#selectevent 2664
#rarity 0
#req_commander 1
#req_unique 1
#req_mintroops 5
#req_code 195
#code 196
#end

#selectevent 2665
#rarity 0
#req_targorder 3
#req_unique 1
#req_mindef 1
#req_mintroops 5
#req_code 196
#code 197
#defence -8
#end

#selectevent 2666
#rarity 0
#req_targorder 3
#req_unique 1
#req_code 197
#com 609
#addequip 1
#4d6units 394
#3d6units -10
#3d6units 1565
#end

#selectevent 2667
#rarity 0
#req_nomonster 609
#req_unique 1
#req_code 198
#req_code 197
#resetcode 200
#resetcode 199
#code 0
-- ro: effect 190 (gold) = 300
#2d6vis 5
#2d6vis 6
#magicitem 2
#end

#selectevent 2668
#rarity 0
#req_rare 30
#req_targpath1 53
#req_targgod 0
#req_code 196
#req_code 195
#end

#selectevent 2669
#rarity 0
#req_rare 50
#req_unique 1
#req_code 197
#req_code 196
#req_code 195
#unrest 5
#end

#selectevent 2670
#rarity 0
#req_rare 25
#req_unique 1
#req_code 197
#req_code 196
#req_code 195
#landgold -3
#unrest 5
#end

#selectevent 2671
#rarity 0
#req_rare 15
#req_unique 1
#req_code 197
#req_code 196
#req_code 195
#unrest 10
#end

#selectevent 2672
#rarity 0
#req_rare 15
#req_unique 1
#req_code 197
#req_code 196
#req_code 195
#unrest 10
#end

#selectevent 2673
#rarity 0
#req_rare 7
#req_unique 3
#req_targorder 3
#req_code 196
#req_code 197
-- ro: effect 39 = 2
#assassin -10
#end

#selectevent 2674
#rarity 0
#req_rare 10
#req_unique 3
#req_targorder 3
#req_code 196
#req_code 197
-- ro: effect 39 = 2
#assassin -11
#end

#selectevent 2675
#rarity 0
#req_rare 10
#req_unique 3
#req_targorder 3
#req_code 196
#req_code 197
-- ro: effect 39 = 2
#assassin 403
#end

#selectevent 2676
#rarity 0
#req_unique 2
#req_rare 15
#req_mintroops 5
#req_targorder 3
#req_code 196
#req_code 197
#com 403
#3d6units 403
#end

#selectevent 2677
#rarity 0
#req_unique 1
#req_targorder 3
#req_rare 15
#req_mintroops 5
#req_freesites 1
#req_code 196
#req_code 197
#addsite -1
#end

#selectevent 2678
#rarity 0
#req_unique 2
#req_rare 15
#req_mintroops 5
#req_targorder 3
#req_code 196
#req_code 197
#2com -10
#1d3units -10
#3d6units -11
#3d6units 715
#1d3units 718
#end

#selectevent 2679
#rarity 0
#req_rare 25
#req_unique 1
#req_code 195
#req_code 196
#code 198
#com 609
#addequip 1
#4com 394
#gainaff 549755813888
#4d6units -10
-- ro: effect 177 (18d6units) = -11
#1d3units 718
#3d6units 715
#end

#selectevent 2680
#rarity 0
#req_rare 50
#req_unique 1
#req_monster 609
#req_indepok
#req_code 198
#com 488
#addequip 9
#end

#selectevent 2681
#rarity 0
#req_rare 25
#req_unique 1
#req_targmnr 609
#req_indepok
#req_code 198
#1d6units -10
#4d6units -10
#addequip 1
#end

#selectevent 2682
#rarity 0
#req_indepok
#req_rare 25
#req_unique 1
#req_targmnr 609
#req_nearbycode 199
#req_code 198
#addequip 9
#addequip 1
#end

#selectevent 2683
#rarity 0
#req_rare 15
#req_unique 1
#req_targmnr 609
#req_indepok
#req_code 198
#1d6units 526
#addequip 1
#end

#selectevent 2684
#rarity 0
#req_rare 25
#req_unique 1
#req_targmnr 609
#req_indepok
#req_code 198
#addequip 9
#addequip 1
#end

#selectevent 2685
#rarity 0
#req_rare 25
#req_unique 1
#req_targmnr 609
#req_indepok
#req_code 198
#addequip 9
#addequip 1
#end

#selectevent 2686
#rarity 0
#req_land 1
#req_rare 25
#req_indepok
#req_nearbycode 198
#req_code 0
#code 199
#unrest 2
#end

#selectevent 2687
#rarity 0
#req_land 1
#req_rare 20
#req_indepok
#req_nearbycode 198
#req_code 0
#code 200
#end

#selectevent 2688
#rarity 0
#req_indepok
#req_unique 4
#req_targmnr 609
#req_nearbycode 200
#req_code 198
#1d6units -10
#3d6units -10
#addequip 1
#resetcode 200
#end

#selectevent 2689
#rarity 0
#req_indepok
#req_unique 2
#req_targmnr 609
#req_nearbycode 199
#req_code 198
#bloodboost 1
#natureboost 1
#resetcode 199
#end

#selectevent 2690
#rarity 0
#req_rare 25
#req_indepok
#req_unique 2
#req_targmnr 609
#req_targpath3 8
#req_code 198
#3d6units 526
#end

#selectevent 2691
#rarity 0
#req_indepok
#req_unique 1
#req_targmnr 609
#req_targpath3 6
#req_code 198
#1d6units 330
#end

#selectevent 2692
#rarity 0
#req_indepok
#req_unique 1
#req_targmnr 609
#req_targpath4 6
#req_code 198
#1d6units 330
#end

#selectevent 2693
#rarity 0
#req_monster 1663
#req_nomonster 1286
#req_nation 24
#req_story 1
#req_rare 10
#req_turn 8
#req_owncapital 1
#req_unique 1
#req_code 0
#code 201
#unrest 10
#delay25 4
#flagland 1
#end

#selectevent 2694
#rarity 0
#req_nation 24
#req_code 201
#code 202
#incscale2 0
#delay50 6
#flagland 0
#end

#selectevent 2695
#rarity 0
#req_unique 1
#req_fort 0
#req_code 0
#req_anycode 202
#req_land 1
#code 203
#com 2618
#addequip 1
#7d6units 1707
#7d6units 1278
#end

#selectevent 2696
#rarity 0
#req_rare 20
#req_unique 1
#req_code 202
#req_nation 24
#req_code 201
#unrest 10
#end

#selectevent 2697
#rarity 0
#req_monster 1286
#req_code 201
#code 0
#unrest -20
#incdom 1
#end

#selectevent 2698
#rarity 0
#req_rare 15
#req_unique 1
#req_nation 24
#req_code 202
#nation -2
#3d6units 1285
#end

#selectevent 2699
#rarity 0
#req_monster 1286
#req_code 202
#unrest -20
#end

#selectevent 2700
#rarity 0
#req_rare 10
#req_site 0
#req_unique 4
#req_nation 24
#req_nearbycode 202
#req_land 1
#taxboost -30
#unrest 15
#end

#selectevent 2701
#rarity 0
#req_rare 15
#req_site 0
#req_unique 2
#req_nation 24
#req_anycode 202
#unrest 10
#end

#selectevent 2702
#rarity 0
#req_rare 15
#req_targmnr 1284
#req_unique 1
#req_code 202
#assassin 676
#end

#selectevent 2703
#rarity 0
#req_rare 15
#req_targmnr 202
#req_unique 1
#req_code 202
#assassin 676
#end

#selectevent 2704
#rarity 0
#req_rare 5
#req_gem 5
#req_unique 1
#req_code 202
#gemloss 5
#end

#selectevent 2705
#rarity 0
#req_rare 10
#req_unique 1
#req_nation 24
#req_owncapital 1
#req_anycode 203
#magicitem 9
#end

#selectevent 2706
#rarity 0
#req_rare 15
#req_unique 4
#req_anycode 202
#req_code 203
#notext
#6d6units 1707
#3d6units 1278
#2com 1278
#end

#selectevent 2707
#rarity 0
#req_rare 25
#req_site 0
#req_unique 5
#req_nearbycode 203
#taxboost -30
#unrest 15
#end

#selectevent 2708
#rarity 0
#req_rare 15
#req_unique 3
#req_fort 0
#req_nearbycode 203
#req_land 1
#2com 1278
#addequip 1
#6d6units 1707
#6d6units 1278
#end

#selectevent 2709
#rarity 0
#req_nomonster 2618
#req_code 203
#code 0
#resetcode 202
#end

#selectevent 2710
#rarity 0
#req_rare 15
#req_unique 1
#req_anycode 203
#req_nation 24
#req_code 202
#nation -2
#com 1288
-- ro: effect 177 (18d6units) = 1287
#end

#selectevent 2711
#rarity 0
#req_rare 5
#req_unique 1
#req_anycode 203
#req_nation 71
#req_code 202
#nation -2
#3d6units 1289
#end

#selectevent 2712
#rarity 0
#req_rare 1
#req_turn 8
#req_land 1
#req_code 0
#req_anycode -20
#notext
#code -33
#incscale2 4
#unrest 3
#end

#selectevent 2713
#rarity 0
#req_rare 3
#req_turn 8
#req_land 0
#req_code 0
#req_anycode -20
#notext
#code -34
#incscale2 4
#unrest 3
#end

#selectevent 2714
#rarity 1
#req_unique 1
#req_foundsite 1
#req_land 1
#com 843
#4com 676
#6d6units 442
#6d6units 676
#6d6units 675
#end

#selectevent 2715
#rarity 1
#req_unique 1
#req_foundsite 1
#req_land 1
#req_targgod 0
#req_targhumanoid 1
#assassin 442
#end

#selectevent 2716
#rarity -1
#req_foundsite 1
#req_targmale 1
#req_targgod 0
#gainaff 549755813888
#end

#selectevent 2717
#rarity -1
#req_foundsite 1
#req_targmale 1
#req_targgod 0
#gainaff 549755813888
#end

#selectevent 2718
#rarity 0
#req_foundsite 1
#req_targpath1 9
#req_targgod 0
#req_unique 1
#gainaff 549755813888
#end

#selectevent 2719
#rarity -1
#req_foundsite 1
#req_targmale 1
#req_targgod 0
#gainaff 549755813888
#end

#selectevent 2720
#rarity 0
#req_targpath2 8
#req_targorder 7
#req_targgod 0
#req_unique 1
#req_site 1
#pathboost 8
#astralboost 1
#gainaff 4096
#gainaff 549755813888
#end

#selectevent 2721
#rarity 0
#req_rare 0
#req_freesites 1
#hiddensite -1
#end

#selectevent 2722
#rarity -1
#req_foundsite 1
#req_targgod 0
#gainaff 549755813888
#end

#selectevent 2723
#rarity -1
#req_ench 57
#req_targgod 0
#gainaff 549755813888
#end

#selectevent 2724
#rarity -1
-- ro: requirement 22 (req_siege) = 0
#req_unique 1
#req_targpath1 53
#req_foundsite 1
#fireboost 1
#end

#selectevent 2725
#rarity -1
-- ro: requirement 22 (req_siege) = 0
#req_unique 1
#req_targpath1 53
#req_foundsite 1
#natureboost 1
#end

#selectevent 2726
#rarity -1
-- ro: requirement 22 (req_siege) = 0
#req_unique 1
#req_targpath1 53
#req_foundsite 1
#deathboost 1
#end

#selectevent 2727
#rarity -1
-- ro: requirement 22 (req_siege) = 0
#req_unique 1
#req_targpath1 53
#req_foundsite 1
#astralboost 1
#end

#selectevent 2728
#rarity -1
-- ro: requirement 22 (req_siege) = 0
#req_foundsite 1
#req_unique 1
#req_targpath1 53
#astralboost 1
#gainaff 549755813888
-- ro: effect 39 = 3
#assassin -6
#gainmark
#end

#selectevent 2729
#rarity 2
#req_turn 15
#req_story 2
#req_noera 1
#req_unique 1
#req_mountain 1
#req_cold 1
#req_indepok
#req_code 0
#req_freesites 1
#req_nonation 66
#code 211
#hiddensite -1
#notext
#end

#selectevent 2730
#rarity 0
#req_capital 0
#req_mountain 0
#req_indepok
#req_unique 1
#req_land 1
#req_freesites 3
#req_nearbycode 211
#req_code 0
#code 212
#hiddensite -1
#notext
#end

#selectevent 2731
#rarity 0
#req_foundsite 1
#req_unique 1
#flagland 1
#end

#selectevent 2732
#rarity 0
#req_rare 35
#req_targorder 50
#req_unique 1
#req_freesites 1
#req_code 212
#code 213
#addsite -1
#end

#selectevent 2733
#rarity 0
#req_unique 1
#req_luck 1
#req_foundsite 1
#req_rare 4
#nation -2
#com 478
#end

#selectevent 2734
#rarity 0
#req_unique 1
#req_luck 2
#req_foundsite 1
#req_rare 4
#nation -2
#com 478
#com 479
#addequip 1
#end

#selectevent 2735
#rarity 0
#req_unique 1
#req_targorder 4
#req_targmnr 478
#req_nomonster 479
#req_code 213
#code 214
#end

#selectevent 2736
#rarity 0
#req_unique 1
#req_targorder 4
#req_targmnr 479
#req_code 213
#code 214
#end

#selectevent 2737
#rarity 0
#req_unique 1
#req_targorder 4
#req_targpath1 55
#req_rare 15
#req_code 213
#code 214
#end

#selectevent 2738
#rarity 0
#req_unique 1
#req_targorder 4
#req_rare 50
#req_code 214
#code 215
#end

#selectevent 2739
#rarity 0
#req_unique 1
#req_targorder 7
#req_code 211
#req_nearowncode 215
#code 217
#flagland 1
#end

#selectevent 2740
#rarity 0
#req_unique 1
#req_targorder 7
#req_code 211
#req_nearowncode 216
#code 217
#end

#selectevent 2741
#rarity 0
#req_unique 1
#req_targmnr 478
#req_code 217
#code 218
#order 3
#addsite -1
#end

#selectevent 2742
#rarity 0
#req_unique 1
#req_targmnr 479
#req_code 217
#code 218
#order 3
#addsite -1
#end

#selectevent 2743
#rarity 0
#req_unique 1
#req_targpath2 2
#req_code 217
#code 218
#order 3
#addsite -1
#end

#selectevent 2744
#rarity 0
#req_unique 1
#req_targpath2 3
#req_code 217
#code 218
#order 3
#addsite -1
#end

#selectevent 2745
#rarity 0
#req_unique 1
#req_targorder 100
#req_notforally 80
#req_nearowncode 216
#req_code 218
#com 913
#addequip 1
#com 553
#addequip 1
#2com 283
#9d6units 841
-- ro: effect 177 (18d6units) = 541
#end

#selectevent 2746
#rarity 0
#req_unique 1
#req_targorder 101
#req_notforally 80
#req_nearowncode 216
#req_code 218
#com 913
#addequip 1
#com 553
#addequip 1
#2com 283
#9d6units 841
-- ro: effect 177 (18d6units) = 541
#end

#selectevent 2747
#rarity 0
#req_unique 1
#req_targorder 101
#req_code 218
#magicitem 9
#magicitem 3
#magicitem 2
#2d6vis 2
-- ro: effect 190 (gold) = 600
#resetcode 215
#resetcode 216
#end

#selectevent 2748
#rarity 0
#req_unique 1
#req_targorder 100
#req_code 218
#code 219
#order 272
#end

#selectevent 2749
#rarity 0
#req_unique 1
#req_targorder 104
#req_code 219
#resetcode 215
#resetcode 216
#code 0
#magicitem 9
#magicitem 3
#2d6vis 2
-- ro: effect 190 (gold) = 600
#end

#selectevent 2750
#rarity 0
#req_unique 1
#req_rare 50
#req_targorder 108
#req_code 219
#nation -2
#code 0
#resetcode 215
#resetcode 216
#com 844
#addequip 9
#addequip 1
#end

#selectevent 2751
#rarity 0
#req_unique 1
#req_rare 50
#req_targorder 108
#req_notforally 80
#req_code 219
#order 0
#code 220
#resetcode 215
#resetcode 216
#com 844
#addequip 9
#addequip 1
#3d6units 3746
#3d6units 3748
#3d6units 3750
#end

#selectevent 2752
#rarity 0
#req_unique 1
#req_nomnr 844
#req_code 220
#code 0
-- ro: effect 190 (gold) = 600
#end

#selectevent 2753
#rarity 0
#req_unique 1
#req_monster 844
#req_indepok
#req_code 220
#1d6units 3746
#1d6units 3748
#3d6units 3750
#end

#selectevent 2754
#rarity 0
#req_unique 1
#req_rare 50
#req_targorder 108
#req_nation 80
#req_code 219
#order 0
#code 221
#resetcode 215
#resetcode 216
#nation 80
#com 844
#addequip 9
#addequip 1
#3d6units 3746
#3d6units 3748
#3d6units 3750
#end

#selectevent 2755
#rarity 0
#req_unique 1
#req_monster 844
#req_fornation 80
#req_code 221
#code 0
#end

#selectevent 2756
#rarity 0
#req_unique 1
#req_nomnr 844
#req_code 221
#code 0
#notext
#end

#selectevent 2757
#rarity 0
#req_unique 1
#req_targorder 4
#req_targmnr 251
#req_code 213
#code 214
#end

#selectevent 2758
#rarity 0
#req_rare 50
#req_unique 1
#req_code 215
#code 216
#end

#selectevent 2759
#rarity 0
#req_unique 1
#req_targorder 4
#req_freesites 1
#req_code 214
#code 225
#end

#selectevent 2760
#rarity 0
#req_unique 1
#req_targorder 7
#req_rare 70
#req_code 225
#req_freesites 1
#addsite -1
#code 214
#4com 1976
#gainaff 549755813888
#12d6units 1976
#12d6units 1976
#end

#selectevent 2761
#rarity 0
#req_rare 5
#req_targorder 50
#req_unique 1
#req_code 213
#req_code 214
#magicitem 9
#magicitem 9
#magicitem 9
#end

#selectevent 2762
#rarity 0
#req_rare 5
#req_targorder 50
#req_unique 1
#req_code 214
#req_code 212
#2d6vis 2
#end

#selectevent 2763
#rarity 0
#req_rare 5
#req_targorder 50
#req_unique 1
#req_code 213
#req_code 212
#magicitem 9
#end

#selectevent 2764
#rarity 0
#req_rare 4
#req_targorder 50
#req_targgod 0
#req_code 214
#req_code 212
-- ro: effect 39 = 2
#assassin -14
#end

#selectevent 2765
#rarity 0
#req_rare 4
#req_targorder 50
#req_targgod 0
#req_code 215
#req_code 213
-- ro: effect 39 = 2
#assassin -5
#end

#selectevent 2766
#rarity 0
#req_rare 10
#req_targorder 50
#req_targgod 0
#req_code 216
#req_code 212
-- ro: effect 39 = 2
#assassin 316
#end

#selectevent 2767
#rarity 0
#req_rare 10
#req_targorder 50
#req_targgod 0
#req_code 214
#req_code 213
-- ro: effect 39 = 2
#assassin 316
#end

#selectevent 2768
#rarity 0
#req_rare 10
#req_targorder 50
#req_targgod 0
#req_code 215
#req_code 213
-- ro: effect 39 = 2
#assassin 1976
#end

#selectevent 2769
#rarity 0
#req_rare 10
#req_targorder 4
#req_targgod 0
#req_unique 1
#req_code 213
-- ro: effect 39 = 2
#assassin 913
#end

#selectevent 2770
#rarity 0
#req_rare 90
#req_targorder 4
#req_targgod 0
#req_unique 1
#req_code 214
-- ro: effect 39 = 2
#assassin 913
#end

#selectevent 2771
#rarity 0
#req_rare 30
#req_unique 1
#req_code 213
#req_code 214
#req_code 215
#stealthcom 786
#notext
#end

#selectevent 2772
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2773
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2774
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2775
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2776
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2777
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2778
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2779
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2780
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2781
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2782
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2783
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2784
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2785
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2786
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2787
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2788
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2789
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2790
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2791
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2792
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2793
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2794
#rarity 0
#req_rare 0
#req_land 1
#req_freesites 1
#hiddensite -1
#end

#selectevent 2795
#rarity 0
#req_rare 0
#revealsite
#end

#selectevent 2796
#rarity 0
#req_unique 1
#req_hiddensite 1
#req_code 0
#code -99
#notext
#end

#selectevent 2797
#rarity 0
#req_unique 1
#req_foundsite 1
#req_code -99
#req_nopathwater 1
#flagland 1
#code 253
#end

#selectevent 2798
#rarity 0
#req_unique 1
#req_foundsite 1
#req_targpath1 2
#req_code 253
#code 226
#order 273
#delay25 8
#end

#selectevent 2799
#rarity 0
#req_foundsite 1
#req_indepok
#req_code 226
#code 0
#order 0
#end

#selectevent 2800
#rarity 0
#req_unique 1
#req_foundsite 1
#req_targpath1 2
#req_code -99
#code 226
#flagland 1
#order 273
#delay25 8
#end

#selectevent 2801
#rarity 0
#req_foundsite 1
#req_indepok
#req_code 226
#code 0
#order 0
#end

#selectevent 2802
#rarity 0
#req_unique 1
#req_foundsite 1
#req_targpath1 2
#req_targorder 100
#req_code 226
#end

#selectevent 2803
#rarity 0
#req_unique 1
#req_foundsite 1
#req_targpath1 2
#req_targorder 104
#req_code 226
#code 0
#2d6vis 2
#end

#selectevent 2804
#rarity 0
#req_rare 13
#req_unique 3
#req_foundsite 1
#req_targpath1 2
#req_targorder 108
#req_code 226
-- ro: effect 39 = 2
#assassin 3735
#end

#selectevent 2805
#rarity 0
#req_rare 25
#req_unique 8
#req_foundsite 1
#req_targpath1 2
#req_targorder 108
#req_code 226
#nation -2
#1unit 3735
#end

#selectevent 2806
#rarity 0
#req_rare 10
#req_foundsite 1
#req_targpath1 2
#req_targorder 108
#req_code 226
-- ro: effect 39 = 2
#assassin 3733
#end

#selectevent 2807
#rarity 0
#req_rare 25
#req_unique 3
#req_foundsite 1
#req_targpath3 2
#req_targorder 108
#req_code 226
#nation -2
#1unit 3733
#end

#selectevent 2808
#rarity 0
#req_rare 10
#req_unique 2
#req_foundsite 1
#req_targpath2 2
#req_targorder 108
#req_code 226
-- ro: effect 39 = 2
#assassin 3730
#end

#selectevent 2809
#rarity 0
#req_rare 25
#req_unique 2
#req_foundsite 1
#req_targpath3 2
#req_targorder 108
#req_code 226
#nation -2
#1unit 3730
#end

#selectevent 2810
#rarity 0
#req_luck 1
#req_rare 12
#req_unique 1
#req_foundsite 1
#req_targpath4 2
#req_targorder 108
#req_code 226
#nation -2
#3d6units 3730
#end

#selectevent 2811
#rarity 0
#req_luck 1
#req_rare 15
#req_unique 1
#req_foundsite 1
#req_targpath3 2
#req_targorder 108
#req_code 226
#nation -2
#3d6units 3733
#end

#selectevent 2812
#rarity 0
#req_rare 15
#req_unique 1
#req_foundsite 1
#req_targpath3 2
#req_targorder 108
#req_code 226
#nation -2
#3d6units 3733
#end

#selectevent 2813
#rarity 0
#req_luck 1
#req_rare 10
#req_unique 1
#req_foundsite 1
#req_targpath2 2
#req_targorder 108
#req_code 226
#nation -2
#3d6units 3735
#end

#selectevent 2814
#rarity 0
#req_rare 15
#req_unique 3
#req_targpath2 2
#req_targorder 108
#req_code 226
#magicitem 9
#end

#selectevent 2815
#rarity 0
#req_rare 6
#req_unique 1
#req_foundsite 1
#req_targpath2 2
#req_targorder 108
#req_code 226
#pathboost 2
#end

#selectevent 2816
#rarity 0
#req_rare 0
#req_land 1
#id 15
#unrest 50
#nation 2
#com 1912
#12d6units 482
#tempunits 1
#com 2635
#5d6units 2630
#kill 5
#end

#selectevent 2817
#rarity 0
#req_rare 10
#req_code -21
#req_land 1
#code 0
#nation -2
#3d6units 633
#end

#selectevent 2818
#rarity 1
#req_growth -2
#req_land 0
#incscale3 4
#curse 10
#end

#selectevent 2819
#rarity 1
#req_coast 1
#req_noseason 3
#req_noseason 2  -- stored twice; a second #req_noseason in a mod replaces the first
#req_turn 10
#req_unique 4
#2com 1064
#9d6units 1064
#end

#selectevent 2820
#rarity 2
#req_story 1
#req_turn 8
#req_coast 1
#req_noseason 3
#req_noseason 2  -- stored twice; a second #req_noseason in a mod replaces the first
#req_unique 1
#req_code 0
#flagland 1
#code 227
#order 1
#unrest 10
#end

#selectevent 2821
#rarity 0
#req_code 227
#code 228
#order 0
#end

#selectevent 2822
#rarity 0
#req_rare 20
#req_targorder 3
#req_code 227
#code 0
#com 1409
#addequip 9
#15d6units 1064
#end

#selectevent 2823
#rarity 0
#req_targorder 3
#req_rare 20
#req_unique 1
#req_code 228
-- ro: effect 190 (gold) = 400
#magicitem 1
#magicitem 1
#end

#selectevent 2824
#rarity 0
#req_targorder 3
#req_rare 20
#req_unique 2
#req_code 228
#2d4vis 4
#end

#selectevent 2825
#rarity 0
#req_rare 70
#req_code 228
#req_targorder 3
#code 0
#com 1409
#addequip 9
#9d6units 1064
#end

#selectevent 2826
#rarity 0
#req_season 3
#req_code 228
#req_code 227
#code 0
#end

#selectevent 2827
#rarity 0
#req_rare 15
#req_noseason 3
#req_unique 3
#req_code 227
#req_code 228
#unrest 15
#end

#selectevent 2828
#rarity 2
#req_coast 1
#req_unique 1
#req_noseason 3
#com 1409
#addequip 9
#15d6units 1064
#end

#selectevent 2829
#rarity 0
#req_code 227
#req_code 228
#req_unique 1
#req_coast 1
#req_unique 1  -- stored twice; a second #req_unique in a mod replaces the first
#req_noseason 3
#com 1409
#addequip 9
#15d6units 1064
#code 0
#end

#selectevent 2830
#rarity 2
#req_story 1
#req_coast 1
#req_notforally 44
#req_notforally 89  -- stored twice; a second #req_notforally in a mod replaces the first
#req_freesites 2
#req_code 0
#code 229
#order 1
#flagland 1
#end

#selectevent 2831
#rarity 0
#req_rare 10
#req_code 231
#req_targorder 100
#req_code 230
#req_code 229
#assassin 1576
#end

#selectevent 2832
#rarity 0
#req_rare 80
#req_targorder 100
#req_code 229
#code 230
#incdom -3
#end

#selectevent 2833
#rarity 0
#req_rare 15
#req_unique 2
#req_targorder 100
#req_code 231
#req_code 230
-- ro: effect 39 = 2
#assassin 757
#end

#selectevent 2834
#rarity 0
#req_rare 80
#req_unique 1
#req_targorder 100
#req_code 230
#code 231
#order 33
#addsite -1
#end

#selectevent 2835
#rarity 0
#req_rare 90
#req_unique 1
#req_targorder 100
#req_code 231
#gainaff 2097152
-- ro: effect 39 = 2
#assassin 757
#end

#selectevent 2836
#rarity 0
#req_targorder 105
#req_mintroops 10
#req_unique 1
#req_code 231
#code 233
#order 36
#delay25 4
#end

#selectevent 2837
#rarity 0
#req_unique 1
#req_code 233
#code 0
#com 332
#addequip 1
#4d6units 757
#com 751
#1d6units 970
#1d6units 968
#com 970
#gainaff 549755813888
#1d6units 969
#3d6units 1565
#1d6units 962
#end

#selectevent 2838
#rarity 0
#req_code 235
#req_targorder 105
#req_code 232
#req_code 233
#req_code 231
#code 0
#com 1575
#4d6units 972
#3d6units 1576
#3d6units 757
#1d6units 967
#com 1575
#1d6units 968
#1d6units 970
#3d6units 1565
#3d6units 962
#end

#selectevent 2839
#rarity 0
#req_targorder 102
#req_code 233
#code 0
#addsite -1
#unrest -10
#incdom -7
#end

#selectevent 2840
#rarity 0
#req_preach 15
#req_dominion 5
#req_code 233
#req_code 232
#req_code 231
#code 0
#incdom 3
#temple 1
#end

#selectevent 2841
#rarity 0
#req_rare 10
#req_code 232
#req_code 230
#req_code 231
#assassin 1576
#end

#selectevent 2842
#rarity 0
#req_rare 10
#req_code 231
#req_code 232
#code 0
#com 332
#addequip 1
#4d6units 757
#com 751
#1d6units 967
#3d6units 968
#1d6units 969
#com 1565
#gainaff 549755813888
#3d6units 1565
#3d6units 962
#end

#selectevent 2843
#rarity 0
#req_rare 10
#req_site 1
#req_targorder 50
#emigration 1
#incdom -3
#unrest 3
#end

#selectevent 2844
#rarity 0
#req_rare 10
#req_commander 1
#req_code 229
#assassin 1576
#end

#selectevent 2845
#rarity 0
#req_preach 100
#req_unique 1
#req_maxdominion 4
#req_code 232
#req_code 231
#incdom -3
#end

#selectevent 2846
#rarity 0
#req_targorder 100
#req_targpath1 55
#req_targgod 0
#req_code 231
#code 232
#order 44
#gainaff 549755813888
#end

#selectevent 2847
#rarity 0
#req_targorder 102
#req_unique 1
#req_code 232
#code 234
#nation -2
#1unit 751
#incdom -7
#end

#selectevent 2848
#rarity 0
#req_unique 1
#req_code 234
#code 0
#incdom -2
#unrest 15
#kill 1
#end

#selectevent 2849
#rarity 0
#req_targorder 102
#req_unique 1
#req_code 232
#code 234
#nation -2
#1unit 753
#incdom -7
#order 0
#end

#selectevent 2850
#rarity 0
#req_targorder 102
#req_unique 1
#req_code 232
#code 234
#nation -2
#1unit 754
#incdom -7
#order 0
#end

#selectevent 2851
#rarity 0
#req_targorder 103
#req_unique 1
#req_code 232
-- ro: effect 39 = 2
#assassin 332
#addequip 1
#addequip 9
#code 235
#order 32
#delay 2
#end

#selectevent 2852
#rarity 0
#req_unique 1
#req_code 235
#code 0
#com 332
#addequip 1
#4d6units 757
#com 751
#1d6units 967
#1d6units 968
#com 967
#gainaff 549755813888
#3d6units 969
#3d6units 1565
#3d6units 962
#end

#selectevent 2853
#rarity 0
#req_code 102
#req_targorder 3
#req_mintroops 15
#req_rare 20
#code 103
#end

#selectevent 2854
#rarity 0
#req_code 102
#req_minunrest 30
#req_poptype 20
#defence -20
#4com 141
#15d6units 140
#15d6units 139
#code 0
#end

#selectevent 2855
#rarity -1
#req_season 0
#req_land 0
#unrest -50
#decscale3 3
#decscale2 4
#end

#selectevent 2856
#rarity 0
#req_story 1
#req_land 1
#req_unique 1
#req_site 1
#req_unclaimedthrone
#req_code 0
#code 236
#order 1
#flagland 1
#end

#selectevent 2857
#rarity 0
#req_targorder 100
#req_unique 1
#req_code 236
#code 237
#end

#selectevent 2858
#rarity 0
#req_claimedthrone
#req_code 239
#req_code 240
#req_code 241
#req_code 242
#req_code 236
#req_code 237
#code 244
#end

#selectevent 2859
#rarity 0
#req_claimedthrone
#req_targgod 1
#req_targpath4 4
#req_rare 33
#req_code 237
#code 0
#end

#selectevent 2860
#rarity 0
#req_targorder 100
#req_code 239
#code 238
#end

#selectevent 2861
#rarity 0
#req_targorder 100
#req_targpath1 53
#req_unique 1
#req_code 244
#req_code 237
#code 239
#order 256
#end

#selectevent 2862
#rarity 0
#req_targorder 108
#req_targpath2 1
#req_code 239
#code 240
#2d4vis 1
#end

#selectevent 2863
#rarity 0
#req_season 0
#req_unique 1
#req_code 242
#req_code 241
#req_code 238
#req_code 240
#code 244
#order 0
#end

#selectevent 2864
#rarity 0
#req_targorder 108
#req_targpath2 0
#req_code 240
#code 241
#2d4vis 0
#end

#selectevent 2865
#rarity 0
#req_targorder 108
#req_targpath2 3
#req_code 241
#code 242
#2d4vis 3
#end

#selectevent 2866
#rarity 0
#req_targorder 108
#req_targpath2 2
#req_code 242
#code 243
#2d4vis 2
#end

#selectevent 2867
#rarity 0
#req_code 243
#code 0
#nation -2
#com 93
#pathboost 1
#com 99
#pathboost 0
#com 312
#pathboost 3
#pathboost 3
#com 309
#pathboost 2
#end

#selectevent 2868
#rarity 0
#req_rare 90
#req_season 0
#req_unique 1
#req_code 244
#kill 2
#taxboost -100
#com 93
#pathboost 1
#pathboost 1
#addequip 1
#addequip 9
#4com 92
#pathboost 1
#addequip 1
#3d6units 520
#6d6units 513
#1d3units 3722
#1d3units 3724
#1d3units 3726
#3d6units 513
#end

#selectevent 2869
#rarity 0
#req_season 1
#req_unique 1
#req_indepok
#req_code 244
#kill 1
#unrest 10
#taxboost -50
#decscale2 2
#com 99
#pathboost 0
#pathboost 0
#addequip 1
#addequip 9
#4com 98
#pathboost 0
#addequip 1
#3d6units 515
#1d3units 814
#1d3units 3716
#1d3units 3717
#1d3units 3718
#1d3units 523
#1d6units 527
#end

#selectevent 2870
#rarity 0
#req_season 2
#req_unique 1
#req_indepok
#req_code 244
#kill 1
#taxboost -100
#com 312
#pathboost 3
#pathboost 3
#pathboost 3
#addequip 1
#1d6units 512
#1d6units 522
#1d3units 3738
#1d3units 3740
#1d6units 3741
#1d6units 2526
#2com 561
#com 447
#addequip 9
#com 447
#addequip 9
#end

#selectevent 2871
#rarity 0
#req_season 3
#req_unique 1
#req_indepok
#req_code 244
#code 245
#kill 1
#incscale3 2
#com 309
#pathboost 2
#pathboost 2
#addequip 1
#4com 309
#pathboost 2
#addequip 1
#1d3units 579
#7d6units 511
#1d3units 3746
#1d3units 3748
#1d3units 3750
#2com 2231
#addequip 9
#end

#selectevent 2872
#rarity 0
#req_unique 1
#req_code 245
#code 0
#end

#selectevent 2873
#rarity 0
#req_claimedthrone
#req_freesites 1
#req_land 1
#req_site 1
#req_story 1
#req_unique 1
#req_code 0
#code 246
#notext
#end

#selectevent 2874
#rarity 0
#req_rare 5
#req_land 1
#req_turn 15
#req_anycode 246
#req_targgod 1
#req_targpath2 5
#req_unique 1
#req_dominion 4
#req_death 2
#nation -2
#com 310
#gainaff 549755813888
#gainaff 262144
#end

#selectevent 2875
#rarity 0
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 246
#code 247
#end

#selectevent 2876
#rarity 0
#req_lab 1
#req_targmnr 310
#req_targaff 549755813888
#req_unique 1
#req_code 247
#code 248
#pathboost 5
#end

#selectevent 2877
#rarity 0
#req_lab 1
#req_targmnr 310
#req_targaff 549755813888
#req_unique 1
#req_code 248
#code 249
#gainaff 2
#end

#selectevent 2878
#rarity 13
#req_unique 1
#req_code 249
#worlddarkness
#worldincscale 3
#end

#selectevent 2879
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 249
#nation -2
#3d6units -2
#end

#selectevent 2880
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 249
#nation -2
#4d6units -15
#end

#selectevent 2881
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 249
#nation -2
#com 190
#end

#selectevent 2882
#rarity 0
#req_rare 33
#req_targmnr 310
#req_targaff 549755813888
#req_unique 1
#req_code 249
#addsite -1
#delay50 4
#end

#selectevent 2883
#rarity 0
#req_targmnr 310
#req_targaff 549755813888
#req_unique 1
#req_code 249
#code 250
#pathboost 5
#end

#selectevent 2884
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_unique 2
#req_code 250
#nation -2
#com 1541
#end

#selectevent 2885
#rarity 0
#req_rare 15
#req_targmnr 310
#req_targaff 549755813888
#req_unique 3
#req_code 250
#nation -2
#1unit 566
#end

#selectevent 2886
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_unique 2
#req_code 250
#nation -2
#1d3units 405
#end

#selectevent 2887
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 249
#nation -2
#com 190
#6d6units -2
#end

#selectevent 2888
#rarity 0
#req_rare 5
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 249
#nation -2
#3d6units 189
#end

#selectevent 2889
#rarity 0
#req_rare 10
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_code 249
#nation -2
#3d6units 676
#end

#selectevent 2890
#rarity 0
#req_rare 8
#req_targmnr 310
#req_targaff 549755813888
#req_targaff 262144  -- stored twice; a second #req_targaff in a mod replaces the first
#req_anycode 249
#resetcode 249
#gainaff 33554432
#end

#selectevent 2891
#rarity 0
#req_rare 5
#req_targmnr 310
#req_targaff 549755813888
#req_unique 1
#req_anycode 249
#kill 85
-- ro: effect 177 (18d6units) = 674
#6d6units 674
#end

#selectevent 2892
#rarity 0
#req_rare 5
#req_monster 310
#req_code 249
#assassin 676
#end

#selectevent 2893
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2894
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2895
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2896
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2897
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2898
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2899
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2900
#rarity 0
#req_rare 0
#req_land 1
#addsite -1
#end

#selectevent 2901
#rarity -2
#req_temple 0
#req_targgod 1
#req_dominion 3
#req_unique 2
#temple 1
#incdom 3
#end

#selectevent 2902
#rarity -1
#req_temple 0
#req_targgod 1
#req_dominion 7
#req_unique 2
#temple 1
#incdom 3
#end

#selectevent 2903
#rarity 0
#req_rare 0
#hiddensite -1
#end

#selectevent 2904
#rarity 0
#req_unique 1
#req_story 1
#req_site 1
#req_code 0
#req_waste 1
#req_targorder 7
#req_targpath1 5
#code 251
#com 1979
#com 1978
#addequip 1
#6d6units 1980
#6d6units 1981
#decscale 1
#end

#selectevent 2905
#rarity 0
#req_rare 25
#req_indepok
#req_targmnr 1978
#req_unique 5
#req_code 251
#1d6units 1980
#1d6units 1981
#end

#selectevent 2906
#rarity 0
#req_rare 30
#req_indepok
#req_targmnr 1979
#req_unique 1
#req_code 251
#addequip 9
#end

#selectevent 2907
#rarity 0
#req_rare 30
#req_indepok
#req_targmnr 1979
#req_unique 1
#req_code 251
#addequip 9
#end

#selectevent 2908
#rarity 0
#req_rare 30
#req_indepok
#req_targmnr 1979
#req_unique 1
#req_code 251
#addequip 9
#end

#selectevent 2909
#rarity 0
#req_rare 25
#req_indepok
#req_targmnr 1978
#req_unique 1
#req_code 251
#addequip 9
#end

#selectevent 2910
#rarity 0
#req_rare 25
#req_indepok
#req_targmnr 1978
#req_unique 3
#req_code 251
#addequip 1
#end

#selectevent 2911
#rarity 0
#req_rare 15
#req_indepok
#req_targmnr 1978
#req_unique 2
#req_code 251
#pathboost 0
#end

#selectevent 2912
#rarity 0
#req_rare 15
#req_indepok
#req_targmnr 1978
#req_unique 3
#req_code 251
#3d6units 2233
#1d6units 524
#end

#selectevent 2913
#rarity 0
#req_nomnr 1978
#req_nomnr 1979
#req_indepok
#req_code 251
#code 0
-- ro: effect 190 (gold) = 400
#1d6vis 0
#magicitem 9
#end

#selectevent 2914
#rarity 0
#req_story 2
#req_rare 3
#req_land 1
#req_anycode -21
#req_code 0
#code -95
#unrest 3
#end

#selectevent 2915
#rarity 2
#req_story 2
#req_land 1
#req_code 0
#code -95
#unrest 3
#end

#selectevent 2916
#rarity 0
#req_rare 15
#req_code -95
#code 0
#notext
#end

#selectevent 2917
#rarity 0
#req_story 2
#req_rare 50
#req_land 1
#req_code -95
#2com 633
#9d6units 633
#code 0
#end

#selectevent 2918
#rarity 0
#req_story 2
#req_rare 30
#req_land 1
#req_code -95
#stealthcom 633
#6d6units 633
#unrest 5
#code 0
#end

#selectevent 2919
#rarity 0
#req_rare 2
#req_land 1
#req_code -95
#2com 633
#3d6units 633
#code 0
#end

#selectevent 2920
#rarity 0
#req_rare 20
#req_land 1
#req_code -95
#code 0
#stealthcom 633
#3d6units 633
#unrest 5
#end

#selectevent 2921
#rarity 0
#req_rare 30
#req_land 1
#req_code -95
#code 0
#4com 1769
#3d6units 1560
#3d6units 1560
#end

#selectevent 2922
#rarity 0
#req_rare 20
#req_land 1
#req_commander 1
#req_code -95
-- ro: effect 39 = 2
#assassin 1560
#end

#selectevent 2923
#rarity 0
#req_rare 30
#req_mindef 1
#req_maxtroops 8
#req_forest 1
#req_code -95
#4com 284
#15d6units 284
#nation -2
#com 1565
#6d6units 1565
#end

#selectevent 2924
#rarity 1
#req_targaff 549755813888
#gainaff 2
#end

#selectevent 2925
#rarity 1
#req_targaff 549755813888
#gainaff 8589934592
#end

#selectevent 2926
#rarity 2
#req_targaff 549755813888
#com -7
#com -6
#end

#selectevent 2927
#rarity 1
#req_targorder 8
#req_targgod 0
#gainmark
#end

#selectevent 2928
#rarity 1
#req_land 1
#req_targorder 8
#req_targgod 0
#com 34
#6d6units 18
#4d6units 17
#12d6units 1565
#com 240
#end

#selectevent 2929
#rarity 1
#req_targorder 8
#req_targgod 0
-- ro: effect 39 = 2
#assassin 1565
#end

#selectevent 2930
#rarity 1
#req_targorder 8
#req_targgod 0
#assassin 428
#end

#selectevent 2931
#rarity -1
#req_targorder 8
#req_targgod 0
#2d6vis 8
#end

#selectevent 2932
#rarity -1
#req_targorder 8
#req_targgod 0
#2d6vis 8
#end

#selectevent 2933
#rarity 1
#req_targorder 8
#req_targgod 0
#gainaff 2
#curse 3
#end

#selectevent 2934
#rarity 1
#req_targorder 8
#req_targgod 0
#req_targmale 1
#gemloss 8
-- ro: effect 39 = 2
#assassin 88
#end

#selectevent 2935
#rarity -1
#req_targorder 8
#req_targgod 0
#addequip 9
#end

#selectevent 2936
#rarity -1
#req_targorder 8
#req_targgod 0
-- ro: effect 190 (gold) = 100
#1d3vis 8
#end

#selectevent 2937
#rarity -2
#req_targorder 8
#req_targgod 0
#pathboost 8
#end

#selectevent 2938
#rarity -2
#req_targorder 8
#req_targgod 0
#req_gem 8
#req_luck 1
#addequip 9
#end

#selectevent 2939
#rarity 1
#req_chaos 2
#req_unluck 1
#req_minunrest 15
#req_minpop 200
#req_turn 15
#req_capital 1
#unrest 25
#kill 10
#emigration 15
#end

#selectevent 2940
#rarity 1
#req_season 1
#req_minpop 20
#req_land 0
#decscale 3
#decscale 4
#incscale3 1
#taxboost -100
#end

#selectevent 2941
#rarity 12
#req_code 0
#code -97
#delay50 4
#worldunrest 4
#end

#selectevent 2942
#rarity 0
#resetcode -97
#resetcode -95
#resetcode -96
#notext
#end

#selectevent 2943
#rarity 0
#req_rare 0
#req_land 1
#req_anycode -97
#req_code 0
#code -96
#unrest 5
#end

#selectevent 2944
#rarity 0
#req_rare 2
#req_land 1
#req_anycode -97
#req_code 0
#code -95
#unrest 5
#end

#selectevent 2945
#rarity 0
#req_rare 1
#req_land 1
#req_anycode -97
#req_code 0
#code -96
#notext
#end

#selectevent 2946
#rarity 0
#req_rare 2
#req_forest 1
#req_unique 1
#req_anycode -97
#com 1596
#addequip 9
#gainaff 549755813888
#4d6units 124
#4d6units 633
#end

#selectevent 2947
#rarity -1
#req_growth 1
#req_forest 1
#req_unique 1
#req_anycode -97
#req_chaos 0
#nation -2
#com 1596
#addequip 9
#gainaff 549755813888
#7d6units 124
#3d6units 633
#end

#selectevent 2948
#rarity 0
#req_story 1
#req_land 1
#req_freesites 2
#req_rare 2
#req_unique 1
#req_nearbysite 1
#req_nation 105
#req_code 0
#code 254
#order 17
#flagland 1
#end

#selectevent 2949
#rarity 0
#req_code 256
#req_targorder 104
#req_code 257
#req_code 255
#req_code 254
#req_code 258
#req_code 259
#code 0
#end

#selectevent 2950
#rarity 0
#req_targorder 100
#req_code 254
#code 255
#end

#selectevent 2951
#rarity 0
#req_unique 1
#req_code 257
#req_code 256
#req_targorder 100
#req_code 255
#end

#selectevent 2952
#rarity 0
#req_unique 1
#req_targpath2 0
#req_code 255
#order 272
#end

#selectevent 2953
#rarity 0
#req_targpath2 0
#req_targgod 0
#req_targorder 108
#req_cold -1
#req_code 255
#end

#selectevent 2954
#rarity 0
#req_targpath2 0
#req_targgod 1
#req_targorder 108
#req_cold -1
#req_code 255
#end

#selectevent 2955
#rarity 0
#req_targpath2 0
#req_unique 1
#req_targorder 108
#req_heat 2
#req_code 255
#code 256
#end

#selectevent 2956
#rarity 0
#req_targpath2 0
#req_targorder 108
#req_rare 25
#req_code 255
-- ro: effect 39 = 2
#assassin 3735
#end

#selectevent 2957
#rarity 0
#req_targpath2 0
#req_targorder 108
#req_rare 25
#req_code 256
-- ro: effect 39 = 2
#assassin 3735
#end

#selectevent 2958
#rarity 0
#req_targpath2 0
#req_targgod 0
#req_targorder 108
#req_rare 25
#req_code 256
#code 257
#order 19
#end

#selectevent 2959
#rarity 0
#req_unique 1
#req_code 258
#req_targorder 100
#req_rare 25
#req_code 257
-- ro: effect 190 (gold) = 350
#1d6vis 2
#1d6vis 0
#magicitem 9
#end

#selectevent 2960
#rarity 0
#req_unique 1
#req_targorder 100
#req_rare 25
#req_code 257
#magicitem 9
#code 258
#end

#selectevent 2961
#rarity 0
#req_unique 1
#req_targorder 100
#req_rare 25
#req_code 258
#magicitem 9
#end

#selectevent 2962
#rarity 0
#req_unique 1
#req_targorder 100
#req_rare 25
#req_code 258
#magicitem 9
#end

#selectevent 2963
#rarity 0
#req_unique 1
#req_code 258
#req_targorder 100
#req_rare 25
#req_code 257
#2com 2526
#3d6units 2526
#code 258
#end

#selectevent 2964
#rarity 0
#req_unique 1
#req_targorder 100
#req_rare 25
#req_code 257
#2com 447
#addequip 9
#1d6units 447
#code 258
#end

#selectevent 2965
#rarity 0
#req_unique 1
#req_targorder 100
#req_rare 25
#req_code 258
#2com 2492
#addequip 9
#3d6units 2492
#end

#selectevent 2966
#rarity 0
#req_unique 1
#req_targorder 100
#req_rare 25
#req_code 258
#code 259
#addsite -1
#order 19
#end

#selectevent 2967
#rarity 0
#req_unique 1
#req_targorder 101
#req_code 258
#req_code 257
#code 259
#addsite -1
#order 19
#end

#selectevent 2968
#rarity 0
#req_unique 1
#req_luck 1
#req_targorder 100
#req_rare 20
#req_code 259
#magicitem 9
#2d4vis 0
-- ro: effect 190 (gold) = 150
#magicitem 2
#end

#selectevent 2969
#rarity 0
#req_unique 1
#req_targorder 101
#req_rare 25
#req_code 257
#2com 447
#addequip 9
#1d6units 447
#code 258
#end

#selectevent 2970
#rarity 0
#req_unique 1
#req_pathholy 1
#req_targorder 101
#req_code 259
#code 260
#temple 1
#addsite -1
#order 1
#end

#selectevent 2971
#rarity 0
#req_unique 1
#req_nopathholy 1
#req_targorder 101
#req_code 259
#end

#selectevent 2972
#rarity 0
#req_unique 1
#req_targpath1 53
#req_rare 20
#req_code 260
#req_code 259
#end

#selectevent 2973
#rarity 0
#req_unique 1
#req_targpath1 53
#req_targorder 100
#req_code 259
#req_code 260
#end

#selectevent 2974
#rarity 0
#req_unique 1
#req_targorder 50
#req_code 260
#code 261
#gainaff 549755813888
#decscale2 2
#incdom -2
#end

#selectevent 2975
#rarity 0
#req_unique 1
#req_rare 10
#req_targorder 50
#req_nomnr 909
#req_code 261
#com 909
#addequip 9
#addequip 1
#code 262
#9d6units 528
#9d6units 3714
#end

#selectevent 2976
#rarity 0
#req_rare 25
#req_unique 3
#req_targmnr 909
#req_code 262
#notext
#addequip 1
#1d6units 258
#1d6units 3714
#end

#selectevent 2977
#rarity 0
#req_unique 4
#req_targorder 50
#req_code 261
#decscale2 2
#incdom -2
#notext
#end

#selectevent 2978
#rarity 0
#req_nomonster 909
#req_code 262
#code 0
#end

#selectevent 2979
#rarity 0
#req_rare 5
#req_luck 2
#req_code 255
#req_unique 1
#nation -2
#com 99
#addequip 9
#end

#selectevent 2980
#rarity 0
#req_site 1
#req_targorder 7
#req_targpath1 8
#req_story 2
#req_code 0
#code 271
#incscale2 0
#decscale2 5
#order 257
#revealsite
#end

#selectevent 2981
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_unique 1
#req_code 271
#incscale2 0
#bloodboost 1
#gainaff 549755813888
#code -45
#bloodboost 1
#order 0
#end

#selectevent 2982
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_rare 30
#req_unique 1
#req_code 271
#incscale2 0
#bloodboost 1
#gainaff 549755813888
#code -45
#banished -12
#order 0
#end

#selectevent 2983
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_rare 25
#req_unique 1
#req_code 271
-- ro: effect 39 = 2
#assassin 366
#end

#selectevent 2984
#rarity 0
#req_targpath1 53
#req_targorder 100
#req_code 272
#req_code 271
#req_unique 1
#end

#selectevent 2985
#rarity 0
#req_targpath1 53
#req_targorder 100
#req_rare 70
#req_unique 1
#req_code 271
#code 272
#end

#selectevent 2986
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_targgod 0
#req_unique 1
#req_code 272
-- ro: requirement 118 = 0
#pathboost 0
#bloodboost 1
#gainaff 549755813888
#code -45
#bloodboost 1
#pathboost 0
#end

#selectevent 2987
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_targgod 0
#req_rare 40
#req_code 272
#pathboost 0
#bloodboost 1
#banished -12
#code -45
#bloodboost 1
#order 0
#gainaff 549755813888
#end

#selectevent 2988
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_targgod 1
#req_unique 1
#req_code 272
#pathboost 0
#bloodboost 1
#gainaff 549755813888
#code 273
#bloodboost 1
#order 0
#end

#selectevent 2989
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_targgod 1
#req_rare 20
#req_code 272
#pathboost 0
#bloodboost 1
#banished -12
#code 273
#bloodboost 1
#order 0
#end

#selectevent 2990
#rarity 0
#req_targpath1 8
#req_targorder 108
#req_rare 33
#req_unique 1
#req_code 272
#nation -2
#pathboost 0
#bloodboost 1
#gainaff 549755813888
#code 273
#com 489
#addequip 9
#4d6units 489
#order 0
#end

#selectevent 2991
#rarity 10
#req_unique 1
#req_story 1
#req_anycode 273
#req_rare 33
#req_code 0
#code -19
#code2 -44
#worldincscale2 0
#end

#selectevent 2992
#rarity 13
#req_rare 80
#req_foundsite 1
#req_unique 1
#req_code 273
#code -45
#revealprov
#end

#selectevent 2993
#rarity 13
#req_foundsite 1
#req_unique 1
#req_code 271
#end

#selectevent 2994
#rarity 0
#req_rare 77
#req_code 273
#req_indepok
#code -45
#notext
#end

#selectevent 2995
#rarity 0
#req_rare 0
#req_code 0
#hiddensite -1
#end

#selectevent 2996
#rarity 0
#req_freesites 1
#req_maxturn 1
#req_rare 15
#req_indepok
#req_poptype 41
#req_unique 1
#req_code 0
#req_capital 0
#req_land 1
#req_story 2
#code 275
#notext
#addsite -1
#end

#selectevent 2997
#rarity 0
#req_unique 1
#req_code 275
#code 0
#resetcode 276
#resetcode 277
#resetcode 278
-- ro: effect 190 (gold) = 80
#3d6vis 8
#end

#selectevent 2998
#rarity 0
#req_indepok
#req_unique 1
#req_code 275
#com 350
#15d6units 351
#6d6units 612
#2com 349
#addequip 1
#notext
#end

#selectevent 2999
#rarity 0
#req_indepok
#req_targmnr 350
#req_unique 1
#req_code 275
#fireboost 350
#fireboost 350
#bloodboost 350
#addequip 1
#gainaff 549755813888
#notext
#end

#selectevent 3000
#rarity 0
#req_indepok
#req_targaff 549755813888
#req_targmnr 350
#req_unique 1
#req_code 275
#addequip 9
#bloodboost 350
#addequip 1
#notext
#end

#selectevent 3001
#rarity 0
#req_rare 10
#req_turn 30
#req_targaff 549755813888
#req_targmnr 350
#req_indepok
#req_unique 3
#addequip 1
#end

#selectevent 3002
#rarity 0
#req_rare 20
#req_targaff 549755813888
#req_targmnr 350
#req_unique 1
#req_code 275
#req_indepok
#addequip 9
#notext
#end

#selectevent 3003
#rarity 0
#req_rare 10
#req_notcode 275
#req_nearbycode 275
#req_capital 0
#req_land 1
#req_code 0
#req_indepok
#req_unique 8
#code 276
#end

#selectevent 3004
#rarity 0
#req_rare 10
#req_nearbycode 275
#req_notcode 275
#req_capital 0
#req_land 1
#req_code 0
#req_indepok
#code 276
#end

#selectevent 3005
#rarity 0
#req_rare 50
#req_anycode 275
#req_indepok
#req_unique 9
#req_code 276
#code 277
#2com 349
#addequip 1
#9d6units 351
#3d6units 612
#end

#selectevent 3006
#rarity 0
#req_rare 8
#req_anycode 275
#req_indepok
#req_unique 6
#req_code 277
#com 349
#addequip 1
#7d6units 351
#3d6units 304
#3d6units 303
#end

#selectevent 3007
#rarity 0
#req_rare 7
#req_anycode 275
#req_indepok
#req_unique 6
#req_code 277
#nation -2
#com 304
#addequip 9
#3d6units 303
#incscale2 0
#end

#selectevent 3008
#rarity 0
#req_anycode 275
#req_unique 2
#req_code 277
#code 278
#end

#selectevent 3009
#rarity 0
#req_rare 33
#req_unique 2
#req_nearbycode 278
#req_land 1
#resetcode 278
#com 304
#7d6units 303
#end

#selectevent 3010
#rarity 0
#req_unique 2
#req_code 277
#code 0
-- ro: effect 190 (gold) = 100
#magicitem 1
#1d3vis 0
#end

#selectevent 3011
#rarity 0
#req_rare 5
#req_anycode 275
#req_indepok
#req_unique 1
#req_code 275
#nation -2
#com 304
#addequip 9
#3d6units 303
#incscale2 0
#end

#selectevent 3012
#rarity 0
#req_rare 10
#req_anycode 275
#req_indepok
#req_unique 1
#req_code 275
#addsite -1
#end

#selectevent 3013
#rarity 0
#req_rare 10
#req_anycode 275
#req_indepok
#req_unique 1
#req_code 276
#addsite -1
#end

#selectevent 3014
#rarity 0
#req_rare 20
#req_indepok
#req_unique 1
#req_code 275
#com 147
#addequip 9
#end

#selectevent 3015
#rarity 0
#req_targmnr 147
#req_indepok
#req_unique 1
#req_code 275
#addequip 9
#end

#selectevent 3016
#rarity 0
#req_poptype 71
#req_rare 15
#req_indepok
#req_maxturn 2
#req_code 0
#req_story 1
#code 279
#end

#selectevent 3017
#rarity 0
#req_unique 1
#req_indepok
#req_targmnr 519
#req_code 279
#natureboost 1
#addequip 9
#addequip 1
#com 1037
#4d6units 1037
#end

#selectevent 3018
#rarity 0
#req_unique 1
#req_indepok
#req_targmnr 519
#req_code 279
#addequip 9
#end

#selectevent 3019
#rarity 0
#req_unique 1
#req_indepok
#req_targmnr 1037
#req_code 279
#addequip 9
#end

#selectevent 3020
#rarity 0
#req_unique 1
#req_indepok
#req_targmnr 1037
#req_code 279
#addequip 9
#end

#selectevent 3021
#rarity 0
#req_code 279
#code 0
-- ro: effect 190 (gold) = 300
#magicitem 1
#magicitem 2
#1d6vis 3
#end

#selectevent 3022
#rarity 0
#req_rare 5
#req_unique 3
#req_land 1
#req_nearbycode 279
#unrest 5
#end

#selectevent 3023
#rarity 0
#req_rare 5
#req_unique 1
#req_turn 15
#req_land 1
#req_nearbycode 279
#req_fort 0
#unrest 5
#2com 1037
#addequip 9
#1d6units 1037
#4d6units 518
#end

#selectevent 3024
#rarity 0
#req_rare 5
#req_unique 4
#req_turn 15
#req_land 1
#req_nearbycode 279
#req_fort 0
#unrest 5
#2com 1037
#1d6units 1037
#3d6units 518
#end

#selectevent 3025
#rarity 0
#req_rare 1
#req_unluck -2
#req_targorder 45
#gainaff 4294967296
#curse 5
#end

#selectevent 3026
#rarity 0
#req_rare 1
#req_unluck -2
#req_targorder 45
#banished -11
#end

#selectevent 3027
#rarity 0
#req_rare 1
#req_luck 1
#req_targorder 44
-- ro: effect 190 (gold) = 200
#magicitem 1
#end

#selectevent 3028
#rarity -1
#req_unique 1
#req_land 0
#req_capital 0
#req_story 1
#req_code 0
#code 280
#order 33
#unrest 5
#flagland 1
#end

#selectevent 3029
#rarity 0
#req_targorder 3
#req_rare 30
#req_code 280
#code 296
#order 33
#end

#selectevent 3030
#rarity 0
#req_unique 1
#req_targorder 100
#req_code 296
#order 288
#end

#selectevent 3031
#rarity 0
#req_unique 1
#req_targorder 105
#req_code 296
#code 297
#com 438
#addequip 9
#end

#selectevent 3032
#rarity 0
#req_unique 1
#req_rare 10
#req_code 297
#code 0
#addsite -1
#end

#selectevent 3033
#rarity 0
#req_targorder 108
#req_rare 50
#req_targpath1 2
#req_code 296
#code 298
#delay25 8
#end

#selectevent 3034
#rarity 0
#req_unique 1
#req_indepok
#code 0
#end

#selectevent 3035
#rarity 0
#req_unique 1
#req_targorder 105
#req_code 296
#code 297
#com 438
#addequip 9
#end

#selectevent 3036
#rarity 0
#req_unique 6
#req_rare 25
#req_code 298
#nation -2
#1unit 438
#end

#selectevent 3037
#rarity 0
#req_unique 2
#req_rare 5
#req_code 298
#com 438
#end

#selectevent 3038
#rarity 0
#req_targmnr 147
#req_indepok
#req_unique 1
#req_code 275
#addequip 9
#end

#selectevent 3039
#rarity 0
#req_targmnr 304
#req_indepok
#req_unique 1
#req_code 275
#addequip 9
#end

#selectevent 3040
#rarity 0
#req_turn 20
#req_targmnr 349
#req_indepok
#req_unique 1
#req_code 275
#addequip 9
#end

#selectevent 3041
#rarity 0
#req_turn 30
#req_rare 8
#req_anycode 275
#req_indepok
#req_unique 6
#req_code 275
#4d6units 814
#3d6units 304
#3d6units 303
#end

#selectevent 3042
#rarity 0
#req_story 1
#req_rare 13
#req_hiddensite 1
#req_unique 1
#req_enchdom 42
#req_code 0
#revealsite
#incscale2 0
#code -93
#end

#selectevent 3043
#rarity 2
#req_land 1
#req_code 0
#req_maxdominion 7
#code -49
#incdom -3
#incscale2 0
#end

#selectevent 3044
#rarity 0
#req_rare 1
#req_land 1
#req_code 0
#req_anycode -44
#notext
#code -49
#incdom -3
#incscale2 0
#end

#selectevent 3045
#rarity 0
#req_rare 50
#req_site 1
#req_targorder 7
#req_targpath1 5
-- ro: effect 39 = 2
#assassin -2
#end

#selectevent 3046
#rarity 0
#req_unique 1
#req_site 1
#req_claimedthrone
#req_rare 2
#req_land 1
#nation -2
#com 95
#addequip 1
#gainaff 549755813888
#1d6units 433
#end

#selectevent 3047
#rarity 2
#req_forest 1
#req_targorder 7
#req_targgod 0
-- ro: effect 39 = 2
#assassin 518
#end

#selectevent 3048
#rarity 2
#req_forest 1
#req_targorder 7
#req_heat 0
#req_targgod 0
#req_growth 0
-- ro: effect 39 = 2
#assassin 592
#end

#selectevent 3049
#rarity 2
#req_forest 1
#req_targorder 7
#req_heat 0
#req_targgod 0
#req_growth 0
-- ro: effect 39 = 2
#assassin 361
#end

#selectevent 3050
#rarity 2
#req_forest 1
#req_targorder 7
#req_targgod 0
-- ro: effect 39 = 2
#assassin 284
#end

#selectevent 3051
#rarity 2
#req_mountain 1
#req_targorder 7
#req_targgod 0
-- ro: effect 39 = 2
#assassin -5
#end

#selectevent 3052
#rarity 2
#req_mountain 1
#req_targorder 7
#req_targgod 0
-- ro: effect 39 = 2
#assassin -14
#end

#selectevent 3053
#rarity 2
#req_mountain 1
#req_targorder 7
#req_targgod 0
-- ro: effect 39 = 2
#assassin 284
#end

#selectevent 3054
#rarity 2
#req_death 1
#req_targorder 7
#req_targgod 0
-- ro: effect 39 = 2
#assassin -15
#end

#selectevent 3055
#rarity 1
#req_site 1
#req_commander 1
#req_targgod 0
-- ro: effect 39 = 2
#assassin 527
#end

#selectevent 3056
#rarity -1
#req_foundsite 1
#req_order 1
#req_unique 1
#req_story 1
#req_code 0
#nation -2
#code 281
#com 93
#gainaff 549755813888
#end

#selectevent 3057
#rarity 0
#req_rare 20
#req_targaff 549755813888
#req_targmnr 93
#req_code 281
#code 282
#deathboost 1
#unrest 5
#end

#selectevent 3058
#rarity 0
#req_rare 20
#req_targpath1 5
#req_targaff 549755813888
#req_targmnr 93
#req_code 282
#nation -2
#1d6units 534
#end

#selectevent 3059
#rarity 0
#req_rare 10
#req_unique 1
#req_targaff 549755813888
#req_targmnr 93
#req_code 282
#magicitem 9
#end

#selectevent 3060
#rarity 0
#req_rare 10
#req_unique 1
#req_targaff 549755813888
#req_targmnr 93
#req_code 282
#magicitem 9
#end

#selectevent 3061
#rarity 0
#req_rare 25
#req_unique 3
#req_targpath1 5
#req_targmnr 93
#req_code 282
#unrest 15
#end

#selectevent 3062
#rarity 0
#req_rare 10
#req_minunrest 10
#req_targpath1 5
#req_targmnr 93
#req_code 282
-- ro: effect 39 = 2
#assassin -13
#end

#selectevent 3063
#rarity 0
#req_rare 40
#req_unique 1
#req_targaff 549755813888
#req_targmnr 93
#req_code 281
#addequip 9
#end

#selectevent 3064
#rarity 0
#req_rare 25
#req_unique 1
#req_targaff 549755813888
#req_targmnr 93
#req_code 281
#1d6vis 1
#end

#selectevent 3065
#rarity 0
#req_rare 5
#req_monster 534
#req_targpath1 5
#req_targmnr 93
#req_code 282
-- ro: effect 39 = 2
#assassin 534
#end

#selectevent 3066
#rarity 0
#req_rare 15
#req_unique 2
#req_targaff 549755813888
#req_targmnr 93
#req_code 282
#magicitem 9
#end

#selectevent 3067
#rarity 0
#req_code 281
#req_code 282
#notext
#code 0
#end

#selectevent 3068
#rarity 0
#req_rare 0
#req_freesites 1
#req_code 0
#hiddensite -1
#end

#selectevent 3069
#rarity 0
#req_rare 1
#req_targpath2 9
#req_dominion 4
#req_code -49
#code 0
#pathboost 9
#end

#selectevent 3070
#rarity 0
#req_rare 1
#req_targpath1 9
#req_maxdominion 4
#req_code -49
#gainaff 8
#end

#selectevent 3071
#rarity 0
#req_rare 1
#req_code -49
#gainaff 8589934592
#gainaff 549755813888
#end

#selectevent 3072
#rarity 0
#req_rare 1
#req_targpath1 55
#req_targmale 1
#req_targgod 0
#req_code -49
#gainaff 8589934592
#gainaff 549755813888
#bloodboost 1
#end

#selectevent 3073
#rarity 0
#req_rare 1
#req_targpath1 55
#req_targmale 0
#req_targgod 0
#req_code -49
#gainaff 8589934592
#gainaff 549755813888
#bloodboost 1
#end

#selectevent 3074
#rarity 0
#req_rare 1
#req_targpath1 8
#req_targaff 549755813888
#req_targgod 0
#req_anycode -49
#banished -12
#end

#selectevent 3075
#rarity 0
#req_rare 1
#req_targgod 0
#req_code -49
#code 0
#kill 1
#gainaff 8
#end

#selectevent 3076
#rarity 0
#req_rare 1
#req_unique 1
#req_maxdef 12
#req_capital 0
#req_code -49
#2com 1565
#15d6units 1565
#com 962
#3d6units 962
#com 304
#addequip 9
#end

#selectevent 3077
#rarity 0
#req_rare 2
#req_ench 61
#req_targpath2 1
#req_unique 2
#req_story 1
#req_land 1
#nation -2
#4d6units 562
#end

#selectevent 3078
#rarity 0
#req_rare 2
#req_unique 2
#req_enchtarget 87
#req_story 1
#req_land 1
#nation -2
#com 88
#3d6units 88
#end

#selectevent 3079
#rarity 0
#req_rare 2
#req_unique 2
#req_enchtarget 87
#req_story 1
#req_land 1
#req_cold 2
#nation -2
#com 632
#addequip 1
#waterboost 1
#3d6units 632
#end

#selectevent 3080
#rarity 0
#req_rare 1
#req_unique 2
#req_enchtarget 87
#req_story 1
#req_land 1
#com 88
#3d6units 88
#end

#selectevent 3081
#rarity 0
#req_story 1
#req_freesites 1
#req_rare 5
#req_unique 1
#req_forest 1
#req_fornation 69
#req_code 0
#code 283
#order 1
#flagland 1
#end

#selectevent 3082
#rarity 0
#req_unique 1
#req_targorder 100
#req_code 283
#code 284
#end

#selectevent 3083
#rarity 0
#req_unique 7
#req_targorder 100
#req_targmnr 793
#req_code 284
#code 285
#gainaff 549755813888
#order 5
#end

#selectevent 3084
#rarity 0
#req_targaff 549755813888
#req_targorder 102
#req_targmnr 793
#req_code 285
#code 286
-- ro: effect 39 = 2
#assassin 926
#end

#selectevent 3085
#rarity 0
#req_nomonster 793
#req_code 286
#code 284
#order 1
#end

#selectevent 3086
#rarity 0
#req_unique 1
#req_code 285
#req_targmnr 803
-- not read by the game (after an empty slot): #order 4
#end

#selectevent 3087
#rarity 0
#req_unique 1
#req_targaff 549755813888
#req_targmnr 793
#req_code 286
#code 287
#addsite -1
#order 0
#flagland 0
#end

#selectevent 3088
#rarity 0
#req_unique 2
#req_rare 3
#req_targorder 50
#req_code 287
#astralboost 1
#end

#selectevent 3089
#rarity 0
#req_unique 3
#req_rare 3
#req_targorder 50
#req_code 287
#addequip 9
#end

#selectevent 3090
#rarity 0
#req_unique 1
#req_rare 25
#req_targorder 50
#req_targmnr 793
#req_code 287
#addequip 9
#end

#selectevent 3091
#rarity 0
#req_unique 1
#req_mintroops 80
#req_notforally 69
#req_nearbycode 287
#req_code 0
#notext
#code 288
#end

#selectevent 3092
#rarity 0
#req_unique 1
#req_code 287
#req_nearbycode 288
#resetcode 288
#code 0
#nation -2
#com 926
#1d6units 1338
#end

#selectevent 3093
#rarity 0
#req_unique 1
#req_notforally 69
#req_code 287
#notext
#code 0
#resetcode 288
#end

#selectevent 3094
#rarity 0
#req_turn 15
#req_story 1
#req_rare 3
#req_unique 1
#req_fornation 69
#req_swamp 1
#req_code 0
#code 289
#notext
#end

#selectevent 3095
#rarity 0
#req_owncapital 1
#req_unique 1
#req_fornation 69
#req_anycode 289
#req_code 0
#code 292
#incscale 4
#unrest 5
#flagland 1
#delay25 12
#end

#selectevent 3096
#rarity 0
#req_unique 1
#req_code 292
#code 291
#incscale3 4
#incscale3 0
#incdom -3
#resetcode 289
#flagland 0
#end

#selectevent 3097
#rarity 0
#req_unique 1
#req_targmnr 1892
#req_code 292
#code 290
#end

#selectevent 3098
#rarity 0
#req_rare 10
#req_unique 3
#req_code 290
#req_code 292
#incscale2 4
#unrest 5
#end

#selectevent 3099
#rarity 0
#req_rare 7
#req_unique 1
#req_code 290
#req_code 292
#incscale2 4
#unrest 5
#end

#selectevent 3100
#rarity 0
#req_rare 7
#req_unique 1
#req_code 290
#req_code 292
#incscale3 4
#unrest 10
#end

#selectevent 3101
#rarity 0
#req_unique 1
#req_code 291
#delay 1
#unrest 15
#kill 1
#end

#selectevent 3102
#rarity 0
#req_unique 1
#req_code 291
#delay 1
#unrest 15
#kill 1
#end

#selectevent 3103
#rarity 0
#req_unique 1
#req_code 291
#code 0
#unrest -50
#incdom 3
#decscale3 0
#decscale3 4
#end

#selectevent 3104
#rarity 0
#req_unique 1
#req_targmnr 793
#req_code 291
#assassin 428
#end

#selectevent 3105
#rarity 0
#req_unique 1
#req_rare 50
#req_anycode 290
#req_targmnr 1892
#req_swamp 1
#resetcode 290
#resetcode 289
#natureboost 1
#end

#selectevent 3106
#rarity 0
#req_unique 1
#req_rare 25
#req_anycode 290
#req_targmnr 1892
#req_swamp 1
#resetcode 290
#resetcode 289
#natureboost 1
#end

#selectevent 3107
#rarity 0
#req_unique 1
#req_rare 25
#req_anycode 290
#req_targmnr 1890
#req_swamp 1
#resetcode 290
#resetcode 289
#natureboost 1
#end

#selectevent 3108
#rarity 0
#req_unique 1
#req_rare 25
#req_anycode 290
#req_targmnr 1891
#req_swamp 1
#resetcode 290
#resetcode 289
#natureboost 1
#end

#selectevent 3109
#rarity 0
#req_rare 2
#req_unique 2
#req_enchtarget 87
#req_story 1
#req_land 1
#req_heat 2
#nation -2
#com 304
#addequip 1
#fireboost 1
#addequip 1
#3d6units 303
#1d6units 304
#end

#selectevent 3110
#rarity 0
#req_rare 2
#req_unique 2
#req_enchtarget 87
#req_story 1
#req_land 1
#req_heat 2
#nation -2
#com 304
#addequip 1
#fireboost 1
#addequip 1
#3d6units 303
#1d6units 304
#end

#selectevent 3111
#rarity 0
#req_rare 2
#req_unique 2
#req_enchtarget 87
#req_story 1
#req_land 1
#req_cold 2
#com 632
#addequip 1
#waterboost 1
#3d6units 632
#end

#selectevent 3112
#rarity 0
#req_rare 10
#req_unique 3
#req_enchtarget 87
#req_story 1
#req_land 1
#req_code 0
#notext
#code -49
#end

#selectevent 3113
#rarity -1
#req_unluck 1
#req_swamp 1
#nation -2
#1unit 2222
#end

#selectevent 3114
#rarity 1
#req_luck 0
#req_swamp 1
#nation -2
#1unit 2222
#end

#selectevent 3115
#rarity 2
#req_land 0
#req_minpop 100
#req_chaos -2
#req_magic 2
#req_turn 10
#incscale3 5
#kill 10
#end

#selectevent 3116
#rarity 1
#req_fornation 69
#req_land 1
#req_code 0
#code -7
#end

#selectevent 3117
#rarity 1
#req_fornation 69
#req_land 1
#req_code 0
#code -7
#end

#selectevent 3118
#rarity 1
#req_fornation 69
#req_forest 1
#req_code 0
#code -7
#end

#selectevent 3119
#rarity 1
#req_fornation 69
#req_mountain 1
#req_code 0
#code -7
#end

#selectevent 3120
#rarity 0
#req_rare 15
#req_targmnr 804
#req_code -7
#code 0
#end

#selectevent 3121
#rarity 0
#req_rare 10
#req_targmnr 807
#req_code -7
#code 0
#end

#selectevent 3122
#rarity 0
#req_fornation 69
#req_rare 1
#req_land 1
#req_anycode 290
#req_code 0
#code -7
#end

#selectevent 3123
#rarity 0
#req_fornation 69
#req_rare 1
#req_land 1
#req_anycode 290
#req_code 0
#code -33
#end

#selectevent 3124
#rarity 0
#req_rare 15
#req_targmnr 804
#req_code -33
#code 0
#end

#selectevent 3125
#rarity 0
#req_rare 10
#req_targmnr 807
#req_code -33
#code 0
#end

#selectevent 3126
#rarity 0
#req_indepok
#req_land 1
#req_maxturn 1
#req_rare 15
#req_indepok  -- stored twice; a second #req_indepok in a mod replaces the first
#req_poptype 43
#req_unique 1
#req_code 0
#req_story 2
#code 301
#notext
#end

#selectevent 3127
#rarity 0
#req_unique 1
#req_code 301
#code 0
#resetcode 302
#resetcode 303
#resetcode 304
#end

#selectevent 3128
#rarity 0
#req_indepok
#req_unique 1
#req_code 301
#notext
#com 356
#15d6units 357
#7d6units 369
#2com 355
#addequip 1
#end

#selectevent 3129
#rarity 0
#req_indepok
#req_targmnr 356
#req_unique 1
#req_code 301
#notext
#deathboost 1
#deathboost 1
#holyboost 1
#addequip 1
#gainaff 549755813888
#end

#selectevent 3130
#rarity 0
#req_indepok
#req_indepok  -- stored twice; a second #req_indepok in a mod replaces the first
#req_targaff 549755813888
#req_targmnr 356
#req_unique 1
#req_code 301
#notext
#addequip 9
#earthboost 1
#earthboost 1
#addequip 1
#end

#selectevent 3131
#rarity 0
#req_rare 15
#req_turn 30
#req_targaff 549755813888
#req_targmnr 356
#req_indepok
#addequip 1
#end

#selectevent 3132
#rarity 0
#req_rare 20
#req_targaff 549755813888
#req_targmnr 355
#req_unique 1
#req_code 301
#notext
#addequip 9
#end

#selectevent 3133
#rarity 0
#req_indepok
#req_rare 10
#req_nearbycode 301
#req_capital 0
#req_land 1
#req_code 0
#code 302
#unrest 10
#end

#selectevent 3134
#rarity 0
#req_indepok
#req_rare 10
#req_nearbycode 301
#req_capital 0
#req_land 1
#req_code 0
#code 302
#end

#selectevent 3135
#rarity 0
#req_rare 50
#req_anycode 301
#req_indepok
#req_unique 9
#req_code 302
#code 303
#2com 355
#addequip 1
#9d6units 357
#3d6units 355
#end

#selectevent 3136
#rarity 0
#req_rare 8
#req_anycode 301
#req_indepok
#req_unique 6
#req_code 303
#com 355
#addequip 1
#7d6units 357
#3d6units 369
#12d6units -15
#end

#selectevent 3137
#rarity 0
#req_rare 7
#req_anycode 301
#req_indepok
#req_unique 3
#req_code 303
#nation -2
#com 234
#addequip 9
#end

#selectevent 3138
#rarity 0
#req_anycode 301
#req_unique 2
#req_code 303
#code 304
#end

#selectevent 3139
#rarity 0
#req_rare 33
#req_unique 2
#req_nearbycode 304
#resetcode 304
#com 188
#15d6units -15
#end

#selectevent 3140
#rarity 0
#req_unique 2
#req_code 303
#code 0
-- ro: effect 190 (gold) = 100
#magicitem 1
#1d3vis 5
#end

#selectevent 3141
#rarity 0
#req_rare 5
#req_anycode 301
#req_indepok
#req_unique 1
#req_code 301
#nation -2
#com 234
#addequip 9
#end

#selectevent 3142
#rarity 0
#req_rare 10
#req_anycode 301
#req_indepok
#req_unique 1
#req_code 301
#addsite -1
#end

#selectevent 3143
#rarity 0
#req_rare 10
#req_anycode 301
#req_indepok
#req_unique 1
#req_code 302
#addsite -1
#end

#selectevent 3144
#rarity 0
#req_targmnr 234
#req_indepok
#req_unique 3
#req_code 301
#addequip 9
#end

#selectevent 3145
#rarity 0
#req_indepok
#req_indepok  -- stored twice; a second #req_indepok in a mod replaces the first
#req_targaff 549755813888
#req_targmnr 356
#req_unique 1
#req_code 301
#notext
#addequip 9
#end

#selectevent 3146
#rarity 0
#req_rare 10
#req_anycode 301
#req_indepok
#req_unique 6
#req_code 303
#com 190
#1d6units 369
#9d6units -15
#9d6units -2
#3d6units 357
#end

#selectevent 3147
#rarity 0
#req_turn 66
#req_anycode 301
#req_indepok
#req_unique 6
#req_code 303
#com 190
#6d6units 369
-- ro: effect 177 (18d6units) = -15
-- ro: effect 177 (18d6units) = -2
#12d6units 357
#end

#selectevent 3148
#rarity 0
#req_turn 66
#req_indepok
#req_targmnr 356
#req_unique 1
#req_code 301
#notext
#deathboost 1
#earthboost 1
#addequip 1
#end

#selectevent 3149
#rarity 1
#req_fornation 69
#req_unique 1
#req_mountain 1
#req_story 1
#req_code 0
#code 306
#unrest 5
#flagland 1
#order 1
#end

#selectevent 3150
#rarity 0
#req_targorder 100
#req_code 306
#code 307
#order 415
#end

#selectevent 3151
#rarity 0
#req_targorder 100
#req_unique 1
#req_code 306
#code 308
#order 415
#end

#selectevent 3152
#rarity 0
#req_targorder 100
#req_code 308
#end

#selectevent 3153
#rarity 0
#req_targorder 102
#req_code 307
#code 309
#incscale2 0
#landgold 5
#end

#selectevent 3154
#rarity 0
#req_targorder 102
#req_code 308
#code 309
#incscale2 0
#landgold 25
#end

#selectevent 3155
#rarity 0
#req_targorder 108
#req_targmnr 803
#req_rare 70
#req_code 307
#req_code 308
#code 0
#incdom 2
#unrest -10
#decscale 0
#end

#selectevent 3156
#rarity 0
#req_targorder 108
#req_targmnr 803
#req_rare 10
#req_code 307
#req_code 308
#code 0
#incdom 2
#unrest -10
#decscale 0
#holyboost 1
#end

#selectevent 3157
#rarity 0
#req_targorder 108
#req_targmnr 808
#req_rare 30
#req_code 307
#req_code 308
#code 0
#incdom 2
#unrest -10
#decscale 0
#holyboost 1
#end

#selectevent 3158
#rarity 0
#req_targorder 108
#req_targmnr 804
#req_rare 30
#req_code 307
#req_code 308
#code 0
#incdom 2
#unrest -10
#decscale 0
#holyboost 1
#end

#selectevent 3159
#rarity 0
#req_targorder 108
#req_targmnr 803
#req_rare 30
#req_code 307
#req_code 308
-- ro: effect 39 = 2
#assassin 1264
#code 310
#end

#selectevent 3160
#rarity 0
#req_targorder 108
#req_targmnr 808
#req_rare 50
#req_code 307
#req_code 308
-- ro: effect 39 = 2
#assassin 1264
#code 310
#end

#selectevent 3161
#rarity 0
#req_targorder 108
#req_targmnr 804
#req_rare 70
#req_code 307
#req_code 308
-- ro: effect 39 = 2
#assassin 1264
#code 310
#end

#selectevent 3162
#rarity 0
#req_targorder 108
#req_targmnr 803
#req_code 310
#code 0
#end

#selectevent 3163
#rarity 0
#req_targorder 108
#req_targmnr 808
#req_code 310
#code 0
#end

#selectevent 3164
#rarity 0
#req_targorder 108
#req_targmnr 804
#req_code 310
#code 0
#end

#selectevent 3165
#rarity 0
#req_targorder 108
#req_code 307
#req_code 308
#code 0
#com 1264
#gainaff 549755813888
#addequip 9
#4d6units 1264
#end

#selectevent 3166
#rarity -1
-- ro: requirement 96 = 1
#req_dominion 6
#claimthrone
#taxboost -100
#end

#selectevent 3167
#rarity -1
-- ro: requirement 96 = 1
#req_dominion 8
#claimthrone
#taxboost -50
#end

#selectevent 3168
#rarity -1
-- ro: requirement 96 = 1
#req_dominion 10
#claimthrone
#end

#selectevent 3169
#rarity 1
#req_chaos -1
#req_pop0ok
#req_land 1
-- ro: requirement 231 = 1
#req_targmanygems 56
#assfollower1d3 482
-- ro: effect 39 = 2
#assassin 1912
#end

#selectevent 3170
#rarity 1
#req_pop0ok
#req_land 1
-- ro: requirement 231 = 1
#req_targmanygems 8
#assfollower1 -13
-- ro: effect 39 = 2
#assassin -13
#end

#selectevent 3171
#rarity 2
#req_magic 1
#req_pop0ok
#req_voidok 1
#req_targmanygems 4
#gainmark
-- ro: effect 39 = 2
#assassin -6
#end

#selectevent 3172
#rarity 2
#req_growth -1
#req_pop0ok
#req_land 1
-- ro: requirement 231 = 1
#req_targmanygems 56
#assfollower1 284
-- ro: effect 39 = 2
#assassin 2361
#end

#selectevent 3173
#rarity 2
#req_forest 1
#req_pop0ok
#req_land 1
#req_targmanygems 6
-- ro: effect 39 = 2
#assassin 782
#end

#selectevent 3174
#rarity 2
#req_pop0ok
#req_land 0
#req_targmanygems 2
-- ro: effect 39 = 2
#assassin 565
#end

#selectevent 3175
#rarity 2
#req_death 1
#req_pop0ok
#req_voidok 1
#req_land 1
#req_targmanygems 5
-- ro: effect 39 = 2
#assassin 566
#end

#selectevent 3176
#rarity 13
-- ro: requirement 99 = -4
#req_unique 1
#worlddecscale2 5
#worldunrest 5
#worldmark 2
#end

#selectevent 3177
#rarity 13
-- ro: requirement 99 = -3
#req_unique 1
#worlddecscale2 5
#worldunrest 10
#worldmark 3
#end

#selectevent 3178
#rarity 13
-- ro: requirement 99 = -2
#req_unique 1
#worlddecscale2 5
#worldunrest 20
#worldmark 5
#end

#selectevent 3179
#rarity 13
-- ro: requirement 99 = -1
#req_permonth 1
#worlddecscale 5
#worldunrest 10
#worldmark 5
#end

#selectevent 3180
#rarity 13
#req_claimedthrone
#req_rare 10
#req_site 1
#req_unique 1
#worldincdom -1
#end

#selectevent 3181
#rarity 13
#req_claimedthrone
#req_rare 25
#req_site 1
#req_unique 1
#worlddecscale2 0
#worldincdom -1
#end

#selectevent 3182
#rarity 13
#req_claimedthrone
#req_rare 25
#req_site 1
-- ro: requirement 58 = 1
#worlddecscale 1
#worldincdom -1
#end

#selectevent 3183
#rarity 13
#req_claimedthrone
#req_rare 25
#req_site 1
-- ro: requirement 58 = 1
#worldincscale2 0
#worldincdom -1
#end

#selectevent 3184
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worlddecscale 0
#worldincdom 1
#end

#selectevent 3185
#rarity 12
#worldunrest 10
#worlddecscale 5
#worldincscale 4
#end

#selectevent 3186
#rarity -9
#end

#selectevent 3187
#rarity -2
#req_death 1
#req_magic 1
#killpop 25
#3d6vis 5
#end

#selectevent 3188
#rarity 2
#req_magic 2
#req_growth 1
#req_turn 8
#req_forest 1
#com 362
#6d6units 361
#com 362
#3d6units 361
#kill 10
#end

#selectevent 3189
#rarity 0
#req_fornation 6
#req_month 5
#req_fort 1
#req_monster 3115
#req_code 0
-- ro: requirement 22 (req_siege) = 0
#code 133
#unrest 15
#end

#selectevent 3190
#rarity 0
#req_code 133
#req_5monsters 3105
-- ro: requirement 22 (req_siege) = 0
#req_month 6
#unrest -50
#kill2d6mon 3105
#killpop 30
#delay 1
#code 0
#end

#selectevent 3191
#rarity 0
-- ro: requirement 22 (req_siege) = 0
#req_land 1
#nation 6
#com 3113
#xp 60
#end

#selectevent 3192
#rarity 0
#req_code 133
-- ro: requirement 22 (req_siege) = 0
#req_month 6
#unrest -40
#killpop 30
#delay 1
#code 0
#end

#selectevent 3193
#rarity 0
-- ro: requirement 22 (req_siege) = 0
#req_land 1
#nation 6
#com 3113
#xp 25
#end

#selectevent 3194
#rarity 0
#req_fornation 6
#req_month 5
#req_fort 1
#req_nomonster 3115
#req_code 0
-- ro: requirement 22 (req_siege) = 0
#unrest 15
#end

#selectevent 3195
#rarity 0
#req_code 168
#req_rare 3
#kill 20
#com 2212
#com 2212
#3d6units 2212
#code 0
#end

#selectevent 3196
#rarity 0
#req_rare 1
#req_code 298
#code 0
#end

#selectevent 3197
#rarity 0
#req_rare 20
#req_nomonster 310
#req_code 249
#req_code 250
#code 0
#end

#selectevent 3198
#rarity -2
#req_land 1
#req_turn 12
#req_season 2
#req_lazy 1
#req_magic 2
#magicitem 9
#unrest 15
#incscale2 1
#delay 2
#end

#selectevent 3199
#rarity 1
#req_land 1
#com 363
#12d6units 1565
#com 1565
#kill 2
#end

#selectevent 3200
#rarity 14
#req_code 0
#req_notanycode -199
#code -199
#arena
#delay 1
#end

#selectevent 3201
#rarity 13
#resetcode -199
#resolvearena1
#end

#selectevent 3202
#rarity 14
#req_code 0
#req_notanycode -199
#code -199
#arena2
#delay 1
#end

#selectevent 3203
#rarity 13
#resetcode -199
#resolvearena2
#end

#selectevent 3204
#rarity -9
#end

#selectevent 3205
#rarity 0
#req_month 0
#req_owncapital 1
#req_fornation 125
#req_fornation 77
#req_deadmnr 3167
#delay50 2
#delayskip 30
#end

#selectevent 3206
#rarity 0
#req_owncapital 1
#magicitem 9
#unrest -15
#end

#selectevent 3207
#rarity 0
#req_owncapital 1
#unrest -10
#end

#selectevent 3208
#rarity 0
#req_fornation 77
#req_owncapital 1
#req_month 1
#req_turn 5
-- ro: requirement 22 (req_siege) = 0
#nation -2
#com 3149
#4d3units 3150
#end

#selectevent 3209
#rarity 0
#req_fornation 95
#req_owncapital 1
#req_month 0
#req_turn 5
-- ro: requirement 22 (req_siege) = 0
#req_nomnr 3201
#nation -2
#com 3201
-- ro: effect 190 (gold) = -50
#unrest -10
#end

#selectevent 3210
#rarity 0
#req_fornation 95
#req_owncapital 1
#req_month 6
#req_turn 5
-- ro: requirement 22 (req_siege) = 0
#req_nomnr 3202
#nation -2
#com 3202
-- ro: effect 190 (gold) = -50
#unrest -10
#end

#selectevent 3211
#rarity 0
#req_fornation 95
#req_temple 1
#req_owncapital 1
#req_monster 3201
#req_monster 3202  -- stored twice; a second #req_monster in a mod replaces the first
#req_month 3
#decscale2 3
-- ro: effect 190 (gold) = 50
#end

#selectevent 3212
#rarity 0
#req_fornation 95
#req_temple 1
#req_owncapital 1
#req_nomonster 3201
#req_monster 3202
#req_month 3
#unrest 15
#end

#selectevent 3213
#rarity 0
#req_fornation 95
#req_temple 1
#req_owncapital 1
#req_monster 3201
#req_nomonster 3202
#req_month 3
#unrest 15
#end

#selectevent 3214
#rarity 0
#req_fornation 95
#req_temple 1
#req_owncapital 1
#req_nomonster 3201
#req_nomonster 3202  -- stored twice; a second #req_nomonster in a mod replaces the first
#req_month 3
#unrest 15
#end

#selectevent 3215
#rarity 0
#req_site 1
#req_fornation 96
#req_turn 30
#req_turnrare -5
#req_owncapital 1
#req_unique 1
#nation -2
#removesite -1
#unrest 40
#kill 20
#com 3229
#addsite 202
#end

#selectevent 3216
#rarity 0
#req_site 1
#req_turn 30
#req_turnrare -5
#req_owncapital 0
#req_unique 1
#removesite -1
#unrest 40
#kill 20
#com 3229
#addsite 202
#end

#selectevent 3217
#rarity 1
#req_site 1
#req_fornation 96
#req_turn 20
#req_unique 1
#nation -2
#removesite -1
#unrest 40
#kill 20
#com 3230
#addsite 202
#end

#selectevent 3218
#rarity 0
#req_site 1
#req_fornation 96
#req_turn 5
-- ro: requirement 152 = 10
#unrest 30
#end

#selectevent 3219
#rarity 0
#req_site 1
#req_fornation 96
#req_turn 15
#req_rare 25
#req_unique 1
#unrest 40
#2d6vis 0
#emigration 10
#end

#selectevent 3220
#rarity 0
#req_rare 5
#req_unique 1
#req_turn 15
#req_monster 3239
#req_land 1
#req_targmnr 153
#nation -2
#gainaff 4194304
#unrest 15
#curse 3
#decscale 5
#incscale 4
#1unit 636
#end

#selectevent 3221
#rarity 0
#req_rare 5
#req_turn 15
#req_monster 3239
#req_land 1
#req_targmnr 152
#nation -2
#transform 2136
#unrest 10
#end

#selectevent 3222
#rarity 0
#req_rare 5
#req_unique 1
#req_turn 15
#req_monster 3236
#req_land 1
#req_targmnr 153
#nation -2
#lab 0
#end

#selectevent 3223
#rarity 0
#req_rare 7
#req_turn 15
#req_monster 3236
#req_land 1
#req_targmnr 152
#nation -2
#transform 3243
#earthboost 152
#end

#selectevent 3224
#rarity 0
#req_rare 7
#req_turn 15
#req_monster 3236
#req_land 1
#req_targmnr 152
#nation -2
#transform 3243
#astralboost 152
#end

#selectevent 3225
#rarity 0
#req_forest 1
#req_rare 7
#req_turn 15
#req_monster 3236
#req_land 1
#req_targmnr 152
#nation -2
#transform 3243
#natureboost 152
#end

#selectevent 3226
#rarity 0
#req_land 1
#req_monster 3259
#req_unique 1
#nation -2
#4d6units 2349
#end

#selectevent 3227
#rarity 0
#req_land 1
#req_fort 1
#req_fornation 67
#req_month 3
-- ro: requirement 115 = 3306
#com 3327
#15d6units 3327
#end

#selectevent 3228
#rarity 0
#req_fornation 67
#req_owncapital 1
#req_unique 1
#req_code 0
-- ro: requirement 117 = 1
#req_turn 12
#req_rare 50
#codedelay 360
#resetcodedelay2 360
#end

#selectevent 3229
#rarity 0
#req_notforally 67
#req_rare 50
#req_anycode 360
#req_owncapital 1
#req_land 1
-- ro: effect 140 = 67
#nation -3
#com 3287
#incdom -2
#resetcodedelay 360
#end

#selectevent 3230
#rarity 0
#req_rare 50
#req_nomnr 3382
#req_monster 3383
#killcom 3383
#end

#selectevent 3231
#rarity 0
#req_rare 50
#req_nomnr 3383
#req_monster 3382
#killcom 3382
#end

#selectevent 3232
#rarity -1
#req_fornation 65
#req_nomnr 3385
#req_owncapital 1
-- ro: effect 190 (gold) = 250
#1d6vis -1
#end

#selectevent 3233
#rarity 0
#req_pop0ok
#req_rare 10
#req_land 1
#req_cave 0
-- ro: requirement 118 = 25
#unrest 25
#kill 3
#end

#selectevent 3234
#rarity 13
#req_monsterbs 3425
#req_mnrbs 3426
#req_mnrbs 3427
#req_mnrbs 3428
#worlddarkness
-- ro: effect 145 = 1
#end

#selectevent 3235
#rarity -1
#req_foundsite 1
-- ro: effect 190 (gold) = 400
#end

#selectevent 3236
#rarity -1
#req_foundsite 1
-- ro: effect 190 (gold) = 250
#end

#selectevent 3237
#rarity -2
#req_foundsite 1
#req_turn 5
-- ro: effect 190 (gold) = 750
#end

#selectevent 3238
#rarity -2
#req_foundsite 1
-- ro: effect 190 (gold) = 400
#end

#selectevent 3239
#rarity -2
#req_foundsite 1
#req_turn 5
-- ro: effect 190 (gold) = 750
#end

#selectevent 3240
#rarity 1
#req_foundsite 1
-- ro: effect 190 (gold) = -150
#unrest 10
#end

#selectevent 3241
#rarity 1
#req_foundsite 1
-- ro: effect 190 (gold) = -150
#unrest 10
#end

#selectevent 3242
#rarity 1
#req_foundsite 1
-- ro: effect 190 (gold) = -250
#unrest 10
#end

#selectevent 3243
#rarity 1
#req_foundsite 1
-- ro: effect 190 (gold) = -250
#unrest 10
#end

#selectevent 3244
#rarity 2
#req_foundsite 1
#req_prod 2
-- ro: effect 190 (gold) = -150
#unrest 10
#removesite -1
#end

#selectevent 3245
#rarity 2
#req_foundsite 1
#req_prod 2
-- ro: effect 190 (gold) = -150
#unrest 10
#removesite -1
#end

#selectevent 3246
#rarity 2
#req_foundsite 1
#req_prod 2
-- ro: effect 190 (gold) = -250
#unrest 10
#removesite -1
#end

#selectevent 3247
#rarity 2
#req_foundsite 1
#req_prod 2
-- ro: effect 190 (gold) = -250
#unrest 10
#removesite -1
#end

#selectevent 3248
#rarity 2
#req_turn 10
#req_foundsite 1
#req_prod 2
#req_magic 2
#com 2523
#4d6units 2525
#com 2522
#3d6units 2524
#end

#selectevent 3249
#rarity 2
#req_turn 10
#req_foundsite 1
#req_prod 2
#req_magic 2
#com 2523
#4d6units 2525
#com 2522
#3d6units 2524
#end

#selectevent 3250
#rarity 2
#req_turn 10
#req_foundsite 1
#req_prod 2
#req_magic 2
#com 2523
#4d6units 2525
#com 2522
#3d6units 2524
#end

#selectevent 3251
#rarity 2
#req_turn 10
#req_foundsite 1
#req_prod 2
#req_magic 2
#com 2523
#4d6units 2525
#com 2522
#3d6units 2524
#end

#selectevent 3252
#rarity 2
#req_code 0
#req_unluck 4
#req_chaos 1
#req_turn 10
#req_capital 0
#req_coast 1
#id 20
#emigration 50
#kill 100
#lab 0
-- ro: effect 194 = 1
#code 105
#setpoptype 72
#end

#selectevent 3253
#rarity -1
#req_land 1
#req_godismnr 602
#req_unique 1
#nation -2
#com 3832
#end

#selectevent 3254
#rarity -1
#req_land 1
#req_godismnr 602
#req_unique 1
#nation -2
#com 3833
#end

#selectevent 3255
#rarity -1
#req_land 1
#req_godismnr 602
#nation -2
#com 3834
#end

#selectevent 3256
#rarity -1
#req_land 1
#req_godismnr 602
#nation -2
#com 3835
#end

#selectevent 3257
#rarity 13
#req_rare 20
#req_unique 5
#req_ench 124
#worldincdom -1
#end

#selectevent 3258
#rarity 1
#req_monster 3664
#req_monster 1318  -- stored twice; a second #req_monster in a mod replaces the first
#killmon 1318
#end

#selectevent 3259
#rarity 1
#req_monster 3664
#req_monster 1319  -- stored twice; a second #req_monster in a mod replaces the first
#killmon 1319
#end

#selectevent 3260
#rarity 1
#req_monster 3664
#req_monster 403  -- stored twice; a second #req_monster in a mod replaces the first
#killmon 403
#end

#selectevent 3261
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worlddecscale 5
#end

#selectevent 3262
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#req_rare 50
#worldincdom -1
#end

#selectevent 3263
#rarity 13
#req_claimedthrone
#req_site 1
-- ro: requirement 58 = 1
#req_rare 50
#worlddecscale 5
#worldincscale 4
#worldmark 2
#end

#selectevent 3264
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worldunrest -25
#end

#selectevent 3265
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worlddecscale 0
#end

#selectevent 3266
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worldunrest -10
#worlddecscale 1
#end

#selectevent 3267
#rarity 13
#req_claimedthrone
#req_site 1
#req_unique 1
#worlddecscale 4
#end

#selectevent 3268
#rarity 0
#req_rare 8
-- ro: requirement 155 = 10
#req_land 1
#req_targorder 8
#req_targgod 0
#req_targimmobile 0
-- ro: effect 39 = 5
#assfollower2 3822
#assfollower1 3823
#assassin 3823
#end

#selectevent 3269
#rarity 0
#req_rare 4
-- ro: requirement 155 = 10
#req_land 1
#req_targorder 8
#req_targgod 0
#req_targimmobile 0
-- ro: effect 39 = 5
#assfollower2 3821
#assfollower2 3823
#assassin 3823
#end

#selectevent 3270
#rarity -1
-- ro: requirement 157 = 1
#req_magic 3
#2d6vis 4
#end

#selectevent 3271
#rarity -1
-- ro: requirement 157 = 1
#req_magic 3
#2d6vis 4
#end

#selectevent 3272
#rarity -2
-- ro: requirement 157 = 1
#req_magic 3
-- ro: effect 20 = 3
#end

#selectevent 3273
#rarity 1
-- ro: requirement 157 = 1
#req_commander 1
-- ro: effect 39 = 2
#assassin -6
#end

#selectevent 3274
#rarity 1
-- ro: requirement 157 = 1
#req_commander 1
-- ro: effect 39 = 2
#assassin -6
#end

#selectevent 3275
#rarity 1
-- ro: requirement 157 = 1
#req_commander 1
#gainmark
#end

#selectevent 3276
#rarity 1
#req_coast 1
#req_unluck 2
#req_turn 10
#com 976
#6d6units 974
#com 976
#4d6units 975
#end

#selectevent 3277
#rarity 1
#req_land 0
#req_unluck 2
#req_turn 10
#com 976
#6d6units 974
#com 976
#4d6units 975
#end

#selectevent 3278
#rarity 1
#req_coast 1
#req_unluck 4
#req_turn 10
#com 2804
#6d6units 974
-- ro: effect 154 = 975
#com 976
#6d6units 975
#com 976
#9d6units 974
#com 976
#9d6units 974
#end

#selectevent 3279
#rarity 1
#req_land 0
#req_unluck 4
#req_turn 10
#com 2804
#6d6units 974
-- ro: effect 154 = 975
#com 976
#6d6units 975
#com 976
#9d6units 974
#com 976
#9d6units 974
#end

#selectevent 3280
#rarity 1
#req_growth 0
#req_cave 1
#unrest 20
#taxboost -50
#end

#selectevent 3281
#rarity 1
#req_land 1
#req_cave 1
#req_unluck 1
#req_turn 8
#nation 4
#com 1616
#5d6units 1615
#com 1616
#5d6units 1615
#kill 10
#end

#selectevent 3282
#rarity 1
#req_capital 0
#req_land 1
#req_cave 1
#req_unluck 3
#req_turn 10
#req_land 1  -- stored twice; a second #req_land in a mod replaces the first
#nation 4
#com 1616
#15d6units 1615
#com 1616
#5d6units 1615
#kill 20
#end

#selectevent 3283
#rarity 1
#req_land 1
#req_farm 1
#req_commander 1
#assassin 410
#end

#selectevent 3284
#rarity 2
#req_land 1
#req_commander 1
#assfollower1d3 410
#assassin 410
#end

#selectevent 3285
#rarity 0
#req_rare 50
#req_code -17
#req_maxdominion -1
#code 0
#taxboost 100
#incdom 3
#unrest -15
#end

#selectevent 3286
#rarity 0
#req_rare 30
#req_land 1
#req_targmnr 811
#req_targseductions 3
#addseductions -10
#nation -2
#com 4054
#end

#selectevent 3287
#rarity 0
#req_rare 30
#req_land 1
#req_targmnr 811
#req_targseductions 3
#addseductions -10
#nation -2
#com 4055
#end

#selectevent 3288
#rarity 0
#req_rare 25
#req_land 1
#req_targmnr 4053
#req_targseductions 1
#addseductions -10
#nation -2
#com 4054
#end

#selectevent 3289
#rarity 0
#req_rare 25
#req_land 1
#req_targmnr 4053
#req_targseductions 1
#addseductions -10
#nation -2
#com 4055
#end

#selectevent 3290
#rarity 0
#req_rare 50
#req_land 1
#req_targmnr 4059
#req_targseductions 3
#addseductions -10
#nation -2
#com 4054
#end

#selectevent 3291
#rarity 0
#req_rare 50
#req_land 1
#req_targmnr 4059
#req_targseductions 3
#addseductions -10
#nation -2
#com 4055
#end

#selectevent 3292
#rarity 12
#req_minglobals 1
#dispglobals 20
#end

#selectevent 3293
#rarity 0
#req_code 105
#req_rare 10
#req_maxpop 1
#code 0
#incpop 100
#end

#selectevent 3294
#rarity 0
#req_code 105
#req_growth 3
#req_luck 2
#req_rare 10
#req_maxpop 1
#code 0
#incpop 200
#end

#selectevent 3295
#rarity 0
#req_code 105
#req_death 3
#req_rare 10
#req_maxpop 1
#code 0
#incscale 4
#end

#selectevent 3296
#rarity 0
#req_code 105
#req_unluck 2
#req_rare 10
#req_maxpop 1
#code 0
#incscale 4
#end

#selectevent 3297
#rarity 2
#req_land 0
#req_commander 1
#assassin 642
#end

#selectevent 3298
#rarity 1
#req_land 1
#req_forest 1
#req_chaos -1
#req_commander 1
#assassin 694
#end

#selectevent 3299
#rarity 1
#req_land 1
#req_cave 1
#req_commander 1
#assassin 2513
#end

#selectevent 3300
#rarity 2
#req_land 0
#req_caveforest 1
#req_commander 1
#assassin 2512
#end

#selectevent 3301
#rarity 2
#req_land 0
#req_drip 1
#req_commander 1
#assassin 2514
#end
