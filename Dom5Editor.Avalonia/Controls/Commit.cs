using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// A value box that saves when it's left or on Enter, and puts the shown value back on Escape
    /// (the WPF editor's DebouncedTextBoxBehavior): controls:Commit.OnLeave="True" on a TextBox
    /// whose Text binding has UpdateSourceTrigger=Explicit. Every save is an edit that rebuilds
    /// the page, so typing itself saves nothing. <see cref="LeavingEvent"/> is raised just before
    /// a save, while the box the cursor goes to is still on the page, so the page can put the
    /// cursor back into that value's new box after the rebuild.
    /// </summary>
    public static class Commit
    {
        public static readonly AttachedProperty<bool> OnLeaveProperty =
            AvaloniaProperty.RegisterAttached<TextBox, bool>("OnLeave", typeof(Commit));

        // the text when the box got the cursor: a save only when it changed
        private static readonly AttachedProperty<string?> ShownProperty =
            AvaloniaProperty.RegisterAttached<TextBox, string?>("Shown", typeof(Commit));

        /// <summary>Raised on the box (bubbling) just before its value is saved.</summary>
        public static readonly RoutedEvent<RoutedEventArgs> LeavingEvent =
            RoutedEvent.Register<RoutedEventArgs>("Leaving", RoutingStrategies.Bubble, typeof(Commit));

        public static bool GetOnLeave(TextBox box) => box.GetValue(OnLeaveProperty);
        public static void SetOnLeave(TextBox box, bool value) => box.SetValue(OnLeaveProperty, value);

        static Commit()
        {
            OnLeaveProperty.Changed.AddClassHandler<TextBox>((box, e) =>
            {
                if (e.NewValue is true)
                {
                    box.GotFocus += OnGotFocus;
                    box.LostFocus += OnLostFocus;
                    // (tunnelling: before the box handles the key itself)
                    box.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
                }
                else
                {
                    box.GotFocus -= OnGotFocus;
                    box.LostFocus -= OnLostFocus;
                    box.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
                }
            });
        }

        private static void OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            if (sender is TextBox box)
                box.SetValue(ShownProperty, box.Text);
        }

        private static void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox box)
                Save(box);
        }

        private static void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox box)
                return;
            if (e.Key == Key.Enter && !box.AcceptsReturn)
            {
                Save(box);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // the text it had when it got the cursor (the binding doesn't rewrite a value that didn't change)
                box.SetCurrentValue(TextBox.TextProperty, box.GetValue(ShownProperty));
                box.SelectAll();
                e.Handled = true;
            }
        }

        /// <summary>Saves the box's text to its binding, if it changed since the box got the cursor.</summary>
        public static void Save(TextBox box)
        {
            if (box.IsReadOnly || box.Text == box.GetValue(ShownProperty))
                return;
            box.SetValue(ShownProperty, box.Text);
            box.RaiseEvent(new RoutedEventArgs(LeavingEvent, box));
            BindingOperations.GetBindingExpressionBase(box, TextBox.TextProperty)?.UpdateSource();
        }
    }
}
