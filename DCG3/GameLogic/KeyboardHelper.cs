using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace DCG3.GameLogic
{
    class KeyboardHelper
    {

        private KeyboardState _old, _new;

        public void Update()
        {
            _new = Keyboard.GetState();
            


            _old = _new;
        }

    }
}
