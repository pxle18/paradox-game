using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Weapons;

namespace VMP_CNR.Module.Interaction
{
    public class MetallDetectorModule : SqlModule<MetallDetectorModule, MetallDetector, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `metaldetectors`;";
        }


        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShape == null || dbPlayer == null || !dbPlayer.IsValid()) return false;
            if(colShapeState == ColShapeState.Enter)
            {
                if(colShape.HasData("detector"))
                {
                    uint mId = colShape.GetData<uint>("detector");
                    MetallDetector metal = Instance.Get(mId);

                    if (metal == null) return false;

                    bool validWeapon = false;
                    foreach(WeaponDetail wp in dbPlayer.Weapons.ToList())
                    {
                        var l_WeaponData = Weapons.Data.WeaponDataModule.Instance.Get(wp.WeaponDataId);
                        if(l_WeaponData != null && l_WeaponData.Weight > 0)
                        {
                            validWeapon = true;
                            break;
                        }
                    }

                    if (!validWeapon)
                    {
                        foreach (Item item in dbPlayer.Container.Slots.Values.ToList())
                        {
                            if (item != null && item.Model != null && (item.Model.Script.ToLower().Contains("w_") || item.Model.Script.ToLower().Contains("ammo")))
                            {
                                validWeapon = true;
                                break;
                            }
                        }
                    }

                    if(validWeapon && metal.LastDetected.AddSeconds(5) <= DateTime.Now)
                    {
                        metal.LastDetected = DateTime.Now;
                        foreach (DbPlayer xPlayer in Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 10.0f))
                        {
                            xPlayer.SendNewNotification($"1337Allahuakbar$detector", duration: 3000);
                        }
                    }
                }
            }

            return false;
        }

    }

    public class MetallDetector : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Range { get; set; }

        public DateTime LastDetected { get; set; }

        public MetallDetector(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Range = reader.GetFloat("radius");

            if (Configurations.Configuration.Instance.DevMode) Spawners.Markers.Create(1, Position, new Vector3(), new Vector3(), 0.7f, 255, 255, 0, 0);

            LastDetected = DateTime.Now;

            Spawners.ColShapes.Create(Position, Range).SetData("detector", Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
