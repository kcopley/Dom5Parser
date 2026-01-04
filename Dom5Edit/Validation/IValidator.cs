using System.Collections.Generic;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// Interface for validators that check mods for issues.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Name of this validator for display purposes.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Validates a mod and returns any issues found.
        /// </summary>
        /// <param name="mod">The mod to validate.</param>
        /// <returns>Collection of validation issues found.</returns>
        IEnumerable<ValidationIssue> Validate(Mod mod);
    }
}
