using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Dom5Edit;
using Dom5Edit.Validation;
using Dom5Editor.UI;

namespace Dom5Editor.UI.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            // Set up keyboard shortcuts
            SetupKeyboardShortcuts();

            // deleting something others use asks first
            ViewModels.EntityTypeTab.Confirm = message =>
                MessageBox.Show(this, message, "Delete", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;
            // an image set on a mod never saved: it's copied next to the .dm file, so save first
            ViewModels.EntityPageViewModel.SaveFirst = () =>
            {
                if (MessageBox.Show(this, "The image is copied into the mod's folder, next to its .dm file, and this mod hasn't been saved yet.\n\nSave it now?",
                        "Save the mod first", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
                    return false;
                SaveModAs();
                return !string.IsNullOrEmpty(_viewModel.CurrentFilePath);
            };

            RestoreLayout();
        }

        private readonly Session.Settings _settings = Session.Settings.Load();

        /// <summary>Not remembering the layout (the snapshot mode sizes the window itself).</summary>
        public bool KeepLayout { get; set; } = true;

        /// <summary>The window where it was last time (if that's still on a screen).</summary>
        private void RestoreLayout()
        {
            if (_settings.Width is double w && _settings.Height is double h && w > 200 && h > 200)
            {
                Width = w;
                Height = h;
            }
            if (_settings.Left is double l && _settings.Top is double t
                && l > SystemParameters.VirtualScreenLeft - 50 && t > SystemParameters.VirtualScreenTop - 50
                && l < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 100
                && t < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 100)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = l;
                Top = t;
            }
            if (_settings.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void RememberLayout()
        {
            if (!KeepLayout)
                return;
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            _settings.Left = bounds.Left;
            _settings.Top = bounds.Top;
            _settings.Width = bounds.Width;
            _settings.Height = bounds.Height;
            _settings.Maximized = WindowState == WindowState.Maximized;
            _settings.LastTab = (_viewModel.SelectedTab as ViewModels.EntityTypeTab)?.Title;
            _settings.Save();
        }

        /// <summary>Opens a mod file (from the dialog or the recent list) and remembers it.</summary>
        private void Open(string path)
        {
            try
            {
                _viewModel.LoadMod(path);
                _settings.AddRecent(path);
                _settings.Save();
                if (_settings.LastTab is string tab && _viewModel.Tabs.OfType<ViewModels.EntityTypeTab>().FirstOrDefault(t => t.Title == tab) is { } found)
                    _viewModel.SelectedTab = found;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to load mod:\n\n{ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecentButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = new System.Windows.Controls.ContextMenu();
            foreach (var path in _settings.RecentFiles)
            {
                var item = new System.Windows.Controls.MenuItem { Header = System.IO.Path.GetFileName(path), ToolTip = path };
                item.Click += (s, a) =>
                {
                    if (ConfirmDiscardChanges())
                        Open(path);
                };
                menu.Items.Add(item);
            }
            if (menu.Items.Count == 0)
                menu.Items.Add(new System.Windows.Controls.MenuItem { Header = "(no recent mods)", IsEnabled = false });
            menu.Items.Add(new System.Windows.Controls.Separator());
            var game = Dom5Edit.Events.GameInstall.Exe();
            var gameItem = new System.Windows.Controls.MenuItem
            {
                Header = "Dominions 6 folder...",
                ToolTip = game != null ? $"The game's texts, sprites and event messages are read from {System.IO.Path.GetDirectoryName(game)}. Pick another folder."
                                       : "Dominions 6 wasn't found: pick its folder (the one with Dominions6.exe) to show the game's texts, sprites and event messages",
            };
            gameItem.Click += (s, a) => PickGameFolder();
            menu.Items.Add(gameItem);
            menu.PlacementTarget = (UIElement)sender;
            menu.IsOpen = true;
        }

        /// <summary>Asks for Dominions6.exe and remembers its folder (read at the next start).</summary>
        private void PickGameFolder()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Where is Dominions 6? Pick Dominions6.exe",
                Filter = "Dominions6.exe|Dominions6.exe|Programs (*.exe)|*.exe",
            };
            if (dialog.ShowDialog(this) != true)
                return;
            _settings.GameFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
            _settings.Save();
            MessageBox.Show(this, $"The editor will read the game's texts, sprites and event messages from {_settings.GameFolder} the next time it starts.",
                "Dominions 6 folder", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetupKeyboardShortcuts()
        {
            // Ctrl+N = New
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => NewMod()),
                Key.N, ModifierKeys.Control));

            // Ctrl+O = Open/Load
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => LoadMod()),
                Key.O, ModifierKeys.Control));

            // Ctrl+S = Save
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => SaveMod(), () => _viewModel.HasMod),
                Key.S, ModifierKeys.Control));

            // Ctrl+Shift+S = Save As
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => SaveModAs(), () => _viewModel.HasMod),
                Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            // Ctrl+Z = Undo
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => _viewModel.Undo(), () => _viewModel.CanUndo),
                Key.Z, ModifierKeys.Control));

            // Ctrl+Y = Redo
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => _viewModel.Redo(), () => _viewModel.CanRedo),
                Key.Y, ModifierKeys.Control));

            // Ctrl+F = the list's search box; Ctrl+P = go to any entity
            InputBindings.Add(new KeyBinding(new RelayCommand(() => VisibleList()?.FocusSearch()), Key.F, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCommand(() => { _viewModel.EnsureJumpTargets(); JumpBox.FocusInput(); }, () => _viewModel.HasMod), Key.P, ModifierKeys.Control));

            // Alt+Left / Alt+Right = back / forward through the entities visited
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => _viewModel.GoBack(), () => _viewModel.CanGoBack),
                Key.Left, ModifierKeys.Alt));
            InputBindings.Add(new KeyBinding(
                new RelayCommand(() => _viewModel.GoForward(), () => _viewModel.CanGoForward),
                Key.Right, ModifierKeys.Alt));
            // the mouse's back and forward buttons
            MouseUp += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.XButton1) _viewModel.GoBack();
                else if (e.ChangedButton == MouseButton.XButton2) _viewModel.GoForward();
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => _viewModel.GoBack();

        private void JumpBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => _viewModel.EnsureJumpTargets();

        private void JumpBox_SelectionChanged(object sender, Controls.ReferenceSelectionChangedEventArgs e)
        {
            _viewModel.JumpTo(e.NewItem);
            Dispatcher.BeginInvoke(new Action(() => JumpBox.SetSelectedIdSilent(null)), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>The entity list on screen, if any.</summary>
        private Controls.EntityListControl? VisibleList() => FindVisual<Controls.EntityListControl>(EntityTabs).FirstOrDefault(c => c.IsVisible);

        private static IEnumerable<T> FindVisual<T>(DependencyObject root) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T t)
                    yield return t;
                foreach (var d in FindVisual<T>(child))
                    yield return d;
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e) => _viewModel.GoForward();

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            NewMod();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMod();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMod();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveModAs();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Redo();
        }

        // the tab row scrolls sideways when the window is too narrow for it: the wheel moves it, and
        // the selected tab is kept in sight
        private void TabStrip_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer strip && strip.ScrollableWidth > 0)
            {
                strip.ScrollToHorizontalOffset(strip.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void EntityTabs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, EntityTabs))
                return; // (a list or combo box inside a tab)
            if (EntityTabs.ItemContainerGenerator.ContainerFromItem(EntityTabs.SelectedItem) is FrameworkElement tab)
                tab.BringIntoView();
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            var results = _viewModel.Validate();
            if (results == null) return;

            var dialog = new ValidationReportWindow(results, _viewModel);
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void NewMod()
        {
            if (!ConfirmDiscardChanges())
                return;

            _viewModel.CreateNewMod();
        }

        private void LoadMod()
        {
            if (!ConfirmDiscardChanges())
                return;

            var dialog = new OpenFileDialog
            {
                Title = "Load Mod File",
                Filter = "Dominions Mod Files (*.dm)|*.dm|All Files (*.*)|*.*",
                DefaultExt = ".dm"
            };

            if (dialog.ShowDialog() == true)
                Open(dialog.FileName);
        }

        private void SaveMod()
        {
            if (string.IsNullOrEmpty(_viewModel.CurrentFilePath))
            {
                SaveModAs();
                return;
            }

            try
            {
                _viewModel.SaveMod(_viewModel.CurrentFilePath);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save mod:\n\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveModAs()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Mod File",
                Filter = "Dominions Mod Files (*.dm)|*.dm|All Files (*.*)|*.*",
                DefaultExt = ".dm",
                FileName = _viewModel.ModName ?? "newmod"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _viewModel.SaveMod(dialog.FileName);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to save mod:\n\n{ex.Message}",
                        "Save Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>Close without asking about unsaved changes (the snapshot mode closes the window itself).</summary>
        public bool SkipCloseConfirmation { get; set; }

        private bool ConfirmDiscardChanges()
        {
            if (!_viewModel.IsDirty || SkipCloseConfirmation)
                return true;

            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to save before continuing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveMod();
                    return !_viewModel.IsDirty; // Only continue if save succeeded
                case MessageBoxResult.No:
                    return true;
                default:
                    return false;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmDiscardChanges())
            {
                e.Cancel = true;
            }
            else
            {
                RememberLayout();
            }
            base.OnClosing(e);
        }
    }
}
