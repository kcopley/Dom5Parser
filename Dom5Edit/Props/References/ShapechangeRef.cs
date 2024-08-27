using Dom5Edit.Commands;

namespace Dom5Edit.Props
{
    public class ShapechangeRef : MonsterOrMontagRef
    {
        public new static Property Create()
        {
            return new ShapechangeRef();
        }

        public override void Resolve()
        {
            base.Resolve();
            if (_monsterRef != null)
            {
                _monsterRef.IsStringRef = false;
            }
        }

        public override void Parse(Command c, string v, string comment)
        {
            base.Parse(c, v, comment);
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
    }
}
