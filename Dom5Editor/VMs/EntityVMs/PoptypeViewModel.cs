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
    public class PoptypeViewModel : IDViewModelBase
    {
        public PoptypeViewModel(ModViewModel mod, Poptype poptype) : base(mod, poptype)
        {
            CoreAttributes = new List<Command>()
            {
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Poptype._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any poptype-specific attribute infos here
        }

        public Poptype Poptype { get { return _entity as Poptype; } }

        public void SetPoptype(Poptype p)
        {
            SetEntity(p);
        }
    }
}
