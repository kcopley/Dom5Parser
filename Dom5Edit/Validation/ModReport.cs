using System.Text;
using System.Text.RegularExpressions;
using Dom5Edit.Entities;
using Dom5Edit.GameData;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// A report for a mod's author (Markdown): what the editor found, worst first, each with its
    /// line in the file and what the game does with it. Three parts: what goes wrong in game
    /// (events that can't happen or lose lines, references to nothing, numbers taken from the
    /// game), lines the game ignores (with "did you mean" for a misspelt command, or the type
    /// whose blocks read it), and things worth a look. What it says about the game comes from
    /// Dominions6.exe (tools/dom6exe) and the modding manuals.
    /// </summary>
    public static class ModReport
    {
        public static string Write(Mod mod, ValidationResult validation, IEnumerable<Events.EventProblem>? eventProblems = null)
        {
            var file = mod.FullFilePath;
            string[] lines = Array.Empty<string>();
            try
            {
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                    lines = File.ReadAllLines(file);
            }
            catch (IOException)
            {
                // quoted lines are left out
            }
            string Quote(int? line) => line is int n && n >= 1 && n <= lines.Length ? lines[n - 1].Trim() : "";

            var wrong = new List<Item>();
            var ignored = new List<Item>();
            var missing = new List<Item>();
            var look = new List<Item>();

            // lines the game doesn't read, from the parser
            foreach (var p in mod.ParseIssues)
            {
                // a #copystats after lines that set what it copies (name, stats, abilities: the exe)
                var copy = Regex.Match(p.Message ?? "", @"^#copystats at line \d+ overwrites \d+ previously defined property\(s\): (.*)$");
                if (p.IssueType == ParseIssueType.PropertiesClearedBySubsequentClear && copy.Success)
                {
                    look.Add(new Item(p.LineNumber, Quote(p.LineNumber), "copystats",
                        $"#copystats replaces what the lines before it in the block set ({copy.Groups[1].Value}): lines meant to change the copy go after it"));
                    continue;
                }
                if (p.IssueType != ParseIssueType.InvalidCommand && p.IssueType != ParseIssueType.NotReadByGame)
                    continue;
                var command = Regex.Match(p.Message ?? "", @"#[A-Za-z_][A-Za-z0-9_]*").Value;
                var typeName = Regex.Match(p.Message ?? "", @"for:? ([A-Z][a-z]+)").Groups[1].Value;
                Enum.TryParse<EntityType>(typeName, true, out var type);
                ignored.Add(new Item(p.LineNumber, Quote(p.LineNumber), command, IgnoredWhy(command, type, typeName)));
            }

            // references, numbers, names, from Validate
            foreach (var v in validation.Issues)
            {
                if (v.Category == "Invalid Command" || v.Severity == ValidationSeverity.Info)
                    continue; // (the parser's notes above say it better)
                var line = v.LineNumber ?? v.Property?.LineNumber;
                var item = new Item(line, Quote(line), v.Category ?? "", Explain(v));
                (v.Category == "Reference" && (v.Message ?? "").StartsWith("Unresolved reference") ? missing
                    : v.Severity == ValidationSeverity.Error || v.Category == "Reference" ? wrong : look).Add(item);
            }

            // events
            foreach (var e in eventProblems ?? Array.Empty<Events.EventProblem>())
            {
                var line = e.Line?.LineNumber is int n && n > 0 ? n
                    : e.Event.Properties.Select(p => p.LineNumber).Where(n2 => n2 > 0).DefaultIfEmpty(0).Min();
                var item = new Item(line > 0 ? line : null, Quote(line), "event", e.Message);
                (e.IsError ? wrong : look).Add(item);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"# {mod.ModName ?? Path.GetFileNameWithoutExtension(file)}: what the mod editor found");
            sb.AppendLine();
            var with = mod.Dependencies.Where(d => d != VanillaLoader.Vanilla && !string.IsNullOrEmpty(d.FullFilePath))
                .Select(d => $"`{Path.GetFileName(d.FullFilePath)}`").ToList();
            sb.AppendLine($"File: `{Path.GetFileName(file)}`" + (string.IsNullOrEmpty(mod.Version) ? "" : $", version {mod.Version}") +
                          (with.Count > 0 ? $", together with {string.Join(", ", with)}" : "") +
                          $". Checked {DateTime.Now:yyyy-MM-dd} against Dominions {GameCommandCatalog.GameVersion ?? "6"} (what the game reads is taken from the game itself).");
            sb.AppendLine();
            sb.AppendLine($"- **Goes wrong in game:** {wrong.Count}");
            sb.AppendLine($"- **Lines the game ignores:** {ignored.Count}");
            if (missing.Count > 0)
                sb.AppendLine($"- **Numbers not in this mod or the game:** {missing.Count}");
            sb.AppendLine($"- **Worth a look:** {look.Count}");
            Section(sb, "Goes wrong in game", "These change what happens in game, or keep something from happening.", wrong);
            Section(sb, "Lines the game ignores", "The game skips these lines: they change nothing.", ignored);
            MissingSection(sb, missing);
            Section(sb, "Worth a look", "Not wrong as such, but probably not what was meant.", look);
            if (wrong.Count + ignored.Count + missing.Count + look.Count == 0)
                sb.AppendLine("\nNothing found.");
            return sb.ToString();
        }

        private sealed record Item(int? Line, string Text, string Key, string Why);

        private static void Section(StringBuilder sb, string title, string intro, List<Item> items)
        {
            if (items.Count == 0)
                return;
            sb.AppendLine();
            sb.AppendLine($"## {title} ({items.Count})");
            sb.AppendLine();
            sb.AppendLine(intro);
            sb.AppendLine();
            // alike ones together (the same message), each with its lines
            foreach (var g in items.GroupBy(i => Regex.Replace(i.Why, @"-?\d+", "N")).OrderByDescending(g => g.Count()))
            {
                var first = g.First();
                sb.AppendLine($"- {first.Why}" + (g.Count() > 1 ? $" ({g.Count()} lines)" : ""));
                foreach (var i in g.OrderBy(i => i.Line ?? 0).Take(12))
                    sb.AppendLine(i.Line is int n ? $"  - line {n}: `{Trim(i.Text)}`" : $"  - {Trim(i.Text)}");
                if (g.Count() > 12)
                    sb.AppendLine($"  - and {g.Count() - 12} more");
            }
        }

        /// <summary>References to numbers nothing has, by kind: the numbers, then a few of the lines.</summary>
        private static void MissingSection(StringBuilder sb, List<Item> items)
        {
            if (items.Count == 0)
                return;
            sb.AppendLine();
            sb.AppendLine($"## Numbers not in this mod or the game ({items.Count})");
            sb.AppendLine();
            sb.AppendLine("Fine if they come from another mod this one needs, loaded with it; if not, these lines fail in game.");
            sb.AppendLine();
            foreach (var g in items.GroupBy(i => i.Why.Split(' ')[0]).OrderByDescending(g => g.Count()))
            {
                var numbers = g.Select(i => i.Why.Split(' ').Last()).Distinct().ToList();
                sb.AppendLine($"- {g.Key} {string.Join(", ", numbers.Take(30))}{(numbers.Count > 30 ? $" and {numbers.Count - 30} more" : "")} ({g.Count()} lines)");
                foreach (var i in g.OrderBy(i => i.Line ?? 0).Take(5))
                    sb.AppendLine(i.Line is int n ? $"  - line {n}: `{Trim(i.Text)}`" : $"  - {Trim(i.Text)}");
                if (g.Count() > 5)
                    sb.AppendLine($"  - and {g.Count() - 5} more");
            }
        }

        private static string Trim(string s) => (s.Length > 110 ? s.Substring(0, 110) + "…" : s).Replace("`", "'");

        /// <summary>Why the game skips a command: misspelt (with the likeliest meant one), another type's command, or not in Dominions 6.</summary>
        private static string IgnoredWhy(string command, EntityType type, string typeName)
        {
            var name = command.TrimStart('#').ToLowerInvariant();
            var kind = typeName.Length > 0 ? typeName.ToLowerInvariant() : "this";
            var read = GameCommandCatalog.CommandsOf(type);
            var others = GameCommandCatalog.ContextsReading(name).ToList();
            if (others.Count > 0)
            {
                var where = string.Join("/", others.Take(3));
                return $"`{command}` isn't read in a {kind} block (it's {("aeiou".Contains(where[0]) ? "an" : "a")} {where} command)";
            }
            var meant = Suggest(name, read);
            return meant.Length > 0 ? $"`{command}` isn't a Dominions 6 command" + meant
                : $"`{command}` isn't a Dominions 6 command for a {kind} (maybe one from an older version)";
        }

        /// <summary>"; did you mean #x?" for the closest command the game reads here (one or two typos away), or "".</summary>
        private static string Suggest(string name, IReadOnlyCollection<string> candidates)
        {
            if (name.Length < 4 || candidates.Count == 0)
                return "";
            int best = int.MaxValue;
            string? meant = null;
            foreach (var c in candidates)
            {
                if (Math.Abs(c.Length - name.Length) > 2)
                    continue;
                int d = Distance(name, c);
                if (d < best)
                {
                    best = d;
                    meant = c;
                }
            }
            int allowed = name.Length <= 6 ? 1 : 2;
            return meant != null && best <= allowed ? $"; did you mean `#{meant}`?" : "";
        }

        /// <summary>Edit distance with transpositions (magciboost -> magicboost is 1).</summary>
        private static int Distance(string a, string b)
        {
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + 1);
                }
            return d[a.Length, b.Length];
        }

        /// <summary>A validation issue said for an author.</summary>
        private static string Explain(ValidationIssue v)
        {
            var m = Regex.Match(v.Message ?? "", @"Unresolved reference to (\w+) ID (-?\d+)");
            if (m.Success)
            {
                var kind = m.Groups[1].Value.ToLowerInvariant();
                return $"{kind} {m.Groups[2].Value}";
            }
            return v.Message ?? "";
        }
    }
}
