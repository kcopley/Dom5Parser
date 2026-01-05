using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Dom5Edit.Commands;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Data item for property display controls (CompactBadge, BadgeWrapPanel).
    /// Supports flag properties (no value) and value properties (with editable int).
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
        /// Optional tag for additional data.
        /// </summary>
        public object Tag { get; set; }
    }
}
