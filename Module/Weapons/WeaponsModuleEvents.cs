using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons.Data;

namespace VMP_CNR.Module.Weapons
{
    public class WeaponsModuleEvents : Script
    {
        [RemoteEvent]
        public void getWeaponAmmoAnswer(Player p_Player, string p_AnswerJson, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;

            var l_Player = p_Player.GetPlayer();

            if (p_AnswerJson == null || p_AnswerJson.Length <= 2) return;

            ClientWeaponData[] l_Data = JsonConvert.DeserializeObject<ClientWeaponData[]>(p_AnswerJson);
            if (l_Data == null) return;

            foreach (var l_Entry in l_Data)
            {
                if (l_Entry == null) continue;

                var l_Detail = l_Player.Weapons.Find(x => x != null && x.WeaponDataId == l_Entry.WeaponDataID);
                if (l_Detail == null)
                    continue;

                if (l_Entry.Ammo > l_Detail.Ammo && l_Detail.Ammo >= 0 && (l_Entry.Ammo - l_Detail.Ammo > 5))
                {
                    WeaponData l_WeaponData = WeaponDataModule.Instance.Get(l_Detail.WeaponDataId);
                    if (l_WeaponData == null)
                        continue;

                    NAPI.Task.Run(() => { l_Player.Player.SetWeaponAmmo((WeaponHash)l_WeaponData.Hash, l_Detail.Ammo); });
                    //l_Player.Player.TriggerNewClient("updateWeaponAmmo", l_Detail.WeaponDataId, l_Detail.Ammo);

                    if (!ServerFeatures.IsActive("ac-ammocheck"))
                        continue;

                    Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"DRINGENDER-Anticheat-Verdacht: {l_Player.GetName()} (Munition versucht zu erhöhen! (Waffe: {l_WeaponData.Name} Vorher: {l_Detail.Ammo} Nachher: {l_Entry.Ammo})");
                    Logging.Logger.LogToAcDetections(l_Player.Id, Logging.ACTypes.WeaponCheat, $"(Weaponcheat (Munition): {l_WeaponData.Name} Munition versucht zu erhöhen! (Waffe: {l_WeaponData.Name} Vorher: {l_Detail.Ammo} Nachher: {l_Entry.Ammo})");
                }
                else
                    l_Detail.Ammo = l_Entry.Ammo;
            }
        }
    }

    public class ClientWeaponData
    {
        [JsonProperty("id")]
        public int WeaponDataID { get; set; }

        [JsonProperty("ammo")]
        public int Ammo { get; set; }
    }
}
