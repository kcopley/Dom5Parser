using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public abstract class Reference : Property
    {
        internal abstract EntityType GetEntityType();

        public override void Parse(Command c, string v, string comment)
        {
            throw new NotImplementedException();
        }

        public override string ToExportString()
        {
            throw new NotImplementedException();
        }

        public virtual void Connect(IDEntity original)
        {
            if (TryGetEntity(out IDEntity newEntity))
            {
                newEntity.UsedByEntities.Add(original);
                original.RequiredEntities.Add(newEntity);
            }
        }

        public abstract void Resolve();

        public abstract bool TryGetEntity(out IDEntity e);

        internal override Property GetDefault()
        {
            throw new NotImplementedException();
        }
    }
}
