using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.UI.ViewModels;
using Dom5Editor.UI.Views;

namespace Dom5Editor.UI
{
    /// <summary>
    /// The snapshot harness's steps that work on the view models alone, shared by the WPF harness
    /// (Dom5Editor/UI/Snapshot.cs) and the Avalonia one (Dom5Editor.Avalonia/Snapshot.cs): the same
    /// command line does the same and logs the same in both. Steps that look at the controls on
    /// screen (--png, --view, --report, --tooltips, ...) stay in each harness; their list is there.
    ///   --mod FILE.dm            load a mod (otherwise a new, empty mod)
    ///   --select TYPE ID         select an entity (monster, weapon, armor, spell, item, site, nation, ...)
    ///   --select-name TYPE NAME  the first entity of the type whose list name contains NAME
    ///   --badge COMMAND VALUE    set the value of the selected entity's badge for COMMAND, as typing it does
    ///   --set COMMAND ARGS       set a value (as a badge or panel does); --add for one more entry
    ///   --remove COMMAND [ARGS]  remove a value (the first, or the one with those arguments)
    ///   --new TYPE               make a new entity (the list's "+ New") and select it
    ///   --delete                 delete the selected entity (the list's "Delete")
    ///   --field LABEL VALUE      set a panel field (choice by name or number, number, reference by ID)
    ///   --stat LABEL VALUE       type in a stat box (a leadership: its class by name, "Good (100)")
    ///   --reset LABEL            a field's reset button (back to what it inherits)
    ///   --add-path F             the magic panel's add-path button; --add-random FAWE 50 adds a random path;
    ///   --toggle-random N D      toggles path D on the Nth random path
    ///   --mod-info FIELD VALUE   a Mod Info box (modname, description, version, domversion, icon; iconfile FILE: its Pick...)
    ///   --copy ID / --sprite-from ID   the copy picker (#copystats, ...) / the sprite picker (#copyspr); 0: none
    ///   --add-clear COMMAND      the clears picker (#clearrec, #clearweapons, ...)
    ///   --flag LABEL on|off      a flags panel checkbox; --path-level F N a magic path's level box
    ///   --panel-new TITLE        a list panel's "+ New weapon/armor"; --panel-add TITLE ID its add box;
    ///   --panel-remove TITLE N   its Nth row's remove button; --copy-edit TITLE N its "Copy & edit"
    ///   --add-badge COMMAND / --ref COMMAND ID   a section's add box / pick in a reference badge
    ///   --event-rarity NAME|N, --event-owner N [NATION], --event-msg TEXT   an event's header and message boxes
    ///   --event-add COMMAND, --event-set COMMAND VALUE, --event-move COMMAND up|down,
    ///   --event-chain followup|delayed|choice, --event-lines   the event page
    ///   --jump TEXT / --back     the "Go to" box / back; --hide-vanilla, --search TYPE TEXT, --facet TYPE NAME: the list
    ///   --file / --file-edit FIND REPLACE   the "in the file" box; --sprite COMMAND FILE a header image
    ///   --page-command NAME      run one of the page's commands (CopyToNewCommand, ...)
    ///   --dump                   log the selected entity's values and where each comes from
    ///   --dump-mod               --dump every entity the mod makes or changes (to compare a mod as made and as reloaded)
    ///   --derived                log what the game makes of its stats (the bracketed values, "in game" notes, table rows)
    ///   --tooltip COMMAND / --used-by / --validate / --icons   log a badge's hint / the "used by" list / Validate / the icons
    ///   --sweep N                open up to N of the mod's entities of every type (0: all); --sweep-vanilla N
    ///                            the same for the game's own entities
    ///   --undo / --redo          undo or redo the last edit
    ///   --status                 log the status bar's text
    ///   --save FILE.dm           save the mod (the editor's Save)
    ///   --time-refresh, --pause SECONDS   timing and memory checks
    /// </summary>
    public sealed class SnapshotSteps
    {
        private readonly MainWindowViewModel _vm;
        private readonly Action<string> _log;
        private readonly Action _pump;

        /// <param name="log">writes a line to the harness's log</param>
        /// <param name="pump">lets bindings, layout and rendering catch up (the harness's dispatcher)</param>
        public SnapshotSteps(MainWindowViewModel vm, Action<string> log, Action pump)
        {
            _vm = vm;
            _log = log;
            _pump = pump;
        }

        /// <summary>Called with a file a step is about to write (the harness puts its log next to the first).</summary>
        public Action<string>? Output { get; set; }

        private void Log(string message) => _log(message);

        private void Pump() => _pump();

