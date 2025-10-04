using GTANetworkAPI;
using System.Threading.Tasks;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerHealth
    {
        public static void ApplyPlayerHealth(this DbPlayer dbPlayer)
        {
            // Guard missing player instance during early load
            if (dbPlayer == null || dbPlayer.Player == null) return;

            if (dbPlayer.Hp > 0)
            {
                if (dbPlayer.Hp > 99) dbPlayer.Hp = 99;
                dbPlayer.SetHealth(dbPlayer.Hp);
            }
            if (dbPlayer.Armor != null && dbPlayer.Armor.Length > 0 && dbPlayer.Armor[0] > 0)
            {
                if(dbPlayer.Armor[0] > 99)
                {
                    dbPlayer.Armor[0] = 99;
                }

                dbPlayer.SetArmor(dbPlayer.Armor[0]);
            }
        }

        public static void UpdatePlayerHealthAndArmor(this DbPlayer dbPlayer)
        {
            if (dbPlayer?.Player == null) return;
            dbPlayer.Hp = dbPlayer.Player.Health;
            if (dbPlayer.Armor != null && dbPlayer.Armor.Length > 0)
            {
                dbPlayer.Armor[0] = dbPlayer.Player.Armor;
            }
        }

        public static void SetHealth(this DbPlayer dbPlayer, int health)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            if (health >= 99) health = 99;

            dbPlayer.SetData("ac-healthchange", 2);
            dbPlayer.SetData("ac_lastHealth", health);

            NAPI.Task.Run(() =>
            {
                if (dbPlayer?.Player == null) return;
                dbPlayer.Hp = health;
                dbPlayer.Player.Health = health;
            });
        }

        public static void SetArmor(this DbPlayer dbPlayer, int Armor, bool Schutzweste = false)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            dbPlayer.visibleArmor = Schutzweste;
            dbPlayer.SetArmorPlayer(Armor);
            //dbPlayer.ApplyArmorVisibility();
        }
    }
}