using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Editor.VMs
{
    public class ArmorRefViewModel : PropertyViewModel
    {
        private ArmorViewModel _selectedID;
        public ArmorRefViewModel(ModViewModel parent, ViewModelBase owner, IDEntity e, ArmorRef p) : base(e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
            InitializeSelectedID();
        }
        public ArmorRefViewModel(ModViewModel parent, ViewModelBase owner, string label, IDEntity e, ArmorRef p) : base(label, e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
            InitializeSelectedID();
        }

        internal ModViewModel _parentMod;
        internal ViewModelBase _owner;
        internal ArmorRef _property;

        private void InitializeSelectedID()
        {
            try
            {
                if (_property.TryGetEntity(out var entity))
                {
                    _selectedID = _parentMod.Armors.FirstOrDefault(x => x.ID == entity.ID);
                    if (_selectedID == null)
                    {
                        Console.WriteLine($"Warning: No matching armor found for ID {entity.ID}");
                    }
                }
                else
                {
                    Console.WriteLine("Warning: Failed to get entity from property");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SelectedID: {ex.Message}");
            }
        }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out StringOrIDRef ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Command.ToString();
                    case ReturnType.TRUE:
                        return ip.Command.ToString();
                }
                return "";
            }
            set
            {
                Source.Set<StringOrIDRef>(Command, id => id.SetEntity(value));
                OnPropertyChanged(Command.ToString());
            }
        }

        public List<ArmorViewModel> IDs
        {
            get { return _parentMod.Armors; }
        }

        public ArmorViewModel SelectedID
        {
            get { return _selectedID; }
            set
            {
                if (value != _selectedID)
                {
                    _selectedID = value;
                    if (_selectedID != null)
                    {
                        _property.Entity = _selectedID.Armor;
                        OnPropertyChanged(nameof(Value));
                    }
                    OnPropertyChanged(nameof(SelectedID));
                    _parentMod.OnPropertyChanged("");
                    _owner.OnPropertyChanged("");
                }
            }
        }
    }
}
