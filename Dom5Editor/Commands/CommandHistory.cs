using System;
using System.Collections.Generic;

namespace Dom5Editor.Commands
{
    /// <summary>
    /// Manages undo/redo stacks for edit commands.
    /// Each document (mod) should have its own CommandHistory instance.
    /// </summary>
    public class CommandHistory
    {
        private readonly Stack<IEditCommand> _undoStack = new Stack<IEditCommand>();
        private readonly Stack<IEditCommand> _redoStack = new Stack<IEditCommand>();
        private int _savePoint = 0;

        /// <summary>
        /// Raised when the command history changes (after execute, undo, or redo).
        /// </summary>
        public event Action HistoryChanged;

        /// <summary>
        /// Returns true if there are commands that can be undone.
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Returns true if there are commands that can be redone.
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Returns true if the document has unsaved changes.
        /// Compares current stack depth against the save point.
        /// </summary>
        public bool IsDirty => _undoStack.Count != _savePoint;

        /// <summary>
        /// Gets the description of the command that would be undone.
        /// </summary>
        public string UndoDescription => CanUndo ? _undoStack.Peek().Description : null;

        /// <summary>
        /// Gets the description of the command that would be redone.
        /// </summary>
        public string RedoDescription => CanRedo ? _redoStack.Peek().Description : null;

        /// <summary>
        /// Gets the number of commands in the undo stack.
        /// </summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>
        /// Gets the number of commands in the redo stack.
        /// </summary>
        public int RedoCount => _redoStack.Count;

        /// <summary>
        /// Executes a command, pushing it onto the undo stack.
        /// Clears the redo stack since we've branched from the previous history.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Execute(IEditCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            OnHistoryChanged();
        }

        /// <summary>
        /// Undoes the most recent command, moving it to the redo stack.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
                return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            OnHistoryChanged();
        }

        /// <summary>
        /// Redoes the most recently undone command, moving it back to the undo stack.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            OnHistoryChanged();
        }

        /// <summary>
        /// Marks the current state as saved. IsDirty will return false
        /// until further changes are made.
        /// </summary>
        public void MarkSaved()
        {
            _savePoint = _undoStack.Count;
            OnHistoryChanged();
        }

        /// <summary>
        /// Clears all history. Use when loading a new document.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _savePoint = 0;
            OnHistoryChanged();
        }

        private void OnHistoryChanged()
        {
            HistoryChanged?.Invoke();
        }
    }
}
