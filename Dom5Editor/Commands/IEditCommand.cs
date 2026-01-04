namespace Dom5Editor.Commands
{
    /// <summary>
    /// Base interface for all edit commands that modify mod data.
    /// Commands support undo/redo operations via Execute and Undo methods.
    /// </summary>
    public interface IEditCommand
    {
        /// <summary>
        /// Human-readable description of this command for display in undo/redo menus.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Executes the command, applying the change to the model.
        /// </summary>
        void Execute();

        /// <summary>
        /// Undoes the command, reverting the model to its previous state.
        /// </summary>
        void Undo();
    }
}
