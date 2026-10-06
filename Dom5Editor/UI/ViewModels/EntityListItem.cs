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

        /// <summary>Where the list sorts it "by ID": its ID, or for entities with none (events), after them in file order.</summary>
        public int SortKey => HasNumber ? ID : int.MaxValue / 2 + Order;

        /// <summary>Whether it has a number in game: an ID, or a game event's number (event 0 is one; a new event has none).</summary>
        public bool HasNumber => Type == EntityType.EVENT ? ID >= 0 : ID > 0;

        /// <summary>Its place among the entities without an ID (their order in the file).</summary>
        internal int Order { get; set; }

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

        // worked out when a row is first shown (the list is virtualized), again after an edit

        internal Func<EntityListItem, string>? DetailProvider { get; set; }
        internal Func<EntityListItem, string, bool>? FacetMatcher { get; set; }
        internal Func<EntityListItem, string, bool>? TextMatcher { get; set; }

        /// <summary>Whether the search text is in more than its name: an event's message.</summary>
        public bool MatchesText(string text) => TextMatcher?.Invoke(this, text) == true;

        /// <summary>Whether the row is in one of its type's own filters ("Rituals", "In a chain"); "All" always.</summary>
        public bool InFacet(string facet) => facet == "All" || FacetMatcher?.Invoke(this, facet) != false;
        internal Func<EntityListItem, System.Windows.Media.ImageSource?>? SpriteProvider { get; set; }
        private string? _detail;
        private System.Windows.Media.ImageSource? _sprite;
        private bool _spriteDone;

        /// <summary>The second line: ID and key stats (a monster's HP, attack, ...; a weapon's damage).</summary>
        public string Detail => _detail ??= (HasNumber ? $"#{ID}" : "") + (DetailProvider?.Invoke(this) is string d && d.Length > 0 ? (HasNumber ? "   " : "") + d : "");

        /// <summary>A small sprite (monsters, items, sites), or null.</summary>
        public System.Windows.Media.ImageSource? Sprite
        {
            get
            {
                if (!_spriteDone)
                {
                    _sprite = SpriteProvider?.Invoke(this);
                    _spriteDone = true;
                }
                return _sprite;
            }
        }

        /// <summary>After an edit to the entity: work the detail and sprite out again when shown.</summary>
        internal void Refresh()
        {
            _detail = null;
            _spriteDone = false;
            OnPropertyChanged(nameof(Detail));
            OnPropertyChanged(nameof(Sprite));
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public override string ToString() => $"{DisplayName} #{ID}";
    }
}
