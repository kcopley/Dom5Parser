namespace Dom5Editor.UI
{
    /// <summary>
    /// What the view models ask of the editor's toolkit (WPF on Windows, Avalonia elsewhere): a file
    /// to open, work to run once the window has drawn, when commands should check again whether
    /// they can run. Each editor sets these at start; unset, the view models do without (no dialog,
    /// work run at once).
    /// </summary>
    public static class Ui
    {
        /// <summary>Asks for a file to open: a title and a filter ("Images (*.tga;*.png)|*.tga;*.png|All files|*.*"); null if cancelled.</summary>
        public static Func<string, string, string?>? PickFile { get; set; }

        /// <summary>Runs work on the UI thread once what's pending (drawing) is done.</summary>
        public static Action<Action>? Post { get; set; }

        /// <summary>Runs work later on the UI thread, or now without a toolkit.</summary>
        public static void Later(Action work)
        {
            if (Post != null)
                Post(work);
            else
                work();
        }

        /// <summary>
        /// Subscribes to "check whether commands can run" (WPF: CommandManager.RequerySuggested);
        /// unset, commands are asked again only when they say so (<see cref="RequeryCommands"/>).
        /// </summary>
        public static Action<EventHandler>? AddRequery { get; set; }
        public static Action<EventHandler>? RemoveRequery { get; set; }

        private static event EventHandler? Requery;

        internal static void Subscribe(EventHandler handler)
        {
            if (AddRequery != null) AddRequery(handler); else Requery += handler;
        }

        internal static void Unsubscribe(EventHandler handler)
        {
            if (RemoveRequery != null) RemoveRequery(handler); else Requery -= handler;
        }

        /// <summary>Tells every command to check again whether it can run (where the toolkit doesn't).</summary>
        public static void RequeryCommands() => Requery?.Invoke(null, EventArgs.Empty);
    }
}
