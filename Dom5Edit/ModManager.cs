using Dom5Edit.Entities;
using Dom5Edit.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dom5Edit
{
    public class ModManager
    {
        public Dictionary<string, MethodInfo> Initializer = new Dictionary<string, MethodInfo>();
        public List<string> DisabledNations = new List<string>();

        public List<Mod> Mods = new List<Mod>();

        public string _ModName = "merged-mod";

        internal static int MONSTER_START_ID = 3486;
        internal static int SITE_START_ID = 1164;
        internal static int EVENT_START_ID = 6000;
        internal static int ARMOR_START_ID = 251;
        internal static int WEAPON_START_ID = 763;
        internal static int ITEM_START_ID = 446;
        internal static int SPELL_START_ID = 1177;
        internal static int NAMETYPE_START_ID = 170;
        internal static int NATION_START_ID = 109;
        internal static int MONTAG_START_ID = 1000;
        internal static int RESTRICTED_ITEM_START_ID = 1;
        internal static int ENCHANTMENT_START_ID = 106;
        internal static int EVENT_CODE_START_ID = -300;
        internal static int EVENT_CODE_EFFECT_START_ID = 14;

        public ModManager()
        {
        }

        public void Import(string folder, List<string> files, bool log)
        {
            //Startup script
            //string localPath = Path.GetDirectoryName(folder);
            //string[] dmFiles = Directory.GetFiles(folder, "*.dm");

            foreach (string dmFile in files)
            {
                string fileName = System.IO.Path.Combine(folder, dmFile);
                if (fileName.StartsWith(_ModName)) continue;
                Mod m = new Mod();
                m.Logging = log;
                m.ModFileName = dmFile;
                m.Parse(fileName);
                Mods.Add(m);
            }

            foreach (Mod m in Mods)
            {
                m.ResolveDependencies(Mods);
                m.Resolve();
            }
        }

        public void Export(string folder)
        {
            using (StreamWriter writer = new StreamWriter(folder + "\\" + _ModName + ".dm"))
            {
                finalizedmod.GenerateDisabledMages(DisabledNations);
                finalizedmod.Export(writer);
            }
        }

        public void ExportMagicPaths(string folder)
        {
            string separator = "\t";
            using (StreamWriter writer = new StreamWriter(folder + "\\mod_magicpaths.txt"))
            {
                Dictionary<string, double[]> nationStuff = new Dictionary<string, double[]>();
                Dictionary<string, double[]> offCapStuff = new Dictionary<string, double[]>();
                foreach (var mod in Mods)
                {
                    foreach (IDEntity e in mod.Nations.Values)
                    {
                        Nation n = e as Nation;
                        if (n.ID < ModManager.NATION_START_ID) continue;
                        bool hasName = n.TryGetName(out string name);
                        var era = n.Era;
                        if (era != null && era.HasValue)
                        {
                            int nationEra = era.Value;
                            switch (nationEra)
                            {
                                case 1:
                                    name = "EA " + name;
                                    break;
                                case 2:
                                    name = "MA " + name;
                                    break;
                                case 3:
                                    name = "LA " + name;
                                    break;
                            }
                        }
                        writer.Write("Nation: " + (hasName ? name : n.ID.ToString()));

                        writer.WriteLine();
                        double[] totalArr = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                        //get recruitables
                        foreach (var m in n.Commanders)
                        {
                            writer.Write("OFF" + separator + m.ID);
                            var arr = GetMagicPaths(m);

                            for (int i = 0; i < arr.Length; i++)
                            {
                                double d = arr[i];
                                totalArr[i] = Math.Max(arr[i], totalArr[i]);
                                writer.Write(separator + d);
                            }
                            writer.WriteLine();
                        }
                        double[] offCap = new double[9];
                        for (int i = 0; i < offCap.Length; i++)
                        {
                            offCap[i] = totalArr[i];
                        }
                        //get cap sites
                        foreach (var s in n.Sites)
                        {
                            foreach (var m in s.HomeCommanders)
                            {
                                writer.Write("CAP" + separator + m.ID);

                                var arr = GetMagicPaths(m);

                                for (int i = 0; i < arr.Length; i++)
                                {
                                    double d = arr[i];
                                    totalArr[i] = Math.Max(arr[i], totalArr[i]);
                                    writer.Write(separator + d);
                                }
                                writer.WriteLine();
                            }
                        }



                        writer.Write(hasName ? name : n.ID.ToString());
                        writer.WriteLine(" -- only off-cap included.");
                        writer.Write(hasName ? name : n.ID.ToString());
                        for (int i = 0; i < offCap.Length; i++)
                        {
                            double d = offCap[i];
                            writer.Write(separator + d);
                        }
                        writer.WriteLine();
                        offCapStuff.Add(hasName ? name : n.ID.ToString(), offCap);

                        writer.Write(hasName ? name : n.ID.ToString());
                        writer.WriteLine(" -- cap mages included.");
                        writer.Write(hasName ? name : n.ID.ToString());
                        for (int i = 0; i < totalArr.Length; i++)
                        {
                            double d = totalArr[i];
                            writer.Write(separator + d);
                        }
                        writer.WriteLine();
                        writer.WriteLine();
                        nationStuff.Add(hasName ? name : n.ID.ToString(), totalArr);
                    }
                }
                writer.WriteLine();
                writer.WriteLine("OFF CAP");
                foreach (var kvp in offCapStuff)
                {
                    writer.Write(kvp.Key);
                    for (int i = 0; i < kvp.Value.Length; i++)
                    {
                        double d = kvp.Value[i];
                        writer.Write(separator + d);
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();
                writer.WriteLine("ON CAP");
                foreach (var kvp in nationStuff)
                {
                    writer.Write(kvp.Key);
                    for (int i = 0; i < kvp.Value.Length; i++)
                    {
                        double d = kvp.Value[i];
                        writer.Write(separator + d);
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();
            }
        }

        private double[] GetMagicPaths(Monster m)
        {
            double[] arr = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var magic in m.MagicSkills)
            {
                switch (magic.Path)
                {
                    case Commands.MagicPaths.FIRE:
                        arr[0] = Math.Max(magic.Level, arr[0]);
                        break;
                    case Commands.MagicPaths.AIR:
                        arr[1] = Math.Max(magic.Level, arr[1]);
                        break;
                    case Commands.MagicPaths.WATER:
                        arr[2] = Math.Max(magic.Level, arr[2]);
                        break;
                    case Commands.MagicPaths.EARTH:
                        arr[3] = Math.Max(magic.Level, arr[3]);
                        break;
                    case Commands.MagicPaths.ASTRAL:
                        arr[4] = Math.Max(magic.Level, arr[4]);
                        break;
                    case Commands.MagicPaths.DEATH:
                        arr[5] = Math.Max(magic.Level, arr[5]);
                        break;
                    case Commands.MagicPaths.NATURE:
                        arr[6] = Math.Max(magic.Level, arr[6]);
                        break;
                    case Commands.MagicPaths.BLOOD:
                        arr[7] = Math.Max(magic.Level, arr[7]);
                        break;
                    case Commands.MagicPaths.PRIEST:
                        arr[8] = Math.Max(magic.Level, arr[8]);
                        break;
                    case Commands.MagicPaths.ELEMENTAL:
                        arr[0] = Math.Max(magic.Level, arr[0]);
                        arr[1] = Math.Max(magic.Level, arr[1]);
                        arr[2] = Math.Max(magic.Level, arr[2]);
                        arr[3] = Math.Max(magic.Level, arr[3]);
                        break;
                    case Commands.MagicPaths.SORCERY:
                        arr[4] = Math.Max(magic.Level, arr[4]);
                        arr[5] = Math.Max(magic.Level, arr[5]);
                        arr[6] = Math.Max(magic.Level, arr[6]);
                        arr[7] = Math.Max(magic.Level, arr[7]);
                        break;
                    case Commands.MagicPaths.ALL:
                        arr[0] = Math.Max(magic.Level, arr[0]);
                        arr[1] = Math.Max(magic.Level, arr[1]);
                        arr[2] = Math.Max(magic.Level, arr[2]);
                        arr[3] = Math.Max(magic.Level, arr[3]);
                        arr[4] = Math.Max(magic.Level, arr[4]);
                        arr[5] = Math.Max(magic.Level, arr[5]);
                        arr[6] = Math.Max(magic.Level, arr[6]);
                        arr[7] = Math.Max(magic.Level, arr[7]);
                        break;
                }
            }
            foreach (var magic in m.CustomMagic)
            {
                foreach (var mpath in magic.Path)
                {
                    switch (mpath)
                    {
                        case Commands.MagicPaths.FIRE:
                            arr[0] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.AIR:
                            arr[1] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.WATER:
                            arr[2] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.EARTH:
                            arr[3] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.ASTRAL:
                            arr[4] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.DEATH:
                            arr[5] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.NATURE:
                            arr[6] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.BLOOD:
                            arr[7] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                        case Commands.MagicPaths.PRIEST:
                            arr[8] += (magic.Chance > .1) ? Math.Ceiling(magic.Chance) : 0;
                            break;
                    }
                }
            }
            return arr;
        }

        private Mod finalizedmod;
        public void Merge()
        {
            Mod finalMod = new Mod();

            finalMod.ModName = "Merged Mod";
            finalMod.Description = "A merger of all valid mods that were parsed";
            finalMod.Version = "1.0";
            finalMod.DomVersion = "5.00";

            foreach (Mod m in Mods)
            {
                foreach (int referenced in m.VanillaMageReferences)
                {
                    //slow but meh
                    if (!finalMod.VanillaMageReferences.Contains(referenced)) finalMod.VanillaMageReferences.Add(referenced);
                }
                //add monsters
                Merge(m.Monsters, m.NamedMonsters, finalMod.Monsters, finalMod.NamedMonsters, MONSTER_START_ID, finalMod.GetNextMonsterID);
                Merge(m.Armors, m.NamedArmors, finalMod.Armors, finalMod.NamedArmors, ARMOR_START_ID, finalMod.GetNextArmorID);
                Merge(m.Weapons, m.NamedWeapons, finalMod.Weapons, finalMod.NamedWeapons, WEAPON_START_ID, finalMod.GetNextWeaponID);
                Merge(m.Nametypes, null, finalMod.Nametypes, null, NAMETYPE_START_ID, finalMod.GetNextNametypeID, true);
                Merge(m.Sites, m.NamedSites, finalMod.Sites, finalMod.NamedSites, SITE_START_ID, finalMod.GetNextSiteID);
                MergeExtraSites(m.SitesThatNeedIDs, finalMod);
                Merge(m.Items, m.NamedItems, finalMod.Items, finalMod.NamedItems, ITEM_START_ID, finalMod.GetNextItemID);
                MergeExtraItems(m.ItemsWithNoNameYet, finalMod);
                MergeNations(m.Nations, finalMod.Nations, m.NationsWithNoID, NATION_START_ID, finalMod.GetNextNationID);
                MergeMontags(m.Montags.Values.ToList(), finalMod);

                Merge(m.Spells, m.NamedSpells, finalMod.Spells, finalMod.NamedSpells, SPELL_START_ID, finalMod.GetNextSpellID);
                MergeExtraSpells(m.SpellsWithNoNameYet, finalMod);

                //Migrate mercs, no conflicts as long as ID's adjusted internally
                foreach (var kvp in m.NamedMercenaries)
                {
                    finalMod.NamedMercenaries.Add(kvp.Key, kvp.Value);
                }
                //general settings (is this even necessary? lol)

                //poptypes
                foreach (var kvp in m.Poptypes)
                {
                    if (!finalMod.Poptypes.ContainsKey(kvp.Key)) finalMod.Poptypes.Add(kvp.Key, kvp.Value);
                }
                //restricted item codes
                MergeRestrictedItems(m.RestrictedItems.Values.ToList(), finalMod);

                //events, just migrate over since they've been adjusted
                foreach (var kvp in m.Events)
                {
                    finalMod.Events.Add(kvp);
                }

                //event codes
                MergeEventCodes(m.EventCodes.Values.ToList(), finalMod);
                MergeEnchantments(m.Enchantments.Values.ToList(), finalMod);
                MergeEventEffectCodes(m.EventEffectCodes.Values.ToList(), finalMod);

            }
            finalizedmod = finalMod;
        }

        public void Merge(Dictionary<int, IDEntity> current, Dictionary<string, IDEntity> named, Dictionary<int, IDEntity> final, Dictionary<string, IDEntity> finalNamed, int START_ID, Func<int> nextID, bool force = false)
        {
            if (force)
            {
                foreach (var kvp in current)
                {
                    if (kvp.Key < START_ID)
                    {
                        final.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        int newID = nextID.Invoke();
                        kvp.Value.ID = newID;
                        final.Add(newID, kvp.Value);
                    }
                }
                return;
            }
            foreach (var kvp in current)
            {
                if (kvp.Key < START_ID && kvp.Value.Selected)
                {
                    //select monster command on a vanilla
                    if (!final.ContainsKey(kvp.Key))
                    {
                        final.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        final[kvp.Key].Properties.AddRange(kvp.Value.Properties);
                    }
                }
                else if (kvp.Key < START_ID)
                {
                    //new monster on a vanilla ID?
                }
                else// if (!kvp.Value.Selected)
                {
                    //assign a new ID upwards
                    int newID = nextID.Invoke();
                    kvp.Value.ID = newID;
                    final.Add(newID, kvp.Value);
                }/*
                else //a select monster on a mod ID?
                {
                }*/
            }

            if (named != null)
            {
                foreach (var kvp in named)
                {
                    if (kvp.Value.Selected && kvp.Value.Named) //else it was already exported
                    {
                        if (kvp.Value.ID != -1 && !current.ContainsKey(kvp.Value.ID) && !finalNamed.ContainsKey(kvp.Key)) //make sure it wasn't already exported through some mixed references
                        {
                            finalNamed.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (kvp.Value.ID == -1 && !finalNamed.ContainsKey(kvp.Key))
                    {
                        int newID = nextID.Invoke();
                        kvp.Value.ID = newID;
                        final.Add(newID, kvp.Value);
                    }
                }
            }
        }

        public void MergeNations(Dictionary<int, IDEntity> current, Dictionary<int, IDEntity> final, List<IDEntity> needsIDs, int START_ID, Func<int> nextID)
        {
            foreach (var kvp in current)
            {
                if (kvp.Key < START_ID)
                {
                    if (!final.ContainsKey(kvp.Key))
                    {
                        final.Add(kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    //assign a new ID upwards
                    int newID = nextID.Invoke();
                    kvp.Value.ID = newID;
                    final.Add(newID, kvp.Value);
                }
            }

            foreach (var entity in needsIDs)
            {
                int newID = nextID.Invoke();
                entity.ID = newID;
                final.Add(newID, entity);
            }
        }

        public void MergeExtraSites(List<IDEntity> items, Mod finalMod)
        {
            foreach (IDEntity item in items)
            {
                item.ID = finalMod.GetNextSiteID();
                finalMod.Sites.Add(item.ID, item);
            }
        }

        public void MergeExtraItems(List<IDEntity> items, Mod finalMod)
        {
            foreach (IDEntity item in items)
            {
                item.ID = finalMod.GetNextItemID();
                finalMod.Items.Add(item.ID, item);
            }
        }

        public void MergeExtraSpells(List<IDEntity> spells, Mod finalMod)
        {
            foreach (IDEntity spell in spells)
            {
                spell.ID = finalMod.GetNextSpellID();
                finalMod.Spells.Add(spell.ID, spell);
            }
        }

        public void MergeMontags(List<Montag> items, Mod finalMod)
        {
            foreach (Montag item in items)
            {
                if (Montag.MontagConstants.Contains(item.MontagID)) continue;
                if (item.DependentMontag == null)
                {
                    item.MontagID = finalMod.GetNextMontagID();
                    finalMod.Montags.Add(item.GetID(), item); //since the montags are all properties on an object, there's no exporting required; this is just keeping ID's consistent
                }
            }
        }

        public void MergeRestrictedItems(List<RestrictedItem> items, Mod finalMod)
        {
            foreach (RestrictedItem item in items)
            {
                if (item.DependentRestrictedItem == null)
                {
                    item.RestrictedItemID = finalMod.GetNextRestrictedItemID();
                    finalMod.RestrictedItems.Add(item.GetID(), item);
                }
            }
        }

        public void MergeEnchantments(List<Enchantment> items, Mod finalMod)
        {
            foreach (Enchantment item in items)
            {
                if (item.DependentEnchantment == null && item.EnchID >= 106)
                {
                    item.EnchID = finalMod.GetNextEnchantmentID();
                    finalMod.Enchantments.Add(item.GetID(), item);
                }
            }
        }

        public void MergeEventCodes(List<EventCode> items, Mod finalMod)
        {
            foreach (EventCode item in items)
            {
                if (item.DependentEventCode == null)
                {
                    item.EventCodeID = finalMod.GetNextEventCodeID();
                    finalMod.EventCodes.Add(item.GetID(), item);
                }
            }
        }

        public void MergeEventEffectCodes(List<EventEffectCode> items, Mod finalMod)
        {
            foreach (EventEffectCode item in items)
            {
                if (item.DependentEventEffectCode == null)
                {
                    item.EventEffectCodeID = finalMod.GetNextEventEffectCodeStartID();
                    finalMod.EventEffectCodes.Add(item.GetID(), item);
                }
            }
        }
    }
}
