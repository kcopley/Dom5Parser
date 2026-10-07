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
    ///   --mod-info FIELD VALUE   a Mod Info box (modname, description, version, domversion, icon; iconfile FILE: its Pick...)
    ///   --copy ID / --sprite-from ID   the copy picker (#copystats, ...) / the sprite picker (#copyspr); 0: none
    ///   --add-clear COMMAND      the clears picker (#clearrec, #clearweapons, ...)
    ///   --flag LABEL on|off      a flags panel checkbox; --path-level F N a magic path's level box
    ///   --panel-new TITLE        a list panel's "+ New weapon/armor"; --panel-add TITLE ID its add box;
    ///   --panel-remove TITLE N   its Nth row's remove button
    ///   --event-rarity NAME|N, --event-owner N [NATION], --event-msg TEXT   an event's header and message boxes
    ///   --dump                   log the selected entity's values and where each comes from
    ///   --dump-mod               --dump every entity the mod makes or changes (to compare a mod as made and as reloaded)
    ///   --derived                log what the game makes of its stats (the bracketed values, "in game" notes, table rows)
    ///   --sweep N                open up to N of the mod's entities of every type (0: all); --sweep-vanilla N
    ///                            the same for the game's own entities
    ///   --undo / --redo          undo or redo the last edit
    ///   --save FILE.dm           save the mod (the editor's Save)
    ///   --png FILE.png           render the window
    ///   --report FILE.png        the check made on opening a mod (its bar, timing) and the report window
    ///   --view FILE.png          render the selected entity's view at its full height
    ///   --scroll-list TYPE N     scroll a type's list N screens (0: to the end), timing each (sprites decode as rows show)
    ///   --flags FILE.png         every nation's flag on one sheet, timed, with a checksum (tools/dom6exe/flags.py check)
    ///   --size W H               window size (default 1400 x 2000)
    /// Messages go to FILE.png.log / FILE.dm.log next to the first output, and to stdout.
    /// Example: Dom5Editor.exe --snapshot --select monster 1 --badge hp 30 --png hp.png --save out.dm
    /// </summary>
    public static class Snapshot
    {
        private static readonly List<string> _log = new List<string>();

        private static T? FindParent<T>(DependencyObject d) where T : DependencyObject
        {
            for (var x = System.Windows.Media.VisualTreeHelper.GetParent(d); x != null; x = System.Windows.Media.VisualTreeHelper.GetParent(x))
                if (x is T t)
                    return t;
            return null;
        }

        public static bool TryRun(string[] args, Application app)
        {
            if (args.Length == 0 || args[0] != "--snapshot")
                return false;
            // test runs keep their backups out of the user's (Settings folder\backups)
            Dom5Edit.ModBackups.Folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Dom5Editor-snapshot-backups");
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
                EntityPageViewModel.SaveFirst = null; // (an image on a mod never saved: the error, not the save dialog)
                window.Show();
                var vm = (MainWindowViewModel)window.DataContext;
                if (!args.Contains("--mod"))
                    vm.CreateNewMod();
                Pump();
                Log("game texts: " + Dom5Edit.VanillaLoader.TextsStatus);

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
                            Log($"sprite from {source}{(page.Error != null ? " error: " + page.Error : "")}: slots {string.Join(", ", now.SpriteSlots.Select(x => $"{x.Label} {(x.HasImage ? $"{x.Image!.PixelWidth}x{x.Image.PixelHeight}" : "none")}"))}");
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
                        case "--tooltips":
                        {
                            // --tooltips: every visible control (button, box, check box, list) with no tooltip on
                            // it or on anything around it, and the tooltips the page's buttons show
                            Pump();
                            int controls = 0, missing = 0;
                            var seen = new HashSet<string>();
                            void Walk(DependencyObject d)
                            {
                                if (d is FrameworkElement fe && fe.IsVisible && fe.IsHitTestVisible && fe.Opacity > 0 && (d is System.Windows.Controls.Primitives.ButtonBase || d is System.Windows.Controls.TextBox
                                    || d is System.Windows.Controls.ComboBox || d is Controls.SearchableReferenceComboBox))
                                {
                                    // inside a combo box's own template: its parts show the combo box's tooltip
                                    bool part = fe.TemplatedParent is System.Windows.Controls.ComboBox || fe.TemplatedParent is System.Windows.Controls.Primitives.ScrollBar
                                                || FindParent<Controls.SearchableReferenceComboBox>(fe) is { } sr && !ReferenceEquals(sr, fe);
                                    if (!part)
                                    {
                                        controls++;
                                        object? tip = null;
                                        for (DependencyObject? x = d; x != null && tip == null; x = System.Windows.Media.VisualTreeHelper.GetParent(x))
                                            if (x is FrameworkElement f && f.ToolTip is object t && !(t is string ts && ts.Length == 0))
                                                tip = t;
                                        var what = $"{d.GetType().Name} '{(d as System.Windows.Controls.ContentControl)?.Content as string ?? (d as System.Windows.Controls.TextBox)?.Text ?? ""}' in {fe.DataContext?.GetType().Name}";
                                        if (tip == null && seen.Add(what))
                                        {
                                            missing++;
                                            Log($"   no tooltip: {what}");
                                        }
                                        else if (tip is string text && d is System.Windows.Controls.Primitives.ButtonBase && seen.Add("tip:" + text))
                                            Log($"   tip: {d.GetType().Name} '{(d as System.Windows.Controls.ContentControl)?.Content as string}': {text.Replace('\n', ' ')}");
                                    }
                                }
                                for (int k = 0; k < System.Windows.Media.VisualTreeHelper.GetChildrenCount(d); k++)
                                    Walk(System.Windows.Media.VisualTreeHelper.GetChild(d, k));
                            }
                            Walk(window);
                            Log($"tooltips: {controls} controls shown, {missing} kinds without a tooltip");
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
                            Log($"   slots: {string.Join(", ", now.SpriteSlots.Select(x => $"{x.Label} {(x.HasImage ? $"{x.Image!.PixelWidth}x{x.Image.PixelHeight}" : "none")}"))}");
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
                        case "--flags":
                        {
                            // --flags FILE.png: every nation's flag as the editor shows it (its #flag file,
                            // else the one the game makes from its colors) on one sheet, with the time it
                            // took and a checksum of the pixels in nation order: tools/dom6exe/flags.py
                            // check prints the one the game's rule gives
                            var session = vm.Session!;
                            var tab = vm.Tabs.OfType<EntityTypeTab>().First(t => t.Type == EntityType.NATION);
                            List<(int ID, BitmapSource? Image)> All() => tab.Items.OrderBy(x => x.ID)
                                .Select(x => (x.ID, Sprites.SpriteLoader.Of(session.Resolve(x.Entity), EntityType.NATION, session.Mod.FullFilePath)))
                                .ToList();
                            var watch = System.Diagnostics.Stopwatch.StartNew();
                            var flags = All();
                            long built = watch.ElapsedMilliseconds;
                            watch.Restart();
                            All(); // cached now
                            long again = watch.ElapsedMilliseconds;
                            using var sha = System.Security.Cryptography.SHA256.Create();
                            var shown = flags.Where(x => x.Image != null).ToList();
                            foreach (var (_, image) in shown)
                            {
                                var bgra = image!.Format == PixelFormats.Bgra32 ? image : new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
                                var px = new byte[bgra.PixelWidth * bgra.PixelHeight * 4];
                                bgra.CopyPixels(px, bgra.PixelWidth * 4, 0);
                                sha.TransformBlock(px, 0, px.Length, null, 0);
                            }
                            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                            const int cell = 132, cols = 12;
                            int rows = Math.Max(1, (flags.Count + cols - 1) / cols);
                            var visual = new DrawingVisual();
                            using (var dc = visual.RenderOpen())
                            {
                                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40)), null, new Rect(0, 0, cols * cell, rows * cell));
                                for (int k = 0; k < flags.Count; k++)
                                {
                                    var (nation, image) = flags[k];
                                    double x = k % cols * cell, y = k / cols * cell;
                                    if (image != null)
                                        dc.DrawImage(image, new Rect(x + 2, y + 2, 128, 128));
                                    dc.DrawText(new FormattedText(nation.ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                        new Typeface("Segoe UI"), 11, Brushes.Yellow, 1.0), new Point(x + 3, y + 2));
                                }
                            }
                            var bitmap = new RenderTargetBitmap(cols * cell, rows * cell, 96, 96, PixelFormats.Pbgra32);
                            bitmap.Render(visual);
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            using (var stream = File.Create(Path.GetFullPath(args[++i])))
                                encoder.Save(stream);
                            Log($"flags: {shown.Count} of {flags.Count} nations ({built} ms, {again} ms again); pixels sha256 {Convert.ToHexString(sha.Hash!).ToLowerInvariant()[..16]}; sheet {args[i]}");
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
                        case "--scroll-list":
                        {
                            // --scroll-list TYPE SCREENS: scroll the type's list a screen at a time (0: to the
                            // end); logs the slowest screen (rows are worked out, sprites decoded, as they show)
                            var t = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                            int screens = int.Parse(args[++i]);
                            var tab = vm.TabOf(t) ?? throw new ArgumentException("no tab for " + t);
                            vm.SelectedTab = tab;
                            window.UpdateLayout();
                            Pump();
                            var list = Visuals<EntityListControl>(window).First(l => l.IsVisible && l.DataContext == tab);
                            var scroller = Visuals<System.Windows.Controls.ScrollViewer>(list).First(s => s.ScrollableHeight > 0);
                            scroller.ScrollToTop();
                            window.UpdateLayout();
                            Pump();
                            var total = System.Diagnostics.Stopwatch.StartNew();
                            long slowest = 0;
                            int n = 0;
                            while ((screens == 0 || n < screens) && scroller.VerticalOffset < scroller.ScrollableHeight)
                            {
                                var watch = System.Diagnostics.Stopwatch.StartNew();
                                scroller.PageDown();
                                window.UpdateLayout();
                                Pump();
                                slowest = Math.Max(slowest, watch.ElapsedMilliseconds);
                                n++;
                            }
                            Log($"scroll-list {t}: {n} screens in {total.ElapsedMilliseconds} ms, slowest {slowest} ms; " +
                                $"memory {GC.GetTotalMemory(true) / (1 << 20)} MB managed, {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / (1 << 20)} MB working set");
                            var all = System.Diagnostics.Stopwatch.StartNew();
                            int shown = tab.Items.Count(x => x.Sprite != null);
                            Log($"   {shown} of {tab.Items.Count} rows have a sprite (the rest worked out in {all.ElapsedMilliseconds} ms)");
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
                        case "--report":
                        {
                            // --report FILE.png: the check made on opening (its bar's text, how long it took)
                            // and the report window, rendered
                            logPath ??= args[i + 1] + ".log";
                            Pump();
                            if (vm.Report == null)
                                vm.CheckOnOpen();
                            Log($"report bar: {vm.ReportSummary} ({vm.ReportMilliseconds} ms)");
                            var report = vm.Report!;
                            foreach (var s in report.Sections)
                                Log($"   {s.Title}: {s.Count}" + string.Concat(s.Groups.Take(3).Select(g => $"\n      {g.Text} [{g.Lines.Count} line(s); first goes to {g.Lines.FirstOrDefault()?.Entity?.Kind} {g.Lines.FirstOrDefault()?.Entity?.ID}]")));
                            var rw = new Views.ModReportWindow(vm, report)
                            {
                                WindowStartupLocation = WindowStartupLocation.Manual, Left = -30000, Top = -30000,
                                Width = 1000, Height = 900, ShowActivated = false,
                            };
                            rw.Show();
                            Pump();
                            Render(rw, args[++i]);
                            rw.Close();
                            Log($"rendered report {args[i]}");
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
                                    id > 0 && session.Mod.TryGet(t, id, null, out var e) ? session.Resolve(e) : null;
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

        /// <summary>Logs an entity's values and where each comes from (--dump).</summary>
        private static void Dump(MainWindowViewModel vm, EntityPageViewModel page)
        {
            Log($"== {page.Type} {page.DisplayName} #{page.ID} ({page.SourceLabel}; {(page.Entity.ParentMod == vm.Session!.Mod ? "the mod's entity" : "vanilla's entity")})");
            foreach (var line in page.Resolved.Structure)
                Log($"   structure {line.ToExportString()}");
            foreach (var v in page.Resolved.Values)
                Log($"   {v.Property.ToExportString(),-40} {page.SourceText(v)}");
            foreach (var (c, a) in page.Resolved.Assets)
                Log($"   asset {c}: {(a.ToExportString() ?? "").Substring(0, Math.Min(60, (a.ToExportString() ?? "").Length))}");
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
