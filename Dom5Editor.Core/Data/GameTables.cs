using System.IO;
using System.Text.Json;
using Dom5Edit;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Data
{
    /// <summary>
    /// Number tables the modding manual defines (docs/pdf_extracted): magic paths, schools, item
    /// types, site rarity; and the spell effect types (spell_effect_types.json: each effect's name and
    /// what its #damage value means).
    /// </summary>
    public static class GameTables
    {
        private static ChoiceOption[] Opts(params (int, string)[] xs) => xs.Select(x => new ChoiceOption(x.Item1, x.Item2)).ToArray();

        /// <summary>Path options with their icons (path 0-9: "path:F" ...; gems: "gem:F").</summary>
        private static ChoiceOption[] PathOpts(string prefix, params (int, string)[] xs) => xs.Select(x => new ChoiceOption(x.Item1, x.Item2)
        {
            Icon = UI.Controls.GameIcons.PathLetter(x.Item1) is string l ? prefix + l : null,
        }).ToArray();

        /// <summary>Monster and item magic paths (#magicskill, #mainpath): 0-8 and holy.</summary>
        public static readonly IReadOnlyList<ChoiceOption> Paths = PathOpts("path:",
            (0, "Fire"), (1, "Air"), (2, "Water"), (3, "Earth"), (4, "Astral"), (5, "Death"), (6, "Nature"),
            (7, "Glamour"), (8, "Blood"), (9, "Holy"));

        /// <summary>Paths for a second requirement (-1: none).</summary>
        public static readonly IReadOnlyList<ChoiceOption> PathsOrNone = new[] { new ChoiceOption(-1, "None") }.Concat(Paths).ToArray();

        /// <summary>Spell paths (#path n p): -1 none, 0-8, 9 priest.</summary>
        public static readonly IReadOnlyList<ChoiceOption> SpellPaths = PathOpts("path:",
            (-1, "None"), (0, "Fire"), (1, "Air"), (2, "Water"), (3, "Earth"), (4, "Astral"), (5, "Death"),
            (6, "Nature"), (7, "Glamour"), (8, "Blood"), (9, "Priest"));

        /// <summary>Gem types (#gems path n): paths 0-8.</summary>
        public static readonly IReadOnlyList<ChoiceOption> GemPaths = Paths.Take(9).Select(o => new ChoiceOption(o.Value, o.Name) { Icon = "gem:" + GemLetter(o.Value) }).ToArray();

        private static string GemLetter(int path) => UI.Controls.GameIcons.PathLetter(path) ?? "";

        public static readonly IReadOnlyList<ChoiceOption> Schools = Opts(
            (-1, "Cannot be researched"), (0, "Conjuration"), (1, "Alteration"), (2, "Evocation"), (3, "Construction"),
            (4, "Enchantment"), (5, "Thaumaturgy"), (6, "Blood"), (7, "Divine"));

        public static readonly IReadOnlyList<ChoiceOption> ItemTypes = Opts(
            (1, "1-handed weapon"), (2, "2-handed weapon"), (3, "Missile weapon"), (4, "Shield"), (5, "Body armor"),
            (6, "Helmet"), (7, "Boots"), (8, "Misc item"), (9, "Crown"), (10, "Barding"));

        /// <summary>Armor #type (the manual: 4 shield, 5 body armor, 6 helmet, 9 barding; 8 is in vanilla data too).</summary>
        public static readonly IReadOnlyList<ChoiceOption> ArmorTypes = Opts(
            (4, "Shield"), (5, "Body armor"), (6, "Helmet"), (8, "Misc"), (9, "Barding"));

        public static readonly IReadOnlyList<ChoiceOption> SiteLevels = Opts(
            (0, "0 (found automatically)"), (1, "1"), (2, "2"), (3, "3"), (4, "4"));

        public static readonly IReadOnlyList<ChoiceOption> SiteRarity = Opts(
            (0, "Common"), (1, "Uncommon"), (2, "Rare"), (5, "Never random"),
            (11, "Throne of Ascension, level 1"), (12, "Throne of Ascension, level 2"), (13, "Throne of Ascension, level 3"));

        /// <summary>A spell effect: its name and what its #damage value means.</summary>
        public sealed class EffectType
        {
            public string Name = "";
            public string ArgumentType = "damage";
            public string? Notes;
        }

        private static Dictionary<int, EffectType>? _effects;
        private static Dictionary<string, string>? _argumentTypes;

        /// <summary>Spell effects by number (from spell_effect_types.json, found next to the spell mapping the vanilla loader uses).</summary>
        public static IReadOnlyDictionary<int, EffectType> Effects
        {
            get
            {
                if (_effects != null)
                    return _effects;
                _effects = new Dictionary<int, EffectType>();
                _argumentTypes = new Dictionary<string, string>();
                var path = VanillaLoader.SpellEffectTypesPath;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    foreach (var e in doc.RootElement.GetProperty("effect_types").EnumerateObject())
                        if (int.TryParse(e.Name, out var n))
                            _effects[n] = new EffectType
                            {
                                // ("Damage {Wpn: #dt_normal}": the weapon damage type it matches is for the data, not the name)
                                Name = System.Text.RegularExpressions.Regex.Replace(e.Value.GetProperty("name").GetString() ?? $"effect {n}", @"\s*\{[^}]*\}", "")
                                    .Replace(" | ", " / "),
                                ArgumentType = e.Value.TryGetProperty("argument_type", out var a) ? a.GetString() ?? "damage" : "damage",
                                Notes = e.Value.TryGetProperty("notes", out var nt) ? nt.GetString() : null,
                            };
                    if (doc.RootElement.TryGetProperty("argument_types", out var args))
                        foreach (var a in args.EnumerateObject())
                            _argumentTypes[a.Name] = a.Value.GetString() ?? "";
                }
                return _effects;
            }
        }

        /// <summary>The effect for a spell's #effect value: rituals add 10000 to the combat effect's number.</summary>
        public static EffectType? EffectOf(int effect) =>
            Effects.TryGetValue(effect, out var e) ? e : Effects.TryGetValue(effect % 10000, out var r) ? r : null;

        public static string ArgumentDescription(string argumentType)
        {
            _ = Effects;
            return _argumentTypes != null && _argumentTypes.TryGetValue(argumentType, out var d) ? d : "";
        }

        /// <summary>Every effect as a choice, combat and ritual (+10000), plus a value not in the table.</summary>
        public static IReadOnlyList<ChoiceOption> EffectChoices(int? current)
        {
            var list = Effects.OrderBy(e => e.Key).Select(e => new ChoiceOption(e.Key, $"{e.Key}: {e.Value.Name}"))
                .Concat(Effects.OrderBy(e => e.Key).Select(e => new ChoiceOption(10000 + e.Key, $"{10000 + e.Key}: {e.Value.Name} (ritual)")))
                .ToList();
            if (current is int c && !list.Any(o => o.Value == c))
                list.Insert(0, new ChoiceOption(c, $"{c}: (not in the effect table)"));
            return list;
        }
    }
}
