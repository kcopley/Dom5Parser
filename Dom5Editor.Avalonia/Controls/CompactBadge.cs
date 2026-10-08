using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// One value of an entity page as a chip (its DataContext: a badge section's PropertyItem), as
    /// the WPF editor's CompactBadge: markers (from this mod, set in this session), an icon, the
    /// label, then what the value needs: a box to type in, a reference's name (click it to pick
    /// another), the open button, the n/r and inh tags, the remove button. A page can have a
    /// hundred of these, so only the parts a badge needs are made. The commands are its
    /// BadgePanel's.
    /// </summary>
    public sealed class CompactBadge : Border
    {
        private PropertyItem? _item;
        private ContentControl? _referenceHost; // the reference's name, then its picker

        public CompactBadge()
        {
            BorderThickness = new Thickness(1);
            CornerRadius = new CornerRadius(3);
            Padding = new Thickness(5, 2);
            Margin = new Thickness(0, 0, 4, 4);
        }

        private BadgePanel? Panel => this.FindAncestorOfType<BadgePanel>();

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (!ReferenceEquals(DataContext, _item))
                Build(DataContext as PropertyItem);
        }

        private void Build(PropertyItem? item)
        {
            _item = item;
            _referenceHost = null;
            if (item == null)
            {
                Child = null;
                return;
            }
            Background = Look.Hex(item.Background);
            BorderBrush = Look.Hex(item.BorderBrush);
            ToolTip.SetTip(this, item.Tooltip);
            var fg = Look.Hex(item.Foreground);
            var row = new StackPanel { Orientation = Orientation.Horizontal };

            // markers: from this mod (gold bar), set in this session (cyan dot)
            if (item.IsModified)
                row.Children.Add(new Rectangle { Width = 2, Fill = Look.Brush("AccentHighlightBrush"), Margin = new Thickness(0, 0, 3, 0), RadiusX = 1, RadiusY = 1 });
            if (item.IsSessionEdit)
                row.Children.Add(new Ellipse { Width = 4, Height = 4, Fill = Look.Brush("SessionEditBrush"), Margin = new Thickness(0, 0, 3, 0), VerticalAlignment = VerticalAlignment.Center });
            if (item.HasIcon && Hooks.ToBitmap(item.IconSource) is { } icon)
            {
                var image = new Image { Source = icon, Width = 12, Height = 12, Margin = new Thickness(0, 0, 3, 0), VerticalAlignment = VerticalAlignment.Center };
                RenderOptions.SetBitmapInterpolationMode(image, Avalonia.Media.Imaging.BitmapInterpolationMode.None);
                row.Children.Add(image);
            }
            if (!string.IsNullOrEmpty(item.IconKind))
                row.Children.Add(new GameIcon { Kind = item.IconKind, Width = 12, Height = 12, Margin = new Thickness(0, 0, 3, 0), VerticalAlignment = VerticalAlignment.Center });
            row.Children.Add(Text(item.DisplayName, 9, fg));

            if (item.HasValue)
                AddValue(row, item, fg);
            if (item.IsReference)
            {
                row.Children.Add(Text(": ", 9, fg));
                _referenceHost = new ContentControl { VerticalAlignment = VerticalAlignment.Center };
                row.Children.Add(_referenceHost);
                // just added from an add box (no value yet): pick it now (it can't be removed yet, so it isn't "editable")
                if (item.Tag == null && item.ReferenceId == 0 && item.AvailableReferences != null && !item.IsLocked)
                    ShowPicker(focus: true);
                else if (!item.IsReferenceEditable)
                    _referenceHost.Content = ReadOnlyReference(item, fg);
                else
                    _referenceHost.Content = ReferenceButton(item);
                if (item.ReferenceId != 0)
                    row.Children.Add(NavigateButton(item));
            }
            if (item.IsNotReadByGame)
                row.Children.Add(Marker("n/r", "Dominions doesn't read this command for this entity type: kept in the mod, but it changes nothing in game"));
            if (item.IsInherited)
                row.Children.Add(Marker("inh", "Inherited: the entity has this from vanilla (or from what it copies), not from a line of the mod. Changing it writes the mod's own line."));
            if (item.CanRemove)
                row.Children.Add(RemoveButton(item, fg));
            Child = row;
        }

        private static TextBlock Text(string? text, double size, IBrush foreground) =>
            new() { Text = text ?? "", FontSize = size, Foreground = foreground, VerticalAlignment = VerticalAlignment.Center };

        /// <summary>": value" in a small box (saved when left or on Enter), and what the value means.</summary>
        private static void AddValue(StackPanel row, PropertyItem item, IBrush fg)
        {
            row.Children.Add(Text(": ", 9, fg));
            var box = new TextBox
            {
                FontSize = 9,
                MinWidth = 24,
                MinHeight = 0,
                Padding = new Thickness(3, 1),
                CornerRadius = default,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = fg,
                Background = Look.Hex("#1AFFFFFF"),
                BorderBrush = Look.Hex("#40FFFFFF"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                IsReadOnly = item.IsLocked,
            };
            Look.FlatBox(box, Look.Hex("#33FFFFFF"), Look.Brush("AccentPrimaryBrush"), Look.Hex("#44FFFFFF"), Look.Brush("AccentHighlightBrush"), new Thickness(0, 0, 0, 1));
            box.Bind(TextBox.TextProperty, new Binding(nameof(PropertyItem.Value)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
            Commit.SetOnLeave(box, true);
            row.Children.Add(box);
            if (!string.IsNullOrEmpty(item.ValueNote))
                row.Children.Add(new TextBlock { Text = item.ValueNote, FontSize = 8, Margin = new Thickness(3, 0, 0, 0), Foreground = Look.Brush("TextSecondaryBrush"), VerticalAlignment = VerticalAlignment.Center });
        }

        /// <summary>A reference that can't be changed here: its name and number.</summary>
        private static Control ReadOnlyReference(PropertyItem item, IBrush fg)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(new TextBlock { Text = item.ReferenceDisplay, FontSize = 9, FontWeight = FontWeight.SemiBold, Foreground = fg, VerticalAlignment = VerticalAlignment.Center });
            if (item.ReferenceId != 0)
                panel.Children.Add(new TextBlock { Text = $"#{item.ReferenceId}", FontSize = 8, Margin = new Thickness(4, 0, 0, 0), Foreground = Look.Brush("TextSecondaryBrush"), VerticalAlignment = VerticalAlignment.Center });
            return panel;
        }

        /// <summary>A reference to change: its name; a click puts a picker in its place, ready to type.</summary>
        private Control ReferenceButton(PropertyItem item)
        {
            var name = new DockPanel();
            var id = new TextBlock { Text = $"#{item.ReferenceId}", FontSize = 8, Margin = new Thickness(6, 0, 0, 0), Foreground = Look.Brush("TextSecondaryBrush"), VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(id, Dock.Right);
            name.Children.Add(id);
            name.Children.Add(new TextBlock { Text = item.ReferenceDisplay, FontSize = 9, Foreground = Look.Brush("TextPrimaryBrush"), TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center });
            var button = new Button
            {
                Content = name,
                MinWidth = 150,
                MaxWidth = 250,
                Height = 20,
                Padding = new Thickness(5, 0),
                CornerRadius = default,
                BorderThickness = new Thickness(0, 0, 0, 1),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Cursor = new Cursor(StandardCursorType.Hand),
            };
            Look.FlatButton(button, Look.Hex("#1AFFFFFF"), Look.Brush("TextPrimaryBrush"), Look.Hex("#33FFFFFF"), Look.Brush("TextPrimaryBrush"), Look.Hex("#40FFFFFF"));
            ToolTip.SetTip(button, $"{item.DisplayName}: click to pick another");
            button.Click += (s, e) =>
            {
                e.Handled = true;
                ShowPicker(focus: true);
            };
            return button;
        }

        private void ShowPicker(bool focus)
        {
            if (_item is not PropertyItem item || _referenceHost == null)
                return;
            var picker = new RefPicker
            {
                MinWidth = 150,
                MaxWidth = 250,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Placeholder = "Select...",
                ItemsSource = item.AvailableReferences,
                SelectedId = item.ReferenceId != 0 ? item.ReferenceId : null,
            };
            picker.SelectionChanged += (s, e) =>
            {
                // the section's command passes it to the badge's own handler, which makes the edit
                var command = Panel?.ReferenceChangedCommand;
                var args = (item, e.OldId ?? 0, e.NewId, e.NewItem?.DisplayName ?? "");
                if (command?.CanExecute(args) == true)
                    command.Execute(args);
            };
            _referenceHost.Content = picker;
            if (!focus)
                return;
            // (a badge just added is made before it's on the page: the cursor goes in once it is)
            if (picker.IsAttachedToVisualTree())
                Dispatcher.UIThread.Post(picker.FocusInput, DispatcherPriority.Loaded);
            else
                picker.AttachedToVisualTree += Attached;

            void Attached(object? s, VisualTreeAttachmentEventArgs e)
            {
                picker.AttachedToVisualTree -= Attached;
                Dispatcher.UIThread.Post(picker.FocusInput, DispatcherPriority.Loaded);
            }
        }

        private Button NavigateButton(PropertyItem item)
        {
            var button = new Button
            {
                Content = "→",
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                Padding = new Thickness(3, 0),
                Margin = new Thickness(4, 0, 0, 0),
                MinWidth = 20,
                Height = 18,
                CornerRadius = new CornerRadius(3),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            Look.FlatButton(button, Look.Brush("BackgroundLightBrush"), Look.Brush("AccentHighlightBrush"), Look.Brush("BackgroundMediumBrush"), Look.Brush("TextPrimaryBrush"), Look.Brush("BorderBrush"));
            ToolTip.SetTip(button, $"Open {item.ReferenceDisplay} (middle click or right click: in a new window)");
            void Open()
            {
                var command = Panel?.NavigateCommand;
                var args = (item.ReferenceType, item.ReferenceId);
                if (command?.CanExecute(args) == true)
                    command.Execute(args);
            }
            Link.SetOpen(button, Open);
            button.Click += (s, e) =>
            {
                e.Handled = true;
                Open();
            };
            return button;
        }

        /// <summary>A small tag after the value ("n/r", "inh").</summary>
        private static Border Marker(string text, string tooltip)
        {
            var tag = new Border
            {
                Background = Look.Brush("AccentSecondaryBrush"),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(2, 0),
                Margin = new Thickness(3, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = text, FontSize = 7.5, Foreground = Brushes.White },
            };
            ToolTip.SetTip(tag, tooltip);
            return tag;
        }

        private Button RemoveButton(PropertyItem item, IBrush fg)
        {
            var button = new Button
            {
                Content = "×",
                FontSize = 10,
                Padding = new Thickness(1, 0),
                Margin = new Thickness(3, 0, 0, 0),
                MinWidth = 12,
                Height = 14,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                BorderThickness = new Thickness(0),
            };
            Look.FlatButton(button, Brushes.Transparent, fg, Brushes.Transparent, Look.Brush("ErrorBrush"));
            ToolTip.SetTip(button, $"Remove {item.DisplayName}");
            button.Click += (s, e) =>
            {
                e.Handled = true;
                var command = Panel?.RemoveCommand;
                if (command?.CanExecute(item) == true)
                    command.Execute(item);
            };
            return button;
        }
    }
}
