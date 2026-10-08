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
        /// Callback invoked, with a line number, where the game reads the file differently than its
        /// lines suggest (a text whose closing quote is missing, "--" inside a text).
        /// </summary>
        public Action<int, string> OnNote { get; set; }

        /// <summary>
        /// Whether the game skips the text of a command ("#msg") in the block being read: its text,
        /// when the closing quote is missing, takes in the command lines up to the next quote
        /// (GameData.GameReading).
        /// </summary>
        public Func<string, bool> SkipsText { get; set; }

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
        /// the name after it is a note. The game reads the string up to its closing quote and
        /// ignores the rest of the line (Confluence: #copyspell "Stellar Strike" (Works as long as
        /// you don't change name)): that rest becomes part of the comment.
        /// </summary>
        private string Unquote(string value, ref string comment)
        {
            LineWasTrimmed = value.StartsWith("\"");
            if (!LineWasTrimmed)
                return value;
            int close = value.IndexOf('"', 1);
            if (close > 0 && close < value.Length - 1)
            {
                var after = value.Substring(close + 1).Trim();
                if (after.Length > 0)
                    comment = string.IsNullOrEmpty(comment) ? after : after + " -- " + comment;
                return value.Substring(1, close - 1);
            }
            return value.Trim('\"');
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
            // the multi-line text's command, whether the game skips its text, its first line, and
            // the command lines the game reads as part of it
            string textCommand = "";
            bool textSkips = false;
            int textStart = 0;
            var swallowed = new List<string>();
            LineNumber = 0;

            while ((s = sr.ReadLine()) != null)
            {
                LineNumber++;
                string raw = s; // as in the file
                s = s.Trim(); //remove whitespaces
                s = s.Replace('\t', ' '); // (the game reads every tab as a space, in texts too)
                if (s.Length < 1)
                {
                    // a blank line inside a multi-line string is a paragraph break, keep it
                    if (isMultiLine)
                    {
                        prevLine = prevLine + NewLine + raw; // (as written: a line of only spaces or tabs keeps them)
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

                if (!isMultiLine && s[0] == '#' && CodeOf(s).IndexOf('"') != -1)
                {
                    //could be a multi-line string (#descr, #details, #msg, #description, ...)
                    //check if has both quotes
                    // (a quote in a comment isn't one: "#req_targitem 872 -- Convergence Sphere\"")
                    string code = CodeOf(s);
                    int firstQuote = code.IndexOf('"');
                    int secondQuote = code.IndexOf('"', firstQuote + 1);
                    string command = FirstWord(s);
                    int dashes = s.IndexOf(commentDelimiter, firstQuote + 1);
                    if (dashes != -1 && (secondQuote == -1 || dashes < secondQuote))
                        OnNote?.Invoke(LineNumber, $"{command}'s text has \"--\" in it: the game drops the rest of the line from the text"
                            + (secondQuote != -1 ? ", its closing quote too, so the text runs on to the next quote in the file" : ""));
                    //first quote mark exists, second does not
                    //either is multi-line, or quote mark forgotten on the end
                    if (firstQuote != -1 && secondQuote == -1)
                    {
                        bool hasAnotherCommand = HasCommandOnLine(s.Substring(firstQuote)); //only check after the first quote
                        // the game reads a text to the next quote, across lines; after a command
                        // that skips its text (#msg, #name, ...) the commands in it are text too
                        bool skips = SkipsText?.Invoke(command) == true;
                        if (!hasAnotherCommand || skips)
                        {
                            isMultiLine = true;
                            textCommand = command;
                            textSkips = skips;
                            textStart = LineNumber;
                            swallowed.Clear();
                            if (hasAnotherCommand)
                                swallowed.Add(s.Substring(firstQuote + GetNextCommandIndex(s.Substring(firstQuote))));
                            prevLine = raw.TrimStart(); // keep trailing spaces: they're part of the text
                            rawPrev = raw;
                            continue;
                        } //if it has another command on that line, the quote was just forgotten
                        string after = s.Substring(firstQuote + 1);
                        int next = GetNextCommandIndex(after);
                        NoteForgottenQuote(command, LineNumber, next == -1 ? after : after.Substring(0, next));
                    }
                    _wholeLine = raw;
                    ProcessStringToLine(s);
                    _wholeLine = null;
                }
                else if (isMultiLine && !string.IsNullOrEmpty(prevLine) && textSkips)
                {
                    // a text the game skips: everything up to the next quote is text, command lines too
                    int quote = raw.IndexOf('"');
                    if (quote == -1)
                    {
                        if (HasCommandOnLine(s) && GetNextCommandIndex(s) == 0)
                            swallowed.Add(s);
                        prevLine = prevLine + NewLine + raw;
                        rawPrev = rawPrev + NewLine + raw;
                        continue;
                    }
                    string before = s.Substring(0, s.IndexOf('"')).Trim();
                    if (before.Length > 0 && HasCommandOnLine(before) && GetNextCommandIndex(before) == 0)
                        swallowed.Add(before + " \"");
                    string text = prevLine + NewLine + raw.Substring(0, quote + 1);
                    // what follows the closing quote on its line is read as usual
                    string rest = raw.Substring(quote + 1).Replace('\t', ' ').Trim();
                    if (rest.Length > 0 && CommandIndexes(rest).Count > 0 && HasCommandOnLine(rest.Substring(GetNextCommandIndex(rest))))
                    {
                        _sharedLine = (++_lineGroups, rawPrev + NewLine + raw, 1 + CommandIndexes(rest).Count);
                        ProcessStringToLine(text);
                        ProcessStringToLine(rest);
                        _sharedLine = null;
                    }
                    else
                    {
                        // (anything else after the quote isn't read: kept in the line as written)
                        _wholeLine = rawPrev + NewLine + raw;
                        ProcessStringToLine(rest.StartsWith(commentDelimiter) ? text + " " + rest : text);
                        _wholeLine = null;
                    }
                    if (swallowed.Count > 0)
                        OnNote?.Invoke(textStart, $"{textCommand} has no closing quote on its line: the game reads lines {textStart}-{LineNumber} as its text, "
                            + $"so {(swallowed.Count == 1 ? "this command in them isn't" : "these commands in them aren't")} read: {string.Join("; ", swallowed.Take(6).Select(c => Shorten(c)))}"
                            + (swallowed.Count > 6 ? $" and {swallowed.Count - 6} more" : ""));
                    prevLine = "";
                    isMultiLine = false;
                    textSkips = false;
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
                            NoteForgottenQuote(textCommand, textStart, prevLine.Substring(prevLine.IndexOf('"') + 1));
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
            if (isMultiLine && !string.IsNullOrEmpty(prevLine))
            {
                // a text whose closing quote never comes: the game reads it to the end of the file
                OnNote?.Invoke(textStart, $"{textCommand} has no closing quote: the game reads the rest of the file, from line {textStart}, as its text");
                _wholeLine = rawPrev;
                ProcessStringToLine(prevLine + "\""); // (closed here only to read it as one text; saved as read)
                _wholeLine = null;
            }
            LineWasTrimmed = false;
            LineNumber = -1;
        }

        /// <summary>A line's first word ("#msg" of "#msg "text...").</summary>
        private static string FirstWord(string s)
        {
            int end = 0;
            while (end < s.Length && s[end] != ' ' && s[end] != '"')
                end++;
            return s.Substring(0, end);
        }

        private static string Shorten(string s) => s.Length > 50 ? s.Substring(0, 50) + "…" : s;

        // texts the game reads through (the commands in them are read too); an item's #name is one
        private static readonly HashSet<string> Descriptions = new() { "#descr", "#details", "#portent", "#cure", "#summary", "#brief", "#name", "#description" };

        /// <summary>
        /// The note for a quote missing at the end of its line, before more commands: a description
        /// runs on to the next quote (the commands after it are still read); a name to look up
        /// (#weapon "Net") must close on its line, so it reads nothing.
        /// </summary>
        private void NoteForgottenQuote(string command, int line, string textAfterQuote)
        {
            if (Descriptions.Contains(command))
                OnNote?.Invoke(line, $"{command} has no closing quote on its line: the game's text runs on to the next quote in the file, so it also shows the lines after line {line} (their commands are still read)");
            else if (textAfterQuote.Trim().Length > 0) // (a lone stray quote changes nothing)
                OnNote?.Invoke(line, $"{command} has no closing quote on its line: the game reads nothing for it (a quoted name must close on its line)");
        }

        /// <summary>A line without its comment (the game drops "--" to the end of the line; one inside closed quotes is kept here as text).</summary>
        private string CodeOf(string s)
        {
            int comment = CommentIndex(s);
            return comment == -1 ? s : s.Substring(0, comment);
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

        /// <summary>
        /// Where a line's comment starts: the first "--" outside closed quotes ("#descr "Hathrans --
        /// an elite class..."" is all text), or -1.
        /// </summary>
        private int CommentIndex(string s)
        {
            int i = s.IndexOf(commentDelimiter);
            if (i == -1 || s.IndexOf('"') == -1)
                return i;
            var quoted = QuotedSpans(s);
            while (i != -1 && quoted.Any(q => q.Start < i && i < q.End))
                i = s.IndexOf(commentDelimiter, i + 1);
            return i;
        }

        /// <summary>The next '#' from <paramref name="from"/>, or -1 (also when it's past the end).</summary>
        private static int NextHash(string s, int from) => from < s.Length ? s.IndexOf('#', from) : -1;

        /// <summary>Where the commands of a line start ('#' outside comments, message tags and closed quotes).</summary>
        private List<int> CommandIndexes(string s)
        {
            int commentIndex = CommentIndex(s);
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
                    var span = quoted.FirstOrDefault(q => q.Start < nextIndex && nextIndex < q.End, (Start: -1, End: -1));
                    if (span.Start != -1 && !ReadThrough(s, commandIndexes, span.Start, nextIndex))
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
        /// Whether the '#' at <paramref name="at"/>, inside quotes opened at <paramref name="quote"/>,
        /// starts a command the game reads: the game reads on through a description's text (#descr,
        /// #details, ...), so a command name there is a command (and the text, which runs to its
        /// closing quote, shows it too); a #msg's or #name's text is skipped.
        /// </summary>
        private bool ReadThrough(string s, List<int> commandIndexes, int quote, int at)
        {
            int owner = commandIndexes.LastOrDefault(i => i < quote, -1);
            if (owner == -1)
                return false;
            string ownerName = FirstWord(s.Substring(owner));
            // (the mod's own #description has its own reader, and a command there is outside any block)
            if (!Descriptions.Contains(ownerName) || ownerName == "#description" || SkipsText?.Invoke(ownerName) == true)
                return false;
            int end = at + 1;
            while (end < s.Length && (char.IsLetterOrDigit(s[end]) || s[end] == '_'))
                end++;
            string name = s.Substring(at, end - at);
            if (!CommandsMap.TryGetCommand(name, out _))
                return false;
            OnNote?.Invoke(LineNumber, $"{ownerName}'s text has {name} in it: the game reads it as a command too (and shows it in the text)");
            return true;
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
                var quoted = commandIndexes.Count > 1 ? QuotedSpans(s) : null;
                for (int i = 0; i < commandIndexes.Count; i++)
                {
                    int nextCommand = i + 1;
                    string line;
                    if (nextCommand < commandIndexes.Count)
                    {
                        line = s.Substring(commandIndexes[i], commandIndexes[nextCommand] - commandIndexes[i]);
                        // a description whose quotes hold a command the game reads too keeps its
                        // whole text, to its closing quote, as the game does (#descr "#descr "...
                        // is the text "#descr ", not an empty one, which would stop the game)
                        var own = quoted!.FirstOrDefault(q => q.Start > commandIndexes[i] && q.Start < commandIndexes[nextCommand] && q.End > commandIndexes[nextCommand], (Start: -1, End: -1));
                        if (own.Start != -1)
                            line = s.Substring(commandIndexes[i], own.End + 1 - commandIndexes[i]);
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
            int commentIndex = CommentIndex(s);
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
                        // (a text without its closing quote is text to the end: "self-inflicted")
                        if (secondQuoteIndex != -1 && singleDash > secondQuoteIndex)
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
                value = Unquote(line.Substring(spaceIndex + 1).Trim(), ref comment);
            }
            else if (tabIndex != -1)
            {
                command = line.Substring(0, tabIndex).Trim();
                value = Unquote(line.Substring(tabIndex + 1).Trim(), ref comment);
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
