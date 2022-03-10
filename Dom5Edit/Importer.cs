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
            //todo: find all classes implementing IDEntity that have a Create method
            //Initializer.Add(Monster.Import, Monster.Create); what the below does, but generically
            /* simple code now
            var types = TypeManager.Instance.GetSubclasses<Entity>();
            foreach (var type in types)
            {

                var create = type.GetMethod("Create");
                if (create == null || create.ReturnType == null || !create.ReturnType.IsSubclassOf(typeof(Entity)) || !create.IsStatic)
                {
                    //does not have a create method returning an entity
                    continue;
                }
                var import = type.GetMethod("GetImport");
                if (import == null || import.ReturnType == null || import.ReturnType != typeof(string) || !import.IsStatic)
                {
                    //does not have a statement to get the import string
                    continue;
                }
                Initializer.Add((string)import.Invoke(null, null), create);
            }

            //null check is end marker
            Initializer.Add("#end", null);
            */
        }

        public void Run()
        {
            //Startup script
            string localPath = Path.GetDirectoryName(Application.ExecutablePath);
            string[] dmFiles = Directory.GetFiles(localPath, "*.dm");


            foreach (string dmFile in dmFiles)
            {
                Mod m = new Mod();
                m.Parse(dmFile);
                int i = 0;
                i++;
            }
        }
    }
}
