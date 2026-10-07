using Dom5Edit.Entities;

namespace Dom5Edit.GameData
{
    /// <summary>
    /// How the game reads a mod's texts, from Dominions6.exe (tools/dom6exe/README.md "Reading .dm
    /// files", dmread.py). A text runs from its opening quote to the next quote in the file, across
    /// lines. After the commands here the game's scan skips the text, so the command lines inside
    /// one whose closing quote is missing aren't read: they're text. After the others (#descr,
    /// #details, #portent, #cure, #summary, #brief, an item's #name) the scan goes on through the
    /// text, so the commands in it are read as well.
    /// </summary>
    public static class GameReading
    {
        private static readonly Dictionary<Type, HashSet<string>> Skipping = new()
        {
            [typeof(Monster)] = new() { "#name", "#spr1", "#spr2", "#xspr1", "#xspr2", "#unmountedspr1", "#unmountedspr2" },
            [typeof(Item)] = new() { "#spr" },
            [typeof(Spell)] = new() { "#name", "#sample" },
            [typeof(Nation)] = new() { "#name", "#epithet", "#flag", "#indepflag" },
            [typeof(Event)] = new() { "#msg" },
            [typeof(Site)] = new() { "#name" },
            [typeof(Armor)] = new() { "#name" },
            [typeof(Weapon)] = new() { "#name", "#sound" },
            [typeof(Mercenary)] = new() { "#name", "#bossname", "#com", "#item", "#unit" },
            [typeof(Nametype)] = new() { "#addname" },
            [typeof(Bless)] = new() { "#name" },
            [typeof(Template)] = new() { "#bless", "#form", "#researchgoal" },
        };

        /// <summary>Whether the game skips the text of <paramref name="command"/> ("#msg") in <paramref name="block"/>'s block.</summary>
        public static bool SkipsText(Entity? block, string command) =>
            block != null && Skipping.TryGetValue(block.GetType(), out var commands) && commands.Contains(command);
    }
}
