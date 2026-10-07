using Dom5Edit.Commands;

namespace Dom5Edit
{
    /// <summary>
    /// Handles parsing of .dm mod files into commands.
    /// Preserves the exact parsing behavior from the original Mod class.
    /// </summary>
    public class ModParser
    {
        /// <summary>The file's line break, used where a multi-line string's lines are joined (the file's own, so a save writes them back the same).</summary>
        public string NewLine { get; set; } = Environment.NewLine;

        private readonly char spaceDelimiter = ' ';
        private readonly string tabDelimiter = "\t";
        private readonly string commentDelimiter = "--";
        private const string GameValuePrefix = "-- ro:";

        // the physical line (or multi-line string) being processed, and its text for a command
        // that is the only one on it
        private string? _wholeLine;
        private string? _segmentText;

        /// <summary>
        /// Represents a parsed command from a .dm file.
        /// </summary>
        public struct ParsedCommand
        {
            public Command Command;
            public string Value;
            public string Comment;
            public int LineNumber;
            /// <summary>The command's text as read (its line, or its part of a line with several).</summary>
            public string RawText;
            /// <summary>For a line with several commands: a number shared by them (0 otherwise), the line as read, and how many commands it has.</summary>
            public int LineGroup;
            public string LineText;
            public int LineCommands;
        }

        private int _lineGroups;
        private int _lineGroup;
        private int _lineCommands;
        private string _lineText;
        // set while a multi-line string's end and the commands after it on its last line are read
        private (int Group, string Text, int Commands)? _sharedLine;

        /// <summary>
        /// Callback invoked for each parsed command.
        /// </summary>
        public Action<ParsedCommand> OnCommand { get; set; }

        /// <summary>
        /// Callback invoked with the text of a line that holds no command the parser reads: blank
        /// and comment lines, #dependency lines, unknown commands. Saving writes them back in place.
        /// </summary>
        public Action<string> OnTrivia { get; set; }

        /// <summary>
        /// Callback invoked for a read-only game value line ("-- ro: label = value"), with the
        /// label and value.
        /// </summary>
        public Action<string, string> OnGameValue { get; set; }

        /// <summary>
        /// Callback invoked for logging/errors.
        /// </summary>
        public Action<int, string> OnLog { get; set; }

        /// <summary>
        /// Current line number being parsed.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Whether the last value had quotes trimmed.
        /// </summary>
        public bool LineWasTrimmed { get; private set; }

        /// <summary>
        /// A quoted argument without its quotes (LineWasTrimmed set). Only an argument that starts
        /// with a quote is a string: in #weapon 474 "Golden Sword" the game reads the number, and
        /// the name after it is a note.
        /// </summary>
        private string Unquote(string value)
        {
            LineWasTrimmed = value.StartsWith("\"");
            return LineWasTrimmed ? value.Trim('\"') : value;
        }

        /// <summary>
        /// Parses a .dm file from a file path.
        /// </summary>
        public void Parse(string dmFile)
        {
            using (StreamReader sr = File.OpenText(dmFile))
            {
                Parse(sr);
            }
        }

