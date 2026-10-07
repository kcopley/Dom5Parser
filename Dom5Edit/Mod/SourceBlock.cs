using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// One #new.../#select... to #end block as it appeared in the parsed file: the entity it
    /// edits and the properties it parsed, in order. An entity built from several blocks (a
    /// #newmonster and later #selectmonster blocks) has several. Saving writes the blocks back in
    /// file order, so copies, clears and forward references mean what they meant in the file.
    /// </summary>
    public class SourceBlock
    {
        public IDEntity Entity { get; }

        /// <summary>True for a #select... header, false for #new....</summary>
        public bool Selected { get; }

        /// <summary>The header's argument as written (an ID, a quoted name's text, or empty).</summary>
        public string Header { get; }

        public string HeaderComment { get; }

        /// <summary>The header line as read, and the entity's ID then (the header is rewritten if it changed).</summary>
        public string? RawHeader { get; set; }
        public int IdAtParse { get; set; }

        /// <summary>The header's line number in the file (0 for a block not read from one).</summary>
        public int HeaderLine { get; set; }

        /// <summary>The #end line as read.</summary>
        public string? RawEnd { get; set; }

        /// <summary>
        /// The block had no #end: the next #new/#select (or the end of the file) closed it, which
        /// the game accepts. A save that keeps the text as read leaves it without one too.
        /// </summary>
        public bool EndsWithoutEnd { get; set; }

        /// <summary>Lines with no command before the header (comments, blank lines), as read.</summary>
        public List<string> LeadingTrivia { get; } = new List<string>();

        /// <summary>Lines with no command inside the block: (number of properties before it, text).</summary>
        public List<(int Before, string Text)> Trivia { get; } = new List<(int, string)>();

        /// <summary>
        /// Every property the block parsed, in order, including ones a later clear or copy took
        /// out of the entity's live list (the game still reads them in this order).
        /// </summary>
        public List<Property> Properties { get; } = new List<Property>();

        public SourceBlock(IDEntity entity, bool selected, string header, string headerComment)
        {
            Entity = entity;
            Selected = selected;
            Header = header ?? "";
            HeaderComment = headerComment ?? "";
        }
    }
}
