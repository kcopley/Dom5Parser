using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dom5Editor.UI.Views;

namespace Dom5Editor.Ava.Views
{
    /// <summary>
    /// An entity's page in a window of its own, next to the main window: the main window's
    /// shortcuts (Ctrl+S saves the mod, Ctrl+Z / Ctrl+Y undo and redo the mod's edits, Alt+arrows
    /// and the mouse's side buttons go back and forward in this window), Ctrl+W closes it.
    /// </summary>
    public partial class PageWindow : Window
    {
        private readonly PageWindowViewModel? _vm;
        private readonly MainWindow? _main;

        public PageWindow()
        {
            InitializeComponent();
        }

        public PageWindow(PageWindowViewModel vm, MainWindow main) : this()
        {
            _vm = vm;
            _main = main;
            DataContext = vm;
            vm.CloseRequested += Close;
            Closed += (s, e) => vm.Detach();
            AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
            KeyDown += OnKeyDown;
            AddHandler(PointerReleasedEvent, (s, e) =>
            {
                if (e.InitialPressMouseButton == MouseButton.XButton1) vm.GoBack();
                else if (e.InitialPressMouseButton == MouseButton.XButton2) vm.GoForward();
            }, RoutingStrategies.Bubble, handledEventsToo: true);
        }

        public PageWindowViewModel? ViewModel => _vm;

        /// <summary>Before the control with the cursor sees the key (a text box would take Alt+arrows): save, close, back and forward.</summary>
        private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm == null)
                return;
            bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
            bool alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
            bool arrowsFree = !(OperatingSystem.IsMacOS() && FocusManager?.GetFocusedElement() is TextBox);
            if (ctrl && e.Key == Key.S) _ = _main?.SaveShortcut();
            else if (ctrl && e.Key == Key.W) Close();
            else if (alt && !ctrl && e.Key == Key.Left && arrowsFree) _vm.GoBack();
            else if (alt && !ctrl && e.Key == Key.Right && arrowsFree) _vm.GoForward();
            else
                return;
            e.Handled = true;
        }

        /// <summary>Undo and redo, after the control with the cursor (a text box undoes its own typing first).</summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm == null)
                return;
            bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
            bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            if (ctrl && (e.Key == Key.Y || (e.Key == Key.Z && shift))) _vm.Redo();
            else if (ctrl && e.Key == Key.Z) _vm.Undo();
            else
                return;
            e.Handled = true;
        }

        private void Back_Click(object? sender, RoutedEventArgs e) => _vm?.GoBack();
        private void Forward_Click(object? sender, RoutedEventArgs e) => _vm?.GoForward();
        private void Undo_Click(object? sender, RoutedEventArgs e) => _vm?.Undo();
        private void Redo_Click(object? sender, RoutedEventArgs e) => _vm?.Redo();

        private void ShowInMain_Click(object? sender, RoutedEventArgs e)
        {
            _vm?.ShowInMainWindow();
            _main?.Activate();
        }
    }
}
