using System.Collections.Generic;
using System.IO;
using System.Text;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;

namespace VMP_CNR.Module.Configurations
{
    public class ConfigurationModule : Module<ConfigurationModule>
    {
        private readonly Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

        public override int GetOrder()
        {
            return 3;
        }

        protected override bool OnLoad()
        {
            data.Clear();

            Configuration.Instance = new DefaultConfiguration(data);
            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.J) return false;

            if (!dbPlayer.CanInteract()) return false;
            if (dbPlayer.RageExtension.IsInVehicle /*|| dbPlayer.Player.IsInVehicle*/) return false;
            if (PhoneCall.IsPlayerInCall(dbPlayer.Player)) return false;

            if (dbPlayer.HasData("salute"))
            {
                dbPlayer.ResetData("salute");
                dbPlayer.StopAnimation();
                return true;
            }
            else
            {
                dbPlayer.SetData("salute", 1);
                dbPlayer.PlayAnimation(AnimationMenuModule.Instance.animFlagDic[(uint)AnimationFlagList.WalkAndLoop], "anim@mp_player_intincarsalutestd@rds@",
                    "idle_a");
                return true;
            }
        }
    }
}