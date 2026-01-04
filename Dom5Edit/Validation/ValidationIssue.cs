using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// Severity level of a validation issue.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>Informational message, not a problem.</summary>
        Info,
        /// <summary>Potential issue that may cause problems.</summary>
        Warning,
        /// <summary>Definite problem that will cause errors.</summary>
        Error
    }

    /// <summary>
    /// Represents a single validation issue found in a mod.
    /// </summary>
    public class ValidationIssue
    {
        /// <summary>
        /// Severity of the issue.
        /// </summary>
        public ValidationSeverity Severity { get; set; }

        /// <summary>
        /// Human-readable description of the issue.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The entity that has the issue, if applicable.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// The specific property with the issue, if applicable.
        /// </summary>
        public Property Property { get; set; }

        /// <summary>
        /// Line number in the source file, if known.
        /// </summary>
        public int? LineNumber { get; set; }

        /// <summary>
        /// Category of the issue for grouping/filtering.
        /// </summary>
        public string Category { get; set; }

        public ValidationIssue() { }

        public ValidationIssue(ValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public override string ToString()
        {
            var location = "";
            if (Entity != null)
            {
                location = $"[{Entity.GetType().Name}";
                if (Entity is IDEntity idEntity)
                    location += $" {idEntity.ID}";
                location += "] ";
            }
            if (LineNumber.HasValue)
                location = $"Line {LineNumber}: " + location;

            return $"{Severity}: {location}{Message}";
        }
    }
}
