using GTANetworkAPI;
using System.Threading.Tasks;
using VMP_CNR.Module.Players.Db;


namespace VMP_CNR.Module.Node
{
    public sealed class NodeModule : Module<NodeModule>
    {
        public void Call(NodeCall nodeCall)
        {
            Request.Call(nodeCall.Name, nodeCall.Args);
        }

        public override bool Load(bool reload = false)
        {
            Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(60000);// Workaround for 

                VehicleLoadup.Instance.StartResyncVehicleBridges();

            });

            return base.Load(reload);
        }

        //Triggers a client event in the players streaming range
        public void TriggerNewClientStreamingRange(DbPlayer dbPlayer, string eventName, string arg)
        {
            Request.TriggerInStreamingRange(dbPlayer.Player.Handle.Value.ToString(), arg);
        }

        public void Trigger(DbPlayer dbPlayer, string eventName, params string[] args)
        {
            
        }
        
        public void SetPlayerProp(DbPlayer dbPlayer, int prop, int drawable, int texture)
        {
            Request.SetPlayerProps(dbPlayer.Player.Value.ToString(), prop.ToString(), drawable.ToString(), texture.ToString());
        }
    }
}