        /// <summary>
        /// Runs the step at args[i] if it's one of these (i is left on its last argument, as the
        /// harnesses' own steps do); false if it isn't, for the harness to run or reject.
        /// </summary>
        public bool TryRun(string[] args, ref int i)
        {
            var vm = _vm;
            switch (args[i])
            {
                case "--mod":
                    if (vm.HasMod && i > 1) throw new ArgumentException("--mod must come first");
                    vm.LoadMod(Path.GetFullPath(args[++i]));
                    Log($"loaded {args[i]}");
                    break;
                case "--select":
                    var type = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                    int id = int.Parse(args[++i]);
                    vm.NavigateToEntity(type, id);
                    Log($"selected {type} {id}: {Selected(vm)?.DisplayName ?? "(not found)"}");
                    break;
                case "--select-name":
                {
                    // --select-name TYPE NAME: the first entity of the type whose list name contains NAME
                    var t = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                    var name = args[++i];
                    var tab = vm.TabOf(t) ?? throw new ArgumentException("no tab for " + t);
                    vm.SelectedTab = tab;
                    tab.SelectedItem = tab.Items.FirstOrDefault(x => x.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase))
                                       ?? throw new InvalidOperationException($"no {t} named {name}");
                    Log($"selected {t} {tab.SelectedItem.DisplayName} #{tab.SelectedItem.ID}");
                    break;
                }
                case "--badge":
                    SetBadge(vm, args[++i], args[++i]);
                    break;
                case "--undo":
                    vm.Undo();
                    Log("undo");
                    break;
                case "--redo":
                    vm.Redo();
                    Log("redo");
                    break;
                case "--new":
                {
                    var t = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                    var tab = vm.TabOf(t) ?? throw new ArgumentException("no tab for " + t);
                    vm.SelectedTab = tab;
                    tab.NewCommand.Execute(null);
                    Log($"new {t}: {tab.SelectedItem?.DisplayName} #{tab.SelectedItem?.ID}{(tab.LastError != null ? " error: " + tab.LastError : "")}");
                    break;
                }
                case "--delete":
                {
                    var tab = vm.SelectedTab as EntityTypeTab ?? throw new InvalidOperationException("no entity tab");
                    var item = tab.SelectedItem;
                    tab.DeleteCommand.Execute(item);
                    Log($"delete {item?.DisplayName} #{item?.ID}: {vm.StatusMessage}");
                    break;
                }
                case "--set":
                case "--add":
                {
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var a = args[++i];
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    if (args[i - 2] == "--set") page.SetValue(c, a); else page.AddValue(c, a);
                    Log($"{args[i - 2].TrimStart('-')} {args[i - 1]} {a}{(page.Error != null ? " error: " + page.Error : "")} ({watch.ElapsedMilliseconds} ms)");
                    break;
                }
                case "--remove":
                {
                    // --remove COMMAND [ARGS]: the first value of the command (with those arguments)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    string? a = i + 1 < args.Length && !args[i + 1].StartsWith("--") ? args[++i] : null;
                    var v = page.Resolved.GetAll(c).FirstOrDefault(x => a == null || x.Arguments.Trim('"') == a.Trim('"'))
                            ?? throw new InvalidOperationException($"no {args[i]} to remove");
                    page.RemoveValue(v);
                    Log($"remove {v.Property.ToExportString()}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--field":
                {
                    // --field LABEL VALUE: set a panel field as picking or typing in it does
                    // (a choice by its option name or number, a number, a reference by ID)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var label = args[++i];
                    var value = args[++i];
                    var field = AllFields(page).FirstOrDefault(f => f.Label == label)
                                ?? throw new InvalidOperationException("no field " + label);
                    switch (field)
                    {
                        case ChoiceField c:
                            c.Selected = c.Options.FirstOrDefault(o => o.Name == value)?.Value ?? int.Parse(value);
                            break;
                        case CommandChoiceField cc:
                            cc.Selected = cc.Options.FirstOrDefault(o => o.Name == value)?.Value ?? int.Parse(value);
                            break;
                        case NumberField n:
                            n.Text = value;
                            break;
                        case RefField r:
                            r.SelectedId = int.Parse(value);
                            break;
                    }
                    Log($"field {label} = {value}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--time-refresh":
                {
                    // how long the selected page takes to rebuild (resolver cached, then a fresh replay)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    page.Refresh();
                    long cached = watch.ElapsedMilliseconds;
                    vm.Session!.Editor.Resolver.Invalidate();
                    watch.Restart();
                    vm.Session.Resolve(page.Entity);
                    long replay = watch.ElapsedMilliseconds;
                    watch.Restart();
                    page.Refresh();
                    long after = watch.ElapsedMilliseconds;
                    watch.Restart();
                    var plan = new Dom5Edit.SavePlan(vm.Session.Mod);
                    long planMs = watch.ElapsedMilliseconds;
                    int lines = plan.Blocks().Sum(b => b.Lines.Count);
                    int edited = vm.Session.Mod.Database.Values.SelectMany(x => x.GetFullList()).Count(e => e.EditedSinceLoadPublic);
                    Log($"refresh {cached} ms (resolver cached); replay {replay} ms; refresh after replay {after} ms; plan {planMs} ms + blocks {watch.ElapsedMilliseconds - planMs} ms, {lines} lines, {edited} entities marked edited");
                    break;
                }
                case "--jump":
                {
                    // --jump TEXT: the "Go to" box, picking the first entity whose name contains TEXT
                    var text = args[++i];
                    vm.EnsureJumpTargets();
                    var item = vm.JumpTargets!.FirstOrDefault(r => r.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                               ?? throw new InvalidOperationException("nothing named " + text);
                    vm.JumpTo(item);
                    Log($"jump {text}: {item.DisplayName} #{item.ID} ({vm.JumpTargets!.Count} entities to go to)");
                    break;
                }
                case "--hide-vanilla":
                {
                    var tab = vm.SelectedTab as EntityTypeTab ?? throw new InvalidOperationException("no entity tab");
                    tab.ShowVanilla = false;
                    Log($"hide vanilla in {tab.Title}");
                    break;
                }
                case "--back":
                    vm.GoBack();
                    Log($"back: {Selected(vm)?.DisplayName}");
                    break;
                case "--copy-edit":
                {
                    // --copy-edit TITLE N: a list panel's "Copy & edit" on its Nth row (from 1)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var title = args[++i];
                    int n = int.Parse(args[++i]);
                    var panel = page.Panels.OfType<ReferenceListPanel>().First(p => p.Title == title);
                    var row = panel.Rows[n - 1];
                    panel.CopyEditCommand.Execute(row);
                    Log($"copy & edit {title} {row.Text}: now on {Selected(vm)?.DisplayName} #{Selected(vm)?.ID}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--add-clear":
                {
                    // --add-clear COMMAND: the CLEARS picker at the top of the page (#clearrec, #clearweapons, ...)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    if (!page.AddableClears.Any(x => x.ID == (int)c))
                        throw new InvalidOperationException("the clears picker doesn't offer " + args[i]);
                    page.AddClearPick = (int)c;
                    Log($"add clear {args[i]}{(page.Error != null ? " error: " + page.Error : "")}: clears {string.Join(", ", Selected(vm)!.Structure.Select(s => s.Text))}");
                    break;
                }
                case "--mod-info":
                {
                    // --mod-info FIELD VALUE: a box on the Mod Info tab (modname, description, version, domversion, icon)
                    var info = vm.Tabs.OfType<ModInfoViewModel>().Single();
                    var field = args[++i];
                    var value = args[++i];
                    switch (field)
                    {
                        case "modname": info.ModName = value; break;
                        case "description": info.ModDescription = value; break;
                        case "version": info.ModVersion = value; break;
                        case "domversion": info.ModDomVersion = value; break;
                        case "icon": info.ModIcon = value; break;
                        case "iconfile": info.SetIcon(Path.GetFullPath(value)); break; // the icon's "Pick..."
                        default: throw new ArgumentException("no Mod Info field " + field);
                    }
                    Log($"mod info {field} = {value}: {vm.StatusMessage}");
                    break;
                }
                case "--copy":
                {
                    // --copy ID: the copy picker at the top of the page (#copystats, #copyweapon, ...; 0: none)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    int source = int.Parse(args[++i]);
                    page.CopySourceId = source == 0 ? null : source;
                    Log($"copy {source}: copies {Selected(vm)?.CopySourceName}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--sprite-from":
                {
                    // --sprite-from ID: the "sprite from" picker next to the copy picker (#copyspr; 0: none)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    int source = int.Parse(args[++i]);
                    page.SpriteCopyId = source == 0 ? null : source;
                    var now = Selected(vm)!;
                    Log($"sprite from {source}{(page.Error != null ? " error: " + page.Error : "")}: slots {string.Join(", ", now.SpriteSlots.Select(x => $"{x.Label} {(x.HasImage ? $"{x.Image!.Width}x{x.Image.Height}" : "none")}"))}");
                    break;
                }
                case "--flag":
                {
                    // --flag LABEL on|off: a checkbox in a flags panel (a weapon's qualities, a form's flags)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var label = args[++i];
                    bool on = args[++i] == "on";
                    var flag = page.Panels.OfType<FlagsPanel>().SelectMany(p => p.Groups).SelectMany(g => g.Flags).FirstOrDefault(f => f.Label == label)
                               ?? throw new InvalidOperationException("no flag " + label);
                    flag.IsOn = on;
                    Log($"flag {label} ({EntityPageViewModel.CommandName(flag.Command)}) {(on ? "on" : "off")}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--path-level":
                {
                    // --path-level F N: type a level in a magic path's box (0 removes the path)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var letter = args[++i];
                    var row = page.Panels.OfType<MagicPanel>().Single().Paths.First(r => r.Icon == "path:" + letter);
                    row.EditText = args[++i];
                    Log($"path {row.Text} level {args[i]}{(page.Error != null ? " error: " + page.Error : "")}: {Selected(vm)?.Panels.OfType<MagicPanel>().Single().Summary}");
                    break;
                }
                case "--panel-new":
                {
                    // --panel-new TITLE: a list panel's "+ New weapon" / "+ New armor" (the new one is opened)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var title = args[++i];
                    var panel = page.Panels.OfType<ReferenceListPanel>().FirstOrDefault(p => p.Title == title && p.CanMakeNew)
                                ?? throw new InvalidOperationException("no list panel with a New button titled " + title);
                    panel.NewCommand.Execute(null);
                    Log($"{panel.NewLabel} in {title}: now on {Selected(vm)?.DisplayName} #{Selected(vm)?.ID}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--panel-add":
                case "--panel-remove":
                {
                    // --panel-add TITLE ID: pick an entity in a list panel's add box; --panel-remove TITLE N: its Nth row's remove button
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    bool add = args[i] == "--panel-add";
                    var title = args[++i];
                    int n = int.Parse(args[++i]);
                    var panel = page.Panels.OfType<ReferenceListPanel>().FirstOrDefault(p => p.Title == title)
                                ?? throw new InvalidOperationException("no list panel titled " + title);
                    if (add)
                        panel.AddPick = n;
                    else
                        panel.RemoveCommand.Execute(panel.Rows[n - 1]);
                    var now = Selected(vm)!;
                    var rows = now.Panels.OfType<ReferenceListPanel>().FirstOrDefault(p => p.Title == title)?.Rows;
                    Log($"{(add ? "add" : "remove")} {title} {n}{(now.Error != null ? " error: " + now.Error : "")}: {string.Join(", ", rows?.Select(r => $"{r.Text} #{r.RefId}") ?? Array.Empty<string>())}");
                    break;
                }
                case "--add-badge":
                {
                    // --add-badge COMMAND: pick the command in a section's add box (a reference
                    // starts as an empty badge with its picker; pick with --ref)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var section = page.Sections.Append(page.Other!).FirstOrDefault(x => x.Available.Any(a => a.Command == c))
                                  ?? throw new InvalidOperationException("no add box offers " + args[i]);
                    section.AddCommand.Execute(section.Available.First(a => a.Command == c));
                    Log($"add badge {args[i]} (in {section.Id}){(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--ref":
                {
                    // --ref COMMAND ID: pick an entity in the (last) reference badge of the command
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    int pick = int.Parse(args[++i]);
                    var badge = page.Sections.Append(page.Other!).SelectMany(x => x.Badges).LastOrDefault(b => b.Command == c && b.IsReference)
                                ?? throw new InvalidOperationException("no reference badge for " + args[i - 1]);
                    var before = badge.ReferenceId;
                    badge.OnReferenceChanged(before, pick, "");
                    Log($"ref {args[i - 1]} {before} -> {pick}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--add-path":
                {
                    // --add-path F: the magic panel's path button
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var magic = page.Panels.OfType<MagicPanel>().Single();
                    var letter = args[++i];
                    var t = magic.AddablePaths.First(p => p.Letter == letter);
                    magic.AddPathCommand.Execute(t);
                    Log($"add path {t.Name}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--add-random":
                {
                    // --add-random FAWE 50: light the paths in the new random path row, set the chance, add it
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var magic = page.Panels.OfType<MagicPanel>().Single();
                    var letters = args[++i];
                    foreach (var t in magic.NewRandom.Toggles)
                        t.IsOn = letters.Contains(t.Letter);
                    magic.NewRandom.Chance = args[++i];
                    magic.AddRandomCommand.Execute(null);
                    Log($"add random {letters} {args[i]}%{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--toggle-random":
                {
                    // --toggle-random N D: toggle path D on the Nth random path row (from 1)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var row = page.Panels.OfType<MagicPanel>().Single().Random[int.Parse(args[++i]) - 1];
                    var letter = args[++i];
                    var t = row.Toggles.First(x => x.Letter == letter);
                    t.IsOn = !t.IsOn;
                    Log($"toggle {t.Name} on random path {args[i - 1]}: now {row.Letters}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--reset":
                {
                    // --reset LABEL: a field's reset button (back to what it inherits)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var label = args[++i];
                    var field = AllFields(page).FirstOrDefault(f => f.Label == label) ?? throw new InvalidOperationException("no field " + label);
                    Log($"reset {label} ({field.ResetTip}){(field.CanReset ? "" : ": nothing to reset")}");
                    field.ResetCommand.Execute(null);
                    if (page.Error != null) Log("   error: " + page.Error);
                    break;
                }
                case "--stat":
                {
                    // --stat LABEL VALUE: type in a stat box (a leadership's class by name: "Good (100)")
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var label = args[++i];
                    var value = args[++i];
                    var stats = page.Panels.OfType<StatsPanel>().Single();
                    var cells = stats.Columns.SelectMany(c => c.Cells).Concat(stats.Footer).ToList();
                    if (cells.OfType<LeaderField>().FirstOrDefault(l => l.Label == label) is LeaderField leader)
                        leader.Class.Selected = leader.Class.Options.First(o => o.Name == value).Value;
                    else
                        cells.OfType<NumberField>().First(f => f.Label == label).Text = value;
                    Log($"stat {label} = {value}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--event-rarity":
                case "--event-owner":
                case "--event-msg":
                {
                    // --event-rarity NAME|N: the "rolled as" box; --event-owner N [NATION]: the "owned by" box (0 independents,
                    // 1 the province's owner, 2 a random enemy, 3 a nation, picked next); --event-msg TEXT: the message box
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var step = args[i];
                    var value = args[++i];
                    switch (step)
                    {
                        case "--event-rarity":
                            var header = page.Panels.OfType<EventHeaderPanel>().Single();
                            header.Rarity = header.Rarities.FirstOrDefault(o => o.Name == value)?.Value ?? int.Parse(value);
                            break;
                        case "--event-owner":
                            page.Panels.OfType<EventHeaderPanel>().Single().Owner = int.Parse(value);
                            if (value == "3")
                                Selected(vm)!.Panels.OfType<EventHeaderPanel>().Single().OwnerNation = int.Parse(args[++i]);
                            break;
                        default:
                            page.Panels.OfType<EventMessagePanel>().Single().Text = value;
                            break;
                    }
                    Log($"{step.Substring(2)} {value}{(Selected(vm)?.Error is string e ? " error: " + e : "")}");
                    break;
                }
                case "--event-add":
                {
                    // --event-add COMMAND: the event page's add picker (requirement or effect)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var panel = page.Panels.OfType<EventLinesPanel>().First(p => p.Addable.Any(a => a.ID == (int)c));
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    panel.AddPick = (int)c;
                    Log($"event add {args[i]}{(page.Error != null ? " error: " + page.Error : "")} ({watch.ElapsedMilliseconds} ms)");
                    break;
                }
                case "--event-set":
                {
                    // --event-set COMMAND VALUE: a line's value, as its editor sets it (number, choice by name or number, 0/1, entity ID, mask)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var value = args[++i];
                    var row = EventRows(page).First(r => r.Command == c);
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    switch (row.Kind)
                    {
                        case "choice":
                        case "bool":
                            row.Selected = row.Options!.FirstOrDefault(o => o.Name == value)?.Value ?? int.Parse(value);
                            break;
                        case "ref":
                            row.RefId = int.Parse(value);
                            break;
                        case "mask":
                            foreach (var o in row.MaskOptions!)
                                if (((long.Parse(value) & o.Bit) == o.Bit) != o.IsOn)
                                    o.IsOn = !o.IsOn;
                            break;
                        default:
                            row.EditText = value;
                            break;
                    }
                    Log($"event set {args[i - 1]} = {value}{(page.Error != null ? " error: " + page.Error : "")} ({watch.ElapsedMilliseconds} ms)");
                    break;
                }
                case "--event-move":
                {
                    // --event-move COMMAND up|down: an effect's move button
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var up = args[++i] == "up";
                    var row = EventRows(page).First(r => r.Command == c);
                    (up ? row.MoveUpCommand : row.MoveDownCommand).Execute(null);
                    Log($"event move {args[i - 1]} {args[i]}{(page.Error != null ? " error: " + page.Error : "")}");
                    break;
                }
                case "--event-chain":
                {
                    // --event-chain followup|delayed|choice: the chain panel's buttons (the new event is selected)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var chain = page.Panels.OfType<EventChainPanel>().Single();
                    var what = args[++i];
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    (what == "followup" ? chain.FollowUpCommand : what == "delayed" ? chain.DelayedCommand : chain.ChoiceCommand).Execute(null);
                    Log($"event {what}: {(page.Error != null ? "error: " + page.Error : "now on " + Selected(vm)?.DisplayName)} ({watch.ElapsedMilliseconds} ms)");
                    break;
                }
                case "--event-lines":
                {
                    // log the selected event as the page reads it
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    foreach (var panel in page.Panels.OfType<EventLinesPanel>())
                    {
                        Log($"   {panel.Title}");
                        foreach (var g in panel.Groups)
                            foreach (var r in g.Rows)
                                Log($"     {(g.Title != null ? "[" + g.Title + "] " : "")}{r.Before}{(r.IsText ? r.EditText + r.Suffix : r.IsChoice || r.IsBool ? r.Options!.FirstOrDefault(o => o.Value == r.Selected)?.Name : r.IsRef ? "#" + r.RefId : r.IsMask ? r.MaskWords : "")}{r.After}" +
                                    (r.Links.Count > 0 ? $"  -> {string.Join("; ", r.Links.Select(l => l.Label + " " + l.Title))}" : "") + (r.HasNote ? $"  ({r.Note})" : ""));
                    }
                    var ch = page.Panels.OfType<EventChainPanel>().Single();
                    Log($"   chain: {ch.ChainText} {ch.Position}; problems: {string.Join("; ", ch.Problems)}");
                    break;
                }
                case "--facet":
                {
                    // --facet TYPE NAME: a list's own filter; logs how many rows it shows
                    var t = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                    var facet = args[++i];
                    var tab = vm.TabOf(t) ?? throw new ArgumentException("no tab for " + t);
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    int n = tab.Items.Count(x => x.InFacet(facet));
                    tab.Facet = facet;
                    Log($"facet {t} {facet}: {n} of {tab.Items.Count} ({watch.ElapsedMilliseconds} ms)");
                    break;
                }
                case "--file-edit":
                {
                    // --file-edit FIND REPLACE: the "in the file" box edited as text (\n for a new line), applied
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    string find = args[++i].Replace("\\n", "\n"), replace = args[++i].Replace("\\n", "\n");
                    page.ShowFile = true;
                    page.EditFileCommand.Execute(null);
                    if (!page.IsEditingFile)
                    {
                        Log($"file edit: can't edit ({page.FileError})");
                        break;
                    }
                    page.FileDraft = page.FileDraft.Replace(find, replace);
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    page.ApplyFileCommand.Execute(null);
                    var now = Selected(vm)!;
                    Log($"file edit ({watch.ElapsedMilliseconds} ms): {(page.FileError != null ? "error: " + page.FileError : "applied")}");
                    now.ShowFile = true;
                    foreach (var line in now.FileText.Split('\n'))
                        Log("   | " + line);
                    break;
                }
                case "--sprite":
                {
                    // --sprite COMMAND FILE: a header image set from a file (as picking it or dropping it does)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var file = args[++i];
                    page.SetImage(c, Path.GetFullPath(file));
                    var now = Selected(vm)!;
                    Log($"sprite {args[i - 1]}: {(now.Error != null ? "error: " + now.Error : now.Notice)}");
                    Log($"   slots: {string.Join(", ", now.SpriteSlots.Select(x => $"{x.Label} {(x.HasImage ? $"{x.Image!.Width}x{x.Image.Height}" : "none")}"))}");
                    break;
                }
                case "--page-command":
                {
                    // --page-command NAME: run one of the selected page's commands (CopyToNewCommand, EditFileCommand, ...)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var name = args[++i];
                    var command = page.GetType().GetProperty(name)?.GetValue(page) as System.Windows.Input.ICommand
                                  ?? throw new ArgumentException($"the page has no {name}");
                    command.Execute(null);
                    var now = Selected(vm)!;
                    Log($"{name}: {(page.Error != null ? "error: " + page.Error : "now on " + now.DisplayName)}");
                    break;
                }
                case "--icons":
                {
                    // where the game's icons come from: compiled in, and the install found (or not)
                    var hp = Sprites.GameArt.Icon("hp");
                    Log($"icons: {Sprites.GameArt.PackedCount} compiled in; install: {Sprites.GameArt.DataFolder ?? "not found"}; hp icon {(hp == null ? "missing" : $"{hp.Width}x{hp.Height}")}; {Dom5Edit.VanillaSprites.Status}");
                    break;
                }
                case "--pause":
                {
                    // --pause SECONDS: keep running (for a memory dump of the process)
                    Log($"pausing {args[i + 1]} s (process {Environment.ProcessId})");
                    var until = DateTime.Now.AddSeconds(int.Parse(args[++i]));
                    while (DateTime.Now < until)
                    {
                        Pump();
                        System.Threading.Thread.Sleep(100);
                    }
                    break;
                }
                case "--search":
                {
                    // --search TYPE TEXT: the list's search box; logs how many rows match and a few of them
                    var t = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                    var text = args[++i];
                    var tab = vm.TabOf(t) ?? throw new ArgumentException("no tab for " + t);
                    vm.SelectedTab = tab;
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var hits = tab.Items.Where(x => x.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase) || x.MatchesText(text)).ToList();
                    Log($"search {t} \"{text}\": {hits.Count} of {tab.Items.Count} ({watch.ElapsedMilliseconds} ms)");
                    foreach (var x in hits.Take(6))
                        Log($"   {x.SourceLabel} #{x.ID} {x.DisplayName}");
                    tab.SearchText = text;
                    Pump();
                    break;
                }
                case "--tooltip":
                {
                    // --tooltip COMMAND: log a badge's hover hint and value note
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    var c = CommandOf(args[++i]);
                    var badge = page.Sections.Append(page.Other!).SelectMany(x => x.Badges).FirstOrDefault(b => b.Command == c)
                                ?? throw new InvalidOperationException("no badge for " + args[i]);
                    Log($"tooltip {args[i]} (value {badge.Value}, note {badge.ValueNote ?? "-"}):");
                    foreach (var line in (badge.Tooltip ?? "").Split('\n'))
                        Log("   | " + line);
                    break;
                }
                case "--used-by":
                {
                    // the selected page's "used by" list
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    Log($"{page.UsedByTitle}");
                    foreach (var row in page.UsedBy)
                        Log($"   {row.TypeLabel} {row.Name} {row.IdText}  ({row.Via})");
                    break;
                }
                case "--validate":
                {
                    var result = vm.Validate();
                    Log($"validate: {vm.StatusMessage}");
                    if (result != null)
                        foreach (var issue in result.Issues.OrderBy(x => x.ToString().StartsWith("Error") ? 0 : 1).Take(60))
                            Log($"   {issue}");
                    break;
                }
                case "--file":
                {
                    // the page's "in the file" box
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    page.ShowFile = true;
                    foreach (var line in page.FileText.Split('\n'))
                        Log("   | " + line);
                    break;
                }
                case "--sweep":
                case "--sweep-vanilla":
                {
                    // --sweep N: open the page of up to N of the mod's entities of every type (all if
                    // N is 0); log any that fails, and the slowest. --sweep-vanilla N: the same for
                    // the game's entities the mod doesn't change
                    bool vanilla = args[i] == "--sweep-vanilla";
                    int limit = int.Parse(args[++i]);
                    int opened = 0, failed = 0;
                    long slowest = 0;
                    var pages = new List<WeakReference>(); // pages left alive after they're closed: a leak
                    string slowestName = "";
                    foreach (var tab in vm.Tabs.OfType<EntityTypeTab>())
                    {
                        vm.SelectedTab = tab;
                        var items = tab.Items.Where(x => vanilla ? x.IsVanilla && !x.IsModified : !x.IsVanilla || x.IsModified).ToList();
                        foreach (var item in limit > 0 ? items.Take(limit) : items)
                        {
                            var watch = System.Diagnostics.Stopwatch.StartNew();
                            try
                            {
                                tab.SelectedItem = item;
                                Pump();
                                _ = tab.Page?.Sections.Count;
                                if (tab.Page != null)
                                    pages.Add(new WeakReference(tab.Page));
                            }
                            catch (Exception ex)
                            {
                                failed++;
                                Log($"   FAIL {tab.Title} {item.DisplayName} #{item.ID}: {ex.GetType().Name}: {ex.Message}");
                            }
                            opened++;
                            if (watch.ElapsedMilliseconds > slowest)
                            {
                                slowest = watch.ElapsedMilliseconds;
                                slowestName = $"{tab.Title} {item.DisplayName} #{item.ID}";
                            }
                        }
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    vm.SelectedTab = vm.Tabs.OfType<EntityTypeTab>().First();
                    foreach (var tab in vm.Tabs.OfType<EntityTypeTab>())
                        tab.SelectedItem = null;
                    Pump();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    Log($"sweep: {opened} pages, {failed} failed; slowest {slowest} ms ({slowestName}); memory {GC.GetTotalMemory(true) / (1 << 20)} MB managed, {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / (1 << 20)} MB working set; {pages.Count(w => w.IsAlive)} of {pages.Count} pages still alive; {vm.Session!.ChangedListeners} session listeners");
                    break;
                }
                case "--derived":
                {
                    // --derived: log the stats with what the game makes of them (in brackets on the page), the
                    // "in game" notes, and the table rows (a unit's attack and damage with each weapon)
                    var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                    Log($"derived values of {page.DisplayName} #{page.ID}:");
                    foreach (var stats in page.Panels.OfType<StatsPanel>())
                    {
                        foreach (var f in stats.Columns.SelectMany(c => c.Cells).Concat(stats.Footer).OfType<NumberField>())
                            Log($"   {f.Label} [{f.Text}]{(f.HasDerived ? " " + f.Derived : "")}{(f.HasDerived ? "   | " + f.DerivedTip.Replace("\n", " | ") : "")}");
                        foreach (var n in stats.Notes)
                            Log($"   in game: {n.Text}   | {n.Tooltip.Replace("\n", " | ")}");
                    }
                    foreach (var list in page.Panels.OfType<ReferenceListPanel>().Where(p => p.IsTable))
                        foreach (var row in list.Rows)
                            Log($"   {list.Title} {row.Text}: {string.Join("  ", row.Cells.Select(c => c.Text))}");
                    if (page is MonsterPageViewModel)
                    {
                        // what working the values out costs (resolver cached, as on a page rebuild)
                        var session = vm.Session!;
                        Dom5Edit.Resolve.ResolvedEntity? Find(EntityType t, int id) =>
                            id > 0 && session.Mod.TryGet(t, id, null!, out var e) ? session.Resolve(e) : null;
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        const int runs = 200;
                        for (int k = 0; k < runs; k++)
                            Dom5Edit.Derived.UnitTotals.Compute(Dom5Edit.Derived.UnitStats.Of(page.Resolved, Find),
                                id => Find(EntityType.MONSTER, id) is { } m ? Dom5Edit.Derived.UnitStats.Of(m, Find) : null);
                        Log($"   (worked out in {watch.Elapsed.TotalMilliseconds * 1000 / runs:0} µs)");
                    }
                    break;
                }
                case "--dump":
                    Dump(vm, Selected(vm) ?? throw new InvalidOperationException("nothing selected"));
                    break;
                case "--dump-mod":
                {
                    // --dump-mod: --dump of every entity the mod makes or changes, type by type in list
                    // order (to compare a mod as made with the same mod saved and loaded again)
                    foreach (var tab in vm.Tabs.OfType<EntityTypeTab>())
                    {
                        var items = tab.Items.Where(x => !x.IsVanilla || x.IsModified).ToList();
                        if (items.Count == 0)
                            continue;
                        vm.SelectedTab = tab;
                        foreach (var item in items)
                        {
                            tab.SelectedItem = item;
                            Pump();
                            Dump(vm, tab.Page!);
                            if (tab.Page!.Panels.OfType<EventChainPanel>().SingleOrDefault() is EventChainPanel ch)
                                Log($"   chain: {ch.ChainText} {ch.Position}; problems: {string.Join("; ", ch.Problems)}");
                        }
                    }
                    break;
                }
                case "--status":
                    // the status bar's text (what the last action said; at the start, a note on the game's install)
                    Log($"status: {vm.StatusMessage}");
                    break;
                case "--save":
                    Output?.Invoke(args[i + 1]);
                    vm.SaveMod(Path.GetFullPath(args[++i]));
                    Log($"saved {args[i]}");
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>The check made on opening a mod, logged: its bar's text, how long it took, each section (--report).</summary>
        public void LogReport()
        {
            var vm = _vm;
            if (vm.Report == null)
                vm.CheckOnOpen();
            Log($"report bar: {vm.ReportSummary} ({vm.ReportMilliseconds} ms)");
            var report = vm.Report!;
            foreach (var s in report.Sections)
                Log($"   {s.Title}: {s.Count}" + string.Concat(s.Groups.Take(3).Select(g => $"\n      {g.Text} [{g.Lines.Count} line(s); first goes to {g.Lines.FirstOrDefault()?.Entity?.Kind} {g.Lines.FirstOrDefault()?.Entity?.ID}]")));
        }

        /// <summary>Logs an entity's values and where each comes from (--dump).</summary>
        private void Dump(MainWindowViewModel vm, EntityPageViewModel page)
        {
            Log($"== {page.Type} {page.DisplayName} #{page.ID} ({page.SourceLabel}; {(page.Entity.ParentMod == vm.Session!.Mod ? "the mod's entity" : "vanilla's entity")})");
            foreach (var line in page.Resolved.Structure)
                Log($"   structure {line.ToExportString()}");
            foreach (var v in page.Resolved.Values)
                Log($"   {v.Property.ToExportString(),-40} {page.SourceText(v)}");
            foreach (var (c, a) in page.Resolved.Assets)
                Log($"   asset {c}: {(a.ToExportString() ?? "").Substring(0, Math.Min(60, (a.ToExportString() ?? "").Length))}");
        }

        /// <summary>A command by its name, with or without the '#' ("hp", "#weapon").</summary>
        public static Command CommandOf(string text)
        {
            if (!text.StartsWith("#")) text = "#" + text;
            return CommandsMap.TryGetCommand(text, out var c) ? c : throw new ArgumentException("unknown command " + text);
        }

        /// <summary>The selected entity's page on the current tab.</summary>
        private static EntityPageViewModel? Selected(MainWindowViewModel vm) => vm.SelectedPage;

        private static IEnumerable<EventLineRow> EventRows(EntityPageViewModel page) =>
            page.Panels.OfType<EventLinesPanel>().SelectMany(p => p.Groups).SelectMany(g => g.Rows);

        /// <summary>Every panel field on the page: fields panels, the stat block (leadership as its class and bonus).</summary>
        private static IEnumerable<PanelField> AllFields(EntityPageViewModel page)
        {
            foreach (var p in page.Panels)
            {
                if (p is FieldsPanel f)
                    foreach (var x in f.Fields)
                        yield return x;
                if (p is ArmyPanel a)
                    foreach (var row in a.Rows.OfType<UnitRowField>())
                    {
                        yield return row.Unit;
                        if (row.Count != null)
                            yield return row.Count;
                    }
                if (p is StatsPanel s)
                    foreach (var cell in s.Columns.SelectMany(c => c.Cells).Concat(s.Footer))
                    {
                        if (cell is PanelField pf)
                            yield return pf;
                        if (cell is LeaderField l)
                        {
                            yield return l.Class;
                            yield return l.Bonus;
                        }
                    }
            }
        }

        /// <summary>Sets a badge's value as typing into it does (the page commits it as an edit).</summary>
        private void SetBadge(MainWindowViewModel vm, string commandText, string value)
        {
            if (!commandText.StartsWith("#")) commandText = "#" + commandText;
            if (!CommandsMap.TryGetCommand(commandText, out var command))
                throw new ArgumentException("unknown command " + commandText);
            var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
            foreach (var section in page.Sections.Append(page.Other!))
            {
                var item = section.Badges.FirstOrDefault(b => b.Command == command);
                if (item != null)
                {
                    item.Value = value;
                    Log($"badge {commandText} = {value} (in {section.Id}){(page.Error != null ? " error: " + page.Error : "")}");
                    return;
                }
            }
            // not shown yet: set it as the add box would, then type the value
            page.SetValue(command, value);
            Log($"set {commandText} {value} (not shown before){(page.Error != null ? " error: " + page.Error : "")}");
        }
    }
}
