using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MontagIDRef : IDRef
    {
        DependentEntity _montag = null;
        bool HasMontagID { get; set; }

        public static Property Create()
        {
            return new MontagIDRef();
        }

        public override void Resolve()
        {
        }

        public override void Parse(Command c, string s, string comment)
        {
            base.Parse(c, s, comment);
            if (c != Command.MONTAG) ID = -ID;
            _montag = this.Parent.ParentMod.AddDependent(EntityType.MONTAG, ID);
            _montag.ReferencedEntities.Add(this.Parent as IDEntity);
            HasMontagID = _montag != null;
        }

        public List<IDEntity> GetConnectedEntities()
        {
            return _montag.ReferencedEntities;
        }

        public override void Connect(IDEntity original)
        {
            var list = GetConnectedEntities();
            foreach (var entity in list)
            {
                entity.UsedByEntities.Add(original);
                original.RequiredEntities.Add(entity);
            }
        }

        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                int _exportID = _montag != null ? _montag.GetID() : ID; //true is left, false is right

                if (_command != Command.MONTAG)
                {
                    _exportID = -_exportID;
                }

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

        internal override EntityType GetEntityType()
        {
            return EntityType.MONTAG;
        }
    }
}
