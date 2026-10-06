using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Resolve;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// One editable value in a panel, shown as a choice, a number or a reference, with where it
    /// comes from. A field edits one command (for a keyed command, the value for one first
    /// argument: "#path 1 p" edits the second path). Setting it makes the edit through the page.
    /// </summary>
    public abstract class PanelField : INotifyPropertyChanged
    {
        protected readonly EntityPageViewModel Page;

        protected PanelField(EntityPageViewModel page, string label, Command command, string? key, string? tooltip)
        {
            Page = page;
            Label = label;
            Command = command;
            Key = key;
            Value = key == null
                ? page.Resolved.Get(command)
                : page.Resolved.GetAll(command).LastOrDefault(v => v.Selector == key);
            // a field's own hint, else the manual's
            tooltip ??= Data.CommandHints.Tooltip(page.Type, command);
            Tooltip = string.Join("\n", new[] { tooltip, EntityPageViewModel.CommandName(command) + (key != null ? " " + key : ""),
                Value != null ? page.SourceText(Value) : "Not set: the game's default" }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Label { get; }
        public Command Command { get; }
        /// <summary>For a keyed command, the first argument this field edits.</summary>
        public string? Key { get; }
        public ResolvedValue? Value { get; }
        public string Tooltip { get; init; }
        public virtual bool IsInherited => Value == null || Value.Source != ValueSource.Own;

        /// <summary>An icon shown with the label (a GameIcon kind: "hp", "path:F", ...), or null.</summary>
        public string? Icon { get; init; }

        /// <summary>The value the mod sets here (its own line), which Reset drops.</summary>
        protected virtual ResolvedValue? OwnValue => Value != null && Value.Source == ValueSource.Own ? Value : null;

        /// <summary>Whether the mod sets this value itself (so it can go back to what it inherits).</summary>
        public bool CanReset => OwnValue != null;

        /// <summary>Drops the mod's own line for this value, back to what it inherits (vanilla, the copy source, the default).</summary>
        public ICommand ResetCommand => new RelayCommand(() => { if (OwnValue is ResolvedValue v) Page.ResetLine(v); });

        public string ResetTip => "Back to what it inherits" + (Page.InheritedText(Command, Key) is string t ? $": {t}" : "");

        /// <summary>The value's arguments, without the key.</summary>
        protected string Arguments
        {
            get
            {
                if (Value == null)
                    return "";
                var args = Page.DisplayArguments(Value.Property);
                return Key != null && args.StartsWith(Key + " ") ? args.Substring(Key.Length + 1) : args;
            }
        }

        protected void Commit(string args) => Page.SetValue(Command, Key != null ? Key + " " + args : args);

        protected void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class ChoiceOption
    {
        public ChoiceOption(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public int Value { get; }
        public string Name { get; }
        /// <summary>An icon shown with the name (a magic path's), or null.</summary>
        public string? Icon { get; init; }
        public override string ToString() => Name;
    }

    /// <summary>A value picked from a list (a path, a school, an item type).</summary>
    public sealed class ChoiceField : PanelField
    {
        private readonly int? _default;

        public ChoiceField(EntityPageViewModel page, string label, Command command, string? key, IReadOnlyList<ChoiceOption> options, int? defaultValue = null, string? tooltip = null)
            : base(page, label, command, key, tooltip)
        {
            Options = options;
            _default = defaultValue;
        }

        public IReadOnlyList<ChoiceOption> Options { get; }

        public int? Selected
        {
            get => int.TryParse(Arguments.Split(' ')[0], out var v) ? v : _default;
            set
            {
                if (value is int v && v != Selected)
                    Commit(v.ToString());
            }
        }
    }

    /// <summary>A number (or text) typed in, with a note that explains it (decoded costs, levels).</summary>
    public sealed class NumberField : PanelField
    {
        private readonly Func<string, string>? _note;
        private readonly string? _default;

        public NumberField(EntityPageViewModel page, string label, Command command, string? key = null, Func<string, string>? note = null, string? defaultValue = null, string? tooltip = null)
            : base(page, label, command, key, tooltip)
        {
            _note = note;
            _default = defaultValue;
        }

        public string Text
        {
            get => Value != null ? Arguments : _default ?? "";
            set
            {
                if (value != Text && !string.IsNullOrWhiteSpace(value))
                    Commit(value.Trim());
            }
        }

        public string Note => _note?.Invoke(Text) ?? "";
        public bool HasNote => Note.Length > 0;
    }

    /// <summary>A reference to another entity, picked from a searchable list, with a button to open it.</summary>
    public sealed class RefField : PanelField
    {
        public RefField(EntityPageViewModel page, string label, Command command, EntityType refType, string? key = null, string? tooltip = null)
            : base(page, label, command, key, tooltip)
        {
            RefType = refType;
            Candidates = page.Session.References(refType);
            OpenCommand = new RelayCommand(() => { if (SelectedId is int id && id != 0) Page.Session.Navigate(RefType, id); });
        }

        public EntityType RefType { get; }
        public IReadOnlyList<ReferenceItem> Candidates { get; }
        public ICommand OpenCommand { get; }

        public int? SelectedId
        {
            get => Value != null ? Page.ReferenceOf(Value.Property, EntityPageViewModel.RefTypeName(RefType)).Id : null;
            set
            {
                if (value is int id && id != SelectedId)
                    Commit(id.ToString());
            }
        }
    }

    /// <summary>
    /// One of several flag commands that are alternatives (leader classes): picking one replaces the
    /// entity's own line for the old one, or adds a line that overrides the inherited one.
    /// </summary>
    public sealed class CommandChoiceField : PanelField
    {
        private readonly int? _default;

        public CommandChoiceField(EntityPageViewModel page, string label, IReadOnlyList<(Command Command, string Name)> choices, string? tooltip = null, int? defaultIndex = null)
            : base(page, label, choices[0].Command, null, tooltip)
        {
            _default = defaultIndex;
            Commands = choices.Select(c => c.Command).ToList();
            Options = choices.Select((c, i) => new ChoiceOption(i, c.Name)).ToList();
            Current = page.Resolved.Values.LastOrDefault(v => Commands.Contains(v.Command));
        }

        public IReadOnlyList<Command> Commands { get; }
        public IReadOnlyList<ChoiceOption> Options { get; }
        public ResolvedValue? Current { get; }
        public override bool IsInherited => Current == null || Current.Source != ValueSource.Own;
        protected override ResolvedValue? OwnValue => Current != null && Current.Source == ValueSource.Own ? Current : null;

        public int? Selected
        {
            get => Current != null ? Commands.ToList().IndexOf(Current.Command) : _default;
            set
            {
                if (value is not int i || i < 0 || i >= Commands.Count || i == Selected)
                    return;
                var chosen = Commands[i];
                var current = Current;
                var entity = Page.Entity;
                Page.EditRun($"Set {EntityPageViewModel.CommandName(chosen)}", (ed, tx) =>
                {
                    var own = ed.OwnEntity(entity);
                    if (current != null && current.Source == ValueSource.Own && own != null)
                        tx.RemoveLine(own, current.Property);
                    tx.Set(entity, chosen, "");
                });
            }
        }
    }

    /// <summary>
    /// A monster's leadership of one kind (units, magic beings, undead): the class (#goodleader,
    /// ...) and the bonus on top (#command, #magiccommand, #undcommand), shown as the game shows
    /// the total.
    /// </summary>
    public sealed class LeaderField
    {
        public static readonly int[] TierValues = { 0, 10, 50, 100, 150, 200 };
        private static readonly string[] TierNames = { "None", "Poor", "OK", "Good", "Expert", "Superior" };

        public LeaderField(EntityPageViewModel page, string label, string icon, Command[] tiers, Command bonus, int? defaultTier, string tooltip)
        {
            Label = label;
            Icon = icon;
            Class = new CommandChoiceField(page, label, tiers.Select((c, i) => (c, $"{TierNames[i]} ({TierValues[i]})")).ToList(),
                tooltip: tooltip, defaultIndex: defaultTier) { Icon = icon };
            Bonus = new NumberField(page, label + " bonus", bonus, defaultValue: "0",
                tooltip: $"{EntityPageViewModel.CommandName(bonus)}: added to the class's {label.ToLowerInvariant()}");
        }

        public string Label { get; }
        public string Icon { get; }
        public CommandChoiceField Class { get; }
        public NumberField Bonus { get; }
        public bool IsInherited => Class.IsInherited && Bonus.IsInherited;

        /// <summary>The hint: the class's, and the total the game shows.</summary>
        public string Hint => $"{Label} in game: {Total} (class + bonus)\n{Class.Tooltip}";

        /// <summary>The leadership in game: the class's value plus the bonus.</summary>
        public string Total => (Class.Selected is int i && i >= 0 && i < TierValues.Length ? TierValues[i] : 0)
                               + (int.TryParse(Bonus.Text, out var b) ? b : 0) is int t ? t.ToString() : "";
    }

    /// <summary>
    /// A monster's stats as the game's unit window lays them out: three columns (body, combat,
    /// movement and age), each value with its icon, then its cost.
    /// </summary>
    public sealed class StatsPanel
    {
        public sealed class Column
        {
            public ObservableCollection<object> Cells { get; } = new ObservableCollection<object>();
        }

        public string Title { get; init; } = "STATS";
        public ObservableCollection<Column> Columns { get; } = new ObservableCollection<Column>();
        /// <summary>A row under the columns (cost).</summary>
        public ObservableCollection<PanelField> Footer { get; } = new ObservableCollection<PanelField>();
    }

    /// <summary>A panel of fields: a spell's paths and cost, an item's slot and paths, a site's path and rarity.</summary>
    public sealed class FieldsPanel
    {
        public FieldsPanel(string title, string hint = "")
        {
            Title = title;
            Hint = hint;
        }

        public string Title { get; }
        public string Hint { get; set; }
        public bool HasHint => !string.IsNullOrEmpty(Hint);
        public ObservableCollection<PanelField> Fields { get; } = new ObservableCollection<PanelField>();
    }

    /// <summary>
    /// A keyed command as a list: one row per first argument (a site's gems per path, a monster's
    /// magic paths), its second argument editable, rows added from a list of keys.
    /// </summary>
    public sealed class KeyedListPanel
    {
        private readonly EntityPageViewModel _page;
        private readonly Command _command;
        private readonly string _newValue;

        public KeyedListPanel(EntityPageViewModel page, string title, Command command, IReadOnlyList<ChoiceOption> keys, string valueLabel, string newValue = "1", string hint = "")
        {
            _page = page;
            _command = command;
            _newValue = newValue;
            Title = title;
            ValueLabel = valueLabel;
            Hint = hint;
            foreach (var v in page.Resolved.GetAll(command))
            {
                var args = ResolvedValue.ArgumentsOf(v.Property).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int key = args.Length > 0 && int.TryParse(args[0], out var k) ? k : 0;
                string rest = string.Join(" ", args.Skip(1));
                var option = keys.FirstOrDefault(o => o.Value == key);
                var row = new PanelRow(v, option?.Name ?? key.ToString(), rest, page.SourceText(v), true) { RefId = key, EditText = rest, Icon = option?.Icon };
                row.Edited = r =>
                {
                    if (string.IsNullOrWhiteSpace(r.EditText) || r.EditText == r.Detail)
                        return;
                    _page.ChangeValue(r.Value, $"{r.RefId} {r.EditText.Trim()}");
                };
                Rows.Add(row);
            }
            var have = new HashSet<int>(Rows.Select(r => r.RefId));
            Addable = keys.Where(o => !have.Contains(o.Value)).Select(o => new ReferenceItem { ID = o.Value, DisplayName = o.Name }).ToList();
            AddableKeys = keys.Where(o => !have.Contains(o.Value) && o.Icon != null).ToList();
            RemoveCommand = new RelayCommand<PanelRow>(r => { if (r != null) _page.RemoveValue(r.Value); });
            AddKeyCommand = new RelayCommand<ChoiceOption>(o => { if (o != null) AddPick = o.Value; });
        }

        /// <summary>Keys with icons (paths, gems) to add by clicking; the picker covers the rest.</summary>
        public IReadOnlyList<ChoiceOption> AddableKeys { get; }
        public bool HasIconKeys => AddableKeys.Count > 0 || Rows.Any(r => r.Icon != null);
        public ICommand AddKeyCommand { get; }

        public string Title { get; }
        public string ValueLabel { get; }
        public string Hint { get; }
        public bool HasHint => !string.IsNullOrEmpty(Hint);
        public ObservableCollection<PanelRow> Rows { get; } = new ObservableCollection<PanelRow>();
        public IReadOnlyList<ReferenceItem> Addable { get; }
        public ICommand RemoveCommand { get; }

        public int? AddPick
        {
            get => null;
            set { if (value is int k) _page.SetValue(_command, $"{k} {_newValue}"); }
        }
    }
}
