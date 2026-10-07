using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// The theme's brushes for controls built in code (CompactBadge, RefPicker), the core's
    /// "#RRGGBB" colours as brushes, and the flat look of small boxes and buttons: Fluent draws
    /// its own hover and focus colours from resources, so a control overrides those resources.
    /// </summary>
    public static class Look
    {
        /// <summary>A brush of the theme (Theme/Colors.axaml), or grey.</summary>
        public static IBrush Brush(string key) =>
            Application.Current != null && Application.Current.TryFindResource(key, out var value) && value is IBrush brush ? brush : Brushes.Gray;

        private static readonly Dictionary<string, IBrush> _hex = new();

        /// <summary>A "#RRGGBB" (or "#AARRGGBB") colour as a brush, made once per colour; null or bad text: transparent.</summary>
        public static IBrush Hex(string? color)
        {
            if (string.IsNullOrEmpty(color))
                return Brushes.Transparent;
            if (_hex.TryGetValue(color, out var brush))
                return brush;
            return _hex[color] = Color.TryParse(color, out var c) ? new ImmutableSolidColorBrush(c) : Brushes.Transparent;
        }

        /// <summary>
        /// A text box without Fluent's own hover and focus colours: these instead (the WPF editor's
        /// light value boxes: a faint fill and an underline that lights up).
        /// </summary>
        public static void FlatBox(TextBox box, IBrush hover, IBrush hoverLine, IBrush focus, IBrush focusLine, Thickness focusThickness)
        {
            box.Resources["TextControlBackgroundPointerOver"] = hover;
            box.Resources["TextControlBorderBrushPointerOver"] = hoverLine;
            box.Resources["TextControlBackgroundFocused"] = focus;
            box.Resources["TextControlBorderBrushFocused"] = focusLine;
            box.Resources["TextControlBorderThemeThicknessFocused"] = focusThickness;
            box.Resources["TextControlForegroundPointerOver"] = box.Foreground ?? Brush("TextPrimaryBrush");
            box.Resources["TextControlForegroundFocused"] = box.Foreground ?? Brush("TextPrimaryBrush");
        }

        /// <summary>A button with these colours instead of Fluent's (normal, pointed at, pressed).</summary>
        public static void FlatButton(Button button, IBrush background, IBrush foreground, IBrush hoverBackground, IBrush hoverForeground, IBrush? border = null, IBrush? hoverBorder = null)
        {
            button.Background = background;
            button.Foreground = foreground;
            button.BorderBrush = border ?? Brushes.Transparent;
            button.Resources["ButtonBackgroundPointerOver"] = hoverBackground;
            button.Resources["ButtonForegroundPointerOver"] = hoverForeground;
            button.Resources["ButtonBorderBrushPointerOver"] = hoverBorder ?? border ?? Brushes.Transparent;
            button.Resources["ButtonBackgroundPressed"] = hoverBackground;
            button.Resources["ButtonForegroundPressed"] = hoverForeground;
            button.Resources["ButtonBorderBrushPressed"] = hoverBorder ?? border ?? Brushes.Transparent;
        }
    }

    /// <summary>Converters for the page's templates.</summary>
    public static class Converters
    {
        /// <summary>
        /// The value put into a text: {Binding Name, Converter={x:Static controls:Converters.Format},
        /// ConverterParameter='Add {0} magic'} (the WPF editor's Format converter; tooltips are
        /// objects, so a binding's StringFormat isn't used for them).
        /// </summary>
        public static readonly IValueConverter Format = new FuncValueConverter<object?, object?, string>(
            (value, parameter) => string.Format(CultureInfo.CurrentCulture, parameter as string ?? "{0}", value));

        /// <summary>An empty text as null: no tooltip (an empty one would still show a box).</summary>
        public static readonly IValueConverter NullIfEmpty = new FuncValueConverter<string?, string?>(s => string.IsNullOrEmpty(s) ? null : s);
    }

    /// <summary>
    /// controls:Smoothing.IsSmooth="{Binding IsSmooth}" on an image: true smooths it (a big picture
    /// shown smaller), false keeps pixel art crisp (RenderOptions can't be bound).
    /// </summary>
    public static class Smoothing
    {
        public static readonly AttachedProperty<bool> IsSmoothProperty =
            AvaloniaProperty.RegisterAttached<Visual, bool>("IsSmooth", typeof(Smoothing));

        static Smoothing()
        {
            IsSmoothProperty.Changed.AddClassHandler<Visual>((visual, e) =>
                RenderOptions.SetBitmapInterpolationMode(visual, e.NewValue is true ? BitmapInterpolationMode.HighQuality : BitmapInterpolationMode.None));
        }

        public static bool GetIsSmooth(Visual visual) => visual.GetValue(IsSmoothProperty);
        public static void SetIsSmooth(Visual visual, bool value) => visual.SetValue(IsSmoothProperty, value);
    }
}
