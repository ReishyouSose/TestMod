using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TestMod.UI
{
    public class TestProjPanel : UIState
    {
        public UIPanel panel;
        public UIText text,text2;
        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.Width.Pixels = 150;
            panel.Height.Pixels = 300;
            panel.VAlign = 0.35f;
            panel.HAlign = 0.8f;
            Append(panel);

            UIText proj = new("proj");
            proj.Top.Pixels = 15;
            proj.Left.Pixels = 30;

            UIText uuid = new("uuid");
            proj.Top.Pixels = 15;
            proj.Left.Pixels = 60;

            text = new("");
            text.Top.Pixels = 30;
            text.Left.Pixels = 30;
            panel.Append(text);

            text2 = new("");
            text2.Top.Pixels = 30;
            text2.Left.Pixels = 60;
            panel.Append(text2);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (Projectile logic in Main.projectile)
            {
                if (logic.active && logic.ModProjectile is BaseLogicProj proj)
                {
                    text.SetText(string.Join("\n", proj.proj));
                    List<Vector2> v2 = new();
                    for (int i = 0; i < proj.proj.Count; i++)
                    {
                        v2.Add(proj.data[i].pos);
                    }
                    text2.SetText(string.Join("\n", v2));
                }
            }
        }
    }
}
