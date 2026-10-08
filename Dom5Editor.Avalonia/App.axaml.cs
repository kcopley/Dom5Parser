using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace Dom5Editor.Ava
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Views.MainWindow? window = null;
                // an error the editor doesn't expect: logged, told, and the editor keeps running
                CrashHandler.Install(() => window);
                // the game's data, the player's game folder, the data files (shared with the WPF editor)
                var problem = EditorStartup.Configure() ?? EditorStartup.LoadVanilla();
                Hooks.Install();
                window = new Views.MainWindow();
                desktop.MainWindow = window;
                // (a page's own window doesn't keep the editor running: they close with the main one)
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Hooks.Owner = window;
                if (problem != null)
                    window.Status(problem + " The editor may not work correctly.");

                // a mod given on the command line (Dom5Editor.Avalonia my.dm), once the window is up
                var file = desktop.Args?.FirstOrDefault(a => a.EndsWith(".dm", StringComparison.OrdinalIgnoreCase) && File.Exists(a));
                if (file != null)
                    window.Opened += (s, e) => window.Open(Path.GetFullPath(file));

                // macOS hands files opened with the editor (Finder's Open With, a .dm dropped on the app) as an event
                if (TryGetFeature(typeof(IActivatableLifetime)) is IActivatableLifetime activatable)
                    activatable.Activated += async (s, e) =>
                    {
                        if (e is FileActivatedEventArgs files && files.Files.FirstOrDefault()?.TryGetLocalPath() is string path)
                            await window.OpenAsking(path);
                    };
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
