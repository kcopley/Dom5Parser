using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// A toggle button for selecting a magic path in the custom magic editor.
    /// Shows the path letter with appropriate coloring based on checked state.
    /// </summary>
    public partial class PathToggleButton : UserControl
    {
        public PathToggleButton()
        {
            InitializeComponent();
        }

        // ========================================
        // Dependency Properties
        // ========================================

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(PathToggleButton),
                new PropertyMetadata("?"));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty PathNameProperty =
            DependencyProperty.Register(nameof(PathName), typeof(string), typeof(PathToggleButton),
                new PropertyMetadata("Unknown"));

        public string PathName
        {
            get => (string)GetValue(PathNameProperty);
            set => SetValue(PathNameProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(PathToggleButton),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(PathToggleButton),
                new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty PathColorProperty =
            DependencyProperty.Register(nameof(PathColor), typeof(string), typeof(PathToggleButton),
                new PropertyMetadata("#808080", OnColorChanged));

        public string PathColor
        {
            get => (string)GetValue(PathColorProperty);
            set => SetValue(PathColorProperty, value);
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(nameof(TextColor), typeof(string), typeof(PathToggleButton),
                new PropertyMetadata("White", OnColorChanged));

        public string TextColor
        {
            get => (string)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        // ========================================
        // Computed Brush Properties
        // ========================================

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Force property change notifications for computed properties
        }

        public Brush SelectedBackground
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(PathColor));
                }
                catch
                {
                    return Brushes.Gray;
                }
            }
        }

        public Brush SelectedForeground
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(TextColor));
                }
                catch
                {
                    return Brushes.White;
                }
            }
        }

        public Brush SelectedBorder
        {
            get
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(PathColor);
                    // Darken for border
                    var darker = Color.FromRgb(
                        (byte)System.Math.Max(0, color.R - 40),
                        (byte)System.Math.Max(0, color.G - 40),
                        (byte)System.Math.Max(0, color.B - 40));
                    return new SolidColorBrush(darker);
                }
                catch
                {
                    return Brushes.DarkGray;
                }
            }
        }

        public Brush UnselectedBackground => new SolidColorBrush(Color.FromRgb(60, 60, 60));
        public Brush UnselectedForeground => new SolidColorBrush(Color.FromRgb(120, 120, 120));
        public Brush UnselectedBorder => new SolidColorBrush(Color.FromRgb(80, 80, 80));
    }
}
