using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Dom5Editor.UI.Views;

namespace Dom5Editor.Ava.Views
{
    /// <summary>
    /// "Merge mods..." (UI.Views.MergeViewModel): the file pickers, the merge, and the way into
    /// the merged mod. Shown over the main window, which waits: the merge reads the game's data
    /// the open mod uses too.
    /// </summary>
    public partial class MergeWindow : Window
    {
        private readonly MergeViewModel _vm = new();

        public MergeWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Closing += (s, e) =>
            {
                // (the merge runs on: it would write the file with nobody to say so)
                if (_vm.IsMerging)
                    e.Cancel = true;
            };
        }

        public MergeViewModel ViewModel => _vm;

        /// <summary>Set when the user asks to open the merged mod: the main window opens it once this closes.</summary>
        public string? OpenAfter { get; private set; }

        /// <summary>The user's Dominions 6 mods folder, where the game finds mods (if it's there).</summary>
        public static string? ModsFolder()
        {
            var folder = OperatingSystem.IsWindows()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dominions6", "mods")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dominions6", "mods");
            return Directory.Exists(folder) ? folder : null;
        }

        private async Task<IStorageFolder?> Folder(string? path) =>
            path != null && Directory.Exists(path) ? await StorageProvider.TryGetFolderFromPathAsync(path) : null;

        private async void Add_Click(object? sender, RoutedEventArgs e)
        {
            var start = _vm.Rows.Count > 0 ? Path.GetDirectoryName(_vm.Rows[^1].File) : ModsFolder();
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Mods to merge (in the order they're picked: change it in the list)",
                AllowMultiple = true,
                FileTypeFilter = Hooks.Filters("Dominions mods (*.dm)|*.dm|All files|*.*"),
                SuggestedStartLocation = await Folder(start),
            });
            foreach (var f in files)
                if (f.TryGetLocalPath() is string path)
                    _vm.Add(path);
        }

        private async void Output_Click(object? sender, RoutedEventArgs e)
        {
            var name = string.Concat((_vm.ModName.Trim().Length > 0 ? _vm.ModName.Trim() : "Merged mods").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save the merged mod",
                DefaultExtension = "dm",
                SuggestedFileName = name + ".dm",
                FileTypeChoices = Hooks.Filters("Dominions mod files (*.dm)|*.dm"),
                SuggestedStartLocation = await Folder(ModsFolder()),
            });
            if (file?.TryGetLocalPath() is not string path)
                return;
            if (File.Exists(path) && !_vm.Rows.Any(r => string.Equals(Path.GetFullPath(r.File), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
                && !await Dialogs.Ask(this, "Replace the file?", $"{Path.GetFileName(path)} exists. The merge replaces it (a copy of it is kept in the backups first).", "Replace", "Cancel"))
                return;
            _vm.OutputFile = path;
        }

        private async void Needs_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is not MergeRow row)
                return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"The mod {row.FileName} is read over (its parent)",
                AllowMultiple = false,
                FileTypeFilter = Hooks.Filters("Dominions mods (*.dm)|*.dm|All files|*.*"),
                SuggestedStartLocation = await Folder(Path.GetDirectoryName(row.File)),
            });
            if (files.Count > 0 && files[0].TryGetLocalPath() is string path)
                _vm.SetNeeds(row, path);
        }

        private void NoNeeds_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is MergeRow row)
                _vm.SetNeeds(row, null);
        }

        private void Up_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is MergeRow row)
                _vm.Move(row, -1);
        }

        private void Down_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is MergeRow row)
                _vm.Move(row, 1);
        }

        private void Remove_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is MergeRow row)
                _vm.Remove(row);
        }

        private async void Merge_Click(object? sender, RoutedEventArgs e) => await _vm.MergeAsync();

        private void OpenMerged_Click(object? sender, RoutedEventArgs e)
        {
            OpenAfter = _vm.MergedFile;
            Close();
        }

        private async void OpenReport_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm.ReportFile is string report && File.Exists(report) && !Desktop.OpenFile(report))
                await Dialogs.Tell(this, "Merge report", $"The report is {report}");
        }

        private async void ShowFolder_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm.MergedFile is string merged && Path.GetDirectoryName(merged) is string folder && !Desktop.OpenFolder(folder))
                await Dialogs.Tell(this, "Merged mod", $"The merged mod is in {folder}");
        }
    }
}
