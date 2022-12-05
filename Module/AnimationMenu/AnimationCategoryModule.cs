using VMP_CNR.Module.Assets.Tattoo;

namespace VMP_CNR.Module.AnimationMenu
{
    public class AnimationCategoryModule : SqlModule<AnimationCategoryModule, AnimationCategory, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `animationmenu_category` ORDER by `order`;";
        }
    }
}
