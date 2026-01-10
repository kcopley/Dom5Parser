using Dom5Edit.Commands;
using Dom5Edit.Props;

namespace Dom5Edit.Entities
{
    /// <summary>
    /// Represents an AI pretender design template for a specific nation.
    /// Templates define how the AI designs its god, including form, magic paths,
    /// dominion strength, scales, blesses, and research priorities.
    /// </summary>
    public class Template : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Template()
        {
            // Pretender configuration
            _propertyMap.Add(Command.FORM, StringProperty.Create);      // Physical form of pretender
            _propertyMap.Add(Command.PRISON, IntProperty.Create);       // 0=awake, 1=dormant, 2=imprisoned
            _propertyMap.Add(Command.MAGIC, IntIntProperty.Create);     // <path> <level> - start magic
            _propertyMap.Add(Command.DOMSTR, IntProperty.Create);       // Dominion strength 1-10
            _propertyMap.Add(Command.SCALE, IntIntProperty.Create);     // <scale nbr> <value> - scales 0-5, value -5 to 5
            _propertyMap.Add(Command.BLESS, StringProperty.Create);     // Bless effect name (can have multiple)

            // AI research priorities
            _propertyMap.Add(Command.RESEARCHGOAL, StringProperty.Create);  // Spell/item name for AI research
            _propertyMap.Add(Command.FAVRIT, StringProperty.Create);        // <disschool> <level> "ritual/item name"
        }

        /// <summary>
        /// The nation ID this template is for.
        /// Set via #newtemplate <nation nbr>.
        /// </summary>
        public int NationId
        {
            get => ID;
            set => ID = value;
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWTEMPLATE;
        }

        internal override Command GetSelectCommand()
        {
            // Templates don't have a select command - only newtemplate
            return Command.NEWTEMPLATE;
        }

        public override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.TEMPLATE;
        }

        public override bool TryGetCopyFrom(out IDEntity copy)
        {
            // Templates can't be copied
            copy = null;
            return false;
        }

        public override string GetName()
        {
            // Display as "Template for Nation #X"
            return $"Nation #{NationId} Template";
        }
    }
}
