using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class SpellRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new SpellRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.SPELL;
        }

        public override string ToExportString()
        {
            if ((Command == Command.NEXTSPELL) && Entity != null && Entity.ID != -1)
                IsStringRef = false;
            return base.ToExportString();
        }
    }
}
