namespace Dom5Edit.Derived
{
    /// <summary>
    /// A magic item's gem cost to forge, by the level each of its paths needs: 5, 10, 15, 25, 40,
    /// 60, 80, 100 or 120 gems for levels 1 to 9, changed by #itemcost1 / #itemcost2 (percent),
    /// rounded (the dom6inspector's MItem.prepareData_PostMod).
    /// </summary>
    public static class ItemCost
    {
        private static readonly int[] ByLevel = { 0, 5, 10, 15, 25, 40, 60, 80, 100, 120 };

        /// <summary>The gems one path costs, or null for a level the table doesn't have.</summary>
        public static int? Gems(int level, int costPercent = 0)
        {
            if (level < 1 || level >= ByLevel.Length)
                return null;
            return (int)Math.Floor(ByLevel[level] * (1 + costPercent / 100.0) + 0.5);
        }

        /// <summary>The cost as the inspector writes it ("10F5W"): gems and path letter, main path then second.</summary>
        public static string Text(int mainPath, int mainLevel, int cost1, int secondPath, int secondLevel, int cost2)
        {
            string Part(int path, int level, int cost) =>
                path >= 0 && path < UnitStats.PathLetters.Length && Gems(level, cost) is int g ? g + UnitStats.PathLetters[path] : "";
            return Part(mainPath, mainLevel, cost1) + Part(secondPath, secondLevel, cost2);
        }
    }
}
