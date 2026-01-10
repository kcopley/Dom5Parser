using Dom5Edit.Commands;
using Dom5Edit.Props;

namespace Dom5Edit.Entities
{
    public class Bless : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Bless()
        {
            // Basic attributes
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.PATH0, IntProperty.Create);
            _propertyMap.Add(Command.COST0, IntProperty.Create);
            _propertyMap.Add(Command.PATH1, IntProperty.Create);
            _propertyMap.Add(Command.COST1, IntProperty.Create);

            // Scale requirements
            _propertyMap.Add(Command.CLEARSCALES, CommandProperty.Create);
            _propertyMap.Add(Command.ORDERSCALE, IntProperty.Create);
            _propertyMap.Add(Command.PRODSCALE, IntProperty.Create);
            _propertyMap.Add(Command.HEATSCALE, IntProperty.Create);
            _propertyMap.Add(Command.GROWTHSCALE, IntProperty.Create);
            _propertyMap.Add(Command.LUCKSCALE, IntProperty.Create);
            _propertyMap.Add(Command.MAGICSCALE, IntProperty.Create);
            _propertyMap.Add(Command.CHAOSSCALE, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHSCALE, IntProperty.Create);
            _propertyMap.Add(Command.COLDSCALE, IntProperty.Create);
            _propertyMap.Add(Command.DEATHSCALE, IntProperty.Create);
            _propertyMap.Add(Command.MISFORTSCALE, IntProperty.Create);
            _propertyMap.Add(Command.DRAINSCALE, IntProperty.Create);

            // Effects
            _propertyMap.Add(Command.CLEARFX, CommandProperty.Create);
        }

        internal override Command GetNewCommand()
        {
            // Blesses are select-only, no #newbless command exists
            return Command.SELECTBLESS;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTBLESS;
        }

        public override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.BLESS;
        }

        public override bool TryGetCopyFrom(out IDEntity copy)
        {
            // Blesses can't be copied
            copy = null;
            return false;
        }
    }
}
