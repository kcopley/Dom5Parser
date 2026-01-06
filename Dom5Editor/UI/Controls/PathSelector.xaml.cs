using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// A compact icon-based path selector control for magic path + level selection.
    /// Supports "None" (-1) as a valid selection for optional paths.
    /// </summary>
    public partial class PathSelector : UserControl
    {
        private bool _isUpdating;
        private readonly DispatcherTimer _debounceTimer;

        public PathSelector()
        {
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;

            SelectPathCommand = new RelayCommand<int>(OnPathSelected);
            IncrementLevelCommand = new SimpleRelayCommand(OnIncrementLevel, () => PathId >= 0 && Level < 10);
            DecrementLevelCommand = new SimpleRelayCommand(OnDecrementLevel, () => PathId >= 0 && Level > 1);

            InitializeComponent();
            InitializePathOptions();
        }

        private void OnIncrementLevel()
        {
            if (Level < 10)
            {
                Level++;
                LevelText = Level.ToString();
                LevelChanged?.Invoke(this, Level);
            }
        }

        private void OnDecrementLevel()
        {
            if (Level > 1)
            {
                Level--;
                LevelText = Level.ToString();
                LevelChanged?.Invoke(this, Level);
            }
        }

        #region Path Option Data

        public class PathOption : INotifyPropertyChanged
        {
            private bool _isSelected;
            private bool _isExcluded;

            public int PathId { get; set; }
            public string Letter { get; set; }
            public string DisplayName { get; set; }
            public string IconPath { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                    }
                }
            }

            public bool IsExcluded
            {
                get => _isExcluded;
                set
                {
                    if (_isExcluded != value)
                    {
                        _isExcluded = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExcluded)));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private static readonly (int Id, string Letter, string Name)[] MagicPathDefinitions =
        {
            (0, "F", "Fire"),
            (1, "A", "Air"),
            (2, "W", "Water"),
            (3, "E", "Earth"),
            (4, "S", "Astral"),
            (5, "D", "Death"),
            (6, "N", "Nature"),
            (7, "G", "Glamour"),
            (8, "B", "Blood"),
        };

        public ObservableCollection<PathOption> PathOptions { get; } = new ObservableCollection<PathOption>();

        private static string GetIconPath(string letter)
        {
            // Get the base directory of the application
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var iconPath = Path.Combine(baseDir, "icons", "magicicons", $"Path_{letter}.png");

            // Fallback to pack URI if file doesn't exist
            if (!File.Exists(iconPath))
            {
                return $"pack://application:,,,/icons/magicicons/Path_{letter}.png";
            }
            return iconPath;
        }

        private void InitializePathOptions()
        {
            foreach (var (id, letter, name) in MagicPathDefinitions)
            {
                PathOptions.Add(new PathOption
                {
                    PathId = id,
                    Letter = letter,
                    DisplayName = name,
                    IconPath = GetIconPath(letter),
                    IsSelected = false
                });
            }

            UpdateSelectedState();
        }

        #endregion

        #region Commands

        public ICommand SelectPathCommand { get; }
        public ICommand IncrementLevelCommand { get; }
        public ICommand DecrementLevelCommand { get; }

        private void OnPathSelected(int pathId)
        {
            if (_isUpdating) return;

            _isUpdating = true;

            // Toggle: if already selected, deselect (set to -1)
            if (PathId == pathId)
            {
                PathId = -1;
                Level = 0;
            }
            else
            {
                PathId = pathId;
                // Set default level to 1 when selecting a path
                if (Level <= 0) Level = 1;
            }

            UpdateSelectedState();
            _isUpdating = false;

            PathChanged?.Invoke(this, PathId);
        }

        private void UpdateSelectedState()
        {
            foreach (var option in PathOptions)
            {
                option.IsSelected = option.PathId == PathId;
            }
            ShowLevel = PathId >= 0;
        }

        #endregion

        #region Debounce Timer

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            CommitLevelChange();
        }

        #endregion

        #region Dependency Properties

        // PathId (-1 = None, 0-8 = magic paths)
        public static readonly DependencyProperty PathIdProperty =
            DependencyProperty.Register(nameof(PathId), typeof(int), typeof(PathSelector),
                new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPathIdChanged));

        public int PathId
        {
            get => (int)GetValue(PathIdProperty);
            set => SetValue(PathIdProperty, value);
        }

        private static void OnPathIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PathSelector selector && !selector._isUpdating)
            {
                selector.UpdateSelectedState();
            }
        }

        // Level (1-9, default 1)
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(nameof(Level), typeof(int), typeof(PathSelector),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLevelChanged));

        public int Level
        {
            get => (int)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PathSelector selector && !selector._isUpdating)
            {
                selector._isUpdating = true;
                selector.LevelText = ((int)e.NewValue).ToString();
                selector._isUpdating = false;
            }
        }

        // Label
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(PathSelector),
                new PropertyMetadata(string.Empty, OnLabelChanged));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PathSelector selector)
            {
                selector.HasLabel = !string.IsNullOrEmpty(e.NewValue as string);
            }
        }

        // HasLabel
        public static readonly DependencyProperty HasLabelProperty =
            DependencyProperty.Register(nameof(HasLabel), typeof(bool), typeof(PathSelector),
                new PropertyMetadata(false));

        public bool HasLabel
        {
            get => (bool)GetValue(HasLabelProperty);
            set => SetValue(HasLabelProperty, value);
        }

        // IsModified
        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register(nameof(IsModified), typeof(bool), typeof(PathSelector),
                new PropertyMetadata(false));

        public bool IsModified
        {
            get => (bool)GetValue(IsModifiedProperty);
            set => SetValue(IsModifiedProperty, value);
        }

        // IsSessionEdit
        public static readonly DependencyProperty IsSessionEditProperty =
            DependencyProperty.Register(nameof(IsSessionEdit), typeof(bool), typeof(PathSelector),
                new PropertyMetadata(false));

        public bool IsSessionEdit
        {
            get => (bool)GetValue(IsSessionEditProperty);
            set => SetValue(IsSessionEditProperty, value);
        }

        // IsInherited
        public static readonly DependencyProperty IsInheritedProperty =
            DependencyProperty.Register(nameof(IsInherited), typeof(bool), typeof(PathSelector),
                new PropertyMetadata(false));

        public bool IsInherited
        {
            get => (bool)GetValue(IsInheritedProperty);
            set => SetValue(IsInheritedProperty, value);
        }

        // LevelText (for textbox binding)
        public static readonly DependencyProperty LevelTextProperty =
            DependencyProperty.Register(nameof(LevelText), typeof(string), typeof(PathSelector),
                new PropertyMetadata("1"));

        public string LevelText
        {
            get => (string)GetValue(LevelTextProperty);
            set => SetValue(LevelTextProperty, value);
        }

        // ShowLevel - whether to show the level input
        public static readonly DependencyProperty ShowLevelProperty =
            DependencyProperty.Register(nameof(ShowLevel), typeof(bool), typeof(PathSelector),
                new PropertyMetadata(false));

        public bool ShowLevel
        {
            get => (bool)GetValue(ShowLevelProperty);
            set => SetValue(ShowLevelProperty, value);
        }

        // ExcludedPathId - path that cannot be selected (used by other selector)
        public static readonly DependencyProperty ExcludedPathIdProperty =
            DependencyProperty.Register(nameof(ExcludedPathId), typeof(int), typeof(PathSelector),
                new PropertyMetadata(-1, OnExcludedPathIdChanged));

        public int ExcludedPathId
        {
            get => (int)GetValue(ExcludedPathIdProperty);
            set => SetValue(ExcludedPathIdProperty, value);
        }

        private static void OnExcludedPathIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PathSelector selector)
            {
                selector.UpdateExcludedState();
            }
        }

        private void UpdateExcludedState()
        {
            foreach (var option in PathOptions)
            {
                option.IsExcluded = option.PathId == ExcludedPathId;
            }
        }

        #endregion

        #region Event Handlers

        private void LevelTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow 1-9 (0 is not valid, but unselected path is)
            e.Handled = !int.TryParse(e.Text, out int value) || value < 1 || value > 9;
        }

        private void LevelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void CommitLevelChange()
        {
            if (_isUpdating) return;

            if (int.TryParse(LevelText, out int value) && value >= 1 && value <= 9)
            {
                _isUpdating = true;
                Level = value;
                _isUpdating = false;

                LevelChanged?.Invoke(this, Level);
            }
            else if (PathId >= 0)
            {
                // Invalid input when path is selected - reset to 1
                _isUpdating = true;
                Level = 1;
                LevelText = "1";
                _isUpdating = false;
            }
        }

        #endregion

        #region Events

        public event EventHandler<int> PathChanged;
        public event EventHandler<int> LevelChanged;

        #endregion

        #region Static Helpers

        public static string GetPathName(int pathId)
        {
            if (pathId < 0 || pathId >= MagicPathDefinitions.Length)
                return "None";
            return MagicPathDefinitions[pathId].Name;
        }

        public static string GetPathLetter(int pathId)
        {
            if (pathId < 0 || pathId >= MagicPathDefinitions.Length)
                return "";
            return MagicPathDefinitions[pathId].Letter;
        }

        #endregion

        #region RelayCommand

        private class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;

            public RelayCommand(Action<T> execute)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                if (parameter is T typedParam)
                    _execute(typedParam);
                else if (parameter != null && typeof(T) == typeof(int) && int.TryParse(parameter.ToString(), out int intVal))
                    _execute((T)(object)intVal);
            }
        }

        private class SimpleRelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public SimpleRelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

            public void Execute(object parameter) => _execute();
        }

        #endregion
    }
}
