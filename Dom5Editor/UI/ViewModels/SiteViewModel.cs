using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for Site entities.
    /// All properties are now JSON-driven via badge panels.
    /// </summary>
    public class SiteViewModel : EntityViewModel
    {
        public SiteViewModel(Site entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Site Site => (Site)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from site_badges.json.
        /// </summary>
        protected override string EntityTypeName => "site";

        // ========================================
        // Copy From Support (#copysite) - editable
        // ========================================

        /// <summary>
        /// Gets or sets the CopySite reference ID.
        /// </summary>
        public int? CopySiteId
        {
            get
            {
                var result = _entity.TryGet<SiteRef>(Command.COPYSITE, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                    return prop.ID;
                return null;
            }
            set
            {
                if (value == null || value == 0)
                {
                    _entity.RemoveProperty(Command.COPYSITE);
                }
                else
                {
                    _entity.Set<SiteRef>(Command.COPYSITE, p => p.Parse(Command.COPYSITE, value.Value.ToString(), ""));
                    if (_entity.TryGet<SiteRef>(Command.COPYSITE, out var prop) == ReturnType.TRUE)
                        RecordPropertyChangeInSession(prop);
                }
                OnPropertyChanged(nameof(CopySiteId));
                OnPropertyChanged(nameof(CopySiteName));
                OnPropertyChanged(nameof(HasCopySite));

                // Refresh all properties that inherit from copysite
                RefreshAllCopyDependentProperties();
            }
        }

        /// <summary>
        /// Refreshes all properties and collections that depend on copysite inheritance.
        /// </summary>
        private void RefreshAllCopyDependentProperties()
        {
            RefreshIdentityBadges();
            RefreshPropertyBadges();
        }

        /// <summary>
        /// Gets the CopySite reference name for display.
        /// </summary>
        public string CopySiteName
        {
            get
            {
                var result = _entity.TryGet<SiteRef>(Command.COPYSITE, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"#{idEntity.ID}";
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        public bool HasCopySite => CopySiteId.HasValue;

        // Cached reference items for copy site selector
        private List<ReferenceItem> _availableSitesForCopy;

        /// <summary>
        /// Gets the available sites as ReferenceItems for the copy site selector.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableSitesForCopy
        {
            get
            {
                if (_availableSitesForCopy == null)
                {
                    _availableSitesForCopy = CachedSites
                        .Where(s => s.ID != ID) // Exclude self
                        .Select(s => new ReferenceItem { ID = s.ID, DisplayName = s.Name, Tag = s })
                        .ToList();
                }
                return _availableSitesForCopy;
            }
        }

        // ========================================
        // Sprite Support (derived from badge data)
        // ========================================

        /// <summary>
        /// Gets the site sprite image if available.
        /// Sites use #look to reference a sprite number.
        /// </summary>
        public System.Windows.Media.ImageSource SpriteImage
        {
            get
            {
                // Site sprites would need to be loaded from game data
                // For now, return null - can be implemented when sprite loading is added
                return null;
            }
        }

        // ========================================
        // Derived Display Properties (kept for header)
        // ========================================

        /// <summary>
        /// Gets the path value for display.
        /// </summary>
        private int? Path => GetIntProperty(Command.PATH);

        /// <summary>
        /// Gets the path display name.
        /// </summary>
        public string PathDisplay
        {
            get
            {
                return Path switch
                {
                    0 => "Fire",
                    1 => "Air",
                    2 => "Water",
                    3 => "Earth",
                    4 => "Astral",
                    5 => "Death",
                    6 => "Nature",
                    7 => "Blood",
                    8 => "Holy",
                    _ => Path?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Gets the path letter for icon lookup (F, A, W, E, S, D, N, B, H).
        /// </summary>
        public string PathLetter
        {
            get
            {
                return Path switch
                {
                    0 => "F",
                    1 => "A",
                    2 => "W",
                    3 => "E",
                    4 => "S",
                    5 => "D",
                    6 => "N",
                    7 => "B",
                    8 => "H",
                    _ => null
                };
            }
        }

        /// <summary>
        /// Gets the level value for display in header.
        /// </summary>
        public int? Level => GetIntProperty(Command.LEVEL);

        /// <summary>
        /// Gets the rarity value for display.
        /// </summary>
        private int? Rarity => GetIntProperty(Command.RARITY);

        /// <summary>
        /// Gets the rarity display name.
        /// </summary>
        public string RarityDisplay
        {
            get
            {
                return Rarity switch
                {
                    0 => "Common",
                    1 => "Uncommon",
                    2 => "Rare",
                    5 => "Never Random",
                    _ => Rarity?.ToString() ?? "-"
                };
            }
        }

        // ========================================
        // Gems (IntIntProperty) - Computed Display
        // ========================================

        /// <summary>
        /// Gets the gem income display text.
        /// #gems takes two parameters: gem type and amount.
        /// </summary>
        public string GemsDisplay
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    var gemType = prop.Value1 switch
                    {
                        0 => "Fire",
                        1 => "Air",
                        2 => "Water",
                        3 => "Earth",
                        4 => "Astral",
                        5 => "Death",
                        6 => "Nature",
                        7 => "Blood",
                        _ => $"Type {prop.Value1}"
                    };
                    return $"{prop.Value2} {gemType}";
                }
                return null;
            }
        }

        public bool HasGems
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out _);
                return result == ReturnType.TRUE;
            }
        }

        /// <summary>
        /// Gets the gem type letter for icon lookup (F, A, W, E, S, D, N, B).
        /// Returns null if no gems defined.
        /// </summary>
        public string GemLetter
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.Value1 switch
                    {
                        0 => "F",
                        1 => "A",
                        2 => "W",
                        3 => "E",
                        4 => "S",
                        5 => "D",
                        6 => "N",
                        7 => "B",
                        _ => null
                    };
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the gem amount.
        /// </summary>
        public int? GemAmount
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                    return prop.Value2;
                return null;
            }
        }

        /// <summary>
        /// Gets the gem type display name (Fire, Air, etc.).
        /// </summary>
        public string GemPathDisplay
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.Value1 switch
                    {
                        0 => "Fire",
                        1 => "Air",
                        2 => "Water",
                        3 => "Earth",
                        4 => "Astral",
                        5 => "Death",
                        6 => "Nature",
                        7 => "Blood",
                        _ => $"Type {prop.Value1}"
                    };
                }
                return null;
            }
        }

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _identityBadges;
        private ObservableCollection<AvailablePropertyItem> _availableIdentityBadges;
        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> IdentityBadges
        {
            get { if (_identityBadges == null) RefreshIdentityBadges(); return _identityBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableIdentityBadges
        {
            get { if (_availableIdentityBadges == null) RefreshIdentityBadges(); return _availableIdentityBadges; }
        }

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeIdentityBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addIdentityBadgeCommand;
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemoveIdentityBadgeCommand => _removeIdentityBadgeCommand ??= CreateRemoveBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<AvailablePropertyItem> AddIdentityBadgeCommand => _addIdentityBadgeCommand ??= CreateAddBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshIdentityBadges()
        {
            var (active, available) = BuildBadgesFromSection("identity", BadgeValueChangedHandler);
            _identityBadges = active;
            _availableIdentityBadges = available;
            OnPropertyChanged(nameof(IdentityBadges));
            OnPropertyChanged(nameof(AvailableIdentityBadges));
            // Header display depends on identity badges
            OnPropertyChanged(nameof(PathDisplay));
            OnPropertyChanged(nameof(PathLetter));
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(RarityDisplay));
        }

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh all badge collections on undo/redo
            RefreshIdentityBadges();
            RefreshPropertyBadges();
        }
    }
}
