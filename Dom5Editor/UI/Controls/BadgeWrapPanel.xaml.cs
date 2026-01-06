using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Container control that displays badges in a horizontal wrapping layout
    /// with an "Add" combobox at the end for adding new items.
    /// </summary>
    public partial class BadgeWrapPanel : UserControl
    {
        public BadgeWrapPanel()
        {
            InitializeComponent();

            // Subscribe to ReferenceClicked events bubbling up from CompactBadge controls
            AddHandler(CompactBadge.ReferenceClickedEvent, new RoutedEventHandler(OnReferenceClicked));
        }

        /// <summary>
        /// Handles ReferenceClicked events from child CompactBadge controls.
        /// </summary>
        private void OnReferenceClicked(object sender, RoutedEventArgs e)
        {
            if (e is ReferenceClickedEventArgs args && NavigateCommand != null)
            {
                // Execute navigation command with (refType, id) tuple
                var param = (args.ReferenceType, args.ReferenceId);
                if (NavigateCommand.CanExecute(param))
                {
                    NavigateCommand.Execute(param);
                }
            }
        }

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(BadgeWrapPanel),
                new PropertyMetadata(null));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region Items

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(BadgeWrapPanel),
                new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty AvailableItemsProperty =
            DependencyProperty.Register(nameof(AvailableItems), typeof(IEnumerable), typeof(BadgeWrapPanel),
                new PropertyMetadata(null));

        public IEnumerable AvailableItems
        {
            get => (IEnumerable)GetValue(AvailableItemsProperty);
            set => SetValue(AvailableItemsProperty, value);
        }

        #endregion

        #region Commands

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(BadgeWrapPanel),
                new PropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(BadgeWrapPanel),
                new PropertyMetadata(null));

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.Register(nameof(NavigateCommand), typeof(ICommand), typeof(BadgeWrapPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Command to execute when a reference badge is clicked for navigation.
        /// The command parameter is a ValueTuple&lt;string, int&gt; containing (ReferenceType, ReferenceId).
        /// </summary>
        public ICommand NavigateCommand
        {
            get => (ICommand)GetValue(NavigateCommandProperty);
            set => SetValue(NavigateCommandProperty, value);
        }

        #endregion

        #region Show/Hide Add Button

        public static readonly DependencyProperty ShowAddButtonProperty =
            DependencyProperty.Register(nameof(ShowAddButton), typeof(bool), typeof(BadgeWrapPanel),
                new PropertyMetadata(true));

        public bool ShowAddButton
        {
            get => (bool)GetValue(ShowAddButtonProperty);
            set => SetValue(ShowAddButtonProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler<object> ItemAdded;

        private void OnAddSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddComboBox.SelectedItem == null)
                return;

            var selectedItem = AddComboBox.SelectedItem;

            // Execute add command if available
            if (AddCommand?.CanExecute(selectedItem) == true)
            {
                AddCommand.Execute(selectedItem);
            }

            // Raise event
            ItemAdded?.Invoke(this, selectedItem);

            // Clear selection after a short delay to allow the command to execute
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AddComboBox.SelectedItem = null;
            }), DispatcherPriority.Input);
        }

        private void OnComboBoxPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Prevent scroll wheel from changing selection when dropdown is closed
            var comboBox = sender as ComboBox;
            if (comboBox != null && !comboBox.IsDropDownOpen)
            {
                e.Handled = true;
            }
        }

        #endregion
    }
}
