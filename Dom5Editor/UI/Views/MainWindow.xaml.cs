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

            // Set up entity navigation
            EntityViewModel.SetMainViewModel(_viewModel);

            // Set up keyboard shortcuts
            SetupKeyboardShortcuts();
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
        }

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
            {
                try
                {
                    _viewModel.LoadMod(dialog.FileName);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to load mod:\n\n{ex.Message}",
                        "Load Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
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

        private bool ConfirmDiscardChanges()
        {
            if (!_viewModel.IsDirty)
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
            base.OnClosing(e);
        }
    }
}
