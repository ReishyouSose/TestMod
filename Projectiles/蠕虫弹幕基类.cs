using System.Collections.Generic;

namespace TestMod.Projectiles
{
    public abstract class 蠕虫弹幕基类 : ModProjectile
    {
        public Player Player => Main.player[Projectile.owner];
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
        public virtual int Amount => 2;
        public virtual float Offset => 1f;
        public virtual bool ModPlayerMinionBool { get; set; }

        public int itemDamage = 0;
        public List<int> list = new();
        public List<(Vector2 pos, float rot)> data = new();
        public virtual double ModifyDamageMult(int count)
        {
            return count * Math.Pow(0.9f, count - 1 > 5 ? 5 : count - 1);
        }
        public void BaseAI()
        {
            //两秒同步一次数据
            if (Main.GameUpdateCount % 120 == 0)
            {
                Projectile.netUpdate = true;
            }
            // 玩家死了干掉弹幕
            if (Player.dead)
            {
                ModPlayerMinionBool = false;
            }
            //有buff保持弹幕
            if (ModPlayerMinionBool)
            {
                Projectile.timeLeft = 2;
            }
        }
        public override void AI()
        {
            BaseAI();
            Player.AddBuff((int)Projectile.ai[0], 2);
            ActionLogic(Projectile, Player);
            Projectile.originalDamage = (int)(ModifyDamageMult(list.Count - 1) *
                Player.GetDamage(DamageClass.Summon).ApplyTo(itemDamage));
            for (int i = 0; i < list.Count; i++)
            {
                if (!Main.projectile[list[i]].active)
                {
                    list.RemoveAt(i--);
                }
            }
            data[0] = (Projectile.Center, Projectile.rotation);
            SetProjPosAndRot(list[0], data[0]);
            for (int i = 1; i <= list.Count; i++)
            {
                data[i] = CalculatePosAndRot(data[i - 1], data[i], Projectile.width * Projectile.scale * Offset);
                if (i < list.Count)
                {
                    SetProjPosAndRot(list[i], data[i]);
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            /*for (int i = 0; i < list.Count; i++)
            {
                Main.projectile[list[i]].Kill();
            }*/
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool intersects = false;
            for (int i = 0; i <= list.Count; i++)
            {
                if (targetHitbox.Intersects(RecCenter(data[i].pos, projHitbox.Width)))
                {
                    intersects = true;
                    break;
                }
            }
            return intersects;
        }/*
        public void DrawSet(SpriteBatch spb, int SummonNum, Color lightColor, float rot)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            bool body = false;
            Rectangle rec = new();
            if (Type == Body)
            {
                body = true;
                rec = new Rectangle(0, (int)Projectile.ai[1] * tex.Height / SummonNum, tex.Width, tex.Height / SummonNum);
            }
            spb.Draw(tex, Projectile.Center - Main.screenPosition, !body ? null : rec, lightColor,
                Projectile.rotation + rot, (!body ? tex.Size() : rec.Size()) / 2f, Projectile.scale,
                Math.Abs(Projectile.rotation + rot) < Math.PI / 2f ? 0 : SpriteEffects.FlipVertically, 0);
            // 瞧见这里的rotation没，所以在SetPosAndDmg函数里又写了一次设置rotation，这可是很重要的
        }*/
        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Utils.DrawLine(Main.spriteBatch, data[i].pos, data[i + 1].pos, Color.White);
            }
            return false;
        }
    }
}
