using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using ThreeD.Menu.Components;

namespace ThreeD.Menu
{
    public static class MenuComposer
    {

        /*
         * 
         * MenuComposer.Compose(m => m.Label("Hello world")
         *                            .Set(l => l.MinSize = new Ve
         * 
         * 
         */

        public static MenuContainer Compose(params Action<MenuComposerGenericContext<MenuContainer>>[] actions)
        {
            var container = new MenuContainer();
            var m = new MenuComposerGenericContext<MenuContainer>(container);
            m.Element = container;
            actions.ToList().ForEach(a =>
            {
                a(m);
            });

            return m.Element;
        }


        public static KeyValuePair<string, Func<string>> KeyValue(this MenuComposerGenericContext<MenuContainer> self,
            string key, Func<string> value)
        {
            return new KeyValuePair<string, Func<string>>(key, value);
        } 

        public static MenuComposerGenericContext<MenuDataList> DataList(
            this MenuComposerGenericContext<MenuContainer> self, params KeyValuePair<string, Func<string>>[] keyValues )
        {
            var ctx = new MenuComposerGenericContext<MenuDataList>(self.Container);

            var dict = new Dictionary<string, Func<string>>();
            keyValues.ToList().ForEach(k => dict.Add(k.Key, k.Value));
            ctx.Element.KeyValues = dict;
            return ctx;
        }   

        public static MenuComposerGenericContext<MenuDataLabel> DataLabel(
            this MenuComposerGenericContext<MenuContainer> self, string name, Func<string> func )
        {
            var ctx = new MenuComposerGenericContext<MenuDataLabel>(self.Container);
            ctx.Element.Text = name;
            ctx.Element.ValueFunc = func;
            return ctx;
        }   

        public static MenuComposerGenericContext<MenuList> List(
            this MenuComposerGenericContext<MenuContainer> self, List<string> list )
        {
            var ctx = new MenuComposerGenericContext<MenuList>(self.Container);
            ctx.Element.Elements = list;
            return ctx;
        }   

        public static MenuComposerGenericContext<MenuLabel> Label(
            this MenuComposerGenericContext<MenuContainer> self, string text)
        {
            var ctx = new MenuComposerGenericContext<MenuLabel>(self.Container);
            ctx.Element.Text = text;
            return ctx;
        }


        public static MenuComposerGenericContext<MenuLabelDynamic> DynamicLabel(
            this MenuComposerGenericContext<MenuContainer> self, Func<string> textFunc )
        {
            var ctx = new MenuComposerGenericContext<MenuLabelDynamic>(self.Container);
            ctx.Element.TextFunc = textFunc;
            return ctx;
        } 


        public static MenuComposerGenericContext<MenuContainer> Box(
            this MenuComposerGenericContext<MenuContainer> self)
        {
            var ctx = new MenuComposerGenericContext<MenuContainer>(self.Container);
            return ctx;
        } 

    }

    public class MenuComposerGenericContext <G> where G : MenuComponent
    {
        public MenuContainer Container { get; set; }
        public G Element { get; set; }

        public MenuComposerGenericContext(MenuContainer container)
        {
            Container = container;
            Element = Activator.CreateInstance(typeof (G)) as G;
        }

        public MenuComposerGenericContext<G> Add()
        {
            Container.Add(Element);
            return this;
        }

        public MenuComposerGenericContext<G> Do(Action<G> action)
        {
            action(Element);
            return this;
        } 
    }

}
