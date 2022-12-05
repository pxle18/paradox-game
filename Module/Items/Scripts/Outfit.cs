using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Outfits;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
//Possible problem. Removed on use, but not possible to add without weapon. Readd item?
namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Outfit(DbPlayer dbPlayer, ItemModel ItemData, Item item)
        {
            try
            {
                if (dbPlayer.RageExtension.IsInVehicle) return false;

                string outfit = ItemData.Script.ToLower().Replace("outfit_", "");

                if (outfit.Length <= 0) return false;

                if (outfit == "original")
                {
                    Chats.sendProgressBar(dbPlayer, 4000);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                    if(item.Data == null || !item.Data.ContainsKey("owner") || item.Data["owner"] != dbPlayer.Id)
                    {
                        dbPlayer.SendNewNotification("Du kannst keine Kleidung von anderen Personen anziehen!");
                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        return false;
                    }
                    dbPlayer.Container.RemoveItem(ItemData, 1);

                    if (item.Data != null && item.Data.ContainsKey("props") && item.Data.ContainsKey("cloth"))
                    {
                        string clothesstring = item.Data["cloth"];
                        Dictionary<int, uint> clothDic = clothesstring.TrimEnd(';').Split(';').ToDictionary(it => Convert.ToInt32(it.Split('=')[0]), it => Convert.ToUInt32(it.Split('=')[1]));

                        string propsstring = item.Data["props"];
                        Dictionary<int, uint> PropsDic = propsstring.TrimEnd(';').Split(';').ToDictionary(it => Convert.ToInt32(it.Split('=')[0]), it => Convert.ToUInt32(it.Split('=')[1]));

                        dbPlayer.Character.Clothes = clothDic;
                        dbPlayer.Character.EquipedProps = PropsDic;
                    }

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                    await Task.Delay(4000);
                    dbPlayer.StopAnimation();
                    dbPlayer.ApplyCharacter(false, true);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                    ClothModule.SaveCharacter(dbPlayer);

                    if (dbPlayer.HasData("outfitactive")) dbPlayer.ResetData("outfitactive");

                    dbPlayer.SendNewNotification("Sie haben die Kleidung erfolgreich angezogen!");
                    return true;
                }

                if (!Int32.TryParse(outfit, out int outfitid))
                {
                    return false;
                }

                // Heist check
                if (outfitid == 66 && !dbPlayer.HasData("heistActive"))
                {
                    dbPlayer.SendNewNotification("Kann nur angezogen werden, wenn ein Heist aktiv ist!", PlayerNotification.NotificationType.ERROR);
                    return false;
                }

                Chats.sendProgressBar(dbPlayer, 4000);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.Container.RemoveItem(ItemData, 1);

                Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
                Data.Add("cloth", String.Join(';', string.Join(";", dbPlayer.Character.Clothes.Select(x => x.Key + "=" + x.Value).ToArray())));
                Data.Add("props", String.Join(',', string.Join(";", dbPlayer.Character.EquipedProps.Select(x => x.Key + "=" + x.Value).ToArray())));
                Data.Add("owner", dbPlayer.Id);

                dbPlayer.Container.AddItem(737, 1, Data);
                dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                await Task.Delay(3000);

                if (dbPlayer == null || !dbPlayer.IsValid()) return false;

                // Armor westen
                if (ItemData.Id == 865 || ItemData.Id == 1346) dbPlayer.SetArmor(100);

                await Task.Delay(1000);

                dbPlayer.StopAnimation();

                dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                OutfitsModule.Instance.SetPlayerOutfit(dbPlayer, outfitid, true);

                ClothModule.SaveCharacter(dbPlayer);

                dbPlayer.SendNewNotification(
                    "Sie haben die Kleidung erfolgreich angezogen!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }
        public static async Task<bool> ClothesBag(DbPlayer dbPlayer, ItemModel ItemData, Item item)
        {
            try
            {
                return false;

                if (dbPlayer.RageExtension.IsInVehicle) return false;

                if (!dbPlayer.IsAGangster() && dbPlayer.IsBadOrga()) return false;

                DbPlayer closestPlayer = Players.Players.Instance.GetClosestPlayerForPlayer(dbPlayer, 2.0f);

                if (closestPlayer == null || !closestPlayer.IsValid()) return false;

                if (!closestPlayer.IsTied || !closestPlayer.IsInDuty() || !closestPlayer.IsACop())
                {
                    dbPlayer.SendNewNotification("Der Beamte muss im Dienst und gefesselt sein!");
                    return false;
                }

                Chats.sendProgressBar(dbPlayer, 30000);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                await NAPI.Task.WaitForMainThread(10000);
                // AUSZIEHEN
                if (closestPlayer.IsMale())
                {
                    closestPlayer.SetClothes(3, 15, 0); //Torso
                    closestPlayer.SetClothes(8, 15, 0); //Undershirt
                    closestPlayer.SetClothes(11, 15, 0); //Tops
                }
                else
                {
                    closestPlayer.SetClothes(3, 15, 0); //Torso
                    closestPlayer.SetClothes(8, 15, 0); //Undershirt
                    closestPlayer.SetClothes(11, 15, 0); //Tops
                }

                closestPlayer.SetData("lastAusgezogen", DateTime.Now);

                await NAPI.Task.WaitForMainThread(5000);
                if (closestPlayer.IsMale())
                {
                    closestPlayer.SetClothes(4, 61, 0);
                }
                else
                {
                    closestPlayer.SetClothes(4, 15, 0);
                }


                await NAPI.Task.WaitForMainThread(5000);
                if (closestPlayer.IsMale())
                {
                    closestPlayer.SetClothes(6, 34, 0);
                }
                else
                {
                    closestPlayer.SetClothes(6, 35, 0);
                }
                ClothModule.SaveCharacter(closestPlayer);

                await NAPI.Task.WaitForMainThread(10000);

                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.IsInjured())
                {
                    return false;
                }

                // something happend to officer || Out of range
                if (closestPlayer.IsInjured() || closestPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f)
                {
                    return false;
                }


                Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
                Data.Add("cloth", String.Join(';', string.Join(";", closestPlayer.Character.Clothes.Select(x => x.Key + "=" + x.Value).ToArray())));
                Data.Add("props", String.Join(',', string.Join(";", closestPlayer.Character.EquipedProps.Select(x => x.Key + "=" + x.Value).ToArray())));
                Data.Add("gender", closestPlayer.IsMale());

                dbPlayer.Container.AddItem(1104, 1, Data);


                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();

                dbPlayer.SendNewNotification("Sie haben die Kleidung erfolgreich eingepackt!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }

        public static async Task<bool> PackedClothesBag(DbPlayer dbPlayer, ItemModel ItemData, Item item)
        {
            try
            {
                if (dbPlayer.RageExtension.IsInVehicle) return false;

                if (!dbPlayer.IsAGangster() && dbPlayer.IsBadOrga()) return false;


                Chats.sendProgressBar(dbPlayer, 4000);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                if (item.Data == null)
                {
                    return false;
                }

                if (item.Data != null && item.Data.ContainsKey("props") && item.Data.ContainsKey("cloth") && item.Data.ContainsKey("gender") && (item.Data["gender"] == dbPlayer.IsMale()))
                {
                    string clothesstring = item.Data["cloth"];
                    Dictionary<int, uint> clothDic = clothesstring.TrimEnd(';').Split(';').ToDictionary(it => Convert.ToInt32(it.Split('=')[0]), it => Convert.ToUInt32(it.Split('=')[1]));

                    string propsstring = item.Data["props"];
                    Dictionary<int, uint> PropsDic = propsstring.TrimEnd(';').Split(';').ToDictionary(it => Convert.ToInt32(it.Split('=')[0]), it => Convert.ToUInt32(it.Split('=')[1]));

                    dbPlayer.Character.Clothes = clothDic;
                    dbPlayer.Character.EquipedProps = PropsDic;
                }
                else
                {
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    return false;
                }

                dbPlayer.Container.RemoveItem(ItemData, 1);

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                await Task.Delay(4000);
                dbPlayer.StopAnimation();
                dbPlayer.ApplyCharacter(false, true);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);

                ClothModule.SaveCharacter(dbPlayer);

                dbPlayer.SendNewNotification("Sie haben die Kleidung erfolgreich angezogen!");

                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }
    }
}
