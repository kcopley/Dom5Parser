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
    /// The window opens off-screen. Steps run in order (those that need only the view models are
    /// in Dom5Editor.Core's UI/SnapshotSteps.cs, shared with the Avalonia harness, so the same
    /// command line works in both):
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
    ///   --used-by                log the selected page's "used by" list
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
                // the steps that need only the view models are shared with the Avalonia harness (Core: UI/SnapshotSteps.cs)
                var steps = new SnapshotSteps(vm, Log, Pump) { Output = file => logPath ??= file + ".log" };

                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--size":
                            i += 2;
                            break;
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
                        case "--flags":
                        {
                            // --flags FILE.png: every nation's flag as the editor shows it (its #flag file,
                            // else the one the game makes from its colors) on one sheet, with the time it
                            // took and a checksum of the pixels in nation order: tools/dom6exe/flags.py
                            // check prints the one the game's rule gives
                            var session = vm.Session!;
                            var tab = vm.Tabs.OfType<EntityTypeTab>().First(t => t.Type == EntityType.NATION);
                            List<(int ID, BitmapSource? Image)> All() => tab.Items.OrderBy(x => x.ID)
                                .Select(x => (x.ID, Converters.Pictures.ToImage(Sprites.SpriteLoader.Of(session.Resolve(x.Entity), EntityType.NATION, session.Mod.FullFilePath))))
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
                        case "--report":
                        {
                            // --report FILE.png: the check made on opening (its bar's text, how long it took)
                            // and the report window, rendered
                            logPath ??= args[i + 1] + ".log";
                            Pump();
                            steps.LogReport();
                            var report = vm.Report!;
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
                        case "--type-tab":
                        {
                            // --type-tab COMMAND VALUE NEXT: type VALUE in COMMAND's box on screen, then Tab to
                            // NEXT's box: logs where the cursor is after the edit rebuilt the page
                            var c = SnapshotSteps.CommandOf(args[++i]);
                            var value = args[++i];
                            var next = SnapshotSteps.CommandOf(args[++i]);
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
                            if (!steps.TryRun(args, ref i))
                                throw new ArgumentException("unknown snapshot step " + args[i]);
                            break;
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


        /// <summary>The selected entity's page on the current tab.</summary>
        private static EntityPageViewModel? Selected(MainWindowViewModel vm) => vm.SelectedPage;




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
