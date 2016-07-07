using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ThreeD.Menu
{
    public interface IMenuRenderer
    {

        void Begin();
        void Render(MenuComponent component);
        void Flush();
    }

    public class MenuRenderer2D : IMenuRenderer
    {

        public delegate Vector2 ScaleFunc(Vector2 v);

        //public delegate Vector2 ScaleInverseFunc(Vector2 v);

        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _pixel;

        private MenuContainer _screen;

       // private Dictionary<Type, Action<MenuComponent, MenuContainer, Vector2>> _renderHandlers;


        public MenuRenderer2D(GraphicsDevice device, SpriteFont font)
        {
            _device = device;
            _spriteBatch = new SpriteBatch(device);
            _font = font;
            _pixel = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new Color[]{Color.White});
           // _renderHandlers = new Dictionary<Type, Action<MenuComponent, MenuContainer, Vector2>>();
            
            _screen = new MenuContainer();
            _screen.MinSize = Vector2.One;

            //_renderHandlers.Add(typeof(MenuLabel), RenderLabel);
            //_renderHandlers.Add(typeof(MenuContainer), RenderContainer);
        

        }

        public void Begin()
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
        }

        public Vector2 ToPixel(Vector2 ratio)
        {
            return new Vector2(ratio.X*_device.Viewport.Width, ratio.Y*_device.Viewport.Height);
        }

        public void Render(MenuComponent component)
        {
            Render(component, _screen, Vector2.Zero, Vector2.One);
        }

        private void Render(MenuComponent component, MenuContainer parent, Vector2 offset, Vector2 scale)
        {
            // draw background & border.
            //var pos = parent.Position + component.Margin;
            var type = component.GetType();

            if (component is MenuContainer)
                RenderContainer(component, parent, offset, scale);

            if (component is MenuLabel)
                RenderLabel(component, parent, offset, scale);

            //_renderHandlers[type](component, parent, offset);
            //RenderContainer(component, parent, offset);
        }

        private void RenderContainer(MenuComponent component, MenuContainer parent, Vector2 offset, Vector2 scale)
        {
            var self = component as MenuContainer;

            var origin = offset;
            origin.X += self.Position.X * scale.X;
            origin.Y += self.Position.Y * scale.Y;

            var size = Size(component, offset, scale);

            // draw box
            DrawBoxBorder(ToPixel(origin), ToPixel(origin + size), component.BorderColor, component.Background);
            
            // draw children
            var localOffset = Vector2.Zero;
            localOffset.Y = .01f;
            self.Components.ForEach(child =>
            {
                var childSize = Size(child, origin, size);
                if (child.Position.Y < 0)
                {
                    child.Y(localOffset.Y);
                    localOffset.Y += (childSize.Y+.01f) / size.Y;
                }
                Render(child, self, origin , size);
            });
        }

        private void RenderLabel(MenuComponent component, MenuContainer parent, Vector2 offset, Vector2 scale)
        {
            var self = component as MenuLabel;
            var origin = new Vector2(self.Position.X * scale.X, self.Position.Y * scale.Y) + offset;
            

            var size = Vector2.Zero;
            size.X = self.MinSize.X * scale.X;
            size.Y = self.MinSize.Y * scale.Y;

            var textPadding = new Vector2(2, 4);
            
            var textSize = _font.MeasureString(self.Text) + textPadding * 2;
            textSize.X /= _device.Viewport.Width;
            textSize.Y /= _device.Viewport.Height;
            //size.X *= parent.MinSize.X;
            //size.Y *= scale.Y;

            size = new Vector2(Math.Max(size.X, textSize.X), Math.Max(size.Y, textSize.Y));


            var centerOffset = Vector2.Zero;
            if (self.IsCentered)
            {
                centerOffset.X = size.X/2 - textSize.X/2;
            }

            DrawBoxBorder(ToPixel(origin), ToPixel(origin + size), self.BorderColor, self.Background);

            _spriteBatch.DrawString(_font, self.Text, ToPixel(origin + centerOffset) + textPadding, self.Color);
            
        }

        public Vector2 Size(MenuComponent component, Vector2 offset, Vector2 scale)
        {
            Vector2 size = Vector2.Zero;
            if (component is MenuLabel)
            {
                var label = component as MenuLabel;
                size.X = label.MinSize.X * scale.X;
                size.Y = label.MinSize.Y * scale.Y;

                var textPadding = new Vector2(2, 4);

                var textSize = _font.MeasureString(label.Text) + textPadding * 2;
                textSize.X /= _device.Viewport.Width;
                textSize.Y /= _device.Viewport.Height;
                size = new Vector2(Math.Max(size.X, textSize.X), Math.Max(size.Y, textSize.Y));
            }

            if (component is MenuContainer)
            {
                var container = component as MenuContainer;
                size.X = component.MinSize.X * scale.X;
                size.Y = component.MinSize.Y * scale.Y;


                // should the box grow ?
                if (container.YSizePolicy == MenuContainer.MenuContainerSizePolicy.Grow)
                    //size.Y = scale.Y - self.Position.Y*2*scale.Y;
                    size.Y = (1 - (container.Position.Y + .01f)) * scale.Y;


                if (container.YSizePolicy == MenuContainer.MenuContainerSizePolicy.Shrink)
                {
                    // get child max (y + height) score. 
                    size.Y = 0;
                    if (container.Components.Count > 0)
                        size.Y = Size(container.Components[0], offset, scale).Y + .02f;

                }

            }

            return size;
        }

        public void Flush()
        {
            _spriteBatch.End();
        }

        private void DrawBox(Vector2 start, Vector2 size, Color color)
        {
            _spriteBatch.Draw(_pixel, start, null, color, 0, Vector2.Zero, size,SpriteEffects.None, 0f);
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            var diff = end - start;
            var angle = (float) Math.Atan2(diff.Y, diff.X);
            var mag = diff.Length();

            _spriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero, new Vector2(mag, 1), SpriteEffects.None, 0f);
        }


        private void DrawBoxBorder(Vector2 topLeft, Vector2 botRight, Color borderColor, Color backgroundColor)
        {

            var diff = botRight - topLeft;

            DrawBox(topLeft, botRight - topLeft, backgroundColor);

            DrawLine(topLeft, topLeft + diff.X * Vector2.UnitX, borderColor);

            DrawLine(
                topLeft + diff.X * Vector2.UnitX,
                botRight,
                borderColor);

            DrawLine(botRight, topLeft + Vector2.UnitY * diff.Y, borderColor);
            DrawLine(topLeft, topLeft + Vector2.UnitY * diff.Y, borderColor);

        }
        

    }

}
