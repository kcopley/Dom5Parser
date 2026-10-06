using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string testMode = args.Length > 0 ? args[0] : "all";
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");

            switch (testMode.ToLower())
            {
                case "vanilla":
                    TestVanilla(basePath, args.Length > 1 ? args[1] : null);
                    break;
                case "mod":
                    TestMod(basePath, args.Length > 1 ? args[1] : null);
                    break;
                case "roundtrip":
                    RoundTrip(basePath, args);
                    break;
                case "edit":
                    Edit(basePath, args);
                    break;
                case "resolve":
                    Resolve(basePath, args);
                    break;
                case "resolve-dump":
                    ResolveDump(basePath, args);
                    break;
                case "all":
                default:
                    TestVanilla(basePath, null);
                    Console.WriteLine("\n" + new string('=', 60) + "\n");
                    TestMod(basePath, null);
                    break;
            }
        }

        static void TestVanilla(string basePath, string? overridePath)
        {
            Console.WriteLine("=== Vanilla.dm Loading Test ===\n");

            string vanillaDmPath = overridePath ?? Path.Combine(basePath, "vanilla.dm");

            Console.WriteLine($"Looking for vanilla.dm at: {Path.GetFullPath(vanillaDmPath)}");

            if (!File.Exists(vanillaDmPath))
            {
                Console.WriteLine("ERROR: vanilla.dm not found!");
                Console.WriteLine("Usage: Dom5Tests vanilla [path-to-vanilla.dm]");
                return;
            }

            Console.WriteLine("Found vanilla.dm, loading...\n");

            // Configure VanillaLoader for Dom6
            VanillaLoader.GameVersion = GameVersion.Dom6;
            VanillaLoader.VanillaDmPath = vanillaDmPath;

            // Configure spell effect data paths
            string spellMappingPath = Path.Combine(basePath, "spell_effects_mapping.json");
            string spellTypesPath = Path.Combine(basePath, "spell_effect_types.json");
            if (File.Exists(spellMappingPath))
            {
                Console.WriteLine($"Loading spell effect data from: {Path.GetFullPath(spellMappingPath)}");
                VanillaLoader.SpellEffectMappingPath = spellMappingPath;
                VanillaLoader.SpellEffectTypesPath = spellTypesPath;
            }

            VanillaLoader.Reload();

            // Report spell effect data status
            var stats = SpellEffectData.Instance.GetStats();
            if (SpellEffectData.Instance.IsLoaded)
            {
                Console.WriteLine($"Spell effect data loaded: {stats.spellCount} spells, {stats.summonEffects} summon effects, {stats.enchantEffects} enchant effects\n");
            }
            else
            {
                Console.WriteLine($"Spell effect data not loaded (using hardcoded fallback). Error: {SpellEffectData.Instance.LoadError ?? "file not found"}\n");
            }

            try
            {
                // Load with logging enabled
                Mod vanilla = new Mod();
                vanilla.Logging = true;
                vanilla.FullFilePath = vanillaDmPath;
                vanilla.Parse(vanillaDmPath);
                vanilla.Resolve();

                PrintModStats(vanilla, "Vanilla");
                CheckLogFile(vanillaDmPath);
                PrintGameValues(vanilla);
                PrintSampleEntities(vanilla);

                Console.WriteLine("\n=== Vanilla Test Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR during loading: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        static void TestMod(string basePath, string? overridePath)
        {
            Console.WriteLine("=== Mod Loading Test (DomEnhanced2_13.dm) ===\n");

            string modPath = overridePath ?? Path.Combine(basePath, "docs", "DomEnhanced2_13.dm");
            string vanillaDmPath = Path.Combine(basePath, "vanilla.dm");

            Console.WriteLine($"Looking for mod at: {Path.GetFullPath(modPath)}");

            if (!File.Exists(modPath))
            {
                Console.WriteLine("ERROR: DomEnhanced2_13.dm not found!");
                Console.WriteLine("Usage: Dom5Tests mod [path-to-mod.dm]");
                return;
            }

            // Ensure vanilla is loaded first
            if (!File.Exists(vanillaDmPath))
            {
                Console.WriteLine("WARNING: vanilla.dm not found, loading mod without vanilla data");
            }
            else
            {
                Console.WriteLine($"Loading vanilla data from: {Path.GetFullPath(vanillaDmPath)}");
                VanillaLoader.GameVersion = GameVersion.Dom6;
                VanillaLoader.VanillaDmPath = vanillaDmPath;
                VanillaLoader.Reload();
            }

            Console.WriteLine($"\nLoading mod: {Path.GetFileName(modPath)}\n");

            try
            {
                // Load mod with logging enabled
                Mod mod = new Mod();
                mod.Logging = true;
                mod.FullFilePath = modPath;
                mod.Parse(modPath);
                mod.Resolve();

                PrintModStats(mod, "Mod");
                CheckLogFile(modPath);
                PrintParseIssues(mod);
                PrintSampleEntities(mod);

                Console.WriteLine("\n=== Mod Test Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR during loading: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Read-only game values ("-- ro:" lines of the exe-written vanilla data).
        /// </summary>
        static void PrintGameValues(Mod mod)
        {
            int total = 0, entities = 0;
            foreach (var set in mod.Database.Values)
                foreach (var e in set.GetFullList())
                    if (e.GameValues.Count > 0) { entities++; total += e.GameValues.Count; }
            Console.WriteLine($"\nRead-only game values: {total} on {entities} entities");
            if (mod.Database[EntityType.MONSTER].TryGetValue(263, out var m))
                foreach (var v in m.GameValues)
                    Console.WriteLine($"  monster 263 {m.Name}: {v}");
        }

        /// <summary>
        /// Parse issues by type, and the commands the game doesn't read (from the game's parser).
        /// </summary>
        static void PrintParseIssues(Mod mod)
        {
            Console.WriteLine($"\n=== Parse Issues ({mod.ParseIssues.Count} total) ===");
            foreach (var g in mod.ParseIssues.GroupBy(i => i.IssueType).OrderByDescending(g => g.Count()))
                Console.WriteLine($"  {g.Key}: {g.Count()}");
            var notRead = mod.ParseIssues.Where(i => i.IssueType == Dom5Edit.Validation.ParseIssueType.NotReadByGame)
                .GroupBy(i => i.Message).OrderByDescending(g => g.Count()).ToList();
            if (notRead.Count > 0)
            {
                Console.WriteLine("\n  Commands the game doesn't read:");
                foreach (var g in notRead)
                    Console.WriteLine($"    [{g.Count()}x] {g.Key}");
            }
        }

        /// <summary>
        /// Imports a .dm mod and re-exports it, so the round-trip harness can verify
        /// the editor preserves mod data. Usage: Dom5Tests roundtrip &lt;input.dm&gt; &lt;output.dm&gt;
        /// </summary>
        static void RoundTrip(string basePath, string[] args)
        {
            string inputPath = args.Length > 1 ? args[1] : null;
            string outputPath = args.Length > 2 ? args[2] : null;
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine("Usage: Dom5Tests roundtrip <input.dm> <output.dm>");
                Environment.ExitCode = 2;
                return;
            }
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"ERROR: input mod not found: {Path.GetFullPath(inputPath)}");
                Environment.ExitCode = 2;
                return;
            }

            LoadVanillaBase(basePath);

            try
            {
                // Default: save in the file's block order (docs/SAVE_FLOW.md). "canonical": one block
                // per entity by type and ID, with copies normalized unless "nonorm" too.
                bool canonical = args.Any(a => a.Equals("canonical", StringComparison.OrdinalIgnoreCase));
                bool normalize = canonical && !args.Any(a => a.Equals("nonorm", StringComparison.OrdinalIgnoreCase));
                Mod mod = new Mod();
                mod.PreserveSourceOrder = !canonical;
                // "regen": every line regenerated (in file order), to test the export itself
                mod.KeepOriginalText = !args.Any(a => a.Equals("regen", StringComparison.OrdinalIgnoreCase));
                mod.FullFilePath = inputPath;
                mod.Parse(inputPath);
                mod.ResolveDependencies();
                mod.Resolve();
                if (normalize) mod.NormalizeCopies(); // Phase 1: materialize + bake divergent copy values
                mod.Export(outputPath);
                Console.WriteLine($"Round-tripped: {Path.GetFullPath(inputPath)} -> {Path.GetFullPath(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during round-trip: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// Loads vanilla.dm as the dependency base (so copies/inheritance resolve), if present.
        /// </summary>
        static void LoadVanillaBase(string basePath)
        {
            string vanillaDmPath = Path.Combine(basePath, "vanilla.dm");
            if (!File.Exists(vanillaDmPath))
            {
                Console.WriteLine("WARNING: vanilla.dm not found; running without vanilla base.");
                return;
            }
            VanillaLoader.GameVersion = GameVersion.Dom6;
            VanillaLoader.VanillaDmPath = vanillaDmPath;
            string spellMappingPath = Path.Combine(basePath, "spell_effects_mapping.json");
            string spellTypesPath = Path.Combine(basePath, "spell_effect_types.json");
            if (File.Exists(spellMappingPath))
            {
                VanillaLoader.SpellEffectMappingPath = spellMappingPath;
                VanillaLoader.SpellEffectTypesPath = spellTypesPath;
            }
            VanillaLoader.Reload();
        }

        /// <summary>
        /// Imports a mod, applies scripted edits, and re-exports it, so the fidelity suite can
        /// check that the saved data differs from an unedited save by exactly those edits.
        /// Usage: Dom5Tests edit &lt;input.dm&gt; &lt;edits.json&gt; &lt;output.dm&gt;
        ///
        /// edits.json: { "edits": [ { "op": "set"|"add"|"remove"|"change"|"reset", "entity": "monster",
        ///                            "id": 7000, "command": "#hp", "value": "25", "from": "..." }, ... ] }
        /// Edits are the editor's operations (Dom5Edit.Editing.ModEditor): set, add, change (the value
        /// whose arguments are "from" to "value"), reset (drop the entity's own lines), remove (every
        /// value of the command, or those whose arguments are "value", until the entity has none in
        /// game: inherited ones by "#x 0" or a group rewrite). Vanilla entities are selected into the
        /// mod on the first edit (copy-on-write).
        /// </summary>
        static void Edit(string basePath, string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Dom5Tests edit <input.dm> <edits.json> <output.dm>");
                Environment.ExitCode = 2;
                return;
            }
            string inputPath = args[1], editsPath = args[2], outputPath = args[3];
            LoadVanillaBase(basePath);
            try
            {
                Mod mod = new Mod();
                mod.FullFilePath = inputPath;
                mod.Parse(inputPath);
                mod.ResolveDependencies();
                mod.Resolve();

                using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(editsPath));
                int applied = 0;
                var editor = new Dom5Edit.Editing.ModEditor(mod);
                foreach (var edit in doc.RootElement.GetProperty("edits").EnumerateArray())
                {
                    ApplyEdit(editor, edit);
                    applied++;
                }
                mod.Resolve(); // resolve references introduced by the edits
                if (!mod.PreserveSourceOrder)
                    mod.NormalizeCopies(); // the canonical writer re-derives copies; in file order they replay
                mod.Export(outputPath); // the editor's Save is the same call (EditorSession.Save)
                Console.WriteLine($"Edited ({applied} edits): {Path.GetFullPath(inputPath)} -> {Path.GetFullPath(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during edit: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// Prints what entities are in game after a mod (Dom5Edit.Resolve), each value with where it
        /// came from. Usage: Dom5Tests resolve &lt;mod.dm | vanilla&gt; &lt;type&gt; &lt;id&gt; [&lt;type&gt; &lt;id&gt; ...]
        /// </summary>
        static void Resolve(string basePath, string[] args)
        {
            LoadVanillaBase(basePath);
            Mod mod;
            if (args[1] == "vanilla")
                mod = VanillaLoader.Vanilla;
            else
            {
                mod = new Mod { FullFilePath = args[1] };
                mod.Parse(args[1]);
                mod.ResolveDependencies();
                mod.Resolve();
            }
            var resolver = Dom5Edit.Resolve.ModResolver.For(mod);
            for (int i = 2; i + 1 < args.Length; i += 2)
            {
                var type = Enum.Parse<EntityType>(args[i], ignoreCase: true);
                int id = int.Parse(args[i + 1]);
                if (!mod.TryGet(type, id, null, out var entity))
                {
                    Console.WriteLine($"{type} {id}: not found");
                    continue;
                }
                var r = resolver.Resolve(entity);
                Console.WriteLine($"{type} {id} \"{r.Entity.Name}\" ({(r.Entity.ParentMod == mod ? "mod" : "base")}{(r.Vanilla != null ? ", selects vanilla" : "")})");
                foreach (var s in r.Structure)
                    Console.WriteLine($"  structure  {s.ToExportString()}");
                foreach (var v in r.Values)
                {
                    string from = v.Source == Dom5Edit.Resolve.ValueSource.Copied ? $"copied from {v.CopiedFrom?.ID} ({v.Via?.Source})" : v.Source.ToString();
                    Console.WriteLine($"  {v.Property.ToExportString(),-40} {from}");
                }
                foreach (var g in r.GameValues)
                    Console.WriteLine($"  ro: {g.Label} = {g.Value}");
            }
        }

        /// <summary>
        /// Dumps every mod monster's resolved stats, weapons, armor and magic paths as JSON, to
        /// compare with another parser. Usage: Dom5Tests resolve-dump &lt;mod.dm&gt; &lt;out.json&gt;
        /// </summary>
        static void ResolveDump(string basePath, string[] args)
        {
            LoadVanillaBase(basePath);
            var mod = new Mod { FullFilePath = args[1] };
            mod.Parse(args[1]);
            mod.ResolveDependencies();
            mod.Resolve();
            var resolver = Dom5Edit.Resolve.ModResolver.For(mod);
            var stats = new Dictionary<Command, string>
            {
                { Command.HP, "hp" }, { Command.ATT, "att" }, { Command.DEF, "def" }, { Command.PROT, "prot" },
                { Command.MR, "mr" }, { Command.MOR, "mor" }, { Command.STR, "str" }, { Command.PREC, "prec" },
                { Command.ENC, "enc" }, { Command.SIZE, "size" }, { Command.AP, "ap" }, { Command.MAPMOVE, "mapmove" },
            };
            string[] paths = { "F", "A", "W", "E", "S", "D", "N", "G", "B", "H" };
            var all = new SortedDictionary<int, Dictionary<string, object>>();
            foreach (var entity in mod.Database[EntityType.MONSTER].GetFullList())
            {
                var r = resolver.Resolve(entity);
                var d = new Dictionary<string, object>();
                d["name"] = r.Get(Command.NAME) is { } n ? ((NameProperty)n.Property).Value : "";
                foreach (var (c, key) in stats)
                    if (r.Get(c) is { } v)
                        d[key] = v.Arguments;
                int RefId(Property p) => p is StringOrIDRef sr ? sr.ID : -1;
                d["weapons"] = r.GetAll(Command.WEAPON).Select(v => RefId(v.Property).ToString()).ToList();
                d["armor"] = r.GetAll(Command.ARMOR).Select(v => RefId(v.Property).ToString()).ToList();
                foreach (var v in r.GetAll(Command.MAGICSKILL))
                    if (v.Property is IntIntProperty ii && ii.Value1 >= 0 && ii.Value1 < paths.Length)
                        d[paths[ii.Value1]] = ii.Value2.ToString();
                all[entity.ID] = d;
            }
            File.WriteAllText(args[2], System.Text.Json.JsonSerializer.Serialize(all));
            Console.WriteLine($"monsters {all.Count}");
        }

        static void ApplyEdit(Dom5Edit.Editing.ModEditor editor, System.Text.Json.JsonElement edit)
        {
            var mod = editor.Mod;
            string op = edit.GetProperty("op").GetString();
            string kind = edit.GetProperty("entity").GetString();
            int id = edit.GetProperty("id").GetInt32();
            string commandText = edit.GetProperty("command").GetString();
            if (!commandText.StartsWith("#")) commandText = "#" + commandText;
            if (!CommandsMap.TryGetCommand(commandText, out Command command))
                throw new ArgumentException($"Unknown command {commandText}");
            string value = edit.TryGetProperty("value", out var v) ? v.GetString() : null;
            string from = edit.TryGetProperty("from", out var f) ? f.GetString() : null;

            var type = kind switch
            {
                "monster" => EntityType.MONSTER, "weapon" => EntityType.WEAPON, "armor" => EntityType.ARMOR,
                "item" => EntityType.ITEM, "spell" => EntityType.SPELL, "site" => EntityType.SITE,
                "nation" => EntityType.NATION,
                _ => throw new ArgumentException($"Unknown entity kind {kind}"),
            };
            if (!mod.TryGet(type, id, null, out var entity))
                throw new InvalidOperationException($"no {kind} {id}");

            // the editor's operations (Dom5Edit.Editing), as the GUI makes them
            switch (op)
            {
                case "set":
                    editor.Set(entity, command, value ?? "");
                    break;
                case "add":
                    editor.Add(entity, command, value ?? "");
                    break;
                case "remove":
                    // every value of the command (or those with this argument), so the entity no longer has it
                    var removed = editor.Run($"Remove {commandText}", tx =>
                    {
                        int n = 0;
                        while (tx.Resolve(entity).GetAll(command).FirstOrDefault(x => value == null || SameArguments(x.Arguments, value)) is { } match)
                        {
                            tx.Remove(entity, match);
                            if (++n > 100) throw new InvalidOperationException("remove doesn't converge");
                        }
                        if (n == 0) throw new InvalidOperationException($"remove: {kind} {id} has no {commandText} {value}");
                    });
                    break;
                case "change":
                    var target = editor.Resolve(entity).GetAll(command).FirstOrDefault(x => SameArguments(x.Arguments, from))
                                 ?? throw new InvalidOperationException($"change: {kind} {id} has no {commandText} {from}");
                    editor.Change(entity, target, value ?? "");
                    break;
                case "reset":
                    editor.Reset(entity, command);
                    break;
                default:
                    throw new ArgumentException($"Unknown op {op}");
            }
        }

        static bool SameArguments(string a, string b) => a.Trim().Trim('"') == (b ?? "").Trim().Trim('"');

        /// <summary>The argument part of a property's export line, without command or comment.</summary>
        static string ArgumentText(Property p)
        {
            string s = p.ToExportString();
            int comment = s.IndexOf(" -- ", StringComparison.Ordinal);
            if (comment >= 0) s = s.Substring(0, comment);
            int space = s.IndexOf(' ');
            return space < 0 ? "" : s.Substring(space + 1).Trim().Trim('"');
        }

        static void PrintModStats(Mod mod, string label)
        {
            Console.WriteLine($"=== {label} Loading Complete ===\n");

            // Print statistics
            Console.WriteLine("Entity counts:");
            foreach (var kvp in mod.Database)
            {
                var list = kvp.Value.GetFullList();
                if (list.Count > 0)
                {
                    Console.WriteLine($"  {kvp.Key}: {list.Count}");
                }
            }

            Console.WriteLine("\nDependent entity counts:");
            foreach (var kvp in mod.Dependents)
            {
                if (kvp.Value.Count > 0)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value.Count}");
                }
            }
        }

        static void CheckLogFile(string dmPath)
        {
            string logFile = dmPath.Replace(".dm", "-log.txt");
            if (File.Exists(logFile))
            {
                string logContent = File.ReadAllText(logFile);
                if (!string.IsNullOrWhiteSpace(logContent))
                {
                    var lines = logContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine($"\n=== Parsing Warnings ({lines.Length} total) ===");

                    // Group and count similar errors
                    var errorCounts = new Dictionary<string, int>();
                    foreach (var line in lines)
                    {
                        // Extract error type
                        string key = line.Trim();
                        if (key.Contains("not resolved for:"))
                        {
                            key = key.Substring(0, key.IndexOf("not resolved for:") + "not resolved for:".Length) + " [ID]";
                        }
                        else if (key.Contains("not known for"))
                        {
                            key = key.Substring(0, key.IndexOf("not known for") + "not known for".Length) + " [Entity]";
                        }

                        if (errorCounts.ContainsKey(key))
                            errorCounts[key]++;
                        else
                            errorCounts[key] = 1;
                    }

                    foreach (var kvp in errorCounts.OrderByDescending(x => x.Value).Take(10))
                    {
                        Console.WriteLine($"  [{kvp.Value}x] {kvp.Key}");
                    }

                    if (errorCounts.Count > 10)
                    {
                        Console.WriteLine($"  ... and {errorCounts.Count - 10} more error types");
                    }
                }
                else
                {
                    Console.WriteLine("\n=== No parsing errors! ===");
                }
            }
            else
            {
                Console.WriteLine("\n=== No log file created (no errors) ===");
            }
        }

        static void PrintSampleEntities(Mod mod)
        {
            Console.WriteLine("\n=== Sample Entities ===");

            var monsters = mod.Database[EntityType.MONSTER].GetFullList();
            if (monsters.Count > 0)
            {
                Console.WriteLine($"\nFirst 5 monsters:");
                foreach (var m in monsters.Take(5))
                {
                    Console.WriteLine($"  [{m.ID}] {m.DisplayName}");
                }

                // Find monsters with magic skills
                var mages = monsters.Cast<Monster>()
                    .Where(m => m.MagicSkills.Any())
                    .Take(5)
                    .ToList();
                if (mages.Count > 0)
                {
                    Console.WriteLine($"\nFirst 5 mages (with magic skills):");
                    foreach (var m in mages)
                    {
                        var skills = string.Join(", ", m.MagicSkills.Select(s => $"{s.Path}:{s.Level}"));
                        Console.WriteLine($"  [{m.ID}] {m.DisplayName} - {skills}");
                    }
                }
                else
                {
                    Console.WriteLine($"\nNo monsters with magic skills found!");
                }
            }

            var weapons = mod.Database[EntityType.WEAPON].GetFullList();
            if (weapons.Count > 0)
            {
                Console.WriteLine($"\nFirst 5 weapons:");
                foreach (var w in weapons.Take(5))
                {
                    Console.WriteLine($"  [{w.ID}] {w.DisplayName}");
                }
            }

            var spells = mod.Database[EntityType.SPELL].GetFullList();
            if (spells.Count > 0)
            {
                Console.WriteLine($"\nFirst 5 spells:");
                foreach (var s in spells.Take(5))
                {
                    Console.WriteLine($"  [{s.ID}] {s.DisplayName}");
                }
            }

            var nations = mod.Database[EntityType.NATION].GetFullList();
            if (nations.Count > 0)
            {
                Console.WriteLine($"\nFirst 5 nations:");
                foreach (var n in nations.Take(5))
                {
                    Console.WriteLine($"  [{n.ID}] {n.DisplayName}");
                }
            }
        }
    }
}
