using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.GameData;
using Dom5Edit.Props;
using Dom5Edit.Resolve;

namespace Dom5Edit.Derived
{
    /// <summary>
    /// A weapon as the unit totals use it: the fields the dom6inspector's unit window reads
    /// (scripts/DMI/MWpn.js). A missile weapon (one with a range) has its #att as precision.
    /// </summary>
    public sealed class WeaponStats
    {
        public int Id;
        public string Name = "";
        public bool Missile;
        /// <summary>Attack bonus; for a missile weapon, its precision bonus.</summary>
        public int Att;
        public int Def;
        public int Len;
        /// <summary>Damage, or null when it isn't a number of hit points (an effect).</summary>
        public int? Dmg;
        public int Rcost;
        /// <summary>#range: squares; -1 to -5 are strength divided by that.</summary>
        public int Range;
        public bool Bonus;
        public bool TwoHanded;
        /// <summary>How much of the wielder's strength the damage gets.</summary>
        public StrengthAdded Strength = StrengthAdded.Full;
        /// <summary>The monster's #weapon line that gives it (when read from a resolved monster).</summary>
        public ResolvedValue? Line;

        /// <summary>A weapon as it is in game (resolved: vanilla and the mod's lines).</summary>
        public static WeaponStats Of(ResolvedEntity w)
        {
            var s = new WeaponStats
            {
                Id = w.Entity.ID,
                Name = UnitStats.Text(w, Command.NAME),
                Range = UnitStats.Int(w, Command.RANGE) ?? 0,
                Att = UnitStats.Int(w, Command.ATT) ?? 0,
                Def = UnitStats.Int(w, Command.DEF) ?? 0,
                Len = UnitStats.Int(w, Command.LEN) ?? 0,
                Dmg = UnitStats.Int(w, Command.DMG),
                Rcost = UnitStats.Int(w, Command.RCOST) ?? 0,
                Bonus = w.Has(Command.BONUS),
                TwoHanded = w.Has(Command.TWOHANDED),
            };
            s.Missile = s.Range != 0;
            // an affliction weapon's #dmg is the afflictions it causes (a bit mask), not damage
            if (w.Has(Command.DT_AFF))
                s.Dmg = null;
            // the last strength line counts (each clears the others' bits); none: all of it
            var str = w.Values.LastOrDefault(v => v.Command is Command.FULLSTR or Command.NOSTR or Command.HALFSTR or Command.THIRDSTR or Command.BOWSTR);
            s.Strength = str?.Command switch
            {
                Command.NOSTR => StrengthAdded.None,
                // the game's #thirdstr does what #halfstr does (tools/dom6exe/README.md); #bowstr is a third
                Command.HALFSTR or Command.THIRDSTR => StrengthAdded.Half,
                Command.BOWSTR => StrengthAdded.Third,
                _ => StrengthAdded.Full,
            };
            return s;
        }
    }

    public enum StrengthAdded { Full, Half, Third, None }

    /// <summary>
    /// An armor as the unit totals use it: type, defence, encumbrance, cost, and protection by
    /// part (body, head, and "general", which the inspector adds to both), as the inspector's
    /// MArmor.js prepares them.
    /// </summary>
    public sealed class ArmorStats
    {
        public const int Shield = 4, Body = 5, Helmet = 6, Misc = 8, Barding = 9;

        public int Id;
        public string Name = "";
        /// <summary>#type: 4 shield, 5 body armor, 6 helmet, 8 misc, 9 barding.</summary>
        public int Type = Body;
        /// <summary>#def as the data has it (a shield's is its parry less its encumbrance).</summary>
        public int Def;
        public int Enc;
        public int Rcost;
        /// <summary>The #prot line's value (a misc armor's replaces natural protection when higher).</summary>
        public int Prot;
        /// <summary>Body and head protection, general (both) already added.</summary>
        public int ProtBody, ProtHead;
        /// <summary>Protection of the part that counts for body and head alike (part 6; no command sets it).</summary>
        public int General;
        public bool Magic;
        /// <summary>The game's map move penalty (ability 582), when the armor has one.</summary>
        public int? MovePenAbility;
        /// <summary>The monster's #armor line that gives it (when read from a resolved monster).</summary>
        public ResolvedValue? Line;

        /// <summary>A shield's parry: its defence plus its encumbrance (the inspector's MArmor).</summary>
        public int Parry => Type == Shield ? Def + Enc : 0;

