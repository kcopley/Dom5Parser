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
    }
}
