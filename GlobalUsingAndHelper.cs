global using Microsoft.Xna.Framework;
global using System;
global using Terraria;
global using Terraria.DataStructures;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using TestMod.Projectiles;
global using static TestMod.Helper;
global using static TestMod.Projectiles.MultipleSectionsMinion;
global using ReLogic.Content;

namespace TestMod
{
    public class Helper
    {
        public static int Chinese()
        {
            return (int)GameCulture.CultureName.Chinese;
        }
        public static NPC ChooseTarget(Projectile proj, float distance = 0)
        {
            NPC target = null;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && Vector2.Distance(proj.Center, npc.Center) < distance)
                {
                    target = npc;
                }
            }
            return target;
        }
        public static void ActionLogic(Projectile Projectile, Player Player, float MaxDisToPlayer = 2000, float SearchDis = 700, float MaxSpeed_NoTarget = 15f, float MaxSpeed_HasTarget = 30f)
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
        public static void SummonSet<T>(Player player, IEntitySource source, int damage, float kb,
            int amount, int SummonBuffType, int Itemdamage, int Logic, int Head, int Body, int Tail) where T : 蠕虫弹幕基类
        {
            (Vector2, float) unit = (Main.MouseWorld, 0);
            int logic = -1;
            foreach (Projectile h in Main.projectile)
            {
                if (h.type == Logic && h.active && h.owner == player.whoAmI)
                {
                    logic = h.whoAmI;
                    break;
                }
                if (logic != 1) break;
            }
            if (logic == -1)
            {
                Projectile L = SpawnMinion(player, source, Logic, damage, kb,SummonBuffType);
                if (L.ModProjectile is T Proj)
                {
                    Proj.itemDamage = Itemdamage;
                    var list = Proj.list;
                    var data = Proj.data;
                    for (int i = 0; i < amount+2; i++) data.Add(unit);

                    list.Add(SpawnMinion(player, source, Head, 0, 0).whoAmI);
                    for (int i = 0; i < amount; i++)
                    {
                        list.Add(SpawnMinion(player, source, Body, 0, 0, i, 0, 1f / amount).whoAmI);
                    }
                    L.ai[1] = SpawnMinion(player, source, Tail, 0, 0).whoAmI;
                }
            }
            else
            {
                if (Main.projectile[logic].ModProjectile is T Proj)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Proj.data.Add((Main.MouseWorld + Vector2.One * i, 0));
                        Proj.list.Add(SpawnMinion(player, source, Body, damage, kb, i, 0, 1f / amount).whoAmI);
                    }
                }
            }
        }
        public static (Vector2 pos, float rot) CalculatePosAndRot((Vector2 pos, float rot) tar, (Vector2 pos, float rot) me, float dis)
        {
            Vector2 chasepos = tar.pos - me.pos;
            if (chasepos == Vector2.Zero) return (chasepos + Vector2.One, 0);
            float chaserot = tar.rot - me.rot;
            if (chaserot != 0)
            {
                chasepos = Vector2.Normalize(chasepos).RotatedBy(MathHelper.WrapAngle(chaserot) * 0.1f);

            }
            Vector2 center = tar.pos - chasepos * dis;
            return (center, chasepos.ToRotation());
        }
        public static void SetProjPosAndRot(int whoami, (Vector2 pos, float rot) data)
        {
            Projectile p = Main.projectile[whoami];
            p.Center = data.pos;
            p.rotation = data.rot;
        }
        public static Rectangle RecCenter(Vector2 center, int Size)
        {
            return new Rectangle((int)center.X - Size / 2, (int)center.Y - Size / 2, Size, Size);
        }
    }
}
