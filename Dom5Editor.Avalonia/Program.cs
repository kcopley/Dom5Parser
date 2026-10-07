using Avalonia;

namespace Dom5Editor.Ava
{
    internal static class Program
    {
        // Avalonia: nothing that needs the platform before AppMain is called
        [STAThread]
        public static int Main(string[] args)
        {
            // Dom5Editor.Avalonia --snapshot ...: render off-screen for checking (Snapshot.cs)
            if (args.Length > 0 && args[0] == "--snapshot")
                return Snapshot.Run(args);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
    }
}
