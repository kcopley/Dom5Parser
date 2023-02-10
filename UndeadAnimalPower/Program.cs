using System;
using System.IO;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace UndeadAnimalPower
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Mod vanilla = VanillaLoader.Vanilla;
            Mod newMod = new Mod();

            var monsters = vanilla.Database[EntityType.MONSTER].GetFullList();
            foreach (var monster in monsters)
            {
                if (monster.HasCommand<CommandProperty>(Command.UNDEAD))
                {
                    Monster newMonster = new Monster();
                    newMonster.ID = monster.ID;
                    var newProp = newMonster.Create<IntProperty>(Command.DEATHPOWER);
                    newProp.Value = 1;
                    newMod.AddEntity<Monster>(monster.ID, "", newMonster);
                }
                else if (monster.HasCommand<CommandProperty>(Command.ANIMAL))
                {
                    Monster newMonster = new Monster();
                    newMonster.ID = monster.ID;
                    var newProp = newMonster.Create<IntProperty>(Command.DEATHPOWER);
                    newProp.Value = -1;
                    newMod.AddEntity<Monster>(monster.ID, "", newMonster);
                }
            }
            newMod.ModName = "Powerful Undead and Animals!!!";

            newMod.FullFilePath = Path.Combine(Environment.CurrentDirectory, "PowerfulUndeadAndAnimals.dm");
            newMod.Export();
        }
    }
}
