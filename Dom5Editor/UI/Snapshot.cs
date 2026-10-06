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
                    ShowInTaskbar = false, ShowActivated = false, SkipCloseConfirmation = true,
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
                            var field = page.Panels.OfType<FieldsPanel>().SelectMany(p => p.Fields).FirstOrDefault(f => f.Label == label)
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

        private static Command CommandOf(string text)
        {
            if (!text.StartsWith("#")) text = "#" + text;
            return CommandsMap.TryGetCommand(text, out var c) ? c : throw new ArgumentException("unknown command " + text);
        }

        /// <summary>The selected entity's page on the current tab.</summary>
        private static EntityPageViewModel? Selected(MainWindowViewModel vm) => vm.SelectedPage;

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
