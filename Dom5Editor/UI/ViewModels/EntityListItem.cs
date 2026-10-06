using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit.Entities;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// One row in an entity list: which entity, its name in game, and whether the mod touches it.
    /// Light on purpose (thousands of vanilla rows); the entity's page is built when it's selected.
    /// EntityListControl filters on IsVanilla / IsModified / IsNew and searches DisplayName and ID.
    /// </summary>
    public sealed class EntityListItem : INotifyPropertyChanged
    {
        private string _displayName;
        private bool _isModified;

        public EntityListItem(EntityType type, IDEntity entity, string displayName, bool isVanilla, bool isModified)
        {
            Type = type;
            Entity = entity;
            _displayName = displayName;
            IsVanilla = isVanilla;
            _isModified = isModified;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public EntityType Type { get; }

        /// <summary>The entity: the mod's, or the vanilla one when the mod doesn't touch it (edits go through the session either way).</summary>
        public IDEntity Entity { get; internal set; }

        public int ID => Entity.ID;

        public string DisplayName
        {
            get => _displayName;
            internal set { if (_displayName != value) { _displayName = value; OnPropertyChanged(); } }
        }

        /// <summary>A vanilla entity (changed by the mod or not).</summary>
        public bool IsVanilla { get; }

        /// <summary>A vanilla entity the mod changes.</summary>
        public bool IsModified
        {
            get => _isModified;
            internal set { if (_isModified != value) { _isModified = value; OnPropertyChanged(); OnPropertyChanged(nameof(SourceLabel)); } }
        }

        /// <summary>An entity the mod adds.</summary>
        public bool IsNew => !IsVanilla;

        public string SourceLabel => IsNew ? "New" : IsModified ? "Changed" : "Vanilla";

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public override string ToString() => $"{DisplayName} #{ID}";
    }
}
