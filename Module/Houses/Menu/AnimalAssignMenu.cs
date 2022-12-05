using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Animal;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Houses
{
    public class AnimalAssignMenuBuilder : MenuBuilder
    {
        public AnimalAssignMenuBuilder() : base(PlayerMenu.AnimalAssignMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Haustier auswählen");

            menu.Add($"Schließen");
            menu.Add($"Haustier abgeben");
            foreach (uint animalId in dbPlayer.AssignedAnimals.ToList())
            {
                Animal.Animal animal = Animal.AnimalModule.Instance.GetAll().Values.Where(k => k.Id == animalId).FirstOrDefault();

                if (animal != null && !animal.Spawned && animal.Ped == null)
                {
                    menu.Add($"{animal.Name}");
                }
            }

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                if (index == 1)
                {
                    if(dbPlayer.PlayerPed != null && dbPlayer.PlayerPed.Spawned != false)
                    {
                        dbPlayer.SendNewNotification($"Sie haben {dbPlayer.PlayerPed.Name} zuhause abgegeben!");
                        dbPlayer.DeleteAnimal();
                    }
                    return false;
                }

                int count = 2;
                foreach (uint animalId in dbPlayer.AssignedAnimals.ToList())
                {
                    Animal.Animal animal = AnimalModule.Instance.GetAll().Values.Where(k => k.Id == animalId).FirstOrDefault();

                    if (animal != null && !animal.Spawned && animal.Ped == null)
                    {
                        if(count == index)
                        {
                            dbPlayer.LoadAnimal(animal.Id, dbPlayer.Player.Position, dbPlayer.Player.Dimension);
                            dbPlayer.SendNewNotification($"Sie haben {animal.Name} gerufen!");
                        }
                        count++;
                    }
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}