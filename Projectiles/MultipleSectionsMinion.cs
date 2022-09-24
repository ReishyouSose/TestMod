using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace TestMod.Projectiles
{
    public abstract class MultipleSectionsMinion : ModProjectile
    {
        public static Projectile SpawnMinion(Player player, IEntitySource source, int type, int damage, float kb, float ai0 = 0, float ai1 = 0, float minionSlots = 0)
        {
            int p = player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, kb);
            Projectile proj = Main.projectile[p];
            proj.ai[0] = ai0;
            proj.ai[1] = ai1;
            proj.minionSlots = minionSlots;
            proj.netUpdate = true;// 召唤时同步数据
            return proj;
        }
        //用于召唤的设置
        public static void SummonSet(Player player, IEntitySource source, int damage, float knockback, int SummonNum, int SummonBuffType, int Logic, int Head, int Body, int Tail)
        {
            int head = -1;
            int tail = -1;
            foreach (Projectile proj in Main.projectile)// 寻找有无逻辑弹幕和尾弹幕
            {
                if (proj.active && proj.owner == player.whoAmI)
                {
                    if (head == -1 && proj.type == Logic)
                    {
                        head = proj.whoAmI;
                    }
                    if (tail == -1 && proj.type == Tail)
                    {
                        tail = proj.whoAmI;
                    }
                    if (head != -1 && tail != -1)
                    {
                        break;
                    }
                }
            }
            // 没有逻辑弹幕和尾弹幕则发射全部
            if (head == -1 && tail == -1)
            {
                int count = 0;// 用于传入体节身份
                // 逻辑弹幕的ai[1]是对应的召唤物buff
                Projectile logic = SpawnMinion(player, source, Logic, damage, knockback, 0, SummonBuffType);
                logic.hide = true;// 逻辑弹幕隐形
                logic.friendly = false;// 逻辑弹幕不造成伤害
                SpawnMinion(player, source, Head, damage, knockback, count);// 生成头体节，头体节的ai[0]是身份，此时是0
                for (int i = 0; i < SummonNum; i++)// 根据传入的单次召唤数量来召唤身体
                {
                    count++;// 身份+1
                    Projectile p = SpawnMinion(player, source, Body, damage, knockback, count, i);//身体节ai[0]是身份，ai[1]是身体的类型，即上边用于确定贴图
                    // 身召唤武器每次使用都是消耗一个召唤栏，头尾、逻辑体节都不消耗召唤栏，把身体占用的召唤栏位设为 1f / 召唤量 以匹配
                    p.minionSlots = 1f / SummonNum;
                }
                SpawnMinion(player, source, Tail, damage, knockback, count + 1);//召唤尾体节，ai[0]是身份，同时也是体节量
                logic.ai[0] = count + 1;// 逻辑弹幕ai[0]是体节量
            }
            // 如果有，则只发射身体体节
            else if (head != -1 && tail != -1)
            {
                int count = 1;// 遍历寻找已有体节数量，算上尾巴，所以从1开始
                foreach (Projectile p in Main.projectile)
                {
                    if (p.owner == player.whoAmI && p.active)
                    {
                        if (p.type == Body) count++;
                    }
                }
                // 以星尘龙为例，前面发射后，有2身，那么遍历结束后这里的count就是3
                for (int i = 0; i < SummonNum; i++)
                {
                    Projectile p = SpawnMinion(player, source, Body, damage, knockback, count, i);
                    p.minionSlots = 1f / SummonNum;
                    count++;
                }
                // 再次发射了两个身体体节，此时count应为5
                // 设置体节量，同步数据
                Main.projectile[head].ai[0] = count;
                Main.projectile[head].netUpdate = true;
                Main.projectile[tail].ai[0] = count;
                Main.projectile[tail].netUpdate = true;
            }
        }
        // 之后在继承了的物品的重写Shoot函数中调用这个方法即可

        //一个一个生草机翻的 Multiple多 Sections节 Minion召唤物 aaaa
        public override void SetDefaults()
        {
            // 此处没有设置召唤物弹幕所占用的召唤栏数量，因为之后会在发射中设置
            // 将弹幕穿透设为-1（无限穿透）使弹幕不会在打到敌人时就死亡，并且使用独立无敌帧来防止严重骗伤
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;// 在AI中会有召唤时弹幕透明度逐渐降低，所以这里先设为完全透明
            Projectile.netImportant = true;
        }
        public virtual int Logic => -1;// 逻辑弹幕Type
        public virtual int Head => -1;// 头体节的弹幕Type
        public virtual int Body => -1;// 身体节的弹幕Type
        public virtual int Tail => -1;// 尾体节的弹幕Type
        public virtual float Offset => 1f;// 用于设定位置的偏移率
        public virtual bool ModPlayerMinionBool { get; set; }// 对应的ModPlayer中的召唤物bool
        public virtual int DustType => -1;// 弹幕的粒子Type，不写的话就没有
        public virtual Color AddLightColor => Color.White;// 从弹幕中心发光的颜色和强度，比值是颜色，大小是强度，基准是1f或255int

        // return true使用默认的行动与定位逻辑，return false自己写
        public virtual bool LogicAI() { return true; }

        public virtual void HeadAI() { }
        public virtual void BodyAI() { }
        public virtual void TailAI() { }

        //上面是需要继承重写的属性或方法（还有一个DefaultAI()，不在这里，下面会说到）
        //下面是偷懒用的（

        public Player Player => Main.player[Projectile.owner];// 弹幕主人
        public int Size => Projectile.width;// 之后用于设定位置的距离

        // 这个方法用来判断弹幕是否是某个体节
        public bool Part(Projectile proj, int partID)
        {
            bool part = false;
            if (partID == 0)
            {
                part = proj.type == Head;
            }
            else if (partID == 1)
            {
                part = proj.type == Body;
            }
            else if (partID == 2)
            {
                part = proj.type == Tail;
            }
            return part && proj.active && proj.owner == Player.whoAmI;
        }
        public override void AI()
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

            if (Type == Logic)// 逻辑弹幕
            {
                Player.AddBuff((int)Projectile.ai[1], 2);// 每帧给玩家对应的召唤物buff
                int count = 0;
                int tail = -1;
                foreach (Projectile p in Main.projectile)// 遍历弹幕，重设体节数，防止召唤栏减少导致体节错位
                {
                    if (Part(p, 1))
                    {
                        count++;
                    }
                    if (Part(p, 2))
                    {
                        count++;
                        tail = p.whoAmI;
                    }
                }
                Projectile.ai[0] = count;
                Main.projectile[tail].ai[0] = count;
                if (LogicAI())// 类似于Item的Shoot函数，return true执行默认，但不会执行LogicAI中写的代码，return false以执行自己写的运动与定位逻辑，当然你可以false然后在里边调用下面这两函数。
                {
                    ActionLogic();
                    SetPosAndDmg();
                }
                else LogicAI();
            }
            else// 非逻辑弹幕
            {
                DefaultAI(DustType, AddLightColor);// 全局逻辑
                if (Type == Head) HeadAI();
                if (Type == Body) BodyAI();
                if (Type == Tail) TailAI();
            }
            //限制弹幕位置在世界范围内
            Projectile.position.X = MathHelper.Clamp(Projectile.position.X, 160f, (Main.maxTilesX * 16 - 160));
            Projectile.position.Y = MathHelper.Clamp(Projectile.position.Y, 160f, (Main.maxTilesY * 16 - 160));
        }
        // 四个参数，离玩家的最大距离，索敌距离（这个值不要大于1000），无攻击目标的最大速度，有攻击目标的最大速度
        public void ActionLogic(float MaxDisToPlayer = 2000, float SearchDis = 700, float MaxSpeed_NoTarget = 15f, float MaxSpeed_HasTarget = 30f)
        {
            // 弹幕超出玩家2000f自动回归，并同步数据
            if (Vector2.Distance(Player.Center, Projectile.Center) > MaxDisToPlayer)
            {
                Projectile.Center = Player.Center;
                Projectile.netUpdate = true;
            }
            int TarWho = -1;// 目标whoAmI
            NPC target = Projectile.OwnerMinionAttackTargetNPC;// 召唤物自动索敌
            if (target != null && target.CanBeChasedBy())
            {
                // 虽然但是，为什么要用 两倍 索敌距离
                if (Projectile.Distance(target.Center) < SearchDis * 2f)
                {
                    TarWho = target.whoAmI;
                }
            }
            // 如果自动索敌没有找到目标，就搜索离玩家1000f内且在弹幕索敌距离内的目标
            if (TarWho < 0)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.CanBeChasedBy() && Player.Distance(npc.Center) < 1000f)
                    {
                        if (Projectile.Distance(npc.Center) < SearchDis)
                        {
                            TarWho = npc.whoAmI;
                        }
                    }
                }
            }
            if (TarWho != -1)// 有攻击目标
            {
                NPC npc = Main.npc[TarWho];
                Vector2 tarVel = npc.Center - Projectile.Center;
                float speed = 0.4f;// 追击速度系数
                                   // 越近越快
                if (tarVel.Length() < 600f)
                {
                    speed = 0.6f;
                }
                if (tarVel.Length() < 300f)
                {
                    speed = 0.8f;
                }
                // 距离比碰撞箱的0.75倍大？
                if (tarVel.Length() > npc.Size.Length() * 0.75f)
                {
                    Projectile.velocity += Vector2.Normalize(tarVel) * speed * 1.5f;
                    //如果追踪方向和速度方向夹角过大，减速
                    if (Vector2.Dot(Projectile.velocity, tarVel) < 0.25f)
                    {
                        Projectile.velocity *= 0.8f;
                    }
                }
                // 限制最大速度
                if (Projectile.velocity.Length() > MaxSpeed_HasTarget)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed_HasTarget;
                }
            }
            else// 无攻击目标
            {
                float speed = 0.2f;// 游荡速度系数
                float dis = Player.Center.Distance(Projectile.Center);// 弹幕到玩家距离
                                                                      // 越近越慢
                if (dis < 200f)
                {
                    speed = 0.12f;
                }
                if (dis < 140f)
                {
                    speed = 0.06f;
                }
                if (dis > 100f)
                {
                    // abs绝对值，sign正负
                    if (Math.Abs(Player.Center.X - Projectile.Center.X) > 20f)
                    {
                        Projectile.velocity.X += speed * Math.Sign(Player.Center.X - Projectile.Center.X);
                    }
                    if (Math.Abs(Player.Center.Y - Projectile.Center.Y) > 10f)
                    {
                        Projectile.velocity.Y += speed * Math.Sign(Player.Center.Y - Projectile.Center.Y);
                    }
                }
                else if (Projectile.velocity.Length() > 2f)
                {
                    Projectile.velocity *= 0.96f;
                }
                if (Math.Abs(Projectile.velocity.Y) < 1f)
                {
                    Projectile.velocity.Y -= 0.1f;
                }
                //无目标最大速度15f,有目标30f
                if (Projectile.velocity.Length() > MaxSpeed_NoTarget)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed_NoTarget;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        // 参数是身体体节数量，返回伤害倍率。这里是每节增加的伤害衰减10%，最多衰减5次，即从召唤第6节开始每多一节全体增伤约60%
        public virtual double ModifyProjDamage(int projCount)
        {
            return projCount * Math.Pow(0.9f, projCount > 5 ? 5 : projCount - 1);
        }

        // 用于记录与设置位置的数组
        public (Vector2 pos, float rot)[] chaseData = new (Vector2 pos, float rot)[1000];

        //设置位置与弹幕伤害的方法，如果自己重写要记得调用上边那个ModifyProjectileDamage方法
        public virtual void SetPosAndDmg()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();// 防止你重写了运动AI并忘了写这个（
            int count = (int)Projectile.ai[0];
            // 应用伤害加成
            double Damage = Player.GetDamage(DamageClass.SummonMeleeSpeed).ApplyTo(Projectile.originalDamage) * ModifyProjDamage(count - 1);
            chaseData[0] = (Projectile.Center, Projectile.rotation);
            for (int i = 1; i <= count; i++)
            {
                Vector2 chaseCenter = chaseData[i - 1].pos;// 目标位置
                float chaseRot = chaseData[i - 1].rot;// 目标角度
                Vector2 myCenter = chaseData[i].pos;// 当前位置
                float myRot = chaseData[i].rot;// 当前角度
                Vector2 chaseDir = Vector2.Normalize(chaseCenter - myCenter);// 目标方向
                if (chaseRot != myRot)
                {
                    // 角度分离时，每帧修正10%
                    chaseDir = chaseDir.RotatedBy(MathHelper.WrapAngle(chaseRot - myRot) * 0.1f);
                }
                // 设定位置，是目标位置-修正后的目标方向的单位向量*距离与偏移率。offset即用于修正这个偏移距离
                Vector2 Center = chaseCenter - chaseDir * Size * Projectile.scale * Offset;
                // 将设定好的位置填充回数组
                chaseData[i] = (Center, chaseDir.ToRotation());
                // 遍历所属体节，设置位置，角度，伤害
                foreach (Projectile p in Main.projectile)
                {
                    if ((p.type == Head || p.type == Body || p.type == Tail) && p.owner == Player.whoAmI && p.active)
                    {
                        if (p.ai[0] == i || p.ai[0] == 0)
                        {
                            int j = p.ai[0] == 0 ? 0 : i;
                            p.Center = chaseData[j].pos;
                            p.rotation = chaseData[j].rot;
                            p.originalDamage = (int)Damage;
                        }
                    }
                }
            }
        }
        public virtual void DefaultAI(int dustType, Color color)
        {
            Lighting.AddLight(Projectile.Center, color.ToVector3());
            if (DustType != -1)
            {
                //召唤时粒子
                if (Projectile.alpha > 0)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Dust dust = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y),
                            Projectile.width, Projectile.height, dustType, Scale: 2f);
                        dust.noGravity = true;
                        dust.noLight = true;
                    }
                    Projectile.alpha -= 51;
                    if (Projectile.alpha < 0)
                    {
                        Projectile.alpha = 0;
                    }
                }
                //粒子
                if (Main.rand.NextBool(30))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                        dustType, Scale: 2f);
                    dust.noGravity = true;
                    dust.fadeIn = 2f;
                }
                // 弹幕kill时
                if (Projectile.timeLeft == 1)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Dust dust = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y),
                            Projectile.width, Projectile.height, dustType, Scale: 2f);
                        dust.noGravity = true;
                    }
                }
            }
        }
        // 用于之后设置绘制的方法
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
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            //只让逻辑弹幕体节去执行这个额外的同步就行
            int count = 0;
            if (Type == Logic) count = (int)Projectile.ai[0];
            for (int i = 0; i <= count; i++)
            {
                writer.WriteVector2(chaseData[i].pos);
                writer.Write(chaseData[i].rot);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            int count = 0;
            if (Type == Logic) count = (int)Projectile.ai[0];
            for (int i = 0; i <= count; i++)
            {
                Vector2 pos = reader.ReadVector2();
                float rot = reader.ReadSingle();
                chaseData[i] = (pos, rot);
            }
        }
    }
}
