using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.UI.Controls;
using Dom5Editor.UI.ViewModels;
using Dom5Editor.UI.Views;

namespace Dom5Editor.UI
{
    /// <summary>
    /// Dom5Editor --snapshot: drives the editor with no one at the screen and renders it to PNG,
    /// to check how views look and what edits do (docs/EDIT_FLOW.md, "How this is verified").
    /// The window opens off-screen. Steps run in order:
    ///   --mod FILE.dm            load a mod (otherwise a new, empty mod)
    ///   --select TYPE ID         select an entity (monster, weapon, armor, spell, item, site, nation, ...)
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
    ///   --dump                   log the selected entity's values and where each comes from
    ///   --undo / --redo          undo or redo the last edit
    ///   --save FILE.dm           save the mod (the editor's Save)
    ///   --png FILE.png           render the window
    ///   --view FILE.png          render the selected entity's view at its full height
    ///   --size W H               window size (default 1400 x 2000)
    /// Messages go to FILE.png.log / FILE.dm.log next to the first output, and to stdout.
    /// Example: Dom5Editor.exe --snapshot --select monster 1 --badge hp 30 --png hp.png --save out.dm
    /// </summary>
    public static class Snapshot
    {
        private static readonly List<string> _log = new List<string>();

        public static bool TryRun(string[] args, Application app)
        {
            if (args.Length == 0 || args[0] != "--snapshot")
                return false;
            int exitCode = 0;
            string? logPath = null;
            MainWindow? window = null;
            try
            {
                int width = 1400, height = 2000;
                for (int i = 1; i < args.Length; i++)
                    if (args[i] == "--size") { width = int.Parse(args[i + 1]); height = int.Parse(args[i + 2]); }

                window = new MainWindow
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -30000, Top = -30000, Width = width, Height = height,
                    ShowInTaskbar = false, ShowActivated = false, SkipCloseConfirmation = true, KeepLayout = false, WindowState = WindowState.Normal,
                };
                app.MainWindow = window;
                EntityTypeTab.Confirm = null; // no dialogs off-screen: deletes go ahead (and say so in the log)
                window.Show();
                var vm = (MainWindowViewModel)window.DataContext;
                if (!args.Contains("--mod"))
                    vm.CreateNewMod();
                Pump();

                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--size":
                            i += 2;
                            break;
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
                        case "--time-view":
                        {
                            // --time-view TYPE NAME: how long selecting a page takes until it's laid out; and how many visuals it has
                            var t = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                            var name = args[++i];
                            var tab = vm.TabOf(t) ?? throw new ArgumentException("no tab for " + t);
                            vm.SelectedTab = tab;
                            Pump();
                            var item = tab.Items.First(x => x.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase));
                            var watch = System.Diagnostics.Stopwatch.StartNew();
                            tab.SelectedItem = item;
                            long model = watch.ElapsedMilliseconds;
                            window.UpdateLayout();
                            long layout = watch.ElapsedMilliseconds;
                            Pump();
                            long shown = watch.ElapsedMilliseconds;
                            Log($"   model {model} ms, layout {layout - model} ms, render {shown - layout} ms");
                            var counts = Visuals<FrameworkElement>(window).GroupBy(v => v.GetType().Name).OrderByDescending(g => g.Count()).Take(6)
                                .Select(g => $"{g.Key} {g.Count()}");
                            Log($"time-view {item.DisplayName}: page {model} ms, shown {shown} ms; visuals {Visuals<FrameworkElement>(window).Count()}: {string.Join(", ", counts)}");
                            var badges = Visuals<CompactBadge>(window).ToList();
                            var rows = Visuals<System.Windows.Controls.ContentPresenter>(window).Where(c => c.Content is PanelRow).ToList();
                            Log($"   {badges.Count} badges, {badges.Sum(b => Visuals<FrameworkElement>(b).Count())} visuals in them; {rows.Count} table rows, {rows.Sum(r => Visuals<FrameworkElement>(r).Count())} visuals; page {Visuals<FrameworkElement>(Visuals<EntityPageView>(window).First()).Count()}");
                            var pickers = Visuals<SearchableReferenceComboBox>(Visuals<EntityPageView>(window).First()).ToList();
                            Log($"   {pickers.Count} pickers ({pickers.Count(p => p.IsVisible)} shown), {pickers.Sum(p => Visuals<FrameworkElement>(p).Count())} visuals in them");
                            break;
                        }
                        case "--scroll":
                        {
                            // --scroll Y: scroll the page to Y; then logs where it is (after the next steps, --scroll -1 just logs)
                            var view = Visuals<EntityPageView>(window).First();
                            var scroller = Visuals<System.Windows.Controls.ScrollViewer>(view).First();
                            double y = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture);
                            if (y >= 0)
                            {
                                scroller.ScrollToVerticalOffset(y);
                                Pump();
                            }
                            Log($"page scrolled to {scroller.VerticalOffset:0} of {scroller.ExtentHeight:0}");
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
                        case "--validate":
                        {
                            var result = vm.Validate();
                            Log($"validate: {vm.StatusMessage}");
                            if (result != null)
                                foreach (var issue in result.Issues.OrderBy(x => x.ToString().StartsWith("Error") ? 0 : 1).Take(8))
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
                        case "--type-tab":
                        {
                            // --type-tab COMMAND VALUE NEXT: type VALUE in COMMAND's box on screen, then Tab to
                            // NEXT's box: logs where the cursor is after the edit rebuilt the page
                            var c = CommandOf(args[++i]);
                            var value = args[++i];
                            var next = CommandOf(args[++i]);
                            window.Activate();
                            System.Windows.Controls.TextBox BoxOf(Command cmd)
                            {
                                // (the page makes parts as they scroll into view)
                                var page = Selected(vm)!;
                                var section = page.Sections.Append(page.Other!).FirstOrDefault(x => x.Badges.Any(b => b.Command == cmd));
                                if (section != null)
                                {
                                    Visuals<EntityPageView>(window).First().ShowPart(section);
                                    Pump();
                                }
                                var badges = Visuals<CompactBadge>(window).Where(b => b.DataContext is PropertyItem p && p.Command == cmd).ToList();
                                var boxes = badges.SelectMany(Visuals<System.Windows.Controls.TextBox>).ToList();
                                return boxes.FirstOrDefault(t => t.IsVisible)
                                       ?? throw new InvalidOperationException($"no box for {cmd}: {badges.Count} badges, {boxes.Count} boxes ({boxes.Count(b => b.IsVisible)} visible); {Visuals<CompactBadge>(window).Count()} badges on screen");
                            }
                            var box = BoxOf(c);
                            System.Windows.Input.Keyboard.Focus(box);
                            box.Text = value;
                            Pump();
                            System.Windows.Input.Keyboard.Focus(BoxOf(next)); // what Tab does
                            Pump();
                            var focused = System.Windows.Input.Keyboard.FocusedElement as DependencyObject;
                            var badge = focused == null ? null : Parent<CompactBadge>(focused);
                            Log($"typed {value} in {args[i - 2]}, tabbed: cursor in {(badge?.DataContext as PropertyItem)?.Command.ToString() ?? "(nothing)"}; page has {Selected(vm)?.Resolved.Get(c)?.Arguments}");
                            break;
                        }
                        case "--sweep":
                        {
                            // --sweep N: open the page of up to N of the mod's entities of every type (all if
                            // N is 0); log any that fails, and the slowest
                            int limit = int.Parse(args[++i]);
                            int opened = 0, failed = 0;
                            long slowest = 0;
                            var pages = new List<WeakReference>(); // pages left alive after they're closed: a leak
                            string slowestName = "";
                            foreach (var tab in vm.Tabs.OfType<EntityTypeTab>())
                            {
                                vm.SelectedTab = tab;
                                var items = tab.Items.Where(x => !x.IsVanilla || x.IsModified).ToList();
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
                        case "--dump":
                        {
                            var page = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
                            Log($"== {page.DisplayName} #{page.ID} ({page.SourceLabel}; {(page.Entity.ParentMod == vm.Session!.Mod ? "the mod's entity" : "vanilla's entity")})");
                            foreach (var line in page.Resolved.Structure)
                                Log($"   structure {line.ToExportString()}");
                            foreach (var v in page.Resolved.Values)
                                Log($"   {v.Property.ToExportString(),-40} {page.SourceText(v)}");
                            foreach (var (c, a) in page.Resolved.Assets)
                                Log($"   asset {c}: {(a.ToExportString() ?? "").Substring(0, Math.Min(60, (a.ToExportString() ?? "").Length))}");
                            break;
                        }
                        case "--save":
                            logPath ??= args[i + 1] + ".log";
                            vm.SaveMod(Path.GetFullPath(args[++i]));
                            Log($"saved {args[i]}");
                            break;
                        case "--png":
                            logPath ??= args[i + 1] + ".log";
                            Pump();
                            Render(window, args[++i]);
                            Log($"rendered {args[i]}");
                            break;
                        case "--view":
                            logPath ??= args[i + 1] + ".log";
                            RenderView(Selected(vm) ?? throw new InvalidOperationException("nothing selected"), width, window, args[++i]);
                            Log($"rendered view {args[i]}");
                            break;
                        default:
                            throw new ArgumentException("unknown snapshot step " + args[i]);
                    }
                    Pump();
                }
            }
            catch (Exception ex)
            {
                Log("ERROR " + ex);
                exitCode = 1;
            }
            finally
            {
                window?.Close();
                if (logPath != null)
                    File.WriteAllLines(logPath, _log);
                app.Shutdown(exitCode);
            }
            return true;
        }

