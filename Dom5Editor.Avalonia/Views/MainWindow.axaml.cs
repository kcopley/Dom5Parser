using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Dom5Editor.UI.Views;

namespace Dom5Editor.Ava.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm = new();

        /// <summary>Set by the snapshot harness: no "save changes?" question when closing.</summary>
        public bool SkipCloseConfirmation { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            KeyDown += OnKeyDown;
            Closing += OnClosing;
        }

        public MainWindowViewModel ViewModel => _vm;

        public void Status(string message) => _vm.StatusMessage = message;

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
            if (ctrl && e.Key == Key.O) { Load_Click(this, e); e.Handled = true; }
            else if (ctrl && e.Key == Key.N) { New_Click(this, e); e.Handled = true; }
            else if (ctrl && e.Key == Key.S && e.KeyModifiers.HasFlag(KeyModifiers.Shift)) { SaveAs_Click(this, e); e.Handled = true; }
            else if (ctrl && e.Key == Key.S) { Save_Click(this, e); e.Handled = true; }
            else if (ctrl && e.Key == Key.Z) { _vm.Undo(); e.Handled = true; }
            else if (ctrl && e.Key == Key.Y) { _vm.Redo(); e.Handled = true; }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) && e.Key == Key.Left) { _vm.GoBack(); e.Handled = true; }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) && e.Key == Key.Right) { _vm.GoForward(); e.Handled = true; }
        }

        private bool _closingConfirmed;

        private async void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            if (SkipCloseConfirmation || _closingConfirmed || !_vm.IsDirty)
                return;
            e.Cancel = true;
            if (await Dialogs.Ask(this, "Unsaved changes", "The mod has unsaved changes. Close without saving them?", "Close", "Keep editing"))
            {
                _closingConfirmed = true;
                Close();
            }
        }

        /// <summary>Asks before losing unsaved edits (a new mod, another mod opened).</summary>
        private async Task<bool> ConfirmDiscard() =>
            !_vm.IsDirty || await Dialogs.Ask(this, "Unsaved changes", "The mod has unsaved changes. Discard them?", "Discard", "Keep them");

        private async void New_Click(object? sender, RoutedEventArgs e)
        {
            if (await ConfirmDiscard())
                _vm.CreateNewMod();
        }

        private async void Load_Click(object? sender, RoutedEventArgs e)
        {
            if (!await ConfirmDiscard())
                return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load Mod File",
                AllowMultiple = false,
                FileTypeFilter = Hooks.Filters("Dominions mod files (*.dm)|*.dm|All files|*.*"),
            });
            if (files.Count > 0 && files[0].TryGetLocalPath() is string path)
                Open(path);
        }

        public void Open(string path)
        {
            try
            {
                _vm.LoadMod(path);
            }
            catch (Exception ex)
            {
                _ = Dialogs.Tell(this, "The mod couldn't be opened", ex.Message);
            }
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_vm.CurrentFilePath))
                SaveAs_Click(sender, e);
            else
                await SaveTo(_vm.CurrentFilePath!);
        }

        private async void SaveAs_Click(object? sender, RoutedEventArgs e)
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Mod As",
                DefaultExtension = "dm",
                SuggestedFileName = System.IO.Path.GetFileName(_vm.CurrentFilePath ?? "mod.dm"),
                FileTypeChoices = Hooks.Filters("Dominions mod files (*.dm)|*.dm"),
            });
            if (file?.TryGetLocalPath() is string path)
                await SaveTo(path);
        }

        private async Task SaveTo(string path)
        {
            // Steam replaces the files in its workshop folder when a mod updates: ask first
            var full = System.IO.Path.GetFullPath(path).Replace('\\', '/');
            if (full.Contains("/steamapps/workshop/content/", StringComparison.OrdinalIgnoreCase)
                && !await Dialogs.Ask(this, "Save into the workshop folder?",
                    $"{System.IO.Path.GetFileName(path)} is in Steam's workshop folder. Steam replaces these files when the mod updates, so changes saved here can be lost. Save here anyway?",
                    "Save here", "Don't save"))
                return;
            try
            {
                _vm.SaveMod(path);
            }
            catch (Exception ex)
            {
                await Dialogs.Tell(this, "The mod wasn't saved", ex.Message);
            }
        }

        private void Undo_Click(object? sender, RoutedEventArgs e) => _vm.Undo();
        private void Redo_Click(object? sender, RoutedEventArgs e) => _vm.Redo();
        private void Back_Click(object? sender, RoutedEventArgs e) => _vm.GoBack();
        private void Forward_Click(object? sender, RoutedEventArgs e) => _vm.GoForward();

        private void Validate_Click(object? sender, RoutedEventArgs e) => OpenReport(rebuild: true);
        private void ReportOpen_Click(object? sender, RoutedEventArgs e) => OpenReport(rebuild: false);
        private void ReportClose_Click(object? sender, RoutedEventArgs e) => _vm.ShowReportBar = false;

        private async void ReportSave_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm.Report is { } report)
                await ReportFiles.SaveForAuthor(this, report);
        }

        private void OpenReport(bool rebuild)
        {
            var report = rebuild || _vm.Report == null ? _vm.BuildReport() : _vm.Report;
            if (report != null)
                new ReportWindow(_vm, report).Show(this);
        }
    }
}
