using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Weapon : IDEntity
    {
        private static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Weapon()
        {
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
            _propertyMap.Add(Command.COPYWEAPON, WeaponRef.Create);
            _propertyMap.Add(Command.DMG, WeaponDamage.Create);
            _propertyMap.Add(Command.DAMAGE, WeaponDamage.Create);
            _propertyMap.Add(Command.NRATT, IntProperty.Create);
            _propertyMap.Add(Command.ATT, IntProperty.Create);
            _propertyMap.Add(Command.DEF, IntProperty.Create);
            _propertyMap.Add(Command.LEN, IntProperty.Create);
            _propertyMap.Add(Command.TWOHANDED, CommandProperty.Create);
            _propertyMap.Add(Command.SOUND, IntProperty.Create);
            _propertyMap.Add(Command.RANGE, IntProperty.Create);
            _propertyMap.Add(Command.PREC, IntProperty.Create);
            _propertyMap.Add(Command.AMMO, IntProperty.Create);
            _propertyMap.Add(Command.RCOST, IntProperty.Create);
            _propertyMap.Add(Command.SAMPLE, FilePathProperty.Create);
            _propertyMap.Add(Command.NATURAL, CommandProperty.Create);
            _propertyMap.Add(Command.DT_NORMAL, CommandProperty.Create);
            _propertyMap.Add(Command.DT_POISON, CommandProperty.Create);
            _propertyMap.Add(Command.DT_DEMON, CommandProperty.Create);
            _propertyMap.Add(Command.DT_SMALL, CommandProperty.Create);
            _propertyMap.Add(Command.DT_MAGIC, CommandProperty.Create);
            _propertyMap.Add(Command.DT_LARGE, CommandProperty.Create);
            _propertyMap.Add(Command.DT_CONSTRUCTONLY, CommandProperty.Create);
            _propertyMap.Add(Command.DT_RAISE, CommandProperty.Create);
            _propertyMap.Add(Command.DT_CAP, CommandProperty.Create);
            _propertyMap.Add(Command.DT_WEAKNESS, CommandProperty.Create);
            _propertyMap.Add(Command.DT_HOLY, CommandProperty.Create);
            _propertyMap.Add(Command.DT_DRAIN, CommandProperty.Create);
            _propertyMap.Add(Command.DT_SIZESTUN, CommandProperty.Create);
            _propertyMap.Add(Command.DT_WEAPONDRAIN, CommandProperty.Create);
            _propertyMap.Add(Command.DT_STUN, CommandProperty.Create);
            _propertyMap.Add(Command.DT_REALSTUN, CommandProperty.Create);
            _propertyMap.Add(Command.DT_INTERRUPT, CommandProperty.Create);
            _propertyMap.Add(Command.DT_BOUNCEKILL, CommandProperty.Create);
            _propertyMap.Add(Command.DT_PARALYZE, CommandProperty.Create);
            _propertyMap.Add(Command.DT_AFF, CommandProperty.Create);
            _propertyMap.Add(Command.POISON, CommandProperty.Create);
            _propertyMap.Add(Command.ACID, CommandProperty.Create);
            _propertyMap.Add(Command.SLASH, CommandProperty.Create);
            _propertyMap.Add(Command.PIERCE, CommandProperty.Create);
            _propertyMap.Add(Command.BLUNT, CommandProperty.Create);
            _propertyMap.Add(Command.COLD, CommandProperty.Create);
            _propertyMap.Add(Command.FIRE, CommandProperty.Create);
            _propertyMap.Add(Command.SHOCK, CommandProperty.Create);
            _propertyMap.Add(Command.MAGIC, CommandProperty.Create);
            _propertyMap.Add(Command.ARMORPIERCING, CommandProperty.Create);
            _propertyMap.Add(Command.ARMORNEGATING, CommandProperty.Create);
            _propertyMap.Add(Command.NOSTR, CommandProperty.Create);
            _propertyMap.Add(Command.BOWSTR, CommandProperty.Create);
            _propertyMap.Add(Command.HALFSTR, CommandProperty.Create);
            _propertyMap.Add(Command.FULLSTR, CommandProperty.Create);
            _propertyMap.Add(Command.MRNEGATES, CommandProperty.Create);
            _propertyMap.Add(Command.MRNEGATESEASILY, CommandProperty.Create);
            _propertyMap.Add(Command.HARDMRNEG, CommandProperty.Create);
            _propertyMap.Add(Command.SIZERESIST, CommandProperty.Create);
            _propertyMap.Add(Command.MIND, CommandProperty.Create);
            _propertyMap.Add(Command.UNDEADIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.INANIMATEIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.FLYINGIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.ENEMYIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.FRIENDLYIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.UNDEADONLY, CommandProperty.Create);
            _propertyMap.Add(Command.SACREDONLY, CommandProperty.Create);
            _propertyMap.Add(Command.DEMONONLY, CommandProperty.Create);
            _propertyMap.Add(Command.DEMONUNDEAD, CommandProperty.Create);
            _propertyMap.Add(Command.INTERNAL, CommandProperty.Create);
            _propertyMap.Add(Command.AOE, IntProperty.Create);
            _propertyMap.Add(Command.BONUS, CommandProperty.Create);
            _propertyMap.Add(Command.SECONDARYEFFECT, WeaponRef.Create);
            _propertyMap.Add(Command.SECONDARYEFFECTALWAYS, WeaponRef.Create);
            _propertyMap.Add(Command.IRONWEAPON, CommandProperty.Create);
            _propertyMap.Add(Command.WOODENWEAPON, CommandProperty.Create);
            _propertyMap.Add(Command.ICEWEAPON, CommandProperty.Create);
            _propertyMap.Add(Command.CHARGE, CommandProperty.Create);
            _propertyMap.Add(Command.FLAIL, CommandProperty.Create);
            _propertyMap.Add(Command.NOREPEL, CommandProperty.Create);
            _propertyMap.Add(Command.UNREPEL, CommandProperty.Create);
            _propertyMap.Add(Command.BEAM, CommandProperty.Create);
            _propertyMap.Add(Command.RANGE050, CommandProperty.Create);
            _propertyMap.Add(Command.RANGE0, CommandProperty.Create);
            _propertyMap.Add(Command.MELEE50, CommandProperty.Create);
            _propertyMap.Add(Command.SKIP, CommandProperty.Create);
            _propertyMap.Add(Command.SKIP2, CommandProperty.Create);
            _propertyMap.Add(Command.EXPLSPR, IntProperty.Create);
            _propertyMap.Add(Command.FLYSPR, FlySprProperty.Create);
            _propertyMap.Add(Command.UWOK, CommandProperty.Create);
            _propertyMap.Add(Command.NOUW, CommandProperty.Create);
            _propertyMap.Add(Command.SOULSLAYING, CommandProperty.Create);
        }

        public Weapon(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        public override void Resolve()
        {
            if (base._resolved) return;
            foreach (var m in Parent.Dependencies)
            {
                if (ID != -1 && m.Weapons.TryGetValue(this.ID, out var entity))
                {
                    entity.Properties.AddRange(this.Properties);
                }
                else if (this.TryGetName(out _name) && m.NamedWeapons.TryGetValue(_name, out var namedentity))
                {
                    namedentity.Properties.AddRange(this.Properties);
                }
            }
            base.Resolve();
        }

        public override void AddNamed(string s)
        {
            //do nothing, weapons are never by name
            if (!Parent.NamedWeapons.ContainsKey(s)) Parent.NamedWeapons.Add(s, this);
        }

        public override bool TryGetIDValue(int id, out IDEntity e)
        {
            if (Parent.Weapons.TryGetValue(id, out IDEntity a))
            {
                e = a;
                return true;
            }
            e = null;
            return false;
        }

        public override bool TryGetNamedValue(string s, out IDEntity e)
        {
            if (Parent.NamedWeapons.TryGetValue(s, out IDEntity a))
            {
                e = a;
                return true;
            }
            e = null;
            return false; //never can be by name
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWWEAPON;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTWEAPON;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedWeapons;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Weapons;
        }
    }
}
