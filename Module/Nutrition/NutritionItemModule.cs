using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Nutrition
{
    public class NutritionItemModule : SqlModule<NutritionItemModule, NutritionItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT nutrition_items.* FROM `nutrition_items`,items_gd WHERE nutrition_items.items_gd_id = items_gd.id";
        }

        public NutritionItem GetItem(uint item)
        {
            var NutritionItem = Instance.GetAll().Where(p => p.Value.Items_gd_id == item);
            return NutritionItem.FirstOrDefault().Value ?? null;
        }

        public void UseItem(DbPlayer dbPlayer, uint item)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return;

            NutritionItem Item = GetItem(item);
            if (Item != null)
            {
                dbPlayer.food[0] += Item.food;
                dbPlayer.drink[0] += Item.drink;
                dbPlayer.lastnutritionitems.Add(new LastNutritionItem(Item.Id, DateTime.Now));
                NutritionModule.Instance.PushNutritionToPlayer(dbPlayer);
            }
        }
    }
}
