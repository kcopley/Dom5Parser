using System;
using System.IO;
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
                bool normalize = !args.Any(a => a.Equals("nonorm", StringComparison.OrdinalIgnoreCase));
                Mod mod = new Mod();
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
        /// edits.json: { "edits": [ { "op": "set"|"add"|"remove", "entity": "monster", "id": 7000,
        ///                            "command": "#hp", "value": "25" }, ... ] }
        /// Edits go through the same core calls as the editor's edit commands: set = Set&lt;IntProperty&gt;
        /// for int commands (SetIntPropertyCommand), otherwise replace; add = AddProperty;
        /// remove = RemoveProperty (all properties with that command, or only those whose
        /// argument equals "value"). Vanilla entities are selected into the mod (copy-on-write).
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
                foreach (var edit in doc.RootElement.GetProperty("edits").EnumerateArray())
                {
                    ApplyEdit(mod, edit);
                    applied++;
                }
                mod.Resolve(); // resolve references introduced by the edits
                mod.NormalizeCopies();
                mod.Export(outputPath);
                Console.WriteLine($"Edited ({applied} edits): {Path.GetFullPath(inputPath)} -> {Path.GetFullPath(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during edit: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }

        static void ApplyEdit(Mod mod, System.Text.Json.JsonElement edit)
        {
            string op = edit.GetProperty("op").GetString();
            string kind = edit.GetProperty("entity").GetString();
            int id = edit.GetProperty("id").GetInt32();
            string commandText = edit.GetProperty("command").GetString();
            if (!commandText.StartsWith("#")) commandText = "#" + commandText;
            if (!CommandsMap.TryGetCommand(commandText, out Command command))
                throw new ArgumentException($"Unknown command {commandText}");
            string value = edit.TryGetProperty("value", out var v) ? v.GetString() : null;
            // The parser strips the quotes around string arguments before entities see them.
            if (value != null && value.Length >= 2 && value[0] == '"' && value[^1] == '"') value = value[1..^1];

            IDEntity entity = kind switch
            {
                "monster" => mod.SelectForEdit<Monster>(id),
                "weapon" => mod.SelectForEdit<Weapon>(id),
                "armor" => mod.SelectForEdit<Armor>(id),
                "item" => mod.SelectForEdit<Item>(id),
                "spell" => mod.SelectForEdit<Spell>(id),
                "site" => mod.SelectForEdit<Site>(id),
                "nation" => mod.SelectForEdit<Nation>(id),
                _ => throw new ArgumentException($"Unknown entity kind {kind}"),
            };

            switch (op)
            {
                case "set":
                    bool isInt = entity.GetPropertyMap().TryGetValue(command, out var create)
                        && create().GetType() == typeof(IntProperty);
                    if (isInt && int.TryParse(value, out int intValue))
                    {
                        entity.Set<IntProperty>(command, p => p.Value = intValue);
                    }
                    else
                    {
                        foreach (var p in entity.Properties.Where(p => p.Command == command).ToList())
                            entity.RemoveProperty(p);
                        entity.Parse(command, value ?? "", "");
                    }
                    break;
                case "add":
                    entity.Parse(command, value ?? "", "");
                    break;
                case "remove":
                    var matches = entity.Properties
                        .Where(p => p.Command == command && (value == null || ArgumentText(p) == value))
                        .ToList();
                    if (matches.Count == 0)
                        throw new InvalidOperationException($"remove: {kind} {id} has no {commandText} {value}");
                    foreach (var p in matches) entity.RemoveProperty(p);
                    break;
                default:
                    throw new ArgumentException($"Unknown op {op}");
            }
        }

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
