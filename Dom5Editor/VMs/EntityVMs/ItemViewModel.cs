using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;

namespace Dom5Editor
{
    public class ItemViewModel : IDViewModelBase
    {
        public ItemViewModel(ModViewModel mod, Item item) : base(mod, item)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAME
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Item._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any item-specific attribute infos here if needed
        }

        public Item Item { get { return _entity as Item; } }

        public void SetItem(Item i)
        {
            SetEntity(i);
        }

        public DescriptionViewModel Description
        {
            get { return new DescriptionViewModel(_entity, Command.DESCR); }
        }

        // Add any item-specific properties or methods here
    }
}