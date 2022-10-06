using System.IO;
using System.Linq;

namespace TestMod.Projectiles
{

    public class GliderPro : ModProjectile
    {
        private float Timer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        private float State
        {
            get { return Projectile.ai[1]; }
            set { Projectile.ai[1] = value; }
        }
        public Vector2 TargetLocation = new Vector2();

        private static float _nearPlayerSpeed = 0.1f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("僚机");
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 3;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.scale = 1.1f;
            // 召唤物必备的属性
            //Main.projPet[Type] = true;
            Projectile.netImportant = true;
            Projectile.minionSlots = 1;
            Projectile.minion = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        /// <summary>
        /// 没有接触伤害
        /// </summary>
        /// <returns></returns>
        public override bool MinionContactDamage()
        {
            return false;
        }
        /// <summary>
        /// 寻找最近的敌方单位
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxDistance"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static NPC FindCloestEnemy(Vector2 position, float maxDistance, Func<NPC, bool> predicate)
        {
            float maxDis = maxDistance;
            NPC res = null;
            foreach (var npc in Main.npc.Where(n => n.active && !n.friendly && predicate(n)))
            {
                float dis = Vector2.Distance(position, npc.Center);
                if (dis < maxDis)
                {
                    maxDis = dis;
                    res = npc;
                }
            }
            return res;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            var modPlayer = player.GetModPlayer<GliderPlayer>();
            // 玩家死亡会让召唤物消失
            if (player.dead)
            {
                modPlayer.Gliders = false;
            }
            if (modPlayer.Gliders)
            {
                // 如果Gliders不为true那么召唤物弹幕只有两帧可活
                Projectile.timeLeft = 2;
            }
            player.AddBuff(ModContent.BuffType<GliderBuff>(), 2);// 把之前说的添加buff放在这里
            // 弹幕的姿态调整
            Projectile.direction = (Projectile.spriteDirection = -Math.Sign(Projectile.velocity.X));
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
            Projectile.netUpdate = true;
            NPC npc = FindCloestEnemy(Projectile.Center, 1200f, (n) =>
            {
                return n.CanBeChasedBy() &&
                !n.dontTakeDamage && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1);
            });
            if (TargetLocation == Vector2.Zero && npc != null && Vector2.Distance(Projectile.Center, player.Center) < 700)
            {
                TargetLocation = npc.Center;
            }
            // 如果鼠标没有控制而且周围没有敌人
            if (npc == null && TargetLocation == Vector2.Zero)
            {
                State = 0;
                Timer = 0;
            }

