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
                            // Found a duplicate - describe how each was defined
                            var existingDesc = DescribeEntity(existingEntity);
                            var currentDesc = DescribeEntity(idEntity);

                            issues.Add(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Error,
                                Message = $"Duplicate {entityType} ID {idEntity.ID}: {currentDesc} conflicts with {existingDesc}",
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

        private static string DescribeEntity(IDEntity entity)
        {
            var action = entity.Selected ? "selected" : "created";

            if (entity.Named)
            {
                // Selected/created by name - show the name used
                return $"{action} by name \"{entity._name}\"";
            }
            else if (entity.ID > 0)
            {
                // Selected/created by ID
                var displayName = !string.IsNullOrEmpty(entity.Name) ? $" ({entity.Name})" : "";
                return $"{action} by ID {entity.ID}{displayName}";
            }
            else
            {
                // Auto-assigned ID
                return $"{action} with auto-assigned ID";
            }
        }
    }
}
