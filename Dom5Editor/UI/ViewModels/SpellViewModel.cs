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

        // ========================================
        // Core Stats (School, Research Level, Path)
        // ========================================

        /// <summary>
        /// Magic school: 0=Conj, 1=Alt, 2=Evo, 3=Const, 4=Ench, 5=Thau, 6=Blood, -1=not researchable
        /// </summary>
        public int? School
        {
            get => GetIntProperty(Command.SCHOOL);
            set => SetIntProperty(Command.SCHOOL, value);
        }
        public bool IsSchoolModified => IsIntPropertyModifiedFromVanilla(Command.SCHOOL);
        public bool IsSchoolSessionEdit => IsPropertyEditedInSession(Command.SCHOOL);
        public bool IsSchoolInherited => IsIntPropertyInherited(Command.SCHOOL);

        /// <summary>
        /// Gets the school display name.
        /// </summary>
        public string SchoolDisplay
        {
            get
            {
                return School switch
                {
                    -1 => "Not Researchable",
                    0 => "Conjuration",
                    1 => "Alteration",
                    2 => "Evocation",
                    3 => "Construction",
                    4 => "Enchantment",
                    5 => "Thaumaturgy",
                    6 => "Blood",
                    _ => School?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Research level required to learn this spell.
        /// </summary>
        public int? ResearchLevel
        {
            get => GetIntProperty(Command.RESEARCHLEVEL);
            set => SetIntProperty(Command.RESEARCHLEVEL, value);
        }
        public bool IsResearchLevelModified => IsIntPropertyModifiedFromVanilla(Command.RESEARCHLEVEL);
        public bool IsResearchLevelSessionEdit => IsPropertyEditedInSession(Command.RESEARCHLEVEL);
        public bool IsResearchLevelInherited => IsIntPropertyInherited(Command.RESEARCHLEVEL);

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
        // Fatigue/Gem Cost
        // ========================================

        /// <summary>
        /// Fatigue cost for this spell.
        /// Encoded as gems*1000 + fatigue (e.g., 2050 = 2 gems + 50 fatigue).
        /// </summary>
        public int? FatigueCost
        {
            get => GetIntProperty(Command.FATIGUECOST);
            set => SetIntProperty(Command.FATIGUECOST, value);
        }
        public bool IsFatigueCostModified => IsIntPropertyModifiedFromVanilla(Command.FATIGUECOST);
        public bool IsFatigueCostSessionEdit => IsPropertyEditedInSession(Command.FATIGUECOST);
        public bool IsFatigueCostInherited => IsIntPropertyInherited(Command.FATIGUECOST);

        /// <summary>
        /// Gets the decoded gem cost (fatigueCost / 1000).
        /// </summary>
        public int GemCost => (FatigueCost ?? 0) / 1000;

        /// <summary>
        /// Gets the decoded fatigue only (fatigueCost % 1000).
        /// </summary>
        public int FatigueOnly => (FatigueCost ?? 0) % 1000;

        /// <summary>
        /// Gets a display string for the fatigue cost.
        /// </summary>
        public string FatigueCostDisplay
        {
            get
            {
                var cost = FatigueCost ?? 0;
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
        // Battle Stats
        // ========================================

        public int? Range
        {
            get => GetIntProperty(Command.RANGE);
            set => SetIntProperty(Command.RANGE, value);
        }
        public bool IsRangeModified => IsIntPropertyModifiedFromVanilla(Command.RANGE);
        public bool IsRangeSessionEdit => IsPropertyEditedInSession(Command.RANGE);
        public bool IsRangeInherited => IsIntPropertyInherited(Command.RANGE);

        public int? Precision
        {
            get => GetIntProperty(Command.PRECISION);
            set => SetIntProperty(Command.PRECISION, value);
        }
        public bool IsPrecisionModified => IsIntPropertyModifiedFromVanilla(Command.PRECISION);
        public bool IsPrecisionSessionEdit => IsPropertyEditedInSession(Command.PRECISION);
        public bool IsPrecisionInherited => IsIntPropertyInherited(Command.PRECISION);

        public int? AOE
        {
            get => GetIntProperty(Command.AOE);
            set => SetIntProperty(Command.AOE, value);
        }
        public bool IsAOEModified => IsIntPropertyModifiedFromVanilla(Command.AOE);
        public bool IsAOESessionEdit => IsPropertyEditedInSession(Command.AOE);
        public bool IsAOEInherited => IsIntPropertyInherited(Command.AOE);

        public int? Damage
        {
            get => GetIntProperty(Command.DAMAGE);
            set => SetIntProperty(Command.DAMAGE, value);
        }
        public bool IsDamageModified => IsIntPropertyModifiedFromVanilla(Command.DAMAGE);
        public bool IsDamageSessionEdit => IsPropertyEditedInSession(Command.DAMAGE);
        public bool IsDamageInherited => IsIntPropertyInherited(Command.DAMAGE);

        public int? Effect
        {
            get => GetIntProperty(Command.EFFECT);
            set => SetIntProperty(Command.EFFECT, value);
        }
        public bool IsEffectModified => IsIntPropertyModifiedFromVanilla(Command.EFFECT);
        public bool IsEffectSessionEdit => IsPropertyEditedInSession(Command.EFFECT);
        public bool IsEffectInherited => IsIntPropertyInherited(Command.EFFECT);

        public int? NrEff
        {
            get => GetIntProperty(Command.NREFF);
            set => SetIntProperty(Command.NREFF, value);
        }
        public bool IsNrEffModified => IsIntPropertyModifiedFromVanilla(Command.NREFF);
        public bool IsNrEffSessionEdit => IsPropertyEditedInSession(Command.NREFF);
        public bool IsNrEffInherited => IsIntPropertyInherited(Command.NREFF);

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

        // ========================================
        // Badge Collections
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

        // Commands for badge operations
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
                Command.SCHOOL => "School",
                Command.RESEARCHLEVEL => "ResearchLevel",
                Command.FATIGUECOST => "FatigueCost",
                Command.RANGE => "Range",
                Command.PRECISION => "Precision",
                Command.AOE => "AOE",
                Command.DAMAGE => "Damage",
                Command.EFFECT => "Effect",
                Command.NREFF => "NrEff",
                _ => null
            };
        }
    }
}
