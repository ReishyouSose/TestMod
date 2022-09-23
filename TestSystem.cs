using Terraria;

namespace TestMod
{
    public class TestSystem : ModSystem
    {
        public override void AddRecipeGroups()
        {
            RecipeGroup group = new RecipeGroup(() => "任意矿石",//这里的string是游戏内的合成组名字
                new int[]
                {
                    ItemID.CobaltOre,
                    ItemID.AdamantiteOre
                });
            RecipeGroup.RegisterGroup("合成组：任意矿石", group);//添加到系统内，这里的string是添加合成组时的名字
        }
    }
}
