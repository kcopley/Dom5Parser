using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
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

    /// <summary>
    /// Interface for edit commands that modify entity properties.
    /// Used by ChangesMod to track property-level changes for export.
    /// </summary>
    public interface IPropertyEditCommand : IEditCommand
    {
        /// <summary>
        /// The entity being modified.
        /// </summary>
        IDEntity Entity { get; }

        /// <summary>
        /// The command/property type being modified.
        /// </summary>
        Command PropertyCommand { get; }

        /// <summary>
        /// True if this command removes a property rather than setting it.
        /// </summary>
        bool IsRemoval { get; }

        /// <summary>
        /// Gets the property after execution, or null if removed.
        /// Call this after Execute() to get the resulting property for recording.
        /// </summary>
        Property GetResultingProperty();

        /// <summary>
        /// Gets the property value that existed before this command was executed.
        /// Used for identity checking - if a value is edited back to its original,
        /// the session change can be automatically removed.
        /// Returns null if the property didn't exist before.
        /// </summary>
        Property GetOriginalProperty();
    }
}
