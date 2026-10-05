#modname "Clear Mid Entity Test"
#description "A #clearweapons partway through a definition erases the weapons added before it but keeps those added after, and leaves stats untouched. Verifies clear-at-point semantics survive a round-trip."

#newmonster 7300
#name "Clear Test"
#hp 50
#att 11
#weapon 4   -- added before the clear; must NOT survive
#weapon 5   -- added before the clear; must NOT survive
#clearweapons
#weapon 6   -- added after the clear; must survive
#prot 7
#end

-- Expected (illwinter / inspector):
--   7300 Clear Test: hp 50, att 11, prot 7, weapons = [6]
-- #clearweapons snapshots at its point: weapons 4 and 5 (before it) are removed,
-- weapon 6 (after it) remains. Stats are unaffected by #clearweapons.
