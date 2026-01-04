using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit
{
    /// <summary>
    /// Handles exporting mod data to .dm file format.
    /// </summary>
    public class ModExporter
    {
        /// <summary>
        /// Exports a mod to a file.
        /// </summary>
        /// <param name="mod">The mod to export.</param>
        /// <param name="filePath">The file path to write to.</param>
        /// <param name="overwrite">Whether to overwrite an existing file.</param>
        public void Export(Mod mod, string filePath, bool overwrite = true)
        {
            if (File.Exists(filePath) && !overwrite)
            {
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                Export(mod, writer);
            }
        }

        /// <summary>
        /// Exports a mod to a stream.
        /// </summary>
        /// <param name="mod">The mod to export.</param>
        /// <param name="writer">The stream writer to write to.</param>
        public void Export(Mod mod, StreamWriter writer)
        {
            WriteHeader(mod, writer);
            writer.WriteLine();
            WriteEntities(mod, writer);
        }

        /// <summary>
        /// Writes the mod header (name, version, description, etc.).
        /// </summary>
        protected virtual void WriteHeader(Mod mod, StreamWriter writer)
        {
            writer.WriteLine(CommandsMap.Format(Command.MODNAME, mod.ModName, true));

            if (!string.IsNullOrEmpty(mod.Version))
                writer.WriteLine(CommandsMap.Format(Command.VERSION, mod.Version));

            if (!string.IsNullOrEmpty(mod.DomVersion))
                writer.WriteLine(CommandsMap.Format(Command.DOMVERSION, mod.DomVersion));

            if (!string.IsNullOrEmpty(mod.Icon))
                writer.WriteLine(CommandsMap.Format(Command.ICON, mod.Icon, true));

            if (!string.IsNullOrEmpty(mod.Description))
                writer.WriteLine(CommandsMap.Format(Command.DESCRIPTION, mod.Description, true));
        }

        /// <summary>
        /// Writes all entities from the mod's database.
        /// </summary>
        protected virtual void WriteEntities(Mod mod, StreamWriter writer)
        {
            foreach (var kvp in mod.Database)
            {
                kvp.Value.Export(writer);
            }
        }

        /// <summary>
        /// Exports only entities associated with specific nations.
        /// </summary>
        /// <param name="mod">The mod to export from.</param>
        /// <param name="writer">The stream writer to write to.</param>
        /// <param name="nations">The nations to filter by.</param>
        /// <param name="inclusive">If true, includes entities used by any of the nations. If false, only entities exclusive to those nations.</param>
        public void ExportForNations(Mod mod, StreamWriter writer, HashSet<Nation> nations, bool inclusive = false)
        {
            WriteHeader(mod, writer);
            writer.WriteLine();

            Func<IDEntity, bool> filter = inclusive
                ? entity => (entity.AssociatedNations.IsSubsetOf(nations) && entity.AssociatedNations.Count > 0)
                         || entity.AssociatedNations.IsSupersetOf(nations)
                : entity => entity.AssociatedNations.IsSubsetOf(nations) && entity.AssociatedNations.Count > 0;

            foreach (var kvp in mod.Database)
            {
                foreach (var entity in kvp.Value.GetFullList().Where(filter))
                {
                    entity.Export(writer);
                    writer.WriteLine();
                }
            }
        }
    }
}
