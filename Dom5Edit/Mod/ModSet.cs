using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit
{
    public static class ModSetExtensions
    {
        //Extension methods to topologically sort the list of mods w/ dependencies prior to merging
        public static IEnumerable<T> TSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle = false)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();

            foreach (var item in source)
                Visit(item, visited, sorted, dependencies, throwOnCycle);

            return sorted;
        }

        private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies, bool throwOnCycle)
        {
            if (!visited.Contains(item))
            {
                visited.Add(item);

                var list_dep = dependencies(item);

                foreach (var dep in list_dep)
                    Visit(dep, visited, sorted, dependencies, throwOnCycle);

                sorted.Add(item);
            }
            else
            {
                if (throwOnCycle && !sorted.Contains(item))
                    throw new Exception("Cyclic dependency found");
            }
        }
    }

    public class ModSet : List<Mod>
    {
        public static ModSet Import(string folder, List<string> files)
        {
            ModSet set = new ModSet();
            foreach (string file in files)
            {
                Mod m = new Mod(Path.Combine(folder, file));
                set.Add(m);
            }

            foreach (Mod m in set)
            {
                if (m.HasDependencies())
                    m.ResolveDependencies(set);
            }

            Func<Mod, IEnumerable<Mod>> sorter = new Func<Mod, IEnumerable<Mod>>(x => x.Dependencies);
            var enumerable = set.TSort<Mod>(sorter);

            set.Clear();
            set.AddRange(enumerable);

            foreach (Mod m in set)
            {
                m.Load(false);
            }

            foreach (Mod m in set)
            {
                m.Resolve();
            }
            return set;
        }

        public Mod MergeAll(string modName)
        {
            Mod finalMod = new Mod();
            finalMod.ModName = modName;
            finalMod.Description = "A merger of all valid mods that were parsed";
            finalMod.Version = "1.0";
            finalMod.DomVersion = "5.00";

            foreach (Mod m in this)
            {
                foreach (int referenced in m.VanillaMageReferences)
                {
                    //slow but meh
                    if (!finalMod.VanillaMageReferences.Contains(referenced)) finalMod.VanillaMageReferences.Add(referenced);
                }

                List<EntityType> types = m.Database.Keys.ToList();
                foreach (var et in types)
                {
                    finalMod.Database[et].Merge(m.Database[et]);
                }

                //general settings (is this even necessary? lol)

                //events, just migrate over since they've been adjusted
                foreach (var kvp in m.Events)
                {
                    finalMod.Events.Add(kvp);
                }

                foreach (var k in m.Dependents.Keys)
                {
                    finalMod.Dependents[k].Merge(k, m.Dependents[k]);
                }
            }

            //string description = "Nation mods merged together. Contains:\n";
            //finalMod.Description = description;
            return finalMod;
        }
    }
}
