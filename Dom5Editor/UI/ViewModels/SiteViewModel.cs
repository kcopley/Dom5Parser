using System;
using System.Collections.ObjectModel;
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
        // Copy From Support (#copysite)
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
                if (value == null)
                {
                    // Remove the copysite reference
                    _entity.RemoveProperty(Command.COPYSITE);
                }
                else
                {
                    // Set the copysite reference using the Set<T> method
                    _entity.Set<SiteRef>(Command.COPYSITE, p => p.Parse(Command.COPYSITE, value.Value.ToString(), ""));
                }
                OnPropertyChanged(nameof(CopySiteId));
                OnPropertyChanged(nameof(CopySiteName));
            }
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
                        return idEntity.Name ?? $"Site #{idEntity.ID}";
                    return prop.Name ?? $"Site #{prop.ID}";
                }
                return null;
            }
        }

        // ========================================
        // Sprite Support (#look command)
        // ========================================

        public int? LookSprite
        {
            get => GetIntProperty(Command.LOOK);
            set => SetIntProperty(Command.LOOK, value);
        }
        public bool IsLookSpriteModified => IsIntPropertyModifiedFromVanilla(Command.LOOK);
        public bool IsLookSpriteSessionEdit => IsPropertyEditedInSession(Command.LOOK);

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
        // Core Stats
        // ========================================

        public int? Path
        {
            get => GetIntProperty(Command.PATH);
            set => SetIntProperty(Command.PATH, value);
        }
        public bool IsPathModified => IsIntPropertyModifiedFromVanilla(Command.PATH);
        public bool IsPathSessionEdit => IsPropertyEditedInSession(Command.PATH);
        public bool IsPathInherited => IsIntPropertyInherited(Command.PATH);

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

        public int? Level
        {
            get => GetIntProperty(Command.LEVEL);
            set => SetIntProperty(Command.LEVEL, value);
        }
        public bool IsLevelModified => IsIntPropertyModifiedFromVanilla(Command.LEVEL);
        public bool IsLevelSessionEdit => IsPropertyEditedInSession(Command.LEVEL);
        public bool IsLevelInherited => IsIntPropertyInherited(Command.LEVEL);

        public int? Rarity
        {
            get => GetIntProperty(Command.RARITY);
            set => SetIntProperty(Command.RARITY, value);
        }
        public bool IsRarityModified => IsIntPropertyModifiedFromVanilla(Command.RARITY);
        public bool IsRaritySessionEdit => IsPropertyEditedInSession(Command.RARITY);
        public bool IsRarityInherited => IsIntPropertyInherited(Command.RARITY);

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
        // Gems (IntIntProperty)
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
        // Single Unified Badge Collection
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;
        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

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
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.PATH => "Path",
                Command.LEVEL => "Level",
                Command.RARITY => "Rarity",
                Command.GOLD => "Gold",
                Command.RES => "Research",
                Command.SUPPLY => "Supply",
                Command.LOOK => "LookSprite",
                Command.COPYSITE => "CopySite",
                _ => null
            };
        }
    }
}
