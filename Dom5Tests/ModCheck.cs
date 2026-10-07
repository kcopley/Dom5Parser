using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Derived;
using Dom5Edit.Entities;
using Dom5Edit.Resolve;
using Dom5Edit.Validation;

namespace Dom5Tests
{
    /// <summary>
    /// Dom5Tests check MOD OUTDIR: everything the editor would make of a mod, in one load. How long
    /// it takes; what it holds; what the parser flagged (unknown commands, lines the game doesn't
    /// read, duplicates) and what Validate reports (missing references, ID ranges), grouped with an
    /// example of each; the event chains' problems; whether every entity resolves and every
    /// monster's game values compute; and whether a save without edits gives back the file byte
    /// for byte (OUTDIR/NAME.save.dm), plus a save with every line regenerated (NAME.regen.dm) for
    /// the independent parser to compare (tools/fidelity, docs/ROUND_TRIP_TESTING.md).
    /// --with OTHER.dm (repeatable) loads a mod this one needs (a submod's parent) alongside, so
    /// references to it resolve.
    /// </summary>
    static class ModCheck
    {
        public static void Run(string basePath, string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: Dom5Tests check <mod.dm> <outdir> [--with <needed.dm>]...");
                Environment.ExitCode = 2;
                return;
            }
            string path = args[1], outDir = args[2];
            Directory.CreateDirectory(outDir);
            var name = Path.GetFileNameWithoutExtension(path);
            Program.LoadVanillaBase(basePath);
            // mods this one needs, loaded alongside
            var needed = new List<Mod>();
            for (int i = 3; i + 1 < args.Length; i++)
                if (args[i] == "--with")
                {
                    var other = new Mod { FullFilePath = args[++i] };
                    other.Parse(other.FullFilePath);
                    other.ResolveDependencies();
                    other.Resolve();
                    needed.Add(other);
                }

            var watch = Stopwatch.StartNew();
            var mod = new Mod { FullFilePath = path };
            mod.Parse(path);
            long parsed = watch.ElapsedMilliseconds;
            mod.ResolveDependencies();
            mod.Dependencies.AddRange(needed);
            mod.Resolve();
            long resolved = watch.ElapsedMilliseconds;
            Console.WriteLine($"== {name}  ({new FileInfo(path).Length / 1024} KB; parse {parsed} ms, resolve {resolved - parsed} ms)");

            // what it holds
            var counts = mod.Database.Where(kv => kv.Value.GetFullList().Count > 0)
                .Select(kv => (kv.Key, All: kv.Value.GetFullList(), New: kv.Value.GetFullList().Count(e => !e.Selected)))
                .OrderByDescending(x => x.All.Count);
            Console.WriteLine("   holds: " + string.Join(", ", counts.Select(c => $"{c.Key.ToString().ToLowerInvariant()} {c.All.Count} ({c.New} new)")));

            // the parser's notes
            Console.WriteLine($"   parser notes: {mod.ParseIssues.Count}");
            foreach (var g in mod.ParseIssues.GroupBy(i => (i.IssueType, Key: Shape(i.Message))).OrderByDescending(g => g.Count()).Take(12))
            {
                var first = g.First();
                Console.WriteLine($"     {g.Count(),5} x {g.Key.IssueType}: {Trim(first.Message, 110)}  (line {first.LineNumber}: {Trim(first.RawContent?.Trim(), 70)})");
            }

            // Validate, as the editor's button runs it
            var watchValidate = Stopwatch.StartNew();
            var result = new ModValidator().ValidateWithSummary(mod);
            Console.WriteLine($"   validate: {result.ErrorCount} errors, {result.WarningCount} warnings, {result.InfoCount} notes ({watchValidate.ElapsedMilliseconds} ms)");
            foreach (var g in result.Issues.Where(i => i.Severity != ValidationSeverity.Info)
                         .GroupBy(i => (i.Severity, i.Category, Key: Shape(i.Message))).OrderBy(g => g.Key.Severity).ThenByDescending(g => g.Count()).Take(14))
            {
                var first = g.First();
                Console.WriteLine($"     {g.Count(),5} x {g.Key.Severity} [{g.Key.Category}] {Trim(first.Message, 120)}" + (first.LineNumber is int l ? $"  (line {l})" : ""));
            }

