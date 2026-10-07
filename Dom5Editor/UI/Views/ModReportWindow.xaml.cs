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

    /// <summary>The report as the window shows it.</summary>
    public sealed class ReportView
    {
        public ReportView(ModReport.Report report)
        {
            Title = $"{report.Name}: what the editor found";
            About = report.About.Replace("`", "");
            Sections = report.Sections.Where(s => s.Count > 0 || s.Key != ModReport.Missing)
                .Select(s => new CountView(s.Key, s.Count, Label(s.Key, s.Count), s.Intro)).ToList();
            Shown = report.Sections.Where(s => s.Count > 0)
                .Select(s => new SectionView(s.Key, s.Title, s.Intro, s.Count, s.Groups.Select(g => new GroupView(g, s.Key)).ToList())).ToList();
        }

        public string Title { get; }
        public string About { get; }
        /// <summary>The counts, every section (an empty "numbers from another mod" left out).</summary>
        public IReadOnlyList<CountView> Sections { get; }

        private static string Label(string key, int n) => key switch
        {
            ModReport.Wrong => n == 1 ? "goes wrong in game" : "go wrong in game",
            ModReport.Ignored => n == 1 ? "line the game ignores" : "lines the game ignores",
            ModReport.Missing => n == 1 ? "number not in this mod or the game" : "numbers not in this mod or the game",
            _ => "worth a look",
        };
        /// <summary>The sections with findings.</summary>
        public IReadOnlyList<SectionView> Shown { get; }
        public bool IsEmpty => Shown.Count == 0;
    }

    public sealed record CountView(string Key, int Count, string Title, string Intro);

    public sealed record SectionView(string Key, string Title, string Intro, int Count, IReadOnlyList<GroupView> Groups);

    public sealed class GroupView : INotifyPropertyChanged
    {
        public GroupView(ModReport.Group group, string section)
        {
            Text = group.Text.Replace("`", "");
            Lines = group.Lines.Select(l => new LineView(l)).ToList();
            // what goes wrong opens up when it's short; long lists stay folded
            _expanded = Lines.Count <= (section == ModReport.Wrong ? 6 : 2);
        }

        public string Text { get; }
        public IReadOnlyList<LineView> Lines { get; }

        private bool _expanded;
        public bool IsExpanded
        {
            get => _expanded;
            set { _expanded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded))); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public sealed class LineView
    {
        public LineView(ModReport.Line line)
        {
            Where = line.Number is int n ? $"line {n}" : "";
            Text = line.Text;
            Entity = line.Entity;
        }

        public string Where { get; }
        public string Text { get; }
        public IDEntity? Entity { get; }
        public bool CanGo => Entity != null;
        public string GoTip => Entity == null ? "" : $"Open {Nouns.Of(Entity.Kind)} {Nouns.Named(Entity.Name, Entity.ID)}";
    }
}
