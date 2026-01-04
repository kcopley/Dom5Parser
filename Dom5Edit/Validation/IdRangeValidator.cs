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

                foreach (var entity in entitySet.GetFullList())
                {
                    if (entity is IDEntity idEntity && !idEntity.IsVanilla)
                    {
                        // Check if ID is below the start range (could conflict with vanilla)
                        if (idEntity.ID > 0 && idEntity.ID < startId)
                        {
                            issues.Add(new ValidationIssue
                            {
                                Severity = ValidationSeverity.Warning,
                                Message = $"{entityType} ID {idEntity.ID} is below modding range (starts at {startId}). May conflict with vanilla entities.",
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
