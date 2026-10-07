using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Dom5Editor.Ava
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // the game's data, the player's game folder, the data files (shared with the WPF editor)
                var problem = EditorStartup.Configure() ?? EditorStartup.LoadVanilla();
                Hooks.Install();
                var window = new Views.MainWindow();
                desktop.MainWindow = window;
                Hooks.Owner = window;
                if (problem != null)
                    window.Status(problem);
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
