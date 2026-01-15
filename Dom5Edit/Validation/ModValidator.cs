using Dom5Edit.Entities;

namespace Dom5Edit.Validation
{
    /// <summary>
    /// Composite validator that runs all registered validators against a mod.
    /// </summary>
    public class ModValidator
    {
        private readonly List<IValidator> _validators;

        /// <summary>
        /// Creates a ModValidator with all default validators enabled.
        /// </summary>
        public ModValidator()
        {
            _validators = new List<IValidator>
            {
                new ReferenceValidator(),
                new IdRangeValidator(),
                new DuplicateIdValidator()
            };
        }

        /// <summary>
        /// Creates a ModValidator with custom validators.
        /// </summary>
        public ModValidator(IEnumerable<IValidator> validators)
        {
            _validators = validators.ToList();
        }

        /// <summary>
        /// Gets the list of validators for customization.
        /// </summary>
        public IList<IValidator> Validators => _validators;

        /// <summary>
        /// Validates a mod using all registered validators.
        /// </summary>
        /// <param name="mod">The mod to validate.</param>
        /// <returns>All validation issues found.</returns>
        public IEnumerable<ValidationIssue> Validate(Mod mod)
        {
            return _validators.SelectMany(v => v.Validate(mod));
        }

        /// <summary>
        /// Validates a mod and returns issues grouped by severity.
        /// </summary>
        public ValidationResult ValidateWithSummary(Mod mod)
        {
            var issues = Validate(mod).ToList();
            return new ValidationResult
            {
                Issues = issues,
                ErrorCount = issues.Count(i => i.Severity == ValidationSeverity.Error),
                WarningCount = issues.Count(i => i.Severity == ValidationSeverity.Warning),
                InfoCount = issues.Count(i => i.Severity == ValidationSeverity.Info)
            };
        }
    }

    /// <summary>
    /// Summary of validation results.
    /// </summary>
    public class ValidationResult
    {
        public IReadOnlyList<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }

        public bool HasErrors => ErrorCount > 0;
        public bool HasWarnings => WarningCount > 0;
        public bool IsValid => !HasErrors;

        public override string ToString()
        {
            if (Issues.Count == 0)
                return "Validation passed with no issues.";

            return $"Validation complete: {ErrorCount} error(s), {WarningCount} warning(s), {InfoCount} info message(s).";
        }

        /// <summary>
        /// Exports validation results to a text file.
        /// </summary>
        /// <param name="filePath">Path to write the report to.</param>
        public void ExportToFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("# Validation Report");
                writer.WriteLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();
                writer.WriteLine($"## Summary");
                writer.WriteLine($"- Errors: {ErrorCount}");
                writer.WriteLine($"- Warnings: {WarningCount}");
                writer.WriteLine($"- Info: {InfoCount}");
                writer.WriteLine($"- Total Issues: {Issues.Count}");
                writer.WriteLine();

                // Group by category
                var byCategory = Issues.GroupBy(i => i.Category ?? "Uncategorized")
                                       .OrderBy(g => g.Key);

                foreach (var group in byCategory)
                {
                    writer.WriteLine($"## {group.Key}");
                    writer.WriteLine();

                    // Group by severity within category
                    var errors = group.Where(i => i.Severity == ValidationSeverity.Error).ToList();
                    var warnings = group.Where(i => i.Severity == ValidationSeverity.Warning).ToList();
                    var infos = group.Where(i => i.Severity == ValidationSeverity.Info).ToList();

                    if (errors.Count > 0)
                    {
                        writer.WriteLine($"### Errors ({errors.Count})");
                        foreach (var issue in errors)
                        {
                            WriteIssue(writer, issue);
                        }
                        writer.WriteLine();
                    }

                    if (warnings.Count > 0)
                    {
                        writer.WriteLine($"### Warnings ({warnings.Count})");
                        foreach (var issue in warnings)
                        {
                            WriteIssue(writer, issue);
                        }
                        writer.WriteLine();
                    }

                    if (infos.Count > 0)
                    {
                        writer.WriteLine($"### Info ({infos.Count})");
                        foreach (var issue in infos)
                        {
                            WriteIssue(writer, issue);
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        private void WriteIssue(StreamWriter writer, ValidationIssue issue)
        {
            var entityInfo = "";
            if (issue.Entity != null)
            {
                var typeName = issue.Entity.GetType().Name;
                if (issue.Entity is IDEntity idEntity)
                {
                    var name = idEntity.Name;
                    entityInfo = !string.IsNullOrEmpty(name)
                        ? $" [{typeName} #{idEntity.ID}: {name}]"
                        : $" [{typeName} #{idEntity.ID}]";
                }
                else
                {
                    entityInfo = $" [{typeName}]";
                }
            }

            writer.WriteLine($"- {issue.Message}{entityInfo}");
        }
    }
}
