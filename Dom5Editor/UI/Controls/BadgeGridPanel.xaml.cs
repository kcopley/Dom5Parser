using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Container control that displays badges in a fixed-column grid layout.
    /// Badges are rendered in JSON order, left-to-right, top-to-bottom.
    /// </summary>
    public partial class BadgeGridPanel : UserControl
    {
        public BadgeGridPanel()
        {
            InitializeComponent();

            // Subscribe to ReferenceClicked events bubbling up from CompactBadge controls
            AddHandler(CompactBadge.ReferenceClickedEvent, new RoutedEventHandler(OnReferenceClicked));

            // Subscribe to ReferenceChanged events bubbling up from CompactBadge controls
            AddHandler(CompactBadge.ReferenceChangedEvent, new RoutedEventHandler(OnReferenceChanged));
        }

        /// <summary>
        /// Handles ReferenceClicked events from child CompactBadge controls.
        /// </summary>
        private void OnReferenceClicked(object sender, RoutedEventArgs e)
        {
            if (e is ReferenceClickedEventArgs args && NavigateCommand != null)
            {
                var param = (args.ReferenceType, args.ReferenceId);
                if (NavigateCommand.CanExecute(param))
                {
                    NavigateCommand.Execute(param);
                }
            }
        }

        /// <summary>
        /// Handles ReferenceChanged events from child CompactBadge controls.
        /// </summary>
        private void OnReferenceChanged(object sender, RoutedEventArgs e)
        {
            if (e is ReferenceChangedRoutedEventArgs args && ReferenceChangedCommand != null)
            {
                if (e.OriginalSource is CompactBadge badge && badge.CommandParameter is PropertyItem item)
                {
                    var param = (item, args.OldId, args.NewId, args.NewName);
                    if (ReferenceChangedCommand.CanExecute(param))
                    {
                        ReferenceChangedCommand.Execute(param);
                    }
                }
            }
        }

        #region Columns

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(BadgeGridPanel),
                new PropertyMetadata(3));

        /// <summary>
        /// Number of columns in the grid. Default is 3.
        /// </summary>
        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region Items

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty AvailableItemsProperty =
            DependencyProperty.Register(nameof(AvailableItems), typeof(IEnumerable), typeof(BadgeGridPanel),
                new PropertyMetadata(null, OnAvailableItemsChanged));

        public IEnumerable AvailableItems
        {
            get => (IEnumerable)GetValue(AvailableItemsProperty);
            set => SetValue(AvailableItemsProperty, value);
        }

        private static void OnAvailableItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (BadgeGridPanel)d;
            panel.UpdateSearchablePropertyItems();
        }

        private void UpdateSearchablePropertyItems()
        {
            if (AvailableItems == null)
            {
                SearchablePropertyItems = null;
                return;
            }

            var items = new List<ReferenceItem>();
            foreach (var item in AvailableItems)
            {
                if (item is AvailablePropertyItem propItem)
                {
                    items.Add(propItem.ToReferenceItem());
                }
            }
            SearchablePropertyItems = items;
        }

        /// <summary>
        /// Converted available items for the searchable property dropdown.
        /// </summary>
        public static readonly DependencyProperty SearchablePropertyItemsProperty =
            DependencyProperty.Register(nameof(SearchablePropertyItems), typeof(IEnumerable<ReferenceItem>), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public IEnumerable<ReferenceItem> SearchablePropertyItems
        {
            get => (IEnumerable<ReferenceItem>)GetValue(SearchablePropertyItemsProperty);
            set => SetValue(SearchablePropertyItemsProperty, value);
        }

        /// <summary>
        /// Searchable available items for reference-type add dropdown.
        /// </summary>
        public static readonly DependencyProperty SearchableAvailableItemsProperty =
            DependencyProperty.Register(nameof(SearchableAvailableItems), typeof(IEnumerable<ReferenceItem>), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public IEnumerable<ReferenceItem> SearchableAvailableItems
        {
            get => (IEnumerable<ReferenceItem>)GetValue(SearchableAvailableItemsProperty);
            set => SetValue(SearchableAvailableItemsProperty, value);
        }

        #endregion

        #region Commands

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        /// <summary>
        /// Command for adding a reference item via the searchable dropdown.
        /// </summary>
        public static readonly DependencyProperty AddReferenceCommandProperty =
            DependencyProperty.Register(nameof(AddReferenceCommand), typeof(ICommand), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public ICommand AddReferenceCommand
        {
            get => (ICommand)GetValue(AddReferenceCommandProperty);
            set => SetValue(AddReferenceCommandProperty, value);
        }

        public static readonly DependencyProperty NavigateCommandProperty =
            DependencyProperty.Register(nameof(NavigateCommand), typeof(ICommand), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Command to execute when a reference badge is clicked for navigation.
        /// </summary>
        public ICommand NavigateCommand
        {
            get => (ICommand)GetValue(NavigateCommandProperty);
            set => SetValue(NavigateCommandProperty, value);
        }

        /// <summary>
        /// Command executed when a reference selection changes in an editable badge.
        /// </summary>
        public static readonly DependencyProperty ReferenceChangedCommandProperty =
            DependencyProperty.Register(nameof(ReferenceChangedCommand), typeof(ICommand), typeof(BadgeGridPanel),
                new PropertyMetadata(null));

        public ICommand ReferenceChangedCommand
        {
            get => (ICommand)GetValue(ReferenceChangedCommandProperty);
            set => SetValue(ReferenceChangedCommandProperty, value);
        }

        #endregion

        #region Show/Hide Add Buttons

        public static readonly DependencyProperty ShowAddButtonProperty =
            DependencyProperty.Register(nameof(ShowAddButton), typeof(bool), typeof(BadgeGridPanel),
                new PropertyMetadata(false, OnShowAddButtonChanged));

        public bool ShowAddButton
        {
            get => (bool)GetValue(ShowAddButtonProperty);
            set => SetValue(ShowAddButtonProperty, value);
        }

        /// <summary>
        /// When true, shows the searchable reference dropdown instead of the simple dropdown.
        /// </summary>
        public static readonly DependencyProperty UseSearchableAddProperty =
            DependencyProperty.Register(nameof(UseSearchableAdd), typeof(bool), typeof(BadgeGridPanel),
                new PropertyMetadata(false, OnShowAddButtonChanged));

        public bool UseSearchableAdd
        {
            get => (bool)GetValue(UseSearchableAddProperty);
            set => SetValue(UseSearchableAddProperty, value);
        }

        /// <summary>
        /// Computed: Show simple add button (ShowAddButton && !UseSearchableAdd)
        /// </summary>
        public static readonly DependencyProperty ShowSimpleAddButtonProperty =
            DependencyProperty.Register(nameof(ShowSimpleAddButton), typeof(bool), typeof(BadgeGridPanel),
                new PropertyMetadata(false));

        public bool ShowSimpleAddButton
        {
            get => (bool)GetValue(ShowSimpleAddButtonProperty);
            set => SetValue(ShowSimpleAddButtonProperty, value);
        }

        /// <summary>
        /// Computed: Show searchable add button (ShowAddButton && UseSearchableAdd)
        /// </summary>
        public static readonly DependencyProperty ShowSearchableAddButtonProperty =
            DependencyProperty.Register(nameof(ShowSearchableAddButton), typeof(bool), typeof(BadgeGridPanel),
                new PropertyMetadata(false));

        public bool ShowSearchableAddButton
        {
            get => (bool)GetValue(ShowSearchableAddButtonProperty);
            set => SetValue(ShowSearchableAddButtonProperty, value);
        }

        private static void OnShowAddButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (BadgeGridPanel)d;
            panel.UpdateAddButtonVisibility();
        }

        private void UpdateAddButtonVisibility()
        {
            ShowSimpleAddButton = ShowAddButton && !UseSearchableAdd;
            ShowSearchableAddButton = ShowAddButton && UseSearchableAdd;
        }

        #endregion

        #region Events

        public event EventHandler<object> ItemAdded;

        private void OnPropertyAddSelectionChanged(object sender, ReferenceSelectionChangedEventArgs e)
        {
            if (e.NewItem == null)
                return;

            // Extract the original AvailablePropertyItem from the Tag
            var selectedItem = e.NewItem.Tag as AvailablePropertyItem;
            if (selectedItem == null)
                return;

            if (AddCommand?.CanExecute(selectedItem) == true)
            {
                AddCommand.Execute(selectedItem);
            }

            ItemAdded?.Invoke(this, selectedItem);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                PropertyAddComboBox.SetSelectedIdSilent(null);
            }), DispatcherPriority.Input);
        }

        private void OnSearchableAddSelectionChanged(object sender, ReferenceSelectionChangedEventArgs e)
        {
            if (e.NewItem == null)
                return;

            if (AddReferenceCommand?.CanExecute(e.NewItem) == true)
            {
                AddReferenceCommand.Execute(e.NewItem);
            }

            if (AddCommand?.CanExecute(e.NewItem) == true)
            {
                AddCommand.Execute(e.NewItem);
            }

            ItemAdded?.Invoke(this, e.NewItem);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchableAddComboBox.SetSelectedIdSilent(null);
            }), DispatcherPriority.Input);
        }

        #endregion
    }
}
