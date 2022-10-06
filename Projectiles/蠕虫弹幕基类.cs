using System.Collections.Generic;
using System.IO;
using static TestMod.NPCs.经典AI;

namespace TestMod.Projectiles
{
    public abstract class 蠕虫弹幕基类 : ModProjectile
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
            Projectile.alpha = 255;
        }
        public Player Player => Main.player[Projectile.owner];
        public int itemDamage;
        public int buffType;
        public int tail;
        public virtual float Offset => 1f;
        public abstract bool ModPlayerMinionBool { get; set; }
        public List<int> proj = new();// 弹幕list
        public List<(Vector2 pos, float rot)> data = new();// 数据list
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
            Player.AddBuff(buffType, 2);
        }
        public virtual void ActionAI() { StarDustDragonAI(Projectile, Player); }
        public override void AI()
        {
            // 基础AI
            BaseAI();
            // 行动AI
            ActionAI();
            // 维护弹幕列表和数据列表，在其内有弹幕死亡（召唤栏突然减少）时剔除元素
            for (int i = 0; i < proj.Count; i++)
            {
                if (!Main.projectile[proj[i]].active)
                {
                    proj.RemoveAt(i);
                    data.RemoveAt(i + 1);
                    i--;
                    Projectile.ai[0] = 1;
                }
            }
            if (Projectile.ai[0] == 1)
            {
                Projectile.netUpdate = true;
                Projectile.ai[0] = 0;
            }
            // 设置伤害
            Projectile.originalDamage = (int)(ModifyDamageMult(proj.Count - 1) *
                Player.GetDamage(DamageClass.Summon).ApplyTo(itemDamage));
            // 更新数据[0]，是逻辑弹幕的中心与角度
            data[0] = (Projectile.Center, Projectile.rotation);
            int dir = Projectile.direction;
            Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // 设置头体节的数据
            SetProjSection(proj[0], data[0], Projectile.spriteDirection, Projectile.originalDamage);
            // 重新计算data中的位置与角度，proj未计入尾体节，但data有，所以这里是<=
            for (int i = 1; i <= proj.Count; i++)
            {
                // 设置身体节的数据
                data[i] = CalculatePosAndRot(data[i - 1], data[i], Projectile.width * Projectile.scale * Offset);
                if (i < proj.Count)// proj中没有尾，是<
                {
                    // 设置身体节的数据
                    SetProjSection(proj[i], data[i], Projectile.spriteDirection, Projectile.originalDamage);
                }
            }
            // 设置尾体节的数据
            SetProjSection(tail, data[proj.Count], Projectile.spriteDirection, Projectile.originalDamage);
            if (dir != Projectile.direction)
            {
                for (int i = 0; i < proj.Count; i++)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, -1, Player.whoAmI, null, proj[i]);
                }
                NetMessage.SendData(MessageID.SyncProjectile, -1, Player.whoAmI, null, tail);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool intersects = false;
            for (int i = 0; i <= proj.Count; i++)
            {
                if (targetHitbox.Intersects(RecCenter(data[i].pos, projHitbox.Width)))
                {
                    intersects = true;
                    break;
                }
            }
            return intersects;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(proj.Count);
            for (int i = 0; i < proj.Count; i++)
            {
                writer.Write(proj[i]);
            }
            writer.Write(tail);
            writer.Write(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteVector2(data[i].pos);
                writer.Write(data[i].rot);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            List<int> newproj = new();
            List<(Vector2 pos, float rot)> newdata = new();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                newproj.Add(reader.ReadInt32());
            }
            proj = newproj;
            tail = reader.ReadInt32();
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                newdata.Add((reader.ReadVector2(), reader.ReadSingle()));
            }
            data = newdata;
        }
        /*public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 0; i < proj.Count; i++)
            {
                Utils.DrawLine(Main.spriteBatch, data[i].pos, data[i + 1].pos, Color.White);
            }
            return false;
        }*/
    }
}
