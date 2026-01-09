using Dom5Edit.Entities;

namespace Dom5Edit.Commands
{
    /// <summary>
    /// Defines property groups that can be cleared by clear commands.
    /// Used to determine which properties are blocked when a clear command is present.
    /// </summary>
    public enum PropertyGroup
    {
        /// <summary>Not clearable by specific clear commands (stats, identity, etc.)</summary>
        None,
        /// <summary>Cleared by #clearweapons</summary>
        Weapons,
        /// <summary>Cleared by #cleararmor</summary>
        Armor,
        /// <summary>Cleared by #clearmagic</summary>
        Magic,
        /// <summary>Cleared by #clearspec</summary>
        Special,
        /// <summary>Sprites - excluded from #copystats, handled by #copyspr</summary>
        Sprites,
        /// <summary>Cleared by #clear (everything)</summary>
        All,
        /// <summary>Nation: Cleared by #cleargods</summary>
        Gods,
        /// <summary>Nation: Cleared by #clearsites</summary>
        Sites,
        /// <summary>Nation: Cleared by #clearnation</summary>
        NationSettings,
        /// <summary>Nation: Cleared by #clearrec</summary>
        Recruitment,
        /// <summary>Poptype: Cleared by #cleardef</summary>
        Defense
    }

    /// <summary>
    /// Maps commands to their property groups for clear command handling.
    /// </summary>
    public static class PropertyGroupMap
    {
        /// <summary>
        /// Gets the property group for a command on a Monster entity.
        /// </summary>
        public static PropertyGroup GetMonsterGroup(Command command)
        {
            // Weapons group - cleared by #clearweapons
            if (command == Command.WEAPON)
                return PropertyGroup.Weapons;

            // Armor group - cleared by #cleararmor
            if (command == Command.ARMOR)
                return PropertyGroup.Armor;

            // Magic group - cleared by #clearmagic
            if (IsMagicCommand(command))
                return PropertyGroup.Magic;

            // Sprites - excluded from copystats, use copyspr
            if (IsSpriteCommand(command))
                return PropertyGroup.Sprites;

            // Stats - NOT clearable (only via #clear or #copystats)
            if (IsStatCommand(command))
                return PropertyGroup.None;

            // Identity/structural - NOT clearable
            if (IsIdentityCommand(command))
                return PropertyGroup.None;

            // Everything else is Special - cleared by #clearspec
            return PropertyGroup.Special;
        }

        /// <summary>
        /// Gets the property group for a command on a Nation entity.
        /// </summary>
        public static PropertyGroup GetNationGroup(Command command)
        {
            // Gods - cleared by #cleargods
            if (IsNationGodsCommand(command))
                return PropertyGroup.Gods;

            // Sites - cleared by #clearsites
            if (IsNationSitesCommand(command))
                return PropertyGroup.Sites;

            // Recruitment - cleared by #clearrec
            if (IsNationRecruitmentCommand(command))
                return PropertyGroup.Recruitment;

            // Most nation settings - cleared by #clearnation
            // This includes things like ideal climate, reanimating priests, heroes, etc.
            return PropertyGroup.NationSettings;
        }

        /// <summary>
        /// Gets the clear command that clears a given property group.
        /// </summary>
        public static Command? GetClearCommand(PropertyGroup group)
        {
            return group switch
            {
                PropertyGroup.Weapons => Command.CLEARWEAPONS,
                PropertyGroup.Armor => Command.CLEARARMOR,
                PropertyGroup.Magic => Command.CLEARMAGIC,
                PropertyGroup.Special => Command.CLEARSPEC,
                PropertyGroup.All => Command.CLEAR,
                PropertyGroup.Gods => Command.CLEARGODS,
                PropertyGroup.Sites => Command.CLEARSITES,
                PropertyGroup.NationSettings => Command.CLEARNATION,
                PropertyGroup.Recruitment => Command.CLEARREC,
                PropertyGroup.Defense => Command.CLEARDEF,
                _ => null
            };
        }

        /// <summary>
        /// Checks if a command is a magic-related command (cleared by #clearmagic).
        /// </summary>
        private static bool IsMagicCommand(Command command)
        {
            return command switch
            {
                Command.MAGICSKILL => true,
                Command.CUSTOMMAGIC => true,
                Command.MAGICBOOST => true,
                // Magic path range commands are abilities, not base magic
                _ => false
            };
        }

