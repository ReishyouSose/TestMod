using System.Collections.Generic;
using System.IO;
using static TestMod.NPCs.经典AI;

namespace TestMod.Projectiles
{
    public abstract class BaseLogicProj : ModProjectile
    {
        // 其实我想写中文类名
        public override string Texture => "Terraria/Images/Extra_98"; // 反正逻辑不画，随便给写个贴图，这张是个很常用的万能贴图
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32; // 正方碰撞箱
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }
        public Player Player => Main.player[Projectile.owner]; // 偷懒用的
        public int itemDamage; // 作为实时更新伤害的基数，传入武器基础面板
        public int buffType; // 给玩家添加buff的type
        public int tail; // 用来存尾体节的whoami
        public virtual float Offset => 1f;// 设置体节位置时的偏移率
        public abstract bool ModPlayerMinionBool { get; set; } // 继承了ModPlayer类中对应的召唤物bool属性
        public List<int> proj = new(); // 弹幕list，存入弹幕WhoAmI，不计尾体节
        public List<(Vector2 pos, float rot)> data = new(); // 数据list，计入尾体节，故count是proj.Count + 1
        public void BaseAI()
        {
            // 玩家死了干掉弹幕
            if (Player.dead)
            {
                ModPlayerMinionBool = false;
            }
            // 有buff保持弹幕,buff中会把这个bool设为true，后面会写到
            if (ModPlayerMinionBool)
            {
                Projectile.timeLeft = 2;
            }
            Player.AddBuff(buffType, 2);// 给玩家上对应buff
        }
        public virtual void ActionAI() { StarDustDragonAI(Projectile, Player); }

        // 参数是身体体节数量，返回伤害倍率。这里是每节增加的伤害衰减10%，最多衰减5次，即从召唤第6节开始每多一节增伤约60%
        public virtual double ModifyDamageMult(int count)
        {
            return count * Math.Pow(0.9f, count - 1 > 5 ? 5 : count - 1);
        }

        public static (Vector2 pos, float rot) CalculatePosAndRot((Vector2 pos, float rot) tar, (Vector2 pos, float rot) me, float dis)
        {
            Vector2 chaseDir = tar.pos - me.pos;// 当前的位置到目标的位置
            if (chaseDir == Vector2.Zero)// 如果这两坐标怼一起了就分开，防止位置变成Nan
            {
                chaseDir = Vector2.One;
            }
            chaseDir = Vector2.Normalize(chaseDir);// 向量单位化
            float chaserot = tar.rot - me.rot;// 目标角度的和当前的角度差
            if (chaserot != 0)// 角度即弹幕的运动（视觉上）方向，当方向不同，就每帧修正这个方向，修正值是差值的10%
            {
                chaseDir = chaseDir.RotatedBy(MathHelper.WrapAngle(chaserot) * 0.1f);
            }
            Vector2 center = tar.pos - chaseDir * dis;// 让目标位置减去修正后算上距离的追击单位向量，即是下一帧应在的位置
            return (center, chaseDir.ToRotation());// 返回应在位置和修正后角度
        }

        // 用于设置体节数据
        public static void SetSection(int whoami, (Vector2 pos, float rot) data, double damage = 0)
        {
            Projectile p = Main.projectile[whoami];
            p.Center = data.pos;
            p.rotation = data.rot;
            p.timeLeft = 2;// 保证弹幕存活，且在逻辑弹幕被右键buff取消后马上死亡
            p.originalDamage = p.damage = (int)damage;// 给体节设置伤害，用于如果你们想让体节射出弹幕进行攻击之类的时候
        }
        public override void AI()
        {
            // 基础AI
            BaseAI();
            // 运动AI
            ActionAI();
            // 维护弹幕列表和数据列表，在其内有弹幕死亡（召唤栏突然减少）时剔除元素
            for (int i = 0; i < proj.Count; i++)
            {
                if (!Main.projectile[proj[i]].active)
                {
                    proj.RemoveAt(i);
                    data.RemoveAt(i + 1);
                    i--;
                    Projectile.ai[0] = 1;// 在有变动时把ai[0]设为1执行同步
                }
            }
            // 设置伤害
            Projectile.originalDamage = (int)(ModifyDamageMult(proj.Count - 1) * Player.GetDamage(DamageClass.Summon).ApplyTo(itemDamage));
            // 防止你自己写运动逻辑并忘了写这个
            Projectile.rotation = Projectile.velocity.ToRotation();
            // 更新数据[0]，是逻辑弹幕的中心与角度
            data[0] = (Projectile.Center, Projectile.rotation);
            // 设置头体节的数据（位置、角度、伤害）
            SetSection(proj[0], data[0], Projectile.originalDamage);
            // 重新计算data中的位置与角度，第一个数据是逻辑弹幕中心与角度，不需重新计算
            // proj未计入尾体节，但data有，所以这里是从1开始且 <= proj.Count
            for (int i = 1; i <= proj.Count; i++)
            {
                // 注意，calculate方法的第三个参数距离，传入弹幕的 碰撞箱宽 * 缩放 * 继承后可重写的偏移率
                data[i] = CalculatePosAndRot(data[i - 1], data[i], Projectile.width * Projectile.scale * Offset);
                if (i < proj.Count)// proj中没有尾，是 <
                {
                    // 设置身体节的数据
                    SetSection(proj[i], data[i], Projectile.originalDamage);
                }
            }
            // 设置尾体节的数据, 逻辑弹幕的ai[1]是尾体节的WhoAmI

            SetSection(tail, data[proj.Count], Projectile.originalDamage);
            if (Projectile.ai[0] == 1)
            {
                Projectile.netUpdate = true;
                Projectile.ai[0] = 0;
            }
        }

