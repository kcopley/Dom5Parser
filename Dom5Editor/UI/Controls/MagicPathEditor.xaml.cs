using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Magic path definitions with colors matching Dominions 5 paths.
    /// Maps to Dom5Edit.Commands.MagicPaths enum values.
    /// </summary>
    public static class MagicPathDefinitions
    {
        // Path definitions: (Id matches MagicPaths enum, Letter, Name, Color, TextColor)
        public static readonly (int Id, string Letter, string Name, Color Color, Color TextColor)[] PathDefs = new[]
        {
            (0, "F", "Fire", Color.FromRgb(220, 60, 40), Colors.White),      // FIRE = 0
            (1, "A", "Air", Color.FromRgb(180, 180, 240), Colors.Black),     // AIR = 1
            (2, "W", "Water", Color.FromRgb(60, 120, 200), Colors.White),    // WATER = 2
            (3, "E", "Earth", Color.FromRgb(139, 90, 43), Colors.White),     // EARTH = 3
            (4, "S", "Astral", Color.FromRgb(255, 215, 0), Colors.Black),    // ASTRAL = 4
            (5, "D", "Death", Color.FromRgb(50, 50, 50), Colors.White),      // DEATH = 5
            (6, "N", "Nature", Color.FromRgb(60, 160, 60), Colors.White),    // NATURE = 6
            (7, "G", "Glamour", Color.FromRgb(180, 100, 180), Colors.White), // GLAMOUR = 7
            (8, "B", "Blood", Color.FromRgb(140, 20, 20), Colors.White),     // BLOOD = 8
            (9, "H", "Holy", Color.FromRgb(255, 255, 200), Colors.Black)     // PRIEST = 9
        };

        public static (string Letter, string Name, SolidColorBrush Color, SolidColorBrush TextColor, SolidColorBrush BorderColor) GetPathInfo(int pathId)
        {
            var def = PathDefs.FirstOrDefault(p => p.Id == pathId);
            if (def.Letter != null)
            {
                return (def.Letter, def.Name, new SolidColorBrush(def.Color),
                        new SolidColorBrush(def.TextColor),
                        new SolidColorBrush(Color.FromRgb(
                            (byte)Math.Max(0, def.Color.R - 30),
                            (byte)Math.Max(0, def.Color.G - 30),
                            (byte)Math.Max(0, def.Color.B - 30))));
            }
            return ("?", "Unknown", Brushes.Gray, Brushes.White, Brushes.DarkGray);
        }
    }

    /// <summary>
    /// Represents an active magic path with its level.
    /// </summary>
    public class MagicPathItem : INotifyPropertyChanged
    {
        private int _level;

        public int PathId { get; set; }
        public string PathLetter { get; set; }
        public string PathName { get; set; }
        public Brush PathColor { get; set; }
        public Brush TextColor { get; set; }
        public Brush BorderColor { get; set; }
        public bool IsInherited { get; set; }
        public bool IsModified { get; set; }
        public bool IsSessionEdit { get; set; }

        public int Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged();
                    LevelChanged?.Invoke(this, value);
                }
            }
        }

        public bool CanRemove => !IsInherited;
        public string RemoveTooltip => IsInherited
            ? "Cannot remove inherited magic path"
            : "Remove this magic path";

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<int> LevelChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents an available magic path that can be added.
    /// </summary>
    public class AvailableMagicPath
    {
        public int PathId { get; set; }
        public string PathLetter { get; set; }
        public string PathName { get; set; }
        public Brush PathColor { get; set; }
        public Brush TextColor { get; set; }
        public Brush BorderColor { get; set; }
    }

    /// <summary>
    /// Editor control for magic paths with colored badges.
    /// </summary>
    public partial class MagicPathEditor : UserControl, INotifyPropertyChanged
    {
        public MagicPathEditor()
        {
            InitializeComponent();
            // Don't set DataContext = this, it breaks parent bindings

            // Set up commands
            AddMagicPathCommand = new RelayCommand<AvailableMagicPath>(OnAddMagicPath);
            RemoveMagicPathCommand = new RelayCommand<MagicPathItem>(OnRemoveMagicPath, item => item?.CanRemove ?? false);

            // Initialize available paths (these are static, always the same)
            InitializeAvailablePaths();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ========================================
        // Dependency Properties
        // ========================================

        public static readonly DependencyProperty MagicPathsProperty =
            DependencyProperty.Register(
                nameof(MagicPaths),
                typeof(ObservableCollection<MagicPathItem>),
                typeof(MagicPathEditor),
                new PropertyMetadata(null));

        public ObservableCollection<MagicPathItem> MagicPaths
        {
            get => (ObservableCollection<MagicPathItem>)GetValue(MagicPathsProperty);
            set => SetValue(MagicPathsProperty, value);
        }

        public static readonly DependencyProperty AvailablePathsProperty =
            DependencyProperty.Register(
                nameof(AvailablePaths),
                typeof(ObservableCollection<AvailableMagicPath>),
                typeof(MagicPathEditor),
                new PropertyMetadata(null));

        public ObservableCollection<AvailableMagicPath> AvailablePaths
        {
            get => (ObservableCollection<AvailableMagicPath>)GetValue(AvailablePathsProperty);
            set => SetValue(AvailablePathsProperty, value);
        }

        // ========================================
        // Commands
        // ========================================

        public ICommand AddMagicPathCommand { get; }
        public ICommand RemoveMagicPathCommand { get; }

        // ========================================
        // Events
        // ========================================

        /// <summary>
        /// Raised when a magic path should be added. Tuple is (PathId, Level).
        /// </summary>
        public event EventHandler<(int PathId, int Level)> PathAdded;

        /// <summary>
        /// Raised when a magic path should be removed.
        /// </summary>
        public event EventHandler<int> PathRemoved;

        /// <summary>
        /// Raised when a magic path level is changed.
        /// </summary>
        public event EventHandler<(int PathId, int Level)> PathLevelChanged;

        // ========================================
        // Initialization
        // ========================================

        private void InitializeAvailablePaths()
        {
            var available = new ObservableCollection<AvailableMagicPath>();
            foreach (var def in MagicPathDefinitions.PathDefs)
            {
                var info = MagicPathDefinitions.GetPathInfo(def.Id);
                available.Add(new AvailableMagicPath
                {
                    PathId = def.Id,
                    PathLetter = info.Letter,
                    PathName = info.Name,
                    PathColor = info.Color,
                    TextColor = info.TextColor,
                    BorderColor = info.BorderColor
                });
            }
            AvailablePaths = available;
        }

        // ========================================
        // Command Handlers
        // ========================================

        private void OnAddMagicPath(AvailableMagicPath path)
        {
            if (path != null)
            {
                // Default level of 1 when adding
                PathAdded?.Invoke(this, (path.PathId, 1));
            }
        }

        private void OnRemoveMagicPath(MagicPathItem item)
        {
            if (item != null && item.CanRemove)
            {
                PathRemoved?.Invoke(this, item.PathId);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
