using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// Validates that all entity references point to existing entities.
    /// </summary>
    public class ReferenceValidator : IValidator
    {
        public string Name => "Reference Validator";

        public IEnumerable<ValidationIssue> Validate(Mod mod)
        {
            var issues = new List<ValidationIssue>();

            // Check all entities with properties that reference other entities
            foreach (var entitySet in mod.Database.Values)
            {
                foreach (var entity in entitySet.GetFullList())
                {
                    if (entity is IDEntity idEntity)
                    {
                        foreach (var issue in ValidateEntityReferences(idEntity, mod))
                        {
                            issues.Add(issue);
                        }
                    }
                }
            }

            return issues;
        }

        private IEnumerable<ValidationIssue> ValidateEntityReferences(IDEntity entity, Mod mod)
        {
            foreach (var prop in entity.Properties)
            {
                if (prop is Reference reference)
                {
                    // Try to get the referenced entity
                    if (!reference.TryGetEntity(out var referencedEntity))
                    {
                        // Reference is unresolved - could be vanilla or missing
                        // Only report if it's not a vanilla reference
                        if (reference is StringOrIDRef stringOrIdRef && stringOrIdRef.ID > 0)
                        {
                            var entityType = reference.GetEntityType();
                            var startId = mod.GetStartID(entityType);

                            // Only warn if the ID is in the modding range but not found
                            if (stringOrIdRef.ID >= startId)
                            {
                                yield return new ValidationIssue
                                {
                                    Severity = ValidationSeverity.Warning,
                                    Message = $"Unresolved reference to {entityType} ID {stringOrIdRef.ID}",
                                    Entity = entity,
                                    Property = prop,
                                    Category = "Reference"
                                };
                            }
                        }
                        else if (reference is IDRef idRef && idRef.ID > 0)
                        {
                            var entityType = reference.GetEntityType();
                            var startId = mod.GetStartID(entityType);

                            if (idRef.ID >= startId)
                            {
                                yield return new ValidationIssue
                                {
                                    Severity = ValidationSeverity.Warning,
                                    Message = $"Unresolved reference to {entityType} ID {idRef.ID}",
                                    Entity = entity,
                                    Property = prop,
                                    Category = "Reference"
                                };
                            }
                        }
                    }
                }
            }
        }
    }
}
