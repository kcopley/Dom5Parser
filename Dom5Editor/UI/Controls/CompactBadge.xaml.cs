using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Compact badge control for displaying properties in horizontal layouts.
    /// Supports flag badges (no value), int-value badges, and reference badges.
    /// </summary>
    public partial class CompactBadge : UserControl
    {
        public CompactBadge()
        {
            InitializeComponent();
        }

        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(CompactBadge),
                new PropertyMetadata("Label"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region Value (for int badges)

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(CompactBadge),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty HasValueProperty =
            DependencyProperty.Register(nameof(HasValue), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(false));

        public bool HasValue
        {
            get => (bool)GetValue(HasValueProperty);
            set => SetValue(HasValueProperty, value);
        }

        #endregion

        #region Styling

        public static readonly DependencyProperty BadgeBackgroundProperty =
            DependencyProperty.Register(nameof(BadgeBackground), typeof(Brush), typeof(CompactBadge),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(60, 60, 60))));

        public Brush BadgeBackground
        {
            get => (Brush)GetValue(BadgeBackgroundProperty);
            set => SetValue(BadgeBackgroundProperty, value);
        }

        public static readonly DependencyProperty BadgeBorderProperty =
            DependencyProperty.Register(nameof(BadgeBorder), typeof(Brush), typeof(CompactBadge),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(80, 80, 80))));

        public Brush BadgeBorder
        {
            get => (Brush)GetValue(BadgeBorderProperty);
            set => SetValue(BadgeBorderProperty, value);
        }

        public static readonly DependencyProperty LabelForegroundProperty =
            DependencyProperty.Register(nameof(LabelForeground), typeof(Brush), typeof(CompactBadge),
                new PropertyMetadata(Brushes.White));

        public Brush LabelForeground
        {
            get => (Brush)GetValue(LabelForegroundProperty);
            set => SetValue(LabelForegroundProperty, value);
        }

        #endregion

        #region State Indicators

        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register(nameof(IsModified), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(false));

        public bool IsModified
        {
            get => (bool)GetValue(IsModifiedProperty);
            set => SetValue(IsModifiedProperty, value);
        }

        public static readonly DependencyProperty IsSessionEditProperty =
            DependencyProperty.Register(nameof(IsSessionEdit), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(false));

        public bool IsSessionEdit
        {
            get => (bool)GetValue(IsSessionEditProperty);
            set => SetValue(IsSessionEditProperty, value);
        }

        public static readonly DependencyProperty IsInheritedProperty =
            DependencyProperty.Register(nameof(IsInherited), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(false));

        public bool IsInherited
        {
            get => (bool)GetValue(IsInheritedProperty);
            set => SetValue(IsInheritedProperty, value);
        }

        #endregion

        #region Remove Command

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(CompactBadge),
                new PropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CompactBadge),
                new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CanRemoveProperty =
            DependencyProperty.Register(nameof(CanRemove), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(true));

        public bool CanRemove
        {
            get => (bool)GetValue(CanRemoveProperty);
            set => SetValue(CanRemoveProperty, value);
        }

        #endregion

        #region Tooltip

        public static readonly DependencyProperty TooltipProperty =
            DependencyProperty.Register(nameof(Tooltip), typeof(string), typeof(CompactBadge),
                new PropertyMetadata(null));

        public string Tooltip
        {
            get => (string)GetValue(TooltipProperty);
            set => SetValue(TooltipProperty, value);
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(CompactBadge),
                new PropertyMetadata(null));

        public ImageSource IconSource
        {
            get => (ImageSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty HasIconProperty =
            DependencyProperty.Register(nameof(HasIcon), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(false));

        public bool HasIcon
        {
            get => (bool)GetValue(HasIconProperty);
            set => SetValue(HasIconProperty, value);
        }

        #endregion

        #region Reference Support

        public static readonly DependencyProperty IsReferenceProperty =
            DependencyProperty.Register(nameof(IsReference), typeof(bool), typeof(CompactBadge),
                new PropertyMetadata(false));

        public bool IsReference
        {
            get => (bool)GetValue(IsReferenceProperty);
            set => SetValue(IsReferenceProperty, value);
        }

        public static readonly DependencyProperty ReferenceDisplayProperty =
            DependencyProperty.Register(nameof(ReferenceDisplay), typeof(string), typeof(CompactBadge),
                new PropertyMetadata(null));

        public string ReferenceDisplay
        {
            get => (string)GetValue(ReferenceDisplayProperty);
            set => SetValue(ReferenceDisplayProperty, value);
        }

        public static readonly DependencyProperty ReferenceTypeProperty =
            DependencyProperty.Register(nameof(ReferenceType), typeof(string), typeof(CompactBadge),
                new PropertyMetadata(null));

        public string ReferenceType
        {
            get => (string)GetValue(ReferenceTypeProperty);
            set => SetValue(ReferenceTypeProperty, value);
        }

        public static readonly DependencyProperty ReferenceIdProperty =
            DependencyProperty.Register(nameof(ReferenceId), typeof(int), typeof(CompactBadge),
                new PropertyMetadata(0));

        public int ReferenceId
        {
            get => (int)GetValue(ReferenceIdProperty);
            set => SetValue(ReferenceIdProperty, value);
        }

        /// <summary>
        /// Routed event for when a reference is clicked for navigation.
        /// </summary>
        public static readonly RoutedEvent ReferenceClickedEvent =
            EventManager.RegisterRoutedEvent(
                "ReferenceClicked",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(CompactBadge));

        /// <summary>
        /// Occurs when a reference badge is clicked for navigation.
        /// </summary>
        public event RoutedEventHandler ReferenceClicked
        {
            add => AddHandler(ReferenceClickedEvent, value);
            remove => RemoveHandler(ReferenceClickedEvent, value);
        }

        /// <summary>
        /// Handles click on the reference display text.
        /// </summary>
        private void OnReferenceClick(object sender, MouseButtonEventArgs e)
        {
            if (IsReference && ReferenceId != 0)
            {
                RaiseEvent(new ReferenceClickedEventArgs(ReferenceClickedEvent, this)
                {
                    ReferenceType = ReferenceType,
                    ReferenceId = ReferenceId
                });
                e.Handled = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Event args for reference clicked events, carrying the reference type and ID.
    /// </summary>
    public class ReferenceClickedEventArgs : RoutedEventArgs
    {
        public string ReferenceType { get; set; }
        public int ReferenceId { get; set; }

        public ReferenceClickedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
        }
    }
}
