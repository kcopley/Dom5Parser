using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;
using Paloma;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for Monster entities.
    /// </summary>
    public class MonsterViewModel : EntityViewModel
    {
        public MonsterViewModel(Monster entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Monster Monster => (Monster)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from monster_badges.json.
        /// </summary>
        protected override string EntityTypeName => "monster";

        // ========================================
        // Basic Info
        // ========================================

        public string FixedName
        {
            get => GetStringProperty(Command.FIXEDNAME);
            set => SetStringProperty(Command.FIXEDNAME, value);
        }
        public bool IsFixedNameModified => IsStringPropertyModifiedFromVanilla(Command.FIXEDNAME);
        public bool IsFixedNameSessionEdit => IsPropertyEditedInSession(Command.FIXEDNAME);

        public string Description
        {
            get => GetStringProperty(Command.DESCR);
            set => SetStringProperty(Command.DESCR, value);
        }
        public bool IsDescriptionModified => IsStringPropertyModifiedFromVanilla(Command.DESCR);
        public bool IsDescriptionSessionEdit => IsPropertyEditedInSession(Command.DESCR);

        public string Sprite1
        {
            get => GetStringProperty(Command.SPR1);
            set => SetStringProperty(Command.SPR1, value);
        }
        public bool IsSprite1Modified => IsStringPropertyModifiedFromVanilla(Command.SPR1);
        public bool IsSprite1SessionEdit => IsPropertyEditedInSession(Command.SPR1);

        public string Sprite2
        {
            get => GetStringProperty(Command.SPR2);
            set => SetStringProperty(Command.SPR2, value);
        }
        public bool IsSprite2Modified => IsStringPropertyModifiedFromVanilla(Command.SPR2);
        public bool IsSprite2SessionEdit => IsPropertyEditedInSession(Command.SPR2);

        // ========================================
        // Copy Commands (fundamental for inheritance) - editable
        // ========================================

        /// <summary>
        /// Gets or sets the CopyStats reference ID.
        /// </summary>
        public int? CopyStatsId
        {
            get
            {
                var result = _entity.TryGet<CopyStatsRef>(Command.COPYSTATS, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.ID;
                }
                return null;
            }
            set
            {
                if (value == null || value == 0)
                {
                    _entity.RemoveProperty(Command.COPYSTATS);
                }
                else
                {
                    _entity.Set<CopyStatsRef>(Command.COPYSTATS, p => p.Parse(Command.COPYSTATS, value.Value.ToString(), ""));
                    if (_entity.TryGet<CopyStatsRef>(Command.COPYSTATS, out var prop) == ReturnType.TRUE)
                        RecordPropertyChangeInSession(prop);
                }
                OnPropertyChanged(nameof(CopyStatsId));
                OnPropertyChanged(nameof(CopyStatsName));
                OnPropertyChanged(nameof(HasCopyStats));
                OnPropertyChanged(nameof(HasCopyCommands));

                // Refresh all properties that inherit from copystats
                RefreshAllCopyDependentProperties();
            }
        }

        /// <summary>
        /// Gets the CopyStats reference name for display.
        /// </summary>
        public string CopyStatsName
        {
            get
            {
                var result = _entity.TryGet<CopyStatsRef>(Command.COPYSTATS, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"#{idEntity.ID}";
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        public bool HasCopyStats => CopyStatsId.HasValue;

        /// <summary>
        /// Gets or sets the CopySpr reference ID.
        /// </summary>
        public int? CopySprId
        {
            get
            {
                var result = _entity.TryGet<MonsterRef>(Command.COPYSPR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.ID;
                }
                return null;
            }
            set
            {
                if (value == null || value == 0)
                {
                    _entity.RemoveProperty(Command.COPYSPR);
                }
                else
                {
                    _entity.Set<MonsterRef>(Command.COPYSPR, p => p.Parse(Command.COPYSPR, value.Value.ToString(), ""));
                    if (_entity.TryGet<MonsterRef>(Command.COPYSPR, out var prop) == ReturnType.TRUE)
                        RecordPropertyChangeInSession(prop);
                }
                OnPropertyChanged(nameof(CopySprId));
                OnPropertyChanged(nameof(CopySprName));
                OnPropertyChanged(nameof(HasCopySpr));
                OnPropertyChanged(nameof(HasCopyCommands));

                // Refresh sprite properties that inherit from copyspr
                OnPropertyChanged(nameof(Sprite1));
                OnPropertyChanged(nameof(Sprite2));
                OnPropertyChanged(nameof(SpriteImage));
                OnPropertyChanged(nameof(Sprite2Image));
            }
        }

        /// <summary>
        /// Gets the CopySpr reference name for display.
        /// </summary>
        public string CopySprName
        {
            get
            {
                var result = _entity.TryGet<MonsterRef>(Command.COPYSPR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"#{idEntity.ID}";
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        public bool HasCopySpr => CopySprId.HasValue;

        /// <summary>
        /// Whether to show the copy from section (has any copy command).
        /// </summary>
        public bool HasCopyCommands => HasCopyStats || HasCopySpr;

        // Cached reference items for copy monster selectors
        private List<ReferenceItem> _availableMonstersForCopy;

        /// <summary>
        /// Gets the available monsters as ReferenceItems for the copy selectors.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableMonstersForCopy
        {
            get
            {
                if (_availableMonstersForCopy == null)
                {
                    _availableMonstersForCopy = CachedMonsters
                        .Where(m => m.ID != ID) // Exclude self
                        .Select(m => new ReferenceItem { ID = m.ID, DisplayName = m.Name, Tag = m })
                        .ToList();
                }
                return _availableMonstersForCopy;
            }
        }

        // ========================================
        // Sprite Images
        // ========================================

        /// <summary>
        /// Loads and returns the sprite 1 image (TGA file).
        /// </summary>
        public BitmapSource SpriteImage => LoadSpriteImage(Command.SPR1);

        /// <summary>
        /// Loads and returns the sprite 2 image (TGA file).
        /// </summary>
        public BitmapSource Sprite2Image => LoadSpriteImage(Command.SPR2);

        private BitmapSource LoadSpriteImage(Command spriteCommand)
        {
            // Try to get sprite path from entity
            var exists = _entity.TryGet<FilePathProperty>(spriteCommand, out var property);

            // Fall back to vanilla for VanillaModified entities
            if (exists != ReturnType.TRUE && exists != ReturnType.COPIED && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    exists = vanillaEntity.TryGet<FilePathProperty>(spriteCommand, out property);
                }
            }

            if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
            {
                try
                {
                    var spritePath = property.Value;
                    if (string.IsNullOrEmpty(spritePath))
                        return null;

                    string filePath;

                    // Check if this is an absolute path (e.g., from VanillaAssetLoader)
                    if (Path.IsPathRooted(spritePath))
                    {
                        filePath = spritePath;
                    }
                    else
                    {
                        // Relative path - resolve against mod directory
                        var spriteAdjusted = spritePath.Trim('.').Trim('/').Replace("/", "\\");

                        var modPath = _entity.ParentMod?.FullFilePath;
                        if (string.IsNullOrEmpty(modPath))
                            return null;

                        var dir = Path.GetDirectoryName(modPath);
                        filePath = Path.Combine(dir, spriteAdjusted);
                    }

                    if (!File.Exists(filePath))
                        return null;

                    // Load based on file extension
                    var extension = Path.GetExtension(filePath).ToLowerInvariant();
                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp")
                    {
                        // Load standard image formats using BitmapImage
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                    else if (extension == ".tga")
                    {
                        // Load TGA using TargaImage
                        var targa = TargaImage.LoadTargaImage(filePath);
                        return targa.ConvertToImage();
                    }
                    else
                    {
                        // Unknown format, try TGA loader as fallback
                        var targa = TargaImage.LoadTargaImage(filePath);
                        return targa.ConvertToImage();
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        // ========================================
        // Stats - Now driven by JSON via StatsBadges collection
        // Individual stat properties removed - use StatsBadges for display/edit
        // ========================================

        // ===== BADGE-BASED COLLECTIONS (Compact UI) =====
        // Sections defined in monster_badges.json: stats, types (read-only), general, combat, resistances
        // Uses base class BuildBadgesFromSection() method

        // Stats badges (grid layout with showDefaults for consistent display)
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _statsBadges;

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> StatsBadges
        {
            get { if (_statsBadges == null) RefreshStatsBadges(); return _statsBadges; }
        }

        private void RefreshStatsBadges()
        {
            var (active, available) = BuildBadgesFromSection("stats", BadgeValueChangedHandler);
            _statsBadges = active;
            OnPropertyChanged(nameof(StatsBadges));
        }

        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _typeBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableTypeBadges;
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _generalBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableGeneralBadges;
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _combatBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableCombatBadges;
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _resistanceBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableResistanceBadges;

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> TypeBadges
        {
            get { if (_typeBadges == null) RefreshTypeBadges(); return _typeBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableTypeBadges
        {
            get { if (_availableTypeBadges == null) RefreshTypeBadges(); return _availableTypeBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> GeneralBadges
        {
            get { if (_generalBadges == null) RefreshGeneralBadges(); return _generalBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableGeneralBadges
        {
            get { if (_availableGeneralBadges == null) RefreshGeneralBadges(); return _availableGeneralBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> CombatBadges
        {
            get { if (_combatBadges == null) RefreshCombatBadges(); return _combatBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableCombatBadges
        {
            get { if (_availableCombatBadges == null) RefreshCombatBadges(); return _availableCombatBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> ResistanceBadges
        {
            get { if (_resistanceBadges == null) RefreshResistanceBadges(); return _resistanceBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableResistanceBadges
        {
            get { if (_availableResistanceBadges == null) RefreshResistanceBadges(); return _availableResistanceBadges; }
        }

        // Commands for badge operations - using base class helpers
        private RelayCommand<PropertyItem> _removeGeneralBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addGeneralBadgeCommand;
        private RelayCommand<PropertyItem> _removeCombatBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCombatBadgeCommand;
        private RelayCommand<PropertyItem> _removeResistanceBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResistanceBadgeCommand;

        public RelayCommand<PropertyItem> RemoveGeneralBadgeCommand => _removeGeneralBadgeCommand ??= CreateRemoveBadgeCommand(RefreshGeneralBadges);
        public RelayCommand<AvailablePropertyItem> AddGeneralBadgeCommand => _addGeneralBadgeCommand ??= CreateAddBadgeCommand(RefreshGeneralBadges);
        public RelayCommand<PropertyItem> RemoveCombatBadgeCommand => _removeCombatBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCombatBadges);
        public RelayCommand<AvailablePropertyItem> AddCombatBadgeCommand => _addCombatBadgeCommand ??= CreateAddBadgeCommand(RefreshCombatBadges);
        public RelayCommand<PropertyItem> RemoveResistanceBadgeCommand => _removeResistanceBadgeCommand ??= CreateRemoveBadgeCommand(RefreshResistanceBadges);
        public RelayCommand<AvailablePropertyItem> AddResistanceBadgeCommand => _addResistanceBadgeCommand ??= CreateAddBadgeCommand(RefreshResistanceBadges);

        // Shared value changed handler for all badge sections (uses base class helper)
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshTypeBadges()
        {
            var (active, available) = BuildBadgesFromSection("types", null);
            _typeBadges = active;
            _availableTypeBadges = available;
            OnPropertyChanged(nameof(TypeBadges));
            OnPropertyChanged(nameof(AvailableTypeBadges));
        }

        private void RefreshGeneralBadges()
        {
            var (active, available) = BuildBadgesFromSection("general", BadgeValueChangedHandler);
            _generalBadges = active;
            _availableGeneralBadges = available;
            OnPropertyChanged(nameof(GeneralBadges));
            OnPropertyChanged(nameof(AvailableGeneralBadges));
        }

        private void RefreshCombatBadges()
        {
            var (active, available) = BuildBadgesFromSection("combat", BadgeValueChangedHandler);
            _combatBadges = active;
            _availableCombatBadges = available;
            OnPropertyChanged(nameof(CombatBadges));
            OnPropertyChanged(nameof(AvailableCombatBadges));
        }

        private void RefreshResistanceBadges()
        {
            var (active, available) = BuildBadgesFromSection("resistances", BadgeValueChangedHandler);
            _resistanceBadges = active;
            _availableResistanceBadges = available;
            OnPropertyChanged(nameof(ResistanceBadges));
            OnPropertyChanged(nameof(AvailableResistanceBadges));
        }

        /// <summary>
        /// Refreshes all properties and collections that depend on copystats inheritance.
        /// Called when CopyStatsId changes to ensure UI reflects the new inherited values.
        /// </summary>
        private void RefreshAllCopyDependentProperties()
        {
            // Refresh all badge collections that may inherit from copystats
            RefreshStatsBadges();
            RefreshTypeBadges();
            RefreshGeneralBadges();
            RefreshCombatBadges();
            RefreshResistanceBadges();

            // Refresh equipment lists (weapons and armor can inherit from copystats)
            RefreshWeaponsList();
            RefreshArmorList();

            // Refresh magic paths (inherit from copystats)
            RefreshMagicPaths();

            // Refresh custom magic (inherit from copystats)
            RefreshCustomMagic();

            // Notify sprite properties may have changed (if copyspr is affected)
            OnPropertyChanged(nameof(SpriteImage));
            OnPropertyChanged(nameof(Sprite2Image));
        }

        // ===== Magic Paths =====
        private System.Collections.ObjectModel.ObservableCollection<UI.Controls.MagicPathItem> _magicPathsList;
        private System.Collections.ObjectModel.ObservableCollection<UI.Controls.AvailableMagicPath> _availableMagicPaths;

        public System.Collections.ObjectModel.ObservableCollection<UI.Controls.MagicPathItem> MagicPathsList
        {
            get
            {
                if (_magicPathsList == null) RefreshMagicPaths();
                return _magicPathsList;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<UI.Controls.AvailableMagicPath> AvailableMagicPaths
        {
            get { if (_availableMagicPaths == null) RefreshMagicPaths(); return _availableMagicPaths; }
        }

        private void RefreshMagicPaths()
        {
            var activePaths = new System.Collections.ObjectModel.ObservableCollection<UI.Controls.MagicPathItem>();
            var availablePaths = new System.Collections.ObjectModel.ObservableCollection<UI.Controls.AvailableMagicPath>();

            // Get magic skills by layering: vanilla -> mod -> session changes
            var monster = _entity as Monster;
            if (monster != null)
            {
                var usedPathIds = new HashSet<int>();

                // Dictionary to collect all magic skills: pathId -> (level, isFromMod, isSessionEdit)
                var allSkills = new Dictionary<int, (int Level, bool IsModified, bool IsSessionEdit)>();

                // First, get vanilla magic skills as base
                var vanillaMonster = GetVanillaEntity() as Monster;
                if (vanillaMonster != null)
                {
                    foreach (var skill in vanillaMonster.MagicSkills)
                    {
                        int pathId = (int)skill.Path;
                        allSkills[pathId] = (skill.Level, false, false);
                    }
                }

                // Then overlay with mod/current entity skills (if different from vanilla entity)
                if (monster != vanillaMonster)
                {
                    foreach (var skill in monster.MagicSkills)
                    {
                        int pathId = (int)skill.Path;
                        // Check if this is a session edit or mod change
                        bool isSessionEdit = IsPropertyEditedInSession(Command.MAGICSKILL);
                        bool isModified = !allSkills.ContainsKey(pathId) || allSkills[pathId].Level != skill.Level;
                        allSkills[pathId] = (skill.Level, isModified, isSessionEdit);
                    }
                }

                // Build UI items from collected skills
                foreach (var kvp in allSkills.OrderBy(k => k.Key))
                {
                    int pathId = kvp.Key;
                    var (level, isModified, isSessionEdit) = kvp.Value;
                    var pathInfo = UI.Controls.MagicPathDefinitions.GetPathInfo(pathId);
                    var item = new UI.Controls.MagicPathItem
                    {
                        PathId = pathId,
                        PathLetter = pathInfo.Letter,
                        PathName = pathInfo.Name,
                        PathColor = pathInfo.Color,
                        TextColor = pathInfo.TextColor,
                        BorderColor = pathInfo.BorderColor,
                        Level = level,
                        IsInherited = false, // TODO: check from copystats
                        IsModified = isModified,
                        IsSessionEdit = isSessionEdit
                    };
                    item.LevelChanged += (s, newLevel) => OnMagicPathLevelChanged(pathId, newLevel);
                    activePaths.Add(item);
                    usedPathIds.Add(pathId);
                }

                // Build available paths (exclude already used ones)
                foreach (var def in UI.Controls.MagicPathDefinitions.PathDefs)
                {
                    if (!usedPathIds.Contains(def.Id))
                    {
                        var pathInfo = UI.Controls.MagicPathDefinitions.GetPathInfo(def.Id);
                        availablePaths.Add(new UI.Controls.AvailableMagicPath
                        {
                            PathId = def.Id,
                            PathLetter = pathInfo.Letter,
                            PathName = pathInfo.Name,
                            PathColor = pathInfo.Color,
                            TextColor = pathInfo.TextColor,
                            BorderColor = pathInfo.BorderColor
                        });
                    }
                }
            }

            _magicPathsList = activePaths;
            _availableMagicPaths = availablePaths;
            OnPropertyChanged(nameof(MagicPathsList));
            OnPropertyChanged(nameof(AvailableMagicPaths));
        }

        private bool IsIntIntPropertyModifiedFromVanilla(Command command, int value1)
        {
            // Check if a specific IntIntProperty with given Value1 is modified from vanilla
            if (_source == EntitySource.Vanilla) return false;
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null) return true;
            // Check if vanilla has this specific path
            var vanillaMonster = vanillaEntity as Monster;
            if (vanillaMonster == null) return true;
            foreach (var skill in vanillaMonster.MagicSkills)
            {
                if ((int)skill.Path == value1) return false;
            }
            return true;
        }

        private void OnMagicPathLevelChanged(int pathId, int newLevel)
        {
            // Find and update the magic skill property
            var monster = _entity as Monster;
            if (monster != null)
            {
                // Remove old and add new
                var props = monster.Properties.Where(p => p.Command == Command.MAGICSKILL)
                    .Cast<Dom5Edit.Props.IntIntProperty>().ToList();
                var existing = props.FirstOrDefault(p => p.Value1 == pathId);

                var newProp = new Dom5Edit.Props.IntIntProperty
                {
                    Command = Command.MAGICSKILL,
                    Value1 = pathId,
                    Value2 = newLevel,
                    HasValue = true
                };

                // Use CommandHistory for undo/redo support
                if (_history != null && existing != null)
                {
                    // Use SetIntIntPropertyCommand for changing level
                    var cmd = new SetIntIntPropertyCommand(_entity, Command.MAGICSKILL, pathId, newLevel);
                    _history.Execute(cmd);
                }
                else
                {
                    // Fallback: direct modification
                    if (existing != null)
                    {
                        monster.RemoveProperty(existing);
                    }
                    monster.AddProperty(newProp);
                }

                HasSessionChanges = true;
            }
        }

        public void AddMagicPath(int pathId, int level)
        {
            var monster = _entity as Monster;
            if (monster != null)
            {
                var newProp = new Dom5Edit.Props.IntIntProperty
                {
                    Command = Command.MAGICSKILL,
                    Value1 = pathId,
                    Value2 = level,
                    HasValue = true
                };

                // Use CommandHistory for undo/redo support
                if (_history != null)
                {
                    var cmd = new AddPropertyCommand(_entity, newProp, $"Add Magic Path {pathId}");
                    _history.Execute(cmd);
                }
                else
                {
                    monster.AddProperty(newProp);
                }

                HasSessionChanges = true;
                RefreshMagicPaths();
            }
        }

        public void RemoveMagicPath(int pathId)
        {
            var monster = _entity as Monster;
            if (monster != null)
            {
                var props = monster.Properties.Where(p => p.Command == Command.MAGICSKILL)
                    .Cast<Dom5Edit.Props.IntIntProperty>().ToList();
                var existing = props.FirstOrDefault(p => p.Value1 == pathId);
                if (existing != null)
                {
                    // Use CommandHistory for undo/redo support
                    if (_history != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existing, $"Remove Magic Path {pathId}");
                        _history.Execute(cmd);
                    }
                    else
                    {
                        monster.RemoveProperty(existing);
                    }

                    HasSessionChanges = true;
                    RefreshMagicPaths();
                }
            }
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh magic paths list when MAGICSKILL changes
            if (command == Command.MAGICSKILL)
            {
                RefreshMagicPaths();
                return;
            }

            // Check if this is a stat command (refresh StatsBadges)
            if (IsStatCommand(command))
            {
                RefreshStatsBadges();
                return;
            }

            // Check if this is a general badge command
            if (IsGeneralBadgeCommand(command))
            {
                RefreshGeneralBadges();
                return;
            }

            // Check if this is a combat badge command
            if (IsCombatBadgeCommand(command))
            {
                RefreshCombatBadges();
                return;
            }

            // Check if this is a resistance badge command
            if (IsResistanceBadgeCommand(command))
            {
                RefreshResistanceBadges();
                return;
            }

            // Check if this is a type badge command
            if (IsTypeBadgeCommand(command))
            {
                RefreshTypeBadges();
                return;
            }

            // Map Command enum to actual property names (remaining properties)
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        /// <summary>
        /// Checks if the command is a core stat (HP, STR, ATT, etc.) handled by StatsBadges.
        /// </summary>
        private static bool IsStatCommand(Command command)
        {
            return command switch
            {
                Command.HP or Command.STR or Command.ATT or Command.DEF or
                Command.PREC or Command.MR or Command.MOR or Command.ENC or
                Command.PROT or Command.AP or Command.MAPMOVE or Command.SIZE => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the command is handled by general badges section.
        /// </summary>
        private static bool IsGeneralBadgeCommand(Command command)
        {
            // General section includes: movement, leadership, special abilities, province effects, stealth, recruitment
            return command switch
            {
                Command.FLYING or Command.AQUATIC or Command.AMPHIBIAN or Command.POORAMPHIBIAN or
                Command.FLOAT or Command.SWIMMING or Command.TELEPORT or Command.MAPTELEPORT or
                Command.BLINK or Command.FORESTSURVIVAL or Command.MOUNTAINSURVIVAL or
                Command.SWAMPSURVIVAL or Command.WASTESURVIVAL or Command.SNOW or
                Command.NOLEADER or Command.POORLEADER or Command.OKLEADER or Command.GOODLEADER or
                Command.EXPERTLEADER or Command.SUPERIORLEADER or Command.NOMAGICLEADER or
                Command.POORMAGICLEADER or Command.OKMAGICLEADER or Command.GOODMAGICLEADER or
                Command.EXPERTMAGICLEADER or Command.SUPERIORMAGICLEADER or Command.NOUNDEADLEADER or
                Command.POORUNDEADLEADER or Command.OKUNDEADLEADER or Command.GOODUNDEADLEADER or
                Command.EXPERTUNDEADLEADER or Command.SUPERIORUNDEADLEADER or
                Command.HEAL or Command.NOHEAL or Command.HEALER or Command.AUTOHEALER or
                Command.NEEDNOTEAT or Command.TAXCOLLECTOR or Command.INQUISITOR or Command.MASON or
                Command.LOCALSUN or Command.COMMASTER or Command.COMSLAVE or Command.SPELLSINGER or
                Command.COMBATCASTER or Command.DRAINIMMUNE or Command.DIVINEINS or Command.NOITEM or
                Command.SPY or Command.ASSASSIN or Command.STEALTHY or Command.SEDUCE or
                Command.POPKILL or Command.INCUNREST or Command.SPREADDOM or Command.PATROLBONUS or
                Command.SUPPLYBONUS or Command.GCOST or Command.RCOST or Command.REQLAB or
                Command.REQTEMPLE or Command.STARTAGE or Command.MAXAGE => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the command is handled by combat badges section.
        /// </summary>
        private static bool IsCombatBadgeCommand(Command command)
        {
            return command switch
            {
                Command.AWE or Command.FEAR or Command.BERSERK or Command.AMBIDEXTROUS or
                Command.DARKVISION or Command.TRAMPLE or Command.DEATHCURSE or Command.BODYGUARD or
                Command.WARNING or Command.STANDARD or Command.FORMATIONFIGHTER or Command.PATIENCE or
                Command.CHAOSPOWER or Command.MAGICPOWER or Command.ETHEREAL or Command.GLAMOUR or
                Command.HEAT or Command.COLD or Command.FIRESHIELD or Command.POISONCLOUD or
                Command.DISEASECLOUD or Command.POISONSKIN or Command.ACIDSHIELD or Command.SLEEPAURA or
                Command.ANIMALAWE or Command.SUNAWE => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the command is handled by resistance badges section.
        /// </summary>
        private static bool IsResistanceBadgeCommand(Command command)
        {
            return command switch
            {
                Command.FIRERES or Command.COLDRES or Command.SHOCKRES or Command.POISONRES or
                Command.REGENERATION or Command.INVULNERABLE or Command.AIRSHIELD or Command.ICEPROT or
                Command.REINVIGORATION or Command.BLUNTRES or Command.PIERCERES or Command.SLASHRES or
                Command.DISEASERES or Command.MAGICIMMUNE or Command.STORMIMMUNE or Command.ACIDRES => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the command is handled by type badges section.
        /// </summary>
        private static bool IsTypeBadgeCommand(Command command)
        {
            return command switch
            {
                Command.HUMANOID or Command.MOUNTEDHUMANOID or Command.QUADRUPED or Command.LIZARD or
                Command.NAGA or Command.SNAKE or Command.BIRD or Command.DJINN or Command.TROGLODYTE or
                Command.MISCSHAPE or Command.MOUNTED or Command.UNDEAD or Command.DEMON or
                Command.MAGICBEING or Command.HOLY or Command.ANIMAL or Command.UNIQUE or
                Command.INANIMATE or Command.MINDLESS or Command.BLIND or Command.COLDBLOOD or
                Command.IMMORTAL or Command.FEMALE or Command.IMMOBILE or Command.STONEBEING or
                Command.PLANT or Command.DRAKE or Command.BUG or Command.LESSERHORROR or
                Command.GREATERHORROR or Command.DOOMHORROR => true,
                _ => false
            };
        }

        /// <summary>
        /// Maps Command enum values to ViewModel property names.
        /// Returns null if no mapping exists (badge sections handle most properties now).
        /// </summary>
        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                // Identity properties still have individual bindings
                Command.FIXEDNAME => "FixedName",
                Command.DESCR => "Description",
                Command.SPR1 => "Sprite1",
                Command.SPR2 => "Sprite2",

                _ => null  // Most properties are now handled by badge collections
            };
        }

        protected override string GetCommandDisplayName(Command command)
        {
            return command switch
            {
                // Type
                Command.HUMANOID => "Humanoid", Command.MOUNTEDHUMANOID => "Mounted Humanoid",
                Command.QUADRUPED => "Quadruped", Command.LIZARD => "Lizard", Command.NAGA => "Naga",
                Command.SNAKE => "Snake", Command.BIRD => "Bird", Command.DJINN => "Djinn",
                Command.TROGLODYTE => "Troglodyte", Command.MISCSHAPE => "Misc Shape",
                Command.MOUNTED => "Mounted", Command.UNDEAD => "Undead", Command.DEMON => "Demon",
                Command.MAGICBEING => "Magic Being", Command.HOLY => "Holy", Command.ANIMAL => "Animal",
                Command.UNIQUE => "Unique", Command.INANIMATE => "Inanimate", Command.MINDLESS => "Mindless",
                Command.BLIND => "Blind", Command.COLDBLOOD => "Cold Blooded", Command.IMMORTAL => "Immortal",
                Command.FEMALE => "Female", Command.IMMOBILE => "Immobile", Command.STONEBEING => "Stone Being",
                Command.PLANT => "Plant", Command.DRAKE => "Drake", Command.BUG => "Bug",
                Command.LESSERHORROR => "Lesser Horror", Command.GREATERHORROR => "Greater Horror",
                Command.DOOMHORROR => "Doom Horror",
                // Leadership
                Command.NOLEADER => "No Leader", Command.POORLEADER => "Poor Leader",
                Command.OKLEADER => "OK Leader", Command.GOODLEADER => "Good Leader",
                Command.EXPERTLEADER => "Expert Leader", Command.SUPERIORLEADER => "Superior Leader",
                Command.NOMAGICLEADER => "No Magic Leader", Command.POORMAGICLEADER => "Poor Magic Leader",
                Command.OKMAGICLEADER => "OK Magic Leader", Command.GOODMAGICLEADER => "Good Magic Leader",
                Command.EXPERTMAGICLEADER => "Expert Magic Leader", Command.SUPERIORMAGICLEADER => "Superior Magic Leader",
                Command.NOUNDEADLEADER => "No Undead Leader", Command.POORUNDEADLEADER => "Poor Undead Leader",
                Command.OKUNDEADLEADER => "OK Undead Leader", Command.GOODUNDEADLEADER => "Good Undead Leader",
                Command.EXPERTUNDEADLEADER => "Expert Undead Leader", Command.SUPERIORUNDEADLEADER => "Superior Undead Leader",
                // Movement
                Command.FLYING => "Flying", Command.AQUATIC => "Aquatic", Command.AMPHIBIAN => "Amphibian",
                Command.POORAMPHIBIAN => "Poor Amphibian", Command.FLOAT => "Float", Command.SWIMMING => "Swimming",
                Command.TELEPORT => "Teleport", Command.MAPTELEPORT => "Map Teleport", Command.BLINK => "Blink",
                Command.FORESTSURVIVAL => "Forest Survival", Command.MOUNTAINSURVIVAL => "Mountain Survival",
                Command.SWAMPSURVIVAL => "Swamp Survival", Command.WASTESURVIVAL => "Waste Survival",
                Command.SNOW => "Snow Movement",
                // Resistances
                Command.FIRERES => "Fire Res", Command.COLDRES => "Cold Res",
                Command.SHOCKRES => "Shock Res", Command.POISONRES => "Poison Res",
                Command.REGENERATION => "Regeneration", Command.INVULNERABLE => "Invulnerability",
                Command.AIRSHIELD => "Air Shield", Command.ICEPROT => "Ice Protection",
                Command.REINVIGORATION => "Reinvigoration", Command.IRONVUL => "Iron Vulnerability",
                Command.BLUNTRES => "Blunt Res", Command.PIERCERES => "Pierce Res", Command.SLASHRES => "Slash Res",
                Command.DISEASERES => "Disease Res", Command.MAGICIMMUNE => "Magic Immune",
                Command.STORMIMMUNE => "Storm Immune", Command.STUNIMMUNITY => "Stun Immune",
                Command.POLYIMMUNE => "Polymorph Immune", Command.ACIDRES => "Acid Res", Command.DECAYRES => "Decay Res",
                // Combat
                Command.AWE => "Awe", Command.FEAR => "Fear", Command.BERSERK => "Berserk",
                Command.AMBIDEXTROUS => "Ambidextrous", Command.DARKVISION => "Dark Vision",
                Command.TRAMPLE => "Trample", Command.DEATHCURSE => "Death Curse",
                Command.BODYGUARD => "Bodyguard", Command.WARNING => "Warning", Command.STANDARD => "Standard",
                Command.FORMATIONFIGHTER => "Formation", Command.PATIENCE => "Patience",
                Command.CHAOSPOWER => "Chaos Power", Command.MAGICPOWER => "Magic Power",
                Command.ETHEREAL => "Ethereal", Command.GLAMOUR => "Glamour",
                // Auras
                Command.HEAT => "Heat Aura", Command.COLD => "Cold Aura", Command.FIRESHIELD => "Fire Shield",
                Command.POISONCLOUD => "Poison Cloud", Command.DISEASECLOUD => "Disease Cloud",
                Command.POISONSKIN => "Poison Skin", Command.POISONARMOR => "Poison Armor",
                Command.ACIDSHIELD => "Acid Shield", Command.SLEEPAURA => "Sleep Aura",
                Command.ANIMALAWE => "Animal Awe", Command.SUNAWE => "Sun Awe", Command.HALTHERETIC => "Halt Heretic",
                // Special
                Command.HEAL => "Heal", Command.NOHEAL => "No Heal", Command.HEALER => "Healer",
                Command.AUTOHEALER => "Auto Healer", Command.NEEDNOTEAT => "Need Not Eat",
                Command.TAXCOLLECTOR => "Tax Collector", Command.INQUISITOR => "Inquisitor",
                Command.MASON => "Mason", Command.LOCALSUN => "Local Sun",
                Command.COMMASTER => "Communion Master", Command.COMSLAVE => "Communion Slave",
                Command.SPELLSINGER => "Spell Singer", Command.COMBATCASTER => "Combat Caster",
                Command.DRAINIMMUNE => "Drain Immune", Command.DIVINEINS => "Divine Inspiration",
                Command.NOITEM => "No Items",
                _ => base.GetCommandDisplayName(command)
            };
        }

        // ========================================
        // Equipment (Weapons & Armor)
        // ========================================

        private ObservableCollection<EquipmentItem> _weaponsList;
        public ObservableCollection<EquipmentItem> WeaponsList
        {
            get
            {
                if (_weaponsList == null)
                    RefreshWeaponsList();
                return _weaponsList;
            }
        }

        private ObservableCollection<EquipmentItem> _armorList;
        public ObservableCollection<EquipmentItem> ArmorList
        {
            get
            {
                if (_armorList == null)
                    RefreshArmorList();
                return _armorList;
            }
        }

        /// <summary>
        /// Refresh weapons list using generic layered reference lookup.
        /// </summary>
        private void RefreshWeaponsList()
        {
            _weaponsList = new ObservableCollection<EquipmentItem>();

            // Use base class generic method for layered lookup
            var weapons = GetLayeredReferenceList<WeaponRef>(Command.WEAPON, EntityType.WEAPON, r => r.ID);

            foreach (var (id, name, isInherited, isModified, isSessionEdit) in weapons)
            {
                var item = new EquipmentItem
                {
                    ID = id,
                    Name = name,
                    IsInherited = isInherited,
                    IsModified = isModified,
                    IsSessionEdit = isSessionEdit,
                    SourceCommand = Command.WEAPON,
                    EntityType = "weapon"
                };

                // Populate stats from resolved weapon entity
                var weaponEntity = ResolveEntityReference(EntityType.WEAPON, id) as Weapon;
                if (weaponEntity != null)
                {
                    PopulateWeaponStats(item, weaponEntity);
                }

                _weaponsList.Add(item);
            }

            OnPropertyChanged(nameof(WeaponsList));
        }

        private void PopulateWeaponStats(EquipmentItem item, Weapon weapon)
        {
            item.Damage = GetWeaponIntStat(weapon, Command.DMG);
            item.Attack = GetWeaponIntStat(weapon, Command.ATT);
            item.Defense = GetWeaponIntStat(weapon, Command.DEF);
            item.Length = GetWeaponIntStat(weapon, Command.LEN);
            item.NumAttacks = GetWeaponIntStat(weapon, Command.NRATT);
            item.Range = GetWeaponIntStat(weapon, Command.RANGE);
            item.Aoe = GetWeaponIntStat(weapon, Command.AOE);
            item.Precision = GetWeaponIntStat(weapon, Command.PREC);
            item.DamageTypes = BuildWeaponDamageTypesString(weapon);
        }

        private int? GetWeaponIntStat(Weapon weapon, Command cmd)
        {
            // Handle #dmg specially - it's a WeaponDamage (StringProperty subclass)
            if (cmd == Command.DMG)
            {
                var result = weapon.TryGet<WeaponDamage>(cmd, out var dmgProp);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                {
                    if (int.TryParse(dmgProp?.Value, out var dmgVal))
                        return dmgVal;
                }
                return null;
            }

            // Standard IntProperty lookup
            var intResult = weapon.TryGet<IntProperty>(cmd, out var prop);
            if (intResult == ReturnType.TRUE || intResult == ReturnType.COPIED)
                return prop?.Value;
            return null;
        }

        private string BuildWeaponDamageTypesString(Weapon weapon)
        {
            var types = new List<string>();

            // Physical damage types
            if (HasWeaponFlag(weapon, Command.PIERCE)) types.Add("Pierce");
            if (HasWeaponFlag(weapon, Command.SLASH)) types.Add("Slash");
            if (HasWeaponFlag(weapon, Command.BLUNT)) types.Add("Blunt");

            // Elemental damage types
            if (HasWeaponFlag(weapon, Command.FIRE)) types.Add("Fire");
            if (HasWeaponFlag(weapon, Command.COLD)) types.Add("Cold");
            if (HasWeaponFlag(weapon, Command.SHOCK)) types.Add("Shock");
            if (HasWeaponFlag(weapon, Command.POISON)) types.Add("Poison");
            if (HasWeaponFlag(weapon, Command.ACID)) types.Add("Acid");
            if (HasWeaponFlag(weapon, Command.MAGIC)) types.Add("Magic");

            // Special modifiers
            if (HasWeaponFlag(weapon, Command.ARMORPIERCING)) types.Add("AP");
            if (HasWeaponFlag(weapon, Command.ARMORNEGATING)) types.Add("AN");

            return string.Join(", ", types);
        }

        private bool HasWeaponFlag(Weapon weapon, Command cmd)
        {
            var result = weapon.TryGet<CommandProperty>(cmd, out _);
            return result == ReturnType.TRUE || result == ReturnType.COPIED;
        }

        // Armor type mapping
        private static readonly Dictionary<int, string> ArmorTypeNames = new()
        {
            {4, "Shield"}, {5, "Body Armor"}, {6, "Helmet"},
            {9, "Crown"}, {10, "Barding"}
        };

        /// <summary>
        /// Refresh armor list using generic layered reference lookup.
        /// </summary>
        private void RefreshArmorList()
        {
            _armorList = new ObservableCollection<EquipmentItem>();

            // Use base class generic method for layered lookup
            var armors = GetLayeredReferenceList<ArmorRef>(Command.ARMOR, EntityType.ARMOR, r => r.ID);

            foreach (var (id, name, isInherited, isModified, isSessionEdit) in armors)
            {
                var item = new EquipmentItem
                {
                    ID = id,
                    Name = name,
                    IsInherited = isInherited,
                    IsModified = isModified,
                    IsSessionEdit = isSessionEdit,
                    SourceCommand = Command.ARMOR,
                    EntityType = "armor"
                };

                // Populate stats from resolved armor entity
                var armorEntity = ResolveEntityReference(EntityType.ARMOR, id) as Armor;
                if (armorEntity != null)
                {
                    PopulateArmorStats(item, armorEntity);
                }

                _armorList.Add(item);
            }

            OnPropertyChanged(nameof(ArmorList));
        }

        private void PopulateArmorStats(EquipmentItem item, Armor armor)
        {
            item.Protection = GetArmorIntStat(armor, Command.PROT);
            item.Defense = GetArmorIntStat(armor, Command.DEF);
            item.Encumbrance = GetArmorIntStat(armor, Command.ENC);

            var typeVal = GetArmorIntStat(armor, Command.TYPE);
            item.ArmorTypeName = typeVal.HasValue && ArmorTypeNames.TryGetValue(typeVal.Value, out var typeName)
                ? typeName
                : "Unknown";
        }

        private int? GetArmorIntStat(Armor armor, Command cmd)
        {
            var result = armor.TryGet<IntProperty>(cmd, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                return prop?.Value;
            return null;
        }

        /// <summary>
        /// Gets the cached list of available weapons for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableWeapons => CachedWeapons;

        /// <summary>
        /// Gets the cached list of available armor for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableArmor => CachedArmors;

        // Selected items for adding
        private AvailableEquipmentItem _selectedWeaponToAdd;
        public AvailableEquipmentItem SelectedWeaponToAdd
        {
            get => _selectedWeaponToAdd;
            set
            {
                _selectedWeaponToAdd = value;
                OnPropertyChanged(nameof(SelectedWeaponToAdd));
            }
        }

        private AvailableEquipmentItem _selectedArmorToAdd;
        public AvailableEquipmentItem SelectedArmorToAdd
        {
            get => _selectedArmorToAdd;
            set
            {
                _selectedArmorToAdd = value;
                OnPropertyChanged(nameof(SelectedArmorToAdd));
            }
        }

        // SearchableReferenceComboBox support - cached lists for stable references
        private List<ReferenceItem> _availableWeaponsForSearch;
        public IEnumerable<ReferenceItem> AvailableWeaponsForSearch
        {
            get
            {
                if (_availableWeaponsForSearch == null)
                {
                    _availableWeaponsForSearch = CachedWeapons
                        .Select(w => new ReferenceItem { ID = w.ID, DisplayName = w.Name, Tag = w })
                        .ToList();
                }
                return _availableWeaponsForSearch;
            }
        }

        private List<ReferenceItem> _availableArmorForSearch;
        public IEnumerable<ReferenceItem> AvailableArmorForSearch
        {
            get
            {
                if (_availableArmorForSearch == null)
                {
                    _availableArmorForSearch = CachedArmors
                        .Select(a => new ReferenceItem { ID = a.ID, DisplayName = a.Name, Tag = a })
                        .ToList();
                }
                return _availableArmorForSearch;
            }
        }

        private int? _selectedWeaponIdToAdd;
        public int? SelectedWeaponIdToAdd
        {
            get => _selectedWeaponIdToAdd;
            set
            {
                _selectedWeaponIdToAdd = value;
                OnPropertyChanged(nameof(SelectedWeaponIdToAdd));
            }
        }

        private int? _selectedArmorIdToAdd;
        public int? SelectedArmorIdToAdd
        {
            get => _selectedArmorIdToAdd;
            set
            {
                _selectedArmorIdToAdd = value;
                OnPropertyChanged(nameof(SelectedArmorIdToAdd));
            }
        }

        // Equipment Commands
        private ICommand _addWeaponCommand;
        public ICommand AddWeaponCommand => _addWeaponCommand ??= new RelayCommand<AvailableEquipmentItem>(AddWeapon);

        private ICommand _removeWeaponCommand;
        public ICommand RemoveWeaponCommand => _removeWeaponCommand ??= new RelayCommand<EquipmentItem>(RemoveWeapon);

        private ICommand _addArmorCommand;
        public ICommand AddArmorCommand => _addArmorCommand ??= new RelayCommand<AvailableEquipmentItem>(AddArmor);

        private ICommand _removeArmorCommand;
        public ICommand RemoveArmorCommand => _removeArmorCommand ??= new RelayCommand<EquipmentItem>(RemoveArmor);

        // Commands for SearchableReferenceComboBox (ID-based selection)
        private ICommand _addWeaponByIdCommand;
        public ICommand AddWeaponByIdCommand => _addWeaponByIdCommand ??= new RelayCommand<int>(AddWeaponById);

        private ICommand _addArmorByIdCommand;
        public ICommand AddArmorByIdCommand => _addArmorByIdCommand ??= new RelayCommand<int>(AddArmorById);

        /// <summary>
        /// Adds a weapon to the monster by ID.
        /// </summary>
        public void AddWeaponById(int id)
        {
            if (id <= 0) return;

            var newProp = new WeaponRef { Parent = _entity, Command = Command.WEAPON };
            newProp.ID = id;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Weapon #{id}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            RecordPropertyChangeInSession(newProp);
            HasSessionChanges = true;
            RefreshWeaponsList();
        }

        /// <summary>
        /// Adds armor to the monster by ID. Called directly from view event handlers.
        /// </summary>
        public void AddArmorById(int id)
        {
            if (id <= 0) return;

            var newProp = new ArmorRef { Parent = _entity, Command = Command.ARMOR };
            newProp.ID = id;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Armor #{id}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            RecordPropertyChangeInSession(newProp);
            HasSessionChanges = true;
            RefreshArmorList();
        }

        private void AddWeapon(AvailableEquipmentItem weapon)
        {
            if (weapon == null) return;

            var newProp = new WeaponRef { Parent = _entity, Command = Command.WEAPON };
            newProp.ID = weapon.ID;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Weapon #{weapon.ID}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            RecordPropertyChangeInSession(newProp);
            HasSessionChanges = true;
            RefreshWeaponsList();
        }

        private void RemoveWeapon(EquipmentItem weapon)
        {
            if (weapon == null || weapon.IsInherited) return;

            var props = _entity.GetMultiple(Command.WEAPON).ToList();
            var toRemove = props.FirstOrDefault(p => p is WeaponRef wr && wr.ID == weapon.ID);

            if (toRemove != null)
            {
                if (_history != null)
                {
                    var cmd = new RemovePropertyCommand(_entity, toRemove, $"Remove Weapon #{weapon.ID}");
                    _history.Execute(cmd);
                }
                else
                {
                    _entity.RemoveProperty(toRemove);
                }

                HasSessionChanges = true;
                RefreshWeaponsList();
            }
        }

        private void AddArmor(AvailableEquipmentItem armor)
        {
            if (armor == null) return;

            var newProp = new ArmorRef { Parent = _entity, Command = Command.ARMOR };
            newProp.ID = armor.ID;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Armor #{armor.ID}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            RecordPropertyChangeInSession(newProp);
            HasSessionChanges = true;
            RefreshArmorList();
        }

        private void RemoveArmor(EquipmentItem armor)
        {
            if (armor == null || armor.IsInherited) return;

            var props = _entity.GetMultiple(Command.ARMOR).ToList();
            var toRemove = props.FirstOrDefault(p => p is ArmorRef ar && ar.ID == armor.ID);

            if (toRemove != null)
            {
                if (_history != null)
                {
                    var cmd = new RemovePropertyCommand(_entity, toRemove, $"Remove Armor #{armor.ID}");
                    _history.Execute(cmd);
                }
                else
                {
                    _entity.RemoveProperty(toRemove);
                }

                HasSessionChanges = true;
                RefreshArmorList();
            }
        }

        // Navigation events for hyperlink-style buttons (to be implemented)
        public event EventHandler<int> WeaponNavigationRequested;
        public event EventHandler<int> ArmorNavigationRequested;

        public void NavigateToWeapon(int weaponId)
        {
            WeaponNavigationRequested?.Invoke(this, weaponId);
        }

        public void NavigateToArmor(int armorId)
        {
            ArmorNavigationRequested?.Invoke(this, armorId);
        }

        // ========================================
        // Available Entities for Reference Selection
        // ========================================

        /// <summary>
        /// Gets the cached list of available monsters for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableMonsters => CachedMonsters;

        /// <summary>
        /// Gets the cached list of available items for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableItems => CachedItems;

        /// <summary>
        /// Gets the cached list of available nations for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableNations => CachedNations;

        // ========================================
        // Reference Properties (Read current values)
        // ========================================

        /// <summary>
        /// Gets the list of start items on this monster.
        /// </summary>
        public ObservableCollection<EquipmentItem> StartItemsList
        {
            get
            {
                var items = new ObservableCollection<EquipmentItem>();
                foreach (var prop in _entity.GetMultiple(Command.STARTITEM))
                {
                    if (prop is ItemRef itemRef && itemRef.HasValue)
                    {
                        items.Add(new EquipmentItem
                        {
                            ID = itemRef.ID,
                            Name = itemRef.Entity?.Name,
                            IsModified = true,
                            SourceCommand = Command.STARTITEM
                        });
                    }
                }
                return items;
            }
        }

        private AvailableEquipmentItem _selectedItemToAdd;
        public AvailableEquipmentItem SelectedItemToAdd
        {
            get => _selectedItemToAdd;
            set
            {
                _selectedItemToAdd = value;
                OnPropertyChanged(nameof(SelectedItemToAdd));
            }
        }

        private ICommand _addStartItemCommand;
        public ICommand AddStartItemCommand => _addStartItemCommand ??= new RelayCommand<AvailableEquipmentItem>(AddStartItem);

        private ICommand _removeStartItemCommand;
        public ICommand RemoveStartItemCommand => _removeStartItemCommand ??= new RelayCommand<EquipmentItem>(RemoveStartItem);

        private void AddStartItem(AvailableEquipmentItem item)
        {
            if (item == null) return;

            var newProp = new ItemRef { Parent = _entity, Command = Command.STARTITEM };
            newProp.ID = item.ID;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Start Item #{item.ID}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            HasSessionChanges = true;
            OnPropertyChanged(nameof(StartItemsList));
        }

        private void RemoveStartItem(EquipmentItem item)
        {
            if (item == null) return;

            var props = _entity.GetMultiple(Command.STARTITEM).ToList();
            var toRemove = props.FirstOrDefault(p => p is ItemRef ir && ir.ID == item.ID);

            if (toRemove != null)
            {
                if (_history != null)
                {
                    var cmd = new RemovePropertyCommand(_entity, toRemove, $"Remove Start Item #{item.ID}");
                    _history.Execute(cmd);
                }
                else
                {
                    _entity.RemoveProperty(toRemove);
                }

                HasSessionChanges = true;
                OnPropertyChanged(nameof(StartItemsList));
            }
        }

        // ========================================
        // CUSTOMMAGIC Support
        // ========================================

        private ObservableCollection<CustomMagicItem> _customMagicList;

        /// <summary>
        /// Gets the list of custom magic configurations on this monster.
        /// Uses layered access: vanilla entries first, then mod entries overlay.
        /// </summary>
        public ObservableCollection<CustomMagicItem> CustomMagicList
        {
            get
            {
                if (_customMagicList == null) RefreshCustomMagic();
                return _customMagicList;
            }
        }

        private void RefreshCustomMagic()
        {
            var items = new ObservableCollection<CustomMagicItem>();
            var knownEntries = new HashSet<(ulong, int)>(); // Track (Bitmask, Chance) pairs
            var vanillaEntity = GetVanillaEntity();

            // Layer 1: Vanilla entries (base layer)
            if (vanillaEntity != null)
            {
                foreach (var prop in vanillaEntity.GetMultiple(Command.CUSTOMMAGIC))
                {
                    if (prop is BitmaskChanceProperty bcp && bcp.HasValue)
                    {
                        var key = (bcp.Bitmask, bcp.Chance);
                        if (!knownEntries.Contains(key))
                        {
                            knownEntries.Add(key);
                            var item = new CustomMagicItem
                            {
                                Bitmask = bcp.Bitmask,
                                Chance = bcp.Chance,
                                Property = null, // Vanilla properties shouldn't be edited directly
                                IsInherited = true,
                                IsModified = false,
                                IsSessionEdit = false
                            };
                            items.Add(item);
                        }
                    }
                }
            }

            // Layer 2: Mod entries (override or extend vanilla)
            foreach (var prop in _entity.GetMultiple(Command.CUSTOMMAGIC))
            {
                if (prop is BitmaskChanceProperty bcp && bcp.HasValue)
                {
                    var key = (bcp.Bitmask, bcp.Chance);

                    // Check if this exact entry already exists from vanilla
                    var existingItem = items.FirstOrDefault(i => i.Bitmask == bcp.Bitmask && i.Chance == bcp.Chance);
                    if (existingItem != null)
                    {
                        // Mark as having a mod property (editable)
                        existingItem.Property = bcp;
                        existingItem.IsInherited = false;
                        existingItem.IsModified = true;
                        existingItem.PropertyChanged += OnCustomMagicItemPropertyChanged;
                    }
                    else if (!knownEntries.Contains(key))
                    {
                        // New entry from mod (not in vanilla)
                        knownEntries.Add(key);
                        var item = new CustomMagicItem
                        {
                            Bitmask = bcp.Bitmask,
                            Chance = bcp.Chance,
                            Property = bcp,
                            IsInherited = false,
                            IsModified = true,
                            IsSessionEdit = false
                        };
                        item.PropertyChanged += OnCustomMagicItemPropertyChanged;
                        items.Add(item);
                    }
                }
            }

            _customMagicList = items;
            OnPropertyChanged(nameof(CustomMagicList));
        }

        private void OnCustomMagicItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CustomMagicItem item && item.Property != null)
            {
                // Sync changes back to the underlying BitmaskChanceProperty
                if (e.PropertyName == nameof(CustomMagicItem.Bitmask) || e.PropertyName == nameof(CustomMagicItem.Chance))
                {
                    item.Property.Bitmask = item.Bitmask;
                    item.Property.Chance = item.Chance;
                    item.IsSessionEdit = true;
                    HasSessionChanges = true;
                }
            }
        }

        private ICommand _addCustomMagicCommand;
        public ICommand AddCustomMagicCommand => _addCustomMagicCommand ??= new RelayCommand(AddCustomMagic);

        private ICommand _removeCustomMagicCommand;
        public ICommand RemoveCustomMagicCommand => _removeCustomMagicCommand ??= new RelayCommand<CustomMagicItem>(RemoveCustomMagic);

        private void AddCustomMagic()
        {
            var newProp = new BitmaskChanceProperty
            {
                Parent = _entity,
                Command = Command.CUSTOMMAGIC,
                Bitmask = 0,
                Chance = 100,
                HasValue = true
            };

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, "Add Custom Magic");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            HasSessionChanges = true;
            RefreshCustomMagic();
        }

        private void RemoveCustomMagic(CustomMagicItem item)
        {
            if (item?.Property == null) return;

            if (_history != null)
            {
                var cmd = new RemovePropertyCommand(_entity, item.Property, "Remove Custom Magic");
                _history.Execute(cmd);
            }
            else
            {
                _entity.RemoveProperty(item.Property);
            }

            HasSessionChanges = true;
            RefreshCustomMagic();
        }

        public void UpdateCustomMagic(CustomMagicItem item, ulong newBitmask, int newChance)
        {
            if (item?.Property == null) return;

            item.Property.Bitmask = newBitmask;
            item.Property.Chance = newChance;
            item.Bitmask = newBitmask;
            item.Chance = newChance;
            item.IsSessionEdit = true;

            HasSessionChanges = true;
        }
    }
}
