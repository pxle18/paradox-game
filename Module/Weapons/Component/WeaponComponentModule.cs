using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Weapons.Component
{
    public class WeaponComponentModule : SqlModule<WeaponComponentModule, WeaponComponent, int>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `weapon_component`;";
        }

        public string serializeComponentSet(List<Weapons.Component.WeaponComponent> dataSet)
        {
            List<uint> data = new List<uint>();

            foreach(Weapons.Component.WeaponComponent comp in dataSet)
            {
                if (comp == null) continue;
                data.Add(comp.Hash);
            }

            return String.Join('|', data);
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.WeaponComponents = new Dictionary<uint, List<WeaponComponent>>();

            Dictionary<uint, List<uint>> components = NAPI.Util.FromJson<Dictionary<uint, List<uint>>>(reader.GetString("weapon_components"));
            if (components == null) return;

            foreach(KeyValuePair<uint, List<uint>> kvp in components)
            {
                if(!dbPlayer.WeaponComponents.ContainsKey(kvp.Key))
                {
                    dbPlayer.WeaponComponents.Add(kvp.Key, new List<WeaponComponent>());
                }

                if(dbPlayer.WeaponComponents[kvp.Key] != null)
                {
                    foreach(uint compId in kvp.Value)
                    {
                        WeaponComponent comp = WeaponComponentModule.Instance.Get((int)compId);
                        if (comp == null) continue;
                        if (!dbPlayer.WeaponComponents[kvp.Key].Contains(comp))
                        {
                            dbPlayer.WeaponComponents[kvp.Key].Add(comp);
                        }
                    }
                }
            }
        }

        public override void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
        {

            if(dbPlayer.HasWeaponComponentsForWeapon((uint)newgun)) 
            {
                dbPlayer.Player.SetSharedData("currentWeaponComponents", (uint)newgun + "." + WeaponComponentModule.Instance.serializeComponentSet(dbPlayer.WeaponComponents[(uint)newgun]));
            }
            else
            {
                dbPlayer.Player.SetSharedData("currentWeaponComponents", (uint)newgun + ".");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsetwcomp(Player player, string weaponComponent)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!UInt32.TryParse(weaponComponent, out uint weaponComponentInt))
            {
                return;
            }

            dbPlayer.GiveWeaponComponent((uint)dbPlayer.Player.CurrentWeapon, weaponComponentInt);
            return;
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandtestwcomp(Player player, string weaponComponent)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!UInt32.TryParse(weaponComponent, out uint weaponComponentInt))
            {
                return;
            }

            dbPlayer.GiveWeaponComponent((uint)dbPlayer.Player.CurrentWeapon, weaponComponentInt, true);
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandremovewcomp(Player player, string weaponComponent)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!UInt32.TryParse(weaponComponent, out uint weaponComponentInt))
            {
                return;
            }

            dbPlayer.RemoveWeaponComponent((uint)dbPlayer.Player.CurrentWeapon, weaponComponentInt);
            dbPlayer.Save();
            return;
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandremoveallwcomp(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            dbPlayer.RemoveAllWeaponComponents((uint)dbPlayer.Player.CurrentWeapon);
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandresetwcomp(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            dbPlayer.ResetAllWeaponComponents();
            return;
        }
    }

    public static class WeaponComponentEvents
    {
        public static void SyncPlayerWeaponComponents(this DbPlayer dbPlayer)
        {
            Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 200).TriggerNewClient("nukePlayerWeaponComponents", dbPlayer.Player);

            foreach (KeyValuePair<uint, List<WeaponComponent>> kvp in dbPlayer.WeaponComponents.ToList().Where(k => k.Value != null && k.Value.Count > 0))
            {

                foreach(WeaponComponent component in kvp.Value.ToList())
                {
                    Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position).TriggerNewClient("updatePlayerWeaponComponent", dbPlayer.Player, kvp.Key, component.Hash, false);
                }

                if(dbPlayer.Player.CurrentWeapon == (WeaponHash)kvp.Key)
                {
                    dbPlayer.Player.SetSharedData("currentWeaponComponents", kvp.Key + "." + WeaponComponentModule.Instance.serializeComponentSet(dbPlayer.WeaponComponents[kvp.Key]));
                }
            }
        }

        public static void GiveWeaponComponent(this DbPlayer dbPlayer, uint weaponHash, uint ComponentHash, bool ignoreDate = false)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!ignoreDate)
            {
                var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

                var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)weaponHash);
                if (l_Weapon == null) return;

                WeaponComponent component = WeaponComponentModule.Instance.GetAll().Values.Where(c => c.Hash == ComponentHash && c.WeaponDataId == l_Weapon.Id).FirstOrDefault();
                if (component == null) return;

                if (!dbPlayer.WeaponComponents.ContainsKey(weaponHash)) dbPlayer.WeaponComponents.Add(weaponHash, new List<WeaponComponent>());

                dbPlayer.WeaponComponents[weaponHash].Add(component);

                if (dbPlayer.Player.CurrentWeapon == (WeaponHash)weaponHash)
                {
                    Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position).TriggerNewClient("updatePlayerWeaponComponent", dbPlayer.Player, weaponHash, ComponentHash, false);
                }

                dbPlayer.Player.SetSharedData("currentWeaponComponents", weaponHash + "." + WeaponComponentModule.Instance.serializeComponentSet(dbPlayer.WeaponComponents[weaponHash]));

                dbPlayer.SaveWeaponComponents();
            }
            else
            {
                if (dbPlayer.Player.CurrentWeapon == (WeaponHash)weaponHash)
                {
                    Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position).TriggerNewClient("updatePlayerWeaponComponent", dbPlayer.Player, weaponHash, ComponentHash, false);
                }

                dbPlayer.Player.SetSharedData("currentWeaponComponents", weaponHash + "." + ComponentHash);
            }
        }

        public static bool HasWeaponComponent(this DbPlayer dbPlayer, uint weaponHash, uint ComponentHash)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

            var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)weaponHash);
            if (l_Weapon == null) return false;

            WeaponComponent component = WeaponComponentModule.Instance.GetAll().Values.Where(c => c.Hash == ComponentHash && c.WeaponDataId == l_Weapon.Id).FirstOrDefault();
            if (component == null) return false;

            return (dbPlayer.WeaponComponents.ContainsKey(weaponHash) && dbPlayer.WeaponComponents[weaponHash].Contains(component));
        }


        public static bool HasWeaponComponentsForWeapon(this DbPlayer dbPlayer, uint weaponHash)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;
            return (dbPlayer.WeaponComponents.ContainsKey(weaponHash));
        }

        public static void RemoveWeaponComponent(this DbPlayer dbPlayer, uint weaponHash, uint ComponentHash)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;


            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

            var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)weaponHash);
            if (l_Weapon == null) return;

            WeaponComponent component = WeaponComponentModule.Instance.GetAll().Values.Where(c => c.Hash == ComponentHash && c.WeaponDataId == l_Weapon.Id).FirstOrDefault();
            if (component == null) return;

            if ((dbPlayer.WeaponComponents.ContainsKey(weaponHash) && dbPlayer.WeaponComponents[weaponHash].Contains(component)))
            {
                dbPlayer.WeaponComponents[weaponHash].Remove(component);

                if (dbPlayer.Player.CurrentWeapon == (WeaponHash)weaponHash)
                {
                    dbPlayer.WeaponComponents[weaponHash].Remove(component);
                    dbPlayer.Player.SetSharedData("currentWeaponComponents", weaponHash + "." + WeaponComponentModule.Instance.serializeComponentSet(dbPlayer.WeaponComponents[weaponHash]));
                }
                else
                {
                    Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 200).TriggerNewClient("updatePlayerWeaponComponent", dbPlayer.Player, weaponHash, ComponentHash, true);
                }
            }

            dbPlayer.SaveWeaponComponents();
        }

        public static void RemoveAllWeaponComponents(this DbPlayer dbPlayer, uint weaponHash)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.WeaponComponents.ContainsKey(weaponHash))
            {
                dbPlayer.WeaponComponents[weaponHash] = new List<WeaponComponent>();

                if(dbPlayer.Player.CurrentWeapon == (WeaponHash)weaponHash)
                {
                    dbPlayer.Player.SetSharedData("currentWeaponComponents", weaponHash + ".");
                }
                else
                {
                    Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 200).TriggerNewClient("resetPlayerWeaponComponents", dbPlayer.Player, weaponHash);
                }
            }

            dbPlayer.SaveWeaponComponents();
        }

        public static void ResetAllWeaponComponents(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.WeaponComponents = new Dictionary<uint, List<WeaponComponent>>();

            dbPlayer.Player.SetSharedData("currentWeaponComponents", (uint)dbPlayer.Player.CurrentWeapon + ".");

            Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 200).TriggerNewClient("nukePlayerWeaponComponents", dbPlayer.Player);

            dbPlayer.SaveWeaponComponents();
        }
    }
}