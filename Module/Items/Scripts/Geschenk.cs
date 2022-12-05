using System;
using System.Threading.Tasks;
using VMP_CNR.Module.Geschenk;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Geschenk(DbPlayer dbPlayer)
        {
            GeschenkModule.Instance.GenerateRandomReward(dbPlayer);
            return false;
        }
    }
}
