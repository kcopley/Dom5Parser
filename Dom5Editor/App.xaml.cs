using System.IO;
using System.Windows;
using Dom5Edit;

namespace Dom5Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure VanillaLoader before any UI loads
            ConfigureVanillaLoader();

            // without the vanilla data the editor can't show or check anything: say where it looks
            if (string.IsNullOrEmpty(VanillaLoader.VanillaDmPath) || !File.Exists(VanillaLoader.VanillaDmPath))
            {
                MessageBox.Show("vanilla.dm wasn't found. It holds the game's own data, which the editor builds on.\n\n" +
                    $"Put vanilla.dm next to the editor ({AppDomain.CurrentDomain.BaseDirectory}).",
                    "Dom6 Mod Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            // Dom5Editor --snapshot ...: render the editor off-screen for checking (UI/Snapshot.cs)
            if (UI.Snapshot.TryRun(e.Args, this))
                return;

            // Force load vanilla data immediately on startup
            // This ensures vanilla.dm is parsed before any mod is loaded
            LoadVanillaData();

            // an error the editor doesn't expect: say so, log it, and keep running (unsaved edits stay)
            DispatcherUnhandledException += (s, args) =>
            {
                try
                {
                    Directory.CreateDirectory(Session.Settings.Folder);
                    File.AppendAllText(Path.Combine(Session.Settings.Folder, "errors.log"), $"{DateTime.Now:u}\n{args.Exception}\n\n");
                }
                catch (Exception) { }
                MessageBox.Show($"Something went wrong:\n\n{args.Exception.Message}\n\nThe editor keeps running; your edits are still there (save to keep them). Details are in {Path.Combine(Session.Settings.Folder, "errors.log")}.",
                    "Dom6 Mod Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            MainWindow = new UI.Views.MainWindow();
            MainWindow.Show();
        }

        private void LoadVanillaData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[App] Loading vanilla data on startup...");
                var vanilla = VanillaLoader.Vanilla;
                if (vanilla != null)
                {
                    int monsterCount = vanilla.Database[Dom5Edit.Entities.EntityType.MONSTER].GetFullList().Count;
                    int siteCount = vanilla.Database[Dom5Edit.Entities.EntityType.SITE].GetFullList().Count;
                    int nationCount = vanilla.Database[Dom5Edit.Entities.EntityType.NATION].GetFullList().Count;
                    System.Diagnostics.Debug.WriteLine($"[App] Vanilla data loaded: {monsterCount} monsters, {siteCount} sites, {nationCount} nations");
                    System.Diagnostics.Debug.WriteLine($"[App] Game texts: {VanillaLoader.TextsStatus}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[App] WARNING: VanillaLoader.Vanilla returned null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] ERROR loading vanilla data: {ex.Message}");
                MessageBox.Show($"Failed to load vanilla.dm:\n{ex.Message}\n\nThe application may not function correctly.",
                    "Vanilla Data Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ConfigureVanillaLoader()
        {
            // Set game version to Dom6
            VanillaLoader.GameVersion = GameVersion.Dom6;

            // Search for vanilla.dm in several locations
            string vanillaDmPath = FindVanillaDm();
            if (!string.IsNullOrEmpty(vanillaDmPath))
            {
                VanillaLoader.VanillaDmPath = vanillaDmPath;
            }

            // Search for spell effect mapping files
            string spellMappingPath = FindFile("spell_effects_mapping.json");
            if (!string.IsNullOrEmpty(spellMappingPath))
            {
                VanillaLoader.SpellEffectMappingPath = spellMappingPath;
            }

            string spellTypesPath = FindFile("spell_effect_types.json");
            if (!string.IsNullOrEmpty(spellTypesPath))
            {
                VanillaLoader.SpellEffectTypesPath = spellTypesPath;
            }

            // Configure assets path for sprite/description loading
            string assetsPath = FindAssetsPath();
            if (!string.IsNullOrEmpty(assetsPath))
            {
                VanillaAssetLoader.AssetsBasePath = assetsPath;
                System.Diagnostics.Debug.WriteLine($"[App] Assets path configured: {assetsPath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[App] WARNING: Assets path not found - sprites and descriptions will not be loaded");
            }
        }

        private string FindVanillaDm()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Search paths in order of preference
            var searchPaths = new[]
            {
                // Direct locations
                Path.Combine(baseDir, "vanilla.dm"),
                Path.Combine(baseDir, "VanillaData", "vanilla.dm"),

                // Navigate up from bin/Debug/net8.0-windows
                Path.Combine(baseDir, "..", "..", "..", "..", "vanilla.dm"),
                Path.Combine(baseDir, "..", "..", "..", "vanilla.dm"),
                Path.Combine(baseDir, "..", "..", "vanilla.dm"),
                Path.Combine(baseDir, "..", "vanilla.dm"),

                // Current working directory
                "vanilla.dm",
                Path.Combine(Directory.GetCurrentDirectory(), "vanilla.dm"),
            };

            foreach (var path in searchPaths)
            {
                try
                {
                    string fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                    // Ignore path resolution errors
                }
            }

            return null;
        }

        private string FindFile(string filename)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var searchPaths = new[]
            {
                Path.Combine(baseDir, filename),
                Path.Combine(baseDir, "..", "..", "..", "..", filename),
                Path.Combine(baseDir, "..", "..", "..", filename),
                Path.Combine(baseDir, "..", "..", filename),
                Path.Combine(baseDir, "..", filename),
                filename,
                Path.Combine(Directory.GetCurrentDirectory(), filename),
            };

            foreach (var path in searchPaths)
            {
                try
                {
                    string fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                    // Ignore path resolution errors
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the base path for sprite/description assets.
        /// Assets are expected to be in icons/ and Data/ subdirectories.
        /// </summary>
        private string FindAssetsPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Search paths where assets might be located
            var searchPaths = new[]
            {
                // Direct locations (e.g., deployed with output)
                baseDir,
                Path.Combine(baseDir, ".."),

                // Navigate up from bin/Debug/net8.0-windows to Dom5Editor project
                Path.Combine(baseDir, "..", "..", ".."),
                Path.Combine(baseDir, "..", "..", "..", ".."),

                // Current working directory
                Directory.GetCurrentDirectory(),
            };

            foreach (var path in searchPaths)
            {
                try
                {
                    string fullPath = Path.GetFullPath(path);
                    // Check if this directory contains the expected asset folders
                    if (Directory.Exists(Path.Combine(fullPath, "icons", "sprites")) &&
                        Directory.Exists(Path.Combine(fullPath, "Data", "unitdescr")))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                    // Ignore path resolution errors
                }
            }

            return null;
        }
    }
}
