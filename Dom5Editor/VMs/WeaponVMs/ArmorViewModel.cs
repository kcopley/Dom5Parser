using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    public class ArmorViewModel : IDViewModelBase
    {
        public ArmorViewModel(ModViewModel mod, Armor arm)
        {
            _entity = arm;
            Parent = mod;

            CoreAttributes = new List<Command>() { Command.NAME };
        }

        public void SetArmor(Armor a)
        {
            this._entity = a;
        }

        public Armor Armor { get { return _entity as Armor; } }

        public int ID
        {
            get
            {
                return _entity != null ? _entity.ID : -1;
            }
            set
            {
                if (_entity != null) _entity.ID = value;
            }
        }

        public NameViewModel Name
        {
            get
            {
                return new NameViewModel(_entity, Command.NAME);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (_entity != null)
                {
                    return "(" + _entity.ID + ") " + _entity.Name;
                }
                else
                {
                    return "<No Name>";
                }
            }
        }
    }
}
