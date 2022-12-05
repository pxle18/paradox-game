using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Windows
{
    public class CustomizationWindow : Window<Func<DbPlayer, CharacterCustomization, bool>>
    {
        private class ShowEvent : Event
        {
            //private string InventoryContent { get; } // --- appears to be empty if used?
            [JsonProperty(PropertyName = "customization")] private CharacterCustomization Customization { get; }
            [JsonProperty(PropertyName = "level")] private int Level { get; }

            public ShowEvent(DbPlayer dbPlayer, CharacterCustomization customization) : base(dbPlayer)
            {
                Customization = customization;
                Level = dbPlayer.HasData("firstCharacter") ? 0 : dbPlayer.Level;
            }
        }
        public override Func<DbPlayer, CharacterCustomization, bool> Show()
        {
            return (player, customization) => OnShow(new ShowEvent(player, customization));
        }

        public CustomizationWindow() : base("CharacterCreator")
        {
        }

        [RemoteEvent]
        public async void UpdateCharacterCustomization(Player player, string charakterJSON, int price, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || String.IsNullOrEmpty(charakterJSON)) return;
            CharacterCustomization customization = JsonConvert.DeserializeObject<CharacterCustomization>(charakterJSON);

            if (dbPlayer.NeuEingereist())
            {
                dbPlayer.Customization = customization;
                dbPlayer.SaveCustomization();
                dbPlayer.ResetData("firstCharacter");
            }
            else
            {
                int result = dbPlayer.TakeAnyMoney(price);

                if (result != -1)
                {
                    // Buy Customization
                    dbPlayer.Customization = customization;
                    dbPlayer.SaveCustomization();
                    dbPlayer.SendNewNotification($"Aussehen geaendert, dir wurden {price}$ vom Konto abgezogen", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                }
                else
                {
                    dbPlayer.SendNewNotification("Nicht genug Geld", notificationType: PlayerNotification.NotificationType.ERROR);
                }
            }

            // Update Charakter
            dbPlayer.ResetData("firstCharacter");

            await dbPlayer.StopCustomization();
        }

        [RemoteEvent]
        public async void StopCustomization(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            dbPlayer.ApplyCharacter();
            await dbPlayer.StopCustomization();
        }

    }
}
