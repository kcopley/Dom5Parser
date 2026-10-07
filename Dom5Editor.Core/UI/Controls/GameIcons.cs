namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// The icon kinds the pages name ("path:F", "gem:S", "hp", ...): the path letters and colours.
    /// Each editor draws them (the Windows one: GameIcon and GameIconDrawings).
    /// </summary>
    public static class GameIcons
    {
        /// <summary>A magic path's colour (the letter: F A W E S D N G B H, R for random), as #RRGGBB.</summary>
        public static readonly IReadOnlyDictionary<string, string> PathColors = new Dictionary<string, string>
        {
            ["F"] = "#F06A2A", ["A"] = "#8FD3FF", ["W"] = "#3B82F6", ["E"] = "#B98446", ["S"] = "#C9A8FF",
            ["D"] = "#C98AA6", ["N"] = "#4CAF50", ["G"] = "#E37AD8", ["B"] = "#C2182E", ["H"] = "#F5C842", ["R"] = "#9E9E9E",
        };

        /// <summary>The letter of a magic path's number (0 fire ... 9 holy), or null.</summary>
        public static string? PathLetter(int path) => path >= 0 && path <= 9 ? "FAWESDNGBH"[path].ToString() : null;
    }
}
