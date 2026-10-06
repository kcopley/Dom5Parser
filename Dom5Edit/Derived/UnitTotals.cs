using Dom5Edit.Commands;

namespace Dom5Edit.Derived
{
    /// <summary>One thing that changes a derived value, for its tooltip: "+4 shield parry".</summary>
    public readonly record struct Contribution(string Label, int Amount)
    {
        public override string ToString() => $"{(Amount > 0 ? "+" : "")}{Amount} {Label}";
    }

    /// <summary>
    /// A value as the game uses it in battle or on the map, worked out from what the monster sets:
    /// where it starts (the monster's own value, or the default when it sets none), what changes
    /// it (weapons, armor, paths, old age, ...) and the result.
    /// </summary>
    public sealed class DerivedValue
    {
        private readonly List<Contribution> _parts = new List<Contribution>();

        public DerivedValue(int? set, int start, string? startLabel = null)
        {
            Set = set;
            Start = start;
            Value = start;
            StartLabel = startLabel ?? (set != null ? "its own" : "not set: the default");
        }

        /// <summary>The value the monster sets, or null when it sets none.</summary>
        public int? Set { get; }
        /// <summary>What the game starts from: the value set, else its default.</summary>
        public int Start { get; }
        public string StartLabel { get; }
        public int Value { get; internal set; }
        public IReadOnlyList<Contribution> Parts => _parts;
        /// <summary>Lines that explain it besides its parts (a formula; casting encumbrance).</summary>
        public List<string> Notes { get; } = new List<string>();

        /// <summary>Whether the game's value isn't the one in the box (or the box is empty).</summary>
        public bool Differs => Set != Value;

        /// <summary>
        /// Adds a change the way the inspector's bonus() does: the amount is cut to a whole
        /// number (towards zero) and nothing is added for 0.
        /// </summary>
        internal void Add(string label, double amount)
        {
            int n = (int)Math.Truncate(amount);
            if (n == 0)
                return;
            _parts.Add(new Contribution(label, n));
            Value += n;
        }

        /// <summary>The value's tooltip lines: where it starts, each change, then notes.</summary>
        public IEnumerable<string> Breakdown()
        {
            yield return $"{Start} {StartLabel}";
            foreach (var p in _parts)
                yield return p.ToString();
            foreach (var n in Notes)
                yield return n;
        }

        public override string ToString() => $"{Value} ({string.Join(", ", Breakdown())})";
    }

    /// <summary>One weapon as this unit uses it: its attack (or precision), damage with strength, length or range.</summary>
    public sealed class WeaponTotals
    {
        public WeaponTotals(WeaponStats weapon) => Weapon = weapon;

        public WeaponStats Weapon { get; }
        /// <summary>Melee: the unit's attack, the dual-wield penalty and the weapon's attack; missile: precision and the weapon's.</summary>
        public int Attack { get; internal set; }
        public List<Contribution> AttackParts { get; } = new List<Contribution>();
        /// <summary>The damage with the strength it adds, or null when the damage isn't a number.</summary>
        public int? Damage { get; internal set; }
        /// <summary>How much strength the damage gets (0: none).</summary>
        public int StrengthAdded { get; internal set; }
        public string StrengthNote { get; internal set; } = "";
        /// <summary>Melee: the length it fights at (big units reach one further); missile: its range.</summary>
        public int Reach { get; internal set; }
        public string ReachNote { get; internal set; } = "";
    }

    /// <summary>
    /// A monster's values as the game uses them, worked out from its own stats and its gear,
    /// paths and age: the dom6inspector's unit window (MUnit.prepareForRender, the weapon table,
    /// map move from prepareData_PostSiteData, gold from MUnit.goldCost), ported. Each value
    /// keeps what changed it, for tooltips. Pure and cheap (well under a millisecond a unit).
    /// docs/DERIVED_VALUES.md has the formulas and where they differ from the inspector.
    /// </summary>
    public sealed class UnitTotals
    {
        public UnitStats Unit { get; }

