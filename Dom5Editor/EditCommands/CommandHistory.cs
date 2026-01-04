using System;
using System.Collections.Generic;
using Dom5Edit;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Manages undo/redo stacks for edit commands.
    /// Each document (mod) should have its own CommandHistory instance.
    /// Optionally integrates with ChangesMod to track property changes for export.
    /// </summary>
    public class CommandHistory
    {
        private readonly Stack<IEditCommand> _undoStack = new Stack<IEditCommand>();
        private readonly Stack<IEditCommand> _redoStack = new Stack<IEditCommand>();
        private int _savePoint = 0;

        /// <summary>
        /// Optional ChangesMod for recording property changes.
        /// When set, property edit commands will record their changes here.
        /// </summary>
        public ChangesMod ChangesMod { get; set; }

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
        /// If ChangesMod is set, records property changes for export.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Execute(IEditCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();

            // Record to ChangesMod if available and applicable
            RecordPropertyChange(command);

            OnHistoryChanged();
        }

        /// <summary>
        /// Undoes the most recent command, moving it to the redo stack.
        /// If ChangesMod is set, reverts the property change.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
                return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);

            // Revert the change in ChangesMod
            RevertPropertyChange(command);

            OnHistoryChanged();
        }

        /// <summary>
        /// Redoes the most recently undone command, moving it back to the undo stack.
        /// If ChangesMod is set, re-records the property change.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);

            // Re-record the change in ChangesMod
            RecordPropertyChange(command);

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

        /// <summary>
        /// Records a property change to ChangesMod if applicable.
        /// </summary>
        private void RecordPropertyChange(IEditCommand command)
        {
            if (ChangesMod == null)
                return;

            if (command is IPropertyEditCommand propCommand)
            {
                var entity = propCommand.Entity;
                if (entity == null)
                    return;

                if (propCommand.IsRemoval)
                {
                    // Record property removal
                    ChangesMod.RecordPropertyRemoval(entity, propCommand.PropertyCommand);
                }
                else
                {
                    // Record property change with the resulting property
                    var property = propCommand.GetResultingProperty();
                    if (property != null)
                    {
                        ChangesMod.RecordPropertyChange(entity, property);
                    }
                }
            }
        }

        /// <summary>
        /// Reverts a property change in ChangesMod when undoing.
        /// This uses ChangesMod.RevertProperty to clear the recorded change.
        /// </summary>
        private void RevertPropertyChange(IEditCommand command)
        {
            if (ChangesMod == null)
                return;

            if (command is IPropertyEditCommand propCommand)
            {
                var entity = propCommand.Entity;
                if (entity == null)
                    return;

                // Get entity changes and revert this specific property
                var entityType = GetEntityType(entity);
                if (ChangesMod.TryGetChanges(entityType, entity.ID, out var changes))
                {
                    changes.RevertProperty(propCommand.PropertyCommand);
                }
            }
        }

        /// <summary>
        /// Gets the EntityType for an entity (mirroring ChangesMod logic).
        /// </summary>
        private static Dom5Edit.Entities.EntityType GetEntityType(Dom5Edit.Entities.IDEntity entity)
        {
            return entity switch
            {
                Dom5Edit.Entities.Monster _ => Dom5Edit.Entities.EntityType.MONSTER,
                Dom5Edit.Entities.Weapon _ => Dom5Edit.Entities.EntityType.WEAPON,
                Dom5Edit.Entities.Armor _ => Dom5Edit.Entities.EntityType.ARMOR,
                Dom5Edit.Entities.Spell _ => Dom5Edit.Entities.EntityType.SPELL,
                Dom5Edit.Entities.Item _ => Dom5Edit.Entities.EntityType.ITEM,
                Dom5Edit.Entities.Site _ => Dom5Edit.Entities.EntityType.SITE,
                Dom5Edit.Entities.Nation _ => Dom5Edit.Entities.EntityType.NATION,
                Dom5Edit.Entities.Event _ => Dom5Edit.Entities.EntityType.EVENT,
                Dom5Edit.Entities.Mercenary _ => Dom5Edit.Entities.EntityType.MERCENARY,
                Dom5Edit.Entities.Nametype _ => Dom5Edit.Entities.EntityType.NAMETYPE,
                Dom5Edit.Entities.Poptype _ => Dom5Edit.Entities.EntityType.POPTYPE,
                _ => throw new ArgumentException($"Unknown entity type: {entity.GetType()}")
            };
        }
    }
}
