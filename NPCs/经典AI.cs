using Terraria.Audio;
using Terraria.ID;

namespace TestMod.NPCs
{
    public class 经典AI
    {
        public static void 千足蜈蚣AI(NPC NPC)
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
                if (NPC.type == 412 && NPC.ai[0] == 0f)
                {
                    NPC.ai[3] = NPC.whoAmI;
                    NPC.realLife = NPC.whoAmI;
                    int num20 = 0;
                    int num21 = NPC.whoAmI;
                    int num22 = 30;
                    for (int num24 = 0; num24 < num22; num24++)
                    {
                        int num25 = 413;
                        if (num24 == num22 - 1)
                        {
                            num25 = 414;
                        }
                        num20 = NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.position.X + (float)(NPC.width / 2)), (int)(NPC.position.Y + (float)NPC.height), num25, NPC.whoAmI);
                        Main.npc[num20].ai[3] = NPC.whoAmI;
                        Main.npc[num20].realLife = NPC.whoAmI;
                        Main.npc[num20].ai[1] = num21;
                        Main.npc[num20].CopyInteractions(NPC);
                        Main.npc[num21].ai[0] = num20;
                        NetMessage.SendData(23, -1, -1, null, num20);
                        num21 = num20;
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
            }
            int num30 = (int)(NPC.position.X / 16f) - 1;
            int num31 = (int)((NPC.position.X + (float)NPC.width) / 16f) + 2;
            int num32 = (int)(NPC.position.Y / 16f) - 1;
            int num33 = (int)((NPC.position.Y + (float)NPC.height) / 16f) + 2;
            if (num30 < 0)
            {
                num30 = 0;
            }
            if (num31 > Main.maxTilesX)
            {
                num31 = Main.maxTilesX;
            }
            if (num32 < 0)
            {
                num32 = 0;
            }
            if (num33 > Main.maxTilesY)
            {
                num33 = Main.maxTilesY;
            }
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
            }
            float num39 = 8f;
            float num40 = 0.07f;
            Vector2 vector2 = new(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
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
                    for (int num54 = 0; num54 < 200; num54++)
                    {
                        if (Main.npc[num54].active && Main.npc[num54].type == NPC.type && num54 != NPC.whoAmI)
                        {
                            Vector2 vector3 = Main.npc[num54].Center - NPC.Center;
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
                if (NPC.type >= 87 && NPC.type <= 92)
                {
                    num58 = 42;
                }
                if (NPC.type >= 454 && NPC.type <= 459)
                {
                    num58 = 36;
                }
                if (NPC.type >= 13 && NPC.type <= 15)
                {
                    num58 = (int)((float)num58 * NPC.scale);
                }
                if (NPC.type >= 513 && NPC.type <= 515)
                {
                    num58 -= 6;
                }
                if (NPC.type >= 412 && NPC.type <= 414)
                {
                    num58 += 6;
                }
                if (NPC.type >= 621 && NPC.type <= 623)
                {
                    num58 = 24;
                }
                if (Main.getGoodWorld && NPC.type >= 13 && NPC.type <= 15)
                {
                    num58 = 62;
                }
                num57 = (num57 - (float)num58) / num57;
                num42 *= num57;
                num43 *= num57;
                NPC.velocity = Vector2.Zero;
                NPC.position.X += num42;
                NPC.position.Y += num43;
                if (NPC.type >= 87 && NPC.type <= 92)
                {
                    if (num42 < 0f)
                    {
                        NPC.spriteDirection = 1;
                    }
                    else if (num42 > 0f)
                    {
                        NPC.spriteDirection = -1;
                    }
                }
                if (NPC.type >= 454 && NPC.type <= 459)
                {
                    if (num42 < 0f)
                    {
                        NPC.spriteDirection = 1;
                    }
                    else if (num42 > 0f)
                    {
                        NPC.spriteDirection = -1;
                    }
                }
                if (NPC.type >= 621 && NPC.type <= 623)
                {
                    if (num42 < 0f)
                    {
                        NPC.spriteDirection = 1;
                    }
                    else if (num42 > 0f)
                    {
                        NPC.spriteDirection = -1;
                    }
                }
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
