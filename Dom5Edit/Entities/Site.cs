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
    public class Site : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Site()
        {
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
            _propertyMap.Add(Command.PATH, IntProperty.Create);
            _propertyMap.Add(Command.GEMS, IntIntProperty.Create);
            _propertyMap.Add(Command.GOLD, IntProperty.Create);
            _propertyMap.Add(Command.RES, IntProperty.Create);
            _propertyMap.Add(Command.LEVEL, IntProperty.Create);
            _propertyMap.Add(Command.RARITY, IntProperty.Create);
            _propertyMap.Add(Command.DECUNREST, IntProperty.Create);
            _propertyMap.Add(Command.SUPPLY, IntProperty.Create);
            _propertyMap.Add(Command.HOMEMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HOMECOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NAT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NATMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NATCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMONLVL2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMONLVL3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMONLVL4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.VOIDGATE, IntProperty.Create);
            _propertyMap.Add(Command.WALLCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WALLUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WALLMULT, IntProperty.Create);
            _propertyMap.Add(Command.UWWALLUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWWALLMULT, IntProperty.Create);
            _propertyMap.Add(Command.UWWALLCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.INCSCALE, IntProperty.Create);
            _propertyMap.Add(Command.DECSCALE, IntProperty.Create);
            _propertyMap.Add(Command.CONJCOST, IntProperty.Create);
            _propertyMap.Add(Command.ALTCOST, IntProperty.Create);
            _propertyMap.Add(Command.EVOCOST, IntProperty.Create);
            _propertyMap.Add(Command.CONSTCOST, IntProperty.Create);
            _propertyMap.Add(Command.ENCHCOST, IntProperty.Create);
            _propertyMap.Add(Command.THAUCOST, IntProperty.Create);
            _propertyMap.Add(Command.BLOODCOST, IntProperty.Create);
            _propertyMap.Add(Command.SCRY, IntProperty.Create);
            _propertyMap.Add(Command.SCRYRANGE, IntProperty.Create);
            _propertyMap.Add(Command.FIRERANGE, IntProperty.Create);
            _propertyMap.Add(Command.AIRRANGE, IntProperty.Create);
            _propertyMap.Add(Command.WATERRANGE, IntProperty.Create);
            _propertyMap.Add(Command.EARTHRANGE, IntProperty.Create);
            _propertyMap.Add(Command.ASTRALRANGE, IntProperty.Create);
            _propertyMap.Add(Command.DEATHRANGE, IntProperty.Create);
            _propertyMap.Add(Command.NATURERANGE, IntProperty.Create);
            _propertyMap.Add(Command.BLOODRANGE, IntProperty.Create);
            _propertyMap.Add(Command.ELEMENTRANGE, IntProperty.Create);
            _propertyMap.Add(Command.SORCERYRANGE, IntProperty.Create);
            _propertyMap.Add(Command.ALLRANGE, IntProperty.Create);
            _propertyMap.Add(Command.HEAL, IntProperty.Create);
            _propertyMap.Add(Command.CURSE, IntProperty.Create);
            _propertyMap.Add(Command.CLUSTER, IntProperty.Create);
            _propertyMap.Add(Command.DISEASE, IntProperty.Create);
            _propertyMap.Add(Command.HORRORMARK, IntProperty.Create);
            _propertyMap.Add(Command.HOLYFIRE, IntProperty.Create);
            _propertyMap.Add(Command.HOLYPOWER, IntProperty.Create);
            _propertyMap.Add(Command.XP, IntProperty.Create);
            _propertyMap.Add(Command.ADVENTURERUIN, IntProperty.Create);
            _propertyMap.Add(Command.LAB, CommandProperty.Create);
            _propertyMap.Add(Command.TEMPLE, CommandProperty.Create);
            _propertyMap.Add(Command.FORT, IntProperty.Create);
            _propertyMap.Add(Command.CLAIM, CommandProperty.Create);
            _propertyMap.Add(Command.DOMINION, IntProperty.Create);
            _propertyMap.Add(Command.GODDOMCHAOS, IntProperty.Create);
            _propertyMap.Add(Command.GODDOMLAZY, IntProperty.Create);
            _propertyMap.Add(Command.GODDOMCOLD, IntProperty.Create);
            _propertyMap.Add(Command.GODDOMDEATH, IntProperty.Create);
            _propertyMap.Add(Command.GODDOMMISFORTUNE, IntProperty.Create);
            _propertyMap.Add(Command.GODDOMDRAIN, IntProperty.Create);
            _propertyMap.Add(Command.BLESSHP, IntProperty.Create);
            _propertyMap.Add(Command.BLESSANIMAWE, IntProperty.Create);
            _propertyMap.Add(Command.BLESSMR, IntProperty.Create);
            _propertyMap.Add(Command.BLESSAWE, IntProperty.Create);
            _propertyMap.Add(Command.BLESSMOR, IntProperty.Create);
            _propertyMap.Add(Command.BLESSSTR, IntProperty.Create);
            _propertyMap.Add(Command.BLESSDARKVIS, IntProperty.Create);
            _propertyMap.Add(Command.BLESSATT, IntProperty.Create);
            _propertyMap.Add(Command.EVIL, CommandProperty.Create);
            _propertyMap.Add(Command.BLESSDEF, IntProperty.Create);
            _propertyMap.Add(Command.BLESSPREC, IntProperty.Create);
            _propertyMap.Add(Command.BLESSFIRERES, IntProperty.Create);
            _propertyMap.Add(Command.BLESSCOLDRES, IntProperty.Create);
            _propertyMap.Add(Command.BLESSSHOCKRES, IntProperty.Create);
            _propertyMap.Add(Command.BLESSPOISRES, IntProperty.Create);
            _propertyMap.Add(Command.BLESSAIRSHLD, IntProperty.Create);
            _propertyMap.Add(Command.BLESSREINVIG, IntProperty.Create);
            _propertyMap.Add(Command.BLESSDTV, IntProperty.Create);
            _propertyMap.Add(Command.WILD, CommandProperty.Create);
            _propertyMap.Add(Command.RECALLGOD, IntProperty.Create);
            _propertyMap.Add(Command.DOMWAR, IntProperty.Create);
            _propertyMap.Add(Command.MINEGOLD, IntProperty.Create);
        }

        public Site(string value, string comment, Mod _parent, bool selected = false) : base()
        {
            //Because a newsite doesn't accept a name there, and it's added below.....
            this.SetID(value, comment);
            Parent = _parent;
            Selected = selected;
            if (ID == -1 && selected)
            {
                Named = true;
                GetNamedList().Add(_name, this);
            }
            else if (ID == -1)
            {
                Parent.SitesThatNeedIDs.Add(this);
            }
            else
            {
                GetIDList().Add(ID, this);
            }
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWSITE;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTSITE;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedSites;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Sites;
        }
    }
}