            if (State == 0)
            {
                MoveAroundPlayer(player);
                if (npc != null || TargetLocation != Vector2.Zero) { State = 1; }
            }
            else if (State == 1)
            {
                Timer++;
                if (player.controlUseTile && Main.myPlayer == Projectile.owner)
                {
                    TargetLocation = Main.MouseWorld;
                    Projectile.netUpdate = true;
                }
                Vector2 diff = TargetLocation - Projectile.Center;
                float distance = diff.Length();
                diff.Normalize();
                Projectile.rotation = diff.ToRotation() + 1.57f;
                // 射击
                if (Timer % 30 < 1)
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), 
                        Projectile.Center + Projectile.velocity + diff * 30, diff * 3f, 
                        /*ModContent.ProjectileType<BlazeBallSmall>()*/ProjectileID.TerraBeam,
                        Projectile.damage + 5, Projectile.knockBack, Projectile.owner);
                }
                if (distance > 500)
                {
                    Projectile.velocity = (Projectile.velocity * 20f + diff * 5) / 21f;
                }
                else
                {
                    Projectile.velocity *= 0.97f;
                }
                // 让召唤物不至于靠的太近
                if (distance > 200)
                {
                    Projectile.velocity = (Projectile.velocity * 40f + diff * 5) / 41f;
                }
                else if (distance < 180)
                {
                    Projectile.velocity = (Projectile.velocity * 20f + diff * -4) / 21f;
                }
                TargetLocation = Vector2.Zero;
            }


            // 召唤物弹幕的后续处理，轨迹，限制等
            if (Projectile.velocity.Length() > 16)
            {
                Projectile.velocity *= 0.98f;
            }
            if (Math.Abs(Projectile.velocity.X) < 0.01f || Math.Abs(Projectile.velocity.Y) < 0.01f)
            {
                Projectile.velocity = Main.rand.NextVector2Circular(1, 1) * 2f;
            }

            if (Projectile.velocity.Length() > 6)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    MyDustId.OrangeFire, -Projectile.velocity.X, -Projectile.velocity.Y, 100, Color.Red, 1.0f);
                dust.noGravity = true;
                dust.position = Projectile.Center - Projectile.velocity;
            }

        }

        /// <summary>
        /// 让召唤物绕着玩家运动
        /// </summary>
        /// <param name="player"></param>
        private void MoveAroundPlayer(Player player)
        {
            Vector2 diff = Projectile.Center - player.Center;
            diff.Normalize();
            //diff = diff.RotatedBy(MathHelper.PiOver2);
            Projectile.velocity -= diff * 0.2f;

            if (Projectile.Center.X < player.Center.X)
            {
                Projectile.velocity.X += _nearPlayerSpeed;
            }
            if (Projectile.Center.X > player.Center.X)
            {
                Projectile.velocity.X -= _nearPlayerSpeed;
            }
            if (Projectile.Center.Y < player.Center.Y)
            {
                Projectile.velocity.Y += _nearPlayerSpeed;
            }
            if (Projectile.Center.Y > player.Center.Y)
            {
                Projectile.velocity.Y -= _nearPlayerSpeed;
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(TargetLocation);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            TargetLocation = reader.ReadVector2();
        }
    }
    public class GliderPlayer : ModPlayer
    {
        public bool Gliders;
        public override void ResetEffects()
        {
            Gliders = false;
        }
    }
    public class GliderBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("僚机部队");
            Description.SetDefault("僚机群会为你战斗");
            Main.buffNoSave[Type] = true;
            // 不显示buff时间
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            GliderPlayer modPlayer = player.GetModPlayer<GliderPlayer>();
            // 如果当前有属于玩家的僚机的弹幕
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GliderPro>()] > 0)
            {
                modPlayer.Gliders = true;
            }
            // 如果玩家取消了这个召唤物就让buff消失
            if (!modPlayer.Gliders)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                // 无限buff时间
                player.buffTime[buffIndex] = 9999;
            }
        }
    }

    public class GliderStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            DisplayName.SetDefault("僚机召唤杖");
            Tooltip.SetDefault("产生可控制的僚机为你作战");
        }
        public override void SetDefaults()
        {
            Item.height = 32;
            Item.width = 32;
            Item.maxStack = 1;
            Item.rare = 7;
            Item.damage = 45;
            Item.value = Item.buyPrice(0, 54, 0, 0);
            Item.noMelee = true;
            Item.useTime = 30;
            Item.knockBack = 1f;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.mana = 10;
            Item.crit = 10;
            Item.staff[Item.type] = true;
            Item.UseSound = SoundID.Item44;
            Item.DamageType = DamageClass.Summon;
            //Item.buffType = ModContent.BuffType<GliderBuff>();
            //Item.buffTime = 3600;
            Item.shoot = ModContent.ProjectileType<GliderPro>();
            Item.shootSpeed = 10f;

        }
        /*
        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer && Main.mouseRight)
            {
                foreach (var proj in Main.projectile.Where(p => p.active && p.friendly && p.type == Item.shoot && p.owner == player.whoAmI))
                {
                    GliderPro pro = (GliderPro)proj.ModProjectile;
                    pro.TargetLocation = Main.MouseWorld;
                }
            }
        }*/
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //if (player.ownedProjectileCounts[mod.ProjectileType("ExecutionerPro")] == 0)
            //{
            //    Projectile.NewProjectile(Main.MouseWorld, new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)),
            //        mod.ProjectileType("ExecutionerPro"),
            //        damage, knockBack, player.whoAmI);
            //}
            //else
            //{
            //    foreach(Projectile p in Main.projectile)
            //    {
            //        if(p.active && p.friendly && p.owner == player.whoAmI && p.type == mod.ProjectileType("ExecutionerPro"))
            //        {
            //            p.Kill();
            //        }
            //    }
            //    Projectile.NewProjectile(Main.MouseWorld, new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-10, 10)), mod.ProjectileType("ExecutionerPro"),
            //        damage, knockBack, player.whoAmI);
            //}
            return true;
        }

        public override void AddRecipes()
        {
            Recipe re = CreateRecipe();
            re.AddIngredient(ItemID.HallowedBar, 18);
            re.AddIngredient(ItemID.SoulofMight, 5);
            re.AddIngredient(ItemID.SoulofLight, 15);
            re.AddIngredient(ItemID.SoulofNight, 15);
            re.AddTile(TileID.MythrilAnvil);
            re.Register();

            re = CreateRecipe();
            re.AddIngredient(ItemID.HallowedBar, 18);
            re.AddIngredient(ItemID.SoulofSight, 5);
            re.AddIngredient(ItemID.SoulofLight, 15);
            re.AddIngredient(ItemID.SoulofNight, 15);
            re.AddTile(TileID.MythrilAnvil);
            re.Register();

            re = CreateRecipe();
            re.AddIngredient(ItemID.HallowedBar, 18);
            re.AddIngredient(ItemID.SoulofFright, 5);
            re.AddIngredient(ItemID.SoulofLight, 15);
            re.AddIngredient(ItemID.SoulofNight, 15);
            re.AddTile(TileID.MythrilAnvil);
            re.Register();
        }
    }
}
