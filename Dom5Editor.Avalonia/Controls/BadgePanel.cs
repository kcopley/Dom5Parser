using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// A badge section's badges (CompactBadge, one per PropertyItem) and its add box: the WPF
    /// editor's BadgeWrapPanel (IsGrid false: badges wrap, the add box after them) and
    /// BadgeGridPanel (IsGrid: Columns columns, the add box under them) in one. The commands are
    /// the section's: add (an AvailablePropertyItem), remove (a PropertyItem), navigate
    /// ((type, id)), a reference picked ((item, old, new, name)).
    /// </summary>
    public sealed class BadgePanel : UserControl
    {
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<BadgePanel, IEnumerable?>(nameof(ItemsSource));
        public static readonly StyledProperty<IEnumerable?> AvailableItemsProperty = AvaloniaProperty.Register<BadgePanel, IEnumerable?>(nameof(AvailableItems));
        public static readonly StyledProperty<bool> ShowAddButtonProperty = AvaloniaProperty.Register<BadgePanel, bool>(nameof(ShowAddButton), true);
        public static readonly StyledProperty<bool> IsGridProperty = AvaloniaProperty.Register<BadgePanel, bool>(nameof(IsGrid));
        public static readonly StyledProperty<int> ColumnsProperty = AvaloniaProperty.Register<BadgePanel, int>(nameof(Columns), 3);
        public static readonly StyledProperty<ICommand?> AddCommandProperty = AvaloniaProperty.Register<BadgePanel, ICommand?>(nameof(AddCommand));
        public static readonly StyledProperty<ICommand?> RemoveCommandProperty = AvaloniaProperty.Register<BadgePanel, ICommand?>(nameof(RemoveCommand));
        public static readonly StyledProperty<ICommand?> NavigateCommandProperty = AvaloniaProperty.Register<BadgePanel, ICommand?>(nameof(NavigateCommand));
        public static readonly StyledProperty<ICommand?> ReferenceChangedCommandProperty = AvaloniaProperty.Register<BadgePanel, ICommand?>(nameof(ReferenceChangedCommand));

        private readonly ItemsControl _badges;
        private readonly RefPicker _add;

        public BadgePanel()
        {
            _badges = new ItemsControl { ItemTemplate = new FuncDataTemplate<PropertyItem>((item, _) => new CompactBadge()) };
            _add = new RefPicker
            {
                Width = 140,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Placeholder = "+ Add property...",
            };
            ToolTip.SetTip(_add, "Add an ability (type a name or #command)");
            _add.SelectionChanged += (s, e) =>
            {
                if (e.NewItem?.Tag is AvailablePropertyItem item && AddCommand?.CanExecute(item) == true)
                    AddCommand.Execute(item);
                // the box is for adding: empty again (when the page didn't rebuild)
                Dispatcher.UIThread.Post(() => _add.SelectedId = null, DispatcherPriority.Input);
            };
            Arrange();
        }

        public IEnumerable? ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public IEnumerable? AvailableItems { get => GetValue(AvailableItemsProperty); set => SetValue(AvailableItemsProperty, value); }
        public bool ShowAddButton { get => GetValue(ShowAddButtonProperty); set => SetValue(ShowAddButtonProperty, value); }
        public bool IsGrid { get => GetValue(IsGridProperty); set => SetValue(IsGridProperty, value); }
        public int Columns { get => GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
        public ICommand? AddCommand { get => GetValue(AddCommandProperty); set => SetValue(AddCommandProperty, value); }
        public ICommand? RemoveCommand { get => GetValue(RemoveCommandProperty); set => SetValue(RemoveCommandProperty, value); }
        public ICommand? NavigateCommand { get => GetValue(NavigateCommandProperty); set => SetValue(NavigateCommandProperty, value); }
        public ICommand? ReferenceChangedCommand { get => GetValue(ReferenceChangedCommandProperty); set => SetValue(ReferenceChangedCommandProperty, value); }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ItemsSourceProperty)
                _badges.ItemsSource = ItemsSource;
            else if (change.Property == AvailableItemsProperty)
                // the add box lists commands by name (their number means nothing to a modder)
                _add.ItemsSource = AvailableItems?.OfType<AvailablePropertyItem>().Select(a => a.ToReferenceItem()).ToList();
            else if (change.Property == ShowAddButtonProperty)
                _add.IsVisible = ShowAddButton;
            else if (change.Property == IsGridProperty || change.Property == ColumnsProperty)
                Arrange();
        }

        /// <summary>Lays the badges out as a grid or wrapping, with the add box under or after them.</summary>
        private void Arrange()
        {
            if (_badges.Parent is Panel oldBadges)
                oldBadges.Children.Remove(_badges);
            if (_add.Parent is Panel oldAdd)
                oldAdd.Children.Remove(_add);
            if (IsGrid)
            {
                _badges.ItemsPanel = new FuncTemplate<Panel?>(() => new UniformGrid { Columns = Math.Max(1, Columns) });
                _add.Margin = new Thickness(0, 4, 0, 0);
                Content = new StackPanel { Children = { _badges, _add } };
            }
            else
            {
                _badges.ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal });
                _add.Margin = new Thickness(0, 0, 0, 3);
                Content = new WrapPanel { Orientation = Orientation.Horizontal, Children = { _badges, _add } };
            }
        }
    }
}