        // 弄个方便的方法，传入中心坐标和宽度，返回宽高等于传入宽度且中心是传入的坐标的矩形（碰撞箱）
        public static Rectangle RecCenter(Vector2 center, int Size)
        {
            return new Rectangle((int)center.X - Size / 2, (int)center.Y - Size / 2, Size, Size);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool intersects = false;
            for (int i = 0; i <= proj.Count; i++)// 是 <= 哦
            {
                if (targetHitbox.Intersects(RecCenter(data[i].pos, projHitbox.Width)))// 这里的碰撞箱已经应用了scale，所以直接拿他的宽
                {
                    intersects = true;
                    break;
                }
            }
            return intersects;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        // 这个是用来方便设置的方法，因为player.SpawnMinionOnCursor不像NewProjectile方法一样能直接写ai01。另外加上了在这里设置弹幕召唤栏位占用
        public static int SpawnMinion(Player player, IEntitySource source, int type, int damage, float kb, float ai0 = 0, float ai1 = 0, float minionSlots = 0)
        {
            int proj = player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, kb);
            Main.projectile[proj].ai[0] = ai0;
            Main.projectile[proj].ai[1] = ai1;
            Main.projectile[proj].minionSlots = minionSlots;
            return proj;// 返回弹幕索引
        }

        public static void SummonSet<T>(Player player, IEntitySource source, int damage, float kb, int amount, int SummonBuffType, int Logic, int Head, int Body, int Tail) where T : BaseLogicProj
        {
            // 寻找是否有属于玩家的这类逻辑弹幕
            int logic = -1;
            foreach (Projectile p in Main.projectile)
            {
                if (p.type == Logic && p.active && p.owner == player.whoAmI)
                {
                    logic = p.whoAmI;
                    break;
                }
            }
            if (logic == -1)// 没找到就发射逻辑弹幕和头身尾
            {
                // 发射逻辑弹幕
                int L = SpawnMinion(player, source, Logic, damage, kb, 1, 0);
                if (Main.projectile[L].ModProjectile is T Proj)// 使用模式匹配强制转换，以获取逻辑弹幕类里的字段属性
                {
                    Proj.itemDamage = damage;// 传入基础伤害
                    Proj.buffType = SummonBuffType;// 传入bufftype
                    var proj = Proj.proj;// 获取逻辑弹幕类里的两个list
                    var data = Proj.data;
                    // 首先，向数据list添加元素。数量是要召唤的体节量+2（一个头一个尾）
                    // 至于这里加了个偏移是为了让之前那个目标位置-当前位置不为零
                    for (int i = 0; i < amount + 2; i++) data.Add((Main.MouseWorld + Vector2.One * i, 0));
                    // 生成头体节
                    int p = SpawnMinion(player, source, Head, damage, kb);
                    proj.Add(p);// 把头体节WhoAmI Add到proj列表
                    for (int i = 0; i < amount; i++)
                    {
                        // 按照传入的单次生成身体数召唤身体，每次使用物品占用一个召唤栏
                        // 所以这里把单个体节占用的召唤栏设为 1f / amount 。放心，弹幕的召唤栏位属性是float（比如双子眼召唤物）
                        int body = SpawnMinion(player, source, Body, damage, kb, i, 1, 1f / amount);
                        proj.Add(body);// 把身体节WhoAmI Add到proj列表
                    }
                    // 尾体节不需加入proj列表，但要把逻辑弹幕的tail设为尾体节的WhoAmI用于之后设置它的数据
                    Proj.tail = SpawnMinion(player, source, Tail, damage, kb);
                }
            }
            else// 有逻辑弹幕（那也就是有头有尾了）
            {
                if (Main.projectile[logic].ModProjectile is T Proj)// 强转
                {
                    for (int i = 0; i < amount; i++)
                    {
                        // 先Add数据列表
                        Proj.data.Add((Main.MouseWorld + Vector2.One * i, 0));
                        // 再Add弹幕列表
                        Proj.proj.Add(SpawnMinion(player, source, Body, 0, 0, i, 1, 1f / amount));
                    }
                    Main.projectile[logic].ai[0] = 1;
                }
            }
        }
        public static void DrawSet(SpriteBatch spb, Projectile proj, int Body, int amount, Color color, float rot)
        {
            Texture2D tex = TextureAssets.Projectile[proj.type].Value;
            bool body = false;
            Rectangle rec = new();
            if (proj.type == Body)// 给身体体节做特判
            {
                body = true;
                // 根据身体的ai[0]来裁剪贴图，这里是纵向裁剪，有需要你们可以写个横向重载
                rec = new Rectangle(0, (int)proj.ai[0] * tex.Height / amount, tex.Width, tex.Height / amount);
            }
            spb.Draw(tex, proj.Center - Main.screenPosition, !body ? null : rec, color,
                proj.rotation + rot, (!body ? tex.Size() : rec.Size()) / 2f, proj.scale,
                Math.Abs(proj.rotation + rot) < Math.PI / 2f ? 0 : SpriteEffects.FlipVertically, 0);
            // 瞧见这里的rotation没，所以在SetPosAndDmg函数里又写了一次设置rotation，这可是很重要的
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            // 发送弹幕list
            writer.Write(proj.Count);
            for (int i = 0; i < proj.Count; i++)
            {
                writer.Write(proj[i]);
            }
            // 发送数据list
            writer.Write(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteVector2(data[i].pos);
                writer.Write(data[i].rot);
            }
            // 发送tail
            writer.Write(tail);
        }

        // 按顺序接收数据，填充新list，将新list赋值回去
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
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                newdata.Add((reader.ReadVector2(), reader.ReadSingle()));
            }
            data = newdata;
            tail = reader.ReadInt32();
        }
    }
}
