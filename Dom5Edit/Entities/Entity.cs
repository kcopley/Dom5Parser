using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public abstract class Entity
    {
        //private string _startComment;
        private string _endComment;
        public Mod Parent { get; set; }
        public List<Entity> ConnectedEntities = new List<Entity>();

        public virtual void SetEndComment(string s)
        {
            _endComment = s;
        }

        public abstract void AddNamed(string s);
        public abstract bool TryGetNamedValue(string s, out IDEntity e);
        public abstract bool TryGetIDValue(int id, out IDEntity e);

        public virtual void SetStartComment(string s)
        {
            _endComment = s;
        }

        public abstract void Parse(Command command, string value, string comment);

        public abstract void Export(StreamWriter writer);
    }
}
