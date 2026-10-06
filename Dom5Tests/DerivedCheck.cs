#nullable enable
using System.Diagnostics;
using System.Text.Json;
using Dom5Edit;
using Dom5Edit.Derived;
using Dom5Edit.Entities;
using Dom5Edit.Resolve;

namespace Dom5Tests
{
    /// <summary>
    /// Checks Dom5Edit.Derived (the values the game works out for a monster: defence with gear,
    /// protection with armor, encumbrance, combat speed, map move, ages, leadership, resources,
    /// gold, per-weapon attack and damage) against the dom6inspector, for every vanilla unit.
    ///
    ///   node tools/derived/inspector_totals.js /mnt/c/Projects/dom6inspector insp.json
    ///   Dom5Tests derived-check insp.json [--show N] [--field NAME]
    ///
    /// Two comparisons:
    /// - formulas: the inspector's own inputs (its unit, weapon and armor values) through our
    ///   formulas, against its results. Differences here are formula differences.
    /// - vanilla.dm: our inputs (the exe-written vanilla data, resolved) through our formulas,
    ///   against the inspector's results. A difference the formulas comparison doesn't have for
    ///   the same unit and value comes from the data (vanilla.dm vs the inspector's CSV).
    /// The inspector's choice of commander or not (how a unit is recruited) is used for both.
    ///
    ///   Dom5Tests derived &lt;mod.dm | vanilla&gt; &lt;monster id&gt;... : prints the values and their parts.
    /// </summary>
    static class DerivedCheck
    {
        // the values compared, as (name, ours, the inspector's output field)
        private static readonly (string Name, Func<UnitTotals, double?> Ours, string Theirs)[] Fields =
        {
            ("hp", t => t.Hp.Value, "hp"), ("str", t => t.Str.Value, "str"), ("att", t => t.Att.Value, "att"),
            ("def", t => t.Def.Value, "def"), ("prec", t => t.Prec.Value, "prec"), ("enc", t => t.Enc.Value, "enc"),
            ("ap", t => t.Ap.Value, "ap"), ("prot", t => t.Prot.Value, "prot"), ("casting_enc", t => t.CastingEnc, "casting_enc"),
            ("mapmove", t => t.MapMove.Value, "mapmove"), ("startage", t => t.StartAge.Value, "startage"), ("maxage", t => t.MaxAge.Value, "maxage"),
            ("leader", t => t.Leader.Value, "leader"), ("magicleader", t => t.MagicLeader.Value, "magicleader"),
            ("undeadleader", t => t.UndeadLeader.Value, "undeadleader"),
            ("fireres", t => t.FireRes.Value, "fireres"), ("coldres", t => t.ColdRes.Value, "coldres"), ("shockres", t => t.ShockRes.Value, "shockres"),
            ("poisonres", t => t.PoisonRes.Value, "poisonres"), ("supplybonus", t => t.SupplyBonus.Value, "supplybonus"), ("fear", t => t.Fear.Value, "fear"),
            ("fireshield", t => t.FireShield.Value, "fireshield"), ("heat", t => t.Heat.Value, "heat"), ("cold", t => t.Cold.Value, "cold"),
            ("rcost", t => t.Rcost?.Value, "rcost"), ("gold", t => t.Gold, "goldcost"),
        };

        // values the inspector leaves unset when 0 (it only writes what a unit has)
        private static readonly HashSet<string> ZeroIsUnset = new HashSet<string>
        {
            "fireres", "coldres", "shockres", "poisonres", "supplybonus", "fear", "fireshield", "heat", "cold",
        };

        public static void Run(string basePath, string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Dom5Tests derived-check <inspector_totals.json> [--show N] [--field NAME]");
                Environment.ExitCode = 2;
                return;
            }
            int show = Option(args, "--show") is string s && int.TryParse(s, out var n) ? n : 3;
            string? only = Option(args, "--field");

            using var doc = JsonDocument.Parse(File.ReadAllText(args[1]));
            var units = doc.RootElement.GetProperty("units").EnumerateArray().ToList();
            Console.WriteLine($"inspector: {units.Count} units ({units.Count(u => !IsWhole(u))} copies for a second recruitment role)");

