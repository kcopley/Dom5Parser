using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Dom5Editor
{
    public class ArmorViewModel : IDViewModelBase
    {
        public ArmorViewModel(ModViewModel mod, Armor armor) : base(mod, armor)
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
            return Armor._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any armor-specific attribute infos here if needed
        }

        public Armor Armor { get { return _entity as Armor; } }

        public void SetArmor(Armor a)
        {
            SetEntity(a);
        }

        // Add any armor-specific properties or methods here
    }
}