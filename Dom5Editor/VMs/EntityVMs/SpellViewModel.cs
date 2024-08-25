using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;

namespace Dom5Editor
{
    public class SpellViewModel : IDViewModelBase
    {
        public SpellViewModel(ModViewModel mod, Spell spell) : base(mod, spell)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAME,
                Command.DESCR,
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Spell._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any spell-specific attribute infos here if needed
        }

        public Spell Spell { get { return _entity as Spell; } }

        public DescriptionViewModel Description
        {
            get { return new DescriptionViewModel(_entity, Command.DESCR); }
        }

        public void SetSpell(Spell s)
        {
            SetEntity(s);
        }

        // Add any spell-specific properties or methods here
    }
}