using Dom5Edit.Commands;
using Dom5Edit.Props;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class EntitySet<T> where T : IDEntity, new()
    {
        public Mod Parent { get; set; }
        public int START_ID { get; set; }
        public int END_ID { get; set; } = -1;
        private int CURRENT_ID { get; set; } = -1;
        private Dictionary<int, T> Entities = new Dictionary<int, T>();
        private List<T> UnIDdEntities = new List<T>();
        private Dictionary<int, T> DisabledEntities = new Dictionary<int, T>();
        public bool TryGetValue(int i, out T entity)
        {
            //dependency code?
            //foreach (var m in Dependencies)
            //{
            //    if (m.Monsters.TryGetValue(i, out entity)) return true;
            //}
            if (i <= 0)
            {
                entity = null;
                return false;
            }

            if (Entities.TryGetValue(i, out entity)) return true;
            return false;
        }
        private Dictionary<string, T> NamedEntities = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
        private List<T> UnnamedEntities = new List<T>();
        public bool TryGetValueNamed(string i, out T entity)
        {
            //foreach (var m in Dependencies)
            //{
            //    if (m.NamedMonsters.TryGetValue(i, out entity)) return true;
            //}
            if (string.IsNullOrEmpty(i))
            {
                entity = null;
                return false;
            }

            if (NamedEntities.TryGetValue(i, out entity)) return true;
            return false;
        }

        /// <summary>
        /// Retrieves an ID'd or named entity from the dictionary. Checks ID first and name second. Use -1 for the ID to skip, or null on the string to skip.
        /// </summary>
        /// <param name="i">The ID of the entity.</param>
        /// <param name="s">The name of the entity.</param>
        /// <param name="entity">The entity returned from the dictionary.</param>
        /// <returns>True if either the ID or name is found; otherwise false.</returns>
        public bool TryGet(int i, string s, out T entity)
        {
            if (TryGetValue(i, out entity)) return true;
            return TryGetValueNamed(s, out entity);
        }

        public void Add(int id, string name, T t)
        {
            if (id > 0)
            {
                if (!Entities.ContainsKey(id))
                {
                    Entities.Add(id, t);
                }
                else
                {
                    Parent.Log("Entity ID: " + id + " was already used inside mod - Type: " + t.GetType());
                }
            }
            else
            {
                UnIDdEntities.Add(t);
            }

            if (!string.IsNullOrEmpty(name))
            {
                if (!NamedEntities.ContainsKey(name))
                {
                    NamedEntities.Add(name, t);
                }
                else
                {
                    Parent.Log("Entity Name: " + name + " was already used inside mod - Type: " + t.GetType());
                }
            }
            else
            {
                UnnamedEntities.Add(t);
            }
        }

        public void GiveName(IDEntity ie, string name)
        {
            if (!NamedEntities.ContainsKey(name))
            {
                NamedEntities.Add(name, (T)ie);
            }
            UnnamedEntities.Remove((T)ie);
        }

        public void GiveID(string name, int id)
        {
            if (NamedEntities.TryGetValue(name, out T t))
            {
                if (id > 0)
                {
                    if (!Entities.ContainsKey(id))
                    {
                        Entities.Add(id, t);
                    }
                }
            }
        }

        internal void Resolve()
        {
            foreach (var kvp in Entities)
            {
                kvp.Value.Resolve();
            }
            foreach (var e in UnIDdEntities)
            {
                e.Resolve();
            }
        }

        internal void Map()
        {
            foreach (var kvp in Entities)
            {
                kvp.Value.Map();
            }
            foreach (var e in UnIDdEntities)
            {
                e.Map();
            }
        }

        internal void Export(StreamWriter writer)
        {
            foreach (var m in DisabledEntities.OrderBy(x => x.Key).Where(x => x.Key >= START_ID))
            {
                m.Value.Export(writer);
                writer.WriteLine();
            }
            foreach (var m in DisabledEntities.OrderBy(x => x.Key).Where(x => x.Key < START_ID))
            {
                m.Value.Export(writer);
                writer.WriteLine();
            }
            foreach (var m in Entities.OrderBy(x => x.Key).Where(x => x.Key >= START_ID))
            {
                m.Value.Export(writer);
                writer.WriteLine();
            }
            foreach (var m in Entities.OrderBy(x => x.Key).Where(x => x.Key < START_ID))
            {
                m.Value.Export(writer);
                writer.WriteLine();
            }
            foreach (var m in UnIDdEntities)
            {
                m.Export(writer);
                writer.WriteLine();
            }
        }

        internal void Disable(EntityType t, int id, Mod mod)
        {
            //do a switch on t
            if (DisabledEntities.ContainsKey(id)) return;
            IDEntity m;
            if (!Entities.ContainsKey(id))
            {
                m = IDEntity.SelectVanillaEntity<Monster>(id, mod);
                m.Properties.Add(CommandProperty.Create(Command.CLEARMAGIC, m));
            }
            else
            {
                m = Entities[id];
                m.Properties.Clear();
                m.Properties.Add(CommandProperty.Create(Command.CLEARMAGIC, m));
            }
            Entities.Remove(id);

            DisabledEntities.Add(id, (T)m);
        }

        public void AssignFirstID()
        {
            CURRENT_ID = START_ID;
        }

        private int GetNextID()
        {
            if (CURRENT_ID < START_ID) CURRENT_ID = START_ID;
            //very crude search unfortunately, but should be fine for our purposes
            while (Entities.ContainsKey(CURRENT_ID))
            {
                CURRENT_ID++;
            }
            if (END_ID != -1 && CURRENT_ID > END_ID)
            {
                Parent.Log("Warning: Mod surpasses limitations on " + typeof(T).ToString() + " ID's!");
            }
            return CURRENT_ID;
        }

        public void Merge(EntitySet<T> set)
        {
            var entityList = set.GetEntityCollection();
            foreach (var entity in entityList)
            {
                if (entity.DependentEntity != null)
                {
                    entity.DependentEntity.Properties.AddRange(entity.Properties);
                    continue;
                }
                if (entity.ID < START_ID && entity.Selected)
                {
                    //select monster command on a vanilla
                    if (!Entities.ContainsKey(entity.ID))
                    {
                        Entities.Add(entity.ID, entity);
                    }
                    else
                    {
                        Entities[entity.ID].Properties.AddRange(entity.Properties);
                    }
                }
                else if (entity.ID < START_ID)
                {
                    //new monster on a vanilla ID?
                }
                else
                {
                    //assign a new ID upwards
                    int newID = GetNextID();
                    entity.ID = newID;
                    Entities.Add(newID, entity);
                }
            }
            foreach (var entity in set.GetUnIDdEntities())
            {
                if (entity.DependentEntity != null)
                {
                    entity.DependentEntity.Properties.AddRange(entity.Properties);
                    continue;
                }
                //assign a new ID upwards
                int newID = GetNextID();
                entity.ID = newID;
                Entities.Add(newID, entity);
            }
        }

        private List<T> GetEntityCollection()
        {
            return Entities.Values.ToList();
        }

        private List<T> GetUnIDdEntities()
        {
            return UnIDdEntities;
        }

        public List<T> GetFullList()
        {
            List<T> ret = new List<T>();
            ret.AddRange(Entities.Values);
            ret.AddRange(UnIDdEntities);
            return ret;
        }
    }
}
