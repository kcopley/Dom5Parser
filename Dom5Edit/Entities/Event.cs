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
    public class Event : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Event()
        {
            _propertyMap.Add(Command.RARITY, IntProperty.Create);
            _propertyMap.Add(Command.REQ_RARE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_UNIQUE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_STORY, IntProperty.Create);
            _propertyMap.Add(Command.REQ_INDEPOK, IntProperty.Create);
            _propertyMap.Add(Command.REQ_ERA, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOERA, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TURN, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAXTURN, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PREGAME, IntProperty.Create);
            _propertyMap.Add(Command.REQ_SEASON, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOSEASON, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NATION, NationRef.Create);
            _propertyMap.Add(Command.REQ_NONATION, NationRef.Create);
            _propertyMap.Add(Command.REQ_FORNATION, NationRef.Create);
            _propertyMap.Add(Command.REQ_NOTNATION, NationRef.Create);
            _propertyMap.Add(Command.REQ_NOTFORNATION, NationRef.Create);
            _propertyMap.Add(Command.REQ_NOTFORALLY, NationRef.Create);
            _propertyMap.Add(Command.EXTRAMSG, NationRef.Create);
            _propertyMap.Add(Command.REQ_GEM, IntProperty.Create);
            _propertyMap.Add(Command.REQ_GOLD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_CAPITAL, IntProperty.Create);
            _propertyMap.Add(Command.REQ_OWNCAPITAL, IntProperty.Create);
            _propertyMap.Add(Command.REQ_POPTYPE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_POP0OK, CommandProperty.Create);
            _propertyMap.Add(Command.REQ_MAXPOP, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MINPOP, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MINDEF, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAXDEF, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MINUNREST, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAXUNREST, IntProperty.Create);
            _propertyMap.Add(Command.REQ_LAB, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TEMPLE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_FORT, IntProperty.Create);
            _propertyMap.Add(Command.REQ_LAND, IntProperty.Create);
            _propertyMap.Add(Command.REQ_COAST, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MOUNTAIN, IntProperty.Create);
            _propertyMap.Add(Command.REQ_FOREST, IntProperty.Create);
            _propertyMap.Add(Command.REQ_FARM, IntProperty.Create);
            _propertyMap.Add(Command.REQ_SWAMP, IntProperty.Create);
            _propertyMap.Add(Command.REQ_WASTE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_CAVE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_FRESHWATER, IntProperty.Create);
            _propertyMap.Add(Command.REQ_FREESITES, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOSITENBR, SiteRef.Create);
            _propertyMap.Add(Command.REQ_FOUNDSITE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_HIDDENSITE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_SITE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NEARBYSITE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_CLAIMEDTHRONE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_UNCLAIMEDTHRONE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_FULLOWNER, NationRef.Create);
            _propertyMap.Add(Command.REQ_DOMOWNER, NationRef.Create);
            _propertyMap.Add(Command.REQ_MYDOMINION, IntProperty.Create);
            _propertyMap.Add(Command.REQ_DOMINION, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAXDOMINION, IntProperty.Create);
            _propertyMap.Add(Command.REQ_DOMCHANCE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_GODISMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_CHAOS, IntProperty.Create);
            _propertyMap.Add(Command.REQ_LAZY, IntProperty.Create);
            _propertyMap.Add(Command.REQ_COLD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_DEATH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_UNLUCK, IntProperty.Create);
            _propertyMap.Add(Command.REQ_UNMAGIC, IntProperty.Create);
            _propertyMap.Add(Command.REQ_ORDER, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PROD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_HEAT, IntProperty.Create);
            _propertyMap.Add(Command.REQ_GROWTH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_LUCK, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAGIC, IntProperty.Create);
            _propertyMap.Add(Command.REQ_COMMANDER, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MONSTER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_2MONSTERS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_5MONSTERS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_NOMONSTER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_MNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_NOMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_DEADMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_MINTROOPS, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAXTROOPS, IntProperty.Create);
            _propertyMap.Add(Command.REQ_HUMANOIDRES, CommandProperty.Create);
            _propertyMap.Add(Command.REQ_RESEARCHER, CommandProperty.Create);
            _propertyMap.Add(Command.REQ_PREACH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHFIRE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHAIR, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHWATER, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHEARTH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHASTRAL, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHDEATH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHNATURE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHBLOOD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_PATHHOLY, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHFIRE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHAIR, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHWATER, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHEARTH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHASTRAL, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHDEATH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHNATURE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHBLOOD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHHOLY, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOPATHALL, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_TARGNOMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_TARGGOD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGPROPHET, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGHUMANOID, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMALE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGPATH1, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGPATH2, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGPATH3, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGPATH4, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGNOPATH1, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGNOPATH2, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGNOPATH3, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGNOPATH4, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGAFF, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGORDER, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGITEM, ItemRef.Create);
            _propertyMap.Add(Command.REQ_TARGNOITEM, ItemRef.Create);
            _propertyMap.Add(Command.REQ_TARGUNDEAD, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGDEMON, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGANIMAL, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGINANIMATE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMINDLESS, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGIMMOBILE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMAGICBEING, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGOWNER, NationRef.Create);
            _propertyMap.Add(Command.REQ_TARGFOREIGNOK, CommandProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMINSIZE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMAXSIZE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_CODE, EventCodeRef.Create);
            _propertyMap.Add(Command.REQ_ANYCODE, EventCodeRef.Create);
            _propertyMap.Add(Command.REQ_NOTANYCODE, EventCodeRef.Create);
            _propertyMap.Add(Command.REQ_NEARBYCODE, EventCodeRef.Create);
            _propertyMap.Add(Command.REQ_NEAROWNCODE, EventCodeRef.Create);
            _propertyMap.Add(Command.REQ_PERMONTH, IntProperty.Create);
            _propertyMap.Add(Command.REQ_NOENCH, EnchIDRef.Create);
            _propertyMap.Add(Command.REQ_ENCH, EnchIDRef.Create);
            _propertyMap.Add(Command.REQ_MYENCH, EnchIDRef.Create);
            _propertyMap.Add(Command.REQ_FRIENDLYENCH, EnchIDRef.Create);
            _propertyMap.Add(Command.REQ_HOSTILEENCH, EnchIDRef.Create);
            _propertyMap.Add(Command.REQ_ENCHDOM, EnchIDRef.Create);
            _propertyMap.Add(Command.REQ_ARENADONE, IntProperty.Create);
            _propertyMap.Add(Command.NATION, NationOwnerRef.Create); //TODO
            _propertyMap.Add(Command.NATIONENCH, EnchIDRef.Create);
            _propertyMap.Add(Command.MSG, StringProperty.Create);
            _propertyMap.Add(Command.NOTEXT, CommandProperty.Create);
            _propertyMap.Add(Command.NOLOG, CommandProperty.Create);
            _propertyMap.Add(Command.MAGICITEM, IntProperty.Create);
            _propertyMap.Add(Command.GOLD, IntProperty.Create);
            _propertyMap.Add(Command.EXACTGOLD, IntProperty.Create);
            _propertyMap.Add(Command.INCSCALE, IntProperty.Create);
            _propertyMap.Add(Command.INCSCALE2, IntProperty.Create);
            _propertyMap.Add(Command.INCSCALE3, IntProperty.Create);
            _propertyMap.Add(Command.ZZ1D3VIS, IntProperty.Create);
            _propertyMap.Add(Command.ZZ1D6VIS, IntProperty.Create);
            _propertyMap.Add(Command.ZZ2D4VIS, IntProperty.Create);
            _propertyMap.Add(Command.ZZ2D6VIS, IntProperty.Create);
            _propertyMap.Add(Command.ZZ3D6VIS, IntProperty.Create);
            _propertyMap.Add(Command.DECSCALE, IntProperty.Create);
            _propertyMap.Add(Command.DECSCALE2, IntProperty.Create);
            _propertyMap.Add(Command.DECSCALE3, IntProperty.Create);
            _propertyMap.Add(Command.ZZ4D6VIS, IntProperty.Create);
            _propertyMap.Add(Command.GEMLOSS, IntProperty.Create);
            _propertyMap.Add(Command.LANDGOLD, IntProperty.Create);
            _propertyMap.Add(Command.LANDPROD, IntProperty.Create);
            _propertyMap.Add(Command.TAXBOOST, IntProperty.Create);
            _propertyMap.Add(Command.DEFENCE, IntProperty.Create);
            _propertyMap.Add(Command.KILL, IntProperty.Create);
            _propertyMap.Add(Command.KILLPOP, IntProperty.Create);
            _propertyMap.Add(Command.INCPOP, IntProperty.Create);
            _propertyMap.Add(Command.INCCORPSES, IntProperty.Create);
            _propertyMap.Add(Command.EMIGRATION, IntProperty.Create);
            _propertyMap.Add(Command.UNREST, IntProperty.Create);
            _propertyMap.Add(Command.INCDOM, IntProperty.Create);
            _propertyMap.Add(Command.FORT, IntProperty.Create);
            _propertyMap.Add(Command.TEMPLE, IntProperty.Create);
            _propertyMap.Add(Command.LAB, IntProperty.Create);
            _propertyMap.Add(Command.REVEALSITE, CommandProperty.Create);
            _propertyMap.Add(Command.ADDSITE, SiteRef.Create);
            _propertyMap.Add(Command.REMOVESITE, SiteRef.Create);
            _propertyMap.Add(Command.HIDDENSITE, SiteRef.Create);
            _propertyMap.Add(Command.VISITORS, CommandProperty.Create);
            _propertyMap.Add(Command.NEWDOM, IntProperty.Create);
            _propertyMap.Add(Command.REVOLT, CommandProperty.Create);
            _propertyMap.Add(Command.REVEALPROV, CommandProperty.Create);
            _propertyMap.Add(Command.CLAIMTHRONE, CommandProperty.Create);
            _propertyMap.Add(Command.ASSASSIN, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ASSOWNER, NationRef.Create);
            _propertyMap.Add(Command.STEALTHCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ2COM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ4COM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ5COM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.TEMPUNITS, IntProperty.Create);
            _propertyMap.Add(Command.ZZ1UNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ1D3UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ2D3UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ3D3UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ4D3UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ1D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ2D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ3D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ4D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ5D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ6D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ7D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ8D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ9D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ10D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ11D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ12D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ13D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ14D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ15D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ZZ16D6UNITS, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.STRIKEUNITS, IntProperty.Create);
            _propertyMap.Add(Command.KILLMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.KILL2D6MON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.KILLCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.CURSE, IntProperty.Create);
            _propertyMap.Add(Command.DISEASE, IntProperty.Create);
            _propertyMap.Add(Command.RESEARCHAFF, BitmaskProperty.Create);
            _propertyMap.Add(Command.GAINAFF, BitmaskProperty.Create);
            _propertyMap.Add(Command.GAINMARK, CommandProperty.Create);
            _propertyMap.Add(Command.BANISHED, IntProperty.Create);
            _propertyMap.Add(Command.ADDEQUIP, IntProperty.Create);
            _propertyMap.Add(Command.TRANSFORM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.POISON, IntProperty.Create);
            _propertyMap.Add(Command.XP, IntProperty.Create);
            _propertyMap.Add(Command.FIREBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.AIRBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WATERBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.EARTHBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ASTRALBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEATHBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NATUREBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BLOODBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HOLYBOOST, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.PATHBOOST, IntProperty.Create);
            _propertyMap.Add(Command.WORLDINCSCALE, IntProperty.Create);
            _propertyMap.Add(Command.WORLDINCSCALE2, IntProperty.Create);
            _propertyMap.Add(Command.WORLDINCSCALE3, IntProperty.Create);
            _propertyMap.Add(Command.WORLDDECSCALE, IntProperty.Create);
            _propertyMap.Add(Command.WORLDDECSCALE2, IntProperty.Create);
            _propertyMap.Add(Command.WORLDDECSCALE3, IntProperty.Create);
            _propertyMap.Add(Command.WORLDUNREST, IntProperty.Create);
            _propertyMap.Add(Command.WORLDINCDOM, IntProperty.Create);
            _propertyMap.Add(Command.WORLDRITREBATE, IntProperty.Create);
            _propertyMap.Add(Command.WORLDDARKNESS, CommandProperty.Create);
            _propertyMap.Add(Command.WORLDCURSE, IntProperty.Create);
            _propertyMap.Add(Command.WORLDDISEASE, IntProperty.Create);
            _propertyMap.Add(Command.WORLDMARK, IntProperty.Create);
            _propertyMap.Add(Command.WORLDHEAL, IntProperty.Create);
            _propertyMap.Add(Command.WORLDAGE, IntProperty.Create);
            _propertyMap.Add(Command.LINGER, IntProperty.Create);
            _propertyMap.Add(Command.FLAGLAND, IntProperty.Create);
            _propertyMap.Add(Command.DELAY, IntProperty.Create);
            _propertyMap.Add(Command.DELAY25, IntProperty.Create);
            _propertyMap.Add(Command.DELAY50, IntProperty.Create);
            _propertyMap.Add(Command.DELAYSKIP, IntProperty.Create);
            _propertyMap.Add(Command.ORDER, BitmaskProperty.Create);
            _propertyMap.Add(Command.CODE, EventCodeRef.Create);
            _propertyMap.Add(Command.CODE2, EventCodeRef.Create);
            _propertyMap.Add(Command.RESETCODE, EventCodeRef.Create);
            _propertyMap.Add(Command.PURGECALENDAR, IntProperty.Create);
            _propertyMap.Add(Command.PURGEDELAYED, IntProperty.Create);
            _propertyMap.Add(Command.ID, EventEffectCodeRef.Create);
            _propertyMap.Add(Command.CODEDELAY, EventCodeRef.Create);
            _propertyMap.Add(Command.CODEDELAY2, EventCodeRef.Create);
            _propertyMap.Add(Command.RESETCODEDELAY, EventCodeRef.Create);
            _propertyMap.Add(Command.RESETCODEDELAY2, EventCodeRef.Create);
            _propertyMap.Add(Command.ARENA, CommandProperty.Create);
            _propertyMap.Add(Command.RESOLVEARENA1, CommandProperty.Create);
            _propertyMap.Add(Command.RESOLVEARENA2, CommandProperty.Create);
            _propertyMap.Add(Command.REQ_TARGNOAFF, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MINCORPSES, IntProperty.Create);
            _propertyMap.Add(Command.REQ_MAXCORPSES, IntProperty.Create);
            _propertyMap.Add(Command.REQ_THRONESITE, IntProperty.Create);
            _propertyMap.Add(Command.SETPOPTYPE, PoptypeIDRef.Create);
            _propertyMap.Add(Command.REQ_TARGNOORDER, BitmaskProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMINMORALE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGMAXMORALE, IntProperty.Create);
            _propertyMap.Add(Command.REQ_TARGNOTOWNER, NationRef.Create);
            _propertyMap.Add(Command.REQ_TARGNOTALLY, NationRef.Create);
            _propertyMap.Add(Command.REQ_TARGALLY, NationRef.Create);
            _propertyMap.Add(Command.REQ_GODISNOTMNR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REQ_WORLDITEM, ItemRef.Create);
            _propertyMap.Add(Command.REQ_NOWORLDITEM, ItemRef.Create);
            _propertyMap.Add(Command.SETXP, IntProperty.Create);
        }

        public Event(string value, string comment, Mod _parent, bool selected = false) : base()
        {
            this.SetID(value, comment);
            Parent = _parent;
            Selected = selected;
            Parent.Events.Add(this);
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWEVENT;
        }

        internal override Command GetSelectCommand()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            throw new NotImplementedException();
        }
    }
}
