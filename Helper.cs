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
