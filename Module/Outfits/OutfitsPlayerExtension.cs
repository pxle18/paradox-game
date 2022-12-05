using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Outfits
{
    public static class OutfitsPlayerExtension
    {
        public static void SetOutfit(this DbPlayer dbPlayer, OutfitTypes outfitType)
        {
            OutfitsModule.Instance.SetPlayerOutfit(dbPlayer, (int)outfitType);
        }
    }
}
