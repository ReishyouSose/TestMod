using Terraria.DataStructures;

namespace TestMod.Items
{
    public abstract class MultipleSectionsMinionItem : ModItem
    {
        //便利的方法
        public static Projectile SpawnMinion(Player player, IEntitySource source, int type, int damage, float kb, float ai0 = 0, float ai1 = 0)
        {
            int p = player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, kb);
            Projectile proj = Main.projectile[p];
            proj.ai[0] = ai0;
            proj.ai[1] = ai1;
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
    }
}
