

using GTANetworkAPI;
using System;
using System.Linq;
using VMP_CNR.Module.Farming;

namespace VMP_CNR.Module.ItemPlacementFiles
{
    public class ItemPlacementFilesModule : SqlModule<ItemPlacementFilesModule, ItemPlacementFile, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `item_placement_files` ORDER BY `active`;";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(FarmSpotModule) };
        }

        protected override bool OnLoad()
        {
            NAPI.World.ResetIplList();
            return base.OnLoad();
        }

        protected override void OnItemLoaded(ItemPlacementFile itemPlacementFile)
        {
            // Wenn IPL in Farms und actual > 0 (active)
            if(FarmSpotModule.Instance.GetAll().Where(fs => fs.Value.IPLs.Contains(itemPlacementFile.Id) && fs.Value.ActualAmount > 0).Count() > 0)
            {
               NAPI.World.RequestIpl(itemPlacementFile.Hash);
            }
            else if (FarmSpotModule.Instance.GetAll().Where(fs => fs.Value.IPLs.Contains(itemPlacementFile.Id) && fs.Value.ActualAmount == 0).Count() > 0)
            {
                NAPI.World.RemoveIpl(itemPlacementFile.Hash);
            }
            // wenns nicht vorhanden ist, ebenfalls normal
            else if(FarmSpotModule.Instance.GetAll().Where(fs => fs.Value.IPLs.Contains(itemPlacementFile.Id)).Count() == 0)
            {
                if (itemPlacementFile.Active)
                {
                    NAPI.World.RequestIpl(itemPlacementFile.Hash);
                }
                else
                {
                    NAPI.World.RemoveIpl(itemPlacementFile.Hash);
                }
            }
        }
    }
}