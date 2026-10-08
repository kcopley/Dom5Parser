using Avalonia.Controls;
using Dom5Editor.Ava.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Dom5Edit.Validation;
using Dom5Editor.UI.Views;

namespace Dom5Editor.Ava.Views
{
    /// <summary>
    /// Every issue Validate found, filtered by severity and text; Go To opens the entity (and
    /// closes the list). The port of the WPF editor's ValidationReportWindow.
    /// </summary>
    public partial class ValidationWindow : Window
    {
        private readonly MainWindowViewModel _main;
        private readonly ValidationResult _result;
        private readonly List<ValidationIssueItem> _all;

        public ValidationWindow() : this(new ValidationResult(), new MainWindowViewModel()) { }

        public ValidationWindow(ValidationResult result, MainWindowViewModel main)
        {
            InitializeComponent();
            _main = main;
            _result = result;
            // Go To: a middle click or the right click menu opens the entity in a window of its own (this list stays)
            Link.Install(this, c => c is Button { Tag: ValidationIssueItem { Entity: { } entity } } ? () => _main.Open(entity) : null);
            _all = result.Issues.Select(i => new ValidationIssueItem(i)).ToList();
            ErrorCountText.Text = $"{result.ErrorCount} Error{(result.ErrorCount == 1 ? "" : "s")}";
            WarningCountText.Text = $"{result.WarningCount} Warning{(result.WarningCount == 1 ? "" : "s")}";
            InfoCountText.Text = $"{result.InfoCount} Info";
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
            Refresh();
        }

        /// <summary>"12 issues" (the footer): what the filters show (the snapshot harness logs it).</summary>
        public string ShownText => FilteredCountText.Text ?? "";

        private bool Shows(ValidationIssueItem issue)
        {
            bool bySeverity = issue.Severity switch
            {
                ValidationSeverity.Error => ShowErrorsCheck.IsChecked == true,
                ValidationSeverity.Warning => ShowWarningsCheck.IsChecked == true,
                ValidationSeverity.Info => ShowInfoCheck.IsChecked == true,
                _ => true,
            };
            if (!bySeverity)
                return false;
            var search = SearchBox.Text?.Trim();
            return string.IsNullOrEmpty(search)
                   || issue.Message?.Contains(search, StringComparison.OrdinalIgnoreCase) == true
                   || issue.Category?.Contains(search, StringComparison.OrdinalIgnoreCase) == true
                   || issue.EntityDisplay?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
        }

        private void Refresh()
        {
            // (called from the check boxes' events while the window is still being built)
            if (_all == null || IssuesList == null)
                return;
            var shown = _all.Where(Shows).ToList();
            IssuesList.ItemsSource = shown;
            FilteredCountText.Text = $"{shown.Count} issue{(shown.Count == 1 ? "" : "s")}";
        }

        private void Filter_Changed(object? sender, RoutedEventArgs e) => Refresh();

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) => Refresh();

        private void GoTo(ValidationIssueItem? issue)
        {
            if (issue?.Entity is not { } entity)
                return;
            _main.NavigateToEntity(entity);
            Close();
        }

        private void GoTo_Click(object? sender, RoutedEventArgs e) => GoTo((sender as Control)?.Tag as ValidationIssueItem);

        private void IssuesList_DoubleTapped(object? sender, TappedEventArgs e) => GoTo(IssuesList.SelectedItem as ValidationIssueItem);

        private void Close_Click(object? sender, RoutedEventArgs e) => Close();

        private async void Export_Click(object? sender, RoutedEventArgs e)
        {
            // a report for the mod's author: what goes wrong in game, lines the game ignores (with
            // "did you mean"), what's worth a look; each with its line (Dom5Edit.Validation.ModReport)
            var session = _main.Session;
            var name = session?.Mod.ModName ?? "mod";
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save the report for the mod's author",
                DefaultExtension = "md",
                SuggestedFileName = string.Concat((name + " - editor report").Split(Path.GetInvalidFileNameChars())) + ".md",
                FileTypeChoices = Hooks.Filters("Markdown (*.md)|*.md|Text files (*.txt)|*.txt"),
            });
            if (file?.TryGetLocalPath() is not string path)
                return;
            try
            {
                if (session != null)
                    await File.WriteAllTextAsync(path, ModReport.Write(session.Mod, _result, session.Events.Problems));
                else
                    _result.ExportToFile(path);
                await Dialogs.Tell(this, "Report saved", $"The report for the mod's author is saved:\n{path}");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                await Dialogs.Tell(this, "Export Error", $"Failed to export report:\n{ex.Message}");
            }
        }
    }
}
