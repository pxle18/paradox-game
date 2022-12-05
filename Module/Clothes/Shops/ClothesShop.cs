using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Clothes.Shops
{
    public class ClothesShop
    {
        public uint Id { get; }
        
        public Vector3 Position { get; }

        public int Teamid { get; }
        
        public string Name { get; }
        
        public ColShape Colshape { get; }

        public bool CouponUsable { get; set; }

        private readonly Dictionary<int, List<Cloth>> clothes;

        private readonly Dictionary<int, ClothesSlot> clothesSlots;

        private readonly Dictionary<int, List<Prop>> props;

        private readonly Dictionary<int, PropsSlot> propsSlots;

        public uint CWSId { get; set; }
        public uint EventID { get; set; }

        public ClothesShop(MySqlDataReader reader, Dictionary<int, List<Cloth>> clothes,
            Dictionary<int, List<Prop>> props, Dictionary<int, ClothesSlot> clothesSlots,
            Dictionary<int, PropsSlot> propsSlots)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Name = reader.GetString("name");
            Colshape = ColShapes.Create(Position, 3.0f);
            Colshape.SetData("clothShopId", Id);
            this.clothes = clothes ?? new Dictionary<int, List<Cloth>>();
            this.props = props ?? new Dictionary<int, List<Prop>>();
            this.clothesSlots = clothesSlots ?? new Dictionary<int, ClothesSlot>();
            this.propsSlots = propsSlots ?? new Dictionary<int, PropsSlot>();
            CouponUsable = reader.GetInt32("no_voucher") == 0;
            CWSId = reader.GetUInt32("cws_id");
            EventID = reader.GetUInt32("event_id");

            if (EventID > 0)
            {
                Colshape.SetData("eventId", EventID);
            }
        }

        public List<Cloth> GetClothesBySlotAndTeam(int slot, uint teamId, int gender)
        {
            List<Cloth> returnClothes = new List<Cloth>();

            if (!clothes.ContainsKey(slot)) return null;

            foreach (Cloth cloth in clothes[slot])
            {
                if (cloth.Teams.Contains(teamId) && cloth.Gender == gender)
                {
                    returnClothes.Add(cloth);
                }
            }

            return returnClothes.Count > 0 ? returnClothes : null;
        }

        public List<Prop> GetPropsBySlotAndTeam(int slot, uint teamId, int gender)
        {
            List<Prop> returnProps = new List<Prop>();

            if (!props.ContainsKey(slot)) return null;

            foreach (Prop prop in props[slot])
            {
                // refactor this if we use hashset on teams (like clothes)
                if (prop != null && prop.TeamId == teamId && prop.Gender == gender)
                {
                    returnProps.Add(prop);
                }
            }

            return returnProps.Count > 0 ? returnProps : null;
        }

        public Dictionary<int, ClothesSlot> GetClothesSlots()
        {
            return clothesSlots;
        }

        public Dictionary<int, PropsSlot> GetPropsSlots()
        {
            return propsSlots;
        }

        public Dictionary<int, ClothesSlot> GetClothesSlotsForPlayer(DbPlayer player)
        {
            var availableSlots = new Dictionary<int, ClothesSlot>();
            foreach (var slot in ClothesShopModule.Instance.GetClothesSlots())
            {
                var clothesBySlot = GetClothesBySlotForPlayer(slot.Key, player);
                if (clothesBySlot != null && clothesBySlot.Any())
                {
                    availableSlots[slot.Key] = slot.Value;
                }
            }

            return availableSlots;
        }

        public Dictionary<int, PropsSlot> GetPropsSlotsForPlayer(DbPlayer player)
        {
            var availableSlots = new Dictionary<int, PropsSlot>();
            foreach (var slot in ClothesShopModule.Instance.GetPropsSlots())
            {
                var clothesBySlot = GetPropsBySlotForPlayer(slot.Key, player);
                if (clothesBySlot != null && clothesBySlot.Any())
                {
                    availableSlots[slot.Key] = slot.Value;
                }
            }

            return availableSlots;
        }

        public List<Prop> GetPropsBySlotAndCategoryForPlayer(int slot, int category, DbPlayer player)
        {
            if (!props.ContainsKey(slot)) return new List<Prop>();

            return props[slot].Where(
                prop => (prop.TeamId == player.TeamId || prop.TeamId == (uint) teams.TEAM_CIVILIAN)
                    && (prop.Gender == player.Customization.Gender || prop.Gender == 3)
                    && prop.SubCatId == category
            ).ToList();
        }

        public List<Cloth> GetClothesBySlotAndCategoryForPlayer(int slot, int category, DbPlayer player)
        {
            if (!clothes.ContainsKey(slot)) return new List<Cloth>();

            return clothes[slot].Where(
                cloth => (cloth.Teams.Contains(player.TeamId) || cloth.Teams.Contains((uint) teams.TEAM_CIVILIAN))
                         && (cloth.Gender == player.Customization.Gender || cloth.Gender == 3)
                         && cloth.SubCatId == category
            ).ToList();
        }

        public List<Cloth> GetClothesBySlotForPlayer(int slot, DbPlayer player)
        {
            var currClothes = new List<Cloth>();
            var slotClothes = GetClothesBySlotAndTeam(slot, player.TeamId, player.Customization.Gender);
            if (slotClothes != null)
            {
                currClothes.AddRange(slotClothes);
            }

            var slotClothesAllGender = GetClothesBySlotAndTeam(slot, player.TeamId, 3);
            if (slotClothesAllGender != null)
            {
                currClothes.AddRange(slotClothesAllGender);
            }

            if (player.TeamId == (int) teams.TEAM_CIVILIAN) return currClothes;
            var slotTeamClothes =
                GetClothesBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, player.Customization.Gender);
            if (slotTeamClothes != null)
            {
                currClothes.AddRange(slotTeamClothes);
            }

            var slotTeamClothesAllGender = GetClothesBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, 3);
            if (slotTeamClothesAllGender != null)
            {
                currClothes.AddRange(slotTeamClothesAllGender);
            }

            return currClothes;
        }

        public List<Prop> GetPropsBySlotForPlayer(int slot, DbPlayer player)
        {
            var currClothes = new List<Prop>();
            var slotClothes = GetPropsBySlotAndTeam(slot, player.TeamId, player.Customization.Gender);
            if (slotClothes != null)
            {
                currClothes.AddRange(slotClothes);
            }

            var slotClothesAllGender = GetPropsBySlotAndTeam(slot, player.TeamId, 3);
            if (slotClothesAllGender != null)
            {
                currClothes.AddRange(slotClothesAllGender);
            }

            if (player.TeamId == (int) teams.TEAM_CIVILIAN) return currClothes;
            var slotTeamClothes =
                GetPropsBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, player.Customization.Gender);
            if (slotTeamClothes != null)
            {
                currClothes.AddRange(slotTeamClothes);
            }

            var slotTeamClothesAllGender = GetPropsBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, 3);
            if (slotTeamClothesAllGender != null)
            {
                currClothes.AddRange(slotTeamClothesAllGender);
            }

            return currClothes;
        }
    }
}