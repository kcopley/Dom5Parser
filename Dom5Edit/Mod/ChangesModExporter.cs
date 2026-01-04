using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// Exports changes from a ChangesMod, either standalone or merged with a loaded mod.
    /// </summary>
    public class ChangesModExporter
    {
        private readonly ChangesMod _changes;

        public ChangesModExporter(ChangesMod changes)
        {
            _changes = changes;
        }

        /// <summary>
        /// Exports the changes to a stream.
        /// If a LoadedMod exists, exports merged result.
        /// If no LoadedMod, exports changes only.
        /// </summary>
        public void Export(StreamWriter writer, string modName, string description = null)
        {
            // Write mod header
            WriteHeader(writer, modName, description);

            if (_changes.LoadedMod != null)
            {
                // Merge mode: export loaded mod + changes
                ExportMerged(writer);
            }
            else
            {
                // Changes only mode: export vanilla overrides and new entities
                ExportChangesOnly(writer);
            }
        }

        private void WriteHeader(StreamWriter writer, string modName, string description)
        {
            writer.WriteLine($"#modname \"{modName}\"");
            if (!string.IsNullOrEmpty(description))
            {
                writer.WriteLine($"#description \"{description}\"");
            }
            writer.WriteLine();
        }

        /// <summary>
        /// Exports changes only (for when editing vanilla without a loaded mod).
        /// Vanilla overrides export as #select* blocks.
        /// New entities export as #new* blocks.
        /// </summary>
        private void ExportChangesOnly(StreamWriter writer)
        {
            // Group changes by entity type for organized output
            var changesByType = _changes.GetAllChanges()
                .GroupBy(c => c.EntityType)
                .OrderBy(g => g.Key);

            foreach (var group in changesByType)
            {
                writer.WriteLine($"-- {group.Key} modifications");
                writer.WriteLine();

                foreach (var changes in group.OrderBy(c => c.EntityId))
                {
                    ExportVanillaOverride(writer, changes);
                }
            }

            // Export new entities
            ExportNewEntities(writer);
        }

        /// <summary>
        /// Exports a vanilla override as a #select* block with only changed properties.
        /// </summary>
        private void ExportVanillaOverride(StreamWriter writer, EntityChanges changes)
        {
            var selectCommand = GetSelectCommand(changes.EntityType);
            if (selectCommand == null) return;

            writer.WriteLine($"{selectCommand} {changes.EntityId}");

            foreach (var prop in changes.ChangedProperties.Values)
            {
                writer.WriteLine(prop.ToExportString());
            }

            writer.WriteLine("#end");
            writer.WriteLine();
        }

        /// <summary>
        /// Exports merged result: loaded mod + changes applied.
        /// </summary>
        private void ExportMerged(StreamWriter writer)
        {
            // Export each entity type from loaded mod with changes applied
            foreach (var kvp in _changes.LoadedMod.Database)
            {
                var entityType = kvp.Key;
                var entitySet = kvp.Value;

                foreach (var entity in entitySet.GetFullList())
                {
                    // Skip removed entities
                    if (_changes.IsEntityRemoved(entityType, entity.ID))
                    {
                        continue;
                    }

                    // Check if this entity has changes
                    if (_changes.TryGetChanges(entityType, entity.ID, out var changes))
                    {
                        ExportModEntityWithChanges(writer, entity, changes);
                    }
                    else
                    {
                        // No changes, export as-is
                        entity.Export(writer);
                        writer.WriteLine();
                    }
                }
            }

            // Export vanilla overrides (entities not in loaded mod but modified)
            var vanillaOverrides = _changes.GetAllChanges()
                .Where(c => c.IsVanillaOverride);

            if (vanillaOverrides.Any())
            {
                writer.WriteLine("-- Vanilla overrides");
                writer.WriteLine();

                foreach (var changes in vanillaOverrides.OrderBy(c => c.EntityType).ThenBy(c => c.EntityId))
                {
                    ExportVanillaOverride(writer, changes);
                }
            }

            // Export new entities
            ExportNewEntities(writer);
        }

        /// <summary>
        /// Exports a mod entity with changes applied.
        /// </summary>
        private void ExportModEntityWithChanges(StreamWriter writer, IDEntity entity, EntityChanges changes)
        {
            // Determine if this is a new entity or select
            var newCommand = GetNewCommand(changes.EntityType);
            var selectCommand = GetSelectCommand(changes.EntityType);

            if (entity.Selected && selectCommand != null)
            {
                writer.WriteLine($"{selectCommand} {entity.ID}");
            }
            else if (newCommand != null)
            {
                writer.WriteLine($"{newCommand} {entity.ID}");
            }

            // Export properties, applying changes
            var exportedCommands = new HashSet<Command>();

            // First, export changed properties
            foreach (var prop in changes.ChangedProperties.Values)
            {
                writer.WriteLine(prop.ToExportString());
                exportedCommands.Add(prop.Command);
            }

            // Then, export original properties that weren't changed or removed
            foreach (var prop in entity.Properties)
            {
                if (exportedCommands.Contains(prop.Command))
                    continue; // Already exported as changed

                if (changes.RemovedProperties.Contains(prop.Command))
                    continue; // Removed, don't export

                writer.WriteLine(prop.ToExportString());
            }

            writer.WriteLine("#end");
            writer.WriteLine();
        }

        /// <summary>
        /// Exports all new entities.
        /// </summary>
        private void ExportNewEntities(StreamWriter writer)
        {
            var newEntities = _changes.GetAllNewEntities().ToList();
            if (!newEntities.Any()) return;

            writer.WriteLine("-- New entities");
            writer.WriteLine();

            foreach (var entity in newEntities)
            {
                entity.Export(writer);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Gets the #select* command for an entity type.
        /// </summary>
        private static string GetSelectCommand(EntityType type)
        {
            return type switch
            {
                EntityType.MONSTER => "#selectmonster",
                EntityType.WEAPON => "#selectweapon",
                EntityType.ARMOR => "#selectarmor",
                EntityType.SPELL => "#selectspell",
                EntityType.ITEM => "#selectitem",
                EntityType.SITE => "#selectsite",
                EntityType.NATION => "#selectnation",
                EntityType.NAMETYPE => "#selectnametype",
                EntityType.POPTYPE => "#selectpoptype",
                EntityType.EVENT => "#selectevent",
                _ => null
            };
        }

        /// <summary>
        /// Gets the #new* command for an entity type.
        /// </summary>
        private static string GetNewCommand(EntityType type)
        {
            return type switch
            {
                EntityType.MONSTER => "#newmonster",
                EntityType.WEAPON => "#newweapon",
                EntityType.ARMOR => "#newarmor",
                EntityType.SPELL => "#newspell",
                EntityType.ITEM => "#newitem",
                EntityType.SITE => "#newsite",
                EntityType.NATION => "#newnation",
                EntityType.MERCENARY => "#newmerc",
                EntityType.EVENT => "#newevent",
                _ => null
            };
        }
    }
}