        /// <summary>The defence it gives besides parry: a shield's is minus its encumbrance.</summary>
        public int DefShown => Type == Shield ? -Enc : Def;

        /// <summary>Map move lost wearing it (body armor only): twice its encumbrance (one less for magic armor), at most 6, unless the game gives one.</summary>
        public int MovePen => Type != Body ? 0 : MovePenAbility ?? Math.Min((Enc - (Magic ? 1 : 0)) * 2, 6);

        public static ArmorStats Of(ResolvedEntity a)
        {
            var s = new ArmorStats
            {
                Id = a.Entity.ID,
                Name = UnitStats.Text(a, Command.NAME),
                Type = UnitStats.Int(a, Command.TYPE) ?? Body,
                Def = UnitStats.Int(a, Command.DEF) ?? 0,
                Enc = UnitStats.Int(a, Command.ENC) ?? 0,
                Rcost = UnitStats.Int(a, Command.RCOST) ?? 0,
                // #magicarmor stores ability 557 = 1; 11 vanilla armors have 557 = 2 (read-only), magic too
                Magic = a.Has(Command.MAGICARMOR) || int.TryParse(UnitStats.GameValue(a, "ability 557"), out var magic) && magic != 0,
            };
            // #prot sets the part(s) of its type, #protparts head and body; the later line counts
            var line = a.Values.LastOrDefault(v => v.Command is Command.PROT or Command.PROTPARTS);
            if (line?.Property is IntIntProperty pp && line.Command == Command.PROTPARTS)
            {
                s.ProtHead = pp.Value1;
                s.ProtBody = pp.Value2;
            }
            else if (line != null)
            {
                s.Prot = UnitStats.Int(a, Command.PROT) ?? 0;
                switch (s.Type)
                {
                    case Shield: break;
                    case Helmet: s.ProtHead = s.Prot; break;
                    case Misc: s.General = s.Prot; break;
                    default: s.ProtBody = s.Prot; break;
                }
            }
            else
            {
                // protection by parts no command sets (read-only: "6:13 1:21"): 1 head, 2 torso, 6 general
                foreach (var part in (UnitStats.GameValue(a, "protection by part") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split(':');
                    if (kv.Length != 2 || !int.TryParse(kv[0], out var k) || !int.TryParse(kv[1], out var v))
                        continue;
                    if (k == 1) s.ProtHead = v;
                    else if (k == 2) s.ProtBody = v;
                    else if (k == 6) s.General = v;
                }
            }
            if (s.General != 0)
            {
                s.ProtBody += s.General;
                s.ProtHead += s.General;
            }
            if (int.TryParse(UnitStats.GameValue(a, "ability 582"), out var pen) && pen != -99)
                s.MovePenAbility = pen;
            return s;
        }
    }

    /// <summary>One random magic path roll (#custommagic): the paths it rolls among, levels and chance.</summary>
    public sealed class RandomPath
    {
        /// <summary>Indexes into F A W E S D N G B H.</summary>
        public List<int> Paths = new List<int>();
        public int Levels = 1;
        /// <summary>Percent; above 100 the inspector counts it as more levels (200: 2 levels at 100%).</summary>
        public int Chance = 100;
    }

    /// <summary>
    /// What the unit totals (UnitTotals) are worked out from: a monster's own stats, its weapons
    /// and armor, magic, ages and costs, as the dom6inspector's unit window reads them. Built from
    /// a resolved monster (<see cref="Of"/>), or filled in directly (the derived-check test feeds
    /// it the inspector's own inputs).
    /// </summary>
    public sealed class UnitStats
    {
        public static readonly string[] PathLetters = { "F", "A", "W", "E", "S", "D", "N", "G", "B", "H" };

        /// <summary>The paths of #magicskill 50 (random), 51 (elemental) and 52 (sorcery).</summary>
        private static readonly int[][] SpecialPaths = { new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new[] { 0, 1, 2, 3 }, new[] { 4, 5, 6, 8 } };

        public int Id;
        public string Name = "";
        // the base stats; not set: the inspector's defaults (MUnit.prepareData_PostMod)
        public int Hp = 10, Str = 10, Att = 10, Def = 10, Prec = 10, Enc = 10, Ap = 12, Prot, Size = 2, Mr = 10, Mor = 10;
        /// <summary>Which base stats the monster doesn't set (the default is used), by command.</summary>
        public HashSet<Command> Defaulted = new HashSet<Command>();
        /// <summary>The size its resource cost is worked out from: #ressize, else its size.</summary>
        public int ResSize = 2;
        /// <summary>#rcost (not set: 1).</summary>
        public int Rcost = 1;
        /// <summary>#gcost as set (10000 and up: worked out by the game).</summary>
        public int Gcost;
        public int RpCost;
        /// <summary>#mapmove as set, or null.</summary>
        public int? MapMove;
        /// <summary>#startage / #maxage as the game reads them (0 is "not set"; a start age of -1 is 0), or null.</summary>
        public int? StartAge, MaxAge;

