using Dom5Edit;
using Dom5Edit.Entities;
using Dom5Edit.Merge;
using Dom5Edit.Resolve;

namespace Dom5Tests
{
    /// <summary>
    /// Dom5Tests renumber MOD.dm OUT.dm TYPE OLD NEW [TYPE OLD NEW]...: moves entities to other
    /// numbers (Merge.Renumbering), saves, reads the save back and compares every entity's values
    /// with the moved model's; prints the lines that changed. docs/MERGING.md, step 1.
    /// </summary>
    internal static class RenumberCheck
    {
        public static void Run(string basePath, string[] args)
        {
            Program.LoadVanillaBase(basePath);
            var mod = Mod.Import(args[1]);
            var outPath = args[2];
            for (int i = 3; i + 2 < args.Length; i += 3)
            {
                var type = Enum.Parse<EntityType>(args[i], true);
                int old = int.Parse(args[i + 1]), now = int.Parse(args[i + 2]);
                if (!mod.Database[type].TryGetValue(old, out var e) || e.ParentMod != mod)
                    throw new ArgumentException($"{mod.DisplayName} has no {type} {old} of its own");
                Renumbering.Move(e, now, new[] { mod });
                Console.WriteLine($"moved {type} {old} -> {now} ({e.Name})");
            }
            mod.Export(outPath);
            // the lines the save changed
            var before = File.ReadAllLines(args[1]);
            var after = File.ReadAllLines(outPath);
            int changed = 0;
            for (int k = 0; k < Math.Min(before.Length, after.Length); k++)
                if (before[k] != after[k] && changed++ < 30)
                    Console.WriteLine($"   line {k + 1}: {before[k].Trim()}  ->  {after[k].Trim()}");
            Console.WriteLine($"{changed} lines changed ({before.Length} -> {after.Length} lines)");
            // read back: every entity the same as the moved model
            var back = Mod.Import(outPath);
            int entities = 0, differ = 0;
            foreach (var (type, set) in mod.Database)
                foreach (var e in set.GetFullList().Where(x => x.ID > 0 && set.TryGetValue(x.ID, out var held) && ReferenceEquals(held, x)))
                {
                    entities++;
                    if (!back.Database[type].TryGetValue(e.ID, out var b))
                    {
                        differ++;
                        Console.WriteLine($"   missing after reading back: {type} {e.ID}");
                        continue;
                    }
                    var x = Values(ModResolver.For(mod).Resolve(e));
                    var y = Values(ModResolver.For(back).Resolve(b));
                    if (!x.SequenceEqual(y) && differ++ < 10)
                        Console.WriteLine($"   {type} {e.ID}: {string.Join(" | ", x.Except(y).Take(3))}  vs  {string.Join(" | ", y.Except(x).Take(3))}");
                }
            Console.WriteLine($"read back: {entities} entities, {differ} differ");
        }

        private static List<string> Values(ResolvedEntity r) =>
            r.Values.Select(v => ResolvedValue.ArgumentsOf(v.Property) is var a ? $"{v.Command} {a}" : "").ToList();
    }
}