        /// <summary>
        /// Parses a .dm file from a stream.
        /// </summary>
        public void Parse(StreamReader sr)
        {
            string s = "";
            bool isMultiLine = false;
            string prevLine = "";
            string rawPrev = ""; // the multi-line string's lines as read
            LineNumber = 0;

            while ((s = sr.ReadLine()) != null)
            {
                LineNumber++;
                string raw = s; // as in the file
                s = s.Trim(); //remove whitespaces
                s = s.Replace('\t', ' ');
                if (s.Length < 1)
                {
                    // a blank line inside a multi-line string is a paragraph break, keep it
                    if (isMultiLine)
                    {
                        prevLine = prevLine + NewLine;
                        rawPrev = rawPrev + NewLine + raw;
                    }
                    else OnTrivia?.Invoke(raw);
                    continue;
                }

                // a stored value no command can set, in the exe-written vanilla data
                if (!isMultiLine && s.StartsWith(GameValuePrefix))
                {
                    var body = s.Substring(GameValuePrefix.Length);
                    int eq = body.LastIndexOf(" = ");
                    if (eq != -1)
                        OnGameValue?.Invoke(body.Substring(0, eq).Trim(), body.Substring(eq + 3).Trim());
                    continue;
                }

                //mod information data
                int ind = s.IndexOf("#dependency", StringComparison.OrdinalIgnoreCase);

                if (ind != -1)
                {
                    if (!isMultiLine) OnTrivia?.Invoke(raw);
                    continue; //skip these lines, grabbed above
                }

                if (!isMultiLine && s[0] == '#' && s.IndexOf('"') != -1)
                {
                    //could be a multi-line string (#descr, #details, #msg, #description, ...)
                    //check if has both quotes
                    int firstQuote = s.IndexOf('"');
                    int secondQuote = s.IndexOf('"', firstQuote + 1);
                    //first quote mark exists, second does not
                    //either is multi-line, or quote mark forgotten on the end
                    if (firstQuote != -1 && secondQuote == -1)
                    {
                        bool hasAnotherCommand = HasCommandOnLine(s.Substring(firstQuote)); //only check after the first quote
                        if (!hasAnotherCommand)
                        {
                            isMultiLine = true;
                            prevLine = raw.TrimStart(); // keep trailing spaces: they're part of the text
                            rawPrev = raw;
                            continue;
                        } //if it has another command on that line, the quote was just forgotten
                    }
                    _wholeLine = raw;
                    ProcessStringToLine(s);
                    _wholeLine = null;
                }
                else if (isMultiLine && !string.IsNullOrEmpty(prevLine))
                {
                    //already on a multi-line, does it continue?
                    int endQuote = s.IndexOf('"');
                    bool anotherCommand = HasCommandOnLine(s);

                    if (endQuote != -1 && !anotherCommand) //ends on this line
                    {
                        string endLine = prevLine + NewLine + raw.TrimEnd();
                        _wholeLine = rawPrev + NewLine + raw;
                        ProcessStringToLine(endLine);
                        _wholeLine = null;
                        prevLine = "";
                        isMultiLine = false;
                    }
                    else if (anotherCommand) // of course a mod author would end a multi-line and start another command on the same line
                    {
                        //split and add up to the # to the previous string, process it
                        //and then process the second string
                        int anotherCommandIndex = GetNextCommandIndex(s);
                        string leftsplit = s.Substring(0, anotherCommandIndex);
                        string rightsplit = s.Substring(anotherCommandIndex);
                        if (string.IsNullOrWhiteSpace(leftsplit))
                        {
                            // nothing before the next command: the closing quote was just forgotten.
                            // Both keep their lines as read.
                            _wholeLine = rawPrev;
                            ProcessStringToLine(prevLine.TrimEnd('\r', '\n'));
                            _wholeLine = raw;
                            ProcessStringToLine(rightsplit);
                            _wholeLine = null;
                        }
                        else
                        {
                            // the string ends on this line and more commands follow it: read as one
                            // line group (saved as read while they're unchanged)
                            _sharedLine = (++_lineGroups, rawPrev + NewLine + raw, 1 + CommandIndexes(rightsplit).Count);
                            ProcessStringToLine(prevLine + NewLine + leftsplit);
                            ProcessStringToLine(rightsplit);
                            _sharedLine = null;
                        }
                        prevLine = "";
                        isMultiLine = false;
                    }
                    else
                    {
                        //no command, no end quote... it must continue as part of the string
                        prevLine = prevLine + NewLine + raw;
                        rawPrev = rawPrev + NewLine + raw;
                    }
                }
                else if (s.StartsWith("--") || GetNextCommandIndex(s) == -1)
                {
                    OnTrivia?.Invoke(raw); // a comment, or text with no command
                }
                else
                {
                    _wholeLine = raw;
                    ProcessStringToLine(s);
                    _wholeLine = null;
                }
            }
            LineWasTrimmed = false;
            LineNumber = -1;
        }

