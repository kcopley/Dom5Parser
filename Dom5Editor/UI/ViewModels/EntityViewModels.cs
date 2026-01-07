using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;
using Dom5Editor.UI;
using Paloma;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Represents an equipment item (weapon or armor) for display in the UI.
    /// </summary>
    public class EquipmentItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayText => string.IsNullOrEmpty(Name) ? $"#{ID}" : $"{Name} (#{ID})";
        public bool IsModified { get; set; }
        public bool IsSessionEdit { get; set; }
        public bool IsInherited { get; set; }
        public bool CanRemove => !IsInherited;
        public Command SourceCommand { get; set; }
        /// <summary>
        /// Entity type for navigation ("weapon" or "armor").
        /// </summary>
        public string EntityType { get; set; }
    }

    /// <summary>
    /// Represents an available equipment item for selection.
    /// </summary>
    public class AvailableEquipmentItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayText => string.IsNullOrEmpty(Name) ? $"#{ID}" : $"{Name} (#{ID})";
        public string Source { get; set; } // "Vanilla" or "Mod"
    }

    /// <summary>
    /// Represents an item slot type option for ComboBox binding.
    /// </summary>
    public class SlotTypeOption
    {
        public int Value { get; }
        public string DisplayName { get; }

        public SlotTypeOption(int value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// ViewModel for Monster entities.
    /// </summary>
    public class MonsterViewModel : EntityViewModel
    {
        public MonsterViewModel(Monster entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Monster Monster => (Monster)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from monster_badges.json.
        /// </summary>
        protected override string EntityTypeName => "monster";

        // ========================================
        // Basic Info
        // ========================================

        public string FixedName
        {
            get => GetStringProperty(Command.FIXEDNAME);
            set => SetStringProperty(Command.FIXEDNAME, value);
        }
        public bool IsFixedNameModified => IsStringPropertyModifiedFromVanilla(Command.FIXEDNAME);
        public bool IsFixedNameSessionEdit => IsPropertyEditedInSession(Command.FIXEDNAME);

        public string Description
        {
            get => GetStringProperty(Command.DESCR);
            set => SetStringProperty(Command.DESCR, value);
        }
        public bool IsDescriptionModified => IsStringPropertyModifiedFromVanilla(Command.DESCR);
        public bool IsDescriptionSessionEdit => IsPropertyEditedInSession(Command.DESCR);

        public string Sprite1
        {
            get => GetStringProperty(Command.SPR1);
            set => SetStringProperty(Command.SPR1, value);
        }
        public bool IsSprite1Modified => IsStringPropertyModifiedFromVanilla(Command.SPR1);
        public bool IsSprite1SessionEdit => IsPropertyEditedInSession(Command.SPR1);

        public string Sprite2
        {
            get => GetStringProperty(Command.SPR2);
            set => SetStringProperty(Command.SPR2, value);
        }
        public bool IsSprite2Modified => IsStringPropertyModifiedFromVanilla(Command.SPR2);
        public bool IsSprite2SessionEdit => IsPropertyEditedInSession(Command.SPR2);

        // ========================================
        // Copy Commands (fundamental for inheritance)
        // ========================================

        /// <summary>
        /// Display text for the copystats reference (monster name or ID).
        /// </summary>
        public string CopyStatsDisplay
        {
            get
            {
                var result = _entity.TryGet<CopyStatsRef>(Command.COPYSTATS, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    // Show the resolved monster name if available, otherwise the raw value
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    // Fall back to raw value if not resolved
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        /// <summary>
        /// Whether the entity has a copystats reference.
        /// </summary>
        public bool HasCopyStats
        {
            get
            {
                var result = _entity.TryGet<CopyStatsRef>(Command.COPYSTATS, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        /// <summary>
        /// Display text for the copyspr reference (monster name or ID).
        /// </summary>
        public string CopySprDisplay
        {
            get
            {
                var result = _entity.TryGet<MonsterRef>(Command.COPYSPR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        /// <summary>
        /// Whether the entity has a copyspr reference.
        /// </summary>
        public bool HasCopySpr
        {
            get
            {
                var result = _entity.TryGet<MonsterRef>(Command.COPYSPR, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        /// <summary>
        /// Whether to show the copy from section (has any copy command).
        /// </summary>
        public bool HasCopyCommands => HasCopyStats || HasCopySpr;

        /// <summary>
        /// Gets the copystats reference ID for navigation.
        /// </summary>
        public int CopyStatsId
        {
            get
            {
                var result = _entity.TryGet<CopyStatsRef>(Command.COPYSTATS, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.ID;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets the copyspr reference ID for navigation.
        /// </summary>
        public int CopySprId
        {
            get
            {
                var result = _entity.TryGet<MonsterRef>(Command.COPYSPR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.ID;
                }
                return 0;
            }
        }

        // ========================================
        // Sprite Images
        // ========================================

        /// <summary>
        /// Loads and returns the sprite 1 image (TGA file).
        /// </summary>
        public BitmapSource SpriteImage => LoadSpriteImage(Command.SPR1);

        /// <summary>
        /// Loads and returns the sprite 2 image (TGA file).
        /// </summary>
        public BitmapSource Sprite2Image => LoadSpriteImage(Command.SPR2);

        private BitmapSource LoadSpriteImage(Command spriteCommand)
        {
            // Try to get sprite path from entity
            var exists = _entity.TryGet<FilePathProperty>(spriteCommand, out var property);

            // Fall back to vanilla for VanillaModified entities
            if (exists != ReturnType.TRUE && exists != ReturnType.COPIED && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    exists = vanillaEntity.TryGet<FilePathProperty>(spriteCommand, out property);
                }
            }

            if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
            {
                try
                {
                    var spritePath = property.Value;
                    if (string.IsNullOrEmpty(spritePath))
                        return null;

                    // Adjust path separators
                    var spriteAdjusted = spritePath.Trim('.').Trim('/').Replace("/", "\\");

                    // Get mod directory
                    var modPath = _entity.ParentMod?.FullFilePath;
                    if (string.IsNullOrEmpty(modPath))
                        return null;

                    var dir = Path.GetDirectoryName(modPath);
                    var filePath = Path.Combine(dir, spriteAdjusted);

                    if (!File.Exists(filePath))
                        return null;

                    var targa = TargaImage.LoadTargaImage(filePath);
                    return targa.ConvertToImage();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        // ========================================
        // Stats
        // ========================================

        public int? Hp
        {
            get => GetIntProperty(Command.HP);
            set => SetIntProperty(Command.HP, value);
        }
        public bool IsHpModified => IsIntPropertyModifiedFromVanilla(Command.HP);
        public bool IsHpSessionEdit => IsPropertyEditedInSession(Command.HP);
        public bool IsHpInherited => IsIntPropertyInherited(Command.HP);

        public int? Size
        {
            get => GetIntProperty(Command.SIZE);
            set => SetIntProperty(Command.SIZE, value);
        }
        public bool IsSizeModified => IsIntPropertyModifiedFromVanilla(Command.SIZE);
        public bool IsSizeSessionEdit => IsPropertyEditedInSession(Command.SIZE);
        public bool IsSizeInherited => IsIntPropertyInherited(Command.SIZE);

        public int? Strength
        {
            get => GetIntProperty(Command.STR);
            set => SetIntProperty(Command.STR, value);
        }
        public bool IsStrengthModified => IsIntPropertyModifiedFromVanilla(Command.STR);
        public bool IsStrengthSessionEdit => IsPropertyEditedInSession(Command.STR);
        public bool IsStrengthInherited => IsIntPropertyInherited(Command.STR);

        public int? Protection
        {
            get => GetIntProperty(Command.PROT);
            set => SetIntProperty(Command.PROT, value);
        }
        public bool IsProtectionModified => IsIntPropertyModifiedFromVanilla(Command.PROT);
        public bool IsProtectionSessionEdit => IsPropertyEditedInSession(Command.PROT);
        public bool IsProtectionInherited => IsIntPropertyInherited(Command.PROT);

        public int? Attack
        {
            get => GetIntProperty(Command.ATT);
            set => SetIntProperty(Command.ATT, value);
        }
        public bool IsAttackModified => IsIntPropertyModifiedFromVanilla(Command.ATT);
        public bool IsAttackSessionEdit => IsPropertyEditedInSession(Command.ATT);
        public bool IsAttackInherited => IsIntPropertyInherited(Command.ATT);

        public int? Defense
        {
            get => GetIntProperty(Command.DEF);
            set => SetIntProperty(Command.DEF, value);
        }
        public bool IsDefenseModified => IsIntPropertyModifiedFromVanilla(Command.DEF);
        public bool IsDefenseSessionEdit => IsPropertyEditedInSession(Command.DEF);
        public bool IsDefenseInherited => IsIntPropertyInherited(Command.DEF);

        public int? Precision
        {
            get => GetIntProperty(Command.PREC);
            set => SetIntProperty(Command.PREC, value);
        }
        public bool IsPrecisionModified => IsIntPropertyModifiedFromVanilla(Command.PREC);
        public bool IsPrecisionSessionEdit => IsPropertyEditedInSession(Command.PREC);
        public bool IsPrecisionInherited => IsIntPropertyInherited(Command.PREC);

        public int? Encumbrance
        {
            get => GetIntProperty(Command.ENC);
            set => SetIntProperty(Command.ENC, value);
        }
        public bool IsEncumbranceModified => IsIntPropertyModifiedFromVanilla(Command.ENC);
        public bool IsEncumbranceSessionEdit => IsPropertyEditedInSession(Command.ENC);
        public bool IsEncumbranceInherited => IsIntPropertyInherited(Command.ENC);

        public int? MagicResistance
        {
            get => GetIntProperty(Command.MR);
            set => SetIntProperty(Command.MR, value);
        }
        public bool IsMagicResistanceModified => IsIntPropertyModifiedFromVanilla(Command.MR);
        public bool IsMagicResistanceSessionEdit => IsPropertyEditedInSession(Command.MR);
        public bool IsMagicResistanceInherited => IsIntPropertyInherited(Command.MR);

        public int? Morale
        {
            get => GetIntProperty(Command.MOR);
            set => SetIntProperty(Command.MOR, value);
        }
        public bool IsMoraleModified => IsIntPropertyModifiedFromVanilla(Command.MOR);
        public bool IsMoraleSessionEdit => IsPropertyEditedInSession(Command.MOR);
        public bool IsMoraleInherited => IsIntPropertyInherited(Command.MOR);

        public int? ActionPoints
        {
            get => GetIntProperty(Command.AP);
            set => SetIntProperty(Command.AP, value);
        }
        public bool IsActionPointsModified => IsIntPropertyModifiedFromVanilla(Command.AP);
        public bool IsActionPointsSessionEdit => IsPropertyEditedInSession(Command.AP);
        public bool IsActionPointsInherited => IsIntPropertyInherited(Command.AP);

        public int? MapMove
        {
            get => GetIntProperty(Command.MAPMOVE);
            set => SetIntProperty(Command.MAPMOVE, value);
        }
        public bool IsMapMoveModified => IsIntPropertyModifiedFromVanilla(Command.MAPMOVE);
        public bool IsMapMoveSessionEdit => IsPropertyEditedInSession(Command.MAPMOVE);
        public bool IsMapMoveInherited => IsIntPropertyInherited(Command.MAPMOVE);

        public int? Eyes
        {
            get => GetIntProperty(Command.EYES);
            set => SetIntProperty(Command.EYES, value);
        }
        public bool IsEyesModified => IsIntPropertyModifiedFromVanilla(Command.EYES);
        public bool IsEyesSessionEdit => IsPropertyEditedInSession(Command.EYES);
        public bool IsEyesInherited => IsIntPropertyInherited(Command.EYES);

        // ========================================
        // Recruitment
        // ========================================

        public int? GoldCost
        {
            get => GetIntProperty(Command.GCOST);
            set => SetIntProperty(Command.GCOST, value);
        }
        public bool IsGoldCostModified => IsIntPropertyModifiedFromVanilla(Command.GCOST);
        public bool IsGoldCostSessionEdit => IsPropertyEditedInSession(Command.GCOST);
        public bool IsGoldCostInherited => IsIntPropertyInherited(Command.GCOST);

        public int? ResourceCost
        {
            get => GetIntProperty(Command.RCOST);
            set => SetIntProperty(Command.RCOST, value);
        }
        public bool IsResourceCostModified => IsIntPropertyModifiedFromVanilla(Command.RCOST);
        public bool IsResourceCostSessionEdit => IsPropertyEditedInSession(Command.RCOST);
        public bool IsResourceCostInherited => IsIntPropertyInherited(Command.RCOST);

        public int? ResourceSize
        {
            get => GetIntProperty(Command.RESSIZE);
            set => SetIntProperty(Command.RESSIZE, value);
        }
        public bool IsResourceSizeModified => IsIntPropertyModifiedFromVanilla(Command.RESSIZE);
        public bool IsResourceSizeSessionEdit => IsPropertyEditedInSession(Command.RESSIZE);
        public bool IsResourceSizeInherited => IsIntPropertyInherited(Command.RESSIZE);

        public bool RequiresLab
        {
            get => GetCommandProperty(Command.REQLAB);
            set => SetCommandProperty(Command.REQLAB, value);
        }
        public bool IsRequiresLabModified => IsCommandPropertyModifiedFromVanilla(Command.REQLAB);
        public bool IsRequiresLabSessionEdit => IsPropertyEditedInSession(Command.REQLAB);
        public bool IsRequiresLabInherited => IsCommandPropertyInherited(Command.REQLAB);

        public bool RequiresTemple
        {
            get => GetCommandProperty(Command.REQTEMPLE);
            set => SetCommandProperty(Command.REQTEMPLE, value);
        }
        public bool IsRequiresTempleModified => IsCommandPropertyModifiedFromVanilla(Command.REQTEMPLE);
        public bool IsRequiresTempleSessionEdit => IsPropertyEditedInSession(Command.REQTEMPLE);
        public bool IsRequiresTempleInherited => IsCommandPropertyInherited(Command.REQTEMPLE);

        // ========================================
        // Type
        // ========================================

        public bool IsHumanoid
        {
            get => GetCommandProperty(Command.HUMANOID);
            set => SetCommandProperty(Command.HUMANOID, value);
        }
        public bool IsHumanoidModified => IsCommandPropertyModifiedFromVanilla(Command.HUMANOID);
        public bool IsHumanoidSessionEdit => IsPropertyEditedInSession(Command.HUMANOID);
        public bool IsHumanoidInherited => IsCommandPropertyInherited(Command.HUMANOID);

        public bool IsMounted
        {
            get => GetCommandProperty(Command.MOUNTED);
            set => SetCommandProperty(Command.MOUNTED, value);
        }
        public bool IsMountedModified => IsCommandPropertyModifiedFromVanilla(Command.MOUNTED);
        public bool IsMountedSessionEdit => IsPropertyEditedInSession(Command.MOUNTED);
        public bool IsMountedInherited => IsCommandPropertyInherited(Command.MOUNTED);

        public bool IsUndead
        {
            get => GetCommandProperty(Command.UNDEAD);
            set => SetCommandProperty(Command.UNDEAD, value);
        }
        public bool IsUndeadModified => IsCommandPropertyModifiedFromVanilla(Command.UNDEAD);
        public bool IsUndeadSessionEdit => IsPropertyEditedInSession(Command.UNDEAD);
        public bool IsUndeadInherited => IsCommandPropertyInherited(Command.UNDEAD);

        public bool IsDemon
        {
            get => GetCommandProperty(Command.DEMON);
            set => SetCommandProperty(Command.DEMON, value);
        }
        public bool IsDemonModified => IsCommandPropertyModifiedFromVanilla(Command.DEMON);
        public bool IsDemonSessionEdit => IsPropertyEditedInSession(Command.DEMON);
        public bool IsDemonInherited => IsCommandPropertyInherited(Command.DEMON);

        public bool IsMagicBeing
        {
            get => GetCommandProperty(Command.MAGICBEING);
            set => SetCommandProperty(Command.MAGICBEING, value);
        }
        public bool IsMagicBeingModified => IsCommandPropertyModifiedFromVanilla(Command.MAGICBEING);
        public bool IsMagicBeingSessionEdit => IsPropertyEditedInSession(Command.MAGICBEING);
        public bool IsMagicBeingInherited => IsCommandPropertyInherited(Command.MAGICBEING);

        public bool IsHoly
        {
            get => GetCommandProperty(Command.HOLY);
            set => SetCommandProperty(Command.HOLY, value);
        }
        public bool IsHolyModified => IsCommandPropertyModifiedFromVanilla(Command.HOLY);
        public bool IsHolySessionEdit => IsPropertyEditedInSession(Command.HOLY);
        public bool IsHolyInherited => IsCommandPropertyInherited(Command.HOLY);

        public bool IsAnimal
        {
            get => GetCommandProperty(Command.ANIMAL);
            set => SetCommandProperty(Command.ANIMAL, value);
        }
        public bool IsAnimalModified => IsCommandPropertyModifiedFromVanilla(Command.ANIMAL);
        public bool IsAnimalSessionEdit => IsPropertyEditedInSession(Command.ANIMAL);
        public bool IsAnimalInherited => IsCommandPropertyInherited(Command.ANIMAL);

        public bool IsUnique
        {
            get => GetCommandProperty(Command.UNIQUE);
            set => SetCommandProperty(Command.UNIQUE, value);
        }
        public bool IsUniqueModified => IsCommandPropertyModifiedFromVanilla(Command.UNIQUE);
        public bool IsUniqueSessionEdit => IsPropertyEditedInSession(Command.UNIQUE);
        public bool IsUniqueInherited => IsCommandPropertyInherited(Command.UNIQUE);

        public bool IsInanimate
        {
            get => GetCommandProperty(Command.INANIMATE);
            set => SetCommandProperty(Command.INANIMATE, value);
        }
        public bool IsInanimateModified => IsCommandPropertyModifiedFromVanilla(Command.INANIMATE);
        public bool IsInanimateSessionEdit => IsPropertyEditedInSession(Command.INANIMATE);
        public bool IsInanimateInherited => IsCommandPropertyInherited(Command.INANIMATE);

        public bool IsMindless
        {
            get => GetCommandProperty(Command.MINDLESS);
            set => SetCommandProperty(Command.MINDLESS, value);
        }
        public bool IsMindlessModified => IsCommandPropertyModifiedFromVanilla(Command.MINDLESS);
        public bool IsMindlessSessionEdit => IsPropertyEditedInSession(Command.MINDLESS);
        public bool IsMindlessInherited => IsCommandPropertyInherited(Command.MINDLESS);

        // ========================================
        // Movement
        // ========================================

        public bool IsFlying
        {
            get => GetCommandProperty(Command.FLYING);
            set => SetCommandProperty(Command.FLYING, value);
        }
        public bool IsFlyingModified => IsCommandPropertyModifiedFromVanilla(Command.FLYING);
        public bool IsFlyingSessionEdit => IsPropertyEditedInSession(Command.FLYING);
        public bool IsFlyingInherited => IsCommandPropertyInherited(Command.FLYING);

        public bool IsAquatic
        {
            get => GetCommandProperty(Command.AQUATIC);
            set => SetCommandProperty(Command.AQUATIC, value);
        }
        public bool IsAquaticModified => IsCommandPropertyModifiedFromVanilla(Command.AQUATIC);
        public bool IsAquaticSessionEdit => IsPropertyEditedInSession(Command.AQUATIC);
        public bool IsAquaticInherited => IsCommandPropertyInherited(Command.AQUATIC);

        public bool IsAmphibian
        {
            get => GetCommandProperty(Command.AMPHIBIAN);
            set => SetCommandProperty(Command.AMPHIBIAN, value);
        }
        public bool IsAmphibianModified => IsCommandPropertyModifiedFromVanilla(Command.AMPHIBIAN);
        public bool IsAmphibianSessionEdit => IsPropertyEditedInSession(Command.AMPHIBIAN);
        public bool IsAmphibianInherited => IsCommandPropertyInherited(Command.AMPHIBIAN);

        public bool IsFloating
        {
            get => GetCommandProperty(Command.FLOAT);
            set => SetCommandProperty(Command.FLOAT, value);
        }
        public bool IsFloatingModified => IsCommandPropertyModifiedFromVanilla(Command.FLOAT);
        public bool IsFloatingSessionEdit => IsPropertyEditedInSession(Command.FLOAT);
        public bool IsFloatingInherited => IsCommandPropertyInherited(Command.FLOAT);

        public bool CanTeleport
        {
            get => GetCommandProperty(Command.TELEPORT);
            set => SetCommandProperty(Command.TELEPORT, value);
        }
        public bool IsCanTeleportModified => IsCommandPropertyModifiedFromVanilla(Command.TELEPORT);
        public bool IsCanTeleportSessionEdit => IsPropertyEditedInSession(Command.TELEPORT);
        public bool IsCanTeleportInherited => IsCommandPropertyInherited(Command.TELEPORT);

        public int? Stealthy
        {
            get => GetIntProperty(Command.STEALTHY);
            set => SetIntProperty(Command.STEALTHY, value);
        }
        public bool IsStealthyModified => IsIntPropertyModifiedFromVanilla(Command.STEALTHY);
        public bool IsStealthySessionEdit => IsPropertyEditedInSession(Command.STEALTHY);
        public bool IsStealthyInherited => IsIntPropertyInherited(Command.STEALTHY);

        // ========================================
        // Resistances
        // ========================================

        public int? FireResistance
        {
            get => GetIntProperty(Command.FIRERES);
            set => SetIntProperty(Command.FIRERES, value);
        }
        public bool IsFireResistanceModified => IsIntPropertyModifiedFromVanilla(Command.FIRERES);
        public bool IsFireResistanceSessionEdit => IsPropertyEditedInSession(Command.FIRERES);
        public bool IsFireResistanceInherited => IsIntPropertyInherited(Command.FIRERES);

        public int? ColdResistance
        {
            get => GetIntProperty(Command.COLDRES);
            set => SetIntProperty(Command.COLDRES, value);
        }
        public bool IsColdResistanceModified => IsIntPropertyModifiedFromVanilla(Command.COLDRES);
        public bool IsColdResistanceSessionEdit => IsPropertyEditedInSession(Command.COLDRES);
        public bool IsColdResistanceInherited => IsIntPropertyInherited(Command.COLDRES);

        public int? ShockResistance
        {
            get => GetIntProperty(Command.SHOCKRES);
            set => SetIntProperty(Command.SHOCKRES, value);
        }
        public bool IsShockResistanceModified => IsIntPropertyModifiedFromVanilla(Command.SHOCKRES);
        public bool IsShockResistanceSessionEdit => IsPropertyEditedInSession(Command.SHOCKRES);
        public bool IsShockResistanceInherited => IsIntPropertyInherited(Command.SHOCKRES);

        public int? PoisonResistance
        {
            get => GetIntProperty(Command.POISONRES);
            set => SetIntProperty(Command.POISONRES, value);
        }
        public bool IsPoisonResistanceModified => IsIntPropertyModifiedFromVanilla(Command.POISONRES);
        public bool IsPoisonResistanceSessionEdit => IsPropertyEditedInSession(Command.POISONRES);
        public bool IsPoisonResistanceInherited => IsIntPropertyInherited(Command.POISONRES);

        public bool IsEthereal
        {
            get => GetCommandProperty(Command.ETHEREAL);
            set => SetCommandProperty(Command.ETHEREAL, value);
        }
        public bool IsEtherealModified => IsCommandPropertyModifiedFromVanilla(Command.ETHEREAL);
        public bool IsEtherealSessionEdit => IsPropertyEditedInSession(Command.ETHEREAL);
        public bool IsEtherealInherited => IsCommandPropertyInherited(Command.ETHEREAL);

        public int? Regeneration
        {
            get => GetIntProperty(Command.REGENERATION);
            set => SetIntProperty(Command.REGENERATION, value);
        }
        public bool IsRegenerationModified => IsIntPropertyModifiedFromVanilla(Command.REGENERATION);
        public bool IsRegenerationSessionEdit => IsPropertyEditedInSession(Command.REGENERATION);
        public bool IsRegenerationInherited => IsIntPropertyInherited(Command.REGENERATION);

        public int? Invulnerable
        {
            get => GetIntProperty(Command.INVULNERABLE);
            set => SetIntProperty(Command.INVULNERABLE, value);
        }
        public bool IsInvulnerableModified => IsIntPropertyModifiedFromVanilla(Command.INVULNERABLE);
        public bool IsInvulnerableSessionEdit => IsPropertyEditedInSession(Command.INVULNERABLE);
        public bool IsInvulnerableInherited => IsIntPropertyInherited(Command.INVULNERABLE);

        // ========================================
        // Combat Abilities
        // ========================================

        public int? Awe
        {
            get => GetIntProperty(Command.AWE);
            set => SetIntProperty(Command.AWE, value);
        }
        public bool IsAweModified => IsIntPropertyModifiedFromVanilla(Command.AWE);
        public bool IsAweSessionEdit => IsPropertyEditedInSession(Command.AWE);
        public bool IsAweInherited => IsIntPropertyInherited(Command.AWE);

        public int? Fear
        {
            get => GetIntProperty(Command.FEAR);
            set => SetIntProperty(Command.FEAR, value);
        }
        public bool IsFearModified => IsIntPropertyModifiedFromVanilla(Command.FEAR);
        public bool IsFearSessionEdit => IsPropertyEditedInSession(Command.FEAR);
        public bool IsFearInherited => IsIntPropertyInherited(Command.FEAR);

        public int? Berserk
        {
            get => GetIntProperty(Command.BERSERK);
            set => SetIntProperty(Command.BERSERK, value);
        }
        public bool IsBerserkModified => IsIntPropertyModifiedFromVanilla(Command.BERSERK);
        public bool IsBerserkSessionEdit => IsPropertyEditedInSession(Command.BERSERK);
        public bool IsBerserkInherited => IsIntPropertyInherited(Command.BERSERK);

        public int? Ambidextrous
        {
            get => GetIntProperty(Command.AMBIDEXTROUS);
            set => SetIntProperty(Command.AMBIDEXTROUS, value);
        }
        public bool IsAmbidextrousModified => IsIntPropertyModifiedFromVanilla(Command.AMBIDEXTROUS);
        public bool IsAmbidextrousSessionEdit => IsPropertyEditedInSession(Command.AMBIDEXTROUS);
        public bool IsAmbidextrousInherited => IsIntPropertyInherited(Command.AMBIDEXTROUS);

        public int? DarkVision
        {
            get => GetIntProperty(Command.DARKVISION);
            set => SetIntProperty(Command.DARKVISION, value);
        }
        public bool IsDarkVisionModified => IsIntPropertyModifiedFromVanilla(Command.DARKVISION);
        public bool IsDarkVisionSessionEdit => IsPropertyEditedInSession(Command.DARKVISION);
        public bool IsDarkVisionInherited => IsIntPropertyInherited(Command.DARKVISION);

        // ========================================
        // Aura Effects
        // ========================================

        public int? Heat
        {
            get => GetIntProperty(Command.HEAT);
            set => SetIntProperty(Command.HEAT, value);
        }
        public bool IsHeatModified => IsIntPropertyModifiedFromVanilla(Command.HEAT);
        public bool IsHeatSessionEdit => IsPropertyEditedInSession(Command.HEAT);
        public bool IsHeatInherited => IsIntPropertyInherited(Command.HEAT);

        public int? Cold
        {
            get => GetIntProperty(Command.COLD);
            set => SetIntProperty(Command.COLD, value);
        }
        public bool IsColdModified => IsIntPropertyModifiedFromVanilla(Command.COLD);
        public bool IsColdSessionEdit => IsPropertyEditedInSession(Command.COLD);
        public bool IsColdInherited => IsIntPropertyInherited(Command.COLD);

        public int? FireShield
        {
            get => GetIntProperty(Command.FIRESHIELD);
            set => SetIntProperty(Command.FIRESHIELD, value);
        }
        public bool IsFireShieldModified => IsIntPropertyModifiedFromVanilla(Command.FIRESHIELD);
        public bool IsFireShieldSessionEdit => IsPropertyEditedInSession(Command.FIRESHIELD);
        public bool IsFireShieldInherited => IsIntPropertyInherited(Command.FIRESHIELD);

        public int? AirShield
        {
            get => GetIntProperty(Command.AIRSHIELD);
            set => SetIntProperty(Command.AIRSHIELD, value);
        }
        public bool IsAirShieldModified => IsIntPropertyModifiedFromVanilla(Command.AIRSHIELD);
        public bool IsAirShieldSessionEdit => IsPropertyEditedInSession(Command.AIRSHIELD);
        public bool IsAirShieldInherited => IsIntPropertyInherited(Command.AIRSHIELD);

        public int? PoisonCloud
        {
            get => GetIntProperty(Command.POISONCLOUD);
            set => SetIntProperty(Command.POISONCLOUD, value);
        }
        public bool IsPoisonCloudModified => IsIntPropertyModifiedFromVanilla(Command.POISONCLOUD);
        public bool IsPoisonCloudSessionEdit => IsPropertyEditedInSession(Command.POISONCLOUD);
        public bool IsPoisonCloudInherited => IsIntPropertyInherited(Command.POISONCLOUD);

        public int? DiseaseCloud
        {
            get => GetIntProperty(Command.DISEASECLOUD);
            set => SetIntProperty(Command.DISEASECLOUD, value);
        }
        public bool IsDiseaseCloudModified => IsIntPropertyModifiedFromVanilla(Command.DISEASECLOUD);
        public bool IsDiseaseCloudSessionEdit => IsPropertyEditedInSession(Command.DISEASECLOUD);
        public bool IsDiseaseCloudInherited => IsIntPropertyInherited(Command.DISEASECLOUD);

        // ========================================
        // Age Properties
        // ========================================

        public int? StartAge
        {
            get => GetIntProperty(Command.STARTAGE);
            set => SetIntProperty(Command.STARTAGE, value);
        }
        public bool IsStartAgeModified => IsIntPropertyModifiedFromVanilla(Command.STARTAGE);
        public bool IsStartAgeSessionEdit => IsPropertyEditedInSession(Command.STARTAGE);
        public bool IsStartAgeInherited => IsIntPropertyInherited(Command.STARTAGE);

        public int? MaxAge
        {
            get => GetIntProperty(Command.MAXAGE);
            set => SetIntProperty(Command.MAXAGE, value);
        }
        public bool IsMaxAgeModified => IsIntPropertyModifiedFromVanilla(Command.MAXAGE);
        public bool IsMaxAgeSessionEdit => IsPropertyEditedInSession(Command.MAXAGE);
        public bool IsMaxAgeInherited => IsIntPropertyInherited(Command.MAXAGE);

        // ========================================
        // Province Effects
        // ========================================

        public int? PopKill
        {
            get => GetIntProperty(Command.POPKILL);
            set => SetIntProperty(Command.POPKILL, value);
        }
        public bool IsPopKillModified => IsIntPropertyModifiedFromVanilla(Command.POPKILL);
        public bool IsPopKillSessionEdit => IsPropertyEditedInSession(Command.POPKILL);
        public bool IsPopKillInherited => IsIntPropertyInherited(Command.POPKILL);

        public int? IncUnrest
        {
            get => GetIntProperty(Command.INCUNREST);
            set => SetIntProperty(Command.INCUNREST, value);
        }
        public bool IsIncUnrestModified => IsIntPropertyModifiedFromVanilla(Command.INCUNREST);
        public bool IsIncUnrestSessionEdit => IsPropertyEditedInSession(Command.INCUNREST);
        public bool IsIncUnrestInherited => IsIntPropertyInherited(Command.INCUNREST);

        public int? SpreadDom
        {
            get => GetIntProperty(Command.SPREADDOM);
            set => SetIntProperty(Command.SPREADDOM, value);
        }
        public bool IsSpreadDomModified => IsIntPropertyModifiedFromVanilla(Command.SPREADDOM);
        public bool IsSpreadDomSessionEdit => IsPropertyEditedInSession(Command.SPREADDOM);
        public bool IsSpreadDomInherited => IsIntPropertyInherited(Command.SPREADDOM);

        public int? PatrolBonus
        {
            get => GetIntProperty(Command.PATROLBONUS);
            set => SetIntProperty(Command.PATROLBONUS, value);
        }
        public bool IsPatrolBonusModified => IsIntPropertyModifiedFromVanilla(Command.PATROLBONUS);
        public bool IsPatrolBonusSessionEdit => IsPropertyEditedInSession(Command.PATROLBONUS);
        public bool IsPatrolBonusInherited => IsIntPropertyInherited(Command.PATROLBONUS);

        public int? SupplyBonus
        {
            get => GetIntProperty(Command.SUPPLYBONUS);
            set => SetIntProperty(Command.SUPPLYBONUS, value);
        }
        public bool IsSupplyBonusModified => IsIntPropertyModifiedFromVanilla(Command.SUPPLYBONUS);
        public bool IsSupplyBonusSessionEdit => IsPropertyEditedInSession(Command.SUPPLYBONUS);
        public bool IsSupplyBonusInherited => IsIntPropertyInherited(Command.SUPPLYBONUS);

        // ========================================
        // Stealth/Assassin
        // ========================================

        public bool IsSpy
        {
            get => GetCommandProperty(Command.SPY);
            set => SetCommandProperty(Command.SPY, value);
        }
        public bool IsSpyModified => IsCommandPropertyModifiedFromVanilla(Command.SPY);
        public bool IsSpySessionEdit => IsPropertyEditedInSession(Command.SPY);
        public bool IsSpyInherited => IsCommandPropertyInherited(Command.SPY);

        public bool IsAssassin
        {
            get => GetCommandProperty(Command.ASSASSIN);
            set => SetCommandProperty(Command.ASSASSIN, value);
        }
        public bool IsAssassinModified => IsCommandPropertyModifiedFromVanilla(Command.ASSASSIN);
        public bool IsAssassinSessionEdit => IsPropertyEditedInSession(Command.ASSASSIN);
        public bool IsAssassinInherited => IsCommandPropertyInherited(Command.ASSASSIN);

        public int? Seduce
        {
            get => GetIntProperty(Command.SEDUCE);
            set => SetIntProperty(Command.SEDUCE, value);
        }
        public bool IsSeduceModified => IsIntPropertyModifiedFromVanilla(Command.SEDUCE);
        public bool IsSeduceSessionEdit => IsPropertyEditedInSession(Command.SEDUCE);
        public bool IsSeduceInherited => IsIntPropertyInherited(Command.SEDUCE);

        // ===== BADGE-BASED COLLECTIONS (Compact UI) =====
        // Sections defined in monster_badges.json: types (read-only), general, combat, resistances
        // Uses base class BuildBadgesFromSection() method

        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _typeBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableTypeBadges;
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _generalBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableGeneralBadges;
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _combatBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableCombatBadges;
        private System.Collections.ObjectModel.ObservableCollection<PropertyItem> _resistanceBadges;
        private System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> _availableResistanceBadges;

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> TypeBadges
        {
            get { if (_typeBadges == null) RefreshTypeBadges(); return _typeBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableTypeBadges
        {
            get { if (_availableTypeBadges == null) RefreshTypeBadges(); return _availableTypeBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> GeneralBadges
        {
            get { if (_generalBadges == null) RefreshGeneralBadges(); return _generalBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableGeneralBadges
        {
            get { if (_availableGeneralBadges == null) RefreshGeneralBadges(); return _availableGeneralBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> CombatBadges
        {
            get { if (_combatBadges == null) RefreshCombatBadges(); return _combatBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableCombatBadges
        {
            get { if (_availableCombatBadges == null) RefreshCombatBadges(); return _availableCombatBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<PropertyItem> ResistanceBadges
        {
            get { if (_resistanceBadges == null) RefreshResistanceBadges(); return _resistanceBadges; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> AvailableResistanceBadges
        {
            get { if (_availableResistanceBadges == null) RefreshResistanceBadges(); return _availableResistanceBadges; }
        }

        // Commands for badge operations - using base class helpers
        private RelayCommand<PropertyItem> _removeGeneralBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addGeneralBadgeCommand;
        private RelayCommand<PropertyItem> _removeCombatBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCombatBadgeCommand;
        private RelayCommand<PropertyItem> _removeResistanceBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResistanceBadgeCommand;

        public RelayCommand<PropertyItem> RemoveGeneralBadgeCommand => _removeGeneralBadgeCommand ??= CreateRemoveBadgeCommand(RefreshGeneralBadges);
        public RelayCommand<AvailablePropertyItem> AddGeneralBadgeCommand => _addGeneralBadgeCommand ??= CreateAddBadgeCommand(RefreshGeneralBadges);
        public RelayCommand<PropertyItem> RemoveCombatBadgeCommand => _removeCombatBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCombatBadges);
        public RelayCommand<AvailablePropertyItem> AddCombatBadgeCommand => _addCombatBadgeCommand ??= CreateAddBadgeCommand(RefreshCombatBadges);
        public RelayCommand<PropertyItem> RemoveResistanceBadgeCommand => _removeResistanceBadgeCommand ??= CreateRemoveBadgeCommand(RefreshResistanceBadges);
        public RelayCommand<AvailablePropertyItem> AddResistanceBadgeCommand => _addResistanceBadgeCommand ??= CreateAddBadgeCommand(RefreshResistanceBadges);

        // Shared value changed handler for all badge sections (uses base class helper)
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshTypeBadges()
        {
            var (active, available) = BuildBadgesFromSection("types", null);
            _typeBadges = active;
            _availableTypeBadges = available;
            OnPropertyChanged(nameof(TypeBadges));
            OnPropertyChanged(nameof(AvailableTypeBadges));
        }

        private void RefreshGeneralBadges()
        {
            var (active, available) = BuildBadgesFromSection("general", BadgeValueChangedHandler);
            _generalBadges = active;
            _availableGeneralBadges = available;
            OnPropertyChanged(nameof(GeneralBadges));
            OnPropertyChanged(nameof(AvailableGeneralBadges));
        }

        private void RefreshCombatBadges()
        {
            var (active, available) = BuildBadgesFromSection("combat", BadgeValueChangedHandler);
            _combatBadges = active;
            _availableCombatBadges = available;
            OnPropertyChanged(nameof(CombatBadges));
            OnPropertyChanged(nameof(AvailableCombatBadges));
        }

        private void RefreshResistanceBadges()
        {
            var (active, available) = BuildBadgesFromSection("resistances", BadgeValueChangedHandler);
            _resistanceBadges = active;
            _availableResistanceBadges = available;
            OnPropertyChanged(nameof(ResistanceBadges));
            OnPropertyChanged(nameof(AvailableResistanceBadges));
        }

        // ===== Magic Paths =====
        private System.Collections.ObjectModel.ObservableCollection<UI.Controls.MagicPathItem> _magicPathsList;
        private System.Collections.ObjectModel.ObservableCollection<UI.Controls.AvailableMagicPath> _availableMagicPaths;

        public System.Collections.ObjectModel.ObservableCollection<UI.Controls.MagicPathItem> MagicPathsList
        {
            get
            {
                if (_magicPathsList == null) RefreshMagicPaths();
                return _magicPathsList;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<UI.Controls.AvailableMagicPath> AvailableMagicPaths
        {
            get { if (_availableMagicPaths == null) RefreshMagicPaths(); return _availableMagicPaths; }
        }

        private void RefreshMagicPaths()
        {
            var activePaths = new System.Collections.ObjectModel.ObservableCollection<UI.Controls.MagicPathItem>();
            var availablePaths = new System.Collections.ObjectModel.ObservableCollection<UI.Controls.AvailableMagicPath>();

            // Get magic skills by layering: vanilla -> mod -> session changes
            var monster = _entity as Monster;
            if (monster != null)
            {
                var usedPathIds = new HashSet<int>();

                // Dictionary to collect all magic skills: pathId -> (level, isFromMod, isSessionEdit)
                var allSkills = new Dictionary<int, (int Level, bool IsModified, bool IsSessionEdit)>();

                // First, get vanilla magic skills as base
                var vanillaMonster = GetVanillaEntity() as Monster;
                if (vanillaMonster != null)
                {
                    foreach (var skill in vanillaMonster.MagicSkills)
                    {
                        int pathId = (int)skill.Path;
                        allSkills[pathId] = (skill.Level, false, false);
                    }
                }

                // Then overlay with mod/current entity skills (if different from vanilla entity)
                if (monster != vanillaMonster)
                {
                    foreach (var skill in monster.MagicSkills)
                    {
                        int pathId = (int)skill.Path;
                        // Check if this is a session edit or mod change
                        bool isSessionEdit = IsPropertyEditedInSession(Command.MAGICSKILL);
                        bool isModified = !allSkills.ContainsKey(pathId) || allSkills[pathId].Level != skill.Level;
                        allSkills[pathId] = (skill.Level, isModified, isSessionEdit);
                    }
                }

                // Build UI items from collected skills
                foreach (var kvp in allSkills.OrderBy(k => k.Key))
                {
                    int pathId = kvp.Key;
                    var (level, isModified, isSessionEdit) = kvp.Value;
                    var pathInfo = UI.Controls.MagicPathDefinitions.GetPathInfo(pathId);
                    var item = new UI.Controls.MagicPathItem
                    {
                        PathId = pathId,
                        PathLetter = pathInfo.Letter,
                        PathName = pathInfo.Name,
                        PathColor = pathInfo.Color,
                        TextColor = pathInfo.TextColor,
                        BorderColor = pathInfo.BorderColor,
                        Level = level,
                        IsInherited = false, // TODO: check from copystats
                        IsModified = isModified,
                        IsSessionEdit = isSessionEdit
                    };
                    item.LevelChanged += (s, newLevel) => OnMagicPathLevelChanged(pathId, newLevel);
                    activePaths.Add(item);
                    usedPathIds.Add(pathId);
                }

                // Build available paths (exclude already used ones)
                foreach (var def in UI.Controls.MagicPathDefinitions.PathDefs)
                {
                    if (!usedPathIds.Contains(def.Id))
                    {
                        var pathInfo = UI.Controls.MagicPathDefinitions.GetPathInfo(def.Id);
                        availablePaths.Add(new UI.Controls.AvailableMagicPath
                        {
                            PathId = def.Id,
                            PathLetter = pathInfo.Letter,
                            PathName = pathInfo.Name,
                            PathColor = pathInfo.Color,
                            TextColor = pathInfo.TextColor,
                            BorderColor = pathInfo.BorderColor
                        });
                    }
                }
            }

            _magicPathsList = activePaths;
            _availableMagicPaths = availablePaths;
            OnPropertyChanged(nameof(MagicPathsList));
            OnPropertyChanged(nameof(AvailableMagicPaths));
        }

        private bool IsIntIntPropertyModifiedFromVanilla(Command command, int value1)
        {
            // Check if a specific IntIntProperty with given Value1 is modified from vanilla
            if (_source == EntitySource.Vanilla) return false;
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null) return true;
            // Check if vanilla has this specific path
            var vanillaMonster = vanillaEntity as Monster;
            if (vanillaMonster == null) return true;
            foreach (var skill in vanillaMonster.MagicSkills)
            {
                if ((int)skill.Path == value1) return false;
            }
            return true;
        }

        private void OnMagicPathLevelChanged(int pathId, int newLevel)
        {
            // Find and update the magic skill property
            var monster = _entity as Monster;
            if (monster != null)
            {
                // Remove old and add new
                var props = monster.Properties.Where(p => p.Command == Command.MAGICSKILL)
                    .Cast<Dom5Edit.Props.IntIntProperty>().ToList();
                var existing = props.FirstOrDefault(p => p.Value1 == pathId);

                var newProp = new Dom5Edit.Props.IntIntProperty
                {
                    Command = Command.MAGICSKILL,
                    Value1 = pathId,
                    Value2 = newLevel,
                    HasValue = true
                };

                // Use CommandHistory for undo/redo support
                if (_history != null && existing != null)
                {
                    // Use SetIntIntPropertyCommand for changing level
                    var cmd = new SetIntIntPropertyCommand(_entity, Command.MAGICSKILL, pathId, newLevel);
                    _history.Execute(cmd);
                }
                else
                {
                    // Fallback: direct modification
                    if (existing != null)
                    {
                        monster.RemoveProperty(existing);
                    }
                    monster.AddProperty(newProp);
                }

                HasSessionChanges = true;
            }
        }

        public void AddMagicPath(int pathId, int level)
        {
            var monster = _entity as Monster;
            if (monster != null)
            {
                var newProp = new Dom5Edit.Props.IntIntProperty
                {
                    Command = Command.MAGICSKILL,
                    Value1 = pathId,
                    Value2 = level,
                    HasValue = true
                };

                // Use CommandHistory for undo/redo support
                if (_history != null)
                {
                    var cmd = new AddPropertyCommand(_entity, newProp, $"Add Magic Path {pathId}");
                    _history.Execute(cmd);
                }
                else
                {
                    monster.AddProperty(newProp);
                }

                HasSessionChanges = true;
                RefreshMagicPaths();
            }
        }

        public void RemoveMagicPath(int pathId)
        {
            var monster = _entity as Monster;
            if (monster != null)
            {
                var props = monster.Properties.Where(p => p.Command == Command.MAGICSKILL)
                    .Cast<Dom5Edit.Props.IntIntProperty>().ToList();
                var existing = props.FirstOrDefault(p => p.Value1 == pathId);
                if (existing != null)
                {
                    // Use CommandHistory for undo/redo support
                    if (_history != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existing, $"Remove Magic Path {pathId}");
                        _history.Execute(cmd);
                    }
                    else
                    {
                        monster.RemoveProperty(existing);
                    }

                    HasSessionChanges = true;
                    RefreshMagicPaths();
                }
            }
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh magic paths list when MAGICSKILL changes
            if (command == Command.MAGICSKILL)
            {
                RefreshMagicPaths();
            }

            // Map Command enum to actual property names (they don't always match)
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        /// <summary>
        /// Maps Command enum values to ViewModel property names.
        /// Returns null if no mapping exists (base class handles it).
        /// </summary>
        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                // Core stats with different names
                Command.HP => "Hp",
                Command.STR => "Strength",
                Command.ATT => "Attack",
                Command.DEF => "Defense",
                Command.PREC => "Precision",
                Command.MR => "MagicResistance",
                Command.MOR => "Morale",
                Command.ENC => "Encumbrance",
                Command.PROT => "Protection",
                Command.AP => "ActionPoints",
                Command.MAPMOVE => "MapMove",
                Command.SIZE => "Size",
                Command.RESSIZE => "ResearchSize",
                Command.EYES => "Eyes",

                // Cost
                Command.GCOST => "GoldCost",
                Command.RCOST => "ResourceCost",

                // Identity
                Command.FIXEDNAME => "FixedName",
                Command.DESCR => "Description",
                Command.SPR1 => "Sprite1",
                Command.SPR2 => "Sprite2",

                _ => null  // Let base class handle or it matches Command name
            };
        }

        protected override string GetCommandDisplayName(Command command)
        {
            return command switch
            {
                // Type
                Command.HUMANOID => "Humanoid", Command.MOUNTEDHUMANOID => "Mounted Humanoid",
                Command.QUADRUPED => "Quadruped", Command.LIZARD => "Lizard", Command.NAGA => "Naga",
                Command.SNAKE => "Snake", Command.BIRD => "Bird", Command.DJINN => "Djinn",
                Command.TROGLODYTE => "Troglodyte", Command.MISCSHAPE => "Misc Shape",
                Command.MOUNTED => "Mounted", Command.UNDEAD => "Undead", Command.DEMON => "Demon",
                Command.MAGICBEING => "Magic Being", Command.HOLY => "Holy", Command.ANIMAL => "Animal",
                Command.UNIQUE => "Unique", Command.INANIMATE => "Inanimate", Command.MINDLESS => "Mindless",
                Command.BLIND => "Blind", Command.COLDBLOOD => "Cold Blooded", Command.IMMORTAL => "Immortal",
                Command.FEMALE => "Female", Command.IMMOBILE => "Immobile", Command.STONEBEING => "Stone Being",
                Command.PLANT => "Plant", Command.DRAKE => "Drake", Command.BUG => "Bug",
                Command.LESSERHORROR => "Lesser Horror", Command.GREATERHORROR => "Greater Horror",
                Command.DOOMHORROR => "Doom Horror",
                // Leadership
                Command.NOLEADER => "No Leader", Command.POORLEADER => "Poor Leader",
                Command.OKLEADER => "OK Leader", Command.GOODLEADER => "Good Leader",
                Command.EXPERTLEADER => "Expert Leader", Command.SUPERIORLEADER => "Superior Leader",
                Command.NOMAGICLEADER => "No Magic Leader", Command.POORMAGICLEADER => "Poor Magic Leader",
                Command.OKMAGICLEADER => "OK Magic Leader", Command.GOODMAGICLEADER => "Good Magic Leader",
                Command.EXPERTMAGICLEADER => "Expert Magic Leader", Command.SUPERIORMAGICLEADER => "Superior Magic Leader",
                Command.NOUNDEADLEADER => "No Undead Leader", Command.POORUNDEADLEADER => "Poor Undead Leader",
                Command.OKUNDEADLEADER => "OK Undead Leader", Command.GOODUNDEADLEADER => "Good Undead Leader",
                Command.EXPERTUNDEADLEADER => "Expert Undead Leader", Command.SUPERIORUNDEADLEADER => "Superior Undead Leader",
                // Movement
                Command.FLYING => "Flying", Command.AQUATIC => "Aquatic", Command.AMPHIBIAN => "Amphibian",
                Command.POORAMPHIBIAN => "Poor Amphibian", Command.FLOAT => "Float", Command.SWIMMING => "Swimming",
                Command.TELEPORT => "Teleport", Command.MAPTELEPORT => "Map Teleport", Command.BLINK => "Blink",
                Command.FORESTSURVIVAL => "Forest Survival", Command.MOUNTAINSURVIVAL => "Mountain Survival",
                Command.SWAMPSURVIVAL => "Swamp Survival", Command.WASTESURVIVAL => "Waste Survival",
                Command.SNOW => "Snow Movement",
                // Resistances
                Command.FIRERES => "Fire Res", Command.COLDRES => "Cold Res",
                Command.SHOCKRES => "Shock Res", Command.POISONRES => "Poison Res",
                Command.REGENERATION => "Regeneration", Command.INVULNERABLE => "Invulnerability",
                Command.AIRSHIELD => "Air Shield", Command.ICEPROT => "Ice Protection",
                Command.REINVIGORATION => "Reinvigoration", Command.IRONVUL => "Iron Vulnerability",
                Command.BLUNTRES => "Blunt Res", Command.PIERCERES => "Pierce Res", Command.SLASHRES => "Slash Res",
                Command.DISEASERES => "Disease Res", Command.MAGICIMMUNE => "Magic Immune",
                Command.STORMIMMUNE => "Storm Immune", Command.STUNIMMUNITY => "Stun Immune",
                Command.POLYIMMUNE => "Polymorph Immune", Command.ACIDRES => "Acid Res", Command.DECAYRES => "Decay Res",
                // Combat
                Command.AWE => "Awe", Command.FEAR => "Fear", Command.BERSERK => "Berserk",
                Command.AMBIDEXTROUS => "Ambidextrous", Command.DARKVISION => "Dark Vision",
                Command.TRAMPLE => "Trample", Command.DEATHCURSE => "Death Curse",
                Command.BODYGUARD => "Bodyguard", Command.WARNING => "Warning", Command.STANDARD => "Standard",
                Command.FORMATIONFIGHTER => "Formation", Command.PATIENCE => "Patience",
                Command.CHAOSPOWER => "Chaos Power", Command.MAGICPOWER => "Magic Power",
                Command.ETHEREAL => "Ethereal", Command.GLAMOUR => "Glamour",
                // Auras
                Command.HEAT => "Heat Aura", Command.COLD => "Cold Aura", Command.FIRESHIELD => "Fire Shield",
                Command.POISONCLOUD => "Poison Cloud", Command.DISEASECLOUD => "Disease Cloud",
                Command.POISONSKIN => "Poison Skin", Command.POISONARMOR => "Poison Armor",
                Command.ACIDSHIELD => "Acid Shield", Command.SLEEPAURA => "Sleep Aura",
                Command.ANIMALAWE => "Animal Awe", Command.SUNAWE => "Sun Awe", Command.HALTHERETIC => "Halt Heretic",
                // Special
                Command.HEAL => "Heal", Command.NOHEAL => "No Heal", Command.HEALER => "Healer",
                Command.AUTOHEALER => "Auto Healer", Command.NEEDNOTEAT => "Need Not Eat",
                Command.TAXCOLLECTOR => "Tax Collector", Command.INQUISITOR => "Inquisitor",
                Command.MASON => "Mason", Command.LOCALSUN => "Local Sun",
                Command.COMMASTER => "Communion Master", Command.COMSLAVE => "Communion Slave",
                Command.SPELLSINGER => "Spell Singer", Command.COMBATCASTER => "Combat Caster",
                Command.DRAINIMMUNE => "Drain Immune", Command.DIVINEINS => "Divine Inspiration",
                Command.NOITEM => "No Items",
                _ => base.GetCommandDisplayName(command)
            };
        }

        // ========================================
        // Equipment (Weapons & Armor)
        // ========================================

        private ObservableCollection<EquipmentItem> _weaponsList;
        public ObservableCollection<EquipmentItem> WeaponsList
        {
            get
            {
                if (_weaponsList == null)
                    RefreshWeaponsList();
                return _weaponsList;
            }
        }

        private ObservableCollection<EquipmentItem> _armorList;
        public ObservableCollection<EquipmentItem> ArmorList
        {
            get
            {
                if (_armorList == null)
                    RefreshArmorList();
                return _armorList;
            }
        }

        /// <summary>
        /// Refresh weapons list using generic layered reference lookup.
        /// </summary>
        private void RefreshWeaponsList()
        {
            _weaponsList = new ObservableCollection<EquipmentItem>();

            // Use base class generic method for layered lookup
            var weapons = GetLayeredReferenceList<WeaponRef>(Command.WEAPON, EntityType.WEAPON, r => r.ID);

            foreach (var (id, name, isInherited, isModified, isSessionEdit) in weapons)
            {
                _weaponsList.Add(new EquipmentItem
                {
                    ID = id,
                    Name = name,
                    IsInherited = isInherited,
                    IsModified = isModified,
                    IsSessionEdit = isSessionEdit,
                    SourceCommand = Command.WEAPON,
                    EntityType = "weapon"
                });
            }

            OnPropertyChanged(nameof(WeaponsList));
        }

        /// <summary>
        /// Refresh armor list using generic layered reference lookup.
        /// </summary>
        private void RefreshArmorList()
        {
            _armorList = new ObservableCollection<EquipmentItem>();

            // Use base class generic method for layered lookup
            var armors = GetLayeredReferenceList<ArmorRef>(Command.ARMOR, EntityType.ARMOR, r => r.ID);

            foreach (var (id, name, isInherited, isModified, isSessionEdit) in armors)
            {
                _armorList.Add(new EquipmentItem
                {
                    ID = id,
                    Name = name,
                    IsInherited = isInherited,
                    IsModified = isModified,
                    IsSessionEdit = isSessionEdit,
                    SourceCommand = Command.ARMOR,
                    EntityType = "armor"
                });
            }

            OnPropertyChanged(nameof(ArmorList));
        }

        /// <summary>
        /// Gets the cached list of available weapons for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableWeapons => CachedWeapons;

        /// <summary>
        /// Gets the cached list of available armor for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableArmor => CachedArmors;

        // Selected items for adding
        private AvailableEquipmentItem _selectedWeaponToAdd;
        public AvailableEquipmentItem SelectedWeaponToAdd
        {
            get => _selectedWeaponToAdd;
            set
            {
                _selectedWeaponToAdd = value;
                OnPropertyChanged(nameof(SelectedWeaponToAdd));
            }
        }

        private AvailableEquipmentItem _selectedArmorToAdd;
        public AvailableEquipmentItem SelectedArmorToAdd
        {
            get => _selectedArmorToAdd;
            set
            {
                _selectedArmorToAdd = value;
                OnPropertyChanged(nameof(SelectedArmorToAdd));
            }
        }

        // Equipment Commands
        private ICommand _addWeaponCommand;
        public ICommand AddWeaponCommand => _addWeaponCommand ??= new RelayCommand<AvailableEquipmentItem>(AddWeapon);

        private ICommand _removeWeaponCommand;
        public ICommand RemoveWeaponCommand => _removeWeaponCommand ??= new RelayCommand<EquipmentItem>(RemoveWeapon);

        private ICommand _addArmorCommand;
        public ICommand AddArmorCommand => _addArmorCommand ??= new RelayCommand<AvailableEquipmentItem>(AddArmor);

        private ICommand _removeArmorCommand;
        public ICommand RemoveArmorCommand => _removeArmorCommand ??= new RelayCommand<EquipmentItem>(RemoveArmor);

        private void AddWeapon(AvailableEquipmentItem weapon)
        {
            if (weapon == null) return;

            var newProp = new WeaponRef { Parent = _entity, Command = Command.WEAPON };
            newProp.ID = weapon.ID;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Weapon #{weapon.ID}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            HasSessionChanges = true;
            RefreshWeaponsList();
        }

        private void RemoveWeapon(EquipmentItem weapon)
        {
            if (weapon == null || weapon.IsInherited) return;

            var props = _entity.GetMultiple(Command.WEAPON).ToList();
            var toRemove = props.FirstOrDefault(p => p is WeaponRef wr && wr.ID == weapon.ID);

            if (toRemove != null)
            {
                if (_history != null)
                {
                    var cmd = new RemovePropertyCommand(_entity, toRemove, $"Remove Weapon #{weapon.ID}");
                    _history.Execute(cmd);
                }
                else
                {
                    _entity.RemoveProperty(toRemove);
                }

                HasSessionChanges = true;
                RefreshWeaponsList();
            }
        }

        private void AddArmor(AvailableEquipmentItem armor)
        {
            if (armor == null) return;

            var newProp = new ArmorRef { Parent = _entity, Command = Command.ARMOR };
            newProp.ID = armor.ID;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Armor #{armor.ID}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            HasSessionChanges = true;
            RefreshArmorList();
        }

        private void RemoveArmor(EquipmentItem armor)
        {
            if (armor == null || armor.IsInherited) return;

            var props = _entity.GetMultiple(Command.ARMOR).ToList();
            var toRemove = props.FirstOrDefault(p => p is ArmorRef ar && ar.ID == armor.ID);

            if (toRemove != null)
            {
                if (_history != null)
                {
                    var cmd = new RemovePropertyCommand(_entity, toRemove, $"Remove Armor #{armor.ID}");
                    _history.Execute(cmd);
                }
                else
                {
                    _entity.RemoveProperty(toRemove);
                }

                HasSessionChanges = true;
                RefreshArmorList();
            }
        }

        // Navigation events for hyperlink-style buttons (to be implemented)
        public event EventHandler<int> WeaponNavigationRequested;
        public event EventHandler<int> ArmorNavigationRequested;

        public void NavigateToWeapon(int weaponId)
        {
            WeaponNavigationRequested?.Invoke(this, weaponId);
        }

        public void NavigateToArmor(int armorId)
        {
            ArmorNavigationRequested?.Invoke(this, armorId);
        }

        // ========================================
        // Available Entities for Reference Selection
        // ========================================

        /// <summary>
        /// Gets the cached list of available monsters for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableMonsters => CachedMonsters;

        /// <summary>
        /// Gets the cached list of available items for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableItems => CachedItems;

        /// <summary>
        /// Gets the cached list of available nations for dropdown binding.
        /// Uses centralized cache from MainWindowViewModel for performance.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> AvailableNations => CachedNations;

        // ========================================
        // Reference Properties (Read current values)
        // ========================================

        /// <summary>
        /// Gets the list of start items on this monster.
        /// </summary>
        public ObservableCollection<EquipmentItem> StartItemsList
        {
            get
            {
                var items = new ObservableCollection<EquipmentItem>();
                foreach (var prop in _entity.GetMultiple(Command.STARTITEM))
                {
                    if (prop is ItemRef itemRef && itemRef.HasValue)
                    {
                        items.Add(new EquipmentItem
                        {
                            ID = itemRef.ID,
                            Name = itemRef.Entity?.Name,
                            IsModified = true,
                            SourceCommand = Command.STARTITEM
                        });
                    }
                }
                return items;
            }
        }

        private AvailableEquipmentItem _selectedItemToAdd;
        public AvailableEquipmentItem SelectedItemToAdd
        {
            get => _selectedItemToAdd;
            set
            {
                _selectedItemToAdd = value;
                OnPropertyChanged(nameof(SelectedItemToAdd));
            }
        }

        private ICommand _addStartItemCommand;
        public ICommand AddStartItemCommand => _addStartItemCommand ??= new RelayCommand<AvailableEquipmentItem>(AddStartItem);

        private ICommand _removeStartItemCommand;
        public ICommand RemoveStartItemCommand => _removeStartItemCommand ??= new RelayCommand<EquipmentItem>(RemoveStartItem);

        private void AddStartItem(AvailableEquipmentItem item)
        {
            if (item == null) return;

            var newProp = new ItemRef { Parent = _entity, Command = Command.STARTITEM };
            newProp.ID = item.ID;
            newProp.Resolve();

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, $"Add Start Item #{item.ID}");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            HasSessionChanges = true;
            OnPropertyChanged(nameof(StartItemsList));
        }

        private void RemoveStartItem(EquipmentItem item)
        {
            if (item == null) return;

            var props = _entity.GetMultiple(Command.STARTITEM).ToList();
            var toRemove = props.FirstOrDefault(p => p is ItemRef ir && ir.ID == item.ID);

            if (toRemove != null)
            {
                if (_history != null)
                {
                    var cmd = new RemovePropertyCommand(_entity, toRemove, $"Remove Start Item #{item.ID}");
                    _history.Execute(cmd);
                }
                else
                {
                    _entity.RemoveProperty(toRemove);
                }

                HasSessionChanges = true;
                OnPropertyChanged(nameof(StartItemsList));
            }
        }

        // ========================================
        // CUSTOMMAGIC Support
        // ========================================

        /// <summary>
        /// Gets the list of custom magic configurations on this monster.
        /// </summary>
        public ObservableCollection<CustomMagicItem> CustomMagicList
        {
            get
            {
                var items = new ObservableCollection<CustomMagicItem>();
                foreach (var prop in _entity.GetMultiple(Command.CUSTOMMAGIC))
                {
                    if (prop is BitmaskChanceProperty bcp && bcp.HasValue)
                    {
                        items.Add(new CustomMagicItem
                        {
                            Bitmask = bcp.Bitmask,
                            Chance = bcp.Chance,
                            Property = bcp
                        });
                    }
                }
                return items;
            }
        }

        private ICommand _addCustomMagicCommand;
        public ICommand AddCustomMagicCommand => _addCustomMagicCommand ??= new RelayCommand(AddCustomMagic);

        private ICommand _removeCustomMagicCommand;
        public ICommand RemoveCustomMagicCommand => _removeCustomMagicCommand ??= new RelayCommand<CustomMagicItem>(RemoveCustomMagic);

        private void AddCustomMagic()
        {
            var newProp = new BitmaskChanceProperty
            {
                Parent = _entity,
                Command = Command.CUSTOMMAGIC,
                Bitmask = 0,
                Chance = 100,
                HasValue = true
            };

            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, newProp, "Add Custom Magic");
                _history.Execute(cmd);
            }
            else
            {
                _entity.AddProperty(newProp);
            }

            HasSessionChanges = true;
            OnPropertyChanged(nameof(CustomMagicList));
        }

        private void RemoveCustomMagic(CustomMagicItem item)
        {
            if (item?.Property == null) return;

            if (_history != null)
            {
                var cmd = new RemovePropertyCommand(_entity, item.Property, "Remove Custom Magic");
                _history.Execute(cmd);
            }
            else
            {
                _entity.RemoveProperty(item.Property);
            }

            HasSessionChanges = true;
            OnPropertyChanged(nameof(CustomMagicList));
        }

        public void UpdateCustomMagic(CustomMagicItem item, ulong newBitmask, int newChance)
        {
            if (item?.Property == null) return;

            item.Property.Bitmask = newBitmask;
            item.Property.Chance = newChance;
            item.Bitmask = newBitmask;
            item.Chance = newChance;

            HasSessionChanges = true;
            OnPropertyChanged(nameof(CustomMagicList));
        }
    }

    /// <summary>
    /// Represents a CUSTOMMAGIC entry for display.
    /// </summary>
    public class CustomMagicItem
    {
        public ulong Bitmask { get; set; }
        public int Chance { get; set; }
        public BitmaskChanceProperty Property { get; set; }

        // Path helpers for UI binding
        public bool HasFire => (Bitmask & (1UL << 0)) != 0;
        public bool HasAir => (Bitmask & (1UL << 1)) != 0;
        public bool HasWater => (Bitmask & (1UL << 2)) != 0;
        public bool HasEarth => (Bitmask & (1UL << 3)) != 0;
        public bool HasAstral => (Bitmask & (1UL << 4)) != 0;
        public bool HasDeath => (Bitmask & (1UL << 5)) != 0;
        public bool HasNature => (Bitmask & (1UL << 6)) != 0;
        public bool HasBlood => (Bitmask & (1UL << 7)) != 0;

        public string PathsDisplay
        {
            get
            {
                var paths = new List<string>();
                if (HasFire) paths.Add("F");
                if (HasAir) paths.Add("A");
                if (HasWater) paths.Add("W");
                if (HasEarth) paths.Add("E");
                if (HasAstral) paths.Add("S");
                if (HasDeath) paths.Add("D");
                if (HasNature) paths.Add("N");
                if (HasBlood) paths.Add("B");
                return paths.Count > 0 ? string.Join("", paths) : "(none)";
            }
        }

        public string Display => $"{PathsDisplay} @ {Chance}%";
    }

    /// <summary>
    /// ViewModel for Weapon entities.
    /// </summary>
    public class WeaponViewModel : EntityViewModel
    {
        public WeaponViewModel(Weapon entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Weapon Weapon => (Weapon)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from weapon_badges.json.
        /// </summary>
        protected override string EntityTypeName => "weapon";

        // ========================================
        // Copy From Support
        // ========================================

        public string CopyWeaponDisplay
        {
            get
            {
                var result = _entity.TryGet<WeaponRef>(Command.COPYWEAPON, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasCopyWeapon
        {
            get
            {
                var result = _entity.TryGet<WeaponRef>(Command.COPYWEAPON, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        // ========================================
        // Core Stats
        // ========================================

        // Special damage type constants
        private const string DamageTypeSummon = "summonunits";
        private const string DamageTypeCloud = "cloud";
        private const int DefaultSummonMonsterId = 297;

        /// <summary>
        /// Gets the raw damage string value (could be a number, "summonunits", "cloud", etc.)
        /// </summary>
        public string DamageRawValue
        {
            get
            {
                var result = _entity.TryGet<StringProperty>(Command.DMG, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                    return prop.Value;
                return null;
            }
        }

        /// <summary>
        /// Gets the effective damage type by checking current value and inherited value.
        /// Returns "summonunits", "cloud", or null for normal/numeric damage.
        /// </summary>
        public string EffectiveDamageType
        {
            get
            {
                // First check direct value
                var rawValue = DamageRawValue;
                if (rawValue == DamageTypeSummon || rawValue == DamageTypeCloud)
                    return rawValue;

                // Check if inherited from copyweapon source
                var copyResult = _entity.TryGet<WeaponRef>(Command.COPYWEAPON, out var copyRef);
                if (copyResult == ReturnType.TRUE && copyRef?.Entity is Weapon sourceWeapon)
                {
                    var sourceResult = sourceWeapon.TryGet<StringProperty>(Command.DMG, out var sourceProp);
                    if ((sourceResult == ReturnType.TRUE || sourceResult == ReturnType.COPIED) && sourceProp != null)
                    {
                        if (sourceProp.Value == DamageTypeSummon || sourceProp.Value == DamageTypeCloud)
                            return sourceProp.Value;
                    }
                }

                return null; // Normal numeric damage
            }
        }

        /// <summary>
        /// Returns true if this weapon summons units (damage value is monster ID).
        /// </summary>
        public bool IsSummonWeapon => EffectiveDamageType == DamageTypeSummon;

        /// <summary>
        /// Returns true if this weapon creates a cloud (damage is read-only).
        /// </summary>
        public bool IsCloudWeapon => EffectiveDamageType == DamageTypeCloud;

        /// <summary>
        /// Gets the label for the damage field based on weapon type.
        /// </summary>
        public string DamageLabel => IsSummonWeapon ? "Summon ID" : (IsCloudWeapon ? "Effect" : "DMG");

        /// <summary>
        /// Gets or sets weapon damage value. For summon weapons, this is the monster ID.
        /// For cloud weapons, this is read-only. For normal weapons, this is damage.
        /// </summary>
        public int? Damage
        {
            get
            {
                var rawValue = DamageRawValue;
                if (rawValue == null)
                    return null;

                // If it's a special type string, return the default or null
                if (rawValue == DamageTypeSummon)
                    return DefaultSummonMonsterId;
                if (rawValue == DamageTypeCloud)
                    return null; // Cloud has no numeric value

                // Try to parse as integer
                if (int.TryParse(rawValue, out int val))
                    return val;

                return null;
            }
            set
            {
                // Don't allow editing cloud weapons
                if (IsCloudWeapon && EffectiveDamageType == DamageTypeCloud)
                    return;

                if (value.HasValue)
                {
                    var result = _entity.TryGet<StringProperty>(Command.DMG, out var prop, checkCopy: false);
                    if (result == ReturnType.TRUE && prop != null)
                    {
                        prop.Value = value.Value.ToString();
                    }
                    else
                    {
                        // Create new property
                        SetIntProperty(Command.DMG, value);
                    }
                }
                else
                {
                    _entity.RemoveProperty(Command.DMG);
                }
                OnPropertyChanged(nameof(Damage));
                OnPropertyChanged(nameof(DamageRawValue));
                OnPropertyChanged(nameof(IsDamageModified));
            }
        }

        /// <summary>
        /// Gets a display string for damage that includes context for special types.
        /// </summary>
        public string DamageDisplayString
        {
            get
            {
                if (IsCloudWeapon)
                    return "Cloud";
                if (IsSummonWeapon)
                {
                    var monsterId = Damage ?? DefaultSummonMonsterId;
                    // Try to get monster name
                    if (VanillaLoader.Vanilla?.Database.TryGetValue(EntityType.MONSTER, out var monsterSet) == true &&
                        monsterSet.TryGetValue(monsterId, out var monster))
                    {
                        return $"Summons: {monster.Name ?? $"#{monsterId}"}";
                    }
                    return $"Summons: #{monsterId}";
                }
                return Damage?.ToString() ?? "";
            }
        }

        public bool IsDamageModified
        {
            get
            {
                var currentResult = _entity.TryGet<StringProperty>(Command.DMG, out var currentProp, checkCopy: false);
                var hasCurrent = currentResult == ReturnType.TRUE;

                if (_entity is IDEntity idEntity &&
                    VanillaLoader.Vanilla?.Database.TryGetValue(EntityType.WEAPON, out var vanillaSet) == true)
                {
                    if (vanillaSet.TryGetValue(idEntity.ID, out var vanillaEntity) && vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<StringProperty>(Command.DMG, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            return hasCurrent && currentProp?.Value != vanillaProp?.Value;
                        }
                    }
                }
                return hasCurrent;
            }
        }
        public bool IsDamageSessionEdit => IsPropertyEditedInSession(Command.DMG);
        public bool IsDamageInherited
        {
            get
            {
                var result = _entity.TryGet<StringProperty>(Command.DMG, out _, checkCopy: false);
                return result == ReturnType.COPIED;
            }
        }

        /// <summary>
        /// Returns true if damage can be edited (not a cloud weapon with no override).
        /// </summary>
        public bool CanEditDamage => !IsCloudWeapon;

        public int? NumberOfAttacks
        {
            get => GetIntProperty(Command.NRATT);
            set => SetIntProperty(Command.NRATT, value);
        }
        public bool IsNumberOfAttacksModified => IsIntPropertyModifiedFromVanilla(Command.NRATT);
        public bool IsNumberOfAttacksSessionEdit => IsPropertyEditedInSession(Command.NRATT);
        public bool IsNumberOfAttacksInherited => IsIntPropertyInherited(Command.NRATT);

        public int? Attack
        {
            get => GetIntProperty(Command.ATT);
            set => SetIntProperty(Command.ATT, value);
        }
        public bool IsAttackModified => IsIntPropertyModifiedFromVanilla(Command.ATT);
        public bool IsAttackSessionEdit => IsPropertyEditedInSession(Command.ATT);
        public bool IsAttackInherited => IsIntPropertyInherited(Command.ATT);

        public int? Defense
        {
            get => GetIntProperty(Command.DEF);
            set => SetIntProperty(Command.DEF, value);
        }
        public bool IsDefenseModified => IsIntPropertyModifiedFromVanilla(Command.DEF);
        public bool IsDefenseSessionEdit => IsPropertyEditedInSession(Command.DEF);
        public bool IsDefenseInherited => IsIntPropertyInherited(Command.DEF);

        public int? Length
        {
            get => GetIntProperty(Command.LEN);
            set => SetIntProperty(Command.LEN, value);
        }
        public bool IsLengthModified => IsIntPropertyModifiedFromVanilla(Command.LEN);
        public bool IsLengthSessionEdit => IsPropertyEditedInSession(Command.LEN);
        public bool IsLengthInherited => IsIntPropertyInherited(Command.LEN);

        public int? ResourceCost
        {
            get => GetIntProperty(Command.RCOST);
            set => SetIntProperty(Command.RCOST, value);
        }
        public bool IsResourceCostModified => IsIntPropertyModifiedFromVanilla(Command.RCOST);
        public bool IsResourceCostSessionEdit => IsPropertyEditedInSession(Command.RCOST);
        public bool IsResourceCostInherited => IsIntPropertyInherited(Command.RCOST);

        public int? AreaOfEffect
        {
            get => GetIntProperty(Command.AOE);
            set => SetIntProperty(Command.AOE, value);
        }
        public bool IsAreaOfEffectModified => IsIntPropertyModifiedFromVanilla(Command.AOE);
        public bool IsAreaOfEffectSessionEdit => IsPropertyEditedInSession(Command.AOE);
        public bool IsAreaOfEffectInherited => IsIntPropertyInherited(Command.AOE);

        // ========================================
        // Ranged Stats
        // ========================================

        public int? Range
        {
            get => GetIntProperty(Command.RANGE);
            set => SetIntProperty(Command.RANGE, value);
        }
        public bool IsRangeModified => IsIntPropertyModifiedFromVanilla(Command.RANGE);
        public bool IsRangeSessionEdit => IsPropertyEditedInSession(Command.RANGE);
        public bool IsRangeInherited => IsIntPropertyInherited(Command.RANGE);

        public int? Precision
        {
            get => GetIntProperty(Command.PREC);
            set => SetIntProperty(Command.PREC, value);
        }
        public bool IsPrecisionModified => IsIntPropertyModifiedFromVanilla(Command.PREC);
        public bool IsPrecisionSessionEdit => IsPropertyEditedInSession(Command.PREC);
        public bool IsPrecisionInherited => IsIntPropertyInherited(Command.PREC);

        public int? Ammo
        {
            get => GetIntProperty(Command.AMMO);
            set => SetIntProperty(Command.AMMO, value);
        }
        public bool IsAmmoModified => IsIntPropertyModifiedFromVanilla(Command.AMMO);
        public bool IsAmmoSessionEdit => IsPropertyEditedInSession(Command.AMMO);
        public bool IsAmmoInherited => IsIntPropertyInherited(Command.AMMO);

        // ========================================
        // Badge Collection (Unified)
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.DMG => "Damage",
                Command.DAMAGE => "Damage",
                Command.NRATT => "NumberOfAttacks",
                Command.ATT => "Attack",
                Command.DEF => "Defense",
                Command.LEN => "Length",
                Command.RCOST => "ResourceCost",
                Command.AOE => "AreaOfEffect",
                Command.RANGE => "Range",
                Command.PREC => "Precision",
                Command.AMMO => "Ammo",
                _ => null
            };
        }

        // ========================================
        // Secondary Effect Support
        // ========================================

        /// <summary>
        /// Returns true if this weapon has a secondary effect (either 25% or 100% chance).
        /// </summary>
        public bool HasSecondaryEffect
        {
            get
            {
                var alwaysResult = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out _);
                if (alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED)
                    return true;

                var result = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out _);
                return result == ReturnType.TRUE || result == ReturnType.COPIED;
            }
        }

        /// <summary>
        /// Gets the display string for the secondary effect, showing name and chance.
        /// </summary>
        public string SecondaryEffectDisplay
        {
            get
            {
                // Check for secondaryeffectalways first (100% chance)
                var alwaysResult = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
                if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp != null)
                {
                    var name = alwaysProp.Entity?.Name ?? $"#{alwaysProp.ID}";
                    return $"{name} (100%)";
                }

                // Check for secondaryeffect (25% chance)
                var result = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                {
                    var name = prop.Entity?.Name ?? $"#{prop.ID}";
                    return $"{name} (25%)";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the referenced secondary effect weapon entity, if any.
        /// </summary>
        public Weapon SecondaryEffectWeapon
        {
            get
            {
                var alwaysResult = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
                if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp?.Entity is Weapon w1)
                    return w1;

                var result = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop?.Entity is Weapon w2)
                    return w2;

                return null;
            }
        }

        /// <summary>
        /// Gets the damage type string for display (Pierce, Slash, Blunt, Fire, etc.).
        /// </summary>
        public string DamageTypes
        {
            get
            {
                var types = new List<string>();
                if (GetWeaponFlag(Command.PIERCE)) types.Add("Pierce");
                if (GetWeaponFlag(Command.SLASH)) types.Add("Slash");
                if (GetWeaponFlag(Command.BLUNT)) types.Add("Blunt");
                if (GetWeaponFlag(Command.FIRE)) types.Add("Fire");
                if (GetWeaponFlag(Command.COLD)) types.Add("Cold");
                if (GetWeaponFlag(Command.SHOCK)) types.Add("Shock");
                if (GetWeaponFlag(Command.POISON)) types.Add("Poison");
                if (GetWeaponFlag(Command.ACID)) types.Add("Acid");
                if (GetWeaponFlag(Command.MAGIC)) types.Add("Magic");
                if (GetWeaponFlag(Command.HOLY)) types.Add("Holy");
                if (GetWeaponFlag(Command.DEMON)) types.Add("Demon");
                return types.Count > 0 ? string.Join(", ", types) : null;
            }
        }

        /// <summary>
        /// Gets special properties string for display (Armor Negating, Armor Piercing, etc.).
        /// </summary>
        public string SpecialProperties
        {
            get
            {
                var props = new List<string>();
                if (GetWeaponFlag(Command.ARMORNEGATING)) props.Add("Armor Negating");
                if (GetWeaponFlag(Command.ARMORPIERCING)) props.Add("Armor Piercing");
                if (GetWeaponFlag(Command.HARDMRNEG)) props.Add("Hard MR Negating");
                if (GetWeaponFlag(Command.MRNEGATES)) props.Add("MR Negates");
                if (GetWeaponFlag(Command.MIND)) props.Add("Mind");
                if (GetWeaponFlag(Command.UNDEADIMMUNE)) props.Add("Undead Immune");
                if (GetWeaponFlag(Command.INANIMATEIMMUNE)) props.Add("Inanimate Immune");
                if (GetWeaponFlag(Command.FLYSPR)) props.Add("Flying Projectile");
                if (GetWeaponFlag(Command.TWOHANDED)) props.Add("Two-Handed");
                if (GetWeaponFlag(Command.NATURAL)) props.Add("Natural");
                if (GetWeaponFlag(Command.CHARGE)) props.Add("Charge");
                if (GetWeaponFlag(Command.FLAIL)) props.Add("Flail");
                if (GetWeaponFlag(Command.BONUS)) props.Add("Bonus");
                if (GetWeaponFlag(Command.NOSTR)) props.Add("No Strength");
                return props.Count > 0 ? string.Join(", ", props) : null;
            }
        }

        private bool GetWeaponFlag(Command command)
        {
            var result = _entity.TryGet<IntProperty>(command, out var prop);
            return (result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null && prop.Value != 0;
        }
    }

    /// <summary>
    /// ViewModel for Armor entities.
    /// </summary>
    public class ArmorViewModel : EntityViewModel
    {
        public ArmorViewModel(Armor entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Armor Armor => (Armor)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from armor_badges.json.
        /// </summary>
        protected override string EntityTypeName => "armor";

        // ========================================
        // Copy From Support
        // ========================================

        public string CopyArmorDisplay
        {
            get
            {
                var result = _entity.TryGet<ArmorRef>(Command.COPYARMOR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasCopyArmor
        {
            get
            {
                var result = _entity.TryGet<ArmorRef>(Command.COPYARMOR, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        // ========================================
        // Armor Type Display (derived from stats badge data)
        // ========================================

        /// <summary>
        /// Gets the armor type display name.
        /// Uses layered property access like the badge system.
        /// </summary>
        public string ArmorTypeDisplay
        {
            get
            {
                var armorType = GetIntProperty(Command.TYPE);
                return armorType switch
                {
                    1 => "Helmet",
                    4 => "Body Armor",
                    5 => "Shield",
                    6 => "Misc Body",
                    _ => armorType?.ToString() ?? "Unknown"
                };
            }
        }

        // ========================================
        // Stats Badge Collection (JSON-driven)
        // ========================================

        private ObservableCollection<PropertyItem> _statsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableStatsBadges;

        public ObservableCollection<PropertyItem> StatsBadges
        {
            get { if (_statsBadges == null) RefreshStatsBadges(); return _statsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableStatsBadges
        {
            get { if (_availableStatsBadges == null) RefreshStatsBadges(); return _availableStatsBadges; }
        }

        // Commands for stats badge operations
        private RelayCommand<PropertyItem> _removeStatsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addStatsBadgeCommand;

        public RelayCommand<PropertyItem> RemoveStatsBadgeCommand => _removeStatsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshStatsBadges);
        public RelayCommand<AvailablePropertyItem> AddStatsBadgeCommand => _addStatsBadgeCommand ??= CreateAddBadgeCommand(RefreshStatsBadges);

        private void RefreshStatsBadges()
        {
            var (active, available) = BuildBadgesFromSection("stats", BadgeValueChangedHandler);
            _statsBadges = active;
            _availableStatsBadges = available;
            OnPropertyChanged(nameof(StatsBadges));
            OnPropertyChanged(nameof(AvailableStatsBadges));
            OnPropertyChanged(nameof(ArmorTypeDisplay)); // Update derived property
        }

        // ========================================
        // Properties Badge Collection (JSON-driven)
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        // Commands for property badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler for all badge sections
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh the appropriate badge collection when undo/redo affects this entity
            RefreshStatsBadges();
            RefreshPropertyBadges();
        }
    }

    /// <summary>
    /// ViewModel for Spell entities.
    /// </summary>
    public class SpellViewModel : EntityViewModel
    {
        public SpellViewModel(Spell entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Spell Spell => (Spell)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from spell_badges.json.
        /// </summary>
        protected override string EntityTypeName => "spell";

        // ========================================
        // Copy Spell Support (#copyspell)
        // ========================================

        /// <summary>
        /// Gets the CopySpell reference display text.
        /// </summary>
        public string CopySpellDisplay
        {
            get
            {
                var result = _entity.TryGet<CopySpellRef>(Command.COPYSPELL, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasCopySpell
        {
            get
            {
                var result = _entity.TryGet<CopySpellRef>(Command.COPYSPELL, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        // ========================================
        // Core Stats (School, Research Level, Path)
        // ========================================

        /// <summary>
        /// Magic school: 0=Conj, 1=Alt, 2=Evo, 3=Const, 4=Ench, 5=Thau, 6=Blood, -1=not researchable
        /// </summary>
        public int? School
        {
            get => GetIntProperty(Command.SCHOOL);
            set => SetIntProperty(Command.SCHOOL, value);
        }
        public bool IsSchoolModified => IsIntPropertyModifiedFromVanilla(Command.SCHOOL);
        public bool IsSchoolSessionEdit => IsPropertyEditedInSession(Command.SCHOOL);
        public bool IsSchoolInherited => IsIntPropertyInherited(Command.SCHOOL);

        /// <summary>
        /// Gets the school display name.
        /// </summary>
        public string SchoolDisplay
        {
            get
            {
                return School switch
                {
                    -1 => "Not Researchable",
                    0 => "Conjuration",
                    1 => "Alteration",
                    2 => "Evocation",
                    3 => "Construction",
                    4 => "Enchantment",
                    5 => "Thaumaturgy",
                    6 => "Blood",
                    _ => School?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Research level required to learn this spell.
        /// </summary>
        public int? ResearchLevel
        {
            get => GetIntProperty(Command.RESEARCHLEVEL);
            set => SetIntProperty(Command.RESEARCHLEVEL, value);
        }
        public bool IsResearchLevelModified => IsIntPropertyModifiedFromVanilla(Command.RESEARCHLEVEL);
        public bool IsResearchLevelSessionEdit => IsPropertyEditedInSession(Command.RESEARCHLEVEL);
        public bool IsResearchLevelInherited => IsIntPropertyInherited(Command.RESEARCHLEVEL);

        // ========================================
        // Path Requirements (IntIntProperty: path slot, path type)
        // ========================================

        /// <summary>
        /// Gets the primary path requirement display.
        /// #path takes two parameters: requirement slot (0 or 1) and path number.
        /// </summary>
        public string PrimaryPathDisplay
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var primaryPath = allPaths.FirstOrDefault(p => p.Value1 == 0);
                if (primaryPath != null)
                {
                    return GetPathDisplayName(primaryPath.Value2);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the primary path level requirement.
        /// #pathlevel takes two parameters: requirement slot (0 or 1) and level.
        /// </summary>
        public int? PrimaryPathLevel
        {
            get
            {
                var allLevels = Spell.Properties.Where(p => p.Command == Command.PATHLEVEL)
                    .Cast<IntIntProperty>().ToList();
                var primaryLevel = allLevels.FirstOrDefault(p => p.Value1 == 0);
                return primaryLevel?.Value2;
            }
        }

        /// <summary>
        /// Gets the secondary path requirement display.
        /// </summary>
        public string SecondaryPathDisplay
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var secondaryPath = allPaths.FirstOrDefault(p => p.Value1 == 1);
                if (secondaryPath != null)
                {
                    return GetPathDisplayName(secondaryPath.Value2);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the secondary path level requirement.
        /// </summary>
        public int? SecondaryPathLevel
        {
            get
            {
                var allLevels = Spell.Properties.Where(p => p.Command == Command.PATHLEVEL)
                    .Cast<IntIntProperty>().ToList();
                var secondaryLevel = allLevels.FirstOrDefault(p => p.Value1 == 1);
                return secondaryLevel?.Value2;
            }
        }

        /// <summary>
        /// Whether the spell has path requirements.
        /// </summary>
        public bool HasPathRequirements => PrimaryPathDisplay != null || SecondaryPathDisplay != null;

        /// <summary>
        /// Gets the path letter for icon lookup (F, A, W, E, S, D, N, G, B).
        /// </summary>
        public string PrimaryPathLetter
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var primaryPath = allPaths.FirstOrDefault(p => p.Value1 == 0);
                if (primaryPath != null)
                {
                    return GetPathLetter(primaryPath.Value2);
                }
                return null;
            }
        }

        public string SecondaryPathLetter
        {
            get
            {
                var allPaths = Spell.Properties.Where(p => p.Command == Command.PATH)
                    .Cast<IntIntProperty>().ToList();
                var secondaryPath = allPaths.FirstOrDefault(p => p.Value1 == 1);
                if (secondaryPath != null)
                {
                    return GetPathLetter(secondaryPath.Value2);
                }
                return null;
            }
        }

        private static string GetPathDisplayName(int pathId)
        {
            return pathId switch
            {
                0 => "Fire",
                1 => "Air",
                2 => "Water",
                3 => "Earth",
                4 => "Astral",
                5 => "Death",
                6 => "Nature",
                7 => "Blood",
                8 => "Holy",
                _ => pathId.ToString()
            };
        }

        private static string GetPathLetter(int pathId)
        {
            return pathId switch
            {
                0 => "F",
                1 => "A",
                2 => "W",
                3 => "E",
                4 => "S",
                5 => "D",
                6 => "N",
                7 => "B",
                8 => "H",
                _ => null
            };
        }

        // ========================================
        // Fatigue/Gem Cost
        // ========================================

        /// <summary>
        /// Fatigue cost for this spell.
        /// Encoded as gems*1000 + fatigue (e.g., 2050 = 2 gems + 50 fatigue).
        /// </summary>
        public int? FatigueCost
        {
            get => GetIntProperty(Command.FATIGUECOST);
            set => SetIntProperty(Command.FATIGUECOST, value);
        }
        public bool IsFatigueCostModified => IsIntPropertyModifiedFromVanilla(Command.FATIGUECOST);
        public bool IsFatigueCostSessionEdit => IsPropertyEditedInSession(Command.FATIGUECOST);
        public bool IsFatigueCostInherited => IsIntPropertyInherited(Command.FATIGUECOST);

        /// <summary>
        /// Gets the decoded gem cost (fatigueCost / 1000).
        /// </summary>
        public int GemCost => (FatigueCost ?? 0) / 1000;

        /// <summary>
        /// Gets the decoded fatigue only (fatigueCost % 1000).
        /// </summary>
        public int FatigueOnly => (FatigueCost ?? 0) % 1000;

        /// <summary>
        /// Gets a display string for the fatigue cost.
        /// </summary>
        public string FatigueCostDisplay
        {
            get
            {
                var cost = FatigueCost ?? 0;
                if (cost == 0) return "-";
                var gems = cost / 1000;
                var fatigue = cost % 1000;
                if (gems > 0 && fatigue > 0)
                    return $"{gems}G + {fatigue}F";
                if (gems > 0)
                    return $"{gems}G";
                return $"{fatigue}F";
            }
        }

        // ========================================
        // Battle Stats
        // ========================================

        public int? Range
        {
            get => GetIntProperty(Command.RANGE);
            set => SetIntProperty(Command.RANGE, value);
        }
        public bool IsRangeModified => IsIntPropertyModifiedFromVanilla(Command.RANGE);
        public bool IsRangeSessionEdit => IsPropertyEditedInSession(Command.RANGE);
        public bool IsRangeInherited => IsIntPropertyInherited(Command.RANGE);

        public int? Precision
        {
            get => GetIntProperty(Command.PRECISION);
            set => SetIntProperty(Command.PRECISION, value);
        }
        public bool IsPrecisionModified => IsIntPropertyModifiedFromVanilla(Command.PRECISION);
        public bool IsPrecisionSessionEdit => IsPropertyEditedInSession(Command.PRECISION);
        public bool IsPrecisionInherited => IsIntPropertyInherited(Command.PRECISION);

        public int? AOE
        {
            get => GetIntProperty(Command.AOE);
            set => SetIntProperty(Command.AOE, value);
        }
        public bool IsAOEModified => IsIntPropertyModifiedFromVanilla(Command.AOE);
        public bool IsAOESessionEdit => IsPropertyEditedInSession(Command.AOE);
        public bool IsAOEInherited => IsIntPropertyInherited(Command.AOE);

        public int? Damage
        {
            get => GetIntProperty(Command.DAMAGE);
            set => SetIntProperty(Command.DAMAGE, value);
        }
        public bool IsDamageModified => IsIntPropertyModifiedFromVanilla(Command.DAMAGE);
        public bool IsDamageSessionEdit => IsPropertyEditedInSession(Command.DAMAGE);
        public bool IsDamageInherited => IsIntPropertyInherited(Command.DAMAGE);

        public int? Effect
        {
            get => GetIntProperty(Command.EFFECT);
            set => SetIntProperty(Command.EFFECT, value);
        }
        public bool IsEffectModified => IsIntPropertyModifiedFromVanilla(Command.EFFECT);
        public bool IsEffectSessionEdit => IsPropertyEditedInSession(Command.EFFECT);
        public bool IsEffectInherited => IsIntPropertyInherited(Command.EFFECT);

        public int? NrEff
        {
            get => GetIntProperty(Command.NREFF);
            set => SetIntProperty(Command.NREFF, value);
        }
        public bool IsNrEffModified => IsIntPropertyModifiedFromVanilla(Command.NREFF);
        public bool IsNrEffSessionEdit => IsPropertyEditedInSession(Command.NREFF);
        public bool IsNrEffInherited => IsIntPropertyInherited(Command.NREFF);

        // ========================================
        // Next Spell Chain
        // ========================================

        /// <summary>
        /// Gets the NextSpell reference display text.
        /// </summary>
        public string NextSpellDisplay
        {
            get
            {
                var result = _entity.TryGet<SpellRef>(Command.NEXTSPELL, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasNextSpell
        {
            get
            {
                var result = _entity.TryGet<SpellRef>(Command.NEXTSPELL, out _);
                return result == ReturnType.TRUE;
            }
        }

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.SCHOOL => "School",
                Command.RESEARCHLEVEL => "ResearchLevel",
                Command.FATIGUECOST => "FatigueCost",
                Command.RANGE => "Range",
                Command.PRECISION => "Precision",
                Command.AOE => "AOE",
                Command.DAMAGE => "Damage",
                Command.EFFECT => "Effect",
                Command.NREFF => "NrEff",
                _ => null
            };
        }
    }

    /// <summary>
    /// ViewModel for Item entities.
    /// </summary>
    public class ItemViewModel : EntityViewModel
    {
        public ItemViewModel(Item entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Item Item => (Item)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from item_badges.json.
        /// </summary>
        protected override string EntityTypeName => "item";

        // ========================================
        // Copy From Support
        // ========================================

        public string CopyItemDisplay
        {
            get
            {
                var result = _entity.TryGet<ItemRef>(Command.COPYITEM, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? prop.ID.ToString();
                }
                return null;
            }
        }

        public bool HasCopyItem
        {
            get
            {
                var result = _entity.TryGet<ItemRef>(Command.COPYITEM, out _, checkCopy: false);
                return result == ReturnType.TRUE;
            }
        }

        // ========================================
        // Core Stats
        // ========================================

        public int? ConstLevel
        {
            get => GetIntProperty(Command.CONSTLEVEL);
            set => SetIntProperty(Command.CONSTLEVEL, value);
        }
        public bool IsConstLevelModified => IsIntPropertyModifiedFromVanilla(Command.CONSTLEVEL);
        public bool IsConstLevelSessionEdit => IsPropertyEditedInSession(Command.CONSTLEVEL);
        public bool IsConstLevelInherited => IsIntPropertyInherited(Command.CONSTLEVEL);

        public int? ItemType
        {
            get => GetIntProperty(Command.TYPE);
            set => SetIntProperty(Command.TYPE, value);
        }
        public bool IsItemTypeModified => IsIntPropertyModifiedFromVanilla(Command.TYPE);
        public bool IsItemTypeSessionEdit => IsPropertyEditedInSession(Command.TYPE);
        public bool IsItemTypeInherited => IsIntPropertyInherited(Command.TYPE);

        /// <summary>
        /// Gets the item slot type display name.
        /// Item types: 1=1-H Weapon, 2=2-H Weapon, 3=Missile Weapon, 4=Shield,
        /// 5=Body Armor, 6=Helmet, 7=Boots, 8=Misc, 9=Crown, 10=Barding
        /// </summary>
        public string ItemTypeDisplay
        {
            get
            {
                return ItemType switch
                {
                    1 => "1-H Weapon",
                    2 => "2-H Weapon",
                    3 => "Missile Weapon",
                    4 => "Shield",
                    5 => "Body Armor",
                    6 => "Helmet",
                    7 => "Boots",
                    8 => "Misc",
                    9 => "Crown",
                    10 => "Barding",
                    _ => ItemType?.ToString() ?? "Unknown"
                };
            }
        }

        /// <summary>
        /// Available slot types for ComboBox binding.
        /// Item types: 1=1-H Weapon, 2=2-H Weapon, 3=Missile Weapon, 4=Shield,
        /// 5=Body Armor, 6=Helmet, 7=Boots, 8=Misc, 9=Crown, 10=Barding
        /// </summary>
        public static List<SlotTypeOption> SlotTypes { get; } = new List<SlotTypeOption>
        {
            new SlotTypeOption(1, "1-H Weapon"),
            new SlotTypeOption(2, "2-H Weapon"),
            new SlotTypeOption(3, "Missile Weapon"),
            new SlotTypeOption(4, "Shield"),
            new SlotTypeOption(5, "Body Armor"),
            new SlotTypeOption(6, "Helmet"),
            new SlotTypeOption(7, "Boots"),
            new SlotTypeOption(8, "Misc"),
            new SlotTypeOption(9, "Crown"),
            new SlotTypeOption(10, "Barding"),
        };

        /// <summary>
        /// Gets or sets the selected slot type for ComboBox binding.
        /// </summary>
        public SlotTypeOption SelectedSlotType
        {
            get => SlotTypes.FirstOrDefault(s => s.Value == ItemType) ?? SlotTypes[7]; // Default to Misc (index 7, value 8)
            set
            {
                if (value != null)
                {
                    ItemType = value.Value;
                    OnPropertyChanged(nameof(SelectedSlotType));
                    OnPropertyChanged(nameof(ItemTypeDisplay));
                    OnSlotTypeChanged(); // Refresh equipment options
                }
            }
        }

        public int? MainPath
        {
            get => GetIntProperty(Command.MAINPATH);
            set
            {
                SetIntProperty(Command.MAINPATH, value);
                OnPropertyChanged(nameof(MainPathDisplay));
                OnPropertyChanged(nameof(PrimaryPathLetter));
                OnPropertyChanged(nameof(PrimaryGemCost));
                OnPropertyChanged(nameof(HasGemCost));
            }
        }
        public bool IsMainPathModified => IsIntPropertyModifiedFromVanilla(Command.MAINPATH);
        public bool IsMainPathSessionEdit => IsPropertyEditedInSession(Command.MAINPATH);
        public bool IsMainPathInherited => IsIntPropertyInherited(Command.MAINPATH);

        /// <summary>
        /// Gets the main path display name.
        /// </summary>
        public string MainPathDisplay
        {
            get
            {
                return MainPath switch
                {
                    0 => "Fire",
                    1 => "Air",
                    2 => "Water",
                    3 => "Earth",
                    4 => "Astral",
                    5 => "Death",
                    6 => "Nature",
                    7 => "Glamour",
                    8 => "Blood",
                    _ => MainPath?.ToString() ?? "-"
                };
            }
        }

        public int? MainLevel
        {
            get => GetIntProperty(Command.MAINLEVEL);
            set
            {
                SetIntProperty(Command.MAINLEVEL, value);
                OnPropertyChanged(nameof(PrimaryGemCost));
                OnPropertyChanged(nameof(HasGemCost));
            }
        }
        public bool IsMainLevelModified => IsIntPropertyModifiedFromVanilla(Command.MAINLEVEL);
        public bool IsMainLevelSessionEdit => IsPropertyEditedInSession(Command.MAINLEVEL);
        public bool IsMainLevelInherited => IsIntPropertyInherited(Command.MAINLEVEL);

        public int? SecondaryPath
        {
            get => GetIntProperty(Command.SECONDARYPATH);
            set
            {
                SetIntProperty(Command.SECONDARYPATH, value);
                OnPropertyChanged(nameof(SecondaryPathDisplay));
                OnPropertyChanged(nameof(SecondaryPathLetter));
                OnPropertyChanged(nameof(SecondaryGemCost));
                OnPropertyChanged(nameof(HasGemCost));
            }
        }
        public bool IsSecondaryPathModified => IsIntPropertyModifiedFromVanilla(Command.SECONDARYPATH);
        public bool IsSecondaryPathSessionEdit => IsPropertyEditedInSession(Command.SECONDARYPATH);
        public bool IsSecondaryPathInherited => IsIntPropertyInherited(Command.SECONDARYPATH);

        /// <summary>
        /// Gets the secondary path display name.
        /// </summary>
        public string SecondaryPathDisplay
        {
            get
            {
                if (SecondaryPath == null || SecondaryPath < 0)
                    return "-";
                return SecondaryPath switch
                {
                    0 => "Fire",
                    1 => "Air",
                    2 => "Water",
                    3 => "Earth",
                    4 => "Astral",
                    5 => "Death",
                    6 => "Nature",
                    7 => "Glamour",
                    8 => "Blood",
                    _ => SecondaryPath?.ToString() ?? "-"
                };
            }
        }

        public int? SecondaryLevel
        {
            get => GetIntProperty(Command.SECONDARYLEVEL);
            set
            {
                SetIntProperty(Command.SECONDARYLEVEL, value);
                OnPropertyChanged(nameof(SecondaryGemCost));
                OnPropertyChanged(nameof(HasGemCost));
            }
        }
        public bool IsSecondaryLevelModified => IsIntPropertyModifiedFromVanilla(Command.SECONDARYLEVEL);
        public bool IsSecondaryLevelSessionEdit => IsPropertyEditedInSession(Command.SECONDARYLEVEL);
        public bool IsSecondaryLevelInherited => IsIntPropertyInherited(Command.SECONDARYLEVEL);

        /// <summary>
        /// Refreshes the path display properties after path selection changes.
        /// </summary>
        public void RefreshPathDisplay()
        {
            OnPropertyChanged(nameof(MainPathDisplay));
            OnPropertyChanged(nameof(SecondaryPathDisplay));
            OnPropertyChanged(nameof(PrimaryGemCost));
            OnPropertyChanged(nameof(SecondaryGemCost));
            OnPropertyChanged(nameof(HasGemCost));
        }

        // ========================================
        // Item Cost Modifiers
        // ========================================

        public int? ItemCost1
        {
            get => GetIntProperty(Command.ITEMCOST1);
            set
            {
                SetIntProperty(Command.ITEMCOST1, value);
                OnPropertyChanged(nameof(PrimaryGemCost));
            }
        }

        public int? ItemCost2
        {
            get => GetIntProperty(Command.ITEMCOST2);
            set
            {
                SetIntProperty(Command.ITEMCOST2, value);
                OnPropertyChanged(nameof(SecondaryGemCost));
            }
        }

        // ========================================
        // Gem Cost Calculation
        // ========================================

        /// <summary>
        /// Calculates gem cost: 5 * level, reduced by itemcost percentage, rounded up.
        /// itemcost values are negative (e.g., -60 means 60% off).
        /// </summary>
        private int CalculateGemCost(int level, int? itemCost)
        {
            if (level <= 0) return 0;
            int baseCost = 5 * level;
            // itemcost is negative (e.g., -60 = 60% reduction), so we use absolute value
            int reductionPercent = Math.Abs(itemCost ?? 0);
            // Apply percentage reduction and round up
            double adjustedCost = baseCost * (100.0 - reductionPercent) / 100.0;
            return (int)Math.Ceiling(adjustedCost);
        }

        /// <summary>
        /// Gets the primary path gem cost.
        /// </summary>
        public int PrimaryGemCost => CalculateGemCost(MainLevel ?? 0, ItemCost1);

        /// <summary>
        /// Gets the secondary path gem cost.
        /// </summary>
        public int SecondaryGemCost => CalculateGemCost(SecondaryLevel ?? 0, ItemCost2);

        /// <summary>
        /// Whether this item has any gem cost to display.
        /// </summary>
        public bool HasGemCost => (MainPath >= 0 && (MainLevel ?? 0) > 0) ||
                                   (SecondaryPath >= 0 && (SecondaryLevel ?? 0) > 0);

        /// <summary>
        /// Gets the primary path letter for icon lookup (F, A, W, E, S, D, N, G, B).
        /// Returns null if no path selected or level is 0.
        /// </summary>
        public string PrimaryPathLetter =>
            (MainPath >= 0 && (MainLevel ?? 0) > 0) ? GetPathLetter(MainPath ?? -1) : null;

        /// <summary>
        /// Gets the secondary path letter for icon lookup.
        /// Returns null if no path selected or level is 0.
        /// </summary>
        public string SecondaryPathLetter =>
            (SecondaryPath >= 0 && (SecondaryLevel ?? 0) > 0) ? GetPathLetter(SecondaryPath ?? -1) : null;

        private static string GetPathLetter(int pathId)
        {
            return pathId switch
            {
                0 => "F",
                1 => "A",
                2 => "W",
                3 => "E",
                4 => "S",
                5 => "D",
                6 => "N",
                7 => "G",
                8 => "B",
                _ => null
            };
        }

        // ========================================
        // Combat Stat Bonuses
        // ========================================

        public int? HP
        {
            get => GetIntProperty(Command.HP);
            set => SetIntProperty(Command.HP, value);
        }
        public bool IsHPModified => IsIntPropertyModifiedFromVanilla(Command.HP);
        public bool IsHPSessionEdit => IsPropertyEditedInSession(Command.HP);
        public bool IsHPInherited => IsIntPropertyInherited(Command.HP);

        public int? Strength
        {
            get => GetIntProperty(Command.STR);
            set => SetIntProperty(Command.STR, value);
        }
        public bool IsStrengthModified => IsIntPropertyModifiedFromVanilla(Command.STR);
        public bool IsStrengthSessionEdit => IsPropertyEditedInSession(Command.STR);
        public bool IsStrengthInherited => IsIntPropertyInherited(Command.STR);

        public int? Attack
        {
            get => GetIntProperty(Command.ATT);
            set => SetIntProperty(Command.ATT, value);
        }
        public bool IsAttackModified => IsIntPropertyModifiedFromVanilla(Command.ATT);
        public bool IsAttackSessionEdit => IsPropertyEditedInSession(Command.ATT);
        public bool IsAttackInherited => IsIntPropertyInherited(Command.ATT);

        public int? Defense
        {
            get => GetIntProperty(Command.DEF);
            set => SetIntProperty(Command.DEF, value);
        }
        public bool IsDefenseModified => IsIntPropertyModifiedFromVanilla(Command.DEF);
        public bool IsDefenseSessionEdit => IsPropertyEditedInSession(Command.DEF);
        public bool IsDefenseInherited => IsIntPropertyInherited(Command.DEF);

        public int? Precision
        {
            get => GetIntProperty(Command.PREC);
            set => SetIntProperty(Command.PREC, value);
        }
        public bool IsPrecisionModified => IsIntPropertyModifiedFromVanilla(Command.PREC);
        public bool IsPrecisionSessionEdit => IsPropertyEditedInSession(Command.PREC);
        public bool IsPrecisionInherited => IsIntPropertyInherited(Command.PREC);

        public int? MagicResistance
        {
            get => GetIntProperty(Command.MR);
            set => SetIntProperty(Command.MR, value);
        }
        public bool IsMagicResistanceModified => IsIntPropertyModifiedFromVanilla(Command.MR);
        public bool IsMagicResistanceSessionEdit => IsPropertyEditedInSession(Command.MR);
        public bool IsMagicResistanceInherited => IsIntPropertyInherited(Command.MR);

        public int? Morale
        {
            get => GetIntProperty(Command.MORALE);
            set => SetIntProperty(Command.MORALE, value);
        }
        public bool IsMoraleModified => IsIntPropertyModifiedFromVanilla(Command.MORALE);
        public bool IsMoraleSessionEdit => IsPropertyEditedInSession(Command.MORALE);
        public bool IsMoraleInherited => IsIntPropertyInherited(Command.MORALE);

        /// <summary>
        /// Returns true if any combat stat bonus is present.
        /// </summary>
        public bool HasCombatStats =>
            HP != null || Strength != null || Attack != null ||
            Defense != null || Precision != null || MagicResistance != null || Morale != null;

        // ========================================
        // Equipment References (Weapon/Armor the item provides)
        // ========================================

        /// <summary>
        /// Gets the weapon reference display string.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public string WeaponDisplay
        {
            get
            {
                var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                if (prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if item provides a weapon.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public bool HasWeapon => HasReferenceProperty<WeaponRef>(Command.WEAPON);

        /// <summary>
        /// Gets the armor reference display string.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public string ArmorDisplay
        {
            get
            {
                var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                if (prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if item provides armor.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public bool HasArmor => HasReferenceProperty<ArmorRef>(Command.ARMOR);

        /// <summary>
        /// Returns true if item provides any equipment (weapon or armor).
        /// </summary>
        public bool HasEquipment => HasWeapon || HasArmor;

        // ========================================
        // Equipment Type Based on Slot
        // ========================================

        /// <summary>
        /// Returns true if this slot type uses weapon equipment.
        /// Weapon slots: 1-H Weapon (1), 2-H Weapon (2), Missile Weapon (3), Boots (7), Misc (8)
        /// </summary>
        public bool UsesWeaponEquipment => ItemType == 1 || ItemType == 2 || ItemType == 3 || ItemType == 7 || ItemType == 8;

        /// <summary>
        /// Returns true if this slot type uses armor equipment.
        /// Armor slots: Shield (4), Body Armor (5), Helmet (6), Crown (9), Barding (10)
        /// </summary>
        public bool UsesArmorEquipment => ItemType == 4 || ItemType == 5 || ItemType == 6 || ItemType == 9 || ItemType == 10;

        /// <summary>
        /// Gets the equipment type label based on slot type.
        /// </summary>
        public string EquipmentTypeLabel => UsesArmorEquipment ? "ARMOR" : "WEAPON";

        /// <summary>
        /// Gets the display string for the current equipment (weapon or armor).
        /// </summary>
        public string EquipmentDisplay => UsesArmorEquipment ? ArmorDisplay : WeaponDisplay;

        /// <summary>
        /// Gets the ID of the current equipment (weapon or armor).
        /// </summary>
        public int? EquipmentId
        {
            get
            {
                if (UsesArmorEquipment)
                {
                    var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                    return prop?.ID;
                }
                else
                {
                    var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                    return prop?.ID;
                }
            }
        }

        /// <summary>
        /// Available equipment items based on slot type.
        /// Uses centralized caches with filtering for weapon/armor based on slot.
        /// </summary>
        private List<AvailableEquipmentItem> _availableEquipment;
        public List<AvailableEquipmentItem> AvailableEquipment
        {
            get
            {
                if (_availableEquipment == null)
                    RefreshAvailableEquipment();
                return _availableEquipment;
            }
        }

        private void RefreshAvailableEquipment()
        {
            _availableEquipment = new List<AvailableEquipmentItem>();

            // Add "None" option first
            _availableEquipment.Add(new AvailableEquipmentItem { ID = 0, Name = "(None)", Source = "" });

            // Use cached lists from MainWindowViewModel for performance
            var sourceList = UsesArmorEquipment ? CachedArmors : CachedWeapons;
            _availableEquipment.AddRange(sourceList);

            OnPropertyChanged(nameof(AvailableEquipment));
        }

        /// <summary>
        /// Gets or sets the selected equipment from the dropdown.
        /// </summary>
        public AvailableEquipmentItem SelectedEquipment
        {
            get
            {
                var id = EquipmentId;
                if (id == null || id == 0)
                    return AvailableEquipment.FirstOrDefault(e => e.ID == 0);
                return AvailableEquipment.FirstOrDefault(e => e.ID == id) ?? AvailableEquipment.FirstOrDefault();
            }
            set
            {
                if (value == null) return;

                if (UsesArmorEquipment)
                {
                    ArmorId = value.ID == 0 ? "" : value.ID.ToString();
                }
                else
                {
                    WeaponId = value.ID == 0 ? "" : value.ID.ToString();
                }

                OnPropertyChanged(nameof(SelectedEquipment));
                OnPropertyChanged(nameof(EquipmentDisplay));
                OnPropertyChanged(nameof(EquipmentId));
            }
        }

        /// <summary>
        /// Called when slot type changes - clear incompatible equipment and refresh list.
        /// </summary>
        private void OnSlotTypeChanged()
        {
            // Clear incompatible equipment when slot type changes
            if (UsesArmorEquipment && HasWeapon)
            {
                // Switching to armor slot - clear weapon
                WeaponId = "";
            }
            else if (UsesWeaponEquipment && HasArmor)
            {
                // Switching to weapon slot - clear armor
                ArmorId = "";
            }

            _availableEquipment = null; // Force refresh
            OnPropertyChanged(nameof(UsesWeaponEquipment));
            OnPropertyChanged(nameof(UsesArmorEquipment));
            OnPropertyChanged(nameof(EquipmentTypeLabel));
            OnPropertyChanged(nameof(AvailableEquipment));
            OnPropertyChanged(nameof(SelectedEquipment));
            OnPropertyChanged(nameof(EquipmentDisplay));
            OnPropertyChanged(nameof(EquipmentId));
            OnPropertyChanged(nameof(HasWeapon));
            OnPropertyChanged(nameof(HasArmor));
            OnPropertyChanged(nameof(HasEquipment));
        }

        // ========================================
        // Equipment ID Setters (for changing references)
        // ========================================

        /// <summary>
        /// Gets or sets the weapon ID as a string for TextBox binding.
        /// Setting to empty/null removes the weapon reference.
        /// </summary>
        public string WeaponId
        {
            get
            {
                var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                return prop?.ID.ToString();
            }
            set
            {
                // Empty/null or "0" clears the weapon reference (#weapon 0 clears)
                if (string.IsNullOrWhiteSpace(value) || value == "0")
                {
                    // Remove the weapon reference
                    var result = _entity.TryGet<WeaponRef>(Command.WEAPON, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existingProp, "Remove weapon");
                        _history.Execute(cmd);
                    }
                }
                else if (int.TryParse(value, out int id) && id > 0)
                {
                    // Remove existing if present first
                    var result = _entity.TryGet<WeaponRef>(Command.WEAPON, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var removeCmd = new RemovePropertyCommand(_entity, existingProp, "Remove weapon");
                        _history.Execute(removeCmd);
                    }

                    // Create a new WeaponRef with the given ID (Parent must be set first)
                    var newRef = new WeaponRef { Parent = _entity, Command = Command.WEAPON };
                    newRef.ID = id;
                    newRef.Resolve();

                    if (_history != null)
                    {
                        var addCmd = new AddPropertyCommand(_entity, newRef, $"Set weapon #{id}");
                        _history.Execute(addCmd);
                    }
                }
                OnPropertyChanged(nameof(WeaponId));
                OnPropertyChanged(nameof(WeaponDisplay));
                OnPropertyChanged(nameof(HasWeapon));
                OnPropertyChanged(nameof(HasEquipment));
                OnPropertyChanged(nameof(WeaponDamage));
                OnPropertyChanged(nameof(WeaponAttack));
                OnPropertyChanged(nameof(WeaponDefense));
                OnPropertyChanged(nameof(WeaponLength));
                OnPropertyChanged(nameof(WeaponNrAtt));
            }
        }

        /// <summary>
        /// Gets or sets the armor ID as a string for TextBox binding.
        /// Setting to empty/null or 0 removes the armor reference.
        /// </summary>
        public string ArmorId
        {
            get
            {
                var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                return prop?.ID.ToString();
            }
            set
            {
                // Empty/null or "0" clears the armor reference (#armor 0 clears)
                if (string.IsNullOrWhiteSpace(value) || value == "0")
                {
                    // Remove the armor reference
                    var result = _entity.TryGet<ArmorRef>(Command.ARMOR, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existingProp, "Remove armor");
                        _history.Execute(cmd);
                    }
                }
                else if (int.TryParse(value, out int id) && id > 0)
                {
                    // Remove existing if present first
                    var result = _entity.TryGet<ArmorRef>(Command.ARMOR, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var removeCmd = new RemovePropertyCommand(_entity, existingProp, "Remove armor");
                        _history.Execute(removeCmd);
                    }

                    // Create a new ArmorRef with the given ID (Parent must be set first)
                    var newRef = new ArmorRef { Parent = _entity, Command = Command.ARMOR };
                    newRef.ID = id;
                    newRef.Resolve();

                    if (_history != null)
                    {
                        var addCmd = new AddPropertyCommand(_entity, newRef, $"Set armor #{id}");
                        _history.Execute(addCmd);
                    }
                }
                OnPropertyChanged(nameof(ArmorId));
                OnPropertyChanged(nameof(ArmorDisplay));
                OnPropertyChanged(nameof(HasArmor));
                OnPropertyChanged(nameof(HasEquipment));
                OnPropertyChanged(nameof(ArmorProtection));
                OnPropertyChanged(nameof(ArmorDefense));
                OnPropertyChanged(nameof(ArmorEncumbrance));
            }
        }

        // ========================================
        // Referenced Weapon Stats (Read-Only)
        // ========================================

        /// <summary>
        /// Gets the referenced weapon entity (if any).
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        private Weapon ReferencedWeapon
        {
            get
            {
                var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                if (prop?.Entity is Weapon weapon)
                    return weapon;
                return null;
            }
        }

        public int? WeaponDamage => GetWeaponIntProperty(Command.DMG);
        public int? WeaponAttack => GetWeaponIntProperty(Command.ATT);
        public int? WeaponDefense => GetWeaponIntProperty(Command.DEF);
        public int? WeaponLength => GetWeaponIntProperty(Command.LEN);
        public int? WeaponNrAtt => GetWeaponIntProperty(Command.NRATT);

        // Weapon Damage Types (flags)
        public bool WeaponIsPierce => GetWeaponFlag(Command.PIERCE);
        public bool WeaponIsSlash => GetWeaponFlag(Command.SLASH);
        public bool WeaponIsBlunt => GetWeaponFlag(Command.BLUNT);
        public bool WeaponIsFire => GetWeaponFlag(Command.FIRE);
        public bool WeaponIsCold => GetWeaponFlag(Command.COLD);
        public bool WeaponIsShock => GetWeaponFlag(Command.SHOCK);
        public bool WeaponIsPoison => GetWeaponFlag(Command.POISON);
        public bool WeaponIsAcid => GetWeaponFlag(Command.ACID);
        public bool WeaponIsMagic => GetWeaponFlag(Command.MAGIC);

        // Weapon Special Properties
        public bool WeaponIsArmorNegating => GetWeaponFlag(Command.ARMORNEGATING);
        public bool WeaponIsArmorPiercing => GetWeaponFlag(Command.ARMORPIERCING);
        public bool WeaponIsTwoHanded => GetWeaponFlag(Command.TWOHANDED);
        public bool WeaponIsFlail => GetWeaponFlag(Command.FLAIL);
        public bool WeaponIsNatural => GetWeaponFlag(Command.NATURAL);

        /// <summary>
        /// Gets a formatted string of weapon damage types.
        /// </summary>
        public string WeaponDamageTypes
        {
            get
            {
                var types = new List<string>();
                if (WeaponIsPierce) types.Add("Pierce");
                if (WeaponIsSlash) types.Add("Slash");
                if (WeaponIsBlunt) types.Add("Blunt");
                if (WeaponIsFire) types.Add("Fire");
                if (WeaponIsCold) types.Add("Cold");
                if (WeaponIsShock) types.Add("Shock");
                if (WeaponIsPoison) types.Add("Poison");
                if (WeaponIsAcid) types.Add("Acid");
                if (WeaponIsMagic) types.Add("Magic");
                return types.Count > 0 ? string.Join(", ", types) : null;
            }
        }

        /// <summary>
        /// Gets a formatted string of weapon special properties.
        /// </summary>
        public string WeaponSpecialProperties
        {
            get
            {
                var props = new List<string>();
                if (WeaponIsArmorNegating) props.Add("Armor Negating");
                if (WeaponIsArmorPiercing) props.Add("Armor Piercing");
                if (WeaponIsTwoHanded) props.Add("Two-Handed");
                if (WeaponIsFlail) props.Add("Flail");
                if (WeaponIsNatural) props.Add("Natural");
                return props.Count > 0 ? string.Join(", ", props) : null;
            }
        }

        // Secondary Effect
        public bool HasWeaponSecondaryEffect => GetWeaponSecondaryEffectId() != null;

        public string WeaponSecondaryEffectDisplay
        {
            get
            {
                var weapon = ReferencedWeapon;
                if (weapon == null) return null;

                // Check secondaryeffectalways first (100% chance)
                var alwaysResult = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
                if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp != null)
                {
                    var name = alwaysProp.Entity?.Name ?? $"#{alwaysProp.ID}";
                    return $"{name} (100%)";
                }

                // Check secondaryeffect (25% chance)
                var result = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                {
                    var name = prop.Entity?.Name ?? $"#{prop.ID}";
                    return $"{name} (25%)";
                }

                return null;
            }
        }

        private int? GetWeaponSecondaryEffectId()
        {
            var weapon = ReferencedWeapon;
            if (weapon == null) return null;

            var alwaysResult = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
            if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp != null)
                return alwaysProp.ID;

            var result = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
            if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                return prop.ID;

            return null;
        }

        private bool GetWeaponFlag(Command command)
        {
            var weapon = ReferencedWeapon;
            if (weapon == null) return false;

            // Flags are stored as CommandProperty (no value) or IntProperty with value
            var cmdResult = weapon.TryGet<CommandProperty>(command, out _);
            if (cmdResult == ReturnType.TRUE || cmdResult == ReturnType.COPIED)
                return true;

            var intResult = weapon.TryGet<IntProperty>(command, out var intProp);
            if ((intResult == ReturnType.TRUE || intResult == ReturnType.COPIED) && intProp?.Value != 0)
                return true;

            return false;
        }

        private int? GetWeaponIntProperty(Command command)
        {
            var weapon = ReferencedWeapon;
            if (weapon == null) return null;

            // Special handling for DMG which is stored as StringProperty
            if (command == Command.DMG)
            {
                var result = weapon.TryGet<StringProperty>(command, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                {
                    if (int.TryParse(prop.Value, out int val))
                        return val;
                }
                return null;
            }

            var intResult = weapon.TryGet<IntProperty>(command, out var intProp);
            if (intResult == ReturnType.TRUE || intResult == ReturnType.COPIED)
                return intProp?.Value;
            return null;
        }

        // ========================================
        // Referenced Armor Stats (Read-Only)
        // ========================================

        /// <summary>
        /// Gets the referenced armor entity (if any).
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        private Armor ReferencedArmor
        {
            get
            {
                var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                if (prop?.Entity is Armor armor)
                    return armor;
                return null;
            }
        }

        public int? ArmorProtection => GetArmorIntProperty(Command.PROT);
        public int? ArmorDefense => GetArmorIntProperty(Command.DEF);
        public int? ArmorEncumbrance => GetArmorIntProperty(Command.ENC);

        private int? GetArmorIntProperty(Command command)
        {
            var armor = ReferencedArmor;
            if (armor == null) return null;

            var result = armor.TryGet<IntProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                return prop?.Value;
            return null;
        }

        // ========================================
        // Badge Collection (Unified)
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.CONSTLEVEL => "ConstLevel",
                Command.TYPE => "ItemType",
                Command.MAINPATH => "MainPath",
                Command.MAINLEVEL => "MainLevel",
                Command.SECONDARYPATH => "SecondaryPath",
                Command.SECONDARYLEVEL => "SecondaryLevel",
                Command.HP => "HP",
                Command.STR => "Strength",
                Command.ATT => "Attack",
                Command.DEF => "Defense",
                Command.PREC => "Precision",
                Command.MR => "MagicResistance",
                Command.MORALE => "Morale",
                _ => null
            };
        }
    }

    /// <summary>
    /// ViewModel for Site entities.
    /// </summary>
    public class SiteViewModel : EntityViewModel
    {
        public SiteViewModel(Site entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Site Site => (Site)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from site_badges.json.
        /// </summary>
        protected override string EntityTypeName => "site";

        // ========================================
        // Copy From Support (#copysite)
        // ========================================

        /// <summary>
        /// Gets or sets the CopySite reference ID.
        /// </summary>
        public int? CopySiteId
        {
            get
            {
                var result = _entity.TryGet<SiteRef>(Command.COPYSITE, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                    return prop.ID;
                return null;
            }
            set
            {
                if (value == null)
                {
                    // Remove the copysite reference
                    _entity.RemoveProperty(Command.COPYSITE);
                }
                else
                {
                    // Set the copysite reference using the Set<T> method
                    _entity.Set<SiteRef>(Command.COPYSITE, p => p.Parse(Command.COPYSITE, value.Value.ToString(), ""));
                }
                OnPropertyChanged(nameof(CopySiteId));
                OnPropertyChanged(nameof(CopySiteName));
            }
        }

        /// <summary>
        /// Gets the CopySite reference name for display.
        /// </summary>
        public string CopySiteName
        {
            get
            {
                var result = _entity.TryGet<SiteRef>(Command.COPYSITE, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"Site #{idEntity.ID}";
                    return prop.Name ?? $"Site #{prop.ID}";
                }
                return null;
            }
        }

        // ========================================
        // Sprite Support (#look command)
        // ========================================

        public int? LookSprite
        {
            get => GetIntProperty(Command.LOOK);
            set => SetIntProperty(Command.LOOK, value);
        }
        public bool IsLookSpriteModified => IsIntPropertyModifiedFromVanilla(Command.LOOK);
        public bool IsLookSpriteSessionEdit => IsPropertyEditedInSession(Command.LOOK);

        /// <summary>
        /// Gets the site sprite image if available.
        /// Sites use #look to reference a sprite number.
        /// </summary>
        public System.Windows.Media.ImageSource SpriteImage
        {
            get
            {
                // Site sprites would need to be loaded from game data
                // For now, return null - can be implemented when sprite loading is added
                return null;
            }
        }

        // ========================================
        // Core Stats
        // ========================================

        public int? Path
        {
            get => GetIntProperty(Command.PATH);
            set => SetIntProperty(Command.PATH, value);
        }
        public bool IsPathModified => IsIntPropertyModifiedFromVanilla(Command.PATH);
        public bool IsPathSessionEdit => IsPropertyEditedInSession(Command.PATH);
        public bool IsPathInherited => IsIntPropertyInherited(Command.PATH);

        /// <summary>
        /// Gets the path display name.
        /// </summary>
        public string PathDisplay
        {
            get
            {
                return Path switch
                {
                    0 => "Fire",
                    1 => "Air",
                    2 => "Water",
                    3 => "Earth",
                    4 => "Astral",
                    5 => "Death",
                    6 => "Nature",
                    7 => "Blood",
                    8 => "Holy",
                    _ => Path?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Gets the path letter for icon lookup (F, A, W, E, S, D, N, B, H).
        /// </summary>
        public string PathLetter
        {
            get
            {
                return Path switch
                {
                    0 => "F",
                    1 => "A",
                    2 => "W",
                    3 => "E",
                    4 => "S",
                    5 => "D",
                    6 => "N",
                    7 => "B",
                    8 => "H",
                    _ => null
                };
            }
        }

        public int? Level
        {
            get => GetIntProperty(Command.LEVEL);
            set => SetIntProperty(Command.LEVEL, value);
        }
        public bool IsLevelModified => IsIntPropertyModifiedFromVanilla(Command.LEVEL);
        public bool IsLevelSessionEdit => IsPropertyEditedInSession(Command.LEVEL);
        public bool IsLevelInherited => IsIntPropertyInherited(Command.LEVEL);

        public int? Rarity
        {
            get => GetIntProperty(Command.RARITY);
            set => SetIntProperty(Command.RARITY, value);
        }
        public bool IsRarityModified => IsIntPropertyModifiedFromVanilla(Command.RARITY);
        public bool IsRaritySessionEdit => IsPropertyEditedInSession(Command.RARITY);
        public bool IsRarityInherited => IsIntPropertyInherited(Command.RARITY);

        /// <summary>
        /// Gets the rarity display name.
        /// </summary>
        public string RarityDisplay
        {
            get
            {
                return Rarity switch
                {
                    0 => "Common",
                    1 => "Uncommon",
                    2 => "Rare",
                    5 => "Never Random",
                    _ => Rarity?.ToString() ?? "-"
                };
            }
        }

        // ========================================
        // Gems (IntIntProperty)
        // ========================================

        /// <summary>
        /// Gets the gem income display text.
        /// #gems takes two parameters: gem type and amount.
        /// </summary>
        public string GemsDisplay
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    var gemType = prop.Value1 switch
                    {
                        0 => "Fire",
                        1 => "Air",
                        2 => "Water",
                        3 => "Earth",
                        4 => "Astral",
                        5 => "Death",
                        6 => "Nature",
                        7 => "Blood",
                        _ => $"Type {prop.Value1}"
                    };
                    return $"{prop.Value2} {gemType}";
                }
                return null;
            }
        }

        public bool HasGems
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out _);
                return result == ReturnType.TRUE;
            }
        }

        /// <summary>
        /// Gets the gem type letter for icon lookup (F, A, W, E, S, D, N, B).
        /// Returns null if no gems defined.
        /// </summary>
        public string GemLetter
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.Value1 switch
                    {
                        0 => "F",
                        1 => "A",
                        2 => "W",
                        3 => "E",
                        4 => "S",
                        5 => "D",
                        6 => "N",
                        7 => "B",
                        _ => null
                    };
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the gem amount.
        /// </summary>
        public int? GemAmount
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                    return prop.Value2;
                return null;
            }
        }

        /// <summary>
        /// Gets the gem type display name (Fire, Air, etc.).
        /// </summary>
        public string GemPathDisplay
        {
            get
            {
                var result = _entity.TryGet<IntIntProperty>(Command.GEMS, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return prop.Value1 switch
                    {
                        0 => "Fire",
                        1 => "Air",
                        2 => "Water",
                        3 => "Earth",
                        4 => "Astral",
                        5 => "Death",
                        6 => "Nature",
                        7 => "Blood",
                        _ => $"Type {prop.Value1}"
                    };
                }
                return null;
            }
        }

        // ========================================
        // Single Unified Badge Collection
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;
        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Inherited");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.PATH => "Path",
                Command.LEVEL => "Level",
                Command.RARITY => "Rarity",
                Command.GOLD => "Gold",
                Command.RES => "Research",
                Command.SUPPLY => "Supply",
                Command.LOOK => "LookSprite",
                Command.COPYSITE => "CopySite",
                _ => null
            };
        }
    }

    /// <summary>
    /// ViewModel for Nation entities.
    /// </summary>
    public class NationViewModel : EntityViewModel
    {
        public NationViewModel(Nation entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Nation Nation => (Nation)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from nation_badges.json.
        /// </summary>
        protected override string EntityTypeName => "nation";

        // ========================================
        // Core Identity Properties
        // ========================================

        /// <summary>
        /// Gets or sets the nation era (1=EA, 2=MA, 3=LA).
        /// </summary>
        public int? Era
        {
            get => GetIntProperty(Command.ERA);
            set => SetIntProperty(Command.ERA, value);
        }
        public bool IsEraModified => IsIntPropertyModifiedFromVanilla(Command.ERA);
        public bool IsEraSessionEdit => IsPropertyEditedInSession(Command.ERA);

        /// <summary>
        /// Gets the era display name.
        /// </summary>
        public string EraDisplay
        {
            get
            {
                return Era switch
                {
                    1 => "Early Age",
                    2 => "Middle Age",
                    3 => "Late Age",
                    _ => Era?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Gets or sets the nation epithet (subtitle).
        /// </summary>
        public string Epithet
        {
            get => GetStringProperty(Command.EPITHET);
            set => SetStringProperty(Command.EPITHET, value);
        }
        public bool IsEpithetModified => IsStringPropertyModifiedFromVanilla(Command.EPITHET);
        public bool IsEpithetSessionEdit => IsPropertyEditedInSession(Command.EPITHET);

        /// <summary>
        /// Gets or sets the nation description.
        /// </summary>
        public string Description
        {
            get => GetStringProperty(Command.DESCR);
            set => SetStringProperty(Command.DESCR, value);
        }
        public bool IsDescriptionModified => IsStringPropertyModifiedFromVanilla(Command.DESCR);
        public bool IsDescriptionSessionEdit => IsPropertyEditedInSession(Command.DESCR);

        /// <summary>
        /// Gets or sets the flag file path.
        /// </summary>
        public string FlagPath
        {
            get
            {
                var result = _entity.TryGet<FilePathProperty>(Command.FLAG, out var prop);
                if (result == ReturnType.TRUE && prop != null)
                    return prop.Value;
                return null;
            }
        }

        // ========================================
        // Data Availability Indicators
        // ========================================

        /// <summary>
        /// Returns true if this is a vanilla nation (may have incomplete data).
        /// </summary>
        public bool IsVanillaNation => Source == EntitySource.Vanilla;

        /// <summary>
        /// Returns true if this is a mod-defined nation (complete data available).
        /// </summary>
        public bool IsModNation => Source == EntitySource.FromMod || Source == EntitySource.VanillaModified;

        /// <summary>
        /// Gets a message about data availability for vanilla nations.
        /// </summary>
        public string DataAvailabilityMessage
        {
            get
            {
                if (IsVanillaNation)
                    return "Some data may not be displayed. Vanilla nation data is incomplete - only mod-defined properties can be shown.";
                return null;
            }
        }

        /// <summary>
        /// Returns true if the data availability warning should be shown.
        /// </summary>
        public bool ShowDataWarning => IsVanillaNation;

        // ========================================
        // Badge Collections - Identity
        // ========================================

        private ObservableCollection<PropertyItem> _identityBadges;
        private ObservableCollection<AvailablePropertyItem> _availableIdentityBadges;

        public ObservableCollection<PropertyItem> IdentityBadges
        {
            get { if (_identityBadges == null) RefreshIdentityBadges(); return _identityBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableIdentityBadges
        {
            get { if (_availableIdentityBadges == null) RefreshIdentityBadges(); return _availableIdentityBadges; }
        }

        private RelayCommand<PropertyItem> _removeIdentityBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addIdentityBadgeCommand;
        public RelayCommand<PropertyItem> RemoveIdentityBadgeCommand => _removeIdentityBadgeCommand ??= CreateRemoveBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<AvailablePropertyItem> AddIdentityBadgeCommand => _addIdentityBadgeCommand ??= CreateAddBadgeCommand(RefreshIdentityBadges);

        private void RefreshIdentityBadges()
        {
            var (active, available) = BuildBadgesFromSection("identity", BadgeValueChangedHandler);
            _identityBadges = active;
            _availableIdentityBadges = available;
            OnPropertyChanged(nameof(IdentityBadges));
            OnPropertyChanged(nameof(AvailableIdentityBadges));
        }

        // ========================================
        // Badge Collections - Recruitment
        // ========================================

        private ObservableCollection<PropertyItem> _recruitmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableRecruitmentBadges;

        public ObservableCollection<PropertyItem> RecruitmentBadges
        {
            get { if (_recruitmentBadges == null) RefreshRecruitmentBadges(); return _recruitmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableRecruitmentBadges
        {
            get { if (_availableRecruitmentBadges == null) RefreshRecruitmentBadges(); return _availableRecruitmentBadges; }
        }

        private RelayCommand<PropertyItem> _removeRecruitmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addRecruitmentBadgeCommand;
        public RelayCommand<PropertyItem> RemoveRecruitmentBadgeCommand => _removeRecruitmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshRecruitmentBadges);
        public RelayCommand<AvailablePropertyItem> AddRecruitmentBadgeCommand => _addRecruitmentBadgeCommand ??= CreateAddBadgeCommand(RefreshRecruitmentBadges);

        private void RefreshRecruitmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("recruitment", BadgeValueChangedHandler);
            _recruitmentBadges = active;
            _availableRecruitmentBadges = available;
            OnPropertyChanged(nameof(RecruitmentBadges));
            OnPropertyChanged(nameof(AvailableRecruitmentBadges));
        }

        // ========================================
        // Badge Collections - Terrain Recruitment
        // ========================================

        private ObservableCollection<PropertyItem> _terrainRecruitmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableTerrainRecruitmentBadges;

        public ObservableCollection<PropertyItem> TerrainRecruitmentBadges
        {
            get { if (_terrainRecruitmentBadges == null) RefreshTerrainRecruitmentBadges(); return _terrainRecruitmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableTerrainRecruitmentBadges
        {
            get { if (_availableTerrainRecruitmentBadges == null) RefreshTerrainRecruitmentBadges(); return _availableTerrainRecruitmentBadges; }
        }

        private RelayCommand<PropertyItem> _removeTerrainRecruitmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addTerrainRecruitmentBadgeCommand;
        public RelayCommand<PropertyItem> RemoveTerrainRecruitmentBadgeCommand => _removeTerrainRecruitmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshTerrainRecruitmentBadges);
        public RelayCommand<AvailablePropertyItem> AddTerrainRecruitmentBadgeCommand => _addTerrainRecruitmentBadgeCommand ??= CreateAddBadgeCommand(RefreshTerrainRecruitmentBadges);

        private void RefreshTerrainRecruitmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("terrain_recruitment", BadgeValueChangedHandler);
            _terrainRecruitmentBadges = active;
            _availableTerrainRecruitmentBadges = available;
            OnPropertyChanged(nameof(TerrainRecruitmentBadges));
            OnPropertyChanged(nameof(AvailableTerrainRecruitmentBadges));
        }

        // ========================================
        // Badge Collections - Coastal/UW
        // ========================================

        private ObservableCollection<PropertyItem> _coastalUwBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCoastalUwBadges;

        public ObservableCollection<PropertyItem> CoastalUwBadges
        {
            get { if (_coastalUwBadges == null) RefreshCoastalUwBadges(); return _coastalUwBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCoastalUwBadges
        {
            get { if (_availableCoastalUwBadges == null) RefreshCoastalUwBadges(); return _availableCoastalUwBadges; }
        }

        private RelayCommand<PropertyItem> _removeCoastalUwBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCoastalUwBadgeCommand;
        public RelayCommand<PropertyItem> RemoveCoastalUwBadgeCommand => _removeCoastalUwBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCoastalUwBadges);
        public RelayCommand<AvailablePropertyItem> AddCoastalUwBadgeCommand => _addCoastalUwBadgeCommand ??= CreateAddBadgeCommand(RefreshCoastalUwBadges);

        private void RefreshCoastalUwBadges()
        {
            var (active, available) = BuildBadgesFromSection("coastal_uw", BadgeValueChangedHandler);
            _coastalUwBadges = active;
            _availableCoastalUwBadges = available;
            OnPropertyChanged(nameof(CoastalUwBadges));
            OnPropertyChanged(nameof(AvailableCoastalUwBadges));
        }

        // ========================================
        // Badge Collections - Heroes
        // ========================================

        private ObservableCollection<PropertyItem> _heroesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableHeroesBadges;

        public ObservableCollection<PropertyItem> HeroesBadges
        {
            get { if (_heroesBadges == null) RefreshHeroesBadges(); return _heroesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableHeroesBadges
        {
            get { if (_availableHeroesBadges == null) RefreshHeroesBadges(); return _availableHeroesBadges; }
        }

        private RelayCommand<PropertyItem> _removeHeroesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addHeroesBadgeCommand;
        public RelayCommand<PropertyItem> RemoveHeroesBadgeCommand => _removeHeroesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshHeroesBadges);
        public RelayCommand<AvailablePropertyItem> AddHeroesBadgeCommand => _addHeroesBadgeCommand ??= CreateAddBadgeCommand(RefreshHeroesBadges);

        private void RefreshHeroesBadges()
        {
            var (active, available) = BuildBadgesFromSection("heroes", BadgeValueChangedHandler);
            _heroesBadges = active;
            _availableHeroesBadges = available;
            OnPropertyChanged(nameof(HeroesBadges));
            OnPropertyChanged(nameof(AvailableHeroesBadges));
        }

        // ========================================
        // Badge Collections - Starting Units
        // ========================================

        private ObservableCollection<PropertyItem> _startingBadges;
        private ObservableCollection<AvailablePropertyItem> _availableStartingBadges;

        public ObservableCollection<PropertyItem> StartingBadges
        {
            get { if (_startingBadges == null) RefreshStartingBadges(); return _startingBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableStartingBadges
        {
            get { if (_availableStartingBadges == null) RefreshStartingBadges(); return _availableStartingBadges; }
        }

        private RelayCommand<PropertyItem> _removeStartingBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addStartingBadgeCommand;
        public RelayCommand<PropertyItem> RemoveStartingBadgeCommand => _removeStartingBadgeCommand ??= CreateRemoveBadgeCommand(RefreshStartingBadges);
        public RelayCommand<AvailablePropertyItem> AddStartingBadgeCommand => _addStartingBadgeCommand ??= CreateAddBadgeCommand(RefreshStartingBadges);

        private void RefreshStartingBadges()
        {
            var (active, available) = BuildBadgesFromSection("starting", BadgeValueChangedHandler);
            _startingBadges = active;
            _availableStartingBadges = available;
            OnPropertyChanged(nameof(StartingBadges));
            OnPropertyChanged(nameof(AvailableStartingBadges));
        }

        // ========================================
        // Badge Collections - Province Defense
        // ========================================

        private ObservableCollection<PropertyItem> _provinceDefenseBadges;
        private ObservableCollection<AvailablePropertyItem> _availableProvinceDefenseBadges;

        public ObservableCollection<PropertyItem> ProvinceDefenseBadges
        {
            get { if (_provinceDefenseBadges == null) RefreshProvinceDefenseBadges(); return _provinceDefenseBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableProvinceDefenseBadges
        {
            get { if (_availableProvinceDefenseBadges == null) RefreshProvinceDefenseBadges(); return _availableProvinceDefenseBadges; }
        }

        private RelayCommand<PropertyItem> _removeProvinceDefenseBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addProvinceDefenseBadgeCommand;
        public RelayCommand<PropertyItem> RemoveProvinceDefenseBadgeCommand => _removeProvinceDefenseBadgeCommand ??= CreateRemoveBadgeCommand(RefreshProvinceDefenseBadges);
        public RelayCommand<AvailablePropertyItem> AddProvinceDefenseBadgeCommand => _addProvinceDefenseBadgeCommand ??= CreateAddBadgeCommand(RefreshProvinceDefenseBadges);

        private void RefreshProvinceDefenseBadges()
        {
            var (active, available) = BuildBadgesFromSection("province_defense", BadgeValueChangedHandler);
            _provinceDefenseBadges = active;
            _availableProvinceDefenseBadges = available;
            OnPropertyChanged(nameof(ProvinceDefenseBadges));
            OnPropertyChanged(nameof(AvailableProvinceDefenseBadges));
        }

        // ========================================
        // Badge Collections - UW Defense
        // ========================================

        private ObservableCollection<PropertyItem> _uwDefenseBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUwDefenseBadges;

        public ObservableCollection<PropertyItem> UwDefenseBadges
        {
            get { if (_uwDefenseBadges == null) RefreshUwDefenseBadges(); return _uwDefenseBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUwDefenseBadges
        {
            get { if (_availableUwDefenseBadges == null) RefreshUwDefenseBadges(); return _availableUwDefenseBadges; }
        }

        private RelayCommand<PropertyItem> _removeUwDefenseBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUwDefenseBadgeCommand;
        public RelayCommand<PropertyItem> RemoveUwDefenseBadgeCommand => _removeUwDefenseBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUwDefenseBadges);
        public RelayCommand<AvailablePropertyItem> AddUwDefenseBadgeCommand => _addUwDefenseBadgeCommand ??= CreateAddBadgeCommand(RefreshUwDefenseBadges);

        private void RefreshUwDefenseBadges()
        {
            var (active, available) = BuildBadgesFromSection("uw_defense", BadgeValueChangedHandler);
            _uwDefenseBadges = active;
            _availableUwDefenseBadges = available;
            OnPropertyChanged(nameof(UwDefenseBadges));
            OnPropertyChanged(nameof(AvailableUwDefenseBadges));
        }

        // ========================================
        // Badge Collections - Pretenders
        // ========================================

        private ObservableCollection<PropertyItem> _pretendersBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePretendersBadges;

        public ObservableCollection<PropertyItem> PretendersBadges
        {
            get { if (_pretendersBadges == null) RefreshPretendersBadges(); return _pretendersBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePretendersBadges
        {
            get { if (_availablePretendersBadges == null) RefreshPretendersBadges(); return _availablePretendersBadges; }
        }

        private RelayCommand<PropertyItem> _removePretendersBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPretendersBadgeCommand;
        public RelayCommand<PropertyItem> RemovePretendersBadgeCommand => _removePretendersBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPretendersBadges);
        public RelayCommand<AvailablePropertyItem> AddPretendersBadgeCommand => _addPretendersBadgeCommand ??= CreateAddBadgeCommand(RefreshPretendersBadges);

        private void RefreshPretendersBadges()
        {
            var (active, available) = BuildBadgesFromSection("pretenders", BadgeValueChangedHandler);
            _pretendersBadges = active;
            _availablePretendersBadges = available;
            OnPropertyChanged(nameof(PretendersBadges));
            OnPropertyChanged(nameof(AvailablePretendersBadges));
        }

        // ========================================
        // Badge Collections - Buildings
        // ========================================

        private ObservableCollection<PropertyItem> _buildingsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableBuildingsBadges;

        public ObservableCollection<PropertyItem> BuildingsBadges
        {
            get { if (_buildingsBadges == null) RefreshBuildingsBadges(); return _buildingsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableBuildingsBadges
        {
            get { if (_availableBuildingsBadges == null) RefreshBuildingsBadges(); return _availableBuildingsBadges; }
        }

        private RelayCommand<PropertyItem> _removeBuildingsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addBuildingsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveBuildingsBadgeCommand => _removeBuildingsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshBuildingsBadges);
        public RelayCommand<AvailablePropertyItem> AddBuildingsBadgeCommand => _addBuildingsBadgeCommand ??= CreateAddBadgeCommand(RefreshBuildingsBadges);

        private void RefreshBuildingsBadges()
        {
            var (active, available) = BuildBadgesFromSection("buildings", BadgeValueChangedHandler);
            _buildingsBadges = active;
            _availableBuildingsBadges = available;
            OnPropertyChanged(nameof(BuildingsBadges));
            OnPropertyChanged(nameof(AvailableBuildingsBadges));
        }

        // ========================================
        // Badge Collections - Scales
        // ========================================

        private ObservableCollection<PropertyItem> _scalesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableScalesBadges;

        public ObservableCollection<PropertyItem> ScalesBadges
        {
            get { if (_scalesBadges == null) RefreshScalesBadges(); return _scalesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableScalesBadges
        {
            get { if (_availableScalesBadges == null) RefreshScalesBadges(); return _availableScalesBadges; }
        }

        private RelayCommand<PropertyItem> _removeScalesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addScalesBadgeCommand;
        public RelayCommand<PropertyItem> RemoveScalesBadgeCommand => _removeScalesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshScalesBadges);
        public RelayCommand<AvailablePropertyItem> AddScalesBadgeCommand => _addScalesBadgeCommand ??= CreateAddBadgeCommand(RefreshScalesBadges);

        private void RefreshScalesBadges()
        {
            var (active, available) = BuildBadgesFromSection("scales", BadgeValueChangedHandler);
            _scalesBadges = active;
            _availableScalesBadges = available;
            OnPropertyChanged(nameof(ScalesBadges));
            OnPropertyChanged(nameof(AvailableScalesBadges));
        }

        // ========================================
        // Badge Collections - Special Mechanics
        // ========================================

        private ObservableCollection<PropertyItem> _specialMechanicsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableSpecialMechanicsBadges;

        public ObservableCollection<PropertyItem> SpecialMechanicsBadges
        {
            get { if (_specialMechanicsBadges == null) RefreshSpecialMechanicsBadges(); return _specialMechanicsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableSpecialMechanicsBadges
        {
            get { if (_availableSpecialMechanicsBadges == null) RefreshSpecialMechanicsBadges(); return _availableSpecialMechanicsBadges; }
        }

        private RelayCommand<PropertyItem> _removeSpecialMechanicsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addSpecialMechanicsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveSpecialMechanicsBadgeCommand => _removeSpecialMechanicsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshSpecialMechanicsBadges);
        public RelayCommand<AvailablePropertyItem> AddSpecialMechanicsBadgeCommand => _addSpecialMechanicsBadgeCommand ??= CreateAddBadgeCommand(RefreshSpecialMechanicsBadges);

        private void RefreshSpecialMechanicsBadges()
        {
            var (active, available) = BuildBadgesFromSection("special_mechanics", BadgeValueChangedHandler);
            _specialMechanicsBadges = active;
            _availableSpecialMechanicsBadges = available;
            OnPropertyChanged(nameof(SpecialMechanicsBadges));
            OnPropertyChanged(nameof(AvailableSpecialMechanicsBadges));
        }

        // ========================================
        // Badge Collections - Bless Bonuses
        // ========================================

        private ObservableCollection<PropertyItem> _blessBonusesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableBlessBonusesBadges;

        public ObservableCollection<PropertyItem> BlessBonusesBadges
        {
            get { if (_blessBonusesBadges == null) RefreshBlessBonusesBadges(); return _blessBonusesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableBlessBonusesBadges
        {
            get { if (_availableBlessBonusesBadges == null) RefreshBlessBonusesBadges(); return _availableBlessBonusesBadges; }
        }

        private RelayCommand<PropertyItem> _removeBlessBonusesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addBlessBonusesBadgeCommand;
        public RelayCommand<PropertyItem> RemoveBlessBonusesBadgeCommand => _removeBlessBonusesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshBlessBonusesBadges);
        public RelayCommand<AvailablePropertyItem> AddBlessBonusesBadgeCommand => _addBlessBonusesBadgeCommand ??= CreateAddBadgeCommand(RefreshBlessBonusesBadges);

        private void RefreshBlessBonusesBadges()
        {
            var (active, available) = BuildBadgesFromSection("bless_bonuses", BadgeValueChangedHandler);
            _blessBonusesBadges = active;
            _availableBlessBonusesBadges = available;
            OnPropertyChanged(nameof(BlessBonusesBadges));
            OnPropertyChanged(nameof(AvailableBlessBonusesBadges));
        }

        // ========================================
        // Badge Collections - AI Hints
        // ========================================

        private ObservableCollection<PropertyItem> _aiHintsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableAiHintsBadges;

        public ObservableCollection<PropertyItem> AiHintsBadges
        {
            get { if (_aiHintsBadges == null) RefreshAiHintsBadges(); return _aiHintsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableAiHintsBadges
        {
            get { if (_availableAiHintsBadges == null) RefreshAiHintsBadges(); return _availableAiHintsBadges; }
        }

        private RelayCommand<PropertyItem> _removeAiHintsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addAiHintsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveAiHintsBadgeCommand => _removeAiHintsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshAiHintsBadges);
        public RelayCommand<AvailablePropertyItem> AddAiHintsBadgeCommand => _addAiHintsBadgeCommand ??= CreateAddBadgeCommand(RefreshAiHintsBadges);

        private void RefreshAiHintsBadges()
        {
            var (active, available) = BuildBadgesFromSection("ai_hints", BadgeValueChangedHandler);
            _aiHintsBadges = active;
            _availableAiHintsBadges = available;
            OnPropertyChanged(nameof(AiHintsBadges));
            OnPropertyChanged(nameof(AvailableAiHintsBadges));
        }

        // ========================================
        // Badge Collections - Admin
        // ========================================

        private ObservableCollection<PropertyItem> _adminBadges;
        private ObservableCollection<AvailablePropertyItem> _availableAdminBadges;

        public ObservableCollection<PropertyItem> AdminBadges
        {
            get { if (_adminBadges == null) RefreshAdminBadges(); return _adminBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableAdminBadges
        {
            get { if (_availableAdminBadges == null) RefreshAdminBadges(); return _availableAdminBadges; }
        }

        private RelayCommand<PropertyItem> _removeAdminBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addAdminBadgeCommand;
        public RelayCommand<PropertyItem> RemoveAdminBadgeCommand => _removeAdminBadgeCommand ??= CreateRemoveBadgeCommand(RefreshAdminBadges);
        public RelayCommand<AvailablePropertyItem> AddAdminBadgeCommand => _addAdminBadgeCommand ??= CreateAddBadgeCommand(RefreshAdminBadges);

        private void RefreshAdminBadges()
        {
            var (active, available) = BuildBadgesFromSection("admin", BadgeValueChangedHandler);
            _adminBadges = active;
            _availableAdminBadges = available;
            OnPropertyChanged(nameof(AdminBadges));
            OnPropertyChanged(nameof(AvailableAdminBadges));
        }

        // ========================================
        // Shared Badge Value Changed Handler
        // ========================================

        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.ERA => "Era",
                Command.EPITHET => "Epithet",
                Command.DESCR => "Description",
                Command.FLAG => "FlagPath",
                _ => null
            };
        }
    }

    /// <summary>
    /// ViewModel for Event entities.
    /// </summary>
    public class EventViewModel : EntityViewModel
    {
        public EventViewModel(Event entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Event Event => (Event)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from event_badges.json.
        /// </summary>
        protected override string EntityTypeName => "event";

        // ========================================
        // Core Properties
        // ========================================

        /// <summary>
        /// Gets the event rarity display.
        /// </summary>
        public string RarityDisplay
        {
            get
            {
                var rarity = GetIntProperty(Command.RARITY);
                return rarity switch
                {
                    0 => "Common",
                    1 => "Uncommon",
                    2 => "Rare",
                    _ => rarity?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Gets the event message text.
        /// </summary>
        public string Message
        {
            get => GetStringProperty(Command.MSG);
            set => SetStringProperty(Command.MSG, value);
        }

        // ========================================
        // Badge Collections - Requirements
        // ========================================

        // General Requirements
        private ObservableCollection<PropertyItem> _generalRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableGeneralRequirementsBadges;

        public ObservableCollection<PropertyItem> GeneralRequirementsBadges
        {
            get { if (_generalRequirementsBadges == null) RefreshGeneralRequirementsBadges(); return _generalRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableGeneralRequirementsBadges
        {
            get { if (_availableGeneralRequirementsBadges == null) RefreshGeneralRequirementsBadges(); return _availableGeneralRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeGeneralRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addGeneralRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveGeneralRequirementsBadgeCommand => _removeGeneralRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshGeneralRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddGeneralRequirementsBadgeCommand => _addGeneralRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshGeneralRequirementsBadges);

        private void RefreshGeneralRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("general_requirements", BadgeValueChangedHandler);
            _generalRequirementsBadges = active;
            _availableGeneralRequirementsBadges = available;
            OnPropertyChanged(nameof(GeneralRequirementsBadges));
            OnPropertyChanged(nameof(AvailableGeneralRequirementsBadges));
        }

        // Nation Requirements
        private ObservableCollection<PropertyItem> _nationRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableNationRequirementsBadges;

        public ObservableCollection<PropertyItem> NationRequirementsBadges
        {
            get { if (_nationRequirementsBadges == null) RefreshNationRequirementsBadges(); return _nationRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableNationRequirementsBadges
        {
            get { if (_availableNationRequirementsBadges == null) RefreshNationRequirementsBadges(); return _availableNationRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeNationRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addNationRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveNationRequirementsBadgeCommand => _removeNationRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshNationRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddNationRequirementsBadgeCommand => _addNationRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshNationRequirementsBadges);

        private void RefreshNationRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("nation_requirements", BadgeValueChangedHandler);
            _nationRequirementsBadges = active;
            _availableNationRequirementsBadges = available;
            OnPropertyChanged(nameof(NationRequirementsBadges));
            OnPropertyChanged(nameof(AvailableNationRequirementsBadges));
        }

        // Location Requirements
        private ObservableCollection<PropertyItem> _locationRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableLocationRequirementsBadges;

        public ObservableCollection<PropertyItem> LocationRequirementsBadges
        {
            get { if (_locationRequirementsBadges == null) RefreshLocationRequirementsBadges(); return _locationRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableLocationRequirementsBadges
        {
            get { if (_availableLocationRequirementsBadges == null) RefreshLocationRequirementsBadges(); return _availableLocationRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeLocationRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addLocationRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveLocationRequirementsBadgeCommand => _removeLocationRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshLocationRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddLocationRequirementsBadgeCommand => _addLocationRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshLocationRequirementsBadges);

        private void RefreshLocationRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("location_requirements", BadgeValueChangedHandler);
            _locationRequirementsBadges = active;
            _availableLocationRequirementsBadges = available;
            OnPropertyChanged(nameof(LocationRequirementsBadges));
            OnPropertyChanged(nameof(AvailableLocationRequirementsBadges));
        }

        // Province Requirements
        private ObservableCollection<PropertyItem> _provinceRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableProvinceRequirementsBadges;

        public ObservableCollection<PropertyItem> ProvinceRequirementsBadges
        {
            get { if (_provinceRequirementsBadges == null) RefreshProvinceRequirementsBadges(); return _provinceRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableProvinceRequirementsBadges
        {
            get { if (_availableProvinceRequirementsBadges == null) RefreshProvinceRequirementsBadges(); return _availableProvinceRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeProvinceRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addProvinceRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveProvinceRequirementsBadgeCommand => _removeProvinceRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshProvinceRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddProvinceRequirementsBadgeCommand => _addProvinceRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshProvinceRequirementsBadges);

        private void RefreshProvinceRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("province_requirements", BadgeValueChangedHandler);
            _provinceRequirementsBadges = active;
            _availableProvinceRequirementsBadges = available;
            OnPropertyChanged(nameof(ProvinceRequirementsBadges));
            OnPropertyChanged(nameof(AvailableProvinceRequirementsBadges));
        }

        // Site Requirements
        private ObservableCollection<PropertyItem> _siteRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableSiteRequirementsBadges;

        public ObservableCollection<PropertyItem> SiteRequirementsBadges
        {
            get { if (_siteRequirementsBadges == null) RefreshSiteRequirementsBadges(); return _siteRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableSiteRequirementsBadges
        {
            get { if (_availableSiteRequirementsBadges == null) RefreshSiteRequirementsBadges(); return _availableSiteRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeSiteRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addSiteRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveSiteRequirementsBadgeCommand => _removeSiteRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshSiteRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddSiteRequirementsBadgeCommand => _addSiteRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshSiteRequirementsBadges);

        private void RefreshSiteRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("site_requirements", BadgeValueChangedHandler);
            _siteRequirementsBadges = active;
            _availableSiteRequirementsBadges = available;
            OnPropertyChanged(nameof(SiteRequirementsBadges));
            OnPropertyChanged(nameof(AvailableSiteRequirementsBadges));
        }

        // Dominion Requirements
        private ObservableCollection<PropertyItem> _dominionRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableDominionRequirementsBadges;

        public ObservableCollection<PropertyItem> DominionRequirementsBadges
        {
            get { if (_dominionRequirementsBadges == null) RefreshDominionRequirementsBadges(); return _dominionRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableDominionRequirementsBadges
        {
            get { if (_availableDominionRequirementsBadges == null) RefreshDominionRequirementsBadges(); return _availableDominionRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeDominionRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addDominionRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveDominionRequirementsBadgeCommand => _removeDominionRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshDominionRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddDominionRequirementsBadgeCommand => _addDominionRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshDominionRequirementsBadges);

        private void RefreshDominionRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("dominion_requirements", BadgeValueChangedHandler);
            _dominionRequirementsBadges = active;
            _availableDominionRequirementsBadges = available;
            OnPropertyChanged(nameof(DominionRequirementsBadges));
            OnPropertyChanged(nameof(AvailableDominionRequirementsBadges));
        }

        // Path Requirements
        private ObservableCollection<PropertyItem> _pathRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePathRequirementsBadges;

        public ObservableCollection<PropertyItem> PathRequirementsBadges
        {
            get { if (_pathRequirementsBadges == null) RefreshPathRequirementsBadges(); return _pathRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePathRequirementsBadges
        {
            get { if (_availablePathRequirementsBadges == null) RefreshPathRequirementsBadges(); return _availablePathRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removePathRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPathRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemovePathRequirementsBadgeCommand => _removePathRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPathRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddPathRequirementsBadgeCommand => _addPathRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshPathRequirementsBadges);

        private void RefreshPathRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("path_requirements", BadgeValueChangedHandler);
            _pathRequirementsBadges = active;
            _availablePathRequirementsBadges = available;
            OnPropertyChanged(nameof(PathRequirementsBadges));
            OnPropertyChanged(nameof(AvailablePathRequirementsBadges));
        }

        // Commander Requirements
        private ObservableCollection<PropertyItem> _commanderRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCommanderRequirementsBadges;

        public ObservableCollection<PropertyItem> CommanderRequirementsBadges
        {
            get { if (_commanderRequirementsBadges == null) RefreshCommanderRequirementsBadges(); return _commanderRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCommanderRequirementsBadges
        {
            get { if (_availableCommanderRequirementsBadges == null) RefreshCommanderRequirementsBadges(); return _availableCommanderRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeCommanderRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCommanderRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveCommanderRequirementsBadgeCommand => _removeCommanderRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCommanderRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddCommanderRequirementsBadgeCommand => _addCommanderRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshCommanderRequirementsBadges);

        private void RefreshCommanderRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("commander_requirements", BadgeValueChangedHandler);
            _commanderRequirementsBadges = active;
            _availableCommanderRequirementsBadges = available;
            OnPropertyChanged(nameof(CommanderRequirementsBadges));
            OnPropertyChanged(nameof(AvailableCommanderRequirementsBadges));
        }

        // Target Requirements
        private ObservableCollection<PropertyItem> _targetRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableTargetRequirementsBadges;

        public ObservableCollection<PropertyItem> TargetRequirementsBadges
        {
            get { if (_targetRequirementsBadges == null) RefreshTargetRequirementsBadges(); return _targetRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableTargetRequirementsBadges
        {
            get { if (_availableTargetRequirementsBadges == null) RefreshTargetRequirementsBadges(); return _availableTargetRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeTargetRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addTargetRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveTargetRequirementsBadgeCommand => _removeTargetRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshTargetRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddTargetRequirementsBadgeCommand => _addTargetRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshTargetRequirementsBadges);

        private void RefreshTargetRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("target_requirements", BadgeValueChangedHandler);
            _targetRequirementsBadges = active;
            _availableTargetRequirementsBadges = available;
            OnPropertyChanged(nameof(TargetRequirementsBadges));
            OnPropertyChanged(nameof(AvailableTargetRequirementsBadges));
        }

        // Code Requirements
        private ObservableCollection<PropertyItem> _codeRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCodeRequirementsBadges;

        public ObservableCollection<PropertyItem> CodeRequirementsBadges
        {
            get { if (_codeRequirementsBadges == null) RefreshCodeRequirementsBadges(); return _codeRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCodeRequirementsBadges
        {
            get { if (_availableCodeRequirementsBadges == null) RefreshCodeRequirementsBadges(); return _availableCodeRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeCodeRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCodeRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveCodeRequirementsBadgeCommand => _removeCodeRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCodeRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddCodeRequirementsBadgeCommand => _addCodeRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshCodeRequirementsBadges);

        private void RefreshCodeRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("code_requirements", BadgeValueChangedHandler);
            _codeRequirementsBadges = active;
            _availableCodeRequirementsBadges = available;
            OnPropertyChanged(nameof(CodeRequirementsBadges));
            OnPropertyChanged(nameof(AvailableCodeRequirementsBadges));
        }

        // Enchantment Requirements
        private ObservableCollection<PropertyItem> _enchantmentRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEnchantmentRequirementsBadges;

        public ObservableCollection<PropertyItem> EnchantmentRequirementsBadges
        {
            get { if (_enchantmentRequirementsBadges == null) RefreshEnchantmentRequirementsBadges(); return _enchantmentRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEnchantmentRequirementsBadges
        {
            get { if (_availableEnchantmentRequirementsBadges == null) RefreshEnchantmentRequirementsBadges(); return _availableEnchantmentRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeEnchantmentRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEnchantmentRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveEnchantmentRequirementsBadgeCommand => _removeEnchantmentRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEnchantmentRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddEnchantmentRequirementsBadgeCommand => _addEnchantmentRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshEnchantmentRequirementsBadges);

        private void RefreshEnchantmentRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("enchantment_requirements", BadgeValueChangedHandler);
            _enchantmentRequirementsBadges = active;
            _availableEnchantmentRequirementsBadges = available;
            OnPropertyChanged(nameof(EnchantmentRequirementsBadges));
            OnPropertyChanged(nameof(AvailableEnchantmentRequirementsBadges));
        }

        // ========================================
        // Badge Collections - Effects
        // ========================================

        // Message
        private ObservableCollection<PropertyItem> _messageBadges;
        private ObservableCollection<AvailablePropertyItem> _availableMessageBadges;

        public ObservableCollection<PropertyItem> MessageBadges
        {
            get { if (_messageBadges == null) RefreshMessageBadges(); return _messageBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableMessageBadges
        {
            get { if (_availableMessageBadges == null) RefreshMessageBadges(); return _availableMessageBadges; }
        }

        private RelayCommand<PropertyItem> _removeMessageBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addMessageBadgeCommand;
        public RelayCommand<PropertyItem> RemoveMessageBadgeCommand => _removeMessageBadgeCommand ??= CreateRemoveBadgeCommand(RefreshMessageBadges);
        public RelayCommand<AvailablePropertyItem> AddMessageBadgeCommand => _addMessageBadgeCommand ??= CreateAddBadgeCommand(RefreshMessageBadges);

        private void RefreshMessageBadges()
        {
            var (active, available) = BuildBadgesFromSection("message", BadgeValueChangedHandler);
            _messageBadges = active;
            _availableMessageBadges = available;
            OnPropertyChanged(nameof(MessageBadges));
            OnPropertyChanged(nameof(AvailableMessageBadges));
        }

        // Resource Effects
        private ObservableCollection<PropertyItem> _resourceEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableResourceEffectsBadges;

        public ObservableCollection<PropertyItem> ResourceEffectsBadges
        {
            get { if (_resourceEffectsBadges == null) RefreshResourceEffectsBadges(); return _resourceEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableResourceEffectsBadges
        {
            get { if (_availableResourceEffectsBadges == null) RefreshResourceEffectsBadges(); return _availableResourceEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeResourceEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResourceEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveResourceEffectsBadgeCommand => _removeResourceEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshResourceEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddResourceEffectsBadgeCommand => _addResourceEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshResourceEffectsBadges);

        private void RefreshResourceEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("resource_effects", BadgeValueChangedHandler);
            _resourceEffectsBadges = active;
            _availableResourceEffectsBadges = available;
            OnPropertyChanged(nameof(ResourceEffectsBadges));
            OnPropertyChanged(nameof(AvailableResourceEffectsBadges));
        }

        // Province Effects
        private ObservableCollection<PropertyItem> _provinceEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableProvinceEffectsBadges;

        public ObservableCollection<PropertyItem> ProvinceEffectsBadges
        {
            get { if (_provinceEffectsBadges == null) RefreshProvinceEffectsBadges(); return _provinceEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableProvinceEffectsBadges
        {
            get { if (_availableProvinceEffectsBadges == null) RefreshProvinceEffectsBadges(); return _availableProvinceEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeProvinceEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addProvinceEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveProvinceEffectsBadgeCommand => _removeProvinceEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshProvinceEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddProvinceEffectsBadgeCommand => _addProvinceEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshProvinceEffectsBadges);

        private void RefreshProvinceEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("province_effects", BadgeValueChangedHandler);
            _provinceEffectsBadges = active;
            _availableProvinceEffectsBadges = available;
            OnPropertyChanged(nameof(ProvinceEffectsBadges));
            OnPropertyChanged(nameof(AvailableProvinceEffectsBadges));
        }

        // Scale Effects
        private ObservableCollection<PropertyItem> _scaleEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableScaleEffectsBadges;

        public ObservableCollection<PropertyItem> ScaleEffectsBadges
        {
            get { if (_scaleEffectsBadges == null) RefreshScaleEffectsBadges(); return _scaleEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableScaleEffectsBadges
        {
            get { if (_availableScaleEffectsBadges == null) RefreshScaleEffectsBadges(); return _availableScaleEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeScaleEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addScaleEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveScaleEffectsBadgeCommand => _removeScaleEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshScaleEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddScaleEffectsBadgeCommand => _addScaleEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshScaleEffectsBadges);

        private void RefreshScaleEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("scale_effects", BadgeValueChangedHandler);
            _scaleEffectsBadges = active;
            _availableScaleEffectsBadges = available;
            OnPropertyChanged(nameof(ScaleEffectsBadges));
            OnPropertyChanged(nameof(AvailableScaleEffectsBadges));
        }

        // Unit Spawn
        private ObservableCollection<PropertyItem> _unitSpawnBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUnitSpawnBadges;

        public ObservableCollection<PropertyItem> UnitSpawnBadges
        {
            get { if (_unitSpawnBadges == null) RefreshUnitSpawnBadges(); return _unitSpawnBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUnitSpawnBadges
        {
            get { if (_availableUnitSpawnBadges == null) RefreshUnitSpawnBadges(); return _availableUnitSpawnBadges; }
        }

        private RelayCommand<PropertyItem> _removeUnitSpawnBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUnitSpawnBadgeCommand;
        public RelayCommand<PropertyItem> RemoveUnitSpawnBadgeCommand => _removeUnitSpawnBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUnitSpawnBadges);
        public RelayCommand<AvailablePropertyItem> AddUnitSpawnBadgeCommand => _addUnitSpawnBadgeCommand ??= CreateAddBadgeCommand(RefreshUnitSpawnBadges);

        private void RefreshUnitSpawnBadges()
        {
            var (active, available) = BuildBadgesFromSection("unit_spawn", BadgeValueChangedHandler);
            _unitSpawnBadges = active;
            _availableUnitSpawnBadges = available;
            OnPropertyChanged(nameof(UnitSpawnBadges));
            OnPropertyChanged(nameof(AvailableUnitSpawnBadges));
        }

        // Unit Effects
        private ObservableCollection<PropertyItem> _unitEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUnitEffectsBadges;

        public ObservableCollection<PropertyItem> UnitEffectsBadges
        {
            get { if (_unitEffectsBadges == null) RefreshUnitEffectsBadges(); return _unitEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUnitEffectsBadges
        {
            get { if (_availableUnitEffectsBadges == null) RefreshUnitEffectsBadges(); return _availableUnitEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeUnitEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUnitEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveUnitEffectsBadgeCommand => _removeUnitEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUnitEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddUnitEffectsBadgeCommand => _addUnitEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshUnitEffectsBadges);

        private void RefreshUnitEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("unit_effects", BadgeValueChangedHandler);
            _unitEffectsBadges = active;
            _availableUnitEffectsBadges = available;
            OnPropertyChanged(nameof(UnitEffectsBadges));
            OnPropertyChanged(nameof(AvailableUnitEffectsBadges));
        }

        // Path Boost
        private ObservableCollection<PropertyItem> _pathBoostBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePathBoostBadges;

        public ObservableCollection<PropertyItem> PathBoostBadges
        {
            get { if (_pathBoostBadges == null) RefreshPathBoostBadges(); return _pathBoostBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePathBoostBadges
        {
            get { if (_availablePathBoostBadges == null) RefreshPathBoostBadges(); return _availablePathBoostBadges; }
        }

        private RelayCommand<PropertyItem> _removePathBoostBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPathBoostBadgeCommand;
        public RelayCommand<PropertyItem> RemovePathBoostBadgeCommand => _removePathBoostBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPathBoostBadges);
        public RelayCommand<AvailablePropertyItem> AddPathBoostBadgeCommand => _addPathBoostBadgeCommand ??= CreateAddBadgeCommand(RefreshPathBoostBadges);

        private void RefreshPathBoostBadges()
        {
            var (active, available) = BuildBadgesFromSection("path_boost", BadgeValueChangedHandler);
            _pathBoostBadges = active;
            _availablePathBoostBadges = available;
            OnPropertyChanged(nameof(PathBoostBadges));
            OnPropertyChanged(nameof(AvailablePathBoostBadges));
        }

        // World Effects
        private ObservableCollection<PropertyItem> _worldEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableWorldEffectsBadges;

        public ObservableCollection<PropertyItem> WorldEffectsBadges
        {
            get { if (_worldEffectsBadges == null) RefreshWorldEffectsBadges(); return _worldEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableWorldEffectsBadges
        {
            get { if (_availableWorldEffectsBadges == null) RefreshWorldEffectsBadges(); return _availableWorldEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeWorldEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addWorldEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveWorldEffectsBadgeCommand => _removeWorldEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshWorldEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddWorldEffectsBadgeCommand => _addWorldEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshWorldEffectsBadges);

        private void RefreshWorldEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("world_effects", BadgeValueChangedHandler);
            _worldEffectsBadges = active;
            _availableWorldEffectsBadges = available;
            OnPropertyChanged(nameof(WorldEffectsBadges));
            OnPropertyChanged(nameof(AvailableWorldEffectsBadges));
        }

        // Event Control
        private ObservableCollection<PropertyItem> _eventControlBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEventControlBadges;

        public ObservableCollection<PropertyItem> EventControlBadges
        {
            get { if (_eventControlBadges == null) RefreshEventControlBadges(); return _eventControlBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEventControlBadges
        {
            get { if (_availableEventControlBadges == null) RefreshEventControlBadges(); return _availableEventControlBadges; }
        }

        private RelayCommand<PropertyItem> _removeEventControlBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEventControlBadgeCommand;
        public RelayCommand<PropertyItem> RemoveEventControlBadgeCommand => _removeEventControlBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEventControlBadges);
        public RelayCommand<AvailablePropertyItem> AddEventControlBadgeCommand => _addEventControlBadgeCommand ??= CreateAddBadgeCommand(RefreshEventControlBadges);

        private void RefreshEventControlBadges()
        {
            var (active, available) = BuildBadgesFromSection("event_control", BadgeValueChangedHandler);
            _eventControlBadges = active;
            _availableEventControlBadges = available;
            OnPropertyChanged(nameof(EventControlBadges));
            OnPropertyChanged(nameof(AvailableEventControlBadges));
        }

        // ========================================
        // Shared Badge Value Changed Handler
        // ========================================

        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged($"Is{propertyName}Modified");
                OnPropertyChanged($"Is{propertyName}SessionEdit");
            }
        }

        private static string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.RARITY => "Rarity",
                Command.MSG => "Message",
                _ => null
            };
        }
    }

    /// <summary>
    /// ViewModel for Mercenary entities.
    /// </summary>
    public class MercenaryViewModel : EntityViewModel
    {
        public MercenaryViewModel(Mercenary entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Mercenary Mercenary => (Mercenary)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from mercenary_badges.json.
        /// </summary>
        protected override string EntityTypeName => "mercenary";

        // ========================================
        // Core Properties
        // ========================================

        public int? Level
        {
            get => GetIntProperty(Command.LEVEL);
            set => SetIntProperty(Command.LEVEL, value);
        }
        public bool IsLevelModified => IsIntPropertyModifiedFromVanilla(Command.LEVEL);
        public bool IsLevelSessionEdit => IsPropertyEditedInSession(Command.LEVEL);

        public string BossName
        {
            get => GetStringProperty(Command.BOSSNAME);
            set => SetStringProperty(Command.BOSSNAME, value);
        }
        public bool IsBossNameModified => IsStringPropertyModifiedFromVanilla(Command.BOSSNAME);
        public bool IsBossNameSessionEdit => IsPropertyEditedInSession(Command.BOSSNAME);

        public int? EraMask
        {
            get => GetIntProperty(Command.ERAMASK);
            set => SetIntProperty(Command.ERAMASK, value);
        }
        public bool IsEraMaskModified => IsIntPropertyModifiedFromVanilla(Command.ERAMASK);
        public bool IsEraMaskSessionEdit => IsPropertyEditedInSession(Command.ERAMASK);

        /// <summary>
        /// Gets the era display string from the era bitmask.
        /// </summary>
        public string EraDisplay
        {
            get
            {
                var mask = EraMask ?? 0;
                if (mask == 0) return "None";
                var eras = new List<string>();
                if ((mask & 1) != 0) eras.Add("EA");
                if ((mask & 2) != 0) eras.Add("MA");
                if ((mask & 4) != 0) eras.Add("LA");
                return string.Join("/", eras);
            }
        }

        // ========================================
        // Unit References
        // ========================================

        public string CommanderDisplay
        {
            get
            {
                var result = _entity.TryGet<MonsterOrMontagRef>(Command.COM, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return GetMonsterOrMontagName(prop);
                }

                // VanillaModified fallback
                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<MonsterOrMontagRef>(Command.COM, out var vanillaProp);
                        if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                        {
                            return GetMonsterOrMontagName(vanillaProp);
                        }
                    }
                }
                return null;
            }
        }

        public string UnitDisplay
        {
            get
            {
                var result = _entity.TryGet<MonsterOrMontagRef>(Command.UNIT, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return GetMonsterOrMontagName(prop);
                }

                // VanillaModified fallback
                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<MonsterOrMontagRef>(Command.UNIT, out var vanillaProp);
                        if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                        {
                            return GetMonsterOrMontagName(vanillaProp);
                        }
                    }
                }
                return null;
            }
        }

        private string GetMonsterOrMontagName(MonsterOrMontagRef prop)
        {
            // Try to get the resolved entity
            if (prop.TryGetEntity(out var entity) && entity != null)
            {
                var name = entity.Name;
                if (!string.IsNullOrEmpty(name))
                    return $"{name} (#{entity.ID})";
                return $"#{entity.ID}";
            }

            // Fallback to export string if entity not resolved
            var exportStr = prop.ToExportString();
            if (!string.IsNullOrEmpty(exportStr))
            {
                // Check if it's a negative ID (montag)
                if (int.TryParse(exportStr, out var id) && id < 0)
                {
                    return $"Montag #{id}";
                }
                return exportStr;
            }
            return null;
        }

        public int? NrUnits
        {
            get => GetIntProperty(Command.NRUNITS);
            set => SetIntProperty(Command.NRUNITS, value);
        }
        public bool IsNrUnitsModified => IsIntPropertyModifiedFromVanilla(Command.NRUNITS);
        public bool IsNrUnitsSessionEdit => IsPropertyEditedInSession(Command.NRUNITS);

        public int? MinMen
        {
            get => GetIntProperty(Command.MINMEN);
            set => SetIntProperty(Command.MINMEN, value);
        }
        public bool IsMinMenModified => IsIntPropertyModifiedFromVanilla(Command.MINMEN);
        public bool IsMinMenSessionEdit => IsPropertyEditedInSession(Command.MINMEN);

        // ========================================
        // Economics
        // ========================================

        public int? MinPay
        {
            get => GetIntProperty(Command.MINPAY);
            set => SetIntProperty(Command.MINPAY, value);
        }
        public bool IsMinPayModified => IsIntPropertyModifiedFromVanilla(Command.MINPAY);
        public bool IsMinPaySessionEdit => IsPropertyEditedInSession(Command.MINPAY);

        public int? RecRate
        {
            get => GetIntProperty(Command.RECRATE);
            set => SetIntProperty(Command.RECRATE, value);
        }
        public bool IsRecRateModified => IsIntPropertyModifiedFromVanilla(Command.RECRATE);
        public bool IsRecRateSessionEdit => IsPropertyEditedInSession(Command.RECRATE);

        // ========================================
        // Equipment
        // ========================================

        public int? XP
        {
            get => GetIntProperty(Command.XP);
            set => SetIntProperty(Command.XP, value);
        }
        public bool IsXPModified => IsIntPropertyModifiedFromVanilla(Command.XP);
        public bool IsXPSessionEdit => IsPropertyEditedInSession(Command.XP);

        public int? RandEquip
        {
            get => GetIntProperty(Command.RANDEQUIP);
            set => SetIntProperty(Command.RANDEQUIP, value);
        }
        public bool IsRandEquipModified => IsIntPropertyModifiedFromVanilla(Command.RANDEQUIP);
        public bool IsRandEquipSessionEdit => IsPropertyEditedInSession(Command.RANDEQUIP);

        public string ItemDisplay
        {
            get
            {
                var result = _entity.TryGet<ItemRef>(Command.ITEM, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return GetReferenceName(prop, EntityType.ITEM) ?? $"#{prop.ID}";
                }

                // VanillaModified fallback
                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<ItemRef>(Command.ITEM, out var vanillaProp);
                        if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                        {
                            return GetReferenceName(vanillaProp, EntityType.ITEM) ?? $"#{vanillaProp.ID}";
                        }
                    }
                }
                return null;
            }
        }

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _identityBadges;
        private ObservableCollection<AvailablePropertyItem> _availableIdentityBadges;
        private ObservableCollection<PropertyItem> _unitsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUnitsBadges;
        private ObservableCollection<PropertyItem> _economicsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEconomicsBadges;
        private ObservableCollection<PropertyItem> _equipmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEquipmentBadges;

        public ObservableCollection<PropertyItem> IdentityBadges
        {
            get { if (_identityBadges == null) RefreshIdentityBadges(); return _identityBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableIdentityBadges
        {
            get { if (_availableIdentityBadges == null) RefreshIdentityBadges(); return _availableIdentityBadges; }
        }

        public ObservableCollection<PropertyItem> UnitsBadges
        {
            get { if (_unitsBadges == null) RefreshUnitsBadges(); return _unitsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUnitsBadges
        {
            get { if (_availableUnitsBadges == null) RefreshUnitsBadges(); return _availableUnitsBadges; }
        }

        public ObservableCollection<PropertyItem> EconomicsBadges
        {
            get { if (_economicsBadges == null) RefreshEconomicsBadges(); return _economicsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEconomicsBadges
        {
            get { if (_availableEconomicsBadges == null) RefreshEconomicsBadges(); return _availableEconomicsBadges; }
        }

        public ObservableCollection<PropertyItem> EquipmentBadges
        {
            get { if (_equipmentBadges == null) RefreshEquipmentBadges(); return _equipmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEquipmentBadges
        {
            get { if (_availableEquipmentBadges == null) RefreshEquipmentBadges(); return _availableEquipmentBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeIdentityBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addIdentityBadgeCommand;
        private RelayCommand<PropertyItem> _removeUnitsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUnitsBadgeCommand;
        private RelayCommand<PropertyItem> _removeEconomicsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEconomicsBadgeCommand;
        private RelayCommand<PropertyItem> _removeEquipmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEquipmentBadgeCommand;

        public RelayCommand<PropertyItem> RemoveIdentityBadgeCommand => _removeIdentityBadgeCommand ??= CreateRemoveBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<AvailablePropertyItem> AddIdentityBadgeCommand => _addIdentityBadgeCommand ??= CreateAddBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<PropertyItem> RemoveUnitsBadgeCommand => _removeUnitsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUnitsBadges);
        public RelayCommand<AvailablePropertyItem> AddUnitsBadgeCommand => _addUnitsBadgeCommand ??= CreateAddBadgeCommand(RefreshUnitsBadges);
        public RelayCommand<PropertyItem> RemoveEconomicsBadgeCommand => _removeEconomicsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEconomicsBadges);
        public RelayCommand<AvailablePropertyItem> AddEconomicsBadgeCommand => _addEconomicsBadgeCommand ??= CreateAddBadgeCommand(RefreshEconomicsBadges);
        public RelayCommand<PropertyItem> RemoveEquipmentBadgeCommand => _removeEquipmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEquipmentBadges);
        public RelayCommand<AvailablePropertyItem> AddEquipmentBadgeCommand => _addEquipmentBadgeCommand ??= CreateAddBadgeCommand(RefreshEquipmentBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshIdentityBadges()
        {
            var (active, available) = BuildBadgesFromSection("identity", BadgeValueChangedHandler);
            _identityBadges = active;
            _availableIdentityBadges = available;
            OnPropertyChanged(nameof(IdentityBadges));
            OnPropertyChanged(nameof(AvailableIdentityBadges));
        }

        private void RefreshUnitsBadges()
        {
            var (active, available) = BuildBadgesFromSection("units", BadgeValueChangedHandler);
            _unitsBadges = active;
            _availableUnitsBadges = available;
            OnPropertyChanged(nameof(UnitsBadges));
            OnPropertyChanged(nameof(AvailableUnitsBadges));
        }

        private void RefreshEconomicsBadges()
        {
            var (active, available) = BuildBadgesFromSection("economics", BadgeValueChangedHandler);
            _economicsBadges = active;
            _availableEconomicsBadges = available;
            OnPropertyChanged(nameof(EconomicsBadges));
            OnPropertyChanged(nameof(AvailableEconomicsBadges));
        }

        private void RefreshEquipmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("equipment", BadgeValueChangedHandler);
            _equipmentBadges = active;
            _availableEquipmentBadges = available;
            OnPropertyChanged(nameof(EquipmentBadges));
            OnPropertyChanged(nameof(AvailableEquipmentBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged(propertyName + "Modified");
                OnPropertyChanged(propertyName + "SessionEdit");
            }

            // Refresh relevant badge collection
            RefreshIdentityBadges();
            RefreshUnitsBadges();
            RefreshEconomicsBadges();
            RefreshEquipmentBadges();
        }

        private string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.LEVEL => nameof(Level),
                Command.BOSSNAME => nameof(BossName),
                Command.ERAMASK => nameof(EraMask),
                Command.NRUNITS => nameof(NrUnits),
                Command.MINMEN => nameof(MinMen),
                Command.MINPAY => nameof(MinPay),
                Command.RECRATE => nameof(RecRate),
                Command.XP => nameof(XP),
                Command.RANDEQUIP => nameof(RandEquip),
                _ => null
            };
        }
    }

    /// <summary>
    /// ViewModel for Poptype entities.
    /// </summary>
    public class PoptypeViewModel : EntityViewModel
    {
        public PoptypeViewModel(Poptype entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Poptype Poptype => (Poptype)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from poptype_badges.json.
        /// </summary>
        protected override string EntityTypeName => "poptype";

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _recruitmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableRecruitmentBadges;
        private ObservableCollection<PropertyItem> _defenseBadges;
        private ObservableCollection<AvailablePropertyItem> _availableDefenseBadges;

        public ObservableCollection<PropertyItem> RecruitmentBadges
        {
            get { if (_recruitmentBadges == null) RefreshRecruitmentBadges(); return _recruitmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableRecruitmentBadges
        {
            get { if (_availableRecruitmentBadges == null) RefreshRecruitmentBadges(); return _availableRecruitmentBadges; }
        }

        public ObservableCollection<PropertyItem> DefenseBadges
        {
            get { if (_defenseBadges == null) RefreshDefenseBadges(); return _defenseBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableDefenseBadges
        {
            get { if (_availableDefenseBadges == null) RefreshDefenseBadges(); return _availableDefenseBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeRecruitmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addRecruitmentBadgeCommand;
        private RelayCommand<PropertyItem> _removeDefenseBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addDefenseBadgeCommand;

        public RelayCommand<PropertyItem> RemoveRecruitmentBadgeCommand => _removeRecruitmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshRecruitmentBadges);
        public RelayCommand<AvailablePropertyItem> AddRecruitmentBadgeCommand => _addRecruitmentBadgeCommand ??= CreateAddBadgeCommand(RefreshRecruitmentBadges);
        public RelayCommand<PropertyItem> RemoveDefenseBadgeCommand => _removeDefenseBadgeCommand ??= CreateRemoveBadgeCommand(RefreshDefenseBadges);
        public RelayCommand<AvailablePropertyItem> AddDefenseBadgeCommand => _addDefenseBadgeCommand ??= CreateAddBadgeCommand(RefreshDefenseBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshRecruitmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("recruitment", BadgeValueChangedHandler);
            _recruitmentBadges = active;
            _availableRecruitmentBadges = available;
            OnPropertyChanged(nameof(RecruitmentBadges));
            OnPropertyChanged(nameof(AvailableRecruitmentBadges));
        }

        private void RefreshDefenseBadges()
        {
            var (active, available) = BuildBadgesFromSection("defense", BadgeValueChangedHandler);
            _defenseBadges = active;
            _availableDefenseBadges = available;
            OnPropertyChanged(nameof(DefenseBadges));
            OnPropertyChanged(nameof(AvailableDefenseBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            RefreshRecruitmentBadges();
            RefreshDefenseBadges();
        }
    }

    /// <summary>
    /// ViewModel for Nametype entities.
    /// </summary>
    public class NametypeViewModel : EntityViewModel
    {
        public NametypeViewModel(Nametype entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Nametype Nametype => (Nametype)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from nametype_badges.json.
        /// </summary>
        protected override string EntityTypeName => "nametype";

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _namesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableNamesBadges;

        public ObservableCollection<PropertyItem> NamesBadges
        {
            get { if (_namesBadges == null) RefreshNamesBadges(); return _namesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableNamesBadges
        {
            get { if (_availableNamesBadges == null) RefreshNamesBadges(); return _availableNamesBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeNamesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addNamesBadgeCommand;

        public RelayCommand<PropertyItem> RemoveNamesBadgeCommand => _removeNamesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshNamesBadges);
        public RelayCommand<AvailablePropertyItem> AddNamesBadgeCommand => _addNamesBadgeCommand ??= CreateAddBadgeCommand(RefreshNamesBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshNamesBadges()
        {
            var (active, available) = BuildBadgesFromSection("names", BadgeValueChangedHandler);
            _namesBadges = active;
            _availableNamesBadges = available;
            OnPropertyChanged(nameof(NamesBadges));
            OnPropertyChanged(nameof(AvailableNamesBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            RefreshNamesBadges();
        }
    }
}
