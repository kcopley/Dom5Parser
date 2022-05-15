using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class EventEffectCodeRef : IDRef
    {
        EventEffectCode _item = null;
        bool HasEventEffectCodeRef { get; set; }

        public static Property Create()
        {
            return new EventEffectCodeRef();
        }

        public override void Resolve()
        {
        }

        public override void Parse(Command c, string s, string comment)
        {
            base.Parse(c, s, comment);
            _item = this.Parent.Parent.AddEventEffectCode(ID);
            HasEventEffectCodeRef = _item != null;
        }

        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                int _exportID = _item != null ? _item.GetID() : ID; //true is left, false is right

                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        return s + " " + _exportID + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + _exportID;
                }
            }
            else return "";
        }
    }
}
