using Dom5Edit.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dom5Edit
{
    public class Importer
    {
        public static char[] Delimiters = new char[1] { ' ' };

        public Dictionary<string, Func<IDEntity>> Initializer = new Dictionary<string, Func<IDEntity>>();

        public List<IDEntity> monsters = new List<IDEntity>();

        public Importer()
        {
            //todo: find all classes implementing IDEntity that have a Create method
            Initializer.Add(Monster.Import, Monster.Create);

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
                    IDEntity current = null;
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] line = s.Split(Delimiters, 2);

                        if (line.Length <= 0) continue; //empty line
                        if (Initializer.ContainsKey(line[0]))
                        {
                            //if initializer contains key, but maps to null, it's the #end command, so assign current to null
                            current = Initializer[line[0]]?.Invoke();
                        }
                        else if (current != null)
                        {
                            //if the current object is still active, we haven't hit #end yet, so parse
                            current.Parse(line);
                        }
                    }
                }
            }
        }
    }
}
