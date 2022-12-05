using GTANetworkAPI;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.NpcSpawner
{
    public sealed class NpcSpawnerModule : Module<NpcSpawnerModule>
    {
        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            foreach (Npc npc in Main.ServerNpcs)
            {
                dbPlayer.Player.TriggerNewClient("loadNpc", npc.PedHash, npc.Position.X, npc.Position.Y, npc.Position.Z, npc.Heading, npc.Dimension);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreatenpc(Player player, string pedhash)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!dbPlayer.IsValid()) return;

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string rotz = player.Heading.ToString().Replace(",", ".");

            if(Enum.TryParse(pedhash, true, out PedHash skin))
            {
                Main.ServerNpcs.Add(new Npc(skin, player.Position, player.Heading, player.Dimension));

                MySQLHandler.ExecuteAsync(
                    $"INSERT INTO additionally_npcs (ped_hash, pos_x, pos_y, pos_z, heading, dimension, description) VALUES ('{skin}', '{x}', '{y}', '{z}', '{rotz}', '{player.Dimension}', 'NPCCreater IC by {dbPlayer.GetName()}');");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreateeaster(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!dbPlayer.IsValid()) return;

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = (player.Position.Z-0.5f).ToString().Replace(",", ".");
            string rotz = player.Heading.ToString().Replace(",", ".");
            
            Main.ServerNpcs.Add(new Npc(PedHash.Rabbit, new Vector3(player.Position.X, player.Position.Y, player.Position.Z), player.Heading, player.Dimension));

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO event_easterrabbits (pos_x, pos_y, pos_z, heading) VALUES ('{x}', '{y}', '{z}', '{rotz}');");

            dbPlayer.SendNewNotification("Osterhase created!");
        }
    }
}