            // formulas: the inspector's inputs
            var theirInputs = new Dictionary<int, UnitStats>();
            foreach (var u in units.Where(IsWhole))
                theirInputs[(int)u.GetProperty("id").GetDouble()] = FromInspector(u);
            UnitStats? TheirMonster(int id) => theirInputs.TryGetValue(id, out var x) ? x : null;

            // vanilla.dm, resolved
            Program.LoadVanillaBase(basePath);
            var vanilla = VanillaLoader.Vanilla;
            var resolver = ModResolver.For(vanilla);
            ResolvedEntity? Find(EntityType t, int id) => id > 0 && vanilla.Database[t].TryGetValue(id, out var e) ? resolver.Resolve(e) : null;
            var ourInputs = new Dictionary<int, UnitStats>();
            UnitStats? OurMonster(int id)
            {
                if (ourInputs.TryGetValue(id, out var x))
                    return x;
                if (Find(EntityType.MONSTER, id) is not ResolvedEntity r)
                    return null;
                return ourInputs[id] = UnitStats.Of(r, Find);
            }

            var formula = new Tally();
            var data = new Tally();
            var watch = Stopwatch.StartNew();
            int computed = 0, missing = 0;
            foreach (var u in units)
            {
                int id = (int)Math.Floor(u.GetProperty("id").GetDouble());
                var outp = u.GetProperty("out");
                if (outp.ValueKind != JsonValueKind.Object)
                    continue;
                bool cmdr = u.GetProperty("cmdr").GetBoolean();
                bool cmdrCost = u.GetProperty("type").ValueKind == JsonValueKind.String && u.GetProperty("type").GetString() == "c";
                string name = u.GetProperty("name").GetString() ?? "";

                // map move got the commander bonus as the unit was typed when the inspector reached it
                bool? cmdrAtMove = u.TryGetProperty("cmdrAtMove", out var cm) && cm.ValueKind is JsonValueKind.True or JsonValueKind.False ? cm.GetBoolean() : null;
                UnitTotals Move(UnitStats s, UnitTotals t, Func<int, UnitStats?> find)
                {
                    if (cmdrAtMove is not bool b || b == s.Commander)
                        return t;
                    var c = Clone(s);
                    c.Commander = b;
                    return UnitTotals.Compute(c, find);
                }

                var theirs = FromInspector(u);
                var byFormula = UnitTotals.Compute(theirs, TheirMonster);
                var fdiff = Compare(byFormula, Move(theirs, byFormula, TheirMonster), outp, formula, id, name, null);

                var mine = OurMonster(id);
                if (mine == null)
                {
                    missing++;
                    continue;
                }
                var copy = Clone(mine);
                copy.Commander = cmdr;
                copy.CommanderCost = cmdrCost;
                copy.Pretender = u.GetProperty("in").GetProperty("pretender").GetBoolean();
                var ours = UnitTotals.Compute(copy, OurMonster);
                computed++;
                Compare(ours, Move(copy, ours, OurMonster), outp, data, id, name, fdiff);
            }
            long ms = watch.ElapsedMilliseconds;

            Console.WriteLine($"\nFORMULAS (the inspector's inputs through Dom5Edit.Derived): {formula.Units} units");
            formula.Print(show, only);
            Console.WriteLine($"\nVANILLA.DM (our inputs, from the exe-written vanilla data): {computed} units ({missing} not in vanilla.dm)");
            Console.WriteLine("  (a difference marked 'data' is one the formulas comparison doesn't have: the inputs differ)");
            data.Print(show, only);

            // input differences behind the data column
            InputDiffs(units, OurMonster, show);

