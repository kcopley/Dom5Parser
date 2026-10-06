using Dom5Edit.Editing;

namespace Dom5Editor.Session
{
    /// <summary>
    /// Undo and redo over the core's edits (Dom5Edit.Editing.IModEdit), and whether the mod has
    /// changed since it was loaded or last saved.
    /// </summary>
    public sealed class EditHistory
    {
        private readonly List<IModEdit> _done = new List<IModEdit>();
        private readonly List<IModEdit> _undone = new List<IModEdit>();
        private IModEdit? _savedAt;

        /// <summary>Raised when an edit is recorded, undone or redone, or the save point moves.</summary>
        public event Action? Changed;

        public bool CanUndo => _done.Count > 0;
        public bool CanRedo => _undone.Count > 0;
        public string? UndoDescription => _done.Count > 0 ? _done[^1].Description : null;
        public string? RedoDescription => _undone.Count > 0 ? _undone[^1].Description : null;

        /// <summary>Whether the mod differs from the last save (or load): the latest edit isn't the one saved after.</summary>
        public bool IsDirty => !ReferenceEquals(_done.Count > 0 ? _done[^1] : null, _savedAt);

        public void Record(IModEdit edit)
        {
            _done.Add(edit);
            _undone.Clear();
            Changed?.Invoke();
        }

        public IModEdit? Undo()
        {
            if (_done.Count == 0)
                return null;
            var edit = _done[^1];
            _done.RemoveAt(_done.Count - 1);
            edit.Undo();
            _undone.Add(edit);
            Changed?.Invoke();
            return edit;
        }

        public IModEdit? Redo()
        {
            if (_undone.Count == 0)
                return null;
            var edit = _undone[^1];
            _undone.RemoveAt(_undone.Count - 1);
            edit.Redo();
            _done.Add(edit);
            Changed?.Invoke();
            return edit;
        }

        public void MarkSaved()
        {
            _savedAt = _done.Count > 0 ? _done[^1] : null;
            Changed?.Invoke();
        }
    }
}
