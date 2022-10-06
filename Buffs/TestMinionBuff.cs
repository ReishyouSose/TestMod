using TestMod.ModPlayers;

namespace TestMod.Buffs
{
    public class TestMinionBuff : ModBuff
    {
        public override string Texture => "TestMod/Pictures/Buffs/召唤Buff";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("测试召唤物Buff");
            Description.SetDefault("测试召唤物正在保护你");

            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 获取ModPlayer实例
            MinionPlayer modPlayer = player.GetModPlayer<MinionPlayer>();
            // 这是玩家某种弹幕的计数
            if (player.ownedProjectileCounts[ModContent.ProjectileType<逻辑>()] > 0)
            {
                modPlayer.TestMinion = true;
            }
            if (!modPlayer.TestMinion)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
                return;
            }
            player.buffTime[buffIndex] = 18000;
        }
    }
}
