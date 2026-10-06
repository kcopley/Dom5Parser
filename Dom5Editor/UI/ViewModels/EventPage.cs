using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Events;
using Dom5Edit.Props;
using Dom5Edit.Resolve;
using Dom5Editor.Data;
using Dom5Editor.Session;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// An event, read as a script (docs/EVENT_EDITOR.md, E-3): how it's rolled and who owns it,
    /// its message, the requirements ("when") by group, the effects ("then") in line order, and
    /// how it chains with other events and spells. Every line reads as a sentence with its value
    /// editable in place.
    /// </summary>
    public sealed class EventPageViewModel : EntityPageViewModel
    {
        public EventPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        /// <summary>Commands shown in the header and message panels, not as lines.</summary>
        internal static readonly HashSet<Command> HeaderCommands = new HashSet<Command>
        {
            Command.RARITY, Command.NATION, Command.NATIONENCH, Command.MSG, Command.HEADER, Command.NOTEXT, Command.NOLOG,
        };

        protected override bool ShowsLongText(Command c) => c != Command.MSG;

        /// <summary>Lines are added in the requirement and effect panels.</summary>
        public override bool ShowsAddBox => false;

        /// <summary>An event's name: its title, from the message.</summary>
        public override string DisplayName => TitleOf(Entity, LinesOf(Resolved));

        /// <summary>An event's title: from its message; a game event without one we can show (no exe to read it from) is "Game event N".</summary>
        public static string TitleOf(IDEntity e, IReadOnlyList<Property> lines) =>
            e.Selected && e.ID >= 0 && EventInfo.Message(lines) == null ? $"Game event {e.ID}" : EventInfo.Title(lines);

        /// <summary>An event's lines in game, with a game event's message (read from the exe, kept apart as a display asset) when it has none of its own.</summary>
        public static IReadOnlyList<Property> LinesOf(ResolvedEntity r)
        {
            var lines = r.Values.Select(v => v.Property).ToList();
            if (!lines.Any(p => p.Command == Command.MSG) && r.Assets.TryGetValue(Command.MSG, out var msg))
                lines.Add(msg);
            return lines;
        }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            var graph = Session.Events;
            Panels.Add(new EventHeaderPanel(this));
            Panels.Add(new EventMessagePanel(this));
            var lines = Resolved.Values.ToList();
            Panels.Add(new EventLinesPanel(this, graph, lines, requirements: true));
            Panels.Add(new EventLinesPanel(this, graph, lines, requirements: false));
            Panels.Add(new EventChainPanel(this, graph));
            covered.UnionWith(HeaderCommands);
            foreach (var c in Entity.GetPropertyMap().Keys)
                if (CommandsMap.TryGetString(c, out var s) && EventCommands.Get(s) != null)
                    covered.Add(c);
        }

        internal static string Name(Command c) => CommandsMap.TryGetString(c, out var s) ? s : c.ToString();
    }

    /// <summary>How the event is rolled (#rarity) and who owns it (#nation, #nationench).</summary>
    public sealed class EventHeaderPanel
    {
        private readonly EntityPageViewModel _page;

        public EventHeaderPanel(EntityPageViewModel page)
        {
            _page = page;
            var rarity = page.Resolved.Get(Command.RARITY);
            RarityValue = rarity;
            Rarities = EventInfo.Rarities.Select(r => new ChoiceOption((int)r.Value, r.Name)).ToList();
            _rarity = EventInfo.Number(rarity?.Property) is long n ? (int)n : null;
            RarityMeaning = _rarity is int v ? EventInfo.Rarities.FirstOrDefault(r => r.Value == v).Meaning ?? "" : "No #rarity set.";

            var nation = page.Resolved.Get(Command.NATION);
            var ench = page.Resolved.Get(Command.NATIONENCH);
            _nation = nation;
            _ench = ench;
            long? owner = EventInfo.Number(nation?.Property);
            _owner = ench != null ? 4 : owner switch { null => 0, -2 => 1, -1 => 2, _ => 3 };
            _ownerNation = owner is long o && o > 0 ? (int)o : null;
            _ownerEnchantment = EventInfo.Number(ench?.Property)?.ToString() ?? "";
            Nations = page.Session.References(EntityType.NATION);
        }

        private readonly ResolvedValue? _nation;
        private readonly ResolvedValue? _ench;

        public ResolvedValue? RarityValue { get; }
        public IReadOnlyList<ChoiceOption> Rarities { get; }
        public string RarityMeaning { get; }
        public string RarityTooltip => CommandHints.Tooltip(EntityType.EVENT, Command.RARITY) ?? "#rarity";

        public int? Rarity
        {
            get => _rarity;
            set
            {
                if (value is int v && v != _rarity)
                    _page.SetValue(Command.RARITY, v.ToString());
            }
        }

        private readonly int? _rarity;

        public static readonly IReadOnlyList<ChoiceOption> OwnerOptions = new[]
        {
            new ChoiceOption(0, "Independents (the default)"), new ChoiceOption(1, "The province's owner"),
            new ChoiceOption(2, "A random enemy"), new ChoiceOption(3, "A nation"), new ChoiceOption(4, "Whoever has an enchantment"),
        };

        private readonly int _owner;

        /// <summary>Who owns the event (gets its units, gold, ...): 0 independents, 1 province owner (-2), 2 random enemy (-1), 3 a nation, 4 an enchantment's owner.</summary>
        public int Owner
        {
            get => _owner;
            set
            {
                if (value == _owner)
                    return;
                var nation = _nation;
                var ench = _ench;
                var entity = _page.Entity;
                _page.EditRun("Set the event's owner", (ed, tx) =>
                {
                    var own = ed.OwnEntity(entity);
                    if (value != 4 && ench != null && own != null && ench.Source == ValueSource.Own)
                        tx.RemoveLine(own, ench.Property);
                    switch (value)
                    {
                        case 0:
                            if (nation != null && own != null && nation.Source == ValueSource.Own)
                                tx.RemoveLine(own, nation.Property);
                            break;
                        case 1: tx.Set(entity, Command.NATION, "-2"); break;
                        case 2: tx.Set(entity, Command.NATION, "-1"); break;
                        case 3: tx.Set(entity, Command.NATION, "2"); break; // a nation to pick (2: independents, special)
                        case 4:
                            if (nation != null && own != null && nation.Source == ValueSource.Own)
                                tx.RemoveLine(own, nation.Property);
                            tx.Set(entity, Command.NATIONENCH, "0");
                            break;
                    }
                });
            }
        }

        public bool IsNationOwner => _owner == 3;
        public bool IsEnchantmentOwner => _owner == 4;
        public IReadOnlyList<ReferenceItem> Nations { get; }

        public int? OwnerNation
        {
            get => _ownerNation;
            set
            {
                if (value is int v && v != _ownerNation)
                    _page.SetValue(Command.NATION, v.ToString());
            }
        }

        private readonly int? _ownerNation;

        public string OwnerEnchantment
        {
            get => _ownerEnchantment;
            set
            {
                if (value != _ownerEnchantment && long.TryParse(value?.Trim(), out var n))
                    _page.SetValue(Command.NATIONENCH, n.ToString());
            }
        }

        private readonly string _ownerEnchantment = "";
        public string OwnerTooltip => (CommandHints.Tooltip(EntityType.EVENT, Command.NATION) ?? "#nation") + "\n\n" + (CommandHints.Tooltip(EntityType.EVENT, Command.NATIONENCH) ?? "");
    }

    /// <summary>The event's message (#msg), its header and options, the site or item it names in brackets.</summary>
    public sealed class EventMessagePanel : INotifyPropertyChanged
    {
        private readonly EntityPageViewModel _page;
        private readonly ResolvedValue? _msg;

        public EventMessagePanel(EntityPageViewModel page)
        {
            _page = page;
            _msg = page.Resolved.Get(Command.MSG);
            _text = _msg?.Property is StringProperty s ? s.Value ?? "" : "";
            // a game event's own message (read from the exe): shown; a change writes the mod's #msg
            if (_msg == null && page.Resolved.Assets.TryGetValue(Command.MSG, out var game) && game is StringProperty g)
            {
                _text = g.Value ?? "";
                IsGameText = true;
            }
            HeaderOptions = new[] { new ChoiceOption(0, "\"An unexpected event has occurred in...\"") }.Concat(EventCommands.Options("header")).ToList();
            _header = EventInfo.Number(page.Resolved.Get(Command.HEADER)?.Property) is long h ? (int)h : 0;
            NoText = page.Resolved.Has(Command.NOTEXT);
            NoLog = page.Resolved.Has(Command.NOLOG);
            var name = EventInfo.BracketName(_text);
            BracketName = name;
            if (name != null)
            {
                var site = page.Session.References(EntityType.SITE).FirstOrDefault(r => string.Equals(r.DisplayName, name, StringComparison.OrdinalIgnoreCase));
                var item = site == null ? page.Session.References(EntityType.ITEM).FirstOrDefault(r => string.Equals(r.DisplayName, name, StringComparison.OrdinalIgnoreCase)) : null;
                if (site != null)
                {
                    NamedNote = $"Names the site {site.DisplayName} #{site.ID}";
                    OpenNamedCommand = new RelayCommand(() => page.Session.Navigate(EntityType.SITE, site.ID));
                }
                else if (item != null)
                {
                    NamedNote = $"Names the item {item.DisplayName} #{item.ID}";
                    OpenNamedCommand = new RelayCommand(() => page.Session.Navigate(EntityType.ITEM, item.ID));
                }
                else
                {
                    NamedNote = $"No site or item is called \"{name}\": the game won't find it";
                    NamedIsWarning = true;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>The text shown is the game's own message (from the player's Dominions6.exe), not a line of the mod.</summary>
        public bool IsGameText { get; }

        private string _text;

        /// <summary>The message; leaving the box saves it.</summary>
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;
                if (string.IsNullOrEmpty(value) && _msg == null)
                {
                    // the game's own text can't be removed (#notext hides it): show it again
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                    return;
                }
                _text = value;
                if (string.IsNullOrEmpty(value))
                    _page.RemoveValue(_msg!);
                else
                    _page.SetValue(Command.MSG, _page.CommitArguments(Command.MSG, value));
            }
        }

        /// <summary>
        /// The message as a player would read it: the header line (unless #header says otherwise),
        /// tags filled with sample names, the bracketed site or item name left out.
        /// </summary>
        public string Preview
        {
            get
            {
                if (NoText)
                    return "(no message: #notext)";
                var body = System.Text.RegularExpressions.Regex.Replace(_text, @"\[([^\[\]]+)\]\s*$", "").TrimEnd();
                foreach (var (tag, sample) in Samples)
                    body = body.Replace(tag, sample, StringComparison.OrdinalIgnoreCase);
                return _header switch
                {
                    0 => "An unexpected event has occured in Ancient Forest.\n\n" + body,
                    1 => body,
                    _ => body,
                };
            }
        }

        private static readonly (string Tag, string Sample)[] Samples =
        {
            ("##landname##", "Ancient Forest"), ("##fullgodname##", "Ulla the Great Mother, Queen of the Forest"), ("##godname##", "Ulla the Great Mother"),
            ("##goddisname##", "Ulla"), ("##disname##", "Ulla"), ("##fulltargname##", "Hrothgar the Warrior Chief"), ("##targname##", "Hrothgar"),
            ("##targhis##", "his"), ("##natname##", "Ulm"), ("##profname##", "Thorgrim"),
        };

        public int Length => _text.Length;
        public string LengthText => $"{_text.Length} / 2399";
        public bool IsTooLong => _text.Length > 2399;
        public string Tooltip => (CommandHints.Tooltip(EntityType.EVENT, Command.MSG) ?? "#msg") + (_msg != null ? "\n" + _page.SourceText(_msg) : "");

        public sealed record TagItem(string Tag, string Meaning);

        /// <summary>The tags the game fills in (manual: "special tags").</summary>
        public static readonly IReadOnlyList<TagItem> Tags = new[]
        {
            new TagItem("##landname##", "the province's name"), new TagItem("##godname##", "the god's name and a title"),
            new TagItem("##fullgodname##", "the god's name and every title"), new TagItem("##disname##", "the disciple's name for a disciple, else the god's"),
            new TagItem("##goddisname##", "the disciple's or the god's name, 50/50"), new TagItem("##targname##", "the target commander's name (not in global events)"),
            new TagItem("##fulltargname##", "the target commander's name and type (not in global events)"), new TagItem("##targhis##", "his or her, for the target commander"),
            new TagItem("##natname##", "the nation's name"), new TagItem("##profname##", "the prophet's or disciple's name"),
        };

        public IReadOnlyList<ChoiceOption> HeaderOptions { get; }

        private readonly int _header;

        /// <summary>#header: 0 the usual "unexpected event" line, 1 without it, 2 no header (the first line is the header).</summary>
        public int Header
        {
            get => _header;
            set
            {
                if (value == _header)
                    return;
                if (value == 0 && _page.Resolved.Get(Command.HEADER) is { } h)
                    _page.RemoveValue(h);
                else
                    _page.SetValue(Command.HEADER, value.ToString());
            }
        }

        public bool NoText { get; }
        public bool NoLog { get; }

        public bool NoTextValue
        {
            get => NoText;
            set { if (value != NoText) Flag(Command.NOTEXT, value); }
        }

        public bool NoLogValue
        {
            get => NoLog;
            set { if (value != NoLog) Flag(Command.NOLOG, value); }
        }

        private void Flag(Command c, bool on)
        {
            if (on)
                _page.SetValue(c, "");
            else if (_page.Resolved.Get(c) is { } v)
                _page.RemoveValue(v);
        }

        public string? BracketName { get; }
        public string NamedNote { get; } = "";
        public bool HasNamed => NamedNote.Length > 0;
        public bool NamedIsWarning { get; }
        public ICommand? OpenNamedCommand { get; }
    }

    /// <summary>A flag of a mask value (an order, an affliction, a terrain), toggled in the line's list.</summary>
    public sealed class MaskOption : INotifyPropertyChanged
    {
        private readonly EventLineRow _row;
        private bool _isOn;

        public MaskOption(EventLineRow row, long bit, string name, bool isOn)
        {
            _row = row;
            Bit = bit;
            Name = name;
            _isOn = isOn;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public long Bit { get; }
        public string Name { get; }

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value)
                    return;
                _isOn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOn)));
                _row.CommitMask();
            }
        }
    }

    /// <summary>A link from a line to what it connects with: an event that sets or checks the same code, a spell, an entity.</summary>
    public sealed class LinkChip
    {
        public LinkChip(string label, string title, Action open, string tooltip = "")
        {
            Label = label;
            Title = title;
            OpenCommand = new RelayCommand(open);
            Tooltip = tooltip;
        }

        public string Label { get; }
        public string Title { get; }
        public string Tooltip { get; }
        public ICommand OpenCommand { get; }
    }

    /// <summary>
    /// One line of an event as a sentence with its value editable in place: a number box, a choice
    /// (a season, a path; for 0/1 lines the two sentences), an entity picker, or a list of flags.
    /// Codes, variables and enchantments link to the events and spells on the other end.
    /// </summary>
    public sealed class EventLineRow : INotifyPropertyChanged
    {
        private readonly EntityPageViewModel _page;

        public EventLineRow(EntityPageViewModel page, EventGraph graph, ResolvedValue value)
        {
            _page = page;
            Value = value;
            var name = EventPageViewModel.Name(value.Command);
            CommandName = name;
            Def = EventCommands.Get(name);
            Arg = Def?.Arg ?? "text";
            var args = page.DisplayArguments(value.Property);
            long? n = EventInfo.Number(value.Property);
            _number = n;
            Icon = Def?.Icon ?? (n is long k ? EventCommands.IconOf(Arg, k) : null);
            Tooltip = string.Join("\n", new[]
            {
                CommandHints.Tooltip(EntityType.EVENT, value.Command) ?? name,
                "", value.Property.ToExportString(), page.SourceText(value),
            });

            var text = Def?.Text ?? name + " {v}";
            int at = text.IndexOf("{v}", StringComparison.Ordinal);
            if (Arg == "bool")
            {
                Kind = "bool";
                Options = new[] { new ChoiceOption(1, Def!.Text1 ?? "yes"), new ChoiceOption(0, Def.Text0 ?? "no") };
                _selected = n is long b ? (int)b : 1;
            }
            else if (Arg == "none" || Def != null && at < 0)
            {
                Kind = "none";
                Before = text;
            }
            else
            {
                Before = at >= 0 ? text[..at] : text + " ";
                var after = at >= 0 ? text[(at + 3)..] : "";
                After = after.Replace("{v}", n is long w ? EventCommands.Words(Arg, w) : args);
                var refType = Arg switch { "monster" => EntityType.MONSTER, "nation" => EntityType.NATION, "site" => EntityType.SITE, "item" => EntityType.ITEM, "poptype" => EntityType.POPTYPE, _ => (EntityType?)null };
                if (refType is EntityType rt && !(n is long neg && neg < 0))
                {
                    Kind = "ref";
                    RefType = rt;
                    var (id, _) = page.ReferenceOf(value.Property, EntityPageViewModel.RefTypeName(rt));
                    _refId = id;
                    Candidates = page.Session.References(rt);
                    OpenCommand = new RelayCommand(() => { if (_refId is int i && i > 0) page.Session.Navigate(rt, i); });
                    if (id > 0 && !Candidates.Any(c => c.ID == id))
                        _missing = $"there's no {rt.ToString().ToLowerInvariant()} #{id} in the mod or the game's data";
                }
                else if (EventCommands.IsMask(Arg))
                {
                    Kind = "mask";
                    long mask = n ?? 0;
                    MaskOptions = EventCommands.Values(Arg).Select(v => new MaskOption(this, v.Value, v.Name, (mask & v.Value) == v.Value && v.Value != 0)).ToList();
                }
                else if (EventCommands.IsChoice(Arg) && (n == null || EventCommands.Values(Arg).Any(v => v.Value == n)))
                {
                    Kind = "choice";
                    Options = EventCommands.Options(Arg);
                    _selected = n is long c ? (int)c : null;
                }
                else
                {
                    Kind = "text";
                    _editText = args;
                    Suffix = Arg == "percent" ? "%" : "";
                }
            }
            AddLinks(graph);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ResolvedValue Value { get; }
        public Property Line => Value.Property;
        public Command Command => Value.Command;
        public string CommandName { get; }
        public EventCommands.Def? Def { get; }
        public string Arg { get; }
        public string? Icon { get; }
        public string Tooltip { get; }
        public bool IsInherited => Value.Source != ValueSource.Own;

        /// <summary>
        /// Whether the line can be removed: the mod's own. A game event's line can't be taken out
        /// one at a time (only #clear empties an event, and that drops its message too).
        /// </summary>
        public bool CanRemove => !IsInherited;

        /// <summary>
        /// Whether the value can be changed here: the mod's own line, or a game line the game
        /// replaces (most requirements). A line that adds (most effects) can't: the mod's would be
        /// added to the game's, not replace it.
        /// </summary>
        public bool CanEdit => !IsInherited || !Stacks(Command);

        private static bool Stacks(Command c) => Dom5Edit.GameData.GameCommandCatalog.EffectOf(EntityType.EVENT, c) is { } e && e.Add.Count > 0;

        /// <summary>How the value is edited: text, choice, bool (the two sentences), ref, mask, none.</summary>
        public string Kind { get; }
        public bool IsText => Kind == "text";
        public bool IsChoice => Kind == "choice";
        public bool IsBool => Kind == "bool";
        public bool IsRef => Kind == "ref";
        public bool IsMask => Kind == "mask";

        public string Before { get; } = "";
        public string After { get; } = "";
        public string Suffix { get; } = "";

        private readonly long? _number;

        private string _editText = "";

        public string EditText
        {
            get => _editText;
            set
            {
                if (value == _editText || string.IsNullOrWhiteSpace(value))
                    return;
                _editText = value;
                _page.ChangeValue(Value, _page.CommitArguments(Command, value.Trim().TrimEnd('%')));
            }
        }

        public IReadOnlyList<ChoiceOption>? Options { get; }
        private readonly int? _selected;

        public int? Selected
        {
            get => _selected;
            set
            {
                if (value is int v && v != _selected)
                    _page.ChangeValue(Value, v.ToString());
            }
        }

        public EntityType? RefType { get; }
        public IReadOnlyList<ReferenceItem>? Candidates { get; }
        private readonly int? _refId;

        public int? RefId
        {
            get => _refId;
            set
            {
                if (value is int v && v != 0 && v != _refId)
                    _page.ChangeValue(Value, v.ToString());
            }
        }

        public ICommand? OpenCommand { get; }

        public IReadOnlyList<MaskOption>? MaskOptions { get; }
        public string MaskWords => _number is long m ? EventCommands.Words(Arg, m) : "";

        internal void CommitMask()
        {
            long mask = MaskOptions!.Where(o => o.IsOn).Aggregate(0L, (m, o) => m | o.Bit);
            if (mask != _number)
                _page.ChangeValue(Value, mask.ToString());
        }

        /// <summary>The events and spells on the other end of a code, variable, enchantment or event id; a delay's next event.</summary>
        public ObservableCollection<LinkChip> Links { get; } = new ObservableCollection<LinkChip>();
        public bool HasLinks => Links.Count > 0;

        /// <summary>A warning on the line (a code no event sets).</summary>
        public string Note { get; private set; } = "";
        public bool HasNote => Note.Length > 0;

        /// <summary>What a special value means (code 0).</summary>
        public string Info { get; private set; } = "";
        public bool HasInfo => Info.Length > 0;

        private readonly string? _missing;

        private void AddLinks(EventGraph graph)
        {
            if (_missing != null)
                Note = _missing;
            var e = _page.Entity;
            var own = _page.Session.Editor.OwnEntity(e) ?? e;
            string Title(IDEntity x) => x.Kind == EntityType.EVENT ? EventInfo.Title(graph.LinesOf(x)) : SpellName(x);
            foreach (var l in graph.To(own).Where(l => ReferenceEquals(l.Checker, Line)))
                Links.Add(new LinkChip(l.Kind switch
                {
                    EventLinkKind.Enchantment => "spell",
                    EventLinkKind.SpellEvent => "cast by",
                    EventLinkKind.Variable => "changed by",
                    EventLinkKind.Choice => "offered by",
                    _ => "set by",
                }, Title(l.From), () => _page.Session.Navigate(l.From), l.Label));
            foreach (var l in graph.From(own).Where(l => ReferenceEquals(l.Setter, Line)))
                Links.Add(new LinkChip(l.Kind switch
                {
                    EventLinkKind.Delay => "next event",
                    EventLinkKind.DelaySkip => "or",
                    EventLinkKind.CodeExcludes => "blocks",
                    EventLinkKind.Choice => "answered by",
                    _ => "checked by",
                }, Title(l.To), () => _page.Session.Navigate(l.To), l.Label));
            if (EventInfo.CodeCheckers.Contains(Command) && _number is long need && need != 0 && Links.Count == 0)
                Note = EventInfo.IsModCode(need) ? "no event sets this code" : "";
            if (EventInfo.CodeSetters.Contains(Command) && _number == 0)
                Info = "0: no code, ends the chain here";
            if (Command == Command.REQ_CODE && _number == 0)
                Info = "0: no code, so no other chain is going on here (the manual asks this of events that set a code)";
            if (EventInfo.EnchantmentCheckers.Contains(Command) && Links.Count == 0 && _number is long ench)
                Note = graph.SpellsOfEnchantment(ench).Count == 0 ? "no spell makes this enchantment" : "";
            if (!CanEdit && Info.Length == 0)
                Info = "the game's line: a changed value would be added to it, not replace it";
        }

        private string SpellName(IDEntity spell) => _page.NameOf(EntityType.SPELL, spell.ID) is { Length: > 0 } s ? $"{s} #{spell.ID}" : $"spell #{spell.ID}";

        public ICommand RemoveCommand => new RelayCommand(() => _page.RemoveValue(Value));

        // set by the panel: the lines it moves past
        internal Property? Previous { get; set; }
        internal Property? Next { get; set; }
        public bool CanMoveUp => Previous != null && !IsInherited;
        public bool CanMoveDown => Next != null && !IsInherited;
        public ICommand MoveUpCommand => new RelayCommand(() => { if (Previous != null) _page.MoveLine(Line, Previous, after: false); });
        public ICommand MoveDownCommand => new RelayCommand(() => { if (Next != null) _page.MoveLine(Line, Next, after: true); });
    }

    /// <summary>A group of an event's lines, with its title (a requirement group).</summary>
    public sealed class EventLineGroup
    {
        public EventLineGroup(string? title, IEnumerable<EventLineRow> rows)
        {
            Title = title;
            Rows = rows.ToList();
        }

        public string? Title { get; }
        public IReadOnlyList<EventLineRow> Rows { get; }
    }

    /// <summary>
    /// An event's requirements ("when", by group) or effects ("then", in line order, which matters:
    /// #tempunits, #assowner, #cleartarg act on the lines after them), and adding one.
    /// </summary>
    public sealed class EventLinesPanel
    {
        private readonly EntityPageViewModel _page;

        public EventLinesPanel(EntityPageViewModel page, EventGraph graph, IReadOnlyList<ResolvedValue> values, bool requirements)
        {
            _page = page;
            IsRequirements = requirements;
            Title = requirements ? "WHEN (all of these)" : "THEN (in this order)";
            bool Mine(ResolvedValue v)
            {
                if (EventPageViewModel.HeaderCommands.Contains(v.Command))
                    return false;
                var def = EventCommands.Get(EventPageViewModel.Name(v.Command));
                return def != null ? def.IsRequirement == requirements : requirements == EventPageViewModel.Name(v.Command).StartsWith("#req_");
            }
            var rows = values.Where(Mine).Select(v => new EventLineRow(page, graph, v)).ToList();
            if (requirements)
            {
                var order = EventCommands.Groups.Select((g, i) => (g.Id, i)).ToDictionary(x => x.Id, x => x.i);
                foreach (var g in rows.GroupBy(r => r.Def?.Group ?? "").OrderBy(g => order.TryGetValue(g.Key, out var i) ? i : 99))
                    Groups.Add(new EventLineGroup(EventCommands.Groups.FirstOrDefault(x => x.Id == g.Key)?.Title ?? "Other", g));
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].Previous = i > 0 ? rows[i - 1].Line : null;
                    rows[i].Next = i + 1 < rows.Count ? rows[i + 1].Line : null;
                }
                Groups.Add(new EventLineGroup(null, rows));
            }
            Count = rows.Count;
            var have = new HashSet<Command>(values.Select(v => v.Command));
            var groupTitles = EventCommands.Groups.ToDictionary(g => g.Id, g => g.Title);
            Addable = EventCommands.All
                .Where(d => d.IsRequirement == requirements && d.Name != "msg" && CommandsMap.TryGetCommand("#" + d.Name, out var c)
                            && page.Entity.GetPropertyMap().ContainsKey(c) && !EventPageViewModel.HeaderCommands.Contains(c)
                            && (!have.Contains(c) || Dom5Edit.Resolve.GameRules.IsRepeatable(EntityType.EVENT, c)))
                .OrderBy(d => EventCommands.Groups.ToList().FindIndex(g => g.Id == d.Group)).ThenBy(d => d.Name)
                .Select(d =>
                {
                    CommandsMap.TryGetCommand("#" + d.Name, out var c);
                    var sentence = d.Arg == "bool" ? d.Text1 ?? d.Name : d.Text.Replace("{v}", "…");
                    return new ReferenceItem
                    {
                        ID = (int)c, DisplayName = $"{(groupTitles.TryGetValue(d.Group, out var t) ? t : d.Group)}: {sentence}  (#{d.Name})",
                        ShowId = false, Tooltip = CommandHints.Tooltip(EntityType.EVENT, c),
                    };
                }).ToList();
        }

        public bool IsRequirements { get; }
        public string Title { get; }
        public int Count { get; }
        public ObservableCollection<EventLineGroup> Groups { get; } = new ObservableCollection<EventLineGroup>();
        public IReadOnlyList<ReferenceItem> Addable { get; }
        public string AddPlaceholder => IsRequirements ? "+ Add a requirement..." : "+ Add an effect...";
        public bool IsEmpty => Count == 0;
        public string EmptyText => IsRequirements ? "No requirements: it can happen anywhere its rarity allows." : "No effects yet.";

        /// <summary>The add picker's choice: adds the command with a starting value (an entity to pick for references: the first of its kind).</summary>
        public int? AddPick
        {
            get => null;
            set
            {
                if (value is not int c || !Enum.IsDefined(typeof(Command), c))
                    return;
                var command = (Command)c;
                var def = EventCommands.Get(EventPageViewModel.Name(command));
                _page.AddValue(command, DefaultArguments(def));
            }
        }

        private string DefaultArguments(EventCommands.Def? def) => def?.Arg switch
        {
            "none" => "",
            "bool" => "1",
            "percent" => "50",
            "code" => def.Name.StartsWith("req_") ? "0" : _page.Session.Events.NextFreeCode().ToString(),
            "monster" => "1",
            "nation" => "2",
            "site" => "1",
            "item" => "1",
            "era" => "1",
            "header" => "1",
            "ordermask" => "12",
            "affmask" => "1",
            "terrainmask" => "128",
            null => "1",
            _ when EventCommands.Values(def.Arg).Count > 0 => EventCommands.Values(def.Arg)[0].Value.ToString(),
            _ => "1",
        };
    }

    /// <summary>
    /// How the event chains: the events and spells that lead to it, the events it leads to, the
    /// chain it's part of, its problems; and making follow-ups (docs/EVENT_EDITOR.md, E-5).
    /// </summary>
    public sealed class EventChainPanel
    {
        private readonly EntityPageViewModel _page;
        private readonly EventGraph _graph;
        private readonly IDEntity _event;

        public EventChainPanel(EntityPageViewModel page, EventGraph graph)
        {
            _page = page;
            _graph = graph;
            _event = page.Session.Editor.OwnEntity(page.Entity) ?? page.Entity;
            string Title(IDEntity x) => x.Kind == EntityType.EVENT ? EventInfo.Title(graph.LinesOf(x))
                : page.NameOf(EntityType.SPELL, x.ID) is { Length: > 0 } s ? $"{s} (spell #{x.ID})" : $"spell #{x.ID}";
            foreach (var l in graph.To(_event))
            {
                var chip = new LinkChip(l.Label, Title(l.From), () => page.Session.Navigate(l.From));
                if (l.IsEventToEvent)
                    ComesFrom.Add(chip);
                else
                    StartedBy.Add(chip);
            }
            foreach (var l in graph.From(_event))
                LeadsTo.Add(new LinkChip(l.Label, Title(l.To), () => page.Session.Navigate(l.To)));
            foreach (var p in graph.ProblemsOf(_event))
                Problems.Add(p);
            var chain = graph.ChainOf(_event);
            ChainText = chain == null ? "Not chained with other events." : $"Part of a chain of {chain.Events.Count} events" +
                (chain.Triggers.Count > 0 ? $", started by {chain.Triggers.Count} spell{(chain.Triggers.Count == 1 ? "" : "s")}" : "") + ".";
            int index = graph.IndexOf(_event);
            Position = index >= 0 ? $"Event {index + 1} of {graph.ModEvents.Count} in the file" : _event.ID >= 0 ? $"The game's event {_event.ID}" : "";
            BuildMap(chain, Title);
            FollowUpCommand = new RelayCommand(MakeFollowUp);
            // a game event's #delay runs the game's next event (N + 1): a new one can't follow it
            DelayedCommand = new RelayCommand(MakeDelayed, () => !_page.Entity.Selected);
            ChoiceCommand = new RelayCommand(MakeChoice);
        }

        public string ChainText { get; }
        public string Position { get; }

        // ---- the chain map ----

        public IReadOnlyList<ChainNode> MapNodes { get; private set; } = Array.Empty<ChainNode>();
        public IReadOnlyList<ChainEdge> MapEdges { get; private set; } = Array.Empty<ChainEdge>();
        public bool HasMap => MapNodes.Count > 1;
        public bool MapOpen { get; private set; }
        public string MapTitle { get; private set; } = "MAP";

        /// <summary>
        /// The chain as cards and arrows; for a big chain, the events within two links of this one.
        /// Spells that start its events are cards too.
        /// </summary>
        private void BuildMap(EventChain? chain, Func<IDEntity, string> title)
        {
            var events = chain?.Events.ToList() ?? new List<IDEntity> { _event };
            const int max = 40;
            if (events.Count > max)
            {
                var near = new HashSet<IDEntity>(ReferenceEqualityComparer.Instance) { _event };
                for (int step = 0; step < 2; step++)
                    foreach (var e in near.ToList())
                        foreach (var l in _graph.From(e).Concat(_graph.To(e)).Where(l => l.IsEventToEvent))
                        {
                            near.Add(l.From);
                            near.Add(l.To);
                        }
                events = events.Where(near.Contains).ToList();
                MapTitle = $"MAP (the {events.Count} events within two links of this one, of {chain!.Events.Count})";
            }
            else
                MapTitle = $"MAP ({events.Count} events)";
            var set = new HashSet<IDEntity>(events, ReferenceEqualityComparer.Instance);
            var nodes = new Dictionary<IDEntity, ChainNode>(ReferenceEqualityComparer.Instance);
            ChainNode Node(IDEntity e)
            {
                if (nodes.TryGetValue(e, out var n))
                    return n;
                bool spell = e.Kind != EntityType.EVENT;
                int at = spell ? -1 : _graph.IndexOf(e);
                // the mod's events by their place in the file, the game's by number (after the mod's)
                var where = e.Selected ? $"game event {e.ID}" : $"event {at + 1}";
                var sub = spell ? $"spell #{e.ID}" : $"{EventInfo.RarityName(EventInfo.Rarity(_graph.LinesOf(e)))} · {where}";
                return nodes[e] = new ChainNode(e, title(e), sub, ReferenceEquals(e, _event), spell, () => _page.Session.Navigate(e))
                    { Order = at >= 0 || spell ? at : _graph.ModEvents.Count + e.ID };
            }
            foreach (var e in events)
                Node(e);
            var edges = new List<ChainEdge>();
            foreach (var group in _graph.Links.Where(l => set.Contains(l.To) && (set.Contains(l.From) || !l.IsEventToEvent))
                         .GroupBy(l => (l.From, l.To, l.Kind)))
            {
                var l = group.First();
                string kind = l.Kind switch
                {
                    EventLinkKind.Code => "code", EventLinkKind.CodeExcludes => "excludes", EventLinkKind.Delay => "delay",
                    EventLinkKind.DelaySkip => "skip", EventLinkKind.Variable => "variable", EventLinkKind.Choice => "choice", _ => "spell",
                };
                edges.Add(new ChainEdge(Node(l.From), Node(l.To), string.Join(", ", group.Select(x => x.Label).Distinct()), kind));
            }
            MapNodes = nodes.Values.ToList();
            MapEdges = edges;
            MapOpen = events.Count <= 25;
        }
        public ObservableCollection<LinkChip> ComesFrom { get; } = new ObservableCollection<LinkChip>();
        public ObservableCollection<LinkChip> LeadsTo { get; } = new ObservableCollection<LinkChip>();
        public ObservableCollection<LinkChip> StartedBy { get; } = new ObservableCollection<LinkChip>();
        public ObservableCollection<EventProblem> Problems { get; } = new ObservableCollection<EventProblem>();
        public bool HasComesFrom => ComesFrom.Count > 0;
        public bool HasLeadsTo => LeadsTo.Count > 0;
        public bool HasStartedBy => StartedBy.Count > 0;
        public bool HasProblems => Problems.Count > 0;

        public ICommand FollowUpCommand { get; }
        public ICommand DelayedCommand { get; }
        public ICommand ChoiceCommand { get; }

        private IReadOnlyList<Property> Lines => _graph.LinesOf(_event);

        /// <summary>The code this event sets (one it already sets, else the next free one, added here with #req_code 0 if it requires none).</summary>
        private long ChainCode(Dom5Edit.Editing.Transaction tx, IDEntity entity)
        {
            var existing = Lines.Where(p => EventInfo.CodeSetters.Contains(p.Command)).Select(EventInfo.Number).FirstOrDefault(n => n is long c && c != 0);
            if (existing is long code)
                return code;
            code = _graph.NextFreeCode();
            if (!Lines.Any(p => EventInfo.CodeCheckers.Contains(p.Command)))
                tx.Add(entity, Command.REQ_CODE, "0");
            tx.Add(entity, Command.CODE, code.ToString());
            return code;
        }

        /// <summary>A new event that happens when this one has set its code: #req_code, and #code 0 to end the chain.</summary>
        private void MakeFollowUp()
        {
            IDEntity? made = null;
            var entity = _page.Entity;
            _page.EditRun("Add a follow-up event", (ed, tx) =>
            {
                long code = ChainCode(tx, entity);
                // (at the end of the file: placed next to this event it could become a #delay's next event)
                made = tx.Create(EntityType.EVENT, null);
                tx.Add(made, Command.RARITY, "0");
                tx.Add(made, Command.REQ_CODE, code.ToString());
                tx.Add(made, Command.MSG, "\"What happens next.\"");
                tx.Add(made, Command.CODE, "0");
            });
            if (made != null && _page.Error == null)
                _page.Session.Navigate(made);
        }

        /// <summary>A new event placed right after this one, which #delay makes happen a turn later.</summary>
        private void MakeDelayed()
        {
            if (Lines.Any(p => EventInfo.Delays.Contains(p.Command)))
            {
                _page.EditRun("Add a delayed follow-up", (ed, tx) => throw new Dom5Edit.Editing.EditException(
                    "This event already has a #delay: its follow-up is the next event in the file"));
                return;
            }
            IDEntity? made = null;
            var entity = _page.Entity;
            _page.EditRun("Add a delayed follow-up", (ed, tx) =>
            {
                tx.Add(entity, Command.DELAY, "1");
                made = tx.Create(EntityType.EVENT, null, placeAfter: entity);
                tx.Add(made, Command.RARITY, "5");
                tx.Add(made, Command.MSG, "\"A turn later...\"");
            });
            if (made != null && _page.Error == null)
                _page.Session.Navigate(made);
        }

        /// <summary>Offers Accept and Decline (#order) with a code; one new event for each answer.</summary>
        private void MakeChoice()
        {
            IDEntity? accept = null;
            var entity = _page.Entity;
            _page.EditRun("Add a player choice", (ed, tx) =>
            {
                long code = ChainCode(tx, entity);
                tx.Add(entity, Command.ORDER, "12");
                IDEntity Answer(long order, string text)
                {
                    var e = tx.Create(EntityType.EVENT, null);
                    tx.Add(e, Command.RARITY, "0");
                    tx.Add(e, Command.REQ_CODE, code.ToString());
                    tx.Add(e, Command.REQ_TARGORDER, order.ToString());
                    tx.Add(e, Command.MSG, $"\"{text}\"");
                    tx.Add(e, Command.CODE, "0");
                    return e;
                }
                Answer(103, "You declined.");
                accept = Answer(102, "You accepted.");
            });
            if (accept != null && _page.Error == null)
                _page.Session.Navigate(accept);
        }
    }
}
