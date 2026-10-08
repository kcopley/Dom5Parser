#modname "Sequence test B"
#description "Chains by number: a hydra (#shrinkhp/#growhp), an #xpshape line, an #xpshapemon, a template"

-- a hydra: 5001 shrinks into 5002, which grows back into 5001 and shrinks into 5003
#newmonster 5001
#copystats 2
#name "Hydra Large"
#shrinkhp 20
#end

#newmonster 5002
#copystats 2
#name "Hydra Medium"
#growhp 30
#shrinkhp 10
#end

#newmonster 5003
#copystats 2
#name "Hydra Small"
#growhp 15
#end

-- an apprentice line by #xpshape: 5010 -> 5011 -> 5012
#newmonster 5010
#copystats 3
#name "Apprentice"
#xpshape 20
#end

#newmonster 5011
#copystats 3
#name "Journeyman"
#xpshape 40
#end

#newmonster 5012
#copystats 3
#name "Master"
#end

-- an explicit target: no chain
#newmonster 5020
#copystats 4
#name "Changeling"
#xpshape 10
#xpshapemon 5021
#end

#newmonster 5021
#copystats 4
#name "Changed One"
#end

-- a ritual summoning its unit and the next number (effect 10141): 5030-5031 move as a pair
#newmonster 5030
#copystats 5
#name "Bird One"
#end

#newmonster 5031
#copystats 5
#name "Bird Two"
#end

#newspell
#name "Call Two Birds"
#effect 10141
#damage 5030
#nreff 1
#end

#selectnation 150
#name "Nation B"
#era 2
#addrecunit 5010
#addreccom "Hydra Large"
#end

#newtemplate 150
#form "Hydra Large (5001)"
#prison 0
#end