        public bool Undead, Demon, Inanimate, Flying, Slave, NoMovePen, Holy, MountedFlag, SlowRec;
        public int Ambidextrous;
        /// <summary>The monster it rides (#mountmnr), its other riders (#nofriders, #coridermnr), its other shape (#shapechange).</summary>
        public int MountId, CoRiderId, Riders, ShapeChange;

        /// <summary>Magic path levels, F A W E S D N G B H (#magicskill).</summary>
        public int[] Paths = new int[10];
        public List<RandomPath> RandomPaths = new List<RandomPath>();

        /// <summary>Leadership classes (0, 10, 50, 100, 150, 200) and the #command bonuses.</summary>
        public int Leader = 50, MagicLeader, UndeadLeader, CommandBonus, MagicCommandBonus, UndeadCommandBonus;
        /// <summary>The leadership bonus some vanilla monsters have that no command sets (ability 160; not for #noleader).</summary>
        public int GameLeaderBonus;

        public int Fear, FireShield, Heat, Cold, FireRes, ColdRes, ShockRes, PoisonRes, SupplyBonus;
        public int Inspirational, SailingShipSize, ForgeBonus, ResearchBonus, Assassin, Spy, Seduce, AutoHealer, AutoDisHealer, Stealthy;

        /// <summary>
        /// Whether it's shown as a commander: commanders move 2 more on the map and get resistances
        /// from their paths (the inspector's isCmdr). The caller decides (how it's recruited).
        /// </summary>
        public bool Commander;
        /// <summary>Whether its gold cost is worked out as a commander's (recruited as one).</summary>
        public bool CommanderCost;
        /// <summary>A pretender: its cost is design points, so the inspector shows no gold or resources.</summary>
        public bool Pretender;

        public List<WeaponStats> Weapons = new List<WeaponStats>();
        public List<ArmorStats> Armor = new List<ArmorStats>();

        public bool IsMage => Paths.Any(p => p > 0) || RandomPaths.Any(r => r.Paths.Count > 0);

