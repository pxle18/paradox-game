using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Attachments;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Injury
{
    public class InjuryMedicModule : Module<InjuryMedicModule>
    {
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsAMedic() || !dbPlayer.IsInDuty()) return false;

            SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehicleFromTeam(dbPlayer.Player.Position, (int)TeamTypes.TEAM_MEDIC, 3);

            if(sxVeh != null && sxVeh.IsValid())
            {
                if (!dbPlayer.Attachments.ContainsKey((int)Attachment.MEDICBAG))
                {
                    AttachmentModule.Instance.AddAttachment(dbPlayer, (int)Attachment.MEDICBAG);
                }
                else
                {
                    AttachmentModule.Instance.RemoveAttachment(dbPlayer, (int)Attachment.MEDICBAG);
                }
                return true;
            }
            return false;
        }
    }
}
