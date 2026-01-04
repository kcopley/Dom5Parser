using Dom5Edit.Commands;
using Dom5Edit.Props;

namespace Dom5Edit.Entities
{
    public class Weapon : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

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
            // Dominions 6 additions:
            _propertyMap.Add(Command.THIRDSTR, CommandProperty.Create); //#thirdstr 
            _propertyMap.Add(Command.SPIRITFORMIMMUNE, CommandProperty.Create); //#spiritformimmune 
            _propertyMap.Add(Command.ILLUSIONSIMMUNE, CommandProperty.Create); //#illusionsimmune 
            _propertyMap.Add(Command.FALSE, CommandProperty.Create); //#false 
            _propertyMap.Add(Command.SPEEDMULT, IntProperty.Create); //#speedmult <1 - 3>
            _propertyMap.Add(Command.NOTMOUNTED, IntProperty.Create); //#notmounted <1 - 2>
            _propertyMap.Add(Command.NOTDISMOUNTED, CommandProperty.Create); //#notdismounted 
            _propertyMap.Add(Command.HOLYIFHIT, IntProperty.Create); //#holyifhit <dmg>
            _propertyMap.Add(Command.KILLMAGICIFHIT, IntProperty.Create); //#killmagicifhit <dmg>
            _propertyMap.Add(Command.KILLDEMONIFHIT, IntProperty.Create); //#killdemonifhit <dmg>
            _propertyMap.Add(Command.HOLYSTUNIFHIT, IntProperty.Create); //#holystunifhit <dmg>
            _propertyMap.Add(Command.PETRIFYIFHIT, IntProperty.Create); //#petrifyifhit <dmg>
            _propertyMap.Add(Command.FIREIFHIT, IntProperty.Create); //#fireifhit <dmg>
            _propertyMap.Add(Command.COLDIFHIT, IntProperty.Create); //#coldifhit <dmg>
            _propertyMap.Add(Command.SHOCKIFHIT, IntProperty.Create); //#shockifhit <dmg>
            _propertyMap.Add(Command.POISONIFDMG, IntProperty.Create); //#poisonifdmg <dmg>
            _propertyMap.Add(Command.AFTERCLOUD, IntIntProperty.Create); //#aftercloud <cloudstr> <cloudtype>
            _propertyMap.Add(Command.AFTERCLOUDAREA, IntProperty.Create); //#aftercloudarea <aoe>
            _propertyMap.Add(Command.NREFF, IntProperty.Create); //#nreff <number>
            _propertyMap.Add(Command.DEFROLL, IntProperty.Create); //#defroll <roll modifier>
            _propertyMap.Add(Command.DMGINSPECTOR, IntProperty.Create); //#dmginspector <value>
            _propertyMap.Add(Command.MORROLL, IntProperty.Create); //#morroll <roll modifier>
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

        internal override EntityType GetEntityType()
        {
            return EntityType.WEAPON;
        }

        public override bool TryGetCopyFrom(out IDEntity copy)
        {
            if (TryGet<WeaponRef>(Command.COPYWEAPON, out var statsRef, false) == ReturnType.TRUE)
            {
                copy = statsRef.Entity;
                return true;
            }
            copy = null;
            return false;
        }
    }
}