            // items: the gem cost to forge ("10F5W")
            if (doc.RootElement.TryGetProperty("items", out var items))
            {
                int same = 0, total = 0;
                var examples = new List<string>();
                foreach (var it in items.EnumerateArray())
                {
                    int id = (int)it.GetProperty("id").GetDouble();
                    if (Find(EntityType.ITEM, id) is not ResolvedEntity r)
                        continue;
                    int? I(Dom5Edit.Commands.Command c) => UnitStats.Int(r, c);
                    string ours = ItemCost.Text(I(Dom5Edit.Commands.Command.MAINPATH) ?? 0, Math.Max(I(Dom5Edit.Commands.Command.MAINLEVEL) ?? 1, 1), I(Dom5Edit.Commands.Command.ITEMCOST1) ?? 0,
                        I(Dom5Edit.Commands.Command.SECONDARYPATH) ?? -1, I(Dom5Edit.Commands.Command.SECONDARYLEVEL) ?? 0, I(Dom5Edit.Commands.Command.ITEMCOST2) ?? 0);
                    string theirs = it.GetProperty("gemcost").GetString() ?? "";
                    total++;
                    if (ours == theirs)
                        same++;
                    else if (examples.Count < show)
                        examples.Add($"{id} {it.GetProperty("name").GetString()}: ours {ours}, inspector {theirs}");
                }
                Console.WriteLine($"\nITEMS: gem cost to forge {same} / {total} ({100.0 * same / Math.Max(1, total):0.00}%)");
                foreach (var e in examples)
                    Console.WriteLine($"    {e}");
            }

            // cost: reading a resolved monster and working its values out
            watch.Restart();
            int count = 0;
            foreach (var m in vanilla.Database[EntityType.MONSTER].GetFullList())
            {
                var stats = UnitStats.Of(resolver.Resolve(m), Find);
                UnitTotals.Compute(stats, id => Find(EntityType.MONSTER, id) is ResolvedEntity r ? UnitStats.Of(r, Find) : null);
                count++;
            }
            Console.WriteLine($"\ntiming: {count} vanilla monsters read and worked out in {watch.ElapsedMilliseconds} ms ({watch.Elapsed.TotalMilliseconds * 1000 / Math.Max(1, count):0} µs each; first pass, resolver cache warm); comparison {ms} ms");
        }

        /// <summary>Prints one monster's values and what makes them up. Usage: Dom5Tests derived &lt;mod.dm | vanilla&gt; &lt;id&gt;...</summary>
        public static void Print(string basePath, string[] args)
        {
            Program.LoadVanillaBase(basePath);
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
            var resolver = ModResolver.For(mod);
            ResolvedEntity? Find(EntityType t, int id) => id > 0 && mod.TryGet(t, id, null!, out var e) ? resolver.Resolve(e) : null;
            foreach (var a in args.Skip(2))
            {
                if (!int.TryParse(a, out var id) || Find(EntityType.MONSTER, id) is not ResolvedEntity r)
                {
                    Console.WriteLine($"monster {a}: not found");
                    continue;
                }
                var u = UnitStats.Of(r, Find);
                var t = UnitTotals.Compute(u, x => Find(EntityType.MONSTER, x) is ResolvedEntity m ? UnitStats.Of(m, Find) : null);
                Console.WriteLine($"\nmonster {id} {u.Name}");
                foreach (var (label, v) in new[]
                {
                    ("hp", t.Hp), ("str", t.Str), ("att", t.Att), ("def", t.Def), ("prec", t.Prec), ("prot", t.Prot), ("enc", t.Enc),
                    ("ap", t.Ap), ("mapmove", t.MapMove), ("startage", t.StartAge), ("maxage", t.MaxAge), ("leader", t.Leader),
                    ("magicleader", t.MagicLeader), ("undeadleader", t.UndeadLeader),
                })
                    Console.WriteLine($"  {label,-12} {v.Value,5}   {string.Join("; ", v.Breakdown())}");
                if (t.Rcost != null)
                    Console.WriteLine($"  {"rcost",-12} {t.Rcost.Value,5}   {string.Join("; ", t.Rcost.Breakdown())}");
                Console.WriteLine($"  {"gold",-12} {t.Gold,5}   {string.Join("; ", t.GoldNotes)}");
                if (t.CastingEnc != null)
                    Console.WriteLine($"  casting enc  {t.CastingEnc,5}");
                foreach (var w in t.Weapons)
                    Console.WriteLine($"  weapon {w.Weapon.Name} #{w.Weapon.Id}: attack {w.Attack} ({string.Join(" ", w.AttackParts)}), damage {w.Damage} ({w.Weapon.Dmg} + {w.StrengthAdded} {w.StrengthNote}), {(w.Weapon.Missile ? "range" : "length")} {w.Reach} {w.ReachNote}");
            }
        }

        private sealed class Tally
        {
            public int Units;
            public readonly Dictionary<string, (int Same, int Differ, int Data)> ByField = new();
            public readonly Dictionary<string, List<string>> Examples = new();