        public DerivedValue Hp { get; }
        public DerivedValue Str { get; }
        public DerivedValue Att { get; }
        public DerivedValue Def { get; }
        public DerivedValue Prec { get; }
        public DerivedValue Enc { get; }
        public DerivedValue Ap { get; }
        /// <summary>Total protection: natural with body armor and helmet, (body x4 + head) / 5.</summary>
        public DerivedValue Prot { get; }
        /// <summary>Natural protection with what adds to it (earth magic, a misc armor), before armor.</summary>
        public int NaturalProt { get; private set; }
        /// <summary>Protection of the body and of the head with armor (natural included), as fractions.</summary>
        public double BodyProt { get; private set; }
        public double HeadProt { get; private set; }
        public DerivedValue MapMove { get; }
        public DerivedValue StartAge { get; private set; } = null!;
        public DerivedValue MaxAge { get; private set; } = null!;
        public DerivedValue Leader { get; }
        public DerivedValue MagicLeader { get; }
        public DerivedValue UndeadLeader { get; }
        public DerivedValue FireRes { get; }
        public DerivedValue ColdRes { get; }
        public DerivedValue ShockRes { get; }
        public DerivedValue PoisonRes { get; }
        public DerivedValue SupplyBonus { get; }
        public DerivedValue Fear { get; }
        public DerivedValue FireShield { get; }
        public DerivedValue Heat { get; }
        public DerivedValue Cold { get; }
        /// <summary>Resource cost with weapons, armor and mount; null for a pretender (design points).</summary>
        public DerivedValue? Rcost { get; private set; }
        /// <summary>Gold cost (worked out by the game when #gcost is 10000 or more); null for a pretender.</summary>
        public int? Gold { get; private set; }
        public List<string> GoldNotes { get; } = new List<string>();
        /// <summary>Encumbrance when casting spells: its own plus twice its armor's (when it wears any).</summary>
        public int? CastingEnc { get; private set; }
        /// <summary>The attack penalty for fighting with more than one weapon, after ambidextrous (0 or less).</summary>
        public int DualWield { get; private set; }
        public List<string> DualWieldNotes { get; } = new List<string>();
        public bool IsOld { get; private set; }
        /// <summary>The armor that counts: the last of each type, in the order the types first appear.</summary>
        public List<ArmorStats> Armor { get; } = new List<ArmorStats>();
        public List<WeaponTotals> Weapons { get; } = new List<WeaponTotals>();

        private UnitTotals(UnitStats u)
        {
            Unit = u;
            DerivedValue Base(Command c, int value) =>
                new DerivedValue(u.Defaulted.Contains(c) ? null : value, value);
            Hp = Base(Command.HP, u.Hp);
            Str = Base(Command.STR, u.Str);
            Att = Base(Command.ATT, u.Att);
            Def = Base(Command.DEF, u.Def);
            Prec = Base(Command.PREC, u.Prec);
            Enc = Base(Command.ENC, u.Enc);
            Ap = Base(Command.AP, u.Ap);
            Prot = Base(Command.PROT, u.Prot);
            MapMove = new DerivedValue(u.MapMove, u.MapMove ?? 14);
            Leader = new DerivedValue(u.Leader, u.Leader, "class");
            MagicLeader = new DerivedValue(u.MagicLeader, u.MagicLeader, "class");
            UndeadLeader = new DerivedValue(u.UndeadLeader, u.UndeadLeader, "class");
            FireRes = new DerivedValue(u.FireRes, u.FireRes);
            ColdRes = new DerivedValue(u.ColdRes, u.ColdRes);
            ShockRes = new DerivedValue(u.ShockRes, u.ShockRes);
            PoisonRes = new DerivedValue(u.PoisonRes, u.PoisonRes);
            SupplyBonus = new DerivedValue(u.SupplyBonus, u.SupplyBonus);
            Fear = new DerivedValue(u.Fear, u.Fear);
            FireShield = new DerivedValue(u.FireShield, u.FireShield);
            Heat = new DerivedValue(u.Heat, u.Heat);
            Cold = new DerivedValue(u.Cold, u.Cold);
        }

