using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Menu.Components
{
    public class MenuDataList : MenuContainer
    {

        public Dictionary<string, Func<string>> KeyValues { get; set; }

        public MenuDataList()
        {
            KeyValues = new Dictionary<string, Func<string>>();
        }

        public override void OnAdd(MenuContainer parent)
        {
            //var comps = new List<MenuComponent>(); // stupidly inefficient, but who cares...
            Y(-1);
            KeyValues.Keys.ToList().ForEach(key =>
            {
                var data = new MenuDataLabel();
                data.Y(-1);
                data.Text = key;
                data.ValueFunc = KeyValues[key];
                Add(data);
            });

            base.OnAdd(parent);
        }

   
    }
}
