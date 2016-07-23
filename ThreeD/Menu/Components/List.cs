using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCG.Framework.Menu.Components
{

   

    public class MenuList : MenuContainer
    {

        public List<string> Elements { get; set; } 

        public MenuList()
        {
            Elements = new List<string>();
            YSizePolicy = MenuContainerSizePolicy.Grow;
        }

        public override List<MenuComponent> Components
        {
            get
            {
                var set = new List<MenuComponent>();
                var count = 0;
                Elements.Select(s =>
                {
                    var label = new MenuLabel();
                    label.Text = s;
                    //label.Height(.1f);
                    label.Width(.98f);
                    label.Y(-1); // stack hack
                    count ++;
                    return label;
                }).ToList().ForEach(set.Add);

                return set;
            }
        }
    }
}
