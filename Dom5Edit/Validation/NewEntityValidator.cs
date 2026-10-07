using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// What a new entity needs that nothing gives it: a new monster's picture (#spr1, or #copyspr;
    /// #copystats copies stats, abilities, weapons and armor, not the sprite: tools/dom6exe). Easy to
    /// miss when a unit is made as a copy of another; DomEnhanced 2.13's 2,382 new monsters all set one.
    /// </summary>
    public class NewEntityValidator : IValidator
    {
        public string Name => "New Entity Validator";

        public IEnumerable<ValidationIssue> Validate(Mod mod)
        {
            if (!mod.Database.TryGetValue(EntityType.MONSTER, out var monsters))
                yield break;
            foreach (var e in monsters.GetFullList())
            {
                if (e.Selected || e.ID <= 0 || e.Properties.Any(p => p.Command == Command.SPR1 || p.Command == Command.COPYSPR))
                    continue;
                var name = e.Properties.OfType<StringProperty>().LastOrDefault(p => p.Command == Command.NAME)?.Value;
                yield return new ValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Message = $"{(string.IsNullOrEmpty(name) ? "This new monster" : name)} has no sprite (#spr1, or #copyspr): the game has no picture to show for it"
                              + (e.Properties.Any(p => p.Command == Command.COPYSTATS) ? " (#copystats doesn't copy the sprite)" : ""),
                    Entity = e,
                    Category = "Sprites",
                };
            }
        }
    }
}
