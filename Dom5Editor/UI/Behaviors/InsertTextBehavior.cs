using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dom5Editor.UI.Behaviors
{
    /// <summary>
    /// A button that types its Tag into a text box at the cursor (an event message's ##tags##):
    /// set InsertTextBehavior.Target on the button to the box.
    /// </summary>
    public static class InsertTextBehavior
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached(
            "Target", typeof(TextBox), typeof(InsertTextBehavior), new PropertyMetadata(null, OnTargetChanged));

        public static TextBox? GetTarget(DependencyObject d) => (TextBox?)d.GetValue(TargetProperty);
        public static void SetTarget(DependencyObject d, TextBox? value) => d.SetValue(TargetProperty, value);

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ButtonBase button)
                return;
            button.Click -= OnClick;
            if (e.NewValue != null)
                button.Click += OnClick;
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not ButtonBase button || GetTarget(button) is not TextBox box || button.Tag is not string text)
                return;
            int at = box.SelectionStart;
            box.SelectedText = text;
            box.SelectionStart = at + text.Length;
            box.SelectionLength = 0;
            box.Focus();
        }
    }
}
