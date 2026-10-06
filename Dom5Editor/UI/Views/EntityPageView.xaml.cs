using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dom5Edit.Commands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    public partial class EntityPageView : UserControl
    {
        // the value box the cursor is in (a badge's command and which of its badges, a panel field's
        // label, a panel row), so it can be put back there after an edit rebuilds the page (Tab from
        // one value to the next keeps working)
        private string? _focus;
        private INotifyPropertyChanged? _page;

        public EntityPageView()
        {
            InitializeComponent();
            // the preview event: it comes before the box being left commits (and the page rebuilds)
            AddHandler(Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotFocus), true);
            DataContextChanged += (s, e) =>
            {
                if (_page != null)
                    _page.PropertyChanged -= OnPageChanged;
                _page = DataContext as INotifyPropertyChanged;
                if (_page != null)
                    _page.PropertyChanged += OnPageChanged;
                _focus = null;
            };
        }

        /// <summary>Scrolls a part of the page (a badge section, a panel) into view, making it if it isn't yet.</summary>
        public void ShowPart(object part)
        {
            int index = PageParts.Items.IndexOf(part);
            if (index < 0)
                return;
            var panel = FindChildren<VirtualizingStackPanel>(PageParts).FirstOrDefault();
            if (panel != null)
            {
                // (BringIndexIntoView is protected: the public one is on VirtualizingStackPanel)
                panel.BringIndexIntoViewPublic(index);
                UpdateLayout();
            }
        }

        /// <summary>Enter in a text box saves it (as leaving it does).</summary>
        private void CommitOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox box)
            {
                box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                e.Handled = true;
            }
        }

        private void OnGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is TextBox box && KeyOf(box) is string key)
                _focus = key;
            else if (e.NewFocus is DependencyObject d && IsDescendant(d))
                _focus = null;
        }

        /// <summary>Which value a text box edits, in terms that survive a rebuild; null for other boxes.</summary>
        private string? KeyOf(TextBox box)
        {
            if (FindParent<CompactBadge>(box) is { DataContext: PropertyItem item } badge)
            {
                var same = FindChildren<CompactBadge>(this).Where(b => b.DataContext is PropertyItem p && p.Command == item.Command).ToList();
                return $"badge:{item.Command}:{same.IndexOf(badge)}";
            }
            return box.DataContext switch
            {
                ViewModels.PanelField f => $"field:{f.Label}",
                ViewModels.PanelRow r => $"row:{r.Value.Command}:{r.Text}:{r.RefId}",
                ViewModels.ItemSlotsPanel => $"slots:{box.GetBindingExpression(TextBox.TextProperty)?.ParentBinding.Path.Path}",
                _ => null,
            };
        }

        /// <summary>After the page rebuilt (PropertyChanged with no name), focus the same value's new box.</summary>
        private void OnPageChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName) || _focus is not string focus)
                return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (FindChildren<TextBox>(this).FirstOrDefault(t => t.IsVisible && KeyOf(t) == focus) is TextBox box)
                {
                    box.Focus();
                    box.SelectAll();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private bool IsDescendant(DependencyObject d)
        {
            for (var x = d; x != null; x = VisualTreeHelper.GetParent(x))
                if (ReferenceEquals(x, this))
                    return true;
            return false;
        }

        private static T? FindParent<T>(DependencyObject d) where T : DependencyObject
        {
            for (var x = VisualTreeHelper.GetParent(d); x != null; x = VisualTreeHelper.GetParent(x))
                if (x is T t)
                    return t;
            return null;
        }

        private static IEnumerable<T> FindChildren<T>(DependencyObject root) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T t)
                    yield return t;
                foreach (var d in FindChildren<T>(child))
                    yield return d;
            }
        }
    
        // an image file dropped on one of the header's images sets it (copied into the mod's folder)
        private void OnSpriteDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnSpriteDrop(object sender, DragEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is UI.ViewModels.SpriteSlot slot
                && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                slot.SetFromFile(files[0]);
            e.Handled = true;
        }
    }
}
