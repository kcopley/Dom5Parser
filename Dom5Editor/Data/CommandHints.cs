using System.IO;
using System.Text;
using System.Text.Json;
using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Editor.Data
{
    /// <summary>
    /// What each command does, from Illwinter's manuals (command_hints.json, made by
    /// tools/command_hints.py): its arguments, its description, and the value table printed with
    /// it. Used for hover hints, and to say what a number means next to it ("Fire", "always
    /// (unlimited)", "disease, curse").
    /// </summary>
    public static class CommandHints
    {
        public sealed class Hint
        {
            public string Args { get; init; } = "";
            public string Text { get; init; } = "";
            public string? Section { get; init; }
            /// <summary>The value table: number and meaning, as printed.</summary>
            public IReadOnlyList<(long Value, string Name)> Values { get; init; } = Array.Empty<(long, string)>();
            /// <summary>The table's header ("Nbr Magic Path", "2^x Affliction": its numbers are bit positions).</summary>
            public string? Header { get; init; }
            public bool IsBitTable => Header?.StartsWith("2^x") == true;
            public bool IsMaskTable => Header?.StartsWith("Mask") == true || Args.Contains("mask", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, Dictionary<string, Hint>>? _hints;

        private static Dictionary<string, Dictionary<string, Hint>> Hints => _hints ??= Load();

        private static Dictionary<string, Dictionary<string, Hint>> Load()
        {
            var result = new Dictionary<string, Dictionary<string, Hint>>();
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "command_hints.json");
                if (!File.Exists(path))
                    return result;
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                foreach (var type in doc.RootElement.GetProperty("types").EnumerateObject())
                {
                    var commands = new Dictionary<string, Hint>();
                    foreach (var c in type.Value.EnumerateObject())
                    {
                        var e = c.Value;
                        var values = new List<(long, string)>();
                        if (e.TryGetProperty("values", out var vs))
                            foreach (var row in vs.EnumerateArray())
                                values.Add((row[0].GetInt64(), row[1].GetString() ?? ""));
                        commands[c.Name] = new Hint
                        {
                            Args = e.TryGetProperty("args", out var a) ? a.GetString() ?? "" : "",
                            Text = e.TryGetProperty("text", out var t) ? t.GetString() ?? "" : "",
                            Section = e.TryGetProperty("section", out var s) ? s.GetString() : null,
                            Header = e.TryGetProperty("header", out var h) ? h.GetString() : null,
                            Values = values,
                        };
                    }
                    result[type.Name] = commands;
                }
            }
            catch (Exception)
            {
                // no hints is no reason to fail: tooltips fall back to the badge config's text
            }
            return result;
        }

        /// <summary>The manual's chapter for an entity type.</summary>
        private static string? Chapter(EntityType type) => type switch
        {
            EntityType.MONSTER => "monster",
            EntityType.WEAPON => "weapon",
            EntityType.ARMOR => "armor",
            EntityType.SPELL => "spell",
            EntityType.ITEM => "item",
            EntityType.SITE => "site",
            EntityType.NATION => "nation",
            EntityType.MERCENARY => "merc",
            EntityType.POPTYPE => "poptype",
            EntityType.NAMETYPE => "nametype",
            EntityType.EVENT => "event",
            EntityType.BLESS => "bless",
            EntityType.TEMPLATE => "template",
            _ => null,
        };

        /// <summary>The manual's entry for a command of an entity type (a numbered family falls back to its first: #batstartsum2 to #batstartsum1), or null.</summary>
        public static Hint? Get(EntityType type, Command command)
        {
            if (Chapter(type) is not string chapter || !CommandsMap.TryGetString(command, out var name))
                return null;
            name = name.TrimStart('#').ToLowerInvariant();
            foreach (var key in new[] { chapter, "general" })
            {
                if (!Hints.TryGetValue(key, out var commands))
                    continue;
                if (commands.TryGetValue(name, out var hint))
                    return hint;
                var family = System.Text.RegularExpressions.Regex.Replace(name, @"\d+", "1");
                if (family != name && commands.TryGetValue(family, out hint))
                    return hint;
            }
            return null;
        }

        /// <summary>A hover hint: "#cmd <args>", the manual's description, and its value table.</summary>
        public static string? Tooltip(EntityType type, Command command)
        {
            var hint = Get(type, command);
            if (hint == null)
                return null;
            CommandsMap.TryGetString(command, out var name);
            var sb = new StringBuilder();
            sb.Append(name).Append(' ').Append(hint.Args).AppendLine();
            sb.Append(Wrap(hint.Text, 90));
            if (hint.Values.Count > 0)
            {
                sb.AppendLine().AppendLine();
                var rows = hint.Values.Take(32).Select(v => $"{(hint.IsBitTable ? (1L << (int)v.Value).ToString() : v.Value.ToString()),8}  {v.Name}");
                sb.Append(string.Join("\n", rows));
                if (hint.Values.Count > 32)
                    sb.Append($"\n   ... {hint.Values.Count - 32} more");
                if (hint.IsBitTable || hint.IsMaskTable)
                    sb.Append("\n(add the numbers together for several)");
            }
            return sb.ToString();
        }

        private static readonly string[] ValueWords = { "path", "gem", "school", "rarity", "scale", "affliction", "order", "type", "era", "season", "mask", "nbr", "plane", "realm", "poptype" };

        /// <summary>
        /// What a value means, from the command's value table: "Fire" for a path number, the
        /// names of the bits set in a mask ("disease, curse"); "" when the table doesn't say, or the
        /// argument isn't one the table describes (an amount, a percent).
        /// </summary>
        public static string ValueNote(EntityType type, Command command, string arguments)
        {
            var hint = Get(type, command);
            if (hint == null || hint.Values.Count == 0)
                return "";
            var first = hint.Args.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            if (!ValueWords.Any(w => first.Contains(w, StringComparison.OrdinalIgnoreCase)))
                return "";
            var arg = arguments.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!long.TryParse(arg, out var value))
                return "";
            if (hint.IsBitTable || hint.IsMaskTable)
            {
                var names = new List<string>();
                long rest = value;
                foreach (var (v, name) in hint.Values)
                {
                    long bit = hint.IsBitTable ? 1L << (int)v : v;
                    if (bit != 0 && (value & bit) == bit)
                    {
                        names.Add(name);
                        rest &= ~bit;
                    }
                }
                if (value == 0)
                    return hint.Values.FirstOrDefault(v => v.Value == 0).Name ?? "";
                return names.Count > 0 && rest == 0 ? string.Join(", ", names) : "";
            }
            return hint.Values.FirstOrDefault(v => v.Value == value).Name ?? "";
        }

        private static string Wrap(string text, int width)
        {
            var sb = new StringBuilder();
            int col = 0;
            foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (col > 0 && col + word.Length + 1 > width)
                {
                    sb.AppendLine();
                    col = 0;
                }
                else if (col > 0)
                {
                    sb.Append(' ');
                    col++;
                }
                sb.Append(word);
                col += word.Length;
            }
            return sb.ToString();
        }
    }
}
