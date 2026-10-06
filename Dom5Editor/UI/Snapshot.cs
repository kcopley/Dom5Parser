using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.UI.Controls;
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
    ///   --undo                   undo the last edit
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
                    ShowInTaskbar = false, ShowActivated = false,
                };
                app.MainWindow = window;
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
                            Log($"selected {type} {id}: {(Selected(vm) as EntityViewModel)?.DisplayName ?? "(not found)"}");
                            break;
                        case "--badge":
                            SetBadge(vm, args[++i], args[++i]);
                            break;
                        case "--undo":
                            vm.Undo();
                            Log("undo");
                            break;
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

        /// <summary>The selected entity's view model on the current tab.</summary>
        private static object? Selected(MainWindowViewModel vm)
        {
            var name = "Selected" + vm.SelectedEntityType switch
            {
                EntityType.MONSTER => "Monster", EntityType.WEAPON => "Weapon", EntityType.ARMOR => "Armor",
                EntityType.SPELL => "Spell", EntityType.ITEM => "Item", EntityType.SITE => "Site",
                EntityType.NATION => "Nation", EntityType.EVENT => "Event", EntityType.MERCENARY => "Mercenary",
                EntityType.POPTYPE => "Poptype", EntityType.NAMETYPE => "Nametype", EntityType.BLESS => "Bless",
                EntityType.TEMPLATE => "Template", _ => "",
            };
            return vm.GetType().GetProperty(name)?.GetValue(vm);
        }

        /// <summary>Sets a badge's value as typing into it does (its ValueChanged handler runs).</summary>
        private static void SetBadge(MainWindowViewModel vm, string commandText, string value)
        {
            if (!commandText.StartsWith("#")) commandText = "#" + commandText;
            if (!CommandsMap.TryGetCommand(commandText, out var command))
                throw new ArgumentException("unknown command " + commandText);
            var entityVm = Selected(vm) ?? throw new InvalidOperationException("nothing selected");
            foreach (var prop in entityVm.GetType().GetProperties())
            {
                if (prop.GetIndexParameters().Length > 0 || !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) || prop.PropertyType == typeof(string))
                    continue;
                if (prop.GetValue(entityVm) is not IEnumerable items)
                    continue;
                foreach (var item in items.OfType<PropertyItem>())
                {
                    if (item.Command == command)
                    {
                        item.Value = value;
                        Log($"badge {commandText} = {value} (in {prop.Name})");
                        return;
                    }
                }
            }
            throw new InvalidOperationException($"no badge for {commandText} on the selected entity");
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
