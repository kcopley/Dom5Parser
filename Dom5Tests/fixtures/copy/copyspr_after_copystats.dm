#modname "adv_7c copyspr wipes order-dependent edit bake"
#description "Backward-ref copy of an edited source needs _materialized to bake the pre-edit value; copyspr after copystats wipes it, so the divergent edit is lost and reload sees the source's POST-edit value."
#newmonster 7500
#name "EditSrcC"
#hp 30
#att 10
#end
#newmonster 7501
#name "SprSrcC"
#spr1 "./bar.tga"
#end
#newmonster 7502
#name "Copier7c"
#copystats 7500
#copyspr 7501
#end
-- edit the stat source AFTER the copy: att 10 -> 99. Must NOT reach 7502.
#selectmonster 7500
#att 99
#end
-- Expected: 7502 att 10 (pre-edit snapshot). On reload #copystats 7500 reproduces att 99,
-- so the editor must bake att 10. If copyspr wiped _materialized, nothing is baked -> att 99 wrongly.
