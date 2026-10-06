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
        // the value box the cursor is in (badge command and which of its badges), so it can be put
        // back there after an edit rebuilds the page (Tab from one value to the next keeps working)
        private (Command Command, int Index)? _focus;
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
            if (e.NewFocus is TextBox box && FindParent<CompactBadge>(box) is { DataContext: PropertyItem item } badge)
                _focus = (item.Command, IndexOf(badge, item));
            else if (e.NewFocus is DependencyObject d && IsDescendant(d))
                _focus = null;
        }

        /// <summary>After the page rebuilt (PropertyChanged with no name), focus the same value's new box.</summary>
        private void OnPageChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName) || _focus is not { } focus)
                return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var badges = FindChildren<CompactBadge>(this).Where(b => b.DataContext is PropertyItem p && p.Command == focus.Command).ToList();
                if (focus.Index < badges.Count && FindChildren<TextBox>(badges[focus.Index]).FirstOrDefault(t => t.IsVisible) is TextBox box)
                {
                    box.Focus();
                    box.SelectAll();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private int IndexOf(CompactBadge badge, PropertyItem item) =>
            FindChildren<CompactBadge>(this).Where(b => b.DataContext is PropertyItem p && p.Command == item.Command).ToList().IndexOf(badge);

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
    }
}
