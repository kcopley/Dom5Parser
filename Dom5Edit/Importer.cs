using Dom5Edit.Entities;
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
        private readonly char spaceDelimiter = ' ';
        private readonly string commentDelimiter = "--";

        public Dictionary<string, MethodInfo> Initializer = new Dictionary<string, MethodInfo>();

        public List<Entity> monsters = new List<Entity>();

        public Importer()
        {
            //todo: find all classes implementing IDEntity that have a Create method
            //Initializer.Add(Monster.Import, Monster.Create); what the below does, but generically
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
        }

        public void Run()
        {
            //Startup script
            string localPath = Path.GetDirectoryName(Application.ExecutablePath);
            string[] dmFiles = Directory.GetFiles(localPath, "*.dm");


            foreach (string dmFile in dmFiles)
            {
                using (StreamReader sr = File.OpenText(dmFile))
                {
                    string s = "";
                    Entity current = null;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (s.Length < 1) continue; //empty line
                        //pull comments first
                        int commentIndex = s.IndexOf(commentDelimiter);
                        string line = s;
                        string comment = ""; //set to empty string, not null
                        if (commentIndex != -1) //has a comment
                        {
                            line = s.Substring(0, commentIndex).Trim();
                            comment = s.Substring(commentIndex + 2).Trim();
                        }

                        //grab the command & value

                        int spaceIndex = line.IndexOf(spaceDelimiter);
                        string command = line;
                        string value = ""; //set to empty string, not null
                        if (spaceIndex != -1) //has a value (but could be spaces before a comment? should be handled by trim above)
                        {
                            command = line.Substring(0, spaceIndex).Trim();
                            value = line.Substring(spaceIndex + 1).Trim();
                        }

                        if (Initializer.ContainsKey(command))
                        {
                            //before we possibly eliminate it, save an end comment
                            if (Initializer[command] == null && current != null) current.SetEndComment(comment);
                            //if initializer contains key, but maps to null, it's the #end command, so assign current to null
                            current = (Entity)Initializer[command]?.Invoke(null, null);
                        }
                        if (current != null)
                        {
                            //if the current object is still active, we haven't hit #end yet, so parse
                            current.Parse(command, value, comment);
                        }
                    }
                }
            }
        }
    }
}
