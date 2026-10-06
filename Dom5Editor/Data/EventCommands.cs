using System.IO;
using System.Text.Json;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Data
{
    /// <summary>
    /// How the event page reads each event command (event_commands.json, docs/EVENT_EDITOR.md E-2):
    /// its group, a sentence with the value in it, and the kind of value (which decides its editor
    /// and how a number reads: "spring", "Fire", "Accept, Decline").
    /// </summary>
    public static class EventCommands
    {
        public sealed class Def
        {
            public string Name { get; init; } = "";
            public string Group { get; init; } = "";
            public string Arg { get; init; } = "number";
            public string Text { get; init; } = "";
            public string? Text1 { get; init; }
            public string? Text0 { get; init; }
            public string? Icon { get; init; }
            public bool IsRequirement => Groups.FirstOrDefault(g => g.Id == Group)?.IsRequirement ?? Name.StartsWith("req_");
        }

        public sealed class GroupDef
        {
            public string Id { get; init; } = "";
            public string Title { get; init; } = "";
            public bool IsRequirement { get; init; }
        }

        private static Dictionary<string, Def>? _defs;
        private static List<GroupDef> _groups = new List<GroupDef>();
        private static readonly Dictionary<string, List<(long Value, string Name)>> _values = new Dictionary<string, List<(long, string)>>();

        private static Dictionary<string, Def> Defs
        {
            get
            {
                if (_defs != null)
                    return _defs;
                _defs = new Dictionary<string, Def>();
                try
                {
                    var path = Path.Combine(AppContext.BaseDirectory, "Data", "event_commands.json");
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    var root = doc.RootElement;
                    _groups = root.GetProperty("groups").EnumerateArray().Select(g => new GroupDef
                    {
                        Id = g.GetProperty("id").GetString() ?? "",
                        Title = g.GetProperty("title").GetString() ?? "",
                        IsRequirement = g.GetProperty("kind").GetString() == "req",
                    }).ToList();
                    foreach (var c in root.GetProperty("commands").EnumerateObject())
                    {
                        string? S(string key) => c.Value.TryGetProperty(key, out var v) ? v.GetString() : null;
                        _defs[c.Name] = new Def
                        {
                            Name = c.Name, Group = S("group") ?? "", Arg = S("arg") ?? "number", Text = S("text") ?? "",
                            Text1 = S("text1"), Text0 = S("text0"), Icon = S("icon"),
                        };
                    }
                    foreach (var v in root.GetProperty("values").EnumerateObject())
                        _values[v.Name] = v.Value.EnumerateArray().Select(row => (row[0].GetInt64(), row[1].GetString() ?? "")).ToList();
                }
                catch (Exception)
                {
                    // without the file, event lines show as their commands
                }
                return _defs;
            }
        }

        public static IReadOnlyList<GroupDef> Groups
        {
            get
            {
                _ = Defs;
                return _groups;
            }
        }

        /// <summary>A command's reading, by its name without '#' ("req_rare"), or null.</summary>
        public static Def? Get(string name) => Defs.TryGetValue(name.TrimStart('#').ToLowerInvariant(), out var d) ? d : null;

        public static IEnumerable<Def> All => Defs.Values;

        /// <summary>Whether a kind of value is one of a list (a season, a scale, ...): edited with a choice.</summary>
        public static bool IsChoice(string arg) => Values(arg).Count > 0 && !IsMask(arg);

        /// <summary>Whether a kind of value is a sum of flags (orders, afflictions, terrain).</summary>
        public static bool IsMask(string arg) => arg.EndsWith("mask");

        public static IReadOnlyList<(long Value, string Name)> Values(string arg)
        {
            _ = Defs;
            return _values.TryGetValue(arg, out var l) ? l : (IReadOnlyList<(long, string)>)Array.Empty<(long, string)>();
        }

        public static IReadOnlyList<ChoiceOption> Options(string arg) =>
            Values(arg).Select(v => new ChoiceOption((int)v.Value, v.Name) { Icon = IconOf(arg, v.Value) }).ToList();

        /// <summary>An icon for a value: a path's or a gem's.</summary>
        public static string? IconOf(string arg, long value) => arg switch
        {
            "path" or "targpath" => UI.Controls.GameIcons.PathLetter((int)value) is string l ? "path:" + l : null,
            "gem" => value <= 7 ? "gem:" + UI.Controls.GameIcons.PathLetter((int)value) : value == 8 ? "gem:B" : null,
            _ => null,
        };

        /// <summary>A value as words: "spring", "Fire", "Accept, Decline", "50%".</summary>
        public static string Words(string arg, long value)
        {
            if (IsMask(arg))
            {
                var names = Values(arg).Where(v => v.Value != 0 && (value & v.Value) == v.Value).Select(v => v.Name).ToList();
                return names.Count > 0 ? string.Join(", ", names) : value.ToString();
            }
            if (Values(arg).FirstOrDefault(v => v.Value == value) is { Name: not null } named)
                return named.Name;
            return arg == "percent" ? value + "%" : value.ToString();
        }
    }
}
