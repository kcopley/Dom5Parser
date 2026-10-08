using Avalonia.Controls;
using Dom5Editor.Ava.Controls;
using Avalonia.Interactivity;
using Dom5Edit.Validation;
using Dom5Editor.UI.Views;

namespace Dom5Editor.Ava.Views
{
    public partial class ReportWindow : Window
    {
        private readonly MainWindowViewModel _main;
        private ModReport.Report _report;

        public ReportWindow() : this(new MainWindowViewModel(), new ModReport.Report("", "", Array.Empty<ModReport.Section>())) { }

        public ReportWindow(MainWindowViewModel main, ModReport.Report report)
        {
            InitializeComponent();
            _main = main;
            _report = report;
            // Go to: a middle click or the right click menu opens the entity in a window of its own
            Link.Install(this, c => c is Button { Tag: LineView { Entity: { } entity } } ? () => _main.Open(entity) : null);
            Show(report);
            KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Escape)
                    Close();
            };
        }

        private void Show(ModReport.Report report)
        {
            _report = report;
            Title = $"Report: {report.Name}";
            DataContext = new ReportView(report);
        }

        private void GoTo_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.Tag is LineView { Entity: { } entity })
                _main.Open(entity); // (with Ctrl: a window of its own)
        }

        private void Recheck_Click(object? sender, RoutedEventArgs e)
        {
            if (_main.BuildReport() is { } report)
                Show(report);
        }

        /// <summary>"Every issue...": Validate's full list, with filters (modal: Go To closes it and shows the entity).</summary>
        private async void AllIssues_Click(object? sender, RoutedEventArgs e)
        {
            if (_main.Validate() is { } results)
                await new ValidationWindow(results, _main).ShowDialog(this);
        }

        private async void Save_Click(object? sender, RoutedEventArgs e) => await ReportFiles.SaveForAuthor(this, _report);

        private async void Copy_Click(object? sender, RoutedEventArgs e)
        {
            if (Clipboard != null)
                await Clipboard.SetTextAsync(ModReport.Markdown(_report));
            _main.StatusMessage = "The report is on the clipboard (Markdown text)";
        }

        private void Close_Click(object? sender, RoutedEventArgs e) => Close();
    }
}
