using Terraria.ModLoader.IO;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace TestMod.Items
{
    public class TestWeapon : ModItem
    {
        public override string Texture => "TestMod/Items/TestItem";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("测试武器");
            Tooltip.SetDefault("这是一把测试武器");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.scale = 1f;
            Item.damage = 50;//伤害值
            Item.DamageType = DamageClass.Melee;//伤害类型
            Item.crit = 4;//暴击率
            Item.useTime = 17;//使用时间
            Item.useAnimation = 17;//使用动画时间
            Item.reuseDelay = 0;//重新使用延迟
            Item.knockBack = 6;//击退
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = false;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.shoot = ModContent.ProjectileType<TestProj>();
            Item.shootSpeed = 10f;
            //Item.useAmmo = AmmoID.None;
            //Item.ammo = AmmoID.Arrow;
            //Item.consumable = false;
            //Item.buffType = BuffID.Frostburn;
            //Item.buffTime = 180;
            //Item.healLife = 100;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
        }
        public override void HoldItem(Player player)//手持物品时生效
        {
            //和饰品更新相同写法
        }
    }
}
