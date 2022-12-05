using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Outfits;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Clothes.Mobile
{
    public class MobileClothModule : Module<MobileClothModule>
    {

        public async Task PlayerSwitchMaskState(DbPlayer dbPlayer)
        {
            try
            {
                int choice = 1; // Maskierung

                if (dbPlayer.Character == null || dbPlayer.Character.Clothes == null || dbPlayer.Character.ActiveClothes == null || dbPlayer.Character.Clothes.Count <= 0) return;
                if (!dbPlayer.CanInteract()) return;

                if (dbPlayer.HasData("lastmaskestate"))
                {
                    DateTime latest = dbPlayer.GetData("lastmaskestate");
                    if (latest.AddSeconds(2) > DateTime.Now) return;
                }

                dbPlayer.SetData("lastmaskestate", DateTime.Now);
                if (!dbPlayer.Character.Clothes.ContainsKey(choice))
                    return;

                uint clothId = dbPlayer.Character.Clothes[choice];
                Cloth cloth = ClothModule.Instance[clothId];
                if (cloth == null) return;

                if (!dbPlayer.Freezed && dbPlayer.CanInteract() && dbPlayer.RageExtension.IsInVehicle == false)
                {
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "missfbi4", "takeoff_mask");
                }

                await Task.Delay(1300);
                if (dbPlayer.Character.ActiveClothes.ContainsKey(choice) && dbPlayer.Character.ActiveClothes[choice]) //Spieler hat das Kleidungsstück an
                {
                    dbPlayer.SetClothes(1, 0, 0);
                    dbPlayer.Character.ActiveClothes[choice] = false;
                }
                else //Spieler hat das Kleidungsstück nicht an
                {
                    if (dbPlayer.HasData("outfitactive"))
                    {
                        int variation = OutfitsModule.Instance.GetOutfitComponentVariation(dbPlayer, dbPlayer.GetData("outfitactive"), 1);
                        int texture = OutfitsModule.Instance.GetOutfitComponentTexture(dbPlayer, dbPlayer.GetData("outfitactive"), 1);
                        dbPlayer.SetClothes(1, variation, texture);
                    }
                    else dbPlayer.SetClothes(1, cloth.Variation, cloth.Texture);

                    if (!dbPlayer.Character.ActiveClothes.ContainsKey(choice))
                    {
                        dbPlayer.Character.ActiveClothes.TryAdd(choice, true);
                    }
                    else dbPlayer.Character.ActiveClothes[choice] = true;
                }

                // remove anim if still nothing occured
                if (!dbPlayer.Freezed && dbPlayer.CanInteract() && dbPlayer.RageExtension.IsInVehicle == false)
                    dbPlayer.StopAnimation();
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }
    }
}
