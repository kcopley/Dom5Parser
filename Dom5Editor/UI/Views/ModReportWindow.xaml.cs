using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dom5Edit.Entities;
using Dom5Edit.Validation;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// The report on a mod (ModReport): what goes wrong in game, lines the game ignores, numbers from
    /// another mod, things worth a look; each line with Go to. Shown from the bar the editor puts up
    /// when a mod is opened, and by Validate. Saves the same report as Markdown for the author.
    /// </summary>
    public partial class ModReportWindow : Window
    {
        private readonly MainWindowViewModel _main;
        private ModReport.Report _report;

        public ModReportWindow(MainWindowViewModel main, ModReport.Report report)
        {
            InitializeComponent();
            _main = main;
            _report = report;
            Show(report);
        }

        private void Show(ModReport.Report report)
        {
            _report = report;
            Title = $"Report: {report.Name}";
            DataContext = new ReportView(report);
        }

        private void GoTo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is LineView { Entity: { } entity })
            {
                _main.Session?.Navigate(entity);
                Owner?.Activate();
            }
        }

        private void Recheck_Click(object sender, RoutedEventArgs e)
        {
            if (_main.BuildReport() is { } report)
                Show(report);
        }

        private void AllIssues_Click(object sender, RoutedEventArgs e)
        {
            var results = _main.Validate();
            if (results != null)
                new ValidationReportWindow(results, _main) { Owner = this }.ShowDialog();
        }

        private void Save_Click(object sender, RoutedEventArgs e) => SaveForAuthor(this, _report);

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ModReport.Markdown(_report));
            _main.StatusMessage = "The report is on the clipboard (Markdown text)";
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        /// <summary>Saves a report as Markdown where the user picks (named after the mod).</summary>
        public static void SaveForAuthor(Window owner, ModReport.Report report)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save the report for the mod's author",
                Filter = "Markdown (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = ".md",
                FileName = string.Concat((report.Name + " - editor report").Split(System.IO.Path.GetInvalidFileNameChars())) + ".md",
            };
            if (dialog.ShowDialog(owner) != true)
                return;
            try
            {
                System.IO.File.WriteAllText(dialog.FileName, ModReport.Markdown(report));
            }
            catch (System.Exception ex) when (ex is System.IO.IOException || ex is System.UnauthorizedAccessException)
            {
                MessageBox.Show(owner, "The report wasn't saved:\n\n" + ex.Message, "Save report", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
