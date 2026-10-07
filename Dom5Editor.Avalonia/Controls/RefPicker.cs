using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// Picks one entity by typing part of its name or number (as the WPF editor's
    /// SearchableReferenceComboBox): ItemsSource (ReferenceItem), SelectedId (two-way),
    /// Placeholder, IsReadOnly; SelectionChanged when the user picks one. The list opens with the
    /// cursor; a pick is made only by Enter, a click, or Tab after typing (moving through the list
    /// with the arrow keys picks nothing: every pick is an edit). Setting SelectedId from outside
    /// raises nothing.
    /// </summary>
    public sealed class RefPicker : UserControl
    {
        public static readonly StyledProperty<IEnumerable<ReferenceItem>?> ItemsSourceProperty =
            AvaloniaProperty.Register<RefPicker, IEnumerable<ReferenceItem>?>(nameof(ItemsSource));
        public static readonly StyledProperty<int?> SelectedIdProperty =
            AvaloniaProperty.Register<RefPicker, int?>(nameof(SelectedId), defaultBindingMode: BindingMode.TwoWay);
        public static readonly StyledProperty<string?> PlaceholderProperty =
            AvaloniaProperty.Register<RefPicker, string?>(nameof(Placeholder));
        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<RefPicker, bool>(nameof(IsReadOnly));

        /// <summary>The most entries listed at once (a list of thousands is slow to lay out and to read): type to narrow.</summary>
        private const int MaxShown = 200;

        private readonly Border _frame;
        private readonly TextBox _box;
        private readonly TextBlock _idText;
        private readonly TextBlock _arrow;
        private readonly Grid _grid;
        private readonly DispatcherTimer _filterTimer;
        private Popup? _popup;
        private Border? _popupFrame;
        private ListBox? _list;
        private TextBlock? _footer;
        private bool _showing; // the box's text is being set to the picked entity's name (not typed)

        /// <summary>Raised when the user picks an entity (not when SelectedId is set from outside).</summary>
        public event EventHandler<RefPickedEventArgs>? SelectionChanged;

        public RefPicker()
        {
            Height = 22;
            _box = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                MinHeight = 0,
                MinWidth = 0,
                Padding = new Thickness(5, 0, 2, 0),
                FontSize = 11.5,
                CornerRadius = default,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = Look.Brush("TextPrimaryBrush"),
            };
            Look.FlatBox(_box, Brushes.Transparent, Brushes.Transparent, Brushes.Transparent, Brushes.Transparent, new Thickness(0));
            _idText = new TextBlock
            {
                FontSize = 10,
                Foreground = Look.Brush("TextMutedBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 2, 0),
            };
            _arrow = new TextBlock
            {
                Text = "▾",
                FontSize = 11,
                Foreground = Look.Brush("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(4, 0, 6, 0),
                Background = Brushes.Transparent, // (so the whole strip takes clicks)
                Cursor = new Cursor(StandardCursorType.Hand),
            };
            Grid.SetColumn(_idText, 1);
            Grid.SetColumn(_arrow, 2);
            _grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"), Children = { _box, _idText, _arrow } };
            _frame = new Border
            {
                Background = Look.Brush("InputBackgroundBrush"),
                BorderBrush = Look.Brush("BorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Child = _grid,
            };
            Content = _frame;

            _filterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            _filterTimer.Tick += (s, e) =>
            {
                _filterTimer.Stop();
                Filter();
            };

            _box.GotFocus += (s, e) =>
            {
                _frame.BorderBrush = Look.Brush("AccentHighlightBrush");
                if (IsReadOnly)
                    return;
                Open();
                Dispatcher.UIThread.Post(() => _box.SelectAll(), DispatcherPriority.Input);
            };
            _box.LostFocus += (s, e) =>
            {
                _frame.BorderBrush = Look.Brush("BorderBrush");
                // (a click on the list moves the cursor there: that click picks)
                if (_list?.IsPointerOver != true)
                    Close(restore: true);
            };
            _box.TextChanged += (s, e) =>
            {
                if (_showing || IsReadOnly || !_box.IsFocused)
                    return;
                Open(filter: false);
                _filterTimer.Stop();
                _filterTimer.Start();
            };
            // a click into the box while the list is closed (after Escape, or a click elsewhere) opens it again
            _box.AddHandler(PointerPressedEvent, (s, e) => { if (!IsReadOnly && _box.IsFocused && _popup?.IsOpen != true) Open(); },
                RoutingStrategies.Bubble, handledEventsToo: true);
            _box.AddHandler(KeyDownEvent, OnKey, RoutingStrategies.Tunnel);
            _arrow.PointerPressed += (s, e) =>
            {
                e.Handled = true;
                if (IsReadOnly)
                    return;
                if (_popup?.IsOpen == true)
                    Close(restore: true);
                else if (_box.IsFocused)
                    Open();
                else
                    FocusInput(); // (the list opens with the cursor)
            };
        }

        public IEnumerable<ReferenceItem>? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public int? SelectedId { get => GetValue(SelectedIdProperty); set => SetValue(SelectedIdProperty, value); }
        public string? Placeholder { get => GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
        public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

        /// <summary>Puts the cursor in the box to type a search (the list opens).</summary>
        public void FocusInput() => _box.Focus();

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ItemsSourceProperty || change.Property == SelectedIdProperty)
                ShowSelected();
            else if (change.Property == PlaceholderProperty)
                _box.Watermark = Placeholder;
            else if (change.Property == IsReadOnlyProperty)
            {
                _box.IsReadOnly = IsReadOnly;
                _arrow.IsVisible = !IsReadOnly;
                _frame.Background = IsReadOnly ? Brushes.Transparent : Look.Brush("InputBackgroundBrush");
            }
        }

        private ReferenceItem? Selected => SelectedId is int id ? ItemsSource?.FirstOrDefault(i => i.ID == id) : null;

        /// <summary>Shows the entity SelectedId names (its name and number), or nothing.</summary>
        private void ShowSelected()
        {
            var item = Selected;
            _showing = true;
            _box.Text = item != null ? item.DisplayName ?? "" : SelectedId is int n && n != 0 ? $"#{n}" : "";
            _showing = false;
            _idText.Text = item != null && item.ShowId ? $"#{item.ID}" : "";
            _idText.IsVisible = _idText.Text.Length > 0;
        }

        private void Open(bool filter = true)
        {
            if (_popup == null)
                MakePopup();
            if (_popup!.IsOpen)
                return;
            _popupFrame!.MinWidth = Math.Max(Bounds.Width, 260);
            _idText.IsVisible = false;
            if (filter)
                Filter();
            _popup.IsOpen = true;
        }

        private void Close(bool restore)
        {
            _filterTimer.Stop();
            if (_popup != null)
                _popup.IsOpen = false;
            if (restore)
                ShowSelected();
        }

        /// <summary>The list under the box, made when first opened (a page has dozens of pickers).</summary>
        private void MakePopup()
        {
            _list = new ListBox
            {
                MaxHeight = 320,
                Background = Brushes.Transparent,
                Focusable = false,
                ItemTemplate = new FuncDataTemplate<ReferenceItem>((item, _) => Row(item)),
            };
            // (Fluent's list rows are tall: a picker lists many)
            _list.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
            {
                Setters = { new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 3)), new Setter(MinHeightProperty, 0.0), new Setter(FocusableProperty, false) },
            });
            _list.Tapped += (s, e) =>
            {
                if ((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true) != null && _list.SelectedItem is ReferenceItem item)
                    Pick(item);
            };
            _footer = new TextBlock { FontSize = 10.5, Foreground = Look.Brush("TextMutedBrush"), Margin = new Thickness(8, 3), IsVisible = false };
            DockPanel.SetDock(_footer, Dock.Bottom);
            _popupFrame = new Border
            {
                Background = Look.Brush("BackgroundLightBrush"),
                BorderBrush = Look.Brush("BorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                MaxWidth = 520,
                Child = new DockPanel { Children = { _footer, _list } },
            };
            _popup = new Popup
            {
                PlacementTarget = _frame,
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                IsLightDismissEnabled = true,
                // (a click elsewhere closes the list and still does what it does)
                OverlayDismissEventPassThrough = true,
                Child = _popupFrame,
            };
            _popup.Closed += (s, e) => { if (!_box.IsFocused) ShowSelected(); };
            _grid.Children.Add(_popup);
        }

        /// <summary>One entry of the list: the name, and its number on the right.</summary>
        private static Control Row(ReferenceItem? item)
        {
            var row = new DockPanel();
            if (item == null)
                return row;
            if (item.ShowId)
            {
                var id = new TextBlock { Text = $"#{item.ID}", Foreground = Look.Brush("TextMutedBrush"), FontSize = 10.5, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
                DockPanel.SetDock(id, Dock.Right);
                row.Children.Add(id);
            }
            row.Children.Add(new TextBlock { Text = item.DisplayName ?? "", FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis });
            if (!string.IsNullOrEmpty(item.Tooltip))
                ToolTip.SetTip(row, item.Tooltip);
            return row;
        }

        /// <summary>The entries that match what's typed (all of them while the box shows the picked one's name).</summary>
        private void Filter()
        {
            if (_list == null)
                return;
            var items = ItemsSource?.ToList() ?? new List<ReferenceItem>();
            var search = (_box.Text ?? "").Trim();
            if (Selected is ReferenceItem picked && search == (picked.DisplayName ?? "").Trim())
                search = "";
            var number = search.TrimStart('#');
            var found = search.Length == 0 ? items : items.Where(i =>
                (i.DisplayName ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                || i.ShowId && number.Length > 0 && i.ID.ToString().StartsWith(number, StringComparison.Ordinal)).ToList();
            var shown = found.Take(MaxShown).ToList();
            _list.ItemsSource = shown;
            _footer!.Text = found.Count == 0 ? "No match" : found.Count > MaxShown ? $"First {MaxShown} of {found.Count}: type to narrow" : "";
            _footer.IsVisible = _footer.Text.Length > 0;
            // the picked one highlighted when the whole list shows, else the first match
            var current = search.Length == 0 ? shown.FirstOrDefault(i => i.ID == SelectedId) : null;
            _list.SelectedItem = current ?? shown.FirstOrDefault();
            if (_list.SelectedItem != null)
                Dispatcher.UIThread.Post(() => { if (_list.SelectedItem != null) _list.ScrollIntoView(_list.SelectedItem); }, DispatcherPriority.Loaded);
        }

        private void OnKey(object? sender, KeyEventArgs e)
        {
            bool open = _popup?.IsOpen == true;
            switch (e.Key)
            {
                case Key.Down:
                case Key.Up:
                    if (!open)
                        Open();
                    else
                    {
                        // (the list is filtered after a pause in typing: catch up first)
                        if (_filterTimer.IsEnabled)
                            Filter();
                        if (_list!.ItemCount > 0)
                        {
                            _list.SelectedIndex = Math.Clamp(_list.SelectedIndex + (e.Key == Key.Down ? 1 : -1), 0, _list.ItemCount - 1);
                            _list.ScrollIntoView(_list.SelectedIndex);
                        }
                    }
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (open)
                    {
                        // (the list is filtered after a pause in typing: catch up first)
                        if (_filterTimer.IsEnabled)
                            Filter();
                        if (_list!.SelectedItem is ReferenceItem item)
                            Pick(item);
                    }
                    e.Handled = true;
                    break;
                case Key.Escape:
                    Close(restore: true);
                    TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
                    e.Handled = true;
                    break;
                case Key.Tab:
                    // Tab after typing picks the match (as the WPF picker); just passing through picks nothing
                    if (open && Typed)
                    {
                        if (_filterTimer.IsEnabled)
                            Filter();
                        if (_list!.SelectedItem is ReferenceItem match)
                            Pick(match);
                    }
                    Close(restore: true);
                    break;
            }
        }

        /// <summary>Whether the box has something typed (not the picked one's name).</summary>
        private bool Typed => (_box.Text ?? "") != (Selected?.DisplayName ?? (SelectedId is int n && n != 0 ? $"#{n}" : ""));

        private void Pick(ReferenceItem item)
        {
            int? old = SelectedId;
            Close(restore: false);
            SelectedId = item.ID; // (a two-way binding makes the edit now)
            ShowSelected();
            if (old != item.ID)
                SelectionChanged?.Invoke(this, new RefPickedEventArgs { OldId = old, NewId = item.ID, NewItem = item });
        }
    }

    /// <summary>The entity picked in a RefPicker, and the one before.</summary>
    public sealed class RefPickedEventArgs : EventArgs
    {
        public int? OldId { get; init; }
        public int NewId { get; init; }
        public ReferenceItem? NewItem { get; init; }
    }
}
