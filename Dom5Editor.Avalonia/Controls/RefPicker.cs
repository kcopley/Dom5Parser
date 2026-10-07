using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// Picks one entity by typing part of its name or number (as the WPF editor's
    /// SearchableReferenceComboBox): ItemsSource (ReferenceItem), SelectedId (two-way),
    /// Placeholder, IsReadOnly; SelectionChanged when the user picks one.
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

        private readonly AutoCompleteBox _box;
        private bool _setting;

        /// <summary>Raised when the user picks an entity (not when SelectedId is set from outside).</summary>
        public event EventHandler<RefPickedEventArgs>? SelectionChanged;

        public RefPicker()
        {
            _box = new AutoCompleteBox
            {
                FilterMode = AutoCompleteFilterMode.Custom,
                ItemFilter = (search, item) => Matches(search, item as ReferenceItem),
                MinimumPrefixLength = 0,
                MaxDropDownHeight = 320,
                ItemTemplate = new FuncDataTemplate<ReferenceItem>((item, _) => new TextBlock
                {
                    Text = item == null ? "" : item.ShowId ? $"{item.DisplayName}  #{item.ID}" : item.DisplayName,
                    [ToolTip.TipProperty] = item?.Tooltip,
                }),
                ItemSelector = (text, item) => (item as ReferenceItem)?.DisplayName ?? text,
            };
            _box.SelectionChanged += (s, e) =>
            {
                if (_setting || _box.SelectedItem is not ReferenceItem item)
                    return;
                int? old = SelectedId;
                _setting = true;
                SelectedId = item.ID;
                _setting = false;
                SelectionChanged?.Invoke(this, new RefPickedEventArgs { OldId = old, NewId = item.ID, NewItem = item });
            };
            Content = _box;
        }

        public IEnumerable<ReferenceItem>? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public int? SelectedId { get => GetValue(SelectedIdProperty); set => SetValue(SelectedIdProperty, value); }
        public string? Placeholder { get => GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
        public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

        /// <summary>The name has the text, or the number starts with it.</summary>
        private static bool Matches(string? search, ReferenceItem? item)
        {
            if (item == null)
                return false;
            if (string.IsNullOrWhiteSpace(search))
                return true;
            search = search.Trim().TrimStart('#');
            return (item.DisplayName ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                   || item.ID.ToString().StartsWith(search, StringComparison.Ordinal);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ItemsSourceProperty)
            {
                _box.ItemsSource = ItemsSource;
                ShowSelected();
            }
            else if (change.Property == SelectedIdProperty && !_setting)
                ShowSelected();
            else if (change.Property == PlaceholderProperty)
                _box.Watermark = Placeholder;
            else if (change.Property == IsReadOnlyProperty)
                _box.IsEnabled = !IsReadOnly;
        }

        /// <summary>Shows the entity SelectedId names (its name), or nothing.</summary>
        private void ShowSelected()
        {
            _setting = true;
            var item = SelectedId is int id ? ItemsSource?.FirstOrDefault(i => i.ID == id) : null;
            _box.SelectedItem = item;
            if (item == null)
                _box.Text = SelectedId is int n ? $"#{n}" : "";
            _setting = false;
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
