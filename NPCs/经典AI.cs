using Terraria.Audio;
using Terraria.ID;

namespace TestMod.NPCs
{
    public class 经典AI
    {
        public static void StarDustDragonAI(Projectile Projectile, Player Player, float MaxDisToPlayer = 2000, float SearchDis = 700, float MaxSpeed_NoTarget = 15f, float MaxSpeed_HasTarget = 30f)
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
                float dis = tarVel.Length();
                // 越近越快
                if (dis < 600f)
                {
                    speed = 0.6f;
                }
                if (dis < 300f)
                {
                    speed = 0.8f;
                }
                // 到npc的距离比npc的碰撞箱大小的0.75倍大，也就是说离NPC还比较远
                if (dis > npc.Size.Length() * 0.75f || Projectile.velocity == Vector2.Zero)
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

        public static void 千足蜈蚣AI(NPC NPC, int Head, int Body, int Tail)
        {
            bool flag = false;
            float num56 = 0.2f;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || (flag && (double)Main.player[NPC.target].position.Y < Main.worldSurface * 16.0))
            {
                NPC.TargetClosest();
            }
            if (Main.player[NPC.target].dead || (flag && (double)Main.player[NPC.target].position.Y < Main.worldSurface * 16.0))
            {
                NPC.EncourageDespawn(300);
                if (flag)
                {
                    NPC.velocity.Y += num56;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.ai[0] == 0f)
                {
                    NPC.ai[3] = NPC.whoAmI;
                    NPC.realLife = NPC.whoAmI;
                    int WhoAmI = NPC.whoAmI;
                    int num22 = 30;
                    for (int i = 0; i < num22; i++)
                    {
                        int type = 413;
                        if (i == num22 - 1)
                        {
                            type = 414;
                        }
                        int section = NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.position.X + (NPC.width / 2)),
                            (int)(NPC.position.Y + NPC.height), type, NPC.whoAmI);
                        Main.npc[section].ai[3] = NPC.whoAmI;
                        Main.npc[section].realLife = NPC.whoAmI;
                        Main.npc[section].ai[1] = WhoAmI;
                        Main.npc[section].CopyInteractions(NPC);
                        Main.npc[WhoAmI].ai[0] = section;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, section);
                        WhoAmI = section;
                    }
                }
            }
            if (NPC.type == 414 || NPC.type == 412)
            {
                if (!Main.npc[(int)NPC.ai[1]].active || Main.npc[(int)NPC.ai[1]].aiStyle != NPC.aiStyle)
                {
                    NPC.life = 0;
                    NPC.HitEffect();
                    NPC.active = false;
                    NetMessage.SendData(28, -1, -1, null, NPC.whoAmI, -1f);
                }
                if (NPC.type == 412)
                {
                    if (!NPC.active && Main.netMode == 2)
                    {
                        NetMessage.SendData(28, -1, -1, null, NPC.whoAmI, -1f);
                    }
                }
            }// 看不懂
            if (NPC.type == 414)
            {
                if (NPC.justHit)
                {
                    NPC.localAI[3] = 3f;
                }
                if (NPC.localAI[2] > 0f)
                {
                    NPC.localAI[2] -= 16f;
                    if (NPC.localAI[2] == 0f)
                    {
                        NPC.localAI[2] = -128f;
                    }
                }
                else if (NPC.localAI[2] < 0f)
                {
                    NPC.localAI[2] += 16f;
                }
                else if (NPC.localAI[3] > 0f)
                {
                    NPC.localAI[2] = 128f;
                    NPC.localAI[3] -= 1f;
                }
            }
            if (NPC.type == 412)
            {
                NPC.position += NPC.netOffset;
                Vector2 value = NPC.Center + (NPC.rotation - (float)Math.PI / 2f).ToRotationVector2() * 8f;
                Vector2 value2 = NPC.rotation.ToRotationVector2() * 16f;
                Dust obj = Main.dust[Dust.NewDust(value + value2, 0, 0, 6, NPC.velocity.X, NPC.velocity.Y, 100, Color.Transparent, 1f + Main.rand.NextFloat() * 3f)];
                obj.noGravity = true;
                obj.noLight = true;
                obj.position -= new Vector2(4f);
                obj.fadeIn = 1f;
                obj.velocity = Vector2.Zero;
                Dust obj2 = Main.dust[Dust.NewDust(value - value2, 0, 0, 6, NPC.velocity.X, NPC.velocity.Y, 100, Color.Transparent, 1f + Main.rand.NextFloat() * 3f)];
                obj2.noGravity = true;
                obj2.noLight = true;
                obj2.position -= new Vector2(4f);
                obj2.fadeIn = 1f;
                obj2.velocity = Vector2.Zero;
                NPC.position -= NPC.netOffset;
            }// 这里面在生成粒子
            float num39 = 8f;
            float num40 = 0.07f;
            Vector2 vector2 = new(NPC.position.X + NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
            float num42 = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2);
            float num43 = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2);

            bool flag2 = false;
            if (NPC.type == 412)
            {
                num39 = 10f;
                num40 = 0.3f;
                int num44 = -1;
                int num46 = (int)(Main.player[NPC.target].Center.X / 16f);
                int num47 = (int)(Main.player[NPC.target].Center.Y / 16f);
                for (int num48 = num46 - 2; num48 <= num46 + 2; num48++)
                {
                    for (int num49 = num47; num49 <= num47 + 15; num49++)
                    {
                        if (WorldGen.SolidTile2(num48, num49))
                        {
                            num44 = num49;
                            break;
                        }
                    }
                    if (num44 > 0)
                    {
                        break;
                    }
                }
                if (num44 > 0)
                {
                    num44 *= 16;
                    float num50 = num44 - 800;
                    if (Main.player[NPC.target].position.Y > num50)
                    {
                        num43 = num50;
                        if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) < 500f)
                        {
                            num42 = ((!(NPC.velocity.X > 0f)) ? (Main.player[NPC.target].Center.X - 600f) : (Main.player[NPC.target].Center.X + 600f));
                        }
                    }
                }
                else
                {
                    num39 = 14f;
                    num40 = 0.5f;
                }
                float num51 = num39 * 1.3f;
                float num52 = num39 * 0.7f;
                float num53 = Vector2.Normalize(NPC.velocity).Length();
                if (num53 > 0f)
                {
                    if (num53 > num51)
                    {
                        NPC.velocity = Vector2.Normalize(NPC.velocity);
                        NPC.velocity *= num51;
                    }
                    else if (num53 < num52)
                    {
                        NPC.velocity = Vector2.Normalize(NPC.velocity);
                        NPC.velocity *= num52;
                    }
                }
                if (num44 > 0)
                {
                    for (int i = 0; i < 200; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == NPC.type && i != NPC.whoAmI)
                        {
                            Vector2 vector3 = Main.npc[i].Center - NPC.Center;
                            if (vector3.Length() < 400f)
                            {
                                vector3.Normalize();
                                vector3 *= 1000f;
                                num42 -= vector3.X;
                                num43 -= vector3.Y;
                            }
                        }
                    }
                }
                else
                {
                    for (int num55 = 0; num55 < 200; num55++)
                    {
                        if (Main.npc[num55].active && Main.npc[num55].type == NPC.type && num55 != NPC.whoAmI)
                        {
                            Vector2 vector4 = Main.npc[num55].Center - NPC.Center;
                            if (vector4.Length() < 60f)
                            {
                                vector4.Normalize();
                                vector4 *= 200f;
                                num42 -= vector4.X;
                                num43 -= vector4.Y;
                            }
                        }
                    }
                }
            }
            num42 = (int)(num42 / 16f) * 16;
            num43 = (int)(num43 / 16f) * 16;
            vector2.X = (int)(vector2.X / 16f) * 16;
            vector2.Y = (int)(vector2.Y / 16f) * 16;
            num42 -= vector2.X;
            num43 -= vector2.Y;

            float num57 = (float)Math.Sqrt(num42 * num42 + num43 * num43);
            if (NPC.ai[1] > 0f && NPC.ai[1] < (float)Main.npc.Length)
            {
                try
                {
                    vector2 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    num42 = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - vector2.X;
                    num43 = Main.npc[(int)NPC.ai[1]].position.Y + (float)(Main.npc[(int)NPC.ai[1]].height / 2) - vector2.Y;
                }
                catch
                {
                }
                NPC.rotation = (float)Math.Atan2(num43, num42) + 1.57f;
                num57 = (float)Math.Sqrt(num42 * num42 + num43 * num43);
                int num58 = NPC.width;
                if (NPC.type >= 412 && NPC.type <= 414)
                {
                    num58 += 6;
                }
                num57 = (num57 - (float)num58) / num57;
                num42 *= num57;
                num43 *= num57;
                NPC.velocity = Vector2.Zero;
                NPC.position.X += num42;
                NPC.position.Y += num43;
            }
            else
            {
                if (!flag2)
                {
                    NPC.TargetClosest();
                    NPC.velocity.Y += 0.11f;
                    if (NPC.velocity.Y > num39)
                    {
                        NPC.velocity.Y = num39;
                    }
                    if ((double)(Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < (double)num39 * 0.4)
                    {
                        if (NPC.velocity.X < 0f)
                        {
                            NPC.velocity.X -= num40 * 1.1f;
                        }
                        else
                        {
                            NPC.velocity.X += num40 * 1.1f;
                        }
                    }
                    else if (NPC.velocity.Y == num39)
                    {
                        if (NPC.velocity.X < num42)
                        {
                            NPC.velocity.X += num40;
                        }
                        else if (NPC.velocity.X > num42)
                        {
                            NPC.velocity.X -= num40;
                        }
                    }
                    else if (NPC.velocity.Y > 4f)
                    {
                        if (NPC.velocity.X < 0f)
                        {
                            NPC.velocity.X += num40 * 0.9f;
                        }
                        else
                        {
                            NPC.velocity.X -= num40 * 0.9f;
                        }
                    }
                }
                else
                {
                    if (NPC.type != 412 && NPC.soundDelay == 0)
                    {
                        float num59 = num57 / 40f;
                        if (num59 < 10f)
                        {
                            num59 = 10f;
                        }
                        if (num59 > 20f)
                        {
                            num59 = 20f;
                        }
                        NPC.soundDelay = (int)num59;
                        SoundEngine.PlaySound(SoundID.Roar);
                    }
                    num57 = (float)Math.Sqrt(num42 * num42 + num43 * num43);
                    float num60 = Math.Abs(num42);
                    float num61 = Math.Abs(num43);
                    float num62 = num39 / num57;
                    num42 *= num62;
                    num43 *= num62;
                    bool flag4 = false;
                    if (flag4)
                    {
                        bool flag5 = true;
                        for (int num63 = 0; num63 < 255; num63++)
                        {
                            if (Main.player[num63].active && !Main.player[num63].dead && Main.player[num63].ZoneCorrupt)
                            {
                                flag5 = false;
                            }
                        }
                        if (flag5)
                        {
                            if (Main.netMode != 1 && (double)(NPC.position.Y / 16f) > (Main.rockLayer + (double)Main.maxTilesY) / 2.0)
                            {
                                NPC.active = false;
                                int num64 = (int)NPC.ai[0];
                                while (num64 > 0 && num64 < 200 && Main.npc[num64].active && Main.npc[num64].aiStyle == NPC.aiStyle)
                                {
                                    int num65 = (int)Main.npc[num64].ai[0];
                                    Main.npc[num64].active = false;
                                    NPC.life = 0;
                                    if (Main.netMode == 2)
                                    {
                                        NetMessage.SendData(23, -1, -1, null, num64);
                                    }
                                    num64 = num65;
                                }
                                if (Main.netMode == 2)
                                {
                                    NetMessage.SendData(23, -1, -1, null, NPC.whoAmI);
                                }
                            }
                            num42 = 0f;
                            num43 = num39;
                        }
                    }
                    bool flag6 = false;
                    if (!flag6)
                    {
                        if ((NPC.velocity.X > 0f && num42 > 0f) || (NPC.velocity.X < 0f && num42 < 0f) || (NPC.velocity.Y > 0f && num43 > 0f) || (NPC.velocity.Y < 0f && num43 < 0f))
                        {
                            if (NPC.velocity.X < num42)
                            {
                                NPC.velocity.X += num40;
                            }
                            else if (NPC.velocity.X > num42)
                            {
                                NPC.velocity.X -= num40;
                            }
                            if (NPC.velocity.Y < num43)
                            {
                                NPC.velocity.Y += num40;
                            }
                            else if (NPC.velocity.Y > num43)
                            {
                                NPC.velocity.Y -= num40;
                            }
                            if ((double)Math.Abs(num43) < (double)num39 * 0.2 && ((NPC.velocity.X > 0f && num42 < 0f) || (NPC.velocity.X < 0f && num42 > 0f)))
                            {
                                if (NPC.velocity.Y > 0f)
                                {
                                    NPC.velocity.Y += num40 * 2f;
                                }
                                else
                                {
                                    NPC.velocity.Y -= num40 * 2f;
                                }
                            }
                            if ((double)Math.Abs(num42) < (double)num39 * 0.2 && ((NPC.velocity.Y > 0f && num43 < 0f) || (NPC.velocity.Y < 0f && num43 > 0f)))
                            {
                                if (NPC.velocity.X > 0f)
                                {
                                    NPC.velocity.X += num40 * 2f;
                                }
                                else
                                {
                                    NPC.velocity.X -= num40 * 2f;
                                }
                            }
                        }
                        else if (num60 > num61)
                        {
                            if (NPC.velocity.X < num42)
                            {
                                NPC.velocity.X += num40 * 1.1f;
                            }
                            else if (NPC.velocity.X > num42)
                            {
                                NPC.velocity.X -= num40 * 1.1f;
                            }
                            if ((double)(Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < (double)num39 * 0.5)
                            {
                                if (NPC.velocity.Y > 0f)
                                {
                                    NPC.velocity.Y += num40;
                                }
                                else
                                {
                                    NPC.velocity.Y -= num40;
                                }
                            }
                        }
                        else
                        {
                            if (NPC.velocity.Y < num43)
                            {
                                NPC.velocity.Y += num40 * 1.1f;
                            }
                            else if (NPC.velocity.Y > num43)
                            {
                                NPC.velocity.Y -= num40 * 1.1f;
                            }
                            if ((double)(Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < (double)num39 * 0.5)
                            {
                                if (NPC.velocity.X > 0f)
                                {
                                    NPC.velocity.X += num40;
                                }
                                else
                                {
                                    NPC.velocity.X -= num40;
                                }
                            }
                        }
                    }
                }
                NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + 1.57f;
            }
        }
    }
}
