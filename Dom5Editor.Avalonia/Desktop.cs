using System.Diagnostics;

namespace Dom5Editor.Ava
{
    /// <summary>The system's own file manager, on each system.</summary>
    public static class Desktop
    {
        /// <summary>
        /// Shows a folder in Explorer (Windows), Finder (macOS) or the desktop's file manager
        /// (Linux: xdg-open). False if that couldn't be started (no xdg-open, say).
        /// </summary>
        public static bool OpenFolder(string folder)
        {
            try
            {
                var start = OperatingSystem.IsWindows() ? new ProcessStartInfo("explorer.exe")
                    : OperatingSystem.IsMacOS() ? new ProcessStartInfo("open")
                    : new ProcessStartInfo("xdg-open");
                start.ArgumentList.Add(folder);
                start.UseShellExecute = false; // (the program itself, by name on the PATH: no shell to quote for)
                using var process = Process.Start(start);
                return process != null;
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is InvalidOperationException)
            {
                return false;
            }
        }
    }
}
