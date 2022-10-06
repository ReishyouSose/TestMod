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
    }
}