        /// <summary>
        /// Checks if a line contains a valid command.
        /// </summary>
        public bool HasCommandOnLine(string s)
        {
            //did they add another command alongside on this line?
            int anotherCommand = GetNextCommandIndex(s);
            if (anotherCommand < 0) return false;
            int spaceAfter = s.IndexOf(' ', anotherCommand);
            bool hasValidCommand = false;

            if (anotherCommand != -1)
            {
                string comm;
                if (spaceAfter != -1)
                {
                    comm = s.Substring(anotherCommand, spaceAfter - anotherCommand);
                }
                else
                {
                    comm = s.Substring(anotherCommand);
                }
                hasValidCommand = CommandsMap.TryGetCommand(comm, out _); //this is a valid command on this line
            }
            return hasValidCommand;
        }

        /// <summary>
        /// Gets the index of the next command, skipping message tags (##landname##, ##godname##, ...).
        /// </summary>
        public int GetNextCommandIndex(string s)
        {
            int nextIndex = s.IndexOf('#');
            while (nextIndex != -1)
            {
                int tag = TagLength(s, nextIndex);
                if (tag == 0)
                    return nextIndex;
                nextIndex = NextHash(s, nextIndex + tag);
            }
            return -1;
        }

        /// <summary>
        /// The length of a message tag at <paramref name="i"/> ("##" letters "##": ##landname##,
        /// ##fulltargname##, ... any the game has), or 0 when it isn't one.
        /// </summary>
        internal static int TagLength(string s, int i)
        {
            if (i + 1 >= s.Length || s[i] != '#' || s[i + 1] != '#')
                return 0;
            int j = i + 2;
            while (j < s.Length && char.IsLetterOrDigit(s[j]))
                j++;
            return j > i + 2 && j + 1 < s.Length && s[j] == '#' && s[j + 1] == '#' ? j + 2 - i : 0;
        }

