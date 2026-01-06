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
            if (MonsterRef != null)
            {
                MonsterRef.IsStringRef = false;
            }
        }

        public override void Parse(Command c, string v, string comment)
        {
            base.Parse(c, v, comment);
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
    }
}
