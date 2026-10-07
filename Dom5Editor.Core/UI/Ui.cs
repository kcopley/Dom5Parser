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
        /// <summary>
        /// Asks for a file to open: a title, a filter ("Images (*.tga;*.png)|*.tga;*.png|All files|*.*")
        /// and what to do with the file chosen (not called if cancelled; called later where dialogs
        /// don't block, as on Mac and Linux).
        /// </summary>
        public static Action<string, string, Action<string>>? PickFile { get; set; }

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

        // the listeners without a toolkit's own list, held weakly by their object (as WPF's
        // CommandManager does): a button that's gone mustn't keep its page alive. (Avalonia's
        // buttons don't always stop listening when they leave the window: a sweep of 33 pages
        // kept 730 of them, and every page they showed.)
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, List<EventHandler>> _requery = new();
        private static readonly List<EventHandler> _requeryStatic = new();

        internal static void Subscribe(EventHandler handler)
        {
            if (AddRequery != null)
                AddRequery(handler);
            else if (handler.Target is object target)
                _requery.GetOrCreateValue(target).Add(handler);
            else
                _requeryStatic.Add(handler);
        }

        internal static void Unsubscribe(EventHandler handler)
        {
            if (RemoveRequery != null)
                RemoveRequery(handler);
            else if (handler.Target is object target)
            {
                if (_requery.TryGetValue(target, out var list))
                    list.Remove(handler);
            }
            else
                _requeryStatic.Remove(handler);
        }

        private static List<EventHandler> RequeryHandlers() =>
            _requery.SelectMany(kv => kv.Value).Concat(_requeryStatic).ToList();

        /// <summary>How many listen for "check again" without the toolkit (the snapshot sweep checks closed pages let go).</summary>
        internal static int RequeryListeners => RequeryHandlers().Count;

        /// <summary>Tells every command to check again whether it can run (where the toolkit doesn't).</summary>
        public static void RequeryCommands()
        {
            foreach (var handler in RequeryHandlers())
                handler(null, EventArgs.Empty);
        }
    }
}
