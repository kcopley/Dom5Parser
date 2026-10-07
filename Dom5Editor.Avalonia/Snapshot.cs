using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dom5Editor.Ava.Controls;
using Dom5Editor.Ava.Views;
using Dom5Editor.UI;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Ava
{
    /// <summary>
    /// Dom5Editor.Avalonia --snapshot STEPS: the editor driven with no one at the screen and
    /// rendered off-screen (Avalonia's headless platform with Skia: no display needed, so it runs
    /// in WSL and Linux CI), as the WPF harness (Dom5Editor/UI/Snapshot.cs) does: the same step
    /// names and log lines, so a command line works with both editors. The steps that need only
    /// the view models (--mod, --select, --set, --add, --remove, --undo, --redo, --save, --sweep,
    /// --used-by, --dump, ... ) are shared: Dom5Editor.Core's UI/SnapshotSteps.cs lists them.
    /// This harness's own, which look at the controls:
    ///   --size W H               the window's size (default 1400 x 2000, as WPF's)
    ///   --png FILE.png           render the window
    ///   --view FILE.png          render the selected entity's page at its full height
    ///   --report FILE.png        the check made on opening a mod (its bar, timing) and the report window
    ///   --validation FILE.png    the full issue list ("Every issue..." in the report window)
    ///   --tooltips               every control shown with no tooltip on it or around it, and the buttons' tips
    ///   --key GESTURE            press keys (Ctrl+F, Ctrl+P, Alt+Left, Down, Enter): logs what has the cursor
    ///   --type TEXT              type into what has the cursor (with --key: the "Go to" box, a search box)
    ///   --load-menu              log the Load ▾ menu (recent mods, the game's folder, backups)
    ///   --game-folder PATH       log what "Dominions 6 folder..." would make of PATH (nothing saved)
    /// Messages go to FILE.png.log / FILE.dm.log next to the first output, and to stdout.
    /// Settings, backups, errors.log and recovery copies go to temp folders, never the user's.
    /// Example: Dom5Editor.Avalonia --snapshot --mod my.dm --select monster 3 --set hp 30 --png hp.png --save out.dm
    /// </summary>
    internal static class Snapshot
    {
        private static readonly List<string> _log = new();
        private static string? _logPath;

        public static int Run(string[] args)
        {
            // test runs keep their backups out of the user's
            Dom5Edit.ModBackups.Folder = Path.Combine(Path.GetTempPath(), "Dom5Editor-snapshot-backups");
            SnapshotSteps.UseTestNeededMods(); // (and the mods each mod needs)
            AppBuilder.Configure<App>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .WithInterFont()
                .SetupWithoutStarting();
            int width = 1400, height = 2000;
            for (int i = 1; i + 2 < args.Length; i++)
                if (args[i] == "--size") { width = int.Parse(args[i + 1]); height = int.Parse(args[i + 2]); }
            int code = 0;
            MainWindow? window = null;
            try
            {
                // the user's settings are read (their game folder), never written: from here on they go to temp
                if ((EditorStartup.Configure() ?? EditorStartup.LoadVanilla()) is string problem)
                    Log(problem);
                Session.Settings.Folder = Path.Combine(Path.GetTempPath(), "Dom5Editor-snapshot-settings");
                Hooks.Install();
                window = new MainWindow { Width = width, Height = height, SkipCloseConfirmation = true, KeepLayout = false };
                Hooks.Owner = window;
                EntityTypeTab.Confirm = null; // no dialogs off-screen: deletes go ahead (and say so in the log)
                EntityPageViewModel.SaveFirst = null; // (an image on a mod never saved: the error, not the save dialog)
                window.Show();
                var vm = window.ViewModel;
                if (!args.Contains("--mod"))
                    vm.CreateNewMod();
                Pump();
                Log("game texts: " + Dom5Edit.VanillaLoader.TextsStatus);
                var steps = new SnapshotSteps(vm, Log, Pump) { Output = file => _logPath ??= file + ".log" };

                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--size":
                            i += 2;
                            break;
                        case "--png":
                            _logPath ??= args[i + 1] + ".log";
                            Pump();
                            Render(window, args[++i]);
                            Log($"rendered {args[i]}");
                            break;
                        case "--view":
                        {
                            _logPath ??= args[i + 1] + ".log";
                            var page = vm.SelectedPage ?? throw new InvalidOperationException("nothing selected");
                            RenderView(page, width, args[++i]);
                            Log($"rendered view {args[i]}");
                            break;
                        }
                        case "--report":
                        {
                            // --report FILE.png: the check made on opening (its bar's text, how long it took)
                            // and the report window, rendered
                            _logPath ??= args[i + 1] + ".log";
                            Pump();
                            steps.LogReport();
                            var rw = new ReportWindow(vm, vm.Report!) { Width = 1000, Height = 900 };
                            rw.Show();
                            Pump();
                            Render(rw, args[++i]);
                            rw.Close();
                            Log($"rendered report {args[i]}");
                            break;
                        }
                        case "--validation":
                        {
                            // --validation FILE.png: Validate's full issue list, as the report window's "Every issue..." shows it
                            _logPath ??= args[i + 1] + ".log";
                            var result = vm.Validate() ?? throw new InvalidOperationException("no mod");
                            var vw = new ValidationWindow(result, vm) { Width = 900, Height = 900 };
                            vw.Show();
                            Pump();
                            Render(vw, args[++i]);
                            Log($"validation: {vm.StatusMessage}; {vw.ShownText}");
                            vw.Close();
                            Log($"rendered validation {args[i]}");
                            break;
                        }
                        case "--tooltips":
                            Tooltips(window);
                            break;
                        case "--key":
                        {
                            // --key GESTURE: press keys as the user would (Ctrl+F, Ctrl+P, Alt+Left, Down, Enter); logs where the cursor is
                            var gesture = KeyGesture.Parse(args[++i]);
                            var raw = RawInputModifiers.None;
                            if (gesture.KeyModifiers.HasFlag(KeyModifiers.Control)) raw |= RawInputModifiers.Control;
                            if (gesture.KeyModifiers.HasFlag(KeyModifiers.Shift)) raw |= RawInputModifiers.Shift;
                            if (gesture.KeyModifiers.HasFlag(KeyModifiers.Alt)) raw |= RawInputModifiers.Alt;
                            if (gesture.KeyModifiers.HasFlag(KeyModifiers.Meta)) raw |= RawInputModifiers.Meta;
                            window.KeyPress(gesture.Key, raw, PhysicalKey.None, null);
                            window.KeyRelease(gesture.Key, raw, PhysicalKey.None, null);
                            Pump();
                            Log($"key {args[i]}: {Focused(window)}; on {vm.SelectedPage?.DisplayName ?? "(nothing)"}");
                            break;
                        }
                        case "--type":
                            // --type TEXT: type into whatever has the cursor
                            window.KeyTextInput(args[++i]);
                            Pump();
                            Log($"typed {args[i]}: {Focused(window)}");
                            break;
                        case "--game-folder":
                        {
                            // --game-folder PATH: what "Dominions 6 folder..." makes of a folder picked (nothing is saved)
                            var (folder, note) = MainWindow.CheckGameFolder(args[++i]);
                            Log($"game folder {args[i]}: {folder ?? "(not the game's)"}; {note.Trim().Replace("\n", " ")}");
                            break;
                        }
                        case "--load-menu":
                            // the Load ▾ menu: its entries and their tooltips
                            foreach (var item in window.LoadMenu())
                                Log(item is MenuItem m ? $"   {m.Header}{(m.IsEnabled ? "" : " (disabled)")}: {ToolTip.GetTip(m)}" : "   ----");
                            Log("load menu");
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
                code = 1;
            }
            finally
            {
                window?.Close();
                if (_logPath != null)
                    File.WriteAllLines(_logPath, _log);
            }
            return code;
        }

        private static void Log(string message)
        {
            _log.Add(message);
            Console.WriteLine(message);
        }

        /// <summary>Lets bindings, layout and rendering catch up.</summary>
        private static void Pump()
        {
            for (int k = 0; k < 4; k++)
            {
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }
        }

        /// <summary>What has the keyboard: "TextBox 'Search' in EntityTypeTabView".</summary>
        private static string Focused(Window window)
        {
            if (window.FocusManager?.GetFocusedElement() is not Control c)
                return "nothing focused";
            var view = c.GetVisualAncestors().OfType<Control>().FirstOrDefault(a => a is UserControl || a is RefPicker || a is Window);
            return $"{c.GetType().Name}{(string.IsNullOrEmpty(c.Name) ? "" : $" '{c.Name}'")} in {view?.GetType().Name}{(string.IsNullOrEmpty(view?.Name) ? "" : $" '{view!.Name}'")}" +
                   (c is TextBox t && !string.IsNullOrEmpty(t.Text) ? $" (\"{t.Text}\")" : "");
        }

        private static void Render(TopLevel top, string path)
        {
            var frame = top.CaptureRenderedFrame() ?? throw new InvalidOperationException("nothing rendered");
            frame.Save(Path.GetFullPath(path));
        }

        /// <summary>
        /// The selected entity's page (the view the main window shows for it) at the list's
        /// neighbour's width and whatever height it needs, so nothing is cut off by scrolling.
        /// </summary>
        private static void RenderView(EntityPageViewModel page, int width, string path)
        {
            var view = new EntityPageView { DataContext = page };
            var host = new Window { Width = width - 320, Height = 600, Content = new Border { Padding = new Thickness(16), Child = view } };
            host.Show();
            Pump();
            // its scroll viewer, given unlimited height, takes the content's height
            var root = (Control)host.Content!;
            root.Measure(new Size(width - 320, double.PositiveInfinity));
            host.Height = Math.Max(200, Math.Ceiling(root.DesiredSize.Height));
            Pump();
            Render(host, path);
            host.Close();
        }

        /// <summary>
        /// --tooltips: every visible control (button, box, check box, picker) with no tooltip on it
        /// or on anything around it, and the tooltips the buttons show (as the WPF harness logs them).
        /// </summary>
        private static void Tooltips(Window window)
        {
            Pump();
            int controls = 0, missing = 0;
            var seen = new HashSet<string>();
            foreach (var c in window.GetVisualDescendants().OfType<Control>())
            {
                if (!(c is Button || c is TextBox || c is ComboBox || c is RefPicker) || !c.IsEffectivelyVisible || !c.IsHitTestVisible || c.Opacity <= 0)
                    continue;
                // parts of a control's own template (a combo box's, a scroll bar's, a picker's box) show its tooltip
                if (c.TemplatedParent is ComboBox or ScrollBar or TextBox or AutoCompleteBox || c.FindAncestorOfType<RefPicker>() is { } picker && !ReferenceEquals(picker, c))
                    continue;
                controls++;
                object? tip = null;
                for (Visual? x = c; x != null && tip == null; x = x.GetVisualParent())
                    if (x is Control f && ToolTip.GetTip(f) is object t && !(t is string ts && ts.Length == 0))
                        tip = t;
                var what = $"{c.GetType().Name} '{(c as ContentControl)?.Content as string ?? (c as TextBox)?.Text ?? ""}' in {c.DataContext?.GetType().Name}";
                if (tip == null && seen.Add(what))
                {
                    missing++;
                    Log($"   no tooltip: {what}");
                }
                else if (tip is string text && c is Button && seen.Add("tip:" + text))
                    Log($"   tip: {c.GetType().Name} '{(c as ContentControl)?.Content as string}': {text.Replace('\n', ' ')}");
            }
            Log($"tooltips: {controls} controls shown, {missing} kinds without a tooltip");
        }
    }
}
