using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// A searchable combo box for selecting entity references.
    /// Supports filtering by name or ID as the user types.
    /// Display shows name with ID separately; dropdown shows both.
    /// </summary>
    public partial class SearchableReferenceComboBox : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _filterDebounceTimer;
        private bool _isUpdatingFromSelection;
        private bool _isUpdatingFromCode;
        private string _lastSelectedName;

        public SearchableReferenceComboBox()
        {
            InitializeComponent();

            // Set up debounce timer for filter (150ms delay)
            _filterDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _filterDebounceTimer.Tick += OnFilterDebounceTimerTick;

            FilteredItems = new ObservableCollection<ReferenceItem>();

            // Handle clicks outside the control to close dropdown
            Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.PreviewMouseDown += OnWindowPreviewMouseDown;
                }
            };

            Unloaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.PreviewMouseDown -= OnWindowPreviewMouseDown;
                }
            };
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsDropdownOpen) return;

            // Check if click is within our control or popup
            var clickedElement = e.OriginalSource as DependencyObject;
            if (clickedElement != null)
            {
                // Check if click is within this control
                if (IsDescendantOf(clickedElement, this)) return;

                // Check if click is within the popup
                if (IsDescendantOf(clickedElement, DropdownPopup.Child)) return;
            }

            // Click was outside - close dropdown
            IsDropdownOpen = false;
        }

        private static bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            if (element == null || parent == null) return false;

            var current = element;
            while (current != null)
            {
                if (current == parent) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Dependency Properties

        /// <summary>
        /// The available items to select from.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<ReferenceItem>), typeof(SearchableReferenceComboBox),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable<ReferenceItem> ItemsSource
        {
            get => (IEnumerable<ReferenceItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// The currently selected item ID.
        /// </summary>
        public static readonly DependencyProperty SelectedIdProperty =
            DependencyProperty.Register(nameof(SelectedId), typeof(int?), typeof(SearchableReferenceComboBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIdChanged));

        public int? SelectedId
        {
            get => (int?)GetValue(SelectedIdProperty);
            set => SetValue(SelectedIdProperty, value);
        }

        /// <summary>
        /// Placeholder text shown when nothing is selected.
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchableReferenceComboBox),
                new PropertyMetadata("Select..."));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>
        /// Whether the control is read-only.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SearchableReferenceComboBox),
                new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        #endregion

        #region Notify Properties

        private string _filterText = "";
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged();

                    // Restart debounce timer when user is typing
                    if (!_isUpdatingFromSelection && IsDropdownOpen)
                    {
                        _filterDebounceTimer.Stop();
                        _filterDebounceTimer.Start();
                    }
                }
            }
        }

        private bool _isDropdownOpen;
        public bool IsDropdownOpen
        {
            get => _isDropdownOpen;
            set
            {
                if (_isDropdownOpen != value)
                {
                    _isDropdownOpen = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        // Opening dropdown - apply filter with a slight delay to ensure bindings are resolved
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ApplyFilter();
                        }), DispatcherPriority.DataBind);
                    }
                    else
                    {
                        // Closing dropdown - restore display text
                        RestoreDisplayText();
                    }
                }
            }
        }

        private ObservableCollection<ReferenceItem> _filteredItems;
        public ObservableCollection<ReferenceItem> FilteredItems
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoResults));
                OnPropertyChanged(nameof(ShowResultCount));
                OnPropertyChanged(nameof(ResultCountText));
            }
        }

        private ReferenceItem _selectedFilteredItem;
        public ReferenceItem SelectedFilteredItem
        {
            get => _selectedFilteredItem;
            set
            {
                if (_selectedFilteredItem != value)
                {
                    _selectedFilteredItem = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Display string for the ID (e.g., "#123").
        /// </summary>
        public string DisplayId => SelectedId.HasValue ? $"#{SelectedId.Value}" : "";

        /// <summary>
        /// True if there's a selected ID to display.
        /// </summary>
        public bool HasSelectedId => SelectedId.HasValue && !IsDropdownOpen;

        public bool HasNoResults => FilteredItems?.Count == 0 && !string.IsNullOrEmpty(FilterText);

        public bool ShowResultCount => FilteredItems?.Count > 0 && ItemsSource != null && IsDropdownOpen;

        public string ResultCountText
        {
            get
            {
                var filtered = FilteredItems?.Count ?? 0;
                var total = ItemsSource?.Count() ?? 0;
                return filtered == total ? $"{total} items" : $"{filtered} of {total}";
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the selected reference changes.
        /// </summary>
        public event EventHandler<ReferenceSelectionChangedEventArgs> SelectionChanged;

        #endregion

        #region Property Changed Handlers

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableReferenceComboBox)d;
            control.ApplyFilter();
            control.RestoreDisplayText();
        }

        private static void OnSelectedIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchableReferenceComboBox)d;
            control.OnPropertyChanged(nameof(DisplayId));
            control.OnPropertyChanged(nameof(HasSelectedId));

            if (!control._isUpdatingFromCode)
            {
                control.RestoreDisplayText();
            }
        }

        #endregion

        #region Event Handlers

        private void OnControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Clicking anywhere on the control focuses the text box and opens dropdown
            if (IsReadOnly) return;

            if (!FilterTextBox.IsFocused)
            {
                FilterTextBox.Focus();
                e.Handled = true;
            }
        }

        private void OnTextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsReadOnly) return;

            // Open dropdown on click (if not already open)
            if (!IsDropdownOpen)
            {
                IsDropdownOpen = true;
            }

            // Select all text after a brief delay to ensure focus is set
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (FilterTextBox.IsFocused)
                {
                    FilterTextBox.SelectAll();
                }
            }), DispatcherPriority.Input);

            // Don't mark as handled - let the textbox get focus naturally
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly) return;

            // Open dropdown and select all text
            IsDropdownOpen = true;
            FilterTextBox.SelectAll();

            // Update display properties
            OnPropertyChanged(nameof(HasSelectedId));
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // Small delay to allow click on dropdown item or other UI interactions
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Check if focus moved to something within our control
                if (!ItemsListBox.IsMouseOver &&
                    !DropdownButton.IsMouseOver &&
                    !FilterTextBox.IsMouseOver &&
                    !FilterTextBox.IsFocused)
                {
                    IsDropdownOpen = false;
                }
            }), DispatcherPriority.Background);
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (IsDropdownOpen && FilteredItems.Count > 0)
                    {
                        var currentIndex = FilteredItems.IndexOf(SelectedFilteredItem);
                        if (currentIndex < FilteredItems.Count - 1)
                        {
                            SelectedFilteredItem = FilteredItems[currentIndex + 1];
                            ItemsListBox.ScrollIntoView(SelectedFilteredItem);
                        }
                        else if (currentIndex == -1)
                        {
                            SelectedFilteredItem = FilteredItems[0];
                            ItemsListBox.ScrollIntoView(SelectedFilteredItem);
                        }
                    }
                    else if (!IsDropdownOpen)
                    {
                        IsDropdownOpen = true;
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (IsDropdownOpen && FilteredItems.Count > 0)
                    {
                        var currentIndex = FilteredItems.IndexOf(SelectedFilteredItem);
                        if (currentIndex > 0)
                        {
                            SelectedFilteredItem = FilteredItems[currentIndex - 1];
                            ItemsListBox.ScrollIntoView(SelectedFilteredItem);
                        }
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (IsDropdownOpen && SelectedFilteredItem != null)
                    {
                        CommitSelection(SelectedFilteredItem);
                    }
                    else if (IsDropdownOpen && FilteredItems.Count > 0)
                    {
                        CommitSelection(FilteredItems[0]);
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    IsDropdownOpen = false;
                    RestoreDisplayText();
                    // Move focus away
                    Keyboard.ClearFocus();
                    e.Handled = true;
                    break;

                case Key.Tab:
                    if (IsDropdownOpen)
                    {
                        if (SelectedFilteredItem != null)
                        {
                            CommitSelection(SelectedFilteredItem);
                        }
                        else if (FilteredItems.Count > 0)
                        {
                            CommitSelection(FilteredItems[0]);
                        }
                        IsDropdownOpen = false;
                    }
                    break;
            }
        }

        private void OnFilterDebounceTimerTick(object sender, EventArgs e)
        {
            _filterDebounceTimer.Stop();
            ApplyFilter();
        }

        private void OnDropdownButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly) return;

            // The ToggleButton already handles opening/closing via IsChecked binding.
            // When open, focus the text box for typing.
            if (IsDropdownOpen)
            {
                // Delay focus to ensure dropdown is fully open
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    FilterTextBox.Focus();
                    FilterTextBox.SelectAll();
                }), DispatcherPriority.Input);
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection highlight only, don't commit yet
        }

        private void OnListBoxItemClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedFilteredItem != null)
            {
                CommitSelection(SelectedFilteredItem);
            }
        }

        #endregion

        #region Methods

        private void ApplyFilter()
        {
            if (ItemsSource == null)
            {
                FilteredItems.Clear();
                return;
            }

            var filter = FilterText?.Trim().ToLowerInvariant() ?? "";
            var filtered = string.IsNullOrEmpty(filter)
                ? ItemsSource.ToList()
                : ItemsSource.Where(item =>
                    (item.DisplayName?.ToLowerInvariant().Contains(filter) ?? false) ||
                    item.ID.ToString().Contains(filter))
                    .ToList();

            // Limit results for performance
            const int maxResults = 100;
            if (filtered.Count > maxResults)
            {
                filtered = filtered.Take(maxResults).ToList();
            }

            FilteredItems.Clear();
            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }

            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(ShowResultCount));
            OnPropertyChanged(nameof(ResultCountText));

            // Auto-select first item if we have results and nothing selected
            if (FilteredItems.Count > 0 && (SelectedFilteredItem == null || !FilteredItems.Contains(SelectedFilteredItem)))
            {
                SelectedFilteredItem = FilteredItems[0];
            }
        }

        private void RestoreDisplayText()
        {
            _isUpdatingFromSelection = true;
            try
            {
                if (SelectedId.HasValue && ItemsSource != null)
                {
                    var selected = ItemsSource.FirstOrDefault(i => i.ID == SelectedId.Value);
                    if (selected != null)
                    {
                        // Show just the name in the text box
                        FilterText = selected.DisplayName ?? "";
                        _lastSelectedName = FilterText;
                        OnPropertyChanged(nameof(DisplayId));
                        OnPropertyChanged(nameof(HasSelectedId));
                        return;
                    }
                }

                // No selection - show placeholder or empty
                FilterText = "";
                _lastSelectedName = "";
                OnPropertyChanged(nameof(DisplayId));
                OnPropertyChanged(nameof(HasSelectedId));
            }
            finally
            {
                _isUpdatingFromSelection = false;
            }
        }

        private void CommitSelection(ReferenceItem item)
        {
            if (item == null) return;

            var oldId = SelectedId;

            _isUpdatingFromCode = true;
            try
            {
                SelectedId = item.ID;
                _lastSelectedName = item.DisplayName ?? "";
            }
            finally
            {
                _isUpdatingFromCode = false;
            }

            IsDropdownOpen = false;
            RestoreDisplayText();

            // Raise selection changed event
            if (oldId != item.ID)
            {
                SelectionChanged?.Invoke(this, new ReferenceSelectionChangedEventArgs
                {
                    OldId = oldId,
                    NewId = item.ID,
                    NewItem = item
                });
            }
        }

        /// <summary>
        /// Programmatically sets the selected item by ID without triggering change events.
        /// </summary>
        public void SetSelectedIdSilent(int? id)
        {
            _isUpdatingFromCode = true;
            try
            {
                SelectedId = id;
                RestoreDisplayText();
            }
            finally
            {
                _isUpdatingFromCode = false;
            }
        }

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents an item in the searchable reference dropdown.
    /// </summary>
    public class ReferenceItem
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional tag for additional data.
        /// </summary>
        public object Tag { get; set; }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(DisplayName)
                ? $"{DisplayName} (#{ID})"
                : $"#{ID}";
        }
    }

    /// <summary>
    /// Event args for reference selection changes.
    /// </summary>
    public class ReferenceSelectionChangedEventArgs : EventArgs
    {
        public int? OldId { get; set; }
        public int NewId { get; set; }
        public ReferenceItem NewItem { get; set; }
    }
}
