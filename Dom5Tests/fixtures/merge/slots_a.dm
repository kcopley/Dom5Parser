#modname "Slots test A"
#description "New spells and items without numbers: the game gives them the first free ones (spells from 1500, items from 700)"

#newspell
#name "A's First Spell"
#end

#newspell
#name "A's Second Spell"
#end

#newspell
#name "A's Third Spell"
#end

#newitem
#name "A's First Item"
#copyitem 1
#end

#newitem 800 -- the game ignores the number: this one is 701
#name "A's Second Item"
#copyitem 1
#end

#newitem
#name "A's Third Item"
#copyitem 1
#end

#newitem
#name "A's Fourth Item"
#copyitem 1
#end
