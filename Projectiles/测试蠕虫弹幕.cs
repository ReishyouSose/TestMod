using TestMod.ModPlayers;

namespace TestMod.Projectiles
{
    public class 逻辑 : 蠕虫弹幕基类
    {
        public override string Texture => "TestMod/Pictures/Projectiles/头";
        public override float Offset => 1f;
        public override bool ModPlayerMinionBool
        {
            get => Player.GetModPlayer<MinionPlayer>().TestMinion;
            set => Player.GetModPlayer<MinionPlayer>().TestMinion = value;
        }
    }
    public abstract class 基本召唤物 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }
        public class 头 : 基本召唤物 { public override string Texture => "TestMod/Pictures/Projectiles/头"; }
        public class 身 : 基本召唤物 { public override string Texture => "TestMod/Pictures/Projectiles/身"; }
        public class 尾 : 基本召唤物 { public override string Texture => "TestMod/Pictures/Projectiles/尾"; }
    }
}
