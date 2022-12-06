using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Injury.InjuryMove.Menu;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Injury.InjuryMove
{
    public class InjuryMoveModule : SqlModule<InjuryMoveModule, InjuryMovePoint, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `lsmc_medicbeds`;";
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new InjuryMoveMenuBuilder());
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(key == Key.E && (dbPlayer.TeamId == (int)TeamTypes.TEAM_MEDIC || (dbPlayer.TeamId == (int)TeamTypes.TEAM_ARMY && dbPlayer.ParamedicLicense)))
            {
                if(dbPlayer.IsInDuty())
                {
                    InjuryMovePoint injuryMovePoint = Instance.GetAll().Values.Where(ip => ip.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f && ip.Dimension == dbPlayer.Player.Dimension).FirstOrDefault();

                    if (injuryMovePoint != null)
                    {
                        DbPlayer injuredPlayer = Players.Players.Instance.GetClosestInjuredForPlayer(dbPlayer, 2.0f);
                        if (injuredPlayer != null && injuredPlayer.IsValid())
                        {
                            dbPlayer.SetData("playerToMove", injuredPlayer.Id);
                            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.InjuryMoveMenu, dbPlayer).Show(dbPlayer);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return false;

            if (!colShape.HasData("InjuryMovePointID"))
                return false;

            uint InjuryMoveID = uint.Parse(colShape.GetData<uint>("InjuryMovePointID").ToString());
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("InjuryMovePointID", InjuryMoveID);

                    if (dbPlayer.IsInjured())
                    {
                        dbPlayer.Player.SetSharedData("voiceRange", (int)VoiceRangeTypes.Whisper);
                        dbPlayer.SetData("voiceType", 3);
                        dbPlayer.Player.TriggerNewClient("setVoiceType", 3);
                    }
                    break;
                case ColShapeState.Exit:
                    if (dbPlayer.HasData("InjuryMovePointID"))
                        dbPlayer.ResetData("InjuryMovePointID");

                    break;
                default:
                    break;
            }

            return true;
        }
    }
}
