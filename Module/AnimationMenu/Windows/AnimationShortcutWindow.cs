using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.AnimationMenu.Windows
{
    public class AnimationShortcutWindow : Window<Func<DbPlayer, List<AnimationShortcutWindow.SimpleAnimation>, bool>>
    {
        /// <summary>
        /// Simple abstract version of a piece of cloth or prop.
        /// </summary>
        public class SimpleAnimation
        {
            public uint Id { get; set; }

            public string Name { get; set; }

            public SimpleAnimation(uint id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        /// <summary>
        /// JSON data which will be delivered.
        /// </summary>
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "animations")]
            private List<SimpleAnimation> Animations { get; }

            public ShowEvent(DbPlayer dbPlayer, List<SimpleAnimation> animations) : base(dbPlayer)
            {
                Animations = animations;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnimationShortcutWindow() : base("AnimationWheelFavoritesList")
        {
        }

        /// <summary>
        /// Event handler if the window will be shown.
        /// </summary>
        /// <returns></returns>
        public override Func<DbPlayer, List<SimpleAnimation>, bool> Show()
        {
            return (player, animations) => OnShow(new ShowEvent(player, animations));
        }

        /// <summary>
        /// Saves the given animation item from itemId to players animation shortcuts slot.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="slotId"></param>
        /// <param name="itemId"></param>
        [RemoteEvent]
        public void AnimationShortcutSetSlot(Player client, string slotId, string itemId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return;

            try
            {
                if (!uint.TryParse(slotId, out var slot)) return;
                if (!uint.TryParse(itemId, out var item)) return;

                if (!AnimationItemModule.Instance.GetAll().ContainsKey(item)) return;
                if (!dbPlayer.AnimationShortcuts.ContainsKey(slot)) return;

                AnimationItem animationItem = AnimationItemModule.Instance.GetAll()[item];

                dbPlayer.AnimationShortcuts[slot] = animationItem.Id;
                dbPlayer.SendNewNotification(
                    $"Animationsslot {slot} mit {animationItem.Name} belegt!"
                );
                dbPlayer.SaveAnimationShortcuts();
                dbPlayer.UpdateAnimationShortcuts();
            }
            catch (Exception e)
            {
                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");
                Logger.Crash(e);
            }
        }
    }
}