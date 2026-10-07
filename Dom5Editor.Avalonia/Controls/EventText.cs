using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// Text boxes of the event page (as the WPF editor's DebouncedTextBoxBehavior and
    /// InsertTextBehavior):
    ///   EventText.Value="{Binding X}" on a TextBox (instead of binding its Text): the value is shown
    ///     in the box and committed on Enter or when the box loses focus; Escape puts it back (every
    ///     commit is an edit that rebuilds the page, so not on each key);
    ///   EventText.InsertInto="BoxName" on a panel of buttons: a button's Tag is typed into the text
    ///     box of that name (the nearest one around the panel) at its cursor (a message's ##tags##).
    /// </summary>
    public static class EventText
    {
        public static readonly AttachedProperty<string?> ValueProperty =
            AvaloniaProperty.RegisterAttached<TextBox, string?>("Value", typeof(EventText), defaultBindingMode: BindingMode.TwoWay);

        public static readonly AttachedProperty<string?> InsertIntoProperty =
            AvaloniaProperty.RegisterAttached<Control, string?>("InsertInto", typeof(EventText));

        public static string? GetValue(TextBox box) => box.GetValue(ValueProperty);
        public static void SetValue(TextBox box, string? value) => box.SetValue(ValueProperty, value);
        public static string? GetInsertInto(Control c) => c.GetValue(InsertIntoProperty);
        public static void SetInsertInto(Control c, string? value) => c.SetValue(InsertIntoProperty, value);

        static EventText()
        {
            // (the box's Text isn't bound: an Explicit binding's UpdateTarget wouldn't undo typing,
            // which sets the box's current value over the binding's)
            ValueProperty.Changed.AddClassHandler<TextBox>((box, e) =>
            {
                box.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
                box.LostFocus -= OnLostFocus;
                // (tunnel: before the box itself acts on the key)
                box.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
                box.LostFocus += OnLostFocus;
                box.Text = e.NewValue as string;
            });
            InsertIntoProperty.Changed.AddClassHandler<Control>((panel, e) =>
            {
                panel.RemoveHandler(InputElement.PointerPressedEvent, OnTagPressed);
                if (e.NewValue is string)
                    panel.AddHandler(InputElement.PointerPressedEvent, OnTagPressed, RoutingStrategies.Tunnel);
            });
        }

        private static void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox box)
                return;
            if (e.Key == Key.Enter)
            {
                Commit(box);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                box.Text = GetValue(box);
                e.Handled = true;
            }
        }

        private static void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox box)
                Commit(box);
        }

        /// <summary>The box's text to the value (and through its binding to the page), when it changed.</summary>
        private static void Commit(TextBox box)
        {
            var text = box.Text ?? "";
            if (text != (GetValue(box) ?? ""))
                box.SetValue(ValueProperty, text);
        }

        /// <summary>
        /// A tag button pressed: its text goes into the box at the cursor. Done on the press, seen
        /// on the way down (tunnel) and marked handled, so the box keeps the focus: losing it would
        /// commit the message, rebuild the page and leave this box (and the insert) behind.
        /// </summary>
        private static void OnTagPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Control panel || GetInsertInto(panel) is not string name || e.Source is not Visual source)
                return;
            var button = source as Button ?? source.FindAncestorOfType<Button>();
            if (button?.Tag is not string text || !e.GetCurrentPoint(panel).Properties.IsLeftButtonPressed)
                return;
            var box = FindBox(panel, name);
            if (box == null)
                return;
            var current = box.Text ?? "";
            int start = Math.Clamp(Math.Min(box.SelectionStart, box.SelectionEnd), 0, current.Length);
            int end = Math.Clamp(Math.Max(box.SelectionStart, box.SelectionEnd), 0, current.Length);
            box.Text = current[..start] + text + current[end..];
            box.SelectionStart = box.SelectionEnd = box.CaretIndex = start + text.Length;
            box.Focus();
            e.Handled = true;
        }

        /// <summary>The text box of that name nearest around the panel.</summary>
        private static TextBox? FindBox(Control panel, string name)
        {
            for (Visual? v = panel.GetVisualParent(); v != null; v = v.GetVisualParent())
                if (v.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(t => t.Name == name) is TextBox box)
                    return box;
            return null;
        }
    }
}
