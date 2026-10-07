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
                case "events":
                    Events(basePath, args);
                    break;
                case "derived-check":
                    DerivedCheck.Run(basePath, args);
                    break;
                case "derived":
                    DerivedCheck.Print(basePath, args);
                    break;
                case "check":
                    ModCheck.Run(basePath, args);
                    break;
                case "usage":
                    UsageSurvey.Run(basePath, args);
                    break;
                case "texts":
                    Texts(basePath, args);
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
        internal static void LoadVanillaBase(string basePath)
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
        /// Usage: Dom5Tests edit &lt;input.dm&gt; &lt;edits.json&gt; &lt;output.dm&gt; [undo]
        ///
        /// edits.json: { "edits": [ { "op": "set"|"add"|"remove"|"change"|"reset"|"text"|"create"|"delete"|"move"|"info",
        ///                            "entity": "monster", "id": 7000, "command": "#hp", "value": "25", "from": "..." }, ... ],
        ///               "reread": [ ... ] }
        /// Edits are the editor's operations (Dom5Edit.Editing.ModEditor): set, add, change (the value
        /// whose arguments are "from" to "value"), reset (drop the entity's own lines), remove (every
        /// value of the command, or those whose arguments are "value", until the entity has none in
        /// game: inherited ones by "#x 0" or a group rewrite), text (the block edited as text, with
        /// "replace" pairs), create (a new entity, "as" names it for later edits), delete, move (one
        /// of the entity's own lines, by "delta" or to just "before" another command's line), info
        /// (a header field: #modname, #description, ...). "fails": "text" on an edit: the editor must
        /// refuse it with that in its message. "@label" in a value: a created entity's number.
        /// Vanilla entities are selected into the mod on the first edit (copy-on-write).
        /// Entities: by "id"; an entity with no number (#newevent, #newmerc) by "index" (the mod's
        /// Nth one in file order, 0 first) or "match" (the one event whose #msg contains the text;
        /// another entity by name, as a #select by name finds it); a created one by "ref". The
        /// numbers created entities got are written to &lt;output&gt;.created.json.
        ///
        /// "undo": after saving, every edit is undone (newest first) and the mod saved to
        /// &lt;output&gt;.undo.dm, then redone and saved to &lt;output&gt;.redo.dm: the suite checks they
        /// equal the unedited and the edited save byte for byte.
        /// "reread" (Reread): values checked by re-reading the saved file with Dom5Parser, for
        /// commands the oracle doesn't read; exit code 3 if one differs.
        /// </summary>
        static void Edit(string basePath, string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Dom5Tests edit <input.dm> <edits.json> <output.dm> [undo]");
                Environment.ExitCode = 2;
                return;
            }
            string inputPath = args[1], editsPath = args[2], outputPath = args[3];
            bool undoCheck = args.Skip(4).Any(a => a.Equals("undo", StringComparison.OrdinalIgnoreCase));
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
                var history = new List<Dom5Edit.Editing.IModEdit>();
                var created = new Dictionary<string, IDEntity>();
                foreach (var edit in doc.RootElement.GetProperty("edits").EnumerateArray())
                {
                    // "fails": the editor must refuse the edit (EditException with this text) and change nothing
                    string? fails = edit.TryGetProperty("fails", out var fl) ? fl.GetString() : null;
                    try
                    {
                        if (ApplyEdit(editor, edit, created) is { } done)
                            history.Add(done);
                        if (fails != null)
                            throw new InvalidOperationException($"the editor made it; expected it refused (\"{fails}\")");
                    }
                    catch (Dom5Edit.Editing.EditException ex) when (fails != null && ex.Message.Contains(fails, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"  refused as expected: {ex.Message}");
                    }
                    catch (Exception ex) when (ex is Dom5Edit.Editing.EditException || ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
                    {
                        throw new InvalidOperationException($"edit {applied + 1} {edit.GetRawText()}: {ex.Message}", ex);
                    }
                    applied++;
                }
                Save(mod, outputPath); // the editor's Save is the same call (EditorSession.Save)
                Console.WriteLine($"Edited ({applied} edits, {history.Count} undo steps): {Path.GetFullPath(inputPath)} -> {Path.GetFullPath(outputPath)}");
                // the numbers created entities got, for expectations that name them by "ref"
                File.WriteAllText(Path.ChangeExtension(outputPath, ".created.json"), System.Text.Json.JsonSerializer.Serialize(
                    created.ToDictionary(x => x.Key, x => x.Value.ID)));
                if (undoCheck)
                {
                    for (int i = history.Count - 1; i >= 0; i--)
                        history[i].Undo();
                    Save(mod, SideFile(outputPath, "undo"));
                    foreach (var e in history)
                        e.Redo();
                    Save(mod, SideFile(outputPath, "redo"));
                    Console.WriteLine($"Undone and redone: {SideFile(outputPath, "undo")}, {SideFile(outputPath, "redo")}");
                }
                if (doc.RootElement.TryGetProperty("reread", out var reread) && !Reread(outputPath, reread))
                    Environment.ExitCode = 3;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during edit: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }

        static void Save(Mod mod, string path)
        {
            mod.Resolve(); // resolve references introduced by the edits
            if (!mod.PreserveSourceOrder)
                mod.NormalizeCopies(); // the canonical writer re-derives copies; in file order they replay
            mod.Export(path);
        }

        /// <summary>out.dm -> out.undo.dm</summary>
        static string SideFile(string path, string tag) =>
            Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path) + "." + tag + Path.GetExtension(path));

        /// <summary>
        /// Re-reads a saved mod with Dom5Parser (parse, resolve) and checks values the oracle doesn't
        /// read (a nation's start army and defenders, nametypes, poptypes, blesses) or can't see
        /// (the order of an event's lines). Each check addresses an entity like an edit does and has
        /// "command" with "values" (every value the entity has for it in game, in order: arguments as
        /// written, a reference as its ID) and/or "includes"/"excludes" (values that must or mustn't
        /// be among them), or "order" (the commands of the entity's own lines as
        /// saved, in order; a subsequence check: other lines may come between). Not independent of
        /// Dom5Parser: it shows the edit reached the file and reads back as made.
        /// </summary>
        static bool Reread(string path, System.Text.Json.JsonElement checks)
        {
            var mod = new Mod { FullFilePath = path };
            mod.Parse(path);
            mod.ResolveDependencies();
            mod.Resolve();
            var editor = new Dom5Edit.Editing.ModEditor(mod);
            bool ok = true;
            foreach (var check in checks.EnumerateArray())
            {
                string what = check.GetRawText();
                string? problem = null;
                try
                {
                    var entity = FindEntity(editor, check, null);
                    if (check.TryGetProperty("command", out var ct))
                    {
                        var command = CommandOf(ct.GetString()!);
                        var actual = editor.Resolve(entity).GetAll(command).Select(v => v.Property).ToList();
                        string Shown() => string.Join(", ", actual.Select(p => ResolvedArguments(p)));
                        if (check.TryGetProperty("values", out var values) && values.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var expected = values.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
                            if (actual.Count != expected.Count || !actual.Zip(expected).All(x => SameValue(x.First, x.Second)))
                                problem = $"values [{Shown()}], expected [{string.Join(", ", expected)}]";
                        }
                        // "includes": values that must be among them (a list the game or the mod has more of)
                        if (check.TryGetProperty("includes", out var includes))
                        {
                            var wanted = includes.ValueKind == System.Text.Json.JsonValueKind.Array
                                ? includes.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                                : new List<string> { includes.GetString() ?? "" };
                            var absent = wanted.Where(w => !actual.Any(p => SameValue(p, w))).ToList();
                            if (absent.Count > 0)
                                problem = $"no {string.Join(", ", absent)} among [{Shown()}]";
                        }
                        // "excludes": values that must not be among them
                        if (check.TryGetProperty("excludes", out var excludes))
                        {
                            var present = excludes.EnumerateArray().Select(x => x.GetString() ?? "").Where(w => actual.Any(p => SameValue(p, w))).ToList();
                            if (present.Count > 0)
                                problem = $"{string.Join(", ", present)} still among [{Shown()}]";
                        }
                    }
                    if (check.TryGetProperty("order", out var order))
                    {
                        var text = editor.BlockText(entity) ?? "";
                        var commands = text.Split('\n').Select(l => l.Trim()).Where(l => l.StartsWith("#"))
                            .Select(l => l.Split(' ', '\t')[0].ToLowerInvariant()).ToList();
                        int at = 0;
                        foreach (var want in order.EnumerateArray().Select(x => x.GetString()!.ToLowerInvariant()))
                        {
                            at = commands.IndexOf(want.StartsWith("#") ? want : "#" + want, at);
                            if (at < 0)
                            {
                                problem = $"lines in order {string.Join(" ", commands)}, expected {string.Join(" ", order.EnumerateArray().Select(x => x.GetString()))} among them";
                                break;
                            }
                            at++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    problem = ex.Message;
                }
                Console.WriteLine(problem == null ? $"  reread ok    {what}" : $"  reread FAIL  {what}: {problem}");
                ok &= problem == null;
            }
            return ok;
        }

        /// <summary>A value's arguments as the game reads them; a reference also as the ID it resolves to.</summary>
        static bool SameValue(Property p, string expected) =>
            SameArguments(Dom5Edit.Resolve.ResolvedValue.ArgumentsOf(p), expected)
            || p is Reference r && r.TryGetEntity(out var target) && target != null && target.ID.ToString() == expected.Trim();

        static string ResolvedArguments(Property p) =>
            Dom5Edit.Resolve.ResolvedValue.ArgumentsOf(p) + (p is Reference r && r.TryGetEntity(out var t) && t != null ? $" (= {t.ID})" : "");

        static Command CommandOf(string text)
        {
            if (!text.StartsWith("#")) text = "#" + text;
            if (!CommandsMap.TryGetCommand(text, out Command command))
                throw new ArgumentException($"Unknown command {text}");
            return command;
        }

        static readonly Dictionary<string, EntityType> Kinds = new Dictionary<string, EntityType>
        {
            { "monster", EntityType.MONSTER }, { "weapon", EntityType.WEAPON }, { "armor", EntityType.ARMOR },
            { "item", EntityType.ITEM }, { "spell", EntityType.SPELL }, { "site", EntityType.SITE },
            { "nation", EntityType.NATION }, { "merc", EntityType.MERCENARY }, { "mercenary", EntityType.MERCENARY },
            { "nametype", EntityType.NAMETYPE }, { "event", EntityType.EVENT }, { "poptype", EntityType.POPTYPE },
            { "bless", EntityType.BLESS }, { "template", EntityType.TEMPLATE },
        };

        static EntityType KindOf(System.Text.Json.JsonElement spec)
        {
            string kind = spec.GetProperty("entity").GetString()!;
            return Kinds.TryGetValue(kind, out var type) ? type : throw new ArgumentException($"Unknown entity kind {kind}");
        }

        /// <summary>The entity an edit or check is about (see Edit: id, index, match, ref).</summary>
        static IDEntity FindEntity(Dom5Edit.Editing.ModEditor editor, System.Text.Json.JsonElement spec, Dictionary<string, IDEntity>? created)
        {
            var mod = editor.Mod;
            var type = KindOf(spec);
            string kind = spec.GetProperty("entity").GetString()!;
            if (spec.TryGetProperty("ref", out var r))
                return created != null && created.TryGetValue(r.GetString()!, out var made) ? made
                    : throw new InvalidOperationException($"no created {kind} \"{r.GetString()}\"");
            if (spec.TryGetProperty("index", out var ix))
            {
                // the mod's entities without a number (#newevent, #newmerc), in file order
                var own = mod.Database[type].GetFullList().Where(e => e.ParentMod == mod && e.ID <= 0 && !e.Selected).ToList();
                int i = ix.GetInt32();
                return i >= 0 && i < own.Count ? own[i] : throw new InvalidOperationException($"no {kind} at index {i} (the mod has {own.Count})");
            }
            if (spec.TryGetProperty("match", out var m))
            {
                string text = m.GetString()!;
                // another entity by name, as a #select by name finds it (the lowest ID with the name)
                if (type != EntityType.EVENT)
                    return mod.TryGet(type, -1, text, out var named) ? named : throw new InvalidOperationException($"no {kind} named \"{text}\"");
                var found = mod.Database[type].GetFullList().Where(e => e.ParentMod == mod && Matches(editor, e, type, text)).ToList();
                return found.Count == 1 ? found[0] : throw new InvalidOperationException($"{found.Count} {kind}s match \"{text}\"");
            }
            int id = spec.GetProperty("id").GetInt32();
            if (!mod.TryGet(type, id, null, out var entity))
                throw new InvalidOperationException($"no {kind} {id}");
            return entity;
        }

        /// <summary>An event whose #msg contains the text.</summary>
        static bool Matches(Dom5Edit.Editing.ModEditor editor, IDEntity e, EntityType type, string text) =>
            editor.Resolve(e).Get(Command.MSG)?.Property is StringProperty msg && msg.Value != null && msg.Value.Contains(text, StringComparison.Ordinal);

        /// <summary>
        /// Prints what entities are in game after a mod (Dom5Edit.Resolve), each value with where it
        /// came from. Usage: Dom5Tests resolve &lt;mod.dm | vanilla&gt; &lt;type&gt; &lt;id&gt; [&lt;type&gt; &lt;id&gt; ...]
        /// </summary>
        static void Resolve(string basePath, string[] args)
        {
            LoadVanillaBase(basePath);
            // --with NEEDED.dm (repeatable): mods read first, the mod over them
            var needed = new List<string>();
            for (int i = 2; i + 1 < args.Length; i++)
                if (args[i] == "--with")
                {
                    needed.Add(args[i + 1]);
                    args = args.Take(i).Concat(args.Skip(i + 2)).ToArray();
                    i--;
                }
            Mod mod;
            if (args[1] == "vanilla")
                mod = VanillaLoader.Vanilla;
            else
                mod = Mod.Import(args[1], Mod.ImportStack(needed));
            var resolver = Dom5Edit.Resolve.ModResolver.For(mod);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (mod.Database[EntityType.MONSTER].GetFullList().FirstOrDefault() is { } any)
                resolver.Resolve(any);
            Console.WriteLine($"(first replay: {watch.ElapsedMilliseconds} ms)");
            watch.Restart();
            var plan = new SavePlan(mod);
            long planMs = watch.ElapsedMilliseconds;
            int lines = plan.Blocks().Sum(b => b.Lines.Count);
            Console.WriteLine($"(save plan: {planMs} ms, blocks: {watch.ElapsedMilliseconds - planMs} ms, {lines} lines)");
            for (int k = 0; k < 3; k++)
            {
                watch.Restart();
                resolver.Invalidate();
                if (mod.Database[EntityType.MONSTER].GetFullList().FirstOrDefault() is { } again)
                    resolver.Resolve(again);
                Console.WriteLine($"(replay after an edit: {watch.ElapsedMilliseconds} ms)");
            }
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
        /// <summary>
        /// Dom5Tests texts [out.dm]: the game's texts as the editor reads them from the player's exe
        /// (Dom5Edit.GameData.VanillaTexts): how many of each kind, against what tools/dom6exe found
        /// in the same exe; a few samples; and that they're shown, not saved: a mod that sets a
        /// vanilla nation's summary and a monster's hp saves just those lines.
        /// </summary>
        static void Texts(string basePath, string[] args)
        {
            LoadVanillaBase(basePath);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var vanilla = VanillaLoader.Vanilla;
            Console.WriteLine($"vanilla loaded in {watch.ElapsedMilliseconds} ms");
            Console.WriteLine("  texts: " + VanillaLoader.TextsStatus);
            int failures = 0;
            void Check(bool ok, string what)
            {
                Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
                if (!ok) failures++;
            }
            string? Asset(EntityType type, int id, Command c) =>
                vanilla.Database[type].TryGetValue(id, out var e) ? e.Properties.OfType<StringProperty>().FirstOrDefault(p => p.IsDisplayAsset && p.Command == c)?.Value : null;

            // per kind, as many as tools/dom6exe found in the exe's own tables
            var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(basePath, "tools", "dom6exe", "data", "texts-6.37.json")));
            foreach (var f in json.RootElement.GetProperty("found").EnumerateObject())
            {
                var parts = f.Name.Split(' ');
                var type = vanilla.Database.Keys.First(t => Dom5Edit.GameData.GameCommandCatalog.ContextOf(t) == parts[0]);
                CommandsMap.TryGetCommand("#" + parts[1], out var command);
                int n = vanilla.Database[type].GetFullList().Count(e => e.Properties.Any(p => p.IsDisplayAsset && p.Command == command));
                Check(n == f.Value.GetInt32(), $"{f.Name}: {n} (tools/dom6exe: {f.Value.GetInt32()})");
            }
            Check(Asset(EntityType.MONSTER, 20, Command.DESCR)?.Length > 100, "monster 20 has a description");
            foreach (var c in new[] { Command.DESCR, Command.SUMMARY, Command.BRIEF })
                Check(Asset(EntityType.NATION, 5, c)?.Length > 20, $"nation 5 has a {c.ToString().ToLowerInvariant()}");

            // shown, never saved
            string dir = Path.Combine(Path.GetTempPath(), "dom5tests-texts");
            Directory.CreateDirectory(dir);
            string modPath = Path.Combine(dir, "texts.dm");
            File.WriteAllText(modPath, "#modname \"texts check\"\n");
            var mod = new Mod { FullFilePath = modPath };
            mod.Parse(modPath);
            mod.ResolveDependencies();
            mod.Resolve();
            var editor = new Dom5Edit.Editing.ModEditor(mod);
            mod.TryGet(EntityType.NATION, 5, null, out var nation);
            mod.TryGet(EntityType.MONSTER, 20, null, out var monster);
            editor.Set(nation, Command.SUMMARY, "\"Summary from a mod\"");
            editor.Set(monster, Command.HP, "12");
            var r = editor.Resolve(editor.OwnEntity(nation) ?? nation);
            Check(r.Get(Command.SUMMARY)?.Property is StringProperty s && s.Value == "Summary from a mod", "the mod's summary replaces the game's");
            Check(r.Get(Command.DESCR) == null && r.Assets.ContainsKey(Command.DESCR) && r.Assets.ContainsKey(Command.BRIEF),
                  "the game's description and brief are shown, not values");
            string outPath = args.Length > 1 ? args[1] : Path.Combine(dir, "texts-out.dm");
            mod.Export(outPath);
            var saved = File.ReadAllText(outPath);
            Console.WriteLine("  saved:\n    " + string.Join("\n    ", saved.Split('\n').Select(l => l.TrimEnd()).Where(l => l.Length > 0)));
            Check(saved.Contains("#summary \"Summary from a mod\"") && saved.Contains("#hp 12"), "the edits are saved");
            Check(!saved.Contains("#descr") && !saved.Contains("#brief") && !saved.Contains(Asset(EntityType.MONSTER, 20, Command.DESCR) ?? "#descr"),
                  "the game's texts aren't");
            Console.WriteLine(failures == 0 ? "texts: all checks pass" : $"texts: {failures} checks fail");
            if (failures > 0)
                Environment.ExitCode = 1;
        }

        /// <summary>
        /// Dom5Tests events MOD.dm [--all]: the mod's events as the editor links them
        /// (Dom5Edit.Events): counts by link kind, every chain with its links, and the problems found.
        /// </summary>
        static void Events(string basePath, string[] args)
        {
            LoadVanillaBase(basePath);
            var mod = new Mod { FullFilePath = args[1] };
            mod.Parse(args[1]);
            mod.ResolveDependencies();
            mod.Resolve();
            var resolver = Dom5Edit.Resolve.ModResolver.For(mod);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var g = Dom5Edit.Events.EventGraph.Build(mod, resolver.Resolve, mod.Below().ToList());
            Console.WriteLine($"{g.Events.Count} events ({g.ModEvents.Count} the mod's), {g.Links.Count} links, {g.Chains.Count} chains, {g.Problems.Count} problems ({watch.ElapsedMilliseconds} ms)");
            Console.WriteLine("  vanilla event messages: " + VanillaLoader.EventMessagesStatus);
            watch.Restart();
            Dom5Edit.Events.EventGraph.Build(mod, resolver.Resolve, mod.Below().ToList(), g.Spells);
            Console.WriteLine($"  rebuilt after an edit (spells kept): {watch.ElapsedMilliseconds} ms");
            Console.WriteLine("  first events: " + string.Join(", ", g.Events.Take(4).Select(e => $"id {e.ID}{(e.Selected ? " (select)" : "")}")));
            foreach (var k in g.Links.GroupBy(l => l.Kind))
                Console.WriteLine($"  {k.Key}: {k.Count()}");
            string Title(IDEntity e) => (g.IsModEvent(e) ? $"[{g.IndexOf(e)}] " : $"[game {e.ID}] ") + Dom5Edit.Events.EventInfo.Title(g.LinesOf(e));
            string Name(IDEntity e) => e.Kind == EntityType.EVENT ? Title(e)
                : $"spell #{e.ID} {(resolver.Resolve(e).Get(Command.NAME)?.Property as Dom5Edit.Props.NameProperty)?.Value}";
            bool all = args.Contains("--all");
            foreach (var chain in all ? g.Chains : g.Chains.Take(8))
            {
                Console.WriteLine($"\nchain {chain.Number}: {chain.Events.Count} events");
                foreach (var t in chain.Triggers)
                    Console.WriteLine($"  started by {Name(t)}");
                foreach (var l in chain.Links.Take(12))
                    Console.WriteLine($"  {Title(l.From)}  --{l.Label}-->  {Title(l.To)}");
            }
            Console.WriteLine();
            foreach (var p in g.Problems.GroupBy(p => System.Text.RegularExpressions.Regex.Replace(p.Message, @"-?\d+", "N")))
                Console.WriteLine($"{p.Count(),4} x {p.First()}   e.g. {Title(p.First().Event)}");
            var enchanted = g.Links.Where(l => l.Kind == Dom5Edit.Events.EventLinkKind.Enchantment).Select(l => l.To).Distinct().Count();
            Console.WriteLine($"\nevents started by an enchantment spell: {enchanted}; by a cause-event spell: {g.Links.Count(l => l.Kind == Dom5Edit.Events.EventLinkKind.SpellEvent)}");
        }

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

        /// <summary>One scripted edit (see Edit); returns the editor's undo step, or null if nothing changed.</summary>
        static Dom5Edit.Editing.IModEdit? ApplyEdit(Dom5Edit.Editing.ModEditor editor, System.Text.Json.JsonElement edit, Dictionary<string, IDEntity> created)
        {
            string op = edit.GetProperty("op").GetString()!;
            // the mod's header (#modname, #description, #icon, #version, #domversion)
            if (op == "info")
                return editor.SetModInfo(CommandOf(edit.GetProperty("command").GetString()!), edit.TryGetProperty("value", out var info) ? info.GetString() : null);
            string kind = edit.GetProperty("entity").GetString()!;
            // "@label" in a value: the number of the entity a "create" edit named so
            string? Arg(string name) => edit.TryGetProperty(name, out var a) && a.GetString() is string s
                ? System.Text.RegularExpressions.Regex.Replace(s, @"@(\w+)", x => created.TryGetValue(x.Groups[1].Value, out var c) ? c.ID.ToString() : x.Value)
                : null;
            string? value = Arg("value");
            string? from = Arg("from");
            if (op == "create")
            {
                var edit0 = editor.Create(KindOf(edit), edit.TryGetProperty("name", out var n) ? n.GetString() : null, out var made);
                if (edit.TryGetProperty("as", out var label))
                    created[label.GetString()!] = made;
                return edit0;
            }
            var entity = FindEntity(editor, edit, created);
            string where = $"{kind} {(entity.ID > 0 ? entity.ID.ToString() : edit.GetRawText())}";
            // (the "text" and "delete" ops have no command)
            string commandText = edit.TryGetProperty("command", out var ct) ? ct.GetString()! : "#end";
            var command = CommandOf(commandText);

            // the editor's operations (Dom5Edit.Editing), as the GUI makes them
            switch (op)
            {
                case "set":
                    return editor.Set(entity, command, value ?? "");
                case "add":
                    return editor.Add(entity, command, value ?? "");
                case "remove":
                    // every value of the command (or those with this argument), so the entity no longer has it
                    return editor.Run($"Remove {commandText}", tx =>
                    {
                        int count = 0;
                        while (tx.Resolve(entity).GetAll(command).FirstOrDefault(x => value == null || SameValue(x.Property, value)) is { } match)
                        {
                            tx.Remove(entity, match);
                            if (++count > 100) throw new InvalidOperationException("remove doesn't converge");
                        }
                        if (count == 0) throw new InvalidOperationException($"remove: {where} has no {commandText} {value}");
                    });
                case "change":
                    var target = editor.Resolve(entity).GetAll(command).FirstOrDefault(x => SameValue(x.Property, from!))
                                 ?? throw new InvalidOperationException($"change: {where} has no {commandText} {from}");
                    return editor.Change(entity, target, value ?? "");
                case "reset":
                    return editor.Reset(entity, command);
                case "text":
                    // the page's "in the file" box: the block as text, with "replace" pairs applied
                    string block = editor.BlockText(entity) ?? throw new InvalidOperationException($"text: {where} has no single block");
                    if (edit.TryGetProperty("replace", out var pairs))
                        foreach (var pair in pairs.EnumerateArray())
                        {
                            var find = pair[0].GetString()!;
                            if (!block.Contains(find))
                                throw new InvalidOperationException($"text: no \"{find}\" in\n{block}");
                            block = block.Replace(find, pair[1].GetString());
                        }
                    return editor.ReplaceText(entity, block);
                case "delete":
                    return editor.Delete(entity);
                case "move":
                    // one of the entity's own lines (the command's, or the one with this argument)
                    var own = editor.OwnEntity(entity) ?? throw new InvalidOperationException($"move: {where} has no lines in the mod");
                    var line = own.Properties.SingleOrDefault(p => p.Command == command && (value == null || SameValue(p, value)))
                               ?? throw new InvalidOperationException($"move: {where} has no single {commandText} {value}");
                    if (edit.TryGetProperty("before", out var before))
                    {
                        var to = CommandOf(before.GetString()!);
                        var targetLine = own.Properties.FirstOrDefault(p => p.Command == to)
                                         ?? throw new InvalidOperationException($"move: {where} has no {before.GetString()}");
                        return editor.MoveLine(entity, line, targetLine, after: false);
                    }
                    return editor.MoveLine(entity, line, edit.GetProperty("delta").GetInt32());
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
