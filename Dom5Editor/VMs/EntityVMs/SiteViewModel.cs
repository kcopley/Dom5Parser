using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    public class SiteViewModel : IDViewModelBase
    {
        public SiteViewModel(ModViewModel mod, Site site) : base(mod, site)
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
            return Site._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any site-specific attribute infos here if needed
        }

        public Site Site { get { return _entity as Site; } }

        public void SetSite(Site s)
        {
            SetEntity(s);
        }

        // Add any site-specific properties or methods here
    }
}
