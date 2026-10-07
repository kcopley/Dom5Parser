using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dom5Editor.Ava.Controls;
using Dom5Editor.UI.Controls;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Ava.Views
{
    public partial class EntityPageView : UserControl
    {
        // the value box the cursor is in, or goes to (a badge's command and which of its badges, a
        // panel field's label, a panel row), so it can be put back there after an edit rebuilds the
        // page (Tab from one value to the next keeps working)
        private string? _focus;
        private INotifyPropertyChanged? _page;

        public EntityPageView()
        {
            InitializeComponent();
            PageParts.ItemTemplate = new PartSelector(this);
            AddHandler(GotFocusEvent, OnGotFocus, RoutingStrategies.Bubble, handledEventsToo: true);
            // a value box is about to save (and the page to rebuild): where the cursor is now going
            AddHandler(Commit.LeavingEvent, (s, e) => _focus = KeyOf(TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as TextBox));
            // an image file dropped on one of the header's images sets it (copied into the mod's folder)
            AddHandler(DragDrop.DragOverEvent, OnSpriteDragOver);
            AddHandler(DragDrop.DropEvent, OnSpriteDrop);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (_page != null)
                _page.PropertyChanged -= OnPageChanged;
            _page = DataContext as INotifyPropertyChanged;
            if (_page != null)
                _page.PropertyChanged += OnPageChanged;
            _focus = null;
        }

        private void OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            if (e.Source is TextBox box && KeyOf(box) is string key)
                _focus = key;
            else if (e.Source is Control)
                _focus = null;
        }

        /// <summary>Which value a text box edits, in terms that survive a rebuild; null for other boxes.</summary>
        private string? KeyOf(TextBox? box)
        {
            if (box == null || !this.IsVisualAncestorOf(box) || box.FindAncestorOfType<RefPicker>() != null)
                return null;
            if (box.FindAncestorOfType<CompactBadge>() is { DataContext: PropertyItem item } badge)
            {
                var same = this.GetVisualDescendants().OfType<CompactBadge>().Where(b => b.DataContext is PropertyItem p && p.Command == item.Command).ToList();
                return $"badge:{item.Command}:{same.IndexOf(badge)}";
            }
            return box.DataContext switch
            {
                PanelField f => $"field:{f.Label}",
                PanelRow r => $"row:{r.Value.Command}:{r.Text}:{r.RefId}",
                RandomPathRow p => $"random:{p.Mask}",
                LeaderField l => $"leader:{l.Label}",
                UnitRowField u => $"unit:{u.Label}",
                ItemSlotsPanel => $"slots:{box.Tag}",
                LongText t => $"text:{t.Command}",
                EntityPageViewModel when box.Tag is string tag => $"page:{tag}",
                _ => null,
            };
        }

        /// <summary>After the page rebuilt (PropertyChanged with no name), the cursor goes into the same value's new box.</summary>
        private void OnPageChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName) || _focus is not string focus)
                return;
            Dispatcher.UIThread.Post(() =>
            {
                if (this.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(t => t.IsEffectivelyVisible && KeyOf(t) == focus) is TextBox box)
                {
                    box.Focus();
                    box.SelectAll();
                }
            }, DispatcherPriority.Loaded);
        }

        private static SpriteSlot? SlotOf(object? source) =>
            (source as Control)?.FindAncestorOfType<Button>(includeSelf: true)?.DataContext as SpriteSlot;

        private void OnSpriteDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = SlotOf(e.Source) != null && e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnSpriteDrop(object? sender, DragEventArgs e)
        {
            if (SlotOf(e.Source) is SpriteSlot slot && e.DataTransfer.TryGetFiles()?.FirstOrDefault()?.TryGetLocalPath() is string file)
                slot.SetFromFile(file);
            e.Handled = true;
        }

        /// <summary>
        /// The template for each part of a page: the page's own parts by kind (the resources
        /// "Part.Header", ...), panels by their type ("Panel.StatsPanel", in Panels.axaml or
        /// EventTemplates.axaml), badge sections by the view's data templates; a panel without a
        /// template yet shows its name.
        /// </summary>
        private sealed class PartSelector : IDataTemplate
        {
            private readonly EntityPageView _view;
            public PartSelector(EntityPageView view) => _view = view;

            public bool Match(object? data) => data != null;

            public Control? Build(object? data)
            {
                if (data is PagePart part)
                {
                    if (_view.TryFindResource("Part." + part.Kind, out var found) && found is IDataTemplate template)
                    {
                        var control = template.Build(part.Page);
                        if (control != null)
                            control.DataContext = part.Page;
                        return control;
                    }
                    return null; // a part this view doesn't show yet
                }
                // a panel: its template by type name, else its base type's
                for (var t = data?.GetType(); t != null && t != typeof(object); t = t.BaseType)
                    if (_view.TryFindResource("Panel." + t.Name, out var byType) && byType is IDataTemplate panel)
                        return panel.Build(data);
                foreach (var template in _view.DataTemplates)
                    if (template.Match(data))
                        return template.Build(data);
                // a panel not ported yet: its name, so the page shows what's missing
                return new TextBlock
                {
                    Text = data?.GetType().Name ?? "",
                    Foreground = Avalonia.Media.Brushes.Gray,
                    FontStyle = FontStyle.Italic,
                    Margin = new Avalonia.Thickness(0, 0, 0, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
            }
        }
    }
}
