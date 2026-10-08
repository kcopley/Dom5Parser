using Dom5Edit.Entities;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// The window a page is in: the main window's tab, or a window of its own (the user: entities
    /// side by side, several edited at once). Its links open there; Ctrl+click opens a new window.
    /// </summary>
    public interface IPageHost
    {
        /// <summary>Opens an entity from a link on the page.</summary>
        void Open(EntityType type, int id);

        /// <summary>Opens an entity that may have no number (an event).</summary>
        void Open(IDEntity entity);

        /// <summary>Opens the entity in a window of its own.</summary>
        void OpenInNewWindow(EntityListItem item);

        /// <summary>Whether this is a page's own window (not the main window).</summary>
        bool IsOwnWindow { get; }
    }
}
