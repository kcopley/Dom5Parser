using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class MonsterRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new MonsterRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.MONSTER;
        }
    }
}
