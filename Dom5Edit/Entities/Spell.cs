using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Spell : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Spell()
        {
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
            _propertyMap.Add(Command.COPYSPELL, CopySpellRef.Create);
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.DESCR, StringProperty.Create);
            _propertyMap.Add(Command.DETAILS, StringProperty.Create);
            _propertyMap.Add(Command.SCHOOL, IntProperty.Create);
            _propertyMap.Add(Command.RESEARCHLEVEL, IntProperty.Create);
            _propertyMap.Add(Command.PATH, IntIntProperty.Create);
            _propertyMap.Add(Command.PATHLEVEL, IntIntProperty.Create);
            _propertyMap.Add(Command.FATIGUECOST, IntProperty.Create);
            _propertyMap.Add(Command.AOE, IntProperty.Create);
            _propertyMap.Add(Command.DAMAGE, WeaponDamage.Create);
            _propertyMap.Add(Command.DAMAGEMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NEXTSPELL, SpellRef.Create);
            _propertyMap.Add(Command.NEXTINGEO, SpellRef.Create);
            _propertyMap.Add(Command.EFFECT, WeaponDamage.Create);
            _propertyMap.Add(Command.NREFF, IntProperty.Create);
            _propertyMap.Add(Command.RANGE, IntProperty.Create);
            _propertyMap.Add(Command.PRECISION, IntProperty.Create);
            _propertyMap.Add(Command.FLIGHTSPR, IntProperty.Create);
            _propertyMap.Add(Command.EXPLSPR, IntProperty.Create);
            _propertyMap.Add(Command.SOUND, IntProperty.Create);
            _propertyMap.Add(Command.STRIKESOUND, IntProperty.Create);
            _propertyMap.Add(Command.SAMPLE, FilePathProperty.Create);
            _propertyMap.Add(Command.PROVRANGE, IntProperty.Create);
            _propertyMap.Add(Command.ONLYGEOSRC, BitmaskProperty.Create);
            _propertyMap.Add(Command.NOGEOSRC, BitmaskProperty.Create);
            _propertyMap.Add(Command.ONLYGEODST, BitmaskProperty.Create);
            _propertyMap.Add(Command.NOGEODST, BitmaskProperty.Create);
            _propertyMap.Add(Command.ONLYCOASTSRC, IntProperty.Create);
            _propertyMap.Add(Command.ONLYATSITE, SiteRef.Create);
            _propertyMap.Add(Command.ONLYFRIENDLYDST, IntProperty.Create);
            _propertyMap.Add(Command.ONLYOWNDST, IntProperty.Create);
            _propertyMap.Add(Command.NOWATERTRACE, IntProperty.Create);
            _propertyMap.Add(Command.NOLANDTRACE, IntProperty.Create);
            _propertyMap.Add(Command.WALKABLE, IntProperty.Create);
            _propertyMap.Add(Command.SPEC, BitmaskProperty.Create);
            _propertyMap.Add(Command.RESTRICTED, NationRef.Create);
            _propertyMap.Add(Command.FARSUMCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NOTFORNATION, NationRef.Create);
            _propertyMap.Add(Command.CASTTIME, IntProperty.Create);
            _propertyMap.Add(Command.GODPATHSPELL, IntProperty.Create);
            _propertyMap.Add(Command.FRIENDLYENCH, IntProperty.Create);
            _propertyMap.Add(Command.HIDDENENCH, IntProperty.Create);
            _propertyMap.Add(Command.NOCASTMINDLESS, IntProperty.Create);
            _propertyMap.Add(Command.SPELLREQFLY, IntProperty.Create);
            _propertyMap.Add(Command.ONLYMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NOTMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.POLYGETMAGIC, IntProperty.Create);
            _propertyMap.Add(Command.MAXBOUNCES, IntProperty.Create);
            _propertyMap.Add(Command.REQSPELLSINGER, CommandProperty.Create);
            _propertyMap.Add(Command.REQTASKMASTER, CommandProperty.Create);
            _propertyMap.Add(Command.REQSEDUCE, CommandProperty.Create);
            _propertyMap.Add(Command.SETHOME, CommandProperty.Create);
            _propertyMap.Add(Command.REQSUN, IntProperty.Create);
            _propertyMap.Add(Command.AINOCAST, IntProperty.Create);
            _propertyMap.Add(Command.AIBADLVL, IntProperty.Create);
            _propertyMap.Add(Command.AISPELLMOD, IntProperty.Create);
            _propertyMap.Add(Command.REQPLANT, CommandProperty.Create);
            _propertyMap.Add(Command.REQNOPLANT, CommandProperty.Create);
        }

        public int CopySpellID = -1;

        public Spell(string value, string comment, Mod _parent, bool selected = false) : base()
        {
            this.SetID(value, comment);
            Parent = _parent;
            Selected = selected;
            if (ID == -1 && value.Length > 0)
            {
                _name = value;
                Named = true;
                GetNamedList().Add(_name, this);
            }
            else if (ID != -1)
            {
                GetIDList().Add(ID, this);
            }
            else
            {
                Parent.SpellsWithNoNameYet.Add(this);
            }
        }

        public override void AddNamed(string s)
        {
            base.AddNamed(s);
            Parent.SpellsWithNoNameYet.Remove(this);
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Spells;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedSpells;
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWSPELL;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTSPELL;
        }
    }
}