        private static void Log(string message)
        {
            _log.Add(message);
            Console.WriteLine(message);
        }

        /// <summary>Lets bindings, layout and rendering catch up.</summary>
        private static void Pump()
        {
            for (int k = 0; k < 3; k++)
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
        }

        private static IEnumerable<T> Visuals<T>(DependencyObject root) where T : DependencyObject
        {
            for (int k = 0; k < VisualTreeHelper.GetChildrenCount(root); k++)
            {
                var child = VisualTreeHelper.GetChild(root, k);
                if (child is T t)
                    yield return t;
                foreach (var d in Visuals<T>(child))
                    yield return d;
            }
        }

        private static T? Parent<T>(DependencyObject d) where T : DependencyObject
        {
            for (var x = VisualTreeHelper.GetParent(d); x != null; x = VisualTreeHelper.GetParent(x))
                if (x is T t)
                    return t;
            return null;
        }

        private static Command CommandOf(string text)
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
        private static void SetBadge(MainWindowViewModel vm, string commandText, string value)
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

        /// <summary>
        /// The entity's view (the template the main window shows for its view model), laid out
        /// at the window's width and whatever height it needs, so nothing is cut off by scrolling.
        /// </summary>
        private static void RenderView(object entityVm, int width, Window window, string path)
        {
            var host = new System.Windows.Controls.ContentControl { Content = entityVm, Width = width - 320 };
            // the views scroll inside a ScrollViewer; with unlimited height it takes the content's height
            host.Measure(new Size(host.Width, double.PositiveInfinity));
            host.Arrange(new Rect(host.DesiredSize));
            host.UpdateLayout();
            Pump();
            host.Measure(new Size(host.Width, double.PositiveInfinity));
            host.Arrange(new Rect(host.DesiredSize));
            host.UpdateLayout();
            Save(host, window.Background, path);
        }

        private static void Save(FrameworkElement root, Brush? background, string path)
        {
            int w = (int)Math.Ceiling(root.ActualWidth), h = (int)Math.Ceiling(root.ActualHeight);
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(background ?? Brushes.Black, null, new Rect(0, 0, w, h));
                dc.DrawRectangle(new VisualBrush(root), null, new Rect(0, 0, w, h));
            }
            var bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.GetFullPath(path));
            encoder.Save(stream);
        }

        private static void Render(Window window, string path)
        {
            var root = (FrameworkElement)window.Content;
            int w = (int)Math.Ceiling(root.ActualWidth), h = (int)Math.Ceiling(root.ActualHeight);
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                // the window's own background, then its content
                dc.DrawRectangle(window.Background ?? Brushes.Black, null, new Rect(0, 0, w, h));
                dc.DrawRectangle(new VisualBrush(root), null, new Rect(0, 0, w, h));
            }
            var bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.GetFullPath(path));
            encoder.Save(stream);
        }
    }
}
