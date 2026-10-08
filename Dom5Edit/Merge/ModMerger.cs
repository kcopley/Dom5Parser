using System.Text;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Merge
{
    /// <summary>One mod to merge, and the mod it's read over if it's a submod (another input, or a mod that stays separate).</summary>
    public sealed class MergeInput
    {
        public MergeInput(string file, string? needs = null)
        {
            File = Path.GetFullPath(file);
            Needs = needs == null ? null : Path.GetFullPath(needs);
        }

        public string File { get; }
        public string? Needs { get; }
    }

    /// <summary>A number a merge moved: in which part, what (monster, event code, ...), from, to.</summary>
    public sealed record MergeMove(string Part, string Kind, int Old, int New, string Name);

    /// <summary>What a merge did and what to look at (docs/MERGING.md, "A report").</summary>
    public sealed class MergeReport
    {
        public List<MergeMove> Moves { get; } = new();
        public List<string> Renames { get; } = new();
        public List<string> Conflicts { get; } = new();
        public List<string> Notes { get; } = new();
        public List<string> Separate { get; } = new();
        public List<string> Parts { get; } = new();

        public string Markdown(string modName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {modName}: merge report").AppendLine();
            sb.AppendLine("Parts, in the order they're merged (a later part's numbers move when they collide with an earlier one's):");
            foreach (var p in Parts)
                sb.AppendLine($"- {p}");
            if (Separate.Count > 0)
            {
                sb.AppendLine().AppendLine("Needed but not merged: enable these before the merged mod, in this order:");
                foreach (var s in Separate)
                    sb.AppendLine($"- {s}");
            }
            sb.AppendLine().AppendLine($"## Numbers moved ({Moves.Count})").AppendLine();
            foreach (var g in Moves.GroupBy(m => (m.Part, m.Kind)))
            {
                sb.AppendLine($"### {g.Key.Part}: {g.Key.Kind} ({g.Count()})").AppendLine();
                foreach (var m in g)
                    sb.AppendLine($"- {m.Old} -> {m.New}{(string.IsNullOrEmpty(m.Name) ? "" : "  " + m.Name)}");
                sb.AppendLine();
            }
            void Section(string title, List<string> lines)
            {
                sb.AppendLine($"## {title} ({lines.Count})").AppendLine();
                foreach (var l in lines)
                    sb.AppendLine($"- {l}");
                sb.AppendLine();
            }
            Section("Renamed (a name-only reference would have found another part's)", Renames);
            Section("Across parts (works as in game: the later part wins)", Conflicts);
            Section("Notes", Notes);
            return sb.ToString();
        }
    }

    public sealed class MergeResult
    {
        public required string OutputFile { get; init; }
        public required MergeReport Report { get; init; }
        /// <summary>The parts as read, with their moved numbers (for checks).</summary>
        public required IReadOnlyList<Mod> Parts { get; init; }
        /// <summary>The needed mods that stay separate, in reading order (the merged mod is read over the last).</summary>
        public required IReadOnlyList<Mod> Separate { get; init; }
    }

    /// <summary>
    /// Merges mods into one (docs/MERGING.md): each part read over what it needs, a later part's
    /// colliding numbers (entities, codes, variables, enchantments, monster tags, item groups,
    /// spell-event ids) moved to free ones, references written by number wherever the game takes
    /// one, a name-only reference's target renamed only when its name would find another part's,
    /// the files parts name copied next to the merged file, and the parts written in order, each
    /// with its own text.
    /// </summary>
    public static class ModMerger
    {
        // the types whose entities a mod numbers itself
        private static readonly EntityType[] Numbered =
        {
            EntityType.MONSTER, EntityType.WEAPON, EntityType.ARMOR, EntityType.SPELL, EntityType.ITEM,
            EntityType.SITE, EntityType.NATION, EntityType.POPTYPE, EntityType.NAMETYPE,
        };

        // the numbers events, spells and items share between mods
        private static readonly EntityType[] Shared =
        {
            EntityType.EVENT_CODE, EntityType.EVENT_VAR, EntityType.ENCHANTMENT, EntityType.MONTAG,
            EntityType.RESTRICTED_ITEM, EntityType.EVENT_CODE_EFFECT,
        };

        private static readonly Command[] PathCommands =
        {
            Command.SPR1, Command.SPR2, Command.XSPR1, Command.XSPR2, Command.SPR, Command.FLAG, Command.INDEPFLAG,
            Command.MOUNTEDSPR1, Command.MOUNTEDSPR2, Command.UNMOUNTEDSPR1, Command.UNMOUNTEDSPR2, Command.SAMPLE,
        };

        public static MergeResult Merge(IReadOnlyList<MergeInput> inputs, string outputFile, string modName)
        {
            if (inputs.Count < 2)
                throw new ArgumentException("a merge needs two mods or more");
            var report = new MergeReport();
            var byFile = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
            var parts = new List<Mod>();
            var separate = new List<Mod>();
            // 1. each part over what it needs: an earlier part, or a mod that stays separate
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                Mod? below = null;
                if (input.Needs != null)
                {
                    int at = inputs.ToList().FindIndex(x => string.Equals(x.File, input.Needs, StringComparison.OrdinalIgnoreCase));
                    if (at > i)
                        throw new ArgumentException($"{Path.GetFileName(input.File)} needs {Path.GetFileName(input.Needs)}: list that one first (the game reads a submod after its parent)");
                    if (!byFile.TryGetValue(input.Needs, out below))
                    {
                        below = Mod.Import(input.Needs);
                        byFile[input.Needs] = below;
                        separate.Add(below);
                        report.Separate.Add($"{below.DisplayName} ({Path.GetFileName(input.Needs)})");
                    }
                }
                var mod = Mod.Import(input.File, below);
                byFile[input.File] = mod;
                parts.Add(mod);
                report.Parts.Add($"{mod.DisplayName} ({Path.GetFileName(input.File)}{(string.IsNullOrEmpty(mod.Version) ? "" : ", version " + mod.Version)})"
                                 + (below != null ? $", read over {below.DisplayName}" : ""));
            }
            // 2. merge mode: references by number where the game takes one (StringOrIDRef, the headers)
            foreach (var m in parts)
                m.KeepReferenceForms = false;
            // 3. numbers
            foreach (var type in Numbered)
                MoveEntities(type, parts, separate, report);
            foreach (var type in Shared)
                MoveShared(type, parts, separate, report);
            Overlaps(parts, report);
            // 4. name-only references whose names would find another part's
            RenameClashes(parts, separate, report);
            // 5. the files parts name, next to the merged file
            var outDir = Path.GetDirectoryName(Path.GetFullPath(outputFile))!;
            Directory.CreateDirectory(outDir);
            CopyFiles(parts, outDir, report);
            // 6. one file: the merged header, then each part in its own order
            Write(parts, outputFile, modName, report);
            File.WriteAllText(Path.ChangeExtension(outputFile, ".report.md"), report.Markdown(modName));
            return new MergeResult { OutputFile = outputFile, Report = report, Parts = parts, Separate = separate };
        }

        private static bool DependsOn(Mod x, Mod y) => x.Below().Contains(y);

        /// <summary>
        /// The entities of a type the mod numbers itself: a #new N, or a #select N of a number
        /// nothing under it has (a new nation, poptype, nametype). Never a game entity's number: a
        /// #new on it replaces the game's entity, on purpose.
        /// </summary>
        private static List<IDEntity> Owned(Mod mod, EntityType type)
        {
            var set = mod.Database[type];
            var vanilla = VanillaLoader.Vanilla?.Database[type];
            bool vanillaHasType = vanilla != null && vanilla.GetFullList().Count > 0;
            return set.GetFullList()
                .Where(e => e.ID > 0 && set.TryGetValue(e.ID, out var held) && ReferenceEquals(held, e) && vanilla?.Has(e.ID) != true
                            && (!e.Selected || vanillaHasType && mod.FindBelow(type, e.ID, null) == null))
                .OrderBy(e => e.ID).ToList();
        }

        private static IEnumerable<int> Numbers(Mod mod, EntityType type) =>
            mod.Database[type].GetFullList().Select(e => e.ID).Where(id => id > 0);

        private static void MoveEntities(EntityType type, List<Mod> parts, List<Mod> separate, MergeReport report)
        {
            var range = parts[0].Database[type];
            // every number anyone uses is taken: a moved entity mustn't land on another
            var taken = new HashSet<int>(parts.Concat(separate).SelectMany(m => Numbers(m, type)));
            if (VanillaLoader.Vanilla != null)
                taken.UnionWith(Numbers(VanillaLoader.Vanilla, type));
            var claimed = new List<(Mod Owner, int Id)>();
            foreach (var s in separate)
                claimed.AddRange(Owned(s, type).Select(e => (s, e.ID)));
            // (poptypes have no range in the tables: mods make theirs from 150 to 249, as the editor does)
            int next = type == EntityType.POPTYPE ? 150 : range.START_ID, end = type == EntityType.POPTYPE ? 249 : range.END_ID;
            foreach (var part in parts)
            {
                var mine = Owned(part, type);
                foreach (var e in mine)
                {
                    int old = e.ID;
                    if (!claimed.Any(c => c.Id == old && !DependsOn(part, c.Owner)))
                        continue;
                    while (taken.Contains(next))
                        next++;
                    if (end > 0 && next > end)
                        throw new InvalidOperationException($"no free {type.ToString().ToLowerInvariant()} number left up to {end}");
                    int now = next;
                    taken.Add(now);
                    var over = parts.Where(p => ReferenceEquals(p, part) || DependsOn(p, part)).ToList();
                    Renumbering.Move(e, now, over);
                    report.Moves.Add(new MergeMove(part.DisplayName, type.ToString().ToLowerInvariant(), old, now, e.Name ?? ""));
                    // a submod's #new on this number replaced this entity on purpose: it moves with it
                    foreach (var sub in over.Where(p => !ReferenceEquals(p, part)))
                        if (sub.Database[type].TryGetValue(old, out var again) && ReferenceEquals(again.ParentMod, sub) && !again.Selected)
                        {
                            Renumbering.Move(again, now, parts.Where(p => ReferenceEquals(p, sub) || DependsOn(p, sub)));
                            report.Notes.Add($"{sub.DisplayName}'s #new{type.ToString().ToLowerInvariant()} {old} replaces {part.DisplayName}'s: moved with it to {now}");
                        }
                }
                claimed.AddRange(mine.Select(e => (part, e.ID)));
            }
        }

        /// <summary>A shared number the mod brings in itself (not a game one, not its parent's): in the mods' range.</summary>
        private static List<DependentEntity> OwnedShared(Mod mod, EntityType type)
        {
            var set = mod.Dependents[type];
            return set.Values.Where(d => d.Dependent == null && !d.IsVanilla
                                         && (set.ID_DOWN ? d.ID <= set.START_ID : d.ID >= set.START_ID))
                .OrderBy(d => Math.Abs(d.ID)).ToList();
        }

        private static void MoveShared(EntityType type, List<Mod> parts, List<Mod> separate, MergeReport report)
        {
            var range = parts[0].Dependents[type];
            var taken = new HashSet<int>(parts.Concat(separate).SelectMany(m => m.Dependents[type].Keys));
            if (VanillaLoader.Vanilla != null)
                taken.UnionWith(VanillaLoader.Vanilla.Dependents[type].Keys);
            var claimed = new List<(Mod Owner, int Id)>();
            foreach (var s in separate)
                claimed.AddRange(OwnedShared(s, type).Select(d => (s, d.ID)));
            int next = range.START_ID;
            foreach (var part in parts)
            {
                var mine = OwnedShared(part, type);
                foreach (var d in mine)
                {
                    int old = d.ID;
                    if (!claimed.Any(c => c.Id == old && !DependsOn(part, c.Owner)))
                        continue;
                    while (taken.Contains(next))
                        next += range.ID_DOWN ? -1 : 1;
                    int now = next;
                    taken.Add(now);
                    Renumbering.Move(part, type, d, now);
                    report.Moves.Add(new MergeMove(part.DisplayName, Kind(type), old, now, ""));
                }
                claimed.AddRange(mine.Select(d => (part, d.ID)));
            }
        }

        private static string Kind(EntityType type) => type switch
        {
            EntityType.EVENT_CODE => "event code",
            EntityType.EVENT_VAR => "event variable",
            EntityType.ENCHANTMENT => "enchantment",
            EntityType.MONTAG => "monster tag",
            EntityType.RESTRICTED_ITEM => "item group (#restricteditem)",
            EntityType.EVENT_CODE_EFFECT => "spell event id (#id)",
            _ => type.ToString().ToLowerInvariant(),
        };

        /// <summary>
        /// What acts across parts and can't be untangled (the game does the same with the mods
        /// enabled one after another): game entities several parts change, game numbers several
        /// parts replace, clearing commands in a later part.
        /// </summary>
        private static void Overlaps(List<Mod> parts, MergeReport report)
        {
            foreach (var type in Numbered.Append(EntityType.EVENT))
            {
                var byId = new Dictionary<int, List<Mod>>();
                foreach (var part in parts)
                    foreach (var e in part.Database[type].GetFullList())
                        if (e.ID > 0 && VanillaLoader.Vanilla?.Database[type].Has(e.ID) == true)
                        {
                            if (!byId.TryGetValue(e.ID, out var list))
                                byId[e.ID] = list = new List<Mod>();
                            if (!list.Contains(part))
                                list.Add(part);
                        }
                foreach (var (id, list) in byId.Where(kv => kv.Value.Count > 1).OrderBy(kv => kv.Key))
                {
                    var name = VanillaLoader.Vanilla!.Database[type].TryGetValue(id, out var v) ? v.Name : "";
                    report.Conflicts.Add($"game {type.ToString().ToLowerInvariant()} {id} {name}: changed by {string.Join(", then ", list.Select(p => p.DisplayName))}");
                }
            }
            var clearing = new[] { Command.CLEARALLEVENTS, Command.CLEARMERCS };
            foreach (var part in parts.Skip(1))
                foreach (var e in Renumbering.Entities(part))
                    foreach (var p in e.Properties.Where(p => clearing.Contains(p.Command)))
                        report.Conflicts.Add($"{part.DisplayName} has {(CommandsMap.TryGetString(p.Command, out var c) ? c : p.Command.ToString())} (line {p.LineNumber}): it clears what the parts before it added");
        }

        /// <summary>
        /// The references the game finds only by name (#startsite, an item's #spell and
        /// #autospell: StringOrIDRef writes them as names): if the name would find another
        /// entity in the merged file (the game takes the lowest number), the target is renamed
        /// with its part's name. The last resort (the user): everything else goes by number.
        /// </summary>
        private static void RenameClashes(List<Mod> parts, List<Mod> separate, MergeReport report)
        {
            var nameOnly = new[] { Command.STARTSITE, Command.SPELL, Command.AUTOSPELL };
            var everyone = new List<Mod>();
            if (VanillaLoader.Vanilla != null)
                everyone.Add(VanillaLoader.Vanilla);
            everyone.AddRange(separate);
            everyone.AddRange(parts);
            foreach (var part in parts)
                foreach (var e in Renumbering.Entities(part).ToList())
                    foreach (var r in e.Properties.OfType<StringOrIDRef>().Where(r => nameOnly.Contains(r.Command)))
                    {
                        if (!r.TryGetEntity(out var target) || target == null || !target.TryGetName(out var name) || string.IsNullOrEmpty(name))
                            continue;
                        var type = target.GetEntityType();
                        var first = everyone.SelectMany(m => m.Database[type].GetFullList())
                            .Where(x => x.TryGetName(out var n) && string.Equals(n, name, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(x => x.ID > 0 ? x.ID : int.MaxValue).FirstOrDefault();
                        if (first == null || ReferenceEquals(first, target) || first.ID == target.ID)
                            continue;
                        var owner = parts.FirstOrDefault(p => ReferenceEquals(p, target.ParentMod));
                        var nameLine = target.Properties.OfType<NameProperty>().LastOrDefault();
                        if (owner == null || nameLine == null)
                        {
                            report.Notes.Add($"{part.DisplayName}: {Line(r)} means {Describe(target)}, but the name finds {Describe(first)} in the merged file, and that name can't be changed here: fix it by hand");
                            continue;
                        }
                        var renamed = $"{name} ({Short(owner)})";
                        nameLine.Value = renamed;
                        report.Renames.Add($"{owner.DisplayName}'s {type.ToString().ToLowerInvariant()} \"{name}\" #{target.ID} -> \"{renamed}\": {Line(r)} in {part.DisplayName} would have found {Describe(first)}");
                    }
        }

        private static string Line(Property p) => (CommandsMap.TryGetString(p.Command, out var c) ? c : p.Command.ToString()) + (p.LineNumber > 0 ? $" (line {p.LineNumber})" : "");

        private static string Describe(IDEntity e) => $"{(e.TryGetName(out var n) ? n : "")} #{e.ID} ({e.ParentMod?.DisplayName ?? "the game"})";

        private static string Short(Mod m)
        {
            var name = m.DisplayName;
            return name.Length <= 24 ? name : name.Substring(0, 24).TrimEnd();
        }

        /// <summary>
        /// The images and sounds a part names (paths relative to its .dm): copied into a folder
        /// for the part next to the merged file, the paths written to match.
        /// </summary>
        private static void CopyFiles(List<Mod> parts, string outDir, MergeReport report)
        {
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in parts)
            {
                var folder = string.Concat(Path.GetFileNameWithoutExtension(part.FullFilePath).Select(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_'));
                while (!folders.Add(folder))
                    folder += "_";
                var partDir = Path.GetDirectoryName(part.FullFilePath)!;
                int copied = 0, missing = 0;
                foreach (var e in Renumbering.Entities(part))
                    foreach (var p in e.Properties.OfType<FilePathProperty>().Where(p => PathCommands.Contains(p.Command)))
                    {
                        // the path the game reads: up to the closing quote (Confluence has text after it)
                        var named = (p.Value ?? "").Split('"')[0].Trim();
                        if (named.Length == 0)
                            continue;
                        var source = Imaging.ModFiles.Resolve(named, part.FullFilePath);
                        if (source == null || !File.Exists(source))
                        {
                            missing++;
                            continue;
                        }
                        var relative = Path.GetRelativePath(partDir, source);
                        if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative))
                            relative = Path.Combine("_outside", Path.GetFileName(source));
                        var target = Path.Combine(outDir, folder, relative);
                        if (!File.Exists(target))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                            File.Copy(source, target);
                            copied++;
                        }
                        p.Value = (folder + "/" + relative).Replace('\\', '/');
                    }
                if (copied > 0 || missing > 0)
                    report.Notes.Add($"{part.DisplayName}: {copied} image/sound files copied into {folder}/" + (missing > 0 ? $"; {missing} named files weren't found (their lines are kept as written)" : ""));
            }
        }

        private static void Write(List<Mod> parts, string outputFile, string modName, MergeReport report)
        {
            var newLine = parts[0].SourceNewLine ?? "\n";
            using var writer = new StreamWriter(outputFile, false, new UTF8Encoding(false)) { NewLine = newLine };
            var description = $"Merged from {string.Join(", ", parts.Select(p => p.DisplayName + (string.IsNullOrEmpty(p.Version) ? "" : " " + p.Version)))}, in this order. " +
                              $"Made with Dom6 Mod Editor; what moved is in {Path.GetFileNameWithoutExtension(outputFile)}.report.md.";
            writer.WriteLine(CommandsMap.Format(Command.MODNAME, modName, true));
            writer.WriteLine(CommandsMap.Format(Command.DESCRIPTION, description, true));
            writer.WriteLine(CommandsMap.Format(Command.VERSION, "1.00"));
            var domVersion = parts.Select(p => p.DomVersion).Where(v => !string.IsNullOrEmpty(v))
                .OrderByDescending(v => decimal.TryParse(v, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0).FirstOrDefault();
            if (domVersion != null)
                writer.WriteLine(CommandsMap.Format(Command.DOMVERSION, domVersion));
            foreach (var part in parts)
            {
                writer.WriteLine();
                writer.WriteLine($"-- ======== {part.DisplayName} ({Path.GetFileName(part.FullFilePath)}) ========");
                part.ExportBody(writer);
            }
        }
    }
}
