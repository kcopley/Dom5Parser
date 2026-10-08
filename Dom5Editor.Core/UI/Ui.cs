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

        /// <summary>
        /// Whether the click (or key) that's being handled asks for a new window: Ctrl (Cmd on a
        /// Mac) held. A link clicked so opens its entity in a window of its own.
        /// </summary>
        public static Func<bool>? WantsNewWindow { get; set; }

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

        // who listens for "check whether commands can run" (a command's CanExecuteChanged), held
        // weakly by their object (as WPF's CommandManager did): a button that's gone mustn't keep
        // its page alive. (Avalonia's buttons don't always stop listening when they leave the
        // window: a sweep of 33 pages kept 730 of them, and every page they showed.) Commands are
        // asked again only when they say so (RequeryCommands).
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, List<EventHandler>> _requery = new();
        private static readonly List<EventHandler> _requeryStatic = new();

        internal static void Subscribe(EventHandler handler)
        {
            if (handler.Target is object target)
                _requery.GetOrCreateValue(target).Add(handler);
            else
                _requeryStatic.Add(handler);
        }

        internal static void Unsubscribe(EventHandler handler)
        {
            if (handler.Target is object target)
            {
                if (_requery.TryGetValue(target, out var list))
                    list.Remove(handler);
            }
            else
                _requeryStatic.Remove(handler);
        }

        private static List<EventHandler> RequeryHandlers() =>
            _requery.SelectMany(kv => kv.Value).Concat(_requeryStatic).ToList();

        /// <summary>How many listen for "check again" (the snapshot sweep checks closed pages let go).</summary>
        internal static int RequeryListeners => RequeryHandlers().Count;

        /// <summary>Tells every command to check again whether it can run.</summary>
        public static void RequeryCommands()
        {
            foreach (var handler in RequeryHandlers())
                handler(null, EventArgs.Empty);
        }
    }
}
