using GTANetworkAPI;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Animal
{
    public class AnimalVariationModule : Module<AnimalVariationModule>
    {
        public Dictionary<Ped, int> VariationAnimals = new Dictionary<Ped, int>();

        protected override bool OnLoad()
        {
            VariationAnimals = new Dictionary<Ped, int>();
            return base.OnLoad();
        }

    }

    public class AnimalVariationEvents : Script
    {
        [RemoteEvent]
        public void requestPedSync(Player player, Ped ped, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(ped != null && AnimalVariationModule.Instance.VariationAnimals.ContainsKey(ped))
            {
                dbPlayer.Player.TriggerNewClient("pedStreamInSync", ped, AnimalVariationModule.Instance.VariationAnimals[ped]);
            }
        }
    }
}
