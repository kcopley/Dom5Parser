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
            _propertyMap.Add(Command.COLOR, FloatFloatFloatProperty.Create);
            _propertyMap.Add(Command.SECONDARYCOLOR, FloatFloatFloatProperty.Create);
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
            _propertyMap.Add(Command.STARTCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTCOM1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTCOM2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ADDFOREIGNUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ADDFOREIGNCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.FORESTREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MOUNTAINREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SWAMPREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WASTEREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.CAVEREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.FORESTCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MOUNTAINCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SWAMPCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WASTECOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.CAVECOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.STARTSCOUT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.STARTUNITTYPE1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.STARTUNITNBRS1, IntProperty.Create);
            _propertyMap.Add(Command.STARTUNITTYPE2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.STARTUNITNBRS2, IntProperty.Create);
            _propertyMap.Add(Command.ADDRECUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ADDRECCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTUNIT1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTUNIT2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.COASTUNIT3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.LANDREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.LANDCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MERCCOST, IntProperty.Create);
            _propertyMap.Add(Command.HERO1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO7, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO8, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO9, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HERO10, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MULTIHERO7, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NOFOREIGNREC, CommandProperty.Create);
            _propertyMap.Add(Command.DEFCOM1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFCOM2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1B, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1C, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1D, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT2B, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFMULT1, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1B, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1C, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1D, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT2, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT2B, IntProperty.Create);
            _propertyMap.Add(Command.WALLCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WALLUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.WALLMULT, IntProperty.Create);
            _propertyMap.Add(Command.BADINDPD, IntProperty.Create);
            _propertyMap.Add(Command.UWDEFCOM1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFCOM2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFUNIT1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFMULT1, IntProperty.Create);
            _propertyMap.Add(Command.UWDEFUNIT1B, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFMULT1B, IntProperty.Create);
            _propertyMap.Add(Command.UWDEFUNIT2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFMULT2, IntProperty.Create);
            _propertyMap.Add(Command.UWDEFUNIT2B, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFMULT2B, IntProperty.Create);
            _propertyMap.Add(Command.UWDEFUNIT1C, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFMULT1C, IntProperty.Create);
            _propertyMap.Add(Command.UWDEFUNIT1D, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWDEFMULT1D, IntProperty.Create);
            _propertyMap.Add(Command.UWWALLUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWWALLMULT, IntProperty.Create);
            _propertyMap.Add(Command.UWWALLCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.CLEARGODS, CommandProperty.Create);
            _propertyMap.Add(Command.ADDGOD, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.HOMEREALM, IntProperty.Create);
            _propertyMap.Add(Command.NOUNDEADGODS, CommandProperty.Create);
            _propertyMap.Add(Command.DELGOD, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.LIKESPOP, PoptypeIDRef.Create);
            _propertyMap.Add(Command.GODREBIRTH, CommandProperty.Create);
            _propertyMap.Add(Command.CHEAPGOD20, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UWBUILD, IntProperty.Create);
            _propertyMap.Add(Command.CHEAPGOD40, MonsterOrMontagRef.Create);
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
            _propertyMap.Add(Command.GUARDSPIRIT, MonsterOrMontagRef.Create);
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

        public override void Resolve()
        {
            if (base._resolved) return;
            foreach (var m in Parent.Dependencies)
            {
                if (ID != -1 && m.Nations.TryGetValue(this.ID, out var entity))
                {
                    entity.Properties.AddRange(this.Properties);
                }
            }
            base.Resolve();
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

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedNations;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Nations;
        }
    }
}