            public void Count(string field, bool same, bool data, string example)
            {
                var c = ByField.GetValueOrDefault(field);
                ByField[field] = same ? (c.Same + 1, c.Differ, c.Data) : (c.Same, c.Differ + 1, c.Data + (data ? 1 : 0));
                if (!same)
                {
                    var key = field + (data ? " (data)" : "");
                    if (!Examples.TryGetValue(key, out var list))
                        Examples[key] = list = new List<string>();
                    list.Add(example);
                }
            }

            public void Print(int show, string? only)
            {
                int same = 0, total = 0;
                foreach (var (field, c) in ByField)
                {
                    same += c.Same;
                    total += c.Same + c.Differ;
                    if (only != null && field != only)
                        continue;
                    string diff = c.Differ == 0 ? "" : $"  {c.Differ} differ" + (c.Data > 0 ? $" ({c.Data} data, {c.Differ - c.Data} formula)" : "");
                    Console.WriteLine($"  {field,-14} {c.Same,5} / {c.Same + c.Differ,-5} {100.0 * c.Same / Math.Max(1, c.Same + c.Differ),6:0.00}%{diff}");
                }
                Console.WriteLine($"  {"all",-14} {same,5} / {total,-5} {100.0 * same / Math.Max(1, total),6:0.00}%");
                foreach (var (key, list) in Examples.OrderBy(k => k.Key))
                {
                    if (only != null && !key.StartsWith(only))
                        continue;
                    foreach (var e in list.Take(show))
                        Console.WriteLine($"    {key}: {e}");
                    if (list.Count > show)
                        Console.WriteLine($"    {key}: ... {list.Count - show} more");
                }
            }
        }

        /// <summary>Compares one unit's values; returns the fields that differ (for the data column).</summary>
        private static HashSet<string> Compare(UnitTotals t, UnitTotals move, JsonElement outp, Tally tally, int id, string name, HashSet<string>? formulaDiffs)
        {
            tally.Units++;
            var differ = new HashSet<string>();
            foreach (var (field, ours, theirsKey) in Fields)
            {
                double? theirs = Num(outp, theirsKey);
                double? mine = ours(field == "mapmove" ? move : t);
                if (ZeroIsUnset.Contains(field))
                {
                    theirs ??= 0;
                    mine ??= 0;
                }
                bool same = theirs == mine;
                if (!same)
                    differ.Add(field);
                tally.Count(field, same, formulaDiffs != null && !formulaDiffs.Contains(field), $"{id} {name}: ours {Show(mine)}, inspector {Show(theirs)}");
            }
            // the weapon table: attack, damage, length or range, weapon by weapon
            var theirWeapons = outp.GetProperty("weapons").EnumerateArray().ToList();
            for (int i = 0; i < Math.Max(theirWeapons.Count, t.Weapons.Count); i++)
            {
                if (i >= theirWeapons.Count || i >= t.Weapons.Count)
                {
                    differ.Add("weapons");
                    tally.Count("wpn list", false, formulaDiffs != null && !formulaDiffs.Contains("weapons"), $"{id} {name}: {t.Weapons.Count} weapons, inspector {theirWeapons.Count}");
                    break;
                }
                var w = t.Weapons[i];
                var tw = theirWeapons[i];
                void One(string field, string mine, string theirs)
                {
                    bool same = mine == theirs;
                    if (!same)
                        differ.Add(field);
                    tally.Count(field, same, formulaDiffs != null && !formulaDiffs.Contains(field),
                        $"{id} {name}, {w.Weapon.Name} #{w.Weapon.Id}: ours {mine}, inspector {theirs}");
                }
                One("wpn att", w.Attack.ToString(), Show(Num(tw, "att")));
                One("wpn dmg", w.Damage?.ToString() ?? "-", Num(tw, "dmg") is double d ? Show(d) : "-");
                One("wpn len", (w.Weapon.Missile ? "r" : "") + w.Reach, tw.GetProperty("len").GetString() ?? "");
            }
            return differ;
        }

