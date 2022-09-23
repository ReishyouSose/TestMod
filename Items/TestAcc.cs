using System;
using Terraria;

namespace TestMod.Items
{
    public class TestAcc:ModItem
    {
        public override string Texture => "TestMod/Pictures/Items/TestAcc";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("TestAcc");
            DisplayName.AddTranslation(Chinese(),"测试饰品");
            Tooltip.SetDefault("This is a test acc");
            Tooltip.AddTranslation(Chinese(), "这是一个测试饰品");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.scale = 1f;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;//稀有度
            Item.value = Item.sellPrice();
            Item.defense = 10;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 10;                   //近战， 远程， 魔法， 召唤 ，无 ，    ，所有
            player.GetDamage<MeleeDamageClass>() += 1f;//Melee,Ranged,Magic,Summon,Default,Generic
            player.GetDamage(DamageClass.Melee) += 1f;
            //player.GetDamage(ModContent.GetInstance<你的伤害类名>()) += xxx;
            player.GetCritChance(DamageClass.Melee) += 1f;
            player.GetArmorPenetration(DamageClass.Melee) += 100;
            player.wingTimeMax += 100;
            player.longInvince = true;
            player.lifeRegen += 10;
            player.statLifeMax2 += 10;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(25);
            recipe.AddIngredient(ItemID.Wood, 1);
            recipe.AddIngredient(ItemID.StoneBlock, 1);
            recipe.AddIngredient(ModContent.ItemType<TestAcc>(), 1);
            if (ModLoader.TryGetMod("ModName", out Mod mod))
            {
                recipe.AddIngredient(mod.Find<ModItem>(("目标物品类名")).Type, 1);
            }
            recipe.AddRecipeGroup("合成组：任意矿石", 10);
            /*Func<string> text;
            int[] stone = new int[2];
            stone[0] = ItemID
            RecipeGroup group = new RecipeGroup(text, stone);*/
            recipe.AddCondition(Recipe.Condition.NearWater);
            recipe.AddTile(TileID.WorkBenches);
            //recipe.ReplaceResult();
            recipe.Register();
        }
    }
}
