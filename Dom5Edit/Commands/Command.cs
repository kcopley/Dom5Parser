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
        HP,
        SPR1,
        SPR2,
        DARKVISION,
        FIRERES,
        MOUNTAINSURVIVAL,
        MR,
        MOR,
        STR,
        ATT,
        DEF,
        PREC,
        AP,
        MAPMOVE,
        ENC,
        SIZE,
        MAXAGE,
        HUMANOID,
        ITEMSLOTS,
        GCOST,
        RCOST,
        RPCOST,
        NAMETYPE
    }

    public class CommandsMap
    {
        private static Dictionary<string, Command> _commandMap = new Dictionary<string, Command>();
        private static Dictionary<Command, string> _stringMap = new Dictionary<Command, string>();

        static CommandsMap()
        {
            _commandMap.Add("#newmonster", Command.NEWMONSTER);
            _commandMap.Add("#selectmonster", Command.SELECTMONSTER);
            _commandMap.Add("#name", Command.NAME);
            _commandMap.Add("#end", Command.END);
            _commandMap.Add("#hp", Command.HP);
            _commandMap.Add("#spr1", Command.SPR1);
            _commandMap.Add("#spr2", Command.SPR2);
            _commandMap.Add("#darkvision", Command.DARKVISION);
            _commandMap.Add("#fireres", Command.FIRERES);
            _commandMap.Add("#mountainsurvival", Command.MOUNTAINSURVIVAL);
            _commandMap.Add("#mr", Command.MR);
            _commandMap.Add("#mor", Command.MOR);
            _commandMap.Add("#str", Command.STR);
            _commandMap.Add("#att", Command.ATT);
            _commandMap.Add("#def", Command.DEF);
            _commandMap.Add("#prec", Command.PREC);
            _commandMap.Add("#ap", Command.AP);
            _commandMap.Add("#mapmove", Command.MAPMOVE);
            _commandMap.Add("#enc", Command.ENC);
            _commandMap.Add("#size", Command.SIZE);
            _commandMap.Add("#maxage", Command.MAXAGE);
            _commandMap.Add("#humanoid", Command.HUMANOID);
            _commandMap.Add("#itemslots", Command.ITEMSLOTS);
            _commandMap.Add("#gcost", Command.GCOST);
            _commandMap.Add("#rcost", Command.RCOST);
            _commandMap.Add("#rpcost", Command.RPCOST);
            _commandMap.Add("#nametype", Command.NAMETYPE);

            //Build an inverted map
            foreach (KeyValuePair<string, Command> kvp in _commandMap)
            {
                _stringMap.Add(kvp.Value, kvp.Key);
            }
        }

        public static bool TryGetCommand(string s, out Command c)
        {
            return _commandMap.TryGetValue(s, out c);
        }

        public static bool TryGetString(Command c, out string s)
        {
            return _stringMap.TryGetValue(c, out s);
        }
    }
}
