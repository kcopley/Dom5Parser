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
    public class NametypeViewModel : IDViewModelBase
    {
        public NametypeViewModel(ModViewModel mod, Nametype nametype) : base(mod, nametype)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAMETYPE
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Nametype._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any nametype-specific attribute infos here
        }

        public Nametype Nametype { get { return _entity as Nametype; } }

        public void SetNametype(Nametype n)
        {
            SetEntity(n);
        }
    }
}
