using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class NameRef : Reference
    {
        public string Name { get; set; }
        public bool HasValue { get; set; }

        public IDEntity entity { get; set; }
        public bool Resolved { get; set; }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            HasValue = s.Length > 0;
            if (HasValue)
            {
                Name = s;
            }
        }

        //Preliminary Example only for now, not optimal
        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                string _exportName = Name;
                if (Resolved && entity.TryGetName(out var _name)) _exportName = _name;

                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        return s + " \"" + _exportName + "\" -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " \"" + _exportName + "\"";
                }
            }
            else return "";
        }
    }
}
