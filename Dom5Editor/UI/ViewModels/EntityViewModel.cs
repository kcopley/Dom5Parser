using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Indicates the source/state of an entity.
    /// </summary>
    public enum EntitySource
    {
        /// <summary>Entity from vanilla (base game) with no modifications.</summary>
        Vanilla,
        /// <summary>Vanilla entity that has been modified by the loaded mod.</summary>
        VanillaModified,
        /// <summary>Entity created by the loaded mod (not in vanilla).</summary>
        FromMod,
        /// <summary>Entity created in the current editing session.</summary>
        New
    }

    /// <summary>
    /// Base ViewModel for all entity types.
    /// Provides common properties and functionality.
    /// </summary>
    public abstract class EntityViewModel : INotifyPropertyChanged
    {
        protected readonly IDEntity _entity;
        protected readonly CommandHistory _history;
        protected EntitySource _source;
        private bool _hasSessionChanges;

        /// <summary>
        /// Cached reference to the vanilla version of this entity (null if entity is new or not in vanilla).
        /// </summary>
        private IDEntity _vanillaEntity;
        private bool _vanillaEntityResolved;

        /// <summary>
        /// Reference to the ChangesMod for tracking session edits.
        /// </summary>
        private ChangesMod _changesMod;

        public event PropertyChangedEventHandler PropertyChanged;

        protected EntityViewModel(IDEntity entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
        {
            _entity = entity;
            _history = history;
            _source = source;
        }

        /// <summary>
        /// The underlying entity.
        /// </summary>
        public IDEntity Entity => _entity;

        /// <summary>
        /// Entity ID.
        /// </summary>
        public int ID => _entity.ID;

        /// <summary>
        /// Display name for lists and headers.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                var name = _entity.Name;

                // For VanillaModified entities, fall back to vanilla name if not in mod
                if (string.IsNullOrEmpty(name) && _source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    name = vanillaEntity?.Name;
                }

                return name ?? $"#{_entity.ID}";
            }
        }

        /// <summary>
        /// Entity name (editable).
        /// </summary>
        public string Name
        {
            get
            {
                var name = _entity.Name;

                // For VanillaModified entities, fall back to vanilla name if not in mod
                if (string.IsNullOrEmpty(name) && _source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    name = vanillaEntity?.Name;
                }

                return name ?? string.Empty;
            }
            set
            {
                if (_entity.Name != value)
                {
                    // TODO: Use SetNameCommand for undo support
                    _entity.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// The source/origin of this entity.
        /// </summary>
        public EntitySource Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    _source = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsVanilla));
                    OnPropertyChanged(nameof(IsFromMod));
                    OnPropertyChanged(nameof(IsModified));
                    OnPropertyChanged(nameof(IsNew));
                    OnPropertyChanged(nameof(SourceLabel));
                }
            }
        }

        /// <summary>
        /// True if this is a vanilla (base game) entity (modified or not).
        /// </summary>
        public bool IsVanilla => _source == EntitySource.Vanilla || _source == EntitySource.VanillaModified;

        /// <summary>
        /// True if this entity was created by the loaded mod (not vanilla).
        /// </summary>
        public bool IsFromMod => _source == EntitySource.FromMod;

        /// <summary>
        /// True if this vanilla entity has been modified by the mod or current session.
        /// </summary>
        public bool IsModified => _source == EntitySource.VanillaModified || _hasSessionChanges;

        /// <summary>
        /// True if this entity was created in the current editing session.
        /// </summary>
        public bool IsNew => _source == EntitySource.New;

        /// <summary>
        /// True if changes were made to this entity in the current session.
        /// </summary>
        public bool HasSessionChanges
        {
            get => _hasSessionChanges;
            set
            {
                if (_hasSessionChanges != value)
                {
                    _hasSessionChanges = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        /// <summary>
        /// Short label describing the entity source for display.
        /// </summary>
        public string SourceLabel => _source switch
        {
            EntitySource.Vanilla => "Vanilla",
            EntitySource.VanillaModified => "Modified",
            EntitySource.FromMod => "Mod",
            EntitySource.New => "New",
            _ => ""
        };

        /// <summary>
        /// Command history for undo/redo support.
        /// </summary>
        public CommandHistory History => _history;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ========================================
        // Property Access Helpers
        // ========================================

        protected string GetStringProperty(Command command)
        {
            var result = _entity.TryGet<StringProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return prop?.Value ?? string.Empty;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out var vanillaProp);
                    if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                    {
                        return vanillaProp?.Value ?? string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        protected void SetStringProperty(Command command, string value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            _entity.Set<StringProperty>(command, p => p.Value = value);

            // Check if value matches original - if so, revert from ChangesMod instead of recording
            if (_changesMod != null)
            {
                var original = GetOriginalStringValue(command);
                if (value == original)
                {
                    _changesMod.RevertPropertyChange(_entity, command);
                }
                else
                {
                    var prop = new StringProperty { Command = command, Value = value };
                    _changesMod.RecordPropertyChange(_entity, prop);
                }
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
            // Notify related session edit and modified properties
            if (propertyName != null)
            {
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Modified");
            }
        }

        protected int? GetIntProperty(Command command)
        {
            var result = _entity.TryGet<IntProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return prop?.Value;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
                    if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                    {
                        return vanillaProp?.Value;
                    }
                }
            }

            return null;
        }

        protected void SetIntProperty(Command command, int? value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (value.HasValue)
            {
                _entity.Set<IntProperty>(command, p => p.Value = value.Value);

                // Check if value matches original - if so, revert from ChangesMod instead of recording
                if (_changesMod != null)
                {
                    var original = GetOriginalIntValue(command);
                    if (value.Value == original)
                    {
                        _changesMod.RevertPropertyChange(_entity, command);
                    }
                    else
                    {
                        var prop = new IntProperty { Command = command, Value = value.Value };
                        _changesMod.RecordPropertyChange(_entity, prop);
                    }
                }
            }
            else
            {
                _entity.Remove<IntProperty>(command);

                // Check if null matches original (property didn't exist originally)
                if (_changesMod != null)
                {
                    var original = GetOriginalIntValue(command);
                    if (original == null)
                    {
                        _changesMod.RevertPropertyChange(_entity, command);
                    }
                    else
                    {
                        _changesMod.RecordPropertyRemoval(_entity, command);
                    }
                }
            }
            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
            // Notify related session edit and modified properties
            if (propertyName != null)
            {
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Modified");
            }
        }

        protected bool GetCommandProperty(Command command)
        {
            var result = _entity.TryGet<CommandProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return true;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);
                    return vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        protected void SetCommandProperty(Command command, bool value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (value)
            {
                _entity.SetCommand<CommandProperty>(command);

                // Check if value matches original - if so, revert from ChangesMod instead of recording
                if (_changesMod != null)
                {
                    var original = GetOriginalCommandValue(command);
                    if (original) // Original was true, setting to true - revert
                    {
                        _changesMod.RevertPropertyChange(_entity, command);
                    }
                    else
                    {
                        var prop = new CommandProperty { Command = command };
                        _changesMod.RecordPropertyChange(_entity, prop);
                    }
                }
            }
            else
            {
                _entity.Remove<CommandProperty>(command);

                // Check if false matches original (property didn't exist originally)
                if (_changesMod != null)
                {
                    var original = GetOriginalCommandValue(command);
                    if (!original) // Original was false, setting to false - revert
                    {
                        _changesMod.RevertPropertyChange(_entity, command);
                    }
                    else
                    {
                        _changesMod.RecordPropertyRemoval(_entity, command);
                    }
                }
            }
            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
            // Notify related session edit and modified properties
            if (propertyName != null)
            {
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Modified");
            }
        }

        protected bool IsIntPropertyInherited(Command command)
        {
            var result = _entity.TryGet<IntProperty>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        protected bool IsStringPropertyInherited(Command command)
        {
            var result = _entity.TryGet<StringProperty>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        protected bool IsCommandPropertyInherited(Command command)
        {
            var result = _entity.TryGet<CommandProperty>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        // ========================================
        // Modification Tracking Infrastructure
        // ========================================

        /// <summary>
        /// Sets the ChangesMod reference for session edit tracking.
        /// </summary>
        public void SetChangesMod(ChangesMod changesMod)
        {
            _changesMod = changesMod;
        }

        // ========================================
        // Original Value Helpers (before session edits)
        // ========================================

        /// <summary>
        /// Gets the original int value (from mod or vanilla, before any session edits).
        /// Used to detect when a value is reset to its original state.
        /// </summary>
        protected int? GetOriginalIntValue(Command command)
        {
            // For vanilla entities, the original is the vanilla value
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<IntProperty>(command, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value;
            }

            // For mod entities (FromMod), check if there's a value in the loaded mod
            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<IntProperty>(command, out var prop);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return prop?.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the original string value (from mod or vanilla, before any session edits).
        /// </summary>
        protected string GetOriginalStringValue(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<StringProperty>(command, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value ?? string.Empty;
            }

            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<StringProperty>(command, out var prop);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return prop?.Value ?? string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the original command (boolean) value (from mod or vanilla, before any session edits).
        /// </summary>
        protected bool GetOriginalCommandValue(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<CommandProperty>(command, out _);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return true;
            }

            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<CommandProperty>(command, out _);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Resets an int property to its original value (from mod or vanilla).
        /// </summary>
        public void ResetIntProperty(Command command, string propertyName)
        {
            var original = GetOriginalIntValue(command);
            if (original.HasValue)
            {
                _entity.Set<IntProperty>(command, p => p.Value = original.Value);
            }
            else
            {
                _entity.Remove<IntProperty>(command);
            }
            _changesMod?.RevertPropertyChange(_entity, command);
            OnPropertyChanged(propertyName);
            OnPropertyChanged($"Is{propertyName}SessionEdit");
            OnPropertyChanged($"Is{propertyName}Modified");
        }

        /// <summary>
        /// Resets a string property to its original value.
        /// </summary>
        public void ResetStringProperty(Command command, string propertyName)
        {
            var original = GetOriginalStringValue(command);
            if (!string.IsNullOrEmpty(original))
            {
                _entity.Set<StringProperty>(command, p => p.Value = original);
            }
            else
            {
                _entity.Remove<StringProperty>(command);
            }
            _changesMod?.RevertPropertyChange(_entity, command);
            OnPropertyChanged(propertyName);
            OnPropertyChanged($"Is{propertyName}SessionEdit");
            OnPropertyChanged($"Is{propertyName}Modified");
        }

        /// <summary>
        /// Resets a command (boolean) property to its original value.
        /// </summary>
        public void ResetCommandProperty(Command command, string propertyName)
        {
            var original = GetOriginalCommandValue(command);
            if (original)
            {
                _entity.SetCommand<CommandProperty>(command);
            }
            else
            {
                _entity.Remove<CommandProperty>(command);
            }
            _changesMod?.RevertPropertyChange(_entity, command);
            OnPropertyChanged(propertyName);
            OnPropertyChanged($"Is{propertyName}SessionEdit");
            OnPropertyChanged($"Is{propertyName}Modified");
        }

        /// <summary>
        /// Checks if an int property has a session edit that can be reset.
        /// </summary>
        public bool CanResetIntProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

        /// <summary>
        /// Checks if a string property has a session edit that can be reset.
        /// </summary>
        public bool CanResetStringProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

        /// <summary>
        /// Checks if a command property has a session edit that can be reset.
        /// </summary>
        public bool CanResetCommandProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

        /// <summary>
        /// Gets the vanilla version of this entity for comparison.
        /// Returns null if entity doesn't exist in vanilla.
        /// </summary>
        protected IDEntity GetVanillaEntity()
        {
            if (_vanillaEntityResolved)
                return _vanillaEntity;

            _vanillaEntityResolved = true;

            // New entities have no vanilla counterpart
            if (_source == EntitySource.New || _source == EntitySource.FromMod)
            {
                _vanillaEntity = null;
                return null;
            }

            // Look up vanilla entity by type and ID
            var entityType = GetEntityTypeFromEntity(_entity);
            if (entityType.HasValue && VanillaLoader.Vanilla?.Database.TryGetValue(entityType.Value, out var vanillaSet) == true)
            {
                vanillaSet.TryGetValue(_entity.ID, out var vanillaEntity);
                _vanillaEntity = vanillaEntity;
            }

            return _vanillaEntity;
        }

        /// <summary>
        /// Gets the EntityType for an entity using pattern matching.
        /// </summary>
        private static EntityType? GetEntityTypeFromEntity(IDEntity entity)
        {
            return entity switch
            {
                Monster _ => EntityType.MONSTER,
                Weapon _ => EntityType.WEAPON,
                Armor _ => EntityType.ARMOR,
                Spell _ => EntityType.SPELL,
                Item _ => EntityType.ITEM,
                Site _ => EntityType.SITE,
                Nation _ => EntityType.NATION,
                Event _ => EntityType.EVENT,
                Mercenary _ => EntityType.MERCENARY,
                Nametype _ => EntityType.NAMETYPE,
                Poptype _ => EntityType.POPTYPE,
                _ => null
            };
        }

        /// <summary>
        /// Checks if an int property differs from vanilla value.
        /// Returns true if the value is different from vanilla (mod change).
        /// </summary>
        protected bool IsIntPropertyModifiedFromVanilla(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                // No vanilla entity - if property exists, it's "modified" (new)
                var result = _entity.TryGet<IntProperty>(command, out _);
                return result == ReturnType.TRUE;
            }

            // Get current value from mod entity
            var currentResult = _entity.TryGet<IntProperty>(command, out var currentProp);
            var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);

            bool currentHasValue = currentResult == ReturnType.TRUE || currentResult == ReturnType.COPIED;
            bool vanillaHasValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;

            // For VanillaModified entities: if mod doesn't have the property,
            // the effective value is vanilla, so it's NOT modified
            if (_source == EntitySource.VanillaModified && !currentHasValue)
                return false;

            if (currentHasValue != vanillaHasValue)
                return true;

            if (!currentHasValue && !vanillaHasValue)
                return false;

            return currentProp?.Value != vanillaProp?.Value;
        }

        /// <summary>
        /// Checks if a string property differs from vanilla value.
        /// </summary>
        protected bool IsStringPropertyModifiedFromVanilla(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                var result = _entity.TryGet<StringProperty>(command, out _);
                return result == ReturnType.TRUE;
            }

            var currentResult = _entity.TryGet<StringProperty>(command, out var currentProp);
            var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out var vanillaProp);

            bool currentHasValue = currentResult == ReturnType.TRUE || currentResult == ReturnType.COPIED;
            bool vanillaHasValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;

            // For VanillaModified entities: if mod doesn't have the property,
            // the effective value is vanilla, so it's NOT modified
            if (_source == EntitySource.VanillaModified && !currentHasValue)
                return false;

            if (currentHasValue != vanillaHasValue)
                return true;

            if (!currentHasValue && !vanillaHasValue)
                return false;

            return currentProp?.Value != vanillaProp?.Value;
        }

        /// <summary>
        /// Checks if a command (boolean) property differs from vanilla.
        /// </summary>
        protected bool IsCommandPropertyModifiedFromVanilla(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                var result = _entity.TryGet<CommandProperty>(command, out _);
                return result == ReturnType.TRUE;
            }

            var currentResult = _entity.TryGet<CommandProperty>(command, out _);
            var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);

            bool currentHasValue = currentResult == ReturnType.TRUE || currentResult == ReturnType.COPIED;
            bool vanillaHasValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;

            // For VanillaModified entities: if mod doesn't have the property,
            // the effective value is vanilla, so it's NOT modified
            if (_source == EntitySource.VanillaModified && !currentHasValue)
                return false;

            return currentHasValue != vanillaHasValue;
        }

        /// <summary>
        /// Checks if a property was edited in the current session (tracked via ChangesMod).
        /// </summary>
        protected bool IsPropertyEditedInSession(Command command)
        {
            if (_changesMod == null)
                return false;

            var entityType = GetEntityTypeFromEntity(_entity);
            if (!entityType.HasValue)
                return false;

            if (!_changesMod.TryGetChanges(entityType.Value, _entity.ID, out var changes))
                return false;

            return changes.ChangedProperties.ContainsKey(command);
        }

        /// <summary>
        /// Checks if a property is modified from vanilla OR edited in session.
        /// This is the general "is modified" check for UI highlighting.
        /// </summary>
        protected bool IsPropertyModified(Command command, PropertyType propertyType)
        {
            // Check session edits first (more recent changes)
            if (IsPropertyEditedInSession(command))
                return true;

            // Check mod vs vanilla
            return propertyType switch
            {
                PropertyType.Int => IsIntPropertyModifiedFromVanilla(command),
                PropertyType.String => IsStringPropertyModifiedFromVanilla(command),
                PropertyType.Command => IsCommandPropertyModifiedFromVanilla(command),
                _ => false
            };
        }

        /// <summary>
        /// Property types for modification checking.
        /// </summary>
        protected enum PropertyType
        {
            Int,
            String,
            Command
        }

        // ========================================
        // List Building Helpers
        // ========================================

        /// <summary>
        /// Builds a list of active command properties from a set of possible commands.
        /// </summary>
        protected System.Collections.ObjectModel.ObservableCollection<Controls.CommandListItem> BuildCommandList(
            params Command[] commands)
        {
            var list = new System.Collections.ObjectModel.ObservableCollection<Controls.CommandListItem>();

            foreach (var cmd in commands)
            {
                bool isActive = GetCommandProperty(cmd);
                if (isActive)
                {
                    list.Add(new Controls.CommandListItem
                    {
                        Command = cmd,
                        DisplayName = GetCommandDisplayName(cmd),
                        IsInherited = IsCommandPropertyInherited(cmd),
                        IsModified = IsCommandPropertyModifiedFromVanilla(cmd),
                        IsSessionEdit = IsPropertyEditedInSession(cmd)
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// Builds a list of available commands that are not currently active.
        /// </summary>
        protected System.Collections.ObjectModel.ObservableCollection<Controls.AvailableCommandItem> BuildAvailableCommandList(
            System.Collections.ObjectModel.ObservableCollection<Controls.CommandListItem> activeList,
            params Command[] allCommands)
        {
            var available = new System.Collections.ObjectModel.ObservableCollection<Controls.AvailableCommandItem>();
            var activeCommands = new System.Collections.Generic.HashSet<Command>();

            foreach (var item in activeList)
            {
                activeCommands.Add(item.Command);
            }

            foreach (var cmd in allCommands)
            {
                if (!activeCommands.Contains(cmd))
                {
                    available.Add(new Controls.AvailableCommandItem
                    {
                        Command = cmd,
                        DisplayName = GetCommandDisplayName(cmd)
                    });
                }
            }

            return available;
        }

        /// <summary>
        /// Builds a list of active int properties from a set of possible commands.
        /// </summary>
        protected System.Collections.ObjectModel.ObservableCollection<Controls.IntPropertyListItem> BuildIntPropertyList(
            params Command[] commands)
        {
            var list = new System.Collections.ObjectModel.ObservableCollection<Controls.IntPropertyListItem>();

            foreach (var cmd in commands)
            {
                int? value = GetIntProperty(cmd);
                if (value.HasValue)
                {
                    var item = new Controls.IntPropertyListItem
                    {
                        Command = cmd,
                        DisplayName = GetCommandDisplayName(cmd),
                        Value = value.Value,
                        IsInherited = IsIntPropertyInherited(cmd),
                        IsModified = IsIntPropertyModifiedFromVanilla(cmd),
                        IsSessionEdit = IsPropertyEditedInSession(cmd)
                    };

                    // Wire up value change handler
                    item.ValueChanged += (sender, newValue) =>
                    {
                        SetIntProperty(cmd, newValue);
                    };

                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Builds a list of available int properties that are not currently active.
        /// </summary>
        protected System.Collections.ObjectModel.ObservableCollection<Controls.AvailableIntPropertyItem> BuildAvailableIntPropertyList(
            System.Collections.ObjectModel.ObservableCollection<Controls.IntPropertyListItem> activeList,
            params Command[] allCommands)
        {
            var available = new System.Collections.ObjectModel.ObservableCollection<Controls.AvailableIntPropertyItem>();
            var activeCommands = new System.Collections.Generic.HashSet<Command>();

            foreach (var item in activeList)
            {
                activeCommands.Add(item.Command);
            }

            foreach (var cmd in allCommands)
            {
                if (!activeCommands.Contains(cmd))
                {
                    available.Add(new Controls.AvailableIntPropertyItem
                    {
                        Command = cmd,
                        DisplayName = GetCommandDisplayName(cmd)
                    });
                }
            }

            return available;
        }

        /// <summary>
        /// Gets a human-readable display name for a command.
        /// </summary>
        protected virtual string GetCommandDisplayName(Command command)
        {
            // Convert command enum to display name
            // Override in subclasses for custom display names
            var name = command.ToString();
            // Convert SCREAMING_CASE to Title Case
            return System.Text.RegularExpressions.Regex.Replace(
                name.ToLower().Replace("_", " "),
                @"\b\w",
                m => m.Value.ToUpper());
        }
    }
}
