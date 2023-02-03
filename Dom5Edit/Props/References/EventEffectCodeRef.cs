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
        DependentEntity _item = null;
        bool HasEventEffectCodeRef { get; set; }

        public static Property Create()
        {
            return new EventEffectCodeRef();
        }

        public List<IDEntity> GetConnectedEntities()
        {
            return _item.ReferencedEntities;
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

        public override void Resolve()
        {
        }

        public override void Parse(Command c, string s, string comment)
        {
            base.Parse(c, s, comment);
            _item = this.Parent.ParentMod.AddDependent(EntityType.EVENT_CODE_EFFECT, ID);
            _item.ReferencedEntities.Add(this.Parent as IDEntity);
            HasEventEffectCodeRef = _item != null;
        }

        public override string ToExportString()
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

        internal override EntityType GetEntityType()
        {
            return EntityType.EVENT_CODE_EFFECT;
        }
    }
}