            // every entity resolves; every monster's game values compute
            var resolver = ModResolver.For(mod);
            int entities = 0, failed = 0;
            string? firstFailure = null;
            var watchResolve = Stopwatch.StartNew();
            foreach (var (type, set) in mod.Database)
                foreach (var e in set.GetFullList())
                {
                    entities++;
                    try
                    {
                        var r = resolver.Resolve(e);
                        if (type == EntityType.MONSTER)
                            UnitTotals.Compute(UnitStats.Of(r, (t, id) => id > 0 && mod.TryGet(t, id, null, out var x) ? resolver.Resolve(x) : null));
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        firstFailure ??= $"{type} #{e.ID}: {ex.GetType().Name}: {ex.Message}";
                    }
                }
            Console.WriteLine($"   resolve + game values: {entities} entities, {failed} failed ({watchResolve.ElapsedMilliseconds} ms)" + (firstFailure != null ? $"; first: {firstFailure}" : ""));

            // the event chains
            Dom5Edit.Events.EventGraph? graph = null;
            try
            {
                graph = Dom5Edit.Events.EventGraph.Build(mod, resolver.Resolve, VanillaLoader.Vanilla);
                Console.WriteLine($"   events: {graph.ModEvents.Count} in the mod, {graph.Chains.Count} chains, {graph.Problems.Count} problems "
                                  + $"({graph.Problems.Count(p => p.IsError)} errors)");
                foreach (var g in graph.Problems.GroupBy(p => (p.IsError, Key: Shape(p.Message))).OrderByDescending(g => g.Key.IsError).ThenByDescending(g => g.Count()).Take(8))
                    Console.WriteLine($"     {g.Count(),5} x {(g.Key.IsError ? "Error" : "Warning")}: {Trim(g.First().Message, 130)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   events: FAILED {ex.GetType().Name}: {ex.Message}");
            }

            // the report for the mod's author (as the editor's Validate window saves it)
            var report = Path.Combine(outDir, name + ".report.md");
            File.WriteAllText(report, ModReport.Write(mod, result, graph?.Problems));
            Console.WriteLine($"   report: {report}");

            // a save without edits gives back the file
            var save = Path.Combine(outDir, name + ".save.dm");
            mod.Export(save);
            try
            {
                SaveCheck.Verify(mod, save);
                Console.WriteLine("   save check (reads back with the same entities): ok");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"   save check: FAILED {ex.Message}");
            }
            var original = File.ReadAllBytes(path);
            var saved = File.ReadAllBytes(save);
            if (original.AsSpan().SequenceEqual(saved))
                Console.WriteLine("   save without edits: byte-identical");
            else
            {
                var a = Encoding.UTF8.GetString(original).Replace("\r\n", "\n").Split('\n');
                var b = Encoding.UTF8.GetString(saved).Replace("\r\n", "\n").Split('\n');
                int i = 0;
                while (i < Math.Min(a.Length, b.Length) && a[i] == b[i])
                    i++;
                bool endings = Encoding.UTF8.GetString(original).Replace("\r\n", "\n") == Encoding.UTF8.GetString(saved).Replace("\r\n", "\n");
                Console.WriteLine(endings ? "   save without edits: same text, line endings differ"
                    : $"   save without edits: DIFFERS ({a.Length} -> {b.Length} lines; first at line {i + 1}: \"{Trim(i < a.Length ? a[i] : "<end>", 80)}\" -> \"{Trim(i < b.Length ? b[i] : "<end>", 80)}\")");
            }

            // every line regenerated, for the independent parser
            var regen = new Mod { FullFilePath = path, KeepOriginalText = false };
            regen.Parse(path);
            regen.ResolveDependencies();
            regen.Dependencies.AddRange(needed);
            regen.Resolve();
            regen.Export(Path.Combine(outDir, name + ".regen.dm"));
        }

        /// <summary>A message with its numbers and quoted names blanked, to group alike ones.</summary>
        private static string Shape(string? message) =>
            Regex.Replace(Regex.Replace(message ?? "", "\"[^\"]*\"", "\"…\""), @"-?\d+", "N");

        private static string Trim(string? s, int max) => s == null ? "" : s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
