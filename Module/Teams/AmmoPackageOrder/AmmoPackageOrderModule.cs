using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Teams.AmmoPackageOrder
{
    public class AmmoPackageOrderModule : Module<AmmoPackageOrderModule>
    {
        public static Vector3 LoadPosition = new Vector3(4427.72, -4451.81, 7.23672);
        public static Vector3 ContainerPosition = new Vector3(4437.54, -4446.48, 7.23679);
        public static int AmmoChestToPackageMultipliert = 10;
        public static int AmmoOrderSourcePrice = 1000;
        public static int BlackPowderToPackageMultiplier = 20;

        public override bool Load(bool reload = false)
        {
            MenuManager.Instance.AddBuilder(new AmmoPackageOrderMenuBuilder());
            return base.Load(reload);
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            if (dbPlayer.Team.Id != (int)TeamTypes.TEAM_HUSTLER && dbPlayer.Team.Id != (uint)TeamTypes.TEAM_ICA) return false;
            if (dbPlayer.TeamRank < 9) return false;

            if (dbPlayer.Player.Position.DistanceTo(LoadPosition) < 1.5f)
            {
                MenuManager.Instance.Build(PlayerMenu.AmmoPackageOrderMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        public override void OnServerBeforeRestart()
        {
            // Sonntags 16 Uhr wende
            if(DateTime.Now.DayOfWeek == DayOfWeek.Sunday && DateTime.Now.Hour == 15)
            {
                foreach (DbTeam dbTeam in TeamModule.Instance.GetAll().Values.ToList())
                {
                    if (dbTeam.IsGangsters() && dbTeam.TeamMetaData != null)
                    {
                        dbTeam.TeamMetaData.OrderedPackets = 0;
                        dbTeam.TeamMetaData.SaveOrderedPackets();
                    }
                }
            }
        }
    }
}
