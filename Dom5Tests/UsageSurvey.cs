using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Resolve;

namespace Dom5Tests
{
    /// <summary>
    /// Dom5Tests usage [MOD.dm]: every kind of "used by" link the editor makes, as its usage index
    /// does (each reference value an entity has, resolved), counted by the referring type, the
    /// command (and a spell's effect) and the type pointed at, with examples. For checking that each
    /// kind of link means what the game means: a spell's #damage is a monster only for some effects.
    /// Without a mod: the game's own data (vanilla.dm and the vanilla events).
    /// </summary>
    static class UsageSurvey
    {
        public static void Run(string basePath, string[] args)
        {
            Program.LoadVanillaBase(basePath);
            Mod mod;
            if (args.Length > 1)
            {
                mod = new Mod { FullFilePath = args[1] };
                mod.Parse(args[1]);
                mod.ResolveDependencies();
                mod.Resolve();
            }
            else
                mod = VanillaLoader.Vanilla!;
            var resolver = ModResolver.For(mod);
            var kinds = new Dictionary<string, (int Count, List<string> Examples)>();
            foreach (var (type, set) in mod.Database)
                foreach (var e in set.GetFullList())
                {
                    ResolvedEntity r;
                    try { r = resolver.Resolve(e); }
                    catch (Exception) { continue; }
                    int effect = type == EntityType.SPELL && r.Get(Command.EFFECT) is { } ev && int.TryParse(ev.Arguments.Split(' ')[0], out int fx) ? fx : 0;
                    foreach (var v in r.Values)
                    {
                        if (v.Property is not Reference rf)
                            continue;
                        if (Dom5Edit.GameData.GameCommandCatalog.IsRead(type, v.Command) == false || !ReferenceRules.NamesEntity(e, r, v.Command))
                            continue; // (as the editor: a line the game skips, or a number it ignores, links to nothing)
                        var targets = new List<IDEntity>();
                        if (rf.TryGetEntity(out var t) && t != null && t.ID > 0)
                            targets.Add(t);
                        if (rf is IMultiReference multi)
                            targets.AddRange(multi.Targets().Where(x => x.ID > 0));
                        foreach (var target in targets)
                        {
                            EntityType tk;
                            try { tk = target.Kind; }
                            catch (NotImplementedException) { continue; }
                            var cmd = CommandsMap.TryGetString(v.Command, out var c) ? c : v.Command.ToString();
                            var key = $"{type.ToString().ToLowerInvariant(),-9} {cmd}{(effect != 0 ? $" (effect {effect})" : "")} -> {tk.ToString().ToLowerInvariant()}";
                            if (!kinds.TryGetValue(key, out var k))
                                k = (0, new List<string>());
                            if (k.Examples.Count < 3)
                                k.Examples.Add($"{Label(e)} -> {Label(target)}  [{v.Arguments}]");
                            kinds[key] = (k.Count + 1, k.Examples);
                        }
                    }
                }
            foreach (var (key, k) in kinds.OrderBy(x => x.Key))
            {
                Console.WriteLine($"{k.Count,6}  {key}");
                foreach (var ex in k.Examples)
                    Console.WriteLine($"          {ex}");
            }
        }

        private static string Label(IDEntity e) => $"#{e.ID} {e.Name}".Trim();
    }
}
