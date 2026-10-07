using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Dom5Edit.Entities;
using Dom5Editor.Ava.Views;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Ava
{
    /// <summary>
    /// Dom5Editor.Avalonia --snapshot STEPS: the editor rendered off-screen (Avalonia's headless
    /// platform with Skia: no display needed), for checking on any system. Steps, in order:
    ///   --size W H               the window's size (before the first step that shows it)
    ///   --mod FILE.dm            open a mod (else a new one)
    ///   --select TYPE ID         open an entity's page (monster 3)
    ///   --select-name TYPE TEXT  the first entity of the type whose list name has TEXT
    ///   --png FILE.png           the window
    ///   --view FILE.png          the selected page, tall
    ///   --report FILE.png        the report window for the mod
    /// Messages go to FILE.log next to the first image, and to the console.
    /// </summary>
    internal static class Snapshot
    {
        private static readonly List<string> _log = new();
        private static string? _logPath;

        public static int Run(string[] args)
        {
            // test runs keep their backups out of the user's
            Dom5Edit.ModBackups.Folder = Path.Combine(Path.GetTempPath(), "Dom5Editor-snapshot-backups");
            AppBuilder.Configure<App>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .WithInterFont()
                .SetupWithoutStarting();
            int width = 1400, height = 1000;
            for (int i = 1; i + 2 < args.Length; i++)
                if (args[i] == "--size") { width = int.Parse(args[i + 1]); height = int.Parse(args[i + 2]); }
            int code = 0;
            try
            {
                if ((EditorStartup.Configure() ?? EditorStartup.LoadVanilla()) is string problem)
                    Log(problem);
                Hooks.Install();
                var window = new MainWindow { Width = width, Height = height, SkipCloseConfirmation = true };
                Hooks.Owner = window;
                window.Show();
                var vm = window.ViewModel;
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
                            window.Open(Path.GetFullPath(args[++i]));
                            Pump();
                            Log($"loaded {args[i]}; {vm.StatusMessage}");
                            break;
                        case "--select":
                        {
                            var type = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                            int id = int.Parse(args[++i]);
                            vm.NavigateToEntity(type, id);
                            Pump();
                            Log($"selected {type} {id}: {vm.SelectedPage?.Title ?? "(not found)"}");
                            break;
                        }
                        case "--select-name":
                        {
                            var type = Enum.Parse<EntityType>(args[++i], ignoreCase: true);
                            var text = args[++i];
                            var tab = vm.TabOf(type) ?? throw new ArgumentException("no tab for " + type);
                            vm.SelectedTab = tab;
                            var item = tab.Items.FirstOrDefault(x => (x.DisplayName ?? "").Contains(text, StringComparison.OrdinalIgnoreCase));
                            tab.SelectedItem = item;
                            Pump();
                            Log($"selected {type} {item?.DisplayName ?? "(not found)"} #{item?.ID}");
                            break;
                        }
                        case "--png":
                            _logPath ??= args[i + 1] + ".log";
                            Pump();
                            Render(window, args[++i]);
                            break;
                        case "--view":
                        {
                            _logPath ??= args[i + 1] + ".log";
                            var page = vm.SelectedPage ?? throw new InvalidOperationException("nothing selected");
                            var host = new Window { Width = width - 300, Height = 2400, Content = new EntityPageView { DataContext = page } };
                            host.Show();
                            Pump();
                            Render(host, args[++i]);
                            host.Close();
                            break;
                        }
                        case "--report":
                        {
                            _logPath ??= args[i + 1] + ".log";
                            var report = vm.Report ?? vm.BuildReport() ?? throw new InvalidOperationException("no mod");
                            var rw = new ReportWindow(vm, report) { Width = 1000, Height = 900 };
                            rw.Show();
                            Pump();
                            Render(rw, args[++i]);
                            rw.Close();
                            break;
                        }
                        default:
                            throw new ArgumentException("unknown step " + args[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("ERROR " + ex);
                code = 1;
            }
            if (_logPath != null)
                File.WriteAllLines(_logPath, _log);
            return code;
        }

        private static void Log(string message)
        {
            _log.Add(message);
            Console.WriteLine(message);
        }

        /// <summary>Lets the window lay out and draw what's pending.</summary>
        private static void Pump()
        {
            for (int k = 0; k < 4; k++)
            {
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }
        }

        private static void Render(TopLevel top, string path)
        {
            var frame = top.CaptureRenderedFrame();
            if (frame == null)
                throw new InvalidOperationException("nothing rendered");
            frame.Save(path);
            Log("rendered " + path);
        }
    }
}
