namespace Dom5Edit.Validation
{
    /// <summary>
    /// Surfaces parse-time issues (duplicates, invalid commands, etc.) as validation issues.
    /// </summary>
    public class ParseIssueValidator : IValidator
    {
        public string Name => "Parse Issue Validator";

        public IEnumerable<ValidationIssue> Validate(Mod mod)
        {
            foreach (var parseIssue in mod.ParseIssues)
            {
                yield return new ValidationIssue
                {
                    Severity = GetSeverity(parseIssue.IssueType),
                    Message = parseIssue.Message,
                    LineNumber = parseIssue.LineNumber,
                    Category = GetCategory(parseIssue.IssueType)
                };
            }
        }

        private static ValidationSeverity GetSeverity(ParseIssueType issueType)
        {
            return issueType switch
            {
                ParseIssueType.InvalidCommand => ValidationSeverity.Warning,
                ParseIssueType.DuplicateId => ValidationSeverity.Error,
                ParseIssueType.DuplicateName => ValidationSeverity.Warning,
                ParseIssueType.IdRangeExceeded => ValidationSeverity.Warning,
                ParseIssueType.Error => ValidationSeverity.Error,
                ParseIssueType.Warning => ValidationSeverity.Warning,
                _ => ValidationSeverity.Info
            };
        }

        private static string GetCategory(ParseIssueType issueType)
        {
            return issueType switch
            {
                ParseIssueType.InvalidCommand => "Invalid Command",
                ParseIssueType.DuplicateId => "Duplicate ID",
                ParseIssueType.DuplicateName => "Duplicate Name",
                ParseIssueType.IdRangeExceeded => "ID Range",
                _ => "Parse Issue"
            };
        }
    }
}
