using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dom5Edit.Commands;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Data item for property display controls (CompactBadge, BadgeWrapPanel).
    /// Supports flag properties (no value), value properties (with editable int),
    /// and reference properties (linking to other entities).
    /// </summary>
    public class PropertyItem : INotifyPropertyChanged
    {
        private string _displayName;
        private string _value;
        private string _value2;
        private bool _hasValue;
        private bool _hasSecondValue;
        private bool _isModified;
        private bool _isSessionEdit;
        private bool _isInherited;
        private bool _canRemove = true;
        private bool _isReference;
        private int _referenceId;
        private string _referenceName;
        private string _referenceType;
        private IEnumerable<ReferenceItem> _availableReferences;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raised when the Value property changes due to user editing.
        /// </summary>
        public event EventHandler<int> ValueChanged;

        /// <summary>
        /// Raised when the Value2 property changes due to user editing (for IntIntProperty badges).
        /// </summary>
        public event EventHandler<int> Value2Changed;

        /// <summary>
        /// Raised when either Value or Value2 changes for IntIntProperty badges.
        /// Args: (value1, value2)
        /// </summary>
        public event EventHandler<(int, int)> IntIntValueChanged;

        /// <summary>
        /// Raised when the reference selection changes due to user editing.
        /// Args: (oldId, newId)
        /// </summary>
        public event EventHandler<ReferenceChangedEventArgs> ReferenceSelectionChanged;

        /// <summary>
        /// The command this badge represents.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// Display name shown on the badge.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Value string (for int badges). Null for flag badges.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    // Fire ValueChanged if we can parse it as int
                    if (int.TryParse(value, out var intVal))
                    {
                        // Clear inherited status when user edits the value
                        // (the value is now explicitly set, not inherited)
                        if (IsInherited)
                        {
                            IsInherited = false;
                        }
                        // Mark as session edit since user changed it
                        if (!IsSessionEdit)
                        {
                            IsSessionEdit = true;
                        }
                        ValueChanged?.Invoke(this, intVal);
                        // Also fire IntIntValueChanged if this is a two-value badge
                        if (HasSecondValue && int.TryParse(_value2, out var val2))
                        {
                            IntIntValueChanged?.Invoke(this, (intVal, val2));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Second value string (for IntIntProperty badges). Null for single-value badges.
        /// </summary>
        public string Value2
        {
            get => _value2;
            set
            {
                if (_value2 != value)
                {
                    _value2 = value;
                    OnPropertyChanged();
                    // Fire Value2Changed if we can parse it as int
                    if (int.TryParse(value, out var intVal))
                    {
                        if (IsInherited)
                        {
                            IsInherited = false;
                        }
                        if (!IsSessionEdit)
                        {
                            IsSessionEdit = true;
                        }
                        Value2Changed?.Invoke(this, intVal);
                        // Also fire IntIntValueChanged if both values are valid
                        if (int.TryParse(_value, out var val1))
                        {
                            IntIntValueChanged?.Invoke(this, (val1, intVal));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// True if this badge has an editable value.
        /// </summary>
        public bool HasValue
        {
            get => _hasValue;
            set { _hasValue = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if this badge has a second editable value (for IntIntProperty).
        /// </summary>
        public bool HasSecondValue
        {
            get => _hasSecondValue;
            set { _hasSecondValue = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if modified from vanilla.
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if edited in this session.
        /// </summary>
        public bool IsSessionEdit
        {
            get => _isSessionEdit;
            set { _isSessionEdit = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if inherited from copystats.
        /// </summary>
        public bool IsInherited
        {
            get => _isInherited;
            set { _isInherited = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsReferenceEditable)); }
        }

        /// <summary>
        /// True if user can remove this badge.
        /// </summary>
        public bool CanRemove
        {
            get => _canRemove;
            set { _canRemove = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsReferenceEditable)); }
        }

        /// <summary>
        /// True if this badge represents a reference to another entity.
        /// </summary>
        public bool IsReference
        {
            get => _isReference;
            set { _isReference = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The ID of the referenced entity (for reference badges).
        /// </summary>
        public int ReferenceId
        {
            get => _referenceId;
            set { _referenceId = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReferenceDisplay)); }
        }

        /// <summary>
        /// The name of the referenced entity (for display).
        /// </summary>
        public string ReferenceName
        {
            get => _referenceName;
            set { _referenceName = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReferenceDisplay)); }
        }

        /// <summary>
        /// The type of entity being referenced (e.g., "monster", "nation", "site").
        /// </summary>
        public string ReferenceType
        {
            get => _referenceType;
            set { _referenceType = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Available reference items for the searchable dropdown.
        /// Set this to enable reference selection.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableReferences
        {
            get => _availableReferences;
            set
            {
                _availableReferences = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReferenceEditable));
            }
        }

        /// <summary>
        /// True if this reference badge can be edited (has available references and is not inherited).
        /// </summary>
        public bool IsReferenceEditable => IsReference && CanRemove && !IsInherited && AvailableReferences != null;

        /// <summary>
        /// Display string for reference badges showing name or ID.
        /// </summary>
        public string ReferenceDisplay => !string.IsNullOrEmpty(ReferenceName)
            ? ReferenceName
            : $"#{ReferenceId}";

        /// <summary>
        /// Called when user selects a different reference from the dropdown.
        /// </summary>
        public void OnReferenceChanged(int oldId, int newId, string newName)
        {
            if (oldId == newId) return;

            var oldIdCopy = ReferenceId;
            ReferenceId = newId;
            ReferenceName = newName;

            ReferenceSelectionChanged?.Invoke(this, new ReferenceChangedEventArgs
            {
                OldId = oldIdCopy,
                NewId = newId,
                NewName = newName
            });
        }

        /// <summary>
        /// Background brush for the badge.
        /// </summary>
        public Brush Background { get; set; } = new SolidColorBrush(Color.FromRgb(60, 60, 60));

        /// <summary>
        /// Border brush for the badge.
        /// </summary>
        public Brush BorderBrush { get; set; } = new SolidColorBrush(Color.FromRgb(80, 80, 80));

        /// <summary>
        /// Foreground brush for text.
        /// </summary>
        public Brush Foreground { get; set; } = Brushes.White;

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public string Tooltip { get; set; }

        private string _iconPath;
        /// <summary>
        /// Optional icon path (relative to icons folder, e.g., "magicicons/Path_F.png").
        /// </summary>
        public string IconPath
        {
            get => _iconPath;
            set
            {
                if (_iconPath != value)
                {
                    _iconPath = value;
                    _iconSource = null; // Reset cached icon
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasIcon));
                    OnPropertyChanged(nameof(IconSource));
                }
            }
        }

        /// <summary>
        /// True if this badge has an icon to display.
        /// </summary>
        public bool HasIcon => !string.IsNullOrEmpty(IconPath);

        private ImageSource _iconSource;
        /// <summary>
        /// Gets the icon image source, loading it lazily from IconPath.
        /// </summary>
        public ImageSource IconSource
        {
            get
            {
                if (_iconSource == null && HasIcon)
                {
                    _iconSource = LoadIcon(IconPath);
                }
                return _iconSource;
            }
        }

        private static ImageSource LoadIcon(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            try
            {
                var possibleBasePaths = new[]
                {
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    AppDomain.CurrentDomain.BaseDirectory,
                    Directory.GetCurrentDirectory()
                };

                foreach (var basePath in possibleBasePaths)
                {
                    if (string.IsNullOrEmpty(basePath))
                        continue;

                    var fullPath = Path.Combine(basePath, "icons", relativePath);
                    if (File.Exists(fullPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Optional tag for additional data.
        /// </summary>
        public object Tag { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Factory Methods

        /// <summary>
        /// Creates a flag property (no value, just presence indicator).
        /// </summary>
        public static PropertyItem CreateFlag(Command command, string displayName, bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                HasValue = false,
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a value property (with editable int value).
        /// </summary>
        public static PropertyItem CreateValue(Command command, string displayName, int value, bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                Value = value.ToString(),
                HasValue = true,
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a colored value property (e.g., for resistances).
        /// </summary>
        public static PropertyItem CreateColoredValue(Command command, string displayName, int value,
            Color backgroundColor, Color borderColor, Color foregroundColor,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                Value = value.ToString(),
                HasValue = true,
                Background = new SolidColorBrush(backgroundColor),
                BorderBrush = new SolidColorBrush(borderColor),
                Foreground = new SolidColorBrush(foregroundColor),
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a two-value property (for IntIntProperty, e.g., #gems, #magicskill).
        /// </summary>
        public static PropertyItem CreateIntIntValue(Command command, string displayName, int value1, int value2,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                Value = value1.ToString(),
                Value2 = value2.ToString(),
                HasValue = true,
                HasSecondValue = true,
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a colored two-value property.
        /// </summary>
        public static PropertyItem CreateColoredIntIntValue(Command command, string displayName, int value1, int value2,
            Color backgroundColor, Color borderColor, Color foregroundColor,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                Value = value1.ToString(),
                Value2 = value2.ToString(),
                HasValue = true,
                HasSecondValue = true,
                Background = new SolidColorBrush(backgroundColor),
                BorderBrush = new SolidColorBrush(borderColor),
                Foreground = new SolidColorBrush(foregroundColor),
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a string value property (for StringProperty, e.g., #descr).
        /// </summary>
        public static PropertyItem CreateStringValue(Command command, string displayName, string value,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                Value = value ?? string.Empty,
                HasValue = true,
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a bitmask value property (for BitmaskProperty, e.g., #itemslots).
        /// Value is stored as a string representation of the ulong bitmask.
        /// </summary>
        public static PropertyItem CreateBitmaskValue(Command command, string displayName, ulong value,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                Value = value.ToString(),
                HasValue = true,
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a reference property (linking to another entity).
        /// </summary>
        public static PropertyItem CreateReference(Command command, string displayName, int referenceId,
            string referenceName, string referenceType,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                HasValue = false, // References don't use the Value field for editing
                IsReference = true,
                ReferenceId = referenceId,
                ReferenceName = referenceName,
                ReferenceType = referenceType,
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        /// <summary>
        /// Creates a colored reference property (for visually distinct entity types).
        /// </summary>
        public static PropertyItem CreateColoredReference(Command command, string displayName, int referenceId,
            string referenceName, string referenceType,
            Color backgroundColor, Color borderColor, Color foregroundColor,
            bool isModified = false, bool isSessionEdit = false)
        {
            return new PropertyItem
            {
                Command = command,
                DisplayName = displayName,
                HasValue = false,
                IsReference = true,
                ReferenceId = referenceId,
                ReferenceName = referenceName,
                ReferenceType = referenceType,
                Background = new SolidColorBrush(backgroundColor),
                BorderBrush = new SolidColorBrush(borderColor),
                Foreground = new SolidColorBrush(foregroundColor),
                IsModified = isModified,
                IsSessionEdit = isSessionEdit
            };
        }

        #endregion
    }

    /// <summary>
    /// Item representing an available option in the Add dropdown.
    /// </summary>
    public class AvailablePropertyItem
    {
        /// <summary>
        /// The command this item represents.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// Display name in the dropdown.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Default value when added (for value badges).
        /// </summary>
        public int? DefaultValue { get; set; }

        /// <summary>
        /// True if this is a reference property.
        /// </summary>
        public bool IsReference { get; set; }

        /// <summary>
        /// The type of entity being referenced (e.g., "monster", "nation", "site").
        /// </summary>
        public string ReferenceType { get; set; }

        /// <summary>
        /// Optional tag for additional data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Converts this item to a ReferenceItem for use in searchable dropdowns.
        /// The original AvailablePropertyItem is stored in the Tag property.
        /// </summary>
        public ReferenceItem ToReferenceItem()
        {
            return new ReferenceItem
            {
                ID = (int)Command,
                DisplayName = DisplayName,
                Tag = this
            };
        }
    }

    /// <summary>
    /// Event args for reference selection changes.
    /// </summary>
    public class ReferenceChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The previous reference ID.
        /// </summary>
        public int OldId { get; set; }

        /// <summary>
        /// The new reference ID.
        /// </summary>
        public int NewId { get; set; }

        /// <summary>
        /// The new reference name (display name of the entity).
        /// </summary>
        public string NewName { get; set; }
    }
}
