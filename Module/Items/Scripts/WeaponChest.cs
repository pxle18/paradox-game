using System;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> WeaponChest(DbPlayer dbPlayer, ItemModel ItemData)
        {
            // Restrict 4 Cops and Brigada
            if (!dbPlayer.IsBadOrga() && !dbPlayer.IsAGangster())
                return false;

            if (dbPlayer.Container.GetItemAmount(301) < 1)
            {
                dbPlayer.SendNewNotification(
                    "Um eine Waffenkiste aufzubrechen benötigen Sie ein Brecheisen!");
                return false;
            }

            Chats.sendProgressBar(dbPlayer, 15000);

            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);

            await Task.Delay(15000);

            dbPlayer.SetCannotInteract(false);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            // Choose Items
            float size = 0;

            while (size < ItemData.Weight)
            {
                // Results: 
                // 72 SMG, 211 AmmoSMG, 
                // 82 SpecialCarbine, 221 Ammo SpecialCarb
                // 54 Flashlight
                // 83 Bullpup, 222 Ammo Bullpup

                // 1244 smg scope, 1239 SMG 60er Mag, 1226 SMG Light, 1207 SMG Schalld.
                // 1248 specc handgriff, 1245 specc scope, 1230 specc light, 1213 specc schalld.
                // 1249 bullp griff, 1247 bullp scope, 1236 bullp light, 1218 bullp schalld.
                Random rnd = new Random();
                int rand = rnd.Next(1, 27);
                uint xItem = 0;
                int amount = 1;
                switch (rand)
                {
                    case 1:
                        xItem = 72;
                        break;
                    case 2:
                        xItem = 211;
                        amount = 3;
                        break;
                    case 3:
                    case 4:
                        xItem = 82;
                        break;
                    case 5:
                    case 6:
                        xItem = 221;
                        amount = 3;
                        break;
                    case 7:
                    case 8:
                        xItem = 54;
                        break;
                    case 9:
                        xItem = 83;
                        break;
                    case 10:
                        xItem = 222;
                        amount = 3;
                        break;
                    case 11:
                        xItem = 77;
                        amount = 1;
                        break;
                    case 12:
                        xItem = 216;
                        amount = 3;
                        break;
                    case 13:
                        xItem = 85;
                        amount = 1;
                        break;
                    case 14:
                        xItem = 224;
                        amount = 5;
                        break;
                    case 15:
                        xItem = 1244;
                        amount = 1;
                        break;
                    case 16:
                        xItem = 1239;
                        amount = 1;
                        break;
                    case 17:
                        xItem = 1226;
                        amount = 1;
                        break;
                    case 18:
                        xItem = 1207;
                        amount = 1;
                        break;
                    case 19:
                        xItem = 1248;
                        amount = 1;
                        break;
                    case 20:
                        xItem = 1245;
                        amount = 1;
                        break;
                    case 21:
                        xItem = 1230;
                        amount = 1;
                        break;
                    case 22:
                        xItem = 1213;
                        amount = 1;
                        break;
                    case 23:
                        xItem = 1249;
                        amount = 1;
                        break;
                    case 24:
                        xItem = 1247;
                        amount = 1;
                        break;
                    case 25:
                        xItem = 1236;
                        amount = 1;
                        break;
                    case 26:
                        xItem = 1218;
                        amount = 1;
                        break;
                    default:
                        xItem = 72;
                        break;
                }

                if (!dbPlayer.Container.AddItem(xItem, amount))
                {
                    dbPlayer.SendNewNotification("Dein Inventar ist leider voll!");
                }
                size += ItemModelModule.Instance.Get(xItem).Weight * amount;
            }

            dbPlayer.SendNewNotification("Sie haben eine Waffenkiste entpackt!", title: "Waffenkiste entpackt");
            return true;
        }

        public static async Task<bool> WeaponChest2(DbPlayer dbPlayer, ItemModel ItemData)
        {
            // Restrict 4 Cops
            if (!dbPlayer.IsBadOrga() && !dbPlayer.IsAGangster())
                return false;

            if (dbPlayer.Container.GetItemAmount(301) < 1)
            {
                dbPlayer.SendNewNotification(
                    "Um eine Militärkiste aufzubrechen benötigen Sie ein Brecheisen!");
                return false;
            }

            Chats.sendProgressBar(dbPlayer, 15000);

            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);

            await Task.Delay(15000);

            dbPlayer.SetCannotInteract(false);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

            // Choose Items
            float size = 0;

            while (size < ItemData.Weight)
            {
                Random rnd = new Random();
                int rand = rnd.Next(1, 59);
                uint xItem = 0;
                int amount = 1;
                switch (rand)
                {
                    case 1:
                        xItem = 487; // goldbarren
                        amount = 10;
                        break;
                    case 2:
                        xItem = 40; // marksman
                        amount = 2;
                        break;
                    case 3:
                    case 4:
                    case 5:
                        xItem = 348; // marksam ammo
                        amount = 1;
                        break;
                    case 6:
                    case 7:
                    case 8:
                        xItem = 77; // gusenberg
                        amount = 1;
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        xItem = 216; // gusenberg mag
                        amount = 10;
                        break;
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        xItem = 81; // advancedrifle
                        amount = 1;
                        break;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                        xItem = 220; // advancedr mag
                        amount = 10;
                        break;
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                        xItem = 40; // schutzweste
                        amount = 5;
                        break;
                    case 28:
                    case 29:
                    case 30:
                    case 31:
                        xItem = 84; // compactrifle
                        amount = 1;
                        break;
                    case 32:
                    case 33:
                    case 34:
                    case 35:
                    case 36:
                    case 37:
                        xItem = 172; // schweissgerät
                        amount = 1;
                        break;
                    case 38:
                    case 39:
                    case 40:
                        xItem = 172; // marksman griff
                        amount = 1;
                        break;
                    case 41:
                    case 42:
                    case 43:
                        xItem = 172; // marksman 16er mag
                        amount = 1;
                        break;
                    case 44:
                    case 45:
                    case 46:
                        xItem = 172; // marksman light
                        amount = 1;
                        break;
                    case 47:
                    case 48:
                    case 49:
                        xItem = 349; // marksman schalld
                        amount = 1;
                        break;
                    case 50:
                    case 51:
                    case 52:
                        xItem = 1246; // advanced scope
                        amount = 1;
                        break;
                    case 53:
                    case 54:
                    case 55:
                        xItem = 2237; // advanced light
                        amount = 1;
                        break;
                    case 56:
                    case 57:
                    case 58:
                        xItem = 1217; // advanced schalld.
                        amount = 1;
                        break;
                    default:
                        xItem = 223;
                        amount = 10;
                        break;
                }

                if (!dbPlayer.Container.AddItem(xItem, amount))
                {
                    dbPlayer.SendNewNotification("Dein Inventar ist leider voll!");
                }
                size += ItemModelModule.Instance.Get(xItem).Weight * amount;
            }

            dbPlayer.SendNewNotification("Sie haben eine Militärkiste entpackt!", title: "Militärkiste entpackt");
            return true;
        }
    }
}
