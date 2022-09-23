namespace TestMod.ModPlayers
{
    public class MinionPlayer : ModPlayer
    {
        public bool TestMinion;
        public override void ResetEffects()
        {
            TestMinion = false;
        }
    }
}
