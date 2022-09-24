using TestMod.ModPlayers;

namespace TestMod.Projectiles
{/*
    public abstract class TestMinion : MultipleSectionsMinion
    {
        // 兴许需要重写一下贴图路径
        public class LogicProj : TestMinion { public override string Texture => "TestMod/Pictures/Projectiles/头"; }
        public class 头 : TestMinion { public override string Texture => "TestMod/Pictures/Projectiles/头"; }
        public class 身 : TestMinion { public override string Texture => "TestMod/Pictures/Projectiles/身"; }
        public class 尾 : TestMinion { public override string Texture => "TestMod/Pictures/Projectiles/尾"; }
        public override bool ModPlayerMinionBool// 把上面继承了ModPlayer类的召唤物bool重写进来
        {
            get => Player.GetModPlayer<MinionPlayer>().TestMinion;
            set => Player.GetModPlayer<MinionPlayer>().TestMinion = value;
        }
        public override int DustType => DustID.IceTorch;// 这是个蓝白色的粒子
        public override float Offset => 0.45f;// 偏移
        // 重写四种弹幕Type
        public override int Logic => ModContent.ProjectileType<LogicProj>();
        public override int Head => ModContent.ProjectileType<头>();
        public override int Body => ModContent.ProjectileType<身>();
        public override int Tail => ModContent.ProjectileType<尾>();
        public override double ModifyProjDamage(int projCount)// 重写伤害加成
        {
            return base.ModifyProjDamage(projCount);// 假装我重写了（
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = Projectile.height = 32;
            Projectile.scale = 1.25f;
            // 想改基本属性就写这，base别丢
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawSet(Main.spriteBatch, 2, lightColor, 0);
            return false;
        }
    }*/
}
