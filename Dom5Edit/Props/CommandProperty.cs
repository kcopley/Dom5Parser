using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dom5Edit.Props
{
    public class CommandProperty : Property
    {
        public string DisplayName
        {
            get { return "#" + this.Command.ToString().ToLower(); }
        }

        public static Property Create()
        {
            return new CommandProperty();
        }

        public static CommandProperty Create(Command c, IDEntity parent)
        {
            return new CommandProperty() { Command = c, Comment = "", Parent = parent };
        }

        public override void Parse(Command c, string s, string comment)
        {
            this.Command = c;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " -- " + Comment;
                }
                else
                {
                    return s;
                }
            }
            else return "";
        }

        internal override Property GetDefault()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(CommandProperty))
            {
                var prop = (obj as CommandProperty);
                return this.Command == prop.Command && this.Parent == prop.Parent;
            }
            else return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Command.GetHashCode() + Parent.GetHashCode();
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is CommandProperty)
            {
                var compare = copyFrom as CommandProperty;
                if (this.Command == compare.Command) return true;
            }
            return false;
        }
    }
}