        /// <summary>
        /// A monster as it is in game. <paramref name="find"/> resolves a referenced entity (its
        /// weapons and armor) by type and ID, in the same mod; null if there's none.
        /// </summary>
        public static UnitStats Of(ResolvedEntity m, Func<EntityType, int, ResolvedEntity?> find)
        {
            var u = new UnitStats { Id = m.Entity.ID, Name = Text(m, Command.NAME) };
            int Stat(Command c, int dflt)
            {
                if (Int(m, c) is int v)
                    return v;
                u.Defaulted.Add(c);
                return dflt;
            }
            u.Hp = Stat(Command.HP, 10);
            u.Str = Stat(Command.STR, 10);
            u.Att = Stat(Command.ATT, 10);
            u.Def = Stat(Command.DEF, 10);
            u.Prec = Stat(Command.PREC, 10);
            u.Enc = Stat(Command.ENC, 10);
            u.Ap = Stat(Command.AP, 12);
            u.Prot = Stat(Command.PROT, 0);
            u.Size = Stat(Command.SIZE, 2);
            u.Mr = Stat(Command.MR, 10);
            u.Mor = Stat(Command.MOR, 10);
            u.ResSize = Int(m, Command.RESSIZE) ?? u.Size;
            u.Rcost = Int(m, Command.RCOST) ?? 1;
            u.Gcost = Int(m, Command.GCOST) ?? 0;
            u.RpCost = Int(m, Command.RPCOST) ?? 0;
            // #teleport stores map move 100 (the catalog's constants) where no #mapmove line counts
            u.MapMove = Int(m, Command.MAPMOVE) ?? Stored(m, Command.MAPMOVE);
            u.StartAge = Int(m, Command.STARTAGE) switch { 0 or null => null, -1 => 0, int a => a };
            u.MaxAge = Int(m, Command.MAXAGE) switch { 0 or null => null, int a => a };

            u.Undead = m.Has(Command.UNDEAD);
            u.Demon = m.Has(Command.DEMON);
            u.Inanimate = m.Has(Command.INANIMATE);
            u.Flying = m.Has(Command.FLYING);
            u.Slave = m.Has(Command.SLAVE);
            u.NoMovePen = m.Has(Command.NOMOVEPEN);
            u.Holy = m.Has(Command.HOLY);
            u.MountedFlag = m.Has(Command.MOUNTED);
            u.SlowRec = m.Values.LastOrDefault(v => v.Command is Command.SLOWREC or Command.NOSLOWREC)?.Command == Command.SLOWREC;
            u.Ambidextrous = Int(m, Command.AMBIDEXTROUS) ?? 0;
            u.MountId = RefId(m.Get(Command.MOUNTMNR)?.Property);
            u.CoRiderId = RefId(m.Get(Command.CORIDERMNR)?.Property);
            u.Riders = Int(m, Command.NOFRIDERS) ?? 0;
            u.ShapeChange = RefId(m.Get(Command.SHAPECHANGE)?.Property);

            foreach (var v in m.GetAll(Command.MAGICSKILL))
            {
                if (v.Property is not IntIntProperty ii)
                    continue;
                if (ii.Value1 >= 0 && ii.Value1 < u.Paths.Length)
                    u.Paths[ii.Value1] = ii.Value2;
                // 50 random (F A W E S D N G), 51 elemental (F A W E), 52 sorcery (S D N B): one pick of that many levels
                else if (ii.Value1 is >= 50 and <= 52)
                    u.RandomPaths.Add(new RandomPath { Paths = SpecialPaths[ii.Value1 - 50].ToList(), Levels = ii.Value2 });
            }
            foreach (var v in m.GetAll(Command.CUSTOMMAGIC))
            {
                var args = v.Arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 0 || !long.TryParse(args[0], out var mask))
                    continue;
                int chance = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 100;
                var r = new RandomPath { Chance = chance };
                // a linked random: every 100 above 100 is one more level (the inspector's parsemod)
                while (r.Chance > 100)
                {
                    r.Chance -= 100;
                    r.Levels++;
                }
                for (int i = 0; i < u.Paths.Length; i++)
                    if ((mask >> (7 + i) & 1) != 0)
                        r.Paths.Add(i);
                // (a mask with no path bits rolls for nothing)
                if (r.Paths.Count > 0)
                    u.RandomPaths.Add(r);
            }

            // leadership: the class, plus the game's own bonus (ability 160; not for #noleader)
            u.Leader = LeaderClass(m, 50, Command.NOLEADER, Command.POORLEADER, Command.OKLEADER, Command.GOODLEADER, Command.EXPERTLEADER, Command.SUPERIORLEADER);
            if (u.Leader > 0 && int.TryParse(GameValue(m, "leadership bonus"), out var lb))
                u.GameLeaderBonus = lb;
            u.MagicLeader = LeaderClass(m, 0, Command.NOMAGICLEADER, Command.POORMAGICLEADER, Command.OKMAGICLEADER, Command.GOODMAGICLEADER, Command.EXPERTMAGICLEADER, Command.SUPERIORMAGICLEADER);
            u.UndeadLeader = LeaderClass(m, 0, Command.NOUNDEADLEADER, Command.POORUNDEADLEADER, Command.OKUNDEADLEADER, Command.GOODUNDEADLEADER, Command.EXPERTUNDEADLEADER, Command.SUPERIORUNDEADLEADER);
            u.CommandBonus = Int(m, Command.COMMAND) ?? 0;
            u.MagicCommandBonus = Int(m, Command.MAGICCOMMAND) ?? 0;
            u.UndeadCommandBonus = Int(m, Command.UNDCOMMAND) ?? 0;

