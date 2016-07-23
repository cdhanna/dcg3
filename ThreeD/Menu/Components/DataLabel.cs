using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DCG.Framework.Menu.Components
{
    public class MenuDataLabel : MenuContainer
    {

        public string Text
        {
            get { return Label.Text; }
            set { Label.Text = value; }
        }

        public Func<string> ValueFunc
        {
            get { return ValueLabel.TextFunc; }
            set { ValueLabel.TextFunc = value; }
        }

        public MenuLabel Label { get; set; }
        public MenuLabelDynamic ValueLabel { get; set; }

        public override Color Color
        {
            set
            {
                if (Label != null)
                {
                    Label.Color = value;
                    ValueLabel.Color = value;
                }
                base.Color = value;
            }
        }

        public MenuDataLabel()
        {

            Label = new MenuLabel();
            Label.Width(.48f);
            Label.X(.01f);
            Label.Y(.1f);

            ValueLabel = new MenuLabelDynamic();
            ValueLabel.Width(.48f);
            ValueLabel.X(.51f);
            ValueLabel.Y(.1f);

            Text = "unknown";
            ValueFunc = () => "no value";

            Components.Add(Label);
            Components.Add(ValueLabel);

            Height(.2f);
            Width(.98f);
            YSizePolicy = MenuContainerSizePolicy.Shrink;
            BorderColor = Color.Gray;
            Background = new Color(0, 0, 0, .3f);
        }

    }
}
