using Avalonia.Threading;
using Dom5Editor.Ava.Views;

namespace Dom5Editor.Ava
{
    /// <summary>
    /// An error the editor doesn't expect (as the WPF App's handler): it's written to errors.log in
    /// the settings folder, a mod with unsaved edits is copied to the recovery folder, the user is
    /// told, and the editor keeps running (unsaved edits stay).
    /// </summary>
    internal static class CrashHandler
    {
        private static bool _telling;

        public static void Install(Func<MainWindow?> window)
        {
            // errors on the UI thread (event handlers, bindings, async void handlers): handled, the editor goes on
            Dispatcher.UIThread.UnhandledException += (s, e) =>
            {
                e.Handled = true;
                var recovery = Record(e.Exception, window());
                Tell(e.Exception, recovery, window());
            };
            // errors on other threads end the process: what can be kept is
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    Record(ex, window());
            };
            // a task nobody waited on failed: logged only
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log(e.Exception);
                e.SetObserved();
            };
        }

        private static string LogPath => Path.Combine(Session.Settings.Folder, "errors.log");

        private static void Log(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Session.Settings.Folder);
                File.AppendAllText(LogPath, $"{DateTime.Now:u}\n{ex}\n\n");
            }
            catch (Exception)
            {
                // nowhere to write: nothing more to do
            }
        }

        /// <summary>Logs the error and copies a mod with unsaved edits to the recovery folder; the copy's path, if one was made.</summary>
        private static string? Record(Exception ex, MainWindow? window)
        {
            Log(ex);
            try
            {
                if (window?.ViewModel.Session is { } session && session.History.IsDirty)
                {
                    var dir = Path.Combine(Session.Settings.Folder, "recovery");
                    Directory.CreateDirectory(dir);
                    var name = string.Concat((session.Mod.ModName ?? "mod").Split(Path.GetInvalidFileNameChars()));
                    var path = Path.Combine(dir, $"{name}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.dm");
                    session.Mod.ExportCopy(path);
                    return path;
                }
            }
            catch (Exception)
            {
                // the copy failed too: the log has the first error
            }
            return null;
        }

        private static async void Tell(Exception ex, string? recovery, MainWindow? window)
        {
            // one message at a time (an error while it's shown is logged only)
            if (_telling || window == null || !window.IsVisible)
                return;
            _telling = true;
            try
            {
                await Dialogs.Tell(window, "Dom6 Mod Editor",
                    $"Something went wrong:\n\n{ex.Message}\n\nThe editor keeps running; your edits are still there (save to keep them). Details are in {LogPath}." +
                    (recovery != null ? $"\n\nA copy of the mod with your unsaved edits is at {recovery}." : ""));
            }
            catch (Exception)
            {
                // the message couldn't be shown: it's in the log
            }
            finally
            {
                _telling = false;
            }
        }
    }
}
