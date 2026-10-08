using Dom5Edit;
using Dom5Edit.Entities;
using Dom5Edit.Merge;
using Dom5Edit.Resolve;
using Dom5Edit.Validation;

namespace Dom5Tests
{
    /// <summary>
    /// Dom5Tests merge OUT.dm NAME A.dm B.dm ... [--needs B.dm=A.dm]...: merges the mods (Merge.ModMerger),
    /// writes OUT.dm and OUT.report.md, then reads the merged file back and checks it: no number
    /// defined twice that wasn't before, and every entity a single part defines or changes has the
    /// values it had in its part (references compared by number, after the moves).
    /// </summary>
    internal static class MergeCheck
    {
        public static void Run(string basePath, string[] args)
        {
            Program.LoadVanillaBase(basePath);
            var outFile = args[1];
            var name = args[2];
            var needs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var files = new List<string>();
            for (int i = 3; i < args.Length; i++)
                if (args[i] == "--needs" && i + 1 < args.Length)
                {
                    var pair = args[++i].Split('=', 2);
                    needs[Path.GetFullPath(pair[0])] = pair[1];
                }
                else
                    files.Add(args[i]);
            var inputs = files.Select(f => new MergeInput(f, needs.TryGetValue(Path.GetFullPath(f), out var n) ? n : null)).ToList();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = ModMerger.Merge(inputs, outFile, name);
            Console.WriteLine($"merged {result.Parts.Count} parts in {watch.ElapsedMilliseconds} ms: {result.Report.Moves.Count} numbers moved, " +
                              $"{result.Report.Renames.Count} renamed, {result.Report.Conflicts.Count} across parts, {result.Report.Notes.Count} notes");
            foreach (var g in result.Report.Moves.GroupBy(m => (m.Part, m.Kind)))
                Console.WriteLine($"   {g.Key.Part}: {g.Count()} {g.Key.Kind} (e.g. {string.Join(", ", g.Take(3).Select(m => $"{m.Old}->{m.New}"))})");
            foreach (var r in result.Report.Renames.Take(5))
                Console.WriteLine($"   renamed: {r}");
            foreach (var n in result.Report.Notes.Take(8))
                Console.WriteLine($"   note: {n}");

            // read back
            var back = Mod.Import(outFile, result.Separate.LastOrDefault());
            var before = result.Parts.Sum(p => p.ParseIssues.Count(x => x.IssueType == ParseIssueType.DuplicateId));
            var dup = back.ParseIssues.Where(x => x.IssueType == ParseIssueType.DuplicateId).ToList();
            Console.WriteLine($"read back: {dup.Count} numbers defined twice (the parts had {before})");
            foreach (var d in dup.Take(5))
                Console.WriteLine($"   {d.Message} (line {d.LineNumber})");
            // every entity one part alone defines or changes: the same values as in its part
            // (a game entity a part selects by name counts under its number too)
            var touched = new Dictionary<(EntityType, int), List<(Mod, IDEntity)>>();
            foreach (var part in result.Parts)
                foreach (var (type, set) in part.Database)
                    foreach (var e in set.GetFullList().Where(x => x.ID > 0 && (set.TryGetValue(x.ID, out var held) && ReferenceEquals(held, x) || x.Selected)))
                    {
                        if (!touched.TryGetValue((type, e.ID), out var list))
                            touched[(type, e.ID)] = list = new();
                        list.Add((part, e));
                    }
            // the game entities each part changes: a part's entity that copies one another part
            // changes takes those changes (or the merge's snapshot of the game's): not compared
            var changedBy = new Dictionary<(EntityType, int), List<Mod>>();
            foreach (var part in result.Parts)
                foreach (var (type, set) in part.Database)
                    foreach (var e in set.GetFullList().Where(x => x.Selected && x.ID > 0 && VanillaLoader.Vanilla?.Database[type].TryGetValue(x.ID, out _) == true))
                    {
                        if (!changedBy.TryGetValue((type, e.ID), out var l))
                            changedBy[(type, e.ID)] = l = new();
                        l.Add(part);
                    }
            // (through a copy of the part's own entity that copies one, too)
            bool CopiesAcross(Mod part, IDEntity e, HashSet<IDEntity>? seen = null)
            {
                seen ??= new HashSet<IDEntity>();
                if (!seen.Add(e))
                    return false;
                foreach (var r in ModResolver.For(part).Resolve(e).Structure.OfType<Dom5Edit.Props.Reference>())
                {
                    // a copy of something its part doesn't have finds another part's in the merged file
                    if (!r.TryGetEntity(out var src) || src == null)
                    {
                        if (r is Dom5Edit.Props.StringOrIDRef s && s.ID > 0)
                            return true;
                        continue;
                    }
                    if (src.ParentMod == null || changedBy.TryGetValue((src.Kind, src.ID), out var by) && by.Any(m => !ReferenceEquals(m, part)))
                        return true;
                    if (ReferenceEquals(src.ParentMod, part) && CopiesAcross(part, src, seen))
                        return true;
                }
                return false;
            }
            int compared = 0, differ = 0, shared = 0, across = 0;
            foreach (var ((type, id), list) in touched)
            {
                if (list.Count > 1)
                {
                    shared++;
                    continue;
                }
                var (part, e) = list[0];
                if (CopiesAcross(part, e))
                {
                    across++;
                    continue;
                }
                if (!back.Database[type].TryGetValue(id, out var b))
                {
                    differ++;
                    Console.WriteLine($"   missing in the merged file: {type} {id} ({part.DisplayName})");
                    continue;
                }
                compared++;
                var x = Values(ModResolver.For(part).Resolve(e));
                var y = Values(ModResolver.For(back).Resolve(b));
                if (!x.SequenceEqual(y) && differ++ < 12)
                    Console.WriteLine($"   {type} {id} ({part.DisplayName}): {string.Join(" | ", x.Except(y).Take(3))}  vs  {string.Join(" | ", y.Except(x).Take(3))}");
            }
            // events (no numbers): in order, part by part
            var partEvents = result.Parts.SelectMany(p => p.Database[EntityType.EVENT].GetFullList().Where(x => x.ID < 0)).ToList();
            var backEvents = back.Database[EntityType.EVENT].GetFullList().Where(x => x.ID < 0).ToList();
            int eventsDiffer = 0;
            for (int k = 0; k < Math.Min(partEvents.Count, backEvents.Count); k++)
                if (!Lines(partEvents[k]).SequenceEqual(Lines(backEvents[k])) && eventsDiffer++ < 5)
                    Console.WriteLine($"   event {k + 1}: {string.Join(" | ", Lines(partEvents[k]).Except(Lines(backEvents[k])).Take(3))}  vs  {string.Join(" | ", Lines(backEvents[k]).Except(Lines(partEvents[k])).Take(3))}");
            Console.WriteLine($"read back: {compared} entities compared, {differ} differ; {shared} changed by several parts, {across} copy a game entity another part changes (not compared); " +
                              $"events {backEvents.Count} of {partEvents.Count}, {eventsDiffer} differ");
        }

        private static List<string> Values(ResolvedEntity r) =>
            r.Values.Select(v => $"{v.Command} {ResolvedValue.ArgumentsOf(v.Property)}".Replace("\r\n", "\n")).ToList();

        private static List<string> Lines(IDEntity e) =>
            e.Properties.Select(p => $"{p.Command} {ResolvedValue.ArgumentsOf(p)}".Replace("\r\n", "\n")).ToList();
    }
}
