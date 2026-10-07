using System.ComponentModel;
using Dom5Edit.Entities;
using Dom5Edit.Validation;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.UI.Views
{
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
