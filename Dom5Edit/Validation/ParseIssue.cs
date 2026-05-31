namespace Dom5Edit.Validation
{
    /// <summary>
    /// Represents an issue detected during mod parsing.
    /// </summary>
    public class ParseIssue
    {
        /// <summary>
        /// The line number where the issue occurred.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The type/category of parse issue.
        /// </summary>
        public ParseIssueType IssueType { get; set; }

        /// <summary>
        /// Human-readable description of the issue.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The raw line content that caused the issue, if available.
        /// </summary>
        public string RawContent { get; set; }

        public ParseIssue() { }

        public ParseIssue(int lineNumber, ParseIssueType issueType, string message)
        {
            LineNumber = lineNumber;
            IssueType = issueType;
            Message = message;
        }

        public override string ToString()
        {
            return $"Line {LineNumber}: [{IssueType}] {Message}";
        }
    }

    /// <summary>
    /// Categories of parse issues.
    /// </summary>
    public enum ParseIssueType
    {
        /// <summary>Unknown or invalid command.</summary>
        InvalidCommand,

        /// <summary>Duplicate entity ID.</summary>
        DuplicateId,

        /// <summary>Duplicate entity name.</summary>
        DuplicateName,

        /// <summary>Entity ID exceeds allowed range.</summary>
        IdRangeExceeded,

        /// <summary>General parse warning.</summary>
        Warning,

        /// <summary>General parse error.</summary>
        Error,

        /// <summary>Properties were cleared by a subsequent clear command in the same entity.</summary>
        PropertiesClearedBySubsequentClear
    }
}
