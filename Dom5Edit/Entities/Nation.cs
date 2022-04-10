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
    public class Nation : IDEntity
    {
        private static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Nation()
        {
            _propertyMap.Add(Command.INDEPFLAG, FilePathProperty.Create);
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.CLEARNATION, CommandProperty.Create);
            _propertyMap.Add(Command.EPITHET, StringProperty.Create);
            _propertyMap.Add(Command.ERA, IntProperty.Create);
            _propertyMap.Add(Command.DESCR, StringProperty.Create);
            _propertyMap.Add(Command.SUMMARY, StringProperty.Create);
            _propertyMap.Add(Command.BRIEF, StringProperty.Create);
            _propertyMap.Add(Command.COLOR, IntIntIntProperty.Create);
            _propertyMap.Add(Command.SECONDARYCOLOR, IntIntIntProperty.Create);
            _propertyMap.Add(Command.FLAG, FilePathProperty.Create);
            _propertyMap.Add(Command.DISABLEOLDNATIONS, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARSITES, CommandProperty.Create);
            _propertyMap.Add(Command.STARTSITE, SiteRef.Create);
            _propertyMap.Add(Command.ISLANDSITE, SiteRef.Create);
            _propertyMap.Add(Command.LIKESTERR, BitmaskProperty.Create);
            _propertyMap.Add(Command.IDEALCOLD, IntProperty.Create);
            _propertyMap.Add(Command.DEFCHAOS, IntProperty.Create);
            _propertyMap.Add(Command.DEFSLOTH, IntProperty.Create);
            _propertyMap.Add(Command.DEFDEATH, IntProperty.Create);
            _propertyMap.Add(Command.DEFMISFORTUNE, IntProperty.Create);
            _propertyMap.Add(Command.DEFDRAIN, IntProperty.Create);
            _propertyMap.Add(Command.UWNATION, CommandProperty.Create);
            _propertyMap.Add(Command.COASTNATION, CommandProperty.Create);
            _propertyMap.Add(Command.RIVERSTART, CommandProperty.Create);
            _propertyMap.Add(Command.CAVENATION, IntProperty.Create);
            _propertyMap.Add(Command.ISLANDNATION, CommandProperty.Create);
            _propertyMap.Add(Command.HATESTERR, BitmaskProperty.Create);
            _propertyMap.Add(Command.KILLCAPPOP, IntProperty.Create);
            _propertyMap.Add(Command.AIHOLDGOD, CommandProperty.Create);
            _propertyMap.Add(Command.AIAWAKE, IntProperty.Create);
            _propertyMap.Add(Command.AIFIRENATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIAIRNATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIWATERNATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIEARTHNATION, CommandProperty.Create);
            _propertyMap.Add(Command.AINATURENATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIASTRALNATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIDEATHNATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIBLOODNATION, CommandProperty.Create);
            _propertyMap.Add(Command.BLOODNATION, CommandProperty.Create);
            _propertyMap.Add(Command.AIGOODBLESS, IntProperty.Create);
            _propertyMap.Add(Command.AIMUSTHAVEMAG, IntProperty.Create);
            _propertyMap.Add(Command.AICHEAPHOLY, CommandProperty.Create);
            _propertyMap.Add(Command.AIHOLYRANGED, CommandProperty.Create);
            _propertyMap.Add(Command.AIHEAVYREC, IntProperty.Create);
            _propertyMap.Add(Command.AIMAGEREC, IntProperty.Create);
            _propertyMap.Add(Command.CLEARREC, CommandProperty.Create);
            _propertyMap.Add(Command.STARTCOM, MonsterRef.Create);
            _propertyMap.Add(Command.COASTCOM1, MonsterRef.Create);
            _propertyMap.Add(Command.COASTCOM2, MonsterRef.Create);
            _propertyMap.Add(Command.ADDFOREIGNUNIT, MonsterRef.Create);
            _propertyMap.Add(Command.ADDFOREIGNCOM, MonsterRef.Create);
            _propertyMap.Add(Command.FORESTREC, MonsterRef.Create);
            _propertyMap.Add(Command.MOUNTAINREC, MonsterRef.Create);
            _propertyMap.Add(Command.SWAMPREC, MonsterRef.Create);
            _propertyMap.Add(Command.WASTEREC, MonsterRef.Create);
            _propertyMap.Add(Command.CAVEREC, MonsterRef.Create);
            _propertyMap.Add(Command.COASTREC, MonsterRef.Create);
            _propertyMap.Add(Command.FORESTCOM, MonsterRef.Create);
            _propertyMap.Add(Command.MOUNTAINCOM, MonsterRef.Create);
            _propertyMap.Add(Command.SWAMPCOM, MonsterRef.Create);
            _propertyMap.Add(Command.WASTECOM, MonsterRef.Create);
            _propertyMap.Add(Command.CAVECOM, MonsterRef.Create);
            _propertyMap.Add(Command.COASTCOM, MonsterRef.Create);
            _propertyMap.Add(Command.STARTSCOUT, MonsterRef.Create);
            _propertyMap.Add(Command.STARTUNITTYPE1, MonsterRef.Create);
            _propertyMap.Add(Command.STARTUNITNBRS1, IntProperty.Create);
            _propertyMap.Add(Command.STARTUNITTYPE2, MonsterRef.Create);
            _propertyMap.Add(Command.STARTUNITNBRS2, IntProperty.Create);
            _propertyMap.Add(Command.ADDRECUNIT, MonsterRef.Create);
            _propertyMap.Add(Command.ADDRECCOM, MonsterRef.Create);
            _propertyMap.Add(Command.UWREC, MonsterRef.Create);
            _propertyMap.Add(Command.UWCOM, MonsterRef.Create);
            _propertyMap.Add(Command.COASTUNIT1, MonsterRef.Create);
            _propertyMap.Add(Command.COASTUNIT2, MonsterRef.Create);
            _propertyMap.Add(Command.COASTUNIT3, MonsterRef.Create);
            _propertyMap.Add(Command.LANDREC, MonsterRef.Create);
            _propertyMap.Add(Command.LANDCOM, MonsterRef.Create);
            _propertyMap.Add(Command.MERCCOST, IntProperty.Create);
            _propertyMap.Add(Command.HERO1, MonsterRef.Create);
            _propertyMap.Add(Command.HERO2, MonsterRef.Create);
            _propertyMap.Add(Command.HERO3, MonsterRef.Create);
            _propertyMap.Add(Command.HERO4, MonsterRef.Create);
            _propertyMap.Add(Command.HERO5, MonsterRef.Create);
            _propertyMap.Add(Command.HERO6, MonsterRef.Create);
            _propertyMap.Add(Command.HERO7, MonsterRef.Create);
            _propertyMap.Add(Command.HERO8, MonsterRef.Create);
            _propertyMap.Add(Command.HERO9, MonsterRef.Create);
            _propertyMap.Add(Command.HERO10, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO1, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO2, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO3, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO4, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO5, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO6, MonsterRef.Create);
            _propertyMap.Add(Command.MULTIHERO7, MonsterRef.Create);
            _propertyMap.Add(Command.NOFOREIGNREC, CommandProperty.Create);
            _propertyMap.Add(Command.DEFCOM1, MonsterRef.Create);
            _propertyMap.Add(Command.DEFCOM2, MonsterRef.Create);
            _propertyMap.Add(Command.DEFUNIT1, MonsterRef.Create);
            _propertyMap.Add(Command.DEFUNIT1B, MonsterRef.Create);
            _propertyMap.Add(Command.DEFUNIT1C, MonsterRef.Create);
            _propertyMap.Add(Command.DEFUNIT1D, MonsterRef.Create);
            _propertyMap.Add(Command.DEFUNIT2, MonsterRef.Create);
            _propertyMap.Add(Command.DEFUNIT2B, MonsterRef.Create);
            _propertyMap.Add(Command.DEFMULT1, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1B, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1C, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1D, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT2, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT2B, IntProperty.Create);
            _propertyMap.Add(Command.WALLCOM, MonsterRef.Create);
            _propertyMap.Add(Command.WALLUNIT, MonsterRef.Create);
            _propertyMap.Add(Command.WALLMULT, IntProperty.Create);
            _propertyMap.Add(Command.BADINDPD, IntProperty.Create);
            _propertyMap.Add(Command.UWWALLUNIT, MonsterRef.Create);
            _propertyMap.Add(Command.UWWALLMULT, IntProperty.Create);
            _propertyMap.Add(Command.UWWALLCOM, MonsterRef.Create);
            _propertyMap.Add(Command.CLEARGODS, CommandProperty.Create);
            _propertyMap.Add(Command.ADDGOD, MonsterRef.Create);
            _propertyMap.Add(Command.HOMEREALM, IntProperty.Create);
            _propertyMap.Add(Command.NOUNDEADGODS, CommandProperty.Create);
            _propertyMap.Add(Command.DELGOD, MonsterRef.Create);
            _propertyMap.Add(Command.LIKESPOP, PoptypeIDRef.Create);
            _propertyMap.Add(Command.GODREBIRTH, CommandProperty.Create);
            _propertyMap.Add(Command.CHEAPGOD20, MonsterRef.Create);
            _propertyMap.Add(Command.UWBUILD, IntProperty.Create);
            _propertyMap.Add(Command.CHEAPGOD40, MonsterRef.Create);
            _propertyMap.Add(Command.FIREBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.AIRBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.WATERBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.EARTHBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.ASTRALBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.DEATHBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.NATUREBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.BLOODBLESSBONUS, IntProperty.Create);
            _propertyMap.Add(Command.MINPRISON, IntProperty.Create);
            _propertyMap.Add(Command.MAXPRISON, IntProperty.Create);
            _propertyMap.Add(Command.FORTERA, IntProperty.Create);
            _propertyMap.Add(Command.FORTCOST, IntProperty.Create);
            _propertyMap.Add(Command.LABCOST, IntProperty.Create);
            _propertyMap.Add(Command.TEMPLECOST, IntProperty.Create);
            _propertyMap.Add(Command.FORESTLABCOST, IntProperty.Create);
            _propertyMap.Add(Command.FORESTTEMPLECOST, IntProperty.Create);
            _propertyMap.Add(Command.TEMPLEPIC, IntProperty.Create);
            _propertyMap.Add(Command.TEMPLEGEMS, IntProperty.Create);
            _propertyMap.Add(Command.HOMEFORT, IntProperty.Create);
            _propertyMap.Add(Command.BUILDFORT, IntProperty.Create);
            _propertyMap.Add(Command.BUILDUWFORT, IntProperty.Create);
            _propertyMap.Add(Command.BUILDCOASTFORT, IntProperty.Create);
            _propertyMap.Add(Command.FORTUNREST, IntProperty.Create);
            _propertyMap.Add(Command.NODEATHSUPPLY, CommandProperty.Create);
            _propertyMap.Add(Command.HALFDEATHINC, CommandProperty.Create);
            _propertyMap.Add(Command.HALFDEATHPOP, CommandProperty.Create);
            _propertyMap.Add(Command.DOMDEATHSENSE, CommandProperty.Create);
            _propertyMap.Add(Command.NATIONINC, IntProperty.Create);
            _propertyMap.Add(Command.CASTLEPROD, IntProperty.Create);
            _propertyMap.Add(Command.TRADECOAST, IntProperty.Create);
            _propertyMap.Add(Command.GOLEMHP, IntProperty.Create);
            _propertyMap.Add(Command.SPREADCOLD, IntProperty.Create);
            _propertyMap.Add(Command.SPREADHEAT, IntProperty.Create);
            _propertyMap.Add(Command.SPREADCHAOS, IntProperty.Create);
            _propertyMap.Add(Command.SPREADLAZY, IntProperty.Create);
            _propertyMap.Add(Command.SPREADDEATH, IntProperty.Create);
            _propertyMap.Add(Command.NOPREACH, CommandProperty.Create);
            _propertyMap.Add(Command.DYINGDOM, CommandProperty.Create);
            _propertyMap.Add(Command.SACRIFICEDOM, CommandProperty.Create);
            _propertyMap.Add(Command.RECALLGOD, IntProperty.Create);
            _propertyMap.Add(Command.DOMKILL, IntProperty.Create);
            _propertyMap.Add(Command.DOMUNREST, IntProperty.Create);
            _propertyMap.Add(Command.AUTOUNDEAD, CommandProperty.Create);
            _propertyMap.Add(Command.GUARDSPIRIT, MonsterRef.Create);
            _propertyMap.Add(Command.SYNCRETISM, IntProperty.Create);
            _propertyMap.Add(Command.DOMWAR, IntProperty.Create);
            _propertyMap.Add(Command.DOMSAIL, CommandProperty.Create);
            _propertyMap.Add(Command.PRIESTREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.UNDEADREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.HORSEREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.WIGHTREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.TOMBWYRMREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.MANIKINREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.SUPAYAREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.GREEKREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.GHOSTREANIM, CommandProperty.Create);
            _propertyMap.Add(Command.SELECTPOPTYPE, PoptypeIDRef.Create);
            _propertyMap.Add(Command.END, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARDEF, CommandProperty.Create);
            _propertyMap.Add(Command.POPPERGOLD, IntProperty.Create);
            _propertyMap.Add(Command.RESOURCEMULT, IntProperty.Create);
            _propertyMap.Add(Command.SUPPLYMULT, IntProperty.Create);
            _propertyMap.Add(Command.UNRESTHALFINC, IntProperty.Create);
            _propertyMap.Add(Command.UNRESTHALFRES, IntProperty.Create);
            _propertyMap.Add(Command.EVENTISRARE, IntProperty.Create);
            _propertyMap.Add(Command.TURMOILINCOME, IntProperty.Create);
            _propertyMap.Add(Command.TURMOILEVENTS, IntProperty.Create);
            _propertyMap.Add(Command.DEATHINCOME, IntProperty.Create);
            _propertyMap.Add(Command.DEATHSUPPLY, IntProperty.Create);
            _propertyMap.Add(Command.DEATHDEATH, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHINCOME, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHRESOURCES, IntProperty.Create);
            _propertyMap.Add(Command.COLDINCOME, IntProperty.Create);
            _propertyMap.Add(Command.COLDSUPPLY, IntProperty.Create);
            _propertyMap.Add(Command.TEMPSCALECAP, IntProperty.Create);
            _propertyMap.Add(Command.MISFORTUNE, IntProperty.Create);
            _propertyMap.Add(Command.LUCKEVENTS, IntProperty.Create);
            _propertyMap.Add(Command.RESEARCHSCALE, IntProperty.Create);
            _propertyMap.Add(Command.CAVELABCOST, IntProperty.Create);
            _propertyMap.Add(Command.CAVETEMPLECOST, IntProperty.Create);
            _propertyMap.Add(Command.SWAMPLABCOST, IntProperty.Create);
            _propertyMap.Add(Command.SWAMPTEMPLECOST, IntProperty.Create);
            _propertyMap.Add(Command.MOUNTLABCOST, IntProperty.Create);
            _propertyMap.Add(Command.MOUNTTEMPLECOST, IntProperty.Create);
            _propertyMap.Add(Command.WASTELABCOST, IntProperty.Create);
            _propertyMap.Add(Command.WASTETEMPLECOST, IntProperty.Create);
            _propertyMap.Add(Command.FUTURESITE, SiteRef.Create);
        }

        public Nation(string value, string comment, Mod _parent, bool selected = false) : base()
        {
            this.SetID(value, comment);
            Parent = _parent;
            Selected = selected;
            if (ID == -1)
            {
                Parent.NationsWithNoID.Add(this);
            }
            else
            {
                GetIDList().Add(ID, this);
            }
        }

        public override void Parse(Command command, string value, string comment)
        {
            base.Parse(command, value, comment);
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWNATION;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTNATION;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        public override void AddNamed(string s)
        {
            //base.AddNamed(s); //do nothing here atm
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Nations;
        }
    }
}
