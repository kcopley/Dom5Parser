using Dom5Edit.Entities;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// Validates that there are no duplicate entity IDs within the same entity type.
    /// </summary>
    public class DuplicateIdValidator : IValidator
    {
        public string Name => "Duplicate ID Validator";

        public IEnumerable<ValidationIssue> Validate(Mod mod)
        {
            var issues = new List<ValidationIssue>();

            foreach (var kvp in mod.Database)
            {
                var entityType = kvp.Key;
                var entitySet = kvp.Value;

                var seenIds = new Dictionary<int, IDEntity>();

                foreach (var entity in entitySet.GetFullList())
                {
                    if (entity is IDEntity idEntity && idEntity.ID > 0)
                    {
                        if (seenIds.TryGetValue(idEntity.ID, out var existingEntity))
                        {
                            // Found a duplicate
                            var existingName = existingEntity.Name ?? $"ID {existingEntity.ID}";
                            var currentName = idEntity.Name ?? $"ID {idEntity.ID}";

                            issues.Add(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Error,
                                Message = $"Duplicate {entityType} ID {idEntity.ID}: '{currentName}' conflicts with '{existingName}'",
                                Entity = entity,
                                Category = "Duplicate ID"
                            });
                        }
                        else
                        {
                            seenIds[idEntity.ID] = idEntity;
                        }
                    }
                }
            }

            return issues;
        }
    }
}
