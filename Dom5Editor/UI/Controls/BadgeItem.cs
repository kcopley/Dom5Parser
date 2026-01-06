using System;
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
        private bool _hasValue;
        private bool _isModified;
        private bool _isSessionEdit;
        private bool _isInherited;
        private bool _canRemove = true;
        private bool _isReference;
        private int _referenceId;
        private string _referenceName;
        private string _referenceType;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raised when the Value property changes due to user editing.
        /// </summary>
        public event EventHandler<int> ValueChanged;

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
                        ValueChanged?.Invoke(this, intVal);
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
            set { _isInherited = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if user can remove this badge.
        /// </summary>
        public bool CanRemove
        {
            get => _canRemove;
            set { _canRemove = value; OnPropertyChanged(); }
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
        /// Display string for reference badges showing name or ID.
        /// </summary>
        public string ReferenceDisplay => !string.IsNullOrEmpty(ReferenceName)
            ? ReferenceName
            : $"#{ReferenceId}";

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
    }
}
