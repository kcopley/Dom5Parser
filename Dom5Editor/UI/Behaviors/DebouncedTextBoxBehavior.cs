using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Dom5Editor.UI.Behaviors
{
    /// <summary>
    /// Attached behavior that adds debounced binding updates to TextBox controls.
    /// When enabled, the TextBox will wait for a pause in typing before updating the binding source.
    /// </summary>
    public static class DebouncedTextBoxBehavior
    {
        private const int DefaultDelayMs = 400;

        /// <summary>
        /// Enables debounced updates for a TextBox.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DebouncedTextBoxBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        /// The delay in milliseconds before updating the binding source.
        /// </summary>
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.RegisterAttached(
                "Delay",
                typeof(int),
                typeof(DebouncedTextBoxBehavior),
                new PropertyMetadata(DefaultDelayMs));

        // Timer storage per TextBox
        private static readonly DependencyProperty TimerProperty =
            DependencyProperty.RegisterAttached(
                "Timer",
                typeof(DispatcherTimer),
                typeof(DebouncedTextBoxBehavior),
                new PropertyMetadata(null));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        public static int GetDelay(DependencyObject obj) => (int)obj.GetValue(DelayProperty);
        public static void SetDelay(DependencyObject obj, int value) => obj.SetValue(DelayProperty, value);

        private static DispatcherTimer GetTimer(DependencyObject obj) => (DispatcherTimer)obj.GetValue(TimerProperty);
        private static void SetTimer(DependencyObject obj, DispatcherTimer value) => obj.SetValue(TimerProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            if ((bool)e.NewValue)
            {
                // Create timer for this TextBox
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(GetDelay(textBox));
                timer.Tick += (s, args) => OnTimerTick(textBox, timer);
                SetTimer(textBox, timer);

                // Subscribe to events
                textBox.TextChanged += OnTextChanged;
                textBox.LostFocus += OnLostFocus;
            }
            else
            {
                // Clean up
                var timer = GetTimer(textBox);
                if (timer != null)
                {
                    timer.Stop();
                    SetTimer(textBox, null);
                }

                textBox.TextChanged -= OnTextChanged;
                textBox.LostFocus -= OnLostFocus;
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            var timer = GetTimer(textBox);
            if (timer == null)
                return;

            // Reset the timer
            timer.Stop();
            timer.Start();
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            // Commit immediately on focus loss
            CommitValue(textBox);
        }

        private static void OnTimerTick(TextBox textBox, DispatcherTimer timer)
        {
            timer.Stop();
            CommitValue(textBox);
        }

        private static void CommitValue(TextBox textBox)
        {
            var timer = GetTimer(textBox);
            timer?.Stop();

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
