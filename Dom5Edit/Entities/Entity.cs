using Dom5Edit.Commands;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public enum EntityType
    {
        MONSTER,
        ARMOR,
        WEAPON,
        ITEM,
        SPELL,
        SITE,
        EVENT,
        MERCENARY,
        POPTYPE,
        NATION,
        NAMETYPE,
        MONTAG,
        RESTRICTED_ITEM,
        ENCHANTMENT,
        EVENT_CODE,
        EVENT_CODE_EFFECT
    }

    public enum ReturnType
    {
        TRUE,
        FALSE,
        COPIED
    }

    public abstract class Entity
    {
        //private string _startComment;
        private string _endComment;
        public Mod ParentMod { get; set; }
        public HashSet<IDEntity> RequiredEntities = new HashSet<IDEntity>();
        public HashSet<IDEntity> UsedByEntities = new HashSet<IDEntity>();

        public string DisplayName { get { return GetName(); } }

        public virtual string GetName()
        {
            return "";
        }

        public virtual void SetEndComment(string s)
        {
            _endComment = s;
        }

        public virtual void SetStartComment(string s)
        {
            _endComment = s;
        }

        public abstract void Parse(Command command, string value, string comment);

        public abstract void Export(StreamWriter writer);
    }
}
