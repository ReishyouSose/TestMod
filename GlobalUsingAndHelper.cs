global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using System;
global using Terraria;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using TestMod.Projectiles;
global using static TestMod.Helper;

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
        public static void SummonSet<T>(Player player, IEntitySource source, int damage, float kb,
            int amount, int SummonBuffType, int Itemdamage, int Logic, int Head, int Body, int Tail) where T : 蠕虫弹幕基类
        {
            int logic = -1;
            foreach (Projectile h in Main.projectile)
            {
                if (h.type == Logic && h.active && h.owner == player.whoAmI)
                {
                    logic = h.whoAmI;
                    break;
                }
            }
            if (logic == -1)
            {
                int L = SpawnMinion(player, source, Logic, damage, kb, SummonBuffType);
                if (Main.projectile[L].ModProjectile is T Proj)
                {
                    Proj.itemDamage = Itemdamage;
                    Proj.buffType = SummonBuffType;
                    var proj = Proj.proj;
                    var data = Proj.data;
                    for (int i = 0; i < amount + 2; i++) data.Add((Main.MouseWorld + Vector2.One * (i + 1), 0));

                    int p = SpawnMinion(player, source, Head, 0, 0);
                    proj.Add(p);
                    for (int i = 0; i < amount; i++)
                    {
                        int body = SpawnMinion(player, source, Body, 0, 0, i, 1, 1f / amount);
                        proj.Add(body);
                    }
                    Proj.tail = SpawnMinion(player, source, Tail, 0, 0);
                    Main.projectile[L].ai[0] = 1;
                }
            }
            else
            {
                if (Main.projectile[logic].ModProjectile is T Proj)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Proj.data.Add((Main.MouseWorld + Vector2.One * i, 0));
                        int proj = SpawnMinion(player, source, Body, 0, 0, i, 1, 1f / amount);
                        Proj.proj.Add(proj);
                    }
                    Main.projectile[logic].ai[0] = 1;
                }
            }
        }
        public static (Vector2 pos, float rot) CalculatePosAndRot((Vector2 pos, float rot) tar, (Vector2 pos, float rot) me, float dis)
        {
            Vector2 chaseDir = Vector2.Normalize(tar.pos - me.pos);
            /*if (chaseDir == Vector2.Zero)
            {
                return (chaseDir + Vector2.One, 0);
            }*/
            float chaserot = tar.rot - me.rot;
            if (chaserot != 0)
            {
                chaseDir = chaseDir.RotatedBy(MathHelper.WrapAngle(chaserot) * 0.1f);
            }
            Vector2 center = tar.pos - chaseDir * dis;

            return (center, chaseDir.ToRotation());
        }
        public static void SetProjSection(int whoami, (Vector2 pos, float rot) data, int dir, double damage = 0)
        {
            Projectile p = Main.projectile[whoami];
            p.Center = data.pos;
            p.rotation = data.rot;
            p.timeLeft = 2;
            p.direction = p.spriteDirection = dir;
            p.originalDamage = p.damage = (int)damage;
        }
        public static Rectangle RecCenter(Vector2 center, int Size)
        {
            return new Rectangle((int)center.X - Size / 2, (int)center.Y - Size / 2, Size, Size);
        }
        public static void DrawSet(SpriteBatch spb, Projectile proj, int Body, int amount, Color lightColor, float rot)
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
            spb.Draw(tex, proj.Center - Main.screenPosition, !body ? null : rec, lightColor,
                proj.rotation + rot, (!body ? tex.Size() : rec.Size()) / 2f, proj.scale,
                proj.spriteDirection == 1 ? 0 : SpriteEffects.FlipVertically/*Math.Abs(proj.rotation + rot) < Math.PI / 2f ? 0 : SpriteEffects.FlipVertically*/, 0);
            // 瞧见这里的rotation没，所以在SetPosAndDmg函数里又写了一次设置rotation，这可是很重要的
        }
        public static int SpawnMinion(Player player, IEntitySource source, int type, int damage, float kb, float ai0 = 0, float ai1 = 0, float minionSlots = 0)
        {
            int proj = player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, kb);
            Main.projectile[proj].ai[0] = ai0;
            Main.projectile[proj].ai[1] = ai1;
            Main.projectile[proj].minionSlots = minionSlots;
            return proj;
        }
    }
}
