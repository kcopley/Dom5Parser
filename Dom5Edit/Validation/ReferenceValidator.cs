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

        // Dependent entity types are stored in Dependents, not Database
        // They are identifiers, not full entities with ID ranges to validate
        private static readonly HashSet<EntityType> DependentEntityTypes = new HashSet<EntityType>
        {
            EntityType.MONTAG,
            EntityType.RESTRICTED_ITEM,
            EntityType.ENCHANTMENT,
            EntityType.EVENT_CODE,
            EntityType.EVENT_CODE_EFFECT,
            EntityType.EVENT_VAR
        };

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
                    foreach (var issue in ValidateSingleReference(reference, entity, mod, prop))
                    {
                        yield return issue;
                    }
                }
            }
        }

        private IEnumerable<ValidationIssue> ValidateSingleReference(Reference reference, IDEntity entity, Mod mod, Property prop)
        {
            var entityType = reference.GetEntityType();

            // Skip validation for dependent entity types (montags, enchantments, etc.)
            // These are identifiers, not entities with standard ID ranges
            if (DependentEntityTypes.Contains(entityType))
            {
                yield break;
            }

            // Handle wrapper types like MonsterOrMontagRef
            if (reference is MonsterOrMontagRef monsterOrMontag)
            {
                if (monsterOrMontag.MonsterRef != null)
                {
                    foreach (var issue in ValidateSingleReference(monsterOrMontag.MonsterRef, entity, mod, prop))
                    {
                        yield return issue;
                    }
                }
                // MontagRefs are dependent types, skip them
                yield break;
            }

            // Handle SpellDamage wrapper (contains MonsterOrMontagRef for summons)
            if (reference is SpellDamage spellDamage)
            {
                var unresolvedId = spellDamage.GetUnresolvedId();
                if (unresolvedId > 0)
                {
                    var spellEntityType = spellDamage.GetEntityType();
                    var startId = mod.GetStartID(spellEntityType);
                    if (unresolvedId >= startId)
                    {
                        yield return new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = $"Unresolved reference to {spellEntityType} ID {unresolvedId}",
                            Entity = entity,
                            Property = prop,
                            LineNumber = prop.LineNumber,
                            Category = "Reference"
                        };
                    }
                }
                yield break;
            }

            // Try to get the referenced entity
            if (!reference.TryGetEntity(out var referencedEntity))
            {
                // Reference is unresolved - could be vanilla or missing
                // Only report if it's not a vanilla reference
                if (reference is StringOrIDRef stringOrIdRef && stringOrIdRef.ID > 0)
                {
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
                            LineNumber = prop.LineNumber,
                            Category = "Reference"
                        };
                    }
                }
                else if (reference is IDRef idRef && idRef.ID > 0)
                {
                    var startId = mod.GetStartID(entityType);

                    if (idRef.ID >= startId)
                    {
                        yield return new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = $"Unresolved reference to {entityType} ID {idRef.ID}",
                            Entity = entity,
                            Property = prop,
                            LineNumber = prop.LineNumber,
                            Category = "Reference"
                        };
                    }
                }
            }
        }
    }
}