            u.Fear = Int(m, Command.FEAR) ?? 0;
            u.FireShield = Int(m, Command.FIRESHIELD) ?? 0;
            // a heat or cold aura flag (read-only) adds 3 to the aura
            u.Heat = (Int(m, Command.HEAT) ?? 0) + (int.TryParse(GameValue(m, "heat aura"), out var ha) ? ha : 0);
            u.Cold = (Int(m, Command.COLD) ?? 0) + (int.TryParse(GameValue(m, "cold aura"), out var ca) ? ca : 0);
            u.FireRes = Int(m, Command.FIRERES) ?? 0;
            u.ColdRes = Int(m, Command.COLDRES) ?? 0;
            u.ShockRes = Int(m, Command.SHOCKRES) ?? 0;
            u.PoisonRes = Int(m, Command.POISONRES) ?? 0;
            u.SupplyBonus = Int(m, Command.SUPPLYBONUS) ?? 0;
            u.Inspirational = Int(m, Command.INSPIRATIONAL) ?? 0;
            // #sailing a b; some vanilla units carry a ship size no command writes (read-only "sailing (abilities 112, 410) = 5 0")
            u.SailingShipSize = m.Get(Command.SAILING)?.Property is IntIntProperty sail ? sail.Value1
                : int.TryParse(GameValue(m, "sailing")?.Split(' ')[0], out var ship) ? ship : 0;
            u.ForgeBonus = Int(m, Command.FORGEBONUS) ?? 0;
            u.ResearchBonus = Int(m, Command.RESEARCHBONUS) ?? 0;
            u.Assassin = m.Has(Command.ASSASSIN) ? 1 : 0;
            u.Spy = m.Has(Command.SPY) ? 1 : 0;
            u.Seduce = Int(m, Command.SEDUCE) ?? 0;
            u.AutoHealer = Int(m, Command.AUTOHEALER) ?? 0;
            u.AutoDisHealer = Int(m, Command.AUTODISHEALER) ?? 0;
            // the stealth it has: 40 plus #stealthy's bonus
            u.Stealthy = m.Has(Command.STEALTHY) ? 40 + (Int(m, Command.STEALTHY) ?? 0) : 0;

            foreach (var v in m.GetAll(Command.WEAPON))
                if (find(EntityType.WEAPON, RefId(v.Property)) is ResolvedEntity w)
                {
                    var ws = WeaponStats.Of(w);
                    ws.Line = v;
                    u.Weapons.Add(ws);
                }
            foreach (var v in m.GetAll(Command.ARMOR))
                if (find(EntityType.ARMOR, RefId(v.Property)) is ResolvedEntity a)
                {
                    var stats = ArmorStats.Of(a);
                    stats.Line = v;
                    u.Armor.Add(stats);
                }
            return u;
        }

        private static int LeaderClass(ResolvedEntity m, int dflt, params Command[] tiers)
        {
            int[] values = { 0, 10, 50, 100, 150, 200 };
            var v = m.Values.LastOrDefault(x => Array.IndexOf(tiers, x.Command) >= 0);
            return v == null ? dflt : values[Array.IndexOf(tiers, v.Command)];
        }

        // ---- reading resolved values ----

        /// <summary>The value's first number, or null if the entity has no such value (or it isn't a number).</summary>
        public static int? Int(ResolvedEntity r, Command c)
        {
            var v = r.Get(c);
            if (v == null)
                return null;
            if (v.Property is IntProperty ip && ip.Value != int.MinValue)
                return ip.Value;
            var args = v.Arguments;
            int space = args.IndexOf(' ');
            return int.TryParse(space < 0 ? args : args.Substring(0, space), out var n) ? n : null;
        }

        /// <summary>
        /// A value another line stores as a constant for this command's field (#teleport: map move
        /// 100), the last such line's; null if none.
        /// </summary>
        public static int? Stored(ResolvedEntity r, Command c)
        {
            var type = r.Entity.Kind;
            var own = GameCommandCatalog.EffectOf(type, c);
            if (own == null || own.Set.Count != 1)
                return null;
            var key = own.Set.First();
            for (int i = r.Values.Count - 1; i >= 0; i--)
                if (GameCommandCatalog.EffectOf(type, r.Values[i].Command)?.Values.TryGetValue(key, out var v) == true)
                    return (int)v;
            return null;
        }

        public static string Text(ResolvedEntity r, Command c) =>
            r.Get(c)?.Property is StringProperty s ? s.Value ?? "" : "";

        /// <summary>A read-only game value whose label starts with this, or null.</summary>
        public static string? GameValue(ResolvedEntity r, string label) =>
            r.GameValues.FirstOrDefault(g => g.Label.StartsWith(label, StringComparison.Ordinal))?.Value;

        /// <summary>The ID a reference line points at (0 if none).</summary>
        public static int RefId(Property? p) => p switch
        {
            null => 0,
            StringOrIDRef r => r.ID,
            MonsterOrMontagRef m when m.MonsterRef != null && m.MonsterRef.HasValue => m.MonsterRef.ID,
            _ => int.TryParse(ResolvedValue.ArgumentsOf(p).Split(' ')[0], out var n) ? n : 0,
        };
    }
}
