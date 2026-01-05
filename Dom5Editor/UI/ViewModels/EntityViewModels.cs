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
using Dom5Editor.VMs;
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
    /// ViewModel for Monster entities.
    /// </summary>
    public class MonsterViewModel : EntityViewModel
    {
        public MonsterViewModel(Monster entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Monster Monster => (Monster)_entity;

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

        // ========================================
        // Command List Properties (for CommandListEditor)
        // ========================================

        private static readonly Command[] TypeCommands = new[]
        {
            Command.HUMANOID, Command.MOUNTEDHUMANOID, Command.QUADRUPED, Command.LIZARD,
            Command.NAGA, Command.SNAKE, Command.BIRD, Command.DJINN, Command.TROGLODYTE,
            Command.MISCSHAPE, Command.MOUNTED, Command.UNDEAD, Command.DEMON,
            Command.MAGICBEING, Command.HOLY, Command.ANIMAL, Command.UNIQUE,
            Command.INANIMATE, Command.MINDLESS, Command.BLIND, Command.COLDBLOOD,
            Command.IMMORTAL, Command.FEMALE, Command.IMMOBILE, Command.STONEBEING,
            Command.PLANT, Command.DRAKE, Command.BUG, Command.LESSERHORROR,
            Command.GREATERHORROR, Command.DOOMHORROR
        };

        private static readonly Command[] LeaderCommands = new[]
        {
            Command.NOLEADER, Command.POORLEADER, Command.OKLEADER, Command.GOODLEADER,
            Command.EXPERTLEADER, Command.SUPERIORLEADER, Command.NOMAGICLEADER,
            Command.POORMAGICLEADER, Command.OKMAGICLEADER, Command.GOODMAGICLEADER,
            Command.EXPERTMAGICLEADER, Command.SUPERIORMAGICLEADER, Command.NOUNDEADLEADER,
            Command.POORUNDEADLEADER, Command.OKUNDEADLEADER, Command.GOODUNDEADLEADER,
            Command.EXPERTUNDEADLEADER, Command.SUPERIORUNDEADLEADER
        };

        private static readonly Command[] MovementCommands = new[]
        {
            Command.FLYING, Command.AQUATIC, Command.AMPHIBIAN, Command.POORAMPHIBIAN,
            Command.FLOAT, Command.SWIMMING, Command.TELEPORT, Command.MAPTELEPORT,
            Command.BLINK, Command.FORESTSURVIVAL, Command.MOUNTAINSURVIVAL,
            Command.SWAMPSURVIVAL, Command.WASTESURVIVAL, Command.SNOW
        };

        private static readonly Command[] ResistanceCommands = new[]
        {
            Command.FIRERES, Command.COLDRES, Command.SHOCKRES, Command.POISONRES,
            Command.REGENERATION, Command.INVULNERABLE, Command.AIRSHIELD, Command.ICEPROT,
            Command.REINVIGORATION, Command.IRONVUL, Command.BLUNTRES, Command.PIERCERES,
            Command.SLASHRES, Command.DISEASERES, Command.MAGICIMMUNE, Command.STORMIMMUNE,
            Command.STUNIMMUNITY, Command.POLYIMMUNE, Command.ACIDRES, Command.DECAYRES
        };

        private static readonly Command[] CombatCommands = new[]
        {
            Command.AWE, Command.FEAR, Command.BERSERK, Command.AMBIDEXTROUS,
            Command.DARKVISION, Command.TRAMPLE, Command.DEATHCURSE, Command.BODYGUARD,
            Command.WARNING, Command.STANDARD, Command.FORMATIONFIGHTER, Command.PATIENCE,
            Command.CHAOSPOWER, Command.MAGICPOWER, Command.ETHEREAL, Command.GLAMOUR
        };

        private static readonly Command[] AuraCommands = new[]
        {
            Command.HEAT, Command.COLD, Command.FIRESHIELD, Command.POISONCLOUD,
            Command.DISEASECLOUD, Command.POISONSKIN, Command.POISONARMOR, Command.ACIDSHIELD,
            Command.SLEEPAURA, Command.ANIMALAWE, Command.SUNAWE, Command.HALTHERETIC
        };

        private static readonly Command[] SpecialCommands = new[]
        {
            Command.HEAL, Command.NOHEAL, Command.HEALER, Command.AUTOHEALER,
            Command.NEEDNOTEAT, Command.TAXCOLLECTOR, Command.INQUISITOR, Command.MASON,
            Command.LOCALSUN, Command.COMMASTER, Command.COMSLAVE, Command.SPELLSINGER,
            Command.COMBATCASTER, Command.DRAINIMMUNE, Command.DIVINEINS, Command.NOITEM
        };

        private System.Collections.ObjectModel.ObservableCollection<CommandListItem> _typeCommandsList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> _availableTypeCommands;
        private System.Collections.ObjectModel.ObservableCollection<CommandListItem> _leaderCommandsList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> _availableLeaderCommands;
        private System.Collections.ObjectModel.ObservableCollection<CommandListItem> _movementCommandsList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> _availableMovementCommands;
        private System.Collections.ObjectModel.ObservableCollection<IntPropertyListItem> _resistancesList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableIntPropertyItem> _availableResistances;
        private System.Collections.ObjectModel.ObservableCollection<IntPropertyListItem> _combatList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableIntPropertyItem> _availableCombat;
        private System.Collections.ObjectModel.ObservableCollection<IntPropertyListItem> _auraList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableIntPropertyItem> _availableAuras;
        private System.Collections.ObjectModel.ObservableCollection<CommandListItem> _specialCommandsList;
        private System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> _availableSpecialCommands;

        public System.Collections.ObjectModel.ObservableCollection<CommandListItem> TypeCommandsList
        {
            get { if (_typeCommandsList == null) RefreshTypeCommands(); return _typeCommandsList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> AvailableTypeCommands
        {
            get { if (_availableTypeCommands == null) RefreshTypeCommands(); return _availableTypeCommands; }
        }

        public System.Collections.ObjectModel.ObservableCollection<CommandListItem> LeaderCommandsList
        {
            get { if (_leaderCommandsList == null) RefreshLeaderCommands(); return _leaderCommandsList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> AvailableLeaderCommands
        {
            get { if (_availableLeaderCommands == null) RefreshLeaderCommands(); return _availableLeaderCommands; }
        }

        public System.Collections.ObjectModel.ObservableCollection<CommandListItem> MovementCommandsList
        {
            get { if (_movementCommandsList == null) RefreshMovementCommands(); return _movementCommandsList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> AvailableMovementCommands
        {
            get { if (_availableMovementCommands == null) RefreshMovementCommands(); return _availableMovementCommands; }
        }

        public System.Collections.ObjectModel.ObservableCollection<IntPropertyListItem> ResistancesList
        {
            get { if (_resistancesList == null) RefreshResistances(); return _resistancesList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableIntPropertyItem> AvailableResistances
        {
            get { if (_availableResistances == null) RefreshResistances(); return _availableResistances; }
        }

        public System.Collections.ObjectModel.ObservableCollection<IntPropertyListItem> CombatList
        {
            get { if (_combatList == null) RefreshCombat(); return _combatList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableIntPropertyItem> AvailableCombat
        {
            get { if (_availableCombat == null) RefreshCombat(); return _availableCombat; }
        }

        public System.Collections.ObjectModel.ObservableCollection<IntPropertyListItem> AuraList
        {
            get { if (_auraList == null) RefreshAuras(); return _auraList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableIntPropertyItem> AvailableAuras
        {
            get { if (_availableAuras == null) RefreshAuras(); return _availableAuras; }
        }

        public System.Collections.ObjectModel.ObservableCollection<CommandListItem> SpecialCommandsList
        {
            get { if (_specialCommandsList == null) RefreshSpecialCommands(); return _specialCommandsList; }
        }

        public System.Collections.ObjectModel.ObservableCollection<AvailableCommandItem> AvailableSpecialCommands
        {
            get { if (_availableSpecialCommands == null) RefreshSpecialCommands(); return _availableSpecialCommands; }
        }

        private void RefreshTypeCommands()
        {
            _typeCommandsList = BuildCommandList(TypeCommands);
            _availableTypeCommands = BuildAvailableCommandList(_typeCommandsList, TypeCommands);
            OnPropertyChanged(nameof(TypeCommandsList));
            OnPropertyChanged(nameof(AvailableTypeCommands));
        }

        private void RefreshLeaderCommands()
        {
            _leaderCommandsList = BuildCommandList(LeaderCommands);
            _availableLeaderCommands = BuildAvailableCommandList(_leaderCommandsList, LeaderCommands);
            OnPropertyChanged(nameof(LeaderCommandsList));
            OnPropertyChanged(nameof(AvailableLeaderCommands));
        }

        private void RefreshMovementCommands()
        {
            _movementCommandsList = BuildCommandList(MovementCommands);
            _availableMovementCommands = BuildAvailableCommandList(_movementCommandsList, MovementCommands);
            OnPropertyChanged(nameof(MovementCommandsList));
            OnPropertyChanged(nameof(AvailableMovementCommands));
        }

        private void RefreshResistances()
        {
            _resistancesList = BuildIntPropertyList(ResistanceCommands);
            _availableResistances = BuildAvailableIntPropertyList(_resistancesList, ResistanceCommands);
            OnPropertyChanged(nameof(ResistancesList));
            OnPropertyChanged(nameof(AvailableResistances));
        }

        private void RefreshCombat()
        {
            _combatList = BuildIntPropertyList(CombatCommands);
            _availableCombat = BuildAvailableIntPropertyList(_combatList, CombatCommands);
            OnPropertyChanged(nameof(CombatList));
            OnPropertyChanged(nameof(AvailableCombat));
        }

        private void RefreshAuras()
        {
            _auraList = BuildIntPropertyList(AuraCommands);
            _availableAuras = BuildAvailableIntPropertyList(_auraList, AuraCommands);
            OnPropertyChanged(nameof(AuraList));
            OnPropertyChanged(nameof(AvailableAuras));
        }

        private void RefreshSpecialCommands()
        {
            _specialCommandsList = BuildCommandList(SpecialCommands);
            _availableSpecialCommands = BuildAvailableCommandList(_specialCommandsList, SpecialCommands);
            OnPropertyChanged(nameof(SpecialCommandsList));
            OnPropertyChanged(nameof(AvailableSpecialCommands));
        }

        public void AddTypeCommand(Command command) { SetCommandProperty(command, true); RefreshTypeCommands(); }
        public void RemoveTypeCommand(Command command) { SetCommandProperty(command, false); RefreshTypeCommands(); }
        public void AddLeaderCommand(Command command) { SetCommandProperty(command, true); RefreshLeaderCommands(); }
        public void RemoveLeaderCommand(Command command) { SetCommandProperty(command, false); RefreshLeaderCommands(); }
        public void AddMovementCommand(Command command) { SetCommandProperty(command, true); RefreshMovementCommands(); }
        public void RemoveMovementCommand(Command command) { SetCommandProperty(command, false); RefreshMovementCommands(); }
        public void AddResistance(Command command, int value) { SetIntProperty(command, value); RefreshResistances(); }
        public void RemoveResistance(Command command) { SetIntProperty(command, null); RefreshResistances(); }
        public void AddCombat(Command command, int value) { SetIntProperty(command, value); RefreshCombat(); }
        public void RemoveCombat(Command command) { SetIntProperty(command, null); RefreshCombat(); }
        public void AddAura(Command command, int value) { SetIntProperty(command, value); RefreshAuras(); }
        public void RemoveAura(Command command) { SetIntProperty(command, null); RefreshAuras(); }
        public void AddSpecialCommand(Command command) { SetCommandProperty(command, true); RefreshSpecialCommands(); }
        public void RemoveSpecialCommand(Command command) { SetCommandProperty(command, false); RefreshSpecialCommands(); }

        // ===== BADGE-BASED COLLECTIONS (Compact UI) =====
        // Three main sections: Types (read-only), General (non-combat), Combat, Resistances

        // Lazy-loaded badge configuration from JSON
        private static BadgeConfig? _badgeConfig;
        private static BadgeConfig BadgeConfig => _badgeConfig ??= BadgeConfigLoader.LoadConfig("monster") ?? new BadgeConfig();

        /// <summary>
        /// Builds badges for a section using JSON configuration.
        /// Uses layered property access: vanilla -> mod -> session changes.
        /// </summary>
        private (System.Collections.ObjectModel.ObservableCollection<PropertyItem> active,
                 System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem> available)
            BuildBadgesFromSection(string sectionId, EventHandler<int>? valueChangedHandler = null)
        {
            var active = new System.Collections.ObjectModel.ObservableCollection<PropertyItem>();
            var available = new System.Collections.ObjectModel.ObservableCollection<AvailablePropertyItem>();
            var usedCommands = new HashSet<Command>();

            var section = BadgeConfig.GetSection(sectionId);
            if (section == null)
            {
                return (active, available);
            }

            // Get vanilla entity for layered comparison
            var vanillaEntity = GetVanillaEntity();

            foreach (var cmdDef in section.Commands)
            {
                if (!BadgeConfigLoader.TryGetCommand(cmdDef, out var command))
                    continue;

                // Layer 1: Get vanilla value (base layer, includes vanilla copystats)
                bool vanillaHasValue = false;
                bool vanillaIsCopied = false;
                int? vanillaValue = null;
                bool vanillaFlagValue = false;

                if (vanillaEntity != null)
                {
                    if (cmdDef.IsFlag)
                    {
                        var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);
                        vanillaFlagValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;
                        vanillaHasValue = vanillaFlagValue;
                        vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                    }
                    else if (cmdDef.IsInt)
                    {
                        var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            vanillaValue = vanillaProp?.Value;
                            vanillaHasValue = true;
                            vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                        }
                    }
                }

                // Layer 2: Get current entity value (mod + session, includes mod copystats)
                bool entityHasValue = false;
                bool entityIsCopied = false;
                bool entityHasDirect = false; // Has the property directly (not from copystats)
                int? entityValue = null;
                bool entityFlagValue = false;

                if (cmdDef.IsFlag)
                {
                    var entityResult = _entity.TryGet<CommandProperty>(command, out _);
                    entityFlagValue = entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED;
                    entityHasValue = entityFlagValue;
                    entityIsCopied = entityResult == ReturnType.COPIED;
                    entityHasDirect = entityResult == ReturnType.TRUE;
                }
                else if (cmdDef.IsInt)
                {
                    var entityResult = _entity.TryGet<IntProperty>(command, out var entityProp);
                    if (entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED)
                    {
                        entityValue = entityProp?.Value;
                        entityHasValue = true;
                        entityIsCopied = entityResult == ReturnType.COPIED;
                        entityHasDirect = entityResult == ReturnType.TRUE;
                    }
                }

                // Determine effective value (entity overrides vanilla)
                bool hasValue = entityHasValue || vanillaHasValue;
                int? effectiveValue = entityHasValue ? entityValue : vanillaValue;
                bool effectiveFlagValue = entityHasValue ? entityFlagValue : vanillaFlagValue;

                // Determine modification and inheritance status
                bool isModified = false;
                bool isInherited = false;
                bool isSessionEdit = IsPropertyEditedInSession(command);

                if (cmdDef.IsFlag)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (entityFlagValue != vanillaFlagValue);
                    // Inherited if value comes from copystats or vanilla (not directly on entity)
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }
                else if (cmdDef.IsInt)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (!vanillaHasValue || entityValue != vanillaValue);
                    // Inherited if value comes from copystats or vanilla (not directly on entity)
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }

                if (hasValue && (cmdDef.IsFlag ? effectiveFlagValue : true))
                {
                    var badge = BadgeConfigLoader.CreatePropertyItem(cmdDef, effectiveValue, isModified, isSessionEdit);
                    // Can only remove if property is directly on the entity (not inherited/copied)
                    badge.CanRemove = !section.ReadOnly && entityHasDirect;
                    badge.IsInherited = section.ReadOnly || isInherited;

                    if (valueChangedHandler != null && cmdDef.IsInt)
                    {
                        badge.ValueChanged += valueChangedHandler;
                    }

                    active.Add(badge);
                    usedCommands.Add(command);
                }
            }

            // Build available list (excluding already used commands)
            if (!section.ReadOnly)
            {
                foreach (var cmdDef in section.Commands)
                {
                    if (BadgeConfigLoader.TryGetCommand(cmdDef, out var command) && !usedCommands.Contains(command))
                    {
                        available.Add(BadgeConfigLoader.CreateAvailableItem(cmdDef));
                    }
                }
            }

            return (active, available);
        }

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

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeGeneralBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addGeneralBadgeCommand;
        private RelayCommand<PropertyItem> _removeCombatBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCombatBadgeCommand;
        private RelayCommand<PropertyItem> _removeResistanceBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResistanceBadgeCommand;

        public RelayCommand<PropertyItem> RemoveGeneralBadgeCommand => _removeGeneralBadgeCommand ??= new RelayCommand<PropertyItem>(b =>
        {
            if (b.HasValue) SetIntProperty(b.Command, null);
            else SetCommandProperty(b.Command, false);
            RefreshGeneralBadges();
        });

        public RelayCommand<AvailablePropertyItem> AddGeneralBadgeCommand => _addGeneralBadgeCommand ??= new RelayCommand<AvailablePropertyItem>(b =>
        {
            if (b.DefaultValue.HasValue) SetIntProperty(b.Command, b.DefaultValue.Value);
            else SetCommandProperty(b.Command, true);
            RefreshGeneralBadges();
        });

        public RelayCommand<PropertyItem> RemoveCombatBadgeCommand => _removeCombatBadgeCommand ??= new RelayCommand<PropertyItem>(b => { SetIntProperty(b.Command, null); RefreshCombatBadges(); });
        public RelayCommand<AvailablePropertyItem> AddCombatBadgeCommand => _addCombatBadgeCommand ??= new RelayCommand<AvailablePropertyItem>(b => { SetIntProperty(b.Command, b.DefaultValue ?? 1); RefreshCombatBadges(); });
        public RelayCommand<PropertyItem> RemoveResistanceBadgeCommand => _removeResistanceBadgeCommand ??= new RelayCommand<PropertyItem>(b => { SetIntProperty(b.Command, null); RefreshResistanceBadges(); });
        public RelayCommand<AvailablePropertyItem> AddResistanceBadgeCommand => _addResistanceBadgeCommand ??= new RelayCommand<AvailablePropertyItem>(b => { SetIntProperty(b.Command, b.DefaultValue ?? 0); RefreshResistanceBadges(); });

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
            var (active, available) = BuildBadgesFromSection("general", OnGeneralBadgeValueChanged);
            _generalBadges = active;
            _availableGeneralBadges = available;
            OnPropertyChanged(nameof(GeneralBadges));
            OnPropertyChanged(nameof(AvailableGeneralBadges));
        }

        private void OnGeneralBadgeValueChanged(object sender, int newValue)
        {
            if (sender is PropertyItem badge)
            {
                SetIntProperty(badge.Command, newValue);
            }
        }

        private void RefreshCombatBadges()
        {
            var (active, available) = BuildBadgesFromSection("combat", OnCombatBadgeValueChanged);
            _combatBadges = active;
            _availableCombatBadges = available;
            OnPropertyChanged(nameof(CombatBadges));
            OnPropertyChanged(nameof(AvailableCombatBadges));
        }

        private void OnCombatBadgeValueChanged(object sender, int newValue)
        {
            if (sender is PropertyItem badge)
            {
                SetIntProperty(badge.Command, newValue);
            }
        }

        private void RefreshResistanceBadges()
        {
            var (active, available) = BuildBadgesFromSection("resistances", OnResistanceBadgeValueChanged);
            _resistanceBadges = active;
            _availableResistanceBadges = available;
            OnPropertyChanged(nameof(ResistanceBadges));
            OnPropertyChanged(nameof(AvailableResistanceBadges));
        }

        private void OnResistanceBadgeValueChanged(object sender, int newValue)
        {
            if (sender is PropertyItem badge)
            {
                SetIntProperty(badge.Command, newValue);
            }
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

            // Refresh command lists when related commands change
            RefreshCommandLists();
            RefreshIntPropertyLists();
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

        /// <summary>
        /// Refreshes all command list collections (movement, type, leader, special).
        /// </summary>
        private void RefreshCommandLists()
        {
            OnPropertyChanged(nameof(MovementCommandsList));
            OnPropertyChanged(nameof(AvailableMovementCommands));
            OnPropertyChanged(nameof(TypeCommandsList));
            OnPropertyChanged(nameof(AvailableTypeCommands));
            OnPropertyChanged(nameof(LeaderCommandsList));
            OnPropertyChanged(nameof(AvailableLeaderCommands));
            OnPropertyChanged(nameof(SpecialCommandsList));
            OnPropertyChanged(nameof(AvailableSpecialCommands));
        }

        /// <summary>
        /// Refreshes all int property list collections (combat, resistances).
        /// </summary>
        private void RefreshIntPropertyLists()
        {
            OnPropertyChanged(nameof(CombatList));
            OnPropertyChanged(nameof(AvailableCombat));
            OnPropertyChanged(nameof(ResistancesList));
            OnPropertyChanged(nameof(AvailableResistances));
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

        private void RefreshWeaponsList()
        {
            _weaponsList = new ObservableCollection<EquipmentItem>();
            var vanillaEntity = GetVanillaEntity();

            // Get all weapon references from the entity (including copied)
            var directWeapons = new HashSet<int>();
            foreach (var prop in _entity.GetMultiple(Command.WEAPON))
            {
                if (prop is WeaponRef weaponRef && weaponRef.HasValue)
                {
                    directWeapons.Add(weaponRef.ID);
                    var item = new EquipmentItem
                    {
                        ID = weaponRef.ID,
                        Name = weaponRef.Entity?.Name,
                        IsModified = IsWeaponModified(weaponRef.ID),
                        IsSessionEdit = IsPropertyEditedInSession(Command.WEAPON),
                        IsInherited = false,
                        SourceCommand = Command.WEAPON
                    };
                    _weaponsList.Add(item);
                }
            }

            // Add inherited weapons from copystats
            if (_entity.TryGetCopyFrom(out var copyFrom) && copyFrom != null)
            {
                AddInheritedWeapons(copyFrom, directWeapons, new HashSet<IDEntity> { _entity });
            }

            OnPropertyChanged(nameof(WeaponsList));
        }

        private void AddInheritedWeapons(IDEntity source, HashSet<int> directWeapons, HashSet<IDEntity> visited)
        {
            if (visited.Contains(source))
                return;
            visited.Add(source);

            foreach (var prop in source.GetMultiple(Command.WEAPON))
            {
                if (prop is WeaponRef weaponRef && weaponRef.HasValue)
                {
                    if (!directWeapons.Contains(weaponRef.ID))
                    {
                        directWeapons.Add(weaponRef.ID);
                        var item = new EquipmentItem
                        {
                            ID = weaponRef.ID,
                            Name = weaponRef.Entity?.Name,
                            IsModified = false,
                            IsSessionEdit = false,
                            IsInherited = true,
                            SourceCommand = Command.WEAPON
                        };
                        _weaponsList.Add(item);
                    }
                }
            }

            // Recurse through copystats chain
            if (source.TryGetCopyFrom(out var nextCopy) && nextCopy != null)
            {
                AddInheritedWeapons(nextCopy, directWeapons, visited);
            }
        }

        private bool IsWeaponModified(int weaponId)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
                return true; // New entity, so it's modified

            // Check if vanilla has this weapon
            var vanillaWeapons = vanillaEntity.GetMultiple(Command.WEAPON)
                .OfType<WeaponRef>()
                .Where(w => w.HasValue)
                .Select(w => w.ID)
                .ToHashSet();

            return !vanillaWeapons.Contains(weaponId);
        }

        private void RefreshArmorList()
        {
            _armorList = new ObservableCollection<EquipmentItem>();
            var vanillaEntity = GetVanillaEntity();

            // Get all armor references from the entity (including copied)
            var directArmor = new HashSet<int>();
            foreach (var prop in _entity.GetMultiple(Command.ARMOR))
            {
                if (prop is ArmorRef armorRef && armorRef.HasValue)
                {
                    directArmor.Add(armorRef.ID);
                    var item = new EquipmentItem
                    {
                        ID = armorRef.ID,
                        Name = armorRef.Entity?.Name,
                        IsModified = IsArmorModified(armorRef.ID),
                        IsSessionEdit = IsPropertyEditedInSession(Command.ARMOR),
                        IsInherited = false,
                        SourceCommand = Command.ARMOR
                    };
                    _armorList.Add(item);
                }
            }

            // Add inherited armor from copystats
            if (_entity.TryGetCopyFrom(out var copyFrom) && copyFrom != null)
            {
                AddInheritedArmor(copyFrom, directArmor, new HashSet<IDEntity> { _entity });
            }

            OnPropertyChanged(nameof(ArmorList));
        }

        private void AddInheritedArmor(IDEntity source, HashSet<int> directArmor, HashSet<IDEntity> visited)
        {
            if (visited.Contains(source))
                return;
            visited.Add(source);

            foreach (var prop in source.GetMultiple(Command.ARMOR))
            {
                if (prop is ArmorRef armorRef && armorRef.HasValue)
                {
                    if (!directArmor.Contains(armorRef.ID))
                    {
                        directArmor.Add(armorRef.ID);
                        var item = new EquipmentItem
                        {
                            ID = armorRef.ID,
                            Name = armorRef.Entity?.Name,
                            IsModified = false,
                            IsSessionEdit = false,
                            IsInherited = true,
                            SourceCommand = Command.ARMOR
                        };
                        _armorList.Add(item);
                    }
                }
            }

            // Recurse through copystats chain
            if (source.TryGetCopyFrom(out var nextCopy) && nextCopy != null)
            {
                AddInheritedArmor(nextCopy, directArmor, visited);
            }
        }

        private bool IsArmorModified(int armorId)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
                return true; // New entity, so it's modified

            // Check if vanilla has this armor
            var vanillaArmor = vanillaEntity.GetMultiple(Command.ARMOR)
                .OfType<ArmorRef>()
                .Where(a => a.HasValue)
                .Select(a => a.ID)
                .ToHashSet();

            return !vanillaArmor.Contains(armorId);
        }

        private List<AvailableEquipmentItem> _availableWeapons;
        public List<AvailableEquipmentItem> AvailableWeapons
        {
            get
            {
                if (_availableWeapons == null)
                    RefreshAvailableWeapons();
                return _availableWeapons;
            }
        }

        private List<AvailableEquipmentItem> _availableArmor;
        public List<AvailableEquipmentItem> AvailableArmor
        {
            get
            {
                if (_availableArmor == null)
                    RefreshAvailableArmor();
                return _availableArmor;
            }
        }

        private void RefreshAvailableWeapons()
        {
            _availableWeapons = new List<AvailableEquipmentItem>();

            // Add vanilla weapons
            if (VanillaLoader.Vanilla?.Database.TryGetValue(EntityType.WEAPON, out var vanillaSet) == true)
            {
                foreach (var entity in vanillaSet.GetFullList())
                {
                    if (entity is Weapon weapon)
                    {
                        _availableWeapons.Add(new AvailableEquipmentItem
                        {
                            ID = weapon.ID,
                            Name = weapon.Name,
                            Source = "Vanilla"
                        });
                    }
                }
            }

            // Add mod weapons (if loaded)
            var mod = _entity.ParentMod;
            if (mod != null && mod.Database.TryGetValue(EntityType.WEAPON, out var modSet))
            {
                foreach (var entity in modSet.GetFullList())
                {
                    if (entity is Weapon weapon && !_availableWeapons.Any(w => w.ID == weapon.ID))
                    {
                        _availableWeapons.Add(new AvailableEquipmentItem
                        {
                            ID = weapon.ID,
                            Name = weapon.Name,
                            Source = "Mod"
                        });
                    }
                }
            }

            _availableWeapons = _availableWeapons.OrderBy(w => w.ID).ToList();
            OnPropertyChanged(nameof(AvailableWeapons));
        }

        private void RefreshAvailableArmor()
        {
            _availableArmor = new List<AvailableEquipmentItem>();

            // Add vanilla armor
            if (VanillaLoader.Vanilla?.Database.TryGetValue(EntityType.ARMOR, out var vanillaSet) == true)
            {
                foreach (var entity in vanillaSet.GetFullList())
                {
                    if (entity is Armor armor)
                    {
                        _availableArmor.Add(new AvailableEquipmentItem
                        {
                            ID = armor.ID,
                            Name = armor.Name,
                            Source = "Vanilla"
                        });
                    }
                }
            }

            // Add mod armor (if loaded)
            var mod = _entity.ParentMod;
            if (mod != null && mod.Database.TryGetValue(EntityType.ARMOR, out var modSet))
            {
                foreach (var entity in modSet.GetFullList())
                {
                    if (entity is Armor armor && !_availableArmor.Any(a => a.ID == armor.ID))
                    {
                        _availableArmor.Add(new AvailableEquipmentItem
                        {
                            ID = armor.ID,
                            Name = armor.Name,
                            Source = "Mod"
                        });
                    }
                }
            }

            _availableArmor = _availableArmor.OrderBy(a => a.ID).ToList();
            OnPropertyChanged(nameof(AvailableArmor));
        }

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
    }
}
