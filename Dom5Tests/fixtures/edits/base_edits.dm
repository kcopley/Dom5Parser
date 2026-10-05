#modname "Edit test base"
#description "Base mod for scripted-edit tests (Dom5Tests edit + tools/fidelity stage 4)."

-- Template with two copies: editing the template should reach both (live template).
#newmonster 7400
#name "Template T"
#hp 10
#att 10
#weapon 1 -- Spear
#end

#newmonster 7401
#name "Copy One"
#copystats 7400
#end

#newmonster 7402
#name "Copy Two"
#copystats 7400
#prot 3
#end

-- Standalone unit with two weapons.
#newmonster 7410
#name "Two Weapons"
#hp 12
#weapon 1 -- Spear
#weapon 23 -- Short Bow
#end

-- A vanilla unit this mod already edits.
#selectmonster 3 -- Serpent Cataphract
#mor 15
#end
