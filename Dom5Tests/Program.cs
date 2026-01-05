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
