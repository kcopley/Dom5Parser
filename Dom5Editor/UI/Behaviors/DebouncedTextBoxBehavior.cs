using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dom5Editor.UI.Behaviors
{
    /// <summary>
    /// Attached behavior for a TextBox whose binding updates explicitly: the value is committed
    /// when the box loses focus or on Enter, and Escape puts the shown value back. (The name is
    /// from when it committed after a pause in typing.)
    /// </summary>
    public static class DebouncedTextBoxBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DebouncedTextBoxBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        // Commits when the box loses focus or on Enter; Escape puts the value back. (It used to
        // commit after a pause in typing, which saved half-typed numbers and rebuilt the page
        // under the cursor: every commit is an edit.)
        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            if ((bool)e.NewValue)
            {
                textBox.LostFocus += OnLostFocus;
                textBox.KeyDown += OnKeyDown;
            }
            else
            {
                textBox.LostFocus -= OnLostFocus;
                textBox.KeyDown -= OnKeyDown;
            }
        }

        private static void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CommitValue(textBox);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                e.Handled = true;
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            CommitValue(textBox);
        }

        private static void CommitValue(TextBox textBox)
        {
            // Only update if value actually changed from source
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);
            if (binding == null)
                return;

            // Check if the text differs from the bound source value
            var sourceValue = binding.DataItem?.GetType()
                .GetProperty(binding.ResolvedSourcePropertyName)?
                .GetValue(binding.DataItem)?.ToString() ?? "";

            if (textBox.Text != sourceValue)
            {
                binding.UpdateSource();
            }
        }
    }
}
