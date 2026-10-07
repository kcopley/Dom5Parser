using Dom5Edit.Entities;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// Validates that entity IDs are within allowed modding ranges.
    /// </summary>
    public class IdRangeValidator : IValidator
    {
        public string Name => "ID Range Validator";

        public IEnumerable<ValidationIssue> Validate(Mod mod)
        {
            var issues = new List<ValidationIssue>();

            foreach (var kvp in mod.Database)
            {
                var entityType = kvp.Key;
                var entitySet = kvp.Value;

                var startId = entitySet.START_ID;
                var endId = entitySet.END_ID;

                // Skip entity types without defined ranges
                if (startId == 0 && endId == 0) continue;

                var fullList = entitySet.GetFullList();

                foreach (var entity in fullList)
                {
                    if (entity is IDEntity idEntity && !idEntity.IsVanilla)
                    {
                        // Check if ID is below the start range (could conflict with vanilla)
                        // Only warn for NEW entities - selected entities are intentionally modifying vanilla
                        // Note: During parsing, vanilla isn't loaded as a dependency yet, so #select commands
                        // on vanilla IDs create new entities with Selected=true. We skip those.
                        if (idEntity.ID > 0 && idEntity.ID < startId && !idEntity.Selected)
                        {
                            // a game entity with that number is replaced by the new one; else it's free
                            // today, but the game's own numbers grow into that range with updates
                            var kind = entityType.ToString().ToLowerInvariant();
                            IDEntity? game = null;
                            bool clashes = VanillaLoader.Vanilla?.Database.TryGetValue(entityType, out var vanillaSet) == true
                                           && vanillaSet.TryGetValue(idEntity.ID, out game);
                            issues.Add(new ValidationIssue
                            {
                                Severity = clashes ? ValidationSeverity.Error : ValidationSeverity.Warning,
                                Message = clashes
                                    ? $"#new{kind} {idEntity.ID} takes the number of the game's own {kind} {game!.Name} #{idEntity.ID}: it replaces that {kind}. New {kind}s are numbered from {startId}."
                                    : $"#new{kind} {idEntity.ID} is below the modding range (from {startId}, the manual). No game {kind} uses {idEntity.ID} today, but the game's own numbers grow into that range with updates.",
                                Entity = entity,
                                Category = "ID Range"
                            });
                        }

                        // Check if ID exceeds the end range
                        if (endId > 0 && idEntity.ID > endId)
                        {
                            issues.Add(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Error,
                                Message = $"{entityType} ID {idEntity.ID} exceeds maximum allowed ({endId}).",
                                Entity = entity,
                                Category = "ID Range"
                            });
                        }
                    }
                }
            }

            return issues;
        }
    }
}
