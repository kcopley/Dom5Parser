using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Commands
{
    public enum Command
    {
        NEWMONSTER,
        SELECTMONSTER,
        NAME,
        END,
    }

    public class CommandsMap
    {
        private static Dictionary<string, Command> _commandMap = new Dictionary<string, Command>();

        static CommandsMap()
        {
            _commandMap.Add("#newmonster", Command.NEWMONSTER);
            _commandMap.Add("#selectmonster", Command.SELECTMONSTER);
            _commandMap.Add("#name", Command.NAME);
            _commandMap.Add("#end", Command.END);
        }

        public static bool TryGetCommand(string s, out Command c)
        {
            return _commandMap.TryGetValue(s, out c);
        }
    }
}