        /// <summary>
        /// Checks if a command is a sprite command (excluded from copystats).
        /// </summary>
        private static bool IsSpriteCommand(Command command)
        {
            return command switch
            {
                Command.SPR1 => true,
                Command.SPR2 => true,
                Command.UNMOUNTEDSPR1 => true,
                Command.UNMOUNTEDSPR2 => true,
                Command.MOUNTEDSPR1 => true,
                Command.MOUNTEDSPR2 => true,
                Command.XSPR1 => true,
                Command.XSPR2 => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if a command is a base stat command (NOT clearable by specific clears).
        /// These can only be cleared by #clear or overwritten by #copystats.
        /// </summary>
        private static bool IsStatCommand(Command command)
        {
            return command switch
            {
                // Core combat stats
                Command.HP => true,
                Command.STR => true,
                Command.ATT => true,
                Command.DEF => true,
                Command.PREC => true,
                Command.PROT => true,
                Command.SIZE => true,
                Command.MR => true,
                Command.MOR => true,
                Command.MORALE => true,
                Command.ENC => true,
                Command.MAPMOVE => true,
                Command.AP => true,
                Command.EYES => true,
                Command.VOIDSANITY => true,

                // Cost stats
                Command.GCOST => true,
                Command.RCOST => true,
                Command.RPCOST => true,
                Command.MCOST => true,
                Command.RESSIZE => true,
                Command.PATHCOST => true,
                Command.HOLYCOST => true,

                // Appearance stats
                Command.DRAWSIZE => true,
                Command.SPECIALLOOK => true,

                // Age stats
                Command.STARTAGE => true,
                Command.MAXAGE => true,
                Command.OLDER => true,
                Command.ADDRANDOMAGE => true,

                // Pretender-specific
                Command.STARTDOM => true,
                Command.HOMEREALM => true,
                Command.TRIPLEGOD => true,
                Command.TRIPLEGODMAG => true,
                Command.MINPRISON => true,
                Command.MAXPRISON => true,

                _ => false
            };
        }

        /// <summary>
        /// Checks if a command is an identity/structural command that should never be cleared.
        /// </summary>
        private static bool IsIdentityCommand(Command command)
        {
            return command switch
            {
                Command.NAME => true,
                Command.FIXEDNAME => true,
                Command.DESCR => true,
                Command.COPYSTATS => true,
                Command.COPYSPR => true,
                Command.CLEAR => true,
                Command.CLEARWEAPONS => true,
                Command.CLEARARMOR => true,
                Command.CLEARMAGIC => true,
                Command.CLEARSPEC => true,
                Command.SELECTMONSTER => true,
                Command.NEWMONSTER => true,
                Command.MONTAG => true,
                Command.MONTAGWEIGHT => true,
                Command.NAMETYPE => true,
                _ => false
            };
        }

        /// <summary>
        /// Nation: Commands cleared by #cleargods
        /// </summary>
        private static bool IsNationGodsCommand(Command command)
        {
            return command switch
            {
                Command.ADDGOD => true,
                Command.DELGOD => true,
                Command.CHEAPGOD20 => true,
                Command.CHEAPGOD40 => true,
                _ => false
            };
        }

        /// <summary>
        /// Nation: Commands cleared by #clearsites
        /// </summary>
        private static bool IsNationSitesCommand(Command command)
        {
            return command switch
            {
                Command.STARTSITE => true,
                _ => false
            };
        }

        /// <summary>
        /// Nation: Commands cleared by #clearrec
        /// </summary>
        private static bool IsNationRecruitmentCommand(Command command)
        {
            return command switch
            {
                Command.ADDRECUNIT => true,
                Command.ADDRECCOM => true,
                Command.ADDFOREIGNUNIT => true,
                Command.ADDFOREIGNCOM => true,
                Command.FORESTREC => true,
                Command.FORESTCOM => true,
                Command.MOUNTAINREC => true,
                Command.MOUNTAINCOM => true,
                Command.SWAMPREC => true,
                Command.SWAMPCOM => true,
                Command.WASTEREC => true,
                Command.WASTECOM => true,
                Command.CAVEREC => true,
                Command.CAVECOM => true,
                Command.COASTREC => true,
                Command.COASTCOM => true,
                Command.STARTUNITTYPE1 => true,
                Command.STARTUNITTYPE2 => true,
                Command.STARTUNITNBRS1 => true,
                Command.STARTUNITNBRS2 => true,
                Command.STARTCOM => true,
                Command.STARTSCOUT => true,
                _ => false
            };
        }
    }
}
