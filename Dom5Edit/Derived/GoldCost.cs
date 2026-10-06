namespace Dom5Edit.Derived
{
    /// <summary>
    /// A monster's gold cost as the recruitment screen shows it: #gcost below 5000 as it is;
    /// from 10000 up worked out by the game from the base above 10000. The dom6inspector's
    /// MUnit.goldCost / monsterGoldCost / magicHalfLevels, ported: a commander pays for its
    /// leadership, paths, priesthood and spying (the largest in full, the others half), sacred
    /// units 30% more, slow recruits 10% less, commanders 40% more; a mount and extra riders are
    /// added at their unit price; commanders and anything over 30 are rounded down to 5.
    /// </summary>
    public static class GoldCost
    {
        private static readonly Dictionary<int, int> LeaderCost = new Dictionary<int, int>
        {
            { 0, 10 }, { 10, 15 }, { 50, 30 }, { 100, 60 }, { 150, 100 }, { 200, 150 },
        };
        // by half levels: the highest path, every other path, and holy (priest levels)
        private static readonly int[] HighestPath = { 0, 15, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300 };
        private static readonly int[] OtherPath = { 0, 10, 20, 40, 60, 80, 100, 120, 140, 160, 180, 200 };
        private static readonly int[] Priest = { 0, 10, 20, 30, 40, 60, 100, 100, 140, 160 };

        /// <summary>
        /// The price. <paramref name="monster"/> finds the mount, co-rider and other shape by ID;
        /// <paramref name="notes"/> gets the steps, for a tooltip (null: not wanted).
        /// </summary>
        public static int Of(UnitStats u, bool commander, Func<int, UnitStats?>? monster, List<string>? notes)
        {
            int gold = MonsterCost(u, commander, worst: true, monster, notes);
            if (commander)
            {
                // a random path counts on its worst path for a quarter, its best for three quarters
                int best = MonsterCost(u, commander, worst: false, monster, null);
                if (best != gold)
                {
                    int mixed = (gold + 3 * best) / 4;
                    notes?.Add($"random paths: {gold} on their weakest path, {best} on their strongest: {mixed}");
                    gold = mixed;
                }
            }
            if (u.Riders > 1)
            {
                var rider = u.CoRiderId > 0 ? monster?.Invoke(u.CoRiderId) : u;
                if (rider != null)
                {
                    int each = MonsterCost(rider, false, worst: true, monster, null);
                    gold += each * (u.Riders - 1);
                    notes?.Add($"+{each * (u.Riders - 1)} {u.Riders - 1} more rider{(u.Riders > 2 ? "s" : "")} at {each}");
                }
            }
            if (u.MountId > 0 && monster?.Invoke(u.MountId) is UnitStats mount)
            {
                int m = MonsterCost(mount, false, worst: true, monster, null);
                gold += m;
                notes?.Add($"+{m} mount ({(mount.Name.Length > 0 ? mount.Name : "#" + mount.Id)})");
            }
            if (commander || gold > 30)
            {
                int rounded = 5 * (int)Math.Floor(gold / 5.0);
                if (rounded != gold)
                    notes?.Add($"rounded down to {rounded}");
                gold = rounded;
            }
            return gold;
        }

        private static int MonsterCost(UnitStats o, bool commander, bool worst, Func<int, UnitStats?>? monster, List<string>? notes)
        {
            // #shapechange: when the other shape has a lower number, its cost is used
            for (int guard = 0; guard < 20 && o.ShapeChange > 0 && o.ShapeChange < o.Id && monster?.Invoke(o.ShapeChange) is UnitStats other; guard++)
            {
                notes?.Add($"costs what its other shape ({(other.Name.Length > 0 ? other.Name : "#" + other.Id)}) costs");
                o = other;
            }
            if (o.Gcost < 5000)
                return o.Gcost;

            int cost = o.Gcost - 10000;
            notes?.Add($"worked out by the game from a base of {cost}");
            int priestCost = 0;
            if (commander)
            {
                // the inspector prices the leadership it has (class and the game's bonus); an odd value as OK
                int leadership = o.Leader + o.GameLeaderBonus;
                int ldr = 10;
                if (leadership > 0)
                {
                    ldr = LeaderCost.TryGetValue(leadership, out var c) ? c : 30;
                    ldr += Math.Max(o.Inspirational, -2) * 10;
                }
                if (o.SailingShipSize > 0)
                    ldr += ldr / 2;

                var levels = HalfLevels(o, worst);
                int highest = -1;
                for (int i = 0; i < 9; i++)
                    if (levels[i] > 0 && (highest == -1 || levels[i] > levels[highest]))
                        highest = i;
                int paths = 0;
                if (highest != -1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        int lvl = Math.Min(levels[i], 11);
                        paths += i == highest ? HighestPath[lvl] : OtherPath[lvl];
                    }
                    if (o.ForgeBonus > 0)
                        paths += paths * o.ForgeBonus / 100;
                    if (paths > 0)
                        paths += Math.Max(o.ResearchBonus, -1) * 5;
                }
                priestCost = Priest[Math.Min(levels[9], 9)];
                int spy = (o.Assassin > 0 ? 40 : 0) + (o.Spy > 0 ? 40 : 0) + (o.Seduce > 0 ? 60 : 0);

                // the largest of the four in full, the others half
                var parts = new[] { ldr, paths, priestCost, spy }.OrderByDescending(x => x).ToArray();
                int added = parts[0] + parts[1] / 2 + parts[2] / 2 + parts[3] / 2;
                cost += added;
                notes?.Add($"+{added} as a commander: leadership {ldr}, paths {paths}, priest {priestCost}, spying {spy} (the largest in full, the others half)");
                if (o.AutoHealer > 0)
                {
                    int h = Math.Min((o.AutoHealer + 2) * 10, 60);
                    cost += h;
                    notes?.Add($"+{h} healer");
                }
                if (o.AutoDisHealer > 0)
                {
                    int h = Math.Min((o.AutoDisHealer + 3) * 5, 30);
                    cost += h;
                    notes?.Add($"+{h} disease healer");
                }
            }
            if (commander && o.Stealthy >= 40)
            {
                cost += 5;
                notes?.Add("+5 stealthy");
            }
            if (o.MountedFlag)
            {
                cost += 10;
                notes?.Add("+10 mounted");
            }
            if (o.Holy || priestCost > 0)
            {
                cost = cost * 130 / 100;
                notes?.Add($"x1.3 sacred: {cost}");
            }
            if (o.SlowRec || o.RpCost >= 4 && o.RpCost <= 6)
            {
                cost = cost * 90 / 100;
                notes?.Add($"x0.9 slow to recruit: {cost}");
            }
            if (commander)
            {
                cost = cost * 140 / 100;
                notes?.Add($"x1.4 commander: {cost}");
            }
            return Math.Max(cost, 0);
        }

        /// <summary>
        /// Magic levels in half levels, F A W E S D N G B H, so a fixed level counts 2. A random
        /// path adds (chance + 25) / 50 to one of its paths: the lowest of them for the worst case,
        /// the highest for the best (ties: the first). 24% or less adds nothing; a roll among
        /// FAWESDNG (the game's "Random") isn't priced.
        /// </summary>
        public static int[] HalfLevels(UnitStats o, bool worst)
        {
            var levels = o.Paths.Select(p => 2 * p).ToArray();
            foreach (var rp in o.RandomPaths)
            {
                int chance = rp.Chance == 100 ? 100 * Math.Max(rp.Levels, 1) : rp.Chance;
                if (chance <= 24)
                    continue;
                if (rp.Paths.Count == 8 && rp.Paths.SequenceEqual(Enumerable.Range(0, 8)))
                    continue;
                int pick = -1;
                foreach (int i in rp.Paths.OrderBy(p => p))
                {
                    if (i < 0 || i >= levels.Length)
                        continue;
                    if (pick == -1 || !worst && levels[i] > levels[pick] || worst && levels[i] < levels[pick])
                        pick = i;
                }
                if (pick != -1)
                    levels[pick] += (chance + 25) / 50;
            }
            return levels;
        }
    }
}
