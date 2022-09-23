using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace TestMod.Projectiles
{
    public class TestProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 180;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.alpha = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.hide = false;
            Projectile.extraUpdates = 0;
            Projectile.MaxUpdates = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
        }
        public override void AI()
        {
            //利用我们之前写的这个方法来找攻击目标
            NPC target = ChooseTarget(Projectile, 1000);
            //目标不是null，即这个弹幕现在有攻击目标才执行
            if (target != null)
            {
                //tovel是弹幕朝着目标的20倍单位向量
                Vector2 tovel = Vector2.Normalize(target.Center - Projectile.Center) * 20;
                //用渐进法设定弹幕速度，让弹幕追踪目标
                Projectile.velocity = (Projectile.velocity * 30 + tovel) / 31f;
            }
            //在弹幕中心添加一个白色的照明，比值为颜色，大小为强度
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3());
            if (Main.GameUpdateCount % 5 == 0)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.IceTorch);
            }
            //弹幕角度，先转九十度让他到正方向（因为贴图是朝上的），再加上朝向速度方向
            Projectile.rotation = MathHelper.Pi / 2f + Projectile.velocity.ToRotation();
        }
        //PreDraw,注意这是一个bool，如果返回true，则是应用原版绘制，返回false，则是关闭原版绘制。即，游戏不会再去绘制这个弹幕，需要你自己写
        //一般来说，想要搞点特效都是得自己写绘制的，已经使用原版绘制的话总是会导致弹幕碰撞箱跟我们看到的弹幕的位置不一样
        public override bool PreDraw(ref Color lightColor)
        {
            //var这玩意不知道你知不知道？，就是能直接变成被赋值的类型
            //Main.spb,这是一个绘制接口，所有的绘制都要用到这个玩意儿
            var spb = Main.spriteBatch;

            //Texture2D,引用Microsoft.Xna.Framework.Graphics，这是贴图，draw的第一个参数
            Texture2D tex = ModContent.Request<Texture2D>(
                //Request<XXX>寻找某个玩意儿，第一个参数，完整路径，
                "TestMod/Projectiles/TestProj",
                //第二个参数（选填）读取方式，强烈建议使用immediateLoad异步读取，问就是加载的大问题
                AssetRequestMode.ImmediateLoad).Value;

            //Draw方法，共有七个重载，我只推荐6和7两种，区别是里面的scale参数一个是float一个是Vector2，下面会介绍
            spb.Draw(
                tex,//第一个参数，贴图

                //第二个参数，绘制位置，为弹幕中心减去屏幕坐标，这是绘制的起始点
                Projectile.Center - Main.screenPosition,

                //第三个参数，绘制区域，填null则是不对贴图进行裁剪，这是一个rectangle(裁剪起始点X，裁剪起始点Y，区域宽，区域高)
                //起始（0,0,），长宽为贴图长宽，这等效于null不裁剪
                /*null*/new Rectangle(0, 0, tex.Width, tex.Height),

                //第四个参数，颜色,这是一个叠加上去的感觉，使用白色以不叠加
                Color.White,
                //第五个参数，旋转角,可以直接用弹幕的旋转角，这样就保持的绘制出来的跟我们之前设定的方向一致
                Projectile.rotation,

                //第六个参数，绘制中心，这是相对贴图而言，比如，我们用的这个贴图是72*72，那么如果想要绘制的时候可以让贴图以贴图中心缩放或旋转
                //我们就可以把绘制中心设置为（36,36），就是贴图长宽除以2的地方，或者用 贴图.Size() / 2f（贴图大小的一半）
                /*new Vector2(36,36)*/tex.Size() / 2f,

                //第七个参数，缩放，float 直接以绘制中心进行等比缩放，1f为不缩放，Vector2为对着XY进行单独拉伸，
                //new Vector2(0.5f, 5f)就是横向压扁到一半，纵向拉长到5倍
                /*1f*/new Vector2(0.5f, 5f),

                //第八个参数，绘制翻转，0是不翻转，1是水平翻转，2是垂直翻转，顺序对应下面三个，咱这里不用翻转
                /*SpriteEffects.None,SpriteEffects.FlipHorizontally,SpriteEffects.FlipVertically*/
                0,

                //第九个参数，绘制深度，我也不知道干嘛的，填0就行了
                0
                );
            return false;
        }
    }
}
