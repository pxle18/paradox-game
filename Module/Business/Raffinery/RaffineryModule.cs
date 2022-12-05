using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Business.Raffinery
{
    public class RaffineryModule : SqlModule<RaffineryModule, Raffinery, uint>
    {
        public static uint RohölItemModelId = 536;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(RaffineryAusbaustufeModule), typeof(BusinessModule) };
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new RaffineryMenuBuilder());
        }
        
        protected override string GetQuery()
        {
            return "SELECT * FROM `business_raffinery`;";
        }

        public Raffinery GetThis(Vector3 position)
        {
            return Instance.GetAll().Values.FirstOrDefault(fs => fs.Position.DistanceTo(position) < 5.0f);
        }
        
        public override void OnFifteenMinuteUpdate()
        {
            foreach (Raffinery raffinery in Instance.GetAll().Values)
            {
                try
                {
                    if (raffinery == null || raffinery.Container == null) continue;
                    if (raffinery.IsOwnedByBusines())
                    {
                        int maxFörderMenge = raffinery.FörderMengeMin * 15;
                        int maxAddableMenge = raffinery.Container.GetMaxItemAddedAmount(RohölItemModelId);

                        if (maxAddableMenge <= 0) continue;

                        if (maxFörderMenge > maxAddableMenge) maxFörderMenge = maxAddableMenge;
                        raffinery.Container.AddItem(RohölItemModelId, raffinery.FörderMengeMin * 15);
                    }
                    else
                    {
                        // Set if its not setted...
                        Business ownerBusiness = BusinessModule.Instance.GetAll().Values.Where(b => b.BusinessBranch != null && b.BusinessBranch.RaffinerieId == raffinery.Id).FirstOrDefault();
                        if (ownerBusiness == null) continue;
                        raffinery.OwnerBusiness = ownerBusiness;
                    }
                }
                catch(Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("raffineryId", out uint raffineryId)) return false;
            Raffinery Raffinery = Instance.Get(raffineryId);

            if (Raffinery == null) return false;
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("raffineryId", raffineryId);
                    if (Raffinery.IsOwnedByBusines())
                    {
                        Business business = Raffinery.GetOwnedBusiness();
                        dbPlayer.SendNewNotification($"Besitzer: {business.Name}", title:$"Oelfoerderpumpe");
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Zum Verkauf (${Raffinery.BuyPrice})", title: $"Oelfoerderpumpe");
                    }
                    return true;
                case ColShapeState.Exit:
                    if (dbPlayer.HasData("raffineryId")) dbPlayer.ResetData("raffineryId");
                    return false;
                default:
                    return false;
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!dbPlayer.RageExtension.IsInVehicle)
            {
                if (dbPlayer.HasData("raffineryId"))
                {
                    Raffinery raffinery = RaffineryModule.Instance.Get(dbPlayer.GetData("raffineryId"));
                    if (raffinery != null && raffinery.Position.DistanceTo(dbPlayer.Player.Position) < 4.0f)
                    {
                        MenuManager.Instance.Build(PlayerMenu.RaffineryMenu, dbPlayer).Show(dbPlayer);
                    }
                }
            }
            return false;
        }
    }
}
