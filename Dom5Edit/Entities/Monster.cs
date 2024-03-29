﻿using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Monster : IDEntity
    {
        private static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Monster()
        {
            _propertyMap.Add(Command.SELECTMONSTER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NEWMONSTER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NAME, NameProperty.Create); //use for name map reference if need be
            _propertyMap.Add(Command.FIXEDNAME, StringProperty.Create);
            _propertyMap.Add(Command.DESCR, StringProperty.Create);
            _propertyMap.Add(Command.SPR1, FilePathProperty.Create);
            _propertyMap.Add(Command.SPR2, FilePathProperty.Create);
            _propertyMap.Add(Command.SPECIALLOOK, IntProperty.Create);
            _propertyMap.Add(Command.DRAWSIZE, IntProperty.Create);
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARWEAPONS, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARARMOR, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARMAGIC, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARSPEC, CommandProperty.Create);
            _propertyMap.Add(Command.COPYSTATS, CopyStatsRef.Create);
            _propertyMap.Add(Command.COPYSPR, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.PATHCOST, IntProperty.Create);
            _propertyMap.Add(Command.STARTDOM, IntProperty.Create);
            _propertyMap.Add(Command.HOMEREALM, IntProperty.Create);
            _propertyMap.Add(Command.GCOST, IntProperty.Create);
            _propertyMap.Add(Command.TRIPLEGOD, IntProperty.Create);
            _propertyMap.Add(Command.TRIPLEGODMAG, IntProperty.Create);
            _propertyMap.Add(Command.UNIFY, CommandProperty.Create);
            _propertyMap.Add(Command.TRIPLE3MON, CommandProperty.Create);
            _propertyMap.Add(Command.MINPRISON, IntProperty.Create);
            _propertyMap.Add(Command.MAXPRISON, IntProperty.Create);
            _propertyMap.Add(Command.SLOWREC, CommandProperty.Create);
            _propertyMap.Add(Command.NOSLOWREC, CommandProperty.Create);
            _propertyMap.Add(Command.RECLIMIT, IntProperty.Create);
            _propertyMap.Add(Command.ENCHREBATE50, EnchIDRef.Create);
            _propertyMap.Add(Command.ENCHREBATE25P, EnchIDRef.Create);
            _propertyMap.Add(Command.ENCHREBATE50P, EnchIDRef.Create);
            _propertyMap.Add(Command.REQLAB, CommandProperty.Create);
            _propertyMap.Add(Command.NOREQLAB, CommandProperty.Create);
            _propertyMap.Add(Command.REQTEMPLE, CommandProperty.Create);
            _propertyMap.Add(Command.NOREQTEMPLE, CommandProperty.Create);
            _propertyMap.Add(Command.HEATREC, IntProperty.Create);
            _propertyMap.Add(Command.COLDREC, IntProperty.Create);
            _propertyMap.Add(Command.CHAOSREC, IntProperty.Create);
            _propertyMap.Add(Command.DEATHREC, IntProperty.Create);
            _propertyMap.Add(Command.AISINGLEREC, CommandProperty.Create);
            _propertyMap.Add(Command.AINOREC, CommandProperty.Create);
            _propertyMap.Add(Command.MONPRESENTREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.OWNSMONREC, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DOMREC, IntProperty.Create);
            _propertyMap.Add(Command.SINGLEBATTLE, CommandProperty.Create);
            _propertyMap.Add(Command.DESERTER, IntProperty.Create);
            _propertyMap.Add(Command.HORRORDESERTER, IntProperty.Create);
            _propertyMap.Add(Command.DEFECTOR, IntProperty.Create);
            _propertyMap.Add(Command.NOWISH, CommandProperty.Create);
            _propertyMap.Add(Command.RCOST, IntProperty.Create);
            _propertyMap.Add(Command.RPCOST, IntProperty.Create);
            _propertyMap.Add(Command.RESSIZE, IntProperty.Create);
            _propertyMap.Add(Command.HP, IntProperty.Create);
            _propertyMap.Add(Command.STR, IntProperty.Create);
            _propertyMap.Add(Command.ATT, IntProperty.Create);
            _propertyMap.Add(Command.DEF, IntProperty.Create);
            _propertyMap.Add(Command.PREC, IntProperty.Create);
            _propertyMap.Add(Command.PROT, IntProperty.Create);
            _propertyMap.Add(Command.SIZE, IntProperty.Create);
            _propertyMap.Add(Command.MR, IntProperty.Create);
            _propertyMap.Add(Command.MOR, IntProperty.Create);
            _propertyMap.Add(Command.ENC, IntProperty.Create);
            _propertyMap.Add(Command.MAPMOVE, IntProperty.Create);
            _propertyMap.Add(Command.AP, IntProperty.Create);
            _propertyMap.Add(Command.EYES, IntProperty.Create);
            _propertyMap.Add(Command.VOIDSANITY, IntProperty.Create);
            _propertyMap.Add(Command.WEAPON, WeaponRef.Create);
            _propertyMap.Add(Command.ARMOR, ArmorRef.Create);
            _propertyMap.Add(Command.POLYIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.HUMANOID, CommandProperty.Create);
            _propertyMap.Add(Command.MOUNTEDHUMANOID, CommandProperty.Create);
            _propertyMap.Add(Command.QUADRUPED, CommandProperty.Create);
            _propertyMap.Add(Command.LIZARD, CommandProperty.Create);
            _propertyMap.Add(Command.NAGA, CommandProperty.Create);
            _propertyMap.Add(Command.SNAKE, CommandProperty.Create);
            _propertyMap.Add(Command.BIRD, CommandProperty.Create);
            _propertyMap.Add(Command.DJINN, CommandProperty.Create);
            _propertyMap.Add(Command.TROGLODYTE, CommandProperty.Create);
            _propertyMap.Add(Command.MISCSHAPE, CommandProperty.Create);
            _propertyMap.Add(Command.STARTITEM, ItemRef.Create);
            _propertyMap.Add(Command.USERESTRICTEDITEM, RestrictedItemIDRef.Create);
            _propertyMap.Add(Command.NOITEM, CommandProperty.Create);
            _propertyMap.Add(Command.ITEMSLOTS, BitmaskProperty.Create);
            _propertyMap.Add(Command.FEMALE, CommandProperty.Create);
            _propertyMap.Add(Command.COLDBLOOD, CommandProperty.Create);
            _propertyMap.Add(Command.DRAKE, CommandProperty.Create);
            _propertyMap.Add(Command.PLANT, CommandProperty.Create);
            _propertyMap.Add(Command.LESSERHORROR, CommandProperty.Create);
            _propertyMap.Add(Command.GREATERHORROR, CommandProperty.Create);
            _propertyMap.Add(Command.DOOMHORROR, CommandProperty.Create);
            _propertyMap.Add(Command.MOUNTED, CommandProperty.Create);
            _propertyMap.Add(Command.HOLY, CommandProperty.Create);
            _propertyMap.Add(Command.ANIMAL, CommandProperty.Create);
            _propertyMap.Add(Command.UNIQUE, CommandProperty.Create);
            _propertyMap.Add(Command.UNDEAD, CommandProperty.Create);
            _propertyMap.Add(Command.BUG, CommandProperty.Create);
            _propertyMap.Add(Command.DEMON, CommandProperty.Create);
            _propertyMap.Add(Command.MAGICBEING, CommandProperty.Create);
            _propertyMap.Add(Command.AUTOCOMPETE, CommandProperty.Create);
            _propertyMap.Add(Command.BLIND, CommandProperty.Create);
            _propertyMap.Add(Command.UWBUG, CommandProperty.Create);
            _propertyMap.Add(Command.STONEBEING, CommandProperty.Create);
            _propertyMap.Add(Command.INANIMATE, CommandProperty.Create);
            _propertyMap.Add(Command.DUNGEON, CommandProperty.Create);
            _propertyMap.Add(Command.LANCEOK, CommandProperty.Create);
            _propertyMap.Add(Command.IMMOBILE, CommandProperty.Create);
            _propertyMap.Add(Command.AQUATIC, CommandProperty.Create);
            _propertyMap.Add(Command.AMPHIBIAN, CommandProperty.Create);
            _propertyMap.Add(Command.POORAMPHIBIAN, CommandProperty.Create);
            _propertyMap.Add(Command.FLOAT, CommandProperty.Create);
            _propertyMap.Add(Command.FLYING, CommandProperty.Create);
            _propertyMap.Add(Command.SWIMMING, CommandProperty.Create);
            _propertyMap.Add(Command.SNOW, CommandProperty.Create);
            _propertyMap.Add(Command.STORMIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.TELEPORT, CommandProperty.Create);
            _propertyMap.Add(Command.MAPTELEPORT, CommandProperty.Create);
            _propertyMap.Add(Command.BLINK, CommandProperty.Create);
            _propertyMap.Add(Command.UNTELEPORTABLE, CommandProperty.Create);
            _propertyMap.Add(Command.NORIVERPASS, CommandProperty.Create);
            _propertyMap.Add(Command.FORESTSURVIVAL, CommandProperty.Create);
            _propertyMap.Add(Command.MOUNTAINSURVIVAL, CommandProperty.Create);
            _propertyMap.Add(Command.SWAMPSURVIVAL, CommandProperty.Create);
            _propertyMap.Add(Command.WASTESURVIVAL, CommandProperty.Create);
            _propertyMap.Add(Command.SAILING, IntIntProperty.Create);
            _propertyMap.Add(Command.GIFTOFWATER, IntProperty.Create);
            _propertyMap.Add(Command.INDEPMOVE, IntProperty.Create);
            _propertyMap.Add(Command.INDEPSTAY, IntProperty.Create);
            _propertyMap.Add(Command.NORANGE, CommandProperty.Create);
            _propertyMap.Add(Command.NOMOVEPEN, CommandProperty.Create);
            _propertyMap.Add(Command.FARSAIL, IntProperty.Create);
            _propertyMap.Add(Command.ILLUSION, CommandProperty.Create);
            _propertyMap.Add(Command.SEDUCE, IntProperty.Create);
            _propertyMap.Add(Command.SUCCUBUS, IntProperty.Create);
            _propertyMap.Add(Command.STEALTHY, IntProperty.Create);
            _propertyMap.Add(Command.SPY, CommandProperty.Create);
            _propertyMap.Add(Command.ASSASSIN, CommandProperty.Create);
            _propertyMap.Add(Command.PATIENCE, IntProperty.Create);
            _propertyMap.Add(Command.SCALEWALLS, CommandProperty.Create);
            _propertyMap.Add(Command.BECKON, IntProperty.Create);
            _propertyMap.Add(Command.FALSEARMY, IntProperty.Create);
            _propertyMap.Add(Command.FOOLSCOUTS, IntProperty.Create);
            _propertyMap.Add(Command.STARTAGE, IntProperty.Create);
            _propertyMap.Add(Command.MAXAGE, IntProperty.Create);
            _propertyMap.Add(Command.OLDER, IntProperty.Create);
            _propertyMap.Add(Command.ADDRANDOMAGE, IntProperty.Create);
            _propertyMap.Add(Command.UWDAMAGE, IntProperty.Create);
            _propertyMap.Add(Command.HEAL, CommandProperty.Create);
            _propertyMap.Add(Command.NOHEAL, CommandProperty.Create);
            _propertyMap.Add(Command.LANDDAMAGE, IntProperty.Create);
            _propertyMap.Add(Command.HEALER, IntProperty.Create);
            _propertyMap.Add(Command.HOMESICK, IntProperty.Create);
            _propertyMap.Add(Command.AUTOHEALER, IntProperty.Create);
            _propertyMap.Add(Command.AUTODISHEALER, IntProperty.Create);
            _propertyMap.Add(Command.AUTODISGRINDER, IntProperty.Create);
            _propertyMap.Add(Command.DISEASERES, IntProperty.Create);
            _propertyMap.Add(Command.WOUNDFEND, IntProperty.Create);
            _propertyMap.Add(Command.HPOVERFLOW, CommandProperty.Create);
            _propertyMap.Add(Command.HPOVERSLOW, IntProperty.Create);
            _propertyMap.Add(Command.CORPSEEATER, IntProperty.Create);
            _propertyMap.Add(Command.DEADHP, IntProperty.Create);
            _propertyMap.Add(Command.STARTAFF, IntProperty.Create);
            _propertyMap.Add(Command.STARTMAJORAFF, IntProperty.Create);
            _propertyMap.Add(Command.STARTINGAFF, BitmaskProperty.Create);
            _propertyMap.Add(Command.INSANE, IntProperty.Create);
            _propertyMap.Add(Command.STARTHEROAB, IntProperty.Create);
            _propertyMap.Add(Command.BLUNTRES, CommandProperty.Create);
            _propertyMap.Add(Command.ETHEREAL, CommandProperty.Create);
            _propertyMap.Add(Command.COLDRES, IntProperty.Create);
            _propertyMap.Add(Command.FIRERES, IntProperty.Create);
            _propertyMap.Add(Command.POISONRES, IntProperty.Create);
            _propertyMap.Add(Command.SHOCKRES, IntProperty.Create);
            _propertyMap.Add(Command.ICEPROT, IntProperty.Create);
            _propertyMap.Add(Command.INVULNERABLE, IntProperty.Create);
            _propertyMap.Add(Command.REGENERATION, IntProperty.Create);
            _propertyMap.Add(Command.DOHEAL, CommandProperty.Create);
            _propertyMap.Add(Command.UNDREGEN, IntProperty.Create);
            _propertyMap.Add(Command.UWREGEN, IntProperty.Create);
            _propertyMap.Add(Command.PIERCERES, CommandProperty.Create);
            _propertyMap.Add(Command.SLASHRES, CommandProperty.Create);
            _propertyMap.Add(Command.REINVIGORATION, IntProperty.Create);
            _propertyMap.Add(Command.AIRSHIELD, IntProperty.Create);
            _propertyMap.Add(Command.IRONVUL, IntProperty.Create);
            _propertyMap.Add(Command.IMMORTAL, CommandProperty.Create);
            _propertyMap.Add(Command.DOMIMMORTAL, CommandProperty.Create);
            _propertyMap.Add(Command.REFORMTIME, IntProperty.Create);
            _propertyMap.Add(Command.SPRINGIMMORTAL, CommandProperty.Create);
            _propertyMap.Add(Command.REFORM, IntProperty.Create);
            _propertyMap.Add(Command.BUGREFORM, IntProperty.Create);
            _propertyMap.Add(Command.HEAT, IntProperty.Create);
            _propertyMap.Add(Command.COLD, IntProperty.Create);
            _propertyMap.Add(Command.UWHEAT, IntProperty.Create);
            _propertyMap.Add(Command.POISONARMOR, IntProperty.Create);
            _propertyMap.Add(Command.POISONSKIN, IntProperty.Create);
            _propertyMap.Add(Command.POISONCLOUD, IntProperty.Create);
            _propertyMap.Add(Command.DISEASECLOUD, IntProperty.Create);
            _propertyMap.Add(Command.ANIMALAWE, IntProperty.Create);
            _propertyMap.Add(Command.AWE, IntProperty.Create);
            _propertyMap.Add(Command.SUNAWE, IntProperty.Create);
            _propertyMap.Add(Command.HALTHERETIC, IntProperty.Create);
            _propertyMap.Add(Command.FEAR, IntProperty.Create);
            _propertyMap.Add(Command.FIRESHIELD, IntProperty.Create);
            _propertyMap.Add(Command.UWFIRESHIELD, IntProperty.Create);
            _propertyMap.Add(Command.BANEFIRESHIELD, IntProperty.Create);
            _propertyMap.Add(Command.ACIDSHIELD, IntProperty.Create);
            _propertyMap.Add(Command.CURSELUCKSHIELD, IntProperty.Create);
            _propertyMap.Add(Command.DAMAGEREV, IntProperty.Create);
            _propertyMap.Add(Command.BLOODVENGEANCE, IntProperty.Create);
            _propertyMap.Add(Command.SLIMER, IntProperty.Create);
            _propertyMap.Add(Command.ENTANGLE, CommandProperty.Create);
            _propertyMap.Add(Command.EYELOSS, CommandProperty.Create);
            _propertyMap.Add(Command.HORRORMARK, CommandProperty.Create);
            _propertyMap.Add(Command.MINDSLIME, IntProperty.Create);
            _propertyMap.Add(Command.OVERCHARGED, IntProperty.Create);
            _propertyMap.Add(Command.SLEEPAURA, IntProperty.Create);
            _propertyMap.Add(Command.SPRINGPOWER, IntProperty.Create);
            _propertyMap.Add(Command.SUMMERPOWER, IntProperty.Create);
            _propertyMap.Add(Command.FALLPOWER, IntProperty.Create);
            _propertyMap.Add(Command.WINTERPOWER, IntProperty.Create);
            _propertyMap.Add(Command.YEARTURN, IntProperty.Create);
            _propertyMap.Add(Command.CHAOSPOWER, IntProperty.Create);
            _propertyMap.Add(Command.COLDPOWER, IntProperty.Create);
            _propertyMap.Add(Command.FIREPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DEATHPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DARKPOWER, IntProperty.Create);
            _propertyMap.Add(Command.STORMPOWER, IntProperty.Create);
            _propertyMap.Add(Command.MAGICPOWER, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHPOWER, IntProperty.Create);
            _propertyMap.Add(Command.DOMPOWER, IntProperty.Create);
            _propertyMap.Add(Command.AMBIDEXTROUS, IntProperty.Create);
            _propertyMap.Add(Command.BERSERK, IntProperty.Create);
            _propertyMap.Add(Command.BLESSBERS, CommandProperty.Create);
            _propertyMap.Add(Command.BLESSFLY, CommandProperty.Create);
            _propertyMap.Add(Command.DARKVISION, IntProperty.Create);
            _propertyMap.Add(Command.SPIRITSIGHT, CommandProperty.Create);
            _propertyMap.Add(Command.INVISIBLE, CommandProperty.Create);
            _propertyMap.Add(Command.DEATHCURSE, CommandProperty.Create);
            _propertyMap.Add(Command.DEATHDISEASE, IntProperty.Create);
            _propertyMap.Add(Command.DEATHPARALYZE, IntProperty.Create);
            _propertyMap.Add(Command.DEATHFIRE, IntProperty.Create);
            _propertyMap.Add(Command.GUARDSPIRITBONUS, IntProperty.Create);
            _propertyMap.Add(Command.TRAMPSWALLOW, CommandProperty.Create);
            _propertyMap.Add(Command.RAISEONKILL, IntProperty.Create);
            _propertyMap.Add(Command.TRAMPLE, CommandProperty.Create);
            _propertyMap.Add(Command.DIGEST, IntProperty.Create);
            _propertyMap.Add(Command.ACIDDIGEST, IntProperty.Create);
            _propertyMap.Add(Command.INCORPORATE, IntProperty.Create);
            _propertyMap.Add(Command.RAISESHAPE, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BEARTATTOO, IntProperty.Create);
            _propertyMap.Add(Command.HORSETATTOO, IntProperty.Create);
            _propertyMap.Add(Command.WOLFTATTOO, IntProperty.Create);
            _propertyMap.Add(Command.BOARTATTOO, IntProperty.Create);
            _propertyMap.Add(Command.SNAKETATTOO, IntProperty.Create);
            _propertyMap.Add(Command.CASTLEDEF, IntProperty.Create);
            _propertyMap.Add(Command.SIEGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.PATROLBONUS, IntProperty.Create);
            _propertyMap.Add(Command.PILLAGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.INQUISITOR, CommandProperty.Create);
            _propertyMap.Add(Command.SUPPLYBONUS, IntProperty.Create);
            _propertyMap.Add(Command.HERETIC, IntProperty.Create);
            _propertyMap.Add(Command.RESOURCES, IntProperty.Create);
            _propertyMap.Add(Command.ICEFORGING, IntProperty.Create);
            _propertyMap.Add(Command.NEEDNOTEAT, CommandProperty.Create);
            _propertyMap.Add(Command.ELEGIST, IntProperty.Create);
            _propertyMap.Add(Command.SPREADDOM, IntProperty.Create);
            _propertyMap.Add(Command.NOBADEVENTS, IntProperty.Create);
            _propertyMap.Add(Command.SHATTEREDSOUL, IntProperty.Create);
            _propertyMap.Add(Command.INCUNREST, IntProperty.Create);
            _propertyMap.Add(Command.INCPROVDEF, IntProperty.Create);
            _propertyMap.Add(Command.TAXCOLLECTOR, CommandProperty.Create);
            _propertyMap.Add(Command.GOLD, IntProperty.Create);
            _propertyMap.Add(Command.ADDUPKEEP, IntProperty.Create);
            _propertyMap.Add(Command.LEPER, IntProperty.Create);
            _propertyMap.Add(Command.POPKILL, IntProperty.Create);
            _propertyMap.Add(Command.INSANIFY, IntProperty.Create);
            _propertyMap.Add(Command.NOHOF, CommandProperty.Create);
            _propertyMap.Add(Command.ALCHEMY, IntProperty.Create);
            _propertyMap.Add(Command.MASON, CommandProperty.Create);
            _propertyMap.Add(Command.INCSCALE, IntProperty.Create);
            _propertyMap.Add(Command.DECSCALE, IntProperty.Create);
            _propertyMap.Add(Command.FORTKILL, IntProperty.Create);
            _propertyMap.Add(Command.THRONEKILL, IntProperty.Create);
            _propertyMap.Add(Command.FARTHRONEKILL, IntProperty.Create);
            _propertyMap.Add(Command.LOCALSUN, CommandProperty.Create);
            _propertyMap.Add(Command.ADEPTSACR, IntProperty.Create);
            _propertyMap.Add(Command.SHAPECHANGE, ShapechangeRef.Create);
            _propertyMap.Add(Command.PROPHETSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.FIRSTSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.SECONDSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.SECONDTMPSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.FORESTSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.PLAINSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.FOREIGNSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.HOMESHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.DOMSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.NOTDOMSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.SPRINGSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.SUMMERSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.AUTUMNSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.WINTERSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.GROWHP, IntProperty.Create);
            _propertyMap.Add(Command.SHRINKHP, IntProperty.Create);
            _propertyMap.Add(Command.XPSHAPE, IntProperty.Create);
            _propertyMap.Add(Command.XPLOSS, IntProperty.Create);
            _propertyMap.Add(Command.TRANSFORMATION, IntProperty.Create);
            _propertyMap.Add(Command.FIREATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.AIRATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.WATERATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.EARTHATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.ASTRALATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.DEATHATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.NATUREATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.BLOODATTUNED, IntProperty.Create);
            _propertyMap.Add(Command.CLEANSHAPE, CommandProperty.Create);
            _propertyMap.Add(Command.LANDSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.WATERSHAPE, ShapechangeRef.Create);
            _propertyMap.Add(Command.TWICEBORN, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.REANIMATOR, IntProperty.Create);
            _propertyMap.Add(Command.REANIMPRIEST, IntProperty.Create);
            _propertyMap.Add(Command.DOMSUMMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DOMSUMMON2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DOMSUMMON20, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.RAREDOMSUMMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.TEMPLETRAINER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MAKEMONSTERS5, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON2, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON4, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SUMMON5, MonsterOrMontagRef.Create);
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
            _propertyMap.Add(Command.BATSTARTSUM1D3, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM1D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM2D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM3D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM4D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM5D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM6D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM7D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM8D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.BATSTARTSUM9D6, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.MONTAG, MontagIDRef.Create);
            _propertyMap.Add(Command.MONTAGWEIGHT, IntProperty.Create);
            _propertyMap.Add(Command.IVYLORD, IntProperty.Create);
            _propertyMap.Add(Command.DRAGONLORD, IntProperty.Create);
            _propertyMap.Add(Command.LAMIALORD, IntProperty.Create);
            _propertyMap.Add(Command.CORPSELORD, IntProperty.Create);
            _propertyMap.Add(Command.ONISUMMON, IntProperty.Create);
            _propertyMap.Add(Command.SLAVER, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.SLAVERBONUS, IntProperty.Create);
            _propertyMap.Add(Command.NAMETYPE, NametypeIDRef.Create);
            _propertyMap.Add(Command.NOLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.POORLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.OKLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.GOODLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.EXPERTLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.SUPERIORLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.COMMAND, IntProperty.Create);
            _propertyMap.Add(Command.NOMAGICLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.POORMAGICLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.OKMAGICLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.GOODMAGICLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.EXPERTMAGICLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.SUPERIORMAGICLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.MAGICCOMMAND, IntProperty.Create);
            _propertyMap.Add(Command.NOUNDEADLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.POORUNDEADLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.OKUNDEADLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.GOODUNDEADLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.EXPERTUNDEADLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.SUPERIORUNDEADLEADER, CommandProperty.Create);
            _propertyMap.Add(Command.UNDCOMMAND, IntProperty.Create);
            _propertyMap.Add(Command.ALMOSTUNDEAD, CommandProperty.Create);
            _propertyMap.Add(Command.ALMOSTLIVING, CommandProperty.Create);
            _propertyMap.Add(Command.INSPIRATIONAL, IntProperty.Create);
            _propertyMap.Add(Command.BEASTMASTER, IntProperty.Create);
            _propertyMap.Add(Command.TASKMASTER, IntProperty.Create);
            _propertyMap.Add(Command.SLAVE, CommandProperty.Create);
            _propertyMap.Add(Command.UNDISCIPLINED, CommandProperty.Create);
            _propertyMap.Add(Command.FORMATIONFIGHTER, IntProperty.Create);
            _propertyMap.Add(Command.BODYGUARD, IntProperty.Create);
            _propertyMap.Add(Command.WARNING, IntProperty.Create);
            _propertyMap.Add(Command.STANDARD, IntProperty.Create);
            _propertyMap.Add(Command.LATEHERO, IntProperty.Create);
            _propertyMap.Add(Command.MAGICSKILL, IntIntProperty.Create);
            _propertyMap.Add(Command.CUSTOMMAGIC, BitmaskChanceProperty.Create);
            _propertyMap.Add(Command.MAGICBOOST, IntIntProperty.Create);
            _propertyMap.Add(Command.MASTERRIT, IntProperty.Create);
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
            _propertyMap.Add(Command.FIXEDRESEARCH, IntProperty.Create);
            _propertyMap.Add(Command.RESEARCHBONUS, IntProperty.Create);
            _propertyMap.Add(Command.INSPIRINGRES, IntProperty.Create);
            _propertyMap.Add(Command.SLOTHRESEARCH, IntProperty.Create);
            _propertyMap.Add(Command.DRAINIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.MAGICIMMUNE, CommandProperty.Create);
            _propertyMap.Add(Command.DIVINEINS, CommandProperty.Create);
            _propertyMap.Add(Command.GEMPROD, IntIntProperty.Create);
            _propertyMap.Add(Command.TMPFIREGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPAIRGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPWATERGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPEARTHGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPASTRALGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPDEATHGEMS, IntProperty.Create);
            _propertyMap.Add(Command.TMPNATUREGEMS, IntProperty.Create);
            _propertyMap.Add(Command.DOUSE, IntProperty.Create);
            _propertyMap.Add(Command.MAKEPEARLS, IntProperty.Create);
            _propertyMap.Add(Command.CARCASSCOLLECTOR, IntProperty.Create);
            _propertyMap.Add(Command.BONUSSPELLS, IntProperty.Create);
            _propertyMap.Add(Command.ONEBATTLESPELL, SpellRef.Create);
            _propertyMap.Add(Command.CROSSBREEDER, IntProperty.Create);
            _propertyMap.Add(Command.DEATHBANISH, IntProperty.Create);
            _propertyMap.Add(Command.KOKYTOSRET, IntProperty.Create);
            _propertyMap.Add(Command.INFERNORET, IntProperty.Create);
            _propertyMap.Add(Command.VOIDRET, IntProperty.Create);
            _propertyMap.Add(Command.ALLRET, IntProperty.Create);
            _propertyMap.Add(Command.RANDOMSPELL, IntProperty.Create);
            _propertyMap.Add(Command.TAINTED, IntProperty.Create);
            _propertyMap.Add(Command.FORGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.FIXFORGEBONUS, IntProperty.Create);
            _propertyMap.Add(Command.MASTERSMITH, IntProperty.Create);
            _propertyMap.Add(Command.COMMASTER, CommandProperty.Create);
            _propertyMap.Add(Command.COMSLAVE, CommandProperty.Create);
            _propertyMap.Add(Command.INDEPSPELLS, IntProperty.Create);
            _propertyMap.Add(Command.FASTCAST, IntProperty.Create);
            _propertyMap.Add(Command.SPELLSINGER, CommandProperty.Create);
            _propertyMap.Add(Command.MAGICSTUDY, IntProperty.Create);
            _propertyMap.Add(Command.BRINGEROFFORTUNE, IntProperty.Create);
            _propertyMap.Add(Command.COMBATCASTER, CommandProperty.Create);
            _propertyMap.Add(Command.UNSURR, IntProperty.Create);
            _propertyMap.Add(Command.SKIRMISHER, IntProperty.Create);
            _propertyMap.Add(Command.MINSIZELEADER, IntProperty.Create);
            _propertyMap.Add(Command.MCOST, IntProperty.Create);
            _propertyMap.Add(Command.LIMITEDREGEN, IntProperty.Create);
            _propertyMap.Add(Command.ENCHREBATE10, EnchIDRef.Create);
            _propertyMap.Add(Command.ENCHREBATE20, EnchIDRef.Create);
            _propertyMap.Add(Command.SAILSIZE, IntProperty.Create);
            _propertyMap.Add(Command.MINDVESSEL, IntProperty.Create);
            _propertyMap.Add(Command.SALTVUL, IntProperty.Create);
            _propertyMap.Add(Command.SHAPECHANCE, IntProperty.Create);
            _propertyMap.Add(Command.NOTSACRED, CommandProperty.Create);
            _propertyMap.Add(Command.APPETITE, IntProperty.Create);
        }

        public Monster(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        public static Monster SelectVanillaMonster(int id, Mod m)
        {
            return new Monster(id.ToString(), "", m, true);
        }

        public static Monster GetNewMonster(int id, Mod m)
        {
            return new Monster(id.ToString(), "", m, false);
        }

        public override void Resolve()
        {
            if (base._resolved) return;
            foreach (var m in Parent.Dependencies)
            {
                if (ID != -1 && m.Monsters.TryGetValue(this.ID, out var entity))
                {
                    entity.Properties.AddRange(this.Properties);
                }
                else if (this.TryGetName(out _name) && m.NamedMonsters.TryGetValue(_name, out var namedentity))
                {
                    namedentity.Properties.AddRange(this.Properties);
                }
            }
            base.Resolve();
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWMONSTER;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTMONSTER;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedMonsters;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Monsters;
        }

        public IEnumerable<MagicSkill> MagicSkills
        {
            get
            {
                var list = this.Properties.FindAll(
                    delegate (Property p)
                    {
                        return p._command == Command.MAGICSKILL;
                    }).Cast<IntIntProperty>();
                foreach (var property in list)
                {
                    yield return new MagicSkill() { Path = (MagicPaths)property.Value1, Level = property.Value2 };
                }
            }
        }

        public IEnumerable<FilePathProperty> Sprites
        {
            get
            {
                var list = this.Properties.FindAll(
                    delegate (Property p)
                    {
                        return p._command == Command.SPR1 || p._command == Command.SPR2;
                    }).Cast<FilePathProperty>();
                return list;
            }
        }

        public IEnumerable<CustomMagic> CustomMagic
        {
            get
            {
                var list = this.Properties.FindAll(
                    delegate (Property p)
                    {
                        return p._command == Command.CUSTOMMAGIC;
                    }).Cast<BitmaskChanceProperty>();
                foreach (var property in list)
                {
                    ulong fire = 128;
                    ulong air = 256;
                    ulong water = 512;
                    ulong earth = 1024;
                    ulong astral = 2048;
                    ulong death = 4096;
                    ulong nature = 8192;
                    ulong blood = 16384;
                    ulong priest = 32768;

                    List<MagicPaths> paths = new List<MagicPaths>();
                    if ((property.Bitmask & fire) == fire) paths.Add(MagicPaths.FIRE);
                    if ((property.Bitmask & air) == air) paths.Add(MagicPaths.AIR);
                    if ((property.Bitmask & water) == water) paths.Add(MagicPaths.WATER);
                    if ((property.Bitmask & earth) == earth) paths.Add(MagicPaths.EARTH);
                    if ((property.Bitmask & astral) == astral) paths.Add(MagicPaths.ASTRAL);
                    if ((property.Bitmask & death) == death) paths.Add(MagicPaths.DEATH);
                    if ((property.Bitmask & nature) == nature) paths.Add(MagicPaths.NATURE);
                    if ((property.Bitmask & blood) == blood) paths.Add(MagicPaths.BLOOD);
                    if ((property.Bitmask & priest) == priest) paths.Add(MagicPaths.PRIEST);
                    double chance = ((double)property.Chance) / 100;
                    if (chance < 1) chance = chance / paths.Count;
                    yield return new CustomMagic() { Path = paths, Chance = chance };
                }
            }
        }
    }
}
