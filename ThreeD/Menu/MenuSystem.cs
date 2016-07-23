using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DCG.Framework.Menu
{
    /// <summary>
    /// The menu system is meant to provide a 1 stop shop for menu'ing
    /// It uses the 2D rendering system
    /// </summary>
    public class MenuSystem
    {
        public IMenuRenderer Renderer { get; set; }
        public MenuContainer Menu { get; set; }

        public MenuSystem(GraphicsDevice device, SpriteFont font)
        {
            Renderer = new MenuRenderer2D(device, font);
        }

        public MenuContainer Set(params Action<MenuComposerGenericContext<MenuContainer>>[] actions)
        {
            var menu = MenuComposer.Compose(actions);
            Menu = menu;
            return menu;
        }

        public void Draw()
        {
            Renderer.Begin();
            Renderer.Render(Menu);
            Renderer.Flush();
        }
    }
}
