using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace TestMod.UI
{
    public class UISystem :ModSystem
    {
        public UserInterface u1;
        public TestProjPanel panel;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                panel = new TestProjPanel();
                panel.Activate();
                u1 = new();
            }
        }
        public override void Unload()
        {
            panel = null;
        }

        public GameTime gametime;
        public override void UpdateUI(GameTime gameTime)
        {
            gametime = gameTime;
            u1?.Update(gameTime);
            u1.SetState(panel);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            //寻找一个名字为Vanilla: Mouse Text的绘制层，也就是绘制鼠标字体的那一层，并且返回那一层的索引
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                //往绘制层集合插入一个成员，第一个参数是插入的地方的索引，第二个参数是绘制层
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(name: "MKInterface",
                    drawMethod: delegate
                    {
                        u1.Draw(Main.spriteBatch, gametime);
                        return true;
                    },
                //绘制层的类型，可以被设置缩放啥的
                InterfaceScaleType.UI));
            }
        }
    }
}
