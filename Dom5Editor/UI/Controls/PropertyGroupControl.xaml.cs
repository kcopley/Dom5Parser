using System.Windows;
using System.Windows.Controls;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// A collapsible property group control for organizing entity properties.
    /// </summary>
    public partial class PropertyGroupControl : UserControl
    {
        public PropertyGroupControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The header text displayed for this group.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(PropertyGroupControl),
                new PropertyMetadata("Properties"));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// Whether the group is expanded.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(PropertyGroupControl),
                new PropertyMetadata(true));

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        /// <summary>
        /// Optional badge text (e.g., property count).
        /// </summary>
        public static readonly DependencyProperty BadgeTextProperty =
            DependencyProperty.Register(
                nameof(BadgeText),
                typeof(string),
                typeof(PropertyGroupControl),
                new PropertyMetadata(null));

        public string BadgeText
        {
            get => (string)GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        /// <summary>
        /// The content to display inside the group.
        /// </summary>
        public static readonly DependencyProperty GroupContentProperty =
            DependencyProperty.Register(
                nameof(GroupContent),
                typeof(object),
                typeof(PropertyGroupControl),
                new PropertyMetadata(null));

        public object GroupContent
        {
            get => GetValue(GroupContentProperty);
            set => SetValue(GroupContentProperty, value);
        }
    }
}
