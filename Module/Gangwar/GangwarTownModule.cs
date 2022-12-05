using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Gangwar
{
    public class GangwarTownModule : SqlModule<GangwarTownModule, GangwarTown, uint>
    {
        public static Vector3 BankPosition = new Vector3(1044.59, -3194.81, -38.1579);

        protected override string GetQuery()
        {
            return "SELECT * FROM `gangwar_towns`;";
        }

        protected override void OnItemLoaded(GangwarTown gwTown)
        {
            // Create Map Blip on load
            if(gwTown.IsActive == 0) return;
            gwTown.Blip = Blips.Create(gwTown.Position, gwTown.Name, 543, 1.0f, color: gwTown.OwnerTeam.Id != 0 ? gwTown.OwnerTeam.BlipColor : 0);
            Main.ServerBlips.Add(gwTown.Blip);
        }

        public GangwarTown FindBank(DbPlayer dbPlayer)
        {
            if (dbPlayer.Player.Dimension == 0) return null;
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.Id == dbPlayer.Id && dbPlayer.Player.Position.DistanceTo(BankPosition) < 1.5f).Value;
        }

        public GangwarTown GetByPosition(Vector3 Position)
        {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.InteriorPosition.DistanceTo(Position) < 3.0f).Value;
        }

        public bool IsGaragePosition(Vector3 Position) {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.AttackerSpawnPosition.DistanceTo(Position) < 3.0f || gt.Value.DefenderSpawnPosition.DistanceTo(Position) < 3.0f).Value != null;
        }

        public bool IsTeamSpawn(Vector3 Position) {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.AttackerSpawnPosition.DistanceTo(Position) < 20.0f || gt.Value.DefenderSpawnPosition.DistanceTo(Position) < 3.0f).Value != null;
        }

        public GangwarTown FindActiveByTeam(Team team)
        {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.IsAttacked && (gt.Value.AttackerTeam == team || gt.Value.DefenderTeam == team)).Value;
        }
        
        public bool IsTeamInGangwar(Team team)
        {
            return Instance.GetAll().Count(g => g.Value.AttackerTeam == team || g.Value.DefenderTeam == team) > 0;
        }

        public GangwarTown FindByOwner(Team team)
        {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.OwnerTeam  != null && gt.Value.OwnerTeam == team).Value;
        }

        public GangwarTown GetNearByPlayer(DbPlayer dbPlayer)
        {
            foreach (KeyValuePair<uint, GangwarTown> kvp in GangwarTownModule.Instance.GetAll())
            {
                if ((dbPlayer.HasData("gangwarId") && dbPlayer.GetData("gangwarId") == kvp.Value.Id))
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        public Vector3 GetGangwarTownSpawnByTeam(Team team) {
            GangwarTown gangwarTown = FindActiveByTeam(team);
            return gangwarTown.AttackerTeam == team ? gangwarTown.AttackerSpawnPosition : gangwarTown.DefenderSpawnPosition;
        }

        public int GetCarShopDiscount(Team team)
        {
            return (Instance.GetOwnedTownsCount(team) * 5);
        }

        public int GetOwnedTownsCount(Team team)
        {
            return Instance.GetAll().Count(gt => gt.Value.OwnerTeam == team && gt.Value.IsActive == 1);
        }
    }
}