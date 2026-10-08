using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit.Merge;
using Dom5Editor.Session;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// "Merge mods...": several mods into one file (Dom5Edit.Merge.ModMerger, docs/MERGING.md),
    /// in the order the game would read them; a submod over its parent. The mods themselves are
    /// never changed; the merged file goes where the user picks (never over one of the mods, and
    /// an existing file is backed up first). The merge runs off the window's thread; the merged
    /// file is read back and checked before it's reported done.
    /// </summary>
    public sealed class MergeViewModel : INotifyPropertyChanged
    {
        private string _modName = "Merged mods";
        private string? _outputFile;
        private bool _isMerging;
        private string _status = "";
        private string? _summary;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>The mods, in the order they're merged (a later one's numbers move when they collide).</summary>
        public ObservableCollection<MergeRow> Rows { get; } = new();

        public string ModName
        {
            get => _modName;
            set { _modName = value ?? ""; Changed(); Changed(nameof(Problem)); Changed(nameof(CanMerge)); }
        }

        /// <summary>The merged mod's file.</summary>
        public string? OutputFile
        {
            get => _outputFile;
            set { _outputFile = value; Changed(); Changed(nameof(OutputText)); Changed(nameof(Problem)); Changed(nameof(CanMerge)); }
        }

        public string OutputText => _outputFile ?? "(choose where to save the merged mod)";

        public bool IsMerging
        {
            get => _isMerging;
            private set { _isMerging = value; Changed(); Changed(nameof(CanMerge)); Changed(nameof(CanEdit)); }
        }

        public bool CanEdit => !_isMerging;

        public string Status
        {
            get => _status;
            private set { _status = value; Changed(); }
        }

        /// <summary>What the merge did (null until it's done): numbers moved, copies kept apart, the check.</summary>
        public string? Summary
        {
            get => _summary;
            private set { _summary = value; Changed(); Changed(nameof(IsDone)); }
        }

        public bool IsDone => _summary != null && MergedFile != null;

        /// <summary>The merged file, once made.</summary>
        public string? MergedFile { get; private set; }

        /// <summary>The merge's report (what moved, what to look at), next to the merged file.</summary>
        public string? ReportFile { get; private set; }

        /// <summary>Why the merge can't run yet (null: it can).</summary>
        public string? Problem
        {
            get
            {
                if (Rows.Count < 2)
                    return "Add two mods or more";
                if (string.IsNullOrWhiteSpace(_modName))
                    return "Give the merged mod a name";
                if (_outputFile == null)
                    return "Choose where to save the merged mod";
                var output = Path.GetFullPath(_outputFile);
                // never over one of the mods (nor the ones they're read over)
                if (Rows.Any(r => Same(r.File, output) || r.Needs != null && Same(r.Needs, output)))
                    return "The merged mod can't be saved over one of the mods: choose another file";
                if (InWorkshop(output))
                    return "Steam replaces the files in its workshop folder when a mod updates: save the merged mod elsewhere (your Dominions 6 mods folder)";
                // a submod after its parent (the game reads it so)
                for (int i = 0; i < Rows.Count; i++)
                    if (Rows[i].Needs is string needs && Rows.Skip(i + 1).Any(r => Same(r.File, needs)))
                        return $"{Rows[i].FileName} is read over {Path.GetFileName(needs)}: move that one above it";
                return null;
            }
        }

        public bool CanMerge => Problem == null && !_isMerging;

        private static bool Same(string a, string b) =>
            string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        public static bool InWorkshop(string path) =>
            Path.GetFullPath(path).Replace('\\', '/').Contains("/steamapps/workshop/content/", StringComparison.OrdinalIgnoreCase);

        /// <summary>Adds a mod at the end, with the mod it's read over as remembered for it (Mod Info's "Mods this one needs").</summary>
        public void Add(string file)
        {
            var full = Path.GetFullPath(file);
            if (Rows.Any(r => Same(r.File, full)))
            {
                Status = $"{Path.GetFileName(full)} is in the list already";
                return;
            }
            var needed = NeededMods.For(full);
            Rows.Add(new MergeRow(this, full, needed.Count > 0 ? Path.GetFullPath(needed[^1]) : null));
            Renumber();
        }

        public void Remove(MergeRow row)
        {
            Rows.Remove(row);
            Renumber();
        }

        public void Move(MergeRow row, int by)
        {
            int i = Rows.IndexOf(row), j = i + by;
            if (i < 0 || j < 0 || j >= Rows.Count)
                return;
            Rows.Move(i, j);
            Renumber();
        }

        /// <summary>The mod a row is read over (its parent), or none.</summary>
        public void SetNeeds(MergeRow row, string? file)
        {
            row.Needs = file == null ? null : Path.GetFullPath(file);
            Renumber();
        }

        internal void Renumber()
        {
            for (int i = 0; i < Rows.Count; i++)
                Rows[i].Order = i + 1;
            foreach (var r in Rows)
                r.Refresh();
            Changed(nameof(Problem));
            Changed(nameof(CanMerge));
            Summary = null;
            MergedFile = null;
        }

        /// <summary>
        /// Merges (off the window's thread), then reads the merged file back: no number defined
        /// twice that the mods didn't define twice. An existing file of that name is backed up
        /// first (ModBackups). The summary says what moved and where the report is.
        /// </summary>
        public async Task MergeAsync()
        {
            if (!CanMerge || _outputFile == null)
                return;
            var output = Path.GetFullPath(_outputFile);
            var inputs = Rows.Select(r => new MergeInput(r.File, r.Needs)).ToList();
            var name = _modName.Trim();
            IsMerging = true;
            Summary = null;
            MergedFile = null;
            Status = $"Merging {Rows.Count} mods (reading them, moving what collides, writing the file)...";
            try
            {
                var (result, twice, before) = await Task.Run(() =>
                {
                    if (File.Exists(output))
                        Dom5Edit.ModBackups.Backup(output, "before a merge replaced it");
                    var r = ModMerger.Merge(inputs, output, name);
                    // read back: numbers defined twice in the merged file, against those the mods had
                    int Twice(Dom5Edit.Mod m) => new Dom5Edit.Validation.DuplicateIdValidator().Validate(m).Count();
                    var merged = Dom5Edit.Mod.Import(output);
                    return (r, Twice(merged), r.Parts.Sum(Twice));
                });
                MergedFile = result.OutputFile;
                ReportFile = Path.ChangeExtension(result.OutputFile, ".report.md");
                var report = result.Report;
                static string N(int n, string one, string many) => n == 1 ? $"1 {one}" : $"{n} {many}";
                var kinds = string.Join(", ", report.Moves.GroupBy(m => m.Kind).OrderByDescending(g => g.Count()).Select(g => $"{g.Count()} {g.Key}"));
                var lines = new List<string>
                {
                    $"Merged {Rows.Count} mods into {Path.GetFileName(result.OutputFile)}.",
                    report.Moves.Count == 0 ? "No numbers collided: nothing moved." : $"{N(report.Moves.Count, "number", "numbers")} moved ({kinds}); every reference follows.",
                };
                if (report.Kept.Count > 0)
                    lines.Add($"{N(report.Kept.Count, "game entity", "game entities")} copied by one mod and changed by an earlier one: the copies stay as the game has them.");
                if (report.Renames.Count > 0)
                    lines.Add($"{N(report.Renames.Count, "entity", "entities")} renamed (a name-only reference would have found another mod's).");
                if (report.Conflicts.Count > 0)
                    lines.Add($"{N(report.Conflicts.Count, "thing acts", "things act")} across mods (both change the same game entity, a clearing command, a number one doesn't define): the report lists them.");
                if (report.Separate.Count > 0)
                    lines.Add($"Enable first, in this order (needed, not merged): {string.Join(", ", report.Separate)}.");
                lines.Add(twice <= before
                    ? "Read back: no number is defined twice that wasn't in the mods."
                    : $"Read back: {N(twice - before, "number is", "numbers are")} defined twice that weren't in the mods: please look at the report before using it.");
                Summary = string.Join("\n", lines);
                Status = "Done";
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException || ex is UnauthorizedAccessException)
            {
                Status = $"The merge didn't go through: {ex.Message}";
            }
            finally
            {
                IsMerging = false;
            }
        }

        private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>One mod to merge: its place in the order, its name and file, the mod it's read over.</summary>
    public sealed class MergeRow : INotifyPropertyChanged
    {
        private readonly MergeViewModel _merge;

        internal MergeRow(MergeViewModel merge, string file, string? needs)
        {
            _merge = merge;
            File = file;
            Needs = needs;
            Name = ModNameOf(file) ?? Path.GetFileNameWithoutExtension(file);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string File { get; }
        public string FileName => Path.GetFileName(File);
        public string Name { get; }
        public int Order { get; internal set; }
        public string? Needs { get; internal set; }

        /// <summary>"read over Sombre_Warhammer_dom6.dm" (merged if it's in the list, else enabled before the merged mod).</summary>
        public string NeedsText => Needs == null ? "" :
            _merge.Rows.Any(r => string.Equals(r.File, Needs, StringComparison.OrdinalIgnoreCase))
                ? $"read over {Path.GetFileName(Needs)} (merged too)"
                : $"read over {Path.GetFileName(Needs)} (not in the list: enable it before the merged mod)";

        public bool HasNeeds => Needs != null;
        public bool CanMoveUp => Order > 1;
        public bool CanMoveDown => Order < _merge.Rows.Count;

        internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

        /// <summary>The mod's #modname, from the top of its file (without reading all of it).</summary>
        private static string? ModNameOf(string file)
        {
            try
            {
                foreach (var line in System.IO.File.ReadLines(file).Take(400))
                {
                    var t = line.TrimStart();
                    if (t.StartsWith("#modname", StringComparison.Ordinal))
                    {
                        int a = t.IndexOf('"'), b = a >= 0 ? t.IndexOf('"', a + 1) : -1;
                        return b > a ? t.Substring(a + 1, b - a - 1).Trim() : null;
                    }
                }
            }
            catch (IOException)
            {
            }
            return null;
        }
    }
}
