using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dom5Editor.UI;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// Links to entities, as a browser treats links (the user): a middle click opens the entity in
    /// a window of its own, and the right click menu offers "Open" and "Open in a new window". A
    /// link is a button whose command is an <see cref="ILinkCommand"/>, or a control given an
    /// open action (<see cref="OpenProperty"/>: a badge's reference button). Only links: a middle
    /// click on anything else (a ✕) does nothing.
    /// </summary>
    public static class Link
    {
        /// <summary>What opening a link control does, for one not opened through an ILinkCommand.</summary>
        public static readonly AttachedProperty<Action?> OpenProperty =
            AvaloniaProperty.RegisterAttached<Control, Action?>("Open", typeof(Link));

        public static void SetOpen(Control control, Action? open) => control.SetValue(OpenProperty, open);
        public static Action? GetOpen(Control control) => control.GetValue(OpenProperty);

        /// <summary>The link under the pointer (the control or one of its parents), as what opening it does; null: not a link.</summary>
        public static Action? Find(object? source, Func<Control, Action?>? other = null)
        {
            for (var v = source as Visual; v != null; v = v.GetVisualParent())
            {
                if (v is not Control c)
                    continue;
                if (GetOpen(c) is Action open)
                    return open;
                if (c is Button { Command: ILinkCommand command } button)
                {
                    var parameter = button.CommandParameter;
                    return command.CanExecute(parameter) ? () => command.Execute(parameter) : null;
                }
                if (other?.Invoke(c) is Action found)
                    return found;
                if (c is Button || c is TopLevel)
                    return null; // (a button that isn't a link, inside a link's area, is itself)
            }
            return null;
        }

        /// <summary>Opens a link in a window of its own (as with Ctrl held).</summary>
        public static void InNewWindow(Action open)
        {
            Hooks.ForceNewWindow = true;
            try
            {
                open();
            }
            finally
            {
                Hooks.ForceNewWindow = false;
            }
        }

        /// <summary>The right click menu of a link.</summary>
        public static ContextMenu Menu(Action open)
        {
            var here = new MenuItem { Header = "Open" };
            ToolTip.SetTip(here, "Open it here (a click does the same)");
            here.Click += (s, e) => open();
            var apart = new MenuItem { Header = "Open in a new window" };
            ToolTip.SetTip(apart, "Open it in a window of its own, to see or edit it beside this one (a middle click or Ctrl+click does the same)");
            apart.Click += (s, e) => InNewWindow(open);
            return new ContextMenu { ItemsSource = new[] { here, apart } };
        }

        /// <summary>
        /// Gives the links inside a view (a page, the report) the middle click and the right click
        /// menu. <paramref name="other"/> finds links the view knows another way (the report's Go to).
        /// </summary>
        public static void Install(Control view, Func<Control, Action?>? other = null)
        {
            view.AddHandler(InputElement.PointerReleasedEvent, (s, e) =>
            {
                if (e.InitialPressMouseButton == MouseButton.Middle && Find(e.Source, other) is Action open)
                {
                    InNewWindow(open);
                    e.Handled = true;
                }
            }, RoutingStrategies.Tunnel, handledEventsToo: true);
            view.AddHandler(Control.ContextRequestedEvent, (s, e) =>
            {
                // (a control with its own menu keeps it: a sprite slot's)
                if (e.Handled || e.Source is Control { ContextMenu: not null } || Find(e.Source, other) is not Action open)
                    return;
                Menu(open).Open(e.Source as Control ?? view);
                e.Handled = true;
            }, RoutingStrategies.Bubble);
        }
    }
}
