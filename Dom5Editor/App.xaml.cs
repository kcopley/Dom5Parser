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
    }
}
