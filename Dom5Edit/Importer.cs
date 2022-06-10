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
    public class Importer
    {
        public Dictionary<string, MethodInfo> Initializer = new Dictionary<string, MethodInfo>();

        public List<Mod> Mods = new List<Mod>();

        private string _ModName = "merged-mod.dm";

        internal static int MONSTER_START_ID = 3500;
        internal static int SITE_START_ID = 1500;
        internal static int EVENT_START_ID = 6000;
        internal static int ARMOR_START_ID = 300;
        internal static int WEAPON_START_ID = 800;
        internal static int ITEM_START_ID = 500;
        internal static int SPELL_START_ID = 1300;
        internal static int NAMETYPE_START_ID = 170;
        internal static int NATION_START_ID = 120;
        internal static int MONTAG_START_ID = 1000;
        internal static int RESTRICTED_ITEM_START_ID = 1;
        internal static int ENCHANTMENT_START_ID = 106;
        internal static int EVENT_CODE_START_ID = -300;
        internal static int EVENT_CODE_EFFECT_START_ID = 14;

        public Importer()
        {
        }

        public void Run(string folder)
        {
            //Startup script
            //string localPath = Path.GetDirectoryName(folder);
            string[] dmFiles = Directory.GetFiles(folder, "*.dm");

            foreach (string dmFile in dmFiles)
            {
                string fileName = System.IO.Path.GetFileName(dmFile);
                if (fileName.Contains(_ModName)) continue;
                Mod m = new Mod();
                m.ModFileName = fileName;
                m.Parse(dmFile);
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
            //foreach (Mod m in Mods)
            //{
            using (StreamWriter writer = new StreamWriter(folder + "\\" + _ModName))
            {
                finalizedmod.Export(writer);
            }
            // }
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
