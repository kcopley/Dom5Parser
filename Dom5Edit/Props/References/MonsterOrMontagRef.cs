using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class MonsterOrMontagRef : Reference
    {
        public MontagIDRef MontagRef;
        public MonsterRef MonsterRef;


        public static Property Create()
        {
            return new MonsterOrMontagRef();
        }

        public override void Resolve()
        {
            if (MontagRef != null)
            {
                MontagRef.Resolve();
            }
            else if (MonsterRef != null)
            {
                MonsterRef.Resolve();
            }

            if (this.Command == Command.DAMAGEMON)
            {
                if (MonsterRef.Entity != null && MonsterRef.Entity.ID != -1)
                {
                    this.Command = Command.DAMAGE;
                    MonsterRef.Command = Command.DAMAGE;
                }
            }
        }

        public override bool TryGetEntity(out IDEntity e)
        {
            e = null;
            if (MontagRef != null)
            {
                return MontagRef.TryGetEntity(out e);
            }
            else if (MonsterRef != null)
            {
                return MonsterRef.TryGetEntity(out e);
            }
            return false;
        }

        public bool TrySetEntity(IDEntity e)
        {
            MontagRef = null;
            MonsterRef.Entity = e;
            return true;
        }

        public override void Parse(Command c, string v, string comment)
        {
            bool hasvalue = v.TryRetrieveNumericFromString(out int i, out _);

            this.Command = c;
            if (hasvalue && i < 0)
            {
                MontagRef = new MontagIDRef();
                MontagRef.Parent = this.Parent;
                MontagRef.Parse(c, v, comment);
            }
            else //monster, possibly by name
            {
                MonsterRef = new MonsterRef();
                MonsterRef.Parent = this.Parent;
                MonsterRef.Parse(c, v, comment);
            }
        }

        public override string ToExportString()
        {
            if (MontagRef != null)
            {
                return MontagRef.ToExportString();
            }
            else if (MonsterRef != null)
            {
                return MonsterRef.ToExportString();
            }
            return "";
        }

        internal override EntityType GetEntityType()
        {
            if (MontagRef != null)
            {
                return MontagRef.GetEntityType();
            }
            else if (MonsterRef != null)
            {
                return MonsterRef.GetEntityType();
            }
            return EntityType.MONSTER;
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is MonsterOrMontagRef)
            {
                var compare = copyFrom as MonsterOrMontagRef;
                if (this.Command == compare.Command && (this.MonsterRef.EqualsProperty<MonsterRef>(compare.MonsterRef) || this.MontagRef.EqualsProperty<MontagIDRef>(compare.MontagRef))) return true;
            }
            return false;
        }
    }
}