        /// <summary>
        /// Works a unit's values out. <paramref name="monster"/> finds another monster's stats by
        /// ID (its mount, co-rider and other shape, for the costs); null: they're left out.
        /// </summary>
        public static UnitTotals Compute(UnitStats u, Func<int, UnitStats?>? monster = null)
        {
            var t = new UnitTotals(u);
            t.Run(monster);
            return t;
        }

        private static readonly string[] PathNames = { "fire", "air", "water", "earth", "astral", "death", "nature", "glamour", "blood", "holy" };

        private int Path(int i) => Unit.Paths[i];

        private void Run(Func<int, UnitStats?>? monster)
        {
            var u = Unit;
            Ages();

            Leader.Add("the game's leadership bonus", u.GameLeaderBonus);
            // what the paths give (the inspector's "magic pathcost bonuses"); leadership only for a leader
            bool leader = Leader.Value > 0;
            int n;
            if ((n = Path(1)) > 0)
            {
                if (leader) MagicLeader.Add("air magic", n * 10);
                if (u.Commander) ShockRes.Add("air magic", n > 2 ? n * 2 - 1 : 0);
            }
            if ((n = Path(8)) > 0 && leader)
            {
                MagicLeader.Add("blood magic", n * 10);
                UndeadLeader.Add("blood magic", n * 10);
            }
            if ((n = Path(5)) > 0)
            {
                if (leader) UndeadLeader.Add("death magic", n * 50);
                if (u.Fear > 0) Fear.Add("death magic", n);
                else if (n >= 5) Fear.Add("death magic", n - 5);
            }
            if ((n = Path(4)) > 0 && leader)
                MagicLeader.Add("astral magic", n * 20);
            if ((n = Path(3)) > 0)
            {
                if (leader) MagicLeader.Add("earth magic", n * 10);
                if (n > 2) Prot.Add("earth magic", n);
            }
            if ((n = Path(0)) > 0)
            {
                if (leader) Leader.Add("fire magic", n * 10);
                if (leader) MagicLeader.Add("fire magic", n * 10);
                if (u.Commander) FireRes.Add("fire magic", n > 2 ? n * 2 - 1 : 0);
                if (u.FireShield > 0) FireShield.Add("fire magic", n);
                if (u.Heat > 0) Heat.Add("fire magic", n);
            }
            if ((n = Path(6)) > 0)
            {
                if (leader) MagicLeader.Add("nature magic", n * 10);
                SupplyBonus.Add("nature magic", n * 10);
                if (u.Commander) PoisonRes.Add("nature magic", n > 2 ? n * 2 - 1 : 0);
            }
            if ((n = Path(2)) > 0)
            {
                if (leader) MagicLeader.Add("water magic", n * 10);
                if (u.Cold > 0) Cold.Add("water magic", n);
                if (u.Commander) ColdRes.Add("water magic", n > 2 ? n * 2 - 1 : 0);
            }
            if ((n = Path(7)) > 0 && leader)
                MagicLeader.Add("glamour magic", n * 10);
            if (leader)
            {
                Leader.Add("#command", u.CommandBonus);
                MagicLeader.Add("#magiccommand", u.MagicCommandBonus);
                UndeadLeader.Add("#undcommand", u.UndeadCommandBonus);
            }

            // old age: from the age it starts at past its max age, a penalty per quarter of its max age (at most 6)
            int oldYears = StartAge.Value - MaxAge.Value;
            if (oldYears >= 0)
            {
                IsOld = true;
                int mult = MaxAge.Value > 0 ? Math.Min(6, 1 + (int)Math.Floor(4.0 * oldYears / MaxAge.Value)) : 6;
                string label = $"old age (starts at {StartAge.Value}, max age {MaxAge.Value})";
                Str.Add(label, -mult);
                Att.Add(label, -mult);
                Def.Add(label, -mult);
                Prec.Add(label, -0.5 * mult);
                Enc.Add(label, mult);
                Hp.Add(label, Hp.Value * (-0.05 * mult));
                Ap.Add(label, Ap.Value * (-0.05 * mult));
            }

            bool mounted = u.MountId > 0;
            if (mounted)
                Def.Add("mounted", 3);

            var mount = mounted ? monster?.Invoke(u.MountId) : null;
            if (!u.Pretender)
            {
                Rcost = new DerivedValue(u.Rcost, u.Rcost);
                if (mount != null)
                {
                    int mr = ResourceCostOf(mount, monster);
                    Rcost.Add($"mount ({(mount.Name.Length > 0 ? mount.Name : "#" + mount.Id)})", mr);
                }
            }

            // weapons: their defence, their resource cost scaled by resource size
            Def.Add("weapons", u.Weapons.Sum(w => w.Def));
            Rcost?.Add("weapons", u.Weapons.Sum(w => w.Rcost * u.ResSize / 3.0));

            // more than one melee weapon (not bonus ones): an attack penalty of their lengths together, less ambidextrous
            var arms = u.Weapons.Where(w => !w.Missile && !w.Bonus).ToList();
            int penalty = -arms.Sum(w => w.Len);
            if (arms.Count > 1 && penalty < 0)
            {
                int amb = Math.Min(u.Ambidextrous, -penalty);
                DualWield = penalty + amb;
                DualWieldNotes.Add($"{penalty} dual wield: the lengths of its {arms.Count} melee weapons");
                if (amb > 0)
                    DualWieldNotes.Add($"+{amb} ambidextrous");
            }

            Gear();
            Protection();
            Encumbrance();
            Movement();
            WeaponLines();
            Costs(monster);
        }

        /// <summary>Start and max age, their defaults by kind, and what the paths add to max age.</summary>
        private void Ages()
        {
            var u = Unit;
            int? start = u.StartAge, max = u.MaxAge;
            string kind;
            int path, dflt;
            if (u.Undead)
            {
                kind = "undead";
                start ??= max is int m ? (int)(m * 0.4) : 187;
                dflt = 500;
                path = 5;
            }
            else if (u.Inanimate)
            {
                kind = "inanimate";
                start ??= 180 * u.Size;
                dflt = 400 * u.Size;
                path = 3;
            }
            else if (u.Demon)
            {
                kind = "demon";
                start ??= max is int m ? (int)(m * 0.4) : 370;
                dflt = 1000;
                path = 8;
            }
            else
            {
                kind = "";
                start ??= max is int m ? (int)(m * 0.5) + (int)(m * 0.1) : 22;
                dflt = 50;
                path = 6;
            }
            string of = kind.Length > 0 ? " for " + (kind == "demon" ? "demons" : kind == "undead" ? "the undead" : "the inanimate") : "";
            StartAge = new DerivedValue(u.StartAge, start.Value, u.StartAge != null ? null
                : u.Inanimate ? $"not set: 180 x its size" : u.MaxAge != null ? $"not set: worked out from its max age{of}" : $"not set: the default{of}");
            MaxAge = new DerivedValue(u.MaxAge, max ?? dflt, u.MaxAge != null ? null : $"not set: the default{of}");
            // each level of the kind's path adds half (death undead, earth inanimate, blood demons, nature the living)
            if (Path(path) > 0)
                MaxAge.Add($"{PathNames[path]} magic", MaxAge.Value * Path(path) * 0.5);
            // fire magic shortens a living being's life
            if (kind.Length == 0 && Path(0) > 0)
                MaxAge.Add("fire magic", MaxAge.Value >= 200 ? Path(0) * -5 : MaxAge.Value >= 50 ? Path(0) * -2 : MaxAge.Value != 0 ? -Path(0) : 0);
        }

        /// <summary>The armor that counts: one of each type (the last), in the order the types first appear.</summary>
        private void Gear()
        {
            var order = new List<int>();
            var byType = new Dictionary<int, ArmorStats>();
            foreach (var a in Unit.Armor)
            {
                if (!byType.ContainsKey(a.Type))
                    order.Add(a.Type);
                byType[a.Type] = a;
            }
            foreach (var type in order)
                Armor.Add(byType[type]);
        }

        private void Protection()
        {
            var u = Unit;
            int natural = Prot.Value;
            int body = 0, head = 0, general = 0, parry = 0, def = 0;
            double rcost = 0;
            foreach (var a in Armor)
            {
                def += a.DefShown;
                if (a.ProtBody != 0) body = a.ProtBody;
                if (a.ProtHead != 0) head = a.ProtHead;
                if (a.General != 0) general = a.General;
                if (a.Type == ArmorStats.Shield)
                    parry = a.Parry;
                else if (a.Type == ArmorStats.Misc && a.Prot - natural > 0)
                {
                    // a misc armor's protection replaces natural protection when higher
                    Prot.Add(a.Name, a.Prot - natural);
                    natural = Prot.Value;
                }
                rcost += a.Rcost * u.ResSize / 3.0;
            }
            Def.Add("armor", def);
            Def.Add("shield parry", parry);
            Rcost?.Add("armor", rcost);
            NaturalProt = natural;
            BodyProt = natural;
            HeadProt = natural;
            if (body != 0 || head != 0)
            {
                // armor over natural protection: p = natural + armor - natural x armor / 40, for the body and the head
                BodyProt = natural + body - natural * body / 40.0;
                HeadProt = natural + head - natural * head / 40.0;
                double total = (BodyProt * 4 + HeadProt) / 5;
                if (HeadProt > 10 && general == 0)
                    total = Math.Floor(total);
                int shown = (int)Math.Floor(total + 0.5);
                Prot.Add("armor", shown - Prot.Value);
                Prot.Notes.Add($"body {Round(BodyProt)}, head {Round(HeadProt)}: (body x4 + head) / 5");
            }
        }

        private void Encumbrance()
        {
            var u = Unit;
            int armor = Armor.Sum(a => a.Enc);
            if (armor == 0)
                return;
            CastingEnc = Enc.Value + armor * 2;
            // mounted, armor weighs half (rounded); with no encumbrance of its own (undead), armor only slows it
            int felt = u.MountId > 0 ? (int)Math.Floor(armor / 2.0 + 0.5) : armor;
            Ap.Add(u.MountId > 0 ? "armor (half: mounted)" : "armor", -felt);
            if (Enc.Value != 0)
                Enc.Add(u.MountId > 0 ? "armor (half: mounted)" : "armor", felt);
            if (u.IsMage)
                Enc.Notes.Add($"casting: {CastingEnc} (its own + twice its armor's)");
        }

        /// <summary>Map move (the inspector's prepareData_PostSiteData): old values converted, +2 for commanders, minus body armor.</summary>
        private void Movement()
        {
            var u = Unit;
            if (u.MapMove == null)
            {
                MapMove.Notes.Add("not set: 14");
                return;
            }
            int mm = u.MapMove.Value;
            // 0: it doesn't move (the inspector converts it like an old value)
            if (mm == 0)
                return;
            if (mm < 100)
            {
                if (mm < 6)
                {
                    int converted = mm * 6 + 2 + (u.Flying ? 6 : 0) - (u.Slave ? 2 : 0);
                    MapMove.Add("an old value (1-5), converted", converted - mm);
                    mm = converted;
                }
                if (u.Commander)
                {
                    MapMove.Add("commander", 2);
                    mm += 2;
                }
                if (mm > 40)
                {
                    int rounded = (mm + 2) / 5 * 5;
                    MapMove.Add("rounded to 5", rounded - mm);
                }
            }
            if (!u.NoMovePen)
            {
                int pen = Armor.Where(a => a.Type == ArmorStats.Body).Sum(a => a.MovePen);
                if (u.Enc == 0)
                    pen = (int)Math.Floor(pen / 2.0);
                MapMove.Add(u.Enc == 0 ? "body armor (half: no encumbrance)" : "body armor", -pen);
            }
        }

        private void WeaponLines()
        {
            var u = Unit;
            foreach (var w in u.Weapons)
            {
                var line = new WeaponTotals(w);
                if (!w.Missile)
                {
                    line.Attack = Att.Value + DualWield + w.Att;
                    line.AttackParts.Add(new Contribution("unit's attack", Att.Value));
                    if (DualWield != 0)
                        line.AttackParts.Add(new Contribution("dual wield", DualWield));
                    line.AttackParts.Add(new Contribution("weapon", w.Att));
                    // a big unit (size 6+) reaches one further with a weapon (not a bonus one)
                    if (u.Size > 5 && !w.Bonus)
                    {
                        line.Reach = w.Len + 1;
                        line.ReachNote = "size 6+: one longer";
                    }
                    else
                        line.Reach = Math.Max(w.Len, 0);
                }
                else
                {
                    line.Attack = Prec.Value + w.Att;
                    line.AttackParts.Add(new Contribution("unit's precision", Prec.Value));
                    line.AttackParts.Add(new Contribution("weapon", w.Att));
                    // a negative range is strength divided by it (thrown weapons)
                    double range = w.Range is <= -1 and >= -5 ? Str.Value / (double)-w.Range : w.Range == 1 ? 0 : w.Range;
                    line.Reach = (int)Math.Floor(range);
                    if (w.Range < 0)
                        line.ReachNote = w.Range == -1 ? "its strength" : $"its strength / {-w.Range}";
                }
                if (w.Dmg is int dmg)
                {
                    int str = Str.Value;
                    int add;
                    (add, line.StrengthNote) = w.Strength switch
                    {
                        StrengthAdded.None => (0, "no strength"),
                        StrengthAdded.Half => ((int)Math.Floor(str / 2.0), "half strength"),
                        StrengthAdded.Third => ((int)Math.Floor(str / 3.0), "a third of strength"),
                        _ when w.TwoHanded && !w.Missile => ((int)Math.Floor(str * 1.25), "strength x1.25 (two-handed)"),
                        _ => (str, "strength"),
                    };
                    line.StrengthAdded = add;
                    line.Damage = dmg + add;
                }
                Weapons.Add(line);
            }
        }

        private void Costs(Func<int, UnitStats?>? monster)
        {
            var u = Unit;
            if (u.Pretender)
                return;
            Gold = GoldCost.Of(u, u.CommanderCost, monster, GoldNotes);
            if (Rcost == null)
                return;
            if (Rcost.Value > 60000)
            {
                Rcost.Notes.Add("over 60000: 1 (gladiators)");
                Rcost.Value = 1;
            }
            if (Gold == 0)
            {
                Rcost.Notes.Add("no gold cost: no resources");
                Rcost.Value = 0;
            }
            else if (Rcost.Value == 0)
            {
                Rcost.Notes.Add("at least 1");
                Rcost.Value = 1;
            }
        }

        /// <summary>
        /// A mount's resource cost as the rider pays it (the inspector's rcostsort): its own, its
        /// weapons' and armor's by its resource size, rounded down; 0 if it has no gold cost.
        /// </summary>
        private static int ResourceCostOf(UnitStats m, Func<int, UnitStats?>? monster)
        {
            double r = m.Rcost + m.Weapons.Sum(w => w.Rcost * m.ResSize / 3.0);
            var byType = new Dictionary<int, ArmorStats>();
            foreach (var a in m.Armor)
                byType[a.Type] = a;
            r += byType.Values.Sum(a => a.Rcost * m.ResSize / 3.0);
            if (r > 60000)
                r = 1;
            if (GoldCost.Of(m, m.CommanderCost, monster, null) == 0)
                return 0;
            return (int)Math.Floor(r == 0 ? 1 : r);
        }

        private static string Round(double v) => Math.Abs(v - Math.Round(v)) < 0.05 ? ((int)Math.Round(v)).ToString() : v.ToString("0.#");
    }
}
