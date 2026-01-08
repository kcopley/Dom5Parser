using System;
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
    /// ViewModel for Spell entities.
    /// </summary>
    public class SpellViewModel : EntityViewModel
    {
        public SpellViewModel(Spell entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Spell Spell => (Spell)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from spell_badges.json.
        /// </summary>
        protected override string EntityTypeName => "spell";

        // ========================================
        // Copy Spell Support (#copyspell)
        // ========================================

        /// <summary>
        /// Gets the CopySpell reference display text.
        /// </summary>
        public string CopySpellDisplay
        {
            get
            {
                var result = _entity.TryGet<CopySpellRef>(Command.COPYSPELL, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasCopySpell
        {
            get
            {
                var result = _entity.TryGet<CopySpellRef>(Command.COPYSPELL, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        /// <summary>
        /// Gets the CopySpell reference ID for navigation.
        /// </summary>
        public int CopySpellId
        {
            get
            {
                var result = _entity.TryGet<CopySpellRef>(Command.COPYSPELL, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.ID;
                    return prop.ID;
                }
                return 0;
            }
        }

        // ========================================
        // Derived Display Properties (used in header/special displays)
        // ========================================

        /// <summary>
        /// Gets the school display name for header display.
        /// Uses layered property access via badge system.
        /// </summary>
        public string SchoolDisplay
        {
            get
            {
                var school = GetIntProperty(Command.SCHOOL);
                return school switch
                {
                    -1 => "Not Researchable",
                    0 => "Conjuration",
                    1 => "Alteration",
                    2 => "Evocation",
                    3 => "Construction",
                    4 => "Enchantment",
                    5 => "Thaumaturgy",
                    6 => "Blood",
                    _ => school?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Research level for header display.
        /// </summary>
        public int? ResearchLevel => GetIntProperty(Command.RESEARCHLEVEL);

        // ========================================
        // Path Requirements (IntIntProperty: path slot, path type)
        // ========================================

        /// <summary>
        /// Gets the primary path requirement display.
        /// #path takes two parameters: requirement slot (0 or 1) and path number.
        /// </summary>
        public string PrimaryPathDisplay
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var primaryPath = allPaths.FirstOrDefault(p => p.Value1 == 0);
                if (primaryPath != null)
                {
                    return GetPathDisplayName(primaryPath.Value2);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the primary path level requirement.
        /// #pathlevel takes two parameters: requirement slot (0 or 1) and level.
        /// </summary>
        public int? PrimaryPathLevel
        {
            get
            {
                var allLevels = Spell.Properties.Where(p => p.Command == Command.PATHLEVEL)
                    .Cast<IntIntProperty>().ToList();
                var primaryLevel = allLevels.FirstOrDefault(p => p.Value1 == 0);
                return primaryLevel?.Value2;
            }
        }

        /// <summary>
        /// Gets the secondary path requirement display.
        /// </summary>
        public string SecondaryPathDisplay
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var secondaryPath = allPaths.FirstOrDefault(p => p.Value1 == 1);
                if (secondaryPath != null)
                {
                    return GetPathDisplayName(secondaryPath.Value2);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the secondary path level requirement.
        /// </summary>
        public int? SecondaryPathLevel
        {
            get
            {
                var allLevels = Spell.Properties.Where(p => p.Command == Command.PATHLEVEL)
                    .Cast<IntIntProperty>().ToList();
                var secondaryLevel = allLevels.FirstOrDefault(p => p.Value1 == 1);
                return secondaryLevel?.Value2;
            }
        }

        /// <summary>
        /// Whether the spell has path requirements.
        /// </summary>
        public bool HasPathRequirements => PrimaryPathDisplay != null || SecondaryPathDisplay != null;

        /// <summary>
        /// Gets the path letter for icon lookup (F, A, W, E, S, D, N, G, B).
        /// </summary>
        public string PrimaryPathLetter
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var primaryPath = allPaths.FirstOrDefault(p => p.Value1 == 0);
                if (primaryPath != null)
                {
                    return GetPathLetter(primaryPath.Value2);
                }
                return null;
            }
        }

        public string SecondaryPathLetter
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var secondaryPath = allPaths.FirstOrDefault(p => p.Value1 == 1);
                if (secondaryPath != null)
                {
                    return GetPathLetter(secondaryPath.Value2);
                }
                return null;
            }
        }

        private static string GetPathDisplayName(int pathId)
        {
            return pathId switch
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
                _ => pathId.ToString()
            };
        }

        private static string GetPathLetter(int pathId)
        {
            return pathId switch
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

        // ========================================
        // Fatigue/Gem Cost Display (derived from fatiguecost property)
        // ========================================

        /// <summary>
        /// Gets the decoded gem cost (fatigueCost / 1000).
        /// </summary>
        public int GemCost => (GetIntProperty(Command.FATIGUECOST) ?? 0) / 1000;

        /// <summary>
        /// Gets the decoded fatigue only (fatigueCost % 1000).
        /// </summary>
        public int FatigueOnly => (GetIntProperty(Command.FATIGUECOST) ?? 0) % 1000;

        /// <summary>
        /// Gets a display string for the fatigue cost.
        /// </summary>
        public string FatigueCostDisplay
        {
            get
            {
                var cost = GetIntProperty(Command.FATIGUECOST) ?? 0;
                if (cost == 0) return "-";
                var gems = cost / 1000;
                var fatigue = cost % 1000;
                if (gems > 0 && fatigue > 0)
                    return $"{gems}G + {fatigue}F";
                if (gems > 0)
                    return $"{gems}G";
                return $"{fatigue}F";
            }
        }

        // ========================================
        // Next Spell Chain
        // ========================================

        /// <summary>
        /// Gets the NextSpell reference display text.
        /// </summary>
        public string NextSpellDisplay
        {
            get
            {
                var result = _entity.TryGet<SpellRef>(Command.NEXTSPELL, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasNextSpell
        {
            get
            {
                var result = _entity.TryGet<SpellRef>(Command.NEXTSPELL, out _);
                return result == ReturnType.TRUE;
            }
        }

        /// <summary>
        /// Gets the NextSpell reference ID for navigation.
        /// </summary>
        public int NextSpellId
        {
            get
            {
                var result = _entity.TryGet<SpellRef>(Command.NEXTSPELL, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.ID;
                    return prop.ID;
                }
                return 0;
            }
        }

        // ========================================
        // Research Badge Collection (JSON-driven)
        // ========================================

        private ObservableCollection<PropertyItem> _researchBadges;
        private ObservableCollection<AvailablePropertyItem> _availableResearchBadges;

        public ObservableCollection<PropertyItem> ResearchBadges
        {
            get { if (_researchBadges == null) RefreshResearchBadges(); return _researchBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableResearchBadges
        {
            get { if (_availableResearchBadges == null) RefreshResearchBadges(); return _availableResearchBadges; }
        }

        // Commands for research badge operations
        private RelayCommand<PropertyItem> _removeResearchBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResearchBadgeCommand;

        public RelayCommand<PropertyItem> RemoveResearchBadgeCommand => _removeResearchBadgeCommand ??= CreateRemoveBadgeCommand(RefreshResearchBadges);
        public RelayCommand<AvailablePropertyItem> AddResearchBadgeCommand => _addResearchBadgeCommand ??= CreateAddBadgeCommand(RefreshResearchBadges);

        private void RefreshResearchBadges()
        {
            var (active, available) = BuildBadgesFromSection("research", BadgeValueChangedHandler);
            _researchBadges = active;
            _availableResearchBadges = available;
            OnPropertyChanged(nameof(ResearchBadges));
            OnPropertyChanged(nameof(AvailableResearchBadges));
            // Update derived properties that depend on research stats
            OnPropertyChanged(nameof(SchoolDisplay));
            OnPropertyChanged(nameof(ResearchLevel));
            OnPropertyChanged(nameof(FatigueCostDisplay));
            OnPropertyChanged(nameof(GemCost));
            OnPropertyChanged(nameof(FatigueOnly));
        }

        // ========================================
        // Combat Stats Badge Collection (JSON-driven)
        // ========================================

        private ObservableCollection<PropertyItem> _combatBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCombatBadges;

        public ObservableCollection<PropertyItem> CombatBadges
        {
            get { if (_combatBadges == null) RefreshCombatBadges(); return _combatBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCombatBadges
        {
            get { if (_availableCombatBadges == null) RefreshCombatBadges(); return _availableCombatBadges; }
        }

        // Commands for combat badge operations
        private RelayCommand<PropertyItem> _removeCombatBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCombatBadgeCommand;

        public RelayCommand<PropertyItem> RemoveCombatBadgeCommand => _removeCombatBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCombatBadges);
        public RelayCommand<AvailablePropertyItem> AddCombatBadgeCommand => _addCombatBadgeCommand ??= CreateAddBadgeCommand(RefreshCombatBadges);

        private void RefreshCombatBadges()
        {
            var (active, available) = BuildBadgesFromSection("combat", BadgeValueChangedHandler);
            _combatBadges = active;
            _availableCombatBadges = available;
            OnPropertyChanged(nameof(CombatBadges));
            OnPropertyChanged(nameof(AvailableCombatBadges));
        }

        // ========================================
        // Properties Badge Collection (JSON-driven)
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

        // Commands for property badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler for all badge sections
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
            // Refresh the appropriate badge collections when undo/redo affects this entity
            RefreshResearchBadges();
            RefreshCombatBadges();
            RefreshPropertyBadges();
        }
    }
}
