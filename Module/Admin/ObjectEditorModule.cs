using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Admin
{
    public class ObjectEditorModule : Module<ObjectEditorModule>
    {
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreateobj(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid()) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Object Editor", Callback = "objecteditor_create", Message = "Object Hash angeben:" });
            return;
        }

    }

    public class ObjectEditorEvents : Script
    {
        [RemoteEvent]
        public void objecteditor_create(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnstring.Length < 3) return;

            if (!Int32.TryParse(returnstring, out int hash))
            {
                hash = (int)NAPI.Util.GetHashKey(returnstring);
            }

            dbPlayer.Player.TriggerNewClient("createObject", hash);
            dbPlayer.SendNewNotification("Object created: " + returnstring);
            return;
        }

        [RemoteEvent]
        public void objed_saveobject(Player player, string hash, string x, string y, string z, string rotx, string roty, string rotz, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            x = x.ToString().Replace(",", ".");
            y = y.ToString().Replace(",", ".");
            z = z.ToString().Replace(",", ".");

            rotx = rotx.ToString().Replace(",", ".");
            roty = roty.ToString().Replace(",", ".");
            rotz = rotz.ToString().Replace(",", ".");


            x = MySqlHelper.EscapeString(x);
            y = MySqlHelper.EscapeString(y);
            z = MySqlHelper.EscapeString(z);
            rotx = MySqlHelper.EscapeString(rotx);
            roty = MySqlHelper.EscapeString(roty);
            rotz = MySqlHelper.EscapeString(rotz);

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO savedobjects (x, y, z, rot_x, rot_y, rot_z, hash) VALUES('{x}', '{y}', '{z}', '{rotx}', '{roty}', '{rotz}', '{hash}')");

            dbPlayer.SendNewNotification("Object saved!");
            return;
        }

        [RemoteEvent]
        public void objed_close(Player player, string hash, Vector3 pos, Vector3 rot, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            
            dbPlayer.SendNewNotification("Objecteditor closed!");
            return;
        }
    }
}
