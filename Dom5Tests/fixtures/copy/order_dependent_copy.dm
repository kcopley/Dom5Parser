#modname "Order Dependent Copy Test"
#description "B copies A before A is edited; C copies A after. B and C must differ."

-- A: base monster (att starts at 10)
#newmonster 7000
#name "Base A"
#hp 10
#att 10
#end

-- B copies A while A.att == 10  -> B must end with att 10 (NOT the later 99)
#newmonster 7001
#name "Copy B early"
#copystats 7000
#prot 5
#end

-- Now A is edited to att 99. This must NOT reach B (B already copied).
#selectmonster 7000
#att 99
#end

-- C copies A after the edit -> C must end with att 99
#newmonster 7002
#name "Copy C late"
#copystats 7000
#prot 7
#end

-- Expected (illwinter / inspector, order-dependent):
--   7000 Base A:      hp 10, att 99
--   7001 Copy B early: hp 10, att 10, prot 5   <-- att 10, the pre-edit snapshot
--   7002 Copy C late:  hp 10, att 99, prot 7
-- The lazy/order-independent editor currently resolves B.att = 99 (A's final state)
-- => round-trip divergence on 7001.att. The materialize fix must make B.att = 10.
