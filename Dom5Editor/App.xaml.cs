using System.IO;
using System.Windows;
using Dom5Edit;

namespace Dom5Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // the shared core's hooks (pictures, dialogs, the dispatcher) set to WPF
            UI.Converters.Pictures.Install(this);

            // the game's data, the player's game folder, the data files (shared with the Mac/Linux editor)
            if (EditorStartup.Configure() is string cannotStart)
            {
                MessageBox.Show(cannotStart, "Dom6 Mod Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            // Dom5Editor --snapshot ...: render the editor off-screen for checking (UI/Snapshot.cs)
            if (UI.Snapshot.TryRun(e.Args, this))
                return;

            // the game's data read now, before a mod is opened
            if (EditorStartup.LoadVanilla() is string problem)
                MessageBox.Show(problem + "\n\nThe editor may not work correctly.", "Dom6 Mod Editor", MessageBoxButton.OK, MessageBoxImage.Warning);

            // an error the editor doesn't expect: say so, log it, and keep running (unsaved edits stay)
            DispatcherUnhandledException += (s, args) =>
            {
                try
                {
                    Directory.CreateDirectory(Session.Settings.Folder);
                    File.AppendAllText(Path.Combine(Session.Settings.Folder, "errors.log"), $"{DateTime.Now:u}\n{args.Exception}\n\n");
                }
                catch (Exception) { }
                // unsaved edits: a copy of the mod as it is now, in case the editor can't go on
                string recovery = "";
                try
                {
                    if ((MainWindow?.DataContext as UI.Views.MainWindowViewModel)?.Session is Session.EditorSession session && session.History.IsDirty)
                    {
                        var dir = Path.Combine(Session.Settings.Folder, "recovery");
                        Directory.CreateDirectory(dir);
                        var name = string.Concat((session.Mod.ModName ?? "mod").Split(Path.GetInvalidFileNameChars()));
                        var path = Path.Combine(dir, $"{name}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.dm");
                        session.Mod.ExportCopy(path);
                        recovery = $"\n\nA copy of the mod with your unsaved edits is at {path}.";
                    }
                }
                catch (Exception) { }
                MessageBox.Show($"Something went wrong:\n\n{args.Exception.Message}\n\nThe editor keeps running; your edits are still there (save to keep them). Details are in {Path.Combine(Session.Settings.Folder, "errors.log")}.{recovery}",
                    "Dom6 Mod Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            MainWindow = new UI.Views.MainWindow();
            MainWindow.Show();
        }

    }
}
