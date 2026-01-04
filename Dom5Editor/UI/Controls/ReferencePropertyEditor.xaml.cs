using System.Windows;
using System.Windows.Controls;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Editor control for entity reference properties (weapons, armor, spells, etc.).
    /// </summary>
    public partial class ReferencePropertyEditor : UserControl
    {
        public ReferencePropertyEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The label for this property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(ReferencePropertyEditor),
                new PropertyMetadata("Reference"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// The referenced entity ID.
        /// </summary>
        public static readonly DependencyProperty ReferenceIdProperty =
            DependencyProperty.Register(
                nameof(ReferenceId),
                typeof(int?),
                typeof(ReferencePropertyEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int? ReferenceId
        {
            get => (int?)GetValue(ReferenceIdProperty);
            set => SetValue(ReferenceIdProperty, value);
        }

        /// <summary>
        /// The referenced entity name (for display).
        /// </summary>
        public static readonly DependencyProperty ReferenceNameProperty =
            DependencyProperty.Register(
                nameof(ReferenceName),
                typeof(string),
                typeof(ReferencePropertyEditor),
                new PropertyMetadata(null));

        public string ReferenceName
        {
            get => (string)GetValue(ReferenceNameProperty);
            set => SetValue(ReferenceNameProperty, value);
        }

        /// <summary>
        /// The type of reference (for filtering in browse dialog).
        /// </summary>
        public static readonly DependencyProperty ReferenceTypeProperty =
            DependencyProperty.Register(
                nameof(ReferenceType),
                typeof(string),
                typeof(ReferencePropertyEditor),
                new PropertyMetadata("Entity"));

        public string ReferenceType
        {
            get => (string)GetValue(ReferenceTypeProperty);
            set => SetValue(ReferenceTypeProperty, value);
        }

        /// <summary>
        /// Event raised when browse is requested.
        /// </summary>
        public static readonly RoutedEvent BrowseRequestedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(BrowseRequested),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ReferencePropertyEditor));

        public event RoutedEventHandler BrowseRequested
        {
            add => AddHandler(BrowseRequestedEvent, value);
            remove => RemoveHandler(BrowseRequestedEvent, value);
        }

        /// <summary>
        /// Event raised when clear is requested.
        /// </summary>
        public static readonly RoutedEvent ClearRequestedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ClearRequested),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ReferencePropertyEditor));

        public event RoutedEventHandler ClearRequested
        {
            add => AddHandler(ClearRequestedEvent, value);
            remove => RemoveHandler(ClearRequestedEvent, value);
        }

        private void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BrowseRequestedEvent, this));
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            ReferenceId = null;
            ReferenceName = null;
            RaiseEvent(new RoutedEventArgs(ClearRequestedEvent, this));
        }
    }
}
