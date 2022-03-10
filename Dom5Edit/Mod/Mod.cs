using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Mods
{
    public class Mod
    {
        private readonly char spaceDelimiter = ' ';
        private readonly string commentDelimiter = "--";

        public string ModName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Version { get; set; }

        public Dictionary<int, Monster> Monsters;

        private Entity _currentEntity = null;

        public Mod()
        {
            Monsters = new Dictionary<int, Monster>();
        }

        public void Parse(string dmFile)
        {
            using (StreamReader sr = File.OpenText(dmFile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Length < 1) continue; //empty line
                                                //pull comments first
                    int commentIndex = s.IndexOf(commentDelimiter);
                    string line = s;
                    string comment = ""; //set to empty string, not null
                    if (commentIndex != -1) //has a comment
                    {
                        line = s.Substring(0, commentIndex).Trim();
                        comment = s.Substring(commentIndex + 2).Trim();
                    }

                    //grab the command & value

                    int spaceIndex = line.IndexOf(spaceDelimiter);
                    string command = line;
                    string value = ""; //set to empty string, not null
                    if (spaceIndex != -1) //has a value (but could be spaces before a comment? should be handled by trim above)
                    {
                        command = line.Substring(0, spaceIndex).Trim();
                        value = line.Substring(spaceIndex + 1).Trim();
                    }

                    if (CommandsMap.TryGetCommand(command, out Command c))
                    {
                        Parse(c, value, comment);
                    }
                    //else unrecognizable command
                }
            }
        }

        /*
         * Entity processing goes here
         */
        public void Parse(Command c, string val, string comment)
        {
            switch (c)
            {
                case Command.NEWMONSTER:
                    _currentEntity = NewMonster(val, comment);
                    break;
                case Command.SELECTMONSTER:
                    _currentEntity = SelectMonster(val, comment);
                    break;
                case Command.END:
                    _currentEntity?.SetEndComment(comment);
                    _currentEntity = null;
                    break;
                default:
                    if (_currentEntity != null) _currentEntity.Parse(c, val, comment); //assume the command is relevant for the current entity
                    //else build list of pre-entity comments, to restore before the next entity on export?
                    break; //nothing
            }
        }

        public Monster NewMonster(string val, string comment)
        {
            Monster m = new Monster(val, comment);
            Monsters.Add(m.ID, m);
            return m;
        }

        public Monster SelectMonster(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Monsters.TryGetValue(id, out Monster m))
            {
                return m;
            }
            else
            {
                return NewMonster(val, comment);
            }
        }
    }
}
