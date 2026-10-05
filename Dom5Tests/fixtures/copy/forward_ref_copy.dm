#modname "Forward Reference Copy Test"
#description "A lower-ID monster copystats a higher-ID monster that is defined earlier in the FILE. File-order parsing resolves it; ID-ordered export emits the copier before its source and breaks the copy unless the editor flattens (bakes) the copied state."

-- Source: defined first in the file, but with a HIGHER id than its copier.
#newmonster 7200
#name "Template T"
#hp 77
#att 12
#def 9
#prot 8
#mr 16
#end

-- Copier: LOWER id, copies the higher-id source. In file order this resolves to T;
-- in the ID-ordered re-export 7100 is written before 7200, so on reload #copystats 7200
-- would find nothing. The materialize+flatten must bake T's stats onto 7100.
#newmonster 7100
#name "Copier C"
#copystats 7200
#mor 30
#end

-- Expected (illwinter / inspector, file order):
--   7100 Copier C: hp 77, att 12, def 9, prot 8, mr 16 (copied from T), mor 30 (own)
--   7200 Template T: hp 77, att 12, def 9, prot 8, mr 16
-- The ID-ordered exporter writes 7100 before 7200; without flattening, reload loses
-- 7100's copied stats (forward reference broken) -> round-trip divergence on every stat.
