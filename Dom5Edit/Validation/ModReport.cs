using System.Text;
using System.Text.RegularExpressions;
using Dom5Edit.Entities;
using Dom5Edit.GameData;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// A report for a mod's author, or for anyone who opens the mod: what the editor found, worst
    /// first, each with its line in the file and what the game does with it. Four parts: what goes
    /// wrong in game (events that can't happen or lose lines, lines the game reads differently than
    /// they look, numbers taken from the game), lines the game ignores (with "did you mean" for a
    /// misspelt command, or the type whose blocks read it), numbers that neither this mod nor the
    /// game has (fine when they come from a mod this one needs), and things worth a look. What it
    /// says about the game comes from Dominions6.exe (tools/dom6exe) and the modding manuals.
    /// <see cref="Build"/> gives it as data (the editor's report window), <see cref="Write"/> as
    /// Markdown (to send to the author).
    /// </summary>
    public static class ModReport
    {
        /// <summary>One line of the file the report points at: its number, its text, and the entity whose block holds it.</summary>
        public sealed record Line(int? Number, string Text, IDEntity? Entity);

        /// <summary>Alike findings (the same message), with their lines.</summary>
        public sealed record Group(string Text, IReadOnlyList<Line> Lines);

        public sealed record Section(string Key, string Title, string Intro, int Count, IReadOnlyList<Group> Groups);

        public sealed record Report(string Name, string About, IReadOnlyList<Section> Sections)
        {
            public int Count(string key) => Sections.FirstOrDefault(s => s.Key == key)?.Count ?? 0;
            public int Total => Sections.Sum(s => s.Count);
        }

        public const string Wrong = "wrong", Ignored = "ignored", Missing = "missing", Look = "look";

        public static string Write(Mod mod, ValidationResult validation, IEnumerable<Events.EventProblem>? eventProblems = null) =>
            Markdown(Build(mod, validation, eventProblems));

        public static Report Build(Mod mod, ValidationResult validation, IEnumerable<Events.EventProblem>? eventProblems = null)
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
            Line At(int? line, IDEntity? entity = null) => new Line(line is int n && n > 0 ? n : null,
                line is int k && k >= 1 && k <= lines.Length ? lines[k - 1].Trim() : "",
                entity ?? (line is int m && m > 0 ? mod.EntityAt(m) : null));

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
                    var replaced = string.Join(", ", copy.Groups[1].Value.Split(", ").Distinct());
                    look.Add(new Item(At(p.LineNumber), $"#copystats replaces what the lines before it in the block set ({replaced}): lines meant to change the copy go after it"));
                    continue;
                }
                if (p.IssueType != ParseIssueType.InvalidCommand && p.IssueType != ParseIssueType.NotReadByGame)
                    continue;
                var command = Regex.Match(p.Message ?? "", @"#[A-Za-z_][A-Za-z0-9_]*").Value;
                var typeName = Regex.Match(p.Message ?? "", @"for:? ([A-Z][a-z]+)").Groups[1].Value;
                Enum.TryParse<EntityType>(typeName, true, out var type);
                ignored.Add(new Item(At(p.LineNumber), IgnoredWhy(command, type, typeName)));
            }

            // references, numbers, names, lines read differently, from Validate
            foreach (var v in validation.Issues)
            {
                if (v.Category == "Invalid Command" || v.Severity == ValidationSeverity.Info)
                    continue; // (the parser's notes above say it better)
                var item = new Item(At(v.LineNumber ?? v.Property?.LineNumber, v.Entity as IDEntity), Explain(v));
                (v.Category == "Reference" && (v.Message ?? "").StartsWith("Unresolved reference") ? missing
                    : v.Severity == ValidationSeverity.Error || v.Category == "Reference" ? wrong : look).Add(item);
            }

            // events
            foreach (var e in eventProblems ?? Array.Empty<Events.EventProblem>())
            {
                var line = e.Line?.LineNumber is int n && n > 0 ? n
                    : e.Event.Properties.Select(p => p.LineNumber).Where(n2 => n2 > 0).DefaultIfEmpty(0).Min();
                (e.IsError ? wrong : look).Add(new Item(At(line, e.Event), e.Message));
            }

            var with = mod.Dependencies.Where(d => d != VanillaLoader.Vanilla && !string.IsNullOrEmpty(d.FullFilePath))
                .Select(d => $"`{Path.GetFileName(d.FullFilePath)}`").ToList();
            var about = $"File: `{Path.GetFileName(file)}`" + (string.IsNullOrEmpty(mod.Version) ? "" : $", version {mod.Version}") +
                        (with.Count > 0 ? $", together with {string.Join(", ", with)}" : "") +
                        $". Checked {DateTime.Now:yyyy-MM-dd} against Dominions {GameCommandCatalog.GameVersion ?? "6"} (what the game reads is taken from the game itself).";
            return new Report(mod.ModName ?? Path.GetFileNameWithoutExtension(file) ?? "mod", about, new[]
            {
                new Section(Wrong, "Goes wrong in game", "These change what happens in game, or keep something from happening.", wrong.Count, Grouped(wrong)),
                new Section(Ignored, "Lines the game ignores", "The game skips these lines: they change nothing.", ignored.Count, Grouped(ignored)),
                new Section(Missing, "Numbers not in this mod or the game", "Fine if they come from another mod this one needs, loaded with it; if not, these lines fail in game.", missing.Count, ByKind(missing)),
                new Section(Look, "Worth a look", "Not wrong as such, but probably not what was meant.", look.Count, Grouped(look)),
            });
        }

        private sealed record Item(Line Line, string Why);

        /// <summary>Alike ones together (the same message but for its numbers), most first, each group's lines in file order.</summary>
        private static List<Group> Grouped(List<Item> items) =>
            items.GroupBy(i => Regex.Replace(i.Why, @"-?\d+", "N")).OrderByDescending(g => g.Count())
                .Select(g => new Group(g.First().Why + (g.Count() > 1 ? $" ({g.Count()} lines)" : ""), g.Select(i => i.Line).OrderBy(l => l.Number ?? 0).ToList()))
                .ToList();

        /// <summary>References to numbers nothing has, by kind ("monster 15992"): the numbers, then the lines.</summary>
        private static List<Group> ByKind(List<Item> items) =>
            items.GroupBy(i => i.Why.Split(' ')[0]).OrderByDescending(g => g.Count())
                .Select(g =>
                {
                    var numbers = g.Select(i => i.Why.Split(' ').Last()).Distinct().ToList();
                    return new Group($"{g.Key} {string.Join(", ", numbers.Take(30))}{(numbers.Count > 30 ? $" and {numbers.Count - 30} more" : "")} ({g.Count()} lines)",
                        g.Select(i => i.Line).OrderBy(l => l.Number ?? 0).ToList());
                })
                .ToList();

        /// <summary>The report as Markdown, for the author.</summary>
        public static string Markdown(Report report)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {report.Name}: what the mod editor found");
            sb.AppendLine();
            sb.AppendLine(report.About);
            sb.AppendLine();
            foreach (var s in report.Sections)
                if (s.Count > 0 || s.Key != Missing)
                    sb.AppendLine($"- **{s.Title}:** {s.Count}");
            foreach (var s in report.Sections.Where(s => s.Count > 0))
            {
                int shown = s.Key == Missing ? 5 : 12;
                sb.AppendLine();
                sb.AppendLine($"## {s.Title} ({s.Count})");
                sb.AppendLine();
                sb.AppendLine(s.Intro);
                sb.AppendLine();
                foreach (var g in s.Groups)
                {
                    sb.AppendLine($"- {g.Text}");
                    foreach (var l in g.Lines.Take(shown))
                        sb.AppendLine(l.Number is int n ? $"  - line {n}: `{Trim(l.Text)}`" : $"  - {Trim(l.Text)}");
                    if (g.Lines.Count > shown)
                        sb.AppendLine($"  - and {g.Lines.Count - shown} more");
                }
            }
            if (report.Total == 0)
                sb.AppendLine("\nNothing found.");
            return sb.ToString();
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
