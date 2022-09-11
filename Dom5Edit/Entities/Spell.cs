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
            _propertyMap.Add(Command.DAMAGE, SpellDamage.Create);
            _propertyMap.Add(Command.DAMAGEMON, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NEXTSPELL, SpellRef.Create);
            _propertyMap.Add(Command.NEXTINGEO, SpellRef.Create);
            _propertyMap.Add(Command.EFFECT, SpellEffect.Create);
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

        public Spell(string value, string comment, Mod _parent, bool selected = false) : base()
        {
            Parent = _parent;
            Selected = selected;
            if (selected)
            {
                this.SetID(value, comment);
                if (ID == -1 && value.Length > 0)
                {
                    _name = value;
                    Named = true;
                    try
                    {
                        GetNamedList().Add(_name.ToLower(), this);
                    }
                    catch
                    {
                        Parent.Log("Spell name: " + _name + " was already used inside mod");
                    }
                }
                else if (ID != -1)
                {
                    try
                    {
                        GetIDList().Add(ID, this);
                    }
                    catch
                    {
                        Parent.Log("Spell ID: " + ID + " was already used inside mod");
                    }
                }
            }
            if (!selected)
            {
                Parent.SpellsWithNoNameYet.Add(this);
                ID = -1;
            }
        }

        public override void Export(StreamWriter writer)
        {
            Selected = true;
            base.Export(writer);
        }

        public override void Resolve()
        {
            if (base._resolved) return;
            foreach (var m in Parent.Dependencies)
            {
                if (ID != -1 && m.Spells.TryGetValue(this.ID, out var entity))
                {
                    entity.Properties.AddRange(this.Properties);
                }
                else if (!string.IsNullOrEmpty(this._name) && m.NamedSpells.TryGetValue(_name, out var namedentity1))
                {
                    namedentity1.Properties.AddRange(this.Properties);
                }
                else if (string.IsNullOrEmpty(this._name) && this.TryGetName(out _name) && m.NamedSpells.TryGetValue(_name, out var namedentity2))
                {
                    namedentity2.Properties.AddRange(this.Properties);
                }
            }
            base.Resolve();
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

        internal bool IsSummon()
        {
            //check for the spell effect
            //if no, check for a copyspell
            //if copyspell is in the mod, check it for effect
            //if no effect, back to copyspell check
            //if copyspell is not in the mod, check the vanilla list
            //return the effect
            bool hasSpellEffect = TryGetSpellEffect(out int effect);
            if (hasSpellEffect)
            {
                return VanillaSpellMap.IsSummonEffect(effect);
            }
            else if (this.IsVanillaID())
            {
                return this.IsVanillaSummon();
            }
            else if (TryGetCopySpellRef(out var copy))
            {
                if (copy.IsVanillaID())
                {
                    if (copy.IsSummon())
                        return true;
                    else return false;
                }
                else if (copy.TryGetSpell(out Spell spell))
                {
                    return spell.IsSummon();
                }
            }
            return false;
        }

        public bool IsVanillaID()
        {
            if (this.ID != -1 && VanillaSpellMap.ContainsSpell(this.ID)) return true;
            else if (this.ID != -1 && this.Named && VanillaSpellMap.ContainsSpell(this?._name)) return true;
            return false;
        }

        public bool IsVanillaSummon()
        {
            if (this.ID != -1 && VanillaSpellMap.IsSummonSpell(this.ID)) return true;
            else if (this.ID != -1 && this.Named && VanillaSpellMap.IsSummonSpell(this?._name)) return true;
            return false;
        }

        public bool IsVanillaEnchant()
        {
            if (this.ID != -1 && VanillaSpellMap.IsEnchantSpell(this.ID)) return true;
            else if (this.ID != -1 && this.Named && VanillaSpellMap.IsEnchantSpell(this?._name)) return true;
            return false;
        }

        public bool IsVanillaEventEffect()
        {
            if (this.ID != -1 && VanillaSpellMap.IsEventEffectSpell(this.ID)) return true;
            else if (this.ID != -1 && this.Named && VanillaSpellMap.IsEventEffectSpell(this?._name)) return true;
            return false;
        }

        internal bool IsEnchant()
        {
            //check for the spell effect
            //if no, check for a copyspell
            //if copyspell is in the mod, check it for effect
            //if no effect, back to copyspell check
            //if copyspell is not in the mod, check the vanilla list
            //return the effect
            if (TryGetSpellEffect(out int effect) && VanillaSpellMap.IsEnchantEffect(effect)) return true;
            else if (this.IsVanillaID())
            {
                return this.IsVanillaEnchant();
            }
            else if (TryGetCopySpellRef(out var copy))
            {
                if (copy.IsVanillaID())
                {
                    if (copy.IsEnchant())
                        return true;
                    else return false;
                }
                else if (copy.TryGetSpell(out Spell spell))
                {
                    return spell.IsEnchant();
                }
            }
            return false;
        }

        internal bool IsEventEffect()
        {
            //check for the spell effect
            //if no, check for a copyspell
            //if copyspell is in the mod, check it for effect
            //if no effect, back to copyspell check
            //if copyspell is not in the mod, check the vanilla list
            //return the effect
            if (TryGetSpellEffect(out int effect) && VanillaSpellMap.IsEventEffect(effect)) return true;
            else if (this.IsVanillaID())
            {
                return this.IsVanillaEventEffect();
            }
            else if (TryGetCopySpellRef(out var copy))
            {
                if (copy.IsVanillaID())
                {
                    if (copy.IsEventEffect())
                        return true;
                    else return false;
                }
                else if (copy.TryGetSpell(out Spell spell))
                {
                    return spell.IsEventEffect();
                }
            }
            return false;
        }

        internal bool TryGetCopySpellRef(out CopySpellRef copy)
        {
            foreach (var prop in this.Properties)
            {
                if (prop is CopySpellRef)
                {
                    copy = (CopySpellRef)prop;
                    return true;
                }
            }
            copy = null;
            return false;
        }

        internal bool TryGetCopySpell(out Spell copy)
        {
            if (TryGetCopySpellRef(out var copyRef))
            {
                if (copyRef.Entity != null && copyRef.Entity is Spell)
                {
                    copy = (Spell)copyRef.Entity;
                    return true;
                }
            }
            copy = null;
            return false;
        }

        internal bool TryGetSpellEffectRef(out SpellEffect effect)
        {
            foreach (var prop in this.Properties)
            {
                if (prop is SpellEffect)
                {
                    effect = (SpellEffect)prop;
                    return true;
                }
            }
            effect = null;
            return false;
        }

        internal bool TryGetSpellEffect(out int effect)
        {
            if (TryGetSpellEffectRef(out var copyRef))
            {
                if (copyRef.HasValue && copyRef.Value != -1)
                {
                    effect = copyRef.Value;
                    return true;
                }
            }
            effect = -1;
            return false;
        }
    }

    public class VanillaSpellMap
    {
        private static Dictionary<int, int> SpellIDEffectMap = new Dictionary<int, int>();
        private static Dictionary<string, int> SpellNameEffectMap = new Dictionary<string, int>();
        private static List<int> _summonEffects = new List<int>()
        {
            1,
            21,
            26,
            37,
            38,
            43,
            50,
            54,
            62,
            89,
            93,
            119,
            130,
            137
        };
        private static List<int> _enchantEffects = new List<int>()
        {
            81,
            82,
            83,
            84,
            85,
            86
        };
        private static List<int> _eventEffects = new List<int>()
        {
            42
        };

        static VanillaSpellMap()
        {
            //maps vanilla spell ID -> spell enchant type
            SpellIDEffectMap.Add(0, 0);
            SpellIDEffectMap.Add(1, 109);
            SpellIDEffectMap.Add(2, 2);
            SpellIDEffectMap.Add(3, 3);
            SpellIDEffectMap.Add(4, 600);
            SpellIDEffectMap.Add(5, 2);
            SpellIDEffectMap.Add(6, 109);
            SpellIDEffectMap.Add(7, 4);
            SpellIDEffectMap.Add(8, 7);
            SpellIDEffectMap.Add(9, 3);
            SpellIDEffectMap.Add(10, 1);
            SpellIDEffectMap.Add(11, 11);
            SpellIDEffectMap.Add(12, 1);
            SpellIDEffectMap.Add(13, 11);
            SpellIDEffectMap.Add(14, 15);
            SpellIDEffectMap.Add(15, 66);
            SpellIDEffectMap.Add(16, 1);
            SpellIDEffectMap.Add(17, 1);
            SpellIDEffectMap.Add(18, 1);
            SpellIDEffectMap.Add(19, 10);
            SpellIDEffectMap.Add(20, 11);
            SpellIDEffectMap.Add(21, 4);
            SpellIDEffectMap.Add(22, 1);
            SpellIDEffectMap.Add(23, 1);
            SpellIDEffectMap.Add(24, 11);
            SpellIDEffectMap.Add(25, 1);
            SpellIDEffectMap.Add(26, 1);
            SpellIDEffectMap.Add(27, 10);
            SpellIDEffectMap.Add(28, 1);
            SpellIDEffectMap.Add(29, 1);
            SpellIDEffectMap.Add(30, 38);
            SpellIDEffectMap.Add(31, 2);
            SpellIDEffectMap.Add(32, 21);
            SpellIDEffectMap.Add(33, 21);
            SpellIDEffectMap.Add(34, 81);
            SpellIDEffectMap.Add(35, 11);
            SpellIDEffectMap.Add(36, 1);
            SpellIDEffectMap.Add(37, 1);
            SpellIDEffectMap.Add(38, 1);
            SpellIDEffectMap.Add(39, 3);
            SpellIDEffectMap.Add(40, 11);
            SpellIDEffectMap.Add(41, 599);
            SpellIDEffectMap.Add(42, 66);
            SpellIDEffectMap.Add(43, 11);
            SpellIDEffectMap.Add(44, 11);
            SpellIDEffectMap.Add(45, 1);
            SpellIDEffectMap.Add(46, 37);
            SpellIDEffectMap.Add(47, 11);
            SpellIDEffectMap.Add(48, 1);
            SpellIDEffectMap.Add(49, 1);
            SpellIDEffectMap.Add(50, 1);
            SpellIDEffectMap.Add(51, 1);
            SpellIDEffectMap.Add(52, 1);
            SpellIDEffectMap.Add(53, 1);
            SpellIDEffectMap.Add(54, 23);
            SpellIDEffectMap.Add(55, 1);
            SpellIDEffectMap.Add(56, 21);
            SpellIDEffectMap.Add(57, 85);
            SpellIDEffectMap.Add(58, 21);
            SpellIDEffectMap.Add(59, 1);
            SpellIDEffectMap.Add(60, 1);
            SpellIDEffectMap.Add(61, 1);
            SpellIDEffectMap.Add(62, 81);
            SpellIDEffectMap.Add(63, 66);
            SpellIDEffectMap.Add(64, 11);
            SpellIDEffectMap.Add(65, 1);
            SpellIDEffectMap.Add(66, 81);
            SpellIDEffectMap.Add(67, 1);
            SpellIDEffectMap.Add(68, 1);
            SpellIDEffectMap.Add(69, 1);
            SpellIDEffectMap.Add(70, 81);
            SpellIDEffectMap.Add(71, 105);
            SpellIDEffectMap.Add(72, 1);
            SpellIDEffectMap.Add(73, 1);
            SpellIDEffectMap.Add(74, 1);
            SpellIDEffectMap.Add(75, 1);
            SpellIDEffectMap.Add(76, 1);
            SpellIDEffectMap.Add(77, 1);
            SpellIDEffectMap.Add(78, 1);
            SpellIDEffectMap.Add(79, 81);
            SpellIDEffectMap.Add(80, 21);
            SpellIDEffectMap.Add(81, 2);
            SpellIDEffectMap.Add(82, 2);
            SpellIDEffectMap.Add(83, 2);
            SpellIDEffectMap.Add(84, 28);
            SpellIDEffectMap.Add(85, 101);
            SpellIDEffectMap.Add(86, 101);
            SpellIDEffectMap.Add(87, 1);
            SpellIDEffectMap.Add(88, 1);
            SpellIDEffectMap.Add(89, 1);
            SpellIDEffectMap.Add(90, 1);
            SpellIDEffectMap.Add(91, 112);
            SpellIDEffectMap.Add(92, 1);
            SpellIDEffectMap.Add(93, 81);
            SpellIDEffectMap.Add(94, 113);
            SpellIDEffectMap.Add(95, 1);
            SpellIDEffectMap.Add(96, 1);
            SpellIDEffectMap.Add(97, 1);
            SpellIDEffectMap.Add(98, 1);
            SpellIDEffectMap.Add(99, 1);
            SpellIDEffectMap.Add(100, 11);
            SpellIDEffectMap.Add(101, 11);
            SpellIDEffectMap.Add(102, 11);
            SpellIDEffectMap.Add(103, 11);
            SpellIDEffectMap.Add(104, 11);
            SpellIDEffectMap.Add(105, 11);
            SpellIDEffectMap.Add(106, 48);
            SpellIDEffectMap.Add(107, 11);
            SpellIDEffectMap.Add(108, 11);
            SpellIDEffectMap.Add(109, 11);
            SpellIDEffectMap.Add(110, 1);
            SpellIDEffectMap.Add(111, 1);
            SpellIDEffectMap.Add(112, 11);
            SpellIDEffectMap.Add(113, 11);
            SpellIDEffectMap.Add(114, 11);
            SpellIDEffectMap.Add(115, 11);
            SpellIDEffectMap.Add(116, 11);
            SpellIDEffectMap.Add(117, 11);
            SpellIDEffectMap.Add(118, 11);
            SpellIDEffectMap.Add(119, 11);
            SpellIDEffectMap.Add(120, 11);
            SpellIDEffectMap.Add(121, 11);
            SpellIDEffectMap.Add(122, 11);
            SpellIDEffectMap.Add(123, 11);
            SpellIDEffectMap.Add(124, 11);
            SpellIDEffectMap.Add(125, 11);
            SpellIDEffectMap.Add(126, 11);
            SpellIDEffectMap.Add(127, 11);
            SpellIDEffectMap.Add(128, 11);
            SpellIDEffectMap.Add(129, 11);
            SpellIDEffectMap.Add(130, 11);
            SpellIDEffectMap.Add(131, 11);
            SpellIDEffectMap.Add(132, 11);
            SpellIDEffectMap.Add(133, 11);
            SpellIDEffectMap.Add(134, 11);
            SpellIDEffectMap.Add(135, 11);
            SpellIDEffectMap.Add(136, 11);
            SpellIDEffectMap.Add(137, 11);
            SpellIDEffectMap.Add(138, 11);
            SpellIDEffectMap.Add(139, 11);
            SpellIDEffectMap.Add(140, 11);
            SpellIDEffectMap.Add(141, 11);
            SpellIDEffectMap.Add(142, 11);
            SpellIDEffectMap.Add(143, 11);
            SpellIDEffectMap.Add(144, 11);
            SpellIDEffectMap.Add(145, 11);
            SpellIDEffectMap.Add(146, 11);
            SpellIDEffectMap.Add(147, 11);
            SpellIDEffectMap.Add(148, 11);
            SpellIDEffectMap.Add(149, 11);
            SpellIDEffectMap.Add(150, 10);
            SpellIDEffectMap.Add(151, 2);
            SpellIDEffectMap.Add(152, 2);
            SpellIDEffectMap.Add(153, 11);
            SpellIDEffectMap.Add(154, 2);
            SpellIDEffectMap.Add(155, 2);
            SpellIDEffectMap.Add(156, 2);
            SpellIDEffectMap.Add(157, 11);
            SpellIDEffectMap.Add(158, 2);
            SpellIDEffectMap.Add(159, 2);
            SpellIDEffectMap.Add(160, 2);
            SpellIDEffectMap.Add(161, 2);
            SpellIDEffectMap.Add(162, 11);
            SpellIDEffectMap.Add(163, 2);
            SpellIDEffectMap.Add(164, 128);
            SpellIDEffectMap.Add(165, 10);
            SpellIDEffectMap.Add(166, 2);
            SpellIDEffectMap.Add(167, 128);
            SpellIDEffectMap.Add(168, 10);
            SpellIDEffectMap.Add(169, 10);
            SpellIDEffectMap.Add(170, 2);
            SpellIDEffectMap.Add(171, 2);
            SpellIDEffectMap.Add(172, 2);
            SpellIDEffectMap.Add(173, 2);
            SpellIDEffectMap.Add(174, 2);
            SpellIDEffectMap.Add(175, 2);
            SpellIDEffectMap.Add(176, 99);
            SpellIDEffectMap.Add(177, 2);
            SpellIDEffectMap.Add(178, 2);
            SpellIDEffectMap.Add(179, 2);
            SpellIDEffectMap.Add(180, 66);
            SpellIDEffectMap.Add(181, 2);
            SpellIDEffectMap.Add(182, 2);
            SpellIDEffectMap.Add(183, 3);
            SpellIDEffectMap.Add(184, 2);
            SpellIDEffectMap.Add(185, 11);
            SpellIDEffectMap.Add(186, 11);
            SpellIDEffectMap.Add(187, 2);
            SpellIDEffectMap.Add(188, 11);
            SpellIDEffectMap.Add(189, 10);
            SpellIDEffectMap.Add(190, 81);
            SpellIDEffectMap.Add(191, 2);
            SpellIDEffectMap.Add(192, 10);
            SpellIDEffectMap.Add(193, 2);
            SpellIDEffectMap.Add(194, 2);
            SpellIDEffectMap.Add(195, 23);
            SpellIDEffectMap.Add(196, 2);
            SpellIDEffectMap.Add(197, 11);
            SpellIDEffectMap.Add(198, 11);
            SpellIDEffectMap.Add(199, 42);
            SpellIDEffectMap.Add(200, 21);
            SpellIDEffectMap.Add(201, 93);
            SpellIDEffectMap.Add(202, 10);
            SpellIDEffectMap.Add(203, 21);
            SpellIDEffectMap.Add(204, 21);
            SpellIDEffectMap.Add(205, 23);
            SpellIDEffectMap.Add(206, 1);
            SpellIDEffectMap.Add(207, 21);
            SpellIDEffectMap.Add(208, 21);
            SpellIDEffectMap.Add(209, 101);
            SpellIDEffectMap.Add(210, 111);
            SpellIDEffectMap.Add(211, 130);
            SpellIDEffectMap.Add(212, 130);
            SpellIDEffectMap.Add(213, 81);
            SpellIDEffectMap.Add(214, 10);
            SpellIDEffectMap.Add(215, 23);
            SpellIDEffectMap.Add(216, 10);
            SpellIDEffectMap.Add(217, 29);
            SpellIDEffectMap.Add(218, 2);
            SpellIDEffectMap.Add(219, 8);
            SpellIDEffectMap.Add(220, 1);
            SpellIDEffectMap.Add(221, 21);
            SpellIDEffectMap.Add(222, 21);
            SpellIDEffectMap.Add(223, 21);
            SpellIDEffectMap.Add(224, 21);
            SpellIDEffectMap.Add(225, 21);
            SpellIDEffectMap.Add(226, 21);
            SpellIDEffectMap.Add(227, 1);
            SpellIDEffectMap.Add(228, 21);
            SpellIDEffectMap.Add(229, 1);
            SpellIDEffectMap.Add(230, 1);
            SpellIDEffectMap.Add(231, 1);
            SpellIDEffectMap.Add(232, 1);
            SpellIDEffectMap.Add(233, 10);
            SpellIDEffectMap.Add(234, 127);
            SpellIDEffectMap.Add(235, 1);
            SpellIDEffectMap.Add(236, 1);
            SpellIDEffectMap.Add(237, 1);
            SpellIDEffectMap.Add(238, 119);
            SpellIDEffectMap.Add(239, 21);
            SpellIDEffectMap.Add(240, 119);
            SpellIDEffectMap.Add(241, 21);
            SpellIDEffectMap.Add(242, 21);
            SpellIDEffectMap.Add(243, 21);
            SpellIDEffectMap.Add(244, 1);
            SpellIDEffectMap.Add(245, 38);
            SpellIDEffectMap.Add(246, 21);
            SpellIDEffectMap.Add(247, 21);
            SpellIDEffectMap.Add(248, 21);
            SpellIDEffectMap.Add(249, 21);
            SpellIDEffectMap.Add(250, 21);
            SpellIDEffectMap.Add(251, 63);
            SpellIDEffectMap.Add(252, 63);
            SpellIDEffectMap.Add(253, 1);
            SpellIDEffectMap.Add(254, 1);
            SpellIDEffectMap.Add(255, 21);
            SpellIDEffectMap.Add(256, 24);
            SpellIDEffectMap.Add(257, 1);
            SpellIDEffectMap.Add(258, 21);
            SpellIDEffectMap.Add(259, 21);
            SpellIDEffectMap.Add(260, 1);
            SpellIDEffectMap.Add(261, 1);
            SpellIDEffectMap.Add(262, 1);
            SpellIDEffectMap.Add(263, 21);
            SpellIDEffectMap.Add(264, 21);
            SpellIDEffectMap.Add(265, 21);
            SpellIDEffectMap.Add(266, 21);
            SpellIDEffectMap.Add(267, 21);
            SpellIDEffectMap.Add(268, 89);
            SpellIDEffectMap.Add(269, 1);
            SpellIDEffectMap.Add(270, 21);
            SpellIDEffectMap.Add(271, 1);
            SpellIDEffectMap.Add(272, 21);
            SpellIDEffectMap.Add(273, 1);
            SpellIDEffectMap.Add(274, 21);
            SpellIDEffectMap.Add(275, 21);
            SpellIDEffectMap.Add(276, 21);
            SpellIDEffectMap.Add(277, 21);
            SpellIDEffectMap.Add(278, 21);
            SpellIDEffectMap.Add(279, 21);
            SpellIDEffectMap.Add(280, 1);
            SpellIDEffectMap.Add(281, 50);
            SpellIDEffectMap.Add(282, 28);
            SpellIDEffectMap.Add(283, 10);
            SpellIDEffectMap.Add(284, 10);
            SpellIDEffectMap.Add(285, 23);
            SpellIDEffectMap.Add(286, 10);
            SpellIDEffectMap.Add(287, 10);
            SpellIDEffectMap.Add(288, 29);
            SpellIDEffectMap.Add(289, 23);
            SpellIDEffectMap.Add(290, 10);
            SpellIDEffectMap.Add(291, 10);
            SpellIDEffectMap.Add(292, 23);
            SpellIDEffectMap.Add(293, 1);
            SpellIDEffectMap.Add(294, 21);
            SpellIDEffectMap.Add(295, 21);
            SpellIDEffectMap.Add(296, 21);
            SpellIDEffectMap.Add(297, 21);
            SpellIDEffectMap.Add(298, 21);
            SpellIDEffectMap.Add(299, 21);
            SpellIDEffectMap.Add(300, 1);
            SpellIDEffectMap.Add(301, 1);
            SpellIDEffectMap.Add(302, 1);
            SpellIDEffectMap.Add(303, 1);
            SpellIDEffectMap.Add(304, 1);
            SpellIDEffectMap.Add(305, 1);
            SpellIDEffectMap.Add(306, 21);
            SpellIDEffectMap.Add(307, 21);
            SpellIDEffectMap.Add(308, 21);
            SpellIDEffectMap.Add(309, 21);
            SpellIDEffectMap.Add(310, 21);
            SpellIDEffectMap.Add(311, 21);
            SpellIDEffectMap.Add(312, 21);
            SpellIDEffectMap.Add(313, 28);
            SpellIDEffectMap.Add(314, 10);
            SpellIDEffectMap.Add(315, 10);
            SpellIDEffectMap.Add(316, 23);
            SpellIDEffectMap.Add(317, 11);
            SpellIDEffectMap.Add(318, 10);
            SpellIDEffectMap.Add(319, 10);
            SpellIDEffectMap.Add(320, 29);
            SpellIDEffectMap.Add(321, 23);
            SpellIDEffectMap.Add(322, 10);
            SpellIDEffectMap.Add(323, 10);
            SpellIDEffectMap.Add(324, 23);
            SpellIDEffectMap.Add(325, 21);
            SpellIDEffectMap.Add(326, 21);
            SpellIDEffectMap.Add(327, 21);
            SpellIDEffectMap.Add(328, 10);
            SpellIDEffectMap.Add(329, 23);
            SpellIDEffectMap.Add(330, 10);
            SpellIDEffectMap.Add(331, 23);
            SpellIDEffectMap.Add(332, 10);
            SpellIDEffectMap.Add(333, 23);
            SpellIDEffectMap.Add(334, 86);
            SpellIDEffectMap.Add(335, 4);
            SpellIDEffectMap.Add(336, 11);
            SpellIDEffectMap.Add(337, 3);
            SpellIDEffectMap.Add(338, 21);
            SpellIDEffectMap.Add(339, 21);
            SpellIDEffectMap.Add(340, 21);
            SpellIDEffectMap.Add(341, 23);
            SpellIDEffectMap.Add(342, 23);
            SpellIDEffectMap.Add(343, 13);
            SpellIDEffectMap.Add(344, 23);
            SpellIDEffectMap.Add(345, 23);
            SpellIDEffectMap.Add(346, 42);
            SpellIDEffectMap.Add(347, 1);
            SpellIDEffectMap.Add(348, 1);
            SpellIDEffectMap.Add(349, 1);
            SpellIDEffectMap.Add(350, 1);
            SpellIDEffectMap.Add(351, 21);
            SpellIDEffectMap.Add(352, 1);
            SpellIDEffectMap.Add(353, 81);
            SpellIDEffectMap.Add(354, 73);
            SpellIDEffectMap.Add(355, 73);
            SpellIDEffectMap.Add(356, 21);
            SpellIDEffectMap.Add(357, 10);
            SpellIDEffectMap.Add(358, 23);
            SpellIDEffectMap.Add(359, 89);
            SpellIDEffectMap.Add(360, 1);
            SpellIDEffectMap.Add(361, 11);
            SpellIDEffectMap.Add(362, 21);
            SpellIDEffectMap.Add(363, 66);
            SpellIDEffectMap.Add(364, 21);
            SpellIDEffectMap.Add(365, 21);
            SpellIDEffectMap.Add(366, 21);
            SpellIDEffectMap.Add(367, 89);
            SpellIDEffectMap.Add(368, 1);
            SpellIDEffectMap.Add(369, 1);
            SpellIDEffectMap.Add(370, 21);
            SpellIDEffectMap.Add(371, 21);
            SpellIDEffectMap.Add(372, 42);
            SpellIDEffectMap.Add(373, 89);
            SpellIDEffectMap.Add(374, 82);
            SpellIDEffectMap.Add(375, 85);
            SpellIDEffectMap.Add(376, 1);
            SpellIDEffectMap.Add(377, 1);
            SpellIDEffectMap.Add(378, 1);
            SpellIDEffectMap.Add(379, 21);
            SpellIDEffectMap.Add(380, 37);
            SpellIDEffectMap.Add(381, 21);
            SpellIDEffectMap.Add(382, 21);
            SpellIDEffectMap.Add(383, 1);
            SpellIDEffectMap.Add(384, 21);
            SpellIDEffectMap.Add(385, 1);
            SpellIDEffectMap.Add(386, 1);
            SpellIDEffectMap.Add(387, 1);
            SpellIDEffectMap.Add(388, 1);
            SpellIDEffectMap.Add(389, 81);
            SpellIDEffectMap.Add(390, 1);
            SpellIDEffectMap.Add(391, 1);
            SpellIDEffectMap.Add(392, 1);
            SpellIDEffectMap.Add(393, 1);
            SpellIDEffectMap.Add(394, 21);
            SpellIDEffectMap.Add(395, 89);
            SpellIDEffectMap.Add(396, 1);
            SpellIDEffectMap.Add(397, 1);
            SpellIDEffectMap.Add(398, 21);
            SpellIDEffectMap.Add(399, 1);
            SpellIDEffectMap.Add(400, 21);
            SpellIDEffectMap.Add(401, 21);
            SpellIDEffectMap.Add(402, 1);
            SpellIDEffectMap.Add(403, 81);
            SpellIDEffectMap.Add(404, 1);
            SpellIDEffectMap.Add(405, 11);
            SpellIDEffectMap.Add(406, 11);
            SpellIDEffectMap.Add(407, 3);
            SpellIDEffectMap.Add(408, 11);
            SpellIDEffectMap.Add(409, 10);
            SpellIDEffectMap.Add(410, 10);
            SpellIDEffectMap.Add(411, 23);
            SpellIDEffectMap.Add(412, 10);
            SpellIDEffectMap.Add(413, 89);
            SpellIDEffectMap.Add(414, 89);
            SpellIDEffectMap.Add(415, 1);
            SpellIDEffectMap.Add(416, 1);
            SpellIDEffectMap.Add(417, 1);
            SpellIDEffectMap.Add(418, 1);
            SpellIDEffectMap.Add(419, 1);
            SpellIDEffectMap.Add(420, 1);
            SpellIDEffectMap.Add(421, 1);
            SpellIDEffectMap.Add(422, 1);
            SpellIDEffectMap.Add(423, 1);
            SpellIDEffectMap.Add(424, 21);
            SpellIDEffectMap.Add(425, 21);
            SpellIDEffectMap.Add(426, 21);
            SpellIDEffectMap.Add(427, 1);
            SpellIDEffectMap.Add(428, 21);
            SpellIDEffectMap.Add(429, 21);
            SpellIDEffectMap.Add(430, 21);
            SpellIDEffectMap.Add(431, 21);
            SpellIDEffectMap.Add(432, 21);
            SpellIDEffectMap.Add(433, 21);
            SpellIDEffectMap.Add(434, 21);
            SpellIDEffectMap.Add(435, 10);
            SpellIDEffectMap.Add(436, 1);
            SpellIDEffectMap.Add(437, 1);
            SpellIDEffectMap.Add(438, 1);
            SpellIDEffectMap.Add(439, 1);
            SpellIDEffectMap.Add(440, 1);
            SpellIDEffectMap.Add(441, 21);
            SpellIDEffectMap.Add(442, 21);
            SpellIDEffectMap.Add(443, 21);
            SpellIDEffectMap.Add(444, 1);
            SpellIDEffectMap.Add(445, 1);
            SpellIDEffectMap.Add(446, 1);
            SpellIDEffectMap.Add(447, 1);
            SpellIDEffectMap.Add(448, 1);
            SpellIDEffectMap.Add(449, 1);
            SpellIDEffectMap.Add(450, 1);
            SpellIDEffectMap.Add(451, 1);
            SpellIDEffectMap.Add(452, 1);
            SpellIDEffectMap.Add(453, 21);
            SpellIDEffectMap.Add(454, 1);
            SpellIDEffectMap.Add(455, 21);
            SpellIDEffectMap.Add(456, 21);
            SpellIDEffectMap.Add(457, 1);
            SpellIDEffectMap.Add(458, 21);
            SpellIDEffectMap.Add(459, 21);
            SpellIDEffectMap.Add(460, 21);
            SpellIDEffectMap.Add(461, 1);
            SpellIDEffectMap.Add(462, 1);
            SpellIDEffectMap.Add(463, 1);
            SpellIDEffectMap.Add(464, 21);
            SpellIDEffectMap.Add(465, 1);
            SpellIDEffectMap.Add(466, 1);
            SpellIDEffectMap.Add(467, 1);
            SpellIDEffectMap.Add(468, 120);
            SpellIDEffectMap.Add(469, 1);
            SpellIDEffectMap.Add(470, 1);
            SpellIDEffectMap.Add(471, 48);
            SpellIDEffectMap.Add(472, 1);
            SpellIDEffectMap.Add(473, 1);
            SpellIDEffectMap.Add(474, 1);
            SpellIDEffectMap.Add(475, 21);
            SpellIDEffectMap.Add(476, 1);
            SpellIDEffectMap.Add(477, 1);
            SpellIDEffectMap.Add(478, 1);
            SpellIDEffectMap.Add(479, 1);
            SpellIDEffectMap.Add(480, 21);
            SpellIDEffectMap.Add(481, 1);
            SpellIDEffectMap.Add(482, 1);
            SpellIDEffectMap.Add(483, 1);
            SpellIDEffectMap.Add(484, 1);
            SpellIDEffectMap.Add(485, 21);
            SpellIDEffectMap.Add(486, 1);
            SpellIDEffectMap.Add(487, 23);
            SpellIDEffectMap.Add(488, 21);
            SpellIDEffectMap.Add(489, 125);
            SpellIDEffectMap.Add(490, 1);
            SpellIDEffectMap.Add(491, 110);
            SpellIDEffectMap.Add(492, 0);
            SpellIDEffectMap.Add(493, 0);
            SpellIDEffectMap.Add(494, 2);
            SpellIDEffectMap.Add(495, 2);
            SpellIDEffectMap.Add(496, 2);
            SpellIDEffectMap.Add(497, 2);
            SpellIDEffectMap.Add(498, 11);
            SpellIDEffectMap.Add(499, 2);
            SpellIDEffectMap.Add(500, 2);
            SpellIDEffectMap.Add(501, 2);
            SpellIDEffectMap.Add(502, 85);
            SpellIDEffectMap.Add(503, 2);
            SpellIDEffectMap.Add(504, 2);
            SpellIDEffectMap.Add(505, 2);
            SpellIDEffectMap.Add(506, 2);
            SpellIDEffectMap.Add(507, 2);
            SpellIDEffectMap.Add(508, 7);
            SpellIDEffectMap.Add(509, 2);
            SpellIDEffectMap.Add(510, 81);
            SpellIDEffectMap.Add(511, 2);
            SpellIDEffectMap.Add(512, 2);
            SpellIDEffectMap.Add(513, 11);
            SpellIDEffectMap.Add(514, 2);
            SpellIDEffectMap.Add(515, 11);
            SpellIDEffectMap.Add(516, 2);
            SpellIDEffectMap.Add(517, 91);
            SpellIDEffectMap.Add(518, 81);
            SpellIDEffectMap.Add(519, 2);
            SpellIDEffectMap.Add(520, 2);
            SpellIDEffectMap.Add(521, 2);
            SpellIDEffectMap.Add(522, 48);
            SpellIDEffectMap.Add(523, 27);
            SpellIDEffectMap.Add(524, 13);
            SpellIDEffectMap.Add(525, 2);
            SpellIDEffectMap.Add(526, 11);
            SpellIDEffectMap.Add(527, 24);
            SpellIDEffectMap.Add(528, 2);
            SpellIDEffectMap.Add(529, 22);
            SpellIDEffectMap.Add(530, 42);
            SpellIDEffectMap.Add(531, 2);
            SpellIDEffectMap.Add(532, 42);
            SpellIDEffectMap.Add(533, 2);
            SpellIDEffectMap.Add(534, 2);
            SpellIDEffectMap.Add(535, 2);
            SpellIDEffectMap.Add(536, 2);
            SpellIDEffectMap.Add(537, 74);
            SpellIDEffectMap.Add(538, 2);
            SpellIDEffectMap.Add(539, 7);
            SpellIDEffectMap.Add(540, 2);
            SpellIDEffectMap.Add(541, 2);
            SpellIDEffectMap.Add(542, 2);
            SpellIDEffectMap.Add(543, 2);
            SpellIDEffectMap.Add(544, 134);
            SpellIDEffectMap.Add(545, 81);
            SpellIDEffectMap.Add(546, 2);
            SpellIDEffectMap.Add(547, 2);
            SpellIDEffectMap.Add(548, 2);
            SpellIDEffectMap.Add(549, 3);
            SpellIDEffectMap.Add(550, 601);
            SpellIDEffectMap.Add(551, 2);
            SpellIDEffectMap.Add(552, 2);
            SpellIDEffectMap.Add(553, 7);
            SpellIDEffectMap.Add(554, 13);
            SpellIDEffectMap.Add(555, 2);
            SpellIDEffectMap.Add(556, 81);
            SpellIDEffectMap.Add(557, 81);
            SpellIDEffectMap.Add(558, 2);
            SpellIDEffectMap.Add(559, 2);
            SpellIDEffectMap.Add(560, 57);
            SpellIDEffectMap.Add(561, 2);
            SpellIDEffectMap.Add(562, 81);
            SpellIDEffectMap.Add(563, 74);
            SpellIDEffectMap.Add(564, 2);
            SpellIDEffectMap.Add(565, 81);
            SpellIDEffectMap.Add(566, 72);
            SpellIDEffectMap.Add(567, 81);
            SpellIDEffectMap.Add(568, 2);
            SpellIDEffectMap.Add(569, 41);
            SpellIDEffectMap.Add(570, 81);
            SpellIDEffectMap.Add(571, 2);
            SpellIDEffectMap.Add(572, 81);
            SpellIDEffectMap.Add(573, 2);
            SpellIDEffectMap.Add(574, 2);
            SpellIDEffectMap.Add(575, 2);
            SpellIDEffectMap.Add(576, 11);
            SpellIDEffectMap.Add(577, 23);
            SpellIDEffectMap.Add(578, 2);
            SpellIDEffectMap.Add(579, 81);
            SpellIDEffectMap.Add(580, 81);
            SpellIDEffectMap.Add(581, 81);
            SpellIDEffectMap.Add(582, 74);
            SpellIDEffectMap.Add(583, 41);
            SpellIDEffectMap.Add(584, 2);
            SpellIDEffectMap.Add(585, 42);
            SpellIDEffectMap.Add(586, 134);
            SpellIDEffectMap.Add(587, 2);
            SpellIDEffectMap.Add(588, 42);
            SpellIDEffectMap.Add(589, 81);
            SpellIDEffectMap.Add(590, 81);
            SpellIDEffectMap.Add(591, 117);
            SpellIDEffectMap.Add(592, 23);
            SpellIDEffectMap.Add(593, 10);
            SpellIDEffectMap.Add(594, 11);
            SpellIDEffectMap.Add(595, 2);
            SpellIDEffectMap.Add(596, 11);
            SpellIDEffectMap.Add(597, 10);
            SpellIDEffectMap.Add(598, 2);
            SpellIDEffectMap.Add(599, 10);
            SpellIDEffectMap.Add(600, 10);
            SpellIDEffectMap.Add(601, 7);
            SpellIDEffectMap.Add(602, 514);
            SpellIDEffectMap.Add(603, 10);
            SpellIDEffectMap.Add(604, 10);
            SpellIDEffectMap.Add(605, 11);
            SpellIDEffectMap.Add(606, 504);
            SpellIDEffectMap.Add(607, 1);
            SpellIDEffectMap.Add(608, 23);
            SpellIDEffectMap.Add(609, 504);
            SpellIDEffectMap.Add(610, 10);
            SpellIDEffectMap.Add(611, 10);
            SpellIDEffectMap.Add(612, 504);
            SpellIDEffectMap.Add(613, 117);
            SpellIDEffectMap.Add(614, 10);
            SpellIDEffectMap.Add(615, 138);
            SpellIDEffectMap.Add(616, 11);
            SpellIDEffectMap.Add(617, 23);
            SpellIDEffectMap.Add(618, 67);
            SpellIDEffectMap.Add(619, 10);
            SpellIDEffectMap.Add(620, 2);
            SpellIDEffectMap.Add(621, 23);
            SpellIDEffectMap.Add(622, 23);
            SpellIDEffectMap.Add(623, 1);
            SpellIDEffectMap.Add(624, 11);
            SpellIDEffectMap.Add(625, 10);
            SpellIDEffectMap.Add(626, 10);
            SpellIDEffectMap.Add(627, 23);
            SpellIDEffectMap.Add(628, 10);
            SpellIDEffectMap.Add(629, 10);
            SpellIDEffectMap.Add(630, 10);
            SpellIDEffectMap.Add(631, 10);
            SpellIDEffectMap.Add(632, 11);
            SpellIDEffectMap.Add(633, 609);
            SpellIDEffectMap.Add(634, 42);
            SpellIDEffectMap.Add(635, 10);
            SpellIDEffectMap.Add(636, 138);
            SpellIDEffectMap.Add(637, 11);
            SpellIDEffectMap.Add(638, 42);
            SpellIDEffectMap.Add(639, 37);
            SpellIDEffectMap.Add(640, 10);
            SpellIDEffectMap.Add(641, 1);
            SpellIDEffectMap.Add(642, 10);
            SpellIDEffectMap.Add(643, 23);
            SpellIDEffectMap.Add(644, 11);
            SpellIDEffectMap.Add(645, 117);
            SpellIDEffectMap.Add(646, 10);
            SpellIDEffectMap.Add(647, 2);
            SpellIDEffectMap.Add(648, 81);
            SpellIDEffectMap.Add(649, 1);
            SpellIDEffectMap.Add(650, 10);
            SpellIDEffectMap.Add(651, 2);
            SpellIDEffectMap.Add(652, 10);
            SpellIDEffectMap.Add(653, 2);
            SpellIDEffectMap.Add(654, 10);
            SpellIDEffectMap.Add(655, 96);
            SpellIDEffectMap.Add(656, 42);
            SpellIDEffectMap.Add(657, 67);
            SpellIDEffectMap.Add(658, 23);
            SpellIDEffectMap.Add(659, 103);
            SpellIDEffectMap.Add(660, 10);
            SpellIDEffectMap.Add(661, 81);
            SpellIDEffectMap.Add(662, 11);
            SpellIDEffectMap.Add(663, 42);
            SpellIDEffectMap.Add(664, 2);
            SpellIDEffectMap.Add(665, 1);
            SpellIDEffectMap.Add(666, 2);
            SpellIDEffectMap.Add(667, 10);
            SpellIDEffectMap.Add(668, 1);
            SpellIDEffectMap.Add(669, 117);
            SpellIDEffectMap.Add(670, 11);
            SpellIDEffectMap.Add(671, 99);
            SpellIDEffectMap.Add(672, 1);
            SpellIDEffectMap.Add(673, 28);
            SpellIDEffectMap.Add(674, 10);
            SpellIDEffectMap.Add(675, 10);
            SpellIDEffectMap.Add(676, 23);
            SpellIDEffectMap.Add(677, 81);
            SpellIDEffectMap.Add(678, 10);
            SpellIDEffectMap.Add(679, 23);
            SpellIDEffectMap.Add(680, 38);
            SpellIDEffectMap.Add(681, 23);
            SpellIDEffectMap.Add(682, 2);
            SpellIDEffectMap.Add(683, 609);
            SpellIDEffectMap.Add(684, 81);
            SpellIDEffectMap.Add(685, 10);
            SpellIDEffectMap.Add(686, 82);
            SpellIDEffectMap.Add(687, 11);
            SpellIDEffectMap.Add(688, 2);
            SpellIDEffectMap.Add(689, 54);
            SpellIDEffectMap.Add(690, 1);
            SpellIDEffectMap.Add(691, 44);
            SpellIDEffectMap.Add(692, 10);
            SpellIDEffectMap.Add(693, 11);
            SpellIDEffectMap.Add(694, 10);
            SpellIDEffectMap.Add(695, 81);
            SpellIDEffectMap.Add(696, 10);
            SpellIDEffectMap.Add(697, 10);
            SpellIDEffectMap.Add(698, 63);
            SpellIDEffectMap.Add(699, 70);
            SpellIDEffectMap.Add(700, 10);
            SpellIDEffectMap.Add(701, 10);
            SpellIDEffectMap.Add(702, 133);
            SpellIDEffectMap.Add(703, 2);
            SpellIDEffectMap.Add(704, 54);
            SpellIDEffectMap.Add(705, 10);
            SpellIDEffectMap.Add(706, 10);
            SpellIDEffectMap.Add(707, 28);
            SpellIDEffectMap.Add(708, 25);
            SpellIDEffectMap.Add(709, 81);
            SpellIDEffectMap.Add(710, 11);
            SpellIDEffectMap.Add(711, 1);
            SpellIDEffectMap.Add(712, 11);
            SpellIDEffectMap.Add(713, 68);
            SpellIDEffectMap.Add(714, 1);
            SpellIDEffectMap.Add(715, 1);
            SpellIDEffectMap.Add(716, 11);
            SpellIDEffectMap.Add(717, 21);
            SpellIDEffectMap.Add(718, 1);
            SpellIDEffectMap.Add(719, 23);
            SpellIDEffectMap.Add(720, 23);
            SpellIDEffectMap.Add(721, 1);
            SpellIDEffectMap.Add(722, 1);
            SpellIDEffectMap.Add(723, 1);
            SpellIDEffectMap.Add(724, 1);
            SpellIDEffectMap.Add(725, 1);
            SpellIDEffectMap.Add(726, 23);
            SpellIDEffectMap.Add(727, 1);
            SpellIDEffectMap.Add(728, 1);
            SpellIDEffectMap.Add(729, 1);
            SpellIDEffectMap.Add(730, 37);
            SpellIDEffectMap.Add(731, 1);
            SpellIDEffectMap.Add(732, 1);
            SpellIDEffectMap.Add(733, 1);
            SpellIDEffectMap.Add(734, 1);
            SpellIDEffectMap.Add(735, 1);
            SpellIDEffectMap.Add(736, 23);
            SpellIDEffectMap.Add(737, 1);
            SpellIDEffectMap.Add(738, 1);
            SpellIDEffectMap.Add(739, 23);
            SpellIDEffectMap.Add(740, 48);
            SpellIDEffectMap.Add(741, 1);
            SpellIDEffectMap.Add(742, 21);
            SpellIDEffectMap.Add(743, 1);
            SpellIDEffectMap.Add(744, 1);
            SpellIDEffectMap.Add(745, 1);
            SpellIDEffectMap.Add(746, 1);
            SpellIDEffectMap.Add(747, 1);
            SpellIDEffectMap.Add(748, 1);
            SpellIDEffectMap.Add(749, 37);
            SpellIDEffectMap.Add(750, 1);
            SpellIDEffectMap.Add(751, 1);
            SpellIDEffectMap.Add(752, 1);
            SpellIDEffectMap.Add(753, 1);
            SpellIDEffectMap.Add(754, 1);
            SpellIDEffectMap.Add(755, 1);
            SpellIDEffectMap.Add(756, 43);
            SpellIDEffectMap.Add(757, 48);
            SpellIDEffectMap.Add(758, 1);
            SpellIDEffectMap.Add(759, 1);
            SpellIDEffectMap.Add(760, 1);
            SpellIDEffectMap.Add(761, 81);
            SpellIDEffectMap.Add(762, 1);
            SpellIDEffectMap.Add(763, 126);
            SpellIDEffectMap.Add(764, 7);
            SpellIDEffectMap.Add(765, 1);
            SpellIDEffectMap.Add(766, 1);
            SpellIDEffectMap.Add(767, 1);
            SpellIDEffectMap.Add(768, 1);
            SpellIDEffectMap.Add(769, 23);
            SpellIDEffectMap.Add(770, 43);
            SpellIDEffectMap.Add(771, 1);
            SpellIDEffectMap.Add(772, 1);
            SpellIDEffectMap.Add(773, 1);
            SpellIDEffectMap.Add(774, 1);
            SpellIDEffectMap.Add(775, 49);
            SpellIDEffectMap.Add(776, 135);
            SpellIDEffectMap.Add(777, 21);
            SpellIDEffectMap.Add(778, 48);
            SpellIDEffectMap.Add(779, 1);
            SpellIDEffectMap.Add(780, 1);
            SpellIDEffectMap.Add(781, 1);
            SpellIDEffectMap.Add(782, 21);
            SpellIDEffectMap.Add(783, 1);
            SpellIDEffectMap.Add(784, 1);
            SpellIDEffectMap.Add(785, 1);
            SpellIDEffectMap.Add(786, 1);
            SpellIDEffectMap.Add(787, 1);
            SpellIDEffectMap.Add(788, 3);
            SpellIDEffectMap.Add(789, 43);
            SpellIDEffectMap.Add(790, 21);
            SpellIDEffectMap.Add(791, 115);
            SpellIDEffectMap.Add(792, 81);
            SpellIDEffectMap.Add(793, 1);
            SpellIDEffectMap.Add(794, 1);
            SpellIDEffectMap.Add(795, 21);
            SpellIDEffectMap.Add(796, 98);
            SpellIDEffectMap.Add(797, 1);
            SpellIDEffectMap.Add(798, 85);
            SpellIDEffectMap.Add(799, 1);
            SpellIDEffectMap.Add(800, 21);
            SpellIDEffectMap.Add(801, 1);
            SpellIDEffectMap.Add(802, 21);
            SpellIDEffectMap.Add(803, 81);
            SpellIDEffectMap.Add(804, 21);
            SpellIDEffectMap.Add(805, 21);
            SpellIDEffectMap.Add(806, 1);
            SpellIDEffectMap.Add(807, 21);
            SpellIDEffectMap.Add(808, 48);
            SpellIDEffectMap.Add(809, 21);
            SpellIDEffectMap.Add(810, 21);
            SpellIDEffectMap.Add(811, 1);
            SpellIDEffectMap.Add(812, 21);
            SpellIDEffectMap.Add(813, 1);
            SpellIDEffectMap.Add(814, 1);
            SpellIDEffectMap.Add(815, 1);
            SpellIDEffectMap.Add(816, 42);
            SpellIDEffectMap.Add(817, 21);
            SpellIDEffectMap.Add(818, 1);
            SpellIDEffectMap.Add(819, 1);
            SpellIDEffectMap.Add(820, 1);
            SpellIDEffectMap.Add(821, 1);
            SpellIDEffectMap.Add(822, 1);
            SpellIDEffectMap.Add(823, 1);
            SpellIDEffectMap.Add(824, 21);
            SpellIDEffectMap.Add(825, 21);
            SpellIDEffectMap.Add(826, 21);
            SpellIDEffectMap.Add(827, 68);
            SpellIDEffectMap.Add(828, 21);
            SpellIDEffectMap.Add(829, 63);
            SpellIDEffectMap.Add(830, 89);
            SpellIDEffectMap.Add(831, 89);
            SpellIDEffectMap.Add(832, 89);
            SpellIDEffectMap.Add(833, 81);
            SpellIDEffectMap.Add(834, 50);
            SpellIDEffectMap.Add(835, 89);
            SpellIDEffectMap.Add(836, 62);
            SpellIDEffectMap.Add(837, 81);
            SpellIDEffectMap.Add(838, 89);
            SpellIDEffectMap.Add(839, 93);
            SpellIDEffectMap.Add(840, 23);
            SpellIDEffectMap.Add(841, 11);
            SpellIDEffectMap.Add(842, 21);
            SpellIDEffectMap.Add(843, 81);
            SpellIDEffectMap.Add(844, 81);
            SpellIDEffectMap.Add(845, 1);
            SpellIDEffectMap.Add(846, 1);
            SpellIDEffectMap.Add(847, 38);
            SpellIDEffectMap.Add(848, 1);
            SpellIDEffectMap.Add(849, 76);
            SpellIDEffectMap.Add(850, 1);
            SpellIDEffectMap.Add(851, 81);
            SpellIDEffectMap.Add(852, 81);
            SpellIDEffectMap.Add(853, 1);
            SpellIDEffectMap.Add(854, 1);
            SpellIDEffectMap.Add(855, 10);
            SpellIDEffectMap.Add(856, 1);
            SpellIDEffectMap.Add(857, 1);
            SpellIDEffectMap.Add(858, 1);
            SpellIDEffectMap.Add(859, 1);
            SpellIDEffectMap.Add(860, 1);
            SpellIDEffectMap.Add(861, 10);
            SpellIDEffectMap.Add(862, 81);
            SpellIDEffectMap.Add(863, 1);
            SpellIDEffectMap.Add(864, 21);
            SpellIDEffectMap.Add(865, 1);
            SpellIDEffectMap.Add(866, 1);
            SpellIDEffectMap.Add(867, 81);
            SpellIDEffectMap.Add(868, 1);
            SpellIDEffectMap.Add(869, 21);
            SpellIDEffectMap.Add(870, 509);
            SpellIDEffectMap.Add(871, 23);
            SpellIDEffectMap.Add(872, 509);
            SpellIDEffectMap.Add(873, 509);
            SpellIDEffectMap.Add(874, 10);
            SpellIDEffectMap.Add(875, 1);
            SpellIDEffectMap.Add(876, 1);
            SpellIDEffectMap.Add(877, 1);
            SpellIDEffectMap.Add(878, 524);
            SpellIDEffectMap.Add(879, 13);
            SpellIDEffectMap.Add(880, 10);
            SpellIDEffectMap.Add(881, 23);
            SpellIDEffectMap.Add(882, 10);
            SpellIDEffectMap.Add(883, 10);
            SpellIDEffectMap.Add(884, 21);
            SpellIDEffectMap.Add(885, 23);
            SpellIDEffectMap.Add(886, 10);
            SpellIDEffectMap.Add(887, 23);
            SpellIDEffectMap.Add(888, 10);
            SpellIDEffectMap.Add(889, 40);
            SpellIDEffectMap.Add(890, 1);
            SpellIDEffectMap.Add(891, 10);
            SpellIDEffectMap.Add(892, 23);
            SpellIDEffectMap.Add(893, 1);
            SpellIDEffectMap.Add(894, 21);
            SpellIDEffectMap.Add(895, 10);
            SpellIDEffectMap.Add(896, 13);
            SpellIDEffectMap.Add(897, 10);
            SpellIDEffectMap.Add(898, 1);
            SpellIDEffectMap.Add(899, 95);
            SpellIDEffectMap.Add(900, 1);
            SpellIDEffectMap.Add(901, 13);
            SpellIDEffectMap.Add(902, 10);
            SpellIDEffectMap.Add(903, 1);
            SpellIDEffectMap.Add(904, 23);
            SpellIDEffectMap.Add(905, 1);
            SpellIDEffectMap.Add(906, 23);
            SpellIDEffectMap.Add(907, 10);
            SpellIDEffectMap.Add(908, 10);
            SpellIDEffectMap.Add(909, 10);
            SpellIDEffectMap.Add(910, 1);
            SpellIDEffectMap.Add(911, 82);
            SpellIDEffectMap.Add(912, 10);
            SpellIDEffectMap.Add(913, 81);
            SpellIDEffectMap.Add(914, 81);
            SpellIDEffectMap.Add(915, 1);
            SpellIDEffectMap.Add(916, 23);
            SpellIDEffectMap.Add(917, 30);
            SpellIDEffectMap.Add(918, 81);
            SpellIDEffectMap.Add(919, 1);
            SpellIDEffectMap.Add(920, 1);
            SpellIDEffectMap.Add(921, 79);
            SpellIDEffectMap.Add(922, 81);
            SpellIDEffectMap.Add(923, 81);
            SpellIDEffectMap.Add(924, 81);
            SpellIDEffectMap.Add(925, 81);
            SpellIDEffectMap.Add(926, 82);
            SpellIDEffectMap.Add(927, 10);
            SpellIDEffectMap.Add(928, 84);
            SpellIDEffectMap.Add(929, 23);
            SpellIDEffectMap.Add(930, 81);
            SpellIDEffectMap.Add(931, 82);
            SpellIDEffectMap.Add(932, 100);
            SpellIDEffectMap.Add(933, 81);
            SpellIDEffectMap.Add(934, 1);
            SpellIDEffectMap.Add(935, 100);
            SpellIDEffectMap.Add(936, 100);
            SpellIDEffectMap.Add(937, 2);
            SpellIDEffectMap.Add(938, 82);
            SpellIDEffectMap.Add(939, 81);
            SpellIDEffectMap.Add(940, 1);
            SpellIDEffectMap.Add(941, 1);
            SpellIDEffectMap.Add(942, 81);
            SpellIDEffectMap.Add(943, 82);
            SpellIDEffectMap.Add(944, 10);
            SpellIDEffectMap.Add(945, 81);
            SpellIDEffectMap.Add(946, 84);
            SpellIDEffectMap.Add(947, 81);
            SpellIDEffectMap.Add(948, 81);
            SpellIDEffectMap.Add(949, 81);
            SpellIDEffectMap.Add(950, 37);
            SpellIDEffectMap.Add(951, 10);
            SpellIDEffectMap.Add(952, 26);
            SpellIDEffectMap.Add(953, 1);
            SpellIDEffectMap.Add(954, 10);
            SpellIDEffectMap.Add(955, 114);
            SpellIDEffectMap.Add(956, 84);
            SpellIDEffectMap.Add(957, 10);
            SpellIDEffectMap.Add(958, 10);
            SpellIDEffectMap.Add(959, 10);
            SpellIDEffectMap.Add(960, 81);
            SpellIDEffectMap.Add(961, 81);
            SpellIDEffectMap.Add(962, 21);
            SpellIDEffectMap.Add(963, 11);
            SpellIDEffectMap.Add(964, 10);
            SpellIDEffectMap.Add(965, 81);
            SpellIDEffectMap.Add(966, 81);
            SpellIDEffectMap.Add(967, 81);
            SpellIDEffectMap.Add(968, 81);
            SpellIDEffectMap.Add(969, 81);
            SpellIDEffectMap.Add(970, 37);
            SpellIDEffectMap.Add(971, 10);
            SpellIDEffectMap.Add(972, 81);
            SpellIDEffectMap.Add(973, 500);
            SpellIDEffectMap.Add(974, 2);
            SpellIDEffectMap.Add(975, 20);
            SpellIDEffectMap.Add(976, 10);
            SpellIDEffectMap.Add(977, 10);
            SpellIDEffectMap.Add(978, 600);
            SpellIDEffectMap.Add(979, 2);
            SpellIDEffectMap.Add(980, 11);
            SpellIDEffectMap.Add(981, 4);
            SpellIDEffectMap.Add(982, 128);
            SpellIDEffectMap.Add(983, 11);
            SpellIDEffectMap.Add(984, 11);
            SpellIDEffectMap.Add(985, 11);
            SpellIDEffectMap.Add(986, 3);
            SpellIDEffectMap.Add(987, 85);
            SpellIDEffectMap.Add(988, 15);
            SpellIDEffectMap.Add(989, 2);
            SpellIDEffectMap.Add(990, 11);
            SpellIDEffectMap.Add(991, 11);
            SpellIDEffectMap.Add(992, 48);
            SpellIDEffectMap.Add(993, 2);
            SpellIDEffectMap.Add(994, 10);
            SpellIDEffectMap.Add(995, 85);
            SpellIDEffectMap.Add(996, 19);
            SpellIDEffectMap.Add(997, 48);
            SpellIDEffectMap.Add(998, 4);
            SpellIDEffectMap.Add(999, 11);
            SpellIDEffectMap.Add(1000, 48);
            SpellIDEffectMap.Add(1001, 500);
            SpellIDEffectMap.Add(1002, 48);
            SpellIDEffectMap.Add(1003, 66);
            SpellIDEffectMap.Add(1004, 53);
            SpellIDEffectMap.Add(1005, 4);
            SpellIDEffectMap.Add(1006, 10);
            SpellIDEffectMap.Add(1007, 10);
            SpellIDEffectMap.Add(1008, 11);
            SpellIDEffectMap.Add(1009, 131);
            SpellIDEffectMap.Add(1010, 42);
            SpellIDEffectMap.Add(1011, 132);
            SpellIDEffectMap.Add(1012, 112);
            SpellIDEffectMap.Add(1013, 11);
            SpellIDEffectMap.Add(1014, 2);
            SpellIDEffectMap.Add(1015, 21);
            SpellIDEffectMap.Add(1016, 80);
            SpellIDEffectMap.Add(1017, 2);
            SpellIDEffectMap.Add(1018, 81);
            SpellIDEffectMap.Add(1019, 28);
            SpellIDEffectMap.Add(1020, 29);
            SpellIDEffectMap.Add(1021, 81);
            SpellIDEffectMap.Add(1022, 81);
            SpellIDEffectMap.Add(1023, 39);
            SpellIDEffectMap.Add(1024, 86);
            SpellIDEffectMap.Add(1025, 28);
            SpellIDEffectMap.Add(1026, 92);
            SpellIDEffectMap.Add(1027, 2);
            SpellIDEffectMap.Add(1028, 64);
            SpellIDEffectMap.Add(1029, 81);
            SpellIDEffectMap.Add(1030, 11);
            SpellIDEffectMap.Add(1031, 94);
            SpellIDEffectMap.Add(1032, 81);
            SpellIDEffectMap.Add(1033, 81);
            SpellIDEffectMap.Add(1034, 81);
            SpellIDEffectMap.Add(1035, 39);
            SpellIDEffectMap.Add(1036, 15);
            SpellIDEffectMap.Add(1037, 11);
            SpellIDEffectMap.Add(1038, 29);
            SpellIDEffectMap.Add(1039, 11);
            SpellIDEffectMap.Add(1040, 81);
            SpellIDEffectMap.Add(1041, 81);
            SpellIDEffectMap.Add(1042, 81);
            SpellIDEffectMap.Add(1043, 90);
            SpellIDEffectMap.Add(1044, 42);
            SpellIDEffectMap.Add(1045, 21);
            SpellIDEffectMap.Add(1046, 28);
            SpellIDEffectMap.Add(1047, 77);
            SpellIDEffectMap.Add(1048, 28);
            SpellIDEffectMap.Add(1049, 28);
            SpellIDEffectMap.Add(1050, 2);
            SpellIDEffectMap.Add(1051, 13);
            SpellIDEffectMap.Add(1052, 10);
            SpellIDEffectMap.Add(1053, 10);
            SpellIDEffectMap.Add(1054, 8);
            SpellIDEffectMap.Add(1055, 21);
            SpellIDEffectMap.Add(1056, 1);
            SpellIDEffectMap.Add(1057, 1);
            SpellIDEffectMap.Add(1058, 2);
            SpellIDEffectMap.Add(1059, 48);
            SpellIDEffectMap.Add(1060, 2);
            SpellIDEffectMap.Add(1061, 2);
            SpellIDEffectMap.Add(1062, 1);
            SpellIDEffectMap.Add(1063, 1);
            SpellIDEffectMap.Add(1064, 1);
            SpellIDEffectMap.Add(1065, 23);
            SpellIDEffectMap.Add(1066, 103);
            SpellIDEffectMap.Add(1067, 10);
            SpellIDEffectMap.Add(1068, 82);
            SpellIDEffectMap.Add(1069, 1);
            SpellIDEffectMap.Add(1070, 1);
            SpellIDEffectMap.Add(1071, 35);
            SpellIDEffectMap.Add(1072, 118);
            SpellIDEffectMap.Add(1073, 1);
            SpellIDEffectMap.Add(1074, 10);
            SpellIDEffectMap.Add(1075, 2);
            SpellIDEffectMap.Add(1076, 1);
            SpellIDEffectMap.Add(1077, 126);
            SpellIDEffectMap.Add(1078, 42);
            SpellIDEffectMap.Add(1079, 82);
            SpellIDEffectMap.Add(1080, 29);
            SpellIDEffectMap.Add(1081, 37);
            SpellIDEffectMap.Add(1082, 103);
            SpellIDEffectMap.Add(1083, 21);
            SpellIDEffectMap.Add(1084, 42);
            SpellIDEffectMap.Add(1085, 1);
            SpellIDEffectMap.Add(1086, 1);
            SpellIDEffectMap.Add(1087, 38);
            SpellIDEffectMap.Add(1088, 1);
            SpellIDEffectMap.Add(1089, 2);
            SpellIDEffectMap.Add(1090, 101);
            SpellIDEffectMap.Add(1091, 50);
            SpellIDEffectMap.Add(1092, 1);
            SpellIDEffectMap.Add(1093, 89);
            SpellIDEffectMap.Add(1094, 126);
            SpellIDEffectMap.Add(1095, 103);
            SpellIDEffectMap.Add(1096, 81);
            SpellIDEffectMap.Add(1097, 37);
            SpellIDEffectMap.Add(1098, 89);
            SpellIDEffectMap.Add(1099, 89);
            SpellIDEffectMap.Add(1100, 42);
            SpellIDEffectMap.Add(1101, 82);
            SpellIDEffectMap.Add(1102, 81);
            SpellIDEffectMap.Add(1103, 21);
            SpellIDEffectMap.Add(1104, 1);
            SpellIDEffectMap.Add(1105, 10);
            SpellIDEffectMap.Add(1106, 10);
            SpellIDEffectMap.Add(1107, 2);
            SpellIDEffectMap.Add(1108, 89);
            SpellIDEffectMap.Add(1109, 63);
            SpellIDEffectMap.Add(1110, 81);
            SpellIDEffectMap.Add(1111, 35);
            SpellIDEffectMap.Add(1112, 102);
            SpellIDEffectMap.Add(1113, 500);
            SpellIDEffectMap.Add(1114, 1);
            SpellIDEffectMap.Add(1115, 81);
            SpellIDEffectMap.Add(1116, 89);
            SpellIDEffectMap.Add(1117, 108);
            SpellIDEffectMap.Add(1118, 1);
            SpellIDEffectMap.Add(1119, 1);
            SpellIDEffectMap.Add(1120, 108);
            SpellIDEffectMap.Add(1121, 1);
            SpellIDEffectMap.Add(1122, 1);
            SpellIDEffectMap.Add(1123, 38);
            SpellIDEffectMap.Add(1124, 10);
            SpellIDEffectMap.Add(1125, 10);
            SpellIDEffectMap.Add(1126, 1);
            SpellIDEffectMap.Add(1127, 81);
            SpellIDEffectMap.Add(1128, 1);
            SpellIDEffectMap.Add(1129, 136);
            SpellIDEffectMap.Add(1130, 136);
            SpellIDEffectMap.Add(1131, 21);
            SpellIDEffectMap.Add(1132, 137);
            SpellIDEffectMap.Add(1133, 1);
            SpellIDEffectMap.Add(1134, 1);
            SpellIDEffectMap.Add(1135, 1);
            SpellIDEffectMap.Add(1136, 1);
            SpellIDEffectMap.Add(1137, 1);
            SpellIDEffectMap.Add(1138, 511);
            SpellIDEffectMap.Add(1139, 511);
            SpellIDEffectMap.Add(1140, 1);
            SpellIDEffectMap.Add(1141, 82);
            SpellIDEffectMap.Add(1142, 2);
            SpellIDEffectMap.Add(1143, 4);
            SpellIDEffectMap.Add(1144, 13);
            SpellIDEffectMap.Add(1145, 8);
            SpellIDEffectMap.Add(1146, 1);
            SpellIDEffectMap.Add(1147, 1);
            SpellIDEffectMap.Add(1148, 21);
            SpellIDEffectMap.Add(1149, 1);
            SpellIDEffectMap.Add(1150, 21);
            SpellIDEffectMap.Add(1151, 21);
            SpellIDEffectMap.Add(1152, 21);
            SpellIDEffectMap.Add(1153, 1);
            SpellIDEffectMap.Add(1154, 23);
            SpellIDEffectMap.Add(1155, 140);
            SpellIDEffectMap.Add(1156, 21);
            SpellIDEffectMap.Add(1157, 1);
            SpellIDEffectMap.Add(1158, 21);
            SpellIDEffectMap.Add(1159, 1);
            SpellIDEffectMap.Add(1160, 21);
            SpellIDEffectMap.Add(1161, 21);
            SpellIDEffectMap.Add(1162, 21);
            SpellIDEffectMap.Add(1163, 21);
            SpellIDEffectMap.Add(1164, 2);
            SpellIDEffectMap.Add(1165, 11);
            SpellIDEffectMap.Add(1166, 500);
            SpellIDEffectMap.Add(1167, 1);
            SpellIDEffectMap.Add(1168, 141);
            SpellIDEffectMap.Add(1169, 21);
            SpellIDEffectMap.Add(1170, 21);
            SpellIDEffectMap.Add(1171, 1);
            SpellIDEffectMap.Add(1172, 89);
            SpellIDEffectMap.Add(1173, 1);
            SpellIDEffectMap.Add(1174, 21);
            SpellIDEffectMap.Add(1175, 1);
            SpellIDEffectMap.Add(1176, 21);
            SpellNameEffectMap.Add("Nothing", 0);
            SpellNameEffectMap.Add("Minor Area Shock", 109);
            SpellNameEffectMap.Add("Major Area Shock", 2);
            SpellNameEffectMap.Add("Large Area Heat Shock", 3);
            SpellNameEffectMap.Add("Mark", 600);
            SpellNameEffectMap.Add("Thunder Shock", 2);
            SpellNameEffectMap.Add("rain stone", 109);
            SpellNameEffectMap.Add("Minor Fear", 4);
            SpellNameEffectMap.Add("Area Weak Poison", 7);
            SpellNameEffectMap.Add("Minor Chill Stun", 3);
            SpellNameEffectMap.Add("Army of Zombies", 1);
            SpellNameEffectMap.Add("Area Decay", 11);
            SpellNameEffectMap.Add("Court of Flame Childs", 1);
            SpellNameEffectMap.Add("Area Rust", 11);
            SpellNameEffectMap.Add("Returning", 15);
            SpellNameEffectMap.Add("Area Paralyze", 66);
            SpellNameEffectMap.Add("Court of Undines", 1);
            SpellNameEffectMap.Add("Summon Longdeads", 1);
            SpellNameEffectMap.Add("Summon Devil", 1);
            SpellNameEffectMap.Add("Fire Resistance", 10);
            SpellNameEffectMap.Add("Large Area Decay", 11);
            SpellNameEffectMap.Add("Major Fear", 4);
            SpellNameEffectMap.Add("Summon 2 Imps", 1);
            SpellNameEffectMap.Add("Summon Vine Man", 1);
            SpellNameEffectMap.Add("Area Feeble Mind", 11);
            SpellNameEffectMap.Add("10 Trolls", 1);
            SpellNameEffectMap.Add("20 imps", 1);
            SpellNameEffectMap.Add("Strength, Barkskin and Regeneration", 10);
            SpellNameEffectMap.Add("Court of Gnomes", 1);
            SpellNameEffectMap.Add("Court of Sylphs", 1);
            SpellNameEffectMap.Add("Phantasmal Archers", 38);
            SpellNameEffectMap.Add("Meteor Shower", 2);
            SpellNameEffectMap.Add("Mummification", 21);
            SpellNameEffectMap.Add("Summon Jinn", 21);
            SpellNameEffectMap.Add("Ark", 81);
            SpellNameEffectMap.Add("Area Chest Wound", 11);
            SpellNameEffectMap.Add("10 Sea Trolls", 1);
            SpellNameEffectMap.Add("15 Draconians", 1);
            SpellNameEffectMap.Add("Court of Sprites", 1);
            SpellNameEffectMap.Add("Heat Stun", 3);
            SpellNameEffectMap.Add("Extra feeble mind battle field", 11);
            SpellNameEffectMap.Add("Extra cold immunity", 599);
            SpellNameEffectMap.Add("Minor Paralysis", 66);
            SpellNameEffectMap.Add("extra limp", 11);
            SpellNameEffectMap.Add("extra cripple", 11);
            SpellNameEffectMap.Add("15 Ether Warriors", 1);
            SpellNameEffectMap.Add("Extra Soulless", 37);
            SpellNameEffectMap.Add("Entangle", 11);
            SpellNameEffectMap.Add("ormflock", 1);
            SpellNameEffectMap.Add("impflock", 1);
            SpellNameEffectMap.Add("7 shades", 1);
            SpellNameEffectMap.Add("3 beast bats", 1);
            SpellNameEffectMap.Add("4 lions", 1);
            SpellNameEffectMap.Add("10 wolves", 1);
            SpellNameEffectMap.Add("cast returning", 23);
            SpellNameEffectMap.Add("Grow Knight", 1);
            SpellNameEffectMap.Add("Grow Lich", 21);
            SpellNameEffectMap.Add("Scrying", 85);
            SpellNameEffectMap.Add("Summon Qarin", 21);
            SpellNameEffectMap.Add("Grow Headless Hoburg", 1);
            SpellNameEffectMap.Add("5 War Trolls", 1);
            SpellNameEffectMap.Add("2 Troll Moose Knights", 1);
            SpellNameEffectMap.Add("Telkhine Malediction", 81);
            SpellNameEffectMap.Add("", 11);
            SpellNameEffectMap.Add("Wolves", 1);
            SpellNameEffectMap.Add("Battle Darkness", 81);
            SpellNameEffectMap.Add("Beast Bats", 1);
            SpellNameEffectMap.Add("Angels of the Choir", 1);
            SpellNameEffectMap.Add("Harbingers of the Choir", 1);
            SpellNameEffectMap.Add("Heat of Buer", 81);
            SpellNameEffectMap.Add("Disbelieve", 105);
            SpellNameEffectMap.Add("5 Troll Guards", 1);
            SpellNameEffectMap.Add("Gate Summon Fire", 1);
            SpellNameEffectMap.Add("Gate Summon Ice", 1);
            SpellNameEffectMap.Add("Gate Summon Storm", 1);
            SpellNameEffectMap.Add("Gate Summon Iron", 1);
            SpellNameEffectMap.Add("10 Tengu Warriors", 1);
            SpellNameEffectMap.Add("15 Karasu Tengus", 1);
            SpellNameEffectMap.Add("Natural Rain", 81);
            SpellNameEffectMap.Add("Open Soul Trap", 21);
            SpellNameEffectMap.Add("area10 cold dmg3", 2);
            SpellNameEffectMap.Add("Cleansing Chime", 2);
            SpellNameEffectMap.Add("Astral Geyser Blast", 2);
            SpellNameEffectMap.Add("Chastisement", 28);
            SpellNameEffectMap.Add("age ten years", 101);
            SpellNameEffectMap.Add("age three years", 101);
            SpellNameEffectMap.Add("6 Maenads", 1);
            SpellNameEffectMap.Add("10 False Horrors", 1);
            SpellNameEffectMap.Add("4 Wheels", 1);
            SpellNameEffectMap.Add("4 Ditanu", 1);
            SpellNameEffectMap.Add("Kill Caster", 112);
            SpellNameEffectMap.Add("1 Horse-face", 1);
            SpellNameEffectMap.Add("Natural Storm", 81);
            SpellNameEffectMap.Add("Astral Harpoon", 113);
            SpellNameEffectMap.Add("Swarm", 1);
            SpellNameEffectMap.Add("Sounder of Boars", 1);
            SpellNameEffectMap.Add("Pack of Wolves", 1);
            SpellNameEffectMap.Add("Grow Monster", 1);
            SpellNameEffectMap.Add("15 Forest Trolls", 1);
            SpellNameEffectMap.Add("earth grip", 11);
            SpellNameEffectMap.Add("Disease", 11);
            SpellNameEffectMap.Add("Disease All Friendly", 11);
            SpellNameEffectMap.Add("Area Cripple", 11);
            SpellNameEffectMap.Add("Record of Creation", 48);
            SpellNameEffectMap.Add("Battlefield Limp", 11);
            SpellNameEffectMap.Add("Battlefield Cripple", 11);
            SpellNameEffectMap.Add("10 Great Olms", 1);
            SpellNameEffectMap.Add("Summon Predatory Birds", 1);
            SpellNameEffectMap.Add("Burning", 11);
            SpellNameEffectMap.Add("Blessing", 10);
            SpellNameEffectMap.Add("Banishment", 2);
            SpellNameEffectMap.Add("Ashes to Ashes", 2);
            SpellNameEffectMap.Add("Purifying Water", 2);
            SpellNameEffectMap.Add("Cleansing", 2);
            SpellNameEffectMap.Add("Pull from the Grave", 2);
            SpellNameEffectMap.Add("Grip", 11);
            SpellNameEffectMap.Add("Wind of Memories", 2);
            SpellNameEffectMap.Add("Memories of Death", 2);
            SpellNameEffectMap.Add("Final Rest", 2);
            SpellNameEffectMap.Add("Decree of the Underworld", 2);
            SpellNameEffectMap.Add("Bewilderment", 11);
            SpellNameEffectMap.Add("Stellar Decree", 2);
            SpellNameEffectMap.Add("Halt", 128);
            SpellNameEffectMap.Add("Sermon of Courage", 10);
            SpellNameEffectMap.Add("Smite Demon", 2);
            SpellNameEffectMap.Add("Holy Word", 128);
            SpellNameEffectMap.Add("Holy Avenger", 10);
            SpellNameEffectMap.Add("Divine Blessing", 10);
            SpellNameEffectMap.Add("Smite", 2);
            SpellNameEffectMap.Add("Heavenly Fire", 2);
            SpellNameEffectMap.Add("Fiery Death", 2);
            SpellNameEffectMap.Add("Watery Death", 2);
            SpellNameEffectMap.Add("Drowning", 2);
            SpellNameEffectMap.Add("Word of Stone", 2);
            SpellNameEffectMap.Add("Petrification", 99);
            SpellNameEffectMap.Add("Heavenly Strike", 2);
            SpellNameEffectMap.Add("Lightning Death", 2);
            SpellNameEffectMap.Add("Word of Power", 2);
            SpellNameEffectMap.Add("Paralyzation", 66);
            SpellNameEffectMap.Add("Syllable of Death", 2);
            SpellNameEffectMap.Add("Death", 2);
            SpellNameEffectMap.Add("Exhaustion", 3);
            SpellNameEffectMap.Add("Word of Thorns", 2);
            SpellNameEffectMap.Add("Tangles", 11);
            SpellNameEffectMap.Add("Bleeding", 11);
            SpellNameEffectMap.Add("Claim Life", 2);
            SpellNameEffectMap.Add("Chestwound", 11);
            SpellNameEffectMap.Add("Fanaticism", 10);
            SpellNameEffectMap.Add("Divine Channeling", 81);
            SpellNameEffectMap.Add("Fire Flies", 2);
            SpellNameEffectMap.Add("Air Shield", 10);
            SpellNameEffectMap.Add("Freezing Touch", 2);
            SpellNameEffectMap.Add("Flying Shards", 2);
            SpellNameEffectMap.Add("Twist Fate", 23);
            SpellNameEffectMap.Add("Hand of Dust", 2);
            SpellNameEffectMap.Add("Sleep Ray", 11);
            SpellNameEffectMap.Add("Bleed", 11);
            SpellNameEffectMap.Add("Monster Boar", 42);
            SpellNameEffectMap.Add("Orgy", 21);
            SpellNameEffectMap.Add("Daughter of Typhon", 93);
            SpellNameEffectMap.Add("Gift of the Sacred Swamp", 10);
            SpellNameEffectMap.Add("Awaken Hamadryad", 21);
            SpellNameEffectMap.Add("Contact Lar", 21);
            SpellNameEffectMap.Add("Awaken Tattoos", 23);
            SpellNameEffectMap.Add("Contact Boar of Carnutes", 21);
            SpellNameEffectMap.Add("Contact Huli Jing", 21);
            SpellNameEffectMap.Add("Thousand Year Ginseng", 101);
            SpellNameEffectMap.Add("Internal Alchemy", 111);
            SpellNameEffectMap.Add("Hannya Pact", 130);
            SpellNameEffectMap.Add("Greater Hannya Pact", 130);
            SpellNameEffectMap.Add("End of Culture", 81);
            SpellNameEffectMap.Add("End of Weakness", 10);
            SpellNameEffectMap.Add("Teaching Sign", 23);
            SpellNameEffectMap.Add("Fear-not Sign", 10);
            SpellNameEffectMap.Add("Welcome Sign", 29);
            SpellNameEffectMap.Add("Earth-touching Sign", 2);
            SpellNameEffectMap.Add("Meditation Sign", 8);
            SpellNameEffectMap.Add("Summon Shikome", 1);
            SpellNameEffectMap.Add("Contact Jigami", 21);
            SpellNameEffectMap.Add("Contact Mori-no-kami", 21);
            SpellNameEffectMap.Add("Summon Ujigami", 21);
            SpellNameEffectMap.Add("Contact Kaijin", 21);
            SpellNameEffectMap.Add("Contact Tatsu", 21);
            SpellNameEffectMap.Add("Summon Kenzoku", 21);
            SpellNameEffectMap.Add("Summon Gozu Mezu", 1);
            SpellNameEffectMap.Add("Contact Yama-no-kami", 21);
            SpellNameEffectMap.Add("Summon Abysian Ancestors", 1);
            SpellNameEffectMap.Add("Reawaken Fossil", 1);
            SpellNameEffectMap.Add("Summon Spectral Infantry", 1);
            SpellNameEffectMap.Add("Contact Scorpion Man", 1);
            SpellNameEffectMap.Add("Inner Furnace", 10);
            SpellNameEffectMap.Add("Infernal Breeding", 127);
            SpellNameEffectMap.Add("Summon Bears", 1);
            SpellNameEffectMap.Add("Summon Simargl", 1);
            SpellNameEffectMap.Add("Summon Firebird", 1);
            SpellNameEffectMap.Add("Send Lady Midday", 119);
            SpellNameEffectMap.Add("Contact Sirin", 21);
            SpellNameEffectMap.Add("Send Vodyanoy", 119);
            SpellNameEffectMap.Add("Summon Rusalka", 21);
            SpellNameEffectMap.Add("Summon Likho", 21);
            SpellNameEffectMap.Add("Contact Alkonost", 21);
            SpellNameEffectMap.Add("Summon Zmey", 1);
            SpellNameEffectMap.Add("Send Bukavac", 38);
            SpellNameEffectMap.Add("Contact Gamayun", 21);
            SpellNameEffectMap.Add("Contact Beregina", 21);
            SpellNameEffectMap.Add("Contact Mountain Vila", 21);
            SpellNameEffectMap.Add("Contact Cloud Vila", 21);
            SpellNameEffectMap.Add("Contact Leshiy", 21);
            SpellNameEffectMap.Add("Grow Fortress", 63);
            SpellNameEffectMap.Add("Fort of the Ancients", 63);
            SpellNameEffectMap.Add("Sacred Crocodile", 1);
            SpellNameEffectMap.Add("Herd of Elephants", 1);
            SpellNameEffectMap.Add("Call Melqart", 21);
            SpellNameEffectMap.Add("Strange Fire", 24);
            SpellNameEffectMap.Add("Memories of Stone", 1);
            SpellNameEffectMap.Add("Dirge for the Dead", 21);
            SpellNameEffectMap.Add("Banquet for the Dead", 21);
            SpellNameEffectMap.Add("Scapegoats", 1);
            SpellNameEffectMap.Add("Summon Se'irim", 1);
            SpellNameEffectMap.Add("Summon Shedim", 1);
            SpellNameEffectMap.Add("Call Malakh", 21);
            SpellNameEffectMap.Add("Call Hashmal", 21);
            SpellNameEffectMap.Add("Call Arel", 21);
            SpellNameEffectMap.Add("Call Ophan", 21);
            SpellNameEffectMap.Add("Call Merkavah", 21);
            SpellNameEffectMap.Add("Release Lord of Civilization", 89);
            SpellNameEffectMap.Add("Summon Mazzikim", 1);
            SpellNameEffectMap.Add("Summon Lilot", 21);
            SpellNameEffectMap.Add("Summon Kusarikkus", 1);
            SpellNameEffectMap.Add("Summon Ugallu", 21);
            SpellNameEffectMap.Add("Call Anzu", 1);
            SpellNameEffectMap.Add("Call Apkallu", 21);
            SpellNameEffectMap.Add("Call Ephor", 21);
            SpellNameEffectMap.Add("Call Spectral Philosopher", 21);
            SpellNameEffectMap.Add("Summon Telkhine", 21);
            SpellNameEffectMap.Add("Summon Hekateride", 21);
            SpellNameEffectMap.Add("Summon Daktyl", 21);
            SpellNameEffectMap.Add("Summon Monster Fish", 1);
            SpellNameEffectMap.Add("Send Tupilak", 50);
            SpellNameEffectMap.Add("Unholy Command", 28);
            SpellNameEffectMap.Add("Unholy Protection", 10);
            SpellNameEffectMap.Add("Unholy Blessing", 10);
            SpellNameEffectMap.Add("Unholy Power", 23);
            SpellNameEffectMap.Add("Apostasy", 29);
            SpellNameEffectMap.Add("Protection of the Sepulchre", 10);
            SpellNameEffectMap.Add("Power of the Sepulchre", 23);
            SpellNameEffectMap.Add("Revive Lictor", 1);
            SpellNameEffectMap.Add("Revive Censor", 21);
            SpellNameEffectMap.Add("Revive Acolyte", 21);
            SpellNameEffectMap.Add("Revive Bishop", 21);
            SpellNameEffectMap.Add("Revive Arch Bishop", 21);
            SpellNameEffectMap.Add("Revive Spectator", 21);
            SpellNameEffectMap.Add("Revive Dusk Elder", 21);
            SpellNameEffectMap.Add("Revive Wailing Lady", 1);
            SpellNameEffectMap.Add("Lictorian Guard", 1);
            SpellNameEffectMap.Add("Lamentation", 1);
            SpellNameEffectMap.Add("Great Lamentation", 1);
            SpellNameEffectMap.Add("Lictorian Legion", 1);
            SpellNameEffectMap.Add("Ermorian Legion", 1);
            SpellNameEffectMap.Add("Revive Shadow Tribune", 21);
            SpellNameEffectMap.Add("Revive Lemur Centurion", 21);
            SpellNameEffectMap.Add("Revive Lemur Senator", 21);
            SpellNameEffectMap.Add("Revive Lemur Consul", 21);
            SpellNameEffectMap.Add("Revive Lemur Acolyte", 21);
            SpellNameEffectMap.Add("Revive Lemur Thaumaturg", 21);
            SpellNameEffectMap.Add("Revive Grand Lemur", 21);
            SpellNameEffectMap.Add("Anathema", 11);
            SpellNameEffectMap.Add("Protection of the Shadelands", 10);
            SpellNameEffectMap.Add("Power of the Shadelands", 23);
            SpellNameEffectMap.Add("Revive Grave Consort", 21);
            SpellNameEffectMap.Add("Revive Tomb Priest", 21);
            SpellNameEffectMap.Add("Revive Tomb King", 21);
            SpellNameEffectMap.Add("Protection of the Grave", 10);
            SpellNameEffectMap.Add("Power of the Grave", 23);
            SpellNameEffectMap.Add("Royal Power", 23);
            SpellNameEffectMap.Add("Royal Protection", 10);
            SpellNameEffectMap.Add("Power of the Reborn King", 23);
            SpellNameEffectMap.Add("Divination", 86);
            SpellNameEffectMap.Add("Tune of Fear", 4);
            SpellNameEffectMap.Add("Tune of Growth", 11);
            SpellNameEffectMap.Add("Tune of Dancing Death", 3);
            SpellNameEffectMap.Add("Carrion Centaur", 21);
            SpellNameEffectMap.Add("Carrion Lady", 21);
            SpellNameEffectMap.Add("Carrion Lord", 21);
            SpellNameEffectMap.Add("Quick Roots", 23);
            SpellNameEffectMap.Add("Regrowth", 23);
            SpellNameEffectMap.Add("Mend the Dead", 13);
            SpellNameEffectMap.Add("Puppet Mastery", 23);
            SpellNameEffectMap.Add("Carrion Growth", 23);
            SpellNameEffectMap.Add("Dark Slumber", 42);
            SpellNameEffectMap.Add("Summon Black Dogs", 1);
            SpellNameEffectMap.Add("Summon Cu Sidhe", 1);
            SpellNameEffectMap.Add("Contact Cu Sidhe", 1);
            SpellNameEffectMap.Add("Summon Barghests", 1);
            SpellNameEffectMap.Add("Summon Bean Sidhe", 21);
            SpellNameEffectMap.Add("Summon Morrigan", 1);
            SpellNameEffectMap.Add("Dance of the Morrigans", 81);
            SpellNameEffectMap.Add("Iron Darts", 73);
            SpellNameEffectMap.Add("Iron Blizzard", 73);
            SpellNameEffectMap.Add("Contact Iron Angel", 21);
            SpellNameEffectMap.Add("Tempering the Will", 10);
            SpellNameEffectMap.Add("Gift of the Moon", 23);
            SpellNameEffectMap.Add("Sanguine Heritage", 89);
            SpellNameEffectMap.Add("Summon Monster Toads", 1);
            SpellNameEffectMap.Add("Contact Couatl", 21);
            SpellNameEffectMap.Add("Parting of the Soul", 66);
            SpellNameEffectMap.Add("Call Ahurani", 21);
            SpellNameEffectMap.Add("Call Celestial Yazad", 21);
            SpellNameEffectMap.Add("Call Fravashi", 21);
            SpellNameEffectMap.Add("Call Amesha Spenta", 89);
            SpellNameEffectMap.Add("Summon Yazatas", 1);
            SpellNameEffectMap.Add("Call Daevas", 1);
            SpellNameEffectMap.Add("Call Jahi", 21);
            SpellNameEffectMap.Add("Call Yata", 21);
            SpellNameEffectMap.Add("Call of the Drugvant", 42);
            SpellNameEffectMap.Add("Call Greater Daeva", 89);
            SpellNameEffectMap.Add("Geoglyphs", 82);
            SpellNameEffectMap.Add("Eyes of the Condors", 85);
            SpellNameEffectMap.Add("Summon Condors", 1);
            SpellNameEffectMap.Add("Summon Huacas", 1);
            SpellNameEffectMap.Add("Summon Supayas", 1);
            SpellNameEffectMap.Add("Contact Harbinger", 21);
            SpellNameEffectMap.Add("Angelic Host", 37);
            SpellNameEffectMap.Add("Heavenly Wrath", 21);
            SpellNameEffectMap.Add("Heavenly Choir", 21);
            SpellNameEffectMap.Add("Bind Harlequin", 1);
            SpellNameEffectMap.Add("Reascendance", 21);
            SpellNameEffectMap.Add("Summon Valkyries", 1);
            SpellNameEffectMap.Add("Awaken Draugar", 1);
            SpellNameEffectMap.Add("Summon Glosos", 1);
            SpellNameEffectMap.Add("Brood of Garm", 1);
            SpellNameEffectMap.Add("Illwinter", 81);
            SpellNameEffectMap.Add("Summon Jaguar Toads", 1);
            SpellNameEffectMap.Add("Summon Jaguars", 1);
            SpellNameEffectMap.Add("Summon Jade Serpent", 1);
            SpellNameEffectMap.Add("Summon Monster Toad", 1);
            SpellNameEffectMap.Add("Summon Tlaloque", 89);
            SpellNameEffectMap.Add("Bind Beast Bats", 1);
            SpellNameEffectMap.Add("Bind Jaguar Fiends", 1);
            SpellNameEffectMap.Add("Contact Civateteo", 21);
            SpellNameEffectMap.Add("Bind Tzitzimitl", 1);
            SpellNameEffectMap.Add("Contact Tlahuelpuchi", 21);
            SpellNameEffectMap.Add("Contact Onaqui", 21);
            SpellNameEffectMap.Add("Rain of Jaguars", 1);
            SpellNameEffectMap.Add("Theft of the Sun", 81);
            SpellNameEffectMap.Add("Summon Sacred Scorpion", 1);
            SpellNameEffectMap.Add("Break the First Soul", 11);
            SpellNameEffectMap.Add("Break the Second Soul", 11);
            SpellNameEffectMap.Add("Break the Third Soul", 3);
            SpellNameEffectMap.Add("Break the Fourth Soul", 11);
            SpellNameEffectMap.Add("Gift of the First Soul", 10);
            SpellNameEffectMap.Add("Gift of the Second Soul", 10);
            SpellNameEffectMap.Add("Gift of the Third Soul", 23);
            SpellNameEffectMap.Add("Gift of the Fourth Soul", 10);
            SpellNameEffectMap.Add("Summon Balam", 89);
            SpellNameEffectMap.Add("Summon Chaac", 89);
            SpellNameEffectMap.Add("Celestial Servant", 1);
            SpellNameEffectMap.Add("Heavenly Rivers", 1);
            SpellNameEffectMap.Add("Celestial Hounds", 1);
            SpellNameEffectMap.Add("Heavenly Fires", 1);
            SpellNameEffectMap.Add("Call Celestial Soldiers", 1);
            SpellNameEffectMap.Add("Call Ancestor", 1);
            SpellNameEffectMap.Add("Wrath of the Ancestors", 1);
            SpellNameEffectMap.Add("Summon Nagas", 1);
            SpellNameEffectMap.Add("Summon Apsaras", 1);
            SpellNameEffectMap.Add("Contact Yaksha", 21);
            SpellNameEffectMap.Add("Contact Yakshini", 21);
            SpellNameEffectMap.Add("Contact Nagini", 21);
            SpellNameEffectMap.Add("Summon Gandharvas", 1);
            SpellNameEffectMap.Add("Contact Nagaraja", 21);
            SpellNameEffectMap.Add("Summon Kinnara", 21);
            SpellNameEffectMap.Add("Contact Nagarishi", 21);
            SpellNameEffectMap.Add("Summon Siddha", 21);
            SpellNameEffectMap.Add("Summon Devata", 21);
            SpellNameEffectMap.Add("Summon Devala", 21);
            SpellNameEffectMap.Add("Summon Rudra", 21);
            SpellNameEffectMap.Add("Celestial Music", 10);
            SpellNameEffectMap.Add("Summon Rakshasas", 1);
            SpellNameEffectMap.Add("Feast of Flesh", 1);
            SpellNameEffectMap.Add("Summon Asrapas", 1);
            SpellNameEffectMap.Add("Summon Rakshasa Warriors", 1);
            SpellNameEffectMap.Add("Summon Sandhyabalas", 1);
            SpellNameEffectMap.Add("Summon Dakini", 21);
            SpellNameEffectMap.Add("Summon Samanishada", 21);
            SpellNameEffectMap.Add("Summon Mandeha", 21);
            SpellNameEffectMap.Add("Summon Danavas", 1);
            SpellNameEffectMap.Add("Host of Ganas", 1);
            SpellNameEffectMap.Add("Summon Vetalas", 1);
            SpellNameEffectMap.Add("Summon Ko-Oni", 1);
            SpellNameEffectMap.Add("Summon Kappa", 1);
            SpellNameEffectMap.Add("Summon Ao-Oni", 1);
            SpellNameEffectMap.Add("Summon Karasu Tengus", 1);
            SpellNameEffectMap.Add("Summon Aka-Oni", 1);
            SpellNameEffectMap.Add("Summon Konoha Tengus", 1);
            SpellNameEffectMap.Add("Ghost General", 21);
            SpellNameEffectMap.Add("Summon Oni", 1);
            SpellNameEffectMap.Add("Contact Dai Tengu", 21);
            SpellNameEffectMap.Add("Contact Nushi", 21);
            SpellNameEffectMap.Add("Summon Kuro-Oni", 1);
            SpellNameEffectMap.Add("Summon Oni General", 21);
            SpellNameEffectMap.Add("Contact Kitsune", 21);
            SpellNameEffectMap.Add("Summon Dai Oni", 21);
            SpellNameEffectMap.Add("Bind Penumbral", 1);
            SpellNameEffectMap.Add("Summon Penumbrals", 1);
            SpellNameEffectMap.Add("Summon Umbrals", 1);
            SpellNameEffectMap.Add("Olm Conclave", 21);
            SpellNameEffectMap.Add("Hall of Statues", 1);
            SpellNameEffectMap.Add("Revive Cavern Wights", 1);
            SpellNameEffectMap.Add("Bind Umbral", 1);
            SpellNameEffectMap.Add("Unleash Imprisoned Ones", 120);
            SpellNameEffectMap.Add("Rhuax Pact", 1);
            SpellNameEffectMap.Add("Barathrus Pact", 1);
            SpellNameEffectMap.Add("Mirror of Earths Memories", 48);
            SpellNameEffectMap.Add("Attentive Statues", 1);
            SpellNameEffectMap.Add("Enliven Sentinel", 1);
            SpellNameEffectMap.Add("Enliven Granite Guard", 1);
            SpellNameEffectMap.Add("Enliven Marble Oracle", 21);
            SpellNameEffectMap.Add("Animate Mercury", 1);
            SpellNameEffectMap.Add("Living Mercury", 1);
            SpellNameEffectMap.Add("Nightmare Construction", 1);
            SpellNameEffectMap.Add("Iron Corpse Reanimation", 1);
            SpellNameEffectMap.Add("Reanimate Ancestor", 21);
            SpellNameEffectMap.Add("Flame Corpse Construction", 1);
            SpellNameEffectMap.Add("Ktonian Legion", 1);
            SpellNameEffectMap.Add("Awaken Shard Wights", 1);
            SpellNameEffectMap.Add("Awaken Sepulchral", 1);
            SpellNameEffectMap.Add("Awaken Tomb Oracle", 21);
            SpellNameEffectMap.Add("Hall of the Dead", 1);
            SpellNameEffectMap.Add("Iron Marionettes", 23);
            SpellNameEffectMap.Add("Contact Void Spectre", 21);
            SpellNameEffectMap.Add("Mind Vessel", 125);
            SpellNameEffectMap.Add("Enslave Sea Trolls", 1);
            SpellNameEffectMap.Add("Dreams of R'lyeh", 110);
            SpellNameEffectMap.Add("Burning Hands", 2);
            SpellNameEffectMap.Add("Fire Darts", 2);
            SpellNameEffectMap.Add("Flame Bolt", 2);
            SpellNameEffectMap.Add("Shocking Grasp", 2);
            SpellNameEffectMap.Add("Slime", 11);
            SpellNameEffectMap.Add("Cold Bolt", 2);
            SpellNameEffectMap.Add("Geyser", 2);
            SpellNameEffectMap.Add("Acid Spray", 2);
            SpellNameEffectMap.Add("Astral Projection", 85);
            SpellNameEffectMap.Add("Star Fires", 2);
            SpellNameEffectMap.Add("Arcane Bolt", 2);
            SpellNameEffectMap.Add("Vine Arrow", 2);
            SpellNameEffectMap.Add("Fire Blast", 2);
            SpellNameEffectMap.Add("Flare", 2);
            SpellNameEffectMap.Add("Sulphur Haze", 7);
            SpellNameEffectMap.Add("Cold Blast", 2);
            SpellNameEffectMap.Add("Rain", 81);
            SpellNameEffectMap.Add("Lightning Bolt", 2);
            SpellNameEffectMap.Add("Shock Wave", 2);
            SpellNameEffectMap.Add("Rust Mist", 11);
            SpellNameEffectMap.Add("Solar Rays", 2);
            SpellNameEffectMap.Add("Web", 11);
            SpellNameEffectMap.Add("Fireball", 2);
            SpellNameEffectMap.Add("Fires from Afar", 91);
            SpellNameEffectMap.Add("Mist", 81);
            SpellNameEffectMap.Add("Freezing Mist", 2);
            SpellNameEffectMap.Add("Acid Bolt", 2);
            SpellNameEffectMap.Add("Magma Bolts", 2);
            SpellNameEffectMap.Add("Arcane Probing", 48);
            SpellNameEffectMap.Add("Magic Duel", 27);
            SpellNameEffectMap.Add("Healing Light", 13);
            SpellNameEffectMap.Add("Shadow Bolt", 2);
            SpellNameEffectMap.Add("Sleep Cloud", 11);
            SpellNameEffectMap.Add("Holy Pyre", 24);
            SpellNameEffectMap.Add("Fire Cloud", 2);
            SpellNameEffectMap.Add("Fate of Oedipus", 22);
            SpellNameEffectMap.Add("Breath of the Desert", 42);
            SpellNameEffectMap.Add("Thunder Strike", 2);
            SpellNameEffectMap.Add("Hurricane", 42);
            SpellNameEffectMap.Add("Water Strike", 2);
            SpellNameEffectMap.Add("Acid Rain", 2);
            SpellNameEffectMap.Add("Blade Wind", 2);
            SpellNameEffectMap.Add("Nether Bolt", 2);
            SpellNameEffectMap.Add("Bolt of Unlife", 74);
            SpellNameEffectMap.Add("Bane Fire Dart", 2);
            SpellNameEffectMap.Add("Breath of the Dragon", 7);
            SpellNameEffectMap.Add("Falling Fires", 2);
            SpellNameEffectMap.Add("Liquid Flames of Rhuax", 2);
            SpellNameEffectMap.Add("Splash of Molten Metal", 2);
            SpellNameEffectMap.Add("Falling Frost", 2);
            SpellNameEffectMap.Add("Orb Lightning", 134);
            SpellNameEffectMap.Add("Storm", 81);
            SpellNameEffectMap.Add("Earthquake", 2);
            SpellNameEffectMap.Add("Cave Collapse", 2);
            SpellNameEffectMap.Add("Gifts from Heaven", 2);
            SpellNameEffectMap.Add("Stellar Cascades", 3);
            SpellNameEffectMap.Add("Astral Geyser", 601);
            SpellNameEffectMap.Add("Celestial Chastisement", 2);
            SpellNameEffectMap.Add("Shadow Blast", 2);
            SpellNameEffectMap.Add("Poison Cloud", 7);
            SpellNameEffectMap.Add("Healing Mists", 13);
            SpellNameEffectMap.Add("Flame Eruption", 2);
            SpellNameEffectMap.Add("Wrathful Skies", 81);
            SpellNameEffectMap.Add("Perpetual Storm", 81);
            SpellNameEffectMap.Add("Cleansing Water", 2);
            SpellNameEffectMap.Add("Magma Eruption", 2);
            SpellNameEffectMap.Add("Mind Hunt", 57);
            SpellNameEffectMap.Add("Astral Fires", 2);
            SpellNameEffectMap.Add("The Wrath of God", 81);
            SpellNameEffectMap.Add("Blast of Unlife", 74);
            SpellNameEffectMap.Add("Bane Fire", 2);
            SpellNameEffectMap.Add("Wailing Winds", 81);
            SpellNameEffectMap.Add("Stream of Life", 72);
            SpellNameEffectMap.Add("Fire Storm", 81);
            SpellNameEffectMap.Add("Ice Strike", 2);
            SpellNameEffectMap.Add("Murdering Winter", 41);
            SpellNameEffectMap.Add("Acid Storm", 81);
            SpellNameEffectMap.Add("Shimmering Fields", 2);
            SpellNameEffectMap.Add("Rain of Stones", 81);
            SpellNameEffectMap.Add("Storm of Thorns", 2);
            SpellNameEffectMap.Add("Nether Darts", 2);
            SpellNameEffectMap.Add("Cloud of Death", 2);
            SpellNameEffectMap.Add("Wind of Death", 11);
            SpellNameEffectMap.Add("Stygian Rains", 23);
            SpellNameEffectMap.Add("Pillar of Fire", 2);
            SpellNameEffectMap.Add("Second Sun", 81);
            SpellNameEffectMap.Add("Maelstrom", 81);
            SpellNameEffectMap.Add("Astral Tempest", 81);
            SpellNameEffectMap.Add("Vortex of Unlife", 74);
            SpellNameEffectMap.Add("Flames from the Sky", 41);
            SpellNameEffectMap.Add("Flame Storm", 2);
            SpellNameEffectMap.Add("Volcanic Eruption", 42);
            SpellNameEffectMap.Add("Chain Lightning", 134);
            SpellNameEffectMap.Add("Niefel Flames", 2);
            SpellNameEffectMap.Add("Tidal Wave", 42);
            SpellNameEffectMap.Add("Strands of Arcane Power", 81);
            SpellNameEffectMap.Add("Distill Gold", 117);
            SpellNameEffectMap.Add("Charge Body", 23);
            SpellNameEffectMap.Add("Aim", 10);
            SpellNameEffectMap.Add("False Fetters", 11);
            SpellNameEffectMap.Add("Fists of Iron", 2);
            SpellNameEffectMap.Add("Earth Grip", 11);
            SpellNameEffectMap.Add("Earth Might", 10);
            SpellNameEffectMap.Add("Hand of Death", 2);
            SpellNameEffectMap.Add("Skeletal Body", 10);
            SpellNameEffectMap.Add("Eagle Eyes", 10);
            SpellNameEffectMap.Add("Poison Touch", 7);
            SpellNameEffectMap.Add("Resist Poison", 514);
            SpellNameEffectMap.Add("Barkskin", 10);
            SpellNameEffectMap.Add("Personal Luck", 10);
            SpellNameEffectMap.Add("Combustion", 11);
            SpellNameEffectMap.Add("Resist Cold", 504);
            SpellNameEffectMap.Add("Phantasmal Warrior", 1);
            SpellNameEffectMap.Add("Mirror Image", 23);
            SpellNameEffectMap.Add("Resist Fire", 504);
            SpellNameEffectMap.Add("Quicken Self", 10);
            SpellNameEffectMap.Add("Ice Shield", 10);
            SpellNameEffectMap.Add("Resist Lightning", 504);
            SpellNameEffectMap.Add("Alchemical Transmutation", 117);
            SpellNameEffectMap.Add("Stoneskin", 10);
            SpellNameEffectMap.Add("Armor of Achilles", 138);
            SpellNameEffectMap.Add("Earth Meld", 11);
            SpellNameEffectMap.Add("Cheat Fate", 23);
            SpellNameEffectMap.Add("Weakness", 67);
            SpellNameEffectMap.Add("Enlarge", 10);
            SpellNameEffectMap.Add("Immolation", 2);
            SpellNameEffectMap.Add("Inner Sun", 23);
            SpellNameEffectMap.Add("Mistform", 23);
            SpellNameEffectMap.Add("Ghost Wolves", 1);
            SpellNameEffectMap.Add("Numbness", 11);
            SpellNameEffectMap.Add("Ironskin", 10);
            SpellNameEffectMap.Add("Protection", 10);
            SpellNameEffectMap.Add("Mossbody", 23);
            SpellNameEffectMap.Add("Luck", 10);
            SpellNameEffectMap.Add("Wind Guide", 10);
            SpellNameEffectMap.Add("Liquid Body", 10);
            SpellNameEffectMap.Add("Quickness", 10);
            SpellNameEffectMap.Add("Slow", 11);
            SpellNameEffectMap.Add("Encase in Ice", 609);
            SpellNameEffectMap.Add("Wolven Winter", 42);
            SpellNameEffectMap.Add("Temper Flesh", 10);
            SpellNameEffectMap.Add("Destruction", 138);
            SpellNameEffectMap.Add("Curse of Stones", 11);
            SpellNameEffectMap.Add("Blight", 42);
            SpellNameEffectMap.Add("Arouse Hunger", 37);
            SpellNameEffectMap.Add("Elemental Fortitude", 10);
            SpellNameEffectMap.Add("Body Ethereal", 10);
            SpellNameEffectMap.Add("Stygian Skin", 23);
            SpellNameEffectMap.Add("Shrink", 11);
            SpellNameEffectMap.Add("Transmute Fire", 117);
            SpellNameEffectMap.Add("Cold Resistance", 10);
            SpellNameEffectMap.Add("Incinerate", 2);
            SpellNameEffectMap.Add("Solar Eclipse", 81);
            SpellNameEffectMap.Add("Phantasmal Army", 1);
            SpellNameEffectMap.Add("Bone Melter", 2);
            SpellNameEffectMap.Add("Lightning Resistance", 10);
            SpellNameEffectMap.Add("Maws of the Earth", 2);
            SpellNameEffectMap.Add("Iron Warriors", 10);
            SpellNameEffectMap.Add("Shatter", 96);
            SpellNameEffectMap.Add("Baleful Star", 42);
            SpellNameEffectMap.Add("Enfeeble", 67);
            SpellNameEffectMap.Add("Invulnerability", 23);
            SpellNameEffectMap.Add("Drain Life", 103);
            SpellNameEffectMap.Add("Wooden Warriors", 10);
            SpellNameEffectMap.Add("Mother Oak", 81);
            SpellNameEffectMap.Add("Blindness", 11);
            SpellNameEffectMap.Add("Hellscape", 42);
            SpellNameEffectMap.Add("Boil", 2);
            SpellNameEffectMap.Add("False Horror", 1);
            SpellNameEffectMap.Add("Frozen Heart", 2);
            SpellNameEffectMap.Add("Wave Warriors", 10);
            SpellNameEffectMap.Add("Manifest Vitriol", 1);
            SpellNameEffectMap.Add("Earth Gem Alchemy", 117);
            SpellNameEffectMap.Add("Iron Bane", 11);
            SpellNameEffectMap.Add("Petrify", 99);
            SpellNameEffectMap.Add("Iron Pigs", 1);
            SpellNameEffectMap.Add("Control", 28);
            SpellNameEffectMap.Add("Battle Fortune", 10);
            SpellNameEffectMap.Add("Skeletal Legion", 10);
            SpellNameEffectMap.Add("Soul Vortex", 23);
            SpellNameEffectMap.Add("Darkness", 81);
            SpellNameEffectMap.Add("Army of Giants", 10);
            SpellNameEffectMap.Add("Phoenix Pyre", 23);
            SpellNameEffectMap.Add("Phantasmal Attack", 38);
            SpellNameEffectMap.Add("Fog Warriors", 23);
            SpellNameEffectMap.Add("Liquify", 2);
            SpellNameEffectMap.Add("Prison of Sedna", 609);
            SpellNameEffectMap.Add("Sea of Ice", 81);
            SpellNameEffectMap.Add("Marble Warriors", 10);
            SpellNameEffectMap.Add("Iron Walls", 82);
            SpellNameEffectMap.Add("Doom", 11);
            SpellNameEffectMap.Add("Bone Grinding", 2);
            SpellNameEffectMap.Add("Curse of the Frog Prince", 54);
            SpellNameEffectMap.Add("Creeping Doom", 1);
            SpellNameEffectMap.Add("Transformation", 44);
            SpellNameEffectMap.Add("Mass Protection", 10);
            SpellNameEffectMap.Add("Conflagration", 11);
            SpellNameEffectMap.Add("Warriors of Muspelheim", 10);
            SpellNameEffectMap.Add("Fata Morgana", 81);
            SpellNameEffectMap.Add("Quickening", 10);
            SpellNameEffectMap.Add("Warriors of Niefelheim", 10);
            SpellNameEffectMap.Add("Wizard's Tower", 63);
            SpellNameEffectMap.Add("Crumble", 70);
            SpellNameEffectMap.Add("Ground Army", 10);
            SpellNameEffectMap.Add("Will of the Fates", 10);
            SpellNameEffectMap.Add("Time Stop", 133);
            SpellNameEffectMap.Add("Disintegrate", 2);
            SpellNameEffectMap.Add("Polymorph", 54);
            SpellNameEffectMap.Add("Army of Gold", 10);
            SpellNameEffectMap.Add("Army of Lead", 10);
            SpellNameEffectMap.Add("Arcane Domination", 28);
            SpellNameEffectMap.Add("Wish", 25);
            SpellNameEffectMap.Add("Utterdark", 81);
            SpellNameEffectMap.Add("Army of Rats", 11);
            SpellNameEffectMap.Add("Summon Cave Grubs", 1);
            SpellNameEffectMap.Add("Tangle Vines", 11);
            SpellNameEffectMap.Add("Summon Animals", 68);
            SpellNameEffectMap.Add("Summon Sea Dogs", 1);
            SpellNameEffectMap.Add("Summon Crocodiles", 1);
            SpellNameEffectMap.Add("Spirit Curse", 11);
            SpellNameEffectMap.Add("Black Servant", 21);
            SpellNameEffectMap.Add("Summon Fire Ants", 1);
            SpellNameEffectMap.Add("Summon Storm Power", 23);
            SpellNameEffectMap.Add("Summon Water Power", 23);
            SpellNameEffectMap.Add("Summon Ogres", 1);
            SpellNameEffectMap.Add("Summon Shades", 1);
            SpellNameEffectMap.Add("Summon Killer Mantis", 1);
            SpellNameEffectMap.Add("Summon Horned Serpents", 1);
            SpellNameEffectMap.Add("Phoenix Power", 23);
            SpellNameEffectMap.Add("Summon Lesser Fire Elemental", 1);
            SpellNameEffectMap.Add("Bind Scorpion Beast", 1);
            SpellNameEffectMap.Add("Summon Lesser Air Elemental", 1);
            SpellNameEffectMap.Add("Call of the Winds", 37);
            SpellNameEffectMap.Add("Summon Amphiptere", 1);
            SpellNameEffectMap.Add("Summon Lesser Water Elemental", 1);
            SpellNameEffectMap.Add("Call Kraken", 1);
            SpellNameEffectMap.Add("Summon Yetis", 1);
            SpellNameEffectMap.Add("Summon Cave Cows", 1);
            SpellNameEffectMap.Add("Summon Earthpower", 23);
            SpellNameEffectMap.Add("Summon Lesser Earth Elemental", 1);
            SpellNameEffectMap.Add("Summon Cave Crab", 1);
            SpellNameEffectMap.Add("Power of the Spheres", 23);
            SpellNameEffectMap.Add("Dark Knowledge", 48);
            SpellNameEffectMap.Add("Revive Wights", 1);
            SpellNameEffectMap.Add("Revive Bane", 21);
            SpellNameEffectMap.Add("Sloth of Bears", 1);
            SpellNameEffectMap.Add("Pride of Lions", 1);
            SpellNameEffectMap.Add("Ambush of Tigers", 1);
            SpellNameEffectMap.Add("Awaken Vine Men", 1);
            SpellNameEffectMap.Add("Awaken Algae Men", 1);
            SpellNameEffectMap.Add("Herd of Buffaloes", 1);
            SpellNameEffectMap.Add("Call of the Wild", 37);
            SpellNameEffectMap.Add("Summon Sea Lions", 1);
            SpellNameEffectMap.Add("Summon Bog Beasts", 1);
            SpellNameEffectMap.Add("Summon Fire Drake", 1);
            SpellNameEffectMap.Add("Summon Flame Jelly", 1);
            SpellNameEffectMap.Add("Summon Wyverns", 1);
            SpellNameEffectMap.Add("Summon Gryphons", 1);
            SpellNameEffectMap.Add("School of Sharks", 43);
            SpellNameEffectMap.Add("Voice of Apsu", 48);
            SpellNameEffectMap.Add("Summon Ice Drake", 1);
            SpellNameEffectMap.Add("Summon Sea Serpent", 1);
            SpellNameEffectMap.Add("Summon Cave Drake", 1);
            SpellNameEffectMap.Add("Light of the Northern Star", 81);
            SpellNameEffectMap.Add("Summon Shade Beasts", 1);
            SpellNameEffectMap.Add("Summon Lammashtas", 126);
            SpellNameEffectMap.Add("Maggots", 7);
            SpellNameEffectMap.Add("Awaken Vine Ogres", 1);
            SpellNameEffectMap.Add("Summon Leogryphs", 1);
            SpellNameEffectMap.Add("Summon Swamp Drake", 1);
            SpellNameEffectMap.Add("Summon Kithaironic Lion", 1);
            SpellNameEffectMap.Add("Strength of Gaia", 23);
            SpellNameEffectMap.Add("Will o' the Wisp", 43);
            SpellNameEffectMap.Add("Summon Fire Elemental", 1);
            SpellNameEffectMap.Add("Summon Summer Lions", 1);
            SpellNameEffectMap.Add("Summon Air Elemental", 1);
            SpellNameEffectMap.Add("Summon Spring Hawks", 1);
            SpellNameEffectMap.Add("Wind Ride", 49);
            SpellNameEffectMap.Add("Raven Feast", 135);
            SpellNameEffectMap.Add("Contact Draconians", 21);
            SpellNameEffectMap.Add("Voice of Tiamat", 48);
            SpellNameEffectMap.Add("Summon Water Elemental", 1);
            SpellNameEffectMap.Add("Contact Sea Trolls", 1);
            SpellNameEffectMap.Add("Summon Winter Wolves", 1);
            SpellNameEffectMap.Add("Contact Naiad", 21);
            SpellNameEffectMap.Add("Naiad Warriors", 1);
            SpellNameEffectMap.Add("Summon Earth Elemental", 1);
            SpellNameEffectMap.Add("Summon Fall Bears", 1);
            SpellNameEffectMap.Add("Contact Trolls", 1);
            SpellNameEffectMap.Add("Spirit Mastery", 1);
            SpellNameEffectMap.Add("Ghost Grip", 3);
            SpellNameEffectMap.Add("Corpse Candle", 43);
            SpellNameEffectMap.Add("Revive Bane Lord", 21);
            SpellNameEffectMap.Add("Acashic Record", 115);
            SpellNameEffectMap.Add("Howl", 81);
            SpellNameEffectMap.Add("Spirits of the Wood", 1);
            SpellNameEffectMap.Add("Contact Forest Trolls", 1);
            SpellNameEffectMap.Add("Awaken Sleeper", 21);
            SpellNameEffectMap.Add("Winged Monkeys", 98);
            SpellNameEffectMap.Add("Summon Manticore", 1);
            SpellNameEffectMap.Add("Vermin Feast", 85);
            SpellNameEffectMap.Add("Summon Fire Snakes", 1);
            SpellNameEffectMap.Add("Summon Flame Spirit", 21);
            SpellNameEffectMap.Add("Summon Great Eagles", 1);
            SpellNameEffectMap.Add("Summon Bishop Fish", 21);
            SpellNameEffectMap.Add("Shark Attack", 81);
            SpellNameEffectMap.Add("Sea King's Court", 21);
            SpellNameEffectMap.Add("Streams from Hades", 21);
            SpellNameEffectMap.Add("Contact Hill Giant", 1);
            SpellNameEffectMap.Add("Troll King's Court", 21);
            SpellNameEffectMap.Add("Acashic Knowledge", 48);
            SpellNameEffectMap.Add("Ether Gate", 21);
            SpellNameEffectMap.Add("Summon Spectre", 21);
            SpellNameEffectMap.Add("Summon Ghosts", 1);
            SpellNameEffectMap.Add("Forest Troll Tribe", 21);
            SpellNameEffectMap.Add("Contact Forest Giant", 1);
            SpellNameEffectMap.Add("Summon Sprites", 1);
            SpellNameEffectMap.Add("Contact Lamias", 1);
            SpellNameEffectMap.Add("Locust Swarms", 42);
            SpellNameEffectMap.Add("Contact Lamia Queen", 21);
            SpellNameEffectMap.Add("Living Fire", 1);
            SpellNameEffectMap.Add("Living Clouds", 1);
            SpellNameEffectMap.Add("Summon Asp Turtle", 1);
            SpellNameEffectMap.Add("Living Water", 1);
            SpellNameEffectMap.Add("Summon Catoblepas", 1);
            SpellNameEffectMap.Add("Living Earth", 1);
            SpellNameEffectMap.Add("Summon Mound Fiend", 21);
            SpellNameEffectMap.Add("Harvester of Sorrows", 21);
            SpellNameEffectMap.Add("Call Wraith Lord", 21);
            SpellNameEffectMap.Add("Animal Horde", 68);
            SpellNameEffectMap.Add("Awaken Ivy King", 21);
            SpellNameEffectMap.Add("Living Castle", 63);
            SpellNameEffectMap.Add("King of Elemental Fire", 89);
            SpellNameEffectMap.Add("Queen of Elemental Air", 89);
            SpellNameEffectMap.Add("Queen of Elemental Water", 89);
            SpellNameEffectMap.Add("Guardians of the Deep", 81);
            SpellNameEffectMap.Add("Earth Attack", 50);
            SpellNameEffectMap.Add("King of Elemental Earth", 89);
            SpellNameEffectMap.Add("Manifestation", 62);
            SpellNameEffectMap.Add("Well of Misery", 81);
            SpellNameEffectMap.Add("King of Banefires", 89);
            SpellNameEffectMap.Add("Call the Eater of the Dead", 93);
            SpellNameEffectMap.Add("Dragon Master", 23);
            SpellNameEffectMap.Add("Wild Growth", 11);
            SpellNameEffectMap.Add("Faerie Court", 21);
            SpellNameEffectMap.Add("The Kindly Ones", 81);
            SpellNameEffectMap.Add("Celestial Rainbow", 81);
            SpellNameEffectMap.Add("Call Ancient Presence", 1);
            SpellNameEffectMap.Add("Call Abomination", 1);
            SpellNameEffectMap.Add("Ghost Riders", 38);
            SpellNameEffectMap.Add("Legion of Wights", 1);
            SpellNameEffectMap.Add("Tartarian Gate", 76);
            SpellNameEffectMap.Add("Awaken Tarrasque", 1);
            SpellNameEffectMap.Add("Wild Hunt", 81);
            SpellNameEffectMap.Add("Enchanted Forests", 81);
            SpellNameEffectMap.Add("Corpse Man Construction", 1);
            SpellNameEffectMap.Add("Clockwork Soldiers", 1);
            SpellNameEffectMap.Add("Legions of Steel", 10);
            SpellNameEffectMap.Add("Construct Manikin", 1);
            SpellNameEffectMap.Add("Clockwork Horrors", 1);
            SpellNameEffectMap.Add("Crusher Construction", 1);
            SpellNameEffectMap.Add("Wooden Construction", 1);
            SpellNameEffectMap.Add("Construct Mandragora", 1);
            SpellNameEffectMap.Add("Weapons of Sharpness", 10);
            SpellNameEffectMap.Add("Forge of the Ancients", 81);
            SpellNameEffectMap.Add("Mechanical Men", 1);
            SpellNameEffectMap.Add("Golem Construction", 21);
            SpellNameEffectMap.Add("Siege Golem", 1);
            SpellNameEffectMap.Add("Iron Dragon", 1);
            SpellNameEffectMap.Add("Mechanical Militia", 81);
            SpellNameEffectMap.Add("Juggernaut Construction", 1);
            SpellNameEffectMap.Add("Poison Golem", 21);
            SpellNameEffectMap.Add("Protection from Fire", 509);
            SpellNameEffectMap.Add("Windrunner", 23);
            SpellNameEffectMap.Add("Protection from Lightning", 509);
            SpellNameEffectMap.Add("Protection from Cold", 509);
            SpellNameEffectMap.Add("Resist Magic", 10);
            SpellNameEffectMap.Add("Animate Skeleton", 1);
            SpellNameEffectMap.Add("Animate Dead", 1);
            SpellNameEffectMap.Add("Reanimation", 1);
            SpellNameEffectMap.Add("Poison Resistance", 524);
            SpellNameEffectMap.Add("Healing Touch", 13);
            SpellNameEffectMap.Add("Flight", 10);
            SpellNameEffectMap.Add("Water Shield", 23);
            SpellNameEffectMap.Add("Breath of Winter", 10);
            SpellNameEffectMap.Add("Flying Shield", 10);
            SpellNameEffectMap.Add("Revive King", 21);
            SpellNameEffectMap.Add("Gift of the Hare", 23);
            SpellNameEffectMap.Add("Personal Regeneration", 10);
            SpellNameEffectMap.Add("Fire Shield", 23);
            SpellNameEffectMap.Add("Gift of Flight", 10);
            SpellNameEffectMap.Add("Seeking Arrow", 40);
            SpellNameEffectMap.Add("Claymen", 1);
            SpellNameEffectMap.Add("Strength of Giants", 10);
            SpellNameEffectMap.Add("Astral Shield", 23);
            SpellNameEffectMap.Add("Raise Skeletons", 1);
            SpellNameEffectMap.Add("Create Revenant", 21);
            SpellNameEffectMap.Add("Regeneration", 10);
            SpellNameEffectMap.Add("Heal", 13);
            SpellNameEffectMap.Add("Flaming Arrows", 10);
            SpellNameEffectMap.Add("Terracotta Army", 1);
            SpellNameEffectMap.Add("Cloud Trapeze", 95);
            SpellNameEffectMap.Add("Vile Water", 1);
            SpellNameEffectMap.Add("Astral Healing", 13);
            SpellNameEffectMap.Add("Antimagic", 10);
            SpellNameEffectMap.Add("Raise Dead", 1);
            SpellNameEffectMap.Add("Twiceborn", 23);
            SpellNameEffectMap.Add("Behemoth", 1);
            SpellNameEffectMap.Add("Haste", 23);
            SpellNameEffectMap.Add("Poison Ward", 10);
            SpellNameEffectMap.Add("Flame Ward", 10);
            SpellNameEffectMap.Add("Thunder Ward", 10);
            SpellNameEffectMap.Add("Watcher", 1);
            SpellNameEffectMap.Add("Trade Wind", 82);
            SpellNameEffectMap.Add("Winter Ward", 10);
            SpellNameEffectMap.Add("Friendly Currents", 81);
            SpellNameEffectMap.Add("Quagmire", 81);
            SpellNameEffectMap.Add("Enliven Gargoyles", 1);
            SpellNameEffectMap.Add("Ritual of Returning", 23);
            SpellNameEffectMap.Add("Dispel", 30);
            SpellNameEffectMap.Add("The Eyes of God", 81);
            SpellNameEffectMap.Add("Pale Riders", 1);
            SpellNameEffectMap.Add("Horde of Skeletons", 1);
            SpellNameEffectMap.Add("Faery Trod", 79);
            SpellNameEffectMap.Add("Gift of Health", 81);
            SpellNameEffectMap.Add("Foul Vapors", 81);
            SpellNameEffectMap.Add("Eternal Pyre", 81);
            SpellNameEffectMap.Add("Heat from Hell", 81);
            SpellNameEffectMap.Add("Vafur Flames", 82);
            SpellNameEffectMap.Add("Arrow Fend", 10);
            SpellNameEffectMap.Add("Dome of Solid Air", 84);
            SpellNameEffectMap.Add("Water Ward", 23);
            SpellNameEffectMap.Add("Grip of Winter", 81);
            SpellNameEffectMap.Add("Frost Dome", 82);
            SpellNameEffectMap.Add("Hidden in Snow", 100);
            SpellNameEffectMap.Add("Riches from Beneath", 81);
            SpellNameEffectMap.Add("Enliven Statues", 1);
            SpellNameEffectMap.Add("Hidden in Sand", 100);
            SpellNameEffectMap.Add("Hidden Underneath", 100);
            SpellNameEffectMap.Add("Opposition", 2);
            SpellNameEffectMap.Add("Dome of Arcane Warding", 82);
            SpellNameEffectMap.Add("Rigor Mortis", 81);
            SpellNameEffectMap.Add("Reanimate Archers", 1);
            SpellNameEffectMap.Add("Ziz", 1);
            SpellNameEffectMap.Add("Relief", 81);
            SpellNameEffectMap.Add("Dome of Flaming Death", 82);
            SpellNameEffectMap.Add("Mass Flight", 10);
            SpellNameEffectMap.Add("Ghost Ship Armada", 81);
            SpellNameEffectMap.Add("Lion Sentinels", 84);
            SpellNameEffectMap.Add("Earth Blood Deep Well", 81);
            SpellNameEffectMap.Add("Solar Brilliance", 81);
            SpellNameEffectMap.Add("Stellar Focus", 81);
            SpellNameEffectMap.Add("Carrion Reanimation", 37);
            SpellNameEffectMap.Add("Life after Death", 10);
            SpellNameEffectMap.Add("Ritual of Rebirth", 26);
            SpellNameEffectMap.Add("Leviathan", 1);
            SpellNameEffectMap.Add("Serpent's Blessing", 10);
            SpellNameEffectMap.Add("Awaken Treelord", 114);
            SpellNameEffectMap.Add("Forest Dome", 84);
            SpellNameEffectMap.Add("Fire Fend", 10);
            SpellNameEffectMap.Add("Frost Fend", 10);
            SpellNameEffectMap.Add("Thunder Fend", 10);
            SpellNameEffectMap.Add("Mists of Deception", 81);
            SpellNameEffectMap.Add("Wrath of the Sea", 81);
            SpellNameEffectMap.Add("Lichcraft", 21);
            SpellNameEffectMap.Add("Unraveling", 11);
            SpellNameEffectMap.Add("Mass Regeneration", 10);
            SpellNameEffectMap.Add("Haunted Forest", 81);
            SpellNameEffectMap.Add("Thetis' Blessing", 81);
            SpellNameEffectMap.Add("Demon Cleansing", 81);
            SpellNameEffectMap.Add("Arcane Nexus", 81);
            SpellNameEffectMap.Add("Fields of the Dead", 81);
            SpellNameEffectMap.Add("Army of the Dead", 37);
            SpellNameEffectMap.Add("Gaia's Blessing", 10);
            SpellNameEffectMap.Add("Gift of Nature's Bounty", 81);
            SpellNameEffectMap.Add("Desiccation", 500);
            SpellNameEffectMap.Add("Farstrike", 2);
            SpellNameEffectMap.Add("Blink", 20);
            SpellNameEffectMap.Add("Communion Master", 10);
            SpellNameEffectMap.Add("Communion Slave", 10);
            SpellNameEffectMap.Add("Horror Mark", 600);
            SpellNameEffectMap.Add("Dust to Dust", 2);
            SpellNameEffectMap.Add("Decay", 11);
            SpellNameEffectMap.Add("Frighten", 4);
            SpellNameEffectMap.Add("Fascination", 128);
            SpellNameEffectMap.Add("Seven Year Fever", 11);
            SpellNameEffectMap.Add("Curse", 11);
            SpellNameEffectMap.Add("Bonds of Fire", 11);
            SpellNameEffectMap.Add("Steal Breath", 3);
            SpellNameEffectMap.Add("Scrying Pool", 85);
            SpellNameEffectMap.Add("Mind Burn", 2);
            SpellNameEffectMap.Add("Sleep", 11);
            SpellNameEffectMap.Add("Rage", 11);
            SpellNameEffectMap.Add("Augury", 48);
            SpellNameEffectMap.Add("Sailors' Death", 2);
            SpellNameEffectMap.Add("Iron Will", 10);
            SpellNameEffectMap.Add("Astral Window", 85);
            SpellNameEffectMap.Add("Teleport", 19);
            SpellNameEffectMap.Add("Haruspex", 48);
            SpellNameEffectMap.Add("Panic", 4);
            SpellNameEffectMap.Add("Prison of Fire", 11);
            SpellNameEffectMap.Add("Auspex", 48);
            SpellNameEffectMap.Add("Curse of the Desert", 500);
            SpellNameEffectMap.Add("Gnome Lore", 48);
            SpellNameEffectMap.Add("Paralyze", 66);
            SpellNameEffectMap.Add("Vengeance of the Dead", 53);
            SpellNameEffectMap.Add("Terror", 4);
            SpellNameEffectMap.Add("Touch of Madness", 10);
            SpellNameEffectMap.Add("Rage of the Cornered Rat", 10);
            SpellNameEffectMap.Add("Wildness", 11);
            SpellNameEffectMap.Add("Cure Disease", 131);
            SpellNameEffectMap.Add("Raging Hearts", 42);
            SpellNameEffectMap.Add("Pyre of Catharsis", 132);
            SpellNameEffectMap.Add("Purifying Flames", 112);
            SpellNameEffectMap.Add("Confusion", 11);
            SpellNameEffectMap.Add("Soul Slay", 2);
            SpellNameEffectMap.Add("Telestic Animation", 21);
            SpellNameEffectMap.Add("Gateway", 80);
            SpellNameEffectMap.Add("Leeching Darkness", 2);
            SpellNameEffectMap.Add("Burden of Time", 81);
            SpellNameEffectMap.Add("Control the Dead", 28);
            SpellNameEffectMap.Add("Charm Animal", 29);
            SpellNameEffectMap.Add("The Ravenous Swarm", 81);
            SpellNameEffectMap.Add("Growing Fury", 81);
            SpellNameEffectMap.Add("Gift of Reason", 39);
            SpellNameEffectMap.Add("Melancholia", 86);
            SpellNameEffectMap.Add("Enslave Mind", 28);
            SpellNameEffectMap.Add("Imprint Souls", 92);
            SpellNameEffectMap.Add("Wither Bones", 2);
            SpellNameEffectMap.Add("Leprosy", 64);
            SpellNameEffectMap.Add("Foul Air", 81);
            SpellNameEffectMap.Add("Syllable of Sleep", 11);
            SpellNameEffectMap.Add("Beckoning", 94);
            SpellNameEffectMap.Add("Purgatory", 81);
            SpellNameEffectMap.Add("Dark Skies", 81);
            SpellNameEffectMap.Add("Vengeful Water", 81);
            SpellNameEffectMap.Add("Divine Name", 39);
            SpellNameEffectMap.Add("Vortex of Returning", 15);
            SpellNameEffectMap.Add("Plague", 11);
            SpellNameEffectMap.Add("Charm", 29);
            SpellNameEffectMap.Add("Hydrophobia", 11);
            SpellNameEffectMap.Add("Gale Gate", 81);
            SpellNameEffectMap.Add("Lure of the Deep", 81);
            SpellNameEffectMap.Add("Soul Drain", 81);
            SpellNameEffectMap.Add("Stygian Paths", 90);
            SpellNameEffectMap.Add("Black Death", 42);
            SpellNameEffectMap.Add("Call the Worm That Walks", 21);
            SpellNameEffectMap.Add("Beast Mastery", 28);
            SpellNameEffectMap.Add("Astral Travel", 77);
            SpellNameEffectMap.Add("Master Enslave", 28);
            SpellNameEffectMap.Add("Undead Mastery", 28);
            SpellNameEffectMap.Add("Blood Burst", 2);
            SpellNameEffectMap.Add("Blood Heal", 13);
            SpellNameEffectMap.Add("Sabbath Master", 10);
            SpellNameEffectMap.Add("Sabbath Slave", 10);
            SpellNameEffectMap.Add("Reinvigoration", 8);
            SpellNameEffectMap.Add("Bind Shadow Imp", 21);
            SpellNameEffectMap.Add("Summon Imps", 1);
            SpellNameEffectMap.Add("Bind Fiery Imps", 1);
            SpellNameEffectMap.Add("Blood Boil", 2);
            SpellNameEffectMap.Add("Bowl of Blood", 48);
            SpellNameEffectMap.Add("Agony", 2);
            SpellNameEffectMap.Add("Banish Demon", 2);
            SpellNameEffectMap.Add("Bind Spine Devil", 1);
            SpellNameEffectMap.Add("Bind Fiend", 1);
            SpellNameEffectMap.Add("Bind Bone Fiends", 1);
            SpellNameEffectMap.Add("Hell Power", 23);
            SpellNameEffectMap.Add("Leeching Touch", 103);
            SpellNameEffectMap.Add("Pain Transfer", 10);
            SpellNameEffectMap.Add("Infernal Circle", 82);
            SpellNameEffectMap.Add("Bind Devil", 1);
            SpellNameEffectMap.Add("Bind Frost Fiend", 1);
            SpellNameEffectMap.Add("Cross Breeding", 35);
            SpellNameEffectMap.Add("Blood Feast", 118);
            SpellNameEffectMap.Add("Bind Serpent Fiend", 1);
            SpellNameEffectMap.Add("Blood Lust", 10);
            SpellNameEffectMap.Add("Hellfire", 2);
            SpellNameEffectMap.Add("Bind Storm Demon", 1);
            SpellNameEffectMap.Add("Call Lesser Horror", 126);
            SpellNameEffectMap.Add("Rain of Toads", 42);
            SpellNameEffectMap.Add("Blood Fecundity", 82);
            SpellNameEffectMap.Add("Hellbind Heart", 29);
            SpellNameEffectMap.Add("Horde from Hell", 37);
            SpellNameEffectMap.Add("Bloodletting", 103);
            SpellNameEffectMap.Add("Bind Succubus", 21);
            SpellNameEffectMap.Add("Wrath of Pazuzu", 42);
            SpellNameEffectMap.Add("Bind Demon Knight", 1);
            SpellNameEffectMap.Add("Awaken Dark Vines", 1);
            SpellNameEffectMap.Add("Send Lesser Horror", 38);
            SpellNameEffectMap.Add("Summon Illearth", 1);
            SpellNameEffectMap.Add("Harm", 2);
            SpellNameEffectMap.Add("Rejuvenate", 101);
            SpellNameEffectMap.Add("Infernal Disease", 50);
            SpellNameEffectMap.Add("Ritual of Five Gates", 1);
            SpellNameEffectMap.Add("Bind Ice Devil", 89);
            SpellNameEffectMap.Add("Call Horror", 126);
            SpellNameEffectMap.Add("Leech", 103);
            SpellNameEffectMap.Add("Blood Rain", 81);
            SpellNameEffectMap.Add("Plague of Locusts", 37);
            SpellNameEffectMap.Add("Bind Arch Devil", 89);
            SpellNameEffectMap.Add("Father Illearth", 89);
            SpellNameEffectMap.Add("Send Dream Horror", 42);
            SpellNameEffectMap.Add("Dome of Corruption", 82);
            SpellNameEffectMap.Add("Astral Corruption", 81);
            SpellNameEffectMap.Add("Curse of Blood", 21);
            SpellNameEffectMap.Add("Blood Rite", 1);
            SpellNameEffectMap.Add("Purify Blood", 10);
            SpellNameEffectMap.Add("Rush of Strength", 10);
            SpellNameEffectMap.Add("Life for a Life", 2);
            SpellNameEffectMap.Add("Bind Heliophagus", 89);
            SpellNameEffectMap.Add("Three Red Seconds", 63);
            SpellNameEffectMap.Add("Blood Vortex", 81);
            SpellNameEffectMap.Add("Improved Cross Breeding", 35);
            SpellNameEffectMap.Add("Horror Seed", 102);
            SpellNameEffectMap.Add("Damage Reversal", 500);
            SpellNameEffectMap.Add("Forces of Darkness", 1);
            SpellNameEffectMap.Add("The Looming Hell", 81);
            SpellNameEffectMap.Add("Bind Demon Lord", 89);
            SpellNameEffectMap.Add("Infernal Prison", 108);
            SpellNameEffectMap.Add("Infernal Forces", 1);
            SpellNameEffectMap.Add("Infernal Tempest", 1);
            SpellNameEffectMap.Add("Claws of Kokytos", 108);
            SpellNameEffectMap.Add("Forces of Ice", 1);
            SpellNameEffectMap.Add("Infernal Crusade", 1);
            SpellNameEffectMap.Add("Send Horror", 38);
            SpellNameEffectMap.Add("Chorus Master", 10);
            SpellNameEffectMap.Add("Chorus Slave", 10);
            SpellNameEffectMap.Add("Sow Dragon Teeth", 1);
            SpellNameEffectMap.Add("Gigantomachia", 81);
            SpellNameEffectMap.Add("Bind Keres", 1);
            SpellNameEffectMap.Add("Curse Tablet", 136);
            SpellNameEffectMap.Add("Seith Curse", 136);
            SpellNameEffectMap.Add("Contact Hesperide", 21);
            SpellNameEffectMap.Add("Call Ladon", 137);
            SpellNameEffectMap.Add("Summon Hound of Twilight", 1);
            SpellNameEffectMap.Add("Dogs of Gold and Silver", 1);
            SpellNameEffectMap.Add("Dog of Gold", 1);
            SpellNameEffectMap.Add("Craft Keledone", 1);
            SpellNameEffectMap.Add("Forge Brass Bull", 1);
            SpellNameEffectMap.Add("Taurobolium", 511);
            SpellNameEffectMap.Add("Blessing of the God-slayer", 511);
            SpellNameEffectMap.Add("Awaken Jotun Draugar", 1);
            SpellNameEffectMap.Add("From Death Comes Life", 82);
            SpellNameEffectMap.Add("Rhapsody of the Dead", 2);
            SpellNameEffectMap.Add("Scare Spirits", 4);
            SpellNameEffectMap.Add("Rhapsody of Life", 13);
            SpellNameEffectMap.Add("Minor Reinvigoration", 8);
            SpellNameEffectMap.Add("Procession of the Underworld", 1);
            SpellNameEffectMap.Add("Summon Okami", 1);
            SpellNameEffectMap.Add("Contact Bakeneko", 21);
            SpellNameEffectMap.Add("Summon Omukade", 1);
            SpellNameEffectMap.Add("Contact Mujina", 21);
            SpellNameEffectMap.Add("Contact Tanuki", 21);
            SpellNameEffectMap.Add("Contact Jorogumo", 21);
            SpellNameEffectMap.Add("Summon Araburu-kami", 1);
            SpellNameEffectMap.Add("Katabasis", 23);
            SpellNameEffectMap.Add("Epopteia", 140);
            SpellNameEffectMap.Add("Contact Jinn", 21);
            SpellNameEffectMap.Add("Summon Jinn Warriors", 1);
            SpellNameEffectMap.Add("Contact Houri", 21);
            SpellNameEffectMap.Add("Summon Hinn", 1);
            SpellNameEffectMap.Add("Summon Ifrit", 21);
            SpellNameEffectMap.Add("Summon Shaytan", 21);
            SpellNameEffectMap.Add("Summon Marid", 21);
            SpellNameEffectMap.Add("Contact Marid", 21);
            SpellNameEffectMap.Add("Smokeless Flame", 2);
            SpellNameEffectMap.Add("Large Area Combustion", 11);
            SpellNameEffectMap.Add("Scorching Wind", 500);
            SpellNameEffectMap.Add("Call Cyclops Tribe", 1);
            SpellNameEffectMap.Add("Call the Birds of Splendour", 141);
            SpellNameEffectMap.Add("Awaken Jinn Block", 21);
            SpellNameEffectMap.Add("Winter's Call", 21);
            SpellNameEffectMap.Add("Summon Rimvaettir", 1);
            SpellNameEffectMap.Add("Summon Dwarf of the Four Directions", 89);
            SpellNameEffectMap.Add("Feast for Ghuls", 1);
            SpellNameEffectMap.Add("Summon Ghulah", 21);
            SpellNameEffectMap.Add("Summon Binn", 1);
            SpellNameEffectMap.Add("Summon Si'lat", 21);
        }

        public static bool ContainsSpell(int id)
        {
            return SpellIDEffectMap.ContainsKey(id);
        }

        public static bool ContainsSpell(string id)
        {
            return SpellNameEffectMap.ContainsKey(id);
        }

        public static bool TryGetEffect(int ID, out int effect)
        {
            return SpellIDEffectMap.TryGetValue(ID, out effect);
        }

        public static bool TryGetEffect(string ID, out int effect)
        {
            return SpellNameEffectMap.TryGetValue(ID, out effect);
        }

        public static bool IsSummonEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _summonEffects.Contains(effect);
        }

        public static bool IsEnchantEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _enchantEffects.Contains(effect);
        }

        public static bool IsEventEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _eventEffects.Contains(effect);
        }

        public static bool IsSummonSpell(int spellID)
        {
            return TryGetEffect(spellID, out int effect) && IsSummonEffect(effect);
        }

        public static bool IsSummonSpell(string spellID)
        {
            return TryGetEffect(spellID, out int effect) && IsSummonEffect(effect);
        }

        public static bool IsEnchantSpell(int spellID)
        {
            return TryGetEffect(spellID, out int effect) && IsEnchantEffect(effect);
        }

        public static bool IsEnchantSpell(string spellID)
        {
            return TryGetEffect(spellID, out int effect) && IsEnchantEffect(effect);
        }

        public static bool IsEventEffectSpell(int spellID)
        {
            return TryGetEffect(spellID, out int effect) && IsEventEffect(effect);
        }

        public static bool IsEventEffectSpell(string spellID)
        {
            return TryGetEffect(spellID, out int effect) && IsEventEffect(effect);
        }
    }
}