        /// <summary>The closed quoted spans of a line (pairs of quotes; an unpaired last one isn't a span).</summary>
        private static List<(int Start, int End)> QuotedSpans(string s)
        {
            var spans = new List<(int, int)>();
            int open = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '"')
                    continue;
                if (open < 0)
                    open = i;
                else
                {
                    spans.Add((open, i));
                    open = -1;
                }
            }
            return spans;
        }

        /// <summary>The next '#' from <paramref name="from"/>, or -1 (also when it's past the end).</summary>
        private static int NextHash(string s, int from) => from < s.Length ? s.IndexOf('#', from) : -1;

        /// <summary>Where the commands of a line start ('#' outside comments, message tags and closed quotes).</summary>
        private List<int> CommandIndexes(string s)
        {
            int commentIndex = s.IndexOf(commentDelimiter);
            //is there another command on the same line?
            List<int> commandIndexes = new List<int>();
            int index = s.IndexOf('#');
            if (index != -1 && (index < commentIndex || commentIndex == -1))
            {
                commandIndexes.Add(index);
                int nextIndex = NextHash(s, index + 1);
                var quoted = QuotedSpans(s);
                while (nextIndex != -1 && (nextIndex < commentIndex || commentIndex == -1))
                {
                    // a message tag (##landname##) is text, not a command; so is a # inside a
                    // closed pair of quotes (#descr "#descr "Chaos Spawn ...": the game reads the
                    // text up to the next quote)
                    int tag = TagLength(s, nextIndex);
                    if (tag > 0)
                    {
                        nextIndex = NextHash(s, nextIndex + tag);
                        continue;
                    }
                    if (quoted.Any(q => q.Start < nextIndex && nextIndex < q.End))
                    {
                        nextIndex = NextHash(s, nextIndex + 1);
                        continue;
                    }
                    commandIndexes.Add(nextIndex);
                    nextIndex = NextHash(s, nextIndex + 1);
                }

            }
            return commandIndexes;
        }

        /// <summary>
        /// Processes a string that may contain multiple commands on one line.
        /// </summary>
        public void ProcessStringToLine(string s)
        {
            List<int> commandIndexes = CommandIndexes(s);
            if (commandIndexes.Count > 0)
            {
                // a line with one command keeps its exact text; parts of a line with several, their own,
                // and the line as read (saved as it was while they are)
                _segmentText = commandIndexes.Count == 1 ? _wholeLine : null;
                if (_sharedLine != null)
                {
                    // the end of a multi-line string and more commands on its last line: one line group
                    _lineGroup = _sharedLine.Value.Group;
                    _lineCommands = _sharedLine.Value.Commands;
                    _lineText = _sharedLine.Value.Text;
                }
                else
                {
                    _lineGroup = commandIndexes.Count > 1 && _wholeLine != null ? ++_lineGroups : 0;
                    _lineCommands = commandIndexes.Count;
                    _lineText = _wholeLine;
                }
                for (int i = 0; i < commandIndexes.Count; i++)
                {
                    int nextCommand = i + 1;
                    string line;
                    if (nextCommand < commandIndexes.Count)
                    {
                        line = s.Substring(commandIndexes[i], commandIndexes[nextCommand] - commandIndexes[i]);
                    }
                    else
                    {
                        line = s.Substring(commandIndexes[i]);
                    }
                    ProcessLine(line);
                }
                _segmentText = null;
                _lineGroup = 0;
            }
        }

        /// <summary>
        /// Processes a single command line.
        /// </summary>
        public void ProcessLine(string s)
        {
            string line = s;
            int commentIndex = s.IndexOf(commentDelimiter);
            string comment = ""; //set to empty string, not null

            if (commentIndex == -1) //check for single dash
            {
                int singleDash = s.IndexOf('-');
                //if single dash exists, if the next character exists, and next char is not an integer
                if (singleDash != -1 && s.Length > singleDash + 1 && !int.TryParse(s[singleDash + 1].ToString(), out _))
                {
                    //if it has quotes, it could be a dash in a description
                    int quoteIndex = s.IndexOf('"');
                    if (quoteIndex != -1)
                    {
                        // assume if there's a first quote, check for a second quote mark
                        int secondQuoteIndex = s.IndexOf('"', quoteIndex + 1);
                        // only allow a single dash as a comment if it comes after a second quote mark
                        if (singleDash > secondQuoteIndex)
                        {
                            line = s.Substring(0, singleDash).Trim();
                            comment = s.Substring(singleDash + 1).Trim();
                        }
                    }
                    else //no quote marks either
                    {
                        line = s.Substring(0, singleDash).Trim();
                        comment = s.Substring(singleDash + 1).Trim();
                    }
                }
            }
            else if (commentIndex != -1) //has a comment
            {
                line = s.Substring(0, commentIndex).Trim();
                comment = s.Substring(commentIndex + 2).Trim();
            }

            //grab the command & value

            int spaceIndex = line.IndexOf(spaceDelimiter);
            int tabIndex = line.IndexOf(tabDelimiter);
            string command = line;
            string value = ""; //set to empty string, not null
            if (spaceIndex != -1) //has a value (but could be spaces before a comment? should be handled by trim above)
            {
                command = line.Substring(0, spaceIndex).Trim();
                value = Unquote(line.Substring(spaceIndex + 1).Trim());
            }
            else if (tabIndex != -1)
            {
                command = line.Substring(0, tabIndex).Trim();
                value = Unquote(line.Substring(tabIndex + 1).Trim());
            }

            if (CommandsMap.TryGetCommand(command, out Command c))
            {
                OnCommand?.Invoke(new ParsedCommand
                {
                    Command = c,
                    Value = value,
                    Comment = comment,
                    LineNumber = LineNumber,
                    RawText = _segmentText ?? s.Trim(),
                    LineGroup = _lineGroup,
                    LineText = _lineGroup != 0 ? _lineText : null,
                    LineCommands = _lineCommands,
                });
            }
            else
            {
                OnLog?.Invoke(LineNumber, $"Invalid or unknown command: {command}");
                OnTrivia?.Invoke(_segmentText ?? s.Trim()); // kept as written
            }
        }

        /// <summary>
        /// Scans a file for dependencies without fully parsing it.
        /// </summary>
        public static List<string> ScanDependencies(string dmFile)
        {
            var dependencies = new List<string>();

            using (StreamReader sr = File.OpenText(dmFile))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    s = s.Trim();
                    s = s.Replace('\t', ' ');
                    if (s.Length < 1) continue;

                    int ind = s.IndexOf("#dependency", StringComparison.OrdinalIgnoreCase);
                    if (ind != -1)
                    {
                        ind += 12;
                        string file = s.Substring(ind);
                        if (file.Length > 0)
                        {
                            file = file.Trim();
                            dependencies.Add(file);
                        }
                    }
                }
            }

            return dependencies;
        }
    }
}
