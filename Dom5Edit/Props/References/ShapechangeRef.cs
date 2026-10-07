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
            // a merge writes the shape's number (an unnumbered new monster has none: kept by name)
            if (MonsterRef != null && Parent?.ParentMod?.KeepReferenceForms == false && MonsterRef.Entity?.ID > 0)
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
