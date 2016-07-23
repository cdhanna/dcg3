using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Menu
{
    public abstract class MenuComponent
    {

        public Vector2 Position { get; set; }


        public Vector2 MinSize { get; set; }

        public virtual Color Color { get; set; }
        public virtual Color Background { get; set; }
        public virtual Color BorderColor { get; set; }
        
        protected MenuComponent()
        {
            Position = new Vector2(.01f, .01f);
            MinSize = new Vector2(.98f, .98f);
            Color = Color.White;
            Background = new Color(0, 0, 0, .5f);
            BorderColor = Color.White;
        }

        public void Height(float h)
        {
            MinSize = new Vector2(MinSize.X, h);
        }

        public void Width(float w)
        {
            MinSize = new Vector2(w, MinSize.Y);
        }
        public void Y(float y)
        {
            Position = new Vector2(Position.X, y);
        }

        public void X(float x)
        {
            Position = new Vector2(x, Position.Y);
        }

        public void Render(IMenuRenderer renderer)
        {
            renderer.Render(this); // drill down method.
        }

        public virtual void OnAdd(MenuContainer parent)
        {
            // do nothing ?
        }

    }

    public class MenuContainer : MenuComponent
    {

        public enum MenuContainerSizePolicy
        {
            Grow, // expands to fill parent height
            Shrink, // shrinks inside of parent to only show children
            Exact // whatever the height value is
        }

        public virtual List<MenuComponent> Components { get; set; }
        public MenuContainerSizePolicy YSizePolicy;
        

        public MenuContainer()
        {
            YSizePolicy = MenuContainerSizePolicy.Exact;
            Components = new List<MenuComponent>();
        }

        public void Add(MenuComponent component)
        {
            component.OnAdd(this);
            Components.Add(component);
        }

    }


    public class MenuLabel : MenuComponent
    {
        public virtual String Text { get; set; }
        public bool IsCentered { get; set; }

        public MenuLabel()
        {
            Text = "";
            Color = Color.White;
            MinSize = new Vector2(.98f, 0);
            IsCentered = false;
            Y(-1);
            // Padding += Vector3.UnitY * .004f + Vector3.UnitX * .006f;
        }

    }

    public class MenuLabelDynamic : MenuLabel
    {
        public Func<string> TextFunc { get; set; }

        public override string Text
        {
            get { return TextFunc(); }

        }

        public MenuLabelDynamic() : this( () => "")
        {
            
        }

        public MenuLabelDynamic(Func<string> textFunc)
        {
            TextFunc = textFunc;
        }
    }

}
