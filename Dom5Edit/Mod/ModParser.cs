using Dom5Edit.Commands;

namespace Dom5Edit
{
    /// <summary>
    /// Handles parsing of .dm mod files into commands.
    /// Preserves the exact parsing behavior from the original Mod class.
    /// </summary>
    public class ModParser
    {
        private readonly char spaceDelimiter = ' ';
        private readonly string tabDelimiter = "\t";
        private readonly string commentDelimiter = "--";

        /// <summary>
        /// Represents a parsed command from a .dm file.
        /// </summary>
        public struct ParsedCommand
        {
            public Command Command;
            public string Value;
            public string Comment;
            public int LineNumber;
        }

        /// <summary>
        /// Callback invoked for each parsed command.
        /// </summary>
        public Action<ParsedCommand> OnCommand { get; set; }

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
            LineNumber = 0;

            while ((s = sr.ReadLine()) != null)
            {
                LineNumber++;
                s = s.Trim(); //remove whitespaces
                s = s.Replace('\t', ' ');
                if (s.Length < 1) continue; //empty line

                //mod information data
                int ind = s.IndexOf("#dependency", StringComparison.OrdinalIgnoreCase);

                if (ind != -1)
                {
                    continue; //skip these lines, grabbed above
                }

                if ((s.IndexOf("#descr") != -1 && s.Length > 6) || (s.IndexOf("#summary") != -1 && s.Length > 8) || (s.IndexOf("#msg") != -1 && s.Length > 4))
                {
                    //could be multi line description
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
                            prevLine = s;
                            continue;
                        } //if it has another command on that line, the quote was just forgotten
                    }
                    ProcessStringToLine(s);
                }
                else if (isMultiLine && !string.IsNullOrEmpty(prevLine))
                {
                    //already on a multi-line, does it continue?
                    int endQuote = s.IndexOf('"');
                    bool anotherCommand = HasCommandOnLine(s);

                    if (endQuote != -1 && !anotherCommand) //ends on this line
                    {
                        string endLine = prevLine + Environment.NewLine + s;
                        ProcessStringToLine(endLine);
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
                        string multiline = prevLine + Environment.NewLine + leftsplit;
                        ProcessStringToLine(multiline);
                        ProcessStringToLine(rightsplit);
                        prevLine = "";
                        isMultiLine = false;
                    }
                    else
                    {
                        //no command, no end quote... it must continue as part of the string
                        prevLine = prevLine + Environment.NewLine + s;
                    }
                }
                else
                {
                    ProcessStringToLine(s);
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
        /// Gets the index of the next command, skipping ##placeholder## patterns.
        /// </summary>
        public int GetNextCommandIndex(string s)
        {
            int nextIndex = s.IndexOf('#');
            while (nextIndex != -1)
            {
                if (s.IndexOf("##fullgodname##", nextIndex) == nextIndex) { nextIndex += 15; }
                else if (s.IndexOf("##godname##", nextIndex) == nextIndex) { nextIndex += 11; }
                else if (s.IndexOf("##disname##", nextIndex) == nextIndex) { nextIndex += 11; }
                else if (s.IndexOf("##fullplayername##", nextIndex) == nextIndex) { nextIndex += 18; }
                else if (s.IndexOf("##playername##", nextIndex) == nextIndex) { nextIndex += 14; }
                else if (s.IndexOf("##playergodname##", nextIndex) == nextIndex) { nextIndex += 17; }
                else if (s.IndexOf("##fullplayergodname##", nextIndex) == nextIndex) { nextIndex += 21; }
                else if (s.IndexOf("##godhe##", nextIndex) == nextIndex) { nextIndex += 9; }
                else if (s.IndexOf("##dishe##", nextIndex) == nextIndex) { nextIndex += 9; }
                else if (s.IndexOf("##godhis##", nextIndex) == nextIndex) { nextIndex += 10; }
                else if (s.IndexOf("##dishis##", nextIndex) == nextIndex) { nextIndex += 10; }
                else if (s.IndexOf("##godhim##", nextIndex) == nextIndex) { nextIndex += 10; }
                else if (s.IndexOf("##dishim##", nextIndex) == nextIndex) { nextIndex += 10; }
                else if (s.IndexOf("##godhimself##", nextIndex) == nextIndex) { nextIndex += 14; }
                else if (s.IndexOf("##dishimself##", nextIndex) == nextIndex) { nextIndex += 14; }
                else if (s.IndexOf("##godthrone##", nextIndex) == nextIndex) { nextIndex += 13; }
                else if (s.IndexOf("##playerthrone##", nextIndex) == nextIndex) { nextIndex += 16; }
                else if (s.IndexOf("##playergodthrone##", nextIndex) == nextIndex) { nextIndex += 19; }
                else if (s.IndexOf("##godnat##", nextIndex) == nextIndex) { nextIndex += 10; }
                else if (s.IndexOf("##disnat##", nextIndex) == nextIndex) { nextIndex += 10; }
                else if (s.IndexOf("##landname##", nextIndex) == nextIndex) { nextIndex += 12; }
                else if (s.IndexOf("##goddisname##", nextIndex) == nextIndex) { nextIndex += 14; }
                else if (s.IndexOf("##targname##", nextIndex) == nextIndex) { nextIndex += 12; }
                else if (s.IndexOf("##fulltargname##", nextIndex) == nextIndex) { nextIndex += 16; }
                else if (s.IndexOf("##targhis##", nextIndex) == nextIndex) { nextIndex += 11; }
                else if (s.IndexOf("##natname##", nextIndex) == nextIndex) { nextIndex += 11; }
                else if (s.IndexOf("##profname##", nextIndex) == nextIndex) { nextIndex += 12; }
                else
                {
                    return nextIndex;
                }
                nextIndex = s.IndexOf('#', nextIndex + 1);
            }
            return -1;
        }

        /// <summary>
        /// Processes a string that may contain multiple commands on one line.
        /// </summary>
        public void ProcessStringToLine(string s)
        {
            // continue on
            int commentIndex = s.IndexOf(commentDelimiter);

            //is there another command on the same line?
            List<int> commandIndexes = new List<int>();
            int index = s.IndexOf('#');
            if (index != -1 && (index < commentIndex || commentIndex == -1))
            {
                commandIndexes.Add(index);
                int nextIndex = s.IndexOf('#', index + 1);
                while (nextIndex != -1 && (nextIndex < commentIndex || commentIndex == -1))
                {
                    if (s.IndexOf("##fullgodname##", nextIndex) == nextIndex) { nextIndex += 15; }
                    else if (s.IndexOf("##godname##", nextIndex) == nextIndex) { nextIndex += 11; }
                    else if (s.IndexOf("##disname##", nextIndex) == nextIndex) { nextIndex += 11; }
                    else if (s.IndexOf("##fullplayername##", nextIndex) == nextIndex) { nextIndex += 18; }
                    else if (s.IndexOf("##playername##", nextIndex) == nextIndex) { nextIndex += 14; }
                    else if (s.IndexOf("##playergodname##", nextIndex) == nextIndex) { nextIndex += 17; }
                    else if (s.IndexOf("##fullplayergodname##", nextIndex) == nextIndex) { nextIndex += 21; }
                    else if (s.IndexOf("##godhe##", nextIndex) == nextIndex) { nextIndex += 9; }
                    else if (s.IndexOf("##dishe##", nextIndex) == nextIndex) { nextIndex += 9; }
                    else if (s.IndexOf("##godhis##", nextIndex) == nextIndex) { nextIndex += 10; }
                    else if (s.IndexOf("##dishis##", nextIndex) == nextIndex) { nextIndex += 10; }
                    else if (s.IndexOf("##godhim##", nextIndex) == nextIndex) { nextIndex += 10; }
                    else if (s.IndexOf("##dishim##", nextIndex) == nextIndex) { nextIndex += 10; }
                    else if (s.IndexOf("##godhimself##", nextIndex) == nextIndex) { nextIndex += 14; }
                    else if (s.IndexOf("##dishimself##", nextIndex) == nextIndex) { nextIndex += 14; }
                    else if (s.IndexOf("##godthrone##", nextIndex) == nextIndex) { nextIndex += 13; }
                    else if (s.IndexOf("##playerthrone##", nextIndex) == nextIndex) { nextIndex += 16; }
                    else if (s.IndexOf("##playergodthrone##", nextIndex) == nextIndex) { nextIndex += 19; }
                    else if (s.IndexOf("##godnat##", nextIndex) == nextIndex) { nextIndex += 10; }
                    else if (s.IndexOf("##disnat##", nextIndex) == nextIndex) { nextIndex += 10; }
                    else if (s.IndexOf("##landname##", nextIndex) == nextIndex) { nextIndex += 12; }
                    else if (s.IndexOf("##goddisname##", nextIndex) == nextIndex) { nextIndex += 14; }
                    else if (s.IndexOf("##targname##", nextIndex) == nextIndex) { nextIndex += 12; }
                    else if (s.IndexOf("##fulltargname##", nextIndex) == nextIndex) { nextIndex += 16; }
                    else if (s.IndexOf("##targhis##", nextIndex) == nextIndex) { nextIndex += 11; }
                    else if (s.IndexOf("##natname##", nextIndex) == nextIndex) { nextIndex += 11; }
                    else if (s.IndexOf("##profname##", nextIndex) == nextIndex) { nextIndex += 12; }
                    else
                    {
                        commandIndexes.Add(nextIndex);
                    }
                    nextIndex = s.IndexOf('#', nextIndex + 1);
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
                value = line.Substring(spaceIndex + 1).Trim();
                if (value.StartsWith("\"") || value.EndsWith("\""))
                {
                    value = value.Trim('\"');
                    this.LineWasTrimmed = true;
                }
                else
                {
                    LineWasTrimmed = false;
                }
            }
            else if (tabIndex != -1)
            {
                command = line.Substring(0, tabIndex).Trim();
                value = line.Substring(tabIndex + 1).Trim();
                if (value.StartsWith("\"") || value.EndsWith("\""))
                {
                    value = value.Trim('\"');
                    this.LineWasTrimmed = true;
                }
                else
                {
                    LineWasTrimmed = false;
                }
            }

            if (CommandsMap.TryGetCommand(command, out Command c))
            {
                OnCommand?.Invoke(new ParsedCommand
                {
                    Command = c,
                    Value = value,
                    Comment = comment,
                    LineNumber = LineNumber
                });
            }
            else
            {
                OnLog?.Invoke(LineNumber, $"Invalid or unknown command: {command}");
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
