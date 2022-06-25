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
    public class Item : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Item()
        {
            //_propertyMap.Add(Command.CLEARALLITEMS, CommandProperty.Create); //I'm actually going to interpret this as not existing
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
            _propertyMap.Add(Command.CONSTLEVEL, IntProperty.Create);
            _propertyMap.Add(Command.MAINPATH, IntProperty.Create);
            _propertyMap.Add(Command.MAINLEVEL, IntProperty.Create);
            _propertyMap.Add(Command.SECONDARYPATH, IntProperty.Create);
            _propertyMap.Add(Command.SECONDARYLEVEL, IntProperty.Create);
            _propertyMap.Add(Command.COPYITEM, ItemRef.Create);
            _propertyMap.Add(Command.COPYSPR, ItemRef.Create);
            _propertyMap.Add(Command.SPR, FilePathProperty.Create);
            _propertyMap.Add(Command.TYPE, IntProperty.Create);
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.SAILING, IntIntProperty.Create);
            _propertyMap.Add(Command.DESCR, StringProperty.Create);
            _propertyMap.Add(Command.WEAPON, WeaponRef.Create);
            _propertyMap.Add(Command.ARMOR, ArmorRef.Create);
            _propertyMap.Add(Command.MAGICBOOST, IntIntProperty.Create);
            _propertyMap.Add(Command.PEN, IntProperty.Create);
            _propertyMap.Add(Command.SPELL, SpellRef.Create);
            _propertyMap.Add(Command.AUTOSPELL, SpellRef.Create);
            _propertyMap.Add(Command.AUTOSPELLREPEAT, IntProperty.Create);
            _propertyMap.Add(Command.RANDOMSPELL, IntProperty.Create);
            _propertyMap.Add(Command.HP, IntProperty.Create);
            _propertyMap.Add(Command.STR, IntProperty.Create);
            _propertyMap.Add(Command.ATT, IntProperty.Create);
            _propertyMap.Add(Command.DEF, IntProperty.Create);
            _propertyMap.Add(Command.PREC, IntProperty.Create);
            _propertyMap.Add(Command.MR, IntProperty.Create);
            _propertyMap.Add(Command.LUCK, CommandProperty.Create);
            _propertyMap.Add(Command.MORALE, IntProperty.Create);
            _propertyMap.Add(Command.QUICKNESS, CommandProperty.Create);
            _propertyMap.Add(Command.VOIDSANITY, IntProperty.Create);
            _propertyMap.Add(Command.BLESS, CommandProperty.Create);
            _propertyMap.Add(Command.FIRERES, IntProperty.Create);
            _propertyMap.Add(Command.COLDRES, IntProperty.Create);
            _propertyMap.Add(Command.BARKSKIN, CommandProperty.Create);
            _propertyMap.Add(Command.SHOCKRES, IntProperty.Create);
            _propertyMap.Add(Command.POISONRES, IntProperty.Create);
            _propertyMap.Add(Command.STONESKIN, CommandProperty.Create);
            _propertyMap.Add(Command.IRONSKIN, CommandProperty.Create);
            _propertyMap.Add(Command.BERS, CommandProperty.Create);
            _propertyMap.Add(Command.EXTRALIFE, CommandProperty.Create);
            _propertyMap.Add(Command.GUARDSPIRITBONUS, IntProperty.Create);
            _propertyMap.Add(Command.LIMITEDREGEN, IntProperty.Create);
            _propertyMap.Add(Command.POLYIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.AUTOBLESS, CommandProperty.Create);
            _propertyMap.Add(Command.MAPSPEED, IntProperty.Create);
            _propertyMap.Add(Command.WATERBREATHING, CommandProperty.Create);
            _propertyMap.Add(Command.FLOAT, CommandProperty.Create);
            _propertyMap.Add(Command.FLY, CommandProperty.Create);
            _propertyMap.Add(Command.STORMIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.RUN, CommandProperty.Create);
            _propertyMap.Add(Command.SNEAKUNIT, IntProperty.Create);
            _propertyMap.Add(Command.STEALTHBOOST, IntProperty.Create);
            _propertyMap.Add(Command.SWIFT, IntProperty.Create);
            _propertyMap.Add(Command.REQEYES, CommandProperty.Create);
            _propertyMap.Add(Command.RESTRICTED, NationRef.Create);
            _propertyMap.Add(Command.NOFIND, CommandProperty.Create);
            _propertyMap.Add(Command.RESTRICTEDITEM, RestrictedItemIDRef.Create);
            _propertyMap.Add(Command.NATIONREBATE, NationRef.Create);
            _propertyMap.Add(Command.NOFORGEBONUS, CommandProperty.Create);
            _propertyMap.Add(Command.ISLANCE, CommandProperty.Create);
            _propertyMap.Add(Command.MINSIZE, IntProperty.Create);
            _propertyMap.Add(Command.MAXSIZE, IntProperty.Create);
            _propertyMap.Add(Command.UNIQUE, CommandProperty.Create);
            _propertyMap.Add(Command.AOE, IntProperty.Create);
            _propertyMap.Add(Command.TAINTED, IntProperty.Create);
            _propertyMap.Add(Command.CURSED, CommandProperty.Create);
            _propertyMap.Add(Command.NOMOUNTED, CommandProperty.Create);
            _propertyMap.Add(Command.CURSE, CommandProperty.Create);
            _propertyMap.Add(Command.NOCOLDBLOOD, CommandProperty.Create);
            _propertyMap.Add(Command.DISEASE, CommandProperty.Create);
            _propertyMap.Add(Command.NODEMON, CommandProperty.Create);
            _propertyMap.Add(Command.CHESTWOUND, CommandProperty.Create);
            _propertyMap.Add(Command.NOUNDEAD, CommandProperty.Create);
            _propertyMap.Add(Command.NOINANIM, CommandProperty.Create);
            _propertyMap.Add(Command.NOIMMOBILE, CommandProperty.Create);
            _propertyMap.Add(Command.FEEBLEMIND, CommandProperty.Create);
            _propertyMap.Add(Command.MUTE, CommandProperty.Create);
            _propertyMap.Add(Command.ONLYMOUNTED, CommandProperty.Create);
            _propertyMap.Add(Command.ONLYCOLDBLOOD, CommandProperty.Create);
            _propertyMap.Add(Command.NHWOUND, CommandProperty.Create);
            _propertyMap.Add(Command.ONLYDEMON, CommandProperty.Create);
            _propertyMap.Add(Command.CRIPPLED, CommandProperty.Create);
            _propertyMap.Add(Command.ONLYUNDEAD, CommandProperty.Create);
            _propertyMap.Add(Command.LOSEEYE, CommandProperty.Create);
            _propertyMap.Add(Command.ONLYINANIM, CommandProperty.Create);
            _propertyMap.Add(Command.ONLYIMMOBILE, CommandProperty.Create);
            _propertyMap.Add(Command.RECUPERATION, CommandProperty.Create);
            _propertyMap.Add(Command.YEARAGING, IntProperty.Create);
            _propertyMap.Add(Command.NOAGING, IntProperty.Create);
            _propertyMap.Add(Command.NOAGINGLAND, IntProperty.Create);
            _propertyMap.Add(Command.ITEMCOST1, IntProperty.Create);
            _propertyMap.Add(Command.ITEMCOST2, IntProperty.Create);
            _propertyMap.Add(Command.ITEMDRAWSIZE, IntProperty.Create);
            _propertyMap.Add(Command.CHAMPPRIZE, CommandProperty.Create);
            _propertyMap.Add(Command.AUTOCOMPETE, CommandProperty.Create);
            _propertyMap.Add(Command.SINGLEBATTLE, CommandProperty.Create);
            _propertyMap.Add(Command.CHAOSREC, IntProperty.Create);
            _propertyMap.Add(Command.STONEBEING, CommandProperty.Create);
            _propertyMap.Add(Command.NORIVERPASS, CommandProperty.Create);
            _propertyMap.Add(Command.UNTELEPORTABLE, CommandProperty.Create);
            _propertyMap.Add(Command.GIFTOFWATER, IntProperty.Create);
            _propertyMap.Add(Command.NOMOVEPEN, CommandProperty.Create);
            _propertyMap.Add(Command.FARSAIL, IntProperty.Create);
            _propertyMap.Add(Command.NORANGE, CommandProperty.Create);
            _propertyMap.Add(Command.SEDUCE, IntProperty.Create);
            _propertyMap.Add(Command.SUCCUBUS, IntProperty.Create);
            _propertyMap.Add(Command.BECKON, IntProperty.Create);
            _propertyMap.Add(Command.FALSEARMY, IntProperty.Create);
            _propertyMap.Add(Command.FOOLSCOUTS, IntProperty.Create);
            _propertyMap.Add(Command.SCALEWALLS, CommandProperty.Create);
            _propertyMap.Add(Command.SLASHRES, CommandProperty.Create);
            _propertyMap.Add(Command.PIERCERES, CommandProperty.Create);
            _propertyMap.Add(Command.BLUNTRES, CommandProperty.Create);
            _propertyMap.Add(Command.ICEPROT, IntProperty.Create);
            _propertyMap.Add(Command.INVULNERABLE, IntProperty.Create);
            _propertyMap.Add(Command.ETHEREAL, CommandProperty.Create);
            _propertyMap.Add(Command.AIRSHIELD, IntProperty.Create);
            _propertyMap.Add(Command.IRONVUL, IntProperty.Create);
            _propertyMap.Add(Command.HEALER, IntProperty.Create);
            _propertyMap.Add(Command.AUTOHEALER, IntProperty.Create);
            _propertyMap.Add(Command.AUTODISHEALER, IntProperty.Create);
            _propertyMap.Add(Command.AUTODISGRINDER, IntProperty.Create);
            _propertyMap.Add(Command.DISEASERES, IntProperty.Create);
            _propertyMap.Add(Command.HOMESICK, IntProperty.Create);
            _propertyMap.Add(Command.UWDAMAGE, IntProperty.Create);
            _propertyMap.Add(Command.REGENERATION, IntProperty.Create);
            _propertyMap.Add(Command.REINVIGORATION, IntProperty.Create);
            _propertyMap.Add(Command.WOUNDFEND, IntProperty.Create);
            _propertyMap.Add(Command.HPOVERFLOW, IntProperty.Create);
            _propertyMap.Add(Command.HPOVERSLOW, IntProperty.Create);
            _propertyMap.Add(Command.DEADHP, IntProperty.Create);
            _propertyMap.Add(Command.DOHEAL, CommandProperty.Create);
            _propertyMap.Add(Command.UNDREGEN, IntProperty.Create);
            _propertyMap.Add(Command.UWREGEN, IntProperty.Create);
            _propertyMap.Add(Command.SPRINGPOWER, IntProperty.Create);
            _propertyMap.Add(Command.SUMMERPOWER, IntProperty.Create);
            _propertyMap.Add(Command.FALLPOWER, IntProperty.Create);
            _propertyMap.Add(Command.WINTERPOWER, IntProperty.Create);
            _propertyMap.Add(Command.YEARTURN, IntProperty.Create);
            _propertyMap.Add(Command.CHAOSPOWER, IntProperty.Create);
            _propertyMap.Add(Command.FIREPOWER, IntProperty.Create);
            _propertyMap.Add(Command.COLDPOWER, IntProperty.Create);
            _propertyMap.Add(Command.MAGICPOWER, IntProperty.Create);
            _propertyMap.Add(Command.STORMPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DARKPOWER, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DEATHPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DOMPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DISEASECLOUD, IntProperty.Create);
            _propertyMap.Add(Command.POISONCLOUD, IntProperty.Create);
            _propertyMap.Add(Command.POISONSKIN, IntProperty.Create);
            _propertyMap.Add(Command.POISONARMOR, IntProperty.Create);
            _propertyMap.Add(Command.ANIMALAWE, IntProperty.Create);
            _propertyMap.Add(Command.AWE, IntProperty.Create);
            _propertyMap.Add(Command.CURSELUCKSHIELD, IntProperty.Create);
            _propertyMap.Add(Command.SUNAWE, IntProperty.Create);
            _propertyMap.Add(Command.HALTHERETIC, IntProperty.Create);
            _propertyMap.Add(Command.FEAR, IntProperty.Create);
            _propertyMap.Add(Command.FIRESHIELD, IntProperty.Create);
            _propertyMap.Add(Command.UWFIRESHIELD, IntProperty.Create);
            _propertyMap.Add(Command.BANEFIRESHIELD, IntProperty.Create);
            _propertyMap.Add(Command.ACIDSHIELD, IntProperty.Create);
            _propertyMap.Add(Command.DAMAGEREV, IntProperty.Create);
            _propertyMap.Add(Command.BLOODVENGEANCE, IntProperty.Create);
            _propertyMap.Add(Command.SLIMER, IntProperty.Create);
            _propertyMap.Add(Command.DEATHCURSE, CommandProperty.Create);
            _propertyMap.Add(Command.DEATHDISEASE, IntProperty.Create);
            _propertyMap.Add(Command.DEATHFIRE, IntProperty.Create);
            _propertyMap.Add(Command.DEATHPARALYZE, IntProperty.Create);
            _propertyMap.Add(Command.UWHEAT, IntProperty.Create);
            _propertyMap.Add(Command.MINDSLIME, IntProperty.Create);
            _propertyMap.Add(Command.HEAT, IntProperty.Create);
            _propertyMap.Add(Command.COLD, IntProperty.Create);
            _propertyMap.Add(Command.OVERCHARGED, IntProperty.Create);
            _propertyMap.Add(Command.EYELOSS, CommandProperty.Create);
            _propertyMap.Add(Command.AMBIDEXTROUS, IntProperty.Create);
            _propertyMap.Add(Command.BERSERK, IntProperty.Create);
            _propertyMap.Add(Command.BLESSBERS, CommandProperty.Create);
            _propertyMap.Add(Command.BLESSFLY, CommandProperty.Create);
            _propertyMap.Add(Command.DARKVISION, IntProperty.Create);
            _propertyMap.Add(Command.TRAMPLE, CommandProperty.Create);
            _propertyMap.Add(Command.TRAMPSWALLOW, CommandProperty.Create);
            _propertyMap.Add(Command.DIGEST, IntProperty.Create);
            _propertyMap.Add(Command.ACIDDIGEST, IntProperty.Create);
            _propertyMap.Add(Command.INCORPORATE, IntProperty.Create);
            _propertyMap.Add(Command.RAISEONKILL, IntProperty.Create);
            _propertyMap.Add(Command.RAISESHAPE, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UNSURR, IntProperty.Create);
            _propertyMap.Add(Command.SPIRITSIGHT, CommandProperty.Create);
            _propertyMap.Add(Command.INVISIBLE, CommandProperty.Create);
            _propertyMap.Add(Command.CASTLEDEF, IntProperty.Create);
            _propertyMap.Add(Command.SIEGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.PATROLBONUS, IntProperty.Create);
            _propertyMap.Add(Command.PILLAGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.SUPPLYBONUS, IntProperty.Create);
            _propertyMap.Add(Command.ICEFORGING, IntProperty.Create);
            _propertyMap.Add(Command.NOBADEVENTS, IntProperty.Create);
            _propertyMap.Add(Command.INCPROVDEF, IntProperty.Create);
            _propertyMap.Add(Command.INCUNREST, IntProperty.Create);
            _propertyMap.Add(Command.LEPER, IntProperty.Create);
            _propertyMap.Add(Command.POPKILL, IntProperty.Create);
            _propertyMap.Add(Command.INSANIFY, IntProperty.Create);
            _propertyMap.Add(Command.INQUISITOR, CommandProperty.Create);
            _propertyMap.Add(Command.HERETIC, IntProperty.Create);
            _propertyMap.Add(Command.ELEGIST, IntProperty.Create);
            _propertyMap.Add(Command.SPREADDOM, IntProperty.Create);
            _propertyMap.Add(Command.SHATTEREDSOUL, IntProperty.Create);
            _propertyMap.Add(Command.TAXCOLLECTOR, CommandProperty.Create);
            _propertyMap.Add(Command.GOLD, IntProperty.Create);
            _propertyMap.Add(Command.ADDUPKEEP, IntProperty.Create);
            _propertyMap.Add(Command.XPLOSS, IntProperty.Create);
            _propertyMap.Add(Command.ALCHEMY, IntProperty.Create);
            _propertyMap.Add(Command.MASON, CommandProperty.Create);
            _propertyMap.Add(Command.INCSCALE, IntProperty.Create);
            _propertyMap.Add(Command.DECSCALE, IntProperty.Create);
            _propertyMap.Add(Command.FORTKILL, IntProperty.Create);
            _propertyMap.Add(Command.THRONEKILL, IntProperty.Create);
            _propertyMap.Add(Command.FARTHRONEKILL, IntProperty.Create);
            _propertyMap.Add(Command.LOCALSUN, CommandProperty.Create);
            _propertyMap.Add(Command.ADEPTSACR, IntProperty.Create);
            _propertyMap.Add(Command.INSPIRATIONAL, IntProperty.Create);
            _propertyMap.Add(Command.BEASTMASTER, IntProperty.Create);
            _propertyMap.Add(Command.TASKMASTER, IntProperty.Create);
            _propertyMap.Add(Command.UNDISCIPLINED, CommandProperty.Create);
            _propertyMap.Add(Command.FORMATIONFIGHTER, IntProperty.Create);
            _propertyMap.Add(Command.BODYGUARD, IntProperty.Create);
            _propertyMap.Add(Command.STANDARD, IntProperty.Create);
            _propertyMap.Add(Command.COMMAND, IntProperty.Create);
            _propertyMap.Add(Command.MAGICCOMMAND, IntProperty.Create);
            _propertyMap.Add(Command.UNDCOMMAND, IntProperty.Create);
            _propertyMap.Add(Command.SKIRMISHER, IntProperty.Create);
            _propertyMap.Add(Command.WARNING, IntProperty.Create);
            _propertyMap.Add(Command.DOUSE, IntProperty.Create);
            _propertyMap.Add(Command.RESEARCHBONUS, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHRESEARCH, IntProperty.Create);
            _propertyMap.Add(Command.INSPIRINGRES, IntProperty.Create);
            _propertyMap.Add(Command.DIVINEINS, CommandProperty.Create);
            _propertyMap.Add(Command.DRAINIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.MAGICIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.FORGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.FIXFORGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.CROSSBREEDER, IntProperty.Create);
            _propertyMap.Add(Command.BONUSSPELLS, IntProperty.Create);
            _propertyMap.Add(Command.COMSLAVE, CommandProperty.Create);
            _propertyMap.Add(Command.COMMASTER, CommandProperty.Create);
            _propertyMap.Add(Command.DEATHBANISH, IntProperty.Create);
            _propertyMap.Add(Command.KOKYTOSRET, IntProperty.Create);
            _propertyMap.Add(Command.INFERNORET, IntProperty.Create);
            _propertyMap.Add(Command.VOIDRET, IntProperty.Create);
            _propertyMap.Add(Command.ALLRET, IntProperty.Create);
            _propertyMap.Add(Command.SPELLSINGER, CommandProperty.Create);
            _propertyMap.Add(Command.FASTCAST, IntProperty.Create);
            _propertyMap.Add(Command.MAGICSTUDY, IntProperty.Create);
            _propertyMap.Add(Command.BRINGEROFFORTUNE, IntProperty.Create);
            _propertyMap.Add(Command.COMBATCASTER, CommandProperty.Create);
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
            _propertyMap.Add(Command.MAKEPEARLS, IntProperty.Create);
            _propertyMap.Add(Command.TMPFIREGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPAIRGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPWATERGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPEARTHGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPASTRALGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPDEATHGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPNATUREGEMS, IntProperty.Create);
            _propertyMap.Add(Command.CARCASSCOLLECTOR, IntProperty.Create);
            _propertyMap.Add(Command.DOMSUMMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DOMSUMMON2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DOMSUMMON20, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.RAREDOMSUMMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.TEMPLETRAINER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATTLESUM1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATTLESUM2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATTLESUM3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATTLESUM4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATTLESUM5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM1D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM2D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM3D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM4D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM5D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM6D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM7D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM8D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM9D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.IVYLORD, IntProperty.Create);
            _propertyMap.Add(Command.DRAGONLORD, IntProperty.Create);
            _propertyMap.Add(Command.LAMIALORD, IntProperty.Create);
            _propertyMap.Add(Command.CORPSELORD, IntProperty.Create);
            _propertyMap.Add(Command.ONISUMMON, IntProperty.Create);
            _propertyMap.Add(Command.REANIMPRIEST, IntProperty.Create);
            _propertyMap.Add(Command.LANDDAMAGE, IntProperty.Create);
            _propertyMap.Add(Command.BATSTARTSUM1D3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.TMPBLOODSLAVES, IntProperty.Create);
            _propertyMap.Add(Command.RESOURCES, IntProperty.Create);
            _propertyMap.Add(Command.MASTERSMITH, IntProperty.Create);
            _propertyMap.Add(Command.MASTERRIT, IntProperty.Create);
            _propertyMap.Add(Command.REFORM, IntProperty.Create);
            _propertyMap.Add(Command.SLEEPAURA, IntProperty.Create);
            _propertyMap.Add(Command.SLAVER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SLAVERBONUS, IntProperty.Create);
            _propertyMap.Add(Command.SALTVUL, IntProperty.Create);
            _propertyMap.Add(Command.SHAPECHANCE, IntProperty.Create);
        }

        public Item(string value, string comment, Mod _parent, bool selected = false) : base()
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
                Parent.ItemsWithNoNameYet.Add(this);
            }
        }

        public override void Resolve()
        {
            if (base._resolved) return;
            foreach (var m in Parent.Dependencies)
            {
                if (ID != -1 && m.Items.TryGetValue(this.ID, out var entity))
                {
                    entity.Properties.AddRange(this.Properties);
                }
                else if (this.TryGetName(out _name) && m.NamedItems.TryGetValue(_name, out var namedentity))
                {
                    namedentity.Properties.AddRange(this.Properties);
                }
            }
            base.Resolve();
        }

        public override void AddNamed(string s)
        {
            base.AddNamed(s);
            Parent.ItemsWithNoNameYet.Remove(this);
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWITEM;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTITEM;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedItems;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Items;
        }
    }
}
