﻿using Terraria.DataStructures;
using TestMod.Buffs;
using static TestMod.Projectiles.TestMinion;

namespace TestMod.Items
{
    public class TestMultipleSectionsMinionItem : MultipleSectionsMinionItem
    {
        public override string Texture => "TestMod/Pictures/Items/召唤杖";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("假的星尘龙法杖");
            Tooltip.SetDefault("测试多体节召唤杖");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.scale = 1f;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useTurn = false;
            Item.UseSound = SoundID.Item44;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.crit = 10;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.damage = 50;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.rare = ItemRarityID.Cyan;
            Item.shoot = ModContent.ProjectileType<TestMinion.LogicProj>();
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SummonSet(player, source, damage, knockback, 2, ModContent.BuffType<TestMinionBuff>(), ModContent.ProjectileType<LogicProj>(), ModContent.ProjectileType<头>(),
                ModContent.ProjectileType<身>(), ModContent.ProjectileType<尾>());
            return false;
        }
    }
}
