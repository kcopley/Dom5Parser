using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Dom5Editor.Ava.Views
{
    /// <summary>Small questions and messages (Avalonia has no message box).</summary>
    public static class Dialogs
    {
        /// <summary>Asks a yes/no question; true for <paramref name="yes"/>.</summary>
        public static async Task<bool> Ask(Window owner, string title, string text, string yes, string no)
        {
            bool answer = false;
            var window = Box(title, text, out var buttons);
            var ok = new Button { Content = yes, Classes = { "accent" } };
            var cancel = new Button { Content = no };
            ok.Click += (s, e) => { answer = true; window.Close(); };
            cancel.Click += (s, e) => window.Close();
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            await window.ShowDialog(owner);
            return answer;
        }

        /// <summary>Tells something, with an OK button.</summary>
        public static async Task Tell(Window owner, string title, string text)
        {
            var window = Box(title, text, out var buttons);
            var ok = new Button { Content = "OK" };
            ok.Click += (s, e) => window.Close();
            buttons.Children.Add(ok);
            await window.ShowDialog(owner);
        }

        private static Window Box(string title, string text, out StackPanel buttons)
        {
            buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
            return new Window
            {
                Title = title,
                Width = 460,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(18),
                    Spacing = 16,
                    Children = { new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap }, buttons },
                },
            };
        }
    }
}
