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
                Mod m = new Mod();
                m.Parse(dmFile);
                Mods.Add(m);
            }
        }

        public void Export(string folder)
        {
            //foreach (Mod m in Mods)
            //{
            using (StreamWriter writer = new StreamWriter(folder + "\\testing.dm"))
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
                var finalMonsters = finalMod.Monsters;

                foreach (var kvp in m.Monsters)
                {
                    if (kvp.Key < 3500 && kvp.Value.Selected)
                    {
                        //select monster command on a vanilla
                        if (!finalMonsters.ContainsKey(kvp.Key))
                        {
                            finalMonsters.Add(kvp.Key, kvp.Value);
                        }
                    }
                    else if (kvp.Key < 3500)
                    {
                        //new monster on a vanilla ID?
                    }
                    else if (!kvp.Value.Selected)
                    {
                        //assign a new ID upwards
                        int newID = finalMod.GetNextMonsterID();
                        int oldID = kvp.Key;

                        kvp.Value.ID = newID;
                        if (m.MonsterIDMap.ContainsKey(oldID))
                        {
                            foreach (var idref in m.MonsterIDMap[oldID])
                            {
                                idref.ID = newID; //adjust every reference to this ID to the new one
                            }
                        }
                        finalMonsters.Add(newID, kvp.Value);
                    }
                    else //a select monster on a mod ID?
                    {
                        finalMonsters.Add(kvp.Key, kvp.Value); //no conflict, so just add it as is
                    }
                }
            }
            finalizedmod = finalMod;
            var i = 9;
            i++;
        }
    }
}
