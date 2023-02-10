using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class MonsterOrMontagRef : Reference
    {
        internal MontagIDRef _montagRef;
        internal MonsterRef _monsterRef;


        public static Property Create()
        {
            return new MonsterOrMontagRef();
        }

        public override void Resolve()
        {
            if (_montagRef != null)
            {
                _montagRef.Resolve();
            }
            else if (_monsterRef != null)
            {
                _monsterRef.Resolve();
            }

            if (this.Command == Command.DAMAGEMON)
            {
                if (_monsterRef.Entity != null && _monsterRef.Entity.ID != -1)
                {
                    this.Command = Command.DAMAGE;
                    _monsterRef.Command = Command.DAMAGE;
                }
            }
        }

        public override bool TryGetEntity(out IDEntity e)
        {
            e = null;
            if (_montagRef != null)
            {
                return _montagRef.TryGetEntity(out e);
            }
            else if (_monsterRef != null)
            {
                return _monsterRef.TryGetEntity(out e);
            }
            return false;
        }

        public override void Parse(Command c, string v, string comment)
        {
            bool hasvalue = v.TryRetrieveNumericFromString(out int i, out _);

            this.Command = c;
            if (hasvalue && i < 0)
            {
                _montagRef = new MontagIDRef();
                _montagRef.Parent = this.Parent;
                _montagRef.Parse(c, v, comment);
            }
            else //monster, possibly by name
            {
                _monsterRef = new MonsterRef();
                _monsterRef.Parent = this.Parent;
                _monsterRef.Parse(c, v, comment);
            }
        }

        public override string ToExportString()
        {
            if (_montagRef != null)
            {
                return _montagRef.ToExportString();
            }
            else if (_monsterRef != null)
            {
                return _monsterRef.ToExportString();
            }
            return "";
        }

        internal override EntityType GetEntityType()
        {
            if (_montagRef != null)
            {
                return _montagRef.GetEntityType();
            }
            else if (_monsterRef != null)
            {
                return _monsterRef.GetEntityType();
            }
            return EntityType.MONSTER;
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is MonsterOrMontagRef)
            {
                var compare = copyFrom as MonsterOrMontagRef;
                if (this.Command == compare.Command && (this._monsterRef.EqualsProperty<MonsterRef>(compare._monsterRef) || this._montagRef.EqualsProperty<MontagIDRef>(compare._montagRef))) return true;
            }
            return false;
        }
    }
}
