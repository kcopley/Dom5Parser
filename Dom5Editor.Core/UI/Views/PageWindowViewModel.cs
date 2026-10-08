using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Editor.Session;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// An entity's page in a window of its own (the user: several entities edited at once, side by
    /// side). The same page as the main window's, on the same mod: an edit in any window shows in
    /// all, and undo is the mod's. Its links open in it, with back and forward of its own;
    /// Ctrl+click opens another window. Its entity deleted, it says so (undo brings it back).
    /// </summary>
    public sealed class PageWindowViewModel : INotifyPropertyChanged, IPageHost
    {
        private readonly MainWindowViewModel _main;
        private readonly EditorSession _session;
        private readonly List<EntityListItem> _back = new();
        private readonly List<EntityListItem> _forward = new();
        private EntityListItem? _item;
        private EntityPageViewModel? _page;
        private string? _goneNote;
        private bool _closed;

        public PageWindowViewModel(MainWindowViewModel main, EditorSession session, EntityListItem item)
        {
            _main = main;
            _session = session;
            session.Changed += OnSessionChanged;
            session.History.Changed += OnHistoryChanged;
            Show(item);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raised when the window should close (another mod was opened).</summary>
        public event Action? CloseRequested;

        /// <summary>Raised once, when the window has closed and its page let go of the mod.</summary>
        public event Action? Closed;

        /// <summary>The page shown (null while its entity is deleted).</summary>
        public EntityPageViewModel? Page => _page;

        /// <summary>The entity's row in its type's list.</summary>
        public EntityListItem? Item => _item;

        /// <summary>The window's title: the entity, its type and number, the mod.</summary>
        public string Title
        {
            get
            {
                if (_item == null)
                    return _session.Mod.ModName ?? "Dom6 Mod Editor";
                var name = _page?.Title is string t && t.Length > 0 ? t : _item.DisplayName;
                var what = Nouns.Of(_item.Type) + (_item.ID > 0 ? $" #{_item.ID}" : "");
                return $"{name} ({what}) - {_session.Mod.ModName ?? "new mod"}";
            }
        }

        /// <summary>Shown instead of the page when its entity is gone.</summary>
        public string? GoneNote => _goneNote;

        public bool IsGone => _goneNote != null;

        public string ModName => _session.Mod.ModName ?? "No name";

        public bool IsDirty => _session.History.IsDirty;
        public bool CanUndo => _session.History.CanUndo;
        public bool CanRedo => _session.History.CanRedo;
        public string UndoTip => _main.UndoTip;
        public string RedoTip => _main.RedoTip;
        public bool CanGoBack => _back.Count > 0;
        public bool CanGoForward => _forward.Count > 0;

        public string BackTip => _back.Count > 0 ? $"Back to {_back[^1].DisplayName} (Alt+Left)" : "Back to the entity shown before in this window (Alt+Left)";
        public string ForwardTip => _forward.Count > 0 ? $"Forward to {_forward[^1].DisplayName} (Alt+Right)" : "Forward again (Alt+Right)";

        public void Undo() => _main.Undo();
        public void Redo() => _main.Redo();

        public void GoBack() => Travel(_back, _forward);
        public void GoForward() => Travel(_forward, _back);

        private void Travel(List<EntityListItem> from, List<EntityListItem> to)
        {
            // (an entity deleted since is passed over)
            while (from.Count > 0)
            {
                var target = Listed(from[^1]);
                from.RemoveAt(from.Count - 1);
                if (target == null)
                    continue;
                if (_item != null && _page != null)
                    to.Add(_item);
                Show(target);
                break;
            }
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
        }

        /// <summary>The row the list has for this one's entity now (the same row, or a new one after an undo), or null if it's gone.</summary>
        private EntityListItem? Listed(EntityListItem item)
        {
            if (_main.TabOf(item.Type) is not { } tab)
                return null;
            if (tab.Lists(item))
                return item;
            return tab.Find(item.Entity) is { } again && tab.Lists(again) ? again : null;
        }

        /// <summary>The entity in the main window too (its tab, selected in the list).</summary>
        public void ShowInMainWindow()
        {
            if (_item != null)
                _main.NavigateToEntity(_item.Entity);
        }

        // ---- IPageHost: the page's links open here ----

        public void Open(EntityType type, int id)
        {
            if (Ui.WantsNewWindow?.Invoke() == true)
                _main.PopOut(type, id);
            else if (_main.FindItem(type, id) is { } item)
                Navigate(item);
            else
                _main.StatusMessage = $"{type} #{id} not found";
        }

        public void Open(IDEntity entity)
        {
            if (Ui.WantsNewWindow?.Invoke() == true)
                _main.PopOut(entity);
            else if (_main.FindItem(entity) is { } item)
                Navigate(item);
            else
                _main.StatusMessage = $"{entity.Kind} not found";
        }

        public void OpenInNewWindow(EntityListItem item) => _main.PopOut(item);

        public bool IsOwnWindow => true;

        /// <summary>Shows another entity in this window (the one before goes on the back list).</summary>
        public void Navigate(EntityListItem item)
        {
            if (ReferenceEquals(item, _item) && _page != null)
                return;
            if (_item != null)
                _back.Add(_item);
            _forward.Clear();
            Show(item);
        }

        private void Show(EntityListItem item)
        {
            DropPage();
            _item = item;
            _goneNote = null;
            _page = EntityPages.Create(_session, item);
            _page.Host = this;
            _page.PropertyChanged += OnPageChanged;
            OnPropertyChanged(string.Empty);
        }

        private void DropPage()
        {
            if (_page == null)
                return;
            _page.PropertyChanged -= OnPageChanged;
            _page.Detach();
            _page = null;
        }

        // the name edited: the title follows
        private void OnPageChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName is nameof(EntityPageViewModel.Name) or nameof(EntityPageViewModel.Title))
                OnPropertyChanged(nameof(Title));
        }

        private void OnSessionChanged(IModEdit edit)
        {
            if (_item == null || _main.TabOf(_item.Type) is null)
                return;
            // still listed; or back with a new row (an undone delete); or deleted
            var now = Listed(_item);
            if (ReferenceEquals(now, _item))
            {
                if (_page == null)
                    Show(now);
                else
                    OnPropertyChanged(nameof(Title));
                return;
            }
            if (now != null)
            {
                Show(now);
                return;
            }
            if (_goneNote != null)
                return;
            DropPage();
            _goneNote = $"{_item.DisplayName} was deleted from the mod. Undo (Ctrl+Z) brings it back.";
            OnPropertyChanged(string.Empty);
        }

        private void OnHistoryChanged()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(UndoTip));
            OnPropertyChanged(nameof(RedoTip));
            OnPropertyChanged(nameof(IsDirty));
        }

        /// <summary>Asks the window to close (another mod opened); the page lets go.</summary>
        public void Close()
        {
            CloseRequested?.Invoke();
            Detach();
        }

        /// <summary>The window closed: the page stops following the mod (nothing keeps it alive).</summary>
        public void Detach()
        {
            if (_closed)
                return;
            _closed = true;
            DropPage();
            _session.Changed -= OnSessionChanged;
            _session.History.Changed -= OnHistoryChanged;
            Closed?.Invoke();
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