        /// <summary>Which of our inputs (from vanilla.dm) differ from the inspector's, entity by entity.</summary>
        private static void InputDiffs(List<JsonElement> units, Func<int, UnitStats?> ours, int show)
        {
            var count = new Dictionary<string, int>();
            var examples = new Dictionary<string, List<string>>();
            var seenWeapons = new HashSet<int>();
            var seenArmor = new HashSet<int>();
            void Diff(string key, string example)
            {
                count[key] = count.TryGetValue(key, out var c) ? c + 1 : 1;
                if (!examples.TryGetValue(key, out var list))
                    examples[key] = list = new List<string>();
                if (list.Count < show)
                    list.Add(example);
            }
            int compared = 0;
            foreach (var u in units.Where(IsWhole))
            {
                int id = (int)u.GetProperty("id").GetDouble();
                var mine = ours(id);
                if (mine == null)
                    continue;
                compared++;
                var theirs = FromInspector(u);
                string name = mine.Name;
                foreach (var (key, a, b) in new (string, object, object)[]
                {
                    ("hp", mine.Hp, theirs.Hp), ("str", mine.Str, theirs.Str), ("att", mine.Att, theirs.Att), ("def", mine.Def, theirs.Def),
                    ("prec", mine.Prec, theirs.Prec), ("enc", mine.Enc, theirs.Enc), ("ap", mine.Ap, theirs.Ap), ("prot", mine.Prot, theirs.Prot),
                    ("size", mine.Size, theirs.Size), ("ressize", mine.ResSize, theirs.ResSize), ("rcost", mine.Rcost, theirs.Rcost),
                    ("gcost", mine.Gcost, theirs.Gcost), ("mapmove", mine.MapMove ?? -1, theirs.MapMove ?? -1),
                    ("startage", mine.StartAge ?? -1, theirs.StartAge ?? -1), ("maxage", mine.MaxAge ?? -1, theirs.MaxAge ?? -1),
                    ("leader", mine.Leader + mine.GameLeaderBonus, theirs.Leader), ("magicleader", mine.MagicLeader, theirs.MagicLeader), ("undeadleader", mine.UndeadLeader, theirs.UndeadLeader),
                    ("paths", string.Join(",", mine.Paths), string.Join(",", theirs.Paths)),
                    ("random paths", string.Join(" ", mine.RandomPaths.Select(r => $"{string.Join("", r.Paths)}:{r.Levels}x{r.Chance}")),
                                     string.Join(" ", theirs.RandomPaths.Select(r => $"{string.Join("", r.Paths)}:{r.Levels}x{r.Chance}"))),
                    ("undead/demon/inanimate", $"{mine.Undead}{mine.Demon}{mine.Inanimate}", $"{theirs.Undead}{theirs.Demon}{theirs.Inanimate}"),
                    ("mount", mine.MountId, theirs.MountId), ("ambidextrous", mine.Ambidextrous, theirs.Ambidextrous),
                    ("fear/heat/cold/fireshield", $"{mine.Fear}/{mine.Heat}/{mine.Cold}/{mine.FireShield}", $"{theirs.Fear}/{theirs.Heat}/{theirs.Cold}/{theirs.FireShield}"),
                    ("resistances", $"{mine.FireRes}/{mine.ColdRes}/{mine.ShockRes}/{mine.PoisonRes}", $"{theirs.FireRes}/{theirs.ColdRes}/{theirs.ShockRes}/{theirs.PoisonRes}"),
                    ("gold inputs", GoldInputs(mine), GoldInputs(theirs)),
                    ("weapon list", string.Join(",", mine.Weapons.Select(w => w.Id)), string.Join(",", theirs.Weapons.Select(w => w.Id))),
                    ("armor list", string.Join(",", mine.Armor.Select(a => a.Id)), string.Join(",", theirs.Armor.Select(a => a.Id))),
                })
                    if (!Equals(a.ToString(), b.ToString()))
                        Diff("unit " + key, $"{id} {name}: ours {a}, inspector {b}");
                foreach (var tw in theirs.Weapons)
                    if (seenWeapons.Add(tw.Id) && mine.Weapons.FirstOrDefault(w => w.Id == tw.Id) is WeaponStats mw)
                        foreach (var (key, a, b) in new (string, object, object)[]
                        {
                            ("att/prec", mw.Att, tw.Att), ("def", mw.Def, tw.Def), ("len", mw.Len, tw.Len), ("dmg", mw.Dmg?.ToString() ?? "-", tw.Dmg?.ToString() ?? "-"),
                            ("rcost", mw.Rcost, tw.Rcost), ("range", mw.Range, tw.Range), ("missile", mw.Missile, tw.Missile), ("bonus", mw.Bonus, tw.Bonus),
                            ("twohanded", mw.TwoHanded, tw.TwoHanded), ("strength", mw.Strength, tw.Strength),
                        })
                            if (!Equals(a.ToString(), b.ToString()))
                                Diff("weapon " + key, $"{tw.Id} {mw.Name}: ours {a}, inspector {b}");
                foreach (var ta in theirs.Armor)
                    if (seenArmor.Add(ta.Id) && mine.Armor.FirstOrDefault(a => a.Id == ta.Id) is ArmorStats ma)
                        foreach (var (key, a, b) in new (string, object, object)[]
                        {
                            ("type", ma.Type, ta.Type), ("def", ma.DefShown, ta.DefShown), ("parry", ma.Parry, ta.Parry), ("enc", ma.Enc, ta.Enc),
                            ("body prot", ma.ProtBody, ta.ProtBody), ("head prot", ma.ProtHead, ta.ProtHead), ("general prot", ma.General, ta.General),
                            ("rcost", ma.Rcost, ta.Rcost), ("map move penalty", ma.MovePen, ta.MovePen),
                        })
                            if (!Equals(a.ToString(), b.ToString()))
                                Diff("armor " + key, $"{ta.Id} {ma.Name}: ours {a}, inspector {b}");
            }
            Console.WriteLine($"\nINPUTS that differ (vanilla.dm vs the inspector's CSV), over {compared} units, {seenWeapons.Count} weapons, {seenArmor.Count} armors:");
            foreach (var (key, c) in count.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"  {key,-34} {c,5}");
                foreach (var e in examples[key])
                    Console.WriteLine($"      {e}");
            }
        }

        private static string GoldInputs(UnitStats u) =>
            $"insp {u.Inspirational} sail {u.SailingShipSize} forge {u.ForgeBonus} research {u.ResearchBonus} spy {u.Assassin}{u.Spy}{u.Seduce} " +
            $"heal {u.AutoHealer}/{u.AutoDisHealer} stealth {u.Stealthy} mounted {u.MountedFlag} holy {u.Holy} slow {u.SlowRec || u.RpCost >= 4 && u.RpCost <= 6} " +
            $"shape {u.ShapeChange} riders {u.Riders}/{u.CoRiderId}";

        /// <summary>A unit's inputs as the inspector had them (inspector_totals.js "in").</summary>
        private static UnitStats FromInspector(JsonElement u)
        {
            var i = u.GetProperty("in");
            int I(string k) => i.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? (int)v.GetDouble() : 0;
            int? N(string k) => i.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? (int)v.GetDouble() : null;
            bool B(string k) => i.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.True;
            var s = new UnitStats
            {
                Id = (int)Math.Floor(u.GetProperty("id").GetDouble()),
                Name = u.GetProperty("name").GetString() ?? "",
                Hp = I("hp"), Str = I("str"), Att = I("att"), Def = I("def"), Prec = I("prec"), Enc = I("enc"), Ap = I("ap"), Prot = I("prot"),
                Size = I("size"), ResSize = I("ressize"), Rcost = I("rcost"), Gcost = I("basecost"), RpCost = I("rpcost"),
                MapMove = N("mapmove"), StartAge = N("startage"), MaxAge = N("maxage"),
                Undead = B("undead"), Demon = B("demon"), Inanimate = B("inanimate"), Flying = B("flying"), Slave = B("slave"),
                NoMovePen = B("nomovepen"), Holy = B("holy"), MountedFlag = B("mountedflag"), SlowRec = B("slowrec"),
                Ambidextrous = I("ambidextrous"), MountId = I("mountmnr"), CoRiderId = I("coridermnr"), Riders = I("nofriders"), ShapeChange = I("shapechange"),
                Paths = i.GetProperty("paths").EnumerateArray().Select(p => (int)p.GetDouble()).ToArray(),
                Leader = I("leader"), MagicLeader = I("magicleader"), UndeadLeader = I("undeadleader"),
                CommandBonus = I("command"), MagicCommandBonus = I("magiccommand"), UndeadCommandBonus = I("undcommand"),
                Fear = I("fear"), FireShield = I("fireshield"), Heat = I("heat"), Cold = I("cold"),
                FireRes = I("fireres"), ColdRes = I("coldres"), ShockRes = I("shockres"), PoisonRes = I("poisonres"), SupplyBonus = I("supplybonus"),
                Inspirational = I("inspirational"), SailingShipSize = I("sailingshipsize"), ForgeBonus = I("forgebonus"), ResearchBonus = I("researchbonus"),
                Assassin = I("assassin"), Spy = I("spy"), Seduce = I("seduce"), AutoHealer = I("autohealer"), AutoDisHealer = I("autodishealer"), Stealthy = I("stealthy"),
                Commander = u.GetProperty("cmdr").GetBoolean(),
                CommanderCost = u.GetProperty("type").ValueKind == JsonValueKind.String && u.GetProperty("type").GetString() == "c",
                Pretender = B("pretender"),
            };
            foreach (var r in i.GetProperty("randompaths").EnumerateArray())
                s.RandomPaths.Add(new RandomPath
                {
                    Paths = r.GetProperty("paths").EnumerateArray().Select(p => (int)p.GetDouble()).ToList(),
                    Levels = (int)r.GetProperty("levels").GetDouble(), Chance = (int)r.GetProperty("chance").GetDouble(),
                });
            foreach (var w in i.GetProperty("weapons").EnumerateArray())
                s.Weapons.Add(new WeaponStats
                {
                    Id = (int)w.GetProperty("id").GetDouble(), Name = w.GetProperty("name").GetString() ?? "",
                    Missile = !w.GetProperty("melee").GetBoolean(), Att = (int)w.GetProperty("att").GetDouble(), Def = (int)w.GetProperty("def").GetDouble(),
                    Len = (int)w.GetProperty("len").GetDouble(), Dmg = w.GetProperty("dmg").ValueKind == JsonValueKind.Number ? (int)w.GetProperty("dmg").GetDouble() : null,
                    Rcost = (int)w.GetProperty("rcost").GetDouble(), Range = (int)w.GetProperty("range").GetDouble(),
                    Bonus = w.GetProperty("bonus").GetBoolean(), TwoHanded = w.GetProperty("twohanded").GetBoolean(),
                    Strength = w.GetProperty("nostr").GetBoolean() ? StrengthAdded.None : w.GetProperty("bowstr").GetBoolean() ? StrengthAdded.Third
                        : w.GetProperty("halfstr").GetBoolean() ? StrengthAdded.Half : StrengthAdded.Full,
                });
            foreach (var a in i.GetProperty("armor").EnumerateArray())
            {
                int type = a.GetProperty("type").GetString() switch
                {
                    "shield" => ArmorStats.Shield, "helm" => ArmorStats.Helmet, "misc" => ArmorStats.Misc, "barding" => ArmorStats.Barding, _ => ArmorStats.Body,
                };
                int enc = (int)a.GetProperty("enc").GetDouble();
                // the inspector stores a shield's defence as -enc and its parry apart; ours is the data's #def
                int def = type == ArmorStats.Shield ? (int)a.GetProperty("parry").GetDouble() - enc : (int)a.GetProperty("def").GetDouble();
                int general = (int)a.GetProperty("general").GetDouble();
                s.Armor.Add(new ArmorStats
                {
                    Id = (int)a.GetProperty("id").GetDouble(), Name = a.GetProperty("name").GetString() ?? "", Type = type, Def = def, Enc = enc,
                    Rcost = (int)a.GetProperty("rcost").GetDouble(), Prot = (int)a.GetProperty("prot").GetDouble(),
                    ProtBody = (int)a.GetProperty("protbody").GetDouble(), ProtHead = (int)a.GetProperty("prothead").GetDouble(), General = general,
                    MovePenAbility = (int)a.GetProperty("movepen").GetDouble(),
                });
            }
            return s;
        }

        private static UnitStats Clone(UnitStats u)
        {
            var c = (UnitStats)typeof(UnitStats).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(u, null)!;
            return c;
        }

        private static bool IsWhole(JsonElement u) => u.GetProperty("id").GetDouble() is var d && d == Math.Floor(d);

        private static double? Num(JsonElement o, string key) =>
            o.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;

        private static string Show(double? v) => v?.ToString() ?? "-";

        private static string? Option(string[] args, string name)
        {
            int i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